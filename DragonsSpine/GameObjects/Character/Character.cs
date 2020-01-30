using System;
using System.Timers;
using System.Collections.Generic;
using System.Collections;
using DragonsSpine.GameWorld;
using GameSpell = DragonsSpine.Spells.GameSpell;
using TextManager = DragonsSpine.GameSystems.Text.TextManager;
using DragonsSpine.Autonomy.EntityBuilding;

namespace DragonsSpine
{
    public class Character
    {
        private static readonly object lockObject = new object();
        private static readonly Point[] directions = { new Point(-1, -1), new Point(-1, 0), new Point(-1, 1), new Point(0, -1), new Point(0, 1), new Point(1, -1), new Point(1, 0), new Point(1, 1) };

        /// <summary>
        /// The professions enum.
        /// The order of this enum cannot be changed (Because the SQL database still uses integers to determine class type.)
        /// </summary>
        public enum ClassType
        {
            None,
            Fighter, // 1
            Thaumaturge,
            Wizard,
            Martial_Artist,
            Thief, // 5
            Knight,
            Ravager,
            Sorcerer, // 8
            Druid,
            Ranger, // 10
            Berserker, // 11
            Shaman, // 12
            All // Used to determine Talent availability
        }

        #region Static Lists
        public static List<PC> LoginList = new List<PC>();
        public static List<PC> CharGenList = new List<PC>(); // list of all PCs at the character generator
        public static List<PC> MenuList = new List<PC>(); // list of all PCs at the menu
        public static List<PC> ConfList = new List<PC>(); // list of all PCs in the conference rooms
        public static List<Character> AllCharList = new List<Character>(); // list of all PCs and NPCs in the World
        public static List<PC> PCInGameWorld = new List<PC>(); // list of all PCs in the World
        public static List<NPC> NPCInGameWorld = new List<NPC>(); // list of all NPCs in the World
        public static List<Adventurer> AdventurersInGameWorldList = new List<Adventurer>();
        #endregion

        public static string[] POSSESSIVE = { "Its", "His", "Her" };
        public static string[] PRONOUN = { "It", "He", "She" };
        public static string[] PRONOUN_2 = { "It", "Him", "Her" };

        #region Constants
        protected const int DISPLAY_BUFFER_SIZE = 10;
        protected const int MAX_INPUT_LENGTH = 1024; //the longest line of input a person is allowed to enter
        public const int INACTIVITY_TIMEOUT = 25;
        public const int MAX_IGNORE = 30;
        public const int MAX_FRIENDS = 30;
        public const int MAX_UNARMED_WEIGHT = 100; // max weight allowed to receive unarmed combat benefits
        public const int MAX_MARKS = 5; // max marks allowed per account
        public const int MAX_CHARS = 8; // max chars allowed per account
        public const int MAX_BELT = 5; // max belt items allowed
        public const int MAX_LOCKER = 20; // max items allowed in locker
        public const int MAX_SACK = 20; // max items allowed in sack
        public const int MAX_POUCH = 20; // max items allowed in pouch
        public const int MAX_RINGS = 8; // max rings worn
        public const int MAX_MACROS = 20;
        public const int WARRIOR_SPECIALIZATION_LEVEL = 9; // required level for warrior types to specialize
        //public const int UDP_OUTPUT_BUFFER = 1024;
        #endregion

        public Timer RoundTimer;
        public Timer ThirdRoundTimer;
        public List<string> DisabledPassiveTalents; // added to Character object in case this would be used with NPC AI later

        public object TemporaryStorage; // currently only used to store ID of player chosen to be deleted, and to store demon names

        public int InitiativeModifier = 0;

        #region Private Data
        private Cell cell = null;
        private List<CommandTasker.CommandType> commandsProcessed = new List<CommandTasker.CommandType>();
        private bool dead = false;
        private int m_facetID = 0;
        private int m_followID = 0; // unique ID of a target this Character is following (also, targetting)
        private CharacterGroup group = null;
        private int groupInviter = -1; // the group leader ID of the group this character has been invited into        
        private bool isPC = false; //TODO get rid of this and use conversion check (ch as PC)
        private int m_landID = 0; // current Land.landID
        private string lastCommand;
        private string m_name;
        private int m_mapID = 0; // current Map.mapID
        private Globals.ePlayerState m_pcState; // player characters only
        private int m_uniqueID; // either playerID or assigned worldNPCID
        private int m_shielding = 0; // magical shielding
        private int m_targetID;
        private bool m_undead = false; // usage: no corpse, turnundead/light spell damage, does not flee in combat
        private int m_xcord = 0;
        private int m_ycord = 0;
        private int m_zcord = 0;
        private int m_preppedRound = 0;
        private List<NPC> m_petList = new List<NPC>(); // only NPCs can be pets
        private Character petOwner = null; // either NPCs or PCs can be pet owners
        private List<string> m_moveList = new List<string>(); // holds currently stacked set of move commands
        private int m_poisoned;
        #endregion

        public List<string> MoveList
        {
            get { return this.m_moveList; }
            set { this.m_moveList = value; }
        }

        #region Public Properties
        #region Location Properties

        public Cell CurrentCell
        {
            get
            {
                return this.cell;
            }
            set
            {
                lock (lockObject)
                {
                    if (this.cell != null)
                    {
                        this.cell.Remove(this);
                    }

                    this.cell = value;

                    if (cell != null)
                    {
                        this.FacetID = cell.FacetID;
                        this.LandID = cell.LandID;
                        this.MapID = cell.MapID;
                        this.X = cell.X;
                        this.Y = cell.Y;
                        this.Z = cell.Z;

                        if ((!this.IsPC || this.PCState == Globals.ePlayerState.PLAYING) && !cell.Characters.Values.Contains(this))
                        {
                            this.cell.Add(this);
                        }
                    }
                }
            }
        }

        public Facet Facet
        {
            get { return World.GetFacetByID(FacetID); }
        }

        public int FacetID
        {
            get { return this.m_facetID; }
            set { this.m_facetID = value; }
        }

        public int LandID
        {
            get { return this.m_landID; }
            set { this.m_landID = value; }
        }

        public int MapID
        {
            get { return this.m_mapID; }
            set { this.m_mapID = value; }
        }

        public int X
        {
            get { return this.m_xcord; }
            set { this.m_xcord = value; }
        }

        public int Y
        {
            get { return this.m_ycord; }
            set { this.m_ycord = value; }
        }

        public int Z
        {
            get { return this.m_zcord; }
            set { this.m_zcord = value; }
        }

        public Map Map
        {
            get
            {
                return World.GetFacetByID(this.FacetID).GetLandByID(this.LandID).GetMapByID(this.MapID);
            }
        }

        public Land Land
        {
            get
            {
                return World.GetFacetByID(this.FacetID).GetLandByID(this.LandID);
            }
        }
        #endregion

        public bool IsResting { get; set; } // set to true when the Character uses the rest command
        public bool IsMeditating { get; set; }

