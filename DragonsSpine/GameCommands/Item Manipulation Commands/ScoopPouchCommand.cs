using System;
using DragonsSpine.GameWorld;

namespace DragonsSpine.Commands
{
    [CommandAttribute("pouchscoop", "Scoop an item or all similar items into your pouch from the ground or a counter/altar.", (int)Globals.eImpLevel.USER, new string[] { "pscoop" },
        1, new string[] { "pscoop <item>", "pscoop all <items>", "pscoop <item> from <location>", "pscoop all <items> from <location>" }, Globals.ePlayerState.PLAYING)]
    public class ScoopCommand : ICommandHandler
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
                                if (tItem.size == Globals.eItemSize.Pouch_Only ||
                                    tItem.size == Globals.eItemSize.Sack_Or_Pouch)
                                {
                                    if (chr.pouchList.Count >= Character.MAX_POUCH)
                                    {
                                        chr.WriteToDisplay("Your pouch is full.");
                                        return true;
                                    }

                                    if (chr.PouchItem(tItem))
                                    {
                                        chr.CurrentCell.Remove(tItem);
                                        chr.WriteToDisplay("You scoop the " + tItem.name + " into your pouch.");
                                        return true;
                                    }
                                    else
                                    {
                                        chr.WriteToDisplay("Your pouch is full.");
                                    }
                                }
                                else
                                {
                                    chr.WriteToDisplay("The " + tItem.name + " is too large to fit into your pouch.");
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
                    int pouchCount = chr.pouchList.Count;
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

                    while (pouchCount < Character.MAX_POUCH)
                    {
                        foreach (Item tItem in new System.Collections.Generic.List<Item>(chr.CurrentCell.Items))
                        {
                            if (tItem.IsArtifact() && chr.PossessesItem(tItem.itemID))
                            {
                                chr.WriteToDisplay(GameSystems.Text.TextManager.ARTIFACT_POSSESSED);
                                continue;
                            }

                            if (tItem.name.ToLower().StartsWith(itemName) &&
                                (tItem.size == Globals.eItemSize.Pouch_Only || tItem.size == Globals.eItemSize.Sack_Or_Pouch))
                            {
                                if (chr.PouchItem(tItem))
                                {
                                    pouchCount++;
                                    successCount++;
                                    chr.CurrentCell.Remove(tItem);
                                }
                            }
                        }
                        break;
                    }

                    if (successCount == 0)
                    {
                        if (pouchCount >= Character.MAX_POUCH)
                        {
                            chr.WriteToDisplay("Your pouch is full.");
                        }
                        else
                        {
                            chr.WriteToDisplay(GameSystems.Text.TextManager.NullItemMessage(itemName + "s"));
                        }
                        return true;
                    }
                    else if (successCount == 1)
                        chr.WriteToDisplay("You scoop 1 " + itemName + " into your pouch.");
                    else
                        chr.WriteToDisplay("You scoop " + successCount + " " + itemName + "s into your pouch.");
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
                                if (tItem.size == Globals.eItemSize.Pouch_Only || tItem.size == Globals.eItemSize.Sack_Or_Pouch)
                                {
                                    if (chr.pouchList.Count >= Character.MAX_POUCH)
                                    {
                                        chr.WriteToDisplay("Your pouch is full.");
                                        return true;
                                    }

                                    if (chr.PouchItem(tItem))
                                    {
                                        counterCell.Remove(tItem);
                                        chr.WriteToDisplay("You scoop the " + tItem.name + " into your pouch.");
                                        return true;
                                    }
                                }
                                else
                                {
                                    chr.WriteToDisplay("The " + tItem.name + " is too large to fit into your pouch.");
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

                    int pouchCount = chr.pouchList.Count;

                    string itemName = sArgs[1].ToLower();

                    if (itemName.EndsWith("s"))
                        itemName = itemName.Substring(0, itemName.Length - 1);

                    if (itemName == "coin")
                    {
                        return new CommandTasker(chr)["scoop", "coins from counter"];
                    }

                    if (pouchCount >= Character.MAX_POUCH)
                    {
                        chr.WriteToDisplay("Your pouch is full.");
                        return true;
                    }

                    int successCount = 0;

                    while (pouchCount < Character.MAX_POUCH)
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
                                (tItem.size == Globals.eItemSize.Pouch_Only || tItem.size == Globals.eItemSize.Sack_Or_Pouch))
                            {
                                if (chr.PouchItem(tItem))
                                {
                                    pouchCount++;
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
                        chr.WriteToDisplay("You scoop 1 " + itemName + " into your pouch from the " + sArgs[3].ToLower() + ".");
                    else
                        chr.WriteToDisplay("You scoop " + successCount + " " + itemName + "s into your pouch from the " + sArgs[3].ToLower() + ".");
                    #endregion
                }
                else
                {
                    chr.WriteToDisplay("Usage for the scoop command:");
                    chr.WriteToDisplay("\"pscoop <item>\" (from ground)");
                    chr.WriteToDisplay("\"pscoop <left | right>\"");
                    chr.WriteToDisplay("\"pscoop <item> from <counter | altar>\"");
                    chr.WriteToDisplay("\"pscoop all <item>\" (from ground)");
                    chr.WriteToDisplay("\"pscoop all <item> from <counter | altar>\"");
                }
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                chr.WriteToDisplay("Usage for the scoop command:");
                chr.WriteToDisplay("\"pscoop <item>\" (from ground)");
                chr.WriteToDisplay("\"pscoop <left | right>\"");
                chr.WriteToDisplay("\"pscoop <item> from <counter | altar>\"");
                chr.WriteToDisplay("\"pscoop all <item>\" (from ground)");
                chr.WriteToDisplay("\"pscoop all <item> from <counter | altar>\"");
            }

            return true;
        }
    }
}
