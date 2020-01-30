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
    [CommandAttribute("close", "Close a door or container.", (int)Globals.eImpLevel.USER, 0, new string[] { "close <item>", "close door <direction" }, Globals.ePlayerState.PLAYING)]
    public class CloseCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("What do you want to close?");
                return true;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            if (sArgs[0] == "door")
            {
                #region door
                Cell doorCell = Map.GetCellRelevantToCell(chr.CurrentCell, sArgs[1], true);

                string newGraphic = doorCell.CellGraphic;

                if (doorCell.CellGraphic == Cell.GRAPHIC_CLOSED_DOOR_VERTICAL || doorCell.CellGraphic == Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL)
                {
                    chr.WriteToDisplay("The door is already closed.");
                    return true;
                }
                else if (doorCell.CellGraphic != Cell.GRAPHIC_OPEN_DOOR_VERTICAL && doorCell.CellGraphic != @"\ ")
                {
                    chr.WriteToDisplay("You don't see a door there.");
                    return true;
                }
                else if (Cell.GetCellDistance(chr.X, chr.Y, doorCell.X, doorCell.Y) > 1)
                {
                    chr.WriteToDisplay("The door is too far away.");
                    return true;
                }
                else if (doorCell.Items.Count > 0 || doorCell.Characters.Count > 0)
                {
                    chr.WriteToDisplay("The doorway is blocked.");
                    return true;
                }

                CloseDoor(doorCell);
                #endregion
            }
            else if (sArgs[0].ToLower() == "bottle")
            {
                #region bottle
                if (chr.RightHand != null && chr.RightHand.baseType == Globals.eItemBaseType.Bottle)
                {
                    Bottle.CloseBottle((Bottle)chr.RightHand);
                }
                else if (chr.LeftHand != null && chr.LeftHand.baseType == Globals.eItemBaseType.Bottle)
                {
                    Bottle.CloseBottle((Bottle)chr.LeftHand);
                }
                else
                {
                    chr.WriteToDisplay("You are not holding a bottle.");
                }
                #endregion
            }
            else if (Autonomy.ItemBuilding.ItemBuilder.BOOK_SYNONYMS.Contains(sArgs[0].ToLower()))
            {
                #region book / spellbook
                Book book = new Book();

                // Get the book from hand
                if (chr.RightHand != null && chr.RightHand.baseType == Globals.eItemBaseType.Book)
                {
                    book = (Book)chr.RightHand;
                    book.CurrentPage = 0;
                    chr.WriteToDisplay("You close the " + sArgs[0] + ".");
                }
                else if (chr.LeftHand != null && chr.LeftHand.baseType == Globals.eItemBaseType.Book)
                {
                    book = (Book)chr.LeftHand;
                    book.CurrentPage = 0;
                    chr.WriteToDisplay("You close the " + sArgs[0] + ".");
                }
                else
                {
                    chr.WriteToDisplay("You are not holding a " + sArgs[0] + ".");
                } 
                #endregion
            }

            return true;
        }

        public static void CloseDoor(Cell doorCell)
        {
            string newGraphic = doorCell.CellGraphic;

            switch (doorCell.CellGraphic)
            {
                case Cell.GRAPHIC_OPEN_DOOR_VERTICAL:
                    newGraphic = Cell.GRAPHIC_CLOSED_DOOR_VERTICAL;
                    break;
                case Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL:
                    newGraphic = Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL;
                    break;
                default:
                    break;
            }
            doorCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.CloseDoor));
            doorCell.CellGraphic = newGraphic;
            doorCell.DisplayGraphic = newGraphic;
        }

    }
}
