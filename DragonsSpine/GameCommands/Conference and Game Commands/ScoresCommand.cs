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
using World = DragonsSpine.GameWorld.World;

namespace DragonsSpine.Commands
{
    [CommandAttribute("scores", "Display a list of player scores based on earned experience.", (int)Globals.eImpLevel.USER, new string[] { "score", "topten" },
        0, new string[] { "scores <profession>", "scores <profession> <amount>", "scores <amount>", "scores <name>" }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class ScoresCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            short amount = 10; // default amount will be 10 rows
            bool devRequest = chr is PC && (chr as PC).ImpLevel >= Globals.eImpLevel.GM; // turn devrequest on if impLevel >= GM
            bool pvp = false;
            var list = new List<string>();

            try
            {
                if (String.IsNullOrEmpty(args)) // default to all classes list of 10
                {
                    list = this.GetScoresList("all", amount, devRequest, pvp);

                    foreach (string line in list)
                        chr.WriteLine(line, ProtocolYuusha.TextType.Listing);

                    return true;
                }
                else
                {
                    string[] scoreArgs = args.Split(" ".ToCharArray());

                    // Get my score.
                    if (scoreArgs[0] == "me" || scoreArgs[0] == "mine" || scoreArgs[0] == "my" || scoreArgs[0].ToLower() == chr.Name.ToLower()) // get my score
                    {
                        if (scoreArgs.Length > 1 && scoreArgs[1].ToLower() == "pvp") { pvp = true; }
                        list = this.GetScoresList(chr.Name, 1, true, pvp);
                    }
                    else
                    {
                        if (scoreArgs.Length > 3)
                        {
                            chr.WriteLine("Formats: /scores, /scores me, /scores <class>, /scores <class> <amount>, /scores <name>, /scores all <amount>", ProtocolYuusha.TextType.Help);
                            return false;
                        }
                        try
                        {
                            if (scoreArgs.Length >= 2 && scoreArgs[1] == "all") { amount = 100; } // top 100
                            else if (scoreArgs.Length >= 2) { amount = Convert.ToInt16(scoreArgs[1]); }
                        }
                        catch (Exception ex)
                        {
                            Utils.LogException(ex);
                            amount = 10;
                            pvp = false;
                            scoreArgs[0] = "all";
                        }

                        if (amount < 1) { amount = 1; }

                        list = this.GetScoresList(scoreArgs[0].ToLower(), amount, devRequest, pvp);
                    }
                }

                chr.WriteLine("", ProtocolYuusha.TextType.Listing);
                foreach (string line in list)
                    chr.WriteLine(line, ProtocolYuusha.TextType.Listing);
                chr.WriteLine("", ProtocolYuusha.TextType.Listing);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                chr.WriteLine("Formats: /scores, /scores me, /scores <class>, /scores <class> <amount>, /scores <name>", ProtocolYuusha.TextType.Error);
                return false;
            }

            return true;
        }

        private List<string> GetScoresList(string classAbbreviation, short amount, bool devRequest, bool pvp)
        {
            string classFullName = "";
            string playerName = "";
            Character.ClassType classType = Character.ClassType.None;
            List<string> list = new List<string>();

            #region Profession Selection
            switch (classAbbreviation.ToLower())
            {
                case "all":
                    classType = Character.ClassType.None;
                    classFullName = "All Classes";
                    break;
                case "b":
                case "bers":
                case "berserker":
                case "berserkers":
                    classType = Character.ClassType.Berserker;
                    classFullName = "Berserkers";
                    break;
                case "k":
                case "ki":
                case "kn":
                case "knight":
                case "knights":
                    classType = Character.ClassType.Knight;
                    classFullName = "Knights";
                    break;
                case "f":
                case "fi":
                case "fighter":
                case "fighters":
                    classType = Character.ClassType.Fighter;
                    classFullName = "Fighters";
                    break;
                case "m":
                case "ma":
                case "martial":
                case "martialartist":
                case "martialartists":
                    classType = Character.ClassType.Martial_Artist;
                    classFullName = "Martial Artists";
                    break;
                case "t":
                case "th":
                case "thaum":
                case "thaums":
                case "thaumaturge":
                case "thaumaturges":
                    classType = Character.ClassType.Thaumaturge;
                    classFullName = "Thaumaturges";
                    break;
                case "w":
                case "wi":
                case "wiz":
                case "wizard":
                case "mage":
                case "wizards":
                    classType = Character.ClassType.Wizard;
                    classFullName = "Wizards";
                    break;
                case "r":
                case "ran":
                case "rang":
                case "ranger":
                case "rangers":
                    classType = Character.ClassType.Ranger;
                    classFullName = "Rangers";
                    break;
                case "rav":
                case "ravager":
                case "ravagers":
                    classType = Character.ClassType.Ravager;
                    classFullName = "Ravagers";
                    break;
                case "tf":
                case "thief":
                case "thiefs":
                case "thieves":
                    classType = Character.ClassType.Thief;
                    classFullName = "Thieves";
                    break;
                case "s":
                case "sr":
                case "sorc":
                case "sorcs":
                case "sorcerer":
                case "sorcerers":
                    classType = Character.ClassType.Sorcerer;
                    classFullName = "Sorcerers";
                    break;
                case "dr":
                case "druid":
                case "druids":
                    classType = Character.ClassType.Druid;
                    classFullName = "Druids";
                    break;
                default:
                    if (PC.PlayerExists(classAbbreviation))
                        playerName = classAbbreviation;
                    break;
            }
            #endregion

            if (playerName == "")
                list.Add("Top " + amount + " " + classFullName + " of " + DragonsSpineMain.Instance.Settings.ServerName);

            bool evils = Array.IndexOf(World.EvilProfessions, classType) != -1;

            if (evils)
                list.Add(" Rank Name            Profession     Level Experience  KPH  Last Online   Karma");
            else list.Add(" Rank Name            Profession     Level Experience  KPH  Last Online");

            string anonymous = "";

            try
            {
                var scores = DAL.DBWorld.GetScoresWithoutSP(classType, amount, devRequest, playerName, pvp);

                if (scores.Count < 1)
                {
                    if (playerName != "")
                    {
                        list.Clear();
                        list.Add("No player named \"" + classAbbreviation + "\" found in the scores list.");
                        return list;
                    }

                    list.Clear();
                    list.Add("No scores found that match your search arguments.");
                    return list;
                }

                foreach (PC score in scores)
                {
                    if (devRequest && score.IsAnonymous)
                        anonymous = " [ANON]";
                    else if (devRequest && score.ImpLevel > Globals.eImpLevel.USER)
                    {
                        score.showStaffTitle = true;
                        anonymous = " " + Conference.GetStaffTitle(score);
                    }
                    else
                        anonymous = "";

                    // rank, name, profession, level, experience, KPH, last online
                    list.Add(Convert.ToInt32(score.TemporaryStorage).ToString().PadLeft(4, ' ') +
                        ". " + score.Name.PadRight(16, ' ') +
                        Utils.FormatEnumString(score.BaseProfession.ToString()).PadRight(15, ' ') +
                        score.Level.ToString().PadLeft(5, ' ') +
                        score.Experience.ToString().PadLeft(11, ' ') +
                        Rules.CalculateKillsPerHour(score).ToString().PadLeft(5, ' ') +
                        "  " + score.lastOnline.ToShortDateString().PadLeft(11, ' ') +
                        (evils ? score.currentKarma.ToString().PadLeft(6, ' ') : "") +
                        "  " + anonymous);
                }
                return list;
            }
            catch (Exception e)
            {
                Utils.Log("World.getScoresList(" + classType + ", " + amount + ", " + Convert.ToString(devRequest) + ", " + Convert.ToString(pvp) + ") " + e.Message + " Stack: " + e.StackTrace, Utils.LogType.Exception);
                Utils.LogException(e);
                return null;
            }
        } 
    }
}