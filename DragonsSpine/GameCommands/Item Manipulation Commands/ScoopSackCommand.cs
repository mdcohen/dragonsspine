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
    [CommandAttribute("scoop", "Scoop an item or all similar items into your sack from the ground or a counter/altar.", (int)Globals.eImpLevel.USER, new string[] { "sc", "scoopsack" },
        1, new string[] { "scoop <item>", "scoop all <items>", "scoop <item> from <location>", "scoop all <items> from <location>" }, Globals.ePlayerState.PLAYING)]
    public class ScoopSackCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Scoop what?");
                return true;
            }

            if (chr.CommandWeight > 3)
            {
                return true;
            }

            try
            {
                if (args.ToLower() == "all coins from counter" || args.ToLower() == "all coins from altar")
                {
                    return new CommandTasker(chr)["scoop", args.Replace("all coins", "coins")];
                }

                string[] sArgs = args.Split(" ".ToCharArray());

                if (sArgs.Length == 1 && sArgs[0].ToLower() != "all")
                {
                    #region scoop <item>
                    if (Item.IsItemOnGround(sArgs[0].ToLower(), chr.FacetID, chr.LandID, chr.MapID, chr.X, chr.Y, chr.Z))
                    {
                        for (int a = 0; a < chr.CurrentCell.Items.Count; a++)
                        {
                            Item tItem = (Item)chr.CurrentCell.Items[a];

                            if (tItem.IsArtifact() && chr.PossessesItem(tItem.itemID))
                            {
                                chr.WriteToDisplay(GameSystems.Text.TextManager.ARTIFACT_POSSESSED);
                                continue;
                            }

                            if (tItem.name.ToLower() == sArgs[0].ToLower())
                            {
                                if (tItem.size == Globals.eItemSize.Sack_Only ||
                                    tItem.size == Globals.eItemSize.Belt_Or_Sack ||
                                    tItem.size == Globals.eItemSize.Sack_Or_Pouch)
                                {
                                    if (chr.SackCountMinusGold >= Character.MAX_SACK && tItem.itemType != Globals.eItemType.Coin)
                                    {
                                        chr.WriteToDisplay("Your sack is full.");
                                        return true;
                                    }

                                    if (chr.SackItem(tItem))
                                    {
                                        chr.CurrentCell.Remove(tItem);
                                        chr.WriteToDisplay("You scoop the " + tItem.name + " into your sack.");
                                        return true;
                                    }
                                    else
                                    {
                                        chr.WriteToDisplay("Your sack is full.");
                                    }
                                }
                                else
                                {
                                    chr.WriteToDisplay("The " + tItem.name + " is too large to fit into your sack.");
                                }
                            }
                        }
                    }
                    else
                    {
                        chr.WriteToDisplay(GameSystems.Text.TextManager.NullItemMessage(sArgs[0]));
                    }
                    #endregion
                }
                else if (sArgs.Length == 2 && sArgs[0].ToLower() == "all")
                {
                    #region scoop all <item>
                    int sackCount = chr.SackCountMinusGold;
                    string itemName = sArgs[1].ToLower();
                    int successCount = 0;
                    if (itemName.EndsWith("s"))
                        itemName = itemName.Substring(0, itemName.Length - 1);

                    if (itemName == "coin")
                    {
                        //ParseCommand(chr, "scoop", "coins");
                        chr.WriteToDisplay("Use 'scoop coins' instead.", ProtocolYuusha.TextType.System);
                        return true;
                    }

                    while (sackCount < Character.MAX_SACK)
                    {
                        foreach (Item tItem in new System.Collections.Generic.List<Item>(chr.CurrentCell.Items))
                        {
                            if (tItem.IsArtifact() && chr.PossessesItem(tItem.itemID))
                            {
                                chr.WriteToDisplay(GameSystems.Text.TextManager.ARTIFACT_POSSESSED);
                                continue;
                            }

                            if (tItem.name.ToLower().StartsWith(itemName) &&
                                (tItem.size == Globals.eItemSize.Sack_Only || tItem.size == Globals.eItemSize.Belt_Or_Sack || tItem.size == Globals.eItemSize.Sack_Or_Pouch))
                            {
                                if (chr.SackItem(tItem))
                                {
                                    sackCount++;
                                    successCount++;
                                    chr.CurrentCell.Remove(tItem);
                                }
                            }
                        }
                        break;
                    }

                    if (successCount == 0)
                    {
                        if (sackCount >= Character.MAX_SACK)
                        {
                            chr.WriteToDisplay("Your sack is full.");
                        }
                        else
                        {
                            chr.WriteToDisplay(GameSystems.Text.TextManager.NullItemMessage(itemName + "s"));
                        }
                        return true;
                    }
                    else if (successCount == 1)
                        chr.WriteToDisplay("You scoop 1 " + itemName + " into your sack.");
                    else
                        chr.WriteToDisplay("You scoop " + successCount + " " + itemName + "s into your sack.");
                    #endregion
                }
                else if (sArgs.Length == 3 && (sArgs[2].ToLower() == "counter" || sArgs[2].ToLower() == "altar"))
                {
                    #region scoop <item> from <counter | altar>
                    Cell counterCell = Map.GetNearestCounterOrAltarCell(chr.CurrentCell);
                    if (counterCell == null)
                    {
                        if (sArgs[2].ToLower() == "counter")
                            chr.WriteToDisplay("You are not near a counter.");
                        else chr.WriteToDisplay("You are not near an altar.");
                        return true;
                    }

                    if (Item.IsItemOnGround(sArgs[0].ToLower(), chr.FacetID, chr.LandID, chr.MapID, counterCell.X, counterCell.Y, counterCell.Z))
                    {
                        for (int a = 0; a < counterCell.Items.Count; a++)
                        {
                            Item tItem = counterCell.Items[a];

                            if (tItem.IsArtifact() && chr.PossessesItem(tItem.itemID))
                            {
                                chr.WriteToDisplay(GameSystems.Text.TextManager.ARTIFACT_POSSESSED);
                                continue;
                            }

                            if (tItem.name.ToLower() == sArgs[0].ToLower())
                            {
                                if (tItem.size == Globals.eItemSize.Sack_Only || tItem.size == Globals.eItemSize.Belt_Or_Sack || tItem.size == Globals.eItemSize.Sack_Or_Pouch)
                                {
                                    if (chr.SackCountMinusGold >= Character.MAX_SACK && tItem.itemType != Globals.eItemType.Coin)
                                    {
                                        chr.WriteToDisplay("Your sack is full.");
                                        return true;
                                    }

                                    if (chr.SackItem(tItem))
                                    {
                                        counterCell.Remove(tItem);
                                        chr.WriteToDisplay("You scoop the " + tItem.name + " into your sack.");
                                        return true;
                                    }
                                }
                                else
                                {
                                    chr.WriteToDisplay("The " + tItem.name + " is too large to fit into your sack.");
                                }
                            }
                        }
                    }
                    else
                    {
                        chr.WriteToDisplay(GameSystems.Text.TextManager.NullItemMessage(sArgs[0]));
                    }
                    #endregion
                }
                else if (sArgs.Length == 4 && sArgs[0].ToLower() == "all" && (sArgs[3].ToLower() == "counter" || sArgs[3].ToLower() == "altar"))
                {
                    #region scoop all <item> from <counter | altar>
                    Cell counterCell = Map.GetNearestCounterOrAltarCell(chr.CurrentCell);
                    if (counterCell == null)
                    {
                        if (sArgs[2].ToLower() == "counter")
                            chr.WriteToDisplay("You are not near a counter.");
                        else chr.WriteToDisplay("You are not near an altar.");
                        return true;
                    }

                    int sackCount = chr.SackCountMinusGold;

                    string itemName = sArgs[1].ToLower();

                    if (itemName.EndsWith("s"))
                        itemName = itemName.Substring(0, itemName.Length - 1);

                    if (itemName == "coin")
                    {
                        return new CommandTasker(chr)["scoop", "coins from counter"];
                    }

                    if (sackCount >= Character.MAX_SACK)
                    {
                        chr.WriteToDisplay("Your sack is full.");
                        return true;
                    }

                    int successCount = 0;

                    while (sackCount < Character.MAX_SACK)
                    {
                        for (int a = counterCell.Items.Count - 1; a >= 0; a--)
                        {
                            Item tItem = counterCell.Items[a];

                            if (tItem.IsArtifact() && chr.PossessesItem(tItem.itemID))
                            {
                                chr.WriteToDisplay(GameSystems.Text.TextManager.ARTIFACT_POSSESSED);
                                continue;
                            }

                            if (tItem.name.ToLower().StartsWith(itemName) &&
                                (tItem.size == Globals.eItemSize.Sack_Only || tItem.size == Globals.eItemSize.Belt_Or_Sack || tItem.size == Globals.eItemSize.Sack_Or_Pouch))
                            {
                                if (chr.SackItem(tItem))
                                {
                                    sackCount++;
                                    successCount++;
                                    counterCell.Remove(tItem);
                                }
                            }
                        }
                        break;
                    }

                    if (successCount == 0)
                    {
                        chr.WriteToDisplay("You don't see any " + itemName + "s on the " + sArgs[3].ToLower() + ".");
                        return true;
                    }
                    else if (successCount == 1)
                        chr.WriteToDisplay("You scoop 1 " + itemName + " into your sack from the " + sArgs[3].ToLower() + ".");
                    else
                        chr.WriteToDisplay("You scoop " + successCount + " " + itemName + "s into your sack from the " + sArgs[3].ToLower() + ".");
                    #endregion
                }
                else
                {
                    chr.WriteToDisplay("Usage for the scoop command:");
                    chr.WriteToDisplay("\"scoop <item>\" (from ground)");
                    chr.WriteToDisplay("\"scoop <left | right>\"");
                    chr.WriteToDisplay("\"scoop <item> from <counter | altar>\"");
                    chr.WriteToDisplay("\"scoop all <item>\" (from ground)");
                    chr.WriteToDisplay("\"scoop all <item> from <counter | altar>\"");
                }
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                chr.WriteToDisplay("Usage for the scoop command:");
                chr.WriteToDisplay("\"scoop <item>\" (from ground)");
                chr.WriteToDisplay("\"scoop <left | right>\"");
                chr.WriteToDisplay("\"scoop <item> from <counter | altar>\"");
                chr.WriteToDisplay("\"scoop all <item>\" (from ground)");
                chr.WriteToDisplay("\"scoop all <item> from <counter | altar>\"");
            }

            return true;
        }
    }
}
