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
using System.Collections;
using System.Collections.Generic;
using DragonsSpine.GameWorld;
using TargetAcquisition = DragonsSpine.GameSystems.Targeting.TargetAquisition;

namespace DragonsSpine.Commands
{
    [CommandAttribute("look", "Display a list of belt items.", (int)Globals.eImpLevel.USER, new string[] { "l" },
        0, new string[] { "look <direction>", "look at <name>", "look up", "look around" }, Globals.ePlayerState.PLAYING)]
    public class LookCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            try
            {
                if (chr.IsBlind) // character is blind
                {
                    chr.WriteToDisplay("You are blind and cannot see!");
                    return true;
                }
                else if (!chr.IsImmortal && !chr.HasNightVision && (chr.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) || chr.CurrentCell.IsAlwaysDark))
                {
                    chr.WriteToDisplay("You cannot see anything in the darkness.");
                    return true;
                }

                chr.CommandWeight = 0;
                string lookTarget = null;
                string lookin = null;
                string lookNum = null;
                string lookPrep = null;

                // 0 = cell description and item list
                // 1 = look IN something
                // 2 = look AT something
                // 3 = look in a direction
                // 4 = 
                // 5 = look at # item in something
                // 6 = look at # <creature>
                // 7 = look ON something

                int lookType = 0;
                int countTo = 0;
                bool lookClosely = false;
                bool lookVeryClosely = false;

                #region look around
                if (args != null && args.ToLower() == "around")//mlt tweaked for more original usage [here]
                {
                    string reltoviewer = "";
                    string signdesc = "";
                    string result = "Today is " + World.CurrentDay + ", the " + Utils.ConvertNumberToString(Array.IndexOf(World.DaysOfTheWeek, World.CurrentDay) + 1) + " day of the week.";
                    Cell lcell = null;

                    // display the state of the sun or moon if outdoors
                    if (chr.CurrentCell.IsOutdoors)
                    {
                        string skydesc = "";
                        switch (World.CurrentDailyCycle)
                        {
                            case World.DailyCycle.Morning: skydesc = " The sun is rising"; break;
                            case World.DailyCycle.Afternoon: skydesc = " The sun is high in the afternoon sky"; break;
                            case World.DailyCycle.Evening: skydesc = " The sun is setting"; break;
                            default: skydesc = " It is night time"; break;
                        }

                        if (World.CurrentDailyCycle != World.DailyCycle.Afternoon)
                        {
                            skydesc += ". The moon is " + Utils.FormatEnumString(World.CurrentLunarCycle.ToString()).ToLower();
                        }

                        result += "" + skydesc + ".\n\r";
                    }

                    result += " Your location is: " + chr.Map.GetZName(chr.Z) + " in " + chr.Map.ShortDesc + ".";

                    #region Check nearby cells for a sign in the cell description.
                    for (int ypos = -3; ypos <= 3; ypos++)
                    {
                        for (int xpos = -3; xpos <= 3; xpos++)
                        {
                            reltoviewer = "";
                            if (ypos < 0) { reltoviewer += "north"; }
                            else if (ypos > 0) { reltoviewer += "south"; }
                            if (xpos < 0) { reltoviewer += "west"; }
                            else if (xpos > 0) { reltoviewer += "east"; }
                            lcell = Cell.GetCell(chr.FacetID, chr.LandID, chr.MapID, chr.X + xpos, chr.Y + ypos, chr.Z);
                            if (lcell != null)
                            {
                                if (lcell.Description.Length > 0)
                                {
                                    if (lcell.Description.StartsWith("[")) { signdesc = lcell.Description.Substring(8, lcell.Description.Length); }
                                    if (chr.CurrentCell != lcell && lcell.Description.StartsWith("[")) { result += "the sign above the doorway to the " + reltoviewer + " reads : " + signdesc; }
                                    else if (chr.CurrentCell == lcell) { result += lcell.Description; }
                                    result += "\n\r";
                                }
                            }
                        }
                    } 
                    #endregion

                    if (chr.CurrentCell.IsMapPortal)
                    {
                        Segue segue = chr.CurrentCell.Segue;
                        if (segue != null)
                            result += " The portal here leads to " + chr.Facet.GetLandByID(segue.LandID).GetMapByID(segue.MapID).ShortDesc + ".";
                    }

                    chr.WriteToDisplay(result);

                    return true;
                }
                #endregion

                if (String.IsNullOrEmpty(args))
                {
                    lookType = 0;
                }
                else
                {
                    #region determine look information

                    string[] sArgs = args.Split(" ".ToCharArray());

                    sArgs[0] = sArgs[0].ToLower();

                    if (sArgs[0].Equals("in"))
                    {
                        lookType = 1;
                        lookTarget = sArgs[1].ToLower();
                    }
                    else if (args.ToLower().Contains("closely")) // closely (or "examine") currently only works with living objects
                    {
                        lookClosely = true;
                        lookVeryClosely = args.ToLower().Contains("very closely");

                        // look closely at # someone
                        if (sArgs.Length == 4 && !lookVeryClosely)
                        {
                            lookType = 6;
                            lookNum = sArgs[2];
                            lookTarget = sArgs[3];
                        }
                        else if (sArgs.Length == 5 && lookVeryClosely) // look very closely at # someone
                        {
                            lookType = 6;
                            lookNum = sArgs[3];
                            lookTarget = sArgs[4];
                        }
                        else if (sArgs.Length == 3)// look closely at someone
                        {
                            lookType = 2;
                            lookTarget = sArgs[2].ToLower();
                        }
                    }
                    else if (sArgs[0].Equals("at"))
                    {
                        if (sArgs.Length < 2)
                        {
                            chr.WriteToDisplay("Look at what?");
                            return true;
                        }
                        if (sArgs[1] == "ground") //Trap "look at ground"
                        {
                            lookType = 0;
                        }
                        else if (sArgs[1] == "mirror")
                        {
                            // wall mirrors are "look very closely" and hand mirrors are not
                            if (chr.CurrentCell.HasMirror)
                            {
                                chr.TargetID = chr.UniqueID;
                                SendLivingObjectDescription(chr, chr, true, true);
                                if (chr.IsHidden)
                                    chr.IsHidden = false;
                                return true;
                            }
                            else if ((chr.RightHand != null && chr.RightHand.name == "mirror") ||
                                (chr.LeftHand != null && chr.LeftHand.name == "mirror"))
                            {
                                chr.TargetID = chr.UniqueID;
                                SendLivingObjectDescription(chr, chr, true, true);
                                if (chr.IsHidden)
                                    chr.IsHidden = false;
                                return true;
                            }
                            else
                            {
                                chr.WriteToDisplay("You do not see a mirror here.");
                                return true;
                            }
                        }
                        else if (sArgs.Length == 4)
                        {
                            lookType = 4;
                            lookTarget = sArgs[1].ToLower();
                            lookin = sArgs[3].ToLower();
                            lookPrep = sArgs[2].ToLower();
                        }
                        else if (sArgs.Length == 5)
                        {
                            lookType = 5;
                            lookTarget = sArgs[2];
                            lookNum = sArgs[1];
                            lookin = sArgs[4];
                        }
                        else if (sArgs.Length == 3)
                        {
                            if (sArgs[1].EndsWith("'s"))
                            {
                                lookType = 8;
                                lookin = sArgs[1].Substring(0, sArgs[1].Length - 2).ToLower();
                                lookTarget = sArgs[2];
                            }
                            else
                            {
                                lookType = 6;
                                lookTarget = sArgs[2];
                                lookNum = sArgs[1];
                            }
                        }
                        else
                        {
                            lookType = 2;
                            lookTarget = sArgs[1].ToLower();
                        }
                    }
                    else if (sArgs[0] == "n" || sArgs[0] == "north" || sArgs[0] == "e" || sArgs[0] == "east" ||
                        sArgs[0] == "s" || sArgs[0] == "south" || sArgs[0] == "w" || sArgs[0] == "west" ||
                        sArgs[0] == "nw" || sArgs[0] == "northwest" || sArgs[0] == "ne" || sArgs[0] == "northeast" ||
                        sArgs[0] == "se" || sArgs[0] == "southeast" || sArgs[0] == "sw" || sArgs[0] == "southwest")
                    {
                        lookType = 3;
                        lookTarget = sArgs[0].ToLower();
                    }
                    else if (sArgs[0] == "on")
                    {
                        if (sArgs[1] == "ground") //Trap "look on ground"
                        {
                            lookType = 0;
                        }
                        else
                        {
                            lookType = 7;
                            lookTarget = sArgs[1].ToLower();
                        }
                    }
                    else
                    {
                        lookType = 2;
                        lookTarget = sArgs[0].ToLower();
                    }
                    #endregion
                }

