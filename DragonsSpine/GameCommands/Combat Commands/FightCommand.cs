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
using Map = DragonsSpine.GameWorld.Map;
using TargetAcquisition = DragonsSpine.GameSystems.Targeting.TargetAquisition;
using EntityLists = DragonsSpine.Autonomy.EntityBuilding.EntityLists;

namespace DragonsSpine.Commands
{
    [CommandAttribute("fight", "Enter combat with an enemy or sparring partner.", (int)Globals.eImpLevel.USER, new string[] { "attack", "kill", "f" }, 2,
        new string[] { "fight <target>" }, Globals.ePlayerState.PLAYING)]
    public class FightCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr is PC && chr.CommandWeight > 3)
                return true;

            if (chr.CommandsProcessed.Contains(CommandTasker.CommandType.Movement))
            {
                if (chr is PC && !chr.HasEffect(Effect.EffectTypes.Speed))
                {
                    chr.WriteToDisplay("You do not have the ability to move and attack in the same round.");
                    return true;
                }
                else if(chr is NPC && (chr as NPC).Speed <= 3 && !chr.HasEffect(Effect.EffectTypes.Speed))
                {
                    chr.WriteToDisplay("You do not have the ability to move and attack in the same round.");
                    return true;
                }
            }

            if (string.IsNullOrEmpty(args))
            {
                chr.WriteToDisplay("Fight what?");
                return true;
            }

            chr.CommandType = CommandTasker.CommandType.Attack;

            Item weapon = chr.RightHand;

            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = null;

            // If hands are empty and gauntlets are worn, gauntlets become the weapon used for calculations.
            if (weapon == null)
                weapon = chr.GetInventoryItem(Globals.eWearLocation.Hands);

            if (sArgs.Length == 2)
            {
                if (int.TryParse(sArgs[0], out int countTo))
                    target = TargetAcquisition.FindTargetInView(chr, sArgs[1].ToLower(), countTo);
                else target = TargetAcquisition.FindTargetInCell(chr, sArgs[0].ToLower());
            }
            else target = TargetAcquisition.FindTargetInCell(chr, sArgs[0].ToLower());

            if (target == null)
            {
                if (EntityLists.EntityListContains(EntityLists.LONGARMED, chr.entity))
                {
                    target = TargetAcquisition.FindTargetInNextCells(chr, sArgs[0].ToLower());
                }

                if (target == null)
                {
                    chr.WriteToDisplay(GameSystems.Text.TextManager.NullTargetMessage(sArgs[0]));
                    return true;
                }
            }

            if (weapon != null)
            {
                if (weapon.skillType == Globals.eSkillType.Bow) // check if a bow is nocked
                {
                    chr.WriteToDisplay("You must nock a bow before shooting it.");
                    return true;
                }

                if (weapon.IsAttunedToOther(chr)) // check if a weapon is attuned
                {
                    chr.CurrentCell.Add(weapon);
                    chr.WriteToDisplay("The " + weapon.name + " leaps from your hand!");
                    chr.UnequipRightHand(weapon);
                    return true;
                }

                if (!weapon.AlignmentCheck(chr)) // check if a weapon has an alignment
                {
                    chr.CurrentCell.Add(weapon);
                    chr.WriteToDisplay("The " + weapon.name + " singes your hand and falls to the ground!");
                    if (weapon.wearLocation == Globals.eWearLocation.Hands)
                    {
                        chr.RemoveWornItem(weapon);
                    }
                    else chr.UnequipRightHand(weapon);
                    Combat.DoDamage(chr, chr, Rules.RollD(1, 4), false);
                    return true;
                }
            }

            // Do combat.
            Combat.DoCombat(chr, target, weapon);

            // Check double attack.
            Combat.CheckDoubleAttack(chr, target, weapon);

            // Hummingbird special attribute is an extra attack.
            Combat.CheckSpecialWeaponAttack(chr, target, weapon);

            // Check dual wield. Double attack is checked again for dual wielded weapon. Dual wielded hummingbird longsword is also checked here.
            Combat.CheckDualWield(chr, target, chr.LeftHand);

            return true;
        }
    }
}
