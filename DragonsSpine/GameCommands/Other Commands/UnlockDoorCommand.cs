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
    [CommandAttribute("unlockdoor", "Unlock a door if you have the appropriate key.", (int)Globals.eImpLevel.USER, new string[] { },
        1, new string[] { "unlockdoor <direction>" }, Globals.ePlayerState.PLAYING)]
    public class UnlockDoorCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("What door do you want to unlock?");
                return false;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            Cell cell = Map.GetCellRelevantToCell(chr.CurrentCell, sArgs[0], true);

            if (cell == null || (!cell.IsLockedVerticalDoor && !cell.IsLockedHorizontalDoor))
            {
                chr.WriteToDisplay("There is nothing to unlock here.");
                return true;
            }

            if (chr.RightHand == null)
            {
                chr.WriteToDisplay("You are not holding a key in your right hand.");
                return true;
            }

            if (Map.UnlockDoor(chr, cell, chr.RightHand))
            {
                if (cell.cellLock.lockSuccess != "") { chr.WriteToDisplay(cell.cellLock.lockSuccess); }
                else { chr.WriteToDisplay("You unlock the door."); }

                if (chr.RightHand.itemID == Item.ID_PRINCESS_CELL_KEY)
                {
                    chr.WriteToDisplay("The key snaps off in the lock.");
                    chr.UnequipRightHand(chr.RightHand);
                }

                return true;
            }
            else if (cell.cellLock.lockFailureString.Length > 0) { chr.WriteToDisplay(cell.cellLock.lockFailureString); }
            else { chr.WriteToDisplay("You do not have the correct key."); }

            return true;
        }
    }
}