                switch (lookType)
                {
                    case 0:
                        #region Look at Room
                        string dispMsg = "You see nothing on the ground.";
                        bool moreThanOne = false;
                        int itemcount = 0;
                        int i = 0;
                        int a = 0;
                        int z = 0;

                        List<Item> tempItemList = chr.CurrentCell.Items;
                        if (chr.CurrentCell.Items.Count > 0) // loop through all the items in the Cell the Character is in
                        {
                            ArrayList templist = new ArrayList();
                            Item[] itemList = new Item[chr.CurrentCell.Items.Count];
                            chr.CurrentCell.Items.CopyTo(itemList);
                            foreach (Item item in itemList)
                            {
                                templist.Add(item);
                            }

                            z = templist.Count - 1;
                            dispMsg = "On the ground you see ";
                            while (z >= 0)
                            {
                                Item item = (Item)templist[z];

                                itemcount = 0;
                                for (i = templist.Count - 1; i > -1; i--)
                                {
                                    Item tmpitem = (Item)templist[i];
                                    if (tmpitem.name == item.name && tmpitem.name == "coins")
                                    {
                                        templist.RemoveAt(i);
                                        itemcount = itemcount + (int)item.coinValue;
                                        z = templist.Count;

                                    }
                                    else if (tmpitem.name == item.name)
                                    {
                                        templist.RemoveAt(i);
                                        z = templist.Count;
                                        itemcount += 1;
                                    }

                                }
                                if (itemcount > 0)
                                {
                                    if (moreThanOne == true)
                                    {
                                        if (z == 0) // second to last item
                                        {
                                            dispMsg += " and ";
                                        }
                                        else
                                        {
                                            dispMsg += ", ";
                                        }
                                    }
                                    dispMsg += GameSystems.Text.TextManager.ConvertNumberToString(itemcount) + Item.GetLookShortDesc(item, itemcount);

                                }
                                moreThanOne = true;
                                z--;
                            }

                            dispMsg += ".";
                            chr.WriteToDisplay(dispMsg);
                            break;
                        }
                        chr.WriteToDisplay(dispMsg);
                        break;
                    #endregion
                    case 1:
                        #region Look in something
                        //Make sure we know what we are looking at
                        Item lookItem = new Item();
                        if (lookTarget == null)
                        {
                            chr.WriteToDisplay("Look in what?");
                            return true;
                        }
                        else if (lookTarget == "left")
                        {
                            if (chr.LeftHand == null)
                            {
                                chr.WriteToDisplay("You have nothing in your left hand.");
                            }
                            else
                            {
                                lookItem = chr.LeftHand;
                                chr.WriteToDisplay("You are looking at " + lookItem.GetLookDescription(chr));
                            }
                        }
                        else if (lookTarget == "right")
                        {
                            if (chr.RightHand == null)
                            {
                                chr.WriteToDisplay("You have nothing in your right hand.");
                            }
                            else
                            {
                                lookItem = chr.RightHand;
                                chr.WriteToDisplay("You are looking at " + lookItem.GetLookDescription(chr));
                            }
                        }
                        else if (lookTarget == "sack")
                        {
                            CommandTasker.ParseCommand(chr, "show", "sack");
                        }
                        else if (lookTarget == "locker")
                        {
                            CommandTasker.ParseCommand(chr, "show", "locker");
                        }
                        else if (lookTarget == "pouch")
                        {
                            CommandTasker.ParseCommand(chr, "show", "pouch");
                        }
                        return true;
                    #endregion
                    case 2:
                        #region Look at something
                        if (String.IsNullOrEmpty(lookTarget))
                        {
                            chr.WriteToDisplay("Look at what?");
                            return true;
                        }
                        else if (lookTarget == "left")
                        {
                            if (chr.LeftHand == null)
                            {
                                chr.WriteToDisplay("You have nothing in your left hand.");
                            }
                            else
                            {
                                lookItem = chr.LeftHand;
                                chr.WriteToDisplay("You are looking at " + lookItem.GetLookDescription(chr));
                            }
                        }
                        else if (lookTarget == "right")
                        {
                            if (chr.RightHand == null)
                            {
                                chr.WriteToDisplay("You have nothing in your right hand.");
                            }
                            else
                            {
                                lookItem = chr.RightHand;
                                chr.WriteToDisplay("You are looking at " + lookItem.GetLookDescription(chr));
                            }
                        }
                        else if (lookTarget == "sky")
                        {
                            if (!chr.CurrentCell.IsOutdoors)
                            {
                                chr.WriteToDisplay("You cannot see the sky.");
                                return true;
                            }

                            string skydesc = "";
                            switch (World.CurrentDailyCycle)
                            {
                                case World.DailyCycle.Morning: skydesc = "The sun is rising."; break;
                                case World.DailyCycle.Afternoon: skydesc = "The sun is high in the afternoon sky."; break;
                                case World.DailyCycle.Evening: skydesc = "The sun is setting."; break;
                                default: skydesc = "It is night time."; break;

                            }
                            if (World.CurrentDailyCycle != World.DailyCycle.Afternoon)
                            {
                                skydesc += " The moon is " + World.CurrentLunarCycle.ToString().ToLower().Replace("_", " ") + ".";
                            }
                            chr.WriteToDisplay(skydesc);
                            return true;
                        }
                        else
                        {
                            ////LOOK AT Items
                            //string disMsg = "";
                            Item tmpItem = new Item();
                            if (lookTarget == "" || lookTarget == null)
                            {
                                chr.WriteToDisplay("I dont understand what you want to do.");
                                return true;
                            }
                            tmpItem = chr.FindAnyItemInSight(lookTarget);  //get a copy of whatever item is in sight
                            if (tmpItem != null)
                            {
                                chr.WriteToDisplay("You are looking at " + tmpItem.GetLookDescription(chr));
                                break;
                            }
                            else
                            {
                                #region Character look description
                                Character tmpNPC = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, lookTarget, false, true);

                                if (tmpNPC == null)
                                {
                                    chr.WriteToDisplay(GameSystems.Text.TextManager.NullTargetMessage(lookTarget));
                                    break;
                                }
                                else
                                {
                                    SendLivingObjectDescription(chr, tmpNPC, lookClosely, lookVeryClosely);
                                }
                                #endregion
                            }
                        }
                        break;
                    #endregion
                    case 3:
                        # region Look at something in a direction
                        moreThanOne = false;
                        // loop through all the items in the Cell the player is in

