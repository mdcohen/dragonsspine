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
using DragonsSpine.GameWorld;

namespace DragonsSpine.Commands
{
    [CommandAttribute("take", "Take an item from the ground or from a container or location.", (int)Globals.eImpLevel.USER, new string[] { "t", "get" },
        1, new string[] { "take <item>", "take <item> from <location>" }, Globals.ePlayerState.PLAYING)]
    public class TakeCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            int taketype = 0;

            string takeitem = "";

            string takefrom = "";

            int takenum = 0;

            Item item = new Item();

            if (args == null)
            {
                chr.WriteToDisplay("Take what?");
                return true;
            }
            else
            {
                String[] sArgs = args.Split(" ".ToCharArray());
                if (sArgs.GetLength(0) < 2)
                {
                    taketype = 1; // take <item>
                    takeitem = sArgs[0].ToLower();
                }
                else if (sArgs.Length == 2)
                {
                    taketype = 2; // Take # <item>
                    try
                    {
                        takenum = Convert.ToInt32(sArgs[0]);
                        takeitem = sArgs[1].ToLower();
                    }
                    catch (Exception)
                    {
                        chr.WriteToDisplay("Useage: Take <#> item");
                        return true;
                    }
                }
                else if (sArgs[1].ToLower().Equals("from"))
                {
                    if (sArgs[2] == null)
                    {
                        chr.WriteToDisplay("Take " + sArgs[0] + " from what?");
                        return true;
                    }
                    else
                    {
                        taketype = 3; // take <item> from <something>
                        takeitem = sArgs[0].ToLower();
                        takefrom = sArgs[2].ToLower();
                    }
                }
                else if (sArgs[0].ToLower().Equals("ring") && sArgs[1].ToLower().Equals("off"))
                {	//Trap "Take Ring off left/right"
                    return CommandTasker.ParseCommand(chr, "remove", args);
                }
                else if (sArgs[1].ToLower().Equals("ring") && sArgs[2].ToLower().Equals("off"))
                {	//Trap "Take x Ring off left/right"
                    return CommandTasker.ParseCommand(chr, "remove", args);
                }
                else if (sArgs[2].ToLower().Equals("from"))
                {
                    if (sArgs[3] == null)
                    {
                        chr.WriteToDisplay("Take " + sArgs[1] + " from what?");
                        return true;
                    }
                    else
                    {
                        taketype = 4; // take # <item> from <something>
                        takeitem = sArgs[1].ToLower();
                        takefrom = sArgs[3].ToLower();
                        takenum = Convert.ToInt32(sArgs[0]);
                    }
                }
                else
                {
                    taketype = 0; // Default type - nothing/catchall
                }
            }
            // Now we switch on the taketype
            switch (taketype)
            {
                case 0: // default type
                    chr.WriteToDisplay("Take what?");
                    break;
                case 1: // take <item>

                    // check if an itemID number was sent as the argument, to help AI find a specific item...
                    #region take item by using item ID
                    bool isNumber = int.TryParse(takeitem, out int uniqueWorldID);
                    if (isNumber)
                    {
                        if (Item.IsItemOnGround(uniqueWorldID, chr.CurrentCell))
                        {
                            Item tItem = Item.FindItemOnGround(uniqueWorldID, chr.CurrentCell);

                            if (chr.EquipEitherHand(tItem))
                                chr.CurrentCell.Remove(tItem);

                            return true;
                        }
                        else
                        {
                            chr.WriteToDisplay(GameSystems.Text.TextManager.NullItemMessage(takeitem));
                            return true;
                        }
                    }
                    #endregion

                    if (Item.IsItemOnGround(takeitem.ToLower(), chr.FacetID, chr.LandID, chr.MapID, chr.X, chr.Y, chr.Z))
                    {
                        for (int a = 0; a < chr.CurrentCell.Items.Count; a++)
                        {
                            Item tItem = (Item)chr.CurrentCell.Items[a];
                            if (tItem.name.ToLower() == takeitem.ToLower())
                            {
                                if (chr.EquipEitherHand(tItem))
                                {
                                    chr.CurrentCell.Remove(tItem);
                                }
                                return true;
                            }
                        }
                    }
                    else
                    {
                        chr.WriteToDisplay(GameSystems.Text.TextManager.NullItemMessage(takeitem));
                        return true;
                    }
                    break;
                case 2: // take # <item>
                    int z = 0;
                    if (takeitem.IndexOf("coin") > -1 || takeitem == "gold")
                    {
                        item = Item.FindItemOnGround(takeitem, chr.FacetID, chr.LandID, chr.MapID, chr.X, chr.Y, chr.Z);
                        foreach (Item itm in chr.CurrentCell.Items)
                        {
                            if (itm.name.ToLower().IndexOf("coin") > -1)
                            {
                                item = itm;
                            }
                        }
                        if (item == null)
                        {
                            chr.WriteToDisplay("You do not see any coins here.");
                            return true;
                        }
                        else if (takenum <= 0)
                        {
                            chr.WriteToDisplay("You cannot do that.");
                            return true;
                        }
                        else if (item.coinValue < takenum)
                        {
                            chr.WriteToDisplay("There aren't that many coins here.");
                            return true;
                        }
                        else if (item.coinValue == takenum)
                        {
                            if (chr.EquipEitherHand(item))
                            {
                                chr.CurrentCell.Remove(item);
                            }
                            return true;
                        }
                        else
                        {
                            if (chr.RightHand == null)
                            {
                                item.coinValue -= takenum;
                                chr.EquipRightHand(Item.CopyItemFromDictionary(Item.ID_COINS));
                                chr.RightHand.coinValue = takenum;
                                return true;
                            }
                            else if (chr.LeftHand == null)
                            {
                                item.coinValue -= takenum;
                                chr.EquipLeftHand(Item.CopyItemFromDictionary(Item.ID_COINS));
                                chr.LeftHand.coinValue = takenum;
                                return true;
                            }
                            else
                            {
                                chr.WriteToDisplay("Your hands are full.");
                                return true;
                            }
                        }
                    }

                    List<Item> tmpList = chr.CurrentCell.Items;

                    if (takenum > tmpList.Count) { chr.WriteToDisplay(GameSystems.Text.TextManager.NullItemMessage(null)); return true; }

                    z = tmpList.Count - 1;

                    int itemcount = 0;

                    foreach (Item tItem in tmpList)
                    {
                        Item tmpitem = tItem;
                        if (tmpitem.name == takeitem)
                        {
                            itemcount++;
                            if (takenum == itemcount)
                            {
                                if (tmpitem.IsArtifact() && chr.PossessesItem(tmpitem.itemID))
                                {
                                    chr.WriteToDisplay("You already possess this artifact.");
                                    return true;
                                }

                                if (chr.EquipEitherHand(tItem))
                                {
                                    chr.CurrentCell.Remove(tItem);
                                }
                                return true;
                            }
                        }
                    }
                    chr.WriteToDisplay("There aren't that many " + takeitem + "'s here.");
                    break;
                case 3: // take <item> from <something>
                    if (chr.RightHand != null && chr.LeftHand != null)
                    {
                        chr.WriteToDisplay("Your hands are full.");
                        return true;
                    }
                    else if (takefrom.ToLower() == "sack" || takefrom.ToLower() == "sac" || takefrom.ToLower() == "sakc")
                    {
                        item = chr.RemoveFromSack(takeitem.ToLower());
                    }
                    else if (takefrom.ToLower() == "pouch" || takefrom.ToLower() == "pou")
                    {
                        item = chr.RemoveFromPouch(takeitem.ToLower());
                    }
                    else if (takefrom.ToLower() == "locker")
                    {
                        if (chr.CurrentCell.IsLocker)
                        {
                            item = chr.RemoveFromLocker(takeitem.ToLower());
                        }
                        else
                        {
                            chr.WriteToDisplay("There are no lockers here.");
                            return true;
                        }
                    }
                    else if (takefrom.ToLower() == "counter" || takefrom.ToLower() == "altar")
                    {
                        if(Map.IsNextToCounter(chr))
                            item = Map.RemoveItemFromCounter(chr, takeitem.ToLower());
                        else
                        {
                            chr.WriteToDisplay("There is no " + takefrom.ToLower() + " here.");
                        }

                        if (item == null || item.name.ToLower() == "undefined")
                        {
                            chr.WriteToDisplay("There is no " + takefrom.ToLower() + " here.");
                        }
                    }
                    else if (takefrom.ToLower() == "ground")
                    {
                        return CommandTasker.ParseCommand(chr, "take", takeitem);
                    }
                    else
                    {
                        chr.WriteToDisplay("I don't understand that command.");
                        break;
                    }

                    if (item != null)
                    {
                        chr.EquipEitherHand(item);
                    }
                    else
                    {
                        chr.WriteToDisplay("The " + takefrom + " does not have a " + takeitem + " in it.");
                    }
                    break;
                case 4: // take # <item> from <something>
                    List<Item> tmplist;
                    switch (takefrom)
                    {
                        case "ground":
                            tmplist = chr.CurrentCell.Items;
                            break;
                        case "sack":
                            tmplist = chr.sackList;
                            break;
                        case "pouch":
                            tmplist = chr.pouchList;
                            break;
                        case "locker":
                            if (!chr.CurrentCell.IsLocker)
                            {
                                chr.WriteToDisplay("There are no lockers here.");
                                return true;
                            }
                            tmplist = chr.lockerList;
                            break;
                        case "counter":
                        case "altar":
                            Cell curCell = Map.GetNearestCounterOrAltarCell(chr.CurrentCell);
                            tmplist = curCell.Items;
                            if (tmplist == null)
                            {
                                chr.WriteToDisplay("I see no " + takefrom + " here.");
                                return true;
                            }
                            break;
                        default:
                            chr.WriteToDisplay("I don't understand that command.");
                            return true;
                    }
                    if (takeitem.IndexOf("coin") > -1 || takeitem == "gold")
                    {
                        foreach (Item itm in tmplist)
                        {
                            if (itm.name.ToLower().IndexOf("coin") > -1)
                            {
                                item = itm;
                            }
                        }
                        if (item == null || item.name.ToLower() == "undefined")
                        {
                            chr.WriteToDisplay(GameSystems.Text.TextManager.NullItemMessage(takeitem));
                            return true;
                        }
                        else if (takenum <= 0)
                        {
                            chr.WriteToDisplay("You cannot do that.");
                            return true;
                        }
                        else if (item.coinValue < takenum)
                        {
                            chr.WriteToDisplay("There aren't that many coins here.");
                            return true;
                        }
                        else if (item.coinValue == takenum)
                        {
                            if (chr.EquipEitherHand(item))
                            {
                                tmplist.Remove(item);
                            }
                            return true;
                        }
                        else
                        {
                            if (chr.RightHand == null)
                            {
                                item.coinValue -= takenum;
                                chr.EquipRightHand(Item.CopyItemFromDictionary(Item.ID_COINS));
                                chr.RightHand.coinValue = takenum;
                                return true;
                            }
                            else if (chr.LeftHand == null)
                            {
                                item.coinValue -= takenum;
                                chr.EquipLeftHand(Item.CopyItemFromDictionary(Item.ID_COINS));
                                chr.LeftHand.coinValue = takenum;
                                return true;
                            }
                            else
                            {
                                chr.WriteToDisplay("Your hands are full.");
                                return true;
                            }
                        }
                    }
                    if (takenum > tmplist.Count) { chr.WriteToDisplay(GameSystems.Text.TextManager.NullItemMessage(null)); return true; }
                    int itmcount = 0;
                    foreach (Item tItem in tmplist)
                    {
                        Item tmpitem = tItem;
                        if (tmpitem.name == takeitem)
                        {
                            itmcount++;
                            if (takenum == itmcount)
                            {
                                if (chr.EquipEitherHand(tItem))
                                {
                                    tmplist.Remove(tItem);
                                    return true;
                                }
                                else
                                {
                                    chr.WriteToDisplay("Your hands are full.");
                                    return true;
                                }
                            }
                        }
                    }
                    chr.WriteToDisplay("There are not that many " + takeitem + "s here.");
                    break;
                default:
                    break;
            }

            return true;
        }
    }
}
