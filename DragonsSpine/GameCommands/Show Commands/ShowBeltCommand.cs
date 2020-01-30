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
    [CommandAttribute("showbelt", "Display a list of belt items.", (int)Globals.eImpLevel.USER, new string[] { "show belt" },
        0, new string[] { "There are no arguments for the show belt command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowBeltCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            foreach (Item item in chr.beltList)
            {
                if (item.IsNocked)
                    chr.WriteToDisplay(item.name + "*");
                else chr.WriteToDisplay(item.name);
            }

            chr.WriteToDisplay("Use \"inventory\" for a list of worn items.");

            return true;
        }
    }
}