                        Cell cll = Map.GetCellRelevantToCell(chr.CurrentCell, lookTarget, false);

                        if (cll.Items.Count > 0)
                        {
                            ArrayList templist = new ArrayList();
                            Item[] itemList = new Item[cll.Items.Count];
                            cll.Items.CopyTo(itemList);
                            foreach (Item item in itemList)
                            {
                                templist.Add(item);
                            }
                            z = templist.Count - 1;
                            if (cll.CellGraphic == Cell.GRAPHIC_COUNTER_PLACEABLE)
                            {
                                dispMsg = "On the counter you see ";
                                if (templist.Count == 0) { dispMsg = "You see nothing on the counter"; }
                            }
                            else if (cll.CellGraphic == Cell.GRAPHIC_ALTAR_PLACEABLE)
                            {
                                dispMsg = "On the altar you see ";
                                if (templist.Count == 0) { dispMsg = "You see nothing on the altar"; }
                            }
                            else
                            {
                                dispMsg = "On the ground you see ";
                                if (templist.Count == 0) { dispMsg = "You see nothing on the ground"; }
                            }

                            while (z >= 0)
                            {

                                Item item = (Item)templist[z];

                                itemcount = 0;
                                for (i = templist.Count - 1; i > -1; i--)
                                {
                                    Item tmpitem = (Item)templist[i];
                                    if (tmpitem.name == item.name && tmpitem.itemType == Globals.eItemType.Coin)
                                    {
                                        templist.RemoveAt(i);
                                        itemcount = itemcount + (int)item.coinValue;
                                        z = templist.Count;

                                    }
                                    else if (tmpitem.name == item.name)
                                    {
                                        templist.RemoveAt(i);
                                        z = templist.Count;
                                        itemcount += 1;
                                    }

                                }
                                if (itemcount > 0)
                                {
                                    if (moreThanOne == true)
                                    {
                                        if (z == 0) // second to last item
                                        {
                                            dispMsg += " and ";
                                        }
                                        else
                                        {
                                            dispMsg += ", ";
                                        }
                                    }
                                    dispMsg += GameSystems.Text.TextManager.ConvertNumberToString(itemcount) + Item.GetLookShortDesc(item, itemcount);

                                }
                                moreThanOne = true;
                                z--;
                            }
                            dispMsg += ".";
                            chr.WriteToDisplay(dispMsg);
                        }
                        else
                        {
                            if (cll.CellGraphic == Cell.GRAPHIC_COUNTER_PLACEABLE)
                            {
                                chr.WriteToDisplay("You see nothing on the counter.");
                            }
                            else if (cll.CellGraphic == Cell.GRAPHIC_ALTAR_PLACEABLE)
                            {
                                chr.WriteToDisplay("You see nothing on the altar.");
                            }
                            else
                            {
                                chr.WriteToDisplay("You see nothing on the ground.");
                            }
                        }
                        break;
                    #endregion
                    case 4:
                        #region Look at something on something
                        dispMsg = GameSystems.Text.TextManager.NullItemMessage(null);
                        if (lookin == "ground")
                        {
                            if (Item.FindItemOnGround(lookTarget, chr.FacetID, chr.LandID, chr.MapID, chr.X, chr.Y, chr.Z) != null)
                            {
                                Item findItem = Item.FindItemOnGround(lookTarget, chr.FacetID, chr.LandID, chr.MapID, chr.X, chr.Y, chr.Z);
                                if (lookTarget.IndexOf("coin") != -1)
                                {
                                    dispMsg = "You are looking at " + (int)findItem.coinValue + " coins.";
                                }
                                else
                                {
                                    dispMsg = "You are looking at " + findItem.GetLookDescription(chr);
                                }
                            }
                            chr.WriteToDisplay(dispMsg);
                            return true;
                        }
                        if (lookin == "left" || lookin == "right")
                        {
                            if (lookin == "left")
                            {
                                if (lookTarget == "ring" && lookPrep == "on") //Trap Look at ring on left
                                {
                                    int ringnum = chr.FindFirstLeftRing();
                                    Item ringitem = null;
                                    if (ringnum == 0)
                                    {
                                        chr.WriteToDisplay("You aren't wearing any rings on that hand.");
                                        return true;
                                    }
                                    switch (ringnum)
                                    {
                                        case 1:
                                            ringitem = chr.LeftRing1;
                                            break;
                                        case 2:
                                            ringitem = chr.LeftRing2;
                                            break;
                                        case 3:
                                            ringitem = chr.LeftRing3;
                                            break;
                                        case 4:
                                            ringitem = chr.LeftRing4;
                                            break;
                                    }
                                    chr.WriteToDisplay("You see a " + ringitem.longDesc + ".");
                                    return true;
                                }
                                if (chr.LeftHand != null && chr.LeftHand.name == lookTarget)
                                {
                                    if (lookTarget.IndexOf("coin") != -1)
                                    {
                                        dispMsg = "You are looking at " + (int)chr.LeftHand.coinValue + " coins.";
                                    }
                                    else
                                    {
                                        dispMsg = "You are looking at " + chr.LeftHand.GetLookDescription(chr);
                                    }
                                }
                                else
                                {
                                    dispMsg = "You don't see a " + lookTarget + " in your left hand.";
                                }
                            }
                            else
                            {
                                if (lookTarget == "ring" && lookPrep == "on") //Trap Look at ring on right
                                {
                                    int ringnum = chr.FindFirstRightRing();
                                    Item ringitem = null;
                                    if (ringnum == 0)
                                    {
                                        chr.WriteToDisplay("You aren't wearing any rings on that hand.");
                                        return true;
                                    }
                                    switch (ringnum)
                                    {
                                        case 1:
                                            ringitem = chr.RightRing1;
                                            break;
                                        case 2:
                                            ringitem = chr.RightRing2;
                                            break;
                                        case 3:
                                            ringitem = chr.RightRing3;
                                            break;
                                        case 4:
                                            ringitem = chr.RightRing4;
                                            break;
                                    }
                                    chr.WriteToDisplay("You see a " + ringitem.longDesc + ".");
                                    return true;
                                }
                                if (chr.RightHand != null && chr.RightHand.name == lookTarget)
                                {
                                    if (lookTarget.IndexOf("coin") != -1)
                                    {
                                        dispMsg = "You are looking at " + (int)chr.RightHand.coinValue + " coins.";
                                    }
                                    else
                                    {
                                        dispMsg = "You are looking at " + chr.RightHand.GetLookDescription(chr);

                                    }
                                }
                                else
                                {
                                    dispMsg = "You don't see a " + lookTarget + " in your right hand.";
                                }
                            }
                            chr.WriteToDisplay(dispMsg);
                            return true;
                        }
                        List<Item> tmpList;
                        if (lookin == "counter" || lookin == "altar")
                        {
                            tmpList = Map.GetCopyOfAllItemsFromCounter(chr);
                            if (tmpList == null)
                            {
                                chr.WriteToDisplay("I see no " + lookin + " here.");
                                return true;
                            }
                            else
                            {
                                dispMsg = "You don't see that on the " + lookin + ".";
                            }
                        }
                        else if (lookin == "locker")
                        {
                            if (!chr.CurrentCell.IsLocker)
                            {
                                chr.WriteToDisplay("I see no " + lookin + " here.");
                                return true;
                            }
                            tmpList = chr.lockerList;
                            dispMsg = "You don't see that in your " + lookin + ".";
                        }
                        else if (lookin == "rings" || lookin == "fingers" || lookin == "hands")
                        {
                            tmpList = chr.GetRings();
                            dispMsg = "You don't see that on your fingers.";
                        }
                        else if (lookin == "belt")
                        {
                            tmpList = chr.beltList;
                            dispMsg = "You don't see that on your " + lookin + ".";
                        }
                        else if (lookin == "sack")
                        {
                            tmpList = chr.sackList;
                            dispMsg = "You don't see that in your " + lookin + ".";
                        }
                        else if (lookin == "pouch")
                        {
                            tmpList = chr.pouchList;
                            dispMsg = "You don't see that in your " + lookin + ".";
                        }
                        else if (lookin == "inv" || lookin == "inventory")
                        {
                            tmpList = chr.wearing;
                            dispMsg = "You don't see that in your " + lookin + ".";
                        }
                        else
                        {
                            chr.WriteToDisplay(GameSystems.Text.TextManager.COMMAND_NOT_UNDERSTOOD);
                            return true;
                        }
                        for (i = tmpList.Count - 1; i > -1; i--)
                        {
                            Item tmpitem = tmpList[i];
                            if (tmpitem.name == "coins" && lookTarget == "coins")
                            {
                                dispMsg = "You are looking at " + (int)tmpitem.coinValue + " coins.";
                                break;
                            }
                            else if (tmpitem.name == lookTarget || tmpitem.UniqueID.ToString() == lookTarget)
                            {
                                dispMsg = "You are looking at " + tmpitem.GetLookDescription(chr);
                            }
                        }
                        chr.WriteToDisplay(dispMsg);
                        break;
                    #endregion
                    case 5:
                        #region Look at # item in/on [location], look at all <item> in/on [location]
                        string[] sArgs = args.Split(" ".ToCharArray());
                        sArgs[0] = sArgs[0].ToLower();
                        dispMsg = "You don't see that in your " + lookin + ".";
                        if (sArgs[4] == "right" || sArgs[4] == "left")
                        {
                            int whichring = Convert.ToInt32(lookNum);
                            if (whichring >= 5) { chr.WriteToDisplay("You do not have that many fingers."); return true; }
                            switch (sArgs[4])
                            {
                                case "right":
                                    if (whichring == 1) { if (chr.RightRing1 == null) { chr.WriteToDisplay("You do not have a ring on that finger."); return true; } dispMsg = "You are looking at " + chr.RightRing1.longDesc + "."; if (chr.RightRing1.special.IndexOf("blueglow") > -1) { dispMsg += " It is emitting a faint blue glow."; } }
                                    else if (whichring == 2) { if (chr.RightRing2 == null) { chr.WriteToDisplay("You do not have a ring on that finger."); return true; } dispMsg = "You are looking at " + chr.RightRing2.longDesc + "."; if (chr.RightRing2.special.IndexOf("blueglow") > -1) { dispMsg += " It is emitting a faint blue glow."; } }
                                    else if (whichring == 3) { if (chr.RightRing3 == null) { chr.WriteToDisplay("You do not have a ring on that finger."); return true; } dispMsg = "You are looking at " + chr.RightRing3.longDesc + "."; if (chr.RightRing3.special.IndexOf("blueglow") > -1) { dispMsg += " It is emitting a faint blue glow."; } }
                                    else if (whichring == 4) { if (chr.RightRing4 == null) { chr.WriteToDisplay("You do not have a ring on that finger."); return true; } dispMsg = "You are looking at " + chr.RightRing4.longDesc + "."; if (chr.RightRing4.special.IndexOf("blueglow") > -1) { dispMsg += " It is emitting a faint blue glow."; } }
                                    else if (whichring <= 0) { dispMsg = "Cant find that ring."; }
                                    break;
                                case "left":
                                    if (whichring == 1) { if (chr.LeftRing1 == null) { chr.WriteToDisplay("You do not have a ring on that finger."); return true; } dispMsg = "You are looking at " + chr.LeftRing1.longDesc + "."; if (chr.LeftRing1.special.IndexOf("blueglow") > -1) { dispMsg += " It is emitting a faint blue glow."; } }
                                    else if (whichring == 2) { if (chr.LeftRing2 == null) { chr.WriteToDisplay("You do not have a ring on that finger."); return true; } dispMsg = "You are looking at " + chr.LeftRing2.longDesc + "."; if (chr.LeftRing2.special.IndexOf("blueglow") > -1) { dispMsg += " It is emitting a faint blue glow."; } }
                                    else if (whichring == 3) { if (chr.LeftRing3 == null) { chr.WriteToDisplay("You do not have a ring on that finger."); return true; } dispMsg = "You are looking at " + chr.LeftRing3.longDesc + "."; if (chr.LeftRing3.special.IndexOf("blueglow") > -1) { dispMsg += " It is emitting a faint blue glow."; } }
                                    else if (whichring == 4) { if (chr.LeftRing4 == null) { chr.WriteToDisplay("You do not have a ring on that finger."); return true; } dispMsg = "You are looking at " + chr.LeftRing4.longDesc + "."; if (chr.LeftRing4.special.IndexOf("blueglow") > -1) { dispMsg += " It is emitting a faint blue glow."; } }
                                    else if (whichring <= 0) { dispMsg = "Cant find that ring."; }
                                    break;
                                default:
                                    break;
                            }
                            chr.WriteToDisplay(dispMsg);
                            return true;
                        }

