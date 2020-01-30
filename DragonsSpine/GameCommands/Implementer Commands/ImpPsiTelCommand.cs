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
    [CommandAttribute("imppsitel", "Go to a specific location in the game world.", (int)Globals.eImpLevel.AGM, new string[] { "psitel" },
        0, new string[] { "psitel <facet> <land> <map> <x> <y> <z>" }, Globals.ePlayerState.PLAYING)]
    public class ImpPsiTelCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Facet number no longer required, as this is currently always 0.");
                chr.WriteToDisplay("Format: psitel <land> <map> <ycord> <xcord> <zcord>");
                return true;
            }
            else
            {
                try
                {
                    string[] sArgs = args.Split(" ".ToCharArray());
                    int facet, land, map;
                    int x, y, z;
                    facet = chr.FacetID;
                    land = Convert.ToInt16(sArgs[0]);
                    map = Convert.ToInt16(sArgs[1]);
                    x = Convert.ToInt32(sArgs[2]);
                    y = Convert.ToInt32(sArgs[3]);
                    z = Convert.ToInt32(sArgs[4]);

                    if (sArgs.Length >= 6)
                    {
                        chr.WriteToDisplay("Facet number no longer required, as this is currently always 0.");
                        chr.WriteToDisplay("Format: psitel <land> <map> <ycord> <xcord> <zcord>");
                        return true;
                    }

                    Cell pCell = Cell.GetCell(facet, land, map, x, y, z);

                    if (pCell == null || pCell.IsOutOfBounds)
                    {
                        chr.WriteToDisplay("That place is outside of the map bounds.");
                        return true;
                    }
                    else
                    {
                        if ((chr as PC).ImpLevel < Globals.eImpLevel.GM)
                        {
                            if (pCell.IsLair || pCell.IsNoRecall)
                            {
                                chr.WriteToDisplay("You do not have sufficient privilege level to teleport directly into a lair or no recall zone.");
                                return true;
                            }

                            if (chr.CurrentCell != null && (chr.CurrentCell.IsLair || chr.CurrentCell.IsNoRecall))
                            {
                                chr.WriteToDisplay("You do not have sufficient privilege level to teleport out of a lair or no recall zone.");
                                return true;
                            }

                            if (chr.WhichHand("corpse") > (int)Globals.eWearOrientation.None)
                            {
                                chr.WriteToDisplay("You do not have sufficient privilege level to teleport while carrying a corpse.");
                                return true;
                            }
                        }

                        if (!chr.IsInvisible)
                            chr.SendToAllInSight(chr.GetNameForActionResult() + " disappears in a poof of smoke and amazing huzzah!");

                        chr.CurrentCell = Cell.GetCell(facet, land, map, x, y, z);

                        if (!chr.IsInvisible)
                        {
                            chr.SendShout("sizzling followed by a thunderclap!");
                            chr.SendToAllInSight(chr.GetNameForActionResult() + " appears with a puff of smoke and huzzah.");
                            chr.WriteToDisplay("You hear sizzling followed by a thunderclap!");
                        }
                    }
                }
                catch
                {
                    chr.WriteToDisplay("Facet number no longer required, as this is currently always 0.");
                    chr.WriteToDisplay("Format: psitel <land> <map> <ycord> <xcord> <zcord>");
                }
            }    

            return true;
        }
    }
}
