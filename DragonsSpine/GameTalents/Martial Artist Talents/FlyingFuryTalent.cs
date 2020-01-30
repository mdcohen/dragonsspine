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

namespace DragonsSpine.Talents
{
    [TalentAttribute("flyingfury", "Flying Fury", "Perform a devastating combination jumpkick and fist strike.", false, 10, 121000, 9, 10, true, new string[] { },
        Character.ClassType.Martial_Artist)]
    public class FlyingFuryTalent : ITalentHandler
    {
        public bool MeetsRequirements(Character chr, Character target)
        {
            return true;
        }

        public bool OnPerform(Character chr, string args)
        {
            if (args == null || args == "")
            {
                chr.WriteToDisplay("Who do you want to attack?");
                return false;
            }

            if (Rules.GetEncumbrance(chr) > Globals.eEncumbranceLevel.Lightly)
            {
                chr.WriteToDisplay("You are too encumbered to perform a Flying Fury maneuver.");
                return false;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = GameSystems.Targeting.TargetAquisition.AcquireTarget(chr, args, GameWorld.Cell.DEFAULT_VISIBLE_DISTANCE, 2);

            // safety net
            if (target == null)
            {
                int id = 0;

                if (args.StartsWith("-") || Int32.TryParse(args, out id))
                    chr.WriteToDisplay("You do not see your target nearby.");
                else chr.WriteToDisplay("You do not see a " + args + " nearby.");
                return false;
            }

            if (GameWorld.Cell.GetCellDistance(chr.X, chr.Y, target.X, target.Y) < 1)
            {
                chr.WriteToDisplay("Your target is too close for a Flying Fury maneuver.");
                return false;
            }

            if (target != null && target.CurrentCell != chr.CurrentCell)
            {
                PathTest pathTest = new PathTest(PathTest.RESERVED_NAME_JUMPKICKCOMMAND, chr.CurrentCell);

                if (!pathTest.SuccessfulPathTest(target.CurrentCell))
                {
                    chr.WriteToDisplay(GameSystems.Text.TextManager.PATH_IS_BLOCKED);
                    pathTest.RemoveFromWorld();
                    return false;
                }

                pathTest.RemoveFromWorld();
            }

            // Two jumpkicks then two fist attacks.

            chr.CommandType = CommandTasker.CommandType.Jumpkick;

            Combat.DoJumpKick(chr, target);

            if(target != null && !target.IsDead)
                Combat.DoJumpKick(chr, target);

            chr.CommandsProcessed.RemoveAll((cmdType) => {return (cmdType == CommandTasker.CommandType.Jumpkick);});

            chr.CommandType = CommandTasker.CommandType.Attack;

            if (target != null && !target.IsDead)
                Combat.DoCombat(chr, target, chr.RightHand);
            if (target != null && !target.IsDead)
                Combat.CheckDoubleAttack(chr, target, chr.RightHand);
            if (target != null && !target.IsDead)
                Combat.DoCombat(chr, target, chr.RightHand);
            if (target != null && !target.IsDead)
                Combat.CheckDoubleAttack(chr, target, chr.RightHand);

            if(target != null && target.CurrentCell != null)
                chr.CurrentCell = target.CurrentCell;

            // TODO: emit flying fury sound

            return true;
        }
    }
}
