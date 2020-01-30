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
    [CommandAttribute("imptitle", "Toggle implementer title.", (int)Globals.eImpLevel.AGM, new string[] { },
        0, new string[] { "There are no arguments for this command." }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class ImpTitleCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if ((chr as PC).showStaffTitle)
            {
                (chr as PC).showStaffTitle = false;
                chr.WriteToDisplay("Your title is now OFF.");
            }
            else
            {
                (chr as PC).showStaffTitle = true;
                chr.WriteToDisplay("Your title is now ON.");
            }

            return true;
        }
    }
}
