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
    [CommandAttribute("remove", "Remove a worn item.", (int)Globals.eImpLevel.USER, new string[] { "removeitem", "rem" },
        1, new string[] { "remove <item>", "remove ring from # [left | right]", "remove <item> from [left | right] [bicep | wrist]" }, Globals.ePlayerState.PLAYING)]
    public class RemoveCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Remove what?");
                return true;
            }

            // quick escape "remove ring" command
            if (args == "ring")
            {
                if (chr.RightHand != null && chr.LeftHand != null)
                {
                    chr.WriteToDisplay("Your hands are full!");
                    return true;
                }

                if (chr.LeftHand == null)
                {
                    return GameCommand.GameCommandDictionary["remove"].Handler.OnCommand(chr, "1 ring from right");
                }
                else if (chr.RightHand == null)
                {
                    return GameCommand.GameCommandDictionary["remove"].Handler.OnCommand(chr, "1 ring from left");
                }
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            sArgs[0] = sArgs[0].ToLower();

            // remove left bracelet
            if (sArgs.Length == 2)
            {
                foreach (Item item in chr.wearing)
                {
                    if (item.name == sArgs[1] && item.wearOrientation.ToString().ToLower() == sArgs[0].ToLower())
                    {
                        if (chr.RightHand == null)
                        {
                            chr.RightHand = item;
                            chr.RemoveWornItem(item);
                        }
                        else if (chr.LeftHand == null)
                        {
                            chr.LeftHand = item;
                            chr.RemoveWornItem(item);
                        }
                        else
                        {
                            chr.WriteToDisplay("Your hands are full!");
                        }
                        return true;
                    }
                }
                chr.WriteToDisplay("You are not wearing a " + sArgs[1] + " there.");
                return true;
            }
            if (sArgs[0] != "ring" && sArgs.Length == 1)
            {
                foreach (Item item in chr.wearing)
                {
                    if (item.name.ToLower() == sArgs[0].ToLower())
                    {
                        if (item.wearOrientation != Globals.eWearOrientation.None)
                        {
                            string[] wearLocs = Enum.GetNames(typeof(Globals.eWearLocation));
                            for (int a = 0; a < wearLocs.Length; a++)
                            {
                                if (wearLocs[a] == item.wearLocation.ToString())
                                {
                                    if (Globals.Max_Wearable.Length >= a + 1)
                                    {
                                        if (Globals.Max_Wearable[a] > 1)
                                        {
                                            chr.WriteToDisplay("Use \"remove <left | right> <item>\".");
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        if (chr.RightHand == null)
                        {
                            chr.RightHand = item;
                            chr.RemoveWornItem(sArgs[0]);
                        }
                        else if (chr.LeftHand == null)
                        {
                            chr.LeftHand = item;
                            chr.RemoveWornItem(sArgs[0]);
                        }
                        else
                        {
                            chr.WriteToDisplay("Your hands are full!");
                            return true;
                        }
                        return true;
                    }
                }
                chr.WriteToDisplay("You are not wearing a " + sArgs[0] + ".");
                return true;
            }
            // Trap "remove ring", "remove ring from L/R" and "remove x ring" here.
            // run findfirstRing functions, reparse sArgs, continue.
            if ((sArgs[0] == "ring" || sArgs[1] == "ring") && sArgs.Length < 4)
            {
                #region remove ring, remove ring from left/right and remove x ring
                //can't remove a ring if wearing gauntlets
                foreach (Item wItem in chr.wearing)
                {
                    if (wItem.wearLocation == Globals.eWearLocation.Hands)
                    {
                        chr.WriteToDisplay("You need to remove your " + wItem.name + " first.");
                        return true;
                    }
                }

                int firstRightRing = chr.FindFirstRightRing();
                int firstLeftRing = chr.FindFirstLeftRing();
                int firstRing = 0;
                if (firstLeftRing == 0 && firstRightRing == 0)
                {
                    chr.WriteToDisplay("You aren't wearing any rings.");
                    return true;
                }
                else
                {
                    if (sArgs.Length == 3)	//"remove ring from left/right"
                    {
                        if (sArgs[2] == "left")
                        {
                            if (firstLeftRing == 0)
                            {
                                chr.WriteToDisplay("You aren't wearing a ring on that hand.");
                                return true;
                            }
                            firstRing = firstLeftRing + 4;
                        }
                        else if (sArgs[2] == "right")
                        {
                            if (firstRightRing == 0)
                            {
                                chr.WriteToDisplay("You aren't wearing a ring on that hand.");
                                return true;
                            }
                            firstRing = firstRightRing;
                        }
                        else
                        {
                            chr.WriteToDisplay("I don't understand that command.");
                            return true;
                        }

                    }
                    else if (sArgs.Length == 2) //"remove x ring" 
                    {
                        switch (sArgs[0])
                        {
                            case "1":
                                if (chr.RightRing1 != null)
                                    firstRing = 1;
                                else
                                    firstRing = 5;
                                break;
                            case "2":
                                if (chr.RightRing2 != null)
                                    firstRing = 2;
                                else
                                    firstRing = 6;
                                break;
                            case "3":
                                if (chr.RightRing3 != null)
                                    firstRing = 3;
                                else
                                    firstRing = 7;
                                break;
                            case "4":
                                if (chr.RightRing4 != null)
                                    firstRing = 4;
                                else
                                    firstRing = 8;
                                break;
                            default:
                                chr.WriteToDisplay("I don't understand that command.");
                                return true;
                        }
                    }
                    else if (sArgs.Length == 1) //"remove ring"
                    {
                        if (firstRightRing > 0)
                        {
                            firstRing = firstRightRing;
                        }
                        else
                        {
                            firstRing = firstLeftRing + 4;
                        }
                    }
                    switch (firstRing) //Now reparse
                    {
                        case 1:
                            args = "1 ring from right";
                            break;
                        case 2:
                            args = "2 ring from right";
                            break;
                        case 3:
                            args = "3 ring from right";
                            break;
                        case 4:
                            args = "4 ring from right";
                            break;
                        case 5:
                            args = "1 ring from left";
                            break;
                        case 6:
                            args = "2 ring from left";
                            break;
                        case 7:
                            args = "3 ring from left";
                            break;
                        case 8:
                            args = "4 ring from left";
                            break;
                    }
                    return new CommandTasker(chr)["remove", args];
                }
                #endregion
            }

            if (sArgs.Length == 4)
            {
                Item tItem = null;

                // remove x ring from right/left
                if (sArgs[1].ToLower() == "ring")
                {
                    #region removing a ring
                    //can't remove a ring if wearing gauntlets
                    foreach (Item wItem in chr.wearing)
                    {
                        if (wItem.wearLocation == Globals.eWearLocation.Hands)
                        {
                            chr.WriteToDisplay("You need to remove your " + wItem.name + " first.");
                            return true;
                        }
                    }

                    if (sArgs[3] == "right")
                    {
                        #region remove x ring from right
                        if (chr.LeftHand != null) { chr.WriteToDisplay("Your left hand must be empty."); return true; }
                        switch (sArgs[0])
                        {
                            case "1":
                                if (chr.RightRing1 == null) { chr.WriteToDisplay("You do not have a ring on that finger."); return true; }

                                tItem = chr.RightRing1;
                                if (tItem.isRecall) // it is a recall ring
                                {
                                    if (!Item.VerifyRecallMagic(chr, tItem))
                                    {
                                        chr.WriteToDisplay("A powerful force cancels your recall magic!");
                                    }
                                    else
                                    {
                                        Item.Recall(chr, tItem, (int)Globals.eWearOrientation.Left);
                                    }
                                }
                                if (tItem.wasRecall) // was it a recall ring that was reset
                                {
                                    tItem.wasRecall = false;
                                    tItem.isRecall = true;
                                }
                                chr.RightRing1 = null;
                                if (tItem.effectType.Length > 0)
                                {Effect.RemoveWornEffectFromCharacter(chr, tItem); 
                                }
                                break;
                            case "2":
                                if (chr.RightRing2 == null) { chr.WriteToDisplay("You do not have a ring on that finger."); return true; }

                                tItem = chr.RightRing2;
                                if (tItem.isRecall) // it is a recall ring
                                {
                                    if (!Item.VerifyRecallMagic(chr, tItem))
                                    {
                                        chr.WriteToDisplay("A powerful force cancels your recall magic!");
                                    }
                                    else
                                    {
                                        Item.Recall(chr, tItem, (int)Globals.eWearOrientation.Left);
                                    }
                                }
                                if (tItem.wasRecall) // was it a recall ring that was reset
                                {
                                    tItem.wasRecall = false;
                                    tItem.isRecall = true;
                                }
                                chr.RightRing2 = null;
                                if (tItem.effectType.Length > 0)
                                {Effect.RemoveWornEffectFromCharacter(chr, tItem); 
                                }
                                break;
                            case "3":
                                if (chr.RightRing3 == null) { chr.WriteToDisplay("You do not have a ring on that finger."); return true; }

                                tItem = chr.RightRing3;
                                if (tItem.isRecall) // it is a recall ring
                                {
                                    if (!Item.VerifyRecallMagic(chr, tItem))
                                    {
                                        chr.WriteToDisplay("A powerful force cancels your recall magic!");
                                    }
                                    else
                                    {
                                        Item.Recall(chr, tItem, (int)Globals.eWearOrientation.Left);
                                    }
                                }
                                if (tItem.wasRecall) // was it a recall ring that was reset
                                {
                                    tItem.wasRecall = false;
                                    tItem.isRecall = true;
                                }
                                chr.RightRing3 = null;
                                if (tItem.effectType.Length > 0)
                                {Effect.RemoveWornEffectFromCharacter(chr, tItem); 
                                }
                                break;
                            case "4":
                                if (chr.RightRing4 == null) { chr.WriteToDisplay("You do not have a ring on that finger."); return true; }

                                tItem = chr.RightRing4;
                                if (tItem.isRecall) // it is a recall ring
                                {
                                    if (!Item.VerifyRecallMagic(chr, tItem))
                                    {
                                        chr.WriteToDisplay("A powerful force cancels your recall magic!");
                                    }
                                    else
                                    {
                                        Item.Recall(chr, tItem, (int)Globals.eWearOrientation.Left);
                                    }
                                }
                                if (tItem.wasRecall) // was it a recall ring that was reset
                                {
                                    tItem.wasRecall = false;
                                    tItem.isRecall = true;
                                }
                                chr.RightRing4 = null;
                                if (tItem.effectType.Length > 0)
                                {Effect.RemoveWornEffectFromCharacter(chr, tItem); 
                                }
                                break;
                            default:
                                chr.WriteToDisplay("That is not a valid ring finger.");
                                return true;
                        }
                        if (chr.LeftHand == null)
                        {
                            chr.LeftHand = tItem;
                        }
                        #endregion
                    }
                    else if (sArgs[3] == "left")
                    {
                        #region remove x ring from left
                        if (chr.RightHand != null) { chr.WriteToDisplay("Your right hand must be empty."); return true; }
                        switch (sArgs[0])
                        {
                            case "1":
                                if (chr.LeftRing1 == null) { chr.WriteToDisplay("You do not have a ring on that finger."); return true; }

                                tItem = chr.LeftRing1;
                                if (tItem.isRecall) // it is a recall ring
                                {
                                    if (!Item.VerifyRecallMagic(chr, tItem))
                                    {
                                        chr.WriteToDisplay("A powerful force cancels your recall magic!");
                                    }
                                    else
                                    {
                                        Item.Recall(chr, tItem, (int)Globals.eWearOrientation.Right);
                                    }
                                }
                                if (tItem.wasRecall) // was it a recall ring that was reset
                                {
                                    tItem.wasRecall = false;
                                    tItem.isRecall = true;
                                }
                                chr.LeftRing1 = null;
                                if (tItem.effectType.Length > 0)
                                {Effect.RemoveWornEffectFromCharacter(chr, tItem); 
                                }
                                break;
                            case "2":
                                if (chr.LeftRing2 == null) { chr.WriteToDisplay("You do not have a ring on that finger."); return true; }

                                tItem = chr.LeftRing2;
                                if (tItem.isRecall) // it is a recall ring
                                {
                                    if (!Item.VerifyRecallMagic(chr, tItem))
                                    {
                                        chr.WriteToDisplay("A powerful force cancels your recall magic!");
                                    }
                                    else
                                    {
                                        Item.Recall(chr, tItem, (int)Globals.eWearOrientation.Right);
                                    }
                                }
                                if (tItem.wasRecall) // was it a recall ring that was reset
                                {
                                    tItem.wasRecall = false;
                                    tItem.isRecall = true;
                                }
                                chr.LeftRing2 = null;
                                if (tItem.effectType.Length > 0)
                                {Effect.RemoveWornEffectFromCharacter(chr, tItem); 
                                }
                                break;
                            case "3":
                                if (chr.LeftRing3 == null) { chr.WriteToDisplay("You do not have a ring on that finger."); return true; }

                                tItem = chr.LeftRing3;
                                if (tItem.isRecall) // it is a recall ring
                                {
                                    if (!Item.VerifyRecallMagic(chr, tItem))
                                    {
                                        chr.WriteToDisplay("A powerful force cancels your recall magic!");
                                    }
                                    else
                                    {
                                        Item.Recall(chr, tItem, (int)Globals.eWearOrientation.Right);
                                    }
                                }
                                if (tItem.wasRecall) // was it a recall ring that was reset
                                {
                                    tItem.wasRecall = false;
                                    tItem.isRecall = true;
                                }
                                chr.LeftRing3 = null;
                                if (tItem.effectType.Length > 0)
                                {Effect.RemoveWornEffectFromCharacter(chr, tItem); 
                                }
                                break;
                            case "4":
                                if (chr.LeftRing4 == null) { chr.WriteToDisplay("You do not have a ring on that finger."); return true; }

                                tItem = chr.LeftRing4;
                                if (tItem.isRecall) // it is a recall ring
                                {
                                    if (!Item.VerifyRecallMagic(chr, tItem))
                                    {
                                        chr.WriteToDisplay("A powerful force cancels your recall magic!");
                                    }
                                    else
                                    {
                                        Item.Recall(chr, tItem, (int)Globals.eWearOrientation.Right);
                                    }
                                }
                                if (tItem.wasRecall) // was it a recall ring that was reset
                                {
                                    tItem.wasRecall = false;
                                    tItem.isRecall = true;
                                }
                                chr.LeftRing4 = null;
                                if (tItem.effectType.Length > 0)
                                {Effect.RemoveWornEffectFromCharacter(chr, tItem); 
                                }
                                break;
                            default:
                                chr.WriteToDisplay("That is not a valid ring finger.");
                                return true;
                        }
                        if (chr.RightHand == null)
                        {
                            chr.RightHand = tItem;
                        }
                        #endregion
                    }
                    else
                    {
                        chr.WriteToDisplay("You do not have any rings there.");
                        return true;
                    }
                    #endregion
                }
                else if (sArgs[3].ToLower() == "wrist" || sArgs[3].ToLower() == "bicep")
                {
                    #region remove <item> from <orientation> <location>
                    if (sArgs[2].ToLower() == "right" && chr.LeftHand != null)
                    {
                        chr.WriteToDisplay("Your left hand must be empty.");
                        return true;
                    }
                    else if (sArgs[2].ToLower() == "left" && chr.RightHand != null)
                    {
                        chr.WriteToDisplay("Your right hand must be empty.");
                        return true;
                    }

                    Globals.eWearLocation wLocation = Globals.eWearLocation.None;
                    Globals.eWearOrientation wOrientation = Globals.eWearOrientation.None;

                    try
                    {
                        wLocation = (Globals.eWearLocation)Enum.Parse(typeof(Globals.eWearLocation), sArgs[3], true);
                        wOrientation = (Globals.eWearOrientation)Enum.Parse(typeof(Globals.eWearOrientation), sArgs[2], true);
                    }
                    catch
                    {
                        chr.WriteToDisplay("You are not wearing a " + sArgs[0] + " on your " + sArgs[2] + " " + sArgs[3] + ".");
                        return true;
                    }

                    // remove <item> from <orientation> <location>
                    foreach (Item wItem in chr.wearing)
                    {
                        if (wItem.name.ToLower() == sArgs[0].ToLower() && wItem.wearLocation == wLocation && wItem.wearOrientation == wOrientation)
                        {
                            tItem = wItem;
                            break;
                        }
                    }

                    if (tItem == null)
                    {
                        chr.WriteToDisplay("You are not wearing a " + sArgs[0] + " on your " + sArgs[2] + " " + sArgs[3] + ".");
                        return true;
                    }
                    else
                    {
                        chr.RemoveWornItem(tItem);

                        if (wOrientation == Globals.eWearOrientation.Right)
                            chr.EquipLeftHand(tItem);
                        else if (wOrientation == Globals.eWearOrientation.Left)
                            chr.EquipRightHand(tItem);
                    }
                    #endregion
                }
                else
                {
                    chr.WriteToDisplay("I don't understand your command. For a full list of game commands visit the Dragon's Spine forums.");
                }
            }

            return true;
        }
    }
}
