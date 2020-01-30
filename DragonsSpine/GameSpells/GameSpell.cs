using System;
using System.Collections;
using System.Collections.Generic;
using DragonsSpine.GameWorld;
using TargetAcquisition = DragonsSpine.GameSystems.Targeting.TargetAquisition;

namespace DragonsSpine.Spells
{
    public class GameSpell
    {
        public static Dictionary<int, GameSpell> GameSpellDictionary = new Dictionary<int, GameSpell>(); // global list of spells by unique ID
        public static Dictionary<string, GameSpell> GameSpellCommandDictionary = new Dictionary<string, GameSpell>(); // global list of spells by unique spell command

        private static readonly string[] MagicWords = {"aazag","alla","alsi","anaku","angarru","anghizidda","anna","annunna","ardata","ashak",
            "baad","dingir","duppira","edin","enaa","endul","enmeshir","enn","ennul","esha","gallu","gidim","gish","ia","idpa","igigi",
            "ina","isa","khitim","kia","kielgallal","kima","ku","lalartu","limutuma","lini","ma","mardukka","masqim","mass","na","naa",
            "namtar","nebo","nenlil","nergal","ninda","ningi","ninn","ninnda","ninnghizhidda","ninnme","nngi","nushi","qutri","raa","sagba",
            "shadu","shammash","shunu","shurrim","telal","uhddil","urruku","uruki","utug","utuk","utuq","ya","yu","zi","zumri","kanpa",
            "ziyilqa","luubluyi","luudnin","luuppatar","xul","ssaratu","zu","barra","kunushi","tamatunu","ega","cuthalu","egura","asaru",
            "urma","muxxisha","akki","ilani","gishtugbi","arrflug"};

        /// <summary>
        /// The order of this enum cannot be changed. Add new GameSpellID ints to the end.
        /// </summary>
        public enum GameSpellID : int
        {
            None, // placeholder for 0
            Curse,
            Strength,
            Fear,
            Light,
            Blind, // 5
            Protection_from_Fire,
            Protection_from_Cold,
            Stun,
            Cure,
            Neutralize_Poison, // 10
            Turn_Undead,
            Lightning_Bolt,
            Banish,
            Death,
            Raise_the_Dead, // 15
            Resist_Fear,
            Create_Snake,
            Summon_Phantasm,
            Resist_Blindness,
            Summon_Demon, // 20
            Protection_from_Fire_and_Ice,
            Resist_Lightning,
            Protection_from_Poison,
            Poison_Cloud,
            Resist_Stun, // 25
            Resist_Death,
            Resistance_from_Blind_and_Fear,
            Lightning_Storm,
            Protection_from_Stun_and_Death,
            Magic_Missile, // 30
            Close_and_Open_Door,
            Bonfire,
            Breathe_Water,
            Shield,
            Darkness, // 35
            Find_Secret_Door,
            Create_Web,
            Create_Portal,
            Fireball,
            Firewall, // 40
            Icestorm,
            Concussion,
            Dispel_Illusion,
            Create_Illusion,
            Wizard_Eye, // 45
            Disintegrate,
            Peek,
            Firebolt,
            Whirlwind,
            Icespear, // 50
            Firestorm,
            Dragon__s_Breath,
            Lightning_Lance,
            Blizzard,
            Create_Lava, // 55
            Hide_in_Shadows,
            Identify,
            Make_Recall,
            Night_Vision,
            Hide_Door, // 60
            Feather_Fall,
            Speed,
            Venom,
            Blessing_of_the_Faithful,
            Locate, // 65
            Flame_Shield,
            Lifeleech,
            Acid_Orb,
            Minor_Protection_from_Fire,
            Image, // 70
            Charm_Animal,
            Protection_from_Undead,
            Dismiss_Undead,
            Command_Undead,
            Power_Word___Silence, // 75
            Acid_Rain,
            Contagion,
            Animate_Dead,
            Heal_Servant,
            Chain_Lightning, // 80
            Halt_Undead,
            Obfuscation,
            Locust_Swarm,
            Summon_Hellhound,
            Cage_Soul, // 85
            Transmute,
            Protection_from_Acid,
            Ghod__s_Hooks,
            Summon_Lamassu,
            Chaos_Portal, // 90
            Ensnare,
            Root,
            Summon_Nature__s_Ally,
            Shelter,
            Regeneration, // 95
            DONOTUSE, // 96 -- odd bug where this will disconnect the user 7/11/2019
            Mark_of_Vitality,
            Wall_of_Fog,
            Ataraxia, // 99
            Thunderwave, // 100
            Hunter__s_Mark,
            Barkskin,
            Stoneskin,
            Detect_Undead, // 104
            Trochilidae, // (Hummingbird Effect)
            Cynosure, // debuff -- target becomes more susceptible to physical attacks
            Protection_from_Hellspawn, // 107
            Ferocity,
            Savagery,
            Bazymon__s_Bounty, // 110 xp gain bonus
            Lagniappe, // 111 skill gain bonus
            The_Withering, // 112 decreased xp gain
            Drudgery, // 113 decreased skill gain
            Gnostikos, // 114 clairvoyance
            Protection_from_Lightning,
            Summon_Humanoid,
            Cognoscere,
            Umbral_Form,
            Tempest, // 119 undead aoe
            Faerie_Fire,
            Iceshard,
            Stranglehold,
            Juvenis, // 123
            Walking_Death, // 124
        };

        #region Constants
        public const int CURSE_SPELL_MULTIPLICAND_NPC = 4;
        public const int CURSE_SPELL_MULTIPLICAND_PC = 5;
        public const int DEATH_SPELL_MULTIPLICAND_NPC = 8;
        public const int DEATH_SPELL_MULTIPLICAND_PC = 10;
        public const int BANISH_SPELL_MULTIPLICAND_NPC = 9;
        public const int BANISH_SPELL_MULTIPLICAND_PC = 11;
        public const int MAX_PETS = 3; // total number of non quest followers/pets currently allowed

        private const int MAGIC_WORDS_LENGTH = 4; // the total number of magic words per spell
        private const int RAISE_DEAD_SPELLID = 15;

        public const string IMAGE_IDENTIFIER = "#";
        #endregion

        #region Private Data
        /// <summary>
        /// Holds the unique spell ID. This value must be unique.
        /// </summary>
        private readonly int _id;

        /// <summary>
        /// Holds the command used to cast the spell.
        /// </summary>
        private readonly string _command;

        /// <summary>
        /// Holds the name of the spell.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Holds the description of the spell.
        /// </summary>
        private readonly string _description;

        /// <summary>
        /// Holds the professions that may cast this spell.
        /// </summary>
        private readonly Character.ClassType[] _classTypes;

        /// <summary>
        /// Holds the spell type, which is primarily used by AI.
        /// </summary>
        private readonly Globals.eSpellType _spellType; // Abjuration, Alteration, Conjuration, Divination, Evocation, Necromancy (primarily used by AI)

        /// <summary>
        /// Holds the target type of the spell.
        /// </summary>
        private readonly Globals.eSpellTargetType _targetType; // Area_Effect, Group, Point_Blank_Area_Effect, Self, Single

        /// <summary>
        /// Holds the mana cost of the spell.
        /// </summary>
        private readonly int _manaCost; // mana cost to cast the spell

        /// <summary>
        /// Holds the required level in order to learn the spell.
        /// </summary>
        private readonly int _requiredLevel; // required casting level

        /// <summary>
        /// Holds the cost of the spell when learning it.
        /// </summary>
        private readonly int _trainingPrice; // purchase price

        /// <summary>
        /// Holds the sound file information played when the spell is cast.
        /// </summary>
        private readonly string _soundFile; // sound file info

        /// <summary>
        /// Holds whether the spell is beneficial. Used by AI to assist in determining which spell to cast at a target.
        /// </summary>
        private readonly bool _beneficial;

        /// <summary>
        /// Holds whether the spell is available at generic spell trainers.
        /// </summary>
        private readonly bool _availableAtTrainer;

        /// <summary>
        /// Holds whether the spell is available as a random scribed scroll in loot.
        /// </summary>
        private bool _foundForScribing;

        /// <summary>
        /// Holds whether the spell is available as a scroll with charge(s) to be cast.
        /// </summary>
        private bool _foundForCasting;

        /// <summary>
        /// Holds whether the spell is available only as lair loot.
        /// </summary>
        private bool _onlyLairs;

        /// <summary>
        /// Holds the ISpellHandler responsible for executing pertinent code when the GameSpell is used in game.
        /// </summary>
        private readonly ISpellHandler _spellHandler;
        #endregion

        #region Public Properties
        public int ID
        {
            get { return _id; }
        }

