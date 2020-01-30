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
    [CommandAttribute("showquests", "Display completed and current quests.", (int)Globals.eImpLevel.USER, new string[] { "show quests", "show quest" },
        0, new string[] { "There are no arguments for the show quests command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowQuestsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.QuestList.Count > 0)
            {
                chr.WriteToDisplay("Quests:");

                foreach (GameQuest q in chr.QuestList)
                    chr.WriteToDisplay("Name:" + q.Name + " Status: " + (q.TimesCompleted > 0 ? "Completed" + (q.TimesCompleted > 1 ? " x " + q.TimesCompleted.ToString() : "") : "In Progress"));
            }
            else chr.WriteToDisplay("You have not completed or started any quests.");

            string questflags = "You do not have any quest flags.";

            if (chr.QuestFlags.Count > 0)
            {
                questflags = "\r\nQuest Flags:";

                foreach (String flag in chr.QuestFlags)
                    questflags += " " + flag;
            }

            chr.WriteToDisplay(questflags);

            return true;
        }
    }
}

