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
    [CommandAttribute("swap", "Swap the items you are holding.", (int)Globals.eImpLevel.USER, 1, new string[] { "There are no arguments for the swap command." }, Globals.ePlayerState.PLAYING)]
    public class SwapCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            //Item tmpItemRight = new Item();
            //Item tmpItemLeft = new Item();

            //tmpItemRight = null;
            //tmpItemLeft = null;

            //// Pull stuff out of hands
            //tmpItemRight = chr.RightHand;
            //tmpItemLeft = chr.LeftHand;

            //// Set it to null
            //chr.RightHand = null;
            //chr.LeftHand = null;

            //// put stuff back in hands, reversing the hand it goes into
            //chr.RightHand = tmpItemLeft;
            //if (chr.RightHand != null && chr.RightHand.nocked && chr.RightHand.name != "crossbow") // un-nock opposite hand if not crossbow
            //{
            //    chr.RightHand.nocked = false;
            //}
            //chr.LeftHand = tmpItemRight;
            //if (chr.LeftHand != null && chr.LeftHand.nocked && chr.LeftHand.name != "crossbow") // un-nock opposite hand if not crossbow
            //{
            //    chr.LeftHand.nocked = false;
            //} 

            Item tmpItemRight = chr.RightHand;
            Item tmpItemLeft = chr.LeftHand;

            //tmpItemRight = null;
            //tmpItemLeft = null;

            // Pull stuff out of hands
            //tmpItemRight = chr.RightHand;
            //tmpItemLeft = chr.LeftHand;

            // Set it to null
            if (chr.RightHand != null) chr.UnequipRightHand(chr.RightHand);
            if (chr.LeftHand != null) chr.UnequipLeftHand(chr.LeftHand);

            // put stuff back in hands, reversing the hand it goes into
            if (tmpItemLeft != null) chr.EquipRightHand(tmpItemLeft);

            if (chr.RightHand != null && chr.RightHand.IsNocked && chr.RightHand.name != "crossbow") // un-nock opposite hand if not crossbow
            {
                chr.RightHand.IsNocked = false;
            }

            if (tmpItemRight != null) chr.EquipLeftHand(tmpItemRight);

            if (chr.LeftHand != null && chr.LeftHand.IsNocked && chr.LeftHand.name != "crossbow") // un-nock opposite hand if not crossbow
            {
                chr.LeftHand.IsNocked = false;
            } 

            return true;
        }
    }
}
