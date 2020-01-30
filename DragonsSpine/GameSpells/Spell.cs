using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DragonsSpine.GameWorld;

namespace DragonsSpine
{
    public class Spell
    {
        public static Dictionary<int, Spell> spellDictionary = new Dictionary<int, Spell>(); // global list of spells
        public static Dictionary<string, Spell> spellCommandDictionary = new Dictionary<string, Spell>();

        #region Constants
        public const int CURSE_SPELL_MULTIPLICAND_NPC = 4;
        public const int CURSE_SPELL_MULTIPLICAND_PC = 5;
        public const int DEATH_SPELL_MULTIPLICAND_NPC = 8;
        public const int DEATH_SPELL_MULTIPLICAND_PC = 9;
        public const int MAX_PETS = 3; // total number of non quest followers/pets currently allowed

        private const int MAGIC_WORDS_LENGTH = 4; // the total number of magic words per spell
        private const int BANISH_MULTI = 10;
        private const int RAISE_DEAD_SPELLID = 15;
        private const int IMAGE_CAST_DIRECTION_SKILL_REQUIREMENT = 11;

        public const string IMAGE_IDENTIFIER = "#";
        #endregion

        private static string[] magicWords = {"aazag","alla","alsi","anaku","angarru","anghizidda","anna","annunna","ardata","ashak",
            "baad","dingir","duppira","edin","enaa","endul","enmeshir","enn","ennul","esha","gallu","gidim","gish","ia","idpa","igigi",
            "ina","isa","khitim","kia","kielgallal","kima","ku","lalartu","limutuma","lini","ma","mardukka","masqim","mass","na","naa",
            "namtar","nebo","nenlil","nergal","ninda","ningi","ninn","ninnda","ninnghizhidda","ninnme","nngi","nushi","qutri","raa","sagba",
            "shadu","shammash","shunu","shurrim","telal","uhddil","urruku","uruki","utug","utuk","utuq","ya","yu","zi","zumri","kanpa",
            "ziyilqa","luubluyi","luudnin","luuppatar","xul","ssaratu","zu","barra","kunushi","tamatunu","ega","cuthalu","egura","asaru",
            "urma","muxxisha","akki","ilani","gishtugbi","arrflug"};

        #region Private Data
        int spellID; // each spell has a unique spellID
        bool beneficial; // used by AI to assist in determining which spell to cast at a target
        Character.ClassType[] classTypes; // array of classType that can cast this spell
        Globals.eSpellType spellType; // Abjuration, Alteration, Conjuration, Divination, Evocation, Necromancy (primarily used by AI)
        Globals.eSpellTargetType targetType; // Area_Effect, Group, Point_Blank_Area_Effect, Self, Single
        int manaCost; // mana cost to cast the spell
        int requiredLevel; // required casting level
        int trainingPrice; // purchase price
        string methodName; // method used
        string description; // description of the spell
        string spellCommand; // spell command
        string name; // name of the spell
        string soundFile; // sound file info
        bool availableAtTrainer; // whether the trainer AI automatically offers this spell for sale at the appropriate skill level
        #endregion

        #region Public Properties
        public int SpellID
        {
            get { return this.spellID; }
        }
        public bool IsBeneficial
        {
            get { return this.beneficial; }
        }
        public Character.ClassType[] ClassTypes
        {
            get { return this.classTypes; }
        }
        public Globals.eSpellType SpellType
        {
            get { return this.spellType; }
        }
        public Globals.eSpellTargetType TargetType
        {
            get { return this.targetType; }
        }
        public int ManaCost
        {
            get { return this.manaCost; }
        }
        public int RequiredLevel
        {
            get { return this.requiredLevel; }
        }
        public int TrainingPrice
        {
            get { return this.trainingPrice; }
        }
        public string Description
        {
            get { return this.description; }
        }
        public string SpellCommand
        {
            get { return this.spellCommand; }
        }
        public string Name
        {
            get { return this.name; }
        }
        public string SoundFile
        {
            get { return this.soundFile; }
        }
        public MethodInfo Method
        {
            get { return typeof(Spell).GetMethod(this.methodName, BindingFlags.Instance | BindingFlags.NonPublic); }
        }
        public bool IsClassSpell(Character.ClassType classType)
        {
            if (Array.IndexOf(this.ClassTypes, classType) > -1)
            {
                return true;
            }
            return false;
        }
        public bool IsAvailableAtTrainer
        {
            get { return this.availableAtTrainer; }
        }
        #endregion

        #region Constructor
        public Spell(System.Data.DataRow dr)
        {
            this.spellID = Convert.ToInt16(dr["spellID"]);
            this.spellCommand = dr["command"].ToString();
            this.name = dr["name"].ToString();
            this.description = dr["description"].ToString();
            this.methodName = dr["methodName"].ToString();
            this.beneficial = Convert.ToBoolean(dr["beneficial"]);
            string[] classTypes = dr["classTypes"].ToString().Split(" ".ToCharArray());
            this.classTypes = new Character.ClassType[classTypes.Length];
            for (int a = 0; a < classTypes.Length; a++)
            this.ClassTypes[a] = (Character.ClassType)Convert.ToInt32(classTypes[a]);
            this.spellType = (Globals.eSpellType)Convert.ToInt32(dr["spellType"]);
            this.targetType = (Globals.eSpellTargetType)Convert.ToInt32(dr["targetType"]);
            this.requiredLevel = Convert.ToByte(dr["requiredLevel"]);
            this.trainingPrice = Convert.ToInt32(dr["trainingPrice"]);
            this.manaCost = Convert.ToByte(dr["manaCost"]);
            this.soundFile = dr["soundFile"].ToString();
            this.methodName = dr["methodName"].ToString();
            this.availableAtTrainer = Convert.ToBoolean(dr["availableAtTrainer"]);
        }
        #endregion

        public Character FindAndConfirmTarget(Character caster, string args)
        {
            args = args.Replace(" at ", " "); // just get rid of the " at " in the args, makes things much easier

            Character target = null;

            if (args == null || args.Length == 0 || args == "" || args.ToLower() == this.spellCommand.ToLower())
            {
                args = caster.Name;
                target = caster;
            }
            else
            {
                // confirm the name of the spell isn't in the arguments
                args = args.ToLower().Replace(this.spellCommand.ToLower() + " ", "");
            }

            string[] sArgs = args.Split(" ".ToCharArray()); // this is for CAST # TARGET            

            // cast <spell command> <#> <target>
            // only <#> and <target> arrive at this method
            if (target == null && sArgs.Length >= 2)
            {
                try
                {
                    target = Map.FindTargetInView(caster, sArgs[1], Convert.ToInt32(sArgs[0]));
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
                        target = Map.FindTargetInView(caster, Convert.ToInt32(args), true, true);
                    }
                    else
                    {
                        target = Map.FindTargetInView(caster, Convert.ToInt32(args), true, false);
                    }

                }
                catch
                {
                    target = Map.FindTargetInView(caster, args, true, true);
                }
            }

            if (caster == null || target == null || caster.preppedSpell == null) return null;

            #region Check targetType.
            switch (caster.preppedSpell.targetType)
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
                default:
                    caster.WriteToDisplay("ERROR: Inform the developers that you could not find and confirm a target for your spell.");
                    target = null;
                    break;
            } 
            #endregion

