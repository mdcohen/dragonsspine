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
using Cell = DragonsSpine.GameWorld.Cell;

namespace DragonsSpine.Commands
{
    [CommandAttribute("impgotolocation", "Go to a specific location in the game world.", (int)Globals.eImpLevel.AGM, new string[] { "impgotoloc" },
        0, new string[] { "impgotoloc <facet> <land> <map> <x> <y> <z>" }, Globals.ePlayerState.PLAYING)]
    public class ImpGoToLocationCommand : ICommandHandler
    {
        //TODO: expand on this by using place names
        public bool OnCommand(Character chr, string args)
        {
            // Scribing crystal.
            if (args.ToLower() == "sc" || args.ToLower() == "scribe" || args.ToLower() == "crystal")
            {
                foreach (GameWorld.Land land in chr.Facet.Lands)
                {
                    foreach (GameWorld.Map map in land.Maps)
                    {
                        foreach (GameWorld.Cell cell in map.cells.Values)
                        {
                            if (cell.HasScribingCrystal)
                            {
                                chr.CurrentCell = cell;
                                chr.WriteToDisplay("You have teleported to a scribing crystal in " + map.Name + ".");
                                return true;
                            }
                        }
                    }
                }

                if (!chr.CurrentCell.HasScribingCrystal)
                {
                    chr.WriteToDisplay("Failed to find a scribing crystal. This shouldn't occur.");
                    return true;
                }
            }

            try
            {
                string[] sArgs = args.Split(" ".ToCharArray());

                short facet, land, map;

                int x, y, z;

                facet = Convert.ToInt16(sArgs[0]);
                land = Convert.ToInt16(sArgs[1]);
                map = Convert.ToInt16(sArgs[2]);
                x = Convert.ToInt32(sArgs[3]);
                y = Convert.ToInt32(sArgs[4]);
                z = Convert.ToInt32(sArgs[5]);

                Cell gotoCell = Cell.GetCell(facet, land, map, x, y, z);

                if (gotoCell == null || gotoCell.IsOutOfBounds)
                    chr.WriteToDisplay("That place is outside of the map bounds.");
                else
                {
                    if (!chr.IsInvisible)
                        chr.SendToAllInSight(chr.Name + " disappears in a poof of smoke!");

                    chr.CurrentCell = gotoCell;

                    if (!chr.IsInvisible)
                        chr.SendToAllInSight(chr.Name + " appears with a poof of smoke.");
                }

                return true;
            }
            catch
            {
                chr.WriteToDisplay("Format: impgotoloc <facet> <land> <map> <x> <y> <z>");
                return false;
            }
        }
    }
}
