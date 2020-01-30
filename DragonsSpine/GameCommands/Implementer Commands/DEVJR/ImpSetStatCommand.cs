using System;

namespace DragonsSpine.Commands
{
    [CommandAttribute("impsetstat", "Give experience to a player.", (int)Globals.eImpLevel.DEVJR, new string[] { "impstat" },
        0, new string[] { "impsetstat <stat> <target>" }, Globals.ePlayerState.PLAYING)]
    public class ImpSetStatCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("impsetstat ('str ... cha, implevel, invis, align') (value) (player)");
                return true;
            }
            else
            {
                String[] sArgs = args.Split(" ".ToCharArray());

                if (sArgs.Length < 3)
                {
                    chr.WriteToDisplay("impsetstat ('str ... cha, implevel, invis, align') (value) (player)");
                    return true;
                }

                string stat = sArgs[0].ToLower();

                Int32.TryParse(sArgs[1], out int num);

                PC target = null;

                foreach (PC chra in Character.PCInGameWorld)
                {
                    if (chra.Name.ToLower() == sArgs[2].ToLower())
                    {
                        target = chra;
                    }
                }

                if (target == null)
                {
                    GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, sArgs[0], 0);
                }

                if (target != null)
                {
                    switch (stat.ToLower())
                    {
                        case "hitsmax":
                        case "hpmax":
                            chr.WriteToDisplay(target.Name + "'s HitsMax changed from " + target.HitsMax + " to " + num + ".");
                            target.HitsMax = num;
                            break;
                        case "staminamax":
                        case "stammax":
                            chr.WriteToDisplay(target.Name + "'s StaminaMax changed from " + target.StaminaMax + " to " + num + ".");
                            target.StaminaMax = num;
                            break;
                        case "manamax":
                            chr.WriteToDisplay(target.Name + "'s ManaMax changed from " + target.ManaMax + " to " + num + ".");
                            target.ManaMax = num;
                            break;
                        case "constitution":
                        case "con":
                            chr.WriteToDisplay(target.Name + "'s constitution changed from " + target.Constitution + " to " + num + ".");
                            target.Constitution = num;
                            break;
                        case "strength":
                        case "str":
                            chr.WriteToDisplay(target.Name + "'s strength changed from " + target.Strength + " to " + num + ".");
                            target.Strength = num;
                            break;
                        case "dexterity":
                        case "dex":
                            chr.WriteToDisplay(target.Name + "'s dexterity changed from " + target.Dexterity + " to " + num + ".");
                            target.Dexterity = num;
                            break;
                        case "intelligence":
                        case "int":
                            chr.WriteToDisplay(target.Name + "'s intelligence changed from " + target.Intelligence + " to " + num + ".");
                            target.Intelligence = num;
                            break;
                        case "wisdom":
                        case "wis":
                            chr.WriteToDisplay(target.Name + "'s wisdom changed from " + target.Wisdom + " to " + num + ".");
                            target.Wisdom = num;
                            break;
                        case "charisma":
                        case "cha":
                            chr.WriteToDisplay(target.Name + "'s charisma changed from " + target.Charisma + " to " + num + ".");
                            target.Charisma = num;
                            break;
                        case "implevel":
                            chr.WriteToDisplay(target.Name + "'s implevel changed from " + target.ImpLevel + " to " + num + ".");
                            target.ImpLevel = (Globals.eImpLevel)num;
                            break;
                        case "invis":
                            if (num == 1)
                            {
                                chr.WriteToDisplay(target.Name + " is now invisible.");
                                if (target.Name != chr.Name) { target.WriteToDisplay("You are now invisible."); }
                                target.IsInvisible = true;
                            }
                            else
                            {
                                chr.WriteToDisplay(target.Name + " is no longer invisible.");
                                if (target.Name != chr.Name) { target.WriteToDisplay("You are no longer invisible."); }
                                target.IsInvisible = false;
                            }
                            break;
                        case "align":
                        case "alignment":
                            {
                                Globals.eAlignment alignment = target.Alignment;

                                try
                                {
                                    alignment = (Globals.eAlignment)Enum.Parse(typeof(Globals.eAlignment), sArgs[1], false);
                                }
                                catch
                                {
                                    chr.WriteToDisplay("Invalid alignment '" + sArgs[2] + "'");
                                }

                                chr.WriteToDisplay(target.Name + "'s alignment changed from " + target.Alignment + " to " + alignment + ".");

                                target.Alignment = alignment;
                            }
                            break;
                        default:
                            break;
                    }

                    foreach(Globals.eSkillType skillType in Enum.GetValues(typeof(Globals.eSkillType)))
                    {
                        if(stat.ToLower() == skillType.ToString().ToLower())
                        {
                            target.SetSkillExperience(skillType, Skills.GetSkillForLevel(Convert.ToInt32(sArgs[1])));
                            chr.WriteToDisplay(target.GetNameForActionResult(false) + "'s " + Utils.FormatEnumString(skillType.ToString()) + " set to skill level " + sArgs[1] + ".");
                            break;
                        }
                    }
                }
                return true;
            }
        }
    }
}