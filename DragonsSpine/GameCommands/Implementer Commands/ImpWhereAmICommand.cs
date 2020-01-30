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
    [CommandAttribute("impwhereami", "Display location information.", (int)Globals.eImpLevel.AGM, new string[] { },
        0, new string[] { "There are currently no arguments for this command." }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class ImpWhereAmICommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            string info = chr.Name + " F: " + chr.FacetID + " L: " + chr.LandID + " M: " + chr.MapID + " ";

            info += "X: " + chr.X + " Y: " + chr.Y + " Z: " + chr.Z;

            chr.WriteToDisplay(info);
            chr.WriteToDisplay("You are in: " + chr.Map.ZPlanes[chr.Z].name + ".");

            return true;
        }
    }
}
