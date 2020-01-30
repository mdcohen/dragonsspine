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
    [CommandAttribute("displaypetdamage", "Toggle display of damage dealt and received by pets.", (int)Globals.eImpLevel.USER, new string[] { "displaypetdmg" },
        0, new string[] { "displaypetdmg", "displaypetdmg [off | on]" }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class DisplayPetDamageCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            PC pc = chr as PC;

            if (string.IsNullOrEmpty(args))
            {
                if (pc.DisplayPetDamage)
                {
                    pc.DisplayPetDamage = false;
                    chr.WriteToDisplay("Display pet damage set to OFF.");
                }
                else
                {
                    pc.DisplayPetDamage = true;
                    chr.WriteToDisplay("Display pet damage set to ON.");
                }
            }
            else
            {
                string[] sArgs = args.Split(" ".ToCharArray());

                if (sArgs[0].ToLower() == "off") { pc.DisplayPetDamage = false; chr.WriteToDisplay("Display pet damage set to OFF."); }
                else if (sArgs[0].ToLower() == "on") { pc.DisplayPetDamage = true; chr.WriteToDisplay("Display pet damage set to ON."); }
                else { chr.WriteToDisplay("Format: displaypetdmg [ on | off ]"); }
            }

            return true;
        }
    }
}
