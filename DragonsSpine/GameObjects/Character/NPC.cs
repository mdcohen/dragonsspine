using System;
using System.Collections.Generic;
using System.Timers;
using DragonsSpine.GameWorld;
using GameSpell = DragonsSpine.Spells.GameSpell;
using TextManager = DragonsSpine.GameSystems.Text.TextManager;
using DragonsSpine.Autonomy.EntityBuilding;
using DragonsSpine.Autonomy.ItemBuilding;

namespace DragonsSpine
{
    public class NPC : Character
    {
        // Added 10/19/2015 for testing.
        public AI.Priority CurrentPriority = AI.Priority.None;
        public AI.ActionType CurrentActionType = AI.ActionType.None;

        public List<AI.Priority> ExcludedPrioritiesList = new List<AI.Priority>(); // holds priorities to be excluded each round

        /// <summary>
        /// Holds the dictionary of NPCs pulled from the database. Key = NPC ID, Value = DataRow.
        /// </summary>
        public static Dictionary<int, System.Data.DataRow> NPCDictionary = new Dictionary<int, System.Data.DataRow>();
        
        #region Public Enumerations (4)
        public enum NPCType { Creature, Merchant }
        public enum AIType { Unknown, Enforcer, Priest, Sheriff, Summoned, Quest }

        /*  CastMode
        *  Never - never casts a spell
        *  Limited - casts spells from this npc's spellList and uses mana
        *  Unlimited - casts any spell from the global spellList and does not use mana
        *  NoPrep - casts spells from this npc's spellList, uses mana, no spell prep needed
        */
        public enum CastMode { Never, Limited, Unlimited, NoPrep }
        #endregion

        public int npcID; // this is only for NPC's
        //public int worldNpcID; // unique identifier for NPC currently active in the game world

        public int HateCenterX = 0;
        public int HateCenterY = 0;
        public int FearCenterY = 0;
        public int FearCenterX = 0;
        public int TotalHate = 0;
        public int TotalFearLove = 0;

        public Character MostHated { get; set; }
        public Character MostFeared { get; set; }
        public Character MostLoved { get; set; }

        public List<Character> targetList = new List<Character>(); // contains all visible character objects (all visible targets, both friend and enemy)
        public List<Character> enemyList = new List<Character>(); // contains only visible enemy character objects
        public List<Character> friendList = new List<Character>(); // contains only visible friend character objects
        public Character previousMostHated;
        public Cell previousMostHatedsCell;
        public string gotoWarmedMagic = "";

        public int m_buffTargetID = 0; // temporary storage of a Character object playerID or worldNpcID for a beneficial spell that was prepped

        public int BuffTargetID
        {
            get { return m_buffTargetID; }
            set { m_buffTargetID = value; }
        }
        public string buffSpellCommand = "";

        private Item m_pivotItem = null;
        public Item PivotItem
        {
            get { return m_pivotItem; }
            set
            {
                if (value == null)
                {
                    m_pivotItem = null; return;
                }

                if(!(value is Item)) return;

                if (!AI.IgnoredPivotItems.ContainsKey(this.UniqueID) || !AI.IgnoredPivotItems[this.UniqueID].Contains((value as Item).UniqueID))
                {
                    m_pivotItem = value;
                }
                else if (AI.IgnoredPivotItems.ContainsKey(this.UniqueID) && AI.IgnoredPivotItems[this.UniqueID].Contains((value as Item).UniqueID))
                {
                    Utils.Log("NPC Pivot Item set to null." + this.GetLogString() + " Item: " + (value as Item).GetLogString(), Utils.LogType.Debug);
                }
                // Log this to debug because AI logic should prevent reaching this point.
            }
        } // used to store an item that is going to be picked up or manipulated by the NPC



        public List<Cell> localCells = new List<Cell>();

        public Dictionary<int, string> abjurationSpells = new Dictionary<int, string>(); // abjuration spells
        public Dictionary<int, string> alterationSpells = new Dictionary<int, string>(); // beneficial alteration spells
        public Dictionary<int, string> alterationHarmfulSpells = new Dictionary<int, string>(); // harmful alteration spells
        public Dictionary<int, string> conjurationSpells = new Dictionary<int, string>(); // conjuration spells
        public Dictionary<int, string> divinationSpells = new Dictionary<int, string>(); // divination spells
        public Dictionary<int, string> evocationSpells = new Dictionary<int, string>(); // evocation spells
        public Dictionary<int, string> evocationAreaEffectSpells = new Dictionary<int, string>(); // area effect evocation spells
        public Dictionary<int, string> necromancySpells = new Dictionary<int, string>();
        public Dictionary<int, string> illusionSpells = new Dictionary<int, string>();

        public Dictionary<int, List<Item>> receivedItems = new Dictionary<int, List<Item>>(); // items received via the give command from a player

        public AIType aiType;

        /*  CastMode
         *  Never - never casts a spell
         *  Limited - casts spells from this npc's spellList and uses mana
         *  Unlimited - casts any spell from the global spellList and does not use mana
         *  NoPrep - casts spells from this npc's spellList, uses mana, no spell prep needed
         */
        public CastMode castMode = CastMode.Never;

        public string shortDesc; // short description of a NPC ( use LOOK AT command to see)
        public string longDesc; // long description of a NPC ( use LOOK CLOSELY AT command to see)

        public Dictionary<int, LootManager.LootRarityLevel> tanningResult;	// list of itemIDs this creature's corpse will produce if tanned

        #region Private Data
        int m_gold = 0;
        string groupMembers = ""; // used during spawning of NPC group members, then used to hold group leader worldNPCID if npc is separated from group
        bool mobile = false; // true if this npc can move, or false if it is perma-rooted (ie: statues)
        string moveString = ""; // string shouted on a move
        private bool m_randomName = false; // spawn with random name
        bool spectral = false; // if true, worn and held items disappear upon death
        private int speed; // how many cells this npc can move in one round
        public bool waterDweller = false; // must spawn in water, remains in water
        int zoneID = 0;
        int roundsRemaining = 0; // rounds remaining until this NPC despawns
        private bool isSummoned;
        #endregion

        #region Public Properties
        public bool IsGuarding
        {
            get { return special.Contains("petguard"); }
            set
            {
                if (value)
                {
                    if (!special.Contains("petguard"))
                    {
                        special += " petguard";
                        special.Trim();
                    }
                }
                else
                {
                    special.Replace("petguard", "");
                    special.Trim();
                }
            }
        }

        public int Gold
        {
            get { return this.m_gold; }
            set { this.m_gold = value; }
        }

        public string GroupMembers
        {
            get { return this.groupMembers; }
            set { this.groupMembers = value; }
        }

        public bool HasRandomName
        {
            get { return this.m_randomName; }
            set { this.m_randomName = value; }
        }

        public bool IsGreedy
        {
            get
            {
                if ((this is Merchant) && (this as Merchant).trainerType > Merchant.TrainerType.None)
                    return false;

                if (animal || EntityLists.IsAnimal(entity))
                    return false;

                if (lairCritter)
                    return false;

                // Vulcan's daughter.
                if ((Alignment != Globals.eAlignment.Lawful || entity == EntityLists.Entity.Alia || BaseProfession == ClassType.Thief) && EntityLists.IsHumanOrHumanoid(this))
                    return true;

                return false;
            }
        }

        public bool IsTrulyImmobile
        {
            get
            {
                if (Name.ToLower().Contains("statue") || species == Globals.eSpecies.Plant)
                    return true;

                return false;
            }
        }

        public bool IsMobile
        {
            get { return this.mobile; }
            set { this.mobile = value; }
        }

        public bool IsSpectral
        {
            get { return this.spectral; }
            set { this.spectral = value; }
        }

        public bool IsWaterDweller
        {
            get { return this.waterDweller; }
            set { this.waterDweller = value; }
        }

        public string MoveString
        {
            get { return this.moveString; }
            set { this.moveString = value; }
        }

        public int SpawnZoneID
        {
            get { return this.zoneID; }
            set { this.zoneID = value; }
        }

        public int Speed
        {
            get { return this.speed; }
            set { this.speed = value; }
        }

        public int RoundsRemaining
        {
            get { return this.roundsRemaining; }
            set { this.roundsRemaining = value; }
        }

        public bool IsSummoned
        {
            get { return this.isSummoned || EntityLists.SUMMONED.Contains(this.entity); }
            set { this.isSummoned = value; }
        }
        #endregion

        #region Lair Specific
        public bool lairCritter = false; // true if this creature will return home to its lair
        public string lairCells = "";
        public List<Cell> lairCellsList = new List<Cell>();
        public int lairXCord = 0;
        public int lairYCord = 0;
        public int lairZCord = 0;
        
