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
    [CommandAttribute("impitemsearch", "Search for a specific item in the game world.", (int)Globals.eImpLevel.DEVJR, new string[] { "isearch" },
        0, new string[] { "impitemsearch <item ID>" }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class ImpItemSearchCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args.Equals(null) || args == "")
            {
                chr.WriteToDisplay("Format: isearch <item ID>");
                return true;
            }

            const int MAX_DISPLAYED_FINDS = 25;

            string[] sArgs = args.Split(" ".ToCharArray());

            if (sArgs.Length == 1)
            {
                try
                {
                    int itemID = Convert.ToInt32(sArgs[0]);

                    int found = 0;

                    foreach (NPC npc in NPC.NPCInGameWorld)
                    {
                        if (found >= MAX_DISPLAYED_FINDS)
                        {
                            found++;
                            continue;
                        }

                        if (npc.lairCellsList.Count > 0)
                        {
                            foreach (Cell cell in npc.lairCellsList)
                            {
                                foreach (Item item in cell.Items)
                                {
                                    if (item.itemID == itemID)
                                    {
                                        found++;
                                        chr.WriteToDisplay(found + ". " + npc.GetLogString() + " has " + item.GetLogString() + " in one of its lair cells.");
                                    }
                                }
                            }
                        }

                        foreach (Item item in npc.wearing)
                        {
                            if (item.itemID == itemID)
                            {
                                found++;
                                chr.WriteToDisplay(found + ". " + npc.GetLogString() + " is wearing " + item.GetLogString());
                            }
                        }

                        foreach (Item ring in npc.GetRings())
                        {
                            if (ring.itemID == itemID)
                            {
                                found++;
                                chr.WriteToDisplay(found + ". " + npc.GetLogString() + " is wearing the ring " + ring.GetLogString());
                            }
                        }

                        foreach (Item item in npc.beltList)
                        {
                            if (item.itemID == itemID)
                            {
                                found++;
                                chr.WriteToDisplay(found + ". " + npc.GetLogString() + " has " + item.GetLogString() + " on its belt.");
                            }
                        }

                        foreach (Item item in npc.sackList)
                        {
                            if (item.itemID == itemID)
                            {
                                found++;
                                chr.WriteToDisplay(found + ". " + npc.GetLogString() + " has " + item.GetLogString() + " in its sack.");
                            }
                        }

                        foreach (Item item in npc.pouchList)
                        {
                            if (item.itemID == itemID)
                            {
                                found++;
                                chr.WriteToDisplay(found + ". " + npc.GetLogString() + " has " + item.GetLogString() + " in its pouch.");
                            }
                        }

                        foreach (GameQuest quest in npc.QuestList)
                        {
                            foreach(int id in quest.RewardItems.Values)
                            {
                                if (id == itemID)
                                {
                                    found++;
                                    chr.WriteToDisplay(found + ". " + npc.GetLogString() + " has Item ID " + itemID + " in its quest rewards for Quest: " + quest.Name);
                                }
                            }
                        }

                        if(npc.lairCellsList.Count > 0)
                        {
                            foreach(Cell cell in npc.lairCellsList)
                            {
                                foreach(Item item in cell.Items)
                                {
                                    if(item.itemID == itemID)
                                    {
                                        found++;
                                        chr.WriteToDisplay(found + ". " + npc.GetLogString() + " has " + item.GetLogString() + " in its lair.");
                                    }
                                }
                            }
                        }

                        if(npc.RightHand != null && npc.RightHand.itemID == itemID)
                        {
                            found++;
                            chr.WriteToDisplay(found + ". " + npc.GetLogString() + " has Item ID " + itemID + " in its right hand.");
                        }

                        if (npc.LeftHand != null && npc.LeftHand.itemID == itemID)
                        {
                            found++;
                            chr.WriteToDisplay(found + ". " + npc.GetLogString() + " has Item ID " + itemID + " in its left hand.");
                        }
                    }

                    chr.WriteToDisplay("Found " + found.ToString() + " total instances of item ID " + itemID + ".");
                }
                catch
                {
                    chr.WriteToDisplay("Format: isearch <item ID>");
                }
            }
            return true;
        }
    }
}
