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
using System.Collections.Generic;

namespace DragonsSpine.Commands
{
    [CommandAttribute("inventory", "Display a list of inventory items.", (int)Globals.eImpLevel.USER, new string[] { "inv", "show inventory", "show inv", "show i", "showinv", "showinventory" },
        0, new string[] { "There are no arguments for the inventory command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowInventoryCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            Item[] inventory = new Item[chr.wearing.Count];

            chr.wearing.CopyTo(inventory);

            // first string is inventory slot, second is name of item to be displayed
            Dictionary<string, string> completeInventory = new Dictionary<string, string>();

            // Create a complete inventory slot dictionary first. Globals.eWearLocation[0] is None.
            for (int a = 1; a < Enum.GetValues(typeof(Globals.eWearLocation)).GetLength(0) - 1; a++)
            {
                // Currently only right and left inventory slots (rings are an exception and are not handled here - show rings command)
                if (Globals.Max_Wearable[a] == 2)
                {
                    completeInventory.Add(Globals.eWearOrientation.Left.ToString() + " " + (Globals.eWearLocation)a, "");
                    completeInventory.Add(Globals.eWearOrientation.Right.ToString() + " " + (Globals.eWearLocation)a, "");
                }
                else if (Globals.Max_Wearable[a] == 1)
                {
                    completeInventory.Add(Convert.ToString((Globals.eWearLocation)a), "");
                }
            }

            // Iterate through each item being worn and add it to the local Dictionary variable completeDictionary.
            foreach (Item item in chr.wearing)
            {
                if (item.wearOrientation > Globals.eWearOrientation.None)
                {
                    if (item.identifiedList.Contains(chr.UniqueID))
                    {
                        completeInventory[Convert.ToString(item.wearOrientation) + " " + item.wearLocation] = item.identifiedName;
                    }
                    else completeInventory[Convert.ToString(item.wearOrientation) + " " + item.wearLocation] = item.unidentifiedName;

                }
                else
                {
                    if (item.identifiedList.Contains(chr.UniqueID))
                    {
                        completeInventory[Convert.ToString(item.wearLocation)] = item.identifiedName;
                    }
                    else completeInventory[Convert.ToString(item.wearLocation)] = item.unidentifiedName;
                }

            }

            // Create a list for easy sorting.
            List<string> list = new List<string>();

            foreach (string key in completeInventory.Keys)
            {
                list.Add("[" + key.ToString() + "] " + completeInventory[key]);
            }

            list.Sort();

            string col1 = "";
            string col2 = "";

            for (int a = 0; a < list.Count; a++)
            {
                if (a % 2 == 0)
                {
                    col1 = list[a].PadRight(37);
                    if (a == list.Count - 1)
                        chr.WriteToDisplay(col1);
                }
                else
                {
                    col2 = list[a];
                    chr.WriteToDisplay(col1 + " " + col2);
                }
            }

            return true;
        }
    }
}