            return target;
        }

        #region Static Methods
        public static bool LoadSpells()
        {
            try
            {
                ArrayList spellsList = DAL.DBWorld.LoadSpells();

                foreach (Spell spell in spellsList)
                {
                    Spell.spellDictionary.Add(spell.SpellID, spell);
                    Spell.spellCommandDictionary.Add(spell.spellCommand, spell);
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
            List<int> keys = new List<int>(spellList.Keys);
            return keys[Rules.dice.Next(0, keys.Count - 1)];
        }

        public static string GetLogString(Spell spell)
        {
            return "[SpellID: " + spell.SpellID + "] " + spell.Name + " (" + spell.spellType + ", " + spell.targetType + ")";
        }

        private static int GetSpellDamageModifier(Character caster)
        {
            if (caster.IsWisdomCaster)
            {
                return Rules.dice.Next(1, (int)(Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Wisdom) / 2));
            }
            else if (caster.IsIntelligenceCaster)
            {
                return Rules.dice.Next(1, (int)(Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Intelligence) / 2));
            }
            return 0;
        }

        public static Spell GetSpell(int spellID)
        {
            if (spellDictionary.ContainsKey(spellID))
            {
                return spellDictionary[spellID];
            }
            return null;
        }

        public static Spell GetSpell(string spellCommand)
        {
            if (spellCommandDictionary.ContainsKey(spellCommand.ToLower()))
            {
                return spellCommandDictionary[spellCommand.ToLower()];
            }
            return null;
        }

        public static void TeachSpell(Character caster, string spellCommand)
        {
            Spell spell = GetSpell(spellCommand);

            string words = GenerateMagicWords();

            caster.spellList.Add(spell.SpellID, words);

            if (caster.TargetName != "")
            {
                caster.WriteToDisplay(caster.TargetName + ": This incantation will cast the spell " + spell.Name + ". (" + spell.SpellCommand + ")");
                caster.WriteToDisplay(caster.TargetName + ": " + words);
            }
        }

        public static void ScribeSpell(Character caster, Spell spell)
        {
            string words = GenerateMagicWords();

            caster.spellList.Add(spell.SpellID, words);

            caster.WriteToDisplay("You have scribed the spell " + spell.Name + " (" + spell.SpellCommand + ") into your spellbook.");
            caster.WriteToDisplay("The incantation is: " + words);
        }

        public static string GenerateMagicWords()
        {
            string words = null;

            for (int a = 1; a <= MAGIC_WORDS_LENGTH; a++)
            {
                if (words == null)
                {
                    words = (magicWords[Rules.dice.Next(0, magicWords.Length - 1)]);
                }
                else
                {
                    words += " " + magicWords[Rules.dice.Next(0, magicWords.Length - 1)];
                }
            }
            return words;
        }

        public static void CastGenericAreaSpell(Cell center, string args, Effect.EffectType EffectType, int EffectPower, string spellName)
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
            if (EffectType == Effect.EffectType.Turn_Undead)
            {
                AreaSize = 3;
            }

            if (EffectType == Effect.EffectType.Nitro)
            {
                AreaSize = 1;
                EffectType = Effect.EffectType.Concussion;
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
                if (EffectType != Effect.EffectType.Turn_Undead)
                {
                    EffectPower = (int)(EffectPower / 2);
                }
            }
            else if (AreaSize == 1)
            {
                EffectPower = EffectPower * 2;
            }
            #endregion

            if (EffectType == Effect.EffectType.Concussion)
            {
                #region Concussion
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);
                Cell[] cellArray = Cell.GetApplicableCellArray(centerCell, AreaSize);

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
                    AreaEffect effect = new AreaEffect(Effect.EffectType.Illusion, Cell.GRAPHIC_RUINS_RIGHT, 1, -1, null, brokenWallsList);
                }
                #endregion
            }
            else if (EffectType == Effect.EffectType.Fire)
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
            else if (EffectType == Effect.EffectType.Ice)
            {
                #region Icestorm
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);
                //Cell[] cellArray = Cell.GetVisibleCellArray(centerCell, 3);
                foreach (Cell cell in areaCellList)
                {
                    foreach(Character chr in cell.Characters.Values)
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

            else if (EffectType == Effect.EffectType.Lightning)
            {
                #region Lightningstorm
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);
                //Cell[] cellArray = Cell.GetVisibleCellArray(centerCell, 3);
                foreach (Cell cell in areaCellList)
                {
                    foreach(Character chr in cell.Characters.Values)
                    {
                        if (!chr.IsDead)
                        {
                            Combat.DoSpellDamage(null, chr, null, EffectPower, "lightning");
                            chr.WriteToDisplay("You have been hit by a thunderous lightning storm!");
                        }
                    }
                }
                EffectGraphic = Cell.GRAPHIC_LIGHTNING_STORM;

                AreaEffect effect = new AreaEffect(EffectType, EffectGraphic, EffectPower, 3, null, areaCellList);
                #endregion

            }
            else if (EffectType == Effect.EffectType.Poison)
            {
                #region PoisonCloud
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);
                //Cell[] cellArray = Cell.GetVisibleCellArray(centerCell, 3);
                foreach (Cell cell in areaCellList)
                {
                    foreach(Character chr in cell.Characters.Values)
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
                    case Effect.EffectType.Light:
                        EffectGraphic = "";
                        spellMsg = "The area is illuminated by a burst of magical " + spellName.ToLower() + "!";
                        break;
                    case Effect.EffectType.Turn_Undead:
                        EffectGraphic = "";
                        spellMsg = "You feel a strong wind race through the area.";
                        spellDuration = 0;
                        break;
                    case Effect.EffectType.Darkness:
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

        public static void CastGenericAreaSpell(Cell center, string args, Effect.EffectType EffectType, int EffectPower, string spellName, Character caster)
        {
            int AreaSize = 2;
            int XCordMod = 0;
            int YCordMod = 0;

            string[] sArgs = args.Split(" ".ToCharArray());

            if (EffectPower == 0)
            {
                EffectPower = 15;
            }

            ArrayList areaCellList = new ArrayList();
            #region Set the size of the effect based on type
            // turn undead effects all undead on the current display map
            if (EffectType == Effect.EffectType.Turn_Undead)
            {
                AreaSize = 3;
            }

            if (EffectType == Effect.EffectType.Nitro)
            {
                AreaSize = 1;
                EffectType = Effect.EffectType.Concussion;
            }
            #endregion
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
                if (EffectType != Effect.EffectType.Turn_Undead)
                {
                    EffectPower = (int)(EffectPower / 2);
                }
            }
            else if (AreaSize == 1)
            {
                EffectPower = EffectPower * 2;
            }
            #endregion

            if (EffectType == Effect.EffectType.Concussion)
            {
                #region Concussion
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);
                Cell[] cellArray = Cell.GetApplicableCellArray(centerCell, AreaSize);

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
                    AreaEffect effect = new AreaEffect(Effect.EffectType.Illusion, Cell.GRAPHIC_RUINS_RIGHT, 1, -1, null, brokenWallsList);
                }
                #endregion

            }
            else if (EffectType == Effect.EffectType.Fire)
            {
                #region Fireball
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);

                centerCell.SendShout("a whoosh of fire!");

                AreaEffect effect = new AreaEffect(EffectType, "**", EffectPower, 2, caster, areaCellList);

                foreach (Cell cell in areaCellList)
                {
                    //Effect.DoAreaEffect(cell, effect);mlt [here]need looking further as 2 why is this commented out

                    foreach(Character chr in cell.Characters.Values)
                    {
                        if (!chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                        {
                            chr.WriteToDisplay("You have been hit by a fireball!");
                        }
                    }
                }
                #endregion
            }
            else if (EffectType == Effect.EffectType.Ice)
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

            else if (EffectType == Effect.EffectType.Lightning)
            {
                #region Lightningstorm
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);

                //if (this.SoundFile != "") { centerCell.EmitSound(this.SoundFile); }

                centerCell.SendShout("a loud thunderclap!");

                AreaEffect effect = new AreaEffect(EffectType, Cell.GRAPHIC_LIGHTNING_STORM, EffectPower, 3, caster, areaCellList);

                foreach (Cell cell in areaCellList)
                {
                    AreaEffect.DoAreaEffect(cell, effect);
                    foreach (Character chr in cell.Characters.Values)
                    {
                        if (!chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                        {
                            chr.WriteToDisplay("You have been hit by a thunderous lightning storm!");
                        }
                    }
                }
                #endregion
            }
            else if (EffectType == Effect.EffectType.Poison)
            {
                #region PoisonCloud
                Cell centerCell = Cell.GetCell(center.FacetID, center.LandID, center.MapID, center.X + XCordMod, center.Y + YCordMod, center.Z);
                centerCell.SendShout("a hissing sound.");
                AreaEffect effect = new AreaEffect(EffectType, Cell.GRAPHIC_POISON_CLOUD, ((int)Skills.GetSkillLevel(caster.magic) * 2), 5, caster, areaCellList);

                foreach (Cell cell in areaCellList)
                {
                    AreaEffect.DoAreaEffect(cell, effect);
                    foreach (Character chr in cell.Characters.Values)
                    {
                        if (!chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                        {
                            chr.WriteToDisplay("You have been hit by a poison cloud!");
                        }
                    }
                }
                #endregion
            }

        }
        
        #endregion

        public void WarmSpell(Character caster)
        {
            if (caster.IsUndead || caster.race == "")
            {
                if (caster.attackSound != "")
                {
                    caster.EmitSound(caster.attackSound);
                }
                else
                {
                    if (caster.gender == Globals.eGender.Female)
                    {
                        caster.EmitSound(Sound.GetCommonSound(Sound.CommonSound.FemaleSpellWarm));
                    }
                    else
                    {
                        caster.EmitSound(Sound.GetCommonSound(Sound.CommonSound.MaleSpellWarm));
                    }
                }
            }
            else
            {
                if (caster.gender == Globals.eGender.Female)
                {
                    caster.EmitSound(Sound.GetCommonSound(Sound.CommonSound.FemaleSpellWarm));
                }
                else
                {
                    caster.EmitSound(Sound.GetCommonSound(Sound.CommonSound.MaleSpellWarm));
                }
            }

            caster.WriteToDisplay("You warm the spell " + this.Name + ".");
        }

        public bool CastSpell(Character caster, string args)
        {
            try
            {
                args = args.Replace(" at ", " "); // remove " at " from all cast command arguments
                args = args.Trim();

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

                bool spellSuccess = Convert.ToBoolean(this.Method.Invoke(this, new Object[] { caster, args }));

                return spellSuccess;
            }
            catch (Exception e)
            {
                Utils.Log("Failure at spell.CastSpell(" + caster.GetLogString() + ", " + args + ")", Utils.LogType.SystemFailure);
                Utils.LogException(e);
                return false;
            }
        }

        private void CastGenericAreaSpell(Character caster, string args, Effect.EffectType EffectType, int EffectPower, string spellName)
        {
            int AreaSize = 2;
            int XCordMod = 0;
            int YCordMod = 0;
            int argCount = 0;

            string[] sArgs = args.Split(" ".ToCharArray());

            if (EffectPower == 0)
            {
                EffectPower = 15;
            }

            // check that they give us a starting cord
            if (sArgs[0] == null)
            {
                // do nothing
            }
            else
            {
                Character target = FindAndConfirmTarget(caster, args);
                if (target != null)
                {
                    if (caster.Y > target.Y)
                    {
                        YCordMod -= (caster.Y - target.Y);
                    }
                    else
                    {
                        YCordMod += target.Y - caster.Y;
                    }
                    if (caster.X > target.X)
                    {
                        XCordMod -= (caster.X - target.X);
                    }
                    else
                    {
                        XCordMod += (target.X - caster.X);
                    }
                }

                //loop through the arguments to find the starting point
                while (argCount < sArgs.Length)
                {
                    switch (sArgs[argCount])
                    {
                        case "north":
                        case "n":
                            YCordMod -= 1;
                            break;
                        case "south":
                        case "s":
                            YCordMod += 1;
                            break;
                        case "west":
                        case "w":
                            XCordMod -= 1;
                            break;
                        case "east":
                        case "e":
                            XCordMod += 1;
                            break;
                        case "northeast":
                        case "ne":
                            YCordMod -= 1;
                            XCordMod += 1;
                            break;
                        case "northwest":
                        case "nw":
                            YCordMod -= 1;
                            XCordMod -= 1;
                            break;
                        case "southeast":
                        case "se":
                            YCordMod += 1;
                            XCordMod += 1;
                            break;
                        case "southwest":
                        case "sw":
                            YCordMod += 1;
                            XCordMod -= 1;
                            break;
                        case "1":
                            AreaSize = 1;
                            break;
                        case "3":
                        case "4":
                        case "5":
                        case "6":
                            AreaSize = 3;
                            break;
                        default:
                            break;
                    }
                    argCount++;

                }

                ArrayList areaCellList = new ArrayList();
                int[] XCastMod = new int[] { 0, 0, 0 };
                int[] YCastMod = new int[] { 0, 0, 0 };

                // turn undead effects all undead in view
                if (EffectType == Effect.EffectType.Turn_Undead)
                {
                    AreaSize = 3;
                }

                //OK now we have the starting point, and the direction, create all the effects needed
                #region Create the list of effect cells
                if (AreaSize == 1)
                {
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z));
                }
                else if (AreaSize > 2)
                {
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod - 2, caster.Y + YCordMod + -2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod - 2, caster.Y + YCordMod + -2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod - 1, caster.Y + YCordMod + -2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod - 1, caster.Y + YCordMod + -2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod + -2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod + -2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 1, caster.Y + YCordMod + -2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 1, caster.Y + YCordMod + -2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 2, caster.Y + YCordMod + -2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 2, caster.Y + YCordMod + -2, caster.Z));

                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod - 2, caster.Y + YCordMod + -1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod - 2, caster.Y + YCordMod + -1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod - 1, caster.Y + YCordMod + -1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod - 1, caster.Y + YCordMod + -1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod + -1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod + -1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 1, caster.Y + YCordMod + -1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 1, caster.Y + YCordMod + -1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 2, caster.Y + YCordMod + -1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 2, caster.Y + YCordMod + -1, caster.Z));

                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + -2, caster.Y + YCordMod, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + -2, caster.Y + YCordMod, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + -1, caster.Y + YCordMod, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + -1, caster.Y + YCordMod, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 1, caster.Y + YCordMod, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 1, caster.Y + YCordMod, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 2, caster.Y + YCordMod, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 2, caster.Y + YCordMod, caster.Z));

                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + -2, caster.Y + YCordMod + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + -2, caster.Y + YCordMod + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + -1, caster.Y + YCordMod + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + -1, caster.Y + YCordMod + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 1, caster.Y + YCordMod + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 1, caster.Y + YCordMod + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 2, caster.Y + YCordMod + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 2, caster.Y + YCordMod + 1, caster.Z));

                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + -2, caster.Y + YCordMod + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + -2, caster.Y + YCordMod + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + -1, caster.Y + YCordMod + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + -1, caster.Y + YCordMod + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 1, caster.Y + YCordMod + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 1, caster.Y + YCordMod + 2, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 2, caster.Y + YCordMod + 2, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 2, caster.Y + YCordMod + 2, caster.Z));


                }
                else
                {

                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod - 1, caster.Y + YCordMod + -1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod - 1, caster.Y + YCordMod + -1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod + -1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod + -1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 1, caster.Y + YCordMod + -1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 1, caster.Y + YCordMod + -1, caster.Z));

                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + -1, caster.Y + YCordMod, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + -1, caster.Y + YCordMod, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 1, caster.Y + YCordMod, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 1, caster.Y + YCordMod, caster.Z));

                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + -1, caster.Y + YCordMod + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + -1, caster.Y + YCordMod + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod + 1, caster.Z));
                    if (!Map.IsSpellPathBlocked(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 1, caster.Y + YCordMod + 1, caster.Z)))
                        areaCellList.Add(Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod + 1, caster.Y + YCordMod + 1, caster.Z));
                }
                #endregion
                //adjust the power of the area spell according to area size - no adjustment if normal
                if (AreaSize >= 3)
                {
                    if (EffectType != Effect.EffectType.Turn_Undead)
                    {
                        EffectPower = (int)(EffectPower / 2);
                    }
                }
                else if (AreaSize == 1)
                {
                    EffectPower = EffectPower * 2;
                }

                if (EffectType == Effect.EffectType.Concussion)
                {
                    #region Concussion
                    Cell centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);
                    Cell[] cellArray = Cell.GetApplicableCellArray(centerCell, AreaSize);

                    if (this.SoundFile != "") { centerCell.EmitSound(this.SoundFile); }

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
                        AreaEffect effect = new AreaEffect(Effect.EffectType.Illusion, Cell.GRAPHIC_RUINS_RIGHT, 1, -1, null, brokenWallsList);
                    }
                    #endregion
                }
                else if (EffectType == Effect.EffectType.Fire)
                {
                    #region Fireball
                    Cell centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);

                    if (this.SoundFile != "") { centerCell.EmitSound(this.SoundFile); }

                    centerCell.SendShout("a whoosh of fire!");

                    AreaEffect effect = new AreaEffect(EffectType, "**", EffectPower, 1, caster, areaCellList);

                    foreach (Cell cell in areaCellList)
                    {
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
                else if (EffectType == Effect.EffectType.Ice)
                {
                    #region Icestorm
                    Cell centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);

                    if (this.SoundFile != "") { centerCell.EmitSound(this.SoundFile); }

                    centerCell.SendShout("the pounding of giant ice pellets!");

                    AreaEffect effect = new AreaEffect(EffectType, Cell.GRAPHIC_ICE_STORM, EffectPower, 1, caster, areaCellList);

                    foreach (Cell cell in areaCellList)
                    {
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
                else if (EffectType == Effect.EffectType.Lightning)
                {
                    #region Lightning Storm
                    Cell centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);

                    if (this.SoundFile != "") { centerCell.EmitSound(this.SoundFile); }

                    centerCell.SendShout("a loud thunderclap!");

                    AreaEffect effect = new AreaEffect(EffectType, Cell.GRAPHIC_LIGHTNING_STORM, EffectPower, 2, caster, areaCellList);

                    foreach (Cell cell in areaCellList)
                    {
                        //Effect.DoAreaEffect(cell, effect);
                        foreach (Character chr in cell.Characters.Values)
                        {
                            if (!chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                            {
                                chr.WriteToDisplay("You have been hit by a thunderous lightning storm!");
                            }
                        }
                    }
                    #endregion
                } 
                else if (EffectType == Effect.EffectType.Poison)
                {
                    #region Poison Cloud
                    Cell centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);
                    AreaEffect effect = new AreaEffect(EffectType, Cell.GRAPHIC_POISON_CLOUD, ((int)Skills.GetSkillLevel(caster.magic) * 2), 5, caster, areaCellList);
                    centerCell.SendShout("a hissing sound.");
                    if (this.SoundFile != "") { centerCell.EmitSound(this.SoundFile); }
                    foreach (Cell cell in areaCellList)
                    {
                        foreach (Character chr in cell.Characters.Values)
                        {
                            if ( !chr.IsDead)
                            {
                                chr.WriteToDisplay("You have been hit by a poison cloud!");
                            }
                        }
                    }
                    #endregion
                }
                else if (EffectType == Effect.EffectType.Turn_Undead)
                {
                    #region Turn Undead
                    Cell centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);

                    if (this.SoundFile != "") { centerCell.EmitSound(this.SoundFile); }

                    AreaEffect effect = new AreaEffect(EffectType, "", EffectPower, 1, caster, areaCellList);

                    foreach (Cell cell in areaCellList)
                    {
                        foreach (Character chr in cell.Characters.Values)
                        {
                            if (!chr.IsDead)
                            {
                                chr.WriteToDisplay("You feel a strong wind race through the area.");
                            }
                        }
                    }
                    #endregion
                }
                else if (EffectType == Effect.EffectType.Light)
                {
                    #region Light
                    Cell centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);

                    if (this.SoundFile != "") { centerCell.EmitSound(this.SoundFile); }

                    AreaEffect effect = new AreaEffect(EffectType, "", EffectPower, Skills.GetSkillLevel(caster.magic), caster, areaCellList);

                    foreach (Cell cell in areaCellList)
                    {
                        if (!cell.IsAlwaysDark)
                        {
                            // stop darkness effect
                            if (cell.AreaEffects.ContainsKey(Effect.EffectType.Darkness))
                            {
                                cell.AreaEffects[Effect.EffectType.Darkness].StopAreaEffect();
                            }

                            // send message to characters
                            foreach (Character chr in cell.Characters.Values)
                            {
                                if (!chr.IsDead)
                                {
                                    chr.WriteToDisplay("The area is illuminated by a burst of magical light!");
                                }
                            }
                        }
                    }
                    #endregion
                }
                else if (EffectType == Effect.EffectType.Darkness)
                {
                    #region Darkness
                    Cell centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);

                    if (this.SoundFile != "") { centerCell.EmitSound(this.SoundFile); }

                    AreaEffect effect = new AreaEffect(EffectType, "??", 0, EffectPower, caster, areaCellList);

                    foreach (Cell cell in areaCellList)
                    {
                        foreach (Character chr in cell.Characters.Values)
                        {
                            if (!chr.IsDead)
                            {
                                chr.WriteToDisplay("The area is covered in a shroud of magical darkness!");
                            }
                        }
                    }
                    #endregion
                }
                else if (EffectType == Effect.EffectType.Acid)
                {
                    Cell centerCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + XCordMod, caster.Y + YCordMod, caster.Z);

                    if (this.SoundFile != "") { centerCell.EmitSound(this.SoundFile); }

                    centerCell.SendShout("a torrent of destructive acid rain!");

                    AreaEffect effect = new AreaEffect(EffectType, Cell.GRAPHIC_ACID_STORM, EffectPower, 1, caster, areaCellList);

                    foreach (Cell cell in areaCellList)
                    {
                        foreach (Character chr in cell.Characters.Values)
                        {
                            if (!chr.IsInvisible && !chr.IsImmortal && !chr.IsDead)
                            {
                                chr.WriteToDisplay("You have been hit by acid rain!");
                            }
                        }
                    }
                }
            }
        }

        private void CastGenericConeSpell(Character caster, string args)
        {
            string[] directionStrings = new[] { "n", "s", "e", "w", "nw", "ne", "sw", "se" };

            if (args == null) args = "";

            args = args.Replace(this.spellCommand + " ", "");
            args = args.Replace(this.spellCommand, "");

            Effect.EffectType effectType = Effect.EffectType.None;
            string effectGraphic = "  ";
            int effectPower = 1;
            string spellMsg = "";
            string spellEmote = "";
            string direction = "";
            string spellSound = "";

            string[] sArgs = args.Split(" ".ToCharArray());

            ArrayList areaCellList = new ArrayList(); // the cells that will be effected by the cone            

            if (this.spellCommand == "drbreath")
            {
                if ((sArgs.Length >= 1 && sArgs[0] == "ice") || caster.species == Globals.eSpecies.IceDragon)
                {
                    effectType = Effect.EffectType.Dragon__s_Breath_Ice;
                    effectGraphic = Cell.GRAPHIC_ICE_STORM;
                    effectPower = Skills.GetSkillLevel(caster.magic) * 6;
                    spellMsg = "You are hit by a frigid blast of " + this.Name.ToLower() + "!";
                    spellEmote = " breathes a cone of ice!";
                    spellSound = Sound.GetCommonSound(Sound.CommonSound.Ice);
                }
                else if ((sArgs.Length >= 1 && sArgs[0] == "wind") || caster.species == Globals.eSpecies.WindDragon)
                {
                    effectType = Effect.EffectType.Dragon__s_Breath_Wind;
                    effectGraphic = Cell.GRAPHIC_WHIRLWIND;
                    effectPower = Skills.GetSkillLevel(caster.magic) * 6;
                    spellMsg = "You are hit by blast of hot wind!";
                    spellEmote = " breathes a cone of stinging wind!";
                    spellSound = Sound.GetCommonSound(Sound.CommonSound.Whirlwind);
                }
                else if (caster.species == Globals.eSpecies.CloudDragon && args.ToLower().Contains("stormbreath")) // TODO: determine if non dragon casters of dragon's breath can cast storm breath
                {
                    effectType = Effect.EffectType.Dragon__s_Breath_Storm;
                    effectGraphic = Cell.GRAPHIC_LIGHTNING_STORM;
                    effectPower = Skills.GetSkillLevel(caster.magic) * 8;
                    spellMsg = "You are pelted by massive hailstones and stung by a frigid blasts of wind and blinding lightning!";
                    spellEmote = " breathes a cone of raging wind, lightning and hailstones!";
                    spellSound = Sound.GetCommonSound(Sound.CommonSound.Whirlwind);
                }
                else
                {
                    effectType = Effect.EffectType.Dragon__s_Breath_Fire;
                    effectGraphic = Cell.GRAPHIC_FIRE;
                    effectPower = Skills.GetSkillLevel(caster.magic) * 6;
                    spellMsg = "You are hit by searing " + this.Name.ToLower() + "! Your skin blisters and burns!";
                    spellEmote = " breathes fire!";
                    spellSound = Sound.GetCommonSound(Sound.CommonSound.Fire);
                }
            }

            Character target = FindAndConfirmTarget(caster, sArgs[sArgs.Length - 1]);

            //if cone is cast at something figure out the direction
            if (target != null && target != caster) // NPCs will not put in a direction, and if PC caster used a name then we need a direction
            {
                if (target.CurrentCell != caster.CurrentCell)
                    direction = Utils.FormatEnumString(Map.GetDirection(caster.CurrentCell, target.CurrentCell).ToString()).ToLower();
                //if (target.X < caster.X && target.Y < caster.Y) { direction = "nw"; }
                //if (target.X < caster.X && target.Y > caster.Y) { direction = "sw"; }
                //if (target.X < caster.X && target.Y == caster.Y) { direction = "w"; }
                //if (target.X > caster.X && target.Y == caster.Y) { direction = "e"; }
                //if (target.X > caster.X && target.Y < caster.Y) { direction = "ne"; }
                //if (target.X > caster.X && target.Y > caster.Y) { direction = "se"; }
                //if (target.X == caster.X && target.Y < caster.Y) { direction = "n"; }
                //if (target.X == caster.X && target.Y > caster.Y) { direction = "s"; }

                else if (target.X == caster.X && target.Y == caster.Y)
                {
                    switch (caster.dirPointer)
                    {
                        case "^": direction = "n"; break;
                        case "v": direction = "s"; break;
                        case ">": direction = "e"; break;
                        case "<": direction = "w"; break;
                        default:
                            switch (Rules.dice.Next(3) + 1)
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
                        switch (Rules.dice.Next(3) + 1)
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

            caster.SendToAllInSight((caster.race != "" ? caster.Name : "The " + caster.Name) + spellEmote);
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

            effectPower = effectPower - Rules.dice.Next(-5, 5);
            if (effectPower < 1) { effectPower = 5; }
            AreaEffect effect = new AreaEffect(effectType, effectGraphic, effectPower, 1, caster, areaCellList);
            return;
        }

        private void CastGenericWallSpell(Character caster, string args, Effect.EffectType EffectType, int EffectPower)
        {
            args = args.Replace(this.SpellCommand, "");
            args = args.Trim();

            string EffectGraphic = "  ";
            string spellMsg = "";
            string spellShout = "";
            bool spellStun = false;

            ArrayList areaCellList = new ArrayList(); // the cells that will be effected by the wall

            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = FindAndConfirmTarget(caster, sArgs[sArgs.Length - 1]);

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
                case Effect.EffectType.Fire:
                    EffectGraphic = "**";
                    spellMsg = "You are burned by a wall of fire!";
                    spellShout = "a whoosh of fire.";
                    break;
                case Effect.EffectType.Ice:
                    EffectGraphic = "~,";
                    spellMsg = "You are consumed in a wall of ice!";
                    spellShout = "a loud hiss and the sound of ice cracking.";
                    spellStun = true;
                    break;
                case Effect.EffectType.Light:
                    EffectGraphic = "";
                    spellMsg = "The area is illuminated by a wall of magical light!";
                    break;
                case Effect.EffectType.Dragon__s_Breath_Fire:
                    EffectGraphic = "**";
                    spellMsg = "You are surrounded by a wall of searing dragon's breath!";
                    spellShout = "a whoosh of fire.";
                    break;
                case Effect.EffectType.Darkness:
                    EffectGraphic = "";
                    spellMsg = "The area is covered in a wall of magical darkness!";
                    break;
                case Effect.EffectType.Lightning:
                    EffectGraphic = Cell.GRAPHIC_LIGHTNING_STORM;
                    spellMsg = "You're are struck by a bolt of lightning!";
                    break;
            }
            // generic shout message
            if (spellShout != "")
            {
                cell.SendShout(spellShout);
            }
            cell.EmitSound(this.soundFile);
            //message to those in the area of effect, and stun for ice wall
            if (spellMsg != "")
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
            AreaEffect effect = new AreaEffect(EffectType, EffectGraphic, EffectPower, EffectType == Effect.EffectType.Lightning ? 1 : Rules.RollD(10, 10), caster, areaCellList);
            return;

        }

        #region Wizard Spells
        private bool castMagicMissile(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            this.SendGenericCastMessage(caster, target, true);

            if (Combat.DoSpellDamage(caster, target, null, (Skills.GetSkillLevel(caster.magic) * 3) + GetSpellDamageModifier(caster), "magic missile") == 1)
            {
                Rules.GiveKillExp(caster, target);
                Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
            }
            return true;
        }

        private bool castShield(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, false);

            int shieldEffect = 0;
            int shieldDuration = 0;
            int skillLevel = Skills.GetSkillLevel(caster.magic);
            if (skillLevel < 5) { shieldEffect = 1; shieldDuration = 30; }
            else if (skillLevel >= 5 && skillLevel <= 10) { shieldEffect = 3; shieldDuration = 60; }
            else if (skillLevel > 10 && skillLevel <= 15) { shieldEffect = 6; shieldDuration = 90; }
            else { shieldEffect = 9; shieldDuration = 120; }
            Effect.CreateCharacterEffect(Effect.EffectType.Shield, shieldEffect, target, Skills.GetSkillLevel(caster.magic) * shieldDuration, caster);
            target.EmitSound(this.SoundFile);
            target.WriteToDisplay("You are surrounded by the blue glow of a " + this.Name + " spell.");
            return true;
        }

        private bool castBonfire(Character caster, string args)
        {
            // clean up the args
            args = args.Replace("bonfire", "");
            args = args.Trim();

            string[] sArgs = args.Split(" ".ToCharArray());

            if (Map.IsSpellPathBlocked(Map.GetCellRelevantToCell(caster.CurrentCell, args, true)))
            {
                //Spell.GenericFailMessage(caster, null);
                return false;
            }

            if (Map.GetCellRelevantToCell(caster.CurrentCell, args, true) != null)
            {
                AreaEffect effect = new AreaEffect(Effect.EffectType.Fire, Cell.GRAPHIC_FIRE, (int)(Skills.GetSkillLevel(caster.magic) * 2.5), (int)(Skills.GetSkillLevel(caster.magic) * 1.5), caster, Map.GetCellRelevantToCell(caster.CurrentCell, args, false));
                Map.GetCellRelevantToCell(caster.CurrentCell, args, true).SendShout("a roaring fire.");
            }

            caster.WriteToDisplay("You create a magical " + this.Name + ".");

            return true;
        }

        private bool castCreateWeb(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null)
            {
                Cell targetCell = Map.GetCellRelevantToCell(caster.CurrentCell, args, false);
                AreaEffect effect = new AreaEffect(Effect.EffectType.Web, Cell.GRAPHIC_WEB, Skills.GetSkillLevel(caster.magic), Skills.GetSkillLevel(caster.magic), caster, targetCell);
                if (targetCell != null)
                    targetCell.EmitSound(this.SoundFile);
            }
            else
            {
                AreaEffect effect = new AreaEffect(Effect.EffectType.Web, Cell.GRAPHIC_WEB, Skills.GetSkillLevel(caster.magic), Skills.GetSkillLevel(caster.magic), caster, target.CurrentCell);
                if(target.CurrentCell != null)
                    target.CurrentCell.EmitSound(this.SoundFile);
            }

            caster.WriteToDisplay("You cast " + this.Name + ".");
            return true;
        }

        private bool castFireBall(Character caster, string args)
        {
            SendGenericCastMessage(caster, null, true);
            CastGenericAreaSpell(caster, args, Effect.EffectType.Fire, Skills.GetSkillLevel(caster.magic) * 4, this.Name);
            return true;
        }

        private bool castFireWall(Character caster, string args)
        {
            caster.WriteToDisplay("You cast " + this.Name + ".");
            CastGenericWallSpell(caster, args, Effect.EffectType.Fire, (int)(Skills.GetSkillLevel(caster.magic) * 4));
            return true;
        }

        private bool castIceStorm(Character caster, string args)
        {
            caster.WriteToDisplay("You cast " + this.Name + ".");
            CastGenericAreaSpell(caster, args, Effect.EffectType.Ice, Skills.GetSkillLevel(caster.magic) * 5, this.Name);
            return true;
        }

        private bool castConcussion(Character caster, string args)
        {
            caster.WriteToDisplay("You cast " + this.Name + ".");
            CastGenericAreaSpell(caster, args, Effect.EffectType.Concussion, (int)(Skills.GetSkillLevel(caster.magic) * 6), this.Name);
            return true;
        }

        private bool castDispel(Character caster, string args)
        {
            caster.WriteToDisplay("You cast " + this.Name + ".");
            int xpos = 0;
            int ypos = 0;

            //clean out the command name
            args = args.Replace(this.SpellCommand, "");
            args = args.Trim();
            string[] sArgs = args.Split(" ".ToCharArray());
            #region Get the direction
            switch (sArgs[0])
            {
                case "south":
                case "s":
                    ypos++;
                    break;
                case "southeast":
                case "se":
                    ypos++;
                    xpos++;
                    break;
                case "southwest":
                case "sw":
                    ypos++;
                    xpos--;
                    break;
                case "west":
                case "w":
                    xpos--;
                    break;
                case "east":
                case "e":
                    xpos++;
                    break;
                case "northeast":
                case "ne":
                    ypos--;
                    xpos++;
                    break;
                case "northwest":
                case "nw":
                    ypos--;
                    xpos--;
                    break;
                case "north":
                case "n":
                    ypos--;
                    break;
                default:
                    break;
            }
            #endregion
            Cell cell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + xpos, caster.Y + ypos, caster.Z);

            foreach (AreaEffect effect in cell.AreaEffects.Values)
            {
                effect.StopAreaEffect();
            }
            return true;
        }

        private bool castIllusion(Character caster, string args)
        {
            //clean out the command name
            args = args.Replace(this.SpellCommand, "");
            args = args.Trim();

            string[] sArgs = args.Split(" ".ToCharArray());

            Cell cell = Map.GetCellRelevantToCell(caster.CurrentCell, args, false);

            if (cell == null)
            {
                caster.WriteToDisplay("Illusion spell format: CAST <illusion type> <direction>");
                return false;
            }

            if (cell.IsMagicDead)
            {
                //caster.WriteToDisplay("Your spell fails.");
                //caster.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                return false;
            }

            AreaEffect effect;

            switch (sArgs[0].ToLower())
            {
                case "wall":
                    SendGenericCastMessage(caster, null, true);
                    effect = new AreaEffect(Effect.EffectType.Illusion, Cell.GRAPHIC_WALL, 1, 200, caster, cell);
                    break;
                case "fire":
                    SendGenericCastMessage(caster, null, true);
                    effect = new AreaEffect(Effect.EffectType.Illusion, Cell.GRAPHIC_FIRE, 1, 200, caster, cell);
                    break;
                case "bridge":
                    SendGenericCastMessage(caster, null, true);
                    effect = new AreaEffect(Effect.EffectType.Illusion, Cell.GRAPHIC_BRIDGE, 1, 200, caster, cell);
                    break;
                case "empty":
                    SendGenericCastMessage(caster, null, true);
                    effect = new AreaEffect(Effect.EffectType.Illusion, Cell.GRAPHIC_EMPTY, 1, 200, caster, cell);
                    break;
                case "water":
                    SendGenericCastMessage(caster, null, true);
                    effect = new AreaEffect(Effect.EffectType.Illusion, Cell.GRAPHIC_WATER, 1, 200, caster, cell);
                    break;
                case "ice":
                    SendGenericCastMessage(caster, null, true);
                    effect = new AreaEffect(Effect.EffectType.Illusion, Cell.GRAPHIC_ICE, 1, 200, caster, cell);
                    break;
                default:
                    caster.WriteToDisplay(sArgs[0] + " is not a valid illusion type.");
                    caster.WriteToDisplay("Valid Illusions: wall | fire | bridge | empty | water | ice");
                    break;
            }
            return true;
        }

        private bool castDisintegrate(Character caster, string args)
        {
            // clean up the args
            args = args.Replace(this.spellCommand, "");
            args = args.Trim();

            string[] sArgs = args.Split(" ".ToCharArray());

            Cell tCell = Map.GetCellRelevantToCell(caster.CurrentCell, args, true);

            if (tCell != null)
            {
                // destroy all but attuned items
                foreach (Item item in new List<Item>(tCell.Items))
                {
                    if (item.attunedID != 0)
                    {
                        tCell.Remove(item);
                    }
                }

                // do spell damage
                foreach (Character chr in tCell.Characters.Values)
                {
                    if (Combat.DoSpellDamage(caster, chr, null, Skills.GetSkillLevel(caster.magic) * 4, "disintegrate") == 1)
                        Rules.GiveAEKillExp(caster, chr);
                }

                // destroy walls for a while
                if (!tCell.IsMagicDead && tCell.DisplayGraphic == Cell.GRAPHIC_WALL)
                {
                    string newDispGraphic = Cell.GRAPHIC_RUINS_RIGHT;
                    if (Rules.RollD(1, 20) >= 10)
                        newDispGraphic = Cell.GRAPHIC_RUINS_LEFT;

                    AreaEffect effect = new AreaEffect(Effect.EffectType.Illusion, newDispGraphic, 0, (int)Skills.GetSkillLevel(caster.magic) * 6, caster, tCell);
                    tCell.SendShout("a wall crumbling.");
                }
            }

            SendGenericCastMessage(caster, null, true);

            return true;
        }

        private bool castPeek(Character caster, string args)
        {
            try
            {
                // make sure they have the correct component, otherwise BOOM.

                if (caster is PC && (caster as PC).ImpLevel < Globals.eImpLevel.GM)
                {
                    Item item = caster.RightHand;

                    if (item == null)
                    {
                        item = caster.LeftHand;
                        if (item == null || item.itemID != Item.ID_TIGERSEYE)
                        {
                            caster.WriteToDisplay("You do not have the required material component for the " + this.Name + " spell.");
                            return false;
                        }
                    }
                }

                string[] sArgs = args.Split(" ".ToCharArray());

                //find and confirm the target
                string tName = sArgs[sArgs.Length - 1];

                Character target = null;
                // find the FIRST match.
                foreach (Character ch in Character.PCList)
                {
                    if (ch.MapID == caster.MapID && ch.Name.ToLower() == tName.ToLower())
                    {
                        target = ch;
                        break;
                    }
                }

                if (target == null)
                {
                    foreach (Character ch in Character.NPCInGameWorldList)
                    {
                        if (ch.MapID == caster.MapID && ch.Name.ToLower() == tName.ToLower())
                        {
                            target = ch;
                            break;
                        }
                    }
                }

                if (target == null || target is PC && Array.IndexOf((target as PC).ignoreList, caster.PlayerID) > -1 || (caster is PC && target is PC &&
                    Array.IndexOf((caster as PC).ignoreList, target.PlayerID) > -1))
                {
                    caster.WriteToDisplay("You cannot sense your target.");
                    return true;
                }
                else
                {
                    // alert the target that they are being peeked
                    if (!caster.IsInvisible)
                    {
                        target.WriteToDisplay("Your eyes tingle for a moment.");
                    }

                    string targetDesc = target is PC || (target is NPC) && (target as NPC).longDesc == "" ? target.Name : (target as NPC).longDesc;

                    caster.WriteToDisplay("You see through the eyes of " + targetDesc + ".");

                    Effect.CreateCharacterEffect(Effect.EffectType.Peek, 1, target, 1, caster);
                }

                

            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
            return true;
        }

        private bool castIceSpear(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);

            // a result of 1 means the target is killed
            if (Combat.DoSpellDamage(caster, target, null, Skills.GetSkillLevel(caster.magic) * 14 + GetSpellDamageModifier(caster), "icespear") == 1)
            {
                Rules.GiveKillExp(caster, target);
                Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
            }
            else
            {
                if (Combat.DoSpellDamage(caster, target, null, 0, "stun") == 1)
                {
                    target.WriteToDisplay("You are stunned!");

                    if (target.preppedSpell != null)
                    {
                        target.preppedSpell = null;
                        target.WriteToDisplay("Your spell has been lost.");
                    }

                    target.Stunned = (short)(Rules.dice.Next(1, (int)(Skills.GetSkillLevel(caster.magic) / 3)) + 1);
                    target.SendToAllInSight((target.race != "" ? target.Name : "The " + target.Name) + " is stunned by an " + this.Name + "!");
                }
                else
                {
                    caster.WriteToDisplay((target.race != "" ? target.Name : "The " + target.Name) + " resists being stunned by your " + this.Name + "!");
                    target.WriteToDisplay("You resist being stunned by the " + this.Name + "!");
                }
            }
            return true;
        }

        private bool castFireBolt(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);        

            if (Combat.DoSpellDamage(caster, target, null, Skills.GetSkillLevel(caster.magic) * 12 + GetSpellDamageModifier(caster), this.name) == 1)
            {
                Rules.GiveKillExp(caster, target);
                Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
            }
            return true;
        }

        private bool castWhirlWind(Character caster, string args)
        {
            SendGenericCastMessage(caster, null, false);

            if (args == null) args = "";

            Cell targetCell = Map.GetCellRelevantToCell(caster.CurrentCell, args, true);

            if(targetCell == null) return false;

            AreaEffect effect = new AreaEffect(Effect.EffectType.Whirlwind, Cell.GRAPHIC_WHIRLWIND, caster.Level, 5, caster, Map.GetCellRelevantToCell(caster.CurrentCell, args, true));
            
            return true;
        }

        private bool castFireStorm(Character caster, string args)
        {
            SendGenericCastMessage(caster, null, true);

            if (args == null) args = "";

            CastGenericAreaSpell(caster, args, Effect.EffectType.Fire, Skills.GetSkillLevel(caster.magic) * 4, this.Name);

            AreaEffect effect = new AreaEffect(Effect.EffectType.Fire_Storm, Cell.GRAPHIC_FIRE, caster.Level, 5, caster, Map.GetCellRelevantToCell(caster.CurrentCell, args, true));
            
            return true;
        }

        private bool castDragonsBreath(Character caster, string args)
        {
            if (args == null) args = "";

            SendGenericCastMessage(caster, null, false);

            CastGenericConeSpell(caster, args);
            
            return true;
        }

        private bool castBlizzard(Character caster, string args)
        {
            caster.WriteToDisplay("You cast " + this.Name + ".");
            if (args == null)
                args = "";
            CastGenericAreaSpell(caster, args, Effect.EffectType.Ice, Skills.GetSkillLevel(caster.magic) * 6, this.Name);
            AreaEffect effect = new AreaEffect(Effect.EffectType.Blizzard, Cell.GRAPHIC_ICE_STORM, caster.Level, 5, caster, Map.GetCellRelevantToCell(caster.CurrentCell, args, true));
            return true;
        }

        private bool castLightningLance(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);
            
            if (Combat.DoSpellDamage(caster, target, null, Skills.GetSkillLevel(caster.magic) * 12, "lightning") == 1)
            {
                Rules.GiveKillExp(caster, target);
                Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
            }
            return true;
        }

        private bool castLightningStorm(Character caster, string args)
        {
            SendGenericCastMessage(caster, null, true);
            if (args == null)
              args = "";
            CastGenericAreaSpell(caster, args, Effect.EffectType.Lightning, Skills.GetSkillLevel(caster.magic) * 5, this.Name);

            AreaEffect effect = new AreaEffect(Effect.EffectType.Lightning_Storm, Cell.GRAPHIC_LIGHTNING_STORM, caster.Level, 5, caster, Map.GetCellRelevantToCell(caster.CurrentCell, args, true));
            return true;
        }

        private bool castLava(Character caster, string args)
        {
            caster.WriteToDisplay("You cast " + this.Name + ".");
            // clean up the args
            args = args.Replace("lava", "");
            args = args.Trim();

            string[] sArgs = args.Split(" ".ToCharArray());

            if (Map.IsSpellPathBlocked(Map.GetCellRelevantToCell(caster.CurrentCell, args, true)))
            {
                //Spell.GenericFailMessage(caster, null);
                return false;
            }

            if (Map.GetCellRelevantToCell(caster.CurrentCell, args, true) != null)
            {
                AreaEffect effect = new AreaEffect(Effect.EffectType.Fire, Cell.GRAPHIC_FIRE, (int)(Skills.GetSkillLevel(caster.magic) * 20), (int)(Skills.GetSkillLevel(caster.magic) * 2), caster, Map.GetCellRelevantToCell(caster.CurrentCell, args, false));
                Map.GetCellRelevantToCell(caster.CurrentCell, args, true).SendShout("a bubbling hiss.");
            }

            caster.WriteToDisplay("You create a boiling pool of " + this.Name + ".");
            return true;
        }

        private bool castPhaseDoor(Character caster, string args)
        {
            List<string> destArray = new List<string>();
            destArray.Clear();

            string key = "";
            int finalX = 0;
            int finalY = 0;
            int sxcord = 0;
            int sycord = 0;
            int szcord = 0;
            int Radius = (int)(Skills.GetSkillLevel(caster.magic) * 0.5);
            bool goodDest = false;
            bool test= true;
            if (caster.CurrentCell.IsNoRecall || caster.CurrentCell.IsMagicDead)
            {
                caster.WriteToDisplay("You fail to shift your location.");
                return true;
            }
            for (int x = -Radius; x <= Radius; x++)
            {
                for (int y = -Radius; y <= Radius; y++)
                {
                    finalX = caster.X + x;
                    finalY = caster.Y + y;
                    key = finalX.ToString() + "," + finalY.ToString() + "," + caster.Z;
                    if (World.GetFacetByIndex(0).GetLandByID(caster.LandID).GetMapByID(caster.MapID).cells.ContainsKey(key))
                    {
                        if (!World.GetFacetByIndex(0).GetLandByID(caster.LandID).GetMapByID(caster.MapID).cells[key].IsOutOfBounds)
                        {
                            destArray.Add(key);
                        }
                    }
                }
            }
            
            for(int times = 0;times <= 4;times++)
            {
                string[] xyzRand = destArray[Rules.dice.Next(0, destArray.Count)].Split(",".ToCharArray());
                sxcord = Convert.ToInt32(xyzRand[0]);
                sycord = Convert.ToInt32(xyzRand[1]);
                szcord = Convert.ToInt32(xyzRand[2]);
                string tile = Map.returnTile(Cell.GetCell(0, caster.LandID, caster.MapID, sxcord, sycord, szcord).DisplayGraphic);
                test = true;
                switch (tile)
                {
                    case Cell.GRAPHIC_WALL:
                    case Cell.GRAPHIC_REEF:
                    case Cell.GRAPHIC_MOUNTAIN:
                    case Cell.GRAPHIC_FOREST_IMPASSABLE:
                    case Cell.GRAPHIC_SECRET_DOOR:		
                    case Cell.GRAPHIC_ALTAR:
                    case Cell.GRAPHIC_ALTAR_PLACEABLE:
                    case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:		
                    case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:
                    case Cell.GRAPHIC_COUNTER:
                    case Cell.GRAPHIC_COUNTER_PLACEABLE:
                    case Cell.GRAPHIC_AIR:
                    case Cell.GRAPHIC_GRATE:		
                        test = false;
                        break;
                }
                if (Cell.GetCell(0, caster.LandID, caster.MapID, sxcord, sycord, szcord).IsNoRecall || 
                    Cell.GetCell(0, caster.LandID, caster.MapID, sxcord, sycord, szcord).IsWithinTownLimits)
                {
                    test = false;
                }
                if (test)
                {
                    goodDest = true;
                }
            }
            if (goodDest)
            {
                this.SendGenericCastMessage(caster, null, true);
                caster.CurrentCell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, sxcord, sycord, szcord);
                caster.WriteToDisplay("You phase to a new location.");
            }
            else
            {
                caster.WriteToDisplay("The spell fails to shift you to a new location.");
            }
            return true;
        }
        
        #endregion
        
        #region Thaumaturge Spells
        private bool castCurse(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            int dmgMultiplier = CURSE_SPELL_MULTIPLICAND_NPC;
            if (caster.IsPC) dmgMultiplier = CURSE_SPELL_MULTIPLICAND_PC; // allow players to do slightly more damage than critters at same skill level

            SendGenericCastMessage(caster, target, true);          
            
            if (Combat.DoSpellDamage(caster, target, null, Skills.GetSkillLevel(caster.magic) * dmgMultiplier + GetSpellDamageModifier(caster), "curse") == 1)
            {
                Rules.GiveKillExp(caster, target);
                Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
            }

            return true;
        }

        private bool castTurnUndead(Character caster, string args)
        {
            caster.WriteToDisplay("You cast " + this.Name + ".");
            this.CastGenericAreaSpell(caster, args, Effect.EffectType.Turn_Undead, Skills.GetSkillLevel(caster.magic) * 10, this.Name);
            return true;
        }

        private bool castLightningBolt(Character caster, string args)
        {
            try
            {
                Character target = FindAndConfirmTarget(caster, args);
                Cell cell = null;
                if (target == null) cell = Map.GetCellRelevantToCell(caster.CurrentCell, args, false);
                else cell = target.CurrentCell;

                if (cell == null && target == null)
                    return false;

                #region Path testing.
                PathTest pathTest = new PathTest();
                pathTest.CurrentCell = caster.CurrentCell;
                pathTest.Name = "cast.command";
                if (!pathTest.BuildMoveList(cell.X, cell.Y, cell.Z))
                    cell = caster.CurrentCell;
                pathTest.RemoveFromWorld(); 
                #endregion

                cell.SendShout("a thunder clap!");
                cell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.ThunderClap));

                List<Character> theAffected = new List<Character>(cell.Characters.Values);

                if (theAffected.Count > 0)
                {
                    int dmgMultiplier = 6;

                    if (caster is PC) dmgMultiplier = 8;

                    int damage = Skills.GetSkillLevel(caster.magic) * dmgMultiplier + GetSpellDamageModifier(caster);

                    if (!caster.IsPC)
                    {
                        if (caster.species == Globals.eSpecies.LightningDrake)
                        {
                            damage = Rules.RollD(Skills.GetSkillLevel(caster.magic), 14);
                        }
                    }

                    foreach (Character affected in new List<Character>(theAffected))
                    {
                        if (Combat.DoSpellDamage(caster, affected, null, damage, "lightning") == 1)
                        {
                            Rules.GiveAEKillExp(caster, affected);
                            Skills.GiveSkillExp(caster, affected, Globals.eSkillType.Magic);
                        }
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

        private bool castBanish(Character caster, string args, Character target)
        {
            if (target == null) { return false; }

            if ((target is NPC) && (target as NPC).IsSummoned)
            {
                SendGenericCastMessage(caster, target, true);
                target.Age += Skills.GetSkillLevel(caster.magic) * Spell.BANISH_MULTI;
                Rules.UnsummonCreature(target as NPC);
            }
            else
            {
                caster.WriteToDisplay("Your target cannot be banished.");
                return false;
            }

            return true;
        }

        private bool castDeath(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            this.SendGenericCastMessage(caster, target, true);

            int totalDamage = (Skills.GetSkillLevel(caster.magic) * (caster.IsPC ? DEATH_SPELL_MULTIPLICAND_PC : DEATH_SPELL_MULTIPLICAND_NPC)) + GetSpellDamageModifier(caster);

            totalDamage += Rules.RollD(1, 2) == 1 ? Rules.RollD(1, 4) : -(Rules.RollD(1, 4));
            
            if (Combat.DoSpellDamage(caster, target, null, totalDamage, this.name.ToLower()) == 1)
            {
                Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
                Rules.GiveKillExp(caster, target);
            }

            return true;
        }

        private bool castRaiseDead(Character caster, string args)
        {
            bool match = false;
            if (caster.CurrentCell.Items.Count > 0)
            {
                int cellCount = caster.CurrentCell.Items.Count;
                Item corpse;
                for (int x = 0; x < cellCount; x++)
                {
                    corpse = caster.CurrentCell.Items[x];
                    if (corpse.itemType == Globals.eItemType.Corpse)
                    {
                        foreach (PC player in Character.PCList)
                        {
                            if (player.Name == corpse.special)
                            {
                                if (player.IsDead)
                                {
                                    player.IsDead = false;
                                    player.IsInvisible = false;
                                    player.CurrentCell = caster.CurrentCell;
                                    player.Hits = (int)(player.HitsMax / 3);
                                    player.Stamina = (int)(player.StaminaMax / 3);
                                    if (player.ManaMax > 0)
                                    {
                                        player.Mana = (int)(player.ManaMax / 3);
                                    }
                                    player.WriteToDisplay("You have been raised from the dead.");
                                    player.SendSound(Sound.GetCommonSound(Sound.CommonSound.DeathRevive));
                                    match = true;
                                }
                            }
                        }
                        if (match)
                        {
                            caster.CurrentCell.Items.RemoveAt(x);
                        }
                        continue;
                    }
                }
            }
            if (!match)
            {
                caster.WriteToDisplay("There is nothing here to raise from the dead.");
                return false;
            }
            return true;
        }

        private bool castResistFear(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);

            SendGenericEnchantMessage(target);

            Effect.CreateCharacterEffect(Effect.EffectType.Resist_Fear, Skills.GetSkillLevel(caster.magic) * 3, target, Skills.GetSkillLevel(caster.magic) * 20, caster);

            return true;
        }

        private bool castProjectileLightning(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if(target == null || target.CurrentCell == null) {return false;}

            int dmgMultiplier = 6;

            if (caster is PC) dmgMultiplier = 8;

            int damage = Skills.GetSkillLevel(caster.magic) * dmgMultiplier + GetSpellDamageModifier(caster);

            if (!caster.IsPC)
            {
                if (caster.species == Globals.eSpecies.LightningDrake)
                {
                    damage = Rules.RollD(Skills.GetSkillLevel(caster.magic), 14);
                }
            }

            CastGenericWallSpell(caster, args, Effect.EffectType.Lightning, damage);
            return true;
        }

        private bool castCreateSnake(Character caster, string args)
        {
            /*      Power   Mana    Type        Armor (item IDs)            Skill (base)    Abilities
             *      1       7       snake       none                        6               none
             *      2       12      asp         none                        7               minor poison
             *      3       17      cobra       none                        8               minor poison
             *      4       22      boa         none                        9               major strength
             *      5       27      serpent     none                        10              wields weapons
            */

            #region Determine number of pets. Return false if at or above MAX_PETS.
            short petCount = 0;
            short serpentCount = 0;

            foreach (NPC pet in caster.Pets)
            {
                if (pet.QuestList.Count == 0)
                {
                    petCount++;
                    if (pet.Name == "serpent")
                        serpentCount++;
                }
            }

            if (petCount >= MAX_PETS)
            {
                caster.WriteToDisplay("You may only control " + MAX_PETS + " pets.");
                return false;
            }
            #endregion

            #region Clean up and then split the arguments.
            args = args.Replace(this.SpellCommand, "");

            args = args.Trim();

            string[] sArgs = args.Split(" ".ToCharArray()); 
            #endregion

            #region Determine power.
            int power = 1; // default power

            if (sArgs.Length > 0)
            {
                try
                {
                    power = Convert.ToInt32(sArgs[0]);

                    if (power > 5)
                        power = 5;
                }
                catch (Exception)
                {
                    power = 1;
                }
            }
            #endregion

            #region Power determines mana cost.
            switch (power)
            {
                case 1:
                    if (caster.Mana < this.ManaCost)
                    {
                        return false;
                    }
                    break;
                case 2:
                    if (caster.Mana < this.ManaCost + 5)
                    {
                        caster.Mana -= 5;
                        return false;
                    }
                    caster.Mana -= 5;
                    break;
                case 3:
                    if (caster.Mana < this.ManaCost + 10)
                    {
                        caster.Mana -= 10;
                        return false;
                    }
                    caster.Mana -= 10;
                    break;
                case 4:
                    if (caster.Mana < this.ManaCost + 15)
                    {
                        caster.Mana -= 15;
                        return false;
                    }
                    caster.Mana -= 15;
                    break;
                case 5:
                    // check mana cost or if caster already has a serpent
                    if (caster.Mana < this.ManaCost + 20 || serpentCount > 0)
                    {
                        caster.Mana -= 20;
                        if (serpentCount > 0)
                            caster.WriteToDisplay("You may only control one serpent at a time.");
                        return false;
                    }
                    caster.Mana -= 20;
                    break;
            } 
            #endregion

            int casterSkillLevel = Skills.GetSkillLevel(caster.magic);

            NPC summoned = NPC.LoadNPC(905, caster.FacetID, caster.LandID, caster.MapID, caster.X, caster.Y, caster.Z, -1);
            //NPC summoned = NPC.CreateNPC(905, caster.FacetID, caster.LandID, caster.MapID, caster.X, caster.Y, caster.Z); // base summoned snake NPC

            summoned.Alignment = caster.Alignment;
            summoned.aiType = NPC.AIType.Summoned;
            summoned.Age = 0;
            summoned.special = "despawn";
            summoned.species = Globals.eSpecies.Reptile;
            summoned.WearItem(Item.CopyItemFromDictionary(8114)); // all summoned snakes wear sandwyrm scales, serpent clears from armor list below
            summoned.canCommand = true;
            summoned.IsMobile = true;
            summoned.IsSummoned = true;
            summoned.IsUndead = false;
            summoned.animal = true; // set to false below for serpent
            summoned.classFullName = "Fighter";
            summoned.BaseProfession = Character.ClassType.Fighter;
            summoned.Level = (short)((casterSkillLevel / 2) + power);

            summoned.RoundsRemaining = Utils.TimeSpanToRounds(new TimeSpan(0, (power * 2) + (casterSkillLevel % 10), 0));

            summoned.HitsMax = Rules.RollD(summoned.Level, 12) + casterSkillLevel;
            summoned.Hits = summoned.HitsMax;

            long skillToNext = Skills.GetSkillToNext(summoned.Level);
            summoned.unarmed = skillToNext;

            switch (power)
            {
                case 1: // snake
                    summoned.Name = "snake";
                    summoned.shortDesc = "snake";
                    summoned.longDesc = "a large black adder";
                    break;
                case 2: // asp
                    summoned.Name = "asp";
                    summoned.shortDesc = "asp";
                    summoned.longDesc = "a venemous asp";
                    summoned.poisonous = (short)(5 + casterSkillLevel);
                    break;
                case 3: // cobra
                    summoned.Name = "cobra";
                    summoned.shortDesc = "cobra";
                    summoned.longDesc = "a huge king cobra";
                    summoned.poisonous = (short)(10 + casterSkillLevel);
                    break;
                case 4: // boa
                    summoned.Name = "boa";
                    summoned.shortDesc = "boa constrictor";
                    summoned.longDesc = "a massive boa constrictor";
                    summoned.Strength = 25;
                    summoned.strengthAdd = 10;
                    break;
                case 5: // serpent
                    summoned.Name = "serpent";
                    summoned.shortDesc = "serpent";
                    summoned.longDesc = "a tall, green-scaled serpent with two muscular arms";
                    summoned.animal = false;
                    summoned.visualKey = "serpent";
                    summoned.Strength = 19;
                    summoned.Intelligence = 8;
                    summoned.Wisdom = 7;
                    summoned.strengthAdd = 6;
                    summoned.mace = 0;
                    summoned.bow = 0;
                    summoned.twoHanded = skillToNext;
                    summoned.sword = skillToNext;
                    summoned.magic = 0;
                    summoned.shuriken = 0;
                    summoned.staff = 0;
                    summoned.rapier = 0;
                    summoned.dagger = 0;
                    summoned.flail = 0;
                    summoned.halberd = skillToNext;
                    summoned.threestaff = 0;
                    summoned.bash = skillToNext;
                    summoned.wearing.Clear();
                    summoned.baseArmorClass = 5;
                    summoned.THAC0Adjustment = -4;
                    Item shortsword = Item.CopyItemFromDictionary(25020);
                    summoned.EquipRightHand(shortsword);
                    summoned.EquipLeftHand(shortsword);
                    break;
            }

            summoned.PetOwner = caster;
            caster.Pets.Add(summoned);
            return true;
        }

        private bool castSummonPhantasm(Character caster, string args)
        {
            //if (!(caster is PC) || (caster as PC).ImpLevel < Globals.eImpLevel.DEV)
            //{
            //    caster.WriteToDisplay("The " + this.Name + " spell is currently disabled.");
            //    return false;
            //}
            
            /*      Power   Mana    Type        Armor (item IDs)            Skill (base)    Spells
             *      1       20      phantasm    leather (8010, 15010)       7               none
             *      2       25      phantasm    chain (8015, 15015          8               none
             *      3       30      djinn       banded mail (8020, 15020)   9               ice storm
             *      4       30      salamander  sally scales (8102)         9               firewall
             *      5       35      efreet      steel (8021, 15021)         10              concussion    
            */

            #region Determine number of pets. Return false if at or above MAX_PETS.
            int petCount = 0;

            foreach (NPC pet in caster.Pets)
            {
                if (pet.QuestList.Count == 0)
                {
                    petCount++;
                }
            }

            if (petCount >= MAX_PETS)
            {
                caster.WriteToDisplay("You may only control " + MAX_PETS + " pets.");
                return false;
            } 
            #endregion

            args = args.Replace(this.SpellCommand, "");

            args = args.Trim();

            string[] sArgs = args.Split(" ".ToCharArray());            

            #region Determine power.
            int power = 1; // default power

            if (sArgs.Length > 0)
            {
                try
                {
                    power = Convert.ToInt32(sArgs[0]);

                    if (power > 5)
                        power = 5;
                }
                catch (Exception)
                {
                    power = 1;
                }
            }
            #endregion

            int magicSkillLevel = Skills.GetSkillLevel(caster.magic);

            #region Verify skill level for power of spell.
            if (magicSkillLevel < 15)
            {
                if (magicSkillLevel < 14 && power == 5)
                {
                    caster.WriteToDisplay("You are not skilled enough yet to summon efreeti.");
                    return true;
                }

                if (magicSkillLevel < 13 && power == 4)
                {
                    caster.WriteToDisplay("You are not skilled enough yet to summon salamanders.");
                    return true;
                }

                if (magicSkillLevel < 12 && power == 3)
                {
                    caster.WriteToDisplay("You are not skilled enough yet to summon djinn.");
                    return true;
                }
            } 
            #endregion

            #region Setup the summoned spirit.

            int npcID = 902;
            List<Item> armorToWear = new List<Item>();

            // TODO add spells
            switch (power)
            {
                case 1: // phantasm with leather
                    if (caster.Mana < this.ManaCost)
                    {
                        return false;
                    }
                    armorToWear.Add(Item.CopyItemFromDictionary(8010));
                    armorToWear.Add(Item.CopyItemFromDictionary(15010));
 
                    break;
                case 2: // phantasm with chain
                    if (caster.Mana < this.ManaCost + 5)
                    {
                        caster.Mana -= 5;
                        return false;
                    }
                    armorToWear.Add(Item.CopyItemFromDictionary(8015));
                    armorToWear.Add(Item.CopyItemFromDictionary(15015));
                    caster.Mana -= 5;
                    break;
                case 3:
                    if (caster.Mana < this.ManaCost + 10)
                    {
                        caster.Mana -= 10;
                        return false;
                    }
                    caster.Mana -= 10;
                    npcID = 903; // djinn with banded mail and icestorm
                    armorToWear.Add(Item.CopyItemFromDictionary(8020));
                    armorToWear.Add(Item.CopyItemFromDictionary(15020));
                    break;
                case 4:
                    if (caster.Mana < this.ManaCost + 10)
                    {
                        caster.Mana -= 10;
                        return false;
                    }
                    caster.Mana -= 10;
                    npcID = 37; // salamander with scales and firewall
                    armorToWear.Add(Item.CopyItemFromDictionary(8102));
                    break;
                case 5:
                    if (caster.Mana < this.ManaCost + 15)
                    {
                        caster.Mana -= 15;
                        return false;
                    }
                    caster.Mana -= 15;
                    npcID = 904; // efreet with steel plate and concussion
                    armorToWear.Add(Item.CopyItemFromDictionary(8021));
                    armorToWear.Add(Item.CopyItemFromDictionary(15021));
                    break;
                default:
                    break;
            }

            // Create the summoned spirit.
            NPC summoned = NPC.LoadNPC(npcID, caster.FacetID, caster.LandID, caster.MapID, caster.X, caster.Y, caster.Z, -1);
            //NPC summoned = NPC.CreateNPC(npcID, caster.FacetID, caster.LandID, caster.MapID, caster.X, caster.Y, caster.Z);

            summoned.wearing.Clear();

            foreach (Item armor in armorToWear)
                summoned.wearing.Add(armor);

            summoned.Alignment = caster.Alignment;
            summoned.aiType = NPC.AIType.Summoned;
            summoned.Age = 0;
            summoned.special = "despawn";
            int fiveMinutes = Utils.TimeSpanToRounds(new TimeSpan(0, 5, 0));
            // 30 minutes + 5 minutes for every skill level past 11 minus 5 minutes for every power of the spell beyond 1.
            summoned.RoundsRemaining = (fiveMinutes * 6) + ((magicSkillLevel - this.RequiredLevel) * fiveMinutes) - ((power - 1) * fiveMinutes);
            summoned.species = Globals.eSpecies.Magical; // this may need to be changed for AI to work properly

            summoned.canCommand = true;
            summoned.IsMobile = true;
            summoned.IsSummoned = true;
            summoned.IsUndead = false;

            summoned.Level = Convert.ToInt16(magicSkillLevel + power - 1);

            summoned.HitsMax = Rules.RollD(summoned.Level, caster.Land.HitDice[(int)summoned.BaseProfession]);
            
            summoned.StaminaMax = Rules.RollD(summoned.Level, caster.Land.StaminaDice[(int)summoned.BaseProfession]);

            // phantasms and salamanders do not get mana
            if (power > 2 && power != 4)
            {
                summoned.ManaMax = Rules.RollD(1, 4) + magicSkillLevel; // probably a little overpowered if mana used a dice roll
            }
            else summoned.ManaMax = 0;

            summoned.Hits = summoned.HitsMax;
            summoned.Mana = summoned.ManaMax;
            summoned.Stamina = summoned.StaminaMax;

            long skillToNext = Skills.GetSkillToNext(power + (magicSkillLevel / 2));

            summoned.mace = skillToNext;
            summoned.bow = skillToNext;
            summoned.unarmed = skillToNext;
            summoned.twoHanded = skillToNext;
            summoned.sword = skillToNext;
            summoned.magic = skillToNext;
            summoned.shuriken = skillToNext;
            summoned.staff = skillToNext;
            summoned.rapier = skillToNext;
            summoned.dagger = skillToNext;
            summoned.flail = skillToNext;
            summoned.halberd = skillToNext;
            summoned.threestaff = skillToNext;

            summoned.PetOwner = caster;
            caster.Pets.Add(summoned);
            #endregion

            if (summoned.CurrentCell != caster.CurrentCell)
                summoned.CurrentCell = caster.CurrentCell;

            summoned.AddToWorld();

            return true;
        }

        private bool castResistBlind(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);

            SendGenericEnchantMessage(target);

            Effect.CreateCharacterEffect(Effect.EffectType.Resist_Blind, Skills.GetSkillLevel(caster.magic) * 3, target, Skills.GetSkillLevel(caster.magic) * 20, caster);

            return true;
        }

        private bool castSummonDemon(Character caster, string args)
        {
            if (args == "" || args == " " || args == null)
            {
                args = "3";//minimum
            }
            string[] sArgs = args.Split(" ".ToCharArray());

            int power = 3; // default power
            int count = sArgs.Length;
            Item armor = Item.CopyItemFromDictionary(8031);
            #region Set the power of the spell - Using a try/catch to set defaults if invalid sArg[1]
            try
            {
                power = Convert.ToInt32(sArgs[1]);
                if (power > 5)
                    power = 5;
            }
            catch (Exception)
            {
                power = Rules.RollD(1, 5);
            }
            #endregion
            #region Setup the summon NPC
            NPC summoned = NPC.LoadNPC(Item.ID_SUMMONEDMOB, caster.FacetID, caster.LandID, caster.MapID, caster.X, caster.Y, caster.Z, -1);
            //NPC summoned = NPC.CreateNPC(Item.ID_SUMMONEDMOB, caster.FacetID, caster.LandID, caster.MapID, caster.X, caster.Y, caster.Z);
            summoned.Alignment = caster.Alignment;
            summoned.HitsMax = Rules.RollD(Skills.GetSkillLevel(caster.magic), 12);
            summoned.Hits = summoned.HitsMax;
            summoned.aiType = NPC.AIType.Summoned;
            summoned.Age = 0;
            //summoned.special = Skills.GetSkillLevel(caster.magic).ToString();
            summoned.RoundsRemaining = Skills.GetSkillLevel(caster.magic);
            summoned.WearItem(armor);
            summoned.canCommand = false;
            summoned.IsMobile = true;
            summoned.IsSummoned = true;
            summoned.IsUndead = false;
            summoned.Level = (short)Skills.GetSkillLevel(caster.magic);
            summoned.animal = false;
            summoned.species = Globals.eSpecies.Demon;
            summoned.shortDesc = "demon";
            summoned.longDesc = "a demon";
            summoned.classFullName = "Fighter";
            summoned.mace = Skills.GetSkillToNext(summoned.Level);
            summoned.bow = Skills.GetSkillToNext(summoned.Level);
            summoned.unarmed = Skills.GetSkillToNext(summoned.Level);
            summoned.twoHanded = Skills.GetSkillToNext(summoned.Level);
            summoned.sword = Skills.GetSkillToNext(summoned.Level);
            summoned.magic = Skills.GetSkillToNext(summoned.Level);
            summoned.shuriken = Skills.GetSkillToNext(summoned.Level);
            summoned.staff = Skills.GetSkillToNext(summoned.Level);
            summoned.rapier = Skills.GetSkillToNext(summoned.Level);
            summoned.dagger = Skills.GetSkillToNext(summoned.Level);
            summoned.flail = Skills.GetSkillToNext(summoned.Level);
            summoned.halberd = Skills.GetSkillToNext(summoned.Level);
            summoned.threestaff = Skills.GetSkillToNext(summoned.Level);
            summoned.BaseProfession = Character.ClassType.Fighter;
            summoned.wearing.Add(armor);
            switch(power)
            {
                case 3:// normal demon
                    summoned.Name = "demon";
                    break;
                case 4: // demon
                    summoned.Name = "demon";
                    summoned.HitsMax += Rules.RollD(Convert.ToInt32(Skills.GetSkillLevel(caster.magic) / 2), 12);
                    summoned.Hits = summoned.HitsMax;
                    summoned.Level ++;
                    summoned.shortDesc = "demon";
                    summoned.longDesc = "a large demon";
                    string chant = Spell.GenerateMagicWords();
                    summoned.spellList.Add(1, chant);
                    summoned.castMode = NPC.CastMode.Limited;
                    summoned.Mana = 50;
                    summoned.ManaMax = 50;
                    break;
                case 5: // greater demon
                    summoned.Name = "greater demon";
                    summoned.HitsMax += Rules.RollD(Skills.GetSkillLevel(caster.magic), 12);
                    summoned.Hits = summoned.HitsMax;
                    summoned.Level += 2;
                    summoned.shortDesc = "greater demon";
                    summoned.longDesc = "a greater demon";
                    summoned.unarmed += 1;
                    string dchant = Spell.GenerateMagicWords();
                    summoned.spellList.Add(14, dchant);
                    summoned.castMode = NPC.CastMode.Limited;
                    summoned.Mana = 70;
                    summoned.ManaMax = 70;
                    break;
                default:// normal demon
                    summoned.Name = "demon";
                    break;
            }
            //demons are not commandable
            //caster.Pets.Add(summoned);
            //summoned.PetOwner = caster;
            #endregion
            Utils.Log("SummonDemon: Power: " + power + " [args = 0: " + sArgs[0] + " 1: " + sArgs[1] + " TTL: " + summoned.special, Utils.LogType.Unknown);
            return true;
        }

        private bool castFear(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            if (Combat.DoSpellDamage(caster, target, null, Skills.GetSkillLevel(caster.magic) * 2, this.Name) == 0)
            {
                SendGenericResistMessages(caster, target);
            }
            else
            {
                SendGenericCastMessage(caster, target, true);
                target.WriteToDisplay("You have been hit by a " + this.Name + " spell!");
                Effect.CreateCharacterEffect(Effect.EffectType.Fear, 1, target, Rules.RollD(1, (int)(Skills.GetSkillLevel(caster.magic) / 2)) + 1, caster);
            }
            return true;
        }

        private bool castResistLightning(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) return false;

            SendGenericCastMessage(caster, target, true);

            SendGenericEnchantMessage(target);

            Effect.CreateCharacterEffect(Effect.EffectType.Resist_Lightning, Skills.GetSkillLevel(caster.magic) * 3, target, Skills.GetSkillLevel(caster.magic) * 20, caster);

            return true;
        }

        private bool castProtectPoison(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);

            SendGenericEnchantMessage(target);
            int amount = Skills.GetSkillLevel(caster.magic) * 5;
            if (amount > 25) 
                amount = 25;
            Effect.CreateCharacterEffect(Effect.EffectType.Protection_from_Poison, amount, target, Skills.GetSkillLevel(caster.magic) * 20, caster);

            return true;
        }

        private bool castPoisonCloud(Character caster, string args)
        {
            SendGenericCastMessage(caster, null, true);
            CastGenericAreaSpell(caster, args, Effect.EffectType.Poison, Convert.ToInt32(Skills.GetSkillLevel(caster.magic) * 1.5), this.Name);
            return true;
        }

        private bool castResistStun(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);

            SendGenericEnchantMessage(target);

            Effect.CreateCharacterEffect(Effect.EffectType.Resist_Stun, Skills.GetSkillLevel(caster.magic) * 3, target, Skills.GetSkillLevel(caster.magic) * 20, caster);

            return true;
        }

        private bool castResistDeath(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);

            SendGenericEnchantMessage(target);

            Effect.CreateCharacterEffect(Effect.EffectType.Resist_Death, Skills.GetSkillLevel(caster.magic) * 3, target, Skills.GetSkillLevel(caster.magic) * 20, caster);

            return true;
        }

        private bool castProtectBlindFear(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);

            SendGenericEnchantMessage(target);
            int amount = Skills.GetSkillLevel(caster.magic) * 5;
            if (amount > 25)
                amount = 25;
            Effect.CreateCharacterEffect(Effect.EffectType.Protection_from_Blind_and_Fear, amount, target, Skills.GetSkillLevel(caster.magic) * 30, caster);
            return true;
        }

        private bool castProtectStunDeath(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);

            SendGenericEnchantMessage(target);

            Effect.CreateCharacterEffect(Effect.EffectType.Protection_from_Stun_and_Death, Skills.GetSkillLevel(caster.magic) * 5, target, Skills.GetSkillLevel(caster.magic) * 30, caster);
            return true;
        }

        private bool castBlind(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);

            if(!Combat.DND_CheckSavingThrow(target, Combat.SavingThrow.Spell, -target.BlindResistance))
            {
                target.WriteToDisplay("You have been blinded!");
                Effect.CreateCharacterEffect(Effect.EffectType.Blind, 1, target, ((int)Skills.GetSkillLevel(caster.magic) / 2) + Rules.dice.Next(-3, 3), caster);
            }
            else SendGenericResistMessages(caster, target);
            return true;
        }

        private bool castStun(Character caster, string args)
        {
            try
            {
                Character target = FindAndConfirmTarget(caster, args);

                if (target == null) { return false; }

                if (target.Stunned > 1)
                {
                    caster.WriteToDisplay("Your target is already stunned.");
                    return true;
                }

                SendGenericCastMessage(caster, target, true);

                if (Combat.DoSpellDamage(caster, target, null, 0, "stun") == 1)
                {
                    target.WriteToDisplay("You are stunned!");

                    if (target.preppedSpell != null)
                    {
                        target.preppedSpell = null;
                        target.WriteToDisplay("Your spell has been lost.");
                    }

                    //stun duration is random rounds from 1 to magic skill level divided by 2
                    short stunAmount = (short)(Rules.dice.Next(1, (int)(Skills.GetSkillLevel(caster.magic) / 2)) + 1);

                    target.Stunned = stunAmount;

                    short stunCount = 1;

                    if (target.Group != null)
                    {
                        foreach (NPC npc in target.Group.GroupNPCList)
                        {
                            if (npc != target)
                            {
                                target.Stunned = stunAmount;

                                stunCount++;
                            }
                        }
                    }

                    if (target.race != "")
                    {
                        target.SendToAllInSight(target.Name + " is stunned.");
                    }
                    else
                    {
                        if (stunCount == 1)
                        {
                            target.SendToAllInSight("The " + target.Name + " is stunned.");
                        }
                        else target.SendToAllInSight(stunCount + " " + Cell.Multinames(target.Name) + " are stunned.");
                    }
                }
                else SendGenericResistMessages(caster, target);
                return true;
            }
            catch (Exception e)
            {
                Utils.Log("Error in Stun Spell. " + caster.GetLogString(), Utils.LogType.Unknown);
                Utils.LogException(e);
                return false;
            }
            
        }

        #endregion

        private bool castBreatheWater(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) return false;

            SendGenericCastMessage(caster, target, true);

            SendGenericEnchantMessage(target);

            Effect.CreateCharacterEffect(Effect.EffectType.Breathe_Water, 1, target, Skills.GetSkillLevel(caster.magic) * 30, caster);

            return true;
        }

        private bool castCloseOpenDoor(Character caster, string args)
        {
            Cell cell = Map.GetCellRelevantToCell(caster.CurrentCell, args, true);
            string newGraphic = cell.DisplayGraphic;
            bool spellSuccess = false;
            string soundFile = "";

            switch (cell.CellGraphic)
            {
                case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:
                    SendGenericCastMessage(caster, null, true);
                    newGraphic = Cell.GRAPHIC_OPEN_DOOR_VERTICAL;
                    soundFile = Sound.GetCommonSound(Sound.CommonSound.OpenDoor);
                    spellSuccess = true;
                    break;
                case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:
                    SendGenericCastMessage(caster, null, true);
                    newGraphic = Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL;
                    soundFile = Sound.GetCommonSound(Sound.CommonSound.OpenDoor);
                    spellSuccess = true;
                    break;
                case Cell.GRAPHIC_OPEN_DOOR_VERTICAL:
                    SendGenericCastMessage(caster, null, true);
                    newGraphic = Cell.GRAPHIC_CLOSED_DOOR_VERTICAL;
                    soundFile = Sound.GetCommonSound(Sound.CommonSound.CloseDoor);
                    spellSuccess = true;
                    break;
                case Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL:
                    SendGenericCastMessage(caster, null, true);
                    newGraphic = Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL;
                    soundFile = Sound.GetCommonSound(Sound.CommonSound.CloseDoor);
                    spellSuccess = true;
                    break;
                default:
                    caster.WriteToDisplay("There is no door there.");
                    break;
            }

            caster.EmitSound(this.SoundFile);

            if (spellSuccess)
            {
                cell.CellGraphic = newGraphic;
                cell.DisplayGraphic = newGraphic;
                cell.EmitSound(soundFile);
                return true;
            }

            return false;
        }

        private bool castProtectFire(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);

            SendGenericEnchantMessage(target);

            int amount = 0;
            int duration = 0;

            int skillLevel = Skills.GetSkillLevel(caster.magic);

            if (skillLevel == 0)
            {
                amount = caster.Level * 5;
                duration = caster.Level * 30;
            }
            else
            {
                amount = skillLevel * 5;
                duration = skillLevel * 30;
            }

            if (amount > 25) { amount = 25; }

            Effect.CreateCharacterEffect(Effect.EffectType.Protection_from_Fire, amount, target, duration, caster);

            return true;
        }

        private bool castDarkness(Character caster, string args)
        {
            this.SendGenericCastMessage(caster, null, true);
            this.CastGenericAreaSpell(caster, args, Effect.EffectType.Darkness, Skills.GetSkillLevel(caster.magic) * 2, this.Name);
            return true;
        }

        private bool castProtectCold(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);

            SendGenericEnchantMessage(target);
            int amount = Skills.GetSkillLevel(caster.magic) * 5;
            if (amount > 25)
                amount = 25;
            Effect.CreateCharacterEffect(Effect.EffectType.Protection_from_Cold, amount, target, Skills.GetSkillLevel(caster.magic) * 20, caster);
            return true;
        }

        private bool castLight(Character caster, string args)
        {
            SendGenericCastMessage(caster, null, true);
            CastGenericAreaSpell(caster, args, Effect.EffectType.Light, Skills.GetSkillLevel(caster.magic), this.Name);
            return true;
        }

        private bool castLocate(Character caster, string args)
        {
            string[] sArgs = args.Split(" ".ToCharArray());

            List<Character> locatedTargets = new List<Character>();

            foreach (Character ch in Character.AllCharList)
            {
                if (ch.FacetID == caster.FacetID && ch.LandID == caster.LandID && ch.MapID == caster.MapID)
                {
                    if (ch.Name.ToLower().StartsWith(sArgs[sArgs.Length - 1].ToLower()))
                    {
                        if (!ch.IsInvisible) // do not add invisible targets (implementors)
                        {
                            locatedTargets.Add(ch);
                        }
                    }
                }
            }

            caster.EmitSound(this.SoundFile);

            if (locatedTargets.Count < 1)
            {
                caster.WriteToDisplay("You cannot sense your target.");
                return true;
            }

            Character target = locatedTargets[0];

            foreach (Character ch in locatedTargets)
            {
                if (Cell.GetCellDistance(caster.X, caster.Y, target.X, target.Y) >
                    Cell.GetCellDistance(caster.X, caster.Y, ch.X, ch.Y))
                {
                    target = ch;
                }
            }

            string directionString = Map.GetDirection(caster.CurrentCell, target.CurrentCell).ToString().ToLower();

            if (directionString.ToLower() == "none")
            {
                caster.WriteToDisplay("Your target is directly in front of you.");
                return true;
            }

            if (target.Z == caster.Z)
            {
                #region Distance Information

                int distance = Cell.GetCellDistance(caster.X, caster.Y, target.X, target.Y);
                string distanceString = "";
                if (distance <= 6)
                {
                    distanceString = "very close!";
                }
                else if (distance > 6 && distance <= 12)
                {
                    distanceString = "fairly close.";
                }
                else if (distance > 12 && distance <= 18)
                {
                    distanceString = "close.";
                }
                else if (distance > 18 && distance <= 24)
                {
                    distanceString = "far away.";
                }
                else if (distance > 24)
                {
                    distanceString = "very far away.";
                }
                #endregion

                caster.WriteToDisplay("You sense that your target is to the " + directionString + " and " + distanceString);
            }
            else
            {
                #region Height Information
                string heightString = "";
                int heightDifference = 0;
                if (target.Z > caster.Z)
                {
                    heightDifference = Math.Abs(Math.Abs(caster.Z) - Math.Abs(target.Z));

                    if (heightDifference > 0 && heightDifference <= 60)
                    {
                        heightString = "above you";
                    }
                    else if (heightDifference > 60 && heightDifference <= 140)
                    {
                        heightString = "far above you";
                    }
                    else
                    {
                        heightString = "very far above you";
                    }
                }
                else
                {
                    heightDifference = Math.Abs(Math.Abs(target.Z) - Math.Abs(caster.Z));
                    if (heightDifference > 0 && heightDifference <= 60)
                    {
                        heightString = "below you";
                    }
                    else if (heightDifference > 60 && heightDifference <= 140)
                    {
                        heightString = "far below you";
                    }
                    else
                    {
                        heightString = "very far below you";
                    }
                }
                #endregion

                caster.WriteToDisplay("You sense that your target is " + heightString + " and to the " + directionString + ".");
            }

            return true;

        }

        private bool castStrength(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);

            SendGenericEnchantMessage(target);

            int effectAmount = 3;

            int skillLevel = Skills.GetSkillLevel(caster.magic);

            if ((caster.IsHybrid && caster.Level >= 18) || (caster.BaseProfession == Character.ClassType.Thaumaturge && skillLevel >= 15))
                effectAmount = 6;

            if (caster.BaseProfession == Character.ClassType.Thaumaturge)
            {
                Effect.CreateCharacterEffect(Effect.EffectType.Temporary_Strength, effectAmount, target, skillLevel * 20, caster);
            }
            else
            {
                Effect.CreateCharacterEffect(Effect.EffectType.Temporary_Strength, effectAmount, target, 180, caster);
            }

            return true;
        }

        private bool castCure(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null)
            {
                if (args == null || args.Length == 0)
                {
                    target = caster;
                }
                else return false;
            }

            if (target.IsUndead)
            {
                caster.WriteToDisplay("The " + this.Name + " does not work on the undead.");
                return false; // spell fails
            }

            int cureAmount = 0;
            int pctHitsLeft = (int)(((float)target.Hits / (float)target.HitsFull) * 100);

            if (caster.BaseProfession != Character.ClassType.Thaumaturge)
            {
                if (pctHitsLeft < 75)
                {
                    cureAmount = (int)((target.HitsFull - target.Hits) * .80);
                }
                else { target.Hits = target.HitsFull; }
            }
            else
            {
                if (pctHitsLeft < 50)
                {
                    int criticalHeal = Rules.RollD(1, 100);
                    if (criticalHeal < Skills.GetSkillLevel(caster.magic))
                    {
                        Skills.GiveSkillExp(caster, (caster.Level - criticalHeal) * 10, Globals.eSkillType.Magic);
                        target.Hits = target.HitsFull;
                    }
                    else { cureAmount = (int)((target.HitsFull - target.Hits) * .80); }
                }
                else { target.Hits = target.HitsFull; }
            }

            // halve the cure amount if diseased
            if (target.EffectsList.ContainsKey(Effect.EffectType.Contagion))
                cureAmount = cureAmount / 2;

            target.Hits += cureAmount;

            if (target.Hits > target.HitsFull) { target.Hits = target.HitsFull; }

            SendGenericCastMessage(caster, target, true);

            if (caster.BaseProfession == Character.ClassType.Knight && target != caster)
                target.SendToAllInSight(target.Name + " is surrounded by a pale blue glow from " + caster.Name + "'s outstretched hand.");
            target.WriteToDisplay("You have been healed.");

            return true;
        }

        private bool castNeutralizePoison(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            this.SendGenericCastMessage(caster, target, true);

            if (target.Poisoned > 0)
            {
                target.WriteToDisplay("The poison has been neutralized by " + caster.Name + ".");
                if (target.EffectsList.ContainsKey(Effect.EffectType.Poison))
                {
                    target.EffectsList[Effect.EffectType.Poison].StopCharacterEffect();
                }
                target.Poisoned = 0;
            }

            //if (target.effectList.ContainsKey(Effect.EffectType.Contagion))
            //    target.effectList[Effect.EffectType.Contagion].StopCharacterEffect();

            return true;
        }

        private bool castPortal(Character caster, string args)
        {
            int xpos = 0;
            int ypos = 0;

            //clean out the command name
            args = args.Replace(this.SpellCommand, "");
            args = args.Trim();
            string[] sArgs = args.Split(" ".ToCharArray());

            switch (sArgs[0])
            {
                case "south":
                case "s":
                    ypos++;
                    break;
                case "southeast":
                case "se":
                    ypos++;
                    xpos++;
                    break;
                case "southwest":
                case "sw":
                    ypos++;
                    xpos--;
                    break;
                case "west":
                case "w":
                    xpos--;
                    break;
                case "east":
                case "e":
                    xpos++;
                    break;
                case "northeast":
                case "ne":
                    ypos--;
                    xpos++;
                    break;
                case "northwest":
                case "nw":
                    ypos--;
                    xpos--;
                    break;
                case "north":
                case "n":
                    ypos--;
                    break;
                default:
                    break;
            }

            Cell cell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + xpos, caster.Y + ypos, caster.Z);
            
            ArrayList cells = new ArrayList();

            cells.Add(cell);

            if (cell.DisplayGraphic == Cell.GRAPHIC_WALL && !cell.IsMagicDead)
            {
                SendGenericCastMessage(caster, null, true);
                AreaEffect effect = new AreaEffect(Effect.EffectType.Illusion, Cell.GRAPHIC_EMPTY, 1, 2, caster, cells);
            }
            else
            {
                //GenericFailMessage(caster, "");
                return false;
            }

            return true;
        }

        private bool castProtectFireIce(Character caster, string args)
        {
            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = FindAndConfirmTarget(caster, sArgs[sArgs.Length - 1]);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);

            SendGenericEnchantMessage(target);
            int amount = Skills.GetSkillLevel(caster.magic) * 5;
            if (amount > 30)
                amount = 30;
            Effect.CreateCharacterEffect(Effect.EffectType.Protection_from_Fire_and_Ice, amount, target, Skills.GetSkillLevel(caster.magic) * 30, caster);
            return true;
        }

        private bool castWizardEye(Character caster, string args)
        {
            Effect.CreateCharacterEffect(Effect.EffectType.Wizard_Eye, 1, caster, 10, caster);
            return true;
        }

        #region Knight Spells
        private bool castBless(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            // the Bless spell can only be cast by knights on those that are lawful
            if (target.Alignment != caster.Alignment)
            {
                caster.WriteToDisplay("You may only bless other " + caster.Alignment.ToString().ToLower() + " beings.");
                return false;
            }

            SendGenericCastMessage(caster, target, true);

            SendGenericEnchantMessage(target);

            Effect.CreateCharacterEffect(Effect.EffectType.Bless, 0, target, 180, caster);
            return true;
        }
        #endregion

        #region Thief Spells
        private bool castFeatherFall(Character caster, string args)
        {
            // currently thieves may only enchant themselves with feather fall

            SendGenericEnchantMessage(caster);
            Effect.CreateCharacterEffect(Effect.EffectType.Feather_Fall, 1, caster, Skills.GetSkillLevel(caster.magic) * 30, caster);

            return true;
        }

        private bool castImprovedDisguise(Character caster, string args)
        {
            SendGenericEnchantMessage(caster);
            Effect.CreateCharacterEffect(Effect.EffectType.Improved_Disguise, 1, caster, Utils.TimeSpanToRounds(new TimeSpan(0, 2, 0)), caster);

            return true;
        }

        private bool castFindSecretDoor(Character caster, string args)
        {

            caster.EmitSound(this.SoundFile);

            int bitcount = 0;
            //loop through all visible cells
            for (int ypos = -3; ypos <= 3; ypos += 1)
            {
                for (int xpos = -3; xpos <= 3; xpos += 1)
                {
                    Cell cell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + xpos, caster.Y + ypos, caster.Z);
                    if (caster.CurrentCell.visCells[bitcount] && cell.IsSecretDoor)
                    {
                        if (cell.AreaEffects.ContainsKey(Effect.EffectType.Hide_Door))
                        {
                            cell.AreaEffects[Effect.EffectType.Hide_Door].StopAreaEffect();
                        }
                        else
                        {
                            AreaEffect effect = new AreaEffect(Effect.EffectType.Find_Secret_Door, Cell.GRAPHIC_EMPTY, 0, (int)(Skills.GetSkillLevel(caster.magic) / 2) + 10, caster, cell);
                            cell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.OpenDoor));
                        }
                    }
                    bitcount++;
                }
            }

            return true;
        }

        private bool castHideDoor(Character caster, string args)
        {
            Cell cell = Map.GetCellRelevantToCell(caster.CurrentCell, args, true);
            bool spellSuccess = false;

            switch (cell.CellGraphic)
            {
                case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:
                case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:
                case Cell.GRAPHIC_OPEN_DOOR_VERTICAL:
                case Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL:
                    caster.WriteToDisplay("You use your magic to conceal the door.");
                    spellSuccess = true;
                    break;
                case Cell.GRAPHIC_EMPTY:
                    if (cell.IsSecretDoor &&
                        (cell.AreaEffects.ContainsKey(Effect.EffectType.Find_Secret_Door) || cell.AreaEffects.ContainsKey(Effect.EffectType.Find_Secret_Rockwall)))
                    {
                        if (cell.AreaEffects.ContainsKey(Effect.EffectType.Find_Secret_Door))
                        {
                            caster.WriteToDisplay("You use your magic to close the secret door.");
                            cell.AreaEffects[Effect.EffectType.Find_Secret_Door].StopAreaEffect();
                        }

                        if (cell.AreaEffects.ContainsKey(Effect.EffectType.Find_Secret_Rockwall))
                        {
                            caster.WriteToDisplay("You use your magic to conceal the secret door.");
                            cell.AreaEffects[Effect.EffectType.Find_Secret_Rockwall].StopAreaEffect();
                        }

                        spellSuccess = true;

                    }
                    break;
                default:
                    caster.WriteToDisplay("Your spell fails.");
                    caster.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                    break;
            }
            if (spellSuccess)
            {
                cell.IsSecretDoor = true;
                AreaEffect effect = new AreaEffect(Effect.EffectType.Hide_Door, Cell.GRAPHIC_WALL, 0, (int)(Skills.GetSkillLevel(caster.magic) / 2), caster, cell);
                return true;
            }
            return false;
        }

        private bool castHideInShadows(Character caster, string args)
        {
            if (Map.IsNextToWall(caster))
            {
                if (!Rules.BreakHideSpell(caster))
                {
                    Effect.CreateCharacterEffect(Effect.EffectType.Hide_in_Shadows, 0, caster, 0, caster);
                    caster.WriteToDisplay("You fade into the shadows.");
                }
                else
                {
                    //GenericFailMessage(caster, "");
                    return false;
                }
            }
            else
            {
                caster.WriteToDisplay("You must be in the shadows to hide.");
            }
            return true;
        }

        private bool castIdentify(Character caster, string args)
        {
            try
            {
                string[] sArgs = args.Split(" ".ToCharArray());

                Item iditem = new Item();

                if (caster.RightHand != null)
                {
                    iditem = caster.RightHand;
                }
                else if (caster.LeftHand != null)
                {
                    iditem = caster.LeftHand;
                }
                else
                {
                    iditem = Item.GetItemOnGround(sArgs[sArgs.Length - 1], caster.FacetID, caster.LandID, caster.MapID, caster.X, caster.Y, caster.Z);
                    if (iditem == null)
                    {
                        caster.WriteToDisplay("You must hold the item in your hands or be standing next to it.");
                        return false;
                    }
                }

                string itmeffect = "";
                string itmenchantment = "";
                string itmcharges = "";
                string itmspecial = "";
                string itmalign = "";
                string itmattuned = "";

                if (iditem.spell > 0)
                {
                    if (iditem.charges == 0) { itmcharges = " There are no charges remaining."; }
                    if (iditem.charges > 1 && iditem.charges < 100) { itmcharges = " There are " + iditem.charges + " charges remaining."; }
                    if (iditem.charges == 1) { itmcharges = " There is 1 charge remaining."; }
                    if (iditem.charges == -1) { itmcharges = " The " + iditem.name + " has unlimited charges."; }

                    itmenchantment = " It contains the spell of " + Spell.GetSpell(iditem.spell).Name + "." + itmcharges;
                }

                System.Text.StringBuilder sb = new System.Text.StringBuilder(40);

                if (iditem.baseType == Globals.eItemBaseType.Figurine || iditem.figExp > 0)
                {
                    sb.AppendFormat(" The {0}'s avatar has " + iditem.figExp + " experience.", iditem.name);
                }
                if (iditem.combatAdds > 0)
                {
                    sb.AppendFormat(" The combat adds are {0}.", iditem.combatAdds);
                }
                if (iditem.silver)
                {
                    sb.AppendFormat(" The {0} is silver.", iditem.name);
                }
                if (iditem.blueglow)
                {
                    sb.AppendFormat(" The {0} is emitting a faint blue glow.", iditem.name);
                }

                itmspecial = sb.ToString();

                //item effects

                if (iditem.effectType.Length > 0)
                {
                    string[] itmEffectType = iditem.effectType.Split(" ".ToCharArray());
                    string[] itmEffectAmount = iditem.effectAmount.Split(" ".ToCharArray());

                    if (itmEffectType.Length == 1 && Effect.GetEffectName((Effect.EffectType)Convert.ToInt32(itmEffectType[0])) != "")
                    {
                        if (iditem.baseType == Globals.eItemBaseType.Bottle)
                        {
                            itmeffect = " Inside the bottle is a potion of " + Effect.GetEffectName((Effect.EffectType)Convert.ToInt32(itmEffectType[0])) + ".";
                        }
                        else
                        {
                            itmeffect = " The " + iditem.name + " contains the enchantment of " + Effect.GetEffectName((Effect.EffectType)Convert.ToInt32(itmEffectType[0])) + ".";
                        }
                    }
                    else
                    {
                        ArrayList itemEffectList = new ArrayList();

                        for (int a = 0; a < itmEffectType.Length; a++)
                        {
                            Effect.EffectType effectType = (Effect.EffectType)Convert.ToInt32(itmEffectType[a]);
                            if (Effect.GetEffectName(effectType).ToLower() != "unknown")
                            {
                                itemEffectList.Add(Effect.GetEffectName(effectType));
                            }
                        }

                        if (itemEffectList.Count > 0)
                        {
                            if (itemEffectList.Count > 1)
                            {
                                itmeffect = " The " + iditem.name + " contains the enchantments of";
                                for (int a = 0; a < itemEffectList.Count; a++)
                                {
                                    if (a != itemEffectList.Count - 1)
                                    {
                                        itmeffect += " " + (string)itemEffectList[a] + ",";
                                    }
                                    else
                                    {
                                        itmeffect += " and " + (string)itemEffectList[a] + ".";
                                    }
                                }
                            }
                            else if (itemEffectList.Count == 1)
                            {
                                if (iditem.baseType == Globals.eItemBaseType.Bottle)
                                {
                                    itmeffect = " Inside the bottle is a potion of " + Effect.GetEffectName((Effect.EffectType)Convert.ToInt32(itmEffectType[0])) + ".";
                                }
                                else
                                {
                                    itmeffect = " The " + iditem.name + " contains the enchantment of " + Effect.GetEffectName((Effect.EffectType)Convert.ToInt32(itmEffectType[0])) + ".";
                                }
                            }
                        }
                    }
                }
                //item alignment
                if (iditem.alignment != Globals.eAlignment.None)
                {
                    string aligncolor = "";
                    switch (iditem.alignment)
                    {
                        case Globals.eAlignment.Lawful:
                            aligncolor = "white";
                            break;
                        case Globals.eAlignment.Neutral:
                            aligncolor = "green";
                            break;
                        case Globals.eAlignment.Chaotic:
                            aligncolor = "purple";
                            break;
                        case Globals.eAlignment.ChaoticEvil:
                        case Globals.eAlignment.Evil:
                            aligncolor = "red";
                            break;
                        case Globals.eAlignment.Amoral:
                            aligncolor = "yellow";
                            break;
                        default:
                            break;
                    }
                    itmalign = " The " + iditem.name + " briefly pulses with a " + aligncolor + " glow.";
                }
                //item attuned
                if (iditem.attunedID != 0)
                {
                    if (iditem.attunedID > 0)
                    {
                        if (iditem.attunedID == caster.PlayerID)
                        {
                            itmattuned = " The " + iditem.name + " is soulbound to you.";
                        }
                        else
                        {
                            itmattuned = " The " + iditem.name + " is soulbound to " + PC.GetName(iditem.attunedID) + ".";
                        }
                    }
                    else
                    {
                        itmattuned = " The " + iditem.name + " is soulbound to another being.";
                    }
                }
                //iditem.identified[iditem.identified.Length - 1] = caster.playerID;

                caster.WriteToDisplay("You are looking at " + iditem.longDesc + "." + itmeffect + itmenchantment + itmspecial + itmalign + itmattuned);

                if (iditem.venom > 0) { caster.WriteToDisplay("The " + iditem.name + " drips a caustic venom."); }

                return true;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return false;
            }
        }

        private bool castMakeRecall(Character caster, string args)
        {
            SendGenericCastMessage(caster, null, true);

            if (caster.RightHand != null && caster.RightHand.itemID == Item.ID_GOLDRING)
            {
                caster.UnequipRightHand(caster.RightHand);
                caster.EquipRightHand(Item.CopyItemFromDictionary(Item.ID_RECALLRING));
            }
            else if (caster.LeftHand != null && caster.LeftHand.itemID == Item.ID_GOLDRING)
            {
                caster.UnequipLeftHand(caster.LeftHand);
                caster.EquipLeftHand(Item.CopyItemFromDictionary(Item.ID_RECALLRING));
            }
            else
            {
                if (System.Configuration.ConfigurationManager.AppSettings["RequireMakeRecallReagent"].ToLower() == "false")
                {
                    Item recallRing = Item.CopyItemFromDictionary(Item.ID_RECALLRING);
                    recallRing.coinValue = 0; // to avoid exploitation
                    caster.EquipEitherHandOrDrop(recallRing);
                }
                else
                {
                    if (caster.RightHand != null)
                    {
                        caster.WriteToDisplay("Your " + caster.RightHand.name + " explodes!");
                        caster.RightHand = null;
                        Combat.DoSpellDamage(null, caster, null, Rules.dice.Next(1, 20), "concussion");
                    }
                    else if (caster.LeftHand != null)
                    {
                        caster.WriteToDisplay("Your " + caster.LeftHand.name + " explodes!");
                        caster.LeftHand = null;
                        Combat.DoSpellDamage(null, caster, null, Rules.dice.Next(1, 20), "concussion");
                    }
                    else
                    {
                        //GenericFailMessage(caster, "");
                        return false;
                    }
                }
            }
            return true;
        }

        private bool castNightVision(Character caster, string args)
        {
            // currently thieves may only enchant themselves with night vision

            SendGenericEnchantMessage(caster);

            Effect.CreateCharacterEffect(Effect.EffectType.Night_Vision, 1, caster, Skills.GetSkillLevel(caster.magic) * 20, caster);

            return true;
        }

        private bool castSpeed(Character caster, string args)
        {
            // currently thieves may only enchant themselves with speed, 9 seconds per skill level
            SendGenericEnchantMessage(caster);
            Effect.CreateCharacterEffect(Effect.EffectType.Speed, Skills.GetSkillLevel(caster.magic), caster, Utils.TimeSpanToRounds(new TimeSpan(0, 0, 9 * Skills.GetSkillLevel(caster.magic))), caster);
            return true;
        }

        private bool castVenom(Character caster, string args)
        {
            // currently thieves may only enchant themselves with venom
            caster.WriteToDisplay("You cast Venom.");   
            if (caster.RightHand != null && caster.RightHand.special.Contains("pierce"))
            {
                caster.WriteToDisplay("Your " + caster.RightHand.name + " drips a caustic venom!");
                caster.RightHand.venom = Convert.ToInt32(Skills.GetSkillLevel(caster.magic) * 1.5);
                caster.RightHand.venTime();
            }
            else if (caster.LeftHand != null && caster.LeftHand.special.Contains("pierce"))
            {
                caster.WriteToDisplay("Your " + caster.LeftHand.name + " drips a caustic venom!");
                caster.LeftHand.venom = Convert.ToInt32(Skills.GetSkillLevel(caster.magic) * 1.5);
                caster.LeftHand.venTime();
            }
            else
            {
                caster.WriteToDisplay("Your spell fails.");
                caster.WriteToDisplay("You can only cast venom on pierce weapons.");
                caster.WriteToDisplay("Dagger,Shuriken,Rapier,Spear,Bow,Crossbow.");
            }
            return true;
        }
        #endregion

        #region Ravager Spells
        private bool castFlameShield(Character caster, string args)
        {
            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = FindAndConfirmTarget(caster, sArgs[sArgs.Length - 1]);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);

            SendGenericEnchantMessage(target);

            int effectAmount = Skills.GetSkillLevel(caster.magic); // magic skill users and scroll/item users (spellPower attribute on Items)
            if (caster.IsHybrid && caster.Level > effectAmount) effectAmount = caster.Level; // Ravagers use their level, if it's greater than the item level...

            Effect.CreateCharacterEffect(Effect.EffectType.FlameShield, effectAmount, target, caster.Level * 8, caster);
            return true;
        }

        private bool castMinorProtectFire(Character caster, string args)
        {
            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = FindAndConfirmTarget(caster, sArgs[sArgs.Length - 1]);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);

            SendGenericEnchantMessage(target);

            int amount = 0;
            int duration = 0;

            int skillLevel = Skills.GetSkillLevel(caster.magic);

            if (skillLevel == 0)
            {
                amount = Convert.ToInt32(caster.Level * 2.5);
                duration = caster.Level * 20;
            }
            else
            {
                amount = Convert.ToInt32(caster.Level * 2.5);
                duration = skillLevel * 20;
            }

            if (amount > 15) { amount = 15; }

            Effect.CreateCharacterEffect(Effect.EffectType.Protection_from_Fire, amount, target, duration, caster);

            return true;
        }
        #endregion

        private bool castLifeleech(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            this.SendGenericCastMessage(caster, target, true);

            // only sorcerers can lifeleech vs. undead
            if (target.IsUndead)// && caster.BaseProfession != Character.ClassType.Sorcerer)
            {
                caster.WriteToDisplay("Your " + this.Name + " has no affect on the undead.");
                return true;
            }

            int damageLevel = caster.Level;
            if (!caster.IsHybrid) damageLevel = Skills.GetSkillLevel(caster.magic);

            if (Combat.DoSpellDamage(caster, target, null, Convert.ToInt32(damageLevel * ((caster is PC) ? CURSE_SPELL_MULTIPLICAND_PC : CURSE_SPELL_MULTIPLICAND_NPC)) + Rules.dice.Next(-5, 5), this.Name) == 1)
            {
                Rules.GiveKillExp(caster, target);
                Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
            }
            return true;
        }

        #region Sorcerer Spells
        private bool castAcidOrb(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }
            if (target == caster) { caster.WriteToDisplay("You cannot cast " + this.Name + " at yourself."); return false; }

            this.SendGenericCastMessage(caster, target, false);            

            int numOrbs = 1;

            if (Skills.GetSkillLevel(caster.magic) > 4 && caster.Mana >= this.ManaCost * 2) numOrbs++; // 2nd orb at magic skill level 5
            if (Skills.GetSkillLevel(caster.magic) > 9 && caster.Mana >= this.ManaCost * 3) numOrbs++; // 3rd orb at magic skill level 10
            if (Skills.GetSkillLevel(caster.magic) > 14 && caster.Mana >= this.ManaCost * 4) numOrbs++; // 4th orb at magic skill level 15
            if (Skills.GetSkillLevel(caster.magic) > 18 && caster.Mana >= this.ManaCost * 5) numOrbs++; // 5th orb at magic skill level 19 (max)

            // add an orb for magic intensity
            if (caster.Map.HasRandomMagicIntensity && Rules.RollD(1, 100) >= 50)
                numOrbs++;

            int attackRoll = 0; //  not currently used

            while (numOrbs > 0)
            {
                caster.EmitSound(this.SoundFile);

                int toHit = Combat.DND_RollToHit(caster, target, this, ref attackRoll); // 0 is miss, 1 is hit, 2 is critical
                short multiplier = 3;

                if (toHit > 0)
                {
                    if (toHit == 2)
                    {
                        multiplier = 4;
                        caster.WriteToDisplay("Your " + this.Name + " does critical damage!");
                    }

                    if (Combat.DoSpellDamage(caster, target, null, (Skills.GetSkillLevel(caster.magic) * multiplier) + GetSpellDamageModifier(caster), this.Name) == 1)
                    {
                        Rules.GiveKillExp(caster, target);
                        Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
                        //Skills.GiveSkillExp(caster, Globals.eSkillType.Shuriken, (this.ManaCost * caster.numAttackers));
                        return true; // target is dead, break out of here
                    }
                    else
                    {
                        // magic skill is earned regardless
                        Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
                        // some shuriken, or throwing item skill is earned as well
                        //Skills.GiveSkillExp(caster, Globals.eSkillType.Shuriken, (this.ManaCost * caster.numAttackers));
                    }
                }
                else
                {
                    if (target is NPC && (target as NPC).longDesc.Length > 0)
                        caster.WriteToDisplay("Your " + this.Name + " misses " + (target as NPC).longDesc + ".");
                    else caster.WriteToDisplay("Your " + this.Name + " misses " + target.Name + ".");
                    target.WriteToDisplay(caster.Name + " misses you with their " + this.Name + "!");
                }

                if (caster.Mana < this.ManaCost) return true; // caster cannot cast any more orbs if no mana left
                else if (numOrbs > 1) caster.Mana -= this.ManaCost; // reduce mana for each orb past the first (first orb mana is reduced before this method is called)

                numOrbs--;
            }
            return true;
        }

        private bool castAcidRain(Character caster, string args)
        {
            SendGenericCastMessage(caster, null, true);
            CastGenericAreaSpell(caster, args, Effect.EffectType.Acid, (Skills.GetSkillLevel(caster.magic) * 4) + GetSpellDamageModifier(caster), this.Name);
            return true;
        }

        private bool castCharmAnimal(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            // auto spell failure on PC targets and non animal
            if (target.IsPC || !target.animal)
            {
                caster.WriteToDisplay("The " + this.Name + " only works on beings with animal intelligence.");
                return false;
            }

            // a charmed animal counts as a pet and a check is made of how many pets a player has
            if (caster.Pets.Count >= MAX_PETS)
            {
                caster.WriteToDisplay("You do not have the ability to control anymore pets.");
                return false;
            }

            SendGenericCastMessage(caster, target, true);

            // some animals cannot be charmed
            if ((target is NPC))
            {
                if ((target as NPC).lairCritter)
                {
                    caster.WriteToDisplay("You fail to charm the " + target.Name + ".");
                    return true;
                }

                // specifically for animals that are not lair creatures, such as Smokey
                switch ((target as NPC).species)
                {
                    case Globals.eSpecies.Smokey:
                        caster.WriteToDisplay("You fail to charm the " + target.Name + ".");
                        return true;
                    default:
                        break;
                }
            }

            // target is NPC, not a lair critter, and saving throw is failed (charisma modifier) then charm
            if (!Combat.DND_CheckSavingThrow(target, Combat.SavingThrow.Spell, Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Charisma - Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Charisma))))
            {
                // animal is charmed for 5 minutes per skill level
                Effect.CreateCharacterEffect(Effect.EffectType.Charm_Animal, 1, target, Utils.TimeSpanToRounds(new TimeSpan(0, 5 * Skills.GetSkillLevel(caster.magic), 0)), caster);
            }
            else caster.WriteToDisplay("You fail to charm the " + target.Name + ".");

            return true;
        }

        private bool castCommandUndead(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            if (target == null) { return false; }

            // auto spell failure on PC targets and non animal
            if (target.IsPC || !target.IsUndead)
            {
                caster.WriteToDisplay("The " + this.Name + " only works on the undead.");
                return false;
            }

            // a commanded undead counts as a pet and a check is made of how many pets a player has
            if (caster.Pets.Count >= MAX_PETS)
            {
                caster.WriteToDisplay("You do not have the ability to control anymore pets.");
                return false;
            }

            SendGenericCastMessage(caster, target, true);

            int savingThrowMod = Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Charisma) - Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Charisma);

            // target is NPC, not a lair critter, and saving throw is failed then command
            if ((target is NPC) && !(target as NPC).lairCritter &&
                !Combat.DND_CheckSavingThrow(target, Combat.SavingThrow.Spell, Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Charisma) - Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Charisma)))
            {
                // undead is commanded for 7 minutes per skill level
                Effect.CreateCharacterEffect(Effect.EffectType.Command_Undead, 1, target, Utils.TimeSpanToRounds(new TimeSpan(0, 7 * Skills.GetSkillLevel(caster.magic), 0)), caster);
            }
            else caster.WriteToDisplay("You fail to command the " + target.Name + ".");

            return true;
        }

        private bool castDismissUndead(Character caster, string args)
        {
            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = FindAndConfirmTarget(caster, sArgs[sArgs.Length - 1]);

            if (target == null) { return false; }

            if (!target.IsUndead)
            {
                caster.WriteToDisplay("The " + this.Name + " spell only works on the undead.");
                return false;
            }

            this.SendGenericCastMessage(caster, target, true);

            int totalDamage = (Skills.GetSkillLevel(caster.magic) * ((caster.IsPC ? DEATH_SPELL_MULTIPLICAND_PC : DEATH_SPELL_MULTIPLICAND_NPC) + 1)) + GetSpellDamageModifier(caster);

            totalDamage += Rules.RollD(1, 2) == 1 ? Rules.RollD(1, 4) : -(Rules.RollD(1, 4));

            if (Combat.DoSpellDamage(caster, target, null, totalDamage, this.name.ToLower()) == 1)
            {
                Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
                Rules.GiveKillExp(caster, target);
            }

            return true;
        }

        private bool castHaltUndead(Character caster, string args)
        {
            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = FindAndConfirmTarget(caster, sArgs[sArgs.Length - 1]);

            if (target == null) { return false; }

            this.SendGenericCastMessage(caster, target, true);

            if (!target.IsUndead)
            {
                caster.WriteToDisplay("Your " + this.Name + " has no affect on the living.");
                return true;
            }

            if (Combat.DoSpellDamage(caster, target, null, 0, "stun") == 1)
            {
                target.Stunned += Convert.ToInt16(Skills.GetSkillLevel(caster.magic) * 2);

                if (target.race != "")
                {
                    caster.WriteToDisplay(target.Name + " is paralyzed.");
                }
                else
                {
                    caster.WriteToDisplay("The " + target.Name + " is paralyzed.");
                }
            }

            return true;
        }

        private bool castImage(Character caster, string args)
        {
            try
            {
                args = args.Replace(this.SpellCommand + " " + caster.Name, "");

                Cell cell = null;

                if (args.Length > 0)
                {
                    cell = Map.GetCellRelevantToCell(caster.CurrentCell, args, false);
                    caster.WriteToDisplay("args = " + args);
                }

                int skillLevel = Skills.GetSkillLevel(caster.magic);

                if (cell != null && args != null && args.Length > 0 && skillLevel < IMAGE_CAST_DIRECTION_SKILL_REQUIREMENT)
                {
                    cell = null;
                    caster.WriteToDisplay("You are not skilled enough to conjure an image in a direction.");
                }

                if (caster.EffectsList.ContainsKey(Effect.EffectType.Image))
                {
                    caster.WriteToDisplay("You are already projecting your image.");
                    return false;
                }

                int numImages = 1; // Convert.ToInt32(Math.Round(skillLevel / 2d, MidpointRounding.AwayFromZero));

                PC image = null;

                SendGenericCastMessage(caster, null, false);

                while (numImages > 0)
                {
                    image = DAL.DBPlayer.GetPCByID(caster.PlayerID);
                    image.beltList.Clear();
                    image.sackList.Clear();
                    image.SackGold = 0;
                    image.Name = Spell.IMAGE_IDENTIFIER + caster.Name; // for displaying to players only
                    image.IsPC = false; // this prevents saving to the database
                    if (cell != null)
                        image.CurrentCell = cell;
                    else image.CurrentCell = caster.CurrentCell; // image will appear where the spell was cast
                    image.Hits = skillLevel * (Rules.RollD(3, 8));
                    // skillLevel * Rules.RollD(3, 4)
                    Effect.CreateCharacterEffect(Effect.EffectType.Image, 1, image, 4, caster); // when this effect is removed the image is removed from the world
                    Effect.CreateCharacterEffect(Effect.EffectType.Image, 1, caster, image.EffectsList[Effect.EffectType.Image].duration, caster);
                    PC.SetCharacterVisualKey(image); // visual image for yuusha and other clients...

                    image.EmitSound(this.SoundFile);

                    numImages--; // iterate
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }

            return true;
        }

        private bool castPowerWordSilence(Character caster, string args)
        {
            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = FindAndConfirmTarget(caster, sArgs[sArgs.Length - 1]);

            if (target == null) { return false; }

            // no saving throw

            int magicSkillLevel = Skills.GetSkillLevel(caster.magic);

            int duration = 10; // start at 10 rounds

            if(magicSkillLevel > target.Level) // higher skill level than target level adds rounds
                duration += magicSkillLevel - target.Level;
            else if(magicSkillLevel < target.Level) // lower magic skill level decreases rounds by half per lower level
                duration -= Convert.ToInt32((target.Level - magicSkillLevel) * .5);

            if(duration < 10) duration = 5; // will always do at least 5 rounds of silence

            SendGenericCastMessage(caster, target, true);

            // no saving throw...
            Effect.CreateCharacterEffect(Effect.EffectType.Silence, 1, target, duration, caster);

            return true;
        }

        /// <summary>
        /// Boosts armor class, improves saving throw and adds protection against spell damage versus all undead targets.
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool castProtectUndead(Character caster, string args)
        {
            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = FindAndConfirmTarget(caster, sArgs[sArgs.Length - 1]);

            if (target == null) { return false; }

            SendGenericCastMessage(caster, target, true);

            SendGenericEnchantMessage(target);

            Effect.CreateCharacterEffect(Effect.EffectType.Protection_from_Undead, 2, target, Skills.GetSkillLevel(caster.magic) * 20, caster);
            return true;
        }

        private bool castContagion(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            SendGenericCastMessage(caster, target, true);

            if (!Combat.DND_CheckSavingThrow(target, Combat.SavingThrow.Spell, 0))
            {
                Effect.CreateCharacterEffect(Effect.EffectType.Contagion, 1, target, Utils.TimeSpanToRounds(new TimeSpan(0, 20 * Skills.GetSkillLevel(caster.magic), 0)), caster);
            }
            else SendGenericResistMessages(caster, target);
            return true;
        }

        private bool castAnimateDead(Character caster, string args)
        {
            #region Determine number of pets.
            int petCount = 0;
            foreach (NPC pet in caster.Pets)
            {
                if (pet.QuestList.Count == 0)
                {
                    petCount++;
                }
            }

            if (petCount >= MAX_PETS)
            {
                caster.WriteToDisplay("You may only control " + MAX_PETS + " pets.");
                return false;
            }
            #endregion

            try
            {
                Corpse corpseWithGem = null;
                Item gemComponent = null;

                #region Find corpse in cell with inserted material component.
                foreach (Item item in caster.CurrentCell.Items)
                {
                    if (item is Corpse)
                    {
                        foreach (Item placeable in (item as Corpse).Contents)
                        {
                            if (placeable.itemID == Item.ID_UNCUTPAINITE)
                            {
                                corpseWithGem = item as Corpse;
                                gemComponent = placeable;
                                break;
                            }
                        }
                    }

                    if (corpseWithGem != null)
                        break;
                }
                #endregion

                if (corpseWithGem == null)
                {
                    caster.WriteToDisplay("You must first prepare a corpse before you cast " + this.Name + ".");
                    return false; // spell failure
                }
                else if (corpseWithGem.IsPlayerCorpse)
                {
                    // catch a player corpse being animated (should not have been able to place the material component in the corpse)
                    caster.WriteToDisplay("Player corpses cannot currently be animated.");
                    if (corpseWithGem.Contents.Contains(gemComponent))
                        corpseWithGem.Contents.Remove(gemComponent);
                    return false;
                }
                else
                {
                    //NPC animated = NPC.CreateNPC((corpseWithGem.Ghost as NPC).npcID, caster.FacetID, caster.LandID, caster.MapID, caster.X, caster.Y, caster.Z);
                    //TODO Make static method in Corpse class to return animated dead NPC.
                    NPC animated = corpseWithGem.Ghost as NPC;

                    if (animated == null)
                    {
                        caster.WriteToDisplay("There was a problem with your " + this.Name + " spell. Please report this to the developers.");
                        return false;
                    }

                    if (corpseWithGem.Contents.Contains(gemComponent))
                        corpseWithGem.Contents.Remove(gemComponent);

                    if (animated.Level > caster.Level || animated.lairCritter || animated.species == Globals.eSpecies.Smokey)
                    {
                        caster.WriteToDisplay("You are unable to animate the corpse properly.");
                        //TODO destroy corpse here?
                        return false;
                    }

                    // get minutes to corpse decay, if over half way to decay then make skeleton otherwise make zombie
                    bool isZombie = false;
                    int remainingRoundsUntilDecomposition = World.NPCCorpseDecayTimer - (DragonsSpineMain.GameRound - corpseWithGem.dropRound);
                    if (remainingRoundsUntilDecomposition >= (World.NPCCorpseDecayTimer / 2))
                        isZombie = true;

                    animated.CurrentCell = caster.CurrentCell;

                    animated = Corpse.BecomeUndead(corpseWithGem, isZombie ? Corpse.AnimateCorpseType.Zombie : Corpse.AnimateCorpseType.Skeleton);
                    
                    animated.Alignment = caster.Alignment;
                    //TODO message that the corpse has been animated sent to all
                    animated.canCommand = true;

                    animated.PetOwner = caster;
                    caster.Pets.Add(animated);

                    animated.AddToWorld();
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }

            return true;
        }

        private bool castHealServant(Character caster, string args)
        {
            Character target = FindAndConfirmTarget(caster, args);

            // fail to heal a target that is not a pet
            if (!(target is NPC) || !caster.Pets.Contains(target as NPC))
            {
                caster.WriteToDisplay("The target of your " + this.Name + " spell is not connected to you.");
                return false;
            }

            // perhaps we should allow the caster to kill themselves by channeling lifeforce?
            if (caster.HitsFull < 12)
            {
                caster.WriteToDisplay("You are too weak to channel lifeforce to your servant.");
                return false;
            }

            int healAmount = caster.HitsFull / 2;
            int lifeforceChannelled = healAmount;

            // halve the heal amount if diseased
            if (target.EffectsList.ContainsKey(Effect.EffectType.Contagion))
                healAmount = healAmount / 2;

            // 0 means the caster survived the lifeforce channel
            if (Combat.DoSpellDamage(caster, caster, null, lifeforceChannelled, "lifeforce drain") == 0)
            {
                target.Hits += healAmount;

                if (target.Hits > target.HitsFull) { target.Hits = target.HitsFull; }

                SendGenericCastMessage(caster, target, true);

                caster.WriteToDisplay("You channel your lifeforce to aid your servant.");
                target.WriteToDisplay("You have been healed.");
            }

            return true;
        }
        #endregion

        private void SendGenericCastMessage(Character caster, Character target, bool emitSound)
        {
            if (target == null) caster.WriteToDisplay("You cast " + this.Name + ".");
            else if (target != caster) caster.WriteToDisplay("You cast " + this.Name + " at " + (target.race != "" ? target.Name : "the " + target.Name) + ".");

            if (emitSound) caster.EmitSound(this.SoundFile);
        }

        private void SendGenericEnchantMessage(Character target)
        {
            target.WriteToDisplay("You have been enchanted with " + this.Name + "!");
            target.EmitSound(this.SoundFile);
        }

        private void SendGenericResistMessages(Character caster, Character target)
        {
            caster.WriteToDisplay((target.race != "" ? target.Name : "The " + target.Name) + " resists your " + this.Name + " spell!");
            target.WriteToDisplay("You resist a " + this.Name + " spell!");
        }
    }
}
