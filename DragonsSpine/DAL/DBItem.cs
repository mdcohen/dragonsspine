using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace DragonsSpine.DAL
{
    internal static class DBItem
    {
        //insertItem = new SqlStoredProcedure("prApp_CatalogItem_Insert", conn);
        //insertItem.Parameters.AddWithValue("@notes", notes);
        //insertItem.Parameters.AddWithValue("@combatAdds", combatAdds);
        //insertItem.Parameters.AddWithValue("@itemID", itemID);
        //insertItem.Parameters.AddWithValue("@itemType", itemType);
        //insertItem.Parameters.AddWithValue("@baseType", baseType);
        //insertItem.Parameters.AddWithValue("@name", name);
        //insertItem.Parameters.AddWithValue("@visualKey", visualKey);
        //insertItem.Parameters.AddWithValue("@unidentifiedName", unidentifiedName);
        //insertItem.Parameters.AddWithValue("@identifiedName", identifiedName);
        //insertItem.Parameters.AddWithValue("@shortDesc", shortDesc);
        //insertItem.Parameters.AddWithValue("@longDesc", longDesc);
        //insertItem.Parameters.AddWithValue("@wearLocation", wearLocation);
        //insertItem.Parameters.AddWithValue("@weight", weight);
        //insertItem.Parameters.AddWithValue("@coinValue", coinValue);
        //insertItem.Parameters.AddWithValue("@size", size);
        //insertItem.Parameters.AddWithValue("@effectType", effectType);
        //insertItem.Parameters.AddWithValue("@effectAmount", effectAmount);
        //insertItem.Parameters.AddWithValue("@effectDuration", effectDuration);
        //insertItem.Parameters.AddWithValue("@special", special);
        //insertItem.Parameters.AddWithValue("@minDamage", minDamage);
        //insertItem.Parameters.AddWithValue("@maxDamage", maxDamage);
        //insertItem.Parameters.AddWithValue("@skillType", skillType);
        //insertItem.Parameters.AddWithValue("@vRandLow", vRandLow);
        //insertItem.Parameters.AddWithValue("@vRandHigh", vRandHigh);
        //insertItem.Parameters.AddWithValue("@key", key);
        //insertItem.Parameters.AddWithValue("@recall", recall);
        //insertItem.Parameters.AddWithValue("@alignment", alignment);
        //insertItem.Parameters.AddWithValue("@spell", spell);
        //insertItem.Parameters.AddWithValue("@spellPower", spellPower);
        //insertItem.Parameters.AddWithValue("@charges", charges);
        //insertItem.Parameters.AddWithValue("@attackType", attackType);
        //insertItem.Parameters.AddWithValue("@blueglow", blueglow);
        //insertItem.Parameters.AddWithValue("@flammable", flammable);
        //insertItem.Parameters.AddWithValue("@fragile", fragile);
        //insertItem.Parameters.AddWithValue("@lightning", lightning);
        //insertItem.Parameters.AddWithValue("@returning", returning);
        //insertItem.Parameters.AddWithValue("@silver", silver);
        //insertItem.Parameters.AddWithValue("@attuneType", attuneType);
        //insertItem.Parameters.AddWithValue("@figExp", figExp);
        //insertItem.Parameters.AddWithValue("@armorClass", armorClass);
        //insertItem.Parameters.AddWithValue("@armorType", armorType);
        //insertItem.Parameters.AddWithValue("@bookType", bookType);
        //insertItem.Parameters.AddWithValue("@maxPages", maxPages);
        //insertItem.Parameters.AddWithValue("@pages", pages);
        //insertItem.Parameters.AddWithValue("@drinkDesc", drinkDesc);
        //insertItem.Parameters.AddWithValue("@fluidDesc", fluidDesc);
        //insertItem.Parameters.AddWithValue("@lootTable", lootTable);

        /// <summary>
        /// All items in the database are loaded into the Item Dictionary.
        /// </summary>
        /// <returns></returns>
        internal static bool LoadItems()
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    var sp = new SqlStoredProcedure("prApp_CatalogItem_Select_All", conn);
                    DataTable dtCatalogItem = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtCatalogItem.Rows)
                    {
                        int itemID = Convert.ToInt32(dr["itemID"]);

                        if (!Item.ItemDictionary.ContainsKey(itemID))
                        {
                            if (dr["notes"].ToString() != Autonomy.ItemBuilding.ItemGenerationManager.ITEM_INSERT_NOTES_DEFAULT)
                            {
                                Item.ItemDictionary.Add(itemID, dr);
                            }
                            else Utils.Log("Failed to add Item ID " + itemID + " to Item Dictionary. Item is a default item and the notes have not been modified.", Utils.LogType.SystemFailure);
                        }
                        else Utils.Log("Failed to add Item ID " + itemID + " to Item Dictionary. Item ID already exists.", Utils.LogType.SystemFailure);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return false;
                }
            }
        }

        internal static int UpdateItemColumn(int itemID, string column, string value)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                var updateItem = new SqlCommand();
                updateItem.Connection = conn;

                updateItem.CommandText = "UPDATE CatalogItem SET " + column + " = '" +
                                         value + "' WHERE itemID = '" + itemID + "'";
                updateItem.Connection.Open();
                var returnValue = updateItem.ExecuteNonQuery();
                updateItem.Connection.Close();

                return returnValue;
            }
        }

        /// <summary>
        /// Insert a new record into CatalogItem by copying an existing record and supplying a new item ID.
        /// </summary>
        /// <param name="copiedItemID">The item ID of the record to copy.</param>
        /// <param name="suppliedItemID">The new records item ID.</param>
        /// <param name="developer">Null if supplied by ItemBuilder, otherwise the developer adding the new record.</param>
        /// <returns>Number of rows inserted or -1 for failure/no insert.</returns>
        internal static int CopyItemRecordAndInsert(int copiedItemID, int suppliedItemID, PC developer)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                var insertInto = new SqlCommand();

                try
                {
                    if (!ItemIDExists(suppliedItemID))
                    {
                        Item copiedItem = Item.CopyItemFromDictionary(copiedItemID);

                        if (copiedItem == null) return -1;

                        insertInto.CommandText = "INSERT INTO CatalogItem (notes, combatAdds, itemID, itemType, baseType, name, visualKey, unidentifiedName, identifiedName, shortDesc, " +
                        "longDesc, wearLocation, [weight], coinValue, size, effectType, effectAmount, effectDuration, special, " +
                        "minDamage, maxDamage, skillType, vRandLow, vRandHigh, [key], recall, alignment, spell, spellPower, charges," +
                        "attackType, blueglow, flammable, fragile, lightning, returning, silver, attuneType, figExp, " +
                        "armorClass, armorType, bookType, maxPages, pages, drinkDesc, fluidDesc, lootTable)";

                        insertInto.CommandText += " SELECT '" + Autonomy.ItemBuilding.ItemGenerationManager.ITEM_INSERT_NOTES_DEFAULT + "', combatAdds, " + suppliedItemID + ", itemType, baseType, name, visualKey, unidentifiedName, identifiedName, shortDesc, " +
                        "longDesc, wearLocation, [weight], coinValue, size, effectType, effectAmount, effectDuration, special, " +
                        "minDamage, maxDamage, skillType, vRandLow, vRandHigh, [key], recall, alignment, spell, spellPower, charges," +
                        "attackType, blueglow, flammable, fragile, lightning, returning, silver, attuneType, figExp, " +
                        "armorClass, armorType, bookType, maxPages, pages, drinkDesc, fluidDesc, lootTable" +
                        " FROM CatalogItem WHERE itemID = " + copiedItemID.ToString();

                        insertInto.Connection = conn;
                        insertInto.Connection.Open();

                        Utils.Log("DB Query: " + insertInto.CommandText, Utils.LogType.DatabaseQuery);

                        var returnValue = insertInto.ExecuteNonQuery();

                        insertInto.Connection.Close();

                        string logString = "Insertion of new copied item from ID " + copiedItemID + " successful. Item was inserted by " + developer == null ? "Item Builder" : developer.GetLogString() + ".";
                        Utils.Log(logString, Utils.LogType.DatabaseQuery);
                        return returnValue;
                    }
                    else if (developer != null)
                    {
                        developer.WriteToDisplay("Item ID " + suppliedItemID + " already exists.");
                    }
                }
                catch (SqlException sqlEx)
                {
                    Utils.LogException(sqlEx);
                    Utils.Log("Error with DB Query: " + insertInto.CommandText, Utils.LogType.DatabaseQuery);
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    Utils.Log("Error with DB Query: " + insertInto.CommandText, Utils.LogType.DatabaseQuery);
                }
            }

            return -1;
        }

        /// <summary>
        /// Insert a new CatalogItem entry into the database. The item has a unique itemID and all other values are default. The item is not placed in the ItemDictionary.
        /// </summary>
        /// <param name="itemID">Assigned item ID.</param>
        /// <param name="developer">The developer who inserted the item into the database. Null if item inserted by Item Builder.</param>
        /// <returns>1 if successful, otherwise failure.</returns>
        internal static int InsertItemWithDefaultValues(int itemID, PC developer)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    if (!ItemIDExists(itemID))
                    {
                        #region Default values.
                        var notes = Autonomy.ItemBuilding.ItemGenerationManager.ITEM_INSERT_NOTES_DEFAULT;
                        var combatAdds = 0;
                        var itemType = Utils.FormatEnumString(Globals.eItemType.Miscellaneous.ToString());
                        var baseType = Utils.FormatEnumString(Globals.eItemBaseType.Unknown.ToString());
                        var name = "name";
                        var visualKey = "unknown";
                        var unidentifiedName = "unidentified name";
                        var identifiedName = "identified name";
                        var shortDesc = "short description";
                        var longDesc = "long description";
                        var wearLocation = Utils.FormatEnumString(Globals.eWearLocation.None.ToString());
                        var weight = .1;
                        var coinValue = 1;
                        var size = Globals.eItemSize.Sack_Only.ToString();
                        var effectType = 0;
                        var effectAmount = "0";
                        var effectDuration = 0;
                        var special = "";
                        var minDamage = 1;
                        var maxDamage = 2;
                        var skillType = Utils.FormatEnumString(Globals.eSkillType.None.ToString());
                        var vRandLow = 0;
                        var vRandHigh = 0;
                        var key = "";
                        var recall = false;
                        var alignment = Utils.FormatEnumString(Globals.eAlignment.None.ToString());
                        var spell = -1;
                        var spellPower = -1;
                        var charges = -1;
                        var attackType = Utils.FormatEnumString(Globals.eWeaponAttackType.All.ToString());
                        var blueglow = false;
                        var flammable = false;
                        var fragile = false;
                        var lightning = false;
                        var returning = false;
                        var silver = false;
                        var attuneType = Utils.FormatEnumString(Globals.eAttuneType.None.ToString());
                        var figExp = 0;
                        var armorClass = 0;
                        var armorType = Utils.FormatEnumString(Globals.eArmorType.None.ToString());
                        var bookType = Utils.FormatEnumString(Globals.eBookType.None.ToString());
                        var maxPages = 0;
                        var pages = "";
                        var drinkDesc = "";
                        var fluidDesc = "";
                        var lootTable = "BG";
                        #endregion

                        var insertItem = new SqlCommand();

                        insertItem.CommandText =
                            "INSERT CatalogItem (notes, combatAdds, itemID, itemType, baseType, name, visualKey, unidentifiedName, identifiedName, shortDesc, " +
                                                "longDesc, wearLocation, [weight], coinValue, size, effectType, effectAmount, effectDuration, special, " +
                                                "minDamage, maxDamage, skillType, vRandLow, vRandHigh, [key], recall, alignment,spell, spellPower, charges ," +
                                                "attackType, blueglow, flammable, fragile, lightning, returning, silver, attuneType, figExp, " +
                                                "armorClass, armorType, bookType, maxPages, pages, drinkDesc, fluidDesc, lootTable)" +
                            " VALUES ('" +
                            notes + "', '" + combatAdds + "', '" + itemID + "', '" + itemType + "', '" + baseType + "', '" + name + "', '" + visualKey + "', '" +
                            unidentifiedName + "','" + identifiedName + "','" + shortDesc +
                            "', '" + longDesc + "', '" + wearLocation + "', '" + weight + "', '" + coinValue + "', '" + size + "', '" + effectType + "', '" +
                            effectAmount + "', '" + effectDuration + "', '" + special +
                            "', '" + minDamage + "', '" + maxDamage + "', '" + skillType + "', '" + vRandLow + "', '" + vRandHigh + "', '" + key + "', '" +
                            recall + "', '" + alignment + "', '" + spell + "', '" + spellPower + "', '" + charges +
                            "', '" + attackType + "', '" + blueglow + "', '" + flammable + "', '" + fragile + "', '" + lightning + "', '" + returning + "', '" + silver + "', '" + attuneType + "', '" + figExp + "', '" +
                            armorClass + "', '" + armorType + "', '" + bookType + "', '" + maxPages + "', '" + pages + "', '" + drinkDesc + "', '" + fluidDesc + "', '" + lootTable + "')";

                        insertItem.Connection = conn;
                        insertItem.Connection.Open();
                        var returnValue = insertItem.ExecuteNonQuery();
                        insertItem.Connection.Close();

                        string logString = "Insertion of new default item with ID " + itemID + " successful. Item was inserted by " + developer == null ? "Item Builder" : developer.GetLogString() + ".";
                        Utils.Log(logString, Utils.LogType.DatabaseQuery);
                        return returnValue;
                    }
                    else if (developer != null)
                    {
                        developer.WriteToDisplay("Item ID " + itemID + " already exists.");
                    }
                }
                catch (Exception e)
                {
                    conn.Close();
                    Utils.LogException(e);
                }
            }

            return -1;
        }

        /// <summary>
        /// Get catalog ID for a row based on item ID.
        /// </summary>
        /// <param name="itemID">Item ID of the row item.</param>
        /// <returns></returns>
        internal static int GetCatalogID(int itemID)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    var sp = new SqlStoredProcedure("prApp_CatalogItem_Select_All", conn);
                    DataTable dtCatalogItem = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtCatalogItem.Rows)
                    {
                        if (itemID == Convert.ToInt32(dr["itemID"]))
                        {
                            return Convert.ToInt32(dr["catalogID"]);
                        }
                    }
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
            }

            return -1;
        }

        internal static List<int> GetItemIDList()
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                List<int> idList = new List<int>();

                try
                {
                    var sp = new SqlStoredProcedure("prApp_CatalogItem_Select_All", conn);
                    DataTable dtCatalogItem = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtCatalogItem.Rows)
                    {
                        idList.Add(Convert.ToInt32(dr["itemID"]));
                    }
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }

                return idList;
            }
        }

        internal static bool ItemIDExists(int itemID)
        {
            if (!Item.ItemDictionary.ContainsKey(itemID))
            {
                using (var conn = DataAccess.GetSQLConnection())
                {
                    try
                    {
                        var sp = new SqlStoredProcedure("prApp_CatalogItem_Select_All", conn);
                        DataTable dtCatalogItem = sp.ExecuteDataTable();
                        foreach (DataRow dr in dtCatalogItem.Rows)
                        {
                            if (itemID == Convert.ToInt32(dr["itemID"]))
                                return true;
                        }
                    }
                    catch (Exception e)
                    {
                        Utils.LogException(e);
                        return true;
                    }
                }

                return false;
            }

            return true;
        }
    }
}
