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
    [CommandAttribute("echo", "Toggle echo of typed characters.", (int)Globals.eImpLevel.USER, new string[] { },
        0, new string[] { "There are no arguments for the echo command." }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class EchoCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if ((chr as PC).echo)
            {
                (chr as PC).echo = false;
                chr.WriteToDisplay("Echo disabled.");
            }
            else
            {
                (chr as PC).echo = true;
                chr.WriteToDisplay("Echo enabled.");
            }

            return true;
        }
    }
}
