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
using System.Linq;
using System.Text;
using DemonName = DragonsSpine.Commands.DemonSummoningChantCommand.DemonName;

namespace DragonsSpine.Commands
{
    [CommandAttribute("glamdrang", "Speaking the name of a demon may be hazardous to your health.",
        (int)Globals.eImpLevel.USER, new string[] { }, 2, new string[] { "There are no arguments when uttering the name of a demon." },
        Globals.ePlayerState.PLAYING)]
    public class GlamdrangCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            Commands.DemonSummoningChantCommand.ChantDemonSummon(DemonName.Glamdrang, chr);

            return true;
        }
    }
}

