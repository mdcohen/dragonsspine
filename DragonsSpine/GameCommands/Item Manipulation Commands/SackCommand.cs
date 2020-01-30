using System;

namespace DragonsSpine.Commands
{
    [CommandAttribute("sack", "Place an item in your sack.", (int)Globals.eImpLevel.USER, new string[] { "sck" },
        1, new string[] { "sack <item>", "sack right", "sack left" }, Globals.ePlayerState.PLAYING)]
    public class SackCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            // Check if they passed any args.
            if (args == null)
            {
                chr.WriteToDisplay("Sack what?");
            }
            // Check if sack is full.
            else
            {

                string[] sArgs = args.Split(" ".ToCharArray());

                Item pItem = null;

                //Did they ask to belt "left" or "right"? If so, check if hand is holding something.
                //If it is, change the string to the actual item name so we can use object helper functions.
                if (sArgs[0] == "right" || sArgs[0] == "left")
                    pItem = sArgs[0] == "left" ? chr.LeftHand : chr.RightHand;

                if (pItem == null)
                {
                    pItem = chr.FindHeldItem(sArgs[0]);  // Copy item from the players hand to check if legal.
                }

                // Look for item on ground, in containers, on counter, in locker
                //if(int.TryParse(sArgs[0], out int uniqueItemID))
                //{
                //    foreach (Item item in chr.CurrentCell.Items)
                //    {
                //        if (item.UniqueID == uniqueItemID)
                //        {
                //            pItem = item;
                //            break;
                //        }
                //    }
                //}

                if (pItem == null)
                {
                    if (sArgs[0] == "left" || sArgs[0] == "right") // The hand was empty.
                    {
                        chr.WriteToDisplay("You have nothing in your " + sArgs[0] + " hand to place in your sack.");
                    }
                    else
                    {
                        chr.WriteToDisplay("You are not holding a " + sArgs[0] + ".");
                    }
                }
                else if (pItem.size != Globals.eItemSize.Sack_Only && pItem.size != Globals.eItemSize.Belt_Or_Sack && pItem.size != Globals.eItemSize.Sack_Or_Pouch)
                {
                    if (pItem.skillType == Globals.eSkillType.Bow && pItem.baseType < Globals.eItemBaseType.Thievery)
                    {
                        chr.WriteToDisplay("You cannot put the " + pItem.name + " in your sack. Try to wear it on your back.");
                    }
                    else { chr.WriteToDisplay("You cannot put " + pItem.shortDesc + " in your sack."); }
                }
                else if (chr.SackCountMinusGold >= Character.MAX_SACK)
                {
                    chr.WriteToDisplay("Your sack is full.");
                }
                else
                {
                    // TODO: this needs to be handled better
                    //if (!pItem.name.ToLower().Contains("crossbow") && !pItem.longDesc.ToLower().Contains("crossbow")) { pItem.IsNocked = false; }

                    if (chr.SackItem(pItem))
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
