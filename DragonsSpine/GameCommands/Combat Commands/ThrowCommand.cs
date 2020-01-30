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
using System.Collections;
using DragonsSpine.GameWorld;
using GameSpell = DragonsSpine.Spells.GameSpell;

namespace DragonsSpine.Commands
{
    [CommandAttribute("throw", "Throw an item in a direction or at a target.", (int)Globals.eImpLevel.USER, new string[] { "thr" },
        1, new string[] { "throw <item> <direction>", "throw <item> at <target>" }, Globals.ePlayerState.PLAYING)]
    public class ThrowCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            int num = 0;

            #region Handle bad arguments or heavy command weight

            // TODO: add support for "throw # item at <target>" and "throw # item at # target"
            if (args == null || args == "" || !args.Contains(" "))
            {
                chr.WriteToDisplay(
                    "Usage of throw: throw <item> <target/direction> | throw <left/right> <target/direction> | throw <item> at # <target>");
                return true;
            }

            if (chr.CommandWeight > 3)
            {
                chr.WriteToDisplay("Command weight limit exceeded. Throw command not processed.");
                return true;
            }

            #endregion

            var sArgs = args.Split(" ".ToCharArray());

            var rightHand = false;

            Item item = null;

            //var thrownAtTarget = false;

            if (args.Contains(" at "))
            {
                args = args.Replace(" at ", " ");
                sArgs = args.Split(" ".ToCharArray());
                //thrownAtTarget = true;
            }
            else
            {
                goto ThrowDirection;
            }

            // left hand match
            if (chr.LeftHand != null &&
                (sArgs[0].ToLower() == "left" || chr.LeftHand.name.ToLower().StartsWith(sArgs[0].ToLower())))
                item = chr.LeftHand;

                // right hand match
            else if (chr.RightHand != null &&
                     (sArgs[0].ToLower() == "right" || chr.RightHand.name.ToLower().StartsWith(sArgs[0].ToLower())))
            {
                item = chr.RightHand;
                rightHand = true;
            }

            #region Find throwable item on belt

