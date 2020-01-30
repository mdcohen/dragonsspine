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
using World = DragonsSpine.GameWorld.World;

namespace DragonsSpine.Commands
{
    [CommandAttribute("deposit",
        "Deposit items into your bank account at specified locations. Accepted items other than coins are converted into coin value after a small fee is deducted.",
        (int)Globals.eImpLevel.USER, new string[] { "dep", "depo" }, 1, new string[] { "deposit <item>" }, Globals.ePlayerState.PLAYING)]
    public class DepositCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.CurrentCell == null || !chr.CurrentCell.IsOrnicLocker)
            {
                chr.WriteToDisplay("You can't do that here.");
                return true;
            }

            int hand = chr.WhichHand("coins");

            if (hand == (int)Globals.eWearOrientation.None) hand = chr.WhichHand("coin");

            if (hand == (int)Globals.eWearOrientation.None) hand = chr.WhichHand("gem");

            Item coinage = null;

            switch (hand)
            {
                case (int)Globals.eWearOrientation.Right:
                    coinage = chr.RightHand;
                    chr.UnequipRightHand(coinage);
                    break;
                case (int)Globals.eWearOrientation.Left:
                    coinage = chr.LeftHand;
                    chr.UnequipLeftHand(coinage);
                    break;
                default:
                    chr.WriteToDisplay("You are not holding anything that can be deposited here.");
                    return true;
            }

            if (coinage != null)
            {
                if (coinage.coinValue <= 0)
                {
                    chr.WriteToDisplay("Invalid coin amount.");
                    return true;
                }

                World.CollectFeeForLottery((coinage.itemType != Globals.eItemType.Coin ? World.FEE_ATM_USE_ITEMS : World.FEE_ATM_USE_COINS), chr.LandID, ref coinage.coinValue);

                (chr as PC).bankGold += coinage.coinValue;

                chr.WriteToDisplay("You now have " + (chr as PC).bankGold + " coin" + (coinage.coinValue > 1 ? "s" : "") + " in your bank account.");

                return true;
            }

            return false;
        }
    }
}
