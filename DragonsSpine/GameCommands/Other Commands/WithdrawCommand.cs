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
    [CommandAttribute("withdraw", "Withdraws coins from your bank account at specified locations.", (int)Globals.eImpLevel.USER, new string[] { "wd", "withd" },
        1, new string[] {"withdraw <amount>"}, Globals.ePlayerState.PLAYING)]
    public class WithdrawCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.CurrentCell == null || !chr.CurrentCell.IsOrnicLocker)
            {
                chr.WriteToDisplay("You can't do that here.");
                return true;
            }

            if (args == null || args == "" || args.StartsWith("-"))
            {
                chr.WriteToDisplay("Invalid command format. Usage: WITHDRAW <amount>");
                return false;
            }

            double amount = 0;

            try
            {
                amount = Convert.ToDouble(args);
            }
            catch
            {
                chr.WriteToDisplay("Invalid command format. Usage: WITHDRAW <amount>");
                return false;
            }

            if (amount < 1)
            {
                chr.WriteToDisplay("Invalid amount to withdraw.");
                return true;
            }

            if ((chr as PC).bankGold < amount)
            {
                chr.WriteToDisplay("You do not have " + amount + " coin" + (amount > 1 ? "s" : "") + " in your bank account.");
                return true;
            }

            Item coinage = Item.CopyItemFromDictionary(Item.ID_COINS);
            coinage.coinValue = amount;
            (chr as PC).bankGold -= amount;

            GameWorld.World.CollectFeeForLottery(GameWorld.World.FEE_ATM_USE_COINS, chr.LandID, ref coinage.coinValue);

            chr.EquipEitherHandOrDrop(coinage);
            chr.WriteToDisplay("You now have " + (chr as PC).bankGold + " coin" + (coinage.coinValue > 1 ? "s" : "") + " in your bank account.");

            return true;
        }
    }
}
