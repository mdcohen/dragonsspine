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
using DragonsSpine.GameWorld;

namespace DragonsSpine.Commands
{
    [CommandAttribute("lockdoor", "Lock a door if you have the appropriate key.", (int)Globals.eImpLevel.USER, new string[] { },
        1, new string[] { "lockdoor <direction>" }, Globals.ePlayerState.PLAYING)]
    public class LockDoorCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("What door do you want to lock?");
                return false;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            Cell cell = Map.GetCellRelevantToCell(chr.CurrentCell, sArgs[0], true);

            if (cell == null || (!cell.IsLockedVerticalDoor && !cell.IsLockedHorizontalDoor))
            {
                chr.WriteToDisplay("There is nothing to lock here.");
                return true;
            }

            if (chr.RightHand == null)
            {
                chr.WriteToDisplay("You are not holding the correct key in your right hand.");
                return true;
            }

            if (Map.LockDoor(cell, chr.RightHand.key))
            {
                chr.WriteToDisplay("You lock the door.");
            }
            else
            {
                if (cell.cellLock.key != chr.RightHand.key)
                {
                    chr.WriteToDisplay("You do not have the correct key.");
                }
                else
                {
                    chr.WriteToDisplay("The door is already locked.");
                }
            }

            return true;
        }
    }
}
