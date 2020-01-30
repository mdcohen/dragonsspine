using System;
using System.Collections.Generic;
using TargetAcquisition = DragonsSpine.GameSystems.Targeting.TargetAquisition;

namespace DragonsSpine.Talents
{
    [TalentAttribute("steal", "Steal", "Steal items and gold from a target.", false, 3, 1000, 1, 10, true, new string[] { "steal from <target>", "steal from # <target>",
        "steal <item> from <target>", "steal <item> from # <target>"},
        Character.ClassType.Thief)]
    public class StealTalent : ITalentHandler
    {
        public bool MeetsRequirements(Character chr, Character target)
        {
            return true;
        }

        public bool OnPerform(Character chr, string args)
        {
            // if character does not have a hand free they cannot steal
            if (chr.GetFirstFreeHand() == (int)Globals.eWearOrientation.None)
            {
                chr.WriteToDisplay("You don't have a hand free to steal with.");
                return false;
            }

            // get the array of arguments -- should be target at [1] or item at [1] and target at [2]
            List<string> sArgs = new List<string>(args.Split(" ".ToCharArray()));
            sArgs.RemoveAll(s => s.Equals("from"));

            // target should be the last argument
            Character target = null;

            string itemTarget = "";
            
            if(sArgs.Count == 1)
            {
                target = TargetAcquisition.FindTargetInCell(chr, sArgs[0]);
            }
            else if (sArgs.Count >= 2)
            {
                if(Int32.TryParse(sArgs[0], out int countTo)) // steal from # <target>
                {
                    target = TargetAcquisition.FindTargetInCell(chr, sArgs[sArgs.Count - 1], countTo);
                }
                else if(sArgs.Count >= 3 && Int32.TryParse(sArgs[1], out countTo)) // steal <item> from # <target>
                {
                    itemTarget = sArgs[0];
                    target = TargetAcquisition.FindTargetInCell(chr, sArgs[sArgs.Count - 1], countTo);
                }
            }

            // target not found
            if (target == null)
            {
                chr.WriteToDisplay("You don't see " + sArgs[sArgs.Count - 1] + " here.");
                return false;
            }

            if (target == chr)
            {
                chr.WriteToDisplay("You cannot steal from yourself.");
                return false;
            }

            if (target != null && target.IsImage)
            {
                chr.WriteToDisplay("You cannot steal from a conjured image.");
                return false;
            }

            // give experience for a steal attempt
            if (target.IsPC && chr.IsPC)
                Skills.GiveSkillExp(chr, Skills.GetSkillLevel(chr.thievery) - Rules.GetExpLevel(target.Experience), Globals.eSkillType.Thievery);
            else
                Skills.GiveSkillExp(chr, target, Globals.eSkillType.Thievery);

            // 50 percent base chance to steal
            int successChance = 50;

            // if going after a specific item success is reduced by max skill level minus thievery level
            if (sArgs.Count >= 2 && itemTarget != "")
                successChance -= 19 - Skills.GetSkillLevel(chr.thievery);

            Item item = null;

            // this is the 100 sided die roll plus the character's thievery skill level
            int roll = Rules.RollD(1, 100);

            // you successfully steal when the roll plus thievery skill level x 2 is greater than the chance
            if (roll + (Skills.GetSkillLevel(chr.thievery) * 2) > successChance)
            {
                if (target.sackList.Count <= 0)
                {
                    chr.WriteToDisplay("Your target has nothing to steal.");
                    return true;
                }

                double sackGold = 0;

                foreach (Item sackItem in target.sackList)
                {
                    if (sackItem.itemType == Globals.eItemType.Coin)
                        sackGold = sackItem.coinValue;
                }

                // if there is no specific item being sought after
                if (itemTarget == "")
                {
                    // steal gold if gold is in sack and another d100 roll is equal to or greater than base chance
                    if (sackGold > 0 && Rules.RollD(1, 100) >= successChance)
                    {
                        item = Item.CopyItemFromDictionary(Item.ID_COINS);
                        double amount = Rules.RollD(1, (int)sackGold);
                        item.coinValue = amount;
                        sackGold -= amount;
                        foreach (Item sackItem in target.sackList)
                        {
                            if (sackItem.itemType == Globals.eItemType.Coin)
                                sackItem.coinValue = sackGold;
                        }
                    }
                    else
                    {
                        int randomItem = Rules.Dice.Next(target.sackList.Count - 1);
                        item = (Item)target.sackList[randomItem];

                        // Attempting to steal an artifact.
                        if (item.IsArtifact() && chr.PossessesItem(item.itemID))
                        {
                            chr.WriteToDisplay("You have been caught!");
                            target.WriteToDisplay(chr.GetNameForActionResult() + " has attempted to steal " + item.shortDesc + " from you.");
                            Combat.DoFlag(chr, target);
                            Rules.BreakHideSpell(chr);
                            return true;
                        }

                        target.sackList.RemoveAt(randomItem);
                    }
                }
                else
                {
                    if (!itemTarget.StartsWith("coin"))
                    {
                        item = target.RemoveFromSack(itemTarget);

                        // Attempting to steal an artifact.
                        if(item.IsArtifact() && (chr as PC).PossessesItem(item.itemID))
                        {
                            target.SackItem(item);

                            chr.WriteToDisplay("You have been caught!");
                            target.WriteToDisplay(chr.GetNameForActionResult() + " has attempted to steal " + item.shortDesc + " from you.");
                            Combat.DoFlag(chr, target);
                            Rules.BreakHideSpell(chr);
                            return true;
                        }
                    }
                    else
                    {
                        if (sackGold > 0)
                        {
                            item = Item.CopyItemFromDictionary(Item.ID_COINS);
                            double amount = Rules.RollD(1, (int)sackGold);
                            item.coinValue = amount;
                            sackGold -= amount;
                            foreach (Item sackItem in target.sackList)
                            {
                                if (sackItem.itemType == Globals.eItemType.Coin)
                                    sackItem.coinValue = sackGold;
                            }
                        }
                        else
                        {
                            chr.WriteToDisplay("You could not find a " + itemTarget + ".");
                            return true;
                        }
                    }

                    if (item == null)
                    {
                        chr.WriteToDisplay("You could not find a " + itemTarget + ".");
                        return true;
                    }
                    // further chance to be caught if going after a specific item
                    else if (roll - (Skills.GetSkillLevel(chr.thievery) * 2) > (successChance / 2))
                    {
                        chr.WriteToDisplay("You have been caught!");
                        target.WriteToDisplay(chr.GetNameForActionResult() + " has stolen " + item.shortDesc + " from you.");
                        Combat.DoFlag(chr, target);
                        Rules.BreakHideSpell(chr);
                    }
                }

                chr.EquipEitherHandOrDrop(item);

                Skills.GiveSkillExp(chr, Skills.GetSkillLevel(chr.thievery) * 50, Globals.eSkillType.Thievery);
                //if (target.IsPC && chr.IsPC)
                //    Skills.GiveSkillExp(chr, Skills.GetSkillLevel(chr.thievery) - Rules.GetExpLevel(target.Experience), Globals.eSkillType.Thievery);
                //else
                //    Skills.GiveSkillExp(chr, target, Globals.eSkillType.Thievery);

                chr.WriteToDisplay("You have stolen " + item.shortDesc + " from " + target.GetNameForActionResult(true) + ".");
            }
            else
            {
                chr.WriteToDisplay("You have been caught!");
                target.WriteToDisplay(chr.Name + " has attempted to steal from you!");
                Combat.DoFlag(chr, target);
                Rules.BreakHideSpell(chr);
            }
            return true;
        }
    }
}
