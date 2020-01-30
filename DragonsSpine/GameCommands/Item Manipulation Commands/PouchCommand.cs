using System;

namespace DragonsSpine.Commands
{
    [CommandAttribute("pouch", "Place an item in your pouch.", (int)Globals.eImpLevel.USER, new string[] { "pch" },
        1, new string[] { "pouch <item>", "pouch right", "pouch left" }, Globals.ePlayerState.PLAYING)]
    public class PouchCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            // Check if they passed any args.
            if (args == null)
            {
                chr.WriteToDisplay("Pouch what?");
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
                    pItem = chr.FindHeldItem(sArgs[0]);  // Copy item from the players hand to check if legal.

                if (pItem == null)
                {
                    if (sArgs[0] == "left" || sArgs[0] == "right") // The hand was empty.
                    {
                        chr.WriteToDisplay("You have nothing in your " + sArgs[0] + " hand to place in your pouch.");
                    }
                    else
                    {
                        chr.WriteToDisplay("You are not holding a " + sArgs[0] + ".");
                    }
                }
                else if (pItem.size != Globals.eItemSize.Pouch_Only && pItem.size != Globals.eItemSize.Sack_Or_Pouch)
                {
                    if (pItem.skillType == Globals.eSkillType.Bow && pItem.baseType < Globals.eItemBaseType.Thievery)
                    {
                        chr.WriteToDisplay("You cannot put the " + pItem.name + " in your pouch. Try to wear it on your back.");
                    }
                    else { chr.WriteToDisplay("You cannot put " + pItem.shortDesc + " in your pouch."); }
                }
                else if (chr.pouchList.Count >= Character.MAX_POUCH)
                {
                    chr.WriteToDisplay("Your pouch is full.");
                }
                else
                {
                    // TODO: this needs to be handled better
                    //if (!pItem.name.ToLower().Contains("crossbow") && !pItem.longDesc.ToLower().Contains("crossbow")) { pItem.IsNocked = false; }

                    if (chr.PouchItem(pItem))
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
