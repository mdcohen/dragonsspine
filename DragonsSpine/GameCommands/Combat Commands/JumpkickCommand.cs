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
using TargetAcquisition = DragonsSpine.GameSystems.Targeting.TargetAquisition;

namespace DragonsSpine.Commands
{
    [CommandAttribute("jumpkick", "Jumpkick a target using unarmed combat skill.", (int)Globals.eImpLevel.USER, new string[] { "jk", "jkick" },
        2, new string[] { "jumpkick <target>" }, Globals.ePlayerState.PLAYING)]
    public class JumpkickCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.CommandWeight > 3)
            {
                chr.WriteToDisplay("Command weight limit exceeded. Jumpkick command not processed.");
                return true;
            }

            if (args == null || args == "")
            {
                chr.WriteToDisplay("Jumpkick who?");
                return true;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            if (sArgs[0] == null)
            {
                chr.WriteToDisplay("Who do you want to jumpkick?");
                return true;
            }

            Character target = null;
            bool includeHidden = chr.IsImmortal;

            if (sArgs.Length == 2)
            {
                int countTo = 0;

                try
                {
                    countTo = Convert.ToInt32(sArgs[0]);
                    target = TargetAcquisition.FindTargetInView(chr, sArgs[1].ToLower(), countTo);
                }
                catch
                {
                    target = TargetAcquisition.FindTargetInView(chr, sArgs[0].ToLower(), false, includeHidden);
                }
            }
            else
            {
                target = TargetAcquisition.FindTargetInView(chr, sArgs[0].ToLower(), false, includeHidden);
            }

            if (target == null)
            {
                chr.WriteToDisplay(GameSystems.Text.TextManager.NullTargetMessage(sArgs[0]));
            }
            else
            {
                if (target.CurrentCell != chr.CurrentCell)
                {
                    PathTest pathTest = new PathTest(PathTest.RESERVED_NAME_JUMPKICKCOMMAND, chr.CurrentCell);

                    if (!pathTest.SuccessfulPathTest(target.CurrentCell))
                    {
                        chr.WriteToDisplay(GameSystems.Text.TextManager.PATH_IS_BLOCKED);
                        pathTest.RemoveFromWorld();
                        return true;
                    }
                    pathTest.RemoveFromWorld();
                }

                chr.CommandType = CommandTasker.CommandType.Jumpkick;

                chr.Stamina -= Cell.GetCellDistance(chr.X, chr.Y, target.X, target.Y);

                Globals.eEncumbranceLevel encumbDesc = Rules.GetEncumbrance(chr);
                if (encumbDesc == Globals.eEncumbranceLevel.Moderately)
                    chr.Stamina -= 2;
                else if (encumbDesc == Globals.eEncumbranceLevel.Heavily)
                    chr.Stamina -= 4;
                else if (encumbDesc == Globals.eEncumbranceLevel.Severely)
                    chr.Stamina -= 6;

                if (chr.Stamina < 0)
                {
                    Combat.DoDamage(chr, chr, Math.Abs(chr.Stamina), false);
                    chr.Stamina = 0;
                }

                chr.CurrentCell = target.CurrentCell;

                chr.updateMap = true;

                Combat.DoJumpKick(chr, target);
            }

            return true;
        }
    }
}
