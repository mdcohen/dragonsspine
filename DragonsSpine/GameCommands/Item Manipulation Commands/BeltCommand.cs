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

namespace DragonsSpine.Commands
{
    [CommandAttribute("belt", "Place an item on your belt.", (int)Globals.eImpLevel.USER, new string[] { "sheathe" },
        1, new string[] { "belt <item>", "belt right", "belt left", "belt <item on ground>" }, Globals.ePlayerState.PLAYING)]
    public class BeltCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            //Check if they passed any args
            if (args == null)
            {
                chr.WriteToDisplay("Belt what?");
            }
            // check if belt is full - only 2 slots
            else
            {
                string[] sArgs = args.Split(" ".ToCharArray());

                Item pItem = null;

                //Did they ask to belt "left" or "right"? If so, check if hand is holding something.
                //If it is, change the string to the actual item name so we can use object helper functions.
                if (sArgs[0] == "right" || sArgs[0] == "left")
                {
                    pItem = sArgs[0] == "left" ? chr.LeftHand : chr.RightHand;
                    //if (sArgs[0] == "left" && chr.LeftHand != null)
                    //    pItem = chr.LeftHand;
                    //else if (sArgs[0] == "right" && chr.RightHand != null)
                    //    pItem = chr.RightHand;
                }

                if(pItem == null)
                    pItem = chr.FindHeldItem(sArgs[0]);  //copy item from the players hand to check if legal.

                if (pItem == null)
                {
                    if (sArgs[0] == "left" || sArgs[0] == "right") //the hand was empty.
                    {
                        chr.WriteToDisplay("You have nothing in your " + sArgs[0] + " hand to belt.");
                    }
                    else
                    {
                        chr.WriteToDisplay("You are not holding a " + sArgs[0] + ".");
                    }
                }
                else if (pItem.size == Globals.eItemSize.Sack_Only ||
                    pItem.size == Globals.eItemSize.Pouch_Only ||
                    pItem.size == Globals.eItemSize.No_Container ||
                    pItem.size == Globals.eItemSize.Sack_Or_Pouch)
                {
                    if (pItem.wearLocation == Globals.eWearLocation.Back)
                    {
                        chr.WriteToDisplay("You cannot belt the " + pItem.name + ". Try to wear it on your back.");
                    }
                    else { chr.WriteToDisplay("You cannot belt " + pItem.shortDesc + "."); }
                }
                else if (chr.beltList.Count >= Character.MAX_BELT)
                {
                    chr.WriteToDisplay("Your belt is full.");
                }
                else
                {
                    // Now that it's all legit, actually take the item out of their hands and belt it.
                    //int handLoc = chr.WhichHand(sArgs[0]);
                    //pItem = chr.FindHeldItem(sArgs[0]); // gets the item from hand, does not unequip it

                    if (pItem != null && pItem.size == Globals.eItemSize.Belt_Large_Slot_Only)
                    {
                        foreach (Item bItem in chr.beltList)
                        {
                            if (bItem.size == Globals.eItemSize.Belt_Large_Slot_Only)
                            {
                                chr.WriteToDisplay("You already have one large item on your belt.");
                                return true;
                            }
                        }
                    }

                    // TODO: this needs to be handled better
                    if (!pItem.name.ToLower().Contains("crossbow") && !pItem.longDesc.ToLower().Contains("crossbow")) { pItem.IsNocked = false; }

                    if (chr.BeltItem(pItem))
                    {
                        if (pItem == chr.RightHand)
                            chr.UnequipRightHand(pItem);
                        else if (pItem == chr.LeftHand)
                            chr.UnequipLeftHand(pItem);
                    }
                }
            }

            return true;
        }
    }
}