            // one hand is empty, check belt for items and use FIRST weapon that may be thrown from belt
            if (item == null && chr.GetFirstFreeHand() != (int)Globals.eWearOrientation.None)
            {
                foreach (string throwFromBelt in Item.ThrowFromBelt)
                {
                    if (throwFromBelt.ToLower().StartsWith(sArgs[0].ToLower()))
                    {
                        item = chr.RemoveFromBelt(throwFromBelt);

                        if (item != null)
                        {
                            switch (chr.GetFirstFreeHand())
                            {
                                case (int)Globals.eWearOrientation.Right:
                                    chr.EquipRightHand(item);
                                    rightHand = true;
                                    break;
                                case (int)Globals.eWearOrientation.Left:
                                    chr.EquipLeftHand(item);
                                    rightHand = false;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        }
                    }
                }
            }

            #endregion

            if (item == null)
            {
                chr.WriteToDisplay("You do not have a " + sArgs[0] + " to throw.");
                return true;
            }

            #region Thrown item is attuned

            if (chr.IsPC && item.IsAttunedToOther(chr))
            {
                chr.CurrentCell.Add(item);
                chr.WriteToDisplay("The " + item.name + " leaps from your hand!");
                if (rightHand)
                {
                    chr.UnequipRightHand(item);
                }
                else chr.UnequipLeftHand(item);

                return true;
            }

            #endregion

            #region Thrown item is aligned

            if (!item.AlignmentCheck(chr))
            {
                chr.CurrentCell.Add(item);
                chr.WriteToDisplay("The " + item.name + " singes your hand and falls to the ground!");
                if (rightHand)
                {
                    chr.RightHand = null;
                    Combat.DoDamage(chr, chr, Rules.Dice.Next(1, 4), false);
                }
                else
                {
                    chr.LeftHand = null;
                    Combat.DoDamage(chr, chr, Rules.Dice.Next(1, 4), false);
                }
                return true;
            }

            #endregion

            Character target = null;

            #region Throw an item at a target

            // throw <item> <target>, throw <item> # <target>, throw <item> <direction>

            // throw <item> at # <target>
            if (sArgs.Length == 3 && char.IsNumber(sArgs[1].ToCharArray()[0]))
            {
                num = Convert.ToInt32(sArgs[1]);
                target = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, sArgs[2].ToLower(), num);
            }
            else
            {
                target = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, sArgs[1].ToLower(), false, chr.IsImmortal);
            }

            Cell targetCell = null;
            var isFigurine = false;

            if (target != null)
            {
                chr.CommandType = CommandTasker.CommandType.Throw;

                #region Thrown item is a weapon

                if (item.itemType == Globals.eItemType.Weapon)
                {
                    targetCell = target.CurrentCell;
                    chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.ThrownWeapon));
                    Combat.DoCombat(chr, target, item);

                    // Possible double attack if returning weapon.
                    if (item != null && item.returning && target != null && !target.IsDead)
                        Combat.CheckDoubleAttack(chr, target, item);

                    if (chr.LeftHand != null && chr.LeftHand.returning && chr.LeftHand.itemType == Globals.eItemType.Weapon && target != null && !target.IsDead)
                        Combat.CheckDualWield(chr, target, chr.LeftHand);

                    /* The code below was added by mlt in order to give Carfel extra attacks.
                        A better way to do this is to add a numAttacks or similar attribute to the Character class. */

                    #region Carfel hard code

                    //if (!chr.IsPC && chr.Name == "Carfel" && target != null && !target.IsDead)
                    //{
                    //    Combat.DoCombat(chr, target, item);
                    //    if (target != null && !target.IsDead)
                    //    {
                    //        Combat.DoCombat(chr, target, item);
                    //    }
                    //}

                    #endregion

                    #region Weapon with "figurine" in special - eg: Ebonwood Staff (snake staff)

                    if (item.special.Contains("figurine"))
                    {
                        if (item == chr.RightHand)
                            chr.UnequipRightHand(item);
                        else if (item == chr.LeftHand)
                            chr.UnequipLeftHand(item);

                        Rules.SpawnFigurine(item, targetCell, chr);
                    }

                    #endregion

                }
                    #endregion

                #region else if Figurine thrown at a target

                else if (item.special.ToLower().Contains("figurine") || item.baseType == Globals.eItemBaseType.Figurine || item.itemID == Item.ID_EBONSNAKESTAFF)
                    // if thrown item is figurine
                {
                    isFigurine = true;

                    // breakable chance if figurine is fragile
                    if (item.fragile)
                    {
                        // 2d100 roll for fig break
                        var figBreakRoll = Rules.RollD(2, 100);

                        if (chr.IsLucky && Rules.RollD(1, 100) >= 10)
                            figBreakRoll++;

                        // two 1's are rolled, character is not lucky
                        if (figBreakRoll == 2)
                        {
                            target.SendShout("something shatter into a hundred pieces.");
                            target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.BreakingGlass));
                            //TODO implement NPCs throwing figurines
                            Utils.Log(chr.GetLogString() + " broke a figurine. Item ID: " + item.GetLogString() + " FigExp: " +
                                    item.figExp.ToString(), Utils.LogType.ItemFigurineUse);
                        }
                        else
                        {
                            Rules.SpawnFigurine(item, target.CurrentCell, chr);
                        }
                    }
                    else
                    {
                        Rules.SpawnFigurine(item, target.CurrentCell, chr);
                    }
                }
                #endregion

                #region else if Bottle thrown at a target

                else if (item.baseType == Globals.eItemBaseType.Bottle || item is SoulGem)
                    // what to do if the thrown item is a bottle
                {
                    if (Combat.CheckFumble(chr, item))
                    {
                        target = chr;
                        chr.SendToAllInSight(chr.Name + " fumbles!");
                        chr.WriteToDisplay("You fumble!");
                    }

                    if (item.effectType.Length > 0)
                    {
                        var effectTypes = item.effectType.Split(" ".ToCharArray());
                        var effectAmounts = item.effectAmount.Split(" ".ToCharArray());
                        var effectDurations = item.effectDuration.Split(" ".ToCharArray());

                        for (int a = 0; a < effectTypes.Length; a++)
                        {
                            var effectType = (Effect.EffectTypes) Convert.ToInt32(effectTypes[a]);

                            if (effectType == Effect.EffectTypes.Nitro)
                            {
                                GameSpell.CastGenericAreaSpell(target.CurrentCell, "", Effect.EffectTypes.Nitro,
                                                               Convert.ToInt32(effectAmounts[a]), "");
                            }
                            else if (effectType == Effect.EffectTypes.Naphtha)
                            {
                                var cells = new ArrayList
                                {
                                    target.CurrentCell
                                };
                                var effect = new AreaEffect(Effect.EffectTypes.Fire, Cell.GRAPHIC_FIRE,
                                                            Convert.ToInt32(effectAmounts[a]),
                                                            Convert.ToInt32(effectDurations[a]), chr, cells);
                            }
                        }
                    }

                    if (item.baseType == Globals.eItemBaseType.Bottle)
                    {
                        target.SendShout("the sound of glass shattering.");
                    }
                    else target.SendShout("the sound of something shattering.");

                    target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.BreakingGlass));

                    if (item == chr.LeftHand)
                        chr.UnequipLeftHand(item);
                    if (item == chr.RightHand)
                        chr.UnequipRightHand(item);

                }
                #endregion

                else
                {
                    #region all other thrown objects

                    targetCell = target.CurrentCell;

                    if (Rules.RollD(1, 2) == 1)
                        chr.WriteToDisplay("Your " + item.name + " bounces harmlessly off of " +
                                           target.GetNameForActionResult(true) + ".");
                    else chr.WriteToDisplay("You miss!");

                    target.WriteToDisplay(chr.GetNameForActionResult() + " misses you!");

                    if (targetCell != null && !targetCell.Items.Contains(item))
                    {
                        if (!item.special.Contains("figurine"))
                        {
                            targetCell.Add(item);

                            if (chr.RightHand == item) chr.UnequipRightHand(item);
                            else if (chr.LeftHand == item) chr.UnequipLeftHand(item);
                        }
                    }

                    #endregion
                }
            }
            else
            {
                if (num > 0)
                {
                    chr.WriteToDisplay(GameSystems.Text.TextManager.NullTargetMessage(sArgs[1] + " " + sArgs[2]));
                }
                else
                {
                    chr.WriteToDisplay(GameSystems.Text.TextManager.NullTargetMessage(sArgs[1]));
                }
                return true;
            }

            // ** item may have been removed from hand if fumbled result in Rules.doCombat
            // if the attack caused fatal damage the item is removed in Combat.DND_Attack 
            if (rightHand && chr.RightHand != null && !item.returning && targetCell != null &&
                !targetCell.Items.Contains(item))
            {
                if (!item.fragile && !isFigurine)
                    targetCell.Add(item);

                chr.UnequipRightHand(item);
            }
            else if (!rightHand && chr.LeftHand != null && !item.returning && targetCell != null &&
                     !targetCell.Items.Contains(item))
            {
                if (!item.fragile && !isFigurine)
                    targetCell.Add(item);

                chr.UnequipLeftHand(item);
            }
            return true;

            #endregion

            ThrowDirection:

            // left hand match
            if (chr.LeftHand != null &&
                (sArgs[0].ToLower() == "left" || chr.LeftHand.name.ToLower().StartsWith(sArgs[0].ToLower())))
                item = chr.LeftHand;

                // right hand match
            else if (chr.RightHand != null &&
                     (sArgs[0].ToLower() == "right" || chr.RightHand.name.ToLower().StartsWith(sArgs[0].ToLower())))
            {
                item = chr.RightHand;
                rightHand = true;
            }

            if (item == null)
            {
                chr.WriteToDisplay("You do not have a " + sArgs[0] + " to throw.");
                return true;
            }

            #region Throw an item in a direction

            // not throwing an item AT a target
            args = "";

            for (int a = 1; a < sArgs.Length; a++)
            {
                args += sArgs[a] + " ";
            }

            Cell cell = Map.GetCellRelevantToCell(chr.CurrentCell, args.Substring(0, args.Length - 1), true);

            if (cell != chr.CurrentCell)
            {
                var pathTest = new PathTest(PathTest.RESERVED_NAME_THROWNOBJECT, chr.CurrentCell);
                if (!pathTest.SuccessfulPathTest(cell))
                    cell = chr.CurrentCell;
                pathTest.RemoveFromWorld();
            }

            int totalHeight = 0; // used to record how fall an object falls

            if (cell != null && cell.DisplayGraphic == Cell.GRAPHIC_AIR)
            {
                Segue segue = null;
                int countLoop = 0;
                do
                {
                    segue =
                        Segue.GetDownSegue(Cell.GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X, cell.Y, cell.Z));

                    countLoop++;

                    if (segue != null)
                    {
                        cell = Cell.GetCell(cell.FacetID, segue.LandID, segue.MapID, segue.X, segue.Y, segue.Z);
                        totalHeight += segue.Height;
                    }
                    else
                    {
                        break;
                    }
                } while (cell.CellGraphic == Cell.GRAPHIC_AIR && countLoop < 100);
            }

            if (item.effectType.Length > 0)
            {
                #region Thrown item causes an effect

                string[] effectTypes = item.effectType.Split(" ".ToCharArray());
                string[] effectAmounts = item.effectAmount.Split(" ".ToCharArray());
                string[] effectDurations = item.effectDuration.Split(" ".ToCharArray());

                for (int a = 0; a < effectTypes.Length; a++)
                {
                    Effect.EffectTypes effectType = (Effect.EffectTypes) Convert.ToInt32(effectTypes[a]);
                    if (effectType == Effect.EffectTypes.Nitro)
                    {
                        GameSpell.CastGenericAreaSpell(cell, "", Effect.EffectTypes.Nitro,
                                                       Convert.ToInt32(effectAmounts[a]), "concussion", chr);
                    }
                    else if (effectType == Effect.EffectTypes.Naphtha)
                    {
                        ArrayList cells = new ArrayList
                        {
                            cell
                        };
                        AreaEffect effect = new AreaEffect(Effect.EffectTypes.Fire, Cell.GRAPHIC_FIRE,
                                                           Convert.ToInt32(effectAmounts[a]),
                                                           Convert.ToInt32(effectDurations[a]), chr, cells);
                    }
                }

                if (item.baseType == Globals.eItemBaseType.Bottle)
                {
                    if (cell.DisplayGraphic != Cell.GRAPHIC_WATER)
                    {
                        cell.SendShout("the sound of glass shattering.");
                        cell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.BreakingGlass));
                    }
                    else cell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.Splash));
                }
                else if (item.fragile)
                {
                    int breakRoll = Rules.RollD(1, 100);

                    // add experience level of figurine to roll
                    if (item.baseType == Globals.eItemBaseType.Figurine)
                    {
                        breakRoll += Rules.GetExpLevel(item.figExp);

                        // less chance for a lucky character to break a figurine
                        if (chr.IsLucky && Rules.RollD(1, 100) >= 10)
                            breakRoll += 20;
                    }

                    if (breakRoll > 25)
                        cell.Add(item);
                    else
                    {
                        cell.SendShout("something shatter into pieces.");
                        cell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.BreakingGlass));
                        if (item.baseType == Globals.eItemBaseType.Figurine)
                        {
                            Utils.Log(chr.GetLogString() + " broke a figurine. Item ID: " + item.GetLogString() + " FigExp: " +
                                    item.figExp.ToString(), Utils.LogType.ItemFigurineUse);
                        }
                    }
                }
                else
                {
                    cell.Add(item);
                }

                #endregion
            }
            else
            {
                #region if figurine or snake staff thrown direction

                if (item.baseType == Globals.eItemBaseType.Figurine || item.special.ToLower().IndexOf("snake") > -1)
                    // if thrown item is figurine
                {
                    // breakable chance if figurine is fragile
                    if (item.fragile)
                    {
                        // 2d100 roll for fig break
                        int figBreakRoll = Rules.RollD(2, 100);

                        // still a small chance for the figurine to break
                        if (chr.IsLucky && Rules.RollD(1, 100) >= 10)
                            figBreakRoll++;

                        // every 20 feet of height dropped increases the chance of a break
                        if (totalHeight > 0)
                        {
                            figBreakRoll -= Convert.ToInt32(totalHeight/20);
                        }

                        if (figBreakRoll <= 2)
                        {
                            cell.SendShout("something shatter into a hundred pieces.");
                            cell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.BreakingGlass));
                            Utils.Log(chr.GetLogString() + " broke a figurine. Item ID: " + item.GetLogString() + " FigExp: " +
                                    item.figExp.ToString(), Utils.LogType.ItemFigurineUse);
                        }
                        else
                        {
                            Rules.SpawnFigurine(item, cell, chr);
                        }
                    }
                    else
                    {
                        Rules.SpawnFigurine(item, cell, chr);
                    }
                }
                    #endregion

                    #region else if fragile

                else if (item.fragile)
                {
                    int breakRoll = Rules.RollD(1, 100);

                    // increase chance to break with every 20 feet dropped
                    if (totalHeight > 0)
                        breakRoll -= Convert.ToInt32(totalHeight/20);

                    if (item is SoulGem)
                        breakRoll -= 100;

                    if (cell != null)
                    {
                        // 25% chance to break
                        if (breakRoll > 25)
                            cell.Add(item);
                        else
                        {
                            cell.SendShout("something shatter into pieces.");
                            cell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.BreakingGlass));
                        }
                    }
                }
                    #endregion

                else if (cell != null) cell.Add(item);
            }

            if (rightHand)
                chr.UnequipRightHand(item);
            else
                chr.UnequipLeftHand(item);

            #endregion

            return true;
        }
    }
}