        #endregion

        public int spawnXCord = 0;
        public int spawnYCord = 0;
        public int spawnZCord = 0;
        public bool wasImmobile = false;

        #region Patrol Specific
        public string patrolRoute = "";	// x|y|z x|y|z etc...
        public bool HasPatrol
        {
            get
            {
                if (this.patrolRoute.Length > 0)
                {
                    return true;
                }
                return false;
            }
        }
        public int patrolCount = 0; // which key we're at on our patrolKeys
        public int patrolWaitRoundsRemaining = 0; // how many more rounds we will wait at current cell
        public List<string> patrolKeys; // initialized when npc's are loaded into the npc dictionary
        #endregion

        #region Constructors
        public NPC()
            : base()
        {
            this.IsPC = false;
            this.UniqueID = World.GetNextNPCUniqueID();
        }

        public NPC(System.Data.DataRow dr) : base()
        {            
            this.IsPC = false;
            this.UniqueID = World.GetNextNPCUniqueID();
            this.npcID = Convert.ToInt32(dr["npcID"]);
            this.Notes = dr["notes"].ToString();
            this.Name = dr["name"].ToString();
            this.attackSound = dr["attackSound"].ToString();
            this.deathSound = dr["deathSound"].ToString();
            this.idleSound = dr["idleSound"].ToString();
            this.MoveString = dr["movementString"].ToString();
            this.shortDesc = dr["shortDesc"].ToString();
            this.longDesc = dr["longDesc"].ToString();
            this.visualKey = dr["visualKey"].ToString();
            this.baseArmorClass = Convert.ToDouble(dr["baseArmorClass"]);
            this.THAC0Adjustment = Convert.ToInt32(dr["thac0Adjustment"]);
            this.special = dr["special"].ToString();
            this.aiType = (NPC.AIType)Enum.Parse(typeof(NPC.AIType), dr["aiType"].ToString(), true);
            
            this.Gold = Convert.ToInt32(dr["gold"]);          
            this.BaseProfession = (Character.ClassType)Enum.Parse(typeof(Character.ClassType), dr["classType"].ToString(), true);
            this.classFullName = dr["classFullName"].ToString();

            this.entity = EntityLists.GetUniqueEntity(this.npcID);

            if (this.entity == EntityLists.Entity.None)
            {
                if (!Enum.TryParse(this.Name.Replace(".", "_"), true, out this.entity))
                {
                    if (!Enum.TryParse(this.BaseProfession.ToString(), true, out this.entity))
                    {
                        Utils.Log("Failed to parse NPC entity using NPC name ( " + this.Name + ") and profession (" + this.BaseProfession.ToString() + ").", Utils.LogType.SystemWarning);
                    }
                }
            }

            // Add to SoundFileNumbers if this NPC's entity does not already exist and at least one of their sounds is not null.
            if (entity != EntityLists.Entity.None && !Sound.SoundFileNumbers.ContainsKey(entity)
                && (attackSound != "" || deathSound != "" || idleSound != ""))
                Sound.SoundFileNumbers.Add(entity, new string[] { attackSound, deathSound, idleSound });

            if (!Enum.TryParse<Globals.eSpecies>(dr["species"].ToString(), out this.species))
                species = EntityBuilder.DetermineSpecies(this.entity);

            if (dr["classFullName"].ToString() == "") this.classFullName = this.BaseProfession.ToString().Replace("_", " ");

            this.Experience = Convert.ToInt32(dr["exp"]);
            this.HitsMax = Convert.ToInt32(dr["hitsMax"]);
            this.Hits = this.HitsMax;
            this.Alignment = (Globals.eAlignment)Enum.Parse(typeof(Globals.eAlignment), dr["alignment"].ToString(), true);
            this.StaminaMax = Convert.ToInt32(dr["stamina"]);
            this.ManaMax = Convert.ToInt32(dr["manaMax"]);
            this.Speed = Convert.ToInt32(dr["speed"]);
            this.Strength = Convert.ToInt32(dr["strength"]);
            this.Dexterity = Convert.ToInt32(dr["dexterity"]);
            this.Intelligence = Convert.ToInt32(dr["intelligence"]);
            this.Wisdom = Convert.ToInt32(dr["wisdom"]);
            this.Constitution = Convert.ToInt32(dr["constitution"]);
            this.Charisma = Convert.ToInt32(dr["charisma"]);

            this.mace = Convert.ToInt64(dr["mace"]);
            this.bow = Convert.ToInt64(dr["bow"]);
            this.dagger = Convert.ToInt64(dr["dagger"]);
            this.flail = Convert.ToInt64(dr["flail"]);
            this.rapier = Convert.ToInt64(dr["rapier"]);
            this.twoHanded = Convert.ToInt64(dr["twoHanded"]);
            this.staff = Convert.ToInt64(dr["staff"]);
            this.shuriken = Convert.ToInt64(dr["shuriken"]);
            this.sword = Convert.ToInt64(dr["sword"]);
            this.threestaff = Convert.ToInt64(dr["threestaff"]);
            this.halberd = Convert.ToInt64(dr["halberd"]);
            this.unarmed = Convert.ToInt32(dr["unarmed"]);
            this.thievery = Convert.ToInt64(dr["thievery"]);
            this.magic = Convert.ToInt64(dr["magic"]);
            this.bash = Convert.ToInt64(dr["bash"]);
            this.Level = Convert.ToInt16(dr["level"]);
            this.animal = Convert.ToBoolean(dr["animal"]);

            this.tanningResult = new Dictionary<int, DragonsSpine.Autonomy.ItemBuilding.LootManager.LootRarityLevel>();

            if (dr["tanningResult"].ToString().Length > 0)
            {
                foreach (int id in Utils.ConvertStringToIntArray(dr["tanningResult"].ToString()))
                {
                    if (!this.tanningResult.ContainsKey(id))
                        this.tanningResult.Add(id, DragonsSpine.Autonomy.ItemBuilding.LootManager.LootRarityLevel.Always);
                }
            }

            this.IsUndead = Convert.ToBoolean(dr["undead"]);
            this.IsSpectral = Convert.ToBoolean(dr["spectral"]);

            if (Convert.ToBoolean(dr["hidden"])) this.AddPermanentEffect(Effect.EffectTypes.Hide_in_Shadows);

            this.poisonous = Convert.ToInt16(dr["poisonous"]);
            this.IsWaterDweller = Convert.ToBoolean(dr["waterDweller"]);

            if (Convert.ToBoolean(dr["fly"])) this.AddPermanentEffect(Effect.EffectTypes.Flight);

            if (Convert.ToBoolean(dr["breatheWater"]) || this.IsWaterDweller) this.AddPermanentEffect(Effect.EffectTypes.Breathe_Water);
            if (Convert.ToBoolean(dr["nightVision"])) this.AddPermanentEffect(Effect.EffectTypes.Night_Vision);

            this.lairCritter = Convert.ToBoolean(dr["lair"]);
            this.lairCells = dr["lairCells"].ToString();
            this.IsMobile = Convert.ToBoolean(dr["mobile"]);
            this.HasRandomName = Convert.ToBoolean(dr["randomName"]);
            this.castMode = (NPC.CastMode)Convert.ToInt32(dr["castMode"]);
            this.canCommand = Convert.ToBoolean(dr["command"]);

            EntityBuilder.SetAttackAndBlockStrings(this);

            this.gender = (Globals.eGender)Convert.ToInt16(dr["gender"]);

            if (this.gender == Globals.eGender.It)
                EntityBuilder.SetGender(this, this.Map.ZPlanes[this.Z]);

            this.race = dr["race"].ToString();
            this.Age = Convert.ToInt32(dr["age"]);
            this.patrolRoute = dr["patrolRoute"].ToString();

            EntityBuilder.SetRegeneration(this);

            if (this.patrolRoute.Length > 0 && Convert.ToBoolean(dr["patrol"]))
            {
                this.patrolKeys = new List<string>();
                string[] route = this.patrolRoute.Split(" ".ToCharArray());
                foreach (string nrt in route)
                {
                    this.patrolKeys.Add(nrt);
                }
            }

            if (this.entity == EntityLists.Entity.None)
            {
                this.immuneBlind = Convert.ToBoolean(dr["immuneBlind"]);
                this.immuneCold = Convert.ToBoolean(dr["immuneCold"]);
                this.immuneCurse = Convert.ToBoolean(dr["immuneCurse"]);
                this.immuneDeath = Convert.ToBoolean(dr["immuneDeath"]);
                this.immuneFear = Convert.ToBoolean(dr["immuneFear"]);
                this.immuneFire = Convert.ToBoolean(dr["immuneFire"]);
                this.immuneLightning = Convert.ToBoolean(dr["immuneLightning"]);
                this.immunePoison = Convert.ToBoolean(dr["immunePoison"]);
                this.immuneStun = Convert.ToBoolean(dr["immuneStun"]);
            }
            else EntityBuilder.SetImmunities(this);

            if (dr["spells"].ToString() != "") // can dictate in the database what spells to give an npc
            {
                string[] knownSpells = dr["spells"].ToString().Split(" ".ToCharArray());
                for (int a = 0; a < knownSpells.Length; a++)
                {
                    int id = Convert.ToInt32(knownSpells[a]);
                    string chant = GameSpell.GenerateMagicWords();
                    while (this.spellDictionary.ContainsValue(chant)) // if this chant exists in the spell list generate another
                    {
                        chant = GameSpell.GenerateMagicWords();
                    }
                    this.spellDictionary.Add(id, chant);
                }
            }
            else if (this.IsSpellUser)
                NPC.CreateGenericSpellList(this);

            if (this.spellDictionary.Count > 0)
                GameSpell.FillSpellLists(this);

            if (dr["quests"].ToString().Length > 0)
                NPC.FillQuestList(this, dr["quests"].ToString());

            // this is only used once during spawning, removed variable 11/5/2013
            //this.groupAmount = Convert.ToInt32(dr["groupAmount"]);
            this.GroupMembers = dr["groupMembers"].ToString(); // used once during spawning of group NPCs, then used later to hold group leader worldNPCID

            // questID~flag^questID~flag, where if questID <= 0 then the quest started is not required to get the flag
            if (dr["questFlags"].ToString().Length > 1)
            {
                string[] s = dr["questFlags"].ToString().Split(ProtocolYuusha.ISPLIT.ToCharArray());

                for (int a = 0; a < s.Length; a++)
                {
                    this.QuestFlags.Add(s[a]);
                }
            }

            EntityBuilder.SetTalents(this);

            //if (Level >= 8 || HitsMax <= 0)
            //    EntityBuilder.SetHitsStaminaMana(this);
            //if (Level >= 8)
            //    Experience = EntityBuilder.DetermineExperienceValue(this);

            EntityBuilder.SetSkillLevels(this);
            EntityBuilder.SetMemorizedSpell(this);
        }
        #endregion

