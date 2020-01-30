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

namespace DragonsSpine.Commands
{
    [CommandAttribute("help", "Display detailed help information about the game.", (int)Globals.eImpLevel.USER,
        0, new string[] { "Typing only 'help' will display a list of arguments." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class HelpCommand : ICommandHandler
    {
        private string[] helpArguments = new string[] { "conference", "map", "movement", "combat", "merchants", "talk", "training", "formulas", "experience" };

        public bool OnCommand(Character chr, string args)
        {
            if (chr.PCState == Globals.ePlayerState.CONFERENCE)
            {
                #region Conference Room Help
                chr.WriteLine("", ProtocolYuusha.TextType.Help);
                chr.WriteLine("Conference Room Commands", ProtocolYuusha.TextType.Help);
                chr.WriteLine("------------------------", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /play - Enter the game.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /exit - Disconnect from " + DragonsSpineMain.Instance.Settings.ServerName + ".", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /list - List your current characters.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /switch # - Switch to character #.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /scores or /topten - Get player rankings.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("     /scores me - Your current score.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("     /scores <class> <amount>", ProtocolYuusha.TextType.Help);
                chr.WriteLine("     /scores <class>", ProtocolYuusha.TextType.Help);
                chr.WriteLine("     /scores <player>", ProtocolYuusha.TextType.Help);
                chr.WriteLine("	    /scores all <amount>", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /page - Toggle paging.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("     /page <name> - Page someone in the game.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /tell or /t - Toggle private tells.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("     /tell <name> <message> - Send a private tell.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /friend - View your friends list.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("     /friend <name> - Add or remove a player from your friends list.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /notify - Toggle friend notification.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /ignore - View your ignore list.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("     /ignore <name> - Add or remove a player from your ignore list.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /users - Shows everyone in the game.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /menu - Return to the main menu.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /afk - Toggle A.F.K. (Away From Keyboard).", ProtocolYuusha.TextType.Help);
                chr.WriteLine("     /afk <message> - Use personal A.F.K. message.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /anon - Toggle anonymous.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /filter - Toggle profanity filter.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /rename <new name> - Change your character's name.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /echo - Toggle command echo.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /macro - View your macros list.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("     /macro # <text> - Set # macro, where # macro is between 0 and " + Character.MAX_MACROS + ".", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /lottery - View current lottery jackpots.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /commands - View a full list of available game commands.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /displaycombatdamage - Toggle combat damage statistics.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("  /help - This help list.", ProtocolYuusha.TextType.Help);
                chr.WriteLine("", ProtocolYuusha.TextType.Help);
                if ((chr as PC).ImpLevel > Globals.eImpLevel.USER)
                {
                    chr.WriteLine("Staff Commands", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /stafftitle - Toggle staff title.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("", ProtocolYuusha.TextType.Help);
                }
                if ((chr as PC).ImpLevel >= Globals.eImpLevel.GM)
                {
                    chr.WriteLine("GM Commands", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /invis - Toggle invisibility.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /announce - Send announcement to all, anonymously.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /selfannounce - Send announcement to all, includes your name.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /immortal - Toggle immortality.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /ban <name> <# of days> - Ban a player (includes their account)", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /boot <name> - Disconnects a player from the server.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /rename <old> <new> - Change a player's name.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /restock - Restock all merchant inventory items.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /clearstores - Clears all non original merchant inventory items.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("", ProtocolYuusha.TextType.Help);
                }
                if ((chr as PC).ImpLevel >= Globals.eImpLevel.DEV)
                {
                    chr.WriteLine("DEV Commands", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /bootplayers - Force all players to quit.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /lockserver - Locks the server.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /unlockserver - Unlocks the server.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /implevel <name> <impLevel#> - Set a player's implevel.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /listf - Lists the Player table columns in the database.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /getf <name> <field name> - Get a player's field value.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /setf <name> <field name> <value> <notify> - Set a player's field value.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /processemptyworld <on|off> - No argument displays status of this attribute.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /purgeaccount <account> - Purge a players account.", ProtocolYuusha.TextType.Help);
                    //chr.WriteLine("  /searchnpcloot <itemID> - Search for an item on an NPC currently in the game.", Protocol.TextType.Help);
                    chr.WriteLine("  /getskill <name> <skill> - Get a PC's skill level.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /setskill <name> <skill> <skill level> - Set a PC's skill level.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /restartserver - Forces a hard shutdown, with no PC saves, and restarts the DragonsSpine.exe process.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("  /deleteplayer | /dplayer <name> - Delete a player from the database.", ProtocolYuusha.TextType.Help);
                    chr.WriteLine("", ProtocolYuusha.TextType.Help);
                }
                chr.WriteLine("", ProtocolYuusha.TextType.Help);
                #endregion
                return true;
            }

            if (args == null || args == "")
            {
                chr.WriteToDisplay("Help is in need of an update if we have any volunteers.");
                chr.WriteToDisplay("Help Topics: ( help <topic> )");
                chr.WriteToDisplay("merchants   talk        magic     training");
                chr.WriteToDisplay("map         movement    combat    formulas");
            }
            else
            {
                string[] sArgs = args.Split(" ".ToCharArray());

                switch (sArgs[0].ToLower())
                {
                    case "map":
                        chr.WriteToDisplay("The map consists of the following tiles:");
                        chr.WriteToDisplay("%% = Air          [_ = Ruins ");
                        chr.WriteToDisplay("~~ = Water        ** = Fire ");
                        chr.WriteToDisplay("@  = Tree         /\\ = Mountain");
                        chr.WriteToDisplay("== = Counter      .~ = Ice");
                        chr.WriteToDisplay("up = Up Stairs    dn = Down Stairs");
                        chr.WriteToDisplay(".  = Empty        [] = Wall ");
                        break;
                    case "movement":
                        chr.WriteToDisplay("N N NE would move you 3 spaces North and one space NorthEast.");
                        chr.WriteToDisplay("You can use up to three moves in one turn. For instance");
                        chr.WriteToDisplay("Portals can be used with a magic chant.");
                        chr.WriteToDisplay("D  = Down        U = Up          Climb Up        Climb Down");
                        chr.WriteToDisplay("NE = NorthEast  NW = NorthWest  SE = SouthEast  SW = SouthWest");
                        chr.WriteToDisplay("N  = North       S = South       E = East        W = West");
                        chr.WriteToDisplay("The possible directions are:");
                        chr.WriteToDisplay("to move using the first letter of the direction.");
                        chr.WriteToDisplay("Movement is done by typing the direction you want");
                        break;
                    case "combat":
                        chr.WriteToDisplay("NAME is the name of the thing you are attacking.");
                        chr.WriteToDisplay("the FIGHT command. The usage is Attack <NAME> where ");
                        chr.WriteToDisplay("Combat is done by using either the Attack command, or");
                        break;
                    case "merchants":
                        chr.WriteToDisplay("Use the command 'show prices' while in front of a counter");
                        chr.WriteToDisplay("to see what items are for sale. To purchase an item place");
                        chr.WriteToDisplay("your gold coins on the counter (put coins on counter) and");
                        chr.WriteToDisplay("then ask the merchant to sell you the item with the command");
                        chr.WriteToDisplay("'<name>, SELL <item name>'. To sell an item use");
                        chr.WriteToDisplay("'<name>, BUY <item name>' or '<name>, BUY ALL'.");
                        chr.WriteToDisplay("Note that you can use the command 'dump <item name> on counter'");
                        chr.WriteToDisplay("to place all <item name> from your sack on the counter in front");
                        chr.WriteToDisplay("of you.");
                        break;
                    case "talk":
                        chr.WriteToDisplay("using the SAY command to display your message to anyone in ");
                        chr.WriteToDisplay("instance \"Where is the armor shop? would work the same as");
                        chr.WriteToDisplay("A shortcut is to use the \" instead of the SAY command. For");
                        chr.WriteToDisplay("usage: SAY <MESSAGE> where message is what you want to say.");
                        chr.WriteToDisplay("You can talk to other players by using the SAY command.");
                        break;
                    case "training":
                        chr.WriteToDisplay("Training will greatly increase the amount of skill experience you earn while using the skill.");
                        //chr.WriteToDisplay("Training Costs:");
                        //for (int a = 0; a <= Skills.MAX_SKILL_LEVEL; a++)
                        //    chr.WriteToDisplay("Level " + a + ": Full Cost = " + Rules.Formula_TrainingCostForLevel(a) + ", Rank = " +
                        //        Skills.GetTrainingCostPerRank(a));
                        break;
                    case "formula":
                    case "formulas":
                        chr.WriteToDisplay("Current Stat Formulas (" + chr.Land.Name + "):");
                        string[] classes = Enum.GetNames(typeof(Character.ClassType));
                        string hitsFormula = "";
                        string stamFormula = "";
                        string manaFormula = "";
                        string outputString = "";
                        for (int a = 1; a < classes.Length; a++)
                        {
                            hitsFormula = "(" + GameWorld.World.HitDice[a].ToString() + " * level) + " + 64;
                            stamFormula = "(" + GameWorld.World.StaminaDice[a].ToString() + " * level)";
                            if (chr.Land.ManaDice[a] > 0)
                            {
                                manaFormula = "(" + GameWorld.World.ManaDice[a].ToString() + " * level)";
                                outputString = "[" + Utils.FormatEnumString(classes[a]) + "] H: " + hitsFormula + " S: " + stamFormula + " M: " + manaFormula;
                            }
                            else
                            {
                                outputString = "[" + Utils.FormatEnumString(classes[a]) + "] H: " + hitsFormula + " S: " + stamFormula;
                            }
                            chr.WriteToDisplay(outputString);
                        }
                        break;
                    case "experience":
                    case "exp":
                        CommandTasker.ParseCommand(chr, "showxp", "");
                        break;
                    case "magic":
                        break;
                    default:
                        break;
                }
            }

            return true;
        }
    }
}
