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
    [CommandAttribute("displaydamageshield", "Toggle display information from your damage shield.", (int)Globals.eImpLevel.USER, new string[] { "displaydmgshield", "displayshield" },
        0, new string[] { "displaydmgshield", "displaydmgshield [off | on]" }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class DisplayDamageShieldCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            PC pc = (chr as PC);

            if (args == null)
            {
                pc.DisplayDamageShield = !pc.DisplayDamageShield;

                chr.WriteToDisplay("Display damage shield option set to " + (pc.DisplayDamageShield ? "ON" : "OFF") + ".");
            }
            else
            {
                if (args.ToLower() == "off") { pc.DisplayDamageShield = false; chr.WriteToDisplay("Display damage shield option set to OFF."); }
                else if (args.ToLower() == "on") { pc.DisplayPetMessages = true; chr.WriteToDisplay("Display damage shield option set to ON."); }
                else { chr.WriteToDisplay("Format: displaydmgshield [off | on]"); }
            }

            return true;
        }
    }
}
