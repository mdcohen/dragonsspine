using System;
using System.Collections.Generic;
using Map = DragonsSpine.GameWorld.Map;
using TargetAcquisition = DragonsSpine.GameSystems.Targeting.TargetAquisition;
using eItemBaseType = DragonsSpine.Globals.eItemBaseType;

namespace DragonsSpine.Talents
{
    /// <summary>
    /// The backstab talent is an activated talent performed by a hidden thief and is not available at generic mentors.
    /// </summary>
    [TalentAttribute("assassinate", "Assassinate", "Coat a piercing weapon with venom, be enchanted with speed, remain hidden AND undetected, to assassinate a humanoid who has no allies in their view.",
        false, 30, 175000, 16, 90, true, new string[] { "assassinate <target>" },
        Character.ClassType.Thief)]
    public class AssassinateTalent : ITalentHandler
    {
        //public static double DamageMultiplier = 4.5;
        public static int IncreasedChanceToHit = 10;

        public static readonly List<eItemBaseType> AllowedItemBaseTypes = new List<eItemBaseType>
        {
            eItemBaseType.Dagger,
            eItemBaseType.Rapier,
            eItemBaseType.Shuriken,
            eItemBaseType.Sword,
            eItemBaseType.Thievery,
            eItemBaseType.Fan,
            eItemBaseType.Whip
        };

        // Thievery skill check to move 3 spaces and backstab.
        private int MoveThreeAndAssassinateSkillRequirement = 12;

        // Thievery skill check always remain hidden after assassinate (until following round checks hidden status).
        private int RemainHiddenAfterAssassinate = 13;

        public static bool MeetsRequirements(Character chr, Character target)
        {
            if (chr.RightHand == null && chr.LeftHand == null)
            {
                chr.WriteToDisplay("You are not holding a weapon with which to carry out an assassination.");
                return false;
            }

            if (Rules.GetEncumbrance(chr) > Globals.eEncumbranceLevel.Moderately)
            {
                chr.WriteToDisplay("You are too encumbered to perform an assassination.");
                return false;
            }

            Item weapon = null;

            if (chr.RightHand != null && chr.RightHand.itemType == Globals.eItemType.Weapon)
                weapon = chr.RightHand;
            else if (chr.LeftHand != null && chr.LeftHand.itemType == Globals.eItemType.Weapon)
                weapon = chr.LeftHand;

            if (weapon == null)
            {
                chr.WriteToDisplay("You are not holding a weapon with which to carry out an assassination.");
                return false;
            }

            if (AllowedItemBaseTypes.Contains(weapon.baseType) || !weapon.special.ToLower().Contains("pierce"))
            {
                chr.WriteToDisplay("You are not holding a piercing weapon to carry out a proper assassination.");
                return false;
            }

            if (weapon.venom <= 0)
            {
                chr.WriteToDisplay("Your blade must be coated in venom to carry out a proper assassination.");
                return false;
            }

            return true;
        }

