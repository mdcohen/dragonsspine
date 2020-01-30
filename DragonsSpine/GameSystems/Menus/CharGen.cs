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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cell = DragonsSpine.GameWorld.Cell;

namespace DragonsSpine
{

    public class CharGen
    {
        public static ArrayList Newbies = new ArrayList();

        public static bool LoadNewbies()
        {
            try
            {
                foreach (Globals.eHomeland homeland in Enum.GetValues(typeof(Globals.eHomeland)))
                {
                    foreach (Character.ClassType classType in Enum.GetValues(typeof(Character.ClassType)))
                    {
                        if (classType != Character.ClassType.None && classType != Character.ClassType.All &&
                            classType != Character.ClassType.Knight && classType != Character.ClassType.Ravager)
                        {
                            Character newbie = new Character
                            {
                                race = homeland.ToString(),
                                BaseProfession = classType
                            };
                            DAL.DBWorld.SetupNewCharacter(newbie);
                            Newbies.Add(newbie);
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return false;
            }
        }

        public static string NewChar()
        {
            string rtnStr;
            rtnStr = "Welcome to the character generator.";
            return rtnStr;
        }

        // List of available races
        public static string PickRace()
        {
            string rtnStr;
            rtnStr = "\r\nRaces:\r\n";
            rtnStr = rtnStr + "  I  -  Illyria\r\n";
            rtnStr = rtnStr + "  M  -  Mu\r\n";
            rtnStr = rtnStr + "  L  -  Lemuria\r\n";
            rtnStr = rtnStr + "  LG -  Leng\r\n";
            rtnStr = rtnStr + "  D  -  Draznia\r\n";
            rtnStr = rtnStr + "  H  -  Hovath\r\n";
            rtnStr = rtnStr + "  MN -  Mnar\r\n";
            rtnStr = rtnStr + "  B  -  Barbarian\r\n";
            rtnStr = rtnStr + "\r\nPlease Select a Race: ";

            return rtnStr;
        }
        // List of available genders
        public static string PickGender()
        {
            string rtnStr;
            rtnStr = "\r\nGender:\r\n";
            rtnStr = rtnStr + "  1 - Male\r\n";
            rtnStr = rtnStr + "  2 - Female\r\n";
            rtnStr = rtnStr + "\r\nPlease Select a Gender: ";
            return rtnStr;
        }


        //roll up stats
        public static string RollStats(Character ch)
        {
            string rtnStr = "";
            string manaStr = "\r\n";

            ch.CurrentCell = Cell.GetCell(0, 0, 0, 41, 34, 0);

            if (ch.Level != 3) { ch.Level = 3; } // set level to 3

            ch.Strength = AdjustMinMaxStat(RollStat() + GetRacialBonus(ch, Globals.eAbilityStat.Strength));
            ch.Dexterity = AdjustMinMaxStat(RollStat() + GetRacialBonus(ch, Globals.eAbilityStat.Dexterity));
            ch.Intelligence = AdjustMinMaxStat(RollStat() + GetRacialBonus(ch, Globals.eAbilityStat.Intelligence));
            ch.Wisdom = AdjustMinMaxStat(RollStat() + GetRacialBonus(ch, Globals.eAbilityStat.Wisdom));
            ch.Constitution = AdjustMinMaxStat(RollStat() + GetRacialBonus(ch, Globals.eAbilityStat.Constitution));
            ch.Charisma = AdjustMinMaxStat(RollStat() + GetRacialBonus(ch, Globals.eAbilityStat.Charisma));

            if (ch.Strength >= 16)
                ch.strengthAdd = 1;
            else ch.strengthAdd = 0;

            if (ch.Dexterity >= 16)
                ch.dexterityAdd = 1;
            else ch.dexterityAdd = 0;

            ch.HitsMax = Rules.GetHitsGain(ch, ch.Level) + (int)(ch.Land.StatCapOperand / 1.5);
            ch.StaminaMax = Rules.GetStaminaGain(ch, 1) + (int)(ch.Land.StatCapOperand / 8);
            ch.ManaMax = 0;
            if (ch.IsSpellUser)
            {
                ch.ManaMax = Rules.GetManaGain(ch, 1) + (int)(ch.Land.StatCapOperand / 8);
                manaStr = "Mana:    " + ch.ManaMax.ToString().PadLeft(2) + "\r\n";
            }

            if (ch.HitsMax > Rules.GetMaximumHits(ch))
                ch.HitsMax = Rules.GetMaximumHits(ch);

            //if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
            //{
            //    return ProtocolYuusha.CHARGEN_ROLLER_RESULTS + ProtocolYuusha.FormatCharGenRollerResults(ch) + ProtocolYuusha.CHARGEN_ROLLER_RESULTS_END;
            //}

            rtnStr = rtnStr + "\r\nCharacter Stats:\r\n";
            rtnStr = rtnStr + "  Strength:     " + ch.Strength.ToString().PadLeft(2).PadRight(10) + "Adds:     " + ch.strengthAdd + "\r\n";
            rtnStr = rtnStr + "  Dexterity:    " + ch.Dexterity.ToString().PadLeft(2).PadRight(10) + "Adds:     " + ch.dexterityAdd + "\r\n";
            rtnStr = rtnStr + "  Intelligence: " + ch.Intelligence.ToString().PadLeft(2) + "\r\n";
            rtnStr = rtnStr + "  Wisdom:       " + ch.Wisdom.ToString().PadLeft(2).PadRight(10) + "Hits:    " + ch.HitsMax.ToString().PadLeft(2) + "\r\n";
            rtnStr = rtnStr + "  Constitution: " + ch.Constitution.ToString().PadLeft(2).PadRight(10) + "Stamina: " + ch.StaminaMax.ToString().PadLeft(2) + "\r\n";
            rtnStr = rtnStr + "  Charisma:     " + ch.Charisma.ToString().PadLeft(2).PadRight(10) + manaStr;
            rtnStr = rtnStr + "\r\nRoll Again? (y,n): ";

            return rtnStr;
        }

        public static string PickClass()
        {
            string rtnStr = "";

            rtnStr = rtnStr + "\r\nPlease select a character class:\r\n";
            rtnStr = rtnStr + "  FI  -  Fighter\r\n";
            rtnStr = rtnStr + "  TH  -  Thaumaturge\r\n";
            rtnStr = rtnStr + "  WI  -  Wizard\r\n";
            rtnStr = rtnStr + "  MA  -  Martial Artist\r\n";
            rtnStr = rtnStr + "  TF  -  Thief (neutral)\r\n";
            rtnStr = rtnStr + "  SR  -  Sorcerer (evil)\r\n";
            rtnStr = rtnStr + "  DR  -  Druid (neutral)\r\n";
            rtnStr = rtnStr + "  RA  -  Ranger\r\n";
            rtnStr = rtnStr + "\r\nSelect a character class: ";

            return rtnStr;
        }

        public static bool VerifyRace(string race)
        {
            switch(race.ToLower())
            {
                case "i":
                    return true;
                case "m":
                    return true;
                case "l":
                    return true;
                case "lg":
                    return true;
                case "d":
                    return true;
                case "h":
                    return true;
                case "mn":
                    return true;
                case "b":
                    return true;
                default:
                    return false;
            }
        }

        public static bool VerifyGender(string gender)
        {
            if(gender == "1" || gender == "2")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool VerifyStatRerollOptions(string stat)
        {
            if(stat == "y" || stat == "n")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Character.ClassType VerifyClass(string choice)
        {
            switch(choice.ToLower())
            {
                case "berserker":
                case "br":
                    return Character.ClassType.Berserker;
                case "fighter":
                case "fi":
                    return Character.ClassType.Fighter;
                case "thaumaturge":
                case "th":
                    return Character.ClassType.Thaumaturge;
                case "wizard":
                case "wi":
                    return Character.ClassType.Wizard;
                case "martial artist":
                case "ma":
                    return Character.ClassType.Martial_Artist;
                case "thief":
                case "tf":
                    return Character.ClassType.Thief;
                case "sorcerer":
                case "sr":
                    return Character.ClassType.Sorcerer;
                case "druid":
                case "dr":
                    return Character.ClassType.Druid;
                case "ranger":
                case "ra":
                    return Character.ClassType.Ranger;
                default:
                    return Character.ClassType.None;
            }
        }

        public static bool CharacterNameDenied(Character chr, string name) 
        {
            bool deny = false;

            string[] specialnames = new string[]{"orc","gnoll","goblin","skeleton","kobold","dragon","drake","griffin","manticore",
                "banshee","demon","bear","boar","vampire","ydnac","lars","sven","oskar","olaf",
                "marlis","neela","phong","ironbar","vulcan","sheriff","rolf","troll","wyvern",
                "ydmos","tanner","crazy.derf","trambuskar","ianta","alia","priest","priestess","statue",
                "shidosha","pazuzu","asmodeus","damballa","glamdrang","samael","perdurabo",
                "thamuz","knight","martialartist","thief","thaum","thaumaturge","wizard","fighter",
                "thisson", PathTest.RESERVED_NAME_THROWNOBJECT};

            string[] silly = new string[]{"pvp","lol","haha","hehe","btw","atm","jeje","rofl","roflmao","lmao","lmfao","lmho","dragonsspine",
                "dragonspine","nobody","somebody","anybody","account","character"};

                string acceptable = "abcdefghijklmnopqrstuvwxyz.";

            foreach(Character ch in Character.AllCharList)
            {
                if(name.ToLower() == ch.Name.ToLower())
                {
                    deny = true;
                }
            }

            foreach(Character npc in Character.NPCInGameWorld)
            {
                if(name.ToLower() == npc.Name.ToLower())
                {
                    deny = true;
                }
            }
            foreach(string special in specialnames)
            {
                if(name.ToLower() == special)
                {
                    deny = true;
                }
            }
            foreach(string nasty in Conference.ProfanityArray)
            {
                if(name.ToLower().IndexOf(nasty) > -1)
                {
                    deny = true;
                }
            }
            foreach(string word in silly)
            {
                if(name.ToLower().IndexOf(word) > -1)
                {
                    deny = true;
                }
            }
            foreach(char charcheck in name.ToLower())
            {
                if(acceptable.IndexOf(charcheck) == -1)
                {
                    deny = true;
                }
            }
            if (name.Length > GameSystems.Text.NameGenerator.NAME_MAX_LENGTH || name.Length < GameSystems.Text.NameGenerator.NAME_MIN_LENGTH)
            {
                string adjective = "";
                if (name.Length > GameSystems.Text.NameGenerator.NAME_MAX_LENGTH)
                {
                    adjective = "long";
                }
                else if (name.Length < GameSystems.Text.NameGenerator.NAME_MIN_LENGTH)
                {
                    adjective = "short";
                }

                chr.WriteLine("The name you have chosen is too " + adjective + ". Character names must be between " + GameSystems.Text.NameGenerator.NAME_MIN_LENGTH + " and " + GameSystems.Text.NameGenerator.NAME_MAX_LENGTH + " letters in length.", ProtocolYuusha.TextType.Error);

                return true;
            }
            else if (deny)
            {
                chr.WriteLine("The name you have chosen is not acceptable.", ProtocolYuusha.TextType.Error);
                return deny;
            }
            else
            {
                if (DAL.DBPlayer.PlayerExists(name))
                {
                    chr.WriteLine("A character with the name you have chosen already exists.", ProtocolYuusha.TextType.Error);
                    return true;
                }
            }

            return deny;
        }

        public static bool PasswordDenied(string password) 
        {
            if(password.Length > Account.PASSWORD_MAX_LENGTH || password.Length < Account.PASSWORD_MIN_LENGTH)
               return true;

            return false;
        }

        public static bool PasswordVerified(PC ch, string vPass) 
        {
            string shavpass = Utils.GetSHA(vPass);

            if(ch.Account.password.Equals(shavpass))
                return true;

            return false;
        }

        public static void SetupNewCharacter(PC ch)
        {
            DAL.DBWorld.SetupNewCharacter(ch);

            #region Set High Skills
            ch.highMace = ch.mace; // high skills
            ch.highBow = ch.bow;
            ch.highFlail = ch.flail;
            ch.highDagger = ch.dagger;
            ch.highRapier = ch.rapier;
            ch.highTwoHanded = ch.twoHanded;
            ch.highStaff = ch.staff;
            ch.highShuriken = ch.shuriken;
            ch.highSword = ch.sword;
            ch.highThreestaff = ch.threestaff;
            ch.highHalberd = ch.halberd;
            ch.highUnarmed = ch.unarmed;
            ch.highThievery = ch.thievery;
            ch.highMagic = ch.magic; 
            #endregion            

            #region 1 Gold Coin, or Sorcerers get a random amount between 250 - 300
            Item coin = Item.CopyItemFromDictionary(Item.ID_COINS); // most new characters get a single shiny coin
            coin.coinValue = 1;
            // sorcerer starts with more gold
            if (ch.BaseProfession == Character.ClassType.Sorcerer) coin.coinValue = Rules.Dice.Next(250, 300);
            ch.SackItem(coin); 
            #endregion

            #region Set Starting Location
            switch (ch.BaseProfession) // starting location in the game
            {
                case Character.ClassType.Thief:
                    ch.LandID = 0;
                    ch.MapID = 0;
                    ch.X = 51;
                    ch.Y = 26;
                    ch.Z = 0;
                    ch.CurrentCell = Cell.GetCell(0, 0, 0, 51, 26, 0);
                    break;
                case Character.ClassType.Sorcerer:
                    ch.LandID = 0;
                    ch.MapID = 0;
                    ch.X = 139;
                    ch.Y = 46;
                    ch.Z = 0;
                    ch.CurrentCell = Cell.GetCell(0, 0, 0, 139, 46, 0);
                    break;
                default: // Kesmai dock
                    ch.LandID = 0;
                    ch.MapID = 0;
                    ch.X = 41;
                    ch.Y = 33;
                    ch.Z = 0;
                    ch.CurrentCell = Cell.GetCell(0, 0, 0, 41, 33, 0);
                    break;
            } 
            #endregion

            ch.Experience = 1600; // set character experience and level
			ch.Level = 3;

            ch.Hits = ch.HitsMax; // set stats to max
            ch.Stamina = ch.StaminaMax;
            ch.Mana = ch.ManaMax;

            ch.dirPointer = "^";

            ch.Save(); // save

            // player ID was created upon save
            ch.UniqueID = PC.GetPlayerID(ch.Name);

            #region New Spellbook
            if (Array.IndexOf(GameWorld.World.SpellWarmingProfessions, ch.BaseProfession) > -1)
            {
                ch.SackItem(Item.CopyItemFromDictionary(Item.ID_SPELLBOOK));
            }
            #endregion

            // create a new mailbox for the new player, as their player ID was assigned suring the call to Save()
            ch.Mailbox = new Mail.GameMail.GameMailbox(ch.UniqueID);

            //ch = (Character)newpc; // cast the newpc back into a Character
            Menu.PrintMainMenu(ch as PC); // back to the menu
			ch.PCState = Globals.ePlayerState.MAINMENU;
        }

        private static int DND_rollStat(int dice)
        {
            int statRoll1 = 0;
            int statRoll2 = 0;

            switch (dice)
            {
                case 3:
                    statRoll1 = Rules.RollD(3, 6);
                    statRoll2 = Rules.RollD(3, 6);
                    break;
                case 4:
                    for (int a = 0; a <= 1; a++)
                    {
                        int[] rolls = new int[4];
                        for (int b = 0; b < rolls.Length; b++)
                        {
                            rolls[b] = Rules.RollD(1, 6);
                        }
                        Array.Sort(rolls);
                        if (a == 0)
                        {
                            statRoll1 = rolls[0] + rolls[1] + rolls[2];
                        }
                        else
                        {
                            statRoll2 = rolls[0] + rolls[1] + rolls[2];
                        }
                    }
                    break;
            }

            if (statRoll1 > statRoll2)
            {
                return statRoll1;
            }
            else
            {
                return statRoll2;
            }	
        }

        public static int RollStat()
        {
            int strRoll1;
            int strRoll2;

            strRoll1 = Rules.Dice.Next(3, 21);
            strRoll2 = Rules.Dice.Next(3, 21);

            return Math.Max(strRoll1, strRoll2);
        }

        public static int AdjustMinMaxStat(int stat)
        {
            if (stat < 3) stat = 3;
            else if (stat > 18) stat = 18;
            return stat;
        }

        public static int GetRacialBonus(Character ch, Globals.eAbilityStat stat)
        {
            switch (ch.race)
            {
                case "Illyria":
                    if (stat == Globals.eAbilityStat.Wisdom || stat == Globals.eAbilityStat.Constitution)
                        return 1;
                    else if (stat == Globals.eAbilityStat.Intelligence)
                        return -1;
                    return 0;
                case "Mu":
                    if (stat == Globals.eAbilityStat.Strength)
                        return 2;
                    else if (stat == Globals.eAbilityStat.Constitution)
                        return 1;
                    else if (stat == Globals.eAbilityStat.Intelligence || stat == Globals.eAbilityStat.Wisdom)
                        return -1;
                    return 0;
                case "Lemuria":
                    if (stat == Globals.eAbilityStat.Dexterity || stat == Globals.eAbilityStat.Charisma)
                        return 1;
                    else if (stat == Globals.eAbilityStat.Wisdom || stat == Globals.eAbilityStat.Constitution)
                        return -1;
                    return 0;
                case "Leng":
                    if (stat == Globals.eAbilityStat.Dexterity)
                        return 2;
                    else if (stat == Globals.eAbilityStat.Constitution)
                        return 1;
                    else if (stat == Globals.eAbilityStat.Intelligence)
                        return -1;
                    else if (stat == Globals.eAbilityStat.Charisma)
                        return -2;
                    return 0;
                case "Draznia":
                    if (stat == Globals.eAbilityStat.Dexterity)
                        return 1;
                    else if (stat == Globals.eAbilityStat.Intelligence)
                        return 2;
                    else if (stat == Globals.eAbilityStat.Constitution)
                        return -2;
                    return 0;
                case "Hovath":
                    if (stat == Globals.eAbilityStat.Wisdom)
                        return 2;
                    else if (stat == Globals.eAbilityStat.Intelligence)
                        return 1;
                    else if (stat == Globals.eAbilityStat.Charisma)
                        return -1;
                    return 0;
                case "Mnar":
                    if (stat == Globals.eAbilityStat.Strength)
                        return 2;
                    else if (stat == Globals.eAbilityStat.Intelligence)
                        return -1;
                    else if (stat == Globals.eAbilityStat.Charisma)
                        return 1;
                    return 0;
                case "Barbarian":
                    if (stat == Globals.eAbilityStat.Strength || stat == Globals.eAbilityStat.Constitution)
                        return 2;
                    else if (stat == Globals.eAbilityStat.Wisdom || stat == Globals.eAbilityStat.Intelligence)
                        return -2;
                    else if (stat == Globals.eAbilityStat.Dexterity)
                        return -1;
                    return 0;
                default:
                    return 0;
            }
        }
    }
}
