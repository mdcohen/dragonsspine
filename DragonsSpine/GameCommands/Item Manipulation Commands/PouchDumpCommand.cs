using System;
using Map = DragonsSpine.GameWorld.Map;

namespace DragonsSpine.Commands
{
    [CommandAttribute("pouchdump", "Dump an item or items on the ground or on a counter/altar.", (int)Globals.eImpLevel.USER, new string[] {"pdump", "pd" }, 1,
        new string[] { "pouchdump <item>", "pouchdump all <item>", "pouchdump <item> on counter", "pouchdump all <item> on counter" }, Globals.ePlayerState.PLAYING)]
    public class PouchDumpCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.CommandWeight > 3)
            {
                return true;
            }

            // args are null, send explanation of the dump command
            if (args == null)
            {
                chr.WriteToDisplay("Usage for the pouchdump command:");
                chr.WriteToDisplay("\"pouchdump <item>\" - dump one <item> from pouch onto ground");
                chr.WriteToDisplay("\"pouchdump <item> on <counter | altar>\" - dump one <item> from pouch onto counter or altar");
                chr.WriteToDisplay("\"pouchdump <item> in locker\" - dump one <item> from pouch into locker");
                chr.WriteToDisplay("\"pouchdump all <items>\" - dump all <items> from pouch onto ground");
                chr.WriteToDisplay("\"pouchdump all <items> on <counter | altar>\" - dump all items from pouch onto counter or altar");
                chr.WriteToDisplay("\"pouchdump all <items> in locker\" - dump all items from pouch into locker");
                return true;
            }

            String[] sArgs = args.Split(" ".ToCharArray());

            int upperBound = chr.pouchList.Count;

            try
            {
                #region pouchdump <item>
                if (sArgs.Length == 1)  // "dump <item>" should just dump one item on the ground or send error message, then return
                {
                    bool itemDumped = false;

                    foreach (Item item in new System.Collections.Generic.List<Item>(chr.pouchList))
                    {
                        if (item.name == sArgs[0].Remove(sArgs[0].Length - 1, 1) || item.name == sArgs[0])
                        {
                            chr.CurrentCell.Add(item);
                            chr.pouchList.Remove(item);
                            // In case coins may be stored in pouches eventually.
                            if (item.itemType == Globals.eItemType.Coin)
                            {
                                if (item.coinValue > 1)
                                    chr.WriteToDisplay("You dumped " + item.coinValue + " coins on the ground.");
                                else
                                    chr.WriteToDisplay("You dumped " + item.coinValue + " coin on the ground.");
                            }
                            else chr.WriteToDisplay("You dumped " + item.shortDesc + " on the ground.");
                            itemDumped = true;
                            break; // dump only one item then break out of here
                        }
                    }

                    if (!itemDumped)
                        chr.WriteToDisplay("You do not have any " + sArgs[0] + " in your pouch.");

                    return true;
                }
                #endregion

                #region pouchdump all <item> || pouchdump all <item> <prep> <location>
                // "dump all <item>" or "dump all <item> <prep> <location>"
                if (sArgs.Length >= 2 && sArgs[0] == "all")
                {
                    double counter = 0; // counter is used to determine how many, if any, items were dumped (for result message)

                    // "pouchdump all <item>" will dump all <item> on ground
                    if (sArgs.Length == 2)
                    {
                        for (int x = upperBound - 1; x >= 0; x--)
                        {
                            Item item = (Item)chr.pouchList[x];

                            // argument matches
                            if (item.name == sArgs[1].Remove(sArgs[1].Length - 1, 1) || item.name == sArgs[1])
                            {
                                chr.CurrentCell.Add(item);
                                chr.pouchList.RemoveAt(x);
                                if (item.itemType == Globals.eItemType.Coin)
                                {
                                    counter = item.coinValue;
                                    break;
                                }
                                else counter++;
                            }
                        }

                        // send result message
                        if (counter == 0)
                        {
                            chr.WriteToDisplay("You do not have any " + sArgs[1] + " in your pouch.");
                        }
                        else
                        {
                            string plural = "";
                            if (counter > 1 && !sArgs[1].EndsWith("s")) plural = "s";
                            chr.WriteToDisplay("You dumped " + counter + " " + sArgs[1] + plural + " on the ground.");
                        }
                        return true;
                    }
                    else
                    {
                        Item item = null;
                        Item dumpedItem = null;

                        switch (sArgs[2])
                        {
                            case "on": // pouchdump all <item> on counter/altar

                                // if not standing near a counter or altar send error message and return
                                if (!Map.IsNextToCounter(chr))
                                {
                                    chr.WriteToDisplay("You are not standing near a counter or altar.");
                                    return true;
                                }

                                // iterate through pouchList
                                for (int x = upperBound - 1; x >= 0; x--)
                                {
                                    item = (Item)chr.pouchList[x];

                                    if (item.name == sArgs[1].Remove(sArgs[1].Length - 1, 1) || item.name == sArgs[1])
                                    {
                                        Map.PutItemOnCounter(chr, item);
                                        dumpedItem = item;
                                        chr.pouchList.RemoveAt(x);
                                        counter++;
                                    }
                                }

                                if (dumpedItem != null && dumpedItem.itemType == Globals.eItemType.Coin)
                                {
                                    if (dumpedItem.coinValue > 1)
                                    {
                                        chr.WriteToDisplay("You dumped " + dumpedItem.coinValue + " coins on the " + sArgs[3] + ".");
                                    }
                                    else
                                    {
                                        chr.WriteToDisplay("You dumped " + dumpedItem.coinValue + " coin on the " + sArgs[3] + ".");
                                    }
                                }
                                else
                                {
                                    if (counter == 0)
                                    {
                                        chr.WriteToDisplay("You do not have any " + sArgs[1] + " in your pouch.");
                                    }
                                    else if (counter == 1)
                                    {
                                        chr.WriteToDisplay("You dumped " + counter + " " + sArgs[1] + " on the " + sArgs[3] + ".");
                                    }
                                    else
                                    {
                                        string plural = "";
                                        if (!sArgs[1].EndsWith("s")) plural = "s";
                                        chr.WriteToDisplay("You dumped " + counter + " " + sArgs[1] + plural + " on the " + sArgs[3] + ".");
                                    }
                                }
                                break;
                            case "in": // dump all <item> in locker -- currently (4/5/2013) lockers are the only thing you can dump items IN
                                if (!chr.CurrentCell.IsLocker)
                                {
                                    chr.WriteToDisplay("You are not standing next to your locker.");
                                    return true;
                                }
                                else
                                {
                                    for (int x = upperBound - 1; x >= 0; x--)
                                    {
                                        item = (Item)chr.pouchList[x];

                                        // argument match
                                        if (item.name == sArgs[1].Remove(sArgs[1].Length - 1, 1) || item.name == sArgs[1])
                                        {
                                            if (item.itemType == Globals.eItemType.Coin)
                                            {
                                                chr.WriteToDisplay("You cannot store coins in your locker.");
                                                return true;
                                            }
                                            if (chr.lockerList.Count < Character.MAX_LOCKER)
                                            {
                                                chr.lockerList.Add(item);
                                                chr.pouchList.RemoveAt(x);
                                                counter++;
                                            }
                                            else
                                            {
                                                string plural = "";
                                                if (!sArgs[1].EndsWith("s")) plural = "s";
                                                chr.WriteToDisplay("You dumped " + counter + " " + sArgs[1] + plural + " into your locker. Your locker is full.");
                                                return true;
                                            }
                                        }
                                    }
                                    chr.WriteToDisplay("You dumped " + counter + " " + sArgs[1] + "s into your locker.");
                                }
                                break;
                            default: // by default, "dump all <item>" will dump all items to current cell
                                for (int x = upperBound - 1; x >= 0; x--)
                                {
                                    item = (Item)chr.pouchList[x];
                                    if (item.name == sArgs[0].Remove(sArgs[0].Length - 1, 1) || item.name == sArgs[0])
                                    {
                                        chr.CurrentCell.Add(item);
                                        chr.pouchList.RemoveAt(x);
                                        counter++;
                                    }
                                }
                                string plurals = "";
                                if (!sArgs[1].EndsWith("s")) plurals = "s";
                                chr.WriteToDisplay("You dumped " + counter + " " + sArgs[1] + plurals + " on the ground.");
                                break;
                        }
                    }
                    return true;
                }
                #endregion

                #region pouchdump <item> <prep> <location>
                switch (sArgs[1])
                {
                    case "on":
                        for (int x = upperBound - 1; x >= 0; x--)
                        {
                            Item item = (Item)chr.pouchList[x];

                            // argument match
                            if (item.name == sArgs[0].Remove(sArgs[0].Length - 1, 1) || item.name == sArgs[0])
                            {
                                if (Map.IsNextToCounter(chr))
                                {
                                    Map.PutItemOnCounter(chr, item);
                                    chr.pouchList.RemoveAt(x);
                                    chr.WriteToDisplay("You dumped " + item.shortDesc + " on the " + sArgs[2] + ".");
                                    break; // stop at one item because first argument is not "all"
                                }
                                else
                                {
                                    chr.CurrentCell.Add(item);
                                    chr.pouchList.RemoveAt(x);
                                    chr.WriteToDisplay("You dumped " + item.shortDesc + " on the ground.");
                                    break; // stop at one item because first argument is not "all"
                                }
                            }
                        }
                        break;
                    case "in": // pouchdump <item> in locker -- currently (4/5/2013) lockers are the only thing you can dump items IN
                        if (!chr.CurrentCell.IsLocker)
                        {
                            chr.WriteToDisplay("You are not standing next to your locker.");
                            return true;
                        }
                        else
                        {
                            if (chr.lockerList.Count >= Character.MAX_LOCKER)
                            {
                                chr.WriteToDisplay("Your locker is full.");
                                return true;
                            }

                            for (int x = upperBound - 1; x >= 0; x--)
                            {
                                Item item = (Item)chr.pouchList[x];

                                // argument match
                                if (item.name == sArgs[1].Remove(sArgs[0].Length - 1, 1) || item.name == sArgs[1])
                                {
                                    if (item.itemType == Globals.eItemType.Coin)
                                    {
                                        chr.WriteToDisplay("You cannot store coins in your locker.");
                                        break;
                                    }
                                    chr.lockerList.Add(item);
                                    chr.pouchList.RemoveAt(x);
                                    chr.WriteToDisplay("You dumped " + item.shortDesc + " in your locker.");
                                    break;
                                }
                            }
                        }
                        break;
                    default: // default is to dump item on the ground
                        for (int x = upperBound - 1; x >= 0; x--)
                        {
                            Item item = (Item)chr.pouchList[x];

                            if (item.name == sArgs[0].Remove(sArgs[0].Length - 1, 1) || item.name == sArgs[0])
                            {
                                chr.CurrentCell.Add(item);
                                chr.pouchList.RemoveAt(x);
                                chr.WriteToDisplay("You dumped " + item.shortDesc + " on the ground.");
                                break; // stop at one item because argument 1 was not "all"
                            }
                        }
                        break;
                }
                #endregion
            }
            catch (Exception e)
            {
                Utils.Log("Command.dump(" + args + ") by " + chr.GetLogString(), Utils.LogType.CommandFailure);
                Utils.LogException(e);
            }

            return true;
        }
    }
}
