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
using System.Collections.Generic;
using System.Text;

namespace DragonsSpine.Mail.GameMail
{
    public class GameMailAttachment
    {
        #region Public Properties
        public long MailID {get; set; } // the mail message this attachment belongs to
        public int ItemID {get; set; } // the item ID of the attached item
        public int AttunedID {get; set; }
        public string Special {get; set; }
        public double CoinValue {get; set; }
        public int Charges {get; set; }
        public Globals.eAttuneType AttuneType {get; set; }
        public long FigExp {get; set; }
        public DateTime TimeCreated {get; set; }
        public string WhoCreated {get; set; }
        public double PaymentRequested {get; set; } 
        #endregion

        #region Constructors(2)
        public GameMailAttachment(System.Data.DataRow dr)
        {
            MailID = Convert.ToInt64(dr["mailID"]);
            ItemID = Convert.ToInt32(dr["itemID"]);
            AttunedID = Convert.ToInt32(dr["attunedID"]);
            Special = dr["special"].ToString();
            CoinValue = Convert.ToDouble(dr["coinValue"]);
            Charges = Convert.ToInt32(dr["charges"]);
            AttuneType = (Globals.eAttuneType)Convert.ToInt32(dr["attuneType"]);
            FigExp = Convert.ToInt64(dr["figExp"]);
            TimeCreated = Convert.ToDateTime(dr["timeCreated"]);
            WhoCreated = dr["whoCreated"].ToString();
            PaymentRequested = Convert.ToDouble(dr["paymentRequested"]);
        }

        public GameMailAttachment(Item item, double paymentRequest)
        {
            MailID = -1;
            ItemID = item.itemID;
            AttunedID = item.attunedID;
            Special = item.special;
            CoinValue = item.coinValue;
            Charges = item.charges;
            AttuneType = item.attuneType;
            FigExp = item.figExp;
            TimeCreated = item.timeCreated;
            WhoCreated = item.whoCreated;
            PaymentRequested = paymentRequest;
        }

        public GameMailAttachment(Item coins)
        {
            MailID = -1;
            ItemID = coins.itemID;
            AttunedID = coins.attunedID;
            Special = coins.special;
            CoinValue = coins.coinValue;
            Charges = coins.charges;
            AttuneType = coins.attuneType;
            FigExp = coins.figExp;
            TimeCreated = coins.timeCreated;
            WhoCreated = coins.whoCreated;
            PaymentRequested = 0;
        }
        #endregion

        public Item GetAttachedItem()
        {
            Item item = Item.CopyItemFromDictionary(ItemID);

            // Quick fix to restore scribed scroll information.
            if (item.baseType == Globals.eItemBaseType.Scroll && Special.Contains("longDesc:"))
                item = Autonomy.ItemBuilding.ScrollManager.CreateSpellScroll(Special);
            else item.special = Special;
            
            item.attunedID = AttunedID;
            item.coinValue = CoinValue;
            item.charges = Charges;
            item.attuneType = AttuneType;
            item.figExp = FigExp;
            item.timeCreated = TimeCreated;
            item.whoCreated = WhoCreated;
            return item;
        }
    }
}
