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
    [CommandAttribute("redraw", "Request a redraw of the display.", (int)Globals.eImpLevel.USER,  0, new string[] { "There are no arguments for the redraw command." }, Globals.ePlayerState.PLAYING)]
    public class RedrawCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol || chr.protocol == "old-kesmai")
            {
                chr.updateAll = true;
            }
            else
            {
                GameWorld.Map.ClearMap(chr);
                chr.CurrentCell.ShowMap(chr as PC);
            }

            return true;
        }
    }
}
