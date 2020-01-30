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
    [CommandAttribute("kick", "Kick a target using unarmed combat skill.", (int)Globals.eImpLevel.USER, new string[] { "k" }, 2, new string[] { "kick <target>" }, Globals.ePlayerState.PLAYING)]
    public class KickCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.CommandWeight > 3)
            {
                chr.WriteToDisplay("Command weight limit exceeded. Kick command not processed.");
                return true;
            }

            if (args == null)
            {
                chr.WriteToDisplay("Who do you want to kick?");
                return true;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            if (sArgs[0] == null)
            {
                chr.WriteToDisplay("Who do you want to kick?");
                return true;
            }

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
                    target = TargetAcquisition.FindTargetInCell(chr, sArgs[0].ToLower());
                }
            }
            else
            {
                target = TargetAcquisition.FindTargetInCell(chr, sArgs[0].ToLower());
            }

            if (target == null)
            {
                chr.WriteToDisplay(GameSystems.Text.TextManager.NullTargetMessage(sArgs[0]));
            }
            else
            {
                chr.CommandType = CommandTasker.CommandType.Kick;

                chr.Stamina -= 1;

                Globals.eEncumbranceLevel encumbDesc = Rules.GetEncumbrance(chr);
                if (encumbDesc == Globals.eEncumbranceLevel.Moderately)
                    chr.Stamina -= 1;
                else if (encumbDesc == Globals.eEncumbranceLevel.Heavily)
                    chr.Stamina -= 2;
                else if (encumbDesc == Globals.eEncumbranceLevel.Severely)
                    chr.Stamina -= 3;

                if (chr.Stamina < 0)
                {
                    Combat.DoDamage(chr, chr, Math.Abs(chr.Stamina), false);
                    chr.Stamina = 0;
                }

                Combat.DoCombat(chr, target, chr.GetInventoryItem(Globals.eWearLocation.Feet));
            }

            return true;
        }
    }
}
