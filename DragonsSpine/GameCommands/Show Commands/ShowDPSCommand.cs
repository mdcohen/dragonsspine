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
    [CommandAttribute("showdps", "Display detailed damage output statistics.", (int)Globals.eImpLevel.USER, new string[] { "show dps", "showdamage", "show damage", "show damage per second" },
        0, new string[] { "There are no arguments for the show DPS command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowDPSCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (!chr.DPSLoggingEnabled)
                chr.WriteToDisplay("You must enable DPS logging to view your DPS statistics.");

            if (!GameSystems.DPSCalculator.DisplayDPSOutput(chr, chr.UniqueID))
                chr.WriteToDisplay("Failed to display DPS statistics.");

            return true;
        }
    }
}