                        Int32.TryParse(lookNum, out countTo);

                        if (lookin == "counter" || lookin == "altar")
                        {
                            tmpList = Map.GetCopyOfAllItemsFromCounter(chr);
                            if (tmpList == null)
                            {
                                chr.WriteToDisplay("I see no " + lookin + " here.");
                                return true;
                            }
                            else
                            {
                                dispMsg = "You don't see that on the " + lookin + ".";
                            }
                        }
                        else if (lookin == "inv" || lookin == "inventory")
                        {
                            tmpList = chr.wearing;
                            dispMsg = "You don't see that on your " + lookin + ".";
                        }
                        else if (lookin == "rings" || lookin == "fingers" || lookin == "hands")
                        {
                            tmpList = chr.GetRings();
                            dispMsg = "You don't see that on your fingers.";
                        }
                        else if (lookin == "locker")
                        {
                            if (!chr.CurrentCell.IsLocker)
                            {
                                chr.WriteToDisplay("I see no " + lookin + " here.");
                                return true;
                            }
                            tmpList = chr.lockerList;
                            dispMsg = "You don't see that in your " + lookin + ".";
                        }
                        else if (lookin == "belt")
                        {
                            tmpList = chr.beltList;
                            dispMsg = "You don't see that on your " + lookin + ".";
                        }
                        else if (lookin == "ground")
                        {
                            tmpList = chr.CurrentCell.Items;//.Clone();
                            dispMsg = "You don't see that on the " + lookin + ".";
                        }
                        else if (lookin == "pouch")
                        {
                            tmpList = chr.pouchList;
                            dispMsg = "You don't see that in your " + lookin + ".";
                        }
                        else //default to sack
                        {
                            tmpList = chr.sackList;
                            dispMsg = "You don't see that in your " + lookin + ".";
                        }