        public void AddPermanentEffect(Effect.EffectTypes effectType)
        {
            Effect effect = new Effect(effectType, 0, -1, this, this);
        }

        public static void CreateGenericSpellListForGroupMember(NPC groupLeader, NPC groupMember)
        {
            if (groupLeader.spellDictionary.Count > 0 &&
                groupLeader.BaseProfession == groupMember.BaseProfession &&
                Skills.GetSkillLevel(groupLeader.magic) <= Skills.GetSkillLevel(groupMember.magic))
            {
                groupMember.spellDictionary.Clear();

                string chant = GameSpell.GenerateMagicWords();

                foreach (int spellID in groupLeader.spellDictionary.Keys)
                {
                    while(groupMember.spellDictionary.ContainsValue(chant))
                        chant = GameSpell.GenerateMagicWords();

                    groupMember.spellDictionary.Add(spellID, chant);
                }
            }
            else CreateGenericSpellList(groupMember);


        }

        public static void CreateGenericSpellList(NPC npc)
        {
            npc.spellDictionary.Clear();

            foreach (var spell in GameSpell.GameSpellDictionary.Values)
            {
                if (spell.IsClassSpell(npc.BaseProfession))
                {
                    if (Skills.GetSkillLevel(npc.magic) >= spell.RequiredLevel || npc.IsHybrid)
                    {
                        if (spell.IsAvailableAtTrainer ||
                            (!spell.IsAvailableAtTrainer && spell.IsFoundForScribing && Rules.CheckPerception(npc)) ||
                            EntityLists.UNIQUE.Contains(npc.entity) || npc.lairCritter)
                        {
                            var chant = GameSpell.GenerateMagicWords();

                            while (npc.spellDictionary.ContainsValue(chant))
                                chant = GameSpell.GenerateMagicWords();

                            npc.spellDictionary.Add(spell.ID, chant);
                        }
                    }
                }
            }
        }

        protected static void FillQuestList(NPC npc, string questInfo)
        {
            string[] quests = questInfo.Split(" ".ToCharArray());
            for (int a = 0; a < quests.Length; a++)
            {
                npc.QuestList.Add(GameQuest.GetQuest(Convert.ToInt32(quests[a])));
            }
        }

