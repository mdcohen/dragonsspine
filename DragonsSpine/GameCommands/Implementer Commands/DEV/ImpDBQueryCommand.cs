using System;

namespace DragonsSpine.Commands
{
    [CommandAttribute("impdbquery", "Execute a database query. Caution: SQL is not currently verified.", (int)Globals.eImpLevel.DEV, new string[] { "impdb" },
        0, new string[] { "impdb [query text]", "impdb default [item | quest], impdb default item copy # #" }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ImpDBQueryCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Query failed.");
                return true;
            }

            //TODO: verify SQL syntax here
            // impdb default item [itemID]
            // impdb default quest
            if (args.ToLower().StartsWith("default"))
            {
                #region Default Inserts (also includes copying of existing row)
                string[] sArgs = args.Split(" ".ToCharArray());

                if (sArgs.Length == 1)
                {
                    chr.WriteToDisplay("Choose the type of default row you wish to insert into the database. Options are: item");
                    return true;
                }

                switch (sArgs[1].ToLower())
                {
                    case "cell":
                        chr.WriteToDisplay("Please use the command impcellmod to insert or modify Cell rows in the database.");
                        return true;
                    case "npc":
                        int copiedNPCID = -1;

                        if (sArgs.Length >= 4)
                        {
                            #region Copy existing NPC and insert new record.
                            // Going to copy a record. /impdb default item copy (itemID to copy) (supplied itemID)
                            if (sArgs[2].ToLower() == "copy")
                            {
                                if (!int.TryParse(sArgs[3], out copiedNPCID))
                                {
                                    chr.WriteToDisplay("NPC ID of record to copy must be supplied. Please use '/impdb default npc copy npcID' or '/impdb default npc copy npcID newItemID'");
                                    return true;
                                }

                                // Requested NPC ID to copy does not exist.
                                if (!NPC.NPCDictionary.ContainsKey(copiedNPCID))
                                {
                                    chr.WriteToDisplay("NPC ID " + copiedNPCID + " does not exist. Copy and insert cancelled.");
                                    return true;
                                }

                                int suppliedNPCID = -1;

                                // Supply a new npc ID if no value was in arguments.
                                if (sArgs.Length == 4)
                                {
                                    suppliedNPCID = Autonomy.EntityBuilding.EntityCreationManager.GetNextAvailableNPCID();
                                }
                                // impdb default NPC copy npcID suppliedNPCID
                                else if (sArgs.Length >= 5 && int.TryParse(sArgs[4], out suppliedNPCID))
                                {
                                    if (NPC.NPCDictionary.ContainsKey(suppliedNPCID))
                                    {
                                        chr.WriteToDisplay("NPC ID (" + suppliedNPCID + ") supplied for new record already exists or is invalid.");
                                        return true;
                                    }
                                    else chr.WriteToDisplay("Supplied ID (" + suppliedNPCID + ") accepted.");
                                }

                                if (DAL.DBNPC.CopyNPCRecordAndInsert(copiedNPCID, suppliedNPCID, chr as PC) > 0)
                                {
                                    chr.WriteToDisplay("New NPC added to the database with NPC ID " + suppliedNPCID + ".");
                                    chr.WriteToDisplay("The NPC currently has all the same values of NPC ID " + copiedNPCID + " except NOTES (and catalogID which is the KEY).");
                                    chr.WriteToDisplay("The NPC will not be added to the NPC dictionary until the notes are manually updated.");
                                    return true;
                                }
                                else
                                {
                                    chr.WriteToDisplay("Insertion of copied NPC " + copiedNPCID + " failed.");
                                    return true;
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            // Assign a new NPC ID and insert into the database.
                            copiedNPCID = Autonomy.EntityBuilding.EntityCreationManager.GetNextAvailableNPCID();
                        }

                        // Only make it here if a new default NPC will be inserted.
                        if (copiedNPCID > 0)
                        {
                            chr.WriteToDisplay("Insertion of default NPC with NPC ID " + copiedNPCID + " failed. Insertion with default values not supported yet.");

                            //if (DAL.DBNPC.InsertNPCWithDefaultValues(copiedNPCID, chr as PC) == 1)
                            //{
                            //    chr.WriteToDisplay("New NPC added to the database with NPC ID " + copiedNPCID + ".");
                            //    chr.WriteToDisplay("The NPC currently has default values. It was not added to the NPC dictionary and will not be added until the notes are manually updated and the server is restarted.");
                            //    return true;
                            //}
                            //else
                            //{
                            //    chr.WriteToDisplay("Insertion of default NPC with NPC ID " + copiedNPCID + " failed.");
                            //    return true;
                            //}
                        }
                        else chr.WriteToDisplay("Insertion of new default NPC failed.");
                        break;
                    case "item":
                        int copiedItemID = -1;

                        if (sArgs.Length >= 4)
                        {
                            #region Copy existing item and insert new record.
                            // Going to copy a record. /impdb default item copy (itemID to copy) (supplied itemID)
                            if (sArgs[2].ToLower() == "copy")
                            {
                                if (!int.TryParse(sArgs[3], out copiedItemID))
                                {
                                    chr.WriteToDisplay("Item ID of record to copy must be supplied. Please use '/impdb default item copy itemID' or '/impdb default item copy itemID newItemID'");
                                    return true;
                                }

                                // Requested item ID to copy does not exist.
                                if (!DAL.DBItem.ItemIDExists(copiedItemID))
                                {
                                    chr.WriteToDisplay("Item ID " + copiedItemID + " does not exist. Copy and insert cancelled.");
                                    return true;
                                }

                                int suppliedItemID = -1;

                                // Supply a new item ID if no value was in arguments.
                                if (sArgs.Length == 4)
                                {
                                    suppliedItemID = Autonomy.ItemBuilding.ItemGenerationManager.GetNextAvailableItemID();
                                }
                                // impdb default item copy itemID suppliedID
                                else if (sArgs.Length >= 5 && int.TryParse(sArgs[4], out suppliedItemID))
                                {
                                    if (DAL.DBItem.ItemIDExists(suppliedItemID))
                                    {
                                        chr.WriteToDisplay("Item ID (" + suppliedItemID + ") supplied for new record already exists or is invalid.");
                                        return true;
                                    }
                                    else chr.WriteToDisplay("Supplied ID (" + suppliedItemID + ") accepted.");
                                }

                                if (DAL.DBItem.CopyItemRecordAndInsert(copiedItemID, suppliedItemID, chr as PC) > 0)
                                {
                                    chr.WriteToDisplay("New item added to the database with item ID " + suppliedItemID + ".");
                                    chr.WriteToDisplay("The item currently has all the same values of Item ID " + copiedItemID + " except NOTES (and catalogID which is the KEY).");
                                    chr.WriteToDisplay("The item will not be added to the item dictionary until the notes are manually updated.");
                                    return true;
                                }
                                else
                                {
                                    chr.WriteToDisplay("Insertion of copied item " + copiedItemID + " failed.");
                                    return true;
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            // Assign a new item ID and insert into the database.
                            copiedItemID = Autonomy.ItemBuilding.ItemGenerationManager.GetNextAvailableItemID();
                        }

                        // Only make it here if a new default item will be inserted.
                        if (copiedItemID > 0)
                        {
                            if (DAL.DBItem.InsertItemWithDefaultValues(copiedItemID, chr as PC) == 1)
                            {
                                chr.WriteToDisplay("New item added to the database with item ID " + copiedItemID + ".");
                                chr.WriteToDisplay("The item currently has default values. It was not added to the item dictionary and will not be added until the notes are manually updated and the server is restarted.");
                                return true;
                            }
                            else
                            {
                                chr.WriteToDisplay("Insertion of default item with item ID " + copiedItemID + " failed.");
                                return true;
                            }
                        }
                        else chr.WriteToDisplay("Insertion of new default item failed.");
                        break;
                    case "quest":
                    case "quests":
                        int copiedQuestID = -1;

                        if (sArgs.Length >= 4)
                        {
                            #region Copy existing item and insert new record.
                            // Going to copy a record. /impdb default item copy (itemID to copy) (supplied itemID)
                            if (sArgs[2].ToLower() == "copy")
                            {
                                if (!Int32.TryParse(sArgs[3], out copiedQuestID))
                                {
                                    chr.WriteToDisplay("Quest ID of record to copy must be supplied. Please use '/impdb default quest copy questID' or '/impdb default quest copy questID newQuestID'");
                                    return true;
                                }

                                // Requested item ID to copy does not exist.
                                if (!GameQuest.QuestDictionary.ContainsKey(copiedQuestID))
                                {
                                    chr.WriteToDisplay("Quest ID " + copiedQuestID + " does not exist. Copy and insert cancelled.");
                                    return true;
                                }

                                //int newQuestID = -1;

                                // Supply a new quest ID if no value was in arguments.
                                //if (sArgs.Length == 4)
                                //{
                                //    newQuestID = Quest.GetNextAvailableQuestID();
                                //}
                                // impdb default quest copy questID suppliedID
                                //else// if (sArgs.Length >= 5 && Int32.TryParse(sArgs[4], out suppliedQuestID))
                                //{
                                    //if (Quest.QuestDictionary.ContainsKey(suppliedQuestID))
                                    //{
                                    //    chr.WriteToDisplay("Quest ID (" + suppliedQuestID + ") supplied for new record already exists or is invalid.");
                                    //    return true;
                                    //}
                                    //else chr.WriteToDisplay("Supplied ID (" + suppliedQuestID + ") accepted.");
                                //}

                                if (DAL.DBQuest.CopyQuestRecordAndInsert(copiedQuestID, chr as PC) > 0)
                                {
                                    System.Collections.Generic.List<int> questIDs = DAL.DBQuest.GetQuestIDList();

                                    chr.WriteToDisplay("New quest added to the database with item ID " + questIDs[questIDs.Count - 1] + ".");
                                    chr.WriteToDisplay("The quest currently has all the same values of Quest ID " + copiedQuestID + " except NOTES.");
                                    chr.WriteToDisplay("The quest will not be added to the quest dictionary until the notes are manually updated and the server is restarted.");
                                    return true;
                                }
                                else
                                {
                                    chr.WriteToDisplay("Insertion of copied item " + copiedQuestID + " failed.");
                                    return true;
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            // Assign a new item ID and insert into the database.
                            System.Collections.Generic.List<int> questIDList = new System.Collections.Generic.List<int>();
                            foreach(GameQuest quest in GameQuest.QuestDictionary.Values)
                                questIDList.Add(quest.QuestID);
                            copiedQuestID = questIDList[Rules.Dice.Next(questIDList.Count)];
                        }

                        // Only make it here if a new default item will be inserted.
                        if (copiedQuestID > 0)
                        {
                            if (DAL.DBQuest.CopyQuestRecordAndInsert(copiedQuestID, chr as PC) == 1)
                            {
                                System.Collections.Generic.List<int> questIDs = DAL.DBQuest.GetQuestIDList();

                                chr.WriteToDisplay("New quest added to the database with quest ID " + questIDs[questIDs.Count - 1] + ".");
                                chr.WriteToDisplay("The quest currently has the same values as quest ID " + copiedQuestID + " except NOTES.");
                                chr.WriteToDisplay("It was not added to the quest dictionary and will not be added until the notes are manually updated and the server is restarted.");
                                return true;
                            }
                            else
                            {
                                chr.WriteToDisplay("Insertion of default quest with quest ID " + copiedQuestID + " failed.");
                                return true;
                            }
                        }
                        else chr.WriteToDisplay("Insertion of new default quest failed.");
                        return true;
                }
                #endregion
            }
            else if (args.ToLower().StartsWith("update"))
            {
                // Safeguard to prevent mistakenly changing values of all entries.
                if (!args.ToLower().Contains(" where "))
                {
                    chr.WriteToDisplay("Query failed. All updates must include a WHERE clause.");
                    return true;
                }
            }
            else if(!args.ToLower().StartsWith("insert")) // only update and inserts allowed for now
            {
                chr.WriteToDisplay("Query failed. Arguments not recognized.");
                return true;
            }

            var rowsAffected = DAL.DataAccess.ExecuteQuery(args);

            if (rowsAffected == -1)
            {
                chr.WriteToDisplay("Query failed. Verify SQL.");
                Utils.Log(chr.GetLogString() + " failed SQL query: " + args, Utils.LogType.DatabaseQuery);
            }
            else
            {
                chr.WriteToDisplay("Query successful. " + rowsAffected + " row(s) affected.");
                Utils.Log(chr.GetLogString() + " performed SQL query: " + args + ". " + rowsAffected + " row(s) affected.", Utils.LogType.DatabaseQuery);
            }

            return true;
        }
    }
}
