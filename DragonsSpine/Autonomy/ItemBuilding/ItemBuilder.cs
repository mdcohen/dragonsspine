using System;
using System.Collections.Generic;

namespace DragonsSpine.Autonomy.ItemBuilding
{
    public class ItemBuilder
    {
        public static List<string> BOOK_SYNONYMS = new List<string>()
        {
            "book", "manual", "spellbook", "tome", "volume"
        };

        public static List<string> BOTTLE_SYNONYMS = new List<string>()
        {
            "balm", "bottle", "flask", "phial", "vial", "ampule", "ampoule"
        };

        /// <summary>
        /// Build a new item that will be inserted into the database.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="shortDesc"></param>
        /// <returns>True if the item was built.</returns>
        public bool BuildItem(string name, string shortDesc)
        {
            Item item = new Item();

            return true;
        }

        /// <summary>
        /// Set default values for an item for proper insertion into the database.
        /// </summary>
        /// <param name="item"></param>
        public void SetDefaultValues(Item item)
        {
        }

        public void OneTimeBuildAndInsert(Globals.eItemType itemType, Globals.eItemBaseType itemBaseType, Globals.eWearLocation wearLocation)
        {
            var selectedRows = new List<System.Data.DataRow>();

            // for armor, leather tunic -- modify AC, name, shortDesc, longDesc, flammable?, "special" and other special attributes
            // grab the default item from the database, modify it then insert if it does not already exist
            foreach (int itemID in Item.ItemDictionary.Keys)
            {
                Item item = Item.CopyItemFromDictionary(itemID);

                if (item.itemType == itemType && item.baseType == itemBaseType && item.wearLocation == wearLocation)
                {
                    // need to create a new item now
                    Item newItem = new Item(item);

                    // now we modify AC, name, shortDesc, longDesc, flammable and special attributes
                    // then insert into database after performing a check if certain data already exists


                }
            }
        }
    }
}