        public static void DoSpawn() // spawn some critters
        {
            //#if DEBUG
            //return;
            //#endif
            var sxcord = 0;
            var sycord = 0;
            var szcord = 0;
            NPC npc = null;
            // Loop through the spawnzonelinks and check the status of each zone
            foreach (var facet in World.Facets)
            {
                DateTime startTime = DateTime.Now;
                foreach (var szl in facet.Spawns.Values)
                {
                    try
                    {
                        if (szl.IsEnabled)
                        {
                            var attempts = 0;
                            // check if numInGame is less than MaxAllowedInWorld
                            if (szl.NumberInZone < szl.MaxAllowedInZone && szl.Timer > szl.SpawnTimer)
                            {
                                npc = CopyNPCFromDictionary(szl.NPCID);
                                while (szl.NumberInZone < szl.MaxAllowedInZone)
                                {
                                    // spawn attempt count is too high, log error and move on
                                    if (attempts >= SpawnZone.MAX_SPAWN_ATTEMPTS)
                                    {
                                        Utils.Log("SpawnZone " + szl.ZoneID + " did not find a suitable spawning point after " + attempts + " attempts." + npc.GetLogString(), Utils.LogType.SystemWarning);
                                        szl.IsEnabled = false;
                                        attempts = 0;
                                        break;
                                    }
                                    else attempts++;
                                    // random spawn z range has precedence
                                    if (szl.SpawnZRange.Count > 1)
                                    {
                                        szl.Z = szl.SpawnZRange[Rules.Dice.Next(0, szl.SpawnZRange.Count)];
                                        if (npc != null)
                                            szl.EstablishSpawnRadius(npc.IsWaterDweller);
                                        else szl.EstablishSpawnRadius(szl.Z);
                                        if (szl.SpawnCells.Count > 0)
                                        {
                                            Tuple<int, int, int> xyzRand = szl.SpawnCells[Rules.Dice.Next(0, szl.SpawnCells.Count)];
                                            sxcord = xyzRand.Item1;
                                            sycord = xyzRand.Item2;
                                            szcord = szl.Z;
                                        }
                                    }
                                    else if (szl.Radius <= 0 && !szl.IsAutonomous) // specific coordinates were set in the database
                                    {
                                        sxcord = szl.X;
                                        sycord = szl.Y;
                                        szcord = szl.Z;
                                    }
                                    else
                                    {
                                        try
                                        {
                                            var xyzRand = szl.SpawnCells[Rules.Dice.Next(0, szl.SpawnCells.Count)];
                                            sxcord = xyzRand.Item1;
                                            sycord = xyzRand.Item2;
                                            szcord = xyzRand.Item3;
                                        }
                                        catch (Exception e)
                                        {
                                            Utils.Log("Error determing random spawn point for SpawnZone " + szl.ZoneID + " LandID: " + szl.LandID + " Z: " + szl.Z, Utils.LogType.SystemFailure);
                                            var npcList = NPC.CopyNPCFromDictionary(szl.NPCID).Name + " NPCList:";
                                            foreach (var npcid in szl.NPCList)
                                                npcList += " " + NPC.CopyNPCFromDictionary(npcid).Name + ",";
                                            Utils.Log("NPCID: " + npcList.Trim(), Utils.LogType.SystemFailure);
                                            Utils.LogException(e);
                                        }
                                    }
                                    if (Cell.GetCell(facet.FacetID, szl.LandID, szl.MapID, sxcord, sycord, szcord) == null)
                                    {
                                        Utils.Log("SpawnZone " + szl.ZoneID + " is spawning outside of map bounds. F: " + facet.FacetID + " L: " + szl.LandID + " M: " + szl.MapID +
                                            " X: " + sxcord + " Y: " + sycord + " Z: " + szl.Z, Utils.LogType.SystemWarning);
                                        break;
                                    }
                                    var goSpawn = true;
                                    var cell = Cell.GetCell(facet.FacetID, szl.LandID, szl.MapID, sxcord, sycord, szl.Z);
                                    var tile = cell != null ? cell.CellGraphic : Cell.GRAPHIC_WALL;
                                    switch (tile)
                                    {
                                        case Cell.GRAPHIC_WALL:        //Don't spawn in wall
                                        case Cell.GRAPHIC_GRATE:    //Don't spawn in a grate
                                        case Cell.GRAPHIC_REEF:        //Don't spawn in wall
                                        case Cell.GRAPHIC_MOUNTAIN:     //Don't spawn in mountain
                                        case Cell.GRAPHIC_FOREST_IMPASSABLE:        //Don't spawn in Impenetrible forest
                                        case Cell.GRAPHIC_SECRET_DOOR:        //Don't spawn in secret door
                                        case Cell.GRAPHIC_ALTAR:        //Don't spawn in an altar
                                        case Cell.GRAPHIC_ALTAR_PLACEABLE:
                                        case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:        //Don't spawn in closed doors
                                        case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:        //Don't spawn in closed doors
                                        case Cell.GRAPHIC_COUNTER:        //Don't spawn in counters
                                        case Cell.GRAPHIC_COUNTER_PLACEABLE:
                                        case Cell.GRAPHIC_TRASHCAN:
                                        case Cell.GRAPHIC_BOXING_RING: //Don't spawn in boxing ring
                                        case Map.RESERVED_GRAPHIC_BOXING_RING:
                                            break;
                                        case Cell.GRAPHIC_FIRE:        //Don't spawn in fire (until we tell you to)
                                            if (!EntityLists.IMMUNE_FIRE.Contains(npc.entity) &&
                                                !EntityLists.FLYING.Contains(npc.entity)) goSpawn = false;
                                            break;
                                        case Cell.GRAPHIC_WATER:
                                            if (!EntityLists.WATER_DWELLER.Contains(npc.entity) && !EntityLists.AMPHIBIOUS.Contains(npc.entity)) goSpawn = false;
                                            break;
                                        case Cell.GRAPHIC_AIR:
                                            if (!EntityLists.FLYING.Contains(npc.entity)) goSpawn = false;
                                            break;
                                        case Cell.GRAPHIC_WEB:
                                            if (!EntityLists.WEB_DWELLERS.Contains(npc.entity)) goSpawn = false;
                                            break;
                                    }
                                    // safety net -- water cell
                                    if (tile != Cell.GRAPHIC_WATER && (EntityLists.WATER_DWELLER.Contains(npc.entity) && !EntityLists.AMPHIBIOUS.Contains(npc.entity)))
                                        goSpawn = false; // this includes aquatic elves as they will prefer to be in water
                                    // do not spawn on a teleport
                                    if (cell.IsTeleport) goSpawn = false;
                                    // do not spawn at lockers
                                    if (cell.IsLocker && !(npc is Merchant)) goSpawn = false;
                                    // do not spawn at a scribing crystal
                                    if (cell.HasScribingCrystal && !(npc is Merchant)) goSpawn = false;
                                    // in the case an out of bounds cell was included in an autonomously created spawn zone  11/21/2015 Eb
                                    if (cell.IsOutOfBounds) goSpawn = false;
                                    if (goSpawn)
                                    {
                                        //#if DEBUG
                                        //                                        Utils.Log("Spawning NPC for spawnzone " + szl.ZoneID, Utils.LogType.SystemTesting);
                                        //#endif
                                        #region goSpawn
                                        try
                                        {
                                            if (szl.NPCList.Count > 0)
                                            {
                                                var choice = Rules.Dice.Next(szl.NPCList.Count);
                                                var npcIDchoice = szl.NPCList[choice];
                                                npc = LoadNPC(szl.NPCList[choice],
                                                              facet.FacetID, szl.LandID, szl.MapID, sxcord, sycord,
                                                              szcord, szl.ZoneID);
                                            }
                                            else
                                            {
                                                npc = LoadNPC(szl.NPCID, facet.FacetID, szl.LandID, szl.MapID, sxcord,
                                                              sycord, szcord, szl.ZoneID);
                                            }
                                            //#if DEBUG
                                            //                                            Utils.Log(npc.GetLogString() + " LOADED.", Utils.LogType.SystemTesting);
                                            //#endif
                                            var groupAmount = 0;
                                            if (!(npc is Adventurer) && !szl.IsAsocial && EntityLists.SOCIAL.Contains(npc.entity))
                                            {
                                                if (!szl.IsAutonomous && !szl.IsUniqueEntity &&
                                                    NPC.NPCDictionary.ContainsKey(npc.npcID))
                                                    groupAmount =
                                                        Convert.ToInt32(NPC.NPCDictionary[npc.npcID]["groupAmount"]);
                                                else groupAmount = szl.GetGroupAmount(npc.npcID);
                                                // if GroupAmount is negative, come up with a random GroupAmount using the absolute value as max.
                                                if (groupAmount < 0) groupAmount = Math.Abs(groupAmount);
                                            }
                                            #region Load Group NPCs and Create Creature Group
                                            if (npc != null && !szl.IsAsocial && groupAmount > 0 && DragonsSpineMain.Instance.Settings.GroupNPCSpawningEnabled)
                                            {
                                                // a initialized to group amount - 1 because there is already 1 npc spawned
                                                for (int a = groupAmount - 1; a > 0; a--)
                                                {
                                                    NPC groupNPC = LoadNPC(npc.npcID, npc.FacetID, npc.LandID, npc.MapID, npc.X, npc.Y, npc.Z, szl.ZoneID);
                                                    if (groupNPC.IsSpellUser)
                                                        NPC.CreateGenericSpellListForGroupMember(npc, groupNPC);
                                                    // give the group npc the same held items as the leader...
                                                    groupNPC.RightHand = null;
                                                    if (npc.RightHand != null)
                                                    {
                                                        groupNPC.RightHand = Item.CopyItemFromDictionary(npc.RightHand.itemID);
                                                    }
                                                    groupNPC.LeftHand = null;
                                                    if (npc.LeftHand != null)
                                                    {
                                                        groupNPC.LeftHand = Item.CopyItemFromDictionary(npc.LeftHand.itemID);
                                                    }
                                                    if (npc.Group == null)
                                                    {
                                                        CharacterGroup.CreateCreatureGroup(npc, groupNPC, groupAmount);
                                                    }
                                                    else
                                                    {
                                                        npc.Group.Add(groupNPC);
                                                    }
                                                    szl.NumberInZone++; //This is to add the total number of mobs in the group to the total in the zone
                                                }
                                            }
                                            else if (npc != null && npc.GroupMembers.Length > 0 && !szl.IsAsocial)
                                            {
                                                string newName = "";
                                                if (npc.m_randomName)
                                                {
                                                    newName = Utils.FormatEnumString(npc.BaseProfession.ToString().ToLower());
                                                    npc.Name = newName;
                                                }
                                                string[] memberIDs = npc.GroupMembers.Split(" ".ToCharArray());
                                                for (int a = 0; a < memberIDs.Length; a++)
                                                {
                                                    NPC groupNPC = LoadNPC(Convert.ToInt32(memberIDs[a]), npc.FacetID, npc.LandID, npc.MapID, npc.X, npc.Y, npc.Z, szl.ZoneID);
                                                    // give the group npc the same held items as the leader...
                                                    groupNPC.RightHand = null;
                                                    if (npc.RightHand != null)
                                                        groupNPC.RightHand = Item.CopyItemFromDictionary(npc.RightHand.itemID);
                                                    groupNPC.LeftHand = null;
                                                    if (npc.LeftHand != null)
                                                        groupNPC.LeftHand = Item.CopyItemFromDictionary(npc.LeftHand.itemID);
                                                    if (npc.Group == null)
                                                        CharacterGroup.CreateCreatureGroup(npc, groupNPC, groupAmount);
                                                    else
                                                        npc.Group.Add(groupNPC);
                                                    if (newName != "") groupNPC.Name = newName;
                                                    szl.NumberInZone++; //This is to add the total number of mobs in the group to the total in the zone
                                                }
                                            }
                                            #endregion
                                            // successful spawn
                                            if (npc != null)
                                            {
                                                if (npc.Map != null && npc.Map.ZPlanes[npc.Z].spawnAlignment != Globals.eAlignment.None)
                                                {
                                                    npc.Alignment = npc.Map.ZPlanes[npc.Z].spawnAlignment;
                                                    if (npc.Group != null)
                                                    {
                                                        foreach (NPC ch in npc.Group.GroupNPCList)
                                                            ch.Alignment = npc.Alignment;
                                                    }
                                                }
                                                // this will set skill levels for trainers based on zAutonomy levels
                                                if (npc is Merchant && (npc as Merchant).trainerType > Merchant.TrainerType.None)
                                                    (npc as Merchant).SetTrainerSkillLevels();
                                                szl.NumberInZone++;
                                                szl.Timer = 0;
                                                if (DragonsSpineMain.ServerStatus == DragonsSpineMain.ServerState.Running && szl.SpawnMessage.Length > 0)
                                                {
                                                    foreach (var ch in new List<PC>(Character.PCInGameWorld))
                                                    {
                                                        if (ch.CurrentCell.LandID == szl.LandID && ch.CurrentCell.MapID == szl.MapID)
                                                        {
                                                            ch.WriteToDisplay(szl.SpawnMessage);
                                                            ch.SendSound(Sound.GetCommonSound(Sound.CommonSound.EarthQuake));
                                                        }
                                                    }
                                                }
                                                attempts = 0;
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Utils.Log("Verify SpawnZone " + szl.ZoneID + " NPCID and / or NPCList", Utils.LogType.SystemWarning);
                                            Utils.LogException(e);
                                        }
                                        #endregion
                                    }
                                    else if (!goSpawn && SpawnZone.MAX_SPAWN_ATTEMPTS > 0 && attempts == SpawnZone.MAX_SPAWN_ATTEMPTS - 1)
                                    {
                                        // Display and log a possible failed spawn attempt.
                                        Cell failedSpawnCell = Cell.GetCell(facet.FacetID, szl.LandID, szl.MapID, sxcord, sycord, szcord);
                                        if (failedSpawnCell != null)
                                        {
                                            Utils.Log("Possible failed spawn attempting: " + failedSpawnCell.GetLogString(false), Utils.LogType.SystemWarning);
                                        }
                                    }
                                }
                            }//end if
                        }
                    } //end if spawn enabled
                    catch (Exception e)
                    {
                        Utils.Log("Error with SpawnZone " + szl.ZoneID + " LandID: " + szl.LandID + " Z: " + szl.Z, Utils.LogType.SystemFailure);
                        string npcList = NPC.CopyNPCFromDictionary(szl.NPCID).Name + ", NPCList:";
                        foreach (int npcid in szl.NPCList)
                            npcList += " " + NPC.CopyNPCFromDictionary(npcid).Name + ",";
                        Utils.Log("NPCID: " + npcList.Trim(), Utils.LogType.SystemFailure);
                        Utils.LogException(e);
                    }
                }
            }
        }

        public static NPC CopyNPCFromDictionary(int npcID)
        {
            if (NPCDictionary.ContainsKey(npcID))
            {
                System.Data.DataRow dr = NPCDictionary[npcID];

                var npcType = (NPCType) Enum.Parse(typeof (NPCType), dr["npcType"].ToString(), true);

                switch (npcType)
                {
                    case NPCType.Merchant:
                        return new Merchant(dr);
                    default:
                        return new NPC(dr);
                }
            }

            if (EntityCreationManager.ContainsNPCID(npcID))
            {
                NPC npc = EntityCreationManager.CopyAutoCreatedNPC(npcID);
                npc.UniqueID = World.GetNextNPCUniqueID();
                EntityBuilder.SetVisualKey(npc.entity, npc);
                return npc;
            }

            Utils.Log("NPC.CopyNPCFromDictionary(" + npcID + ") NPC ID does not exist.", Utils.LogType.SystemWarning);
            return null;
        }

        public static NPC LoadNPC(int npcID, int facetID, int landID, int mapID, int xcord, int ycord, int zcord, int spawnzoneid)
        {
            var npc = NPC.CopyNPCFromDictionary(Convert.ToInt32(npcID));

            if (npc == null)
                return null;

            try
            {
                npc.CurrentCell = Cell.GetCell(facetID, landID, mapID, xcord, ycord, zcord);

                if (npc.CurrentCell != null)
                {
                    // give NPC inherent nightvision if spawning in a Cell that is always dark or has no light...
                    if ((npc.Map.ZPlanes[npc.Z].lightLevel == Map.LightLevel.None || npc.CurrentCell.IsAlwaysDark) && !npc.EffectsList.ContainsKey(Effect.EffectTypes.Night_Vision))
                        npc.AddPermanentEffect(Effect.EffectTypes.Night_Vision);
                }
               
                npc.spawnXCord = npc.X;
                npc.spawnYCord = npc.Y;
                npc.spawnZCord = npc.Z;

                #region Patrol Route Placement
                // if npc is lair critter with patrol, set patrolCount to next cell in patrol route (if there is a match)
                if (npc.patrolKeys != null && npc.HasPatrol && npc.lairCritter)
                {
                    for (int a = 0; a < npc.patrolKeys.Count; a++)
                    {
                        string[] xyz = npc.patrolKeys[a].Split("|".ToCharArray());
                        if (xyz[0].ToLower() == "w") // is wait patrol key
                            continue;
                        if (Convert.ToInt32(xyz[0]) == npc.X && Convert.ToInt32(xyz[1]) == npc.Y &&
                            Convert.ToInt32(xyz[2]) == npc.Z)
                        {
                            npc.patrolCount = a + 1;
                            if (npc.patrolCount > npc.patrolKeys.Count - 1)
                            {
                                npc.patrolCount = 0;
                            }
                            break;
                        }
                    }
                }
                else if (npc.HasPatrol && npc.patrolKeys != null) // place NPC at random patrol cell
                {
                    int rollOdds = Rules.Dice.Next(0, npc.patrolKeys.Count);
                    string[] xyz = npc.patrolKeys[rollOdds].Split("|".ToCharArray());

                    while (xyz[0].ToLower() == "w") // found wait patrol key
                    {
                        rollOdds = Rules.Dice.Next(0, npc.patrolKeys.Count);
                        xyz = npc.patrolKeys[rollOdds].Split("|".ToCharArray());
                    }

                    npc.CurrentCell = Cell.GetCell(facetID, landID, mapID, Convert.ToInt16(xyz[0]), Convert.ToInt16(xyz[1]),
                        Convert.ToInt32(xyz[2]));

                    npc.patrolCount = rollOdds + 1;

                    if (npc.patrolCount > npc.patrolKeys.Count - 1)
                        npc.patrolCount = 0;
                }
                #endregion

                if (npc.Level <= 0)
                {
                    try
                    {
                        npc.Level = Convert.ToInt16(Rules.Dice.Next(npc.CurrentCell.Map.SuggestedMinimumLevel, npc.CurrentCell.Map.SuggestedMaximumLevel));
                    }
                    catch
                    {
                        npc.Level = Rules.RollD(1, 12) + 3;
                    }
                }

                if (npc.gender == Globals.eGender.Random) // determine gender if random
                    EntityBuilder.SetGender(npc, npc.Map.ZPlanes[npc.Z]);

                if (npc.aiType == AIType.Priest && npc.species != Globals.eSpecies.Human)
                {
                    if (npc.Map.MapID == Map.ID_UNDERKINGDOM)
                        npc.species = Globals.eSpecies.Kobold;
                    else npc.species = Globals.eSpecies.Human;

                    if (npc.species != Globals.eSpecies.Human)
                    {
                        npc.Name = npc.species.ToString().ToLower() + ".cleric";
                        npc.classFullName = "cleric";
                    }
                }

                if (npc.race.ToLower() == "random") // determine race if random
                    npc.race = (string)Enum.GetNames(typeof(Globals.eHomeland)).GetValue(Rules.Dice.Next(0, Enum.GetNames(typeof(Globals.eHomeland)).Length));

                if (npc.Age < World.AgeCycles[0]) // determine age if random (young or very young)
                    npc.Age = Rules.Dice.Next(World.AgeCycles[0], World.AgeCycles[2] + 1);

                if (npc.HasRandomName) // determine name if random
                    npc.Name = GameSystems.Text.NameGenerator.GetRandomName(npc);

                // adjust npc experience based on map multiplier
                npc.Experience = Convert.ToInt64(npc.Experience * npc.Map.ExperienceModifier);

                EntityBuilder.SetStatAdds(npc);

                LootTable loot = LootManager.GetLootTable(npc, npc.Map.ZPlanes[zcord]);

                LootManager.GiveLootToNPC(npc, loot);

                #region NPC's Resistances
                //set npc resistance
                if (npc.Level >= 3)
                {
                    switch (npc.BaseProfession)
                    {
                        //TODO: fd, id, ld (immunities)
                        case ClassType.Berserker:
                            npc.StunResistance = 1;
                            npc.LightningResistance = 1;
                            npc.DeathResistance = 1;
                            npc.FearResistance = 1;
                            break;
                        case ClassType.Fighter:
                            npc.StunResistance = 2;
                            break;
                        case ClassType.Knight:
                            npc.DeathResistance = 2;
                            break;
                        case ClassType.Ravager:
                            npc.PoisonResistance = 2;
                            break;
                        case ClassType.Martial_Artist:
                            npc.LightningResistance = 2;
                            break;
                        case ClassType.Thaumaturge:
                            npc.BlindResistance = 1;
                            npc.FearResistance = 1;
                            break;
                        case ClassType.Thief:
                            npc.PoisonResistance = 1;
                            npc.BlindResistance = 1;
                            break;
                        case ClassType.Wizard:
                            npc.FireResistance = 1;
                            npc.ColdResistance = 1;
                            break;
                        case ClassType.Druid:
                            npc.FireResistance = 1;
                            npc.LightningResistance = 1;
                            break;
                        case ClassType.Ranger:
                            npc.FireResistance = 1;
                            npc.BlindResistance = 1;
                            break;
                        default:
                            break;
                    }
                    if (npc.Level > 3)
                    {
                        int addResistance = npc.Level - 3;
                        npc.FireResistance += addResistance;
                        npc.ColdResistance += addResistance;
                        npc.LightningResistance += addResistance;
                        npc.DeathResistance += addResistance;
                        npc.BlindResistance += addResistance;
                        npc.FearResistance += addResistance;
                        npc.StunResistance += addResistance;
                        npc.PoisonResistance += addResistance;
                    }
                }
                #endregion

                //set random coin values for npc sack items
                LootManager.SetRandomCoinValues(npc.sackList);
                LootManager.SetRandomCoinValues(npc.beltList);
                LootManager.SetRandomCoinValues(npc.wearing);
                LootManager.SetRandomCoinValues(npc.pouchList);

                #region Add Effects
                #region Add Held Item Effects
                if (npc.RightHand != null && npc.RightHand.effectType.Length > 0 && npc.RightHand.wearLocation == Globals.eWearLocation.None)
                {
                    Effect.AddWornEffectToCharacter(npc, npc.RightHand);
                }
                if (npc.LeftHand != null && npc.LeftHand.effectType.Length > 0 && npc.LeftHand.wearLocation == Globals.eWearLocation.None)
                {
                    Effect.AddWornEffectToCharacter(npc, npc.LeftHand);
                }
                #endregion

                #region Add Ring Effects
                if (npc.RightRing1 != null && npc.RightRing1.effectType.Length > 0)
                {
                    Effect.AddWornEffectToCharacter(npc, npc.RightRing1);
                }
                if (npc.RightRing2 != null && npc.RightRing2.effectType.Length > 0)
                {
                    Effect.AddWornEffectToCharacter(npc, npc.RightRing2);
                }
                if (npc.RightRing3 != null && npc.RightRing3.effectType.Length > 0)
                {
                    Effect.AddWornEffectToCharacter(npc, npc.RightRing3);
                }
                if (npc.RightRing4 != null && npc.RightRing4.effectType.Length > 0)
                {
                    Effect.AddWornEffectToCharacter(npc, npc.RightRing4);
                }

                if (npc.LeftRing1 != null && npc.LeftRing1.effectType.Length > 0)
                {
                    Effect.AddWornEffectToCharacter(npc, npc.LeftRing1);
                }
                if (npc.LeftRing2 != null && npc.LeftRing2.effectType.Length > 0)
                {
                    Effect.AddWornEffectToCharacter(npc, npc.LeftRing2);
                }
                if (npc.LeftRing3 != null && npc.LeftRing3.effectType.Length > 0)
                {
                    Effect.AddWornEffectToCharacter(npc, npc.LeftRing3);
                }
                if (npc.LeftRing4 != null && npc.LeftRing4.effectType.Length > 0)
                {
                    Effect.AddWornEffectToCharacter(npc, npc.LeftRing4);
                }
                #endregion

                if ((npc.BaseProfession == ClassType.Knight || npc.BaseProfession == ClassType.Ravager) && npc.HasKnightRing)
                {
                    npc.Mana = npc.ManaMax;
                }

                #endregion

                // give warrior NPCs fighter specialization if they pass perception check
                if (Array.IndexOf(World.WeaponSpecializationProfessions, npc.BaseProfession) != -1 && npc.Level >= WARRIOR_SPECIALIZATION_LEVEL &&
                    npc.RightHand != null && EntityLists.IsHumanOrHumanoid(npc) &&
                    Rules.CheckPerception(npc))
                {
                    npc.fighterSpecialization = npc.RightHand.skillType;
                }

                if (npc.HitsMax <= 0)
                    npc.HitsMax = 5 * npc.Level;
                    npc.HitsMax += Rules.GetHitsGain(npc, npc.Level);

                npc.Hits = npc.HitsMax;

                if (npc.ManaMax <= 0 && npc.IsSpellUser)
                    npc.ManaMax = Rules.GetManaGain(npc, npc.Level);

                npc.Mana = npc.ManaMax;

                if (npc.StaminaMax <= 0)
                    npc.StaminaMax = Rules.GetStaminaGain(npc, npc.Level);

                npc.Stamina = npc.StaminaMax;

                // SpawnZoneID -1 is sent by code to create an NPC on the fly.
                if (spawnzoneid != -1 && Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["AdventurersEnabled"]) && Adventurer.MeetsAdventurerRequirements(npc))
                {
                    // modifying the number of scores collected increases chances adventurers will spawn
                    var cloneList = DAL.DBWorld.GetScoresWithoutSP(npc.BaseProfession, 50, true, "", false);

                    // quickly randomize the list
                    int n = cloneList.Count;
                    while (n > 1)
                    {
                        int k = (Rules.Dice.Next(0, n) % n);
                        n--;
                        PC value = cloneList[k];
                        cloneList[k] = cloneList[n];
                        cloneList[n] = value;
                    }

                    foreach (PC clone in cloneList)
                    {
                        if (clone.Level == npc.Level && clone.ImpLevel == Globals.eImpLevel.USER)
                        {
                            npc = new Adventurer(clone, npc);
                            Utils.Log(npc.GetLogString() + " created from " + clone.Name, Utils.LogType.Adventurer);
                            break;
                        }
                    }
                }

                if(npc.Name.StartsWith("-"))
                {
                    npc.Name = Utils.FormatEnumString(npc.entity.ToString()).ToLower();
                    Utils.Log("NPC spawned with unique ID number as name. Changed to entity name.", Utils.LogType.SystemWarning);
                }

                npc.SpawnZoneID = spawnzoneid;
                npc.AddToWorld();

                #region Lair Critter Loot and Cell Flagging
                // if this is a lair critter let's take care of its lair loot and lair cell flagging
                if (npc.lairCritter) // spawn point becomes their home
                {
                    npc.lairXCord = npc.X;
                    npc.lairYCord = npc.Y;
                    npc.lairZCord = npc.Z;

                    if (npc.lairCells.Length > 0)
                    {
                        string[] lc = npc.lairCells.Split(" ".ToCharArray());
                        foreach (string lcell in lc)
                        {
                            string[] lc2 = lcell.Split("|".ToCharArray());
                            int lx = Convert.ToInt32(lc2[0]);
                            int ly = Convert.ToInt32(lc2[1]);
                            Cell lairCell = Cell.GetCell(npc.FacetID, npc.LandID, npc.MapID, lx, ly, npc.Z);
                            if (lairCell != null)
                            {
                                lairCell.IsLair = true;
                                npc.lairCellsList.Add(lairCell);
                            }
                        }

                        if (npc.lairCellsList.Count > 0)
                            LootManager.LoadLairLoot(npc);
                    }
                }
                
                #endregion

                npc.RoundTimer.Start();
                npc.ThirdRoundTimer.Start();
                return npc;
            }
            catch (Exception e)
            {
                Utils.Log(
                    "Failure at NPC.LoadNPC(NPCnum: " + npcID + ", land: " + landID + ", map: " + mapID + ", xcord: " +
                    xcord + ", ycord: " + ycord + ", spawnzoneid: " + spawnzoneid + ")", Utils.LogType.SystemFailure);
                Utils.LogException(e);
                return null;
            }
        }

