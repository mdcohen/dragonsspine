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
    [CommandAttribute("impaddstoreitem", "Add a store item to a visible NPC.", (int)Globals.eImpLevel.DEVJR, new string[] { "impaddstore" },
        0, new string[] { "impaddstore <notes+use+plus+signs> <target> <item id> <sell price> <stocked> (if stocked -1 item is always available)" }, Globals.ePlayerState.PLAYING)]
    public class ImpAddStoreItemCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null || args == "")
            {
                chr.WriteToDisplay("Usage: impaddstore <notes+use+plus+signs> <target> <item id> <sell price> <stocked> (if stocked -1 item is always available)");
                return false;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            if (sArgs.Length != 5)
            {
                chr.WriteToDisplay("Incorrect number of arguments. Usage: impaddstore <notes+use+plus+signs> <target> <item id> <sell price> <stocked> (if stocked -1 item is always available)");
                return false;
            }

            Character target = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, sArgs[1], false, true);

            if (target == null)
            {
                chr.WriteToDisplay("Target not found: " + sArgs[1]);
                return false;
            }

            if (!(target is Merchant))
            {
                chr.WriteToDisplay("Target is not a merchant.");
                return false;
            }

            int itemID = -1;

            if (!Int32.TryParse(sArgs[2], out itemID))
            {
                chr.WriteToDisplay("Invalid item ID number: " + sArgs[2]);
                return false;
            }

            Item item = Item.CopyItemFromDictionary(itemID);

            if (item == null)
            {
                chr.WriteToDisplay("Item ID not found in Item Catalog: " + itemID);
                return false;
            }

            double sellPrice = 0;

            if (!Double.TryParse(sArgs[3], out sellPrice))
            {
                chr.WriteToDisplay("Invalid sell price: " + sArgs[3]);
                return false;
            }

            int stocked = 0;

            if (!Int32.TryParse(sArgs[4], out stocked) || stocked < -1)
            {
                chr.WriteToDisplay("Invalid stocked amount: " + sArgs[4]);
                return false;
            }

            StoreItem storeItem = new StoreItem();
            storeItem.notes = sArgs[0].Replace("+", " ");
            storeItem.itemID = item.itemID;
            storeItem.original = true;
            storeItem.sellPrice = sellPrice;
            storeItem.stocked = stocked;
            storeItem.seller = "";
            storeItem.restock = stocked;
            storeItem.charges = item.charges;
            storeItem.figExp = item.figExp;

            if (DAL.DBWorld.InsertStoreItem((target as Merchant).SpawnZoneID, storeItem) != -1)
            {
                chr.WriteToDisplay(item.notes + " inserted into store items for " + target.Name + " for " + sellPrice + ".");
                return true;
            }

            return false;
        }
    }
}
