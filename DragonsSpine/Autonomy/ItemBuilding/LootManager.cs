#region 
/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Data;
using DragonsSpine.GameWorld;
using ArrayList = System.Collections.ArrayList;
using DragonsSpine.Autonomy.EntityBuilding;
using DragonsSpine.Autonomy.ItemBuilding.ArmorSets;

namespace DragonsSpine.Autonomy.ItemBuilding
{
    public static class LootManager
    {
        /// <summary>
        /// Base percentages for loot chance. This value may be modified (positive or negative) in AppSettings via LootPercentageModifier.
        /// </summary>
        private const int PCT_VERYCOMMON = 80;
        private const int PCT_COMMON = 55;
        private const int PCT_UNCOMMON = 30;
        private const int PCT_RARE = 15;
        private const int PCT_VERYRARE = 10;
        private const int PCT_ULTRARARE = 5;
        private const int PCT_UNIQUE = 25;

        private const int LEVEL_REQ_ULTRARARE = 10;
        private const int LEVEL_REQ_VERYRARE = 8;
        private const int LEVEL_REQ_RARE = 6;

        private enum LootType
        {
            None, // not possible to EVER be found as loot (DEV items, DEV/GM created items, also Armory items)
            Armory, // basic weaponry and shields (used when equipping humanoids)
            Armor, // basic armor, typically placed in loot tables via ArmorSets
            Spawned, // ground spawn (berries)
            Foraged, // foraged items
            Dropped, // loot dropped from NPCs (sacked)
            Worn, // loot worn by an entity
            Belted, // loot placed on an NPCs belt
            HeldRight, // loot always held in the right hand of an entity
            HeldLeft, // loot always held in the left hand of an entity
            HeldDual, // always hold item in both hands (duplicate items)
            Lair, // loot found only in lair cells for an entity -- a limit via code is placed on the amount of Lair loot that will be instantiated (1 chance based on rarity level per lair cell)
            Purchased, // items only found on merchants
            Crafted, // loot crafted by combining ingredients
            Quest, // loot received ONLY as the result of quest completion
            Tanned, // loot only found when tanned from an NPC / Entity -- note these items are typically worn by entities for their benefits, though not visible
            Unique, // loot specifically assigned to one (or more?) particular NPC / Entity
            Artifact, // this loot type means the item will be placed in the game world by autonomy
            Merchant, // loot only found on a merchant
        }

        public enum LootRarityLevel
        {
            Never, // not possible to EVER be found as loot (DEV items?)
            Always, // 100%
            VeryCommon, // 80%
            Common, // 55%
            Uncommon, // 30%
            Rare, // 15%
            VeryRare, // 10%
            UltraRare, // 5%
            Unique, // 5%? Same as UltraRare?
            Lair, // May be moved to settings. Currently set at 50%. 8/7/2018 Eb
            Lore, // This is for artifacts that may be held, worn, dropped. Checks are made to determine if the player already has it.
        }

        #region Antiquated as of 12/14/2015 -- hard coded lists with item ID constants.
		private static readonly List<int> AllBasicWeaponry = new List<int>(){Item.ID_WOODEN_SHORTBOW,Item.ID_YEW_LONGBOW,Item.ID_WOODEN_CROSSBOW,Item.ID_WOODEN_FLAIL,
            Item.ID_IRON_HALBERD,Item.ID_STEEL_WARHAMMER,Item.ID_LARGE_IRON_BATTLEAXE,Item.ID_MACE,Item.ID_STEEL_RAPIER,Item.ID_WOODEN_STAFF,Item.ID_WOODEN_SPEAR,
            Item.ID_WOODEN_THREESTAFF,Item.ID_SHARP_STEEL_KATANA,Item.ID_FINE_STEEL_LONGSWORD,Item.ID_IRON_SHORTSWORD,Item.ID_IRON_GREATSWORD,
            Item.ID_STONE_DAGGER, Item.ID_STEEL_DAGGER, Item.ID_DAGGER_PLUS_TWO };

        private static readonly List<int> MeleeWeapons = new List<int>(){Item.ID_WOODEN_FLAIL,Item.ID_IRON_HALBERD,Item.ID_STEEL_WARHAMMER,Item.ID_LARGE_IRON_BATTLEAXE,
            Item.ID_MACE,Item.ID_STEEL_RAPIER,Item.ID_WOODEN_STAFF,Item.ID_WOODEN_SPEAR,Item.ID_WOODEN_THREESTAFF,Item.ID_SHARP_STEEL_KATANA,Item.ID_FINE_STEEL_LONGSWORD,
            Item.ID_IRON_SHORTSWORD,Item.ID_IRON_GREATSWORD};

        private static readonly List<int> RangeWeapons = new List<int>() { Item.ID_WOODEN_CROSSBOW, Item.ID_WOODEN_SHORTBOW, Item.ID_YEW_LONGBOW };

        private static readonly List<int> MeleePiercingWeapons = new List<int>() { Item.ID_STEEL_RAPIER, Item.ID_WOODEN_SPEAR, Item.ID_STONE_DAGGER, Item.ID_STEEL_DAGGER, Item.ID_DAGGER_PLUS_TWO };

        private static readonly List<int> ThaumMeleeWeapons = new List<int>() { Item.ID_WOODEN_FLAIL, Item.ID_STEEL_WARHAMMER, Item.ID_MACE, Item.ID_IRON_GREATSWORD };
         
	    private static readonly List<int> WizAndSorcMeleeWeapons = new List<int>() { Item.ID_EBONSNAKESTAFF, Item.ID_WOODEN_STAFF, Item.ID_DAGGER_PLUS_TWO, Item.ID_STEEL_DAGGER, Item.ID_STONE_DAGGER };
        #endregion

        // Key = unique Item ID, Value = Tuple of item type, base type, and skill type for faster access.
        private static Dictionary<int, Tuple<Globals.eItemType, Globals.eItemBaseType, Globals.eSkillType>> LootItemsDictionary = new Dictionary<int, Tuple<Globals.eItemType, Globals.eItemBaseType, Globals.eSkillType>>();

        // Holds a list of item IDs for items with loot table info Labeled as LootType.Artifact or RarityLevel.Lore.
        // Not dispersed in regular loot. Separate calculations are done for placing these items on NPCs or in their lairs.
        private static List<int> ArtifactsList = new List<int>();

        // Key = unique Item ID, Value = Tuple of List<EntityList.Entity, >, rarity level, loot type, list of Land short descriptions, list of Map names, list of zPlanes integers, required level
        static Dictionary<int, Tuple<List<EntityLists.Entity>, LootRarityLevel, LootType, List<string>, List<string>, List<int>, int>>
            LootInfoDictionary = new Dictionary<int,Tuple<List<EntityLists.Entity>,LootRarityLevel,LootType,List<string>,List<string>, List<int>, int>>();

        static Dictionary<int, Tuple<List<EntityLists.Entity>, LootRarityLevel, LootType, List<string>, List<string>, List<int>, int>>
            ArmoryInfoDictionary = new Dictionary<int, Tuple<List<EntityLists.Entity>, LootRarityLevel, LootType, List<string>, List<string>, List<int>, int>>();

        // Profession restrictions on a loot item, with entity and lists of professions that may have the item as loot.
        static Dictionary<int, Dictionary<EntityLists.Entity, List<Character.ClassType>>> LootProfessionRestrictions = new Dictionary<int, Dictionary<EntityLists.Entity, List<Character.ClassType>>>();

        // Alignment restrictions on a loot item, with entity and lists of alignments that may have the item as loot.
        static Dictionary<int, Dictionary<EntityLists.Entity, List<Globals.eAlignment>>> LootAlignmentRestrictions = new Dictionary<int, Dictionary<EntityLists.Entity, List<Globals.eAlignment>>>();

        // Key = unique NPC ID, Value = zPlane name and LootTable associated with the unique NPC ID.
        //public static Dictionary<int, Tuple<string, LootTable>> NPCLootTables = new Dictionary<int, Tuple<string, LootTable>>();

        // Item1 = NPC ID, Item 2 = zPlane name, Item 3 = LootTable
        //public static List<Tuple<int, string, LootTable>> NPCLootTables = new List<Tuple<int, string, LootTable>>();
        public static Dictionary<Tuple<int, string>, LootTable> NPCLootTablesDictionary = new Dictionary<Tuple<int, string>, LootTable>();

        public static int NPCLootTablesCount
        {
            //get { return NPCLootTables.Count; }
            get { return NPCLootTablesDictionary.Count; }
        }

        public static bool LootTableExists(int npcID, string zName, out LootTable loot)
        {
            Tuple<int,string> tuple = Tuple.Create<int, string>(npcID, zName);

            if (NPCLootTablesDictionary.ContainsKey(tuple))
            {
                loot = NPCLootTablesDictionary[tuple];
                return true;
            }
            else loot = new LootTable(npcID);
            return false;
        }

