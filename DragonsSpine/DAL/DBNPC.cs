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
using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace DragonsSpine.DAL
{
    /// <summary>
    /// Summary description for DBNPC.
    /// </summary>
    internal static class DBNPC
    {
        internal static bool LoadNPCDictionary()
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_NPC_Select_All", conn);
                    DataTable dtNPCs = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtNPCs.Rows)
                    {
                        int npcID = Convert.ToInt32(dr["npcID"]);

                        if (!NPC.NPCDictionary.ContainsKey(npcID))
                        {
                            NPC.NPCDictionary.Add(npcID, dr);
                        }
                        else Utils.Log("DAL.DBNPC.LoadNPCDictionary attempted to add an NPC ID that already exists. NPCID: " + npcID, Utils.LogType.SystemWarning);
                    }
                    Utils.Log("Loaded NPCs (" + NPC.NPCDictionary.Count.ToString() + ")", Utils.LogType.SystemGo);
                    return true;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return false;
                }
            }
        }

        internal static NPC GetNPCByID(int NpcID)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_NPC_Select", conn);
                    sp.AddParameter("@npcID", SqlDbType.Int, 4, ParameterDirection.Input, NpcID);
                    DataTable dtNPCs = sp.ExecuteDataTable();
                    return new NPC(dtNPCs.Rows[0]);
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        /// <summary>
        /// Deprecated as of 10/16/2015. -Eb
        /// </summary>
        /// <param name="diff"></param>
        /// <returns></returns>
        internal static ArrayList GetRandomNPCs(int diff)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                ArrayList mylist = new ArrayList();
                int num = 6;
                SqlStoredProcedure sp = new SqlStoredProcedure("prApp_NPC_Select_Random", conn);
                ArrayList npclist = new ArrayList();
                sp.AddParameter("@difficulty", SqlDbType.Int, 8, ParameterDirection.Input, diff);
                DataTable dtNPCs = sp.ExecuteDataTable();
                foreach (DataRow dr in dtNPCs.Rows)
                {
                    npclist.Add(new NPC(dr));
                }
                for (int x = 0; x < num; x++)
                {
                    mylist.Add(npclist[Rules.Dice.Next(npclist.Count)]);
                }
                return mylist;
            }
        }

        /// <summary>
        /// Insert a new record into CatalogItem by copying an existing record and supplying a new item ID.
        /// </summary>
        /// <param name="copiedItemID">The item ID of the record to copy.</param>
        /// <param name="suppliedItemID">The new records item ID.</param>
        /// <param name="developer">Null if supplied by ItemBuilder, otherwise the developer adding the new record.</param>
        /// <returns>Number of rows inserted or -1 for failure/no insert.</returns>
        internal static int CopyNPCRecordAndInsert(int copiedNPCID, int suppliedNPCID, PC developer)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                var insertInto = new SqlCommand();

                try
                {
                    if (!NPC.NPCDictionary.ContainsKey(suppliedNPCID))
                    {
                        NPC copiedNPC = NPC.CopyNPCFromDictionary(copiedNPCID);

                        if (copiedNPC == null) return -1;

                        insertInto.CommandText = "INSERT INTO NPC (npcID,notes,name,attackSound,deathSound,idleSound,movementString,shortDesc,longDesc,visualKey" +
",classFullName,baseArmorClass,thac0Adjustment,special,npcType,aiType,species,gold,classType,exp,hitsMax,alignment,stamina,manaMax,speed,strength,dexterity" +
",intelligence,wisdom,constitution,charisma,lootVeryCommonAmount,lootVeryCommonArray,lootVeryCommonOdds,lootCommonAmount,lootCommonArray,lootCommonOdds" +
",lootRareAmount,lootRareArray,lootRareOdds,lootVeryRareAmount,lootVeryRareArray,lootVeryRareOdds,lootLairAmount,lootLairArray,lootLairOdds,lootAlwaysArray" +
",lootBeltAmount,lootBeltArray,lootBeltOdds,spawnArmorArray,spawnLeftHandArray,spawnLeftHandOdds,spawnRightHandArray,mace,bow,dagger,flail,rapier,twoHanded" +
",staff,shuriken,sword,threestaff,halberd,thievery,unarmed,magic,bash,level,animal,tanningResult,undead,spectral,hidden,poisonous,waterDweller,fly,breatheWater" +
",nightVision,lair,lairCells,mobile,command,castMode,randomName,attackString1,attackString2,attackString3,attackString4,attackString5,attackString6,blockString1" +
",blockString2,blockString3,merchantType,merchantMarkup,trainerType,interactiveType,gender,race,age,patrol,patrolRoute,immuneFire,immuneCold,immunePoison,immuneLightning" +
",immuneCurse,immuneDeath,immuneStun,immuneFear,immuneBlind,spells,quests,groupAmount,groupMembers,weaponRequirement,questFlags)";

                        insertInto.CommandText += " SELECT " + suppliedNPCID + ", '" + Autonomy.EntityBuilding.EntityCreationManager.NPC_INSERT_NOTES_DEFAULT + "', name,attackSound,deathSound,idleSound,movementString,shortDesc,longDesc,visualKey" +
",classFullName,baseArmorClass,thac0Adjustment,special,npcType,aiType,species,gold,classType,exp,hitsMax,alignment,stamina,manaMax,speed,strength,dexterity" +
",intelligence,wisdom,constitution,charisma,lootVeryCommonAmount,lootVeryCommonArray,lootVeryCommonOdds,lootCommonAmount,lootCommonArray,lootCommonOdds" +
",lootRareAmount,lootRareArray,lootRareOdds,lootVeryRareAmount,lootVeryRareArray,lootVeryRareOdds,lootLairAmount,lootLairArray,lootLairOdds,lootAlwaysArray" +
",lootBeltAmount,lootBeltArray,lootBeltOdds,spawnArmorArray,spawnLeftHandArray,spawnLeftHandOdds,spawnRightHandArray,mace,bow,dagger,flail,rapier,twoHanded" +
",staff,shuriken,sword,threestaff,halberd,thievery,unarmed,magic,bash,level,animal,tanningResult,undead,spectral,hidden,poisonous,waterDweller,fly,breatheWater" +
",nightVision,lair,lairCells,mobile,command,castMode,randomName,attackString1,attackString2,attackString3,attackString4,attackString5,attackString6,blockString1" +
",blockString2,blockString3,merchantType,merchantMarkup,trainerType,interactiveType,gender,race,age,patrol,patrolRoute,immuneFire,immuneCold,immunePoison,immuneLightning" +
",immuneCurse,immuneDeath,immuneStun,immuneFear,immuneBlind,spells,quests,groupAmount,groupMembers,weaponRequirement,questFlags" +
                        " FROM NPC WHERE npcID = " + copiedNPCID.ToString();

                        insertInto.Connection = conn;
                        insertInto.Connection.Open();

                        Utils.Log("DB Query: " + insertInto.CommandText, Utils.LogType.DatabaseQuery);

                        var returnValue = insertInto.ExecuteNonQuery();

                        insertInto.Connection.Close();

                        string logString = "Insertion of new copied NPC from ID " + copiedNPCID + " successful. NPC was inserted by " + developer == null ? "EntityCreationManager" : developer.GetLogString() + ".";
                        Utils.Log(logString, Utils.LogType.DatabaseQuery);
                        return returnValue;
                    }
                    else if (developer != null)
                    {
                        developer.WriteToDisplay("NPC ID " + suppliedNPCID + " already exists.");
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
        internal static int InsertNPCWithDefaultValues(int npcID, PC developer)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    if (!NPC.NPCDictionary.ContainsKey(npcID))
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

                        var insertNPC = new SqlCommand();

                        insertNPC.CommandText = "INSERT INTO NPC (npcID,notes,name,attackSound,deathSound,idleSound,movementString,shortDesc,longDesc,visualKey" +
",classFullName,baseArmorClass,thac0Adjustment,special,npcType,aiType,species,gold,classType,exp,hitsMax,alignment,stamina,manaMax,speed,strength,dexterity" +
",intelligence,wisdom,constitution,charisma,lootVeryCommonAmount,lootVeryCommonArray,lootVeryCommonOdds,lootCommonAmount,lootCommonArray,lootCommonOdds" +
",lootRareAmount,lootRareArray,lootRareOdds,lootVeryRareAmount,lootVeryRareArray,lootVeryRareOdds,lootLairAmount,lootLairArray,lootLairOdds,lootAlwaysArray" +
",lootBeltAmount,lootBeltArray,lootBeltOdds,spawnArmorArray,spawnLeftHandArray,spawnLeftHandOdds,spawnRightHandArray,mace,bow,dagger,flail,rapier,twoHanded" +
",staff,shuriken,sword,threestaff,halberd,thievery,unarmed,magic,bash,level,animal,tanningResult,undead,spectral,hidden,poisonous,waterDweller,fly,breatheWater" +
",nightVision,lair,lairCells,mobile,command,castMode,randomName,attackString1,attackString2,attackString3,attackString4,attackString5,attackString6,blockString1" +
",blockString2,blockString3,merchantType,merchantMarkup,trainerType,interactiveType,gender,race,age,patrol,patrolRoute,immuneFire,immuneCold,immunePoison,immuneLightning" +
",immuneCurse,immuneDeath,immuneStun,immuneFear,immuneBlind,spells,quests,groupAmount,groupMembers,weaponRequirement,questFlags)" +
                        " VALUES ('" + npcID + "'";
                            //notes + "', '" + combatAdds + "', '" + itemID + "', '" + itemType + "', '" + baseType + "', '" + name + "', '" + visualKey + "', '" +
                            //unidentifiedName + "','" + identifiedName + "','" + shortDesc +
                            //"', '" + longDesc + "', '" + wearLocation + "', '" + weight + "', '" + coinValue + "', '" + size + "', '" + effectType + "', '" +
                            //effectAmount + "', '" + effectDuration + "', '" + special +
                            //"', '" + minDamage + "', '" + maxDamage + "', '" + skillType + "', '" + vRandLow + "', '" + vRandHigh + "', '" + key + "', '" +
                            //recall + "', '" + alignment + "', '" + spell + "', '" + spellPower + "', '" + charges +
                            //"', '" + attackType + "', '" + blueglow + "', '" + flammable + "', '" + fragile + "', '" + lightning + "', '" + returning + "', '" + silver + "', '" + attuneType + "', '" + figExp + "', '" +
                            //armorClass + "', '" + armorType + "', '" + bookType + "', '" + maxPages + "', '" + pages + "', '" + drinkDesc + "', '" + fluidDesc + "', '" + lootTable + "')";

                        insertNPC.Connection = conn;
                        insertNPC.Connection.Open();
                        var returnValue = insertNPC.ExecuteNonQuery();
                        insertNPC.Connection.Close();

                        string logString = "Insertion of new default NPC with ID " + npcID + " successful. Item was inserted by " + developer == null ? "EntityCreationManager" : developer.GetLogString() + ".";
                        Utils.Log(logString, Utils.LogType.DatabaseQuery);
                        return returnValue;
                    }
                    else if (developer != null)
                    {
                        developer.WriteToDisplay("NPC ID " + npcID + " already exists.");
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
    }
}	

