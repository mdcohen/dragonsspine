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
    [CommandAttribute("put", "Put an item somewhere.", (int)Globals.eImpLevel.USER, 1, new string[] { "There are many arguments for the put command." }, Globals.ePlayerState.PLAYING)]
    public class PutCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            args = args.ToLower();
            string putTarget = null;
            string putItem = null;
            string putLocation = null;
            long putAmount = 0;		//This is mostly for coin
            int putCount = 0; // put gem on # corpse
            string putType = "";
            string leftOrRight = "";

            //Check if they passed any args
            if (args == null)
            {
                chr.WriteToDisplay("Put what where?");
            }
            else
            {
                try
                {
                    string[] sArgs = args.Split(" ".ToCharArray());

                    //We need putItem, putTarget, putType, and putAmount if it exists

                    switch (sArgs.Length)
                    {
                        case 0:
                            break;
                        case 1:
                            chr.WriteToDisplay("Put " + sArgs[0] + " where?");
                            break;
                        case 2:
                            chr.WriteToDisplay("Do what?");
                            break;
                        case 3:
                            if (sArgs[0] == "left") // trap "put left/right in sack/locker" "put gem on corpse"
                            {
                                if (chr.LeftHand == null)
                                {
                                    chr.WriteToDisplay("Your left hand is empty.");
                                    return true;
                                }
                                else { putItem = chr.LeftHand.name; leftOrRight = "left"; }
                            }
                            else if (sArgs[0] == "right")
                            {
                                if (chr.RightHand == null)
                                {
                                    chr.WriteToDisplay("Your right hand is empty.");
                                    return true;
                                }
                                else { putItem = chr.RightHand.name; leftOrRight = "right"; }
                            }
                            else { putItem = sArgs[0]; }
                            putType = sArgs[1];
                            putTarget = sArgs[2];
                            break;
                        case 4: // put gem on # corpse
                            if (sArgs[0] == "gem")
                            {
                                putItem = sArgs[0];
                                putType = sArgs[1];
                                if (!Int32.TryParse(sArgs[2], out putCount))
                                {
                                    chr.WriteToDisplay("Invalid command. Command format: put gem on # corpse");
                                    return true;
                                }
                                putTarget = sArgs[3];
                            }
                            else if (sArgs[0] == "ring")
                            {
                                putItem = sArgs[0];
                                putType = sArgs[1];
                                putLocation = sArgs[2];
                                putTarget = sArgs[3];
                            }
                            else if (sArgs[1].Contains("coin"))
                            {
                                putAmount = Convert.ToInt64(sArgs[0], 10);
                                putItem = sArgs[1];
                                putType = sArgs[2];
                                putTarget = sArgs[3];
                            }
                            else // put bracer on left wrist AND put armband on left bicep
                            {
                                putItem = sArgs[0]; // bracer
                                putType = sArgs[1]; // on
                                putLocation = sArgs[2]; // left
                                putTarget = sArgs[3]; // wrist
                            }
                            break;
                        default:
                            chr.WriteToDisplay("Do what?");
                            break;
                    }
                    //0 = no argument
                    //1 = put IN
                    //2 = put ON
                    //3 = put UNDER
                    switch (putType.ToLower())
                    {
                        case "":
                            chr.WriteToDisplay("Put what where?");
                            break;
                        case "in":
                            #region "in"
                            int hand = (int)Globals.eWearOrientation.None;

                            if (leftOrRight != "") hand = chr.WhichHand(leftOrRight);
                            else hand = chr.WhichHand(putItem);

                            if (hand == (int)Globals.eWearOrientation.None)
                            {
                                if (Int32.TryParse(putItem, out int idnum))
                                    chr.WriteToDisplay("You aren't holding that.");
                                else chr.WriteToDisplay("You aren't holding a " + putItem + ".");
                            }
                            else if (putTarget.ToLower() == "sack")
                            {
                                #region sack
                                if (hand == (int)Globals.eWearOrientation.Right)
                                {
                                    if (putItem == "coins")
                                    {
                                        foreach (Item itm in chr.sackList)
                                        {
                                            if (itm.itemType == Globals.eItemType.Coin)
                                            {
                                                itm.coinValue += chr.RightHand.coinValue;
                                                chr.UnequipRightHand(itm);
                                            }
                                        }
                                    }
                                    if (chr.RightHand != null)
                                    {
                                        if (chr.RightHand.size == Globals.eItemSize.Sack_Only || chr.RightHand.size == Globals.eItemSize.Belt_Or_Sack || chr.RightHand.size == Globals.eItemSize.Sack_Or_Pouch)
                                        {
                                            if (chr.SackCountMinusGold < Character.MAX_SACK || chr.RightHand.itemType == Globals.eItemType.Coin)
                                            {
                                                if(chr.SackItem(chr.RightHand))
                                                    chr.UnequipRightHand(chr.RightHand);
                                            }
                                            else
                                            {
                                                chr.WriteToDisplay("Your sack is full.");
                                            }
                                        }
                                        else { chr.WriteToDisplay("The " + chr.RightHand.name + " won't fit in your sack."); }
                                    }
                                }
                                else if (hand == (int)Globals.eWearOrientation.Left)
                                {
                                    if (putItem == "coins")
                                    {
                                        foreach (Item itm in chr.sackList)
                                        {
                                            if (itm.name == "coins")
                                            {
                                                itm.coinValue += chr.LeftHand.coinValue;
                                                chr.UnequipLeftHand(chr.LeftHand);
                                            }
                                        }
                                    }
                                    if (chr.LeftHand != null)
                                    {
                                        if (chr.LeftHand.size == Globals.eItemSize.Sack_Only || chr.LeftHand.size == Globals.eItemSize.Belt_Or_Sack || chr.LeftHand.size == Globals.eItemSize.Sack_Or_Pouch)
                                        {
                                            if (chr.SackCountMinusGold < Character.MAX_SACK || chr.LeftHand.itemType == Globals.eItemType.Coin)
                                            {
                                                if(chr.SackItem(chr.LeftHand))
                                                    chr.UnequipLeftHand(chr.LeftHand);
                                            }
                                            else
                                            {
                                                chr.WriteToDisplay("Your sack is full.");
                                            }
                                        }
                                        else { chr.WriteToDisplay("The " + chr.LeftHand.name + " won't fit in your sack."); }
                                    }
                                }
                                #endregion
                                return true;
                            }
                            else if (chr.CurrentCell.IsLocker && putTarget.ToLower() == "locker")
                            {
                                #region locker
                                if (putItem == "coins" || putItem == "coin")
                                {
                                    chr.WriteToDisplay("Coins must be deposited in the bank.");
                                    return true;
                                }
                                if (putItem == "corpse")
                                {
                                    chr.WriteToDisplay("You notice a sign on the wall, 'Please do not store dead bodies in your locker. Thanks, +janitor'");
                                    return true;
                                }

                                if (leftOrRight == "left")
                                {
                                    if (chr.lockerList.Count >= Character.MAX_LOCKER)
                                    {
                                        chr.WriteToDisplay("Your locker is full.");
                                    }
                                    else
                                    {
                                        chr.lockerList.Add(chr.LeftHand);
                                        chr.UnequipLeftHand(chr.LeftHand);
                                    }
                                }
                                else if (leftOrRight == "right")
                                {
                                    if (chr.lockerList.Count >= Character.MAX_LOCKER)
                                    {
                                        chr.WriteToDisplay("Your locker is full.");
                                    }
                                    else
                                    {
                                        chr.lockerList.Add(chr.RightHand);
                                        chr.UnequipRightHand(chr.RightHand);
                                    }
                                }
                                else if (hand == (int)Globals.eWearOrientation.Right)
                                {
                                    if (chr.lockerList.Count >= Character.MAX_LOCKER)
                                    {
                                        chr.WriteToDisplay("Your locker is full.");
                                    }
                                    else
                                    {
                                        chr.lockerList.Add(chr.RightHand);
                                        chr.UnequipRightHand(chr.RightHand);
                                    }
                                }
                                else if (hand == (int)Globals.eWearOrientation.Left)
                                {
                                    if (chr.lockerList.Count >= Character.MAX_LOCKER)
                                    {
                                        chr.WriteToDisplay("Your locker is full.");
                                    }
                                    else
                                    {
                                        chr.lockerList.Add(chr.LeftHand);
                                        chr.UnequipLeftHand(chr.LeftHand);
                                    }
                                }
                                #endregion
                                return true;
                            }
                            else if (putTarget.ToLower() == "corpse")
                            {
                                #region corpse
                                if (chr.RightHand == null || chr.LeftHand == null)
                                {
                                    chr.WriteToDisplay("You must hold both an item such as a dagger and the item you want to place in your hands.");
                                    return true;
                                }

                                // check if holding a baseType dagger item
                                if ((chr.RightHand != null && chr.RightHand.baseType == Globals.eItemBaseType.Dagger) ||
                                    (chr.LeftHand != null && chr.LeftHand.baseType == Globals.eItemBaseType.Dagger))
                                {
                                    Corpse corpse = (Corpse)Item.FindItemOnGround(Item.ID_CORPSE, chr.CurrentCell);

                                    if (corpse == null)
                                    {
                                        chr.WriteToDisplay("You do not see a " + putTarget + " here.");
                                        return true;
                                    }
                                    else if (putCount > 1)
                                    {
                                        int count = 0;
                                        foreach (Corpse item in chr.CurrentCell.Items)
                                        {
                                            count++;
                                            if (count == putCount)
                                            {
                                                corpse = item;
                                                break;
                                            }
                                        }
                                    }
                                    if (corpse != null && PC.GetOnline(corpse.special) == null)
                                    {
                                        Item placeable = null;

                                        if (chr.RightHand != null && chr.RightHand.baseType == Globals.eItemBaseType.Dagger)
                                            placeable = chr.LeftHand;
                                        else if (chr.LeftHand != null && chr.LeftHand.baseType == Globals.eItemBaseType.Dagger)
                                            placeable = chr.RightHand;

                                        if (placeable != null)
                                        {
                                            if (placeable == chr.RightHand)
                                            {
                                                corpse.Contents.Add(chr.RightHand);
                                                chr.UnequipRightHand(chr.RightHand);
                                            }
                                            else if (placeable == chr.LeftHand)
                                            {
                                                corpse.Contents.Add(chr.LeftHand);
                                                chr.UnequipLeftHand(chr.LeftHand);
                                            }

                                            chr.WriteToDisplay("You make an inscision and place the " + placeable.name + " inside " + corpse.longDesc + ".");
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        chr.WriteToDisplay("You cannot insert items into a player's corpse. That's a little odd.");
                                        return true;
                                    }
                                }
                                else
                                {
                                    chr.WriteToDisplay("You must use a dagger to insert an item into a corpse.");
                                    return true;
                                }
                            #endregion
                            }
                            else if (putTarget.ToLower() == "pouch")
                            {
                                #region pouch
                                if (hand == (int)Globals.eWearOrientation.Right)
                                {
                                    if (chr.RightHand != null)
                                    {
                                        if (chr.RightHand.itemType == Globals.eItemType.Coin)
                                        {
                                            chr.WriteToDisplay("Coins may only be placed in your sack.");
                                        }
                                        else if (chr.RightHand.size == Globals.eItemSize.Sack_Or_Pouch || chr.RightHand.size == Globals.eItemSize.Pouch_Only)
                                        {
                                            if (chr.pouchList.Count < Character.MAX_POUCH)
                                            {
                                                if(chr.PouchItem(chr.RightHand))
                                                    chr.UnequipRightHand(chr.RightHand);
                                            }
                                            else
                                            {
                                                chr.WriteToDisplay("Your pouch is full.");
                                            }
                                        }
                                        else { chr.WriteToDisplay("The " + chr.RightHand.name + " won't fit in your pouch."); }
                                    }
                                }
                                else if (hand == (int)Globals.eWearOrientation.Left)
                                {
                                    if (chr.LeftHand != null)
                                    {
                                        if(chr.LeftHand.itemType == Globals.eItemType.Coin)
                                        {
                                            chr.WriteToDisplay("Coins may only be placed in your sack.");
                                        }
                                        else if (chr.LeftHand.size == Globals.eItemSize.Sack_Or_Pouch || chr.LeftHand.size == Globals.eItemSize.Pouch_Only)
                                        {
                                            if (chr.pouchList.Count < Character.MAX_POUCH)
                                            {
                                                if(chr.PouchItem(chr.LeftHand))
                                                    chr.UnequipLeftHand(chr.LeftHand);
                                            }
                                            else
                                            {
                                                chr.WriteToDisplay("Your pouch is full.");
                                            }
                                        }
                                        else { chr.WriteToDisplay("The " + chr.LeftHand.name + " won't fit in your pouch."); }
                                    }
                                }
                                #endregion
                            }
                            break;
                            #endregion
                        case "on":
                            #region "on"
                            #region putTarget is counter or altar
                            if (putTarget == "counter" || putTarget == "coutner" || putTarget == "altar" || putTarget == "alter")
                            {
                                if (Map.IsNextToCounter(chr))
                                {
                                    #region Dealing with coins here
                                    if (putAmount > 0 && (putItem == "coins" || putItem == "coin"))
                                    {
                                        hand = chr.WhichHand("coins");
                                        if (hand == (int)Globals.eWearOrientation.None)
                                        {
                                            chr.WriteToDisplay("You are not holding any coins.");
                                        }
                                        else
                                        {
                                            if (chr.RightHand.coinValue >= putAmount)
                                            {
                                                Item coins = Item.CopyItemFromDictionary(Item.ID_COINS);
                                                coins.coinValue = putAmount;
                                                chr.RightHand.coinValue -= putAmount;
                                                if (chr.RightHand.coinValue == 0)
                                                {
                                                    chr.UnequipRightHand(chr.RightHand);
                                                }
                                                Map.PutItemOnCounter(chr, coins);
                                                return true;
                                            }
                                            else if (chr.LeftHand.coinValue >= putAmount)
                                            {
                                                Item coins = Item.CopyItemFromDictionary(Item.ID_COINS);
                                                coins.coinValue = putAmount;
                                                chr.LeftHand.coinValue -= putAmount;
                                                if (chr.LeftHand.coinValue == 0)
                                                {
                                                    chr.UnequipLeftHand(chr.LeftHand);
                                                }
                                                Map.PutItemOnCounter(chr, coins);
                                                return true;
                                            }
                                            else
                                            {
                                                chr.WriteToDisplay("You don't have " + putAmount + " coins.");
                                                return true;
                                            }
                                        }
                                    }
                                    #endregion

                                    //else, we are dealing with items
                                    hand = chr.WhichHand(putItem);
                                    if (hand == (int)Globals.eWearOrientation.None)
                                    {
                                        chr.WriteToDisplay("You are not holding a " + putItem + ".");
                                        return true;
                                    }
                                    else
                                    {
                                        if (leftOrRight == "left" || hand == (int)Globals.eWearOrientation.Left)
                                        {
                                            Map.PutItemOnCounter(chr, chr.LeftHand);
                                            chr.UnequipLeftHand(chr.LeftHand);
                                        }
                                        else if (leftOrRight == "right" || hand == (int)Globals.eWearOrientation.Right)
                                        {
                                            Map.PutItemOnCounter(chr, chr.RightHand);
                                            chr.UnequipRightHand(chr.RightHand);
                                        }
                                        return true;
                                    }
                                }
                                chr.WriteToDisplay("You are not near a " + putTarget + ".");
                                return true;
                            }
                            #endregion

                            // currently only used to put the Hummingbird Amulet on the pedestal for the Hummingbird Longsword
                            else if (putTarget == "pedestal")
                            {
                                #region pedestal (Hummingbird Amulet Event)
                                //TODO: create a TriggeredEvent class

                                Cell hummingbirdCell = Cell.GetCell(chr.FacetID, Land.ID_BEGINNERSGAME, Map.ID_UNDERKINGDOM, 87, 31, -245);

                                hand = chr.WhichHand(putItem);

                                if (chr.CurrentCell == hummingbirdCell)
                                {
                                    if (hand == (int)Globals.eWearOrientation.None)
                                    {
                                        chr.WriteToDisplay("You are not holding a " + putItem + ".");
                                    }
                                    else
                                    {
                                        Item item = (leftOrRight == "left" || hand == (int)Globals.eWearOrientation.Left) ? chr.LeftHand : chr.RightHand;
                                        Item amulet = Item.CopyItemFromDictionary(Item.ID_HUMMINGBIRD_AMULET);

                                        // same ID, full charges
                                        if (item != null && item.itemID == amulet.itemID && item.charges == amulet.charges)
                                        {
                                            if (!item.IsAttunedToOther(chr))
                                            {
                                                AreaEffect areaEffect = new AreaEffect(Effect.EffectTypes.Whirlwind, Cell.GRAPHIC_WHIRLWIND, 50, 5, null, chr.CurrentCell);
                                                chr.CurrentCell.EmitSound(Spells.GameSpell.GameSpellDictionary[(int)Spells.GameSpell.GameSpellID.Whirlwind].SoundFile);
                                                Item sword = null;
                                                switch (chr.Alignment)
                                                {
                                                    case Globals.eAlignment.Evil:
                                                        sword = Item.CopyItemFromDictionary(Item.ID_HUMMINGBIRD_LONGSWORD_EVIL);
                                                        break;
                                                    case Globals.eAlignment.Neutral:
                                                        sword = Item.CopyItemFromDictionary(Item.ID_HUMMINGBIRD_LONGSWORD_NEUTRAL);
                                                        break;
                                                    case Globals.eAlignment.Lawful:
                                                        sword = Item.CopyItemFromDictionary(Item.ID_HUMMINGBIRD_LONGSWORD_LAWFUL);
                                                        break;
                                                }

                                                item.charges--; // reduce charges in Hummingbird Amulet so the same one may not be used to get another Hummingbird Longsword

                                                if (sword != null)
                                                {
                                                    chr.CurrentCell.SendToAllInSight("A longsword appears above you, spinning rapidly in the howling wind before slowly descending to hover within reach!");
                                                    sword.AttuneItem(chr);
                                                    chr.CurrentCell.Add(sword);
                                                }
                                            }
                                            else // item is attuned to another
                                            {
                                                chr.WriteToDisplay("You put the " + putItem + " on the " + putTarget + " and nothing happens. The " + item.name + " is soulbound to another.");
                                            }
                                        }
                                        else
                                        {
                                            chr.WriteToDisplay("You put the " + putItem + " on the " + putTarget + " and nothing happens.");
                                        }

                                        if (leftOrRight == "left" || hand == (int)Globals.eWearOrientation.Left)
                                        {
                                            chr.CurrentCell.Add(chr.LeftHand);
                                            chr.UnequipLeftHand(chr.LeftHand);
                                        }
                                        else if (leftOrRight == "right" || hand == (int)Globals.eWearOrientation.Right)
                                        {
                                            chr.CurrentCell.Add(chr.RightHand);
                                            chr.UnequipRightHand(chr.RightHand);
                                        }
                                    }
                                }
                                else
                                {
                                    if (hand == (int)Globals.eWearOrientation.None)
                                    {
                                        chr.WriteToDisplay("You are not holding a " + putItem + ".");
                                    }
                                    else
                                    {
                                        chr.WriteToDisplay("You do not see a " + putTarget + " here.");
                                    }
                                } 
                                #endregion
                                return true;
                            }

                            #region putTarget "right" or "left"
                            if (putTarget == "right" || putTarget == "left")
                            {
                                hand = chr.WhichHand(putItem);

                                if (hand == (int)Globals.eWearOrientation.None)
                                {
                                    chr.WriteToDisplay("You are not holding a " + putItem + ".");
                                    return true;
                                }

                                // can't wear a ring if wearing gauntlets
                                foreach (Item wItem in chr.wearing)
                                {
                                    // Add items that don't cover fingers so wearing rings is allowed while item is equipped.
                                    if (wItem.wearLocation == Globals.eWearLocation.Hands && !wItem.longDesc.ToLower().Contains("fingerless"))
                                    {
                                        chr.WriteToDisplay("You need to remove your " + wItem.name + " first.");
                                        return true;
                                    }
                                }

                                //TODO: simplify the code below

                                #region put ring on left hand
                                if (leftOrRight == "right" || hand == (int)Globals.eWearOrientation.Right)
                                {
                                    if (putTarget == "right") { chr.WriteToDisplay("You must have the ring in your left hand."); return true; }
                                    Item tItem;
                                    tItem = chr.RightHand;

                                    switch (putLocation) // check which finger and make sure its empty
                                    {
                                        case "1":
                                            if (chr.LeftRing1 != null)
                                            {
                                                chr.WriteToDisplay("You already have a ring there.");
                                                return true;
                                            }
                                            if (tItem.isRecall && chr.CurrentCell.IsNoRecall && !chr.IsImmortal)
                                            {
                                                chr.WriteToDisplay("A powerful force prevents you from using recall magic!");
                                                return true;
                                            }
                                            // Check if the ring is attuned
                                            if (tItem.IsAttunedToOther(chr))
                                            {
                                                if (tItem.itemID == Item.ID_KNIGHTRING || tItem.itemID == Item.ID_RAVAGERRING) //what happens if it's a knight ring
                                                {
                                                    chr.WriteToDisplay("The ring explodes!");
                                                    Combat.DoSpellDamage(null, chr, null, Rules.Dice.Next(1, 20), "concussion");
                                                    chr.UnequipRightHand(chr.RightHand);
                                                    return true;
                                                }
                                                else
                                                {
                                                    chr.WriteToDisplay("The ring" + GameSystems.Text.TextManager.SOULDBOUND_TO_ANOTHER_AFFIX);
                                                    return true;
                                                }
                                            }
                                            // Check if the ring has alignment
                                            if (!tItem.AlignmentCheck(chr))
                                            {
                                                chr.WriteToDisplay("The ring singes your finger and will not remain in place.");
                                                Combat.DoDamage(chr, chr, Rules.RollD(1, 4), false);
                                                return true;
                                            }
                                            if (tItem.attuneType == Globals.eAttuneType.Wear) { tItem.AttuneItem(chr); } // attune the ring
                                            chr.LeftRing1 = tItem;
                                            chr.LeftRing1.wearOrientation = Globals.eWearOrientation.LeftRing1;
                                            chr.RightHand = null;
                                            //currently no message is given if the recall does not set
                                            if (tItem.isRecall)
                                            {
                                                Item.SetRecallVariables(tItem, chr);
                                            }
                                            //apply effects
                                            if (tItem.effectType.Length > 0)
                                            {
                                                Effect.AddWornEffectToCharacter(chr, chr.LeftRing1);
                                            }
                                            break;
                                        case "2":
                                            if (chr.LeftRing2 != null)
                                            {
                                                chr.WriteToDisplay("You already have a ring there.");
                                                return true;
                                            }
                                            if (tItem.isRecall && chr.CurrentCell.IsNoRecall && !chr.IsImmortal)
                                            {
                                                chr.WriteToDisplay("A powerful force prevents you from using recall magic!");
                                                return true;
                                            }
                                            // Check if the ring is attuned
                                            if (tItem.IsAttunedToOther(chr))
                                            {
                                                if (tItem.itemID == Item.ID_KNIGHTRING || tItem.itemID == Item.ID_RAVAGERRING) //what happens if it's a knight ring
                                                {
                                                    chr.WriteToDisplay("The ring explodes!");
                                                    Combat.DoSpellDamage(null, chr, null, Rules.Dice.Next(1, 20), "concussion");
                                                    chr.UnequipRightHand(chr.RightHand);
                                                    return true;
                                                }
                                                else
                                                {
                                                    chr.WriteToDisplay("The ring" + GameSystems.Text.TextManager.SOULDBOUND_TO_ANOTHER_AFFIX);
                                                    return true;
                                                }
                                            }
                                            // Check if the ring has alignment
                                            if (!tItem.AlignmentCheck(chr))
                                            {
                                                chr.WriteToDisplay("The ring singes your finger and will not remain in place.");
                                                Combat.DoDamage(chr, chr, Rules.RollD(1, 4), false);
                                                return true;
                                            }
                                            if (tItem.attuneType == Globals.eAttuneType.Wear) { tItem.AttuneItem(chr); } // attune item
                                            chr.LeftRing2 = tItem;
                                            chr.LeftRing2.wearOrientation = Globals.eWearOrientation.LeftRing2;
                                            chr.RightHand = null;
                                            if (tItem.isRecall)
                                            {
                                                Item.SetRecallVariables(tItem, chr);
                                            }
                                            //apply effects
                                            if (tItem.effectType.Length > 0)
                                            {
                                                Effect.AddWornEffectToCharacter(chr, chr.LeftRing2);
                                            }
                                            break;
                                        case "3":
                                            if (chr.LeftRing3 != null)
                                            {
                                                chr.WriteToDisplay("You already have a ring there.");
                                                return true;
                                            }
                                            if (tItem.isRecall && chr.CurrentCell.IsNoRecall && !chr.IsImmortal)
                                            {
                                                chr.WriteToDisplay("A powerful force prevents you from using recall magic!");
                                                return true;
                                            }
                                            // Check if the ring is attuned
                                            if (tItem.IsAttunedToOther(chr))
                                            {
                                                if (tItem.itemID == Item.ID_KNIGHTRING || tItem.itemID == Item.ID_RAVAGERRING) //what happens if it's a knight ring
                                                {
                                                    chr.WriteToDisplay("The ring explodes!");
                                                    Combat.DoSpellDamage(null, chr, null, Rules.Dice.Next(1, 20), "concussion");
                                                    chr.UnequipRightHand(chr.RightHand);
                                                    return true;
                                                }
                                                else
                                                {
                                                    chr.WriteToDisplay("The ring" + GameSystems.Text.TextManager.SOULDBOUND_TO_ANOTHER_AFFIX);
                                                    return true;
                                                }
                                            }
                                            // Check if the ring has alignment
                                            if (!tItem.AlignmentCheck(chr))
                                            {
                                                chr.WriteToDisplay("The ring singes your finger and will not remain in place.");
                                                Combat.DoDamage(chr, chr, Rules.RollD(1, 4), false);
                                                return true;
                                            }
                                            if (tItem.attuneType == Globals.eAttuneType.Wear) { tItem.AttuneItem(chr); }
                                            chr.LeftRing3 = tItem;
                                            chr.LeftRing3.wearOrientation = Globals.eWearOrientation.LeftRing3;
                                            chr.RightHand = null;
                                            if (tItem.isRecall)
                                            {
                                                Item.SetRecallVariables(tItem, chr);
                                            }
                                            //apply effects
                                            if (tItem.effectType.Length > 0)
                                            {
                                                Effect.AddWornEffectToCharacter(chr, chr.LeftRing3);
                                            }
                                            break;
                                        case "4":
                                            if (chr.LeftRing4 != null)
                                            {
                                                chr.WriteToDisplay("You already have a ring there.");
                                                return true;
                                            }
                                            if (tItem.isRecall && chr.CurrentCell.IsNoRecall && !chr.IsImmortal)
                                            {
                                                chr.WriteToDisplay("A powerful force prevents you from using recall magic!");
                                                return true;
                                            }
                                            // Check if the ring is attuned
                                            if (tItem.IsAttunedToOther(chr))
                                            {
                                                if (tItem.itemID == Item.ID_KNIGHTRING || tItem.itemID == Item.ID_RAVAGERRING) //what happens if it's a knight ring
                                                {
                                                    chr.WriteToDisplay("The ring explodes!");
                                                    Combat.DoSpellDamage(null, chr, null, Rules.Dice.Next(1, 20), "concussion");
                                                    chr.UnequipRightHand(chr.RightHand);
                                                    return true;
                                                }
                                                else
                                                {
                                                    chr.WriteToDisplay("The ring" + GameSystems.Text.TextManager.SOULDBOUND_TO_ANOTHER_AFFIX);
                                                    return true;
                                                }
                                            }
                                            // Check if the ring has alignment
                                            if (!tItem.AlignmentCheck(chr))
                                            {
                                                chr.WriteToDisplay("The ring singes your finger and will not remain in place.");
                                                Combat.DoDamage(chr, chr, Rules.RollD(1, 4), false);
                                                return true;
                                            }
                                            if (tItem.attuneType == Globals.eAttuneType.Wear) { tItem.AttuneItem(chr); }
                                            chr.LeftRing4 = tItem;
                                            chr.LeftRing4.wearOrientation = Globals.eWearOrientation.LeftRing4;
                                            chr.RightHand = null;
                                            if (tItem.isRecall)
                                            {
                                                Item.SetRecallVariables(tItem, chr);
                                            }
                                            //apply effects
                                            if (tItem.effectType.Length > 0)
                                            { Effect.AddWornEffectToCharacter(chr, chr.LeftRing4); 
                                            }
                                            break;
                                        default:
                                            chr.WriteToDisplay("You cannot put a ring there.");
                                            break;
                                    }

                                    if (chr.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol && chr.PCState == Globals.ePlayerState.PLAYING)
                                    {
                                        ProtocolYuusha.SendCharacterStats(chr as PC, chr);
                                        ProtocolYuusha.SendCharacterRings(chr);
                                    }
                                    return true;
                                }
                                #endregion

                                #region put ring on right hand
                                if (leftOrRight == "right" || hand == (int)Globals.eWearOrientation.Left)
                                {
                                    // if the ring is in the wrong hand
                                    if (putTarget == "left") { chr.WriteToDisplay("You must have the ring in your right hand."); return true; }

                                    Item tItem = chr.LeftHand;

                                    switch (putLocation) // check which finger and make sure it's empty
                                    {
                                        case "1":
                                            if (chr.RightRing1 != null)
                                            {
                                                chr.WriteToDisplay("You already have a ring there.");
                                                return true;
                                            }
                                            if (tItem.isRecall && chr.CurrentCell.IsNoRecall && !chr.IsImmortal)
                                            {
                                                chr.WriteToDisplay("A powerful force prevents you from using recall magic!");
                                                return true;
                                            }
                                            // Check if the ring is attuned
                                            if (tItem.IsAttunedToOther(chr))
                                            {
                                                if (tItem.itemID == Item.ID_KNIGHTRING || tItem.itemID == Item.ID_RAVAGERRING) //what happens if it's a knight ring
                                                {
                                                    chr.WriteToDisplay("The ring explodes!");
                                                    Combat.DoSpellDamage(null, chr, null, Rules.Dice.Next(1, 20), "concussion");
                                                    chr.UnequipLeftHand(chr.LeftHand);
                                                    return true;
                                                }
                                                else
                                                {
                                                    chr.WriteToDisplay("The ring" + GameSystems.Text.TextManager.SOULDBOUND_TO_ANOTHER_AFFIX);
                                                    return true;
                                                }
                                            }
                                            // Check if the ring has alignment
                                            if (!tItem.AlignmentCheck(chr))
                                            {
                                                chr.WriteToDisplay("The ring singes your finger and will not remain in place.");
                                                Combat.DoDamage(chr, chr, Rules.RollD(1, 4), false);
                                                return true;
                                            }
                                            if (tItem.attuneType == Globals.eAttuneType.Wear) { tItem.AttuneItem(chr); }
                                            chr.RightRing1 = tItem;
                                            chr.RightRing1.wearOrientation = Globals.eWearOrientation.RightRing1;
                                            chr.LeftHand = null;
                                            if (tItem.isRecall)
                                            {
                                                Item.SetRecallVariables(tItem, chr);
                                            }
                                            //apply effects
                                            if (tItem.effectType.Length > 0)
                                            {Effect.AddWornEffectToCharacter(chr, chr.RightRing1); 
                                            }
                                            break;
                                        case "2":
                                            if (chr.RightRing2 != null)
                                            {
                                                chr.WriteToDisplay("You already have a ring there.");
                                                return true;
                                            }
                                            if (tItem.isRecall && chr.CurrentCell.IsNoRecall && !chr.IsImmortal)
                                            {
                                                chr.WriteToDisplay("A powerful force prevents you from using recall magic!");
                                                return true;
                                            }
                                            // Check if the ring is attuned
                                            if (tItem.IsAttunedToOther(chr))
                                            {
                                                if (tItem.itemID == Item.ID_KNIGHTRING || tItem.itemID == Item.ID_RAVAGERRING) //what happens if it's a knight ring
                                                {
                                                    chr.WriteToDisplay("The ring explodes!");
                                                    Combat.DoSpellDamage(null, chr, null, Rules.Dice.Next(1, 20), "concussion");
                                                    chr.UnequipLeftHand(chr.LeftHand);
                                                    return true;
                                                }
                                                else
                                                {
                                                    chr.WriteToDisplay("The ring" + GameSystems.Text.TextManager.SOULDBOUND_TO_ANOTHER_AFFIX);
                                                    return true;
                                                }
                                            }
                                            // Check if the ring has alignment
                                            if (!tItem.AlignmentCheck(chr))
                                            {
                                                chr.WriteToDisplay("The ring singes your finger and will not remain in place.");
                                                Combat.DoDamage(chr, chr, Rules.RollD(1, 4), false);
                                                return true;
                                            }
                                            if (tItem.attuneType == Globals.eAttuneType.Wear) { tItem.AttuneItem(chr); }
                                            chr.RightRing2 = tItem;
                                            chr.RightRing2.wearOrientation = Globals.eWearOrientation.RightRing2;
                                            chr.LeftHand = null;
                                            if (tItem.isRecall)
                                            {
                                                Item.SetRecallVariables(tItem, chr);
                                            }
                                            //apply effects
                                            if (tItem.effectType.Length > 0)
                                            {Effect.AddWornEffectToCharacter(chr, chr.RightRing2);
                                            }
                                            break;
                                        case "3":
                                            if (chr.RightRing3 != null)
                                            {
                                                chr.WriteToDisplay("You already have a ring there.");
                                                return true;
                                            }
                                            if (tItem.isRecall && chr.CurrentCell.IsNoRecall && !chr.IsImmortal)
                                            {
                                                chr.WriteToDisplay("A powerful force prevents you from using recall magic!");
                                                return true;
                                            }
                                            // Check if the ring is attuned
                                            if (tItem.IsAttunedToOther(chr))
                                            {
                                                if (tItem.itemID == Item.ID_KNIGHTRING || tItem.itemID == Item.ID_RAVAGERRING) //what happens if it's a knight ring
                                                {
                                                    chr.WriteToDisplay("The ring explodes!");
                                                    Combat.DoSpellDamage(null, chr, null, Rules.Dice.Next(1, 20), "concussion");
                                                    chr.UnequipLeftHand(chr.LeftHand);
                                                    return true;
                                                }
                                                else
                                                {
                                                    chr.WriteToDisplay("The ring" + GameSystems.Text.TextManager.SOULDBOUND_TO_ANOTHER_AFFIX);
                                                    return true;
                                                }
                                            }
                                            // Check if the ring has alignment
                                            if (!tItem.AlignmentCheck(chr))
                                            {
                                                chr.WriteToDisplay("The ring singes your finger and will not remain in place.");
                                                Combat.DoDamage(chr, chr, Rules.RollD(1, 4), false);
                                                return true;
                                            }
                                            if (tItem.attuneType == Globals.eAttuneType.Wear) { tItem.AttuneItem(chr); }
                                            chr.RightRing3 = tItem;
                                            chr.RightRing3.wearOrientation = Globals.eWearOrientation.RightRing3;
                                            chr.LeftHand = null;
                                            if (tItem.isRecall)
                                            {
                                                Item.SetRecallVariables(tItem, chr);
                                            }
                                            //apply effects
                                            if (tItem.effectType.Length > 0)
                                            { Effect.AddWornEffectToCharacter(chr, chr.RightRing3); 
                                            }
                                            break;
                                        case "4":
                                            if (chr.RightRing4 != null)
                                            {
                                                chr.WriteToDisplay("You already have a ring there.");
                                                return true;
                                            }
                                            if (tItem.isRecall && chr.CurrentCell.IsNoRecall && !chr.IsImmortal)
                                            {
                                                chr.WriteToDisplay("A powerful force prevents you from using recall magic!");
                                                return true;
                                            }
                                            // Check if the ring is attuned
                                            if (tItem.IsAttunedToOther(chr))
                                            {
                                                if (tItem.itemID == Item.ID_KNIGHTRING || tItem.itemID == Item.ID_RAVAGERRING) //what happens if it's a knight ring
                                                {
                                                    chr.WriteToDisplay("The ring explodes!");
                                                    Combat.DoSpellDamage(null, chr, null, Rules.Dice.Next(1, 20), "concussion");
                                                    chr.UnequipLeftHand(chr.LeftHand);
                                                    return true;
                                                }
                                                else
                                                {
                                                    chr.WriteToDisplay("The ring" + GameSystems.Text.TextManager.SOULDBOUND_TO_ANOTHER_AFFIX);
                                                    return true;
                                                }
                                            }
                                            // Check if the ring has alignment
                                            if (!tItem.AlignmentCheck(chr))
                                            {
                                                chr.WriteToDisplay("The ring singes your finger and will not remain in place.");
                                                Combat.DoDamage(chr, chr, Rules.RollD(1, 4), false);
                                                return true;
                                            }
                                            if (tItem.attuneType == Globals.eAttuneType.Wear) { tItem.AttuneItem(chr); }
                                            chr.RightRing4 = tItem;
                                            chr.RightRing4.wearOrientation = Globals.eWearOrientation.RightRing4;
                                            chr.LeftHand = null;
                                            if (tItem.isRecall)
                                            {
                                                Item.SetRecallVariables(tItem, chr);
                                            }
                                            //apply effects
                                            if (tItem.effectType.Length > 0)
                                            {Effect.AddWornEffectToCharacter(chr, chr.RightRing4); 
                                            }
                                            break;
                                        default:
                                            chr.WriteToDisplay("You cannot put a ring there.");
                                            break;
                                    }
                                    if (chr.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol && chr.PCState == Globals.ePlayerState.PLAYING)
                                    {
                                        ProtocolYuusha.SendCharacterStats(chr as PC, chr);
                                        ProtocolYuusha.SendCharacterRings(chr);
                                    }
                                    return true;
                                }
                                #endregion
                            }
                            #endregion
                            else
                            {
                                hand = chr.WhichHand(putItem);
                                Item item = null;

                                if (hand == (int)Globals.eWearOrientation.None)
                                {
                                    chr.WriteToDisplay("You are not holding a " + putItem + ".");
                                    return true;
                                }

                                int inventoryCount = 0;

                                switch (putTarget.ToLower())
                                {
                                    case "ear":
                                        #region ear
                                        foreach (Item wItem in chr.wearing)
                                        {
                                            if (wItem.wearLocation == Globals.eWearLocation.Ear)
                                            {
                                                inventoryCount++;

                                                if (wItem.wearOrientation == Globals.eWearOrientation.Left && putLocation.ToLower() == "left")
                                                {
                                                    chr.WriteToDisplay("You are already wearing a " + putItem + " on your left " + putTarget + ".");
                                                    return true;
                                                }
                                                else if (wItem.wearOrientation == Globals.eWearOrientation.Right && putLocation.ToLower() == "right")
                                                {
                                                    chr.WriteToDisplay("You are already wearing a " + putItem + " on your right " + putTarget + ".");
                                                    return true;
                                                }
                                            }
                                        }

                                        if (hand == (int)Globals.eWearOrientation.Right)
                                            item = chr.RightHand;
                                        else item = chr.LeftHand;

                                        if (item.wearLocation != Globals.eWearLocation.Ear)
                                        {
                                            chr.WriteToDisplay("You cannot wear a " + putItem + " on your ear.");
                                            return true;
                                        }

                                        if (inventoryCount >= Globals.Max_Wearable[(int)item.wearLocation])
                                        {
                                            chr.WriteToDisplay("You do not have an inventory location available for your " + putTarget + ".");
                                            return true;
                                        }

                                        item.wearOrientation = (Globals.eWearOrientation)Enum.Parse(typeof(Globals.eWearOrientation), putLocation, true);
                                        chr.WearItem(item);
                                        break;
                                        #endregion
                                    case "wrist":
                                        #region wrist
                                        foreach (Item wItem in chr.wearing)
                                        {
                                            if (wItem.wearLocation == Globals.eWearLocation.Wrist)
                                            {
                                                inventoryCount++;

                                                if (wItem.wearOrientation == Globals.eWearOrientation.Left && putLocation.ToLower() == "left")
                                                {
                                                    chr.WriteToDisplay("You are already wearing a " + putItem + " on your left " + putTarget + ".");
                                                    return true;
                                                }
                                                else if (wItem.wearOrientation == Globals.eWearOrientation.Right && putLocation.ToLower() == "right")
                                                {
                                                    chr.WriteToDisplay("You are already wearing a " + putItem + " on your right " + putTarget + ".");
                                                    return true;
                                                }
                                            }
                                        }

                                        if (hand == (int)Globals.eWearOrientation.Right)
                                            item = chr.RightHand;
                                        else item = chr.LeftHand;

                                        if(item.wearLocation != Globals.eWearLocation.Wrist)
                                        {
                                            chr.WriteToDisplay("You cannot wear a " + putItem + " on your wrist.");
                                            return true;
                                        }

                                        if (inventoryCount >= Globals.Max_Wearable[(int)item.wearLocation])
                                        {
                                            chr.WriteToDisplay("You do not have an inventory location available for your " + putTarget + ".");
                                            return true;
                                        }

                                        item.wearOrientation = (Globals.eWearOrientation)Enum.Parse(typeof(Globals.eWearOrientation), putLocation, true);
                                        chr.WearItem(item);
                                        break;
                                        #endregion
                                    case "bicep":
                                        #region bicep
                                        foreach (Item wItem in chr.wearing)
                                        {
                                            if (wItem.wearLocation == Globals.eWearLocation.Bicep)
                                            {
                                                inventoryCount++;

                                                if (wItem.wearOrientation == Globals.eWearOrientation.Left && putLocation.ToLower() == "left")
                                                {
                                                    chr.WriteToDisplay("You are already wearing a " + putItem + " on your left " + putTarget + ".");
                                                    return true;
                                                }
                                                else if (wItem.wearOrientation == Globals.eWearOrientation.Right && putLocation.ToLower() == "right")
                                                {
                                                    chr.WriteToDisplay("You are already wearing a " + putItem + " on your right " + putTarget + ".");
                                                    return true;
                                                }
                                            }
                                        }

                                        if (hand == (int)Globals.eWearOrientation.Right)
                                            item = chr.RightHand;
                                        else item = chr.LeftHand;

                                        if (item.wearLocation != Globals.eWearLocation.Bicep)
                                        {
                                            chr.WriteToDisplay("You cannot wear a " + putItem + " on your bicep.");
                                            return true;
                                        }

                                        if (inventoryCount >= Globals.Max_Wearable[(int)item.wearLocation])
                                        {
                                            chr.WriteToDisplay("You do not have an inventory location available for your " + putTarget + ".");
                                            return true;
                                        }

                                        item.wearOrientation = (Globals.eWearOrientation)Enum.Parse(typeof(Globals.eWearOrientation), putLocation, true);
                                        chr.WearItem(item);
                                        break;
                                        #endregion
                                    case "corpse":
                                        //iterate through corpses in cell according to putCount
                                        //need sacrificial dagger?
                                        break;
                                    default:
                                        chr.WriteToDisplay("You can't put a " + putItem + " on a " + putTarget + ".");
                                        break;
                                }
                            }
                            break;
                            #endregion
                        case "under":
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Utils.Log("Failure at Command.put( " + args + " ) by " + chr.GetLogString(), Utils.LogType.SystemFailure);
                    Utils.LogException(e);
                }
            }

            return true;
        }
    }
}