        public bool OnPerform(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Assasinate who?");
                return false;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = TargetAcquisition.AcquireTarget(chr, args, GameWorld.Cell.DEFAULT_VISIBLE_DISTANCE, 0);

            // failed to find the target
            if (target == null)
            {
                chr.WriteToDisplay("You don't see a " + (sArgs.Length >= 2 ? sArgs[0] + " " + sArgs[1] : sArgs[0]) + " here.");
                return false;
            }

            if (target.CurrentCell != chr.CurrentCell)
            {
                if (!PathTest.SuccessfulPathTest(PathTest.RESERVED_NAME_JUMPKICKCOMMAND, chr.CurrentCell, target.CurrentCell))
                    return false;
            }

            if (target.IsPC)
            {
                chr.WriteToDisplay("Fellow players may not be assassinated, yet.");
                return false;
            }

            if (target is NPC && (target as NPC).lairCritter)
            {
                chr.WriteToDisplay("Your target has a lair and is on guard for assassination attempts.");
                return false;
            }

            if (target.seenList.Contains(chr) && (target is PC || ((target is NPC) && (target as NPC).enemyList.Contains(chr))))
            {
                chr.WriteToDisplay("Your intended target is aware of your presence.");
                return false;
            }

            if (target is NPC && (target as NPC).friendList.Count > 0)
            {
                if ((target as NPC).friendList.Count == 1 && (target as NPC).friendList.Contains(chr))
                {
                    // continue, thief was visible briefly and appeared as a friend
                }
                else
                {
                    chr.WriteToDisplay("Your target has allies nearby. An assassination target must be alone.");
                    return false;
                }
            }

            if (target is NPC && (target as NPC).enemyList.Count > 0)
            {
                chr.WriteToDisplay("Your target has visible enemies nearby. An assassination target must be alone.");
                return false;
            }

            // Only humans, humanoids and giantkin may be assassinated.
            if (!Autonomy.EntityBuilding.EntityLists.IsHumanOrHumanoid(target)
                && !Autonomy.EntityBuilding.EntityLists.IsGiantKin(target))
            {
                chr.WriteToDisplay("Only humans, humanoids and giantkin may be the target of an assassination.");
                return false;
            }

            // Unique entities cannot be assassinated.
            if (Autonomy.EntityBuilding.EntityLists.UNIQUE.Contains(target.entity))
            {
                chr.WriteToDisplay("Unique entities cannot be assassinated. They have a level of perception that keeps them aware of such attempts.");
                return false;
            }

            // Assassination requires Speed.
            if (!chr.HasSpeed)
            {
                chr.WriteToDisplay("You must be enchanted with Speed to carry out an assassination.");
                return false;
            }

            int distance = DragonsSpine.GameWorld.Cell.GetCellDistance(chr.X, chr.Y, target.X, target.Y);
            int thieverySkillLevel = Skills.GetSkillLevel(chr.thievery);

            // check minimum thievery skill requirement for allowed distance
            if (distance > 2 && (thieverySkillLevel < MoveThreeAndAssassinateSkillRequirement))
            {
                chr.WriteToDisplay("Your intended target is too far away. Improve your thievery skill.");
                return false;
            }

            // verify path is not blocked
            if (distance > 0)
            {
                if (!PathTest.SuccessfulPathTest(PathTest.RESERVED_NAME_COMMANDSUFFIX, chr.CurrentCell, target.CurrentCell))
                {
                    chr.WriteToDisplay(GameSystems.Text.TextManager.PATH_IS_BLOCKED);
                    return false;
                }
            }

            Item weapon = null;

            if (chr.RightHand != null && chr.RightHand.itemType == Globals.eItemType.Weapon)
                weapon = chr.RightHand;
            else if (chr.LeftHand != null && chr.LeftHand.itemType == Globals.eItemType.Weapon)
                weapon = chr.LeftHand;

            if (weapon == null) return false;

            chr.CommandType = CommandTasker.CommandType.Assassinate;
            chr.CurrentCell = target.CurrentCell;
            chr.updateMap = true;
            Combat.DoCombat(chr, target, weapon);

            chr.CommandsProcessed.RemoveAll(cmdType => cmdType == CommandTasker.CommandType.Assassinate);

            // give skill experience
            Skills.GiveSkillExp(chr, Skills.GetSkillLevel(chr.thievery) * 75, Globals.eSkillType.Thievery);

            if (!chr.HasSpeed || !Map.IsNextToWall(chr) || !Rules.FullStatCheck(chr, Globals.eAbilityStat.Dexterity))
            {
                chr.IsHidden = false;
            }
            else if (thieverySkillLevel < RemainHiddenAfterAssassinate && !Rules.FullStatCheck(chr, Globals.eAbilityStat.Dexterity))
            {
                Map.CheckHiddenStatus(chr);
            }

            if (chr.IsHidden && !chr.EffectsList[Effect.EffectTypes.Hide_in_Shadows].IsPermanent)
                chr.WriteToDisplay(GameSystems.Text.TextManager.REMAINED_HIDDEN);

            return true;
        }
    }
}
