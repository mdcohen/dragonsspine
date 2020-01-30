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
    [CommandAttribute("shoot", "Shoot a target with a weapon. This command will search your belt for the first weapon that can be thrown.",
        (int)Globals.eImpLevel.USER, new string[] { "sh" }, 2, new string[] { "shoot <target>" }, Globals.ePlayerState.PLAYING)]
    public class ShootCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            try
            {
                //if (chr.cmdWeight > 3)
                //{
                //    chr.WriteToDisplay("Command weight limit exceeded. Shoot command not processed.");
                //    return true;
                //}

                if (args == null || args == "")
                {
                    chr.WriteToDisplay("Shoot what?");
                    return true;
                }

                // weapon in right hand is returning and not a bow
                if (chr.RightHand != null && chr.RightHand.returning && chr.RightHand.skillType != Globals.eSkillType.Bow)
                {
                    return CommandTasker.ParseCommand(chr, "throw", chr.RightHand.name + " at " + args);
                }
                else if (chr.LeftHand != null && chr.LeftHand.returning && chr.LeftHand.skillType != Globals.eSkillType.Bow) // weapon in left hand is returning and not a bow
                {
                    return CommandTasker.ParseCommand(chr, "throw", chr.LeftHand.name + " at " + args);
                }

                // determine what weapon is being shot
                Item weapon = null;

                // bow in right hand is nocked or autofire (returning)
                if (chr.RightHand != null && chr.RightHand.RequiresOneFreeHandToShoot() && (chr.RightHand.IsNocked || chr.RightHand.returning))
                {
                    if (chr.GetFirstFreeHand() != (int)Globals.eWearOrientation.Left)
                    {
                        chr.WriteToDisplay("Your left hand must be empty to shoot " + chr.RightHand.shortDesc + ".");
                        return true;
                    }

                    weapon = chr.RightHand; goto ShootBowOrSling;
                }// else if bow in left hand is nocked (check for returning bow (autofire is done above)
                else if (chr.LeftHand != null && chr.LeftHand.RequiresOneFreeHandToShoot() && (chr.LeftHand.IsNocked || chr.LeftHand.returning))
                {
                    if (chr.GetFirstFreeHand() != (int)Globals.eWearOrientation.Right)
                    {
                        chr.WriteToDisplay("Your right hand must be empty to shoot " + chr.LeftHand.shortDesc + ".");
                        return true;
                    }

                    weapon = chr.LeftHand; goto ShootBowOrSling;
                }

                foreach(Item item in chr.wearing)
                {
                    if(item.baseType == Globals.eItemBaseType.Bow)
                    {
                        weapon = item;
                        break;
                    }
                }

                if (weapon != null && (weapon.baseType == Globals.eItemBaseType.Bow || weapon.baseType == Globals.eItemBaseType.Sling))
                    goto ShootBowOrSling;

                // one hand is empty, check belt for throwable weapon

                if (weapon == null)
                {
                    int firstFreeHand = chr.GetFirstFreeHand();

                    if (firstFreeHand != (int)Globals.eWearOrientation.None)
                    {
                        #region One hand is not empty, check belt for throwable weapon.
                        foreach (Item beltItem in new System.Collections.Generic.List<Item>(chr.beltList))
                        {
                            foreach (string thrownFromBelt in Item.ThrowFromBelt)
                            {
                                if (beltItem.name == thrownFromBelt)
                                {
                                    string notifyMessage = "";

                                    switch (firstFreeHand)
                                    {
                                        case 1:
                                            if (chr.LeftHand != null && (chr.LeftHand.skillType == Globals.eSkillType.Bow || chr.LeftHand.baseType == Globals.eItemBaseType.Sling) && !chr.LeftHand.IsNocked)
                                                notifyMessage = "Your " + chr.LeftHand.name + " is not nocked. ";
                                            break;
                                        case 2:
                                            if (chr.RightHand != null && (chr.RightHand.skillType == Globals.eSkillType.Bow || chr.RightHand.baseType == Globals.eItemBaseType.Sling) && !chr.RightHand.IsNocked)
                                                notifyMessage = "Your " + chr.RightHand.name + " is not nocked. ";
                                            break;
                                        default:
                                            break;
                                    }
                                    chr.WriteToDisplay(notifyMessage + "You throw " + beltItem.shortDesc + " from your belt.");
                                    return CommandTasker.ParseCommand(chr, "throw", beltItem.name + " at " + args);
                                }
                            }
                        }
                        #endregion
                    }
                }

                // last resort, use any item in the left then right hand that is not a shooting weapon such as a bow or sling
                if (chr.LeftHand != null && !chr.LeftHand.RequiresOneFreeHandToShoot())
                {
                    return CommandTasker.ParseCommand(chr, "throw", chr.LeftHand.name + " at " + args);
                }
                else if (chr.RightHand != null && chr.RightHand.RequiresOneFreeHandToShoot())
                {
                    return CommandTasker.ParseCommand(chr, "throw", chr.RightHand.name + " at " + args);                    
                }

                if (weapon == null)
                {
                    chr.WriteToDisplay("You do not have a weapon ready to shoot.");
                    return true;
                }

            ShootBowOrSling:

                if (weapon == null)
                {
                    chr.WriteToDisplay("You do not have a weapon ready to shoot.");
                    return true;
                }

                if (weapon.skillType == Globals.eSkillType.Bow || weapon.baseType == Globals.eItemBaseType.Sling)
                {
                    string nocking = weapon.baseType == Globals.eItemBaseType.Sling ? "loading" : "nocking";
                    string nocked = weapon.baseType == Globals.eItemBaseType.Sling ? "loaded" : "nocked";

                    if (!weapon.returning && chr.CommandsProcessed.Contains(CommandTasker.CommandType.Nock))
                    {
                        chr.WriteToDisplay("You are still " + nocking + " your " + weapon.name + ".");
                        return true;
                    }

                    if (!weapon.IsNocked && !weapon.returning)
                    {
                        chr.WriteToDisplay("The " + weapon.name + " is not " + nocked + ".");
                        return true;
                    }

                    // wrist xbow is a hands item
                    if (chr.GetFirstFreeHand() == (int)Globals.eWearOrientation.None && weapon != null && weapon.wearLocation != Globals.eWearLocation.Hands)
                    {
                        chr.WriteToDisplay("You must have one empty hand to shoot a " + Utils.FormatEnumString(weapon.baseType.ToString()).ToLower() + ".");
                        return true;
                    }
                }

                #region Check if weapon is attuned
                if (chr.IsPC && weapon.IsAttunedToOther(chr))
                {
                    chr.CurrentCell.Add(weapon);
                    chr.WriteToDisplay("The " + weapon.name + " leaps from your hand!");

                    if (weapon == chr.RightHand)
                        chr.UnequipRightHand(weapon);
                    else chr.UnequipLeftHand(weapon);
                    return true;
                }
                #endregion

                #region Check alignment of weapon
                if (!weapon.AlignmentCheck(chr))
                {
                    if (!weapon.name.ToLower().Contains("crossweapon") && !weapon.longDesc.ToLower().Contains("crossweapon"))
                        weapon.IsNocked = false;
                    chr.CurrentCell.Add(weapon);
                    chr.WriteToDisplay("The " + weapon.name + " singes your hand and falls to the ground!");
                    if (weapon == chr.RightHand) chr.UnequipRightHand(weapon);
                    else chr.UnequipLeftHand(weapon);
                    Combat.DoDamage(chr, chr, Rules.RollD(1, 4), false);
                    return true;
                }
                #endregion

                string[] sArgs = args.Split(" ".ToCharArray());

                Character target;

                if (sArgs.Length == 2 && char.IsNumber(sArgs[0].ToCharArray()[0]))
                {
                    target = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, sArgs[1], Convert.ToInt32(sArgs[0]));
                }
                else target = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, sArgs[0], false, chr.IsImmortal);

                if (target == null)
                {
                    chr.WriteToDisplay(GameSystems.Text.TextManager.NullTargetMessage(args));
                    return true;
                }

                chr.CommandType = CommandTasker.CommandType.Shoot;

                Combat.DoCombat(chr, target, weapon);

                // Possible double attack if returning weapon.
                if (weapon != null && weapon.returning)
                    Combat.CheckDoubleAttack(chr, target, weapon);

                if(chr.LeftHand != null && chr.LeftHand.returning)
                    Combat.CheckDualWield(chr, target, chr.LeftHand);

                // Implemented check for "returning" bows (crossbows) which are auto-nock.
                if (weapon != null)
                {
                    if (weapon == chr.RightHand && !chr.RightHand.returning) chr.RightHand.IsNocked = false;
                    else if (weapon == chr.LeftHand && !chr.LeftHand.returning) chr.LeftHand.IsNocked = false;
                    else if (weapon.wearLocation == Globals.eWearLocation.Hands && !weapon.returning)
                        weapon.IsNocked = false;
                    else if (weapon.returning) // auto-nocking sound
                    {
                        if (weapon.name == "crossbow" || weapon.longDesc.ToLower().Contains("crossbow"))
                            chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.NockCrossbow));
                        else if(weapon.name.Contains("bow")) // should there be auto-nocking bows that don't have a mechanism?
                            chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.NockBow));
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return false;
            }

            return true;
        }
    }
}
