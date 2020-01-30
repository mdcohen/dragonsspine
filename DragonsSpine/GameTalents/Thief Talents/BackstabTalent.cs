using Map = DragonsSpine.GameWorld.Map;
using TargetAcquisition = DragonsSpine.GameSystems.Targeting.TargetAquisition;
using eItemBaseType = DragonsSpine.Globals.eItemBaseType;

namespace DragonsSpine.Talents
{
    /// <summary>
    /// The backstab talent is an activated talent performed by a hidden thief and is not available at generic mentors.
    /// </summary>
    [TalentAttribute("backstab", "Backstab", "Launch a precise attack, while hidden and undetected, at a target.", false, 20, 20000, 11, 5, true, new string[] { "backstab <target>" },
        Character.ClassType.Thief)]
    public class BackstabTalent : ITalentHandler
    {
        public static System.Collections.Generic.List<eItemBaseType> AllowedBackstabItemBaseTypes = new System.Collections.Generic.List<eItemBaseType>()
        {
            eItemBaseType.Dagger,
            eItemBaseType.Rapier,
            eItemBaseType.Sword,
            eItemBaseType.Fan,
        };

        public static double DamageMultiplier = 4.5;
        public static int IncreasedChanceToHit = 5;

        // Thievery skill check to move 3 spaces and backstab.
        private readonly int MoveThreeAndBackstabSkillRequirement = 10;

        // Thievery skill check always remain hidden after backstab (until following round checks hidden status).
        private readonly int RemainHiddenAfterBackstabWithSpeed = 13;

        public static bool MeetsRequirements(Character chr, Character target)
        {
            return true;
        }

        public bool OnPerform(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Backstab who?");
                return false;
            }

            if(chr.RightHand == null && chr.LeftHand == null)
            {
                chr.WriteToDisplay("You are not holding a weapon to backstab with.");
                return false;
            }

            if (Rules.GetEncumbrance(chr) > Globals.eEncumbranceLevel.Moderately)
            {
                chr.WriteToDisplay("You are too encumbered to backstab.");
                return false;
            }

            Item weapon = null;

            if(chr.RightHand != null && chr.RightHand.itemType == Globals.eItemType.Weapon)
                weapon = chr.RightHand;
            else if(chr.LeftHand != null && chr.LeftHand.itemType == Globals.eItemType.Weapon)
                weapon = chr.LeftHand;

            if(weapon == null)
            {
                chr.WriteToDisplay("You are not holding a weapon you can backstab with.");
                return false;
            }

            if(!AllowedBackstabItemBaseTypes.Contains(weapon.baseType))
            {
                chr.WriteToDisplay("You are not holding a weapon you can backstab with.");
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

            if(target.seenList.Contains(chr))
            {
                chr.WriteToDisplay("Your intended target is aware of your presence.");
                return false;
            }

            if(!chr.HasSpeed)
            {
                chr.WriteToDisplay("You must be enchanted with Speed to perform a backstab.");
                return false;
            }

            int distance = DragonsSpine.GameWorld.Cell.GetCellDistance(chr.X, chr.Y, target.X, target.Y);
            int thieverySkillLevel = Skills.GetSkillLevel(chr.thievery);

            // check minimum thievery skill requirement
            if (distance > 2 && (thieverySkillLevel < MoveThreeAndBackstabSkillRequirement))
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

            chr.CommandType = CommandTasker.CommandType.Backstab;
            chr.CurrentCell = target.CurrentCell;
            chr.updateMap = true;
            Combat.DoCombat(chr, target, weapon);

            chr.CommandsProcessed.RemoveAll(cmdType => cmdType == CommandTasker.CommandType.Backstab);

            // give skill experience
            Skills.GiveSkillExp(chr, Skills.GetSkillLevel(chr.thievery) * 50, Globals.eSkillType.Thievery);

            if (thieverySkillLevel < RemainHiddenAfterBackstabWithSpeed && !Rules.FullStatCheck(chr, Globals.eAbilityStat.Dexterity))
                chr.IsHidden = false;
            else if (!chr.HasSpeed || !Map.IsNextToWall(chr) || !Rules.FullStatCheck(chr, Globals.eAbilityStat.Dexterity))
                chr.IsHidden = false;
            else
                Map.CheckHiddenStatus(chr);

            if (chr.IsHidden && !chr.EffectsList[Effect.EffectTypes.Hide_in_Shadows].IsPermanent)
                chr.WriteToDisplay(GameSystems.Text.TextManager.REMAINED_HIDDEN);

            return true;
        }
    }
}
