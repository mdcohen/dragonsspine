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
    [CommandAttribute("balance",
        "Display balance of your bank account at specified locations.",
        (int)Globals.eImpLevel.USER, new string[] { "bal"}, 1, new string[] { "There are no arguments for the balance command." }, Globals.ePlayerState.PLAYING)]
    public class BalanceCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.CurrentCell == null || !chr.CurrentCell.IsOrnicLocker)
            {
                chr.WriteToDisplay("You can't do that here.");
                return true;
            }

            chr.WriteToDisplay("You have " + (chr as PC).bankGold + " coin" + ((chr as PC).bankGold > 1 ? "s" : "") + " in your bank account.");

            return true;
        }
    }
}
