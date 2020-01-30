using System;
using ArrayList = System.Collections.ArrayList;
using TargetAcquisition = DragonsSpine.GameSystems.Targeting.TargetAquisition;

namespace DragonsSpine.Talents
{
    [TalentAttribute("snoop", "Snoop", "Snoop at items carried by a target.", false, 2, 3500, 6, 5, true, new string[] { "snoop <target>", "snoop # <target>" }, Character.ClassType.Thief)]
    public class SnoopTalent : ITalentHandler
    {
        public bool MeetsRequirements(Character chr, Character target)
        {
            return true;
        }

        public bool OnPerform(Character chr, string args)
        {
            if (!chr.IsHidden && !chr.IsImmortal)
            {
                chr.WriteToDisplay("You must be hidden to be successful.");
                return false;
            }

            if (args == null)
            {
                chr.WriteToDisplay("Snoop at who?");
                return false;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            Character target;

            int countTo;
            if (sArgs.Length >= 2 && Int32.TryParse(sArgs[0], out countTo))
                target = TargetAcquisition.FindTargetInCell(chr, sArgs[1], countTo);
            else target = TargetAcquisition.FindTargetInCell(chr, sArgs[0]);

            // failed to find the target
            if (target == null)
            {
                chr.WriteToDisplay("You don't see a " + (sArgs.Length >= 2 ? sArgs[0] + " " + sArgs[1] : sArgs[0]) + " here.");
                return false;
            }

            int successChance = 19 - Skills.GetSkillLevel(chr.thievery);

            if (chr.IsImmortal || Rules.RollD(1, 20) > successChance)
            {
                if (target.sackList.Count > 0)
                {
                    int z = 0, i = 0;
                    string dispMsg = "";
                    double itemcount = 0;
                    bool moreThanOne = false;
                    ArrayList templist = new ArrayList();
                    Item[] itemList = new Item[target.sackList.Count];
                    target.sackList.CopyTo(itemList);
                    foreach (Item item in itemList)
                    {
                        templist.Add(item);
                    }
                    z = templist.Count - 1;
                    dispMsg = "In " + target.GetNameForActionResult(true) + "'s sack you see ";
                    while (z >= 0)
                    {

                        Item item = (Item)templist[z];

                        itemcount = 0;
                        for (i = templist.Count - 1; i > -1; i--)
                        {
                            Item tmpitem = (Item)templist[i];
                            if (tmpitem.name == item.name && tmpitem.name.IndexOf("coin") > -1)
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
                            if (moreThanOne)
                            {
                                if (z == 0)
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
                    chr.WriteToDisplay(target.GetNameForActionResult() + " isn't carrying anything in " + Character.POSSESSIVE[(int)target.gender].ToLower() + " sack.");

                if (target.pouchList.Count > 0)
                {
                    int z = 0, i = 0;
                    string dispMsg = "";
                    double itemcount = 0;
                    bool moreThanOne = false;
                    ArrayList templist = new ArrayList();
                    Item[] itemList = new Item[target.pouchList.Count];
                    target.pouchList.CopyTo(itemList);
                    foreach (Item item in itemList)
                    {
                        templist.Add(item);
                    }
                    z = templist.Count - 1;
                    dispMsg = "In " + target.GetNameForActionResult(true) + "'s pouch you see ";
                    while (z >= 0)
                    {

                        Item item = (Item)templist[z];

                        itemcount = 0;
                        for (i = templist.Count - 1; i > -1; i--)
                        {
                            Item tmpitem = (Item)templist[i];
                            if (tmpitem.name == item.name && tmpitem.name.IndexOf("coin") > -1)
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
                            if (moreThanOne)
                            {
                                if (z == 0)
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
                    chr.WriteToDisplay(target.GetNameForActionResult() + " isn't carrying anything in " + Character.POSSESSIVE[(int)target.gender].ToLower() + " pouch.");
            }
            else
            {
                chr.WriteToDisplay("Your attempt to snoop has failed.");

                // if an intelligence check succeeds then the peeking thief has been caught
                if (Rules.CheckPerception(target))
                {
                    target.WriteToDisplay(chr.Name + " has attempted to peek at your belongings.");
                    chr.WriteToDisplay("Your failure has been noticed!");
                    Rules.BreakHideSpell(chr);
                    Combat.DoFlag(chr, target);
                }
            }

            // give skill experience
            Skills.GiveSkillExp(chr, Skills.GetSkillLevel(chr.thievery) * 20, Globals.eSkillType.Thievery);

            return true;
        }
    }
}