                        itemcount = 0;
                        for (a = 0; a < tmpList.Count; a++) // loop through the items in the list
                        {
                            Item item = tmpList[a]; // copy the item
                            if (item.name == lookTarget.ToLower() || item.name.ToLower() == lookTarget.ToLower().Substring(0, lookTarget.Length - 1) || (item.name == lookTarget.ToLower().Substring(0, lookTarget.Length - 2) && lookNum.ToLower() == "all")) // this is the name of the item we're looking for
                            {
                                itemcount++;
                                if (lookNum.ToLower() == "all")
                                {
                                    string desc = item.GetLookDescription(chr);
                                    chr.WriteToDisplay(itemcount.ToString() + ". " + GameSystems.Text.TextManager.Capitalize(item.GetLookDescription(chr)) + ".");
                                }
                                else
                                {
                                    if (itemcount == countTo)
                                    {
                                        dispMsg = "You are looking at " + item.GetLookDescription(chr);
                                        break;
                                    }
                                }
                            }
                        }

                        if (lookNum.ToLower() != "all")
                            chr.WriteToDisplay(dispMsg);
                        else if (itemcount <= 0) // lookNum was "all"
                            chr.WriteToDisplay("There are no " + lookTarget + "s there.");

                        break;
                        #endregion
                    case 6:
                        #region Look at # something
                        countTo = 0;
                        if (lookNum.ToLower() != "all")
                        {
                            try
                            {
                                countTo = Convert.ToInt32(lookNum);
                            }
                            catch
                            {
                                Utils.Log("Command.look(" + args + ")", Utils.LogType.CommandFailure);
                                chr.WriteToDisplay(lookNum + " is not a number.");
                                return true;
                            }
                        }

                        dispMsg = GameSystems.Text.TextManager.YOU_DONT_SEE_THAT_HERE;

                        itemcount = 1;

                        #region look at # item (on ground)

                        foreach (Item item in new List<Item>(chr.CurrentCell.Items))
                        {
                            if (item.name == lookTarget || item.name == lookTarget.Substring(0, lookTarget.Length - 1))
                            {
                                if (itemcount == countTo)
                                {
                                    dispMsg = "You are looking at " + item.GetLookDescription(chr);
                                    break;
                                }
                                else if (lookNum.ToLower() == "all")
                                {
                                    chr.WriteToDisplay("You are looking at " + item.GetLookDescription(chr));
                                    dispMsg = "";
                                }
                                else itemcount++;
                            }
                        }

                        #endregion

                        #region look at # target
                        if (dispMsg == GameSystems.Text.TextManager.YOU_DONT_SEE_THAT_HERE)
                        {
                            Character targ = TargetAcquisition.FindTargetInView(chr, lookTarget, countTo);

                            if (targ != null)
                            {
                                SendLivingObjectDescription(chr, targ, lookClosely, lookVeryClosely);
                                return true;
                            }
                        #endregion
                        }

                        if(dispMsg.Length > 0)
                            chr.WriteToDisplay(dispMsg);

                        break;
                        #endregion
                    case 7:
                        #region Look on something

                        moreThanOne = false;

                        List<Item> TmpList = Map.GetCopyOfAllItemsFromCounter(chr);

