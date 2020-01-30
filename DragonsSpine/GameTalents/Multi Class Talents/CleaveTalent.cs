using System;
using System.Collections.Generic;

namespace DragonsSpine.Talents
{
    [TalentAttribute("cleave", "Cleave", "Cleave through nearby opponents with a polearm, sword or two-handed weapon. At skill level Astonishing you may move and cleave in the same round.",
        false, 12, 925700, 10, 5, true, new string[] { "cleave <target>", "cleave # <target>" }, Character.ClassType.Berserker, Character.ClassType.Fighter, Character.ClassType.Knight,
        Character.ClassType.Ranger, Character.ClassType.Ravager)]
    public class CleaveTalent : ITalentHandler
    {
        public static double DamageMultiplier = 1.5;
        private const int MOVE_AND_CLEAVE_LEVEL = 13; // Skill level 13 = Astonishing

        private static List<Globals.eItemBaseType> CleavingWeapons = new List<Globals.eItemBaseType>
            { Globals.eItemBaseType.Halberd, Globals.eItemBaseType.Sword, Globals.eItemBaseType.TwoHanded };

        public static bool MeetsRequirements(Character chr, Character target)
        {
            if (Rules.GetEncumbrance(chr) > Globals.eEncumbranceLevel.Moderately)
            {
                chr.WriteToDisplay("You are too encumbered to perform a Cleave.");
                return false;
            }

            if (chr.RightHand == null)
            {
                chr.WriteToDisplay("You are not holding a weapon in your right hand to Cleave with.");
                return false;
            }

            //if (chr.RightHand.TwoHandedPreferred() && chr.LeftHand != null)
            //{
            //    chr.WriteToDisplay("A two handed weapon must be wielded with both hands to perform Cleave.");
            //    return false;
            //}            

            if (!CleavingWeapons.Contains(chr.RightHand.baseType))
            {
                chr.WriteToDisplay("You cannot Cleave with " + chr.RightHand.shortDesc + ".");
                return false;
            }

            return true;
        }

        public bool OnPerform(Character chr, string args)
        {
            if (args == null || args == "")
            {
                chr.WriteToDisplay("Cleave what?");
                return false;
            }

            if (!MeetsRequirements(chr, null))
                return false;

            int maxDistance = 0;

            // if first command is movement and bash skill level is less than 12 (Expert) then fail a move and bash attempt
            if (chr.CommandsProcessed.Contains(CommandTasker.CommandType.Movement) && Skills.GetSkillLevel(chr, chr.RightHand.skillType) < MOVE_AND_CLEAVE_LEVEL)
            {
                chr.WriteToDisplay("You are not skilled enough with " + chr.RightHand.shortDesc + " to move and Cleave.");
                return false;
            }
            else maxDistance = GameWorld.Cell.DEFAULT_VISIBLE_DISTANCE; // can move 3 and cleave

            if (Skills.GetSkillLevel(chr, chr.RightHand.skillType) < MOVE_AND_CLEAVE_LEVEL)
                maxDistance = 0;
            
            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = GameSystems.Targeting.TargetAquisition.AcquireTarget(chr, sArgs, maxDistance, 0);

            if (target == null)
            {
                chr.WriteToDisplay(GameSystems.Text.TextManager.NullTargetMessage(args));
                return false;
            }

            if (PathTest.SuccessfulPathTest(PathTest.RESERVED_NAME_JUMPKICKCOMMAND, chr.CurrentCell, target.CurrentCell))
            {
                chr.CommandsProcessed.Add(CommandTasker.CommandType.Cleave); // Currently this is only done for damage adjustments in Combat.DoDamage.

                // Make sure the Fighter has moved to the target's Cell since a pathtest was performed and a check was already done to confirm proper skill level.
                if (chr.CurrentCell != target.CurrentCell)
                    chr.CurrentCell = target.CurrentCell;

                foreach (Character _targ in chr.CurrentCell.Characters.Values)
                {
                    if (_targ == null || _targ.IsDead) continue;

                    // Could fumble? Make sure right hand is not empty.
                    if (_targ.Alignment == target.Alignment || _targ == target)
                    {
                        if(chr.RightHand != null)
                            // Do combat.
                            Combat.DoCombat(chr, _targ, chr.RightHand);

                        if (_targ == null || _targ.IsDead) continue; // redundancy

                        if(chr.RightHand != null) // possible loss of weapon (fumble)
                            // Check double attack.
                            Combat.CheckDoubleAttack(chr, _targ, chr.RightHand);

                        if (_targ == null || _targ.IsDead) continue; // redundancy

                        if(chr.RightHand != null) // possible loss of weapon (fumble)
                            // Hummingbird special attribute is an extra attack.
                            Combat.CheckSpecialWeaponAttack(chr, _targ, chr.RightHand);

                        if (_targ == null || _targ.IsDead) continue; // redundancy

                        // Make sure left hand weapon is a cleaving weapon.
                        if (chr.LeftHand != null && CleavingWeapons.Contains(chr.LeftHand.baseType))
                            // Check dual wield. Double attack is checked again for dual wielded weapon. Dual wielded hummingbird longsword is also checked here.
                            Combat.CheckDualWield(chr, _targ, chr.LeftHand);
                    }
                }
            }
            else
            {
                chr.WriteToDisplay("You cannot reach your target to perform a Cleave.");
                return false;
            }

            return true;
        }
    }
}
