using System;
using System.Linq;
using System.Collections.Generic;
using DragonsSpine.GameWorld;

namespace DragonsSpine.Autonomy.EntityBuilding
{
    public static class EntityCreationManager
    {
        public static string NPC_INSERT_NOTES_DEFAULT = "Default NPC";

        /// <summary>
        /// Holds the dictionary of NPCs created from Z Plane autonomy information. Key = Tuple(Entity, profession/synonym, npcID), Value = NPC.
        /// </summary>
        public static Dictionary<Tuple<EntityLists.Entity, string, int>, NPC> AutoCreatedNPCDictionary = new Dictionary<Tuple<EntityLists.Entity, string, int>, NPC>();

        /// <summary>
        /// Temporary storage of newly created 
        /// </summary>
        public static Dictionary<int, int> GroupAmounts = new Dictionary<int, int>();

        /// <summary>
        /// Check AutoCreatedNPCDictionary for an npcID. Confirms NPC was created autonomously.
        /// </summary>
        /// <param name="npcID"></param>
        /// <returns></returns>
        public static bool ContainsNPCID(int npcID)
        {
            foreach (Tuple<EntityLists.Entity, string, int> tuple in AutoCreatedNPCDictionary.Keys)
            {
                if (tuple.Item3 == npcID) return true;
            }

            return false;
        }

        public static NPC CopyAutoCreatedNPC(int npcID)
        {
            foreach (Tuple<EntityLists.Entity, string, int> tuple in AutoCreatedNPCDictionary.Keys)
            {
                if (tuple.Item3 == npcID)
                {
                    return AutoCreatedNPCDictionary[tuple].CloneNPC();
                }
            }

            return null;
        }

        public static int GetAutoCreatedNPCID(EntityLists.Entity entity)
        {
            foreach (Tuple<EntityLists.Entity, string, int> tuple in AutoCreatedNPCDictionary.Keys)
            {
                if (tuple.Item1 == entity)
                {
                    return AutoCreatedNPCDictionary[tuple].npcID;
                }
            }

            return -1;
        }

        public static int GetAutoCreatedNPCID(string identifier)
        {
            EntityLists.Entity entity = EntityLists.Entity.None;

            if (!Enum.TryParse<EntityLists.Entity>(identifier, true, out entity))
            {
                foreach (Tuple<EntityLists.Entity, string, int> tuple in AutoCreatedNPCDictionary.Keys)
                {
                    if (tuple.Item2 == identifier)
                    {
                        return AutoCreatedNPCDictionary[tuple].npcID;
                    }
                }
            }
            else return GetAutoCreatedNPCID(entity);            

            return -1;
        }
        
