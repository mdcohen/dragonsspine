namespace DragonsSpine.Talents
{
    [TalentAttribute("legsweep", "Leg Sweep", "Drop low and sweep your legs through a group of targets. A White Sash Martial Artist may move and leg sweep in the same round.",
        false, 11, 1225000, 17, 10, true, new string[] { "legsweep <target>", "legsweep # <target>" }, Character.ClassType.Martial_Artist)]
    public class LegSweepTalent : ITalentHandler
    {
        private const int MOVE_AND_LEGSWEEP_LEVEL = 16; // Skill level 16 = White Sash

        public bool MeetsRequirements(Character chr, Character target)
        {
            return true;
        }

        public bool OnPerform(Character chr, string args)
        {
            if (args == null || args == "")
            {
                chr.WriteToDisplay("Leg Sweep what?");
                return false;
            }

            if (Rules.GetEncumbrance(chr) > Globals.eEncumbranceLevel.Lightly)
            {
                chr.WriteToDisplay("You are too encumbered to perform a Leg Sweep.");
                return false;
            }

            int maxDistance = 0;

            // if first command is movement and bash skill level is less than 12 (Expert) then fail a move and bash attempt
            if (chr.CommandsProcessed.Contains(CommandTasker.CommandType.Movement) && Skills.GetSkillLevel(chr.unarmed) < MOVE_AND_LEGSWEEP_LEVEL)
            {
                chr.WriteToDisplay("You are not skilled enough to move and Leg Sweep.");
                return false;
            }
            else maxDistance = GameWorld.Cell.DEFAULT_VISIBLE_DISTANCE; // can move 3 and leg sweep

            int skillLevel = Skills.GetSkillLevel(chr, Globals.eSkillType.Unarmed);

            if (skillLevel < MOVE_AND_LEGSWEEP_LEVEL)
                maxDistance = 0;

            Character target = GameSystems.Targeting.TargetAquisition.AcquireTarget(chr, args.Split(" ".ToCharArray()), maxDistance, 0);

            // safety net
            if (target == null)
            {
                chr.WriteToDisplay(GameSystems.Text.TextManager.NullTargetMessage(args));
                return false;
            }

            // Check pathing from Character to target.
            PathTest pathTest = new PathTest(PathTest.RESERVED_NAME_JUMPKICKCOMMAND, chr.CurrentCell);

            if (target != null && pathTest.SuccessfulPathTest(target.CurrentCell))
            {
                chr.CommandsProcessed.Add(CommandTasker.CommandType.Leg_Sweep); // Currently this is only done for damage adjustments in Combat.DoDamage.

                // Make sure the Martial Artist has moved to the target's Cell since a pathtest was performed and a check was already done to confirm proper skill level.
                if (chr.CurrentCell != target.CurrentCell)
                    chr.CurrentCell = target.CurrentCell;

                foreach (Character _targ in chr.CurrentCell.Characters.Values)
                {
                    if (_targ == null || _targ.IsDead || _targ == chr) continue;

                    if (_targ.Alignment == target.Alignment || _targ == target || ((_targ is NPC) && (_targ as NPC).MostHated == chr))
                    {
                        skillLevel--;
                        Combat.DoCombat(chr, _targ, chr.GetInventoryItem(Globals.eWearLocation.Feet));
                    }

                    if (skillLevel <= 0) break;
                }
            }
            else
            {
                chr.WriteToDisplay("You cannot reach your target to perform a Leg Sweep.");
                return false;
            }

            return true;
        }
    }
}
