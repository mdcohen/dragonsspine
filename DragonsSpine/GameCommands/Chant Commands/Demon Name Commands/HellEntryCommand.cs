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
using DemonName = DragonsSpine.Commands.DemonSummoningChantCommand.DemonName;

namespace DragonsSpine.Commands
{
    [CommandAttribute("aazagtamatunulimutumaenaacuthaluuhddil", "You have learned the chant to enter Hell. We wish you the best of luck.",
        (int)Globals.eImpLevel.USER, new string[] { "aazag tamatunu limutuma enaa cuthalu uhddil" }, 2, new string[] { "There are no arguments." },
        Globals.ePlayerState.PLAYING)]
    public class HellEntryCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            return true;
        }
    }
}