        public static bool BuildNewEntities(out int spawnZonesCount)
        {
            spawnZonesCount = 0;

            foreach (Facet facet in World.Facets){
                foreach (Land land in facet.Lands){
                    foreach (Map map in land.Maps){                        
                        foreach (ZPlane zPlane in map.ZPlanes.Values)
                        {
                            try
                            {
                                if (zPlane.zAutonomy != null && zPlane.zAutonomy.entities != null && zPlane.zAutonomy.entities.Length > 0)
                                {
                                    // name(class|class2),name,name,name(class|class|class)
                                    string[] critters = zPlane.zAutonomy.entities.Split(",".ToCharArray());

                                    List<Tuple<string, EntityLists.Entity, string>> creatureDescProfessionTuples = new List<Tuple<string, EntityLists.Entity, string>>();

                                    #region Create list of Tuples with EntityLists.Entity and profession or profession synonym string.
                                    foreach (string s in critters)
                                    {
                                        EntityLists.Entity cr = EntityLists.Entity.Alligator;

                                        string desc = "";

                                        // adjective(s) for descriptive, and other purposes
                                        if (s.StartsWith("["))
                                        {
                                            desc = s.Substring(s.IndexOf("[") + 1, s.IndexOf("]") - s.IndexOf("[") - 1);
                                        }

                                        string critterType = "";

                                        if (s.Contains("(")) // various professions
                                        {
                                            if (s.StartsWith("["))
                                            {
                                                critterType = s.Substring(s.IndexOf("]") + 1, s.IndexOf("(") - s.IndexOf("]") - 1);
                                            }
                                            else critterType = s.Substring(0, s.IndexOf("("));
                                            
                                            if (!Enum.TryParse(critterType, true, out cr))
                                            {
                                                Utils.Log("Error parsing CreatureLists.Creature (" + critterType + ") from zPlane: " + zPlane.ToString(), Utils.LogType.SystemWarning);
                                                continue;
                                            }

                                            string[] classesList = s.Substring(s.IndexOf("(") + 1, s.IndexOf(")") - s.IndexOf("(") - 1).Split("|".ToCharArray());

                                            foreach (string cl in classesList)
                                            {
                                                Tuple<string, EntityLists.Entity, string> tuple = Tuple.Create(desc, cr, cl);

                                                if (!creatureDescProfessionTuples.Contains(tuple))
                                                    creatureDescProfessionTuples.Add(tuple);
                                            }
                                        }
                                        else
                                        {
                                            if (s.StartsWith("["))
                                            {
                                                critterType = s.Substring(s.IndexOf("]") + 1, s.Length - s.IndexOf("]") - 1);
                                            }
                                            else critterType = s;

                                            if (!Enum.TryParse(critterType, true, out cr))
                                            {
                                                Utils.Log("Error parsing CreatureLists.Creature ( " + critterType + " ) from zPlane: " + zPlane.ToString(), Utils.LogType.SystemWarning);
                                                continue;
                                            }

                                            Tuple<string, EntityLists.Entity, string> tuple = Tuple.Create(desc, cr, "Fighter");

                                            if (!creatureDescProfessionTuples.Contains(tuple))
                                                creatureDescProfessionTuples.Add(tuple);
                                        }
                                    }
                                    #endregion

                                    NPC npc = null;
                                    List<int> NPCIDList = new List<int>();
                                    List<int> WaterDwellingNPCIDList = new List<int>();
                                    Dictionary<int, Tuple<int, int>> SpawnGroupAmounts = new Dictionary<int, Tuple<int, int>>();
                                    Dictionary<int, Tuple<int, int>> WaterSpawnGroupAmounts = new Dictionary<int, Tuple<int, int>>();

                                    foreach (Tuple<string, EntityLists.Entity, string> tuple in creatureDescProfessionTuples)
                                    {
                                        npc = null;

                                        #region Check if an entity/profession, in the same level range, has already been built. If so, choose it for the SpawnZone.
                                        foreach (Tuple<EntityLists.Entity, string, int> createdNPC in AutoCreatedNPCDictionary.Keys)
                                        {
                                            // if an Entity/profession pair has already been created, check level and use if within range.
                                            if (createdNPC.Item1 == tuple.Item2 && createdNPC.Item2 == tuple.Item3 && AutoCreatedNPCDictionary[createdNPC].shortDesc.Contains(tuple.Item1))
                                            {
                                                if (AutoCreatedNPCDictionary[createdNPC].Level >= zPlane.zAutonomy.minimumSuggestedLevel &&
                                                    AutoCreatedNPCDictionary[createdNPC].Level <= zPlane.zAutonomy.maximumSuggestedLevel)
                                                {
                                                    npc = AutoCreatedNPCDictionary[createdNPC].CloneNPC();
                                                    break;
                                                }
                                            }
                                        }
                                        #endregion

                                        if (npc == null)
                                        {
                                            if (EntityLists.IsMerchant(tuple.Item2))
                                            {
                                                npc = new Merchant();

                                                if (EntityLists.TRAINER_SPELLS.Contains(tuple.Item2))
                                                    (npc as Merchant).trainerType = Merchant.TrainerType.Spell;
                                                else if (EntityLists.TRAINER_ANIMAL.Contains(tuple.Item2))
                                                    (npc as Merchant).trainerType = Merchant.TrainerType.Animal;

                                                if (EntityLists.MENTOR.Contains(tuple.Item2))
                                                    (npc as Merchant).interactiveType = Merchant.InteractiveType.Mentor;

                                                if (!EntityLists.MOBILE.Contains(tuple.Item2))
                                                    npc.IsMobile = false;
                                            }
                                            else
                                            {
                                                npc = new NPC();
                                            }

                                            EntityBuilder builder = new EntityBuilder();

                                            if (!builder.BuildEntity(tuple.Item1, tuple.Item2, npc, zPlane, tuple.Item3))
                                            {
                                                // Log an issue with building a new entity, move on to next.
                                                Utils.Log("Failed to build entity: " + tuple.Item1.ToString() + ", " + tuple.Item2.ToString() + " ZPlane: " + zPlane.ToString(), Utils.LogType.SystemWarning);
                                                continue;
                                            }
                                            else
                                            {
                                                // Built a new entity (NPC), add it to the dictionary.
                                                AutoCreatedNPCDictionary.Add(Tuple.Create(tuple.Item2, tuple.Item3, npc.npcID), npc);
                                            }
                                        }

                                        if (npc != null)
                                        {
                                            if (!NPCIDList.Contains(npc.npcID))
                                            {
                                                if (!npc.IsWaterDweller)
                                                    NPCIDList.Add(npc.npcID);
                                                else WaterDwellingNPCIDList.Add(npc.npcID);

                                                // Currently limiting social groups of spell warming professions to humans only.
                                                // Let's say other humanoid casters don't work well together as there is too much of a power struggle.
                                                // Maybe in the future there will be exceptions to this rule.
                                                if (EntityLists.SOCIAL.Contains(tuple.Item2) && ((npc.IsSpellWarmingProfession && EntityLists.HUMAN.Contains(tuple.Item2)) || !npc.IsSpellWarmingProfession)
                                                    && !SpawnGroupAmounts.ContainsKey(npc.npcID))
                                                {
                                                    int low = 2;
                                                    int high = 4;

                                                    // Animals typically have more members in a group.
                                                    if (EntityLists.ANIMAL.Contains(tuple.Item2)) high += Rules.Dice.Next(0, Rules.RollD(1, 2));

                                                    // Humanoids and humans have a chance to be alone.
                                                    if (EntityLists.IsHumanOrHumanoid(npc) && Rules.RollD(1, 100) < 15)
                                                    {
                                                        low = 1;
                                                        high = 2;
                                                    }

                                                    if (!npc.IsWaterDweller)
                                                        SpawnGroupAmounts.Add(npc.npcID, Tuple.Create(low, high));
                                                    else WaterSpawnGroupAmounts.Add(npc.npcID, Tuple.Create(low, high));
                                                }
                                            }
                                            else
                                            {
                                                Utils.Log("NPC ID already exists in SpawnZone NPCIDList in BuildNewEntities(). NPC: " + npc.GetLogString() + " ZPlane: " + zPlane.ToString(), Utils.LogType.SystemWarning);
                                            }
                                        }
                                    }

                                    if (NPCIDList.Count > 0)
                                    {
                                        SpawnZone szl = new SpawnZone(facet, NPCIDList[0], NPCIDList, SpawnGroupAmounts, land.LandID, map.MapID, zPlane.zCord, false);
                                        facet.Add(szl);
                                        if (!SpawnZone.Spawns.ContainsKey(szl.ZoneID))
                                            SpawnZone.Spawns.Add(szl.ZoneID, szl);
                                        spawnZonesCount++;
                                    }
                                    else if (WaterDwellingNPCIDList.Count > 0)
                                    {
                                        SpawnZone szl = new SpawnZone(facet, WaterDwellingNPCIDList[0], NPCIDList, SpawnGroupAmounts, land.LandID, map.MapID, zPlane.zCord, true);
                                        facet.Add(szl);
                                        if(!SpawnZone.Spawns.ContainsKey(szl.ZoneID))
                                            SpawnZone.Spawns.Add(szl.ZoneID, szl);
                                        spawnZonesCount++;
                                    }
                                }

                                if (zPlane.zAutonomy != null && zPlane.zAutonomy.uniqueEntities.Count > 0)
                                {
                                    EntityBuilder builder = new EntityBuilder();

                                    foreach (ZUniqueEntity zUnique in zPlane.zAutonomy.uniqueEntities)
                                    {
                                        NPC npc;

                                        if (EntityLists.IsMerchant(zUnique.entity))
                                        {
                                            npc = new Merchant();

                                            if (EntityLists.TRAINER_SPELLS.Contains(zUnique.entity))
                                                (npc as Merchant).trainerType = Merchant.TrainerType.Spell;
                                            else if (EntityLists.TRAINER_ANIMAL.Contains(zUnique.entity))
                                                (npc as Merchant).trainerType = Merchant.TrainerType.Animal;

                                            if (EntityLists.MENTOR.Contains(zUnique.entity))
                                                (npc as Merchant).interactiveType = Merchant.InteractiveType.Mentor;

                                            if (!EntityLists.MOBILE.Contains(zUnique.entity))
                                                npc.IsMobile = false;
                                        }
                                        else
                                        {
                                            npc = new NPC();
                                        }

                                        npc.lairCritter = zUnique.hasLair;
                                        npc.lairCells = zUnique.lairCells;

                                        if (!builder.BuildEntity(zUnique.description, zUnique.entity, npc, zPlane, zUnique.profession))
                                        {
                                            // Log an issue with building a new unique entity, move on to next.
                                            Utils.Log("Failed to build unique entity: " + zUnique.description + ", " + zUnique.entity + " ZPlane: " + zPlane.ToString(), Utils.LogType.SystemWarning);
                                            continue;
                                        }
                                        else
                                        {
                                            // Built a new unique entity (NPC), add it to the dictionary.
                                            AutoCreatedNPCDictionary.Add(Tuple.Create(zUnique.entity, zUnique.profession, npc.npcID), npc);
                                        }

                                        // add spawn zone
                                        SpawnZone szl = new SpawnZone(facet, zUnique, npc, land.LandID, map.MapID, zPlane.zCord, npc.IsWaterDweller);
                                        facet.Add(szl);
                                        if (!SpawnZone.Spawns.ContainsKey(szl.ZoneID))
                                            SpawnZone.Spawns.Add(szl.ZoneID, szl);
                                        spawnZonesCount++;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Utils.Log("EntityCreationManager.BuildNewEntities issue with map:" + map.Name + " zPlane:" + zPlane.name + ". Check autonomous tags and info.", Utils.LogType.ExceptionDetail);
                                Utils.LogException(e);
                            }
                        }
                    }
                }
            }

            return true;
        }

        public static int GetNextAvailableNPCID()
        {
                List<int> idNumbers = new List<int>(NPC.NPCDictionary.Keys);
                idNumbers.Sort();

                int[] values = Enumerable.Range(idNumbers[0], idNumbers[idNumbers.Count - 1] - idNumbers[0]).ToArray();

                foreach (int num in values)
                    if (!idNumbers.Contains(num)) return num;

            return idNumbers[idNumbers.Count - 1] + 1;
        }
    }
}
