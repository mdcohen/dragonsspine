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
    [CommandAttribute("impitemcatsearch", "Search for items in the item catalog.", (int)Globals.eImpLevel.DEVJR, new string[] { "icat", "itemcat" },
        0, new string[] { "icat", "icat <item name>", "icat <basetype>" }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class ImpItemCatalogSearchCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            string[] sArgs = args.Split(" ".ToCharArray());

            if (args.Equals(null) || args == "")
            {
                foreach (int itemID in Item.ItemDictionary.Keys)
                {
                    chr.WriteToDisplay(itemID + " " + Item.GetItemNotesFromItemDictionary(itemID) + " (" + Item.GetItemNameFromItemDictionary(itemID) + ")");
                }
                return true;
            }
            else if (sArgs.Length == 1)
            {
                int itemIDSearch = 0;

                if (sArgs[0].ToLower() == "count" || sArgs[0].ToLower() == "name" || sArgs[0].ToLower() == "names")
                {
                    #region Gathering counts of items with the same name.
                    chr.WriteToDisplay("Listing count of items in the currently loaded Item Dictionary...");

                    System.Collections.Generic.Dictionary<string, int> itemNameCounts = new System.Collections.Generic.Dictionary<string, int>();

                    System.Collections.Generic.List<string> itemNames = new System.Collections.Generic.List<string>();

                    foreach (int itemID in Item.ItemDictionary.Keys)
                    {
                        string name = Item.GetItemNameFromItemDictionary(itemID);

                        if (!itemNames.Contains(name))
                            itemNames.Add(Item.GetItemNameFromItemDictionary(itemID));

                        if (!itemNameCounts.ContainsKey(name))
                            itemNameCounts.Add(name, 1);
                        else
                        {
                            int count;
                            if (itemNameCounts.TryGetValue(name, out count))
                                itemNameCounts[name] = count + 1;
                        }
                    }

                    itemNames.Sort(); // should sort alphabetically

                    foreach (string name in itemNames)
                    {
                        if (itemNameCounts.ContainsKey(name))
                            chr.WriteToDisplay(name + " (" + itemNameCounts[name] + ")");
                    } 
                    #endregion
                }// else looking for an item ID
                else if (Int32.TryParse(sArgs[0], out itemIDSearch))
                {
                    if (Item.ItemDictionary.ContainsKey(itemIDSearch))
                    {
                        Item item = Item.CopyItemFromDictionary(itemIDSearch);

                        chr.WriteToDisplay("Item ID exists. (" + itemIDSearch + ") " + item.notes + " (" + item.name + ")");
                    }
                    else
                        chr.WriteToDisplay("Item ID does NOT exist. (" + itemIDSearch + ")");
                }
                else
                {
                    chr.WriteToDisplay("Searching for currently loaded Items in the ItemDictionary with '" + sArgs[0] + "'...");
                    chr.WriteToDisplay("This command checks the item name, the first letter of the item, and the baseType (such as dagger)");

                    try
                    {
                        foreach (int itemID in Item.ItemDictionary.Keys)
                        {
                            System.Data.DataRow row = Item.ItemDictionary[itemID];

                            if (row["name"].ToString() == sArgs[0] || row["name"].ToString().StartsWith(sArgs[0]) || row["baseType"].ToString().ToLower() == sArgs[0].ToLower().ToLower())
                            {
                                chr.WriteToDisplay(itemID + " [" + row["catalogID"].ToString() + "] " + row["identifiedName"].ToString() + " (" + row["notes"].ToString() + ")");
                            }
                        }
                    }
                    catch
                    {
                        chr.WriteToDisplay("Format: icat <item name> or icat <itemID>");
                    }
                }
            }
            return true;
        }
    }
}