        public string Command
        {
            get { return _command; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Description
        {
            get { return _description; }
        }

        public Character.ClassType[] ClassTypes
        {
            get { return _classTypes; }
        }

        public Globals.eSpellType SpellType
        {
            get { return _spellType; }
        }

        public Globals.eSpellTargetType TargetType
        {
            get { return _targetType; }
        }

        public int ManaCost
        {
            get { return _manaCost; }
        }

        public int RequiredLevel
        {
            get { return _requiredLevel; }
        }

        public int TrainingPrice
        {
            get { return _trainingPrice; }
        }

        public string SoundFile
        {
            get { return _soundFile; }
        }

        public bool IsBeneficial
        {
            get { return _beneficial; }
        }

        public bool IsAvailableAtTrainer
        {
            get { return _availableAtTrainer; }
        }

        public bool IsFoundForScribing
        {
            get { return _foundForScribing; }
        }

        public bool IsFoundForCasting
        {
            get { return _foundForCasting; }
        }

        public bool OnlyFoundInLairs
        {
            get { return _onlyLairs; }
        }

        public ISpellHandler Handler
        {
            get { return _spellHandler; }
        }
        #endregion

        #region Constructor
        public GameSpell(int id, string command, string name, string description, Globals.eSpellType spellType, Globals.eSpellTargetType targetType, int manaCost,
            int requiredLevel, int trainingPrice, string soundFile, bool beneficial, bool availableAtTrainer,
            bool foundForScribing, bool foundForCasting, bool onlyLairs, Character.ClassType[] classTypes, ISpellHandler handler)
        {
            _id = id;
            _command = command;
            _name = name;
            _description = description;
            _spellType = spellType;
            _targetType = targetType;
            _manaCost = manaCost;
            _requiredLevel = requiredLevel;
            _trainingPrice = trainingPrice;
            _soundFile = soundFile;
            _beneficial = beneficial;
            _availableAtTrainer = availableAtTrainer;
            _foundForScribing = foundForScribing;
            _foundForCasting = foundForCasting;
            _onlyLairs = onlyLairs;

            _classTypes = classTypes;
            _spellHandler = handler;

            _spellHandler.ReferenceSpell = this;
        }
        #endregion

        public static bool LoadGameSpells()
        {
            try
            {
                foreach (Type t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (Array.IndexOf(t.GetInterfaces(), typeof(ISpellHandler)) > -1)
                    {
                        var a = (SpellAttribute)t.GetCustomAttributes(typeof(SpellAttribute), true)[0];

                        var gameSpell = new GameSpell(a.ID, a.Command, a.Name, a.Description, a.SpellType, a.TargetType, a.ManaCost, a.RequiredLevel, a.TrainingPrice,
                            a.SoundFile, a.IsBeneficial, a.IsAvailableAtTrainer, a.IsFoundForScribing, a.IsFoundForCasting, a.OnlyFoundInLairs, a.ClassTypes, (ISpellHandler)Activator.CreateInstance(t));

                        // Add the GameSpell to the dictionaries.
                        if (!GameSpellDictionary.ContainsKey(gameSpell.ID))
                            GameSpellDictionary.Add(gameSpell.ID, gameSpell);
                        else Utils.Log("GameSpell already exists in GameSpell Dictionary: ID = " + gameSpell.ID, Utils.LogType.SystemWarning);

                        if (!GameSpellCommandDictionary.ContainsKey(gameSpell.Command))
                            GameSpellCommandDictionary.Add(gameSpell.Command, gameSpell);
                        else Utils.Log("GameSpell already exists in GameSpell Command Dictionary: Command = " + gameSpell.Command, Utils.LogType.SystemWarning);
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return false;
            }
            return true;
        }

        public static int GetRandomSpellID(Dictionary<int, string> spellList)
        {
            var keys = new List<int>(spellList.Keys);
            return keys[Rules.Dice.Next(0, keys.Count - 1)];
        }

        public static string GetLogString(GameSpell spell)
        {
            return "[GameSpell ID: " + spell.ID + "] " + spell.Name + " (" + spell.SpellType + ", " + spell.TargetType + ")";
        }

        public static int GetSpellDamageModifier(Character caster)
        {
            if (caster.IsWisdomCaster)
                return Rules.Dice.Next(Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Wisdom) / 2, Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Wisdom));
            else if (caster.IsIntelligenceCaster)
                return Rules.Dice.Next(Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Intelligence) / 2, Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Intelligence));
            return 0;
        }

        public static GameSpell GetSpell(int spellID)
        {
            if (GameSpellDictionary.ContainsKey(spellID))
            {
                return GameSpellDictionary[spellID];
            }
            return null;
        }

        public static GameSpell GetSpell(string spellCommand)
        {
            if (GameSpellCommandDictionary.ContainsKey(spellCommand.ToLower()))
            {
                return GameSpellCommandDictionary[spellCommand.ToLower()];
            }
            return null;
        }

