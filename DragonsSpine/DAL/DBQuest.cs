using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DragonsSpine.DAL
{
    internal static class DBQuest
    {
        internal static List<int> GetQuestIDList()
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                List<int> idList = new List<int>();

                try
                {
                    var sp = new SqlStoredProcedure("prApp_Quest_Select", conn);
                    DataTable dtCatalogItem = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtCatalogItem.Rows)
                    {
                        idList.Add(Convert.ToInt32(dr["questID"]));
                    }
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }

                return idList;
            }
        }

        internal static bool QuestIDExists(int questID)
        {
            if (!GameQuest.QuestDictionary.ContainsKey(questID))
            {
                using (var conn = DataAccess.GetSQLConnection())
                {
                    try
                    {
                        var sp = new SqlStoredProcedure("prApp_Quest_Select", conn);
                        DataTable dtCatalogItem = sp.ExecuteDataTable();
                        foreach (DataRow dr in dtCatalogItem.Rows)
                        {
                            if (questID == Convert.ToInt32(dr["questID"]))
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

        internal static int CopyQuestRecordAndInsert(int copiedQuestID, PC developer)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                var insertInto = new SqlCommand();

                try
                {
                    if (QuestIDExists(copiedQuestID))
                    {
                        GameQuest copiedQuest = GameQuest.CopyQuest(copiedQuestID);

                        if (copiedQuest == null) return -1;

                        insertInto.CommandText = "INSERT INTO Quest (notes, name, description, completedDescription, requirements, requiredFlags, coinValues," +
                        "rewardClass, rewardTitle, rewardFlags, requiredItems, rewardItems, rewardExperience, rewardStats, rewardTeleports, responseStrings," +
                        "hintStrings, flagStrings, stepStrings, finishStrings, failStrings, classTypes, alignments, maximumLevel, minimumLevel, repeatable, stepOrder," +
                        "soundFiles, totalSteps, despawnsNPC, masterQuestID, teleportGroup, methodNames)";

                        insertInto.CommandText += " SELECT '" + GameQuest.QUEST_INSERT_NOTES_DEFAULT + "', name, description, completedDescription, requirements, requiredFlags, coinValues," +
                        "rewardClass, rewardTitle, rewardFlags, requiredItems, rewardItems, rewardExperience, rewardStats, rewardTeleports, responseStrings," +
                        "hintStrings, flagStrings, stepStrings, finishStrings, failStrings, classTypes, alignments, maximumLevel, minimumLevel, repeatable, stepOrder," +
                        "soundFiles, totalSteps, despawnsNPC, masterQuestID, teleportGroup, methodNames" +
                        " FROM Quest WHERE questID = " + copiedQuestID.ToString();

                        insertInto.Connection = conn;
                        insertInto.Connection.Open();

                        Utils.Log("DB Query: " + insertInto.CommandText, Utils.LogType.DatabaseQuery);

                        var returnValue = insertInto.ExecuteNonQuery();

                        insertInto.Connection.Close();

                        string logString = "Insertion of new copied quest from ID " + copiedQuestID + " successful. Quest was inserted by " + developer == null ? "Quest Builder" : developer.GetLogString() + ".";
                        Utils.Log(logString, Utils.LogType.DatabaseQuery);
                        return returnValue;
                    }
                    else if (developer != null)
                    {
                        developer.WriteToDisplay("Quest ID " + copiedQuestID + " does not exist.");
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
    }
}