        public static string GetLogString(NPC npc)
        {
            if (npc.CurrentCell == null)
            {
                return "[NpcID: " + npc.npcID + " | UniqueID: " + npc.UniqueID + "] " + npc.Name + " [" + Utils.FormatEnumString(npc.Alignment.ToString()) + " " + Utils.FormatEnumString(npc.BaseProfession.ToString()) + "(" + npc.Level + ")]";
            }
            else
            {
                return "[NpcID: " + npc.npcID + " | UniqueNpcID: " + npc.UniqueID + "] " + npc.Name + " [" + Utils.FormatEnumString(npc.Alignment.ToString()) + " " + Utils.FormatEnumString(npc.BaseProfession.ToString()) + "(" + npc.Level + ")] (" + World.GetFacetByIndex(0).GetLandByID(npc.LandID).ShortDesc +
                " - " + World.GetFacetByID(npc.FacetID).GetLandByID(npc.LandID).GetMapByID(npc.MapID).Name + " " + npc.X + ", " + npc.Y + ", " + npc.Z + ")";
            }
        }

        protected override void ThirdRoundEvent(object obj, ElapsedEventArgs e)
        {
            if (IsImage) return;

            base.ThirdRoundEvent(obj, e);
        }

        public override void RoundEvent()
        {
            if (IsImage) return;

            CommandsProcessed.Clear();

            if (!DragonsSpineMain.Instance.Settings.ProcessEmptyWorld && World.GetNumberPlayersInMap(MapID) <= 0)
                return;

            if (this.CurrentCell != null)
                this.Map.UpdateCellVisible(this.CurrentCell);

            #region Despawning and unsummoning.
            if (this.RoundsRemaining >= 0)//mlt was >0 too many instances of having sent here with 0
            {
                this.RoundsRemaining--;

                if (this.RoundsRemaining <= 0)
                {
                    if (this.special.ToLower().Contains("figurine"))
                    {
                        if (this.CurrentCell == null) // TODO: determine where figurine object was not garbage collected after being slain
                        {
                            this.RemoveFromWorld();
                            return;
                        }

                        Cell cell = null;
                        int bitcount = 0;
                        int targetsfound = 0;
                        //loop through all visible cells
                        for (int ypos = -3; ypos <= 3; ypos += 1)
                        {
                            for (int xpos = -3; xpos <= 3; xpos += 1)
                            {
                                if (this.CurrentCell.visCells[bitcount])
                                {
                                    cell = Cell.GetCell(this.FacetID, this.LandID, this.MapID, this.X + xpos, this.Y + ypos, this.Z);
                                    foreach (Character chr in cell.Characters.Values)
                                    {
                                        if (chr != null && chr.Alignment != this.Alignment)
                                        {
                                            targetsfound++;
                                            break;
                                        }
                                    }
                                }
                                bitcount++;
                            }
                        }

                        if (this.CurrentCell == null)
                        {
                            this.RemoveFromWorld();
                            return;
                        }

                        if (targetsfound < 1)
                        {
                            Rules.DespawnFigurine(this);
                            return;
                        }
                        else
                        {
                            this.RoundsRemaining++;
                        }
                    }
                    else if (this.special.ToLower().Contains("despawn"))
                    {
                        Rules.UnsummonCreature(this);
                        return;
                    }
                }
            }
            #endregion

            NumAttackers = 0;
            CommandWeight = 0;
            InitiativeModifier = 0;

            if (IsSummoned) Age++;  // currently only summoned NPCs age

            if (Stunned == 0)
            {
                if (IsFeared)
                {
                    if (IsBlind) NPC.AIRandomlyMoveCharacter(this);
                    else
                    {
                        Character fearer = this.EffectsList[Effect.EffectTypes.Fear].Caster;

                        if (fearer != null && fearer.CurrentCell != null)
                            AI.BackAwayFromCell(this, fearer.CurrentCell);
                        else if (this.CurrentCell != null)
                            AI.BackAwayFromCell(this, this.CurrentCell);
                        else NPC.AIRandomlyMoveCharacter(this);
                    }

                    EmitSound(Sound.GetCommonSound(Sound.CommonSound.Feared));
                }
                else
                {
                    int hitsGain = 0;
                    int staminaGain = 0;
                    int manaGain = 0;

                    // Resting. Not diseased. Less than 3 rounds since last damage.
                    if (IsResting && !EffectsList.ContainsKey(Effect.EffectTypes.Contagion) &&
                        DamageRound < DragonsSpineMain.GameRound - 3)
                    {
                        #region Regenerate hits, stamina and mana.
                        if (this.Hits < this.HitsFull)  // increase stats
                        {
                            hitsGain++;
                            hitsGain += this.hitsRegen;
                        }

                        if (this.Stamina < this.StaminaFull)
                        {
                            staminaGain++;
                            staminaGain += this.staminaRegen;
                        }

                        if (this.Mana < this.ManaFull)
                        {
                            manaGain++;
                            manaGain += this.manaRegen;
                        }
                        #endregion
                    }

                    // more regeneration if meditating
                    if (IsMeditating && !EffectsList.ContainsKey(Effect.EffectTypes.Contagion) && DamageRound < DragonsSpineMain.GameRound - 3)
                    {
                        #region Regenerate hits, stamina and mana.
                        if (this.Hits < this.HitsFull)  // increase stats
                        {
                            hitsGain++;
                            hitsGain += this.hitsRegen;
                        }

                        if (this.Stamina < this.StaminaFull)
                        {
                            staminaGain++;
                            staminaGain += this.staminaRegen;
                        }

                        if (this.Mana < this.ManaFull)
                        {
                            manaGain++;
                            manaGain += this.manaRegen;
                        }
                        #endregion
                    }

                    // Confirm we're not going over full. (Full = Adjustment + Max)
                    if (hitsGain > 0)
                    {
                        if (Hits + hitsGain > HitsFull)
                            Hits = HitsFull;
                        else Hits += hitsGain;
                    }

                    if (staminaGain > 0)
                    {
                        if (Stamina + staminaGain > StaminaFull)
                            Stamina = StaminaFull;
                        else Stamina += staminaGain;
                    }

                    if (manaGain > 0)
                    {
                        if (Mana + manaGain > this.ManaFull)
                            Mana = ManaFull;
                        else Mana += manaGain;
                    }

                    if (Group == null || (Group != null && Group.GroupLeaderID == UniqueID))
                        DoAI();
                }
            }
            else Stunned -= 1;

            if (this.Group != null && this.Group.GroupLeaderID == this.UniqueID)
            {
                foreach (NPC groupNPC in new List<NPC>(this.Group.GroupNPCList))
                    if (groupNPC.UniqueID != this.UniqueID)
                        if (groupNPC.CurrentCell != this.CurrentCell)
                            groupNPC.CurrentCell = this.CurrentCell;
            }

            #region Follow Mode for Controlled NPCs
            if (this.PetOwner != null && this.canCommand && this.FollowID != 0 && this.CurrentCell != this.PetOwner.CurrentCell)
            {
                Character owner = this.PetOwner;

                // Search view for the Character we're following.
                if (GameSystems.Targeting.TargetAquisition.FindTargetInView(this, owner.Name, false, false) != null)
                {
                    PathTest pathTest = new PathTest("PathTest.Bug", this.CurrentCell)
                    {
                        IsInvisible = true
                    };
                    pathTest.AddToWorld();

                    if (!pathTest.SuccessfulPathTest(owner.CurrentCell))
                    {
                        this.AIGotoXYZ(owner.CurrentCell.X, owner.CurrentCell.Y, owner.CurrentCell.Z);
                    }
                    else
                    {
                        this.CurrentCell = owner.CurrentCell;
                    }

                    pathTest.RemoveFromWorld();
                }
                else
                {
                    bool foundPetOwner = GameSystems.Targeting.TargetAquisition.FindTargetInView(this, this.PetOwner) != null;

                    // check for segue cell here if we are not guarding and are following our owner
                    if (FollowID == PetOwner.UniqueID && !foundPetOwner && !IsGuarding && CurrentCell.IsStairsDown || CurrentCell.IsStairsUp || CurrentCell.IsOneHandClimbDown || CurrentCell.IsOneHandClimbUp ||
                        CurrentCell.IsTwoHandClimbDown || CurrentCell.IsTwoHandClimbUp)
                    {
                        Segue segue = this.CurrentCell.Segue;

                        if (segue != null)
                        {
                            Cell cellToCheck = Cell.GetCell(this.FacetID, segue.LandID, segue.MapID, segue.X, segue.Y, segue.Z);
                            // Cell above or below has our controller, let's go there.
                            if (cellToCheck.Characters.Values.Contains(this.PetOwner))
                            {
                                foundPetOwner = true;

                                if (this.CurrentCell.IsStairsDown && !this.CurrentCell.IsStairsUp)
                                    CommandTasker.ParseCommand(this, "down", "");
                                else if (this.CurrentCell.IsStairsUp && !this.CurrentCell.IsStairsDown)
                                    CommandTasker.ParseCommand(this, "up", "");
                                else if (this.CurrentCell.IsStairsUp && this.CurrentCell.IsStairsDown) // spiral stairs
                                {
                                    if (segue.Z < this.CurrentCell.Z)
                                        CommandTasker.ParseCommand(this, "down", "");
                                    else if (segue.Z > this.CurrentCell.Z)
                                        CommandTasker.ParseCommand(this, "up", "");
                                }
                                else if (this.CurrentCell.IsOneHandClimbDown || this.CurrentCell.IsTwoHandClimbDown)
                                    CommandTasker.ParseCommand(this, "climb", "down");
                                else if (this.CurrentCell.IsOneHandClimbUp || this.CurrentCell.IsTwoHandClimbUp)
                                    CommandTasker.ParseCommand(this, "climb", "up");
                            }
                        }
                        else
                        {
                            if (this.CurrentCell.IsUpSegue)
                                segue = Segue.GetDownSegue(this.CurrentCell); // get target down segue cell
                            else if (this.CurrentCell.IsDownSegue)
                                segue = Segue.GetUpSegue(this.CurrentCell); // get target up segue cell

                            if (segue != null)
                            {
                                Cell cellToCheck = Cell.GetCell(this.FacetID, segue.LandID, segue.MapID, segue.X, segue.Y, segue.Z);

                                // Cell above or below has our controller, let's go there.
                                if (cellToCheck.Characters.Values.Contains(this.PetOwner))
                                {
                                    foundPetOwner = true;

                                    if (this.CurrentCell.IsStairsDown)
                                        CommandTasker.ParseCommand(this, "down", "");
                                    else if (this.CurrentCell.IsStairsUp)
                                        CommandTasker.ParseCommand(this, "up", "");
                                    else if (this.CurrentCell.IsOneHandClimbDown || this.CurrentCell.IsTwoHandClimbDown)
                                        CommandTasker.ParseCommand(this, "climb", "down");
                                    else if (this.CurrentCell.IsOneHandClimbUp || this.CurrentCell.IsTwoHandClimbUp)
                                        CommandTasker.ParseCommand(this, "climb", "up");
                                }
                            }
                        }
                    }

                    // searched segues and couldn't find our owner
                    if (!foundPetOwner)
                    {
                        // Quest NPCs will shout a fail string if they cannot find their controller.
                        if (QuestList.Count > 0)
                            if (QuestList[0].FailStrings.Count > 0)
                                SendShout(Name + ": " + QuestList[0].FailStrings[Convert.ToInt16(Rules.Dice.Next(1, QuestList[0].FailStrings.Count))]);

                        // Remove perma root if the one we're following is not around and we're not supposed to be guarding.
                        if (EffectsList.ContainsKey(Effect.EffectTypes.Hello_Immobility) && !IsGuarding)
                            EffectsList[Effect.EffectTypes.Hello_Immobility].StopCharacterEffect();
                    }
                }

            }
            #endregion

            NumAttackers = 0;

            if (this.CurrentCell != null)
                this.Map.UpdateCellVisible(this.CurrentCell);
        }

