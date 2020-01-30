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

namespace DragonsSpine.Commands
{
    [CommandAttribute("impdisarm", "Disarms a target. Both left and right hand items drop to the ground.", (int)Globals.eImpLevel.GM, new string[] { },
        0, new string[] { "impdisarm <target in view>" }, Globals.ePlayerState.PLAYING)]
    public class ImpDisarmCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            Character target = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, args, true, true);

            if (target == null)
            {
                chr.WriteToDisplay("You do not see " + args + " here.");
                return true;
            }

            if (target.RightHand != null)
            {
                target.CurrentCell.Add(target.RightHand);
                chr.WriteToDisplay(target.GetNameForActionResult() + " drops " + target.RightHand.shortDesc + ".");
                target.UnequipRightHand(target.RightHand);
            }

            if (target.LeftHand != null)
            {
                target.CurrentCell.Add(target.LeftHand);
                chr.WriteToDisplay(target.GetNameForActionResult() + " drops " + target.LeftHand.shortDesc + ".");
                target.UnequipLeftHand(target.LeftHand);
            }

            return true;
        }
    }
}