        public static void TeachSpell(Character caster, string spellCommand, string teacher)
        {
            var spell = GetSpell(spellCommand);

            string words = GenerateMagicWords();

            while (caster.spellDictionary.ContainsValue(words))
                words = GenerateMagicWords();

            caster.spellDictionary.Add(spell.ID, words);

            if (caster is PC)
                (caster as PC).savePlayerSpells = true;

            if (!String.IsNullOrEmpty(teacher))
            {
                caster.WriteToDisplay(teacher + ": This incantation will cast the spell " + spell.Name + ". (" + spell.Command + ")");
                caster.WriteToDisplay(teacher + ": " + words);
            }

            if(caster is PC && caster.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                ProtocolYuusha.SendCharacterSpells(caster as PC, caster);
        }

        public static string GenerateMagicWords()
        {
            string words = null;

            for (int a = 1; a <= MAGIC_WORDS_LENGTH; a++)
            {
                if (words == null)
                    words = MagicWords[Rules.Dice.Next(0, MagicWords.Length)];
                else
                    words += " " + MagicWords[Rules.Dice.Next(0, MagicWords.Length)];
            }
            return words;
        }

        public static void CastGenericAreaSpell(Cell center, string args, Effect.EffectTypes EffectType, int EffectPower, string spellName)
        {
            int AreaSize = 2;
            int XCordMod = 0;
            int YCordMod = 0;

            string EffectGraphic = "";

            string[] sArgs = args.Split(" ".ToCharArray());

            if (EffectPower == 0)
            {
                EffectPower = 15;
            }
            // check that they give us a starting cord
            //loop through the arguments to find the starting point

            ArrayList areaCellList = new ArrayList();
            int[] XCastMod = new int[] { 0, 0, 0 };
            int[] YCastMod = new int[] { 0, 0, 0 };

            // turn undead effects all undead on the current display map
            if (EffectType == Effect.EffectTypes.Turn_Undead)
            {
                AreaSize = 3;
            }

            if (EffectType == Effect.EffectTypes.Nitro)
            {
                AreaSize = 1;
                EffectType = Effect.EffectTypes.Concussion;
            }

            #region Create the list of effect cells
            if (AreaSize == 1)
            {
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y, center.Z));
            }
            else if (AreaSize > 2)
            {
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 2, center.Y + -2, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 2, center.Y + -2, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 1, center.Y + -2, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 1, center.Y + -2, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + -2, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + -2, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + -2, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + -2, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y + -2, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y + -2, center.Z));

                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 2, center.Y + -1, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 2, center.Y + -1, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 1, center.Y + -1, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 1, center.Y + -1, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + -1, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + -1, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + -1, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + -1, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y + -1, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y + -1, center.Z));

                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -2, center.Y, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -2, center.Y, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y, center.Z));

                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -2, center.Y + 1, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -2, center.Y + 1, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y + 1, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y + 1, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + 1, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + 1, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + 1, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + 1, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y + 1, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y + 1, center.Z));

                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -2, center.Y + 2, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -2, center.Y + 2, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y + 2, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y + 2, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + 2, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + 2, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + 2, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + 2, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y + 2, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y + 2, center.Z));


            }
            else
            {

                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 1, center.Y + -1, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 1, center.Y + -1, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + -1, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + -1, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + -1, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + -1, center.Z));

                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y, center.Z));

                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y + 1, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y + 1, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + 1, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + 1, center.Z));
                if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + 1, center.Z)))
                    areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + 1, center.Z));
            }
            #endregion

            #region Adjust EffectPower based on AreaSize and EffectType
            if (AreaSize >= 3)
            {
                if (EffectType != Effect.EffectTypes.Turn_Undead)
                {
                    EffectPower = (int)(EffectPower / 2);
                }
            }
            else if (AreaSize == 1)
            {
                EffectPower = EffectPower * 2;
            }
            #endregion

            if (EffectType == Effect.EffectTypes.Concussion)
            {
                #region Concussion
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);
                var cellArray = Cell.GetApplicableCellArray(centerCell, AreaSize);

                foreach (Cell cell in areaCellList)
                {
                    Combat.DoConcussionDamage(cell, EffectPower, null);
                }

                ArrayList brokenWallsList = new ArrayList();

                for (int j = 0; j < cellArray.Length; j++)
                {
                    if (cellArray[j] != null)
                    {
                        if ((cellArray[j].CellGraphic == Cell.GRAPHIC_WALL || cellArray[j].CellGraphic == Cell.GRAPHIC_SECRET_DOOR) && !cellArray[j].IsMagicDead)
                        {
                            if (Rules.RollD(1, 100) < 30)
                            {
                                brokenWallsList.Add(cellArray[j]);
                            }
                        }
                    }
                }
                if (brokenWallsList.Count > 0)
                {
                    AreaEffect effect = new AreaEffect(Effect.EffectTypes.Illusion, Cell.GRAPHIC_RUINS_RIGHT, 1, -1, null, brokenWallsList);
                }
                #endregion
            }
            else if (EffectType == Effect.EffectTypes.Fire)
            {
                #region Fireball
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);
                //Cell[] cellArray = Cell.GetVisibleCellArray(centerCell, 3);

                EffectGraphic = "**";

                AreaEffect effect = new AreaEffect(EffectType, EffectGraphic, EffectPower, 2, null, areaCellList);


                foreach (Cell cell in areaCellList)
                {
                    AreaEffect.DoAreaEffect(cell, effect);

                    foreach (Character chr in cell.Characters.Values)
                    {
                        if (!chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                        {
                            chr.WriteToDisplay("You have been hit by a fireball!");
                        }
                    }
                }
                #endregion
            }
            else if (EffectType == Effect.EffectTypes.Ice)
            {
                #region Icestorm
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);
                //Cell[] cellArray = Cell.GetVisibleCellArray(centerCell, 3);
                foreach (Cell cell in areaCellList)
                {
                    foreach (Character chr in cell.Characters.Values)
                    {
                        if (!chr.IsDead)
                        {
                            Combat.DoSpellDamage(null, chr, null, EffectPower, "ice");
                            chr.WriteToDisplay("You have been hit by a raging ice storm!");
                        }
                    }
                }

                EffectGraphic = Cell.GRAPHIC_ICE_STORM;

                AreaEffect effect = new AreaEffect(EffectType, EffectGraphic, EffectPower, 1, null, areaCellList);
                #endregion
            }
            else if (EffectType == Effect.EffectTypes.Lightning_Storm)
            {
                #region Lightningstorm
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);
                foreach (Cell cell in areaCellList)
                {
                    foreach (Character chr in cell.Characters.Values)
                    {
                        if (!chr.IsDead)
                        {
                            Combat.DoSpellDamage(null, chr, null, EffectPower, "lightning");
                            chr.WriteToDisplay("You have been hit by a violent lightning storm!");
                        }
                    }
                }
                EffectGraphic = Cell.GRAPHIC_LIGHTNING_STORM;

                AreaEffect effect = new AreaEffect(EffectType, EffectGraphic, EffectPower, 3, null, areaCellList);
                #endregion

            }
            else if (EffectType == Effect.EffectTypes.Poison)
            {
                #region PoisonCloud
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);
                //Cell[] cellArray = Cell.GetVisibleCellArray(centerCell, 3);
                foreach (Cell cell in areaCellList)
                {
                    foreach (Character chr in cell.Characters.Values)
                    {
                        if (!chr.IsDead)
                        {
                            Combat.DoSpellDamage(null, chr, null, EffectPower, "poison");
                            chr.WriteToDisplay("You have been hit by a poison cloud!");
                        }
                    }
                }
                EffectGraphic = Cell.GRAPHIC_POISON_CLOUD;

                AreaEffect effect = new AreaEffect(EffectType, EffectGraphic, EffectPower, 5, null, areaCellList);
                #endregion

            }
            else
            {
                string spellMsg = "none";
                int spellDuration = 1;

                switch (EffectType)
                {
                    case Effect.EffectTypes.Light:
                        EffectGraphic = "";
                        spellMsg = "The area is illuminated by a burst of magical " + spellName.ToLower() + "!";
                        break;
                    case Effect.EffectTypes.Turn_Undead:
                        EffectGraphic = "";
                        spellMsg = "You feel a strong wind race through the area.";
                        spellDuration = 0;
                        break;
                    case Effect.EffectTypes.Darkness:
                        EffectGraphic = "";
                        spellMsg = "The area is covered in a shroud of magical " + spellName.ToLower() + ".";
                        break;
                    default:
                        EffectGraphic = "";
                        break;
                }
                if (spellMsg != "none")
                {
                    foreach (Cell cell in areaCellList)
                    {
                        foreach (Character chr in cell.Characters.Values)
                        {
                            chr.WriteToDisplay(spellMsg);
                        }
                    }
                }

                AreaEffect effect = new AreaEffect(EffectType, EffectGraphic, EffectPower, spellDuration, null, areaCellList);
            }
        }

        public static void CastGenericAreaSpell(Cell center, string args, Effect.EffectTypes EffectType, int EffectPower, string spellName, Character caster)
        {
            int radius = 2; // 1 = single cell, 2 = center cell + 1 cell radius ... 3 = all cells in view
            int XCordMod = 0;
            int YCordMod = 0;

            string[] sArgs = args.Split(" ".ToCharArray()); // Not used? -Eb 9/10/2015

            // Some sort of fail safe? -Eb 9/10/2015
            if (EffectPower == 0)
                EffectPower = 15;

            ArrayList areaCellList = new ArrayList();
            #region Set the size of the effect based on type
            // turn undead effects all undead on the current display map
            if (EffectType == Effect.EffectTypes.Turn_Undead)
                radius = 3;

            if (EffectType == Effect.EffectTypes.Nitro)
            {
                radius = 1;
                EffectType = Effect.EffectTypes.Concussion;
            }

            if (EffectType == Effect.EffectTypes.Lightning_Storm)
                radius = 3;

            #endregion

            #region Create the list of effect cells.
            if (radius <= 1)
            {
                if (!Map.IsSpellPathBlocked(center) && !areaCellList.Contains(center))
                    areaCellList.Add(center);
            }
            else
            {
                var cellsArray = Cell.GetApplicableCellArray(center, radius);

                if (!areaCellList.Contains(center))
                    areaCellList.Add(center);

                foreach (Cell appCell in cellsArray)
                    if (!Map.IsSpellPathBlocked(appCell) && !areaCellList.Contains(appCell))
                        areaCellList.Add(appCell);
            }
            #endregion

            #region Antiquated code. -Eb 9/10/2015
            //else if (Radius > 2)
            //{
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 2, center.Y + -2, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 2, center.Y + -2, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 1, center.Y + -2, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 1, center.Y + -2, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + -2, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + -2, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + -2, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + -2, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y + -2, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y + -2, center.Z));

            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 2, center.Y + -1, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 2, center.Y + -1, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 1, center.Y + -1, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 1, center.Y + -1, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + -1, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + -1, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + -1, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + -1, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y + -1, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y + -1, center.Z));

            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -2, center.Y, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -2, center.Y, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y, center.Z));

            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -2, center.Y + 1, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -2, center.Y + 1, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y + 1, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y + 1, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + 1, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + 1, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + 1, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + 1, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y + 1, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y + 1, center.Z));

            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -2, center.Y + 2, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -2, center.Y + 2, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y + 2, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y + 2, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + 2, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + 2, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + 2, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + 2, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y + 2, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 2, center.Y + 2, center.Z));


            //}
            //else
            //{

            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 1, center.Y + -1, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X - 1, center.Y + -1, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + -1, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + -1, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + -1, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + -1, center.Z));

            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y, center.Z));

            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y + 1, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + -1, center.Y + 1, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + 1, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X, center.Y + 1, center.Z));
            //    if (!Map.IsSpellPathBlocked(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + 1, center.Z)))
            //        areaCellList.Add(Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + 1, center.Y + 1, center.Z));
            //}
            #endregion

            #region Adjust EffectPower based on Radius and EffectType
            if (radius > 3) // Out of map view.
            {
                if (EffectType != Effect.EffectTypes.Turn_Undead)
                    EffectPower = (int)(EffectPower / 2);
            }
            else if (radius == 1)
                EffectPower = EffectPower * 2;
            #endregion

            switch (EffectType)
            {
                case Effect.EffectTypes.Concussion:
                    #region Concussion
                    Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);
                    var cellArray = Cell.GetApplicableCellArray(centerCell, radius);

                    foreach (Cell cell in areaCellList)
                    {
                        Combat.DoConcussionDamage(cell, EffectPower, caster);
                    }

                    ArrayList brokenWallsList = new ArrayList();

                    for (int j = 0; j < cellArray.Length; j++)
                    {
                        if (cellArray[j] != null)
                        {
                            if ((cellArray[j].CellGraphic == Cell.GRAPHIC_WALL || cellArray[j].CellGraphic == Cell.GRAPHIC_SECRET_DOOR) && !cellArray[j].IsMagicDead)
                            {
                                if (Rules.RollD(1, 100) < 30)
                                {
                                    brokenWallsList.Add(cellArray[j]);
                                }
                            }
                        }
                    }

                    if (brokenWallsList.Count > 0)
                    {
                        AreaEffect effect = new AreaEffect(Effect.EffectTypes.Illusion, Cell.GRAPHIC_RUINS_RIGHT, 1, -1, null, brokenWallsList);
                    }
                    #endregion
                    break;
            }

            if (EffectType == Effect.EffectTypes.Fire)
            {
                #region Fireball
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);

                centerCell.SendShout("a whoosh of fire!");

                AreaEffect effect = new AreaEffect(EffectType, "**", EffectPower, 2, caster, areaCellList);

                foreach (Cell cell in areaCellList)
                {
                    //Effect.DoAreaEffect(cell, effect);mlt [here]need looking further as 2 why is this commented out

                    foreach (Character chr in cell.Characters.Values)
                    {
                        if (!chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                        {
                            chr.WriteToDisplay("You have been hit by a fireball!");
                        }
                    }
                }
                #endregion
            }
            else if (EffectType == Effect.EffectTypes.Ice)
            {
                #region Icestorm
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);

                //if (this.SoundFile != "") { centerCell.EmitSound(this.SoundFile); }

                centerCell.SendShout("the pounding of giant ice pellets!");

                AreaEffect effect = new AreaEffect(EffectType, Cell.GRAPHIC_ICE_STORM, EffectPower, 2, caster, areaCellList);

                foreach (Cell cell in areaCellList)
                {
                    AreaEffect.DoAreaEffect(cell, effect);
                    foreach (Character chr in cell.Characters.Values)
                    {
                        if (!chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                        {
                            chr.WriteToDisplay("You have been hit by a raging ice storm!");
                        }
                    }
                }
                #endregion
            }
            else if (EffectType == Effect.EffectTypes.Lightning_Storm)
            {
                #region Lightning Storm
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);

                centerCell.SendShout("an ear-splitting thunderclap!");

                AreaEffect effect = new AreaEffect(EffectType, Cell.GRAPHIC_LIGHTNING_STORM, EffectPower, Rules.RollD(1, 4) + 2, caster, areaCellList);

                foreach (Cell cell in areaCellList)
                {
                    AreaEffect.DoAreaEffect(cell, effect);
                    foreach (Character chr in cell.Characters.Values)
                    {
                        if (!chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                        {
                            chr.WriteToDisplay("You have been hit by a violent lightning storm!");
                        }
                    }
                }
                #endregion
            }
            else if (EffectType == Effect.EffectTypes.Poison_Cloud)
            {
                #region PoisonCloud
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);
                centerCell.SendShout("a hissing sound.");
                AreaEffect effect = new AreaEffect(EffectType, Cell.GRAPHIC_POISON_CLOUD, ((int)Skills.GetSkillLevel(caster.magic) * 2), 8, caster, areaCellList);

                foreach (Cell cell in areaCellList)
                {
                    AreaEffect.DoAreaEffect(cell, effect);

                    foreach (Character chr in cell.Characters.Values)
                        if (!chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                            chr.WriteToDisplay("You have been surrounded by a poison cloud!");
                }
                #endregion
            }
            else if (EffectType == Effect.EffectTypes.Locust_Swarm)
            {
                #region Locust Swarm
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);
                centerCell.SendShout("the sound of winged death approaching!");
                int multiplier = 7;

                if (caster.FindHeldItem(Item.ID_BLOODWOOD_TOTEM) != null)
                    multiplier += 2;

                AreaEffect effect = new AreaEffect(EffectType, Cell.GRAPHIC_LOCUST_SWARM, (Skills.GetSkillLevel(caster.magic) * multiplier) + GameSpell.GetSpellDamageModifier(caster), 6, caster, areaCellList);

                foreach (Cell cell in areaCellList)
                {
                    AreaEffect.DoAreaEffect(cell, effect);

                    foreach (Character chr in cell.Characters.Values)
                        if (!chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                            chr.WriteToDisplay("You are surrounded by a swarm of stinging locusts!");
                }
                #endregion
            }
        }

        public void CastGenericAreaSpell(Character caster, string args, Effect.EffectTypes EffectType, int EffectPower, string spellName)
        {
            int radius = 2; // default radius of 2 from target/target cell
            int XCordMod = 0;
            int YCordMod = 0;
            int argCount = 0;

            string[] sArgs = args.Split(" ".ToCharArray());

            if (EffectPower == 0)
                EffectPower = 15;

            // Check arguments for starting point of effect or target.
            if (sArgs[0] == null)
            {
                // do nothing
            }
            else
            {
                Character target = FindAndConfirmSpellTarget(caster, args);

                if (target != null)
                {
                    if (caster.Y > target.Y)
                        YCordMod -= (caster.Y - target.Y);
                    else
                        YCordMod += target.Y - caster.Y;
                    if (caster.X > target.X)
                        XCordMod -= (caster.X - target.X);
                    else
                        XCordMod += (target.X - caster.X);
                }
                else
                {
                    #region Loop through arguments to find the starting point.

                    int maxDirectionArgs = 3;
                    while (argCount < sArgs.Length)
                    {
                        if (maxDirectionArgs <= 0) break;

                        switch (sArgs[argCount])
                        {
                            case "north":
                            case "n":
                                YCordMod -= 1;
                                maxDirectionArgs--;
                                break;
                            case "south":
                            case "s":
                                YCordMod += 1;
                                maxDirectionArgs--;
                                break;
                            case "west":
                            case "w":
                                XCordMod -= 1;
                                maxDirectionArgs--;
                                break;
                            case "east":
                            case "e":
                                XCordMod += 1;
                                maxDirectionArgs--;
                                break;
                            case "northeast":
                            case "ne":
                                YCordMod -= 1;
                                XCordMod += 1;
                                maxDirectionArgs--;
                                break;
                            case "northwest":
                            case "nw":
                                YCordMod -= 1;
                                XCordMod -= 1;
                                maxDirectionArgs--;
                                break;
                            case "southeast":
                            case "se":
                                YCordMod += 1;
                                XCordMod += 1;
                                maxDirectionArgs--;
                                break;
                            case "southwest":
                            case "sw":
                                YCordMod += 1;
                                XCordMod -= 1;
                                maxDirectionArgs--;
                                break;
                            case "1":
                                radius = 1;
                                break;
                            case "2":
                                radius = 2;
                                break;
                            case "3":
                                radius = 3;
                                break;
                            case "4":
                                radius = 4;
                                break;
                            case "5":
                                radius = 5;
                                break;
                            case "6":
                                radius = 6;
                                break;
                            default:
                                break;
                        }
                        argCount++;
                    }
                    #endregion
                }

                // turn undead effects all undead in view only
                if (EffectType == Effect.EffectTypes.Turn_Undead)
                    radius = 3;
                else if (EffectType == Effect.EffectTypes.Thunderwave)
                    radius = 4;
                else if (EffectType == Effect.EffectTypes.Ensnare)
                    radius = 1;

                var areaCellList = new ArrayList();
                var centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);

                #region Create the list of effect cells.
                if (radius <= 1)
                {
                    EffectPower = EffectPower * 2;

                    if (!Map.IsSpellPathBlocked(centerCell) && !areaCellList.Contains(centerCell))
                        areaCellList.Add(centerCell);
                }
                else
                {
                    if (radius > 2)
                        EffectPower = EffectPower / 2;

                    var cellsArray = Cell.GetApplicableCellArray(centerCell, radius);

                    if (!areaCellList.Contains(centerCell))
                        areaCellList.Add(centerCell);

                    foreach (Cell appCell in cellsArray)
                        if (!Map.IsSpellPathBlocked(appCell) && !areaCellList.Contains(appCell))
                            areaCellList.Add(appCell);
                }
                #endregion

                AreaEffect effect = null;
                int duration = 1;
                switch (EffectType)
                {
                    case Effect.EffectTypes.Concussion:
                        #region Concussion
                        centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);
                        var cellArray = Cell.GetApplicableCellArray(centerCell, radius);
                        if (!string.IsNullOrEmpty(SoundFile)) { centerCell.EmitSound(this.SoundFile); }
                        effect = new AreaEffect(EffectType, Cell.GRAPHIC_EMPTY, EffectPower, duration, caster, areaCellList);
                        foreach (Cell cell in areaCellList)
                        {
                            Combat.DoConcussionDamage(cell, EffectPower, caster);
                        }

                        ArrayList brokenWallsList = new ArrayList();

                        for (int j = 0; j < cellArray.Length; j++)
                        {
                            if (cellArray[j] != null)
                            {
                                if ((cellArray[j].CellGraphic == Cell.GRAPHIC_WALL || cellArray[j].CellGraphic == Cell.GRAPHIC_SECRET_DOOR) && !cellArray[j].IsMagicDead)
                                {
                                    if (Rules.RollD(1, 100) < 30)
                                    {
                                        brokenWallsList.Add(cellArray[j]);
                                    }
                                }
                            }
                        }

                        if (brokenWallsList.Count > 0)
                            effect = new AreaEffect(Effect.EffectTypes.Illusion, Cell.GRAPHIC_RUINS_RIGHT, 1, -1, null, brokenWallsList);
                        #endregion
                        break;
                    case Effect.EffectTypes.Fire:
                        #region Fireball
                        centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);

                        if (!string.IsNullOrEmpty(SoundFile)) { centerCell.EmitSound(this.SoundFile); }
                        
                        centerCell.SendShout("a whoosh of fire!");
                        if (radius == 1) // add some power to a focused spell 
                        {
                            EffectPower = EffectPower * 3;
                            duration = 3;
                        }

                        effect = new AreaEffect(EffectType, Cell.GRAPHIC_FIRE, EffectPower, duration, caster, areaCellList);
                        var fireballGraphic = new AreaEffect(Effect.EffectTypes.Fireball, Cell.GRAPHIC_EMPTY, EffectPower, duration, caster, areaCellList);

                        foreach (Cell cell in areaCellList)
                        {
                            foreach (Character chr in cell.Characters.Values)
                            {
                                if (!chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                                {
                                    chr.WriteToDisplay("You have been hit by a fireball!");
                                    // Fireball kills ensnare.
                                    if (chr.EffectsList.ContainsKey(Effect.EffectTypes.Ensnare))
                                        chr.EffectsList[Effect.EffectTypes.Ensnare].StopCharacterEffect();
                                }
                            }
                        }
                        #endregion
                        break;
                    case Effect.EffectTypes.Ice:
                        #region Icestorm
                        centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);

                        if (SoundFile != "") { centerCell.EmitSound(SoundFile); }
                        duration = 1;
                        centerCell.SendShout("the pounding of giant ice pellets!");
                        if (radius == 1) // add some power to a focused spell 
                        {
                            EffectPower = EffectPower * 3;
                            duration = 3;
                        }
                        effect = new AreaEffect(EffectType, Cell.GRAPHIC_ICE_STORM, EffectPower, duration, caster, areaCellList);

                        foreach (Cell cell in areaCellList)
                        {
                            foreach (Character chr in cell.Characters.Values)
                            {
                                if (!chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                                {
                                    chr.WriteToDisplay("You have been hit by a raging ice storm!");
                                    // Icestorm kills flame shield.
                                    if (chr.EffectsList.ContainsKey(Effect.EffectTypes.Flame_Shield))
                                        chr.EffectsList[Effect.EffectTypes.Flame_Shield].StopCharacterEffect();
                                }
                            }
                        }
                        #endregion
                        break;
                    case Effect.EffectTypes.Lightning_Storm:
                        #region Lightning Storm
                        centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);
                        if (this.SoundFile != "") { centerCell.EmitSound(this.SoundFile); }
                        centerCell.SendShout("an ear-splitting thunderclap!");
                        effect = new AreaEffect(EffectType, Cell.GRAPHIC_LIGHTNING_STORM, EffectPower, 2, caster, areaCellList);
                        foreach (Cell cell in areaCellList)
                        {
                            foreach (Character chr in cell.Characters.Values)
                                if (!chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                                    chr.WriteToDisplay("You have been hit by a violent lightning storm!");
                        }
                        #endregion
                        break;
                    case Effect.EffectTypes.Poison_Cloud:
                    case Effect.EffectTypes.Poison:
                        #region Poison Cloud
                        centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);
                        effect = new AreaEffect(EffectType, Cell.GRAPHIC_POISON_CLOUD, EffectPower, 2, caster, areaCellList);
                        centerCell.SendShout("a hissing sound.");
                        if (this.SoundFile != "") { centerCell.EmitSound(this.SoundFile); }
                        foreach (Cell cell in areaCellList)
                        {
                            foreach (Character chr in cell.Characters.Values)
                                if (!chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                                    chr.WriteToDisplay("You have been hit by a poison cloud!");
                        }
                        #endregion
                        break;
                    case Effect.EffectTypes.Turn_Undead:
                        #region Turn Undead
                        centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);
                        if (SoundFile != "") { centerCell.EmitSound(SoundFile); }
                        effect = new AreaEffect(EffectType, "", EffectPower, 1, caster, areaCellList);
                        foreach (Cell cell in areaCellList)
                            foreach (Character chr in cell.Characters.Values)
                                if (!chr.IsDead)
                                    chr.WriteToDisplay("You feel a strong, divine wind race through the area.");
                        #endregion
                        break;
                    case Effect.EffectTypes.Light:
                        #region Light
                        centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);

                        if (this.SoundFile != "") { centerCell.EmitSound(this.SoundFile); }

                        effect = new AreaEffect(EffectType, "", EffectPower, Skills.GetSkillLevel(caster.magic), caster, areaCellList);

                        foreach (Cell cell in areaCellList)
                        {
                            // stop darkness effect, even if it is permanent (darkness will return after the light effect ends)
                            if (cell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness))
                                cell.AreaEffects[Effect.EffectTypes.Darkness].StopAreaEffect();
                            if(cell.IsAlwaysDark)
                            {
                                cell.DisplayGraphic = cell.CellGraphic;
                            }
                            // send message to characters
                            foreach (Character chr in cell.Characters.Values)
                            {
                                if (!chr.IsDead)
                                    chr.WriteToDisplay("The area is illuminated by a burst of magical light!");
                            }
                        }

                        #endregion
                        break;
                    case Effect.EffectTypes.Darkness:
                        #region Darkness
                        centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);

                        if (this.SoundFile != "") { centerCell.EmitSound(this.SoundFile); }

                        effect = new AreaEffect(EffectType, Cell.GRAPHIC_DARKNESS, 0, EffectPower, caster, areaCellList);

                        foreach (Cell cell in areaCellList)
                            foreach (Character chr in cell.Characters.Values)
                                if (!chr.IsDead)
                                    chr.WriteToDisplay("The area is covered in a shroud of magical darkness!");
                        #endregion
                        break;
                    case Effect.EffectTypes.Acid:
                        #region Acid
                        centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);

                        if (SoundFile != "") { centerCell.EmitSound(SoundFile); }

                        centerCell.SendShout("a torrent of destructive acid rain!");

                        effect = new AreaEffect(EffectType, Cell.GRAPHIC_ACID_STORM, EffectPower, 1, caster, areaCellList);

                        foreach (Cell cell in areaCellList)
                        {
                            foreach (Character chr in cell.Characters.Values)
                            {
                                if (!chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                                {
                                    chr.WriteToDisplay("You have been hit by acid rain!");
                                    // Acid kills ensnare.
                                    if (chr.EffectsList.ContainsKey(Effect.EffectTypes.Ensnare))
                                        chr.EffectsList[Effect.EffectTypes.Ensnare].StopCharacterEffect();
                                }
                            }
                        }
                        #endregion
                        break;
                    case Effect.EffectTypes.Locust_Swarm:
                        #region Locust Swarm
                        centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);
                        if (SoundFile != "") { centerCell.EmitSound(SoundFile); }
                        centerCell.SendShout("the sound of millions of wings buzzing!");
                        effect = new AreaEffect(EffectType, Cell.GRAPHIC_LOCUST_SWARM, EffectPower, 2, caster, areaCellList);
                        foreach (Cell cell in areaCellList)
                        {
                            foreach (Character chr in cell.Characters.Values)
                                if (!chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                                {
                                    if (!chr.IsBlind && !Combat.DND_CheckSavingThrow(chr, Combat.SavingThrow.Spell, 0))
                                    {
                                        chr.WriteToDisplay("You are blinded by the swarm!");
                                        Effect.CreateCharacterEffect(Effect.EffectTypes.Blind, 1, chr, 1, caster);
                                        // Locust Swarm kills ensnare.
                                        if (chr.EffectsList.ContainsKey(Effect.EffectTypes.Ensnare))
                                            chr.EffectsList[Effect.EffectTypes.Ensnare].StopCharacterEffect();
                                        // Locust Swarm kills flame shield
                                        if (chr.EffectsList.ContainsKey(Effect.EffectTypes.Flame_Shield))
                                            chr.EffectsList[Effect.EffectTypes.Flame_Shield].StopCharacterEffect();
                                    }
                                }
                        }
                        #endregion
                        break;
                    case Effect.EffectTypes.Thunderwave:
                        #region Thunderwave
                        centerCell = caster.CurrentCell;

                        if (SoundFile != "") { centerCell.EmitSound(SoundFile); }

                        centerCell.SendShout("a wave of powerful thunder!");
                        
                        effect = new AreaEffect(EffectType, "", EffectPower, duration, caster, areaCellList);

                        foreach (Cell cell in areaCellList)
                        {
                            foreach (Character chr in cell.Characters.Values)
                            {
                                if (chr != caster && !chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                                {
                                    chr.WriteToDisplay("You have been hit by an invisible wave of thunder!");
                                }
                            }
                        }
                        #endregion
                        break;
                    case Effect.EffectTypes.Ensnare:
                        #region Ensnare
                        centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);

                        if (SoundFile != "") { centerCell.EmitSound(SoundFile); }

                        if (target != null)
                            target.SendToAllInSight("Thick vines suddenly sprout up around " + target.GetNameForActionResult(true) + "!");

                        effect = new AreaEffect(EffectType, "\"\"", EffectPower, duration, caster, areaCellList);

                        foreach (Cell cell in areaCellList)
                        {
                            foreach (Character chr in cell.Characters.Values)
                            {
                                if (chr != caster && !chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                                {
                                    Effect.CreateCharacterEffect(Effect.EffectTypes.Ensnare, 1, chr, Rules.Dice.Next(Skills.GetSkillLevel(caster.magic), Skills.GetSkillLevel(caster.magic) * 2), caster);
                                    chr.WriteToDisplay("You have been ensnared in vines!");
                                    // Ensnare cancels out Flame Shield (Flame Shield may be cast again and kill the vines...)
                                    if (chr.EffectsList.ContainsKey(Effect.EffectTypes.Flame_Shield))
                                    {
                                        chr.WriteToDisplay("The vines snuff out your flame shield!");
                                        chr.EffectsList[Effect.EffectTypes.Flame_Shield].StopCharacterEffect();
                                    }
                                }
                            }
                        }
                        #endregion
                        break;
                }
            }
        }

        public void CastConeSpell(Character caster, GameSpell spell, string args)
        {
            string[] directionStrings = new[] { "n", "s", "e", "w", "nw", "ne", "sw", "se" };

            if (args == null) args = "";

            args = args.Replace(spell.Command + " ", "");
            args = args.Replace(spell.Command, "");

            Effect.EffectTypes effectType = Effect.EffectTypes.None;
            string effectGraphic = "  ";
            int effectPower = 1;
            string spellMsg = "";
            string spellEmote = "";
            string direction = "";
            string spellSound = "";

            string[] sArgs = args.Split(" ".ToCharArray());

            ArrayList areaCellList = new ArrayList(); // the cells that will be effected by the cone            

            if (spell.Command == "drbreath")
            {
                if ((sArgs.Length >= 1 && sArgs[0] == "ice") ||
                    caster.species == Globals.eSpecies.IceDragon ||
                    caster.entity == Autonomy.EntityBuilding.EntityLists.Entity.Blue_Dragon)
                {
                    effectType = Effect.EffectTypes.Dragon__s_Breath_Ice;
                    effectGraphic = Cell.GRAPHIC_ICE_STORM;
                    effectPower = Skills.GetSkillLevel(caster.magic) * 8;
                    spellMsg = "You are hit by a frigid blast of " + this.Name.ToLower() + "!";
                    spellEmote = " breathes a cone of ice!";
                    spellSound = Sound.GetCommonSound(Sound.CommonSound.IceStorm);
                }
                else if ((sArgs.Length >= 1 && sArgs[0] == "wind") ||
                    caster.species == Globals.eSpecies.WindDragon)
                {
                    effectType = Effect.EffectTypes.Dragon__s_Breath_Wind;
                    effectGraphic = Cell.GRAPHIC_WHIRLWIND;
                    effectPower = Skills.GetSkillLevel(caster.magic) * 8;
                    spellMsg = "You are hit by a blast of hot wind!";
                    spellEmote = " breathes a cone of stinging wind!";
                    spellSound = Sound.GetCommonSound(Sound.CommonSound.Whirlwind);
                }
                else if (caster.species == Globals.eSpecies.CloudDragon && args.ToLower().Contains("stormbreath")) // TODO: determine if non dragon casters of dragon's breath can cast storm breath
                {
                    effectType = Effect.EffectTypes.Dragon__s_Breath_Storm;
                    effectGraphic = Cell.GRAPHIC_LIGHTNING_STORM;
                    effectPower = Skills.GetSkillLevel(caster.magic) * 8;
                    spellMsg = "You are pelted by massive hailstones and stung by a frigid blasts of wind and blinding lightning!";
                    spellEmote = " breathes a cone of raging wind, lightning and hailstones!";
                    spellSound = Sound.GetCommonSound(Sound.CommonSound.Whirlwind);
                }
                else if (caster.entity == Autonomy.EntityBuilding.EntityLists.Entity.Broodmother ||
                    caster.entity == Autonomy.EntityBuilding.EntityLists.Entity.Green_Dragon) // TODO: determine if non dragon casters of dragon's breath can cast storm breath
                {
                    effectType = Effect.EffectTypes.Dragon__s_Breath_Poison;
                    effectGraphic = Cell.GRAPHIC_POISON_CLOUD;
                    effectPower = Skills.GetSkillLevel(caster.magic) * 8;
                    spellMsg = "You are smothered by a cone of poisonous gas!";
                    spellEmote = " breathes a cone of rancid, poisonous gas!";
                    spellSound = Sound.GetCommonSound(Sound.CommonSound.Whirlwind);
                }
                else
                {
                    effectType = Effect.EffectTypes.Dragon__s_Breath_Fire;
                    effectGraphic = Cell.GRAPHIC_FIRE;
                    effectPower = Skills.GetSkillLevel(caster.magic) * 8;
                    spellMsg = "You are hit by searing " + this.Name.ToLower() + "! Your skin blisters and burns!";
                    spellEmote = " breathes fire!";
                    spellSound = Sound.GetCommonSound(Sound.CommonSound.Fireball);
                }
            }

            Character target = spell.FindAndConfirmSpellTarget(caster, sArgs[sArgs.Length - 1]);

            //if cone is cast at something figure out the direction
            if (target != null && target != caster) // NPCs will not put in a direction, and if PC caster used a name then we need a direction
            {
                if (target.CurrentCell != caster.CurrentCell)
                    direction = Utils.FormatEnumString(Map.GetDirection(caster.CurrentCell, target.CurrentCell).ToString()).ToLower();

                else if (target.X == caster.X && target.Y == caster.Y)
                {
                    switch (caster.dirPointer)
                    {
                        case "^": direction = "n"; break;
                        case "v": direction = "s"; break;
                        case ">": direction = "e"; break;
                        case "<": direction = "w"; break;
                        default:
                            switch (Rules.Dice.Next(3) + 1)
                            {
                                case 1: direction = "n"; break;
                                case 2: direction = "s"; break;
                                case 3: direction = "e"; break;
                                case 4: direction = "w"; break;
                            }
                            break;
                    }

                    areaCellList.Add(caster.CurrentCell);
                }
            }
            else if (Array.IndexOf(directionStrings, sArgs[sArgs.Length - 1]) > -1)
            {
                direction = sArgs[sArgs.Length - 1];
            }
            else // no direction and no target
            {
                switch (caster.dirPointer)
                {
                    case "^": direction = "n"; break;
                    case "v": direction = "s"; break;
                    case ">": direction = "e"; break;
                    case "<": direction = "w"; break;
                    default:
                        switch (Rules.Dice.Next(3) + 1)
                        {
                            case 1: direction = "n"; break;
                            case 2: direction = "s"; break;
                            case 3: direction = "e"; break;
                            case 4: direction = "w"; break;
                        }
                        break;
                }

                areaCellList.Add(caster.CurrentCell);
            }

            Cell cell = Map.GetCellRelevantToCell(caster.CurrentCell, args, false); // get the immediate direction of the cone

            if (!Map.IsSpellPathBlocked(cell))
                areaCellList.Add(cell);

            #region Add cells in the cone to the ArrayList based on the direction.
            switch (direction)
            {
                case "north":
                case "n":
                    #region north
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y - 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y - 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y - 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y - 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y - 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y - 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y - 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y - 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y - 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y - 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y - 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y - 3, caster.Z));
                    #endregion
                    break;
                case "south":
                case "s":
                    #region south
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y + 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y + 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y + 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y + 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y + 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y + 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y + 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y + 3, caster.Z));
                    #endregion
                    break;
                case "west":
                case "w":
                    #region west
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y - 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y - 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y - 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y - 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y - 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y - 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y + 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y + 3, caster.Z));
                    #endregion
                    break;
                case "east":
                case "e":
                    #region east
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y - 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y - 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y - 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y - 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y - 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y - 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y + 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y + 3, caster.Z));
                    #endregion
                    break;
                case "northwest":
                case "nw":
                    #region northwest
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y - 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y - 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y - 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y - 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y - 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y - 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y - 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y - 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 1, caster.Z));
                    #endregion
                    break;
                case "northeast":
                case "ne":
                    #region northeast
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y - 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y - 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y - 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y - 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y - 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y - 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y - 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y - 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 1, caster.Z));
                    #endregion
                    break;
                case "southwest":
                case "sw":
                    #region southwest
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y + 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y + 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 3, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y + 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y + 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 2, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 1, caster.Z));
                    #endregion
                    break;
                case "southeast":
                case "se":
                    #region southeast
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y + 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y + 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 3, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y + 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y + 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 2, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 3, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 3, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 1, caster.Z));
                    #endregion
                    break;
            }
            #endregion

            caster.SendToAllInSight(caster.GetNameForActionResult() + spellEmote);

            if (spellSound != "") caster.EmitSound(spellSound);

            if (spellMsg != "")
            {
                foreach (Cell bCell in areaCellList)
                {
                    foreach (Character chr in bCell.Characters.Values)
                    {
                        if (chr != caster)
                        {
                            chr.WriteToDisplay(spellMsg);
                        }
                    }
                }
            }

            effectPower = effectPower - Rules.Dice.Next(-5, 5);
            if (effectPower < 1) { effectPower = 5; }
            AreaEffect effect = new AreaEffect(effectType, effectGraphic, effectPower, 1, caster, areaCellList);
            return;
        }

        public void CastWallSpell(Character caster, GameSpell spell, string args, Effect.EffectTypes EffectType, int EffectPower)
        {
            args = args.Replace(spell.Command, "");
            args = args.Trim();

            string EffectGraphic = "  ";
            string spellMsg = "";
            string spellShout = "";
            bool spellStun = false;

            ArrayList areaCellList = new ArrayList(); // the cells that will be effected by the wall

            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = spell.FindAndConfirmSpellTarget(caster, sArgs[sArgs.Length - 1]);

            Cell cell = null;

            string castDirection = "";

            if (target != null)
            {
                castDirection = Map.GetDirection(caster.CurrentCell, target.CurrentCell).ToString().ToLower();
                cell = target.CurrentCell;
            }
            else
            {
                cell = Map.GetCellRelevantToCell(caster.CurrentCell, args, false); // get the center cell, where the firewall starts
                castDirection = sArgs[sArgs.Length - 1];
            }

            areaCellList.Add(cell);
            switch (castDirection)
            {
                case "north":
                case "south":
                case "n":
                case "s":
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y, caster.Z));
                    break;
                case "west":
                case "east":
                case "w":
                case "e":
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 1, caster.Z));
                    break;
                case "northeast":
                case "southwest":
                case "ne":
                case "sw":
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 1, caster.Z));
                    break;
                case "northwest":
                case "southeast":
                case "se":
                case "nw":
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 1, caster.Z));
                    break;
                default:
                    areaCellList.Remove(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y - 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X - 1, cell.Y + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X, cell.Y + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, cell.X + 1, cell.Y + 1, caster.Z));
                    break;
            }

            switch (EffectType)
            {
                // Graphic, spell messages, spell shouts, effect power changes -- double check area cells list
                case Effect.EffectTypes.Fog:
                    EffectGraphic = Cell.GRAPHIC_FOG;
                    spellMsg = "You are rapidly surrounded by fog!";
                    EffectPower = 0;
                    break;
                case Effect.EffectTypes.Fire:
                    EffectGraphic = Cell.GRAPHIC_FIRE;
                    spellMsg = "You are burned by a wall of fire!";
                    spellShout = "a whoosh of fire.";
                    break;
                case Effect.EffectTypes.Ice:
                    EffectGraphic = Cell.GRAPHIC_ICE_STORM;
                    spellMsg = "You are consumed in a wall of ice!";
                    spellShout = "a loud hiss and the sound of ice cracking.";
                    spellStun = true;
                    break;
                case Effect.EffectTypes.Light:
                    EffectGraphic = "";
                    spellMsg = "The area is illuminated by a wall of magical light!";
                    break;
                case Effect.EffectTypes.Dragon__s_Breath_Fire:
                    EffectGraphic = Cell.GRAPHIC_FIRE;
                    spellMsg = "You are surrounded by a wall of searing dragon's breath!";
                    spellShout = "a whoosh of fire.";
                    break;
                case Effect.EffectTypes.Darkness:
                    EffectGraphic = Cell.GRAPHIC_DARKNESS;
                    spellMsg = "The area is covered in a wall of magical darkness!";
                    break;
                case Effect.EffectTypes.Lightning:
                    EffectGraphic = Cell.GRAPHIC_LIGHTNING_STORM;
                    spellMsg = "You are struck by a bolt of lightning!";
                    break;
                case Effect.EffectTypes.Shelter:
                    EffectGraphic = Cell.GRAPHIC_FOREST_IMPASSABLE;
                    spellMsg = "You are surrounded by a wall of thick oak trees and thorny brambles!";
                    spellShout = "an increasingly loud creaking of expanding wood.";
                    for(int i = 0; i < areaCellList.Count; i++)
                    {
                        if ((areaCellList[i] as Cell).Characters.Count > 0)
                            areaCellList.RemoveAt(i);
                    }
                    EffectPower = 0;
                    break;
            }
            // generic shout message
            if (!string.IsNullOrEmpty(spellShout)) cell.SendShout(spellShout);

            if(!string.IsNullOrEmpty(spell.SoundFile)) cell.EmitSound(spell.SoundFile);
            //message to those in the area of effect, and stun for ice wall
            if (!string.IsNullOrEmpty(spellMsg))
            {
                foreach (Cell c in areaCellList)
                {
                    foreach (Character chr in c.Characters.Values)
                    {
                        if (!chr.IsDead)
                        {
                            Combat.DoSpellDamage(caster, chr, null, EffectPower, EffectType.ToString().ToLower());
                            chr.WriteToDisplay(spellMsg);
                            if (spellStun)
                            {
                                if (Combat.DoSpellDamage(caster, chr, null, 0, "stun") == 1)
                                {
                                    chr.Stunned += (short)Rules.RollD(1, 4);
                                }
                            }
                        }

                    }
                }
            }
            // create the wall
            int skillLevel = Skills.GetSkillLevel(caster, Globals.eSkillType.Magic);
            if (skillLevel <= 1) skillLevel = 2;
            int duration = Rules.RollD(skillLevel, 10);
            if (EffectType == Effect.EffectTypes.Lightning) duration = 1;

            AreaEffect effect = new AreaEffect(EffectType, EffectGraphic, EffectPower, duration, caster, areaCellList);
        }

        public void CastChainSpell(Character caster, Character target, Effect.EffectTypes effectType, int damage, string spellType, string description, string soundFile)
        {
            if (target == null) return;
            if (target.CurrentCell == null) return;

            Cell[] visibleCells = Cell.GetApplicableCellArray(target.CurrentCell, Cell.DEFAULT_VISIBLE_DISTANCE);

            List<Cell> randomizedCells = new List<Cell>(visibleCells);

            foreach (Cell cell in randomizedCells)
                if (cell.Characters.Count < 1) randomizedCells.Remove(cell);

            Utils.Shuffle(ref randomizedCells);

            int numAffectedCharacters = Rules.RollD(2, Rules.RollD(1, 4));

            List<Character> buzzed = new List<Character>();
            List<Cell> areaEffectCells = new List<Cell>();

            while (numAffectedCharacters > 0)
            {
                Cell chosenCell = randomizedCells[Rules.Dice.Next(randomizedCells.Count)];
                buzzed.Add(chosenCell.Characters[Rules.Dice.Next(chosenCell.Characters.Count)]);
                areaEffectCells.Add(chosenCell);
                numAffectedCharacters--;
            }

            foreach (Character linedUp in new List<Character>(buzzed))
            {
                // Sound effects and words and stuff.
                linedUp.SendToAllInSight(linedUp.GetNameForActionResult() + " is struck by " + description + " of " + spellType + "!");
                linedUp.WriteToDisplay("You are struck by " + description + " of " + spellType + "!");
                linedUp.EmitSound(soundFile);

                if (Combat.DoSpellDamage(caster, linedUp, null, damage, spellType) == 1)
                {
                    Rules.GiveKillExp(caster, target);
                    Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
                }
            }
        }

        public void CastPathSpell(Character caster, string args, Effect.EffectTypes effectType, int damage, string spellType, string description, string soundFile)
        {
            int XCordMod = 0;
            int YCordMod = 0;
            int argCount = 0;

            string[] sArgs = args.Split(" ".ToCharArray());
            var areaCellList = new ArrayList();

            // Check arguments for starting point of effect or target.
            if (sArgs[0] == null)
            {
                // do nothing
            }
            else
            {
                Character target = FindAndConfirmSpellTarget(caster, args);

                if (target != null)
                {
                    if (caster.Y > target.Y)
                        YCordMod -= (caster.Y - target.Y);
                    else
                        YCordMod += target.Y - caster.Y;
                    if (caster.X > target.X)
                        XCordMod -= (caster.X - target.X);
                    else
                        XCordMod += (target.X - caster.X);
                }
                else
                {
                    int maxDirectionArgs = Skills.GetSkillLevel(caster.magic);
                    Cell cellLine = null;
                    Cell curCell = caster.CurrentCell;
                    while (argCount < sArgs.Length)
                    {
                        if (maxDirectionArgs <= 0) break;
                        switch (sArgs[argCount])
                        {
                            case "north":
                            case "n":
                                YCordMod -= 1;
                                cellLine = Cell.GetCell(curCell.FacetID, curCell.LandID, curCell.MapID, curCell.X+XCordMod, curCell.Y+YCordMod, curCell.Z);
                                maxDirectionArgs--;
                                break;
                            case "south":
                            case "s":
                                YCordMod += 1;
                                cellLine = Cell.GetCell(curCell.FacetID, curCell.LandID, curCell.MapID, curCell.X + XCordMod, curCell.Y + YCordMod, curCell.Z);
                                maxDirectionArgs--;
                                break;
                            case "west":
                            case "w":
                                XCordMod -= 1;
                                cellLine = Cell.GetCell(curCell.FacetID, curCell.LandID, curCell.MapID, curCell.X + XCordMod, curCell.Y + YCordMod, curCell.Z);
                                maxDirectionArgs--;
                                break;
                            case "east":
                            case "e":
                                XCordMod += 1;
                                cellLine = Cell.GetCell(curCell.FacetID, curCell.LandID, curCell.MapID, curCell.X + XCordMod, curCell.Y + YCordMod, curCell.Z);
                                maxDirectionArgs--;
                                break;
                            case "northeast":
                            case "ne":
                                YCordMod -= 1;
                                XCordMod += 1;
                                cellLine = Cell.GetCell(curCell.FacetID, curCell.LandID, curCell.MapID, curCell.X + XCordMod, curCell.Y + YCordMod, curCell.Z);
                                maxDirectionArgs--;
                                break;
                            case "northwest":
                            case "nw":
                                YCordMod -= 1;
                                XCordMod -= 1;
                                cellLine = Cell.GetCell(curCell.FacetID, curCell.LandID, curCell.MapID, curCell.X + XCordMod, curCell.Y + YCordMod, curCell.Z);
                                maxDirectionArgs--;
                                break;
                            case "southeast":
                            case "se":
                                YCordMod += 1;
                                XCordMod += 1;
                                cellLine = Cell.GetCell(curCell.FacetID, curCell.LandID, curCell.MapID, curCell.X + XCordMod, curCell.Y + YCordMod, curCell.Z);
                                maxDirectionArgs--;
                                break;
                            case "southwest":
                            case "sw":
                                YCordMod += 1;
                                XCordMod -= 1;
                                cellLine = Cell.GetCell(curCell.FacetID, curCell.LandID, curCell.MapID, curCell.X + XCordMod, curCell.Y + YCordMod, curCell.Z);
                                maxDirectionArgs--;
                                break;
                        }
                        argCount++;
                        areaCellList.Add(cellLine);
                    }
                }

                
                switch(effectType)
                {
                    case Effect.EffectTypes.Lightning:
                        foreach(Cell c in areaCellList)
                        {
                            if (c.Characters.Count > 0)
                            {
                                List<Character> affectedList = new List<Character>(c.Characters.Values);
                                if (affectedList.Count > 0)
                                {
                                    foreach (Character affected in affectedList)
                                    {
                                        affected.SendToAllInSight(affected.GetNameForActionResult() + " is struck by " + description + " of " + spellType + "!");
                                        affected.WriteToDisplay("You have been hit by " + description + " of " + spellType + "!");
                                        if (Combat.DoSpellDamage(caster, affected, null, damage, "lightning") == 1)
                                        {
                                            Rules.GiveAEKillExp(caster, affected);
                                            Skills.GiveSkillExp(caster, affected, Globals.eSkillType.Magic);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public Character FindAndConfirmSpellTarget(Character caster, string args)
        {
            args = args.Replace(" at ", " "); // just get rid of the " at " in the args, makes things much easier

            Character target = null;

            if (args == null || args.Length == 0 || args == "" || args.ToLower() == this.Command.ToLower())
            {
                args = caster.Name;
                target = caster;
            }
            else
            {
                // confirm the name of the spell isn't in the arguments
                args = args.ToLower().Replace(this.Command.ToLower() + " ", "");
            }

            string[] sArgs = args.Split(" ".ToCharArray()); // this is for CAST # TARGET
            if (int.TryParse(args, out int uniqueID))
            {
                target = TargetAcquisition.FindTargetInSeenList(caster.seenList, uniqueID);

                if (this.IsBeneficial && target != null) return target;
                else if (!this.IsBeneficial) // must verify the target is still in view
                {
                    if (target != null)
                    {
                        if (TargetAcquisition.FindTargetInView(caster, target) != null) return target;
                    }
                    else
                    {
                        target = TargetAcquisition.FindTargetInView(caster, uniqueID, true, false);
                    }
                }

                if (target != null) return target;
            }

            // cast <spell command> <#> <target>
            // only <#> and <target> arrive at this method
            if (target == null && sArgs.Length >= 2)
            {
                try
                {
                    target = TargetAcquisition.FindTargetInView(caster, sArgs[1], Convert.ToInt32(sArgs[0]));
                }
                catch
                {
                    // do nothing
                }
            }

            if (target == null)
            {
                try
                {
                    if ((caster.Group != null && caster.Group.IsGroupMember(args)) || caster.Name == args)
                    {
                        target = TargetAcquisition.FindTargetInView(caster, Convert.ToInt32(args), true, true);
                    }
                    else
                    {
                        target = TargetAcquisition.FindTargetInView(caster, Convert.ToInt32(args), true, false);
                    }

                }
                catch
                {
                    target = TargetAcquisition.FindTargetInView(caster, args, true, true);
                }
            }

            if (caster == null || target == null || caster.preppedSpell == null) return null;

            #region Check targetType.
            switch (caster.preppedSpell.TargetType)
            {
                case Globals.eSpellTargetType.Area_Effect:
                    if (target == null || args == null)
                    {
                        target = caster;
                    }
                    if (target is Merchant)
                    {
                        if ((target as Merchant).merchantType > Merchant.MerchantType.None)
                        {
                            caster.WriteToDisplay("You cannot cast spells at a merchant yet.");
                            target = null;
                        }
                        if ((target as Merchant).trainerType > Merchant.TrainerType.None)
                        {
                            caster.WriteToDisplay("You cannot cast spells at a trainer yet.");
                            target = null;
                        }
                    }
                    break;
                case Globals.eSpellTargetType.Group:
                    // not implemented yet
                    break;
                case Globals.eSpellTargetType.Point_Blank_Area_Effect:
                    target = caster;
                    break;
                case Globals.eSpellTargetType.Self:
                    target = caster;
                    break;
                case Globals.eSpellTargetType.Single:
                    //if (target == null && name != null)
                    //{
                    //    caster.WriteToDisplay("You do not see " + name + " here.");
                    //};
                    break;
                case Globals.eSpellTargetType.Single_or_Group:
                    break;
                default:
                    caster.WriteToDisplay("ERROR: Inform the developers that you could not find and confirm a target for your spell.");
                    target = null;
                    break;
            }
            #endregion

            return target;
        }

        public void SendGenericCastMessage(Character caster, Character target, bool emitSound)
        {
            if (target == null) caster.WriteToDisplay("You cast " + Name + ".");
            else if (target != caster) caster.WriteToDisplay("You cast " + Name + " at " + target.GetNameForActionResult(true) + ".");

            if (emitSound) caster.EmitSound(SoundFile);
        }

        public void SendGenericEnchantMessage(Character caster, Character target)
        {
            target.WriteToDisplay("You have been enchanted with " + Name + "!");

            if(caster != null && caster != target)
                target.EmitSound(SoundFile);
        }

        public void SendGenericResistMessages(Character caster, Character target)
        {
            caster.WriteToDisplay(target.GetNameForActionResult() + " resists your " + Name + " spell!");
            target.WriteToDisplay("You resist a " + Name + " spell!");

            //// AI spell resist tracking.
            //if (caster is NPC)
            //{
            //    // Caster is already tracking resisted spells.
            //    if (AI.ResistedSpells.ContainsKey(caster.UniqueID))
            //    {
            //        // Target has not already resisted one of the caster's spells.
            //        if (!AI.ResistedSpells[caster.UniqueID].ContainsKey(target.UniqueID))
            //        {
            //            Dictionary<int, int> resistCountDict = new Dictionary<int,int>();
            //            resistCountDict.Add(this.ID, 1); // First resist of this GameSpell.                        

            //            // Key = caster.UniqueID
            //            // Value = Dictionary<Key = target.UniqueID, Value = Dictionary<GameSpell.ID, ResistCount>
            //            AI.ResistedSpells[caster.UniqueID].Add(target.UniqueID, resistCountDict);
            //        }
            //        else if (AI.ResistedSpells[caster.UniqueID][target.UniqueID].ContainsKey(this.ID))
            //        {
            //            // Increment the number of times this GameSpell has been resisted by the target.
            //            AI.ResistedSpells[caster.UniqueID][target.UniqueID][this.ID]++;
            //        }
            //        else
            //        {
            //            AI.ResistedSpells[caster.UniqueID][target.UniqueID].Add(this.ID, 1);
            //        }
            //    }
            //    else
            //    {
            //        Dictionary<int, int> resistCountDict = new Dictionary<int, int>();
            //        resistCountDict.Add(this.ID, 1); // First resist of this GameSpell.

            //        Dictionary<int, Dictionary<int, int>> resistedSpellsDict = new Dictionary<int, Dictionary<int, int>>();
            //        resistedSpellsDict.Add(target.UniqueID, resistCountDict);

            //        AI.ResistedSpells.Add(caster.UniqueID, resistedSpellsDict);
            //    }
            //}
        }

        public void SendGenericStrickenMessage(Character caster, Character target)
        {
            target.WriteToDisplay("You have been stricken with " + Name + "!");
            target.SendToAllInSight(target.GetNameForActionResult() + " is affected by " + Name + "!");

            if (caster != null && caster != target)
                target.EmitSound(SoundFile);
        }

        public void SendGenericUnaffectedMessage(Character caster, Character target)
        {
            target.WriteToDisplay("You are unaffected by " + Name + "!");

            if (caster != null)
            {
                caster.WriteToDisplay(target.GetNameForActionResult() + " is unaffected by your " + Name + " spell.");
                if (caster != target)
                target.EmitSound(SoundFile);
            }
        }

        public void WarmSpell(Character caster)
        {
            if (caster.IsUndead || !Autonomy.EntityBuilding.EntityLists.IsHumanOrHumanoid(caster))
            {
                if (!string.IsNullOrEmpty(caster.attackSound))
                    caster.EmitSound(caster.attackSound);
                else
                    caster.EmitSound(Sound.GetCommonSound(caster.gender == Globals.eGender.Female ? Sound.CommonSound.FemaleSpellWarm : Sound.CommonSound.MaleSpellWarm));
            }
            else
            {
                caster.EmitSound(Sound.GetCommonSound(caster.gender == Globals.eGender.Female ? Sound.CommonSound.FemaleSpellWarm : Sound.CommonSound.MaleSpellWarm));
                //caster.SendSound(Sound.GetCommonSound(caster.gender == Globals.eGender.Female ? Sound.CommonSound.FemaleSpellWarm : Sound.CommonSound.MaleSpellWarm));
            }

            caster.WriteToDisplay("You warm the spell " + Name + ".");
        }

        public bool IsClassSpell(Character.ClassType classType)
        {
            if (Array.IndexOf(this.ClassTypes, classType) > -1)
                return true;

            return false;
        }

        public static void ScribeSpell(Character caster, GameSpell spell)
        {
            string words = GenerateMagicWords();

            caster.spellDictionary.Add(spell.ID, words);

            if (caster is PC)
                (caster as PC).savePlayerSpells = true;

            caster.WriteToDisplay("You have scribed the spell " + spell.Name + " (" + spell.Command + ") into your spellbook.");
            caster.WriteToDisplay("The incantation is: " + words);

            if (caster.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                ProtocolYuusha.SendCharacterSpells(caster as PC, caster);
        }

        public bool CastSpell(Character caster, string args)
        {
            try
            {
                args = args.Replace(" at ", " "); // remove " at " from all cast command arguments
                args = args.Trim();
                args = args.Replace(this.Command, ""); // primarily added for the impcast command to remove the spell command from args
                args = args.Trim();

                if (caster is NPC && IsBeneficial)
                {
                    (caster as NPC).buffSpellCommand = "";
                    (caster as NPC).BuffTargetID = 0;
                }

                #region Spell Cast Logging
                if (caster.IsPC)
                {
                    if (caster.preppedSpell.IsBeneficial)
                    {
                        Utils.Log(caster.GetLogString() + " cast " + GetLogString(this) + " with args (" + args + ")", Utils.LogType.SpellBeneficialFromPlayer);
                    }
                    else
                    {
                        Utils.Log(caster.GetLogString() + " cast " + GetLogString(this) + " with args (" + args + ")", Utils.LogType.SpellHarmfulFromPlayer);
                    }
                }
                else
                {
                    if (caster.preppedSpell.IsBeneficial)
                    {
                        Utils.Log(caster.GetLogString() + " cast " + GetLogString(this) + " with args (" + args + ")", Utils.LogType.SpellBeneficialFromCreature);
                    }
                    else
                    {
                        Utils.Log(caster.GetLogString() + " cast " + GetLogString(this) + " with args (" + args + ")", Utils.LogType.SpellHarmfulFromCreature);
                    }
                }
                #endregion

                bool spellSuccess = Convert.ToBoolean(Handler.OnCast(caster, args));

                //int skillLevel = Skills.GetSkillLevel(caster, Globals.eSkillType.Magic);

                //if (caster.IsSpellWarmingProfession && IsClassSpell(caster.BaseProfession))
                //{
                //    if (spellSuccess && skillLevel >= RequiredLevel)
                //    {
                //        Skills.GiveSkillExp(caster, skillLevel * ManaCost, Globals.eSkillType.Magic);
                //    }
                //    else
                //    {
                //        Skills.GiveSkillExp(caster, skillLevel, Globals.eSkillType.Magic);
                //    }
                //}

                if (spellSuccess && caster is PC && caster.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                    ProtocolYuusha.SendCharacterCastSpell(caster as PC, ID);


                return spellSuccess;
            }
            catch (Exception e)
            {
                Utils.Log("Failure at spell.CastSpell(" + caster.GetLogString() + ", " + args + ")", Utils.LogType.SystemFailure);
                Utils.LogException(e);
                return false;
            }
        }

        public static void FillSpellLists(NPC npc)
        {
            npc.abjurationSpells.Clear();
            npc.alterationHarmfulSpells.Clear();
            npc.alterationSpells.Clear();
            npc.conjurationSpells.Clear();
            npc.divinationSpells.Clear();
            npc.evocationAreaEffectSpells.Clear();
            npc.evocationSpells.Clear();
            npc.illusionSpells.Clear();
            npc.necromancySpells.Clear();

            GameSpell spell;

            foreach (var id in npc.spellDictionary.Keys)
            {
                spell = GameSpell.GetSpell(id);
                var spellChant = npc.spellDictionary[id];

                switch (spell.SpellType)
                {
                    case Globals.eSpellType.Abjuration:
                        npc.abjurationSpells.Add(spell.ID, spellChant);
                        break;
                    case Globals.eSpellType.Alteration: // two types of alteration spell, beneficial and harmful
                        if (spell.IsBeneficial)
                        {
                            npc.alterationSpells.Add(spell.ID, spellChant);
                        }
                        else
                        {
                            npc.alterationHarmfulSpells.Add(spell.ID, spellChant);
                        }
                        break;
                    case Globals.eSpellType.Conjuration:
                        npc.conjurationSpells.Add(spell.ID, spellChant);
                        break;
                    case Globals.eSpellType.Divination:
                        npc.divinationSpells.Add(spell.ID, spellChant);
                        break;
                    case Globals.eSpellType.Evocation: // we separate these primarily for use by wizards
                        if (spell.TargetType == Globals.eSpellTargetType.Area_Effect)
                        {
                            npc.evocationAreaEffectSpells.Add(spell.ID, spellChant);
                        }
                        else
                        {
                            npc.evocationSpells.Add(spell.ID, spellChant);
                        }
                        break;
                    case Globals.eSpellType.Necromancy:
                        npc.necromancySpells.Add(spell.ID, spellChant);
                        break;
                }
            }
        }

        public static string GetSpellCastingNoun(Character.ClassType profession)
        {
            switch (profession)
            {
                case Character.ClassType.Sorcerer:
                    return "sorcery";
                case Character.ClassType.Thaumaturge:
                    return "thaumaturgy";
                case Character.ClassType.Wizard:
                    return "wizardry";
                case Character.ClassType.Druid:
                    return "druidism";
                case Character.ClassType.Thief:
                    return "shadow magic";
                case Character.ClassType.Ranger:
                    return "warden magic";
                default:
                    return "";
            }
        }
    }
}
