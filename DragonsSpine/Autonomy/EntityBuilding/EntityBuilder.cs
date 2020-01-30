/**
 * Written by Michael Cohen (ebonyofold) in 2014.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Entity = DragonsSpine.Autonomy.EntityBuilding.EntityLists.Entity;
using ZPlane = DragonsSpine.GameWorld.ZPlane;
using GameSpell = DragonsSpine.Spells.GameSpell;

namespace DragonsSpine.Autonomy.EntityBuilding
{
    public class EntityBuilder
    {
        // TODO: Create an enum and expand abilities based on synonyms. Will look at the .classFullName (current label) variable in Character.cs.
        // This seems like a good means to creating subclasses. 12/5/2015 Eb

        #region Profession Synonyms
        public static string[] FIGHTER_SYNONYMS = new string[]
        { "bruiser", "champion", "fighter", "guard", "hero", "soldier", "trooper", "warrior" };

        public static string[] BERSERKER_SYNONYMS = new string[]
        { "fury", "hysteric", "maniac", "wildling"};

        public static string[] DRUID_SYNONYMS = new string[] { "druid", "druidess", "forester" };

        public static string[] KNIGHT_SYNONYMS = new string[]
        { "cavalier", "crusader", "paladin" };

        // Note druid/druidess should be moved to DRUID_SYNONYMS when/if druids are introduced.
        public static string[] THAUMATURGE_SYNONYMS = new string[]
        { "acolyte", "augur", "cleric", "cultist", "diabolist", "fiendish cleric", "mystic", "shaman", "templar", "thaumaturge" };

        public static string[] WIZARD_SYNONYMS = new string[]
        { "apprentice", "archmage", "mage", "magus", "psimage", "seer", "soothsayer", "wizard" };

        public static string[] MARTIALARTIST_SYNONYMS = new string[]
        { "brawler", "martialartist", "martial_artist", "monk", "ninja", "pugilist", "slugger" };

        // Note if bards are ever implemented some of these synonyms should be moved to BARD_SYNONYMS.
        public static string[] THIEF_SYNONYMS = new string[]
        { "assassin", "bard", "cutthroat", "dirge", "minstrel", "mugger", "rogue", "scout", "thief" };

        public static string[] SORCERER_SYNONYMS = new string[]
        { "anathema", "necromancer", "sorcerer", "warlock", "witch" };

        public static string[] RANGER_SYNONYMS = new string[]
        { "ranger", "warden", "woodsman" };

        public static string[] RAVAGER_SYNONYMS = new string[]
        { "hellknight", "plaguelord", "ravager", "shadowknight" };
        #endregion

        private int CreateUniqueNPCID()
        {
            for (int a = 40000; a < 80000; a++)
            {
                if (!NPC.NPCDictionary.ContainsKey(a) && !EntityCreationManager.ContainsNPCID(a))
                {
                    return a;
                }
            }

            return -1;
        }

        private int DetermineAge(NPC npc)
        {
            // Children.
            if (npc.Name.ToLower().Contains("child"))
                return GameWorld.World.AgeCycles[0];

            if (EntityLists.IsHumanOrHumanoid(npc))
                return GameWorld.World.AgeCycles[Rules.Dice.Next(0, 2)];

            switch (npc.entity)
            {
                case Entity.Broodmother:
                case Entity.Leng_Red_Dragon:
                case Entity.Oakvael_Wind_Dragon:
                case Entity.Annwn_Red_Dragon:
                case Entity.Axe_Glacier_Blue_Dragon:
                case Entity.Titan:
                case Entity.Rift_Glacier_Cloud_Dragon:
                    return GameWorld.World.AgeCycles[GameWorld.World.AgeCycles.Count - 1]; // ancient
                default:
                    if (EntityLists.WYRMKIN.Contains(npc.entity))
                        return GameWorld.World.AgeCycles[Rules.Dice.Next(0, 3)];
                    else return GameWorld.World.AgeCycles[Rules.Dice.Next(0, 2)];
            }
        }

        public static Globals.eSpecies DetermineSpecies(Entity entity)
        {
            if (entity.ToString().ToLower().Contains("vampire") || EntityLists.UNDEAD.Contains(entity))
                return Globals.eSpecies.Unnatural;

            if (EntityLists.HUMAN.Contains(entity) && !EntityLists.ELVES.Contains(entity))
                return Globals.eSpecies.Human;

            if (EntityLists.ELVES.Contains(entity) && !EntityLists.HUMAN.Contains(entity))
                return Globals.eSpecies.Elvish;

            switch (entity)
            {
                case Entity.Annwn_Red_Dragon:
                case Entity.Kesmai_Red_Dragon:
                case Entity.Leng_Red_Dragon:
                case Entity.Red_Dragon:
                    return Globals.eSpecies.FireDragon;
                case Entity.Blue_Dragon:
                case Entity.Axe_Glacier_Blue_Dragon:
                    return Globals.eSpecies.IceDragon;
                case Entity.Axe_Glacier_Lightning_Drake:
                case Entity.Leng_Lightning_Drake:
                case Entity.Rift_Glacier_Lightning_Drake:
                case Entity.Drake:
                case Entity.Wandering_Lightning_Drake:
                    return Globals.eSpecies.LightningDrake;
                case Entity.Rift_Glacier_Cloud_Dragon:
                    return Globals.eSpecies.CloudDragon;
                case Entity.Titan:
                    return Globals.eSpecies.Titan;
                default:
                    break;
            }

            if (EntityLists.PLANT.Contains(entity)) return Globals.eSpecies.Plant;

            if (EntityLists.UNDEAD.Contains(entity)) return Globals.eSpecies.Unnatural;

            return Globals.eSpecies.Unknown;
        }

        public static NPC.CastMode DetermineCastMode(NPC npc)
        {
            if (EntityLists.IsFullBloodedWyrmKin(npc)) return NPC.CastMode.Unlimited;

            if (npc.IsHybrid) return NPC.CastMode.NoPrep;

            if (EntityLists.CASTMODE_UNLIMITED.Contains(npc.entity)) return NPC.CastMode.Unlimited;

            if (EntityLists.CASTMODE_NOPREP.Contains(npc.entity)) return NPC.CastMode.NoPrep;

            return NPC.CastMode.Limited;
        }

        private Character.ClassType DetermineProfession(Entity entity, string profession)
        {
            if (EntityLists.UNIQUE.Contains(entity) && (profession.ToLower() == "fighter" || profession == ""))
                return DetermineProfession(entity);

            Character.ClassType classType = Character.ClassType.Fighter;

            if (!Enum.TryParse(entity.ToString(), true, out classType))
            {
                if (!FIGHTER_SYNONYMS.Contains(profession.ToLower()))
                {
                    if (!Enum.TryParse(profession, true, out classType))
                    {
                        if (THAUMATURGE_SYNONYMS.Contains(profession.ToLower()))
                            classType = Character.ClassType.Thaumaturge;
                        else if (DRUID_SYNONYMS.Contains(profession.ToLower()))
                            classType = Character.ClassType.Druid;
                        else if (WIZARD_SYNONYMS.Contains(profession.ToLower()))
                            classType = Character.ClassType.Wizard;
                        else if (MARTIALARTIST_SYNONYMS.Contains(profession.ToLower()))
                            classType = Character.ClassType.Martial_Artist;
                        else if (THIEF_SYNONYMS.Contains(profession.ToLower()))
                            classType = Character.ClassType.Thief;
                        else if (RANGER_SYNONYMS.Contains(profession.ToLower()))
                            classType = Character.ClassType.Ranger;
                        else if (RAVAGER_SYNONYMS.Contains(profession.ToLower()))
                            classType = Character.ClassType.Ravager;
                        else if (SORCERER_SYNONYMS.Contains(profession.ToLower()))
                            classType = Character.ClassType.Sorcerer;
                        else if (KNIGHT_SYNONYMS.Contains(profession.ToLower()))
                            classType = Character.ClassType.Knight;
                        else if (BERSERKER_SYNONYMS.Contains(profession.ToLower()))
                            classType = Character.ClassType.Berserker;
                    }
                }
            }

            // failsafe to fighter
            if (classType == Character.ClassType.None ||
                classType == Character.ClassType.All) classType = Character.ClassType.Fighter;

            return classType;
        }

        private Character.ClassType DetermineProfession(Entity entity)
        {
            switch (entity)
            {
                case Entity.Animal_Trainer:
                    if (Rules.Dice.Next(1, 100) >= 50)
                        return Character.ClassType.Ranger;
                    else return Character.ClassType.Druid;
                // Berserkers
                // Rangers
                case Entity.Rhed:
                    return Character.ClassType.Ranger;
                // Druids
                case Entity.Laurelena:
                    return Character.ClassType.Druid;
                // Wizards
                case Entity.Archmage:
                case Entity.Overlord:
                    return Character.ClassType.Wizard;
                // Thieves
                case Entity.Alia:
                case Entity.Drow_Master:
                case Entity.Carfel:
                    return Character.ClassType.Thief;
                // Martial Artists
                case Entity.Great_Schema:
                case Entity.Succubus_Prima:
                    return Character.ClassType.Martial_Artist;
                // Knights
                case Entity.High_Elf_Cdr:
                    return Character.ClassType.Knight;
                // Fighters
                case Entity.Rift_Glacier_Cloud_Dragon:
                    return Character.ClassType.Fighter;
                // Sorcerers
                case Entity.Cyprial:
                case Entity.Sea_Hag: // Sartila is her name
                    return Character.ClassType.Sorcerer;
                // Thaumaturges
                case Entity.Drow_Matriarch:
                case Entity.Drow_Priestess:
                    return Character.ClassType.Thaumaturge;
            }

            return Character.ClassType.Fighter;
        }

        private int DetermineLevel(Entity entity, ZPlane zPlane)
        {
            // Override for small animals and weak entities (children).
            if (EntityLists.ANIMAL_SMALL.Contains(entity) || EntityLists.WEAK.Contains(entity))
                return Rules.Dice.Next(3, 7);

            int min = 0;
            int max = 0;

            if (zPlane.zAutonomy != null)
            {
                min = zPlane.zAutonomy.minimumSuggestedLevel;
                max = zPlane.zAutonomy.maximumSuggestedLevel;
            }

            if (min < 1) min = 10 + Rules.RollD(2, 3);
            if (max < 1) max = 18 + Rules.RollD(1, 3);

            int level = Rules.Dice.Next(min, max + 1);

            if (EntityLists.IsMerchant(entity))
                level = max;

            if (EntityLists.UNIQUE.Contains(entity) && !EntityLists.IsMerchant(entity))
                level += Rules.RollD(1, 3);

            if (EntityLists.SUPERIOR_HEALTH.Contains(entity))
                level += Rules.RollD(1, 2);

            return level;
        }

        private int DetermineGold(NPC npc)
        {
            if(npc.lairCritter)
            {
                return npc.Level * Rules.Dice.Next(805, 1000);
            }
            else if (EntityLists.UNIQUE.Contains(npc.entity) && !EntityLists.ANIMAL.Contains(npc.entity))
            {
                return npc.Level * Rules.Dice.Next(505, 705);
            }
            else if (!EntityLists.ANIMAL.Contains(npc.entity))
            {
                return npc.Level * Rules.Dice.Next(55, 205);
            }
            else return 0;
        }

        public static long DetermineExperienceValue(NPC npc)
        {
            int uBound = 1000;
            int lBound = 700;

            if (npc is Adventurer)
            {
                uBound += 500;
                lBound += 350;
            }

            if(EntityLists.UNIQUE.Contains(npc.entity))
            {
                uBound += 300;
                lBound += 200;
            }

            if (EntityLists.HARD_HITTERS.Contains(npc.entity) || EntityLists.SUPERIOR_HEALTH.Contains(npc.entity) ||
                EntityLists.SUPERIOR_MANA.Contains(npc.entity))
            {
                uBound += 300;
                lBound += 200;
            }

            if (EntityLists.ANIMAL_SMALL.Contains(npc.entity) || EntityLists.WEAK.Contains(npc.entity))
            {
                uBound -= 200;
                lBound -= 100;
            }

            if(npc.talentsDictionary.Count > 0)
            {
                uBound += 50 * npc.talentsDictionary.Count;
                lBound += 50 * npc.talentsDictionary.Count;
            }

            if (lBound > uBound) lBound = uBound - 1;

            return npc.Level * Rules.Dice.Next(lBound, uBound);
        }

        private string FormatEntityName(NPC npc)
        {
            string name = npc.entity.ToString();

            try
            {
                if (!EntityLists.CAPITALIZED.Contains(npc.entity))
                    name = name.ToLower();

                name = name.Replace("___", "'-");
                name = name.Replace("__", "-");
                name = name.Replace("_", ".");

                // temporary? 12/5/2015 Eb
                if (EntityLists.UNIQUE.Contains(npc.entity) && !EntityLists.CAPITALIZED.Contains(npc.entity) && EntityLists.WYRMKIN.Contains(npc.entity) && name.Contains("."))
                    name = name.Substring(name.LastIndexOf("."));

                // If name is too long chances are the profession synonym may be removed from the end.
                if (name.Length > GameSystems.Text.NameGenerator.NAME_MAX_LENGTH && name.Contains("."))
                {
                    if(EntityLists.IsFullBloodedWyrmKin(npc) && name.Contains("."))
                        name = name.Split(".".ToCharArray())[1];
                    else name = name.Substring(0, name.LastIndexOf("."));
                }

                // Not sure why I added this. Just a precautionary measure. 12/5/2015 Eb
                if (name.EndsWith("."))
                    name = name.Substring(0, name.LastIndexOf(".") + 1);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }

            return name;
        }

        /// <summary>
        /// This version of BuildEntity is called from the implementor create NPC command.
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="entity"></param>
        /// <param name="zPlane"></param>
        /// <param name="profession"></param>
        /// <returns></returns>
        public NPC BuildEntity(string desc, Entity entity, ZPlane zPlane, string profession)
        {
            NPC npc;

            if (EntityLists.IsMerchant(entity))
            {
                npc = new Merchant();
                npc.npcID = CreateUniqueNPCID();

                if (EntityLists.TRAINER_SPELLS.Contains(entity))
                    (npc as Merchant).trainerType = Merchant.TrainerType.Spell;
                else if (EntityLists.TRAINER_ANIMAL.Contains(entity))
                    (npc as Merchant).trainerType = Merchant.TrainerType.Animal;

                if (EntityLists.MENTOR.Contains(entity))
                    (npc as Merchant).interactiveType = Merchant.InteractiveType.Mentor;

                if(!EntityLists.MOBILE.Contains(entity))
                    npc.IsMobile = false;
            }
            else
            {
                npc = new NPC
                {
                    npcID = CreateUniqueNPCID()
                };
            }

            // results in an error if invalid npcID
            if (npc.npcID < 0) return null;

            npc.BaseProfession = DetermineProfession(entity, profession);

            npc.classFullName = profession.ToUpperInvariant();//(Utils.FormatEnumString(npc.BaseProfession.ToString())

            if (npc.BaseProfession != Character.ClassType.Fighter && npc.classFullName.ToLower() == "fighter")
                npc.classFullName = npc.BaseProfession.ToString();

            // no upper case letters, capitalize classFullName
            if (!npc.classFullName.Any(c => char.IsUpper(c)))
                npc.classFullName = char.ToUpper(profession[0]) + profession.Substring(1);

            npc.Level = DetermineLevel(entity, zPlane);
            npc.aiType = NPC.AIType.Unknown;
            npc.waterDweller = EntityLists.WATER_DWELLER.Contains(entity);

            npc.entity = entity;
            // if false then the NPC doesn't exist in the database, so all variables must be set via code decisions.
            if (!SetCommonVariables(npc, zPlane, out System.Data.DataRow selDataRow) || npc.lairCritter)
            {
                // bloodhulk, yaun-ti, hellhound, unique lair critters
                SquareOne(desc, npc, zPlane, profession);
            }
            else
            {
                SquareTwo(desc, npc, entity, zPlane, profession);
            }

            npc.Notes = Utils.FormatEnumString(entity.ToString()) + " " + Utils.FormatEnumString(npc.BaseProfession.ToString()) + " (" + npc.longDesc + ")";

            SetHitsStaminaMana(npc);
            SetRegeneration(npc);
            SetTalents(npc);

            npc.Gold = DetermineGold(npc);
            npc.Experience = DetermineExperienceValue(npc);

            SetSkillLevels(npc);
            SetBaseArmorClassAndTHAC0Adjustment(npc);

            npc.animal = EntityLists.ANIMAL.Contains(entity);
            npc.IsUndead = EntityLists.UNDEAD.Contains(entity);
            npc.IsSpectral = EntityLists.INCORPOREAL.Contains(entity);
            npc.poisonous = EntityLists.POISONOUS.Contains(entity) ? (short)(npc.Level * 2) : (short)0;

            // lairs are created via the map files under the <autonomy> node

            // plants, statues, merchants and then check those forced to be mobile
            npc.IsMobile = !EntityLists.PLANT.Contains(entity) &&
                entity != Entity.Statue &&
                !EntityLists.MERCHANTS.Contains(entity) &&
                !EntityLists.MOBILE.Contains(entity);

            #region No means of creating patrollers via autonomy at this time. 1/18/2014
            //this.patrolRoute = dr["patrolRoute"].ToString();
            //if (this.patrolRoute.Length > 0)
            //{
            //    this.patrolKeys = new List<string>();
            //    string[] route = this.patrolRoute.Split(" ".ToCharArray());
            //    foreach (string nrt in route)
            //    {
            //        this.patrolKeys.Add(nrt);
            //    }
            //}        
            #endregion

            // Some summoning spells set spell casting variables.
            if (npc.spellDictionary == null || npc.spellDictionary.Count <= 0)
                SetSpellCastingVariables(npc, selDataRow);
            else GameSpell.FillSpellLists(npc);

            #region TODO: Quests
            //if (dr["quests"].ToString().Length > 0)
            //{
            //    NPC.FillQuestList(this, dr["quests"].ToString());
            //}

            // group amount set up in AutonomyManager.cs

            //this.WeaponRequirement = dr["weaponRequirement"].ToString();

            //// questID~flag^questID~flag, where if questID <= 0 then the quest started is not required to get the flag
            //if (dr["questFlags"].ToString().Length > 1)
            //{
            //    string[] s = dr["questFlags"].ToString().Split(Protocol.ISPLIT.ToCharArray());

            //    for (int a = 0; a < s.Length; a++)
            //    {
            //        this.QuestFlags.Add(s[a]);
            //    }
            //} 
            #endregion

            npc.Hits = npc.HitsFull;
            npc.Mana = npc.ManaFull;
            npc.Stamina = npc.StaminaFull;

            SetMemorizedSpell(npc);

            return npc;
        }

        /// <summary>
        /// BuildEntity is called when a new entity is created from scratch, dependent upon variables in Map (map text files) zPlane data.
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="entity"></param>
        /// <param name="npc"></param>
        /// <param name="zPlane"></param>
        /// <param name="profession"></param>
        /// <returns></returns>
        public bool BuildEntity(string desc, Entity entity, NPC npc, ZPlane zPlane, string profession)
        {
            npc.npcID = CreateUniqueNPCID();

            // results in an error if invalid npcID
            if (npc.npcID < 0) return false;

            npc.BaseProfession = DetermineProfession(entity, profession);

            npc.classFullName = Utils.FormatEnumString(npc.BaseProfession.ToString());

            npc.Level = DetermineLevel(entity, zPlane);

            npc.aiType = NPC.AIType.Unknown;

            npc.waterDweller = EntityLists.WATER_DWELLER.Contains(entity);

            npc.entity = entity;

            // if false then the NPC doesn't exist in the database, so all variables must be set via code decisions.
            if (!SetCommonVariables(npc, zPlane, out System.Data.DataRow selDataRow) || npc.lairCritter)
            {
                // bloodhulk, yaun-ti, hellhound, unique lair critters
                SquareOne(desc, npc, zPlane, profession);
            }
            else
            {
                SquareTwo(desc, npc, entity, zPlane, profession);
            }

            npc.Notes = Utils.FormatEnumString(entity.ToString()) + " " + Utils.FormatEnumString(npc.BaseProfession.ToString()) + " (" + npc.longDesc + ")";

            //ItemBuilding.LootManager.CreateNewLootTable(npc, zPlane);

            SetHitsStaminaMana(npc);
            SetRegeneration(npc);
            SetTalents(npc);

            npc.Gold = DetermineGold(npc);
            npc.Experience = DetermineExperienceValue(npc);

            SetSkillLevels(npc);
            SetBaseArmorClassAndTHAC0Adjustment(npc);

            npc.animal = EntityLists.ANIMAL.Contains(entity);
            npc.IsUndead = EntityLists.UNDEAD.Contains(entity);
            npc.IsSpectral = EntityLists.INCORPOREAL.Contains(entity);
            npc.poisonous = EntityLists.POISONOUS.Contains(entity) ? (short)(npc.Level * 2) : (short)0;

            // plants, statues, merchants and then check those forced to be mobile
            npc.IsMobile = !EntityLists.PLANT.Contains(entity) &&
                entity != Entity.Statue &&
                !EntityLists.MERCHANTS.Contains(entity) &&
                !EntityLists.MOBILE.Contains(entity);

            #region No means of creating patrollers via autonomy at this time. 1/18/2014
            //this.patrolRoute = dr["patrolRoute"].ToString();
            //if (this.patrolRoute.Length > 0)
            //{
            //    this.patrolKeys = new List<string>();
            //    string[] route = this.patrolRoute.Split(" ".ToCharArray());
            //    foreach (string nrt in route)
            //    {
            //        this.patrolKeys.Add(nrt);
            //    }
            //}        
            #endregion

            // For some summoning spells the spell casting variables are already set.
            if (npc.spellDictionary == null || npc.spellDictionary.Count <= 0)
                SetSpellCastingVariables(npc, selDataRow);
            else GameSpell.FillSpellLists(npc);

            #region TODO: Quests
            //if (dr["quests"].ToString().Length > 0)
            //{
            //    NPC.FillQuestList(this, dr["quests"].ToString());
            //}

            // group amount set up in AutonomyManager.cs

            //this.WeaponRequirement = dr["weaponRequirement"].ToString();

            //// questID~flag^questID~flag, where if questID <= 0 then the quest started is not required to get the flag
            //if (dr["questFlags"].ToString().Length > 1)
            //{
            //    string[] s = dr["questFlags"].ToString().Split(Protocol.ISPLIT.ToCharArray());

            //    for (int a = 0; a < s.Length; a++)
            //    {
            //        this.QuestFlags.Add(s[a]);
            //    }
            //} 
            #endregion

            npc.Hits = npc.HitsFull;
            npc.Mana = npc.ManaFull;
            npc.Stamina = npc.StaminaFull;

            SetMemorizedSpell(npc);

            return true;
        }

        /// <summary>
        /// Back to square one. This is called for an NPC (Entity) that is being built from scratch because no information was garnered from the database.
        /// </summary>
        /// <param name="desc">A long description assigned to the new entity.</param>
        /// <param name="npc">The actual NPC being built for cloning later.</param>
        /// <param name="zPlane">The ZPlane this autonomous creation was first established in.</param>
        /// <param name="profession">The NPC's profession or profession synonym.</param>
        private void SquareOne(string desc, NPC npc, ZPlane zPlane, string profession)
        {
            if ((EntityLists.HUMAN.Contains(npc.entity) || EntityLists.ELVES.Contains(npc.entity)) && Enum.IsDefined(typeof(Character.ClassType), npc.entity.ToString()))
                npc.race = GameWorld.World.homelands[Rules.Dice.Next(0, GameWorld.World.homelands.Length - 1)];
            else if (EntityLists.HUMAN.Contains(npc.entity) || EntityLists.ELVES.Contains(npc.entity))
            {
                npc.race = zPlane.mapName;
            }
            else npc.race = "";

            SetGender(npc, zPlane);
            SetName(npc, profession);
            SetVisualKey(npc.entity, npc);
            SetSounds(npc.entity, npc);

            npc.MoveString = "";

            npc.species = EntityBuilder.DetermineSpecies(npc.entity);
            npc.special = "";

            //npc.castMode = EntityBuilder.DetermineCastMode(npc);

            SetAlignment(npc);

            SetSpeed(npc);

            SetAbilityScores(npc);
            SetStatAdds(npc);

            //if (selctdDataRow["tanningResult"].ToString().Length > 0) npc.tanningResult = Utils.ConvertStringToIntArray(selctdDataRow["tanningResult"].ToString());

            npc.IsInvisible = EntityLists.INVISIBLE.Contains(npc.entity);

            if (EntityLists.HIDDEN.Contains(npc.entity))
                npc.AddPermanentEffect(Effect.EffectTypes.Hide_in_Shadows);

            if (EntityLists.FLYING.Contains(npc.entity))
                npc.AddPermanentEffect(Effect.EffectTypes.Flight);

            if (EntityLists.FLAMESHIELDED.Contains(npc.entity))
                npc.AddPermanentEffect(Effect.EffectTypes.Flame_Shield);

            npc.baseArmorClass = 10;
            npc.THAC0Adjustment = 0;

            SetAttackAndBlockStrings(npc);

            SetImmunities(npc);

            npc.canCommand = false;

            // The base professions are also EntityList.Entity values. If they can be parsed into a class type then the NPC will have a random name.            

            npc.Age = DetermineAge(npc);

            SetAlignment(npc);
            SetDescriptions(desc, npc, zPlane, profession);
        }

        /// <summary>
        /// The name of this method is a joke. It's simply all the methods called is SquareOne isn't called when building an entity.
        /// Usually this would be called if most data for a new entity came from an existing NPC in the database.
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="entity"></param>
        /// <param name="zPlane"></param>
        /// <param name="profession"></param>
        private void SquareTwo(string desc, NPC npc, Entity entity, ZPlane zPlane, string profession)
        {
            SetGender(npc, zPlane);
            SetName(npc, profession);
            SetVisualKey(entity, npc);
            SetDescriptions(desc, npc, zPlane, profession);
            SetAttackAndBlockStrings(npc);
            SetAlignment(npc);
            npc.species = DetermineSpecies(entity);
            SetSkillLevels(npc);
        }

        private bool SetCommonVariables(NPC npc, ZPlane zPlane, out System.Data.DataRow outDataRow)
        {
            System.Data.DataRow selctdDataRow = null; // holds the selected DataRow

            #region Iterate through existing NPCs, find similar NPC closest in level. Select DataRow.
            foreach (System.Data.DataRow dr in NPC.NPCDictionary.Values)
            {
                // name and profession match
                if (dr["name"].ToString().ToLower() == FormatEntityName(npc).ToLower()
                    && dr["classFullName"].ToString().ToLower() == Utils.FormatEnumString(npc.BaseProfession.ToString()).ToLower()
                    && dr["npcType"].ToString() == NPC.NPCType.Creature.ToString()) // no Merchants
                {
                    if (selctdDataRow == null || EntityLists.GetUniqueEntity(Convert.ToInt32(dr["npcID"])) == Entity.None)
                    {
                        selctdDataRow = dr;
                        outDataRow = dr;
                    }
                    else
                    {
                        // compare differences between levels, choose one closest to the builded NPC's level
                        if (Math.Abs(npc.Level - Convert.ToInt32(dr["level"])) < Math.Abs(npc.Level - Convert.ToInt32(selctdDataRow["level"])))
                            selctdDataRow = dr;
                    }
                }
            }
            #endregion

            // If a match is not found, go back to SquareOne. Otherwise mimic values from the database that represent years of fine tuning. Adjust later.
            if (selctdDataRow == null)
            {
                outDataRow = null;
                return false;
            }
            else
            {
                npc.Name = selctdDataRow["name"].ToString();
                npc.shortDesc = selctdDataRow["shortDesc"].ToString();
                npc.longDesc = selctdDataRow["longDesc"].ToString();

                npc.attackSound = selctdDataRow["attackSound"].ToString();
                npc.deathSound = selctdDataRow["deathSound"].ToString();
                npc.idleSound = selctdDataRow["idleSound"].ToString();
                npc.MoveString = selctdDataRow["movementString"].ToString();

                npc.visualKey = selctdDataRow["visualKey"].ToString();

                npc.species = (Globals.eSpecies)Enum.Parse(typeof(Globals.eSpecies), selctdDataRow["species"].ToString(), true);
                npc.special = selctdDataRow["special"].ToString();

                npc.Alignment = (Globals.eAlignment)Enum.Parse(typeof(Globals.eAlignment), selctdDataRow["alignment"].ToString(), true);

                npc.Speed = Convert.ToInt32(selctdDataRow["speed"]);

                npc.Strength = Convert.ToInt32(selctdDataRow["strength"]);
                npc.Dexterity = Convert.ToInt32(selctdDataRow["dexterity"]);
                npc.Intelligence = Convert.ToInt32(selctdDataRow["intelligence"]);
                npc.Wisdom = Convert.ToInt32(selctdDataRow["wisdom"]);
                npc.Constitution = Convert.ToInt32(selctdDataRow["constitution"]);
                npc.Charisma = Convert.ToInt32(selctdDataRow["charisma"]);

                if (selctdDataRow["tanningResult"].ToString().Length > 0)
                {
                    npc.tanningResult = new Dictionary<int, ItemBuilding.LootManager.LootRarityLevel>();
                    foreach (int id in Utils.ConvertStringToIntArray(selctdDataRow["tanningResult"].ToString()))
                    {
                        if (!npc.tanningResult.ContainsKey(id))
                            npc.tanningResult.Add(id, ItemBuilding.LootManager.LootRarityLevel.Always);
                    }
                }

                if (Convert.ToBoolean(selctdDataRow["hidden"])) npc.AddPermanentEffect(Effect.EffectTypes.Hide_in_Shadows);
                if (Convert.ToBoolean(selctdDataRow["fly"])) npc.AddPermanentEffect(Effect.EffectTypes.Flight);
                if (Convert.ToBoolean(selctdDataRow["breatheWater"]) || npc.IsWaterDweller) npc.AddPermanentEffect(Effect.EffectTypes.Breathe_Water);
                if (Convert.ToBoolean(selctdDataRow["nightVision"])) npc.AddPermanentEffect(Effect.EffectTypes.Night_Vision);

                npc.HasRandomName = Convert.ToBoolean(selctdDataRow["randomName"]);

                npc.baseArmorClass = Convert.ToDouble(selctdDataRow["baseArmorClass"]);
                npc.THAC0Adjustment = Convert.ToInt32(selctdDataRow["thac0Adjustment"]);

                npc.immuneBlind = Convert.ToBoolean(selctdDataRow["immuneBlind"]);
                npc.immuneCold = Convert.ToBoolean(selctdDataRow["immuneCold"]);
                npc.immuneCurse = Convert.ToBoolean(selctdDataRow["immuneCurse"]);
                npc.immuneDeath = Convert.ToBoolean(selctdDataRow["immuneDeath"]);
                npc.immuneFear = Convert.ToBoolean(selctdDataRow["immuneFear"]);
                npc.immuneFire = Convert.ToBoolean(selctdDataRow["immuneFire"]);
                npc.immuneLightning = Convert.ToBoolean(selctdDataRow["immuneLightning"]);
                npc.immunePoison = Convert.ToBoolean(selctdDataRow["immunePoison"]);
                npc.immuneStun = Convert.ToBoolean(selctdDataRow["immuneStun"]);

                npc.canCommand = Convert.ToBoolean(selctdDataRow["command"]);
                npc.gender = (Globals.eGender)Convert.ToInt16(selctdDataRow["gender"]);
                npc.race = selctdDataRow["race"].ToString();
                npc.Age = Convert.ToInt32(selctdDataRow["age"]);

                //if (!ItemBuilding.LootManager.NPCLootTables.ContainsKey(npc.npcID))
                //    ItemBuilding.LootManager.CreateNewLootTable(npc, zPlane);

                outDataRow = selctdDataRow;
                return true;
            }
        }

        /// <summary>
        /// This method uses Lists of Entities in EntityList to determine if an Entity should have a GameTalent available.
        /// The structure of the names for Lists in EntityLists is TALENT_TALENTCOMMAND. Must be explicit or it won't be granted to the Entity.
        /// </summary>
        /// <param name="npc"></param>
        public static void SetTalents(NPC npc)
        {
            // used to gather property information
            var entityListType = (typeof(EntityLists));

            // the commands associated with each property pertaining to a talent
            var talentCommands = new Dictionary<string, List<Entity>>();

            try
            {
                foreach (FieldInfo fieldInfo in entityListType.GetFields())
                {
                    if (fieldInfo.Name.ToLower().StartsWith("talent_"))
                    {
                        talentCommands.Add(fieldInfo.Name.ToLower().Replace("talent_", ""), (List<Entity>)fieldInfo.GetValue(new List<Entity>()));
                    }
                }

                // check talentCommands for existence
                // if it exists, check if talentProperty exists
                // then check the List<Entity> to see if this entity is in the list
                // if so, give them the talent

                foreach (string talentCommand in talentCommands.Keys)
                {
                    // Talent exists.
                    if (Talents.GameTalent.GameTalentDictionary.ContainsKey(talentCommand.ToLower()))
                    {
                        Talents.GameTalent gameTalent = Talents.GameTalent.GameTalentDictionary[talentCommand];

                        // Entity should have talent as defined in EntityLists. Profession eligible, doesn't have the talent, and meets level req.
                        if (talentCommands[talentCommand].Contains(npc.entity) && gameTalent.IsProfessionElgible(npc.BaseProfession) &&
                            !npc.talentsDictionary.ContainsKey(talentCommand) &&
                            npc.Level >= gameTalent.MinimumLevel)
                        {
                            npc.talentsDictionary.Add(talentCommand, DateTime.UtcNow);
                        }
                    }
                    // else log warning -- coder error
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static void SetAttackAndBlockStrings(NPC npc)
        {
            if (EntityLists.ANTLERED.Contains(npc.entity))
            {
                npc.attackStrings.Add("slams you with its antlers");
                npc.attackStrings.Add("pierces you with an antler");
                npc.blockStrings.Add("blocks you with its antlers");
            }

            if (EntityLists.BEAKED.Contains(npc.entity))
            {
                npc.attackStrings.Add("stabs you with its beak");
                npc.attackStrings.Add("pierces you with its beak");

                npc.blockStrings.Add("blocks you with its beak");
            }

            if (EntityLists.BITER.Contains(npc.entity))
            {
                npc.attackStrings.Add("bites you");

                if (!npc.attackStrings.Contains("savages you with its teeth"))
                    npc.attackStrings.Add("savages you with its teeth");
            }

            if (EntityLists.CLAWED.Contains(npc.entity))
            {
                npc.attackStrings.Add("hits with a claw");
                npc.attackStrings.Add("rakes you with its claws");
                npc.blockStrings.Add("blocks you with a claw");
            }

            if (EntityLists.FANGED.Contains(npc.entity))
                npc.attackStrings.Add("sinks its fangs into you");

            if (EntityLists.HOOVED.Contains(npc.entity))
            {
                npc.attackStrings.Add("pummels you with its hooves");
                npc.blockStrings.Add("blocks you with a hoof");
            }

            if (EntityLists.HORNED.Contains(npc.entity))
            {
                npc.attackStrings.Add("rams you with a horn");
                npc.attackStrings.Add("impales you on its horn");
                npc.blockStrings.Add("blocks you with a horn");
            }

            if (EntityLists.INCORPOREAL.Contains(npc.entity))
            {
                npc.attackStrings.Add("reaches inside you");
            }

            if (EntityLists.MAULER.Contains(npc.entity))
            {
                npc.attackStrings.Add("mauls you");
                npc.attackStrings.Add("savages you with its teeth");
            }

            if (EntityLists.PINCERED.Contains(npc.entity))
            {
                npc.attackStrings.Add("clamps onto you with its pincer");
            }

            if (EntityLists.PLANT.Contains(npc.entity))
            {
                npc.attackStrings.Add("pierces you with its thorns");

                if (EntityLists.POISONOUS.Contains(npc.entity))
                    npc.attackStrings.Add("spits its toxin at you");
            }

            if (EntityLists.REPTILIAN.Contains(npc.entity))
                npc.blockStrings.Add("slithers out of the way of your attack");

            if (EntityLists.SMASHER.Contains(npc.entity))
                npc.attackStrings.Add("smashes into you");

            if (EntityLists.STINGER.Contains(npc.entity))
                npc.attackStrings.Add("stings you");

            if (EntityLists.TALONED.Contains(npc.entity))
            {
                npc.attackStrings.Add("tears at you with a talon");
                npc.attackStrings.Add("rends you with a talon");
                npc.blockStrings.Add("blocks you with a talon");
            }

            if (EntityLists.TAILPIERCER.Contains(npc.entity))
            {
                npc.attackStrings.Add("pierces you with its barbed tail");
                npc.blockStrings.Add("blocks you with its tail");
            }

            if (EntityLists.TAILWHIPPER.Contains(npc.entity))
            {
                npc.attackStrings.Add("whips you with its tail");
                npc.blockStrings.Add("blocks you with its tail");
            }

            if (EntityLists.TENTACLED.Contains(npc.entity))
            {
                npc.attackStrings.Add("slaps you with a tentacle");
                npc.blockStrings.Add("You are blocked by a tentacle");
            }

            if (EntityLists.TUSKED.Contains(npc.entity))
            {
                npc.attackStrings.Add("pierces you with a tusk");
                npc.attackStrings.Add("slams you with its tusks");
                npc.blockStrings.Add("You are blocked by a tusk");
            }

            if (EntityLists.WING_BUFFETER.Contains(npc.entity))
                npc.attackStrings.Add("buffets you with its wings");

            if (EntityLists.WINGED.Contains(npc.entity))
            {
                npc.attackStrings.Add("slaps you with a wing");
                npc.blockStrings.Add("You are blocked by a wing");
            }

            if (EntityLists.ARACHNID.Contains(npc.entity))
                npc.attackStrings.Add("latches onto your neck and bites you");

            if (EntityLists.ARTHROPOD.Contains(npc.entity))
                npc.blockStrings.Add("Your attack bounces harmlessly off " + npc.Name + "'s carapace");
        }

        public void SetDescriptions(string desc, NPC npc, ZPlane zPlane, string synonym)
        {
            //string particle = "a "; // default particle
            string suffix = "";

            // perhaps a call to npc.FilterDisplayText here
            if (desc.ToLower().Contains("undead"))
                npc.IsUndead = true;

            if (desc.ToLower().Contains("|"))
            {
                string[] descArray = desc.Split("|".ToCharArray());

                suffix = descArray[1].Trim();
                desc = descArray[0].Trim();
            }

            string name = FormatEntityName(npc);

            if (npc.HasRandomName)
                name = npc.Name;
            else
            {
                npc.Name.Replace(synonym, "");
                name = name.Replace(".", " ");
                name = name.Replace(synonym, "");
            }

            // syntax
            if (EntityLists.ELVES.Contains(npc.entity))
                name = name.Replace("elf", " elven");

            // Do not add Fighter, Thaumaturge, etc classTypes as a synonym (this is done in LivingObjectDescription when required).
            foreach (string classType in Enum.GetNames(typeof(Character.ClassType)))
            {
                if (Utils.FormatEnumString(classType) == synonym)
                {
                    synonym = "";
                    break;
                }
            }

            // Some entities start with The (The Lost One).
            if (name.ToLower().StartsWith("the."))
                name = name.Substring(name.IndexOf(".") + 1, name.Length - name.IndexOf(".") + 1);

            if (!EntityLists.ANIMAL.Contains(npc.entity) && !EntityLists.IsFullBloodedWyrmKin(npc))
            {
                npc.shortDesc = (desc.Length > 0 ? desc + " " : "") + name + (synonym.Length > 0 ? " " + synonym : "");
                npc.longDesc = (desc.Length > 0 ? desc + " " : "") + name + (synonym.Length > 0 ? " " + synonym : "") + (suffix.Length > 0 ? " " + suffix : "");
            }
            else
            {
                npc.shortDesc = (desc.Length > 0 ? desc + " " : "") + npc.entity.ToString().ToLower().Replace(".", "").Replace("_", " ");
                npc.longDesc = (desc.Length > 0 ? desc + " " : "") + npc.entity.ToString().ToLower().Replace(".", "").Replace("_", " ") + (suffix.Length > 0 ? " " + suffix : "");

            }

            // remove white space characters
            npc.shortDesc = npc.shortDesc.Replace("  ", " ");
            npc.shortDesc = npc.shortDesc.Trim();

            npc.longDesc = npc.longDesc.Replace("  ", " ");
            npc.longDesc = npc.longDesc.Trim();
        }

        public static void SetGender(NPC npc, ZPlane zPlane)
        {
            if (EntityLists.FEMALE.Contains(npc.entity) || (zPlane.zAutonomy != null && zPlane.zAutonomy.genderExclusive == Globals.eGender.Female))
            {
                npc.gender = Globals.eGender.Female;
                return;
            }
            else if (EntityLists.MALE.Contains(npc.entity) || (zPlane.zAutonomy != null && zPlane.zAutonomy.genderExclusive == Globals.eGender.Male))
            {
                npc.gender = Globals.eGender.Male;
                return;
            }
            else if (zPlane.zAutonomy != null && zPlane.zAutonomy.genderExclusive == Globals.eGender.It)
            {
                npc.gender = Globals.eGender.It;
                return;
            }

            if (EntityLists.IsHumanOrHumanoid(npc))
            {
                int roll = Rules.RollD(1, 20);

                if (EntityLists.MATRIARCHAL.Contains(npc.entity))
                    roll -= 12; // need a 20 to be female

                if (EntityLists.IsGiantKin(npc))
                    roll += 5; // need a 1 or 2 for female gender

                if ((roll > 7 || EntityLists.MALE.Contains(npc.entity)) && !EntityLists.FEMALE.Contains(npc.entity))
                    npc.gender = Globals.eGender.Male;
                else
                {
                    npc.gender = Globals.eGender.Female;

                    if (EntityLists.HUMAN.Contains(npc.entity) || npc.species == Globals.eSpecies.Human)
                    {
                        npc.visualKey = npc.visualKey.Replace("male_", "female_");
                        if (npc.Name == "priest") npc.Name = "priestess";
                        else if (npc.Name == "druid") npc.Name = "druidess";
                    }
                }
            }
            else if (EntityLists.WYRMKIN.Contains(npc.entity))
            {
                // most wyrms will be female, the males fight amongst themselves often for the right to procreate
                if ((Rules.RollD(1, 20) >= 5 || EntityLists.FEMALE.Contains(npc.entity)) && !EntityLists.MALE.Contains(npc.entity))
                    npc.gender = Globals.eGender.Female;
                else npc.gender = Globals.eGender.Male;
            }
            else
            {
                // 50 percent humans and humanoids
                if (EntityLists.IsHumanOrHumanoid(npc))
                {
                    if (Rules.RollD(1, 100) <= 50)
                        npc.gender = Globals.eGender.Male;
                    else npc.gender = Globals.eGender.Female;
                }// 70 percent male giant kin
                else if (EntityLists.IsGiantKin(npc))
                {
                    if (Rules.RollD(1, 100) <= 70)
                        npc.gender = Globals.eGender.Male;
                    else npc.gender = Globals.eGender.Female;
                } // 60 percent male animals
                else if (EntityLists.ANIMAL.Contains(npc.entity))
                {
                    if (Rules.RollD(1, 100) <= 40)
                        npc.gender = Globals.eGender.Male;
                    else npc.gender = Globals.eGender.Female;
                }// far smaller percentage of male wyrms, they always fight amongst themselves
                else if (EntityLists.IsFullBloodedWyrmKin(npc))
                {
                    if (Rules.RollD(1, 100) <= 15)
                        npc.gender = Globals.eGender.Male;
                    else npc.gender = Globals.eGender.Female;
                }
                else
                    npc.gender = Globals.eGender.Male;
            }
        }

        public static void SetMemorizedSpell(NPC npc)
        {
            if (!npc.HasTalent(Talents.GameTalent.TALENTS.Memorize)) return;

            // Thieves
            if (npc.spellDictionary.ContainsKey((int)GameSpell.GameSpellID.Venom))
                npc.MemorizedSpellChant = npc.spellDictionary[(int)GameSpell.GameSpellID.Venom];

            // Specialized -- thieves with memorize will memorize Summon Humanoid
            if (Spells.SummonHumanoidSpell.SummonHumanoidAvailability.Contains(npc.entity) && npc.spellDictionary.ContainsKey((int)GameSpell.GameSpellID.Summon_Humanoid))
                npc.MemorizedSpellChant = npc.spellDictionary[(int)GameSpell.GameSpellID.Summon_Humanoid];

            // Sorcerers
            if (npc.spellDictionary.ContainsKey((int)GameSpell.GameSpellID.Acid_Orb))
                npc.MemorizedSpellChant = npc.spellDictionary[(int)GameSpell.GameSpellID.Acid_Orb];

            // Wizards
            if (npc.spellDictionary.ContainsKey((int)GameSpell.GameSpellID.Magic_Missile))
                npc.MemorizedSpellChant = npc.spellDictionary[(int)GameSpell.GameSpellID.Magic_Missile];

            if (npc.spellDictionary.ContainsKey((int)GameSpell.GameSpellID.Icespear))
                npc.MemorizedSpellChant = npc.spellDictionary[(int)GameSpell.GameSpellID.Icespear];

            // Thaums
            if (npc.spellDictionary.ContainsKey((int)GameSpell.GameSpellID.Curse))
                npc.MemorizedSpellChant = npc.spellDictionary[(int)GameSpell.GameSpellID.Curse];

            if (npc.spellDictionary.ContainsKey((int)GameSpell.GameSpellID.Ghod__s_Hooks))
                npc.MemorizedSpellChant = npc.spellDictionary[(int)GameSpell.GameSpellID.Ghod__s_Hooks];
        }

        public void SetName(NPC npc, string profession)
        {
            if (EntityLists.RANDOM_NAME.Contains(npc.entity) || Enum.IsDefined(typeof(Character.ClassType), npc.entity.ToString()))
                npc.HasRandomName = true; // If the entity is the same as a profession, give the NPC a name.

            if (npc.HasRandomName)
                npc.Name = GameSystems.Text.NameGenerator.GetRandomName(npc);
            else npc.Name = FormatEntityName(npc);

            

            if (!npc.HasRandomName && !npc.Name.Contains(".") && !EntityLists.CAPITALIZED.Contains(npc.entity) &&
                npc.IsSpellUser && !EntityLists.ANIMAL.Contains(npc.entity) && !EntityLists.UNDEAD.Contains(npc.entity) && !npc.IsUndead)
            {
                string name = npc.Name + "." + profession;
                if (name.Length <= GameSystems.Text.NameGenerator.NAME_MAX_LENGTH)
                    npc.Name = name;
            }

            while (npc.Name.StartsWith("."))
                npc.Name = npc.Name.Substring(1);
        }

        private void SetAlignment(NPC npc)
        {
            if (EntityLists.AMORAL.Contains(npc.entity))
                npc.Alignment = Globals.eAlignment.Amoral;
            else if (EntityLists.CHAOTIC_EVIL_ALIGNMENT.Contains(npc.entity))
                npc.Alignment = Globals.eAlignment.ChaoticEvil;
            else if (EntityLists.EVIL_ALIGNMENT.Contains(npc.entity))
                npc.Alignment = Globals.eAlignment.Evil;
            else if (EntityLists.LAWFUL_ALIGNMENT.Contains(npc.entity))
                npc.Alignment = Globals.eAlignment.Lawful;
            else if (EntityLists.NEUTRAL_ALIGNMENT.Contains(npc.entity))
                npc.Alignment = Globals.eAlignment.Neutral;
            else
            {
                switch (npc.BaseProfession)
                {
                    case Character.ClassType.Ravager:
                    case Character.ClassType.Sorcerer:
                        npc.Alignment = Globals.eAlignment.Evil;
                        break;
                    default:
                        npc.Alignment = Globals.eAlignment.Chaotic;
                        break;
                }
            }
        }

        public static void SetImmunities(NPC npc)
        {
            npc.immuneAcid = EntityLists.IMMUNE_ACID.Contains(npc.entity) || EntityLists.INCORPOREAL.Contains(npc.entity);
            npc.immuneBlind = EntityLists.IMMUNE_BLINDNESS.Contains(npc.entity) || EntityLists.INCORPOREAL.Contains(npc.entity);
            npc.immuneCold = EntityLists.IMMUNE_COLD.Contains(npc.entity) || EntityLists.INCORPOREAL.Contains(npc.entity);
            npc.immuneCurse = EntityLists.IMMUNE_CURSE.Contains(npc.entity);
            npc.immuneDeath = EntityLists.IMMUNE_DEATH.Contains(npc.entity);
            npc.immuneFear = EntityLists.IMMUNE_FEAR.Contains(npc.entity) || npc.IsUndead;
            npc.immuneFire = EntityLists.IMMUNE_FIRE.Contains(npc.entity) || EntityLists.INCORPOREAL.Contains(npc.entity);
            npc.immuneLightning = EntityLists.IMMUNE_LIGHTNING.Contains(npc.entity) || EntityLists.INCORPOREAL.Contains(npc.entity);
            npc.immunePoison = EntityLists.IMMUNE_POISON.Contains(npc.entity) || EntityLists.INCORPOREAL.Contains(npc.entity);
            npc.immuneStun = EntityLists.IMMUNE_STUN.Contains(npc.entity) || EntityLists.INCORPOREAL.Contains(npc.entity);
        }

        /// <summary>
        /// This is called when creating an NPC entity on the fly. Ie: Summon Phantasm, Summon Hellhound, and Figurine Spirit creation.
        /// The only requirements before this is called is the NPC object have a valid entity and level.
        /// </summary>
        /// <param name="npc"></param>
        public void SetOnTheFlyVariables(NPC npc)
        {
            if (npc.UniqueID >= -1) npc.UniqueID = GameWorld.World.GetNextNPCUniqueID();

            SetAbilityScores(npc);
            SetStatAdds(npc);
            SetHitsStaminaMana(npc);
            SetSkillLevels(npc);
            SetBaseArmorClassAndTHAC0Adjustment(npc);
            SetImmunities(npc);
            SetSounds(npc.entity, npc);
        }

        public static void SetSkillLevels(NPC npc)
        {
            int mod = 0;

            if (EntityLists.IsMerchant(npc.entity))
                mod += 5;

            if (EntityLists.UNIQUE.Contains(npc.entity) && !EntityLists.IsMerchant(npc.entity))
                mod += 2;

            if (EntityLists.HARD_HITTERS.Contains(npc.entity) && !EntityLists.UNIQUE.Contains(npc.entity))
                mod++;

            // Set unarmed and magic skill for animals.
            if (EntityLists.ANIMAL.Contains(npc.entity) && !EntityLists.ANIMALS_WIELDING_WEAPONS.Contains(npc.entity))
            {
                npc.SetSkillExperience(Globals.eSkillType.Unarmed, Skills.GetSkillForLevel(npc.Level - 3 + mod));

                if (npc.castMode > NPC.CastMode.Never)
                    npc.SetSkillExperience(Globals.eSkillType.Magic, Skills.GetSkillForLevel(npc.Level - 3 + mod));

                return;
            }

            // Cover all bases and set all skills.
            foreach (Globals.eSkillType skillType in Enum.GetValues(typeof(Globals.eSkillType)).Cast<Globals.eSkillType>())
                npc.SetSkillExperience(skillType, Skills.GetSkillForLevel(Math.Max(1, npc.Level - 5 + mod)));

            // Rangers have higher bow skill.
            if(npc.BaseProfession == Character.ClassType.Ranger)
                npc.SetSkillExperience(Globals.eSkillType.Bow, Skills.GetSkillForLevel(Math.Max(1, npc.Level - 3 + mod)));

            // unarmed skill
            if (npc.BaseProfession == Character.ClassType.Martial_Artist)
                npc.SetSkillExperience(Globals.eSkillType.Unarmed, Skills.GetSkillForLevel(Math.Max(1, npc.Level - 6 + mod)));
            else npc.SetSkillExperience(Globals.eSkillType.Unarmed, Skills.GetSkillForLevel(Math.Max(1, npc.Level - 8 + mod)));

            // magic skill
            if (npc.IsSpellWarmingProfession)
            {
                npc.SetSkillExperience(Globals.eSkillType.Magic, Skills.GetSkillForLevel(Math.Max(1, npc.Level - 4 + mod)));

                if(npc.BaseProfession == Character.ClassType.Sorcerer)
                    npc.SetSkillExperience(Globals.eSkillType.Shuriken, Skills.GetSkillForLevel(Math.Max(1, npc.Level - 3 + mod)));
            }
            else if(npc.castMode > NPC.CastMode.Never)
            {
                npc.SetSkillExperience(Globals.eSkillType.Magic, Skills.GetSkillForLevel(Math.Max(1, npc.Level - 3 + mod)));
            }
        }

        /// <summary>
        /// TODO: This needs some work in the future.
        /// </summary>
        /// <param name="npc"></param>
        public void SetBaseArmorClassAndTHAC0Adjustment(NPC npc)
        {
            npc.baseArmorClass = 7;
            npc.THAC0Adjustment = -1;

            if (EntityLists.INCORPOREAL.Contains(npc.entity) || EntityLists.IsHellspawn(npc) || EntityLists.ANIMAL.Contains(npc.entity))
            {
                npc.baseArmorClass = 5;
                npc.THAC0Adjustment = -2;
            }

            if (EntityLists.UNIQUE.Contains(npc.entity))
            {
                npc.baseArmorClass -= 2;
                npc.THAC0Adjustment -= 1;
            }

            if (EntityLists.IsFullBloodedWyrmKin(npc))
            {
                npc.baseArmorClass -= 1;
                npc.THAC0Adjustment -= 1;
            }

            //if (!EntityLists.IsHumanOrHumanoid(npc))
            //{
            //    npc.baseArmorClass -= 2 + (npc.Level / 8);
            //    npc.THAC0Adjustment -= npc.Level / 8;
            //}
        }

        private void SetSounds(Entity entity, NPC npc)
        {
            // NPCs loaded from the database store their sound files based on entity.
            if (Sound.SoundFileNumbers.ContainsKey(entity))
            {
                var soundsArray = Sound.SoundFileNumbers[entity];
                npc.attackSound = soundsArray[0];
                npc.deathSound = soundsArray[1];
                npc.idleSound = soundsArray[2];
            }

            // Humans, elves and fey sound the same.
            if (EntityLists.HUMAN.Contains(npc.entity) || EntityLists.ELVES.Contains(npc.entity) ||
                EntityLists.FEY.Contains(npc.entity))
            {
                npc.attackSound = "";
                npc.idleSound = "";

                switch (npc.gender)
                {
                    case Globals.eGender.Male:
                        string[] maleDeathSounds = new string[] { "0273", "0274", "0275" };
                        npc.deathSound = maleDeathSounds[Rules.Dice.Next(0, maleDeathSounds.Length - 1)];
                        break;
                    case Globals.eGender.Female:
                        string[] femaleDeathSounds = new string[] { "0276", "0277", "0278" };
                        npc.deathSound = femaleDeathSounds[Rules.Dice.Next(0, femaleDeathSounds.Length - 1)];
                        break;
                }
                return;
            }

            if (EntityLists.IsFullBloodedWyrmKin(npc))
            {
                npc.attackSound = "0031";
                npc.deathSound = "0043";
                npc.idleSound = "0019";
            }
            else if(EntityLists.IsGiantKin(npc))
            {
                npc.attackSound = "0289";
                npc.deathSound = "0290";
                npc.idleSound = "0286";
            }
            else if (EntityLists.DEMONS.Contains(entity))
            {
                npc.attackSound = "0007";
                npc.deathSound = "0137";
                npc.idleSound = "0117";
            }
            else if (EntityLists.EQUINE.Contains(entity))
            {
                npc.attackSound = "0154";
                npc.deathSound = "0173";
                npc.idleSound = "0135";
            }
            else if (EntityLists.GRIFFIN_ARCHETYPE.Contains(entity) || entity == Entity.Eagle || entity == Entity.Aarakocra)
            {
                npc.attackSound = "0021";
                npc.deathSound = "0033";
                npc.idleSound = "0009";
            }
            else if (EntityLists.FELINE.Contains(entity))
            {
                npc.attackSound = "0027";
                npc.deathSound = "0039";
                npc.idleSound = "0015";
            }
            else if (EntityLists.CANIFORMS.Contains(entity))
            {
                npc.attackSound = "0030";
                npc.deathSound = "0042";
                npc.idleSound = "0018";
            }
            else if (EntityLists.REPTILIAN.Contains(entity))
            {
                npc.attackSound = "0029";
                npc.deathSound = "0041";
                npc.idleSound = "0017";
            }
            else if (EntityLists.CANINE.Contains(entity))
            {
                npc.attackSound = "0026";
                npc.deathSound = "0038";
                npc.idleSound = "0014";
            }
            else if(EntityLists.SUS.Contains(entity))
            {
                npc.attackSound = "0024";
                npc.deathSound = "0036";
                npc.idleSound = "0012";
            }
            else if (string.IsNullOrEmpty(npc.attackSound))
            {
                switch (entity)
                {
                    case Entity.Apparition:
                    case Entity.Banshee:
                        npc.attackSound = "0147";
                        npc.deathSound = "0166";
                        npc.idleSound = "0128";
                        break;
                    case Entity.Draugr:
                    case Entity.Ghast:
                    case Entity.Ghoul:
                    case Entity.Kesmai_Crypt_Ghoul:
                        npc.attackSound = "0123";
                        npc.deathSound = "0006";
                        npc.idleSound = "0116";
                        break;
                    case Entity.Kraken:
                        npc.attackSound = "0294";
                        npc.deathSound = "0295";
                        npc.idleSound = "";
                        break;
                    case Entity.Confessor_Ghost:
                    case Entity.Ghost:
                    case Entity.Presence:
                    case Entity.Waft:
                        npc.attackSound = "0122";
                        npc.deathSound = "0005";
                        npc.idleSound = "0115";
                        break;
                    case Entity.Shadow:
                    case Entity.Spectre:
                        npc.attackSound = "0121";
                        npc.deathSound = "0004";
                        npc.idleSound = "0114";
                        break;
                    default:
                        break;
                }
            }

            if (EntityLists.IsHumanOrHumanoid(npc) && string.IsNullOrEmpty(npc.deathSound))
            {
                switch (npc.gender)
                {
                    case Globals.eGender.Male:
                        string[] maleDeathSounds = new string[] { "0273", "0274", "0275" };
                        npc.deathSound = maleDeathSounds[Rules.Dice.Next(0, maleDeathSounds.Length - 1)];
                        break;
                    case Globals.eGender.Female:
                        string[] femaleDeathSounds = new string[] { "0276", "0277", "0278" };
                        npc.deathSound = femaleDeathSounds[Rules.Dice.Next(0, femaleDeathSounds.Length - 1)];
                        break;
                }
            }
            // otherwise no sounds are set (this is done so it will be obvious while a dev is playing)
        }

        private void SetSpeed(NPC npc)
        {
            if (EntityLists.LUMBERING.Contains(npc.entity))
                npc.Speed = 1;
            else if (EntityLists.IsFullBloodedWyrmKin(npc))
                npc.Speed = Rules.Dice.Next(4, 6);
            else if (EntityLists.FLYING.Contains(npc.entity))
                npc.Speed = 4;
            else if (EntityLists.IsHumanOrHumanoid(npc))
                npc.Speed = 3;
            else npc.Speed = Rules.Dice.Next(2, 3);
        }

        // This is only temporary. 3/11/2017 Eb
        public static void SetVisualKey(Entity entity, NPC npc)
        {
            if (npc.HasRandomName || (EntityLists.IsHuman(npc) && !EntityLists.UNIQUE.Contains(entity)))
            {
                npc.visualKey = npc.gender.ToString().ToLower() + "_" + npc.BaseProfession.ToString().ToLower() + "_npc";
                // Not ready to give NPCs random color visual keys yet...
                List<string> colorOptions = new List<string>() { "red", "gray", "green", "purple", "yellow", "brown"};
                npc.visualKey += "_" + colorOptions[new Random(Guid.NewGuid().GetHashCode()).Next(0, colorOptions.Count)];
                if (npc.visualKey.EndsWith("_"))
                    npc.visualKey = npc.visualKey.Substring(0, npc.visualKey.Length - 1);
                return;
            }

            if (!EntityLists.UNIQUE.Contains(entity))
            {
                npc.visualKey = npc.Name.ToLower().Replace(".", "_");

                // By default genderless or male. If female, and not ALWAYS female (nymphs, sprites, dryads etc)
                if (EntityLists.IsHumanOrHumanoid(npc) && npc.gender == Globals.eGender.Female && !EntityLists.FEMALE.Contains(entity))
                    npc.visualKey = npc.visualKey + "_female";
            }
            else npc.visualKey = npc.entity.ToString().ToLower();
        }

        private void SetSpellCastingVariables(NPC npc, System.Data.DataRow dr)
        {
            if (dr != null)
            {
                npc.castMode = (NPC.CastMode)Convert.ToInt32(dr["castMode"]);

                if (dr["spells"].ToString() != "") // can dictate in the database what spells to give an npc
                {
                    string[] knownSpells = dr["spells"].ToString().Split(" ".ToCharArray());
                    for (int a = 0; a < knownSpells.Length; a++)
                    {
                        int id = Convert.ToInt32(knownSpells[a]);
                        string chant = GameSpell.GenerateMagicWords();
                        while (npc.spellDictionary.ContainsValue(chant)) // if this chant exists in the spell list generate another
                            chant = GameSpell.GenerateMagicWords();
                        npc.spellDictionary.Add(id, chant);
                    }
                }
                else if (npc.IsSpellUser)
                    NPC.CreateGenericSpellList(npc);
            }
            else
            {
                bool spellDictSet = false;

                //TODO: Need to work on this. 12/5/2015 Eb

                switch (npc.entity)
                {
                    case Entity.Audrey:
                        npc.castMode = NPC.CastMode.NoPrep;
                        npc.spellDictionary = new Dictionary<int, string>() { { (int)GameSpell.GameSpellID.Chain_Lightning, GameSpell.GenerateMagicWords() } };
                        npc.ManaMax = 200;
                        spellDictSet = true;
                        break;
                    case Entity.Shadow:
                        npc.castMode = NPC.CastMode.NoPrep;
                        npc.spellDictionary = new Dictionary<int, string>
                        {
                            { (int)GameSpell.GameSpellID.Darkness, GameSpell.GenerateMagicWords() }, // darkness
                            { (int)GameSpell.GameSpellID.Lifeleech, GameSpell.GenerateMagicWords() } // lifeleech
                        };
                        npc.ManaMax = npc.Level * 2;
                        spellDictSet = true;
                        break;
                    case Entity.Spider:
                        npc.castMode = NPC.CastMode.NoPrep;
                        npc.spellDictionary = new Dictionary<int, string>
                        {
                            { (int)GameSpell.GameSpellID.Create_Web, GameSpell.GenerateMagicWords() } // web
                        };
                        npc.ManaMax = npc.Level;
                        spellDictSet = true;
                        break;
                    case Entity.Vampire_Lord:
                        npc.castMode = NPC.CastMode.NoPrep;
                        npc.spellDictionary = new Dictionary<int, string>
                        {
                            { (int)GameSpell.GameSpellID.Death, GameSpell.GenerateMagicWords() } // death
                        };
                        npc.ManaMax = npc.Level * 3;
                        spellDictSet = true;
                        break;
                }

                // building an entirely new spell casting creature
                if (npc.IsSpellWarmingProfession && EntityLists.IsHumanOrHumanoid(npc) && !spellDictSet)
                {
                    npc.castMode = NPC.CastMode.Limited; // makes them a spell warmer
                    NPC.CreateGenericSpellList(npc);
                }
                else if(!npc.IsHybrid && npc.IsSpellUser && EntityLists.IsHumanOrHumanoid(npc) && !spellDictSet)
                {
                    npc.castMode = NPC.CastMode.NoPrep; // likely a knight or ravager
                    NPC.CreateGenericSpellList(npc);
                }
            }

            // NPCs with spell chants
            if (npc.spellDictionary.Count > 0)
                GameSpell.FillSpellLists(npc);
        }

        private void SetAbilityScores(NPC npc)
        {
            // Override for small animals.
            if (EntityLists.ANIMAL_SMALL.Contains(npc.entity))
            {
                npc.Strength = Rules.RollD(1, 6);
                npc.Dexterity = Rules.RollD(1, 10) + 5;
                npc.Intelligence = 3;
                npc.Wisdom = 3;
                npc.Constitution = Rules.RollD(1, 6);
                npc.Charisma = Rules.RollD(2, 6);
                return;
            }
            // Override for weak beings. EG: children
            else if (EntityLists.WEAK.Contains(npc.entity))
            {
                npc.Strength = Rules.RollD(1, 6);
                npc.Dexterity = Rules.RollD(1, 6);
                npc.Intelligence = Rules.RollD(2, 6);
                npc.Wisdom = Rules.RollD(2, 6);
                npc.Constitution = Rules.RollD(1, 6);
                npc.Charisma = Rules.RollD(3, 6);
                return;
            }
            
            #region Base Stats
            if (npc.IsPureMelee || npc.IsHybrid ||
                EntityLists.IsFullBloodedWyrmKin(npc) || EntityLists.SMASHER.Contains(npc.entity) || EntityLists.SUPERIOR_HEALTH.Contains(npc.entity))
                npc.Strength = Math.Max(CharGen.RollStat(), CharGen.RollStat());
            else npc.Strength = CharGen.RollStat();
            npc.Strength = CharGen.AdjustMinMaxStat(npc.Strength);

            if (Array.IndexOf(GameWorld.World.ThieverySkillProfessions, npc.BaseProfession) != -1 || npc.entity == Entity.Archer || npc.entity == Entity.Arbalist)
                npc.Dexterity = Math.Max(CharGen.RollStat(), CharGen.RollStat());
            else npc.Dexterity = CharGen.RollStat();
            npc.Dexterity = CharGen.AdjustMinMaxStat(npc.Dexterity);

            // non animalian, and wyrms (since they are animals)
            if (!EntityLists.ANIMAL.Contains(npc.entity) || EntityLists.IsFullBloodedWyrmKin(npc))
            {
                if (npc.IsIntelligenceCaster)
                    npc.Intelligence = Math.Max(CharGen.RollStat(), CharGen.RollStat());
                else npc.Intelligence = CharGen.RollStat();
                npc.Intelligence = CharGen.AdjustMinMaxStat(npc.Intelligence);
                if (npc.IsWisdomCaster)
                    npc.Wisdom = Math.Max(CharGen.RollStat(), CharGen.RollStat());
                else npc.Wisdom = CharGen.RollStat();
                npc.Wisdom = CharGen.AdjustMinMaxStat(npc.Wisdom);
            }
            else
            {
                npc.Intelligence = Rules.Dice.Next(3, 5);
                npc.Wisdom = Rules.Dice.Next(3, 5);
            }

            if (npc.IsPureMelee || npc.IsHybrid ||
                EntityLists.IsFullBloodedWyrmKin(npc) || EntityLists.SMASHER.Contains(npc.entity) || EntityLists.SUPERIOR_HEALTH.Contains(npc.entity))
                npc.Constitution = Math.Max(CharGen.RollStat(), CharGen.RollStat());
            else npc.Constitution = CharGen.RollStat();
            npc.Constitution = CharGen.AdjustMinMaxStat(npc.Constitution);

            if (npc.BaseProfession == Character.ClassType.Sorcerer)
                npc.Charisma = Math.Max(CharGen.RollStat(), CharGen.RollStat());
            else npc.Charisma = CharGen.RollStat();
            npc.Charisma = CharGen.AdjustMinMaxStat(npc.Charisma);
            #endregion

            //Adjustments.
            if (EntityLists.IsFullBloodedWyrmKin(npc)
                || EntityLists.SMASHER.Contains(npc.entity) ||
                EntityLists.SUPERIOR_HEALTH.Contains(npc.entity) ||
                EntityLists.IsGiantKin(npc) || EntityLists.HARD_HITTERS.Contains(npc.entity))
            {
                npc.Strength += Rules.RollD(2, 4);
                npc.Constitution += Rules.RollD(1, 6);
            }

            if (npc.entity == Entity.Archer || npc.entity == Entity.Thief || npc.BaseProfession == Character.ClassType.Thief)
                npc.Dexterity += Rules.RollD(1, 4);

            if (EntityLists.IsFullBloodedWyrmKin(npc) || EntityLists.UNIQUE.Contains(npc.entity))
            {
                npc.Intelligence += Rules.RollD(1, 4);
                npc.Wisdom += Rules.RollD(1, 4);
                npc.Charisma += Rules.RollD(1, 4);
            }
                
        }

        public static void SetStatAdds(NPC npc)
        {
            if (npc.Strength >= 15)
            {
                npc.strengthAdd = 1;

                //melee
                if (npc.IsPureMelee || npc.IsHybrid)
                {
                    switch (npc.Level)
                    {
                        case 6:
                        case 7:
                        case 8:
                            npc.strengthAdd += 1; break;
                        case 9:
                        case 10:
                        case 11:
                            npc.strengthAdd += 2; break;
                        case 12:
                        case 13:
                        case 14:
                            npc.strengthAdd += 3; break;
                        case 15:
                        case 16:
                        case 17:
                            npc.strengthAdd += 4; break;
                        case 18:
                        case 19:
                        case 20:
                            npc.strengthAdd += 5; break;
                        case 21:
                        case 22:
                        case 23:
                            npc.strengthAdd += 6; break;
                        case 24:
                        case 25:
                        case 26:
                            npc.strengthAdd += 7; break;
                        case 27:
                        case 28:
                        case 29:
                            npc.strengthAdd += 8; break;
                        case 30:
                        case 31:
                        case 32:
                            npc.strengthAdd += 9; break;
                        default:
                            break;
                    }
                }
                //magic users
                else
                {
                    switch (npc.Level)
                    {
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                            npc.strengthAdd += 1; break;
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                            npc.strengthAdd += 2; break;
                        case 15:
                        case 16:
                        case 17:
                        case 18:
                            npc.strengthAdd += 3; break;
                        case 19:
                        case 20:
                        case 21:
                        case 22:
                            npc.strengthAdd += 4; break;
                        case 23:
                        case 24:
                        case 25:
                        case 26:
                            npc.strengthAdd += 5; break;
                        case 27:
                        case 28:
                        case 29:
                        case 30:
                            npc.strengthAdd += 6; break;
                        case 31:
                        case 32:
                            npc.strengthAdd += 7; break;
                        default:
                            break;
                    }
                }
            }

            if (npc.Dexterity >= 15)
            {
                npc.dexterityAdd = 1;

                //melee
                if (npc.IsPureMelee || npc.IsHybrid)
                {
                    switch (npc.Level)
                    {
                        case 6:
                        case 7:
                        case 8:
                            npc.dexterityAdd += 1; break;
                        case 9:
                        case 10:
                        case 11:
                            npc.dexterityAdd += 2; break;
                        case 12:
                        case 13:
                        case 14:
                            npc.dexterityAdd += 3; break;
                        case 15:
                        case 16:
                        case 17:
                            npc.dexterityAdd += 4; break;
                        case 18:
                        case 19:
                        case 20:
                            npc.dexterityAdd += 5; break;
                        case 21:
                        case 22:
                        case 23:
                            npc.dexterityAdd += 6; break;
                        case 24:
                        case 25:
                        case 26:
                            npc.dexterityAdd += 7; break;
                        case 27:
                        case 28:
                        case 29:
                            npc.dexterityAdd += 8; break;
                        case 30:
                        case 31:
                        case 32:
                            npc.dexterityAdd += 9; break;
                        default:
                            break;
                    }
                }
                //magic users
                else
                {
                    switch (npc.Level)
                    {
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                            npc.dexterityAdd += 1; break;
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                            npc.dexterityAdd += 2; break;
                        case 15:
                        case 16:
                        case 17:
                        case 18:
                            npc.dexterityAdd += 3; break;
                        case 19:
                        case 20:
                        case 21:
                        case 22:
                            npc.dexterityAdd += 4; break;
                        case 23:
                        case 24:
                        case 25:
                        case 26:
                            npc.dexterityAdd += 5; break;
                        case 27:
                        case 28:
                        case 29:
                        case 30:
                            npc.dexterityAdd += 6; break;
                        case 31:
                        case 32:
                            npc.dexterityAdd += 7; break;
                        default:
                            break;
                    }
                }
            }
        }

        public static void SetHitsStaminaMana(NPC npc)
        {
            npc.HitsMax = (8 * npc.Level) + Rules.GetHitsGain(npc, npc.Level);

            npc.StaminaMax = (3 * npc.Level) + Rules.GetStaminaGain(npc, npc.Level);

            if (npc is Adventurer)
            {
                npc.HitsMax += npc.Level * 3;
                npc.StaminaMax += npc.Level * 2;
            }

            if (EntityLists.SUPERIOR_HEALTH.Contains(npc.entity))
            {
                npc.HitsMax += npc.Level * 1100;
                npc.StaminaMax += npc.Level * 120;
            }

            if (npc.IsSpellWarmingProfession || npc.castMode == NPC.CastMode.Limited || npc.castMode == NPC.CastMode.NoPrep ||
                npc.IsSummoned || npc.species == Globals.eSpecies.Magical || npc.spellDictionary.Count > 0)
            {
                if(npc.ManaMax <= 0) // EG: mana is set in the Summon Phantasm code before this step -Eb
                    npc.ManaMax = Rules.GetManaGain(npc, npc.Level);

                if (npc is Adventurer) npc.ManaMax += npc.Level;
            }

            if (EntityLists.SUPERIOR_MANA.Contains(npc.entity))
            {
                npc.ManaMax += npc.Level * 550;
            }

            if (Array.IndexOf(GameWorld.World.HybridProfessions, npc.BaseProfession) != -1 && npc.ManaMax > Effect.HYBRID_MANA_MAX)
                npc.ManaMax = Effect.HYBRID_MANA_MAX;
                
        }

        public static void SetRegeneration(NPC npc)
        {
            switch (npc.entity)
            {
                case Entity.Great_Schema:
                    npc.staminaRegen += npc.Level;
                    break;
                case Entity.Illithid:
                case Entity.Illithid_Elder:
                    npc.manaRegen += npc.Level;
                    break;
                case Entity.Overlord:
                    npc.hitsRegen += npc.Level;
                    npc.manaRegen += npc.Level;
                    break;
                case Entity.Titan:
                    npc.hitsRegen += npc.Level;
                    npc.staminaRegen += npc.Level;
                    break;
                case Entity.Trog:
                case Entity.Troll:
                case Entity.Troll_King:
                    npc.hitsRegen += npc.Level;
                    break;
            }
        }
    }
}
