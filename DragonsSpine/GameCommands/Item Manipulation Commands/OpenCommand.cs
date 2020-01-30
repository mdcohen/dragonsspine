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
    [CommandAttribute("open", "Open a door, container, or book.", (int)Globals.eImpLevel.USER, 0, new string[] { "open <item>", "open door <direction" }, Globals.ePlayerState.PLAYING)]
    public class OpenCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null || args == "")
            {
                chr.WriteToDisplay("What do you want to open?");
                return true;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            if (sArgs[0] == "right" && chr.RightHand != null && chr.RightHand.baseType == Globals.eItemBaseType.Bottle)
            {
                Bottle.OpenBottle((Bottle)chr.RightHand, chr);
            }
            else if (sArgs[0] == "left" && chr.LeftHand != null && chr.LeftHand.baseType == Globals.eItemBaseType.Bottle)
            {
                Bottle.OpenBottle((Bottle)chr.LeftHand, chr);
            }
            else if(chr.RightHand != null && chr.RightHand.name == sArgs[0] && chr.RightHand.baseType == Globals.eItemBaseType.Bottle)
            {
                Bottle.OpenBottle((Bottle)chr.RightHand, chr);
            }
            else if (chr.LeftHand != null && chr.LeftHand.name == sArgs[0] && chr.LeftHand.baseType == Globals.eItemBaseType.Bottle)
            {
                Bottle.OpenBottle((Bottle)chr.LeftHand, chr);
            }
            else if (Autonomy.ItemBuilding.ItemBuilder.BOTTLE_SYNONYMS.Contains(sArgs[0].ToLower()))
            {
                if (chr.RightHand != null && chr.RightHand.baseType == Globals.eItemBaseType.Bottle)
                {
                    Bottle.OpenBottle((Bottle)chr.RightHand, chr);
                }
                else if (chr.LeftHand != null && chr.LeftHand.baseType == Globals.eItemBaseType.Bottle)
                {
                    Bottle.OpenBottle((Bottle)chr.LeftHand, chr);
                }
                else
                {
                    chr.WriteToDisplay("You are not holding a " + sArgs[0] + ".");
                }

                return true;
            }
            else if (Autonomy.ItemBuilding.ItemBuilder.BOOK_SYNONYMS.Contains(sArgs[0].ToLower()))
            {
                return CommandTasker.ParseCommand(chr, "flip", args);
            }
            else if (sArgs[0].ToLower() == "door")
            {
                if (sArgs.Length == 1)
                {
                    chr.WriteToDisplay("Format: open door <direction>");
                    return true;
                }

                Cell cell = Map.GetCellRelevantToCell(chr.CurrentCell, sArgs[1], true);
                string newGraphic = cell.CellGraphic;

                if (cell.DisplayGraphic == Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL || cell.DisplayGraphic == Cell.GRAPHIC_OPEN_DOOR_VERTICAL)
                {
                    chr.WriteToDisplay("The door is already open.");
                    return true;
                }
                else if (cell.DisplayGraphic != Cell.GRAPHIC_CLOSED_DOOR_VERTICAL && cell.DisplayGraphic != Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL)
                {
                    chr.WriteToDisplay("You don't see a door there.");
                    return true;
                }

                OpenCommand.OpenDoor(cell);
            }

            return true;
        }

        public static void OpenDoor(Cell doorCell)
        {
            string newGraphic = doorCell.CellGraphic;

            switch (doorCell.DisplayGraphic)
            {
                case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:
                    newGraphic = Cell.GRAPHIC_OPEN_DOOR_VERTICAL;
                    break;
                case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:
                    newGraphic = Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL;
                    break;
                default:
                    break;
            }
            doorCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.OpenDoor));
            doorCell.DisplayGraphic = newGraphic;
            doorCell.CellGraphic = newGraphic;
        }

    }
}