                        if (TmpList == null)
                        {
                            if (lookTarget.ToLower() == "altar")
                            {
                                chr.WriteToDisplay("You do not see an altar here.");
                                return true;
                            }
                            else
                            {
                                chr.WriteToDisplay("You do not see a " + lookTarget + " here.");
                                return true;
                            }
                        }
                        if (TmpList.Count > 0)
                        {
                            z = TmpList.Count - 1;

                            dispMsg = "On the " + lookTarget + " you see ";

                            while (z >= 0)
                            {
                                Item item = (Item)TmpList[z];

                                itemcount = 0;
                                for (i = TmpList.Count - 1; i > -1; i--)
                                {
                                    Item tmpitem = (Item)TmpList[i];
                                    if (tmpitem.name == item.name && tmpitem.itemType == Globals.eItemType.Coin)
                                    {
                                        TmpList.RemoveAt(i);
                                        itemcount = itemcount + (int)item.coinValue;
                                        z = TmpList.Count;

                                    }
                                    else if (tmpitem.name == item.name)
                                    {
                                        TmpList.RemoveAt(i);
                                        z = TmpList.Count;
                                        itemcount++;
                                    }

                                }
                                if (itemcount > 0)
                                {
                                    if (moreThanOne)
                                    {
                                        if (z == 0) // second to last item
                                        {
                                            dispMsg += " and ";
                                        }
                                        else
                                        {
                                            dispMsg += ", ";
                                        }
                                    }
                                    dispMsg += GameSystems.Text.TextManager.ConvertNumberToString(itemcount) + Item.GetLookShortDesc(item, itemcount);
                                }
                                moreThanOne = true;
                                z--;
                            }

                        }
                        else
                        {
                            dispMsg = "You see nothing on the " + lookTarget;
                        }
                        dispMsg += ".";
                        chr.WriteToDisplay(dispMsg);
                        break;
                        #endregion
                    case 8:
                        #region Look at someone's item
                        {
                            bool includeHidden = chr.IsImmortal;

                            Character target = TargetAcquisition.FindTargetInView(chr, lookin, false, includeHidden);

                            if (target == null)
                            {
                                chr.WriteToDisplay("You don't see " + lookin + " here.");
                                break;
                            }
                            if (lookTarget == null)
                            {
                                chr.WriteToDisplay("Look at " + lookin + "'s what?");
                                return true;
                            }
                            else if (lookTarget.ToLower() == "right" || (target.RightHand != null && lookTarget == target.RightHand.name.ToLower()))
                            {
                                if (target.RightHand == null)
                                {
                                    chr.WriteToDisplay("You see nothing in " + target.GetNameForActionResult(true) + "'s right hand.");
                                }
                                else
                                {
                                    if (target.RightHand.name.ToLower() == "coins")
                                    {
                                        chr.WriteToDisplay("In " + target.GetNameForActionResult(true) + "'s right hand you see gold coins.");
                                    }
                                    else
                                    {
                                        chr.WriteToDisplay("In " + target.GetNameForActionResult(true) + "'s right hand you see " + target.RightHand.GetLookDescription(chr));
                                    }
                                }
                            }
                            else if (lookTarget.ToLower() == "left" || (target.LeftHand != null && lookTarget.ToLower() == target.LeftHand.name.ToLower()))
                            {
                                if (target.LeftHand == null)
                                {
                                    chr.WriteToDisplay("You see nothing in " + target.GetNameForActionResult(true) + "'s left hand.");
                                }
                                else
                                {
                                    if (target.LeftHand.name.ToLower() == "coins")
                                    {
                                        chr.WriteToDisplay("In " + target.GetNameForActionResult(true) + "'s left hand you see gold coins.");
                                    }
                                    else
                                    {
                                        chr.WriteToDisplay("In " + target.GetNameForActionResult(true) + "'s left hand you see " + target.LeftHand.GetLookDescription(chr));
                                    }
                                }
                            }
                            else
                            {
                                bool match = false;
                                foreach (Item wItem in target.wearing)
                                {
                                    if (lookTarget.ToLower() == wItem.name.ToLower() || lookTarget == wItem.UniqueID.ToString())
                                    {
                                        chr.WriteToDisplay("You are looking at " + wItem.GetLookDescription(chr));
                                        match = true;
                                        break;
                                    }
                                }
                                if (!match)
                                {
                                    chr.WriteToDisplay(target.GetNameForActionResult() + " is not holding or wearing that.");
                                }
                            }
                        }
                        break;
                        #endregion
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return true;
            }
            return true;
        }

        private void SendLivingObjectDescription(Character looker, Character observed, bool lookClosely, bool lookVeryClosely)
        {
            try
            {
                if (observed.IsWizardEye)
                {
                    looker.WriteToDisplay("You are looking at a " + observed.Alignment.ToString().ToLower() + " " + observed.Name + ". The " + observed.Name + " is unhurt.");
                    return;
                }

                #region Local Variables
                int perc = (int)(((float)observed.Hits / (float)observed.HitsFull) * 100);
                //bool lookClosely = false;
                string damageDesc = "unhurt";
                string align = "a " + Utils.FormatEnumString(observed.Alignment.ToString()).ToLower();
                string agedesc = "a young";
                string armordesc = " is wearing no armor.";
                string examinedesc = "";
                string torso = "none";
                string legs = "none";
                string back = "none";
                string head = "none";
                string neck = "none";
                string shoulders = "none";
                string face = "none";
                string waist = "none";
                string gauntlets = "none";
                string feet = "none";
                string iscarrying = "";
                string flameshield = "";
                string ensnared = "";
                string barkskin = "";
                string stoneskin = "";
                string undead = "";
                string islookingatyou = "";
                string sentDescription = "";
                #endregion

                bool detectThief = Rules.DetectThief(observed, looker);

                #region Alignment
                if (observed.Alignment == Globals.eAlignment.Amoral) { align = "an amoral"; }
                else
                {
                    if (detectThief)
                    {
                        if (observed.Alignment == Globals.eAlignment.Lawful) { align = "a lawful"; }
                        if (observed.Alignment == Globals.eAlignment.Neutral) { align = "a neutral"; }
                        if (observed.Alignment == Globals.eAlignment.Chaotic) { align = "a chaotic"; }
                        if (observed.Alignment == Globals.eAlignment.Evil || observed.Alignment == Globals.eAlignment.ChaoticEvil) { align = "an evil"; }
                    }
                    else
                    {
                        if (looker.Alignment == Globals.eAlignment.Lawful) { align = "a lawful"; }
                        if (looker.Alignment == Globals.eAlignment.Neutral) { align = "a neutral"; }
                        if (looker.Alignment == Globals.eAlignment.Chaotic) { align = "a chaotic"; }
                        if (looker.Alignment == Globals.eAlignment.Evil || looker.Alignment == Globals.eAlignment.ChaoticEvil) { align = "an evil"; }
                    }
                }
                #endregion

                if (observed is NPC && (observed as NPC).IsSummoned)
                    align += " summoned";

                #region Age
                if (observed.Age != 0)
                    agedesc = observed.GetAgeDescription(true);
                #endregion

                #region Damage Description
                if (perc >= 0 && perc < 11) damageDesc = "near death";
                else if (perc >= 11 && perc < 31) damageDesc = "badly wounded";
                else if (perc >= 31 && perc < 71) damageDesc = "wounded";
                else if (perc >= 71 && perc < 99) damageDesc = "barely wounded";
                else if (perc >= 100) damageDesc = "unhurt";
                #endregion

                bool foundInTanningResult = false;

                foreach (Item wornItem in observed.wearing)
                {
                    if ((observed is NPC) && ((observed as NPC).tanningResult != null && (observed as NPC).tanningResult.Count > 0))
                    {
                        foreach (int tanningID in (observed as NPC).tanningResult.Keys)
                        {
                            if (tanningID == wornItem.itemID)
                            {
                                foundInTanningResult = true;
                                break;
                            }
                        }

                        if (foundInTanningResult)
                        {
                            foundInTanningResult = false;
                            continue;
                        }
                    }

                    switch (wornItem.wearLocation)
                    {
                        case Globals.eWearLocation.Head: // head examine
                            if (lookClosely) { head = wornItem.longDesc; }
                            break;
                        case Globals.eWearLocation.Neck: // neck examine
                            if (lookClosely) { neck = wornItem.longDesc; }
                            break;
                        case Globals.eWearLocation.Face: // face examine
                            if (lookClosely) { face = wornItem.longDesc; }
                            break;
                        case Globals.eWearLocation.Shoulders: // shoulders examine
                            if (lookClosely) { shoulders = wornItem.longDesc; }
                            break;
                        case Globals.eWearLocation.Back: // back
                            if (wornItem.baseType == Globals.eItemBaseType.Armor)
                            {
                                if (looker.GetInventoryItem(Globals.eWearLocation.Torso) != null)
                                    back = " is wearing " + wornItem.longDesc + " over ";
                                else back = " is wearing " + wornItem.longDesc + ".";
                            }
                            else
                            {
                                if (looker.GetInventoryItem(Globals.eWearLocation.Torso) != null)
                                {
                                    back = " has " + wornItem.longDesc + " strapped to " + Character.POSSESSIVE[(int)observed.gender].ToLower() +
                                        " back. " + Character.PRONOUN[(int)observed.gender] + " is wearing ";
                                }
                                else
                                {
                                    back = " has " + wornItem.longDesc + " strapped to " + Character.POSSESSIVE[(int)observed.gender].ToLower() +
                                        " back. ";
                                }
                            }
                            break;
                        case Globals.eWearLocation.Torso: // torso
                            torso = wornItem.longDesc;
                            break;
                        case Globals.eWearLocation.Legs: // legs
                            legs = wornItem.longDesc;
                            break;
                        case Globals.eWearLocation.Feet: // feet
                            if (lookClosely) { feet = wornItem.longDesc; }
                            break;
                        case Globals.eWearLocation.Hands: // gauntlets examine
                            if (lookClosely) { gauntlets = wornItem.longDesc; }
                            break;
                        case Globals.eWearLocation.Waist: // waist examine
                            if(lookClosely) { waist = wornItem.longDesc; }
                            break;
                        default:
                            break;
                    }
                }

                #region Wearing back armor such as a robe.
                if (back != "none")
                {
                    if (torso != "none")
                        armordesc = back + torso + ".";
                    if (legs != "none")
                        armordesc = back + torso + " and " + legs + ".";
                    if (legs == "none" && torso == "none")
                        armordesc = back + ".";
                }
                else
                {
                    if (torso != "none")
                    {
                        if (legs != "none")
                            armordesc = " is wearing " + torso + " and " + legs + ".";
                        else
                            armordesc = " is wearing " + torso + ".";
                    }
                    else if (legs != "none") // torso = none, and wearing legs
                        armordesc = " is wearing " + legs + ".";
                } 
                #endregion

                #region Look closely -- show other visible inventory items such as head, neck, shoulders, gauntlets and boots.
                if (lookClosely)
                {
                    if (head != "none")
                        examinedesc = " On " + Character.POSSESSIVE[(int)observed.gender].ToLower() + " head is " + head + ".";
                    if (neck != "none")
                        examinedesc = examinedesc + " Around " + Character.POSSESSIVE[(int)observed.gender].ToLower() + " neck is " + neck + ".";
                    if (shoulders != "none")
                        examinedesc = examinedesc + " Draped over " + Character.POSSESSIVE[(int)observed.gender].ToLower() + " shoulders is " + shoulders + ".";
                    if (waist != "none")
                        examinedesc = examinedesc + " Around " + Character.POSSESSIVE[(int)observed.gender].ToLower() + " waist is " + waist + ".";
                    if (gauntlets != "none")
                        examinedesc = examinedesc + " On " + Character.POSSESSIVE[(int)observed.gender].ToLower() + " hands are " + gauntlets + ".";
                    if (feet != "none")
                        examinedesc = examinedesc + " On " + Character.POSSESSIVE[(int)observed.gender].ToLower() + " feet are " + feet + ".";
                    if (face != "none")
                        examinedesc = examinedesc + " Covering " + Character.POSSESSIVE[(int)observed.gender].ToLower() + " face is " + face + ".";
                }
                #endregion
                
                if(lookVeryClosely)
                {
                    List<Item> rings = observed.GetRings();

                    if (rings != null && rings.Count > 0)
                    {
                        if (observed.GetInventoryItem(Globals.eWearLocation.Hands) == null) // no gauntlets/gloves so rings are visible
                        {
                            string ringsDesc = " " + Character.PRONOUN[(int)observed.gender] + " is wearing";
                            foreach (Item ring in rings)
                                ringsDesc += " " + ring.longDesc + ",";
                            ringsDesc = ringsDesc.Substring(0, ringsDesc.Length - 1); // removes last comma
                            examinedesc = examinedesc + ringsDesc + ".";
                        }
                    }

                    // scars
                    //examinedesc = examinedesc + " " + observed.GetNameForActionResult() + " does not have any noticeable scars.";
                }
                
                #region Held items.
		        if (observed.RightHand != null && observed.LeftHand == null)
                    iscarrying = " " + Character.PRONOUN[(int)observed.gender] + " is carrying " + observed.RightHand.shortDesc + ".";
                if (observed.RightHand == null && observed.LeftHand != null)
                    iscarrying = " " + Character.PRONOUN[(int)observed.gender] + " is carrying " + observed.LeftHand.shortDesc + ".";
                if (observed.RightHand != null && observed.LeftHand != null)
                    iscarrying = " " + Character.PRONOUN[(int)observed.gender] + " is carrying " + observed.RightHand.shortDesc + " and " + observed.LeftHand.shortDesc + ".";
                #endregion

                string isRestingOrMeditating = "";

                #region Ready to return look description if tmpNPC is a player.
                if (observed.IsPC || observed is PC)
                {
                    //is the LOOKer a knight? do we fool them into thinking we're not a thief?
                    string pcClass = observed.classFullName.ToLower();
                    switch (observed.BaseProfession)
                    {
                        case Character.ClassType.Thief:
                            pcClass = detectThief ? "thief" : "fighter";
                            break;
                        default:
                            break;
                    }
                    if (observed.TargetID == looker.UniqueID)
                        islookingatyou = " " + observed.Name + " is looking at you.";

                    if (observed.Stunned > 0)
                        islookingatyou = " " + observed.Name + " is stunned.";

                    if (observed.IsResting && !observed.IsMeditating) isRestingOrMeditating = " " + Character.PRONOUN[(int)observed.gender] + " is resting.";
                    else if (observed.IsMeditating) isRestingOrMeditating = " " + Character.POSSESSIVE[(int)observed.gender] + " eyes are closed.";

                    looker.TargetID = observed.UniqueID;

                    //armordesc = armordesc.Replace(".none", "");
                    //armordesc = armordesc.Replace(" over none ", " ");
                    //armordesc = armordesc.Replace(".a", " over a");
                    //armordesc = armordesc.Replace(".an", " over an");

                    sentDescription = "You are looking at " + agedesc + " " + observed.gender.ToString().ToLower() + " " + align.Substring(align.IndexOf(" ") + 1) + " " + pcClass + " from " + Character.RaceToString(observed) +
                        ". " + observed.Name + armordesc + iscarrying + examinedesc + " " + observed.Name + " is " + damageDesc + ".";
                }
                #endregion

                //get npc class if not a player
                else
                {
                    string lookclass = "fighter";
                    switch (observed.BaseProfession)
                    {
                        case Character.ClassType.Thief:
                            if (detectThief)
                                lookclass = "thief";
                            else
                                lookclass = "fighter";
                            break;
                        default:
                            lookclass = Utils.FormatEnumString(observed.BaseProfession.ToString()).ToLower();
                            break;
                    }

                    if (observed.HasEffect(Effect.EffectTypes.Barkskin))
                        barkskin = " " + Character.POSSESSIVE[(int)observed.gender] + " skin is covered in bark.";

                    if (observed.HasEffect(Effect.EffectTypes.Stoneskin))
                        barkskin = " " + Character.POSSESSIVE[(int)observed.gender] + " skin has the appearance of polished stone.";

                    if (observed.HasEffect(Effect.EffectTypes.Flame_Shield))
                        flameshield = " " + Character.PRONOUN[(int)observed.gender] + " is surrounded by a flame shield.";

                    if (observed.HasEffect(Effect.EffectTypes.Ensnare))
                        ensnared = " " + Character.PRONOUN[(int)observed.gender] + " is ensnared.";

                    if(looker.HasEffect(Effect.EffectTypes.Detect_Undead) && observed.IsUndead)
                        undead = " " + Character.PRONOUN[(int)observed.gender] + " is undead.";

                    if (Autonomy.EntityBuilding.EntityLists.WYRMKIN.Contains(observed.entity))
                    {
                        #region All wyrm kin; dragons, drakes and wyrms.
                        if (observed.TargetID == looker.UniqueID)
                            islookingatyou = " " + observed.GetNameForActionResult() + " is looking at you.";
                        if (observed.Stunned > 0)
                            islookingatyou = " " + observed.GetNameForActionResult() + " is stunned.";

                        string longDesc = (observed as NPC).longDesc;
                        longDesc = (observed as NPC).longDesc.Replace("a ", "");
                        longDesc = (observed as NPC).longDesc.Replace("an ", "");

                        // currently do not display if one of these races is resting or meditating
                        sentDescription = "You are looking at " + agedesc + " " + (observed as NPC).longDesc + ". " + Character.PRONOUN[(int)observed.gender] +
                            " is " + damageDesc + ".";
                        #endregion
                    }
                    else if (Autonomy.EntityBuilding.EntityLists.IsHuman(observed)) // tmpNPC has a race
                    {
                        #region Humans
                        if (observed.TargetID == looker.UniqueID)
                            islookingatyou = " " + observed.GetNameForActionResult() + " is looking at you.";
                        if (observed.Stunned > 0)
                            islookingatyou = " " + observed.GetNameForActionResult() + " is stunned.";
                        if (observed.IsResting && !observed.IsMeditating) isRestingOrMeditating = " " + Character.PRONOUN[(int)observed.gender] + " is resting.";
                        else if (observed.IsMeditating) isRestingOrMeditating = " " + Character.POSSESSIVE[(int)observed.gender] + " eyes are closed.";

                        sentDescription = "You are looking at " + agedesc + " " + observed.gender.ToString().ToLower() + " " + lookclass +
                            " from " + Character.RaceToString(observed) + ". " + observed.GetNameForActionResult() + armordesc + iscarrying + examinedesc +
                            " " + Character.PRONOUN[(int)observed.gender] + " is " + damageDesc + "."; 
                        #endregion
                    }
                    else
                    {
                        if (observed.animal || Autonomy.EntityBuilding.EntityLists.ANIMAL.Contains(observed.entity) || Autonomy.EntityBuilding.EntityLists.ANIMAL_SMALL.Contains(observed.entity)
                            || observed.species == Globals.eSpecies.Plant || Autonomy.EntityBuilding.EntityLists.PLANT.Contains(observed.entity) ||
                            observed.entity == Autonomy.EntityBuilding.EntityLists.Entity.Ent)
                        {
                            #region Animals and plants.
                            if (observed.TargetID == looker.UniqueID)
                                islookingatyou = " " + observed.GetNameForActionResult() + " is looking at you.";

                            if (observed.Stunned > 0)
                                islookingatyou = " " + observed.GetNameForActionResult() + " is stunned.";

                            // animals should not ever meditate...
                            if (observed.IsResting) isRestingOrMeditating = " " + Character.PRONOUN[(int)observed.gender] + " is resting.";

                            sentDescription = "You are looking at " + align + " " + (observed as NPC).Name + ". " + Character.PRONOUN[(int)observed.gender] +
                                " is " + damageDesc + ".";
                            #endregion
                        }
                        else
                        {
                            if (observed.TargetID == looker.UniqueID)
                                islookingatyou = " " + observed.GetNameForActionResult() + " is looking at you.";

                            if (observed.Stunned > 0)
                                islookingatyou = " " + observed.GetNameForActionResult() + " is stunned.";

                            if (observed.IsResting && !observed.IsMeditating) isRestingOrMeditating = " " + Character.PRONOUN[(int)observed.gender] + " is resting.";
                            else if (observed.IsMeditating) isRestingOrMeditating = " " + Character.POSSESSIVE[(int)observed.gender] + " eyes are closed.";

                            armordesc = armordesc.Replace(".none", "");
                            armordesc = armordesc.Replace(" over none ", " ");
                            armordesc = armordesc.Replace(" over .", ".");

                            sentDescription = "You are looking at " + align + " " + (observed as NPC).longDesc + ". " + Character.PRONOUN[(int)observed.gender] +
                                armordesc + iscarrying + examinedesc + " " + Character.PRONOUN[(int)observed.gender] + " is " + damageDesc + ".";
                        }
                    }
                }

                // TODO: May need to check the code if something has a belt and it shouldn't... 10/18/2015 Eb
                if (lookClosely && (Autonomy.EntityBuilding.EntityLists.IsHumanOrHumanoid(observed) || observed is PC) && observed.beltList.Count > 0)
                {
                    string beltDisplay = " On " + Character.POSSESSIVE[(int)observed.gender].ToLower() + " belt you see";

                    // On his belt you see a dagger, a longsword and an axe.

                    foreach (Item beltItem in observed.beltList)
                    {
                        if (observed.beltList.Count > 1 && observed.beltList.IndexOf(beltItem) == observed.beltList.Count - 1)
                            beltDisplay += " and " + beltItem.longDesc;
                        else beltDisplay += " " + beltItem.longDesc;

                        if (beltItem == observed.beltList[observed.beltList.Count - 1]) beltDisplay += ".";
                        else if (observed.beltList.Count > 1 && beltItem != observed.beltList[observed.beltList.Count - 1]) beltDisplay += ",";
                    }

                    sentDescription = sentDescription + beltDisplay; // add belt information
                }

                sentDescription = sentDescription.Replace(".none", "");
                sentDescription = sentDescription.Replace(" over none ", " ");
                sentDescription = sentDescription.Replace(".a", " over a");
                sentDescription = sentDescription.Replace(".an", " over an");
                sentDescription = sentDescription.Replace(" over .", ".");

                // final sent description
                sentDescription = sentDescription + undead + isRestingOrMeditating + barkskin + stoneskin + flameshield + ensnared + islookingatyou;

                looker.WriteToDisplay(sentDescription);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }
    }
}