        public Globals.eAlignment Alignment
        {
            get; set;
        }
        public List<CommandTasker.CommandType> CommandsProcessed
        {
            get { return this.commandsProcessed; }
        }
        public CommandTasker.CommandType CommandType
        {
            set
            {
                this.commandsProcessed.Add(value);
            }
        }
        public int FollowID
        {
            get { return this.m_followID; }
            set { this.m_followID = value; }
        }
        public CharacterGroup Group
        {
            get { return this.group; }
            set { this.group = value; }
        }
        public int GroupInviter
        {
            get { return this.groupInviter; }
            set { this.groupInviter = value; }
        }
        public bool InUnderworld
        {
            get
            {
                if (this.Map.Name == "Praetoseba")
                {
                    return true;
                }
                return false;
            }
        }
        public bool IsBeltLargeSlotAvailable
        {
            get
            {
                if (this.beltList.Count >= Character.MAX_BELT) { return false; }
                for (int a = 0; a < this.beltList.Count; a++)
                {
                    Item item = (Item)this.beltList[a];
                    if (item.size == Globals.eItemSize.Belt_Large_Slot_Only)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public bool IsDead
        {
            get { return this.dead; }
            set { this.dead = value; }
        }
        public bool IsHybrid
        {
            get
            {
                return Array.IndexOf(World.HybridProfessions, this.BaseProfession) != -1;
            }
        }
        public bool IsIntelligenceCaster
        {
            get
            {
                return Array.IndexOf(World.IntelligenceCasters, this.BaseProfession) != -1;
            }
        }
        public bool IsLucky
        {
            get
            {
                if (this.EffectsList.ContainsKey(Effect.EffectTypes.Animal_Affinity))
                {
                    return true;
                }

                foreach (Item ring in this.GetRings())
                {
                    if (ring.special.IndexOf("lucky") != -1)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public bool IsPC
        {
            get { return this.isPC; }
            set { this.isPC = value; }
        }
        public int PreppedRound
        {
            get { return m_preppedRound; }
            set { m_preppedRound = value; }
        }
        public bool IsPureMelee
        {
            get
            {
                return Array.IndexOf(World.PureMeleeProfessions, this.BaseProfession) != -1;
            }
        }
        public bool IsSpellUser
        {
            get
            {
                return Array.IndexOf(World.SpellUsingProfessions, this.BaseProfession) != -1;
            }
        }
        public bool IsSpellWarmingProfession
        {
            get
            {
                return Array.IndexOf(World.SpellWarmingProfessions, this.BaseProfession) != -1;
            }
        }

        public bool IsUndead
        {
            get { return this.m_undead || EntityLists.UNDEAD.Contains(this.entity); }
            set { this.m_undead = value; }
        }
        public bool IsWisdomCaster
        {
            get
            {
                return Array.IndexOf(World.WisdomCasters, this.BaseProfession) != -1;
            }
        }

        //public bool IsWyrmKin
        //{
        //    get
        //    {
        //        return entity.ToString().ToLower().EndsWith("dragon") ||
        //            entity.ToString().ToLower().EndsWith("drake") ||
        //            EntityLists.WYRMKIN.Contains(this.entity);
        //    }
        //}

        public string LastCommand
        {
            get { return this.lastCommand; }
            set { this.lastCommand = value; }
        }
        public string Name
        {
            get { return this.m_name; }
            set
            {
                if (value.Length > GameSystems.Text.NameGenerator.NAME_MAX_LENGTH)
                    value = value.Substring(0, GameSystems.Text.NameGenerator.NAME_MAX_LENGTH);
                m_name = value;
            }
        }
        public string Notes { get; set; }
        public Globals.ePlayerState PCState
        {
            get { return this.m_pcState; }
            set { this.m_pcState = value; }
        }
        public Character PetOwner
        {
            get { return this.petOwner; }
            set { this.petOwner = value; }
        }
        public List<NPC> Pets
        {
            get { return this.m_petList; }
        }
        public int UniqueID
        {
            get { return this.m_uniqueID; }
            set { this.m_uniqueID = value; }
        }
        public int Poisoned
        {
            get { return m_poisoned; }
            set { m_poisoned = value; }
        }

        public int Shielding
        {
            get { return this.m_shielding; }
            set { this.m_shielding = value; }
        }

        public int TargetID
        {
            get { return this.m_targetID; }
            set { this.m_targetID = value; }
        }

        public int SackCountMinusGold
        {
            get
            {
                int count = 0;
                foreach (Item itm in this.sackList)
                {
                    if (itm.itemType != Globals.eItemType.Coin)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public bool DPSLoggingEnabled
        {
            get;
            set;
        }
        #endregion

        #region Effect Properties
        public bool CanBreatheWater { get { return HasEffect(Effect.EffectTypes.Breathe_Water); } }
        public bool CanFly { get { return HasEffect(Effect.EffectTypes.Flight); } }
        public bool IsHidden
        {
            get
            {
                return HasEffect(Effect.EffectTypes.Hide_in_Shadows);
            }
            set
            {
                if (value == false)
                {
                    if (this.EffectsList.ContainsKey(Effect.EffectTypes.Hide_in_Shadows))
                    {
                        if (!this.EffectsList[Effect.EffectTypes.Hide_in_Shadows].IsPermanent)
                        {
                            this.EffectsList[Effect.EffectTypes.Hide_in_Shadows].StopCharacterEffect();

                            if (this.IsPC && this.PCState == Globals.ePlayerState.PLAYING)
                            {
                                this.WriteToDisplay("You are no longer hidden!");
                            }
                        }
                    }

                }
            }
        }
        public bool IsFeared { get { return HasEffect(Effect.EffectTypes.Fear); } }
        public bool HasFeatherFall { get { return HasEffect(Effect.EffectTypes.Feather_Fall); } }
        public bool IsBlind { get { return HasEffect(Effect.EffectTypes.Blind); } }
        public bool IsPeeking { get { return HasEffect(Effect.EffectTypes.Peek); } }
        public bool IsWizardEye { get { return HasEffect(Effect.EffectTypes.Wizard_Eye); } }
        public bool IsPolymorphed { get { return HasEffect(Effect.EffectTypes.Polymorph); } }
        public bool IsShapeshifted { get { return HasEffect(Effect.EffectTypes.Shapeshift); } }
        public bool IsImage
        {
            get
            {
                if (IsPC) return false;
                return EffectsList.ContainsKey(Effect.EffectTypes.Image);
            }
        }
        public bool HasNightVision { get { return HasEffect(Effect.EffectTypes.Night_Vision); } }
        public bool HasKnightRing { get { return HasEffect(Effect.EffectTypes.Sacred_Ring); } } // also for Ravagers
        public bool HasSpeed { get { return HasEffect(Effect.EffectTypes.Speed); } }
        public bool HasOfuscation { get { return HasEffect(Effect.EffectTypes.Obfuscation); } }
        #endregion

        #region Effects and Quests Lists
        /// <summary>
        /// Holds a list of all temporary effects.
        /// </summary>
        public System.Collections.Concurrent.ConcurrentDictionary<Effect.EffectTypes, Effect> EffectsList =
            new System.Collections.Concurrent.ConcurrentDictionary<Effect.EffectTypes, Effect>();

        /// <summary>
        /// Holds a list of all worn effects, modified every time an item is worn or held.
        /// </summary>
        public List<Effect> WornEffectsList = new List<Effect>(); // effects worn

        /* questList
         * For players this is their list of completed and current quests.
         * For NPCs this is their list of quests they give or are a part of.
         */

        /// <summary>
        /// For PCs QuestList holds started and completed Quest objects. For NPCs QuestList holds Quest objects they are a part of.
        /// </summary>
        public List<GameQuest> QuestList = new List<GameQuest>();

        /// <summary>
        /// For PCs QuestFlags holds earned flags. For NPCs QuestFlags holds flags given when the NPC is slain by a PC.
        /// </summary>
        public List<string> QuestFlags = new List<string>();
        public List<string> ContentFlags = new List<string>();
        #endregion

        #region Protection
        protected int acidProtection = 0;
        public int AcidProtection
        {
            get { return this.acidProtection; }
            set { this.acidProtection = value; }
        }
        protected int fireProtection = 0;
        public int FireProtection
        {
            get { return this.fireProtection; }
            set { this.fireProtection = value; }
        }
        protected int coldProtection = 0;
        public int ColdProtection
        {
            get { return this.coldProtection; }
            set { this.coldProtection = value; }
        }
        protected int lightningProtection = 0;
        public int LightningProtection
        {
            get { return this.lightningProtection; }
            set { this.lightningProtection = value; }
        }
        protected int deathProtection = 0;
        public int DeathProtection
        {
            get { return this.deathProtection; }
            set { this.deathProtection = value; }
        }
        protected int poisonProtection = 0;
        public int PoisonProtection
        {
            get { return this.poisonProtection; }
            set { this.poisonProtection = value; }
        }
        #endregion

        #region Resistance
        protected int acidResistance = 0;
        public int AcidResistance
        {
            get { return this.acidResistance; }
            set { this.acidResistance = value; }
        }
        protected int fireResistance = 0;
        public int FireResistance
        {
            get { return this.fireResistance; }
            set { this.fireResistance = value; }
        }
        protected int coldResistance = 0;
        public int ColdResistance
        {
            get { return this.coldResistance; }
            set { this.coldResistance = value; }
        }
        private int lightningResistance = 0;
        public int LightningResistance
        {
            get { return this.lightningResistance; }
            set { this.lightningResistance = value; }
        }
        protected int deathResistance = 0;
        public int DeathResistance
        {
            get { return this.deathResistance; }
            set { this.deathResistance = value; }
        }
        protected int blindResistance = 0;
        public int BlindResistance
        {
            get { return this.blindResistance; }
            set { this.blindResistance = value; }
        }
        protected int fearResistance = 0;
        public int FearResistance
        {
            get { return this.fearResistance; }
            set { this.fearResistance = value; }
        }
        private int stunResistance = 0;
        public int StunResistance
        {
            get { return this.stunResistance; }
            set { this.stunResistance = value; }
        }
        protected int poisonResistance = 0;
        public int PoisonResistance
        {
            get { return this.poisonResistance; }
            set { this.poisonResistance = value; }
        }
        protected int zonkResistance = 0;
        public int ZonkResistance
        {
            get { return this.zonkResistance; }
            set { this.zonkResistance = value; }
        }
        #endregion      

        bool immortal = false; // when true heals impLevel >= GM to full stats each round
        public bool IsImmortal
        {
            get { return this.immortal; }
            set { this.immortal = value; }
        }

        //TODO: move this to NPC.cs??
        bool wasImmortal = false;
        public bool WasImmortal
        {
            get { return this.wasImmortal; }
            set { this.wasImmortal = value; }
        }

        bool invisible = false;	// can't be seen or contacted
        public bool IsInvisible
        {
            get { return this.invisible; }
            set { this.invisible = value; }
        }
        bool wasInvisible = false;
        public bool WasInvisible // record invisible boolean for Peek spell
        {
            get { return this.wasInvisible; }
            set { this.wasInvisible = value; }
        }

        public bool animal = false;	// true if this uses attackString and blockString in combat

        public bool canCommand = false; // true if the character can be commanded
        public short poisonous = 0; // greater than 0 is a chance to poison and poison amount max
        public string special = ""; // used to handle special qualities of a creature (eg: blueglow)
        public Character MyClone; // Used for Wizard Eye and Peek spell
        public string dirPointer = ".";

        public List<string> attackStrings = new List<string>();
        public List<string> blockStrings = new List<string>();

        short stunned = 0; // rounds of stun left
        public short Stunned
        {
            get { return this.stunned; }
            set
            {
                if (!this.IsPC || this.PCState == Globals.ePlayerState.PLAYING)
                {
                    if (value > this.stunned)
                    {
                        if (this.preppedSpell != null)
                        {
                            this.preppedSpell = null;
                            this.WriteToDisplay("You have lost your warmed spell.");
                            this.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                        }

                        // Assign a new leader of the group if stunned.
                        if ((this is NPC) && this.Group != null && this.Group.GroupLeaderID == this.UniqueID && this.Group.GroupMemberIDList.Count > 1)
                        {
                            foreach (NPC grpMember in this.Group.GroupNPCList)
                            {
                                if (grpMember != this)
                                {
                                    this.Group.GroupLeaderID = grpMember.UniqueID;
                                    break;
                                }
                            }
                        }
                    }
                }

                this.stunned = value;

            }
        }

        public short floating = 3; // 2 more rounds until drowning
        public GameSpell preppedSpell;
        public string visualKey = "";
        public string visualKeyOverride = "";
        public Globals.eGender gender = Globals.eGender.Male;
        public string race = ""; // character's race
        public Globals.eSpecies species = Globals.eSpecies.Unknown;
        public Autonomy.EntityBuilding.EntityLists.Entity entity;
        public string classFullName = "Fighter"; // full character class name

        ClassType baseProfession = ClassType.Fighter;

        public ClassType BaseProfession
        {
            get { return this.baseProfession; }
            set
            {
                this.baseProfession = value;
                PC.SetCharacterVisualKey(this); // images change if base profession changes
            }
        }

        #region Protocol and Client Specific
        public string protocol = "normal";
        public bool usingClient = false; // using the official Dragon's Spine client
        public bool sentWorldInformation = false; // sent world information
        public bool sentImplementorInformation = false; // sent implementor information
        #endregion

        #region Old Kesmai Protocol Update Bools
        public bool updateAll = true;
        public bool updateHits = true;
        public bool updateExp = true;
        public bool updateStam = true;
        public bool updateMP = true;
        public bool updateMap = true;
        public bool updateRight = true;
        public bool updateLeft = true;
        #endregion

        /// <summary>
        /// Key = spell ID, Value = spell chant
        /// </summary>
        public Dictionary<int, string> spellDictionary = new Dictionary<int, string>();

        /// <summary>
        /// Key = talent command, Value = time of last use
        /// </summary>
        public Dictionary<string, DateTime> talentsDictionary = new Dictionary<string, DateTime>();

        public double baseArmorClass = 10.0; // relates to AD&D Armor Class
        public double encumbrance = 0;

        public string MemorizedSpellChant = "";

        #region Immunities
        public bool immuneFire = false;	// true if immune to fire based attacks
        public bool immuneCold = false;	// true if immune to cold based attacks
        public bool immunePoison = false;	// true if immune to poison based attacks
        public bool immuneLightning = false;	// true if immune to lightning based attacks
        public bool immuneCurse = false;	// true if immune to curse magic (undead are already cursed)
        public bool immuneDeath = false;	// true if immune to death magic
        public bool immuneStun = false;	// true if immune to stun magic
        public bool immuneFear = false;	// true if immune to fear magic
        public bool immuneBlind = false;	// true if immune to blindness magic
        public bool immuneAcid = false;
        #endregion

        #region Class Specific
        //public List<Globals.eSkillType> fighterSpecialization = new List<Globals.eSkillType>();
        public Globals.eSkillType fighterSpecialization = Globals.eSkillType.None; // fighter specialization - increased skill gain, chance to hit, chance to block
        #endregion

        #region Regeneration
        public int hitsRegen = 0;
        public int staminaRegen = 0;
        public int manaRegen = 0;
        #endregion

        #region Statistics
        int level;
        public int Level
        {
            get { return this.level; }
            set { this.level = value; }
        }
        int hits;
        public virtual int Hits
        {
            get { return this.hits; }
            set
            {
                this.hits = value;
                if (this is PC && protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol && this.PCState == Globals.ePlayerState.PLAYING)
                {
                    ProtocolYuusha.SendPlayerHitsUpdate(this as PC);
                }
            }
        }
        int hitsmax;
        public int HitsMax
        {
            get { return this.hitsmax; }
            set { this.hitsmax = value; }
        }
        int hitsAdjustment = 0;
        public int HitsAdjustment
        {
            get { return this.hitsAdjustment; }
            set { this.hitsAdjustment = value; }
        }
        int hitsDoctored = 0;
        public int HitsDoctored
        {
            get { return this.hitsDoctored; }
            set { this.hitsDoctored = value; }
        }

        // The amount of absolute full hits. Includes adjustments and doctored hits.
        public int HitsFull
        {
            get { return hitsmax + hitsAdjustment + hitsDoctored; }
        }
        int mana;
        public int Mana
        {
            get { return this.mana; }
            set
            {
                this.mana = value;
                if (this.mana < 0) this.mana = 0;
                if (this is PC && this.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol && this.PCState == Globals.ePlayerState.PLAYING)
                {
                    ProtocolYuusha.SendPlayerManaUpdate(this as PC);
                }
            }
        }
        int manaMax;
        public int ManaMax
        {
            get { return this.manaMax; }
            set { this.manaMax = value; }
        }
        int manaAdjustment = 0;
        public int ManaAdjustment
        {
            get { return this.manaAdjustment; }
            set { this.manaAdjustment = value; }
        }
        public int ManaFull
        {
            get { return this.manaMax + this.manaAdjustment; }
        }
        long exp;
        public long Experience
        {
            get { return this.exp; }
            set
            {
                if (!this.IsPC)
                {
                    this.exp = value;
                }
                else
                {
                    long previousExpValue = exp;

                    exp = value;

                    if (PCState == Globals.ePlayerState.PLAYING)
                    {
                        if (this is PC && protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                            ProtocolYuusha.SendPlayerExperienceUpdate(this as PC, value - previousExpValue);

                        if (Rules.GetExpLevel(exp) > Rules.GetExpLevel(previousExpValue))
                        {
                            if (Hits < HitsFull || Stamina < StaminaFull)
                                WriteToDisplay("You have earned enough experience for your next level! Rest when you are at full health and stamina to advance.");
                            else
                                WriteToDisplay("You have earned enough experience for your next level! Rest to advance.");
                        }
                    }
                }
            }
        }
        int staminaMax;
        public int StaminaMax
        {
            get { return this.staminaMax; }
            set { this.staminaMax = value; }
        }
        int stamina;
        public int Stamina
        {
            get { return this.stamina; }
            set
            {
                this.stamina = value;
                if (this is PC && this.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol && this.PCState == Globals.ePlayerState.PLAYING)
                {
                    ProtocolYuusha.SendPlayerStaminaUpdate(this as PC);
                }
            }
        }
        int staminaAdjustment = 0;
        public int StaminaAdjustment
        {
            get { return this.staminaAdjustment; }
            set
            {
                this.staminaAdjustment = value;
            }
        }
        public int StaminaFull
        {
            get { return staminaMax + staminaAdjustment; }
        }
        int numKills;
        public int Kills
        {
            get { return this.numKills; }
            set { this.numKills = value; }
        }
        int numDeaths;
        public int Deaths
        {
            get { return this.numDeaths; }
            set { this.numDeaths = value; }
        }
        int age;
        public int Age
        {
            get { return this.age; }
            set { this.age = value; }
        }

        #endregion

        private int thac0Adjustment = 0;
        public int THAC0Adjustment
        {
            get { return this.thac0Adjustment; }
            set { this.thac0Adjustment = value; }
        }

        public List<int> FlaggedUniqueIDs = new List<int>(); // array of player IDs this character has flagged for self defense

        #region Attributes
        public int Strength
        {
            get;set;
        }
        public int Dexterity
        {
            get; set;
        }
        public int Intelligence
        {
            get; set;
        }
        public int Wisdom
        {
            get; set;
        }
        public int Constitution
        {
            get; set;
        }
        public int Charisma
        {
            get; set;
        }

        public int TempStrength
        {
            get; set;
        }
        public int TempDexterity
        {
            get; set;
        }
        public int TempIntelligence
        {
            get; set;
        }
        public int TempWisdom
        {
            get; set;
        }
        public int TempConstitution
        {
            get; set;
        }
        public int TempCharisma
        {
            get; set;
        }

        public int strengthAdd;
        public int dexterityAdd;
        #endregion

        public int NumAttackers { get; set; }

        #region Skills
        public long mace; // current skills
        public long bow;
        public long flail;
        public long dagger;
        public long rapier;
        public long twoHanded;
        public long staff;
        public long shuriken;
        public long sword;
        public long threestaff;
        public long halberd;
        public long unarmed;
        public long thievery;
        public long magic;
        public long bash;
        #endregion

        public int CommandWeight { get; set; }
        public int DamageRound { get; set; } = -3;

        /// <summary>
        /// Used for both NPC and PC classes. For NPCs it adds all visible friends/enemies. For PCs it is used for acquiring targets.
        /// </summary>
        public List<Character> seenList;

        /// <summary>
        /// Containers for storage.
        /// </summary>
        public List<Item> wearing; // worn items
        public List<Item> sackList; // sack for storage
        public List<Item> beltList; // belt for storage
        public List<Item> lockerList; // locker for storage
        public List<Item> pouchList; // pouch for storage

        public Item RightHand;
        public Item LeftHand;

        public Item RightRing1; // breaking rings down to each finger
        public Item RightRing2;
        public Item RightRing3;
        public Item RightRing4;
        public Item LeftRing1;
        public Item LeftRing2;
        public Item LeftRing3;
        public Item LeftRing4;

        public string attackSound;
        public string deathSound;
        public string idleSound;

        public Queue<string> inputCommandQueue; // our queue for storing complete commands once they are entirely received

        #region Constructor
        public Character()
        {
            Alignment = Globals.eAlignment.Lawful;
            Notes = "";

            this.ThirdRoundTimer = new Timer(DragonsSpineMain.MasterRoundInterval * 3);
            this.ThirdRoundTimer.Elapsed += new ElapsedEventHandler(ThirdRoundEvent);
            this.ThirdRoundTimer.AutoReset = true;
            this.RoundTimer = new Timer(DragonsSpineMain.MasterRoundInterval)
            {
                AutoReset = true
            };
            this.UniqueID = -1;

            this.wearing = new List<Item>();
            this.sackList = new List<Item>();
            this.beltList = new List<Item>();
            this.lockerList = new List<Item>();
            this.pouchList = new List<Item>();

            this.seenList = new List<Character>();
            this.Name = "Nobody";
            this.spellDictionary = new Dictionary<int, string>();
            this.DisabledPassiveTalents = new List<string>();
            this.DPSLoggingEnabled = false;
        }
        #endregion

        public int InputCommandQueueCount() { return inputCommandQueue.Count; }

        public string InputCommandQueueDequeue()
        {
            try
            {
                return inputCommandQueue.Dequeue();
            }
            catch (InvalidOperationException e)
            {
                Utils.LogException(e);
                Commands.GameCommand.GameCommandDictionary["rest"].Handler.OnCommand(this, "");
                return inputCommandQueue.Dequeue();
            }
        }

        public void AddToWorld()
        {
            if (IsPC)
            {
                PCState = Globals.ePlayerState.PLAYING;
                if (protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                    Write(ProtocolYuusha.GAME_ENTER);
            }
            else if (this is NPC)
            {
                if (UniqueID >= -1) UniqueID = World.GetNextNPCUniqueID(); // confirm the NPC has a proper unique ID
            }

            RoundTimer.Start();
            ThirdRoundTimer.Start();

            Cell.Move(this, null, LandID, MapID, X, Y, Z);

            IO.AddToWorld.Add(this);
            IO.pplToAddToWorld = true;
        }

        public void AddProtoClientToWorld()
        {
            Cell.Move(this, null, this.LandID, this.MapID, this.X, this.Y, this.Z);
            //this.RoundTimer.Start();
            //this.ThirdRoundTimer.Start();
            //IO.AddToWorld.Add(this);
            //IO.pplToAddToWorld = true;
        }

        public void RemoveFromWorld()
        {
            if (CurrentCell != null)
            {
                CurrentCell.Remove(this);

                CurrentCell = null;
            }

            // finally, starting 9/27/2019 PCs and NPCs both use these timers.
            RoundTimer.Stop();
            ThirdRoundTimer.Stop();

            if (IsWizardEye)
                EffectsList[Effect.EffectTypes.Wizard_Eye].StopCharacterEffect();

            if (IsPeeking)
                EffectsList[Effect.EffectTypes.Peek].StopCharacterEffect();

            if (EffectsList.ContainsKey(Effect.EffectTypes.Image))
                EffectsList[Effect.EffectTypes.Image].StopCharacterEffect();

            IO.RemoveFromWorld.Add(this);
            IO.pplToRemoveFromWorld = true;
        }

        #region Timer Related
        protected virtual void ThirdRoundEvent(object obj, ElapsedEventArgs e)
        {
            if (!IsDead)
            {
                #region Regenerate hits, stamina and mana if not diseased.
                if (!HasEffect(Effect.EffectTypes.Contagion))
                {
                    if (Hits < HitsFull) // gain a point of health
                    {
                        int hitsGain = 1;
                        hitsGain += hitsRegen;
                        if (Hits + hitsGain > HitsFull) Hits = HitsFull;
                        else if (Hits + hitsGain < 0)
                        {
                            Hits = 0;
                            Rules.DoDeath(this, null);
                            return;
                        }
                        else Hits += hitsGain;
                        updateHits = true;
                    }
                    if (Stamina < StaminaFull) // gain a point of stamina
                    {
                        if (Hits == HitsFull) // only gain stamina back if hits are at max
                        {
                            int stamGain = 1;
                            stamGain += staminaRegen;
                            if (Stamina + stamGain > StaminaFull) Stamina = StaminaFull;
                            else if (Stamina + stamGain < 0) Stamina = 0;
                            else Stamina += stamGain;
                            updateStam = true;
                        }
                    }

                    if (Mana < ManaFull && preppedSpell == null && !CommandsProcessed.Contains(CommandTasker.CommandType.Cast)) // gain a point of mana
                    {
                        int manaGain = 1;
                        manaGain += manaRegen;
                        if (Mana + manaGain > ManaFull) Mana = ManaFull; // confirm we didn't regen more mana than we have
                        else if (Mana + manaGain < 0) Mana = 0;
                        else Mana += manaGain;
                        updateMP = true;
                    }
                }
                #endregion

                #region Handle Poison
                if (Poisoned > 1) // do poison every three rounds
                {
                    if (Hits - Poisoned <= 0)
                    {
                        WriteToDisplay(TextManager.POISON_DEATH);
                        SendToAllInSight(this.Name + TextManager.POISON_DEATH_BROADCAST);

                        if (EffectsList.ContainsKey(Effect.EffectTypes.Poison) && EffectsList[Effect.EffectTypes.Poison].Caster != this)
                        {
                            Rules.GiveAEKillExp(EffectsList[Effect.EffectTypes.Poison].Caster, this);
                            Skills.GiveSkillExp(EffectsList[Effect.EffectTypes.Poison].Caster, this, Globals.eSkillType.Magic);
                            Rules.DoDeath(this, EffectsList[Effect.EffectTypes.Poison].Caster);
                            return;
                        }

                        Rules.DoDeath(this, null);
                    }
                    else
                    {
                        Hits -= Poisoned;
                        WriteToDisplay(TextManager.POISON_TREMOR);
                        SendToAllInSight(this.Name + TextManager.POISON_TREMOR_BROADCAST);
                        updateHits = true;
                        Poisoned--;

                        if (EffectsList.ContainsKey(Effect.EffectTypes.Poison) && EffectsList[Effect.EffectTypes.Poison].Caster != this)
                            Skills.GiveSkillExp(EffectsList[Effect.EffectTypes.Poison].Caster, Poisoned + 1, Globals.eSkillType.Magic);
                    }
                }
                else if (Poisoned == 1)
                {
                    if (Hits - Poisoned <= 0)
                    {
                        WriteToDisplay(TextManager.POISON_DEATH);
                        SendToAllInSight(Name + TextManager.POISON_DEATH_BROADCAST);
                        if (this.EffectsList[Effect.EffectTypes.Poison].Caster != this)
                        {
                            Rules.GiveAEKillExp(EffectsList[Effect.EffectTypes.Poison].Caster, this);
                            Skills.GiveSkillExp(EffectsList[Effect.EffectTypes.Poison].Caster, this, Globals.eSkillType.Magic);
                            Rules.DoDeath(this, EffectsList[Effect.EffectTypes.Poison].Caster);
                            return;
                        }
                    }
                    else
                    {
                        Hits -= Poisoned;
                        WriteToDisplay(TextManager.POISON_EXPIRATION);
                        updateHits = true;
                        if (EffectsList.ContainsKey(Effect.EffectTypes.Poison))
                            EffectsList[Effect.EffectTypes.Poison].StopCharacterEffect();
                        Poisoned = 0;
                    }
                }
                #endregion
            }
        }

        public virtual void RoundEvent()
        {

        }
        #endregion

        public void RemoveFromServer()
        {

            if (!(this is PC)) return;

            PC pc = null;

            try
            {
                try
                {
                    if (protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                        WriteLine(ProtocolYuusha.LOGOUT);
                }
                catch { }

                try
                {
                    IO.ProcessCommands(this);
                }
                catch { }

                pc = (PC)this;

                try
                {
                    #region Disband from group
                    if (this.Group != null)
                    {
                        this.Group.DisbandPlayerGroupMember(this.UniqueID, false);
                    }
                    #endregion
                }
                catch { }

                try
                {
                    #region Remove pets from world
                    foreach (NPC pet in new List<NPC>(pc.Pets)) //TODO move this to a seperate method, call it upon pet owner death?
                    {
                        pet.BreakFollowMode();

                        if (pet.IsSummoned)
                            Rules.UnsummonCreature(pet);
                        else if (pet.EffectsList.ContainsKey(Effect.EffectTypes.Charm_Animal))
                            pet.EffectsList[Effect.EffectTypes.Charm_Animal].StopCharacterEffect();
                        else if (pet.EffectsList.ContainsKey(Effect.EffectTypes.Command_Undead))
                            pet.EffectsList[Effect.EffectTypes.Command_Undead].StopCharacterEffect();
                        else if (pet.IsUndead) // animated dead
                        {
                            pet.canCommand = false;
                            pet.PetOwner = null;
                            if (this.Pets.Contains(pet))
                                this.Pets.Remove(pet);
                        }
                    }
                    #endregion
                }
                catch { }

                try
                {
                    #region Quick fix to remove player ID from flagged NPCs
                    foreach (NPC npc in NPCInGameWorld)
                    {
                        if (npc.FlaggedUniqueIDs.Contains(UniqueID))
                            npc.FlaggedUniqueIDs.RemoveAll(id => id == UniqueID);
                    }
                    #endregion
                }
                catch { }

                //Protocol.UpdateUserLists(); // send updated user lists to protocol users

                try
                {
                    Conference.FriendNotify(this as PC, false); // notify friends of logoff
                }
                catch { }

                try
                {
                    // Player may not quit with corpses.
                    if (pc.RightHand != null && pc.RightHand.itemType == Globals.eItemType.Corpse)
                    {
                        pc.CurrentCell.Add(pc.RightHand);
                        pc.UnequipRightHand(pc.RightHand);
                    }

                    if (pc.LeftHand != null && pc.LeftHand.itemType == Globals.eItemType.Corpse)
                    {
                        pc.CurrentCell.Add(pc.LeftHand);
                        pc.UnequipLeftHand(pc.LeftHand);
                    }
                }
                catch { }

                pc.lastOnline = DateTime.UtcNow;

                if(protocol != "Proto")
                    pc.DisconnectSocket(); // clean up the socket

                System.Threading.Tasks.Task saveTask = new System.Threading.Tasks.Task(pc.Save);
                saveTask.Start();
                //saveTask.Wait();

            }
            catch (Exception e)
            {
                Utils.LogException(e);
                
                if (pc != null)
                    pc.DisconnectSocket(); // clean up the socket
            }
        }

        public int GetWeaponSkillLevel(Item weapon)
        {
            if (!this.IsPC)
            {
                if (weapon == null)
                {
                    if (GetSkillExperience(Globals.eSkillType.Unarmed) == 0)
                    {
                        return this.level;
                    }
                    else
                    {
                        return Skills.GetSkillLevel(this.unarmed);
                    }
                }
                else
                {
                    if (GetSkillExperience(weapon.skillType) == 0)
                    {
                        return this.level;
                    }
                    else
                    {
                        return this.level;
                    }
                }
            }
            if (weapon == null)
            {
                return Skills.GetSkillLevel(this.unarmed);
            }
            else
            {
                return Skills.GetSkillLevel(GetSkillExperience(weapon.skillType));
            }
        }

        public long GetSkillExperience(Globals.eSkillType skillType)
        {
            switch (skillType)
            {
                case Globals.eSkillType.Bow:
                    return this.bow;
                case Globals.eSkillType.Sword:
                    return this.sword;
                case Globals.eSkillType.Two_Handed:
                    return this.twoHanded;
                case Globals.eSkillType.Unarmed:
                    return this.unarmed;
                case Globals.eSkillType.Staff:
                    return this.staff;
                case Globals.eSkillType.Dagger:
                    return this.dagger;
                case Globals.eSkillType.Polearm:
                    return this.halberd;
                case Globals.eSkillType.Rapier:
                    return this.rapier;
                case Globals.eSkillType.Shuriken:
                    return this.shuriken;
                case Globals.eSkillType.Magic:
                    return this.magic;
                case Globals.eSkillType.Mace:
                    return this.mace;
                case Globals.eSkillType.Flail:
                    return this.flail;
                case Globals.eSkillType.Threestaff:
                    return this.threestaff;
                case Globals.eSkillType.Thievery:
                    return this.thievery;
                case Globals.eSkillType.Bash:
                    return this.bash;
                default:
                    return -1;
            }
        }

        public void SetSkillExperience(Globals.eSkillType skillType, long amount)
        {
            switch (skillType)
            {
                case Globals.eSkillType.Bow:
                    this.bow = amount;
                    break;
                case Globals.eSkillType.Sword:
                    this.sword = amount;
                    break;
                case Globals.eSkillType.Two_Handed:
                    this.twoHanded = amount;
                    break;
                case Globals.eSkillType.Unarmed:
                    this.unarmed = amount;
                    break;
                case Globals.eSkillType.Staff:
                    this.staff = amount;
                    break;
                case Globals.eSkillType.Dagger:
                    this.dagger = amount;
                    break;
                case Globals.eSkillType.Polearm:
                    this.halberd = amount;
                    break;
                case Globals.eSkillType.Rapier:
                    this.rapier = amount;
                    break;
                case Globals.eSkillType.Shuriken:
                    this.shuriken = amount;
                    break;
                case Globals.eSkillType.Magic:
                    this.magic = amount;
                    break;
                case Globals.eSkillType.Mace:
                    this.mace = amount;
                    break;
                case Globals.eSkillType.Flail:
                    this.flail = amount;
                    break;
                case Globals.eSkillType.Threestaff:
                    this.threestaff = amount;
                    break;
                case Globals.eSkillType.Thievery:
                    this.thievery = amount;
                    break;
                case Globals.eSkillType.Bash:
                    this.bash = amount;
                    break;
            }
        }

        public void SendToAllInSight(string text) // send text to everyone in sight, except this character
        {
            if (this.IsInvisible && this is PC) return; // safety net so invisible developers do not unintentionally send anything

            int bitcount = 0;
            Cell curCell = null;

            for (int ypos = -3; ypos <= 3; ypos++)
            {
                for (int xpos = -3; xpos <= 3; xpos++)
                {
                    if (Cell.CellRequestInBounds(CurrentCell, xpos, ypos))
                    {
                        if (CurrentCell.visCells[bitcount]) // check the PC list, and char list of the cell
                        {
                            curCell = Cell.GetCell(FacetID, LandID, MapID, X + xpos, Y + ypos, Z);

                            if (curCell != null)
                            {
                                foreach (Character recipient in curCell.Characters.Values) // search for the character in the charlist of the cell
                                {
                                    // characters who are meditating are lost in a trance and do not see messages broadcast by other Characters
                                    // characters who are blind do not see broadcast text
                                    if (!recipient.IsMeditating && !recipient.IsBlind && recipient != this)
                                    {
                                        if (recipient.IsPC && IsPC) // players sending messages to other players
                                        {
                                            if (Array.IndexOf((recipient as PC).ignoreList, this.UniqueID) == -1) // ignore list
                                            {
                                                if ((recipient as PC).filterProfanity) // profanity filter
                                                    text = Conference.FilterProfanity(text);

                                                recipient.WriteToDisplay(text);

                                                // broadcast death to client users
                                                if (text.EndsWith(TextManager.IS_SLAIN_TEXT) &&
                                                    (recipient is PC) && (recipient as PC).protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                                                {
                                                    recipient.Write(ProtocolYuusha.GAME_CHARACTER_DEATH + UniqueID + ProtocolYuusha.GAME_CHARACTER_DEATH_END);
                                                }
                                            }
                                        }
                                        else if (recipient.IsPC && !IsPC) // non players sending messages to other players
                                        {
                                            recipient.WriteToDisplay(text);

                                            // broadcast death to client users
                                            if (text.EndsWith(TextManager.IS_SLAIN_TEXT) &&
                                                (recipient is PC) && (recipient as PC).protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                                            {
                                                recipient.Write(ProtocolYuusha.GAME_CHARACTER_DEATH + UniqueID + ProtocolYuusha.GAME_CHARACTER_DEATH_END);
                                            }
                                        }
                                        else
                                        {
                                            // TODO: possible future AI interpretations 10/19/2015 Eb
                                        }
                                    }
                                }
                            }
                        }
                        bitcount++;
                    }
                }
            }
        }

        public void SendToAllDEVInSight(string message)
        {
            int bitcount = 0;

            for (int ypos = -3; ypos <= 3; ypos++)
            {
                for (int xpos = -3; xpos <= 3; xpos++)
                {
                    if (Cell.CellRequestInBounds(this.CurrentCell, xpos, ypos))
                    {
                        if (this.CurrentCell.visCells[bitcount]) // check the PC list, and char list of the cell
                        {
                            Cell curCell = Cell.GetCell(this.FacetID, this.LandID, this.MapID, this.X + xpos, this.Y + ypos, this.Z);

                            if (curCell != null)
                            {
                                foreach (Character recipient in curCell.Characters.Values) // search for the character in the charlist of the cell
                                {
                                    if ((recipient is PC) && (recipient as PC).ImpLevel >= Globals.eImpLevel.DEVJR)
                                    {
                                        recipient.WriteToDisplay("DEVONLY: " + message);
                                    }
                                }
                            }
                        }
                        bitcount++;
                    }
                }
            }
        }

        public void SendSoundToAllInRange(string soundFile) // everyone but the sound source hears the sound
        {
            if (this.IsInvisible && this is PC) return;

            Cell cell = null;

            for (int ypos = -6; ypos <= 6; ypos += 1)
            {
                for (int xpos = -6; xpos <= 6; xpos += 1)
                {
                    if (Cell.CellRequestInBounds(this.CurrentCell, xpos, ypos))
                    {
                        cell = Cell.GetCell(this.FacetID, this.LandID, this.MapID, this.X + xpos, this.Y + ypos, this.Z);

                        if (cell != null)
                        {
                            foreach (Character chr in cell.Characters.Values) // search for the character in the charlist of the cell
                            {
                                if (chr != this)
                                {
                                    if (chr.IsPC)
                                    {
                                        if (Array.IndexOf((chr as PC).ignoreList, this.UniqueID) == -1)
                                        {
                                            chr.SendSound(soundFile);
                                        }
                                    }
                                    else
                                    {
                                        chr.SendSound(soundFile);
                                    }
                                }
                            }//end foreach
                        }
                    }
                }
            }
        }

        public void SendSound(string soundFile) // only the sound source hears the sound
        {
            try
            {
                if (soundFile == null || soundFile == "")
                {
                    return;
                }
                else if (soundFile.Length < 4)
                {
                    Utils.Log("Sound file length less than 4 at sendSound(" + soundFile + ")", Utils.LogType.SystemWarning);
                    return;
                }

                if (this.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                {
                    this.Write(ProtocolYuusha.SOUND + soundFile + ProtocolYuusha.VSPLIT + "0" + ProtocolYuusha.VSPLIT + "0" + ProtocolYuusha.SOUND_END);
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public void EmitSound(string soundFile) // everyone hears the sound
        {
            try
            {
                if (soundFile == null || soundFile == "")
                {
                    return;
                }

                else if (soundFile.Length < 4)
                {
                    Utils.Log("Sound file length less than 4 at character.EmitSound(" + soundFile + ")", Utils.LogType.SystemWarning);
                    return;
                }

                // safety -- do not emit sounds while invisible if you're a PC.
                if (this is PC && IsInvisible)
                {
                    if (protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                    {
                        WriteToDisplay("You would've emitted a sound while invisible. This message is for DEV informative purposes only.");

                        Write(ProtocolYuusha.SOUND + soundFile + ProtocolYuusha.VSPLIT + 0 + ProtocolYuusha.VSPLIT + (int)Map.Direction.None + ProtocolYuusha.SOUND_END);
                    }
                    return;
                }

                Cell soundCell = null;

                for (int ypos = -6; ypos <= 6; ypos += 1)
                {
                    for (int xpos = -6; xpos <= 6; xpos += 1)
                    {
                        if (Cell.CellRequestInBounds(CurrentCell, xpos, ypos))
                        {
                            soundCell = Cell.GetCell(FacetID, LandID, MapID, X + xpos, Y + ypos, Z);
                            if (soundCell != null && soundCell.Characters.Count > 0)
                            {
                                try
                                {
                                    foreach (Character chr in soundCell.Characters.Values)
                                    {
                                        if (chr == null) continue;
                                        if (chr is NPC) continue; // Currently do not emit sounds to NPCs.

                                        if (chr.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                                        {
                                            if (chr != this)
                                            {
                                                if (Array.IndexOf((chr as PC).ignoreList, UniqueID) == -1) // check ignore list
                                                {
                                                    var distance = Cell.GetCellDistance(X, Y, chr.X, chr.Y);
                                                    var direction = Convert.ToInt32(Map.GetDirection(CurrentCell, chr.CurrentCell));
                                                    chr.Write(ProtocolYuusha.SOUND + soundFile + ProtocolYuusha.VSPLIT + distance + ProtocolYuusha.VSPLIT + direction + ProtocolYuusha.SOUND_END);
                                                }
                                            }
                                            else
                                            {
                                                var distance = Cell.GetCellDistance(X, Y, chr.X, chr.Y);
                                                var direction = Convert.ToInt32(Map.GetDirection(CurrentCell, chr.CurrentCell));
                                                chr.Write(ProtocolYuusha.SOUND + soundFile + ProtocolYuusha.VSPLIT + distance + ProtocolYuusha.VSPLIT + direction + ProtocolYuusha.SOUND_END);
                                            }
                                        }
                                    }
                                }
                                catch (InvalidOperationException)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utils.Log("Sound failure at EmitSound(" + soundFile + ")", Utils.LogType.SystemFailure);
                Utils.LogException(e);
            }
        }

        public void SendShout(string text)
        {
            Cell cell = null;

            for (int ypos = -6; ypos <= 6; ypos += 1)
            {
                for (int xpos = -6; xpos <= 6; xpos += 1)
                {
                    try
                    {
                        if (Cell.CellRequestInBounds(this.CurrentCell, xpos, ypos))
                        {
                            cell = Cell.GetCell(this.FacetID, this.LandID, this.MapID, this.X + xpos, this.Y + ypos, this.Z);

                            if (cell != null && cell.Characters.Count > 0)
                            {
                                foreach (Character chr in cell.Characters.Values)
                                {
                                    if (Rules.DetectInvisible(this, chr))
                                    {
                                        if (chr != this && chr != null && chr.IsPC && this.IsPC) // players sending shouts to other players
                                        {
                                            if ((chr as PC).ignoreList != null && Array.IndexOf((chr as PC).ignoreList, this.UniqueID) == -1)
                                            {
                                                if ((chr as PC).filterProfanity) // filter profanity if setting is true
                                                {
                                                    text = Conference.FilterProfanity(text);
                                                }
                                                chr.WriteToDisplay(GetTextDirection(chr, this.X, this.Y) + text);
                                            }
                                        }
                                        else if (chr != this && chr != null && chr.IsPC && !this.IsPC) // non players sending shouts to players
                                        {
                                            chr.WriteToDisplay(GetTextDirection(chr, this.X, this.Y) + text);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Utils.LogException(e);
                    }
                }
            }
            return;

        }

        public static string GetTextDirection(Character ch, int x, int y)
        {
            if (x < ch.X && y < ch.Y)
            {
                return "To the northwest you hear ";
            }
            else if (x < ch.X && y > ch.Y)
            {
                return "To the southwest you hear ";
            }
            else if (x > ch.X && y < ch.Y)
            {
                return "To the northeast you hear ";
            }
            else if (x > ch.X && y > ch.Y)
            {
                return "To the southeast you hear ";
            }
            else if (x == ch.X && y > ch.Y)
            {
                return "To the south you hear ";
            }
            else if (x == ch.X && y < ch.Y)
            {
                return "To the north you hear ";
            }
            else if (x < ch.X && y == ch.Y)
            {
                return "To the west you hear ";
            }
            else if (x > ch.X && y == ch.Y)
            {
                return "To the east you hear ";
            }
            return "You hear ";
        }

        public void SendToAll(string text) // send to everyone in the world
        {
            for (int a = 0; a < PCInGameWorld.Count; a++)
            {
                Character ch = Character.PCInGameWorld[a];
                if (ch != null)
                {
                    ch.WriteToDisplay(text);
                }
            }
        }

        public virtual void Write(string message) // sends a text message to the player
        {
        }

        public virtual void WriteLine(string message, ProtocolYuusha.TextType textType) // write a line with a carriage return, include protocol
        {
        }

        public virtual void WriteLine(string message) // write a line with a carriage return line feed, exclude protocol
        {
        }

        public virtual void WriteToDisplay(string message, ProtocolYuusha.TextType textType)
        {
        }

        public virtual void WriteToDisplay(string message)
        {
        }

        public string FilterDisplayText(string text)
        {
            text = text.Replace("%n", this.Name);
            text = text.Replace("%r", this.race);
            text = text.Replace("%R", this.race);
            text = text.Replace("%a", Utils.FormatEnumString(this.Alignment.ToString()).ToLower());
            text = text.Replace("%A", Utils.FormatEnumString(this.Alignment.ToString()));
            text = text.Replace("%c", this.classFullName.ToLower());
            text = text.Replace("%C", this.classFullName);
            text = text.Replace("%t", World.CurrentDailyCycle.ToString().ToLower());
            text = text.Replace("%T", World.CurrentDailyCycle.ToString());
            text = text.Replace("%G", this.gender.ToString());
            text = text.Replace("%g", this.gender.ToString().ToLower());
            text = text.Replace("%p", Character.PRONOUN[Convert.ToInt32(this.gender)].ToLower());
            text = text.Replace("%P", Character.PRONOUN[Convert.ToInt32(this.gender)]);

            return text;
        }

        /// <summary>
        /// Determine which hand an item is in by name.
        /// </summary>
        /// <param name="itemName">The name of the item.</param>
        /// <returns>0 if not found, 1 if in right hand, 2 if in left hand.</returns>
        public int WhichHand(string itemName)
        {
            if (int.TryParse(itemName, out int uniqueItemID))
                return WhichHand(uniqueItemID);

            if (RightHand != null)
            {
                if (itemName.ToLower() == "right" || itemName.ToLower() == "r" || this.RightHand.name.ToLower() == itemName || this.RightHand.UniqueID.ToString() == itemName)
                    return (int)Globals.eWearOrientation.Right;
            }

            if (LeftHand != null)
            {
                if (itemName.ToLower() == "left" || itemName.ToLower() == "l" || this.LeftHand.name.ToLower() == itemName || this.LeftHand.UniqueID.ToString() == itemName)
                    return (int)Globals.eWearOrientation.Left;
            }

            return (int)Globals.eWearOrientation.None;
        }

        public int WhichHand(int uniqueItemID)
        {
            if (RightHand != null && RightHand.UniqueID == uniqueItemID)
                return (int)Globals.eWearOrientation.Right;
            else if (LeftHand != null && LeftHand.UniqueID == uniqueItemID)
                return (int)Globals.eWearOrientation.Left;

            return (int)Globals.eWearOrientation.None;
        }

        /// <summary>
        /// Find a held item by item ID.
        /// </summary>
        /// <param name="id">The unique item ID of the item, or the database itemID of the item.</param>
        /// <returns>The item, if found, and null otherwise.</returns>
        public Item FindHeldItem(int id)
        {
            if (RightHand != null && (RightHand.UniqueID == id || RightHand.itemID == id)) return RightHand;
            if (LeftHand != null && (LeftHand.UniqueID == id || LeftHand.itemID == id)) return LeftHand;
            return null;
        }

        /// <summary>
        /// Find a held item by name.
        /// </summary>
        /// <param name="itemName">The item name to look for.</param>
        /// <returns>The item, if found, and null otherwise.</returns>
        public Item FindHeldItem(string itemName)
        {
            if (RightHand != null && RightHand.name == itemName)
                return RightHand;

            if (LeftHand != null && LeftHand.name == itemName)
                return LeftHand;

            if (int.TryParse(itemName, out int uniqueItemID))
                return FindHeldItem(uniqueItemID);

            var itemLoc = WhichHand(itemName);

            if (itemLoc == (int)Globals.eWearOrientation.Right)
                return RightHand;
            else if (itemLoc == (int)Globals.eWearOrientation.Left)
                return LeftHand;
            return null;
        }

        /// <summary>
        /// Unequips a held item if it is in a hand.
        /// </summary>
        /// <param name="itemID">The held item name to look for.</param>
        /// <param name="itemLoc">The hand location the item was retrieved from.</param>
        /// <returns>The held item.</returns>
        public Item GetHeldItem(int itemID, out int itemLoc)
        {
            Item item = null;
            itemLoc = (int)Globals.eWearOrientation.None;

            if (RightHand.itemID == itemID)
            {
                itemLoc = (int)Globals.eWearOrientation.Right;
                item = this.RightHand;
                UnequipRightHand(item);
            }
            else if (LeftHand.itemID == itemID)
            {
                itemLoc = (int)Globals.eWearOrientation.Left;
                item = this.LeftHand;
                UnequipLeftHand(item);
            }

            return item;
        }

        /// <summary>
        /// Unequips a held item if it is in a hand.
        /// </summary>
        /// <param name="itemName">The held item name to look for.</param>
        /// <returns>The held item.</returns>
        public Item GetHeldItem(string itemName)
        {
            var itemLoc = WhichHand(itemName);

            Item item = null;

            if (itemLoc == (int)Globals.eWearOrientation.Right)
            {
                item = this.RightHand;
                this.UnequipRightHand(this.RightHand);
            }
            else if (itemLoc == (int)Globals.eWearOrientation.Left)
            {
                item = this.LeftHand;
                this.UnequipLeftHand(this.LeftHand);
            }

            return item;
        }

        /// <summary>
        /// Check hands, ground and inventory for an item by name.
        /// </summary>
        /// <param name="itemName">The name of the item.</param>
        /// <returns>The found item.</returns>
        public Item FindAnyItemInSight(string itemName)
        {
            try
            {
                // hands
                if (this.WhichHand(itemName) == (int)Globals.eWearOrientation.Right)
                {
                    return RightHand;
                }
                if (this.WhichHand(itemName) == (int)Globals.eWearOrientation.Left)
                {
                    return LeftHand;
                }

                // ground
                if (Item.FindItemOnGround(itemName.ToLower(), this.FacetID, this.LandID, this.MapID, this.X, this.Y, this.Z) != null)
                {
                    return Item.FindItemOnGround(itemName.ToLower(), this.FacetID, this.LandID, this.MapID, this.X, this.Y, this.Z);
                }

                // inventory
                foreach (Item item in wearing)
                {
                    if (item != null && item.name.ToLower().Equals(itemName.ToLower()))
                    {
                        return item;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return null;
            }
        }

        public int FindFirstLeftRing() //returns first finger with a ring on left hand
        {
            if (this.LeftRing1 != null) return 1;
            if (this.LeftRing2 != null) return 2;
            if (this.LeftRing3 != null) return 3;
            if (this.LeftRing4 != null) return 4;
            return 0;
        }

        public int FindFirstRightRing() //returns first finger with a ring on right hand
        {
            if (this.RightRing1 != null) return 1;
            if (this.RightRing2 != null) return 2;
            if (this.RightRing3 != null) return 3;
            if (this.RightRing4 != null) return 4;
            return 0;
        }

        public void WarmSpell(int spellID)
        {
            preppedSpell = GameSpell.GetSpell(spellID); // load the prepped spell

            if (preppedSpell == null) // failed to load the spell
            {
                Utils.Log("Failed at Character.prepSpell(" + spellID + ") Could not find spellID.", Utils.LogType.SystemFailure);
                return;
            }

            preppedSpell.WarmSpell(this); // send warming message

            if (DragonsSpineMain.Instance.Settings.DetailedSpellLogging)
            {
                if (IsPC) // log
                    Utils.Log(GetLogString() + " warmed " + GameSpell.GetLogString(preppedSpell) + ".", Utils.LogType.SpellWarmingFromPlayer);
                else
                    Utils.Log(GetLogString() + " warmed " + GameSpell.GetLogString(preppedSpell) + ".", Utils.LogType.SpellWarmingFromCreature);
            }
        }

        public bool RemoveWornItem(Item item)
        {
            if (this is PC && item.IsCursed() && !IsImmortal)
            {
                this.WriteToDisplay("You are unable to remove " + item.shortDesc + ". It has attached itself to your body.");
                return false;
            }

            if (wearing.Contains(item))
            {
                wearing.Remove(item);
                if (item.effectType.Length > 0) { Effect.RemoveWornEffectFromCharacter(this, item); }
            }

            return true;
        }

        public Item RemoveWornItem(string name) // remove item worn
        {
            foreach (Item item in this.wearing)
            {
                if (item.name == name || (int.TryParse(name, out int uniqueID) && uniqueID == item.UniqueID))
                {
                    wearing.Remove(item);
                    if (item.effectType.Length > 0) { Effect.RemoveWornEffectFromCharacter(this, item); }
                    return item;
                }
            }
            return null;
        }

        public Item RemoveFromSack(Item item)
        {
            if (this.sackList.Contains(item))
            {
                this.sackList.Remove(item);
                return item;
            }
            else return null;
        }

        public Item RemoveFromSack(string name) // remove item from sack
        {
            Item itemFromSack = null;
            for (int index = 1; index <= this.sackList.Count; index++)
            {
                if (this.sackList[index - 1].name == name || (int.TryParse(name, out int uniqueID) && uniqueID == this.sackList[index - 1].UniqueID))
                {
                    itemFromSack = this.sackList[index - 1];
                    this.sackList.RemoveAt(index - 1);
                    break;
                }
            }
            return itemFromSack;
        }

        public Item RemoveFromPouch(string name) // remove item from sack
        {
            Item itemFromPouch = null;
            for (int index = 1; index <= this.pouchList.Count; index++)
            {
                if (this.pouchList[index - 1].name == name || (int.TryParse(name, out int uniqueID) && uniqueID == this.pouchList[index - 1].UniqueID))
                {
                    itemFromPouch = this.pouchList[index - 1];
                    this.pouchList.RemoveAt(index - 1);
                    break;
                }
            }
            return itemFromPouch;
        }

        public Item RemoveFromLocker(string name) // remove item from locker
        {
            Item itemFromLocker = null;
            for (int index = 1; index <= this.lockerList.Count; index++)
            {
                if (this.lockerList[index - 1].name == name || (int.TryParse(name, out int uniqueID) && uniqueID == this.lockerList[index - 1].UniqueID))
                {
                    itemFromLocker = this.lockerList[index - 1];
                    this.lockerList.RemoveAt(index - 1);
                    break;
                }
            }
            return itemFromLocker;
        }

        public Item RemoveFromBelt(string name) // remove item from belt
        {
            Item itemFromBelt = null;
            for (int index = 1; index <= this.beltList.Count; index++)
            {
                if (this.beltList[index - 1].name == name || (int.TryParse(name, out int uniqueID) && uniqueID == this.beltList[index - 1].UniqueID))
                {
                    itemFromBelt = this.beltList[index - 1];
                    this.beltList.RemoveAt(index - 1);
                    break;
                }
            }
            return itemFromBelt;
        }

        public Item RemoveFromBelt(int worldItemID)
        {
            Item itemFromBelt = null;
            for (int index = 1; index <= this.beltList.Count; index++)
            {
                if (this.beltList[index - 1].UniqueID == worldItemID)
                {
                    itemFromBelt = this.beltList[index - 1];
                    this.beltList.RemoveAt(index - 1);
                    break;
                }
            }
            return itemFromBelt;
        }

        public string RightHandItemName() // for internal use
        {
            string righthand;

            righthand = "          ";

            if (this.RightHand != null)
            {
                righthand = this.RightHand.name;
                if (this.RightHand.name == "crossbow" && this.RightHand.IsNocked)
                {
                    righthand = righthand + "*";
                }
            }
            return righthand;
        }

        public string LeftHandItemName() // for internal use
        {
            string lefthand;

            lefthand = "          ";
            if (this.RightHand != null && this.RightHand.IsNocked && this.RightHand.name != "crossbow")
            {
                lefthand = "*";
            }
            else
            {
                if (this.LeftHand != null)
                {
                    lefthand = this.LeftHand.name;
                    if (this.LeftHand.name == "crossbow" && this.LeftHand.IsNocked)
                    {
                        lefthand = lefthand + "*";
                    }
                }
            }
            return lefthand;
        }

        public string GetVisibleArmorName()
        {
            string armor = "none";

            Item torso = this.GetInventoryItem(Globals.eWearLocation.Torso);
            Item back = this.GetInventoryItem(Globals.eWearLocation.Back);
            Item shoulders = this.GetInventoryItem(Globals.eWearLocation.Shoulders);
            Item legs = this.GetInventoryItem(Globals.eWearLocation.Legs);
            Item waist = this.GetInventoryItem(Globals.eWearLocation.Waist);

            if (this.animal && this is NPC)
            {                
                if (this is NPC)
                {
                    EntityLists.Entity npcEntity = (this as NPC).entity;
                    if (EntityLists.FELINE.Contains(npcEntity) || EntityLists.CANINE.Contains(npcEntity) ||
                        EntityLists.CANIFORMS.Contains(npcEntity) || npcEntity == EntityLists.Entity.Axe_Glacier_Yeti)
                        armor = "fur";
                    else if (EntityLists.ARTHROPOD.Contains(npcEntity))
                        armor = "carapace";
                    else if (EntityLists.IsHellspawn(this) || EntityLists.WYRMKIN.Contains(npcEntity) || npcEntity == EntityLists.Entity.Wyvern || npcEntity == EntityLists.Entity.Gargoyle)
                        armor = "scales";
                    else if (EntityLists.INCORPOREAL.Contains(npcEntity))
                        armor = "mist";
                    else if (EntityLists.GRIFFIN_ARCHETYPE.Contains(npcEntity) || EntityLists.AVIAN.Contains(npcEntity))
                        armor = "feathers";
                    else if (npcEntity == EntityLists.Entity.Ent || npcEntity == EntityLists.Entity.Shambling_Mound)
                        armor = "bark";
                }
                else if (shoulders != null && ((this as NPC).tanningResult != null && (this as NPC).tanningResult.ContainsKey(shoulders.itemID))) armor = shoulders.name.ToLower();
                else if (back != null && ((this as NPC).tanningResult != null && (this as NPC).tanningResult.ContainsKey(back.itemID))) armor = back.name.ToLower();
                else if (torso != null) armor = Utils.FormatEnumString(torso.armorType.ToString()).ToLower();
            }
            else if ((this is NPC) && ((this as NPC).tanningResult != null && (this as NPC).tanningResult.Count > 0))
            {
                if (shoulders != null && (this as NPC).tanningResult.ContainsKey(shoulders.itemID))
                    armor = shoulders.name.ToLower();
                else if (back != null && (this as NPC).tanningResult.ContainsKey(back.itemID))
                    armor = back.name.ToLower();
                else if (torso != null && (this as NPC).tanningResult.ContainsKey(torso.itemID))
                    armor = Utils.FormatEnumString(torso.armorType.ToString()).ToLower();
                if (this is NPC)
                {
                    EntityLists.Entity ent = (this as NPC).entity;
                    if (ent == EntityLists.Entity.Trog || ent == EntityLists.Entity.Troll)
                    {
                        armor = "leather";
                    }
                }
            }
            else // normal critter / npc
            {
                if (back != null && back.baseType != Globals.eItemBaseType.Armor) armor = "scabbard";
                if (shoulders != null) armor = shoulders.name.ToLower(); // set shoulders 1st
                if (legs != null) armor = Utils.FormatEnumString(legs.armorType.ToString()).ToLower();  //legs.name.ToLower();
                if (waist != null) armor = waist.name.ToLower();
                if (torso != null) armor = Utils.FormatEnumString(torso.armorType.ToString()).ToLower(); //torso.name.ToLower(); 
                if (back != null) armor = back.name.ToLower();
                if (this is NPC)
                {
                    EntityLists.Entity ent = (this as NPC).entity;
                    if (ent == EntityLists.Entity.Gargoyle)
                    {
                        armor = "scales";
                    }
                    if(ent == EntityLists.Entity.Statue)
                    {
                        armor = "stone";
                    }
                }

            }

            if (armor == "breastplate" || armor == "steel" || armor == "banded") armor = "plate";
            if (armor == "platemail") armor = "plate";
            if (armor == "scalemail") armor = "scales";
            if (armor == "chainmail") armor = "chain";
            if (armor == "studded") armor = "leather";

            return armor;
        }

        public static void ValidatePlayer(PC ch)
        {
            if (!ch.IsPC) { return; }

            try
            {
                if (ch.PCState == Globals.ePlayerState.PLAYING && ch.CurrentCell == null)
                {
                    ch.WriteToDisplay("You are currently out of map bounds. Please report this. Attempting to place you at a safe point...");
                    Cell safeCell = Cell.GetCell(ch.FacetID, ch.LandID, ch.MapID, ch.Map.KarmaResX, ch.Map.KarmaResY, ch.Map.KarmaResZ);

                    if (safeCell == null)
                    {
                        ch.WriteToDisplay("Current map safe point failed. Placing you at default safe point.");
                        ch.CurrentCell = Cell.GetCell(ch.FacetID, 0, 0, 53, 7, 0);
                    }
                    else
                    {
                        ch.CurrentCell = safeCell;
                        ch.WriteToDisplay("Safe point placement successful.");
                    }
                }
            }
            catch(Exception e)
            {
                Utils.LogException(e);
            }

            try
            {
                if (!ch.IsDead)
                {
                    if (ch.IsBlind || ch.IsResting || ch.IsMeditating)
                    {
                        if (ch.FollowID != 0)
                            ch.BreakFollowMode();
                    }

                    if (ch.RightHand != null)
                    {
                        if (ch.RightHand is Corpse && (ch.RightHand as Corpse).IsPlayerCorpse)
                        {
                            PC pc = PC.GetOnline((ch.RightHand as Corpse).Ghost.Name);
                            if (pc == null || (pc != null && !pc.IsDead))
                            {
                                ch.WriteToDisplay("The corpse of " + (ch.RightHand as Corpse).Ghost.Name + " disintegrates.");
                                ch.UnequipRightHand(ch.RightHand);
                            }
                            else
                            {
                                if (pc.CurrentCell != ch.CurrentCell)
                                {
                                    pc.CurrentCell = ch.CurrentCell;
                                    pc.Timeout = Character.INACTIVITY_TIMEOUT;
                                }
                            }
                        }
                        else if (ch.RightHand.itemType == Globals.eItemType.Coin && ch.RightHand.coinValue > 300000)
                        {
                            Utils.Log(ch.GetLogString() + " is carrying " + ch.RightHand.coinValue + " coins in their right hand.", Utils.LogType.CoinLogging);
                        }
                    }

                    if (ch.LeftHand != null)
                    {
                        if (ch.LeftHand is Corpse && (ch.LeftHand as Corpse).IsPlayerCorpse)
                        {
                            PC pc = PC.GetOnline((ch.LeftHand as Corpse).Ghost.Name);
                            if (pc == null || (pc != null && !pc.IsDead))
                            {
                                ch.WriteToDisplay("The corpse of " + (ch.LeftHand as Corpse).Ghost.Name + " disintegrates.");
                                ch.UnequipLeftHand(ch.LeftHand);
                            }
                            else
                            {
                                if (pc.CurrentCell != ch.CurrentCell)
                                {
                                    pc.CurrentCell = ch.CurrentCell;
                                    pc.Timeout = Character.INACTIVITY_TIMEOUT;
                                }
                            }
                        }
                        else if (ch.LeftHand.itemType == Globals.eItemType.Coin && ch.LeftHand.coinValue > 300000)
                        {
                            Utils.Log(ch.GetLogString() + " is carrying " + ch.LeftHand.coinValue + " coins in their left hand.", Utils.LogType.CoinLogging);
                        }
                    }
                }

                #region Set High Skills
                if (ch.mace > ch.highMace) { ch.highMace = ch.mace; }
                if (ch.bow > ch.highBow) { ch.highBow = ch.bow; }
                if (ch.flail > ch.highFlail) { ch.highFlail = ch.flail; }
                if (ch.dagger > ch.highDagger) { ch.highDagger = ch.dagger; }
                if (ch.rapier > ch.highRapier) { ch.highRapier = ch.rapier; }
                if (ch.twoHanded > ch.highTwoHanded) { ch.highTwoHanded = ch.twoHanded; }
                if (ch.staff > ch.highStaff) { ch.highStaff = ch.staff; }
                if (ch.shuriken > ch.highShuriken) { ch.highShuriken = ch.shuriken; }
                if (ch.sword > ch.highSword) { ch.highSword = ch.sword; }
                if (ch.threestaff > ch.highThreestaff) { ch.highThreestaff = ch.threestaff; }
                if (ch.halberd > ch.highHalberd) { ch.highHalberd = ch.halberd; }
                if (ch.unarmed > ch.highUnarmed) { ch.highUnarmed = ch.unarmed; }
                if (ch.thievery > ch.highThievery) { ch.highThievery = ch.thievery; }
                if (ch.magic > ch.highMagic) { ch.highMagic = ch.magic; }
                if (ch.bash > ch.highBash) { ch.highBash = ch.bash; }
                #endregion

                if (ch.Shielding < 0)
                    ch.Shielding = 0;

                if (!ch.IsImmortal)
                {
                    #region Validate Encumbrance
                    ch.encumbrance = ch.GetEncumbrance();
                    #endregion
                }
                else
                {
                    ch.encumbrance = 0;
                }

                if (Rules.BreakHideSpell(ch))
                    ch.IsHidden = false;

                if (ch.ImpLevel < Globals.eImpLevel.GM)
                {
                    #region Confirm Not Invisible If Not Dead
                    if (!ch.IsDead && ch.ImpLevel < Globals.eImpLevel.GM)
                    {
                        if (ch.IsInvisible)
                            ch.IsInvisible = false;
                    }
                    #endregion

                    #region Validate Maximum Shield
                    if (ch.Shielding > ch.Land.MaxShielding)
                        ch.Shielding = ch.Land.MaxShielding;
                    #endregion

                    #region Validate Maximum Hits, Mana and Stamina
                    int maximumHits = Rules.GetMaximumHits(ch);
                    int maximumMana = Rules.GetMaximumMana(ch);
                    int maximumStamina = Rules.GetMaximumStamina(ch);

                    //validate character hp, stamina, mana
                    if (ch.HitsMax > maximumHits)
                    {
                        ch.HitsMax = maximumHits;
                        ch.Hits = ch.HitsFull;
                    }
                    if (ch.StaminaMax > maximumStamina)
                    {
                        ch.StaminaMax = maximumStamina;
                        ch.Stamina = ch.StaminaFull;

                    }
                    if (ch.ManaMax > maximumMana)
                    {
                        ch.ManaMax = maximumMana;
                        ch.Mana = ch.ManaFull;
                    }
                    #endregion
                }

                if (ch.ImpLevel >= Globals.eImpLevel.GM && ch.IsImmortal) // keeps ghods at max hp, stamina, mana if immortal is true
                {
                    ch.Hits = ch.HitsFull;
                    ch.Stamina = ch.StaminaFull;
                    ch.Mana = ch.ManaFull;
                    ch.Stunned = 0;
                    ch.floating = 4;
                    ch.encumbrance = 0;
                }

                if (ch.bankGold > Merchant.MAX_BANK_GOLD)
                {
                    double overLimitAmount = ch.bankGold;
                    ch.bankGold = Merchant.MAX_BANK_GOLD;
                    ch.WriteToDisplay("Your bank gold is over the limit and has been adjusted. Please visit the forums if you have any questions.");
                    Utils.Log(ch.GetLogString() + " had their bank gold automatically adjusted from " + overLimitAmount + " to " + Merchant.MAX_BANK_GOLD + ".", Utils.LogType.SystemWarning);
                }
            }
            catch(Exception e)
            {
                Utils.LogException(e);
            }

            PC.SetCharacterVisualKey(ch);
        }

        public bool WearItem(Item item)
        {
            if (item != null)
            {
                if (wearing.Contains(item))
                {
                    Utils.Log(GetLogString() + " attempted to add same item to worn items more than once.", Utils.LogType.Debug);
                    return false;
                }

                if (item.wearLocation == Globals.eWearLocation.None)
                    return false;

                if (this is PC && item.IsAttunedToOther(this))
                {
                    string isare = "is";
                    if (item.name.ToLower().EndsWith("s")) { isare = "are"; }
                    this.WriteToDisplay("The " + item.name + " " + isare + " soulbound to another individual.");
                    return false;
                }

                if (!item.AlignmentCheck(this))
                {
                    this.WriteToDisplay("The " + item.name + " does not match your alignment.");
                    return false;
                }

                int inventoryCount = 0;

                foreach (Item wornItem in this.wearing)
                {
                    if (wornItem.wearLocation == item.wearLocation)
                        inventoryCount++;
                }

                if (inventoryCount >= Globals.Max_Wearable[(int)item.wearLocation])
                {
                    if (this is PC && DragonsSpine.DragonsSpineMain.ServerStatus == DragonsSpineMain.ServerState.Running)
                    {
                        var itemPlural = item.name + "s";
                        if (itemPlural.EndsWith("ss")) itemPlural = itemPlural.Substring(0, itemPlural.Length - 1);
                        this.WriteToDisplay("You are already wearing the maximum amount of " + itemPlural + ".");
                    }
                    return false;
                }

                wearing.Add(item);

                if (this is PC && item.attuneType == Globals.eAttuneType.Wear) { item.AttuneItem(this); }

                if (item.effectType.Length > 0)
                    Effect.AddWornEffectToCharacter(this, item); // add worn effect

                if (item == RightHand) // empty the hand the item came from
                    UnequipRightHand(item);
                else if (item == this.LeftHand)
                    UnequipLeftHand(item);

                if (protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol && this.PCState == Globals.ePlayerState.PLAYING)
                {
                    ProtocolYuusha.SendCharacterStats(this as PC, this);
                    ProtocolYuusha.SendCharacterInventory(this);
                }
                return true;
            }
            else return false;
        }

        public bool BeltItem(Item item)
        {
            if (item != null)
            {
                if (this.beltList.Contains(item))
                {
                    Utils.Log(this.GetLogString() + " attempted to add same item to belt more than once.", Utils.LogType.Debug);
                    return false;
                }

                // TODO: this may cause items to disappear 11/17/2015 Eb
                if (this.beltList.Count >= Character.MAX_BELT)
                    return false;

                // insert the item to the first index
                this.beltList.Insert(0, item);

                if (item.attuneType == Globals.eAttuneType.Take && this is PC) { item.AttuneItem(this); }

                return true;
            }
            else return false;
        }

        public bool SackItem(Item item)
        {
            if (item != null)
            {
                if (this.sackList.Contains(item))
                {
                    Utils.Log(this.GetLogString() + " attempted to add same item to sack more than once.", Utils.LogType.Debug);
                    return false;
                }

                if (item.size > Globals.eItemSize.Belt_Or_Sack)
                    return false;

                if (item.itemType == Globals.eItemType.Coin)
                {
                    foreach (Item coins in new List<Item>(this.sackList))
                    {
                        if (coins.itemType == Globals.eItemType.Coin)
                        {
                            coins.coinValue += item.coinValue;
                            return true;
                        }
                    }
                    this.sackList.Add(item);
                    return true;
                }

                if (this.SackCountMinusGold >= MAX_SACK)
                    return false;

                if (item.attuneType == Globals.eAttuneType.Take && this is PC) { item.AttuneItem(this); }

                // TODO: track down this bug
                //if (this is NPC && this.sackList.Count > 2) return false;

                this.sackList.Insert(0, item);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool PouchItem(Item item)
        {
            if (item != null)
            {
                if (this.pouchList.Contains(item))
                {
                    Utils.Log(this.GetLogString() + " attempted to add same item to pouch more than once.", Utils.LogType.Debug);
                    return false;
                }

                if (item.size > Globals.eItemSize.Sack_Or_Pouch)
                    return false;

                if (item.itemType == Globals.eItemType.Coin)
                    return false;

                if (this.pouchList.Count >= MAX_POUCH)
                    return false;

                if (item.attuneType == Globals.eAttuneType.Take && this is PC) { item.AttuneItem(this); }

                // TODO: track down this bug
                //if (this is NPC && this.sackList.Count > 2) return false;

                this.pouchList.Insert(0, item);

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool EquipRightHand(Item item)
        {
            if (item != null)
            {
                if (item.IsArtifact() && PossessesItem(item.itemID))
                {
                    WriteToDisplay(TextManager.ARTIFACT_POSSESSED);
                    return false;
                }

                if (LeftHand != null && LeftHand.IsNocked && LeftHand.name != "crossbow" && !LeftHand.returning) // un-nock opposite hand if not crossbow
                {
                    LeftHand.IsNocked = false;
                }

                RightHand = item; // place the item in character's right hand

                if (item.attuneType == Globals.eAttuneType.Take) { item.AttuneItem(this); } // attune the item

                if (!item.IsAttunedToOther(this) && item.AlignmentCheck(this)) // add effects
                {
                    if (item.effectType.Length > 0 && item.wearLocation == Globals.eWearLocation.None)
                        Effect.AddWornEffectToCharacter(this, item);
                }

                if (item.size >= Globals.eItemSize.Belt_Large_Slot_Only && IsHidden)
                {
                    if (!EffectsList[Effect.EffectTypes.Hide_in_Shadows].IsPermanent && (!this.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) && !CurrentCell.IsAlwaysDark))
                    {
                        IsHidden = false;
                    }
                }
                return true;
            }
            return false;
        }

        public bool EquipLeftHand(Item item)
        {
            if (item != null)
            {
                if (item.IsArtifact() && PossessesItem(item.itemID))
                {
                    WriteToDisplay(TextManager.ARTIFACT_POSSESSED);
                    return false;
                }

                if (RightHand != null && RightHand.IsNocked && RightHand.name != "crossbow" && !RightHand.returning) // un-nock opposite hand if not crossbow
                {
                    RightHand.IsNocked = false;
                }

                LeftHand = item; // place the item in character's left hand

                if (item.attuneType == Globals.eAttuneType.Take) { item.AttuneItem(this); } // attune the item

                if (!item.IsAttunedToOther(this) && item.AlignmentCheck(this)) // add effects if not soulbound to another character
                {
                    if (item.effectType.Length > 0 && item.wearLocation == Globals.eWearLocation.None)
                        Effect.AddWornEffectToCharacter(this, item);
                }

                if (item.size == Globals.eItemSize.Belt_Large_Slot_Only || item.size == Globals.eItemSize.No_Container)
                {
                    if (IsHidden)
                    {
                        if (EffectsList.ContainsKey(Effect.EffectTypes.Hide_in_Shadows))
                        {
                            if (!EffectsList[Effect.EffectTypes.Hide_in_Shadows].IsPermanent)
                            {
                                IsHidden = false;
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public bool UnequipRightHand(Item item)
        {
            if (item != null)
            {
                RightHand = null; // empty right hand
                if (!item.IsAttunedToOther(this) && item.AlignmentCheck(this))
                {
                    if (item.effectType.Length > 0 && item.wearLocation == Globals.eWearLocation.None)
                        Effect.RemoveWornEffectFromCharacter(this, item);
                }
                return true;
            }
            return false;
        }

        public bool UnequipLeftHand(Item item)
        {
            if (item != null)
            {
                LeftHand = null; // empty left hand
                if (!item.IsAttunedToOther(this) && item.AlignmentCheck(this))
                {
                    if (item.effectType.Length > 0 && item.wearLocation == Globals.eWearLocation.None
                        )Effect.RemoveWornEffectFromCharacter(this, item);
                }
                return true;
            }
            return false;
        }

        public bool EquipEitherHand(Item item)
        {
            if (item == null || item.name.ToLower() == "undefined")
                return false;

            if (item.IsArtifact() && PossessesItem(item.itemID))
            {
                WriteToDisplay(TextManager.ARTIFACT_POSSESSED);
                return false;
            }

            if (RightHand == null)
            {
                return EquipRightHand(item);
            }
            else if (LeftHand == null)
            {
                return EquipLeftHand(item);
            }
            else
            {
                WriteToDisplay("Your hands are full.");
                return false;
            }
        }

        /// <summary>
        /// Check for a free hand. Returns 1 if right hand is empty, 2 if left hand is empty and 0 if neither hands are empty.
        /// </summary>
        /// <returns>Returns 1 if right hand is empty, 2 if left hand is empty and 0 if neither hands are empty.</returns>
        public int GetFirstFreeHand()
        {
            if (RightHand == null)
                return (int)Globals.eWearOrientation.Right;
            if (LeftHand == null)
                return (int)Globals.eWearOrientation.Left;
            return (int)Globals.eWearOrientation.None;
        }

        public bool EquipEitherHandOrDrop(Item item)
        {
            if (item.IsArtifact() && PossessesItem(item.itemID))
            {
                WriteToDisplay(TextManager.ARTIFACT_POSSESSED);

                if (!CurrentCell.Items.Contains(item))
                    CurrentCell.Add(item);

                return false;
            }

            if (RightHand == null)
            {
                return EquipRightHand(item);
            }
            else if (LeftHand == null)
            {
                return EquipLeftHand(item);
            }
            else
            {
                // no dupes
                if (!CurrentCell.Items.Contains(item))
                    CurrentCell.Add(item);

                return false;
            }
        }

        public List<Item> GetRings()
        {
            List<Item> ringsList = new List<Item>();

            if (this.RightRing1 != null)
            {
                ringsList.Add(this.RightRing1);
            }
            if (this.RightRing2 != null)
            {
                ringsList.Add(this.RightRing2);
            }
            if (this.RightRing3 != null)
            {
                ringsList.Add(this.RightRing3);
            }
            if (this.RightRing4 != null)
            {
                ringsList.Add(this.RightRing4);
            }
            if (this.LeftRing1 != null)
            {
                ringsList.Add(this.LeftRing1);
            }
            if (this.LeftRing2 != null)
            {
                ringsList.Add(this.LeftRing2);
            }
            if (this.LeftRing3 != null)
            {
                ringsList.Add(this.LeftRing3);
            }
            if (this.LeftRing4 != null)
            {
                ringsList.Add(this.LeftRing4);
            }
            return ringsList;
        }

        public virtual Character CloneCharacter()
        {
            return (Character)this.MemberwiseClone();
        }

        public virtual string GetLogString()
        {
            if (this is PC) return (this as PC).GetLogString();
            else if (this is Merchant) return (this as Merchant).GetLogString();
            else if (this is Adventurer) return (this as Adventurer).GetLogString();
            else return (this as NPC).GetLogString();
        }

        public string GetAgeDescription(bool fullDescription)
        {
            int index = 0;

            if (this.Age < World.AgeCycles[0])
            {
                index = 0;
            }
            else if (this.Age >= World.AgeCycles[0] && this.Age < World.AgeCycles[1])
            {
                index = 1;
            }
            else if (this.Age >= World.AgeCycles[1] && this.Age < World.AgeCycles[2])
            {
                index = 2;
            }
            else if (this.Age >= World.AgeCycles[2] && this.Age < World.AgeCycles[3])
            {
                index = 3;
            }
            else if (this.Age >= World.AgeCycles[3] && this.Age < World.AgeCycles[4])
            {
                index = 4;
            }
            else
            {
                index = 5;
            }

            string ageDescription;

            if (EntityLists.WYRMKIN.Contains(entity))//this.IsWyrmKin)
            {
                ageDescription = World.age_wyrmKin[index];
            }
            else
            {
                ageDescription = World.age_humanoid[index];
            }

            if (!fullDescription)
            {
                ageDescription = ageDescription.Substring(ageDescription.IndexOf(" "));
            }
            return ageDescription;
        }

        public void BreakFollowMode()
        {
            if (!IsDead && IsPC && FollowID != 0)
            {
                AllCharList.ForEach(delegate (Character chr)
                {
                    if (chr.UniqueID == FollowID)
                    {
                        WriteToDisplay("You are no longer following " + chr.Name + ".");
                    }
                });
            }

            this.FollowID = 0;
        }

        public Item GetInventoryItem(Globals.eWearLocation wearLoc)
        {
            foreach (Item item in this.wearing)
            {
                if (item.wearLocation == wearLoc)
                    return item;
            }

            return null;
        }

        public Item GetInventoryItem(Globals.eWearLocation wearLoc, Globals.eWearOrientation wearOrientation)
        {
            if (wearLoc == Globals.eWearLocation.Finger)
            {
                switch (wearOrientation)
                {
                    case Globals.eWearOrientation.LeftRing1:
                        return this.LeftRing1;
                    case Globals.eWearOrientation.LeftRing2:
                        return this.LeftRing2;
                    case Globals.eWearOrientation.LeftRing3:
                        return this.LeftRing3;
                    case Globals.eWearOrientation.LeftRing4:
                        return this.LeftRing4;
                    case Globals.eWearOrientation.RightRing1:
                        return this.RightRing1;
                    case Globals.eWearOrientation.RightRing2:
                        return this.RightRing2;
                    case Globals.eWearOrientation.RightRing3:
                        return this.RightRing3;
                    case Globals.eWearOrientation.RightRing4:
                        return this.RightRing4;
                }
            }

            foreach (Item item in this.wearing)
            {
                if (item.wearLocation == wearLoc && item.wearOrientation == wearOrientation)
                {
                    return item;
                }
            }

            return null;
        }

        public Item GetSpecificRing(bool right, int number)
        {
            switch (number)
            {
                case 1:
                    if (right)
                        return RightRing1;
                    return LeftRing1;
                case 2:
                    if (right)
                        return RightRing2;
                    return LeftRing2;
                case 3:
                    if (right)
                        return RightRing3;
                    return LeftRing3;
                case 4:
                    if (right)
                        return RightRing4;
                    return LeftRing4;
                default:
                    return null;
            }
        }

        public void SetSpecificRing(bool right, int number, Item ring)
        {
            switch (number)
            {
                case 1:
                    if (right)
                        RightRing1 = ring;
                    else LeftRing1 = ring;
                    break;
                case 2:
                    if (right)
                        RightRing2 = ring;
                    else LeftRing2 = ring;
                    break;
                case 3:
                    if (right)
                        RightRing3 = ring;
                    else LeftRing3 = ring;
                    break;
                case 4:
                    if (right)
                        RightRing4 = ring;
                    else LeftRing4 = ring;
                    break;
            }
        }

        public double GetEncumbrance()
        {
            double encumbrance = 0;

            #region Rings
            foreach (Item rItem in GetRings())
                encumbrance += rItem.weight;
            #endregion

            #region Sack
            foreach (Item sItem in sackList)
            {
                if (sItem.itemType != Globals.eItemType.Coin)
                    encumbrance += sItem.weight;
                else
                    encumbrance += (int)(sItem.coinValue / 100);
            }
            #endregion

            #region Pouch
            foreach(Item pItem in pouchList)
                encumbrance += pItem.weight;
            #endregion

            #region Belt
            foreach (Item bItem in beltList)
                encumbrance += bItem.weight;
            #endregion

            #region Worn Items
            foreach (Item wItem in wearing)
                encumbrance += wItem.weight;
            #endregion

            #region Right Hand
            if (RightHand != null)
            {
                if (RightHand.itemType != Globals.eItemType.Coin)
                    encumbrance += RightHand.weight;
                else
                    encumbrance += (int)(RightHand.coinValue / 100);
            }
            #endregion

            #region Left Hand
            if (LeftHand != null)
            {
                if (LeftHand.itemType != Globals.eItemType.Coin)
                    encumbrance += LeftHand.weight;
                else
                    encumbrance += (int)(LeftHand.coinValue / 100);
            }
            #endregion

            return encumbrance;
        }

        public GameQuest GetQuest(int questID)
        {
            foreach (GameQuest q in this.QuestList)
            {
                if (q.QuestID == questID)
                {
                    return q;
                }
            }
            return null;
        }

        public static string RaceToString(Character chr)
        {
            string race = chr.race;

            if (race.ToLower() == "random") race = "";

            if (race == "Barbarian") race = "the plains";
            else if (race == "" && (chr.species == Globals.eSpecies.Human || (chr is NPC && EntityLists.HUMAN.Contains(chr.entity)))) race = chr.Map.Name; // they're a local
            else if (race == "") race = "a distant land";

            return race;
        }

        public void DoAIMove()
        {
            if (this is NPC && !(this as NPC).IsMobile) { return; }

            this.MoveList.Clear();

            if (this is NPC) (this as NPC).localCells.Clear();

            try
            {
                List<Cell> cList = Map.GetAdjacentCells(this.CurrentCell, this);

                if (cList != null)
                {
                    Cell nCell = (Cell)cList[Rules.Dice.Next(cList.Count)];

                    this.AIGotoXYZ(nCell.X, nCell.Y, nCell.Z);
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return;
            }
            return;
        }

        public void DoNextListMove() // process the next available move in the creature's movelist
        {
            // Handle one directional move at a time due to 
            // current interpret command code
            //if (this.debug > 4) return;

            int speed = (this is NPC ? (this as NPC).Speed : 3);

            for (int i = 0; i < speed && MoveList.Count != 0; i++)
            {
                if (MoveList[0] == null || MoveList[0] == "")
                {
                    break;
                }

                CommandTasker.ParseCommand(this, (string)MoveList[0], "");

                if (MoveList.Count > 0)
                {
                    MoveList.RemoveAt(0);

                    if (MoveList.Count == 0)
                    {
                        try
                        {
                            if (!(this is NPC) || ((!this.animal && !this.IsUndead && !(this as NPC).IsSpectral && (this as NPC).MostHated == null && Rules.CheckPerception(this)) || (this as NPC).HasPatrol))
                            {
                                Cell cell = null;

                                for (int xpos = -1; xpos <= 1; xpos++)
                                {
                                    for (int ypos = -1; ypos <= 1; ypos++)
                                    {
                                        if (Cell.CellRequestInBounds(this.CurrentCell, xpos, ypos))
                                        {
                                            cell = Cell.GetCell(this.FacetID, this.LandID, this.MapID, this.X + xpos, this.Y + ypos, this.Z);

                                            if (cell != null)
                                            {
                                                if ((this is NPC) && (this as NPC).MostHated == null && cell.IsOpenDoor &&
                                                    Cell.GetCellDistance(this.X, this.Y, cell.X, cell.Y) == 1 &&
                                                    cell.Items.Count == 0 &&
                                                    cell.Characters.Count == 0 && Rules.CheckPerception(this))
                                                {
                                                    CommandTasker.ParseCommand(this, "close", "door " + Map.GetDirection(this.CurrentCell, cell).ToString().ToLower());
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Utils.LogException(e);
                            return;
                        }
                    }
                }
            }
        }

        public void AIGotoXYZ(int x, int y, int z)
        {
            //this.debug++;

            //if (this.debug > 4)
            //    return; // dont let things call this function more than 4 times a round.

            if (this.X == x && this.Y == y && this.Z == z) { return; } // bail out if we aren't going anywhere

            if (this.BuildMoveList(x, y, z)) // search for target and build a move list
                this.DoNextListMove();
            else this.DoAIMove(); // search algorithm was unable to reach target so do something random
        }

        /// <summary>
        /// Uses a breadth-first search algorithm to build a list of 
        /// moves to take the Creature from its current location 
        /// to (x,y,z).  Called by Creature.AIGotoXYZ()
        /// </summary>
        /// <param name="ch">the Creature to move</param>
        /// <param name="x">target x coord</param>
        /// <param name="y">target y coord</param>
        public bool BuildMoveList(int x, int y, int z)
        {
            int xMax = World.GetFacetByID(this.FacetID).GetLandByID(this.LandID).GetMapByID(this.MapID).ZPlanes[this.Z].xcordMax;
            int yMax = World.GetFacetByID(this.FacetID).GetLandByID(this.LandID).GetMapByID(this.MapID).ZPlanes[this.Z].ycordMax;
            int xMin = World.GetFacetByID(this.FacetID).GetLandByID(this.LandID).GetMapByID(this.MapID).ZPlanes[this.Z].xcordMin;
            int yMin = World.GetFacetByID(this.FacetID).GetLandByID(this.LandID).GetMapByID(this.MapID).ZPlanes[this.Z].ycordMin;

            Point origin = new Point(this.X, this.Y);
            Point target = new Point(x, y);
            Point pos, current;
            int cost;
            int lowest_cost;

            List<Point> unfinished = new List<Point>();
            List<string> moves = new List<string>();
            // the hashtable values are cell cost value
            Hashtable travelled = new Hashtable();
            travelled[origin] = 0;

            pos = current = origin;

            try
            {
                while (pos != target && current != null)
                {
                    foreach (Point neighbor in Get_neighbor_pos(current, xMax, yMax, xMin, yMin))
                    {
                        try
                        {
                            Tuple<int, int, int> key = new Tuple<int, int, int>(neighbor.x, neighbor.y, z);
                            if (this.Map.cells.ContainsKey(key))
                                cost = this.GetCellCost(this.Map.cells[key]);
                            else
                                cost = 10000;
                        }
                        catch (Exception e)
                        {
                            Utils.LogException(e);
                            continue;
                        }

                        if (cost >= 10000)
                            continue;

                        pos = neighbor;

                        int v;
                        if (travelled.Contains(pos))
                            v = (int)travelled[pos];
                        else v = 0;

                        if (0 == v && pos != origin)
                        {
                            travelled[pos] = 1 + (int)travelled[current] + cost;
                            if (pos == target) break;
                            unfinished.Add(pos);
                        }
                    }

                    if (pos != target)
                    {
                        try
                        {
                            if (unfinished.Count > 0)
                            {
                                current = unfinished[0];
                                unfinished.RemoveAt(0);
                            }
                            else
                                current = null;
                        }
                        catch
                        {
                            // No unfinished cells
                            current = null;
                        }
                    }
                }

                current = pos;

                if (current != target)
                {
                    // Target was unreachable!
                    return false;
                }

                // now begin building the MoveList using the cost values
                while (current != origin)
                {
                    lowest_cost = 100;
                    Point best_point = null;
                    foreach (Point neighbor in Get_neighbor_pos(current, xMax, yMax, xMin, yMin))
                    {
                        int v;
                        pos = neighbor;

                        if (!travelled.ContainsKey(pos))
                            continue;

                        v = (int)travelled[pos];

                        if (v < lowest_cost)
                        {
                            lowest_cost = v;
                            best_point = pos;
                        }
                    }

                    if (best_point == null) { break; }

                    moves.Add(NPC.GetDirectionString(best_point, current));

                    current = best_point;
                }

                moves.Reverse();
                this.MoveList = moves;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                Utils.Log(this.GetLogString() + " failed BuildMoveList(" + x + ", " + y + ", " + z + ")", Utils.LogType.Debug);
            }


            if (this.MoveList.Count <= 0)
                return false; // Catch for 0 move movelist - Cant get there from here.

            return true;
        }

        /// <summary>
        /// Extremelly important method for performing living object movement calculations.
        /// </summary>
        /// <param name="cell">The cell in which this living Character object is evaluating.</param>
        /// <returns>1 = Good, 5 = Bad (possible if no other 1 cell cost movements exist), Infinity = Never</returns>
        public int GetCellCost(Cell cell)
        {
            int infinity = 10000;
            int cost = 5;
            bool isWaterDweller = ((this is NPC) && (this as NPC).IsWaterDweller) || EntityLists.WATER_DWELLER.Contains(this.entity);

            if (cell == null)
                return infinity;

            //if (cell.Z != this.Z)
            //    return infinity;

            // never leave water if you belong there
            if (cell.CellGraphic != Cell.GRAPHIC_WATER && isWaterDweller)
                return infinity;

            // lava
            if (cell.AreaEffects.ContainsKey(Effect.EffectTypes.Lava) && !EntityLists.FLYING.Contains(entity) && !EntityLists.INCORPOREAL.Contains(entity))
                return infinity;

            // light
            if (cell.AreaEffects.ContainsKey(Effect.EffectTypes.Light) && (IsUndead || EntityLists.UNDEAD.Contains(entity)) && Rules.CheckPerception(this))
                return infinity;

            // never go through a locked door?
            if (cell.IsLockedHorizontalDoor && !cell.AreaEffects.ContainsKey(Effect.EffectTypes.Unlocked_Horizontal_Door))
                return infinity;

            // never go through a locked door?
            if (cell.IsLockedVerticalDoor && !cell.AreaEffects.ContainsKey(Effect.EffectTypes.Unlocked_Vertical_Door))
                return infinity;

            // never go out of bounds
            if (cell.IsOutOfBounds) return infinity;

            try
            {
                switch (cell.DisplayGraphic)
                {
                    case Cell.GRAPHIC_ICE_STORM:
                        if (Name == PathTest.RESERVED_NAME_THROWNOBJECT || Name.EndsWith(PathTest.RESERVED_NAME_COMMANDSUFFIX))
                            cost = 1;
                        else cost = 5;
                        break;
                    case Cell.GRAPHIC_WATER:
                        if (Name == PathTest.RESERVED_NAME_THROWNOBJECT || Name == PathTest.RESERVED_NAME_JUMPKICKCOMMAND ||
                            Name.EndsWith(PathTest.RESERVED_NAME_COMMANDSUFFIX))
                        {
                            cost = 1;
                            break;
                        }

                        if(EntityLists.AQUAPHOBIC.Contains(entity))
                        {
                            cost = infinity;
                            break;
                        }

                        if (isWaterDweller ||
                            CanBreatheWater ||
                            this.PetOwner != null ||
                            EntityLists.AMPHIBIOUS.Contains(this.entity) ||
                            this.CurrentCell.CellGraphic == Cell.GRAPHIC_WATER)
                        {
                            cost = 1;

                            // Move out of a current water cell if you don't belong in water.
                            //if (this.CurrentCell.CellGraphic == Cell.GRAPHIC_WATER)
                            //{
                            //    if (!isWaterDweller && !CanBreatheWater && !EntityLists.AMPHIBIOUS.Contains(this.entity) && !CanFly)
                            //    {
                            //        // log it for debugging
                            //        Utils.Log(this.GetLogString() + " is in a WATER cell and probably shouldn't be.", Utils.LogType.Debug);
                            //    }
                            //}
                        }
                        else if (CanFly || IsUndead)
                        {
                            // Flying or undead has a mostHated or is already in (over) water. Low cost.
                            if ((this is NPC) && (this as NPC).MostHated != null || this.CurrentCell.CellGraphic == Cell.GRAPHIC_WATER || DragonsSpineMain.ServerStatus <= DragonsSpineMain.ServerState.Running)
                            {
                                cost = 1;
                            }
                            else // No MostHated. Don't opt to fly over or enter water.
                            {
                                cost = infinity;
                            }
                        }
                        else
                        {
                            if (Cell.IsNextToLand(cell) && this is NPC && (this as NPC).MostHated != null)
                                cost = 1;
                            else cost = infinity;
                        }
                        break;
                    case Cell.GRAPHIC_AIR:
                        if (this.Name == PathTest.RESERVED_NAME_THROWNOBJECT)
                        {
                            cost = 1;
                            break;
                        }
                        if (CanFly)
                        {
                            // Flying or undead has a mostHated or is already in (over) water. Low cost.
                            if ((this is NPC) && (this as NPC).MostHated != null || CurrentCell.CellGraphic == Cell.GRAPHIC_AIR || DragonsSpineMain.ServerStatus <= DragonsSpineMain.ServerState.Running)
                            {
                                cost = 1;
                            }
                            else // No MostHated. Don't opt to fly.
                            {
                                cost = infinity;
                            }
                        }
                        else
                        {
                            cost = infinity;
                        }
                        break;
                    case Cell.GRAPHIC_WEB:
                        if (this.Name == PathTest.RESERVED_NAME_THROWNOBJECT || this.Name.EndsWith(PathTest.RESERVED_NAME_COMMANDSUFFIX))
                        {
                            cost = 1;
                            break;
                        }
                        if (CanFly)
                        {
                            cost = 5;
                        }
                        else
                        {
                            if (species == Globals.eSpecies.Arachnid || EntityLists.WEB_DWELLERS.Contains(this.entity))
                            {
                                cost = 1;
                            }
                            else if ((this is NPC) && (this as NPC).MostHated != null)
                            {
                                cost = 2;
                            }
                            else
                            {
                                cost = infinity;
                            }
                        }
                        break;
                    case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:
                    case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:
                        if (this.Name.Contains(PathTest.RESERVED_NAME_JUMPKICKCOMMAND))
                        {
                            cost = infinity;
                            break;
                        }
                        if (this.Name.Contains(PathTest.RESERVED_NAME_AREAEFFECT))
                        {
                            cost = infinity;
                            break;
                        }
                        if (this.Name == PathTest.RESERVED_NAME_THROWNOBJECT)
                        {
                            cost = 1;
                            break;
                        }
                        cost = 1;
                        break;
                    case Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL:
                    case Cell.GRAPHIC_OPEN_DOOR_VERTICAL:
                        cost = 1;
                        break;
                    case Cell.GRAPHIC_ICE:
                        if (this.Name == PathTest.RESERVED_NAME_THROWNOBJECT || this.Name.EndsWith(PathTest.RESERVED_NAME_COMMANDSUFFIX))
                        {
                            cost = 1;
                            break;
                        }
                        if (cell.AreaEffects.ContainsKey(Effect.EffectTypes.Ice) || cell.AreaEffects.ContainsKey(Effect.EffectTypes.Dragon__s_Breath_Ice))
                        {
                            if (immuneCold) // immune to fire damage
                            {
                                cost = 1;
                            }
                            else
                            {
                                if ((this is NPC) && (this as NPC).MostHated != null)
                                {
                                    cost = 5;
                                }
                                else // do not move into ice if we're not going after an enemy
                                {
                                    cost = infinity;
                                }
                            }
                        }
                        else
                        {
                            cost = 1;
                        }
                        break;
                    case Cell.GRAPHIC_ICE_WALL:
                        if (this.Name == PathTest.RESERVED_NAME_THROWNOBJECT || this.Name.EndsWith(PathTest.RESERVED_NAME_COMMANDSUFFIX))
                        {
                            cost = 1;
                            break;
                        }
                        if (cell.AreaEffects.ContainsKey(Effect.EffectTypes.Blizzard))
                        {
                            if (immuneCold) // immune to cold damage
                            {
                                cost = 1;
                            }
                            else
                            {
                                if ((this is NPC) && (this as NPC).MostHated != null)
                                {
                                    cost = 5;
                                }
                                else // do not move into blizzard if we're not going after an enemy
                                {
                                    cost = infinity;
                                }
                            }
                        }
                        else
                        {
                            cost = 1;
                        }
                        break;
                    case Cell.GRAPHIC_FIRE:
                        if (this.Name == PathTest.RESERVED_NAME_THROWNOBJECT || this.Name.EndsWith(PathTest.RESERVED_NAME_COMMANDSUFFIX))
                        {
                            cost = 1;
                            break;
                        }
                        if (immuneFire) // immune to fire damage
                        {
                            cost = 1;
                        }
                        else cost = infinity;
                        break;
                    case Cell.GRAPHIC_POISON_CLOUD:
                        if (this.Name == PathTest.RESERVED_NAME_THROWNOBJECT || this.Name.EndsWith(PathTest.RESERVED_NAME_COMMANDSUFFIX))
                        {
                            cost = 1;
                            break;
                        }
                        if (immunePoison) // immune to poison
                        {
                            cost = 1;
                        }
                        else
                        {
                            if ((this is NPC) && (this as NPC).MostHated != null)
                            {
                                cost = 5;
                            }
                            else // do not move into poison if we're not going after an enemy
                            {
                                cost = infinity;
                            }
                        }
                        break;
                    case Cell.GRAPHIC_LIGHTNING_STORM:
                        if (this.Name == PathTest.RESERVED_NAME_THROWNOBJECT || this.Name.EndsWith(PathTest.RESERVED_NAME_COMMANDSUFFIX))
                        {
                            cost = 1;
                            break;
                        }
                        if (immuneLightning) // immune to lightning damage
                        {
                            cost = 1;
                        }
                        else
                        {
                            if ((this is NPC) && (this as NPC).MostHated != null)
                            {
                                cost = 5;
                            }
                            else // do not move into lightning if we're not going after an enemy
                            {
                                cost = infinity;
                            }
                        }
                        break;
                    case Cell.GRAPHIC_FOG:
                    case Cell.GRAPHIC_ACID_STORM:
                        if (Name == PathTest.RESERVED_NAME_THROWNOBJECT || this.Name.EndsWith(PathTest.RESERVED_NAME_COMMANDSUFFIX))
                        {
                            cost = 1;
                            break;
                        }
                        if ((this is NPC) && (this as NPC).MostHated != null)
                        {
                            cost = 5;
                        }
                        else // do not move into acid if we're not going after an enemy
                        {
                            cost = infinity;
                        }
                        break;
                    // now the impassable cells
                    case Cell.GRAPHIC_WALL:
                    case Cell.GRAPHIC_MOUNTAIN:
                    case Cell.GRAPHIC_FOREST_IMPASSABLE:
                    case Cell.GRAPHIC_SECRET_DOOR:
                    case Cell.GRAPHIC_LOCKED_DOOR_HORIZONTAL:
                    case Cell.GRAPHIC_LOCKED_DOOR_VERTICAL:
                    case Cell.GRAPHIC_COUNTER:
                    case Cell.GRAPHIC_ALTAR_PLACEABLE:
                    case Cell.GRAPHIC_REEF:
                    case Cell.GRAPHIC_COUNTER_PLACEABLE:
                    case Cell.GRAPHIC_ALTAR:
                    case Cell.GRAPHIC_GRATE:
                        cost = infinity;
                        break;
                    default:
                        cost = 1;
                        break;
                }

                if (cost < infinity)
                {
                    if (this.EffectsList.ContainsKey(Effect.EffectTypes.Hide_in_Shadows) &&
                        !this.EffectsList[Effect.EffectTypes.Hide_in_Shadows].IsPermanent && !Map.IsNextToWall(this))
                    {
                        cost += 5;
                    }
                }

                return cost;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return -1;
            }
        }

        private List<Point> Get_neighbor_pos(Point current, int xmax, int ymax, int xmin, int ymin)
        {
            List<Point> neighbors = new List<Point>();

            try
            {
                if (current == null)
                {
                    return neighbors;
                }

                Point pt;
                foreach (Point d in directions)
                {
                    pt = new Point(current.x + d.x, current.y + d.y);
                    if (pt.x >= xmin && pt.x <= (xmax - 2) && pt.y >= ymin && pt.y <= (ymax - 2))
                    {
                        neighbors.Add(pt);
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
            return neighbors;
        }

        private static string GetDirectionString(Point beg, Point end)
        {
            Point dp = end - beg;
            string lhs = "", rhs = "";

            if (dp.y == -1)
                lhs = "n";
            else if (dp.y == 1)
                lhs = "s";

            if (dp.x == -1)
                rhs = "w";
            else if (dp.x == 1)
                rhs = "e";

            return lhs + rhs;
        }

        /// <summary>
        /// Moves a Character into an adjacent cell, regardless if the cell is blocked (wall, mountain, etc).
        /// If the Character is blinded and not feared they will crawl.
        /// </summary>
        /// <param name="target">The Character to move.</param>
        public static void AIRandomlyMoveCharacter(Character target)
        {
            string[] directions = new string[] { "n", "s", "e", "w", "ne", "nw", "se", "sw" };

            object prevTemporaryStorage = target.TemporaryStorage;
            target.TemporaryStorage = "override movement";

            CommandTasker.ParseCommand(target, directions[Rules.Dice.Next(0, directions.Length)], "");

            target.TemporaryStorage = prevTemporaryStorage;

            return;
        }

        /// <summary>
        /// Determine level of visibility around the Character. Do not use this method to extend a search beyond Cell.DEFAULT_VISIBLE_DISTANCE.
        /// </summary>
        /// <returns>The perimeter of visibility around the character. -1 = no visibility, 0 = current cell only, 1 = 1 cell around the Character, etc.</returns>
        public int GetVisibilityDistance()
        {
            if (this.CurrentCell == null) return -1;

            //Visibility impairements disabled 2/28/2014. TODO: Resolve.

            //if (this.IsImmortal) return Cell.DEFAULT_VISIBLE_DISTANCE;

            //// cell is dark and the character does not have nightvision, no visibility
            //if (this.CurrentCell.IsAlwaysDark || this.CurrentCell.AreaEffects.ContainsKey(Effect.EffectType.Darkness))
            //{
            //    if (!this.HasNightVision) return -1; // character will not even be able to see in front of them
            //    else return Cell.DEFAULT_VISIBLE_DISTANCE - 1;
            //}

            //// or area is dimly lit. the only times the Character may have an issue with visibility
            //if (this.CurrentCell.IsOutdoors)
            //{
            //    if (World.CurrentDailyCycle == World.DailyCycle.Night)
            //    {
            //        #region Night Time Visibility
            //        // during a full moon
            //        if (World.CurrentLunarCycle == World.LunarCycle.Full || this.HasNightVision)
            //        {
            //            return Cell.DEFAULT_VISIBLE_DISTANCE - 1;
            //        }

            //        // determine light source
            //        Globals.eLightSource cellLightsource = Globals.eLightSource.None;
            //        Globals.eLightSource personalLightsource = Globals.eLightSource.None;

            //        if (this.CurrentCell.HasLightSource(out cellLightsource) || this.HasLightSource(out personalLightsource))
            //        {
            //            Globals.eLightSource brighterLightsource = (Globals.eLightSource)Math.Max((int)cellLightsource, (int)personalLightsource);

            //            switch (brighterLightsource)
            //            {
            //                case Globals.eLightSource.RadiantOrb:
            //                    return Cell.DEFAULT_VISIBLE_DISTANCE; // should be 3
            //                case Globals.eLightSource.LightSpell: // should be one of the brightest light sources
            //                case Globals.eLightSource.TownLimits: // torches along the walls in town
            //                    return 2;
            //                case Globals.eLightSource.StrongItemBlueglow:
            //                case Globals.eLightSource.AnyFireEffect:
            //                    return 1;
            //                case Globals.eLightSource.WeakItemBlueglow:
            //                    return 0;
            //                default:
            //                    {
            //                        return 0;
            //                    }
            //            }
            //        }
            //        else return -1; // character cannot see 
            //        #endregion
            //    }
            //    else // day time, check forestry level (rangers will eventually come into play here)
            //    {
            //        // not an animal, standing in forested area
            //        // so many things to check for in the future here - Ranger profession, heightened senses
            //        if (!this.animal && (this.CurrentCell.CellGraphic == Cell.GRAPHIC_FOREST_FULL || this.CurrentCell.CellGraphic == Cell.GRAPHIC_FOREST_LEFT || this.CurrentCell.CellGraphic == Cell.GRAPHIC_FOREST_RIGHT))
            //        {
            //            switch (this.Map.ZPlanes[this.Z].forestry)
            //            {
            //                case Map.ForestedLevel.Heavy:
            //                    return 2;
            //                case Map.ForestedLevel.Very_Heavy:
            //                    return 1;
            //                default:
            //                    break;
            //            }
            //        }
            //    }
            //}
            //else // not outdoors, check light level
            //{
            //    Map.LightLevel lightLevel = this.Map.ZPlanes[this.Z].lightLevel;

            //    if (lightLevel < Map.LightLevel.Normal)
            //    {
            //        if (this.HasNightVision) return Cell.DEFAULT_VISIBLE_DISTANCE - 1; // no matter what, nightvision improves visibility

            //        // determine light source
            //        Globals.eLightSource cellLightsource = Globals.eLightSource.None;
            //        Globals.eLightSource personalLightsource = Globals.eLightSource.None;

            //        if (this.CurrentCell.HasLightSource(out cellLightsource) || this.HasLightSource(out personalLightsource))
            //        {
            //            Globals.eLightSource brighterLightsource = (Globals.eLightSource)Math.Max((int)cellLightsource, (int)personalLightsource);

            //            switch (brighterLightsource)
            //            {
            //                case Globals.eLightSource.RadiantOrb:
            //                    return Cell.DEFAULT_VISIBLE_DISTANCE; // should be 3
            //                case Globals.eLightSource.LightSpell: // should be one of the brightest light sources
            //                    return Math.Max((int)2, (int)lightLevel);
            //                case Globals.eLightSource.TownLimits: // torches along the walls in town
            //                case Globals.eLightSource.StrongItemBlueglow:
            //                case Globals.eLightSource.AnyFireEffect:
            //                    return Math.Max((int)1, (int)lightLevel);
            //                case Globals.eLightSource.WeakItemBlueglow:
            //                    return Math.Max((int)0, (int)lightLevel);
            //            }
            //        }
            //        else return (int)lightLevel;
            //    }
            //}

            return Cell.DEFAULT_VISIBLE_DISTANCE;
        }

        /// <summary>
        /// Determine if the Character has a lightsource and return the lightsource if true.
        /// </summary>
        /// <param name="lightsource">The lightsource can be an effect or an item.</param>
        /// <returns>True if a lightsource is present on the Character.</returns>
        public bool HasLightSource(out Globals.eLightSource lightsource)
        {
            // order of light source strength
            // none, weak blue glow, any fire effect, strong blue glow, 

            Globals.eLightSource lightsourceToReturn = Globals.eLightSource.None;
            bool result = false;

            // 12/26/2013 Radiant Orb is currently maximum light source and grants full visibility.
            if (this.EffectsList.ContainsKey(Effect.EffectTypes.Radiant_Orb))
            {
                lightsourceToReturn = Globals.eLightSource.RadiantOrb;
                result = true;
            }

            if (lightsourceToReturn < Globals.eLightSource.RadiantOrb)
            {
                #region Check for a held torch.
                if (this.RightHand != null && this.RightHand.baseType == Globals.eItemBaseType.Torch)
                {
                    lightsourceToReturn = (Globals.eLightSource)Math.Max((int)Globals.eLightSource.AnyFireEffect, (int)lightsourceToReturn);
                    result = true;
                }
                else if (this.LeftHand != null && this.LeftHand.baseType == Globals.eItemBaseType.Torch)
                {
                    lightsourceToReturn = (Globals.eLightSource)Math.Max((int)Globals.eLightSource.AnyFireEffect, (int)lightsourceToReturn);
                    result = true;
                }
                #endregion

                #region Check for flame shield.
                if (lightsourceToReturn < Globals.eLightSource.AnyFireEffect && this.EffectsList.ContainsKey(Effect.EffectTypes.Flame_Shield))
                {
                    lightsourceToReturn = (Globals.eLightSource)Math.Max((int)Globals.eLightSource.AnyFireEffect, (int)lightsourceToReturn);
                    result = true;
                }
                #endregion

                #region Check for blue glow items.
                if (lightsourceToReturn < Globals.eLightSource.WeakItemBlueglow)
                {
                    if (this.RightHand != null && this.RightHand.blueglow)
                    {
                        lightsourceToReturn = (Globals.eLightSource)Math.Max((int)Item.GetBlueglowStrength(this.RightHand), (int)lightsourceToReturn);
                        result = true;
                    }

                    if (this.LeftHand != null && this.LeftHand.blueglow)
                    {
                        lightsourceToReturn = (Globals.eLightSource)Math.Max((int)Item.GetBlueglowStrength(this.LeftHand), (int)lightsourceToReturn);
                        result = true;
                    }

                    // Currently only gauntlets are an inventory item with blueglow. 1/13/2014
                    Item gauntlets = GetInventoryItem(Globals.eWearLocation.Hands);

                    if (gauntlets != null && gauntlets.blueglow)
                    {
                        lightsourceToReturn = (Globals.eLightSource)Math.Max((int)Item.GetBlueglowStrength(gauntlets), (int)lightsourceToReturn);
                        result = true;
                    }

                    //foreach (Item item in this.wearing)
                    //{
                    //    if (item.blueglow)
                    //    {
                    //        lightsourceToReturn = (Globals.eLightSource)Math.Max((int)Item.GetBlueglowStrength(item), (int)lightsourceToReturn);
                    //        result = true;
                    //    }
                    //}
                }
                #endregion
            }

            lightsource = lightsourceToReturn;
            return result;
        }

        public bool HasTalent(Talents.GameTalent.TALENTS talent)
        {
            return this.talentsDictionary.ContainsKey(talent.ToString().ToLower());
        }

        public string GetNameForActionResult()
        {
            if (string.IsNullOrEmpty(Name)) return "NullName";

            if ((Name.Length > 0 && char.IsUpper(Name[0])) || Name.StartsWith(GameSpell.IMAGE_IDENTIFIER))
                return Name;

            if (this is NPC npc && npc.shortDesc.Length > 0)
                return "The " + npc.shortDesc;
            else if (Name.Length > 0 && char.IsLower(Name[0]))
                return "The " + Name;

            return Name;
        }

        public string GetNameForActionResult(bool lowercaseThe)
        {
            return lowercaseThe ? GetNameForActionResult().Replace("The ", "the ") : GetNameForActionResult();
        }

        // Currently used to check for possession of artifacts.
        // Only one instance of a specific artifact may be possessed by a Player.
        public bool PossessesItem(int itemID)
        {
            if (this.RightHand != null && this.RightHand.itemID == itemID) return true;
            if (this.LeftHand != null && this.LeftHand.itemID == itemID) return true;

            List<List<Item>> containers = GetAllContainers();

            foreach (List<Item> list in containers)
                foreach (Item item in list)
                    if (item.itemID == itemID) return true;

            return false;
        }

        /// <summary>
        /// Returns a List of all containers a Character uses.
        /// </summary>
        private List<List<Item>> GetAllContainers()
        {
            // Belt, locker, pouch, sack, wearing.
            List<List<Item>> containers = new List<List<Item>>();
            containers.Add(beltList);
            containers.Add(lockerList);
            containers.Add(pouchList);
            containers.Add(sackList);
            containers.Add(wearing);

            return containers;
        }

        public bool HasEffect(Effect.EffectTypes effectType)
        {
            if (EffectsList.ContainsKey(effectType))
            {
                return true;
            }

            foreach (Effect wornEffect in WornEffectsList)
            {
                if (wornEffect.EffectType == effectType) return true;
            }
            
            return false;
        }

        public bool HasEffect(Effect.EffectTypes effectType, out Effect effect)
        {
            bool result = false;
            Effect resultEffect = null;

            // Effects in the EffectsList do not stack (yet)
            if (EffectsList.ContainsKey(effectType))
            {
                resultEffect = EffectsList[effectType];
                result = true;
                //effect = EffectsList[effectType];
                //return true;
            }

            // Worn effects do stack with other same worn effects, and with spell effect in EffectsList.
            foreach (Effect wornEffect in WornEffectsList)
            {
                if (wornEffect.EffectType == effectType)
                {
                    result = true;

                    if (resultEffect == null)
                        resultEffect = wornEffect;
                    else resultEffect.Power += wornEffect.Power;
                }
            }

            effect = resultEffect;
            return result;
        }
    }
}