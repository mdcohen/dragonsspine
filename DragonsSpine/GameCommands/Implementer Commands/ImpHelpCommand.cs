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

namespace DragonsSpine.Commands
{
    [CommandAttribute("imphelp", "Display a list of commands available to current implementer level.", (int)Globals.eImpLevel.AGM, new string[] { },
        0, new string[] { "There are currently no arguments for this command." }, Globals.ePlayerState.CONFERENCE)]
    public class ImpHelpCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            List<string> commandsList = new List<string>();

            foreach (GameCommand cmd in GameCommand.GameCommandDictionary.Values)
            {
                if (cmd.PrivLevel > (int)Globals.eImpLevel.USER && cmd.PrivLevel <= (int)(chr as PC).ImpLevel)
                {
                    string uses = "";
                    foreach (string s in cmd.Usages) uses += " " + s + ",";
                    uses = uses.Substring(0, uses.Length - 1);
                    uses = uses.Trim();

                    commandsList.Add(cmd.Command + ": " + cmd.Description + " Uses: " + uses);
                }
            }

            commandsList.Sort();

            foreach (string s in commandsList)
                chr.WriteToDisplay(s);

            return true;
        }
    }
}