        public virtual void DoAI()
        {
            if (this.IsDead || this.IsImage) return;
            if (this is PathTest) return;

            #region Regeneration of hits, stamina and mana if resting or meditating.
            int hitsGain = 0;
            int staminaGain = 0;
            int manaGain = 0;

            if (this.IsResting && !this.EffectsList.ContainsKey(Effect.EffectTypes.Contagion) && this.DamageRound < DragonsSpineMain.GameRound - 3)
            {
                #region Regenerate hits, stamina and mana.
                if (this.Hits < this.HitsFull)  // increase stats
                {
                    hitsGain++;
                    hitsGain += this.hitsRegen;
                }

                if (this.Stamina < this.StaminaFull)
                {
                    staminaGain++;
                    staminaGain += this.staminaRegen;
                }

                if (this.Mana < this.ManaFull)
                {
                    manaGain++;
                    manaGain += this.manaRegen;
                }
                #endregion
            }

            // more regeneration if meditating
            if (this.IsMeditating && !this.EffectsList.ContainsKey(Effect.EffectTypes.Contagion) && this.DamageRound < DragonsSpineMain.GameRound - 3)
            {
                #region Regenerate hits, stamina and mana.
                if (this.Hits < this.HitsFull)  // increase stats
                {
                    hitsGain++;
                    hitsGain += this.hitsRegen;
                }

                if (this.Stamina < this.StaminaFull)
                {
                    staminaGain++;
                    staminaGain += this.staminaRegen;
                }

                if (this.Mana < this.ManaFull)
                {
                    manaGain++;
                    manaGain += this.manaRegen;
                }
                #endregion
            }

            // Confirm we're not going over full. (Full = Adjustment + Max)
            if (hitsGain > 0)
            {
                if (this.Hits + hitsGain > this.HitsFull)
                    this.Hits = this.HitsFull;
                else this.Hits += hitsGain;
            }

            if (staminaGain > 0)
            {
                if (this.Stamina + staminaGain > this.StaminaFull)
                    this.Stamina = this.StaminaFull;
                else this.Stamina += staminaGain;
            }

            if (manaGain > 0)
            {
                if (this.Mana + manaGain > this.ManaFull)
                    this.Mana = this.ManaFull;
                else this.Mana += manaGain;
            }

            #endregion

            // Creatures not in a group, only group leaders, and no pets.
            if (this.Group == null || this.Group.GroupLeaderID == this.UniqueID || this.Group.GroupNPCList.Count <= 1)
            {
                // currently spell users do not group with non spell users
                if (this.Group != null && this.Group.GroupLeaderID == this.UniqueID && this.IsSpellUser && this.Group.GroupMemberWarmedSpells != null)
                    this.Group.GroupMemberWarmedSpells.Clear();

                if (AI.CreateContactList(this))
                {
                    AI.AssignHateFearLove(this);
                    AI.Rate(this);
                }
            }
        }

