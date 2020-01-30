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
using World = DragonsSpine.GameWorld.World;
using TargetAcquisition = DragonsSpine.GameSystems.Targeting.TargetAquisition;

namespace DragonsSpine.Commands
{
    [CommandAttribute("give", "Give an item to a target.", (int)Globals.eImpLevel.USER, new string[] { },
        0, new string[] { "give <item> to <target in view>" }, Globals.ePlayerState.PLAYING)]
    public class GiveCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null || args == "")
            {
                chr.WriteToDisplay("Format: give <item> to <target>");
                return false;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            if (sArgs.Length < 3)
            {
                chr.WriteToDisplay("Format: give <item> to <target>");
                return false;
            }

            try
            {
                string giveItemName = sArgs[0];
                string giveTarget = sArgs[2];

                Character target = TargetAcquisition.FindTargetInCell(chr, giveTarget);

                if (target == null)
                {
                    chr.WriteToDisplay("You do not see " + giveTarget + " here.");
                    return false;
                }

                if (chr.WhichHand(giveItemName) == (int)Globals.eWearOrientation.None)
                {
                    chr.WriteToDisplay("You do not appear to have a " + giveItemName + " in your hands.");
                    return false;
                }

                // Give target is a player.
                if (target.IsPC)
                {
                    if (Array.IndexOf((target as PC).ignoreList, chr.UniqueID) != -1)
                    {
                        chr.WriteToDisplay("You cannot give an item to " + target.Name + ".");
                        return false;
                    }

                    int hand = target.GetFirstFreeHand();

                    if (hand == (int)Globals.eWearOrientation.None)
                    {
                        chr.WriteToDisplay(target.Name + " does not have an empty hand.");
                        target.WriteToDisplay(chr.Name + " would like to give you an item but you do not have an empty hand.");
                        return false;
                    }

                    Item giveItem = chr.GetHeldItem(giveItemName);

                    if(giveItem.IsArtifact() && target.PossessesItem(giveItem.itemID))
                    {
                        chr.WriteToDisplay(target.GetNameForActionResult() + " attempted to give you an artifact you already possess.");
                        target.WriteToDisplay(chr.GetNameForActionResult() + " already possesses this artifact.");
                        return true;
                    }

                    if (giveItem != null)
                    {
                        switch (hand)
                        {
                            case (int)Globals.eWearOrientation.Right:
                                if (!target.EquipRightHand(giveItem))
                                    giveItem = null;
                                break;
                            case (int)Globals.eWearOrientation.Left:
                                if (!target.EquipLeftHand(giveItem))
                                    giveItem = null;
                                break;
                        }
                    }

                    if (giveItem != null)
                    {
                        string itemDesc = giveItem.shortDesc + ".";

                        if(giveItem.venom > 0)
                            itemDesc += " It is coated with a caustic venom.";

                        target.WriteToDisplay(chr.Name + " has given you " + itemDesc);

                        if (giveItem == chr.RightHand)
                            chr.UnequipRightHand(chr.RightHand);
                        else if (giveItem == chr.LeftHand)
                            chr.UnequipLeftHand(giveItem);
                    }

                    return true;
                }
                else
                {
                    NPC npc = (NPC)target;

                    Item item = chr.GetHeldItem(giveItemName);

                    if(item == null)
                    {
                        chr.WriteToDisplay("You do not have a " + giveItemName);
                        return true;
                    }

                    if (item.attunedID > 0 && item.attunedID != chr.UniqueID)
                    {
                        if (!npc.animal && npc.Alignment == chr.Alignment)
                        {
                            npc.SendToAllInSight(npc.Name + ": The " + item.name + " does not belong to you, " + chr.Name + ". I cannot accept it.");
                        }
                        chr.EquipEitherHandOrDrop(item);
                        return false;
                    }

                    if (npc.receivedItems.ContainsKey(chr.UniqueID))
                    {
                        if (!npc.receivedItems[chr.UniqueID].Contains(item) && (item.attunedID <= 0 || item.attunedID == chr.UniqueID))
                        {
                            npc.receivedItems[chr.UniqueID].Add(item);
                        }
                        else
                        {
                            goto manageItem;
                        }
                    }
                    else
                    {
                        List<Item> itemList = new List<Item>();
                        itemList.Add(item);
                        npc.receivedItems.Add(chr.UniqueID, itemList);
                    }

                    #region Pets
                    if(npc.PetOwner == chr)
                    {
                        if(Autonomy.EntityBuilding.EntityLists.ANIMAL.Contains(npc.entity) && !Autonomy.EntityBuilding.EntityLists.ANIMALS_WIELDING_WEAPONS.Contains(npc.entity))
                        {
                            chr.WriteToDisplay(npc.GetNameForActionResult(false) + " cannot hold items.");
                            goto manageItem;
                        }

                        if(npc.GetFirstFreeHand() != (int)Globals.eWearOrientation.None) // either right or left hand is free
                        {
                            if(npc.EquipEitherHand(item))
                            {
                                chr.WriteToDisplay("You have given " + item.shortDesc + " to " + npc.GetNameForActionResult(true) + ".");
                                return true;
                            }
                            else
                            {
                                chr.WriteToDisplay("You were unable to give " + item.shortDesc + " to " + npc.GetNameForActionResult(true) + ".");
                                goto manageItem;
                            }
                        }
                        else
                        {
                            chr.WriteToDisplay(npc.GetNameForActionResult(false) + " does not have a free hand.");
                            goto manageItem;
                        }
                    }
                    #endregion

                    #region Confessor AI
                    if ((npc is Merchant) && (npc as Merchant).interactiveType == Merchant.InteractiveType.Confessor)
                    {
                        if (item.baseType == Globals.eItemBaseType.Dagger && item.silver) // any silver dagger will work
                        {
                            if (chr.Alignment == Globals.eAlignment.Neutral && chr.BaseProfession != Character.ClassType.Thief)
                            {
                                chr.Alignment = Globals.eAlignment.Lawful;
                                chr.WriteToDisplay(npc.Name + ": You are forgiven, " + chr.Name + ".");
                                return true;
                            }
                            else if (chr.Alignment == Globals.eAlignment.Evil && Array.IndexOf(World.EvilProfessions, chr.BaseProfession) != -1)
                            {
                                chr.WriteToDisplay(npc.Name + ": You are inherently evil. There is no returning to the light for you. Maybe you should reroll as a goody two-shoes knight?");
                                return true;
                            }
                            else if (chr.Alignment == Globals.eAlignment.Evil && Array.IndexOf(World.EvilProfessions, chr.BaseProfession) == -1)
                            {
                                chr.Alignment = Globals.eAlignment.Neutral;
                                chr.WriteToDisplay(npc.Name + ": You have taken the first step back to the light.");
                                return true;
                            }
                        }
                        else if (item.baseType == Globals.eItemBaseType.Figurine || item.figExp > 0)
                        {
                            if ((chr as PC).currentKarma > 0)
                            {
                                (chr as PC).currentKarma--;
                                chr.WriteToDisplay(npc.Name + ": You have been absolved of " + ((chr as PC).currentKarma > 0 ? "some" : "all") + " guilt, " + chr.Name + ".");
                            }

                            return true;
                        }
                    }
                    #endregion

                    #region Quest NPC
                    if (npc.QuestList.Count > 0)
                    {
                        foreach (GameQuest quest in npc.QuestList)
                        {
                            GameQuest activeQuest = chr.GetQuest(quest.QuestID);

                            short a = 0;

                            if (quest.RequiredItems.ContainsValue(item.itemID) && quest.PlayerMeetsRequirements((PC)chr, true))
                            {
                                if (quest.StepOrder) // this quest has a specified order
                                {
                                    if (activeQuest != null) // chr already has this quest and has not yet completed it, or has already completed it
                                    {
                                        if (quest.IsRepeatable || activeQuest.TimesCompleted <= 0) // quest can be repeated
                                        {
                                            if (activeQuest.Requirements[activeQuest.CurrentStep] == GameQuest.QuestRequirement.Item &&
                                                activeQuest.RequiredItems[activeQuest.CurrentStep] == item.itemID)
                                            {
                                                activeQuest.FinishStep(npc, (PC)chr, activeQuest.CurrentStep);
                                                return true;
                                            }
                                            else
                                            {
                                                goto manageItem;
                                            }
                                        }
                                        else
                                        {
                                            chr.WriteToDisplay("You have already completed \"" + quest.Name + "\".");
                                            chr.EquipEitherHandOrDrop(item);
                                            return true;
                                        }
                                    }
                                }
                                else
                                {
                                    if (activeQuest != null) // chr already has this quest, or has completed it
                                    {
                                        if (quest.IsRepeatable || activeQuest.TimesCompleted <= 0) // quest can be repeated
                                        {
                                            a = 0;

                                            foreach (Item recvdItem in npc.receivedItems[chr.UniqueID]) // count received items
                                            {
                                                a++;
                                            }

                                            if (a == quest.RequiredItems.Count)
                                            {
                                                if (quest.CoinValues.ContainsKey(a))
                                                {
                                                    if (item.coinValue < quest.CoinValues[a])
                                                    {
                                                        goto manageItem;
                                                    }
                                                }

                                                goto questCompleted;
                                            }
                                            else
                                            {
                                                if (quest.CoinValues.ContainsKey(a))
                                                {
                                                    if (item.coinValue < quest.CoinValues[a])
                                                    {
                                                        goto manageItem;
                                                    }
                                                }

                                                if (a > 0)
                                                {
                                                    quest.FinishStep((NPC)target, (PC)chr, a);
                                                }
                                                return true;
                                            }
                                        }
                                        else
                                        {
                                            chr.WriteToDisplay("You have already completed \"" + quest.Name + "\".");
                                            chr.EquipEitherHandOrDrop(item);
                                            return true;
                                        }
                                    }
                                    else // chr does not already have this quest, or has never completed it
                                    {
                                        quest.BeginQuest((PC)chr, true);

                                        a = 0;

                                        foreach (Item recvdItem in npc.receivedItems[chr.UniqueID]) // count received items
                                        {
                                            a++;
                                        }

                                        if (a == quest.RequiredItems.Count)
                                        {
                                            if (quest.CoinValues.ContainsKey(a))
                                            {
                                                if (item.coinValue < quest.CoinValues[a])
                                                {
                                                    goto manageItem;
                                                }
                                            }

                                            goto questCompleted;
                                        }
                                        else
                                        {
                                            if (quest.CoinValues.ContainsKey(a))
                                            {
                                                if (item.coinValue < quest.CoinValues[a])
                                                {
                                                    goto manageItem;
                                                }
                                            }

                                            if (a > 0 && quest.FinishStrings.ContainsKey(a))
                                            {
                                                if (quest.FinishStrings[a] != "")
                                                {
                                                    chr.WriteToDisplay(npc.Name + ": " + quest.FinishStrings[a]);
                                                }
                                            }
                                            return true;
                                        }
                                    }
                                }

                            questCompleted:

                                // break out of this now so the player does not lose an item for a quest that is not available yet
                                if (!quest.PlayerMeetsRequirements((PC)chr, true))
                                {
                                    chr.EquipEitherHandOrDrop(item);
                                    return false;
                                }

                                // verify the quest is started
                                if (activeQuest == null)
                                {
                                    activeQuest = chr.GetQuest(quest.QuestID);
                                    quest.BeginQuest((PC)chr, false);
                                }

                                activeQuest.FinishStep(npc, (PC)chr, a);

                                // clear npc's received items list
                                npc.receivedItems[chr.UniqueID].Clear();

                                return true; // break the foreach loop
                            }
                        }
                    }
                    #endregion

                manageItem:

                    // Give the item back to the player or drop it
                    chr.EquipEitherHandOrDrop(item);
                    npc.receivedItems.Clear();
                    return false;
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return false;
            }
        }
    }
}
