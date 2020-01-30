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
using TargetAcquisition = DragonsSpine.GameSystems.Targeting.TargetAquisition;

namespace DragonsSpine.Commands
{
    [CommandAttribute("chase", "Chase a fleeing target.", (int)Globals.eImpLevel.USER, 2, new string[] { "chase <target>" }, Globals.ePlayerState.PLAYING)]
    public class ChaseCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.CommandWeight > 3)
            {
                chr.WriteToDisplay("Command weight limit exceeded. Chase command not processed.");
                return true;
            }

            if (args == null || args == "")
            {
                chr.WriteToDisplay("Chase who?");
                return true;
            }
            else
            {
                string[] sArgs = args.Split(" ".ToCharArray());
                Character target = null;

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
                        target = TargetAcquisition.FindTargetInView(chr, sArgs[0].ToLower(), false, false);
                    }
                }
                else
                {
                    target = TargetAcquisition.FindTargetInView(chr, sArgs[0].ToLower(), false, false);
                }

                if (target == null)
                {
                    chr.WriteToDisplay(GameSystems.Text.TextManager.NullItemMessage(sArgs[0]));
                    return true;
                }
                else
                {
                    chr.FollowID = target.UniqueID;
                    if (chr.CurrentCell != target.CurrentCell)
                    {
                        if (!PathTest.SuccessfulPathTest(PathTest.RESERVED_NAME_COMMANDSUFFIX, chr.CurrentCell, target.CurrentCell))
                        {
                            if (chr is NPC)
                            {
                                NPC chaser = chr as NPC;
                                chaser.AIGotoXYZ(target.CurrentCell.X, target.CurrentCell.Y, target.CurrentCell.Z);
                            }
                            return false;
                        }
                        else
                        {
                            chr.CurrentCell = target.CurrentCell;
                        }
                    }
                    chr.CommandType = CommandTasker.CommandType.Movement;
                }
            }

            return true;
        }
    }
}
