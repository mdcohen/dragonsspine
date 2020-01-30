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
using System.Collections;
using System.Collections.Specialized;
using System.IO;

namespace DragonsSpine
{
    public class StoreItem
    {
        public int stockID;	// this unique ID is supplied by the database
        public string notes;
        public int itemID;
        public bool original; // store items flagged original will not be deleted if stockedNum <= 0
        public double sellPrice; // this is the price the merchant sells this item for
        public int stocked; // this shows how many of this item is in stock, if -1 then item is always stocked
        public string seller; // who sold the item to the merchant
        public int restock; // the amount to restock stockedNum to if restock call is made

        public int charges;
        public long figExp;

        public StoreItem()
        {
            this.notes = "";
        }

        public StoreItem(System.Data.DataRow dr)
        {
            this.stockID = Convert.ToInt32(dr["stockID"]);
            this.notes = dr["notes"].ToString();
            this.itemID = Convert.ToInt32(dr["itemID"]);
            this.original = Convert.ToBoolean(dr["original"]);
            this.sellPrice = Convert.ToInt32(dr["sellPrice"]);
            this.stocked = Convert.ToInt16(dr["stocked"]);
            this.seller = dr["seller"].ToString();
            this.charges = Convert.ToInt16(dr["charges"]);
            this.figExp = Convert.ToInt64(dr["figExp"]);
        }

        public static void RestockStores()
        {
            int restock = DAL.DBWorld.RestockStoreItems();

            if (restock != -1)
                Utils.Log("Restocked " + restock.ToString() + " store records.", Utils.LogType.SystemGo);
        }

        public static void ClearStores()
        {
            int cleared = DAL.DBWorld.ClearStoreItems();

            if (cleared != -1)
                Utils.Log("Deleted " + cleared.ToString() + " store records.", Utils.LogType.SystemGo);
        }

        public static bool VerifyStoreInsert(Item item, Merchant.MerchantType merchantType) // return true if the merchant will add it to its inventory
        {
            bool insert = false;

            switch (merchantType)
            {
                case Merchant.MerchantType.Pawn:
                    //items with effects
                    if (item.effectType.Length > 0) { insert = true; }
                    //items with a special attribute
                    if (item.special.Length > 0) { insert = true; }
                    //don't add empty bottles
                    if (item.special.Contains("empty")) { insert = false; }
                    //items with a spell
                    if (item.spell != -1) { insert = true; }
                    //items that will attune are usually valuable
                    if (item.attuneType != Globals.eAttuneType.None) { insert = true; }
                    break;
                case Merchant.MerchantType.Barkeep:
                    //items that are drinkable
                    if (item.baseType == Globals.eItemBaseType.Bottle) { insert = true; }
                    //don't add empty bottles
                    if (item.special.IndexOf("empty") != -1) { insert = false; }
                    //items such as lockpicks
                    if (item.baseType == Globals.eItemBaseType.Thievery) { insert = true; }
                    break;
                case Merchant.MerchantType.Weapon:
                    //all weapons
                    if (item.itemType == Globals.eItemType.Weapon) { insert = true; }
                    //all shields
                    if (item.baseType == Globals.eItemBaseType.Shield) { insert = true; }
                    break;
                case Merchant.MerchantType.Armor:
                    //everything that fits into the wearing array
                    if (item.itemType == Globals.eItemType.Wearable) { insert = true; }
                    //all shields
                    if (item.baseType == Globals.eItemBaseType.Shield) { insert = true; }
                    break;
                case Merchant.MerchantType.Apothecary:
                    if (item.baseType == Globals.eItemBaseType.Bottle) { insert = true; }
                    if (item.itemType == Globals.eItemType.Edible) { insert = true; }
                    if (item.itemType == Globals.eItemType.Potable) { insert = true; }
                    //don't add empty bottles
                    if (item.special.IndexOf("empty") != -1) { insert = false; }
                    break;
                case Merchant.MerchantType.Book:
                    if (item.baseType == Globals.eItemBaseType.Book) { insert = true; }
                    break;
                case Merchant.MerchantType.Jewellery:
                    switch (item.baseType)
                    {
                        case Globals.eItemBaseType.Ring:
                        case Globals.eItemBaseType.Amulet:
                        case Globals.eItemBaseType.Bracelet:
                        case Globals.eItemBaseType.Gem:
                        case Globals.eItemBaseType.Figurine:
                            insert = true;
                            break;
                        case Globals.eItemBaseType.Armor:
                            if (item.wearLocation == Globals.eWearLocation.Bicep)
                                insert = true;
                            break;
                        default:
                            insert = false;
                            break;
                    }
                    break;
                case Merchant.MerchantType.General:
                    if (item.baseType == Globals.eItemBaseType.Book) { insert = true; }
                    if (item.baseType == Globals.eItemBaseType.Thievery) { insert = true; }
                    if (item.baseType == Globals.eItemBaseType.Bottle) { insert = true; }
                    //don't add empty bottles
                    if (item.special.IndexOf("empty") != -1) { insert = false; }
                    break;
                case Merchant.MerchantType.Magic:
                    if (item.spell != -1)
                    {
                        insert = true;
                    }
                    if (item.effectType.Length > 0)
                    {
                        insert = true;
                    }
                    break;
            }
            return insert;
        }
    }
}