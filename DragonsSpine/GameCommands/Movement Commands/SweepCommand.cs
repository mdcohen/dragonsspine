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
    [CommandAttribute("sweep", "Display a list of belt items.", (int)Globals.eImpLevel.USER, 2, new string[] { "There are no arguments for the show belt command." }, Globals.ePlayerState.PLAYING)]
    public class SweepCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.CommandWeight > 3)
            {
                chr.WriteToDisplay("Command weight limit exceeded. Sweep command not processed.");
                return true;
            }

            if (chr.RightHand == null)
            {
                chr.WriteToDisplay("You do not have a broom in your right hand.");
                return true;
            }
            if (args == null)
            {
                chr.WriteToDisplay("Sweep where?");
                return true;
            }

            Item[] tempItemList = Item.GetAllItemsFromGround(chr.CurrentCell);

            string[] sArgs = args.Split(" ".ToCharArray());

            if (sArgs[0].ToLower() == "u" || sArgs[0].ToLower() == "up")
            {
                if (chr.CurrentCell.CellGraphic == Cell.GRAPHIC_UPSTAIRS || chr.CurrentCell.CellGraphic == Cell.GRAPHIC_UP_AND_DOWNSTAIRS)
                {
                    chr.CommandType = CommandTasker.CommandType.Movement;
                    Map.MoveCharacter(chr, "up", "sweep");
                    foreach (Item item in tempItemList)
                        chr.CurrentCell.Add(item);
                    return true;
                }
                else
                {
                    chr.WriteToDisplay("You don't see any stairs leading up here.");
                    return true;
                }
            }

            if (sArgs[0].ToLower() == "d" || sArgs[0].ToLower() == "down")
            {
                if (chr.CurrentCell.CellGraphic == Cell.GRAPHIC_DOWNSTAIRS || chr.CurrentCell.CellGraphic == Cell.GRAPHIC_UP_AND_DOWNSTAIRS)
                {
                    chr.CommandType = CommandTasker.CommandType.Movement;
                    Map.MoveCharacter(chr, "down", "sweep");
                    foreach (Item item in tempItemList)
                        chr.CurrentCell.Add(item);
                    return true;
                }
                else
                {
                    chr.WriteToDisplay("You don't see any stairs leading down here.");
                    return true;
                }
            }

            chr.CommandType = CommandTasker.CommandType.Sweep;

            Map.MoveCharacter(chr, sArgs[0], "sweep");

            foreach (Item item in tempItemList)
                chr.CurrentCell.Add(item);

            return true;
        }
    }
}
