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
    [CommandAttribute("impimmortal", "Toggle immortality.", (int)Globals.eImpLevel.GM, new string[] { "immortal", "impimm", "imm" },
        0, new string[] { "There are no arguments for this command." }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class ImpImmortalCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.IsImmortal)
            {
                chr.IsImmortal = false;
                chr.WriteToDisplay("You are no longer immortal.");
            }
            else
            {
                chr.WriteToDisplay("You are now immortal.");
                chr.IsImmortal = true;
            }

            if (chr is PC && (chr as PC).ImpLevel < Globals.eImpLevel.DEV) // log immortal if a staff member other than dev
                Utils.Log(chr.GetLogString() + " toggled immortal to " + Convert.ToString(chr.IsImmortal) + ".", Utils.LogType.CommandImmortal);

            return true;
        }
    }
}