        /// <summary>
        /// Build a Dictionary with unique Item.itemID as the Key and a Tuple containing itemType, baseType and skillType for indexing.
        /// Also builds the LootInfoDictionary with more specific loot information.
        /// </summary>
        /// <returns></returns>
        public static bool BuildLootDictionaries()
        {
            foreach (int itemID in Item.ItemDictionary.Keys)
            {
                DataRow dr = Item.ItemDictionary[itemID];

                #region Add to dictionary based on item type, base type and skill type.
                if (!LootItemsDictionary.ContainsKey(itemID))
                {
                    try
                    {
                        Globals.eItemType itemType = (Globals.eItemType)Enum.Parse(typeof(Globals.eItemType), dr["itemType"].ToString());
                        Globals.eItemBaseType baseType = (Globals.eItemBaseType)Enum.Parse(typeof(Globals.eItemBaseType), dr["baseType"].ToString());
                        Globals.eSkillType skillType = (Globals.eSkillType)Enum.Parse(typeof(Globals.eSkillType), dr["skillType"].ToString());

                        LootItemsDictionary.Add(itemID, Tuple.Create(itemType, baseType, skillType));
                    }
                    catch (Exception e)
                    {
                        Utils.Log("Failure at LootBuilder.BuildLootDictionary() while adding to LootItemsDictionary. ItemID: " + itemID, Utils.LogType.SystemFailure);
                        Utils.LogException(e);
                        return false;
                    }
                }
                #endregion

                if (!LootInfoDictionary.ContainsKey(itemID))
                {
                    // Entities (separated by |, All or all), LootRarityLevel, LootType, LandID (optional, further split with |), MapID (optional, further split with |)
                    // EG: entity1|entity2|,Uncommon,Dropped,AG|BG,Axe_Glacer|Rift_Glacier,Zone INT,level
                    // EG: [chaotic_evil]entity1|entity2|entity3,UltraRare,Dropped,AG,Innkadi,-2850,18
                    // EG: [chaotic_evil](sorcerer)entity1|[lawful](knight)entity2|entity3,UltraRare,Dropped,AG,Innkadi,-2850,18
                    string lootInfo = dr["lootTable"].ToString();

                    List<EntityLists.Entity> entityList = new List<EntityLists.Entity>();// { EntityLists.Entity.All }; // All NPCs will have the loot put in their appropriate loot table variable
                    LootRarityLevel rarityLevel = LootRarityLevel.Never; // default -- this means it will not drop as loot
                    LootType lootType = LootType.Dropped; // default -- most loot is dropped when killing an NPC
                    List<string> landsList = new List<string>() { "AG|BG" }; // land short descriptions (AG|BG|UW) or All
                    List<string> mapsList = new List<string>() { "All" }; // more specific loot info with map names (Island of Kesmai|Annwn) or All
                    List<int> zPlanesList = new List<int>();
                    int levelRequirement = 0;

                    // Item will be added to LootInfoDictionary but not added to any loot tables.
                    if (string.IsNullOrEmpty(lootInfo))
                    {
                        LootInfoDictionary.Add(itemID, Tuple.Create(entityList, rarityLevel, lootType, landsList, mapsList, zPlanesList, levelRequirement));
                        Item item = Item.CopyItemFromDictionary(itemID);
                        Utils.Log(item.notes + " (Item ID: " + itemID + ") not added to any loot tables.", Utils.LogType.LootTableAbsent);
                    }
                    else
                    {
                        // Index
                        // 0 = npc entities ("All" or "all" means all non animal NPCs will get the loot)
                        // 1 = rarity level
                        // 2 = loot type (dropped, lair, etc)
                        // 3 = lands by shortDesc (BG, AG -- or BG|AG)
                        // 4 = maps by name (Island_of_Kesmai, Axe_Glacier -- or Island of Kesmai|Axe Glacier)
                        // 5 = zone integers
                        // 6 = minimum level requirement
                        // Note maps may be preceded by a minus symbol meaning it is excluded

                        // Primary loot info array. Split up by the index above.
                        string[] info = lootInfo.Split(",".ToCharArray());

                        // List of entities to receive this loot item in their loot tables.
                        string[] entityInfoList = info[0].Split("|".ToCharArray());

                        // [Chaotic]Snakeman(Thaumaturge Wizard Sorcerer)|[Chaotic]Ninja|Yaun__Ti(Thaumaturge Wizard Sorcerer)

                        Dictionary<EntityLists.Entity, List<Globals.eAlignment>> alignmentList = new Dictionary<EntityLists.Entity, List<Globals.eAlignment>>();
                        Dictionary<EntityLists.Entity, List<Character.ClassType>> professionList = new Dictionary<EntityLists.Entity, List<Character.ClassType>>();

                        foreach (string entityName in entityInfoList)
                        {
                            string entityString = entityName;

                            string aString = ""; // alignment info
                            string pString = ""; // profession info

                            if (entityName.StartsWith("["))
                                aString = lootInfo.Substring(1, lootInfo.IndexOf("]") - 1);

                            if (entityName.Contains("("))
                                pString = info[0].Substring(info[0].IndexOf("(") + 1, info[0].IndexOf(")") - info[0].IndexOf("(") - 1);

                            // Remove alignment info from entityName.
                            if (aString != "")
                            {
                                entityString = entityString.Replace(aString, "");
                                entityString = entityString.Replace("[]", "");
                            }

                            // Remove profession info from entityName.
                            if (pString != "")
                            {
                                entityString = entityString.Replace(pString, "");
                                entityString = entityString.Replace("()", "");
                            }

                            // Need to parse entity type.
                            if (!Enum.TryParse(entityString, true, out EntityLists.Entity entity))
                            {
                                // log failure to parse
                                Utils.Log("Failed to parse " + entityString + " into EntityLists.Entity in LootManager.BuildLootDictionaries for item ID " + itemID + ". Not added to the LootInfo dictionary.", Utils.LogType.LootWarning);
                                continue;
                            }
                            else if (!entityList.Contains(entity)) entityList.Add(entity); // specific NPC entities to have the loot item                                

                            #region Alignment Restrictions
                            if (!string.IsNullOrEmpty(aString))
                            {
                                string[] alignmentInfoList = aString.Split(" ".ToCharArray());

                                foreach (string alignment in alignmentInfoList)
                                {
                                    if (!Enum.TryParse<Globals.eAlignment>(alignment, true, out Globals.eAlignment align))
                                    {
                                        // Log error parsing alignment.
                                    }
                                    else
                                    {
                                        bool containsAlignment = false;

                                        foreach (List<Globals.eAlignment> lists in alignmentList.Values)
                                        {
                                            if (lists.Contains(align))
                                            {
                                                containsAlignment = true;
                                                break;
                                            }
                                        }

                                        if (!containsAlignment)
                                        {
                                            if (alignmentList.ContainsKey(entity))
                                                alignmentList[entity].Add(align);
                                            else alignmentList.Add(entity, new List<Globals.eAlignment>() { align });
                                        }
                                    }
                                }

                                if (alignmentList.Count > 0)
                                {
                                    if (!LootAlignmentRestrictions.ContainsKey(itemID))
                                    {
                                        Dictionary<EntityLists.Entity, List<Globals.eAlignment>> restrictions = new Dictionary<EntityLists.Entity, List<Globals.eAlignment>>
                                        {
                                            { entity, alignmentList[entity] }
                                        };
                                        LootAlignmentRestrictions.Add(itemID, restrictions);
                                    }
                                    else if (LootAlignmentRestrictions[itemID].ContainsKey(entity))
                                    {
                                        LootAlignmentRestrictions[itemID].Add(entity, alignmentList[entity]);
                                    }
                                }
                            }
                            #endregion

                            #region Profession Restrictions
                            if (!string.IsNullOrEmpty(pString))
                            {
                                string[] professionInfoList = pString.Split(" ".ToCharArray());

                                foreach (string profession in professionInfoList)
                                {
                                    Character.ClassType prof;
                                    if (!Enum.TryParse<Character.ClassType>(profession, true, out prof))
                                    {
                                        // Log?
                                    }
                                    else if (professionList.ContainsKey(entity))
                                    {
                                        if (!professionList[entity].Contains(prof))
                                            professionList[entity].Add(prof);
                                    }
                                    else
                                    {
                                        professionList.Add(entity, new List<Character.ClassType> { prof });
                                    }
                                }

                                if (professionList.Count > 0)
                                {
                                    if (!LootProfessionRestrictions.ContainsKey(itemID))
                                    {
                                        Dictionary<EntityLists.Entity, List<Character.ClassType>> restrictions = new Dictionary<EntityLists.Entity, List<Character.ClassType>>
                                        {
                                            { entity, professionList[entity] }
                                        };
                                        LootProfessionRestrictions.Add(itemID, restrictions);
                                    }
                                    else if (!LootProfessionRestrictions[itemID].ContainsKey(entity)) // Item ID already exists in profession restrictions.
                                    {
                                        LootProfessionRestrictions[itemID].Add(entity, professionList[entity]);
                                    }
                                }
                            }
                        }
                        #endregion

                        // Rarity -- If no rarity information the item will not be placed in any LootTables.
                        if (info.Length < 2 || !Enum.TryParse<LootRarityLevel>(info[1], false, out rarityLevel))
                        {
                            Utils.Log(dr["notes"].ToString() + " (Item ID: " + itemID + ") does not meet criteria for RarityLevel. The item will not be found anywhere in the game world.", Utils.LogType.LootWarningRarityLevel);
                        }
                        else
                        {
                            #region Items to be placed in LootTables.
                            // Loot Type (dropped, lair, etc)
                            if (info.Length < 3 || !Enum.TryParse<LootType>(info[2], false, out lootType))
                            {
                                // log warning and continue
                                if (entityList.Contains(EntityLists.Entity.All))
                                    Utils.Log(dr["notes"].ToString() + " (Item ID: " + itemID + ") does not meet criteria for LootType. All entities have a chance to drop this item.", Utils.LogType.LootWarningLootType);
                            }

                            // lootType
                            if(info.Length >= 3)
                            {
                                if (!Enum.TryParse<LootType>(info[2], false, out lootType))
                                    Utils.Log(dr["notes"].ToString() + " (Item ID: " + itemID + ") lootType failed to parse correctly. Will remain default: " + lootType.ToString() + ".", Utils.LogType.LootWarningLootType);
                            }

                            // Land(s) the loot is located in.
                            if (info.Length < 4 || info[3].ToLower() == "all")
                            {
                                // log warning and add all lands and maps (loot will be available in all areas)
                                //if (entityList.Contains(EntityLists.Entity.All))
                                //    Utils.Log(dr["notes"].ToString() + " (Item ID: " + itemID + ") does not meet criteria for Land placement. The item may be found on all entities, in all lands (except Underworld) and in all maps.", Utils.LogType.LootWarningLandPlacement);

                                foreach (Land land in World.GetFacetByIndex(0).Lands)
                                {
                                    if (land.LandID != Land.ID_UNDERWORLD || (info.Length >= 4 && info[3].ToLower() == "all"))
                                    {
                                        landsList.Add(land.ShortDesc);
                                        foreach (Map map in land.Maps)
                                        {
                                            mapsList.Add(map.Name);
                                            foreach(int zKey in map.ZPlanes.Keys)
                                            {
                                                zPlanesList.Add(zKey);
                                            }
                                        }
                                    }
                                }

                                // level requirement remains 0
                            }
                            else
                            {
                                // add all lands
                                foreach (string landShortDesc in info[3].Split("|".ToCharArray()))
                                {
                                    if (!landsList.Contains(landShortDesc))
                                        landsList.Add(landShortDesc);
                                }

                                // Specified maps.
                                if (info.Length >= 5 && info[4].ToLower() != "all")
                                {
                                    List<string> mapNames = new List<string>();

                                    foreach (string mapName in info[4].Split("|".ToCharArray()))
                                    {
                                        if (!mapName.StartsWith("-"))
                                        {
                                            string formattedMapName = Utils.FormatEnumString(mapName);

                                            if (!mapNames.Contains(formattedMapName))
                                                mapNames.Add(formattedMapName);
                                        }
                                    }

                                    foreach (Land land in World.GetFacetByIndex(0).Lands)
                                    {
                                        if (landsList.Contains(land.ShortDesc))
                                        {
                                            foreach (Map map in land.Maps)
                                            {
                                                if (!mapsList.Contains(map.Name) && mapNames.Contains(map.Name))
                                                {
                                                    mapsList.Add(map.Name);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // add all maps
                                    foreach (Land land in World.GetFacetByIndex(0).Lands)
                                    {
                                        if (land.LandID != Land.ID_UNDERWORLD)
                                        {
                                            foreach (Map map in land.Maps)
                                                mapsList.Add(map.Name);
                                        }
                                    }
                                }

                                if (info.Length >= 6 && info[5].ToLower() != "all")
                                {
                                    // Determine if integers.
                                    foreach (string z in info[5].Split("|".ToCharArray()))
                                    {
                                        if (!int.TryParse(z, out int zInt))
                                        {
                                            Utils.Log("Z Plane " + z + " unable to be converted into an integer for " + dr["notes"].ToString() + " info: " + info, Utils.LogType.LootWarning);
                                        }
                                        else if (!zPlanesList.Contains(zInt)) zPlanesList.Add(zInt);
                                    }

                                }
                                else
                                {
                                    // Maps must be loaded by this point. Add every single existing zPlane. A check will be made when giving loot to NPCs.
                                    foreach (Land land in World.GetFacetByIndex(0).Lands)
                                    {
                                        foreach (Map map in land.Maps)
                                        {
                                            foreach (int zCord in map.ZPlanes.Keys)
                                            {
                                                if (!zPlanesList.Contains(zCord))
                                                    zPlanesList.Add(zCord);
                                            }
                                        }
                                    }
                                }

                                if (info.Length >= 7 && info[6].ToLower() != "all")
                                {
                                    if(!int.TryParse(info[6], out int reqLvl))
                                    {
                                        Utils.Log("Loot Table Level Requirement " + info + " unable to be parsed into an integer for " + dr["notes"].ToString() + " info: " + info, Utils.LogType.LootWarning);
                                    }
                                    else levelRequirement = reqLvl;
                                }
                                else levelRequirement = 0; // no level requirement
                            }
                            #endregion
                        }
                        
                        // Final addition to LootInfoDictionary. Will be used in building loot lists.
                        LootInfoDictionary.Add(itemID, Tuple.Create(entityList, rarityLevel, lootType, landsList, mapsList, zPlanesList, levelRequirement));

                        //Utils.Log(dr["notes"].ToString() + " (Item ID: " + itemID + ") Entity Count: " + entityList.Count + " Rarity: " + rarityLevel.ToString() + " LootType: " + lootType.ToString() + " Lands Count: " + landsList.Count + " Maps Count: " + mapsList.Count + " ZPlanes Count: " + zPlanesList.Count, Utils.LogType.LootTable); 
                    }
                }
            }

            // Build Armory and Artifacts here.
            foreach (int itemID in LootInfoDictionary.Keys)
            {
                if (LootInfoDictionary[itemID].Item3 == LootType.Armory)
                {
                    if (!ArmoryInfoDictionary.ContainsKey(itemID))
                        ArmoryInfoDictionary.Add(itemID, LootInfoDictionary[itemID]);
                }

                if (LootInfoDictionary[itemID].Item2 == LootRarityLevel.Lore || LootInfoDictionary[itemID].Item3 == LootType.Artifact)
                {
                    if (!ArtifactsList.Contains(itemID)) ArtifactsList.Add(itemID);
                }
            }

            return true;
        }

        public static LootTable GetLootTable(NPC npc, ZPlane zPlane)
        {
            LootTable loot = new LootTable(npc.npcID) ; // initializes LootTable, sets default values

            if (LootTableExists(npc.npcID, zPlane.name, out loot))
                return loot;

            try
            {
                // Now search through LootInfo for this entity or all. Remember animals do not get "All" loot, they receive loot only if they are listed.

                // LootInfoDictionary
                // Key = unique Item ID, Value = Tuple of List<EntityList.Entity, >, rarity level, loot type, list of Land short descriptions, list of Map names, list of zPlanes integers
                // Tuple Item 1 = Entities
                // Tuple Item 2 = RarityLevel
                // Tuple Item 3 = LootType
                // Tuple Item 4 = List<Land.ShortDescriptions>
                // Tuple Item 5 = List<Map.Names>
                // Tuple Item 6 = List<ZPlane Integers>
                // Tuple Item 7 = Required Level
                foreach (int itemID in LootInfoDictionary.Keys)
                {
                    // Check ALIGNMENT restriction for this item and entity.
                    if (LootAlignmentRestrictions.ContainsKey(itemID) && LootAlignmentRestrictions[itemID].ContainsKey(npc.entity) &&
                        !LootAlignmentRestrictions[itemID][npc.entity].Contains(npc.Alignment))
                        continue;

                    // Check PROFESSION restriction for this item and entity.
                    if (LootProfessionRestrictions.ContainsKey(itemID) && LootProfessionRestrictions[itemID].ContainsKey(npc.entity) &&
                        !LootProfessionRestrictions[itemID][npc.entity].Contains(npc.BaseProfession))
                        continue;

                    // Artifacts do not get added to a regular loot lists.
                    if (ArtifactsList.Contains(itemID))
                    {
                        if (!loot.lootArtifactsList.Contains(itemID)) loot.lootArtifactsList.Add(itemID);
                        continue;
                    }

                    // Tuple used to verify RARITY, LOOT TYPE, LAND, MAP and ZPLANE restrictions.
                    // Tuple Item 1 = Entities
                    // Tuple Item 2 = RarityLevel
                    // Tuple Item 3 = LootType
                    // Tuple Item 4 = List<Land.ShortDescriptions>
                    // Tuple Item 5 = List<Map.Names>
                    // Tuple Item 6 = List<ZPlane Integers>
                    // Tuple Item 7 = Required Level
                    Tuple<List<EntityLists.Entity>, LootRarityLevel, LootType, List<string>, List<string>, List<int>, int> tuple = LootInfoDictionary[itemID];

                    // First verify this loot should be available in the LAND, MAP and ZPLANE.
                    // Or if only one entity has this piece of loot, chances are it goes on them.
                    if ((tuple.Item1.Contains(npc.entity) || tuple.Item1.Contains(EntityLists.Entity.All)) &&
                        tuple.Item4.Contains(zPlane.landShortDesc) && tuple.Item5.Contains(zPlane.mapName) && tuple.Item6.Contains(zPlane.zCord) && npc.Level >= tuple.Item7)
                    {
                        // Add always loot to always list.
                        if (tuple.Item2 == LootRarityLevel.Always && !loot.lootAlwaysList.Contains(itemID)) loot.lootAlwaysList.Add(itemID);

                        // Add always worn item wear location to hasAlwaysWorn list.
                        if(tuple.Item2 == LootRarityLevel.Always && tuple.Item3 == LootType.Worn)
                        {
                            Item item = Item.CopyItemFromDictionary(itemID);

                            if (item != null && !loot.alwaysWorn.Contains(item.wearLocation))
                                loot.alwaysWorn.Add(item.wearLocation);
                        }

                        switch (tuple.Item3)
                        {
                            case LootType.Unique:
                                if(!loot.lootUniqueList.Contains(itemID))
                                    loot.lootUniqueList.Add(itemID);
                                break;
                            case LootType.Dropped:
                                #region Dropped Loot
                                switch (tuple.Item2)
                                {
                                    case LootRarityLevel.Always:
                                        if (!loot.lootAlwaysList.Contains(itemID))
                                            loot.lootAlwaysList.Add(itemID);
                                        break;
                                    case LootRarityLevel.VeryCommon:
                                        if (!loot.lootVeryCommonList.Contains(itemID))
                                            loot.lootVeryCommonList.Add(itemID);
                                        break;
                                    case LootRarityLevel.Common:
                                        if (!loot.lootCommonList.Contains(itemID))
                                            loot.lootCommonList.Add(itemID);
                                        break;
                                    case LootRarityLevel.Uncommon:
                                        if (!loot.lootUncommonList.Contains(itemID))
                                            loot.lootUncommonList.Add(itemID);
                                        break;
                                    case LootRarityLevel.Rare:
                                        if (!loot.lootRareList.Contains(itemID))
                                            loot.lootRareList.Add(itemID);
                                        break;
                                    case LootRarityLevel.VeryRare:
                                        if (!loot.lootVeryRareList.Contains(itemID))
                                            loot.lootVeryRareList.Add(itemID);
                                        break;
                                    case LootRarityLevel.UltraRare:
                                        if (!loot.lootUltraRareList.Contains(itemID))
                                            loot.lootUltraRareList.Add(itemID);
                                        break;
                                }
                                break;
                                #endregion
                            case LootType.HeldLeft:
                                loot.spawnLeftHandList.Clear();
                                loot.spawnLeftHandList.Add(itemID);
                                break;
                            case LootType.HeldRight:
                                loot.spawnRightHandList.Clear();
                                loot.spawnRightHandList.Add(itemID);
                                break;
                            case LootType.HeldDual:
                                loot.spawnLeftHandList.Clear();
                                loot.spawnLeftHandList.Add(itemID);
                                loot.spawnRightHandList.Clear();
                                loot.spawnRightHandList.Add(itemID);
                                break;
                            case LootType.Worn:
                                if (!loot.spawnArmorList.Contains(itemID))
                                {
                                    foreach (int id in loot.spawnArmorList)
                                    {
                                        Item itemInList = Item.CopyItemFromDictionary(id);
                                        Item thisItem = Item.CopyItemFromDictionary(itemID);

                                        // Remove any items already existing in loot.spawnArmorList if they are the same wearLocation as this.
                                        if (itemInList.wearLocation == thisItem.wearLocation)
                                        {
                                            loot.spawnArmorList.Remove(id);
                                            break;
                                        }
                                    }

                                    if (!loot.spawnArmorList.Contains(itemID))
                                        loot.spawnArmorList.Add(itemID);
                                }
                                break;
                            case LootType.Lair:
                                if (npc.lairCritter)
                                {
                                    if (!loot.lootLairList.Contains(itemID))
                                        loot.lootLairList.Add(itemID);
                                }
                                break;
                            case LootType.Tanned:
                                if (EntityLists.ANIMAL.Contains(npc.entity))
                                {
                                    if (npc.tanningResult == null) npc.tanningResult = new Dictionary<int, LootRarityLevel>();
                                    else if (!npc.tanningResult.ContainsKey(itemID))
                                        npc.tanningResult.Add(itemID, tuple.Item2);
                                }

                                // Tanned items are worn by animals & other beings to account for bonuses. They are not added to a corpse when the animal is killed.
                                if (!loot.spawnArmorList.Contains(itemID))
                                    loot.spawnArmorList.Add(itemID);
                                break;
                            case LootType.Belted:
                                if (!EntityLists.ANIMAL.Contains(npc.entity))
                                {
                                    if (!loot.lootBeltList.Contains(itemID))
                                        loot.lootBeltList.Add(itemID);
                                }
                                break;
                        }
                    }
                }

                // If it's an on the fly entity or deserves to wear armor, get an ArmorSet. This is done after initial loot table work for items already in spawnArmorList.
                if (EntityLists.IsHumanOrHumanoid(npc) ||
                    EntityLists.IsGiantKin(npc) ||
                    EntityLists.IsHellspawn(npc) ||
                    (EntityLists.UNDEAD.Contains(npc.entity) && !EntityLists.ANIMAL.Contains(npc.entity)))
                {
                    List<int> armorSet = GetArmorSet(npc, loot.spawnArmorList);
                    if (loot.spawnArmorList == null) loot.spawnArmorList = new List<int>();
                    foreach (int armorID in armorSet)
                        if (!loot.spawnArmorList.Contains(armorID)) loot.spawnArmorList.Add(armorID);

                    // Now right and left hands.
                    if (loot.spawnRightHandList == null) loot.spawnRightHandList = new List<int>();
                    if (loot.spawnRightHandList.Count <= 0) loot.spawnRightHandList.AddRange(GetBasicWeaponsFromArmory(npc, true));

                    if (loot.spawnLeftHandList == null) loot.spawnLeftHandList = new List<int>();
                    if (loot.spawnLeftHandList.Count <= 0) loot.spawnLeftHandList.AddRange(GetBasicWeaponsFromArmory(npc, false));
                }

                // If we made it here the loot table does not exist in the LootTable list.
                NPCLootTablesDictionary.Add(Tuple.Create<int, string>(npc.npcID, zPlane.name), loot);//new Tuple<int, string, LootTable>(npc.npcID, zPlane.name, loot));
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                loot = new LootTable(npc.npcID);
            }

            return loot;
        }

        /// <summary>
        /// Called when creating an NPC in NPC.LoadNPC.
        /// </summary>
        /// <param name="npc">The associated NPC.</param>
        /// <param name="loot">The associated LootTable.</param>
        public static void GiveLootToNPC(NPC npc, LootTable loot)
        {
            if (loot == null)
            {
                Utils.Log("LootTable does not exist for " + npc.GetLogString(), Utils.LogType.LootWarning);
                return;
            }

            Item gold = null;
            int lootNum = 0;

            npc.sackList = new List<Item>();
            npc.pouchList = new List<Item>();

//#if DEBUG
//            Utils.Log("Giving loot to NPC " + npc.GetLogString(), Utils.LogType.SystemTesting);
//#endif

            #region NPC's Items
            try
            {
                #region Always Has Loot -- Done first as to have enough room in the Sack.
                if (loot.lootAlwaysList.Count > 0)
                {
                    foreach (int itemID in loot.lootAlwaysList)
                    {
                        Item item = Item.CopyItemFromDictionary(itemID);
                        Item removedWornItem = null;

                        // If there is an item on the loot table that is always supposed to be worn, make sure the slot is empty.
                        if (item != null && item.itemType == Globals.eItemType.Wearable && loot.alwaysWorn.Contains(item.wearLocation))
                        {
                            foreach (Item wornItem in new List<Item>(npc.wearing))
                            {
                                if (wornItem.wearLocation == item.wearLocation)
                                {
                                    removedWornItem = wornItem;
                                    npc.RemoveWornItem(wornItem);
                                    break;
                                }
                            }

                            if (npc.WearItem(item))
                            {
                                Utils.Log(NPC.GetLogString(npc) + " wears ALWAYS loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootAlways);
                            }
                            else
                            {
                                Utils.Log(npc.GetLogString() + " could not wear ALWAYS item [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootWarning); // different loot warning

                                if (removedWornItem != null) npc.WearItem(removedWornItem); // wear the item we took off in an attempt to wear ALWAYS loot
                            }
                        }
                        else if (loot.spawnRightHandList.Contains(itemID) && !loot.spawnLeftHandList.Contains(itemID))
                        {
                            if (npc.RightHand != null)
                                npc.UnequipRightHand(npc.RightHand);

                            if (item != null && npc.EquipRightHand(item))
                                Utils.Log(NPC.GetLogString(npc) + " wields ALWAYS RIGHT HAND item [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootAlways);
                            else Utils.Log(npc.GetLogString() + " could not wield ALWAYS RIGHT HAND item [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootWarning);
                        }
                        else if (loot.spawnLeftHandList.Contains(itemID) && !loot.spawnRightHandList.Contains(itemID))
                        {
                            if (npc.LeftHand != null)
                                npc.UnequipLeftHand(npc.LeftHand);

                            if (item != null && npc.EquipLeftHand(item))
                                Utils.Log(NPC.GetLogString(npc) + " wields ALWAYS LEFT HAND item [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootAlways);
                            else Utils.Log(npc.GetLogString() + " could not wield ALWAYS LEFT HAND item [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootWarning);
                        }
                        else if (loot.spawnLeftHandList.Contains(itemID) && loot.spawnRightHandList.Contains(itemID))
                        {
                            if (item != null)
                            {
                                if (npc.RightHand != null)
                                    npc.UnequipRightHand(npc.RightHand);
                                if (npc.LeftHand != null)
                                    npc.UnequipLeftHand(npc.LeftHand);

                                npc.EquipRightHand(item);
                                item = Item.CopyItemFromDictionary(itemID);
                                npc.EquipLeftHand(item);
                            }
                        }
                        else if (loot.lootAlwaysList.Contains(itemID) && loot.lootBeltList.Contains(itemID))
                        {
                            if (item != null && npc.BeltItem(item))
                                Utils.Log(NPC.GetLogString(npc) + " has belted ALWAYS loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootAlways);
                            else Utils.Log(npc.GetLogString() + " could not belt ALWAYS loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootWarning);
                        }
                        else if (item != null && npc.PouchItem(item))
                            Utils.Log(NPC.GetLogString(npc) + " has pouched ALWAYS loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootAlways);
                        else if (item != null && npc.SackItem(item))
                            Utils.Log(NPC.GetLogString(npc) + " has sacked ALWAYS loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootAlways);
                        else if (item != null && npc.BeltItem(item))
                            Utils.Log(NPC.GetLogString(npc) + " has belted ALWAYS loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootAlways);
                        else if (npc.tanningResult != null && !npc.tanningResult.ContainsKey(item.itemID) && !EntityLists.ANIMAL.Contains(npc.entity))
                        {
                            Utils.Log(npc.GetLogString() + " could not sack, wield or wear or belt an ALWAYS loot item [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootWarning);
                        }
                    }
                }
                #endregion

                #region Unique Loot -- Uses UltraRare priority.
                if (loot.lootUniqueList.Count > 0)
                {
                    foreach (int itemID in loot.lootUniqueList)
                    {
                        if (GetLootSuccess(LootManager.LootRarityLevel.Unique))
                        {
                            Item item = Item.CopyItemFromDictionary(itemID);

                            if (item != null)
                            {
                                if (npc.WearItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " wears UNIQUE loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootUnique);
                                }
                                else if (npc.PouchItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " pouches UNIQUE loot [" + item.itemID + "] " + item.notes, Utils.LogType.LootUnique);
                                }
                                else if (npc.SackItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " sacks UNIQUE loot [" + item.itemID + "] " + item.notes, Utils.LogType.LootUnique);
                                }
                                else if (!npc.BeltItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " could not wear, pouch, sack or belt UNIQUE loot [" + item.itemID + "] " + item.notes, Utils.LogType.LootWarning);
                                }
                            }
                        }
                    }
                }
                #endregion

                // (Non animals, non wyrms, has hands, not unarmed preferred) OR animals that are supposed to wield weapons.
                if ((!EntityLists.ANIMAL.Contains(npc.entity) && !EntityLists.WYRMKIN.Contains(npc.entity)
                    && !npc.animal && !EntityLists.NO_HANDS.Contains(npc.entity) && !EntityLists.UNARMED_PREFERRED.Contains(npc.entity))
                    || EntityLists.ANIMALS_WIELDING_WEAPONS.Contains(npc.entity)) // This may cause an issue with existing "animals" such as Smokey. 11/21/2015 Eb
                {
                    #region Right Hand
                    if (npc.RightHand == null && loot.spawnRightHandList.Count > 0)
                    {
                        lootNum = Rules.Dice.Next(loot.spawnRightHandList.Count);
                        npc.EquipRightHand(Item.CopyItemFromDictionary(loot.spawnRightHandList[lootNum]));
                    }
                    #endregion

                    #region Left Hand
                    if (loot.spawnLeftHandList.Count > 0 && (npc.RightHand == null || (npc.HasTalent(Talents.GameTalent.TALENTS.DualWield) &&
                        npc.RightHand != null && !npc.RightHand.TwoHandedPreferred())))
                    {
                        lootNum = Rules.Dice.Next(loot.spawnLeftHandList.Count);
                        Item leftHandItem = Item.CopyItemFromDictionary(loot.spawnLeftHandList[lootNum]);
                        int loopCount = 0;

                        while (leftHandItem.TwoHandedPreferred() && loopCount <= loot.spawnLeftHandList.Count)
                        {
                            lootNum = Rules.Dice.Next(loot.spawnLeftHandList.Count);
                            npc.EquipLeftHand(Item.CopyItemFromDictionary(loot.spawnLeftHandList[lootNum]));
                            loopCount++;
                        }

                        if (!leftHandItem.TwoHandedPreferred())
                            npc.EquipLeftHand(leftHandItem);
                    }
                    #endregion

                    #region Shields
                    // Martial artists lose held items below if they don't use Martial Arts skill.
                    if ((npc.IsPureMelee || npc.IsHybrid) && (npc.RightHand != null && !npc.RightHand.TwoHandedPreferred() && npc.LeftHand == null))
                    {
                        if (Rules.RollD(1, 100) >= 50 && Rules.CheckPerception(npc))
                        {
                            if (npc.Level < 8)
                                npc.EquipLeftHand(Item.CopyItemFromDictionary(Item.ID_WOODEN_SHIELD));
                            else if (npc.Level < 17)
                            {
                                if (Rules.RollD(1, 100) <= 25)
                                    npc.EquipLeftHand(Item.CopyItemFromDictionary(Item.ID_WOODEN_SHIELD));
                                else npc.EquipLeftHand(Item.CopyItemFromDictionary(Item.ID_IRON_SHIELD));
                            }
                            else
                            {
                                if (Rules.RollD(1, 100) <= 25)
                                    npc.EquipLeftHand(Item.CopyItemFromDictionary(Item.ID_IRON_SHIELD));
                                else npc.EquipLeftHand(Item.CopyItemFromDictionary(Item.ID_STEEL_KITE_SHIELD));
                            }

                        }
                    }
                    #endregion
                }

                // (Non animals, non wyrms, has hands) OR animals that are supposed to wield weapons.
                if ((!EntityLists.ANIMAL.Contains(npc.entity) && !EntityLists.WYRMKIN.Contains(npc.entity)
                    && !npc.animal && !EntityLists.NO_HANDS.Contains(npc.entity))
                    || EntityLists.ANIMALS_WIELDING_WEAPONS.Contains(npc.entity))
                {
                    #region Belt Loot
                    if (loot.lootBeltList.Count > 0)
                    {
                        // Get random itemID from belt list. 
                        int itemID = loot.lootBeltList[Rules.Dice.Next(loot.lootBeltList.Count)];

                        // LootTable dynamics dicate if there is a rarer item that may be placed on the
                        // belt it should be the only one listed.

                        if (GetLootSuccess(LootInfoDictionary[itemID].Item2))
                        {
                            Utils.LogType logType = Utils.LogType.LootManager;

                            switch (LootInfoDictionary[itemID].Item2)
                            {
                                case LootRarityLevel.Rare:
                                    logType = Utils.LogType.LootBeltRare;
                                    break;
                                case LootRarityLevel.VeryRare:
                                    logType = Utils.LogType.LootBeltVeryRare;
                                    break;
                                case LootRarityLevel.UltraRare:
                                    logType = Utils.LogType.LootBeltUltraRare;
                                    break;
                                default:
                                    break;
                            }

                            Item item = Item.CopyItemFromDictionary(itemID);

                            if (npc.beltList.FindAll(i => i.itemID == item.itemID).Count <= 0)
                            {
                                if (npc.BeltItem(item))
                                    Utils.Log(NPC.GetLogString(npc) + " has belted " + logType.ToString() + " loot [" + item.itemID + "] " + item.notes + ".", logType);
                                else Utils.Log(NPC.GetLogString(npc) + " failed to belt [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootWarning);
                            }
                        }
                    }
                    #endregion
                }

                #region Armor (includes tanned armor)
                if (loot.spawnArmorList != null && loot.spawnArmorList.Count > 0)
                {
                    lootNum = 0;
                    int num = Rules.Dice.Next(1, loot.spawnArmorList.Count);

                    while (lootNum <= num)
                    {
                        int itemNum = Rules.Dice.Next(loot.spawnArmorList.Count);
                        Item wItem = Item.CopyItemFromDictionary(loot.spawnArmorList[itemNum]);
                        if (wItem != null)
                        {
                            if (Globals.Max_Wearable[(int)wItem.wearLocation] > 1)
                            {
                                if (npc.GetInventoryItem(wItem.wearLocation, Globals.eWearOrientation.Right) == null)
                                    wItem.wearOrientation = Globals.eWearOrientation.Right;
                                else if (npc.GetInventoryItem(wItem.wearLocation, Globals.eWearOrientation.Left) == null)
                                    wItem.wearOrientation = Globals.eWearOrientation.Left;
                                else wItem = null;
                            }

                            if (wItem != null)
                            {
                                if (npc.animal || EntityLists.ANIMAL.Contains(npc.entity))
                                {
                                    if(npc.tanningResult == null)
                                        npc.tanningResult = new Dictionary<int, LootRarityLevel>();

                                    if(!npc.tanningResult.ContainsKey(wItem.itemID))
                                        npc.tanningResult.Add(wItem.itemID, LootRarityLevel.Always);
                                }

                                npc.WearItem(wItem);
                            }
                        }
                        lootNum++;
                    }
                }
                #endregion

                #region Lawful knights get a knight ring.
                if (npc.BaseProfession == Character.ClassType.Knight && npc.Alignment == Globals.eAlignment.Lawful)
                {
                    npc.RightRing1 = Item.CopyItemFromDictionary(Item.ID_KNIGHTRING);
                    npc.RightRing1.attuneType = Globals.eAttuneType.None;
                }
                #endregion

                #region Evil ravagers get a ravager ring.
                if (npc.BaseProfession == Character.ClassType.Ravager && npc.Alignment == Globals.eAlignment.Evil)
                {
                    npc.RightRing1 = Item.CopyItemFromDictionary(Item.ID_RAVAGERRING);
                    npc.RightRing1.attuneType = Globals.eAttuneType.None;
                }
                #endregion

                #region Gold
                if (npc.Gold > 0)
                {
                    gold = Item.CopyItemFromDictionary(Item.ID_COINS);
                    gold.coinValue = Rules.Dice.Next(npc.Gold / 2, npc.Gold + 1);
                    if (gold.coinValue <= 1)
                        gold.coinValue = 3;
                    npc.SackItem(gold);
                }
              
                #endregion

                // Lair loot is used when loading lair cells.

                //only non lair critters use very common, common, rare, very rare loot arrays (lair critters use these arrays in Map.loadLairLoot)
                if (!(npc is Merchant) && (!npc.lairCritter || (npc.lairCritter && npc.lairCells.Length <= 0)))
                {
                    // Perhaps this should be removed soon and instead values are set during the LootTable initalization process. -Eb 1/9/2016
                    LootManager.AdjustLootAmounts(npc, loot);

                    #region Very Common Loot
                    if (loot.lootVeryCommonList.Count > 0)
                    {
                        int counter = loot.lootVeryCommonAmount;

                        while (counter > 0)
                        {
                            if (GetLootSuccess(LootManager.LootRarityLevel.VeryCommon))
                            {
                                Item item = Item.CopyItemFromDictionary(loot.lootVeryCommonList[Rules.Dice.Next(loot.lootVeryCommonList.Count)]);

                                if (item != null && npc.PouchItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has pouched VERY COMMON loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootVeryCommon);
                                }
                                else if (item != null && npc.SackItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has sacked VERY COMMON loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootVeryCommon);
                                }
                                else if (item != null && npc.BeltItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has belted VERY COMMON loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootVeryCommon);
                                }
                                else if (item != null)
                                {
                                    Utils.Log(npc.GetLogString() + " FAILED TO pouch, sack or belt VERY COMMON loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootVeryCommon);
                                }
                                else
                                {
                                    Utils.Log(npc.GetLogString() + " FAILED TO HAVE LOOT VERY COMMON ITEM BECAUSE ITEM IS NULL.", Utils.LogType.LootWarning);
                                }
                            }

                            counter--;
                        }
                    }
                    #endregion

                    #region Common Loot
                    if (loot.lootCommonList.Count > 0)
                    {
                        int counter = loot.lootCommonAmount;

                        while (counter > 0)
                        {
                            if (GetLootSuccess(LootManager.LootRarityLevel.Common))
                            {
                                Item item = Item.CopyItemFromDictionary(loot.lootCommonList[Rules.Dice.Next(loot.lootCommonList.Count)]);

                                if (item != null && npc.PouchItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has pouched COMMON loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootCommon);
                                }
                                else if (item != null && npc.SackItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has sacked COMMON loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootCommon);
                                }
                                else if (item != null && npc.BeltItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has belted COMMON loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootCommon);
                                }
                                else if (item != null)
                                {
                                    Utils.Log(npc.GetLogString() + " FAILED TO pouch, sack or belt COMMON loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootCommon);
                                }
                                else
                                {
                                    Utils.Log(npc.GetLogString() + " FAILED TO HAVE LOOT COMMON ITEM BECAUSE ITEM IS NULL.", Utils.LogType.LootWarning);
                                }
                            }

                            counter--;
                        }
                    }
                    #endregion

                    #region Uncommon Loot
                    if (loot.lootUncommonList.Count > 0)
                    {
                        int counter = loot.lootUncommonAmount;

                        while (counter > 0)
                        {
                            if (GetLootSuccess(LootManager.LootRarityLevel.Uncommon))
                            {
                                Item item = Item.CopyItemFromDictionary(loot.lootUncommonList[Rules.Dice.Next(loot.lootUncommonList.Count)]);

                                if (item != null && npc.PouchItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has pouched UNCOMMON loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootUncommon);
                                }
                                else if (item != null && npc.SackItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has sacked UNCOMMON loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootUncommon);
                                }
                                else if (item != null && npc.BeltItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has belted UNCOMMON loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootUncommon);
                                }
                                else if (item != null)
                                {
                                    Utils.Log(npc.GetLogString() + " FAILED TO pouch, sack or belt UNCOMMON loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootUncommon);
                                }
                                else
                                {
                                    Utils.Log(npc.GetLogString() + " FAILED TO HAVE LOOT UNCOMMON ITEM BECAUSE ITEM IS NULL.", Utils.LogType.LootWarning);
                                }
                            }

                            counter--;
                        }
                    }
                    #endregion

                    #region Rare Loot
                    if (loot.lootRareList.Count > 0)
                    {
                        int counter = loot.lootRareAmount;

                        while (counter > 0)
                        {
                            if (GetLootSuccess(LootManager.LootRarityLevel.Rare))
                            {
                                Item item = Item.CopyItemFromDictionary(loot.lootRareList[Rules.Dice.Next(loot.lootRareList.Count)]);

                                if (item != null && npc.PouchItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has pouched RARE loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootRare);
                                }
                                else if (item != null && npc.SackItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has sacked RARE loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootRare);
                                }
                                else if (item != null && npc.BeltItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has belted RARE loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootRare);
                                }
                                else if (item != null)
                                {
                                    Utils.Log(npc.GetLogString() + " FAILED TO pouch, sack or belt RARE loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootRare);
                                }
                                else
                                {
                                    Utils.Log(npc.GetLogString() + " FAILED TO HAVE LOOT RARE ITEM BECAUSE ITEM IS NULL.", Utils.LogType.LootWarning);
                                }
                            }

                            counter--;
                        }
                    }
                    #endregion

                    #region Very Rare Loot
                    if (loot.lootVeryRareList.Count > 0)
                    {
                        int counter = loot.lootVeryRareAmount;

                        while (counter > 0)
                        {
                            if (GetLootSuccess(LootManager.LootRarityLevel.VeryRare))
                            {
                                Item item = Item.CopyItemFromDictionary(loot.lootVeryRareList[Rules.Dice.Next(loot.lootVeryRareList.Count)]);

                                if (item != null && npc.PouchItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has pouched VERY RARE loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootVeryRare);
                                }
                                else if (item != null && npc.SackItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has sacked VERY RARE loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootVeryRare);
                                }
                                else if (item != null && npc.BeltItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has belted VERY RARE loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootVeryRare);
                                }
                                else if(item != null)
                                {
                                    Utils.Log(npc.GetLogString() + " FAILED TO pouch, sack and belt VERY RARE loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootVeryRare);
                                }
                                else
                                {
                                    Utils.Log(npc.GetLogString() + " FAILED TO HAVE VERY RARE LOOT ITEM BECAUSE ITEM IS NULL.", Utils.LogType.LootWarning);
                                }
                            }

                            counter--;
                        }
                    }
                        #endregion

                    #region Ultra Rare Loot
                    if (loot.lootUltraRareList.Count > 0)
                    {
                        int counter = loot.lootUltraRareAmount;

                        while (counter > 0)
                        {
                            if (GetLootSuccess(LootManager.LootRarityLevel.UltraRare))
                            {
                                Item item = Item.CopyItemFromDictionary(loot.lootUltraRareList[Rules.Dice.Next(loot.lootUltraRareList.Count)]);

                                if (item != null && npc.PouchItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has pouched ULTRA RARE loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootUltraRare);
                                }
                                else if (item != null && npc.SackItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has sacked ULTRA RARE loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootUltraRare);
                                }
                                else if (item != null && npc.BeltItem(item))
                                {
                                    Utils.Log(NPC.GetLogString(npc) + " has belted ULTRA RARE loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootUltraRare);
                                }
                                else if (item != null)
                                {
                                    Utils.Log(npc.GetLogString() + " FAILED TO pouch, sack and belt ULTRA RARE loot [" + item.itemID + "] " + item.notes + ".", Utils.LogType.LootUltraRare);
                                }
                                else
                                {
                                    Utils.Log(npc.GetLogString() + " FAILED TO HAVE ULTRA RARE LOOT ITEM BECAUSE ITEM IS NULL.", Utils.LogType.LootWarning);
                                }

                            }

                            counter--;
                        }
                    }
                    #endregion

                    #region Scrolls
                    if (EntityLists.IsHumanOrHumanoid(npc) && (npc.IsSpellWarmingProfession || (!npc.IsSpellWarmingProfession && Rules.RollD(1, 100) <= ScrollManager.HUMANOID_NONSPELLWARMER_SCROLL_CHANCE))
                        && Rules.RollD(1, 100) <= ScrollManager.HUMANOID_SCROLL_CHANCE)
                    {
                        var scroll = ScrollManager.CreateUnavailableSpellScroll(npc);

                        if (scroll != null) npc.SackItem(scroll);
                    }
                    #endregion

                    #region Wands
                    //if (EntityLists.IsHumanOrHumanoid(npc) && (npc.IsSpellWarmingProfession || (!npc.IsSpellWarmingProfession && Rules.RollD(1, 100) <= WandManager.HUMANOID_NONSPELLWARMER_WAND_CHANCE))
                    //    && Rules.RollD(1, 100) <= WandManager.HUMANOID_WAND_CHANCE)
                    //{
                    //    var wand;

                    //    if (Rules.RollD(1, 100) <= 50)
                    //        wand = WandManager.CreateSpellWand(npc);
                    //    else wand = WandManager.CreateDormantWand(npc);

                    //    if (wand != null) npc.SackItem(wand);
                    //}
                    #endregion

                    #region Balms for Unique Human/Humanoid, non undead
                    if(EntityLists.IsHumanOrHumanoid(npc.entity) &&
                        !EntityLists.UNDEAD.Contains(npc.entity) &&
                        (EntityLists.UNIQUE.Contains(npc.entity) || Rules.CheckPerception(npc)) && Rules.RollD(1, 100) > 50)
                    {
                        int balmCount = Rules.Dice.Next(0, 2);

                        while(balmCount > 0 && npc.SackCountMinusGold < Character.MAX_SACK)
                        {
                            Item balm = Item.CopyItemFromDictionary(Item.ID_BALM);
                            npc.SackItem(balm);

                            balmCount--;
                        }
                    }
                    #endregion

                    #region Verify martial artists are not running around with weapons.
                    if (npc.BaseProfession == Character.ClassType.Martial_Artist)
                    {
                        if (npc.RightHand != null && npc.RightHand.skillType != Globals.eSkillType.Unarmed && Skills.GetSkillLevel(npc.unarmed) > Skills.GetSkillLevel(npc, npc.RightHand.skillType))
                            if (npc.BeltItem(npc.RightHand)) npc.UnequipRightHand(npc.RightHand);

                        if (npc.LeftHand != null && npc.LeftHand.skillType != Globals.eSkillType.Unarmed && Skills.GetSkillLevel(npc.unarmed) > Skills.GetSkillLevel(npc, npc.LeftHand.skillType))
                            if (npc.BeltItem(npc.LeftHand)) npc.UnequipLeftHand(npc.LeftHand);
                    } 
                    #endregion
                }
            }
            catch (Exception e)
            {
                Utils.Log(npc.GetLogString() + " has an issue with their LootTable in LootManager.GiveLootToNPC", Utils.LogType.LootWarning);
                Utils.LogException(e);
            }
            #endregion
        }

        /// <summary>
        /// Set gold coinValue for Items in a List.
        /// </summary>
        /// <param name="list">The List to iterate through such as the sackList, beltList, or worn inventory.</param>
        public static void SetRandomCoinValues(List<Item> list)
        {
            foreach (Item item in list)
            {
                if (item.vRandLow > 0)
                    item.coinValue = Rules.Dice.Next(item.vRandLow, item.vRandHigh);
            }
        }

        public static void SetRandomCoinValue(Item item)
        {
            if (item.vRandLow > 0)
                item.coinValue = Rules.Dice.Next(item.vRandLow, item.vRandHigh);
        }

        public static void AdjustLootAmounts(NPC npc, LootTable loot)
        {
            int amount = 1;

            if (EntityLists.UNIQUE.Contains(npc.entity))
                amount++;

            if (EntityLists.SUPERIOR_HEALTH.Contains(npc.entity) || EntityLists.SUPERIOR_MANA.Contains(npc.entity))
                amount++;

            loot.lootVeryCommonAmount = amount;
            if (npc.lairCritter && npc.lairCellsList.Count > 0)
                loot.lootVeryCommonAmount += Rules.RollD(2, 4);
            loot.lootCommonAmount = amount;
            if (npc.lairCritter && npc.lairCellsList.Count > 0)
                loot.lootCommonAmount += Rules.RollD(1, 6);
            loot.lootUncommonAmount = amount;
            if (npc.lairCritter && npc.lairCellsList.Count > 0)
                loot.lootUncommonAmount += Rules.RollD(1, 4);

            loot.lootRareAmount = npc.Level < LEVEL_REQ_RARE ? 0: 1;
            loot.lootVeryRareAmount = npc.Level < LEVEL_REQ_VERYRARE ? 0 : 1;
            loot.lootUltraRareAmount = npc.Level < LEVEL_REQ_ULTRARARE ? 0 : 1;
        }

        public static void LoadLairLoot(NPC lairNPC)
        {
            var critterLair = new ArrayList(lairNPC.lairCellsList);

            // Fire effect in every lair cell.
            if (critterLair.Count > 0)
            {
                var effect = new AreaEffect(Effect.EffectTypes.Fire, Cell.GRAPHIC_FIRE, lairNPC.Level * 5, 1, lairNPC, critterLair);
            }

            LootTable loot = GetLootTable(lairNPC, lairNPC.Map.ZPlanes[lairNPC.Z]);

            AdjustLootAmounts(lairNPC, loot); // different every time

            // Log which Lair Loot has already been placed. Limit each lair item to one drop per lair refresh. Check rolls for chance of spawn.
            List<int> LairLootPlaced = new List<int>();
            // Log which Always Loot has already been placed. Limit each always item to one drop per lair refresh.
            List<int> LairAlwaysLootPlaced = new List<int>();
            
            foreach (Cell lairCell in critterLair)
            {
                lairCell.Items.Clear();

                Item gold = Item.CopyItemFromDictionary(Item.ID_COINS);
                gold.coinValue = Rules.Dice.Next((int)lairNPC.Gold, lairNPC.Gold * 2);
                lairCell.Add(gold);

                #region Very Common Loot
                if (loot.lootVeryCommonList.Count > 0)
                {
                    int counter = loot.lootVeryCommonAmount;

                    while (counter > 0)
                    {
                        if (GetLootSuccess(LootManager.LootRarityLevel.VeryCommon))
                        {
                            Item item = Item.CopyItemFromDictionary(loot.lootVeryCommonList[Rules.Dice.Next(loot.lootVeryCommonList.Count)]);
                            lairCell.Add(item);
                        }

                        counter--;
                    }
                }
                #endregion

                #region Common Loot
                if (loot.lootCommonList.Count > 0)
                {
                    int counter = loot.lootCommonAmount;

                    while (counter > 0)
                    {
                        if (GetLootSuccess(LootManager.LootRarityLevel.Common))
                        {
                            Item item = Item.CopyItemFromDictionary(loot.lootCommonList[Rules.Dice.Next(loot.lootCommonList.Count)]);
                            lairCell.Add(item);
                        }

                        counter--;
                    }
                }
                #endregion

                #region Uncommon Loot
                if (loot.lootUncommonList.Count > 0)
                {
                    int counter = loot.lootUncommonAmount;

                    while (counter > 0)
                    {
                        if (GetLootSuccess(LootManager.LootRarityLevel.Uncommon))
                        {
                            Item item = Item.CopyItemFromDictionary(loot.lootUncommonList[Rules.Dice.Next(loot.lootUncommonList.Count)]);
                            lairCell.Add(item);
                        }

                        counter--;
                    }
                }
                #endregion

                #region Add more very common, common, and uncommon gems for shiny lovers (wyrmkin && humanoids).
                if (EntityLists.WYRMKIN.Contains(lairNPC.entity) || EntityLists.IsHumanOrHumanoid(lairNPC))
                {
                    foreach (Item item in new List<Item>(lairCell.Items))
                    {
                        if (item.baseType == Globals.eItemBaseType.Gem && Rules.RollD(1, 100) >= (50 - lairNPC.Level))
                        {
                            lairCell.Add(Item.CopyItemFromDictionary(item.itemID));
                        }
                    }
                } 
                #endregion

                #region Rare Loot
                if (loot.lootRareList.Count > 0)
                {
                    int counter = loot.lootRareAmount;

                    while (counter > 0)
                    {
                        if (GetLootSuccess(LootManager.LootRarityLevel.Rare))
                        {
                            Item item = Item.CopyItemFromDictionary(loot.lootRareList[Rules.Dice.Next(loot.lootRareList.Count)]);
                            lairCell.Add(item);
                        }

                        counter--;
                    }
                }
                #endregion

                #region Very Rare Loot
                if (loot.lootVeryRareList.Count > 0)
                {
                    int counter = loot.lootVeryRareAmount;

                    while (counter > 0)
                    {
                        if (GetLootSuccess(LootManager.LootRarityLevel.VeryRare))
                        {
                            Item item = Item.CopyItemFromDictionary(loot.lootVeryRareList[Rules.Dice.Next(loot.lootVeryRareList.Count)]);
                            lairCell.Add(item);
                        }

                        counter--;
                    }
                }
                #endregion

                #region Ultra Rare Loot
                if (loot.lootUltraRareList.Count > 0)
                {
                    int counter = loot.lootUltraRareAmount;

                    while (counter > 0)
                    {
                        if (GetLootSuccess(LootManager.LootRarityLevel.UltraRare))
                        {
                            Item item = Item.CopyItemFromDictionary(loot.lootUltraRareList[Rules.Dice.Next(loot.lootUltraRareList.Count)]);
                            lairCell.Add(item);
                        }

                        counter--;
                    }
                }
                #endregion

                #region Lair Loot
                if (loot.lootLairList.Count > 0)
                {
                    foreach (int itemID in loot.lootLairList)
                    {
                        if (LairLootPlaced.Contains(itemID))
                            continue;
                        else if (GetLootSuccess(LootInfoDictionary[itemID].Item2))
                        {
                            Item item = Item.CopyItemFromDictionary(itemID);
                            lairCell.Add(item);
                            LairLootPlaced.Add(itemID);
                        }
                    }
                } 
                #endregion

                #region Scrolls
                if (Rules.RollD(1, 100) <= ScrollManager.LAIR_LOOT_SCROLL_CHANCE)
                {
                    Item scroll = ScrollManager.CreateUnavailableSpellScroll(lairNPC);
                    if (scroll != null) lairCell.Items.Add(scroll);
                } 
                #endregion
            }
        }

        public static int GetLootCount(bool specificInfo)
        {
            if (!specificInfo) return LootItemsDictionary.Count;
            else return LootInfoDictionary.Count;
        }

        /// <summary>
        /// Find an approriate armor set for the NPC.
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>
        public static List<int> GetArmorSet(NPC npc, List<int> spawnArmorList)
        {
            var armorList = new List<int>();

            if(EntityLists.ANIMAL.Contains(npc.entity))
                return armorList;

            try
            {

                if (npc.Level < 8)
                {
                    armorList = ArmorSet.ArmorSetDictionary[ArmorSet.FULL_LEATHER].GetArmorList(npc);
                }
                else
                {
                    switch (npc.BaseProfession)
                    {
                        case Character.ClassType.Martial_Artist:
                            armorList = ArmorSet.ArmorSetDictionary[ArmorSet.BASIC_LEATHER].GetArmorList(npc);
                            break;
                        case Character.ClassType.Berserker:
                        case Character.ClassType.Fighter:
                        case Character.ClassType.Ravager:
                        case Character.ClassType.Knight:

                            {
                                int roll = Rules.RollD(1, 3);
                                if (npc.Level > 17 || EntityLists.UNIQUE.Contains(npc.entity)) roll = 4;
                                switch (roll)
                                {
                                    case 0:
                                    case 1:
                                        armorList = ArmorSet.ArmorSetDictionary[ArmorSet.FULL_CHAINMAIL].GetArmorList(npc);
                                        break;
                                    case 2:
                                        armorList = ArmorSet.ArmorSetDictionary[ArmorSet.FULL_SCALEMAIL].GetArmorList(npc);
                                        break;
                                    case 3:
                                        armorList = ArmorSet.ArmorSetDictionary[ArmorSet.FULL_BANDED_MAIL].GetArmorList(npc);
                                        break;
                                    default:
                                        armorList = ArmorSet.ArmorSetDictionary[ArmorSet.FULL_STEEL].GetArmorList(npc);
                                        break;
                                }
                                break;
                            }
                        case Character.ClassType.Druid:
                        case Character.ClassType.Sorcerer:
                        case Character.ClassType.Thief:
                        case Character.ClassType.Wizard:
                            {
                                int roll = Rules.RollD(1, 100);

                                if (npc.BaseProfession == Character.ClassType.Thief) roll += 20;

                                if (roll <= 65)
                                    armorList = ArmorSet.ArmorSetDictionary[ArmorSet.FULL_LEATHER].GetArmorList(npc);
                                else armorList = ArmorSet.ArmorSetDictionary[ArmorSet.FULL_STUDDED_LEATHER].GetArmorList(npc);
                            }
                            break;
                        case Character.ClassType.Ranger:
                        case Character.ClassType.Thaumaturge:
                            {
                                int roll = Rules.RollD(1, 3);
                                if (npc.Level > 17) roll++;
                                switch (roll)
                                {
                                    case 1:
                                        armorList = ArmorSet.ArmorSetDictionary[ArmorSet.FULL_STUDDED_LEATHER].GetArmorList(npc);
                                        break;
                                    case 2:
                                        armorList = ArmorSet.ArmorSetDictionary[ArmorSet.FULL_CHAINMAIL].GetArmorList(npc);
                                        break;
                                    default:
                                        armorList = ArmorSet.ArmorSetDictionary[ArmorSet.FULL_SCALEMAIL].GetArmorList(npc);
                                        break;
                                }
                            }
                            break;
                    }
                }

                if (npc.IsSpellUser && npc.Level >= 8 && Rules.RollD(1, 100) >= (50 - (npc.Level * 2)))
                {
                    if (EntityLists.LAWFUL_ALIGNMENT.Contains(npc.entity))
                        armorList.Add(Item.ID_SILVER_ROBE_BLACK_LION);
                    else armorList.Add(Item.ID_BLACK_ROBE_RED_RUNES);
                }

                // NPC has special armor it spawns with.
                if (spawnArmorList != null && spawnArmorList.Count > 0)
                {
                    foreach (int existingID in spawnArmorList)
                    {
                        Item inList = Item.CopyItemFromDictionary(existingID);

                        foreach (int id in new List<int>(armorList))
                        {
                            Item nowList = Item.CopyItemFromDictionary(id);

                            if (nowList.wearLocation == inList.wearLocation)
                            {
                                armorList.Remove(id);
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }

            return armorList;
        }

        /// <summary>
        /// Get weapons to be wielded in right or left hands.
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="rightHand"></param>
        /// <returns></returns>
        public static List<int> GetBasicWeaponsFromArmory(NPC npc, bool rightHand)
        {
            // Make sure marital artists receive proper weaponry, or none for unarmed combat.
            bool includePlusOne = npc.Level >= 5;
            bool includePlusTwo = npc.Level >= 9;
            bool includePlusThree = npc.Level >= 13;
            bool includePlusFour = npc.Level >= 16; // None of these in the armory yet. 3/11/2016
            bool includePlusFive = npc.Level >= 18; // None of these in the armory yet. 3/11/2016

            List<int> wieldedWeaponList = new List<int>();

            if (rightHand)
            {
                foreach (int id in ArmoryInfoDictionary.Keys)
                {
                    Item item = Item.CopyItemFromDictionary(id);

                    if (item.itemType != Globals.eItemType.Weapon || item.itemType == Globals.eItemType.Wearable || item.baseType == Globals.eItemBaseType.Shield)
                        continue;

                    // Do not give non "Unarmed" skill weapons to a martial artist.
                    if (npc.BaseProfession == Character.ClassType.Martial_Artist && item.skillType != Globals.eSkillType.Unarmed) continue;

                    // No shields in right hand.
                    if (item.baseType == Globals.eItemBaseType.Shield) continue;

                    // Don't give thieves anything but bows? This doesn't seem right. 3/13/2016 Eb
                    // Changed to "pierce" weapons. This is so they don't hesistate to cast venom in combat. 6/5/2019 Eb
                    if (npc.BaseProfession == Character.ClassType.Thief && !item.special.ToLower().Contains("pierce"))
                        continue;

                    // No bows in their right hand.
                    if (npc.BaseProfession != Character.ClassType.Thief && item.baseType == Globals.eItemBaseType.Bow)
                    {
                        if(!EntityLists.ARCHERY_PREFERRED.Contains(npc.entity))
                            continue;

                        if (npc.entity == EntityLists.Entity.Arbalist && !item.name.ToLower().Contains("crossbow"))
                            continue;
                    }

                    switch (item.combatAdds)
                    {
                        case 1:
                            if (!includePlusOne) continue;
                            break;
                        case 2:
                            if (!includePlusTwo) continue;
                            break;
                        case 3:
                            if (!includePlusThree) continue;
                            break;
                        case 4:
                            if (!includePlusFour) continue;
                            break;
                        case 5:
                            if (!includePlusFive) continue;
                            break;
                    }

                    wieldedWeaponList.Add(item.itemID);
                }
            }
            else if (npc.RightHand == null || !npc.RightHand.TwoHandedPreferred() && npc.HasTalent(Talents.GameTalent.TALENTS.DualWield))
            {
                foreach (int id in ArmoryInfoDictionary.Keys)
                {
                    Item item = Item.CopyItemFromDictionary(id);

                    if (item.itemType != Globals.eItemType.Weapon || item.itemType == Globals.eItemType.Wearable || item.baseType == Globals.eItemBaseType.Shield)
                        continue;

                    // Do not give non "Unarmed" skill weapons to a martial artist.
                    if (npc.BaseProfession == Character.ClassType.Martial_Artist && item.skillType != Globals.eSkillType.Unarmed) continue;

                    // No bows for the left hand.
                    if (item.baseType == Globals.eItemBaseType.Bow) continue;

                    // No two handed weapons in left hand.
                    if (item.TwoHandedPreferred()) continue;

                    switch (item.combatAdds)
                    {
                        case 1:
                            if (!includePlusOne) continue;
                            break;
                        case 2:
                            if (!includePlusTwo) continue;
                            break;
                        case 3:
                            if (!includePlusThree) continue;
                            break;
                        case 4:
                            if (!includePlusFour) continue;
                            break;
                        case 5:
                            if (!includePlusFive) continue;
                            break;
                    }

                    wieldedWeaponList.Add(item.itemID);
                }
            }
            else if (npc.RightHand != null && !npc.RightHand.TwoHandedPreferred() && (npc.IsHybrid || npc.BaseProfession == Character.ClassType.Fighter))
            {
                foreach (int id in ArmoryInfoDictionary.Keys)
                {
                    Item item = Item.CopyItemFromDictionary(id);

                    if (item.itemType != Globals.eItemType.Weapon || item.itemType == Globals.eItemType.Wearable) continue;

                    // No bows for the left hand.
                    if (item.baseType == Globals.eItemBaseType.Bow) continue;

                    if (item.baseType == Globals.eItemBaseType.Shield && Rules.RollD(1, 100) + npc.Level >= 30)
                    {
                        switch (item.combatAdds)
                        {
                            case 1:
                                if (!includePlusOne) continue;
                                break;
                            case 2:
                                if (!includePlusTwo) continue;
                                break;
                            case 3:
                                if (!includePlusThree) continue;
                                break;
                            case 4:
                                if (!includePlusFour) continue;
                                break;
                            case 5:
                                if (!includePlusFive) continue;
                                break;
                        }

                        wieldedWeaponList.Add(item.itemID);
                    }
                }
            }

            return wieldedWeaponList;
        }

        public static bool SpawnArtifacts(out int artifactsCount)
        {
            artifactsCount = 0;

            foreach (Facet facet in World.Facets)
            {
                foreach (Land land in facet.Lands)
                {
                    foreach (Map map in land.Maps)
                    {
                        foreach (ZPlane zPlane in map.ZPlanes.Values)
                        {
                            if (zPlane.zAutonomy != null && zPlane.zAutonomy.artifacts != null && zPlane.zAutonomy.artifacts.Count > 0)
                            {
                                foreach (ZArtifact artifact in zPlane.zAutonomy.artifacts)
                                {
                                    Item item = Item.CopyItemFromDictionary(artifact.itemID);

                                    if (item == null)
                                    {
                                        Utils.Log("Failed to find Item ID " + artifact.itemID + " while spawning artifacts.", Utils.LogType.SystemWarning);
                                        continue;
                                    }

                                    Cell cell = Cell.GetCell(facet.FacetID, land.LandID, map.MapID, artifact.xCord, artifact.yCord, zPlane.zCord);

                                    if (cell == null)
                                    {
                                        Utils.Log("Failed to find cell to place artifact: " + item.notes + " while spawning artifacts.", Utils.LogType.SystemWarning);
                                        continue;
                                    }

                                    cell.Add(item);
                                    artifactsCount++;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        public static bool GetLootSuccess(LootRarityLevel rarity)
        {
            //Never, // not possible to EVER be found as loot (DEV items?)
            //Always, // 100%
            //VeryCommon, // 80%
            //Common, // 55%
            //Uncommon, // 30%
            //Rare, // 15%
            //VeryRare, // 10%
            //UltraRare // 5%

            int modifier = DragonsSpineMain.Instance.Settings.LootPercentageModifier;

            switch (rarity)
            {
                case LootRarityLevel.Never:
                    return false;
                case LootRarityLevel.Always:
                    return true;
                case LootRarityLevel.VeryCommon:
                    if(Rules.RollD(1, 100) <= PCT_VERYCOMMON + modifier) return true;
                    break;
                case LootRarityLevel.Common:
                    if (Rules.RollD(1, 100) <= PCT_COMMON + modifier) return true;
                    break;
                case LootRarityLevel.Uncommon:
                    if (Rules.RollD(1, 100) <= PCT_UNCOMMON + modifier) return true;
                    break;
                case LootRarityLevel.Rare:
                    if (Rules.RollD(1, 100) <= PCT_RARE + modifier) return true;
                    break;
                case LootRarityLevel.VeryRare:
                    if (Rules.RollD(1, 100) <= PCT_VERYRARE + modifier) return true;
                    break;
                case LootRarityLevel.UltraRare:
                    if (Rules.RollD(1, 100) <= PCT_ULTRARARE + modifier) return true;
                    break;
                case LootRarityLevel.Unique:
                    if (Rules.RollD(1, 100) <= PCT_UNIQUE + modifier) return true;
                    break;
                default:
                    break;
            }

            return false;
        }
    }
}
