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
using DragonsSpine.Talents;

namespace DragonsSpine.Commands
{
    [CommandAttribute("showtalents", "Display learned talents.", (int)Globals.eImpLevel.USER, new string[] { "show talent", "show talents", "showtalent", "talents" },
        0, new string[] { "There are no arguments for the show talents command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowTalentsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.talentsDictionary.Count <= 0)
            {
                chr.WriteToDisplay("You have not learned any talents.");
            }
            else
            {
                foreach (string s in chr.talentsDictionary.Keys)
                {
                    if(!GameTalent.GameTalentDictionary.ContainsKey(s))
                    {
                        Utils.Log("Invalid talent in " + chr.GetLogString() + " list. String: " + s + " -- Deleting from PlayerTalents database.", Utils.LogType.SystemWarning);
                        chr.talentsDictionary.Remove(s);
                        DAL.DBPlayer.DeletePlayerTalent((chr as PC).UniqueID, s);
                        continue;
                    }

                    GameTalent gt = GameTalent.GameTalentDictionary[s];

                    string info = gt.Name + " - " + gt.Description + " (" + (gt.IsPassive ? "passive, " + gt.PerformanceCost.ToString() + " stamina" : gt.PerformanceCost.ToString() + " stamina") + ")";

                    if (!gt.IsPassive)
                    {
                        if (DateTime.UtcNow - gt.DownTime >= chr.talentsDictionary[s])
                            info += " [READY]";
                        else
                        {
                            int roundsRemaining = Utils.TimeSpanToRounds(gt.DownTime - (DateTime.UtcNow - chr.talentsDictionary[s]));
                            info += " [" + roundsRemaining + " RNDS]";
                        }
                    }
                    else if (chr.DisabledPassiveTalents.Contains(gt.Command))
                    {
                        info += " [DISABLED]";
                    }
                    

                    chr.WriteToDisplay(info);
                    // TODO: add time until available
                }

                chr.WriteToDisplay("You have learned " + chr.talentsDictionary.Count + " talent" + (chr.talentsDictionary.Count > 1 ? "s" : "") + ".");
            }

            return true;
        }
    }
}