        public bool HasManaAvailable(int spellID)
        {
            if (this.castMode == CastMode.Unlimited)
                return true;

            if (this.Mana >= GameSpell.GetSpell(spellID).ManaCost)
                return true;

            return false;
        }

        public bool HasManaAvailable(string spellCommand)
        {
            if (this.castMode == CastMode.Unlimited)
                return true;

            if (this.Mana >= GameSpell.GetSpell(spellCommand).ManaCost)
                return true;

            return false;
        }

        public bool IsHealer() // returns true if this npc can heal itself and others
        {
            if (aiType == AIType.Enforcer)
                return true;

            return spellDictionary.ContainsKey(GameSpell.GetSpell("cure").ID); // since the cure spell is currently our only healing spell...
        }

        public void AIGotoNewZ(int x, int y, int z)
        {
            Cell newZCell = Cell.GetCell(FacetID, LandID, MapID, x, y, z);

            if (newZCell != null)
                CurrentCell = newZCell;
        } 

        public override void WriteToDisplay(string message)
        {
            if (DragonsSpineMain.Instance.Settings.DetailedNPCDisplayLogging)
            {
                if (!message.ToLower().Contains("you warm the spell") && !message.ToLower().Contains("you have been enchanted")
                    && !message.ToLower().Contains("you cast") && !message.ToLower().Contains("spell has worn off"))
                    Utils.Log(GetLogString() + ": " + message, Utils.LogType.NPCWriteToDisplay);
                else Utils.Log(GetLogString() + ": " + message, Utils.LogType.NPCWriteToDisplaySpells);
            }

            // TODO Enable this for future AI work (AI detecting messages) 3/15/2016 Eb
           // base.WriteToDisplay(message);

            if (PetOwner != null && (PetOwner is PC) && (PetOwner as PC).DisplayPetMessages)
                PetOwner.WriteToDisplay("<< " + Name + " >> " + message);
        }

        public override string GetLogString()
        {
            try
            {
                return "(NPC) [ID: " + this.npcID + " | UniqueNPCID: " + UniqueID + "] " + Name +
                        " [" + Utils.FormatEnumString(Alignment.ToString()) + " " + Utils.FormatEnumString(BaseProfession.ToString()) + "(" + Level + ")] (" +
                        (CurrentCell != null ? CurrentCell.GetLogString(false) : "Current Cell = null") + ")";
            }
            catch (Exception e)
            {
                Utils.LogException(e);

                try
                {
                    return "(NPC) NPCID: " + npcID + " (null)";
                }
                catch
                {
                    return "(NPC) [Exception in GetLogString()]";
                }
            }
        }

        public NPC CloneNPC()
        {
            return (NPC)MemberwiseClone();
        }

        

        public override string ToString()
        {
            try
            {
                return this.Land.Name + " (" + this.Land.LandID + "), " + this.Map.Name + " (" + this.Map.MapID + "), X: " + this.X + " Y: " + this.Y + " Z: " + this.Z;
                     
            }
            catch { }

            return base.ToString();
        }
    }
}
