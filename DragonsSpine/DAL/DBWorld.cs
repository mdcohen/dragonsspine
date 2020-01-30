using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using DragonsSpine.GameWorld;
namespace DragonsSpine.DAL
{
    internal static class DBWorld
    {
        public const string CELL_INSERT_NOTES_DEFAULT = "Default Cell";
        internal static bool LoadFacets()
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Facet_Select_All", conn);
                    DataTable dtFacets = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtFacets.Rows)
                    {
                        World.Add(new Facet(dr));
                        Utils.Log("Added Facet: " + dr["Name"].ToString(), Utils.LogType.SystemGo);
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
        internal static bool LoadLands(Facet facet) // return true if lands were loaded correctly
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Land_Select", conn);
                    DataTable dtLands = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtLands.Rows)
                    {
                        facet.Add(new Land(facet.FacetID, dr));
                        Utils.Log("Added Land: " + dr["Name"].ToString() + " to Facet: " + facet.Name, Utils.LogType.SystemGo);
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
        internal static bool LoadMaps(Land land)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Map_Select", conn);
                    sp.AddParameter("@landID", SqlDbType.SmallInt, 2, ParameterDirection.Input, land.LandID);
                    DataTable dtMaps = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtMaps.Rows)
                    {
                        //if (dr["name"].ToString() != "Innkadi")
                        //{
                        land.Add(new Map(land.FacetID, dr));
                        Utils.Log("Loaded Map: " + dr["name"].ToString(), Utils.LogType.SystemGo);
                        //}
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
        internal static bool LoadQuests()
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Quest_Select", conn);
                    DataTable dtQuest = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtQuest.Rows)
                    {
                        GameQuest.Add(new GameQuest(dr));
                        //Utils.Log("Loaded Quest: " + dr["name"].ToString(), Utils.LogType.SystemGo);
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
        internal static List<Cell> GetCellList(string name)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    List<Cell> cellsList = new List<Cell>();
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Cell_Select_By_Map", conn);
                    sp.AddParameter("@mapName", SqlDbType.NVarChar, 50, ParameterDirection.Input, name);
                    DataTable dtCellsInfo = sp.ExecuteDataTable();
                    if (dtCellsInfo != null)
                    {
                        foreach (DataRow dr in dtCellsInfo.Rows)
                        {
                            cellsList.Add(new Cell(dr));
                        }
                        Utils.Log("Loaded Cells: " + name, Utils.LogType.SystemGo);
                        return cellsList;
                    }
                    else
                    {
                        Utils.Log("DBWorld.GetCellsList returned null for map " + name, Utils.LogType.SystemFailure);
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }
        #region CharGen
        internal static void SetupNewCharacter(Character ch)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_CharGen_Select", conn);
                    sp.AddParameter("@race", SqlDbType.VarChar, 20, ParameterDirection.Input, ch.race);
                    sp.AddParameter("@classType", SqlDbType.Int, 4, ParameterDirection.Input, (int)ch.BaseProfession);
                    DataTable dtSetupNewChar = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtSetupNewChar.Rows)
                        ConvertRowToNewCharacter(ch, dr);
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
            }
        }
        internal static void ConvertRowToNewCharacter(Character newpc, DataRow drItem)
        {
            try
            {
                PC.SetCharacterVisualKey(newpc);
                // alignment
                newpc.Alignment = (Globals.eAlignment)Convert.ToInt32(drItem["alignment"]);
                // starting skills
                newpc.bow = Convert.ToInt32(drItem["bow"]);
                newpc.dagger = Convert.ToInt32(drItem["dagger"]);
                newpc.flail = Convert.ToInt32(drItem["flail"]);
                newpc.halberd = Convert.ToInt32(drItem["halberd"]);
                newpc.mace = Convert.ToInt32(drItem["mace"]);
                newpc.rapier = Convert.ToInt32(drItem["rapier"]);
                newpc.shuriken = Convert.ToInt32(drItem["shuriken"]);
                newpc.staff = Convert.ToInt32(drItem["staff"]);
                newpc.sword = Convert.ToInt32(drItem["sword"]);
                newpc.threestaff = Convert.ToInt32(drItem["threestaff"]);
                newpc.thievery = Convert.ToInt32(drItem["thievery"]);
                newpc.magic = Convert.ToInt32(drItem["magic"]);
                newpc.unarmed = Convert.ToInt32(drItem["unarmed"]);
                newpc.bash = Convert.ToInt32(drItem["bash"]);
                // right hand
                if (Convert.ToInt32(drItem["rightHand"]) != 0)
                {
                    newpc.RightHand = Item.CopyItemFromDictionary(Convert.ToInt32(drItem["rightHand"]));
                }
                // left hand
                if (Convert.ToInt32(drItem["leftHand"]) != 0)
                    newpc.LeftHand = Item.CopyItemFromDictionary(Convert.ToInt32(drItem["leftHand"]));
                // worn items
                string[] startingGear = drItem["wearing"].ToString().Split(" ".ToCharArray());
                foreach (string wItem in startingGear)
                {
                    int id = Convert.ToInt32(wItem);
                    if (id != 0)
                        newpc.wearing.Add(Item.CopyItemFromDictionary(id));
                }
                // belt items
                string[] startingBelt = drItem["belt"].ToString().Split(" ".ToCharArray());
                foreach (string bItem in startingBelt)
                {
                    int id = Convert.ToInt32(bItem);
                    if (id != 0)
                        newpc.BeltItem(Item.CopyItemFromDictionary(id));
                }
                // sack items
                //string[] startingSack = drItem["sack"].ToString().Split(" ".ToCharArray());
                //foreach (string sItem in startingSack)
                //{
                //    int id = Convert.ToInt32(sItem);
                //    if(id != 0)
                //        newpc.SackItem(Item.CopyItemFromDictionary(id));
                //}
                // starting spells
                if (drItem["spells"].ToString() != "0")
                {
                    String[] spellArray = drItem["spells"].ToString().Split(" ".ToCharArray());
                    string spellChant = "";
                    foreach (string spellID in spellArray)
                    {
                        spellChant = DragonsSpine.Spells.GameSpell.GenerateMagicWords();
                        while (newpc.spellDictionary.ContainsValue(spellChant))
                        {
                            spellChant = DragonsSpine.Spells.GameSpell.GenerateMagicWords();
                        }
                        newpc.spellDictionary.Add(Convert.ToInt32(spellID), spellChant);
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }
        #endregion
        #region Stores
        internal static int ClearStoreItems()
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Stores_Clear", conn);
                    return sp.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }
        internal static int RestockStoreItems()
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Stores_Restock", conn);
                    DataTable dtRestockStores = sp.ExecuteDataTable();
                    if (dtRestockStores == null) { return -1; }
                    else { return sp.ExecuteNonQuery(); }
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }
        internal static int UpdateStoreItem(int stockID, int stocked) // set a new stocked amount for this stockID
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Stores_Update", conn);
                    sp.AddParameter("@stockID", SqlDbType.Int, 4, ParameterDirection.Input, stockID);
                    sp.AddParameter("@stocked", SqlDbType.Int, 4, ParameterDirection.Input, stocked);
                    DataTable dtStockItem = sp.ExecuteDataTable();
                    if (dtStockItem == null)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }
        internal static int DeleteStoreItem(int stockID) // delete this stockID from a store
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Stores_Delete", conn);
                    sp.AddParameter("@stockID", SqlDbType.Int, 4, ParameterDirection.Input, stockID);
                    return sp.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }
        internal static int InsertStoreItem(int spawnZoneID, StoreItem storeItem)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Stores_Insert", conn);
                    sp.AddParameter("@spawnZoneID", SqlDbType.Int, 4, ParameterDirection.Input, spawnZoneID);
                    sp.AddParameter("@notes", SqlDbType.NVarChar, 255, ParameterDirection.Input, storeItem.notes);
                    sp.AddParameter("@original", SqlDbType.Bit, 1, ParameterDirection.Input, storeItem.original);
                    sp.AddParameter("@itemID", SqlDbType.Int, 4, ParameterDirection.Input, storeItem.itemID);
                    sp.AddParameter("@sellPrice", SqlDbType.Int, 4, ParameterDirection.Input, storeItem.sellPrice);
                    sp.AddParameter("@stocked", SqlDbType.SmallInt, 2, ParameterDirection.Input, storeItem.stocked);
                    sp.AddParameter("@charges", SqlDbType.SmallInt, 2, ParameterDirection.Input, storeItem.charges);
                    sp.AddParameter("@figExp", SqlDbType.BigInt, 8, ParameterDirection.Input, storeItem.figExp);
                    sp.AddParameter("@seller", SqlDbType.VarChar, 20, ParameterDirection.Input, storeItem.seller);
                    return sp.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }
        internal static List<StoreItem> LoadStoreItems(int spawnZoneID)
        {
            List<StoreItem> store = new List<StoreItem>();
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Stores_Select", conn);
                    sp.AddParameter("@spawnZoneID", SqlDbType.Int, 4, ParameterDirection.Input, spawnZoneID);
                    DataTable dtScoresItem = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtScoresItem.Rows)
                    {
                        store.Add(new StoreItem(dr));
                    }
                    return store;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }
        #endregion
        #region Scores
        internal static ArrayList p_getScores()
        {
            ArrayList scoresList = new ArrayList();
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    PC score = new PC();
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Scores_Select", conn);
                    sp.AddParameter("@devRequest", SqlDbType.Bit, 1, ParameterDirection.Input, true);
                    sp.AddParameter("@classType", SqlDbType.Int, 4, ParameterDirection.Input, 0);
                    DataTable dtScores = sp.ExecuteDataTable();
                    int max = ProtocolYuusha.MAX_SCORES;
                    foreach (DataRow dr in dtScores.Rows)
                    {
                        if (max <= 0) { return scoresList; }
                        score = new PC();
                        score = ConvertRowToScore(score, dr, true);
                        score.UniqueID = Convert.ToInt32(dr["playerID"]);
                        score.IsAnonymous = (bool)PC.GetField(score.UniqueID, "anonymous", score.IsAnonymous, null);
                        scoresList.Add(score);
                        max--;
                    }
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
            }
            return scoresList;
        }
        internal static List<PC> GetScores(Character.ClassType classType, int amount, bool devRequest, string playerName, bool pvp)
        {
            var scoresList = new List<PC>();
            var score = new PC();
            int rank = 1;
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Scores_Select", conn);
                    sp.AddParameter("@devRequest", SqlDbType.Bit, 1, ParameterDirection.Input, devRequest);
                    sp.AddParameter("@classType", SqlDbType.VarChar, 50, ParameterDirection.Input, classType.ToString());
                    DataTable dtScores = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtScores.Rows)
                    {
                        if (playerName != "")
                        {
                            if (dr["name"].ToString().ToLower() == playerName.ToString().ToLower())
                            {
                                score = new PC();
                                score.UniqueID = Convert.ToInt32(dr["playerID"]);
                                score.IsAnonymous = (bool)PC.GetField(score.UniqueID, "anonymous", score.IsAnonymous, null);
                                if (score.IsAnonymous)
                                {
                                    if (devRequest)
                                    {
                                        score.TemporaryStorage = rank;
                                        score = ConvertRowToScore(score, dr, devRequest);
                                        scoresList.Add(score);
                                    }
                                    return scoresList;
                                }
                                else
                                {
                                    score.TemporaryStorage = rank;
                                    score = ConvertRowToScore(score, dr, devRequest);
                                    scoresList.Add(score);
                                    return scoresList;
                                }
                            }
                            if (Convert.ToInt32(dr["impLevel"]) == 0 || score.Name.ToLower() == playerName.ToLower())
                            {
                                rank++;
                            }
                        }
                        else
                        {
                            if (amount == 0) { break; }
                            score = new PC();
                            score.UniqueID = Convert.ToInt32(dr["playerID"]);
                            score.IsAnonymous = (bool)PC.GetField(score.UniqueID, "anonymous", score.IsAnonymous, null);
                            if (devRequest) // devs get to see a list of everyone
                            {
                                score.TemporaryStorage = rank; // player ID is used to store the player's rank
                                score = ConvertRowToScore(score, dr, devRequest);
                                scoresList.Add(score);
                                if (!score.IsAnonymous && score.ImpLevel == Globals.eImpLevel.USER)
                                {
                                    amount--;
                                }
                            }
                            else // only add the score to the list if they are not anonymous
                            {
                                score.TemporaryStorage = rank;
                                score = ConvertRowToScore(score, dr, devRequest);
                                scoresList.Add(score);
                                amount--;
                            }
                            rank++;
                        }
                    }
                    return scoresList;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }
        internal static List<PC> GetScoresWithoutSP(Character.ClassType classType, int amount, bool devRequest, string playerName, bool pvp)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                var scoresList = new List<PC>();
                var score = new PC();
                int rank = 1;
                try
                {
                    var selectScores = new SqlCommand();
                    var selectByClassType = classType != Character.ClassType.None && classType != Character.ClassType.All ? "WHERE classType = '" + classType.ToString() + "'" : "";
                    var selectDevRequest = devRequest ? "1" : "0";
                    selectScores.Connection = conn;
                    selectScores.CommandText = "SELECT [playerID], land, [name], classType, classFullName, [level], [exp], numKills, roundsPlayed, lastOnline, pvpKills, pvpDeaths, currentKarma, impLevel";
                    selectScores.CommandText += " FROM Player " + selectByClassType;
                    if (!devRequest)
                    {
                        if (selectByClassType.Length > 0)
                            selectScores.CommandText += " AND impLevel = '0'";
                        else selectScores.CommandText += "WHERE impLevel = '0'";
                    }
                    selectScores.CommandText = selectScores.CommandText.TrimEnd();
                    selectScores.CommandText += " ORDER BY [exp] DESC";
                    DataTable dtScores = new DataTable();
                    selectScores.Connection.Open();
                    dtScores.Load(selectScores.ExecuteReader());
                    selectScores.Connection.Close();
                    foreach (DataRow dr in dtScores.Rows)
                    {
                        if (playerName != "")
                        {
                            if (dr["name"].ToString().ToLower() == playerName.ToString().ToLower())
                            {
                                score = new PC();
                                score.UniqueID = Convert.ToInt32(dr["playerID"]);
                                score.IsAnonymous = (bool)PC.GetField(score.UniqueID, "anonymous", score.IsAnonymous, null);
                                if (score.IsAnonymous)
                                {
                                    if (devRequest)
                                    {
                                        score.TemporaryStorage = rank;
                                        score = ConvertRowToScore(score, dr, devRequest);
                                        scoresList.Add(score);
                                    }
                                    return scoresList;
                                }
                                else
                                {
                                    score.TemporaryStorage = rank;
                                    score = ConvertRowToScore(score, dr, devRequest);
                                    scoresList.Add(score);
                                    return scoresList;
                                }
                            }
                            if (Convert.ToInt32(dr["impLevel"]) == 0 || score.Name.ToLower() == playerName.ToLower())
                            {
                                rank++;
                            }
                        }
                        else
                        {
                            if (amount == 0) { break; }
                            score = new PC();
                            score.UniqueID = Convert.ToInt32(dr["playerID"]);
                            score.IsAnonymous = (bool)PC.GetField(score.UniqueID, "anonymous", score.IsAnonymous, null);
                            if (devRequest) // devs get to see a list of everyone
                            {
                                score.TemporaryStorage = rank; // player ID is used to store the player's rank
                                score = ConvertRowToScore(score, dr, devRequest);
                                scoresList.Add(score);
                                if (!score.IsAnonymous && score.ImpLevel == Globals.eImpLevel.USER)
                                {
                                    amount--;
                                }
                            }
                            else // only add the score to the list if they are not anonymous
                            {
                                score.TemporaryStorage = rank;
                                score = ConvertRowToScore(score, dr, devRequest);
                                scoresList.Add(score);
                                amount--;
                            }
                            rank++;
                        }
                    }
                    return scoresList;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
            }
            return null;
        }
        internal static PC ConvertRowToScore(PC score, DataRow dr, bool devRequest)
        {
            try
            {
                if (dr != null)
                {
                    if (!score.IsAnonymous || devRequest)
                    {
                        score.Name = dr["name"].ToString();
                        score.classFullName = dr["classFullName"].ToString();
                        score.BaseProfession = (Character.ClassType)Enum.Parse(typeof(Character.ClassType), dr["classType"].ToString(), true);
                        score.Level = Convert.ToInt16(dr["level"]);
                        score.Experience = Convert.ToInt64(dr["exp"]);
                        score.Kills = Convert.ToInt32(dr["numKills"]);
                        score.RoundsPlayed = Convert.ToInt64(dr["roundsPlayed"]);
                        score.lastOnline = Convert.ToDateTime(dr["lastOnline"]);
                        score.pvpNumDeaths = Convert.ToInt64(dr["pvpDeaths"]);
                        score.pvpNumKills = Convert.ToInt64(dr["pvpKills"]);
                        score.ImpLevel = (Globals.eImpLevel)Convert.ToInt32(dr["impLevel"]);
                        score.LandID = Convert.ToInt16(dr["land"]);
                        score.currentKarma = Convert.ToInt32(dr["currentKarma"]);
                    }
                    else
                    {
                        score.Name = "--";
                        score.classFullName = dr["classFullName"].ToString();
                        score.BaseProfession = (Character.ClassType)Enum.Parse(typeof(Character.ClassType), dr["classType"].ToString(), true);
                        score.Level = Convert.ToInt16(dr["level"]);
                        score.Experience = Convert.ToInt64(dr["exp"]);
                        score.Kills = Convert.ToInt32(dr["numKills"]);
                        score.RoundsPlayed = Convert.ToInt64(dr["roundsPlayed"]);
                        score.lastOnline = Convert.ToDateTime(dr["lastOnline"]);
                        score.pvpNumDeaths = Convert.ToInt64(dr["pvpDeaths"]);
                        score.pvpNumKills = Convert.ToInt64(dr["pvpKills"]);
                        score.ImpLevel = Globals.eImpLevel.USER;
                        score.LandID = Convert.ToInt16(dr["land"]);
                        score.currentKarma = Convert.ToInt32(dr["currentKarma"]);
                    }
                }
                return score;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return null;
            }
        }
        #endregion
        internal static bool LoadSpawnZones()
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_SpawnZone_Select_All", conn);
                    DataTable dtCatalogItem = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtCatalogItem.Rows)
                    {
                        SpawnZone.Add(new SpawnZone(dr));
                    }
                    Utils.Log("Loaded SpawnZones (" + SpawnZone.Spawns.Count + ")", Utils.LogType.SystemGo);
                    return true;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return false;
                }
            }
        }
        internal static ArrayList GetSpawnLinksByMap(int map)
        {
            ArrayList zones = new ArrayList();
            using (var conn = DataAccess.GetSQLConnection())
            {
                SqlStoredProcedure sp = new SqlStoredProcedure("prApp_SpawnZone_Select_By_Map", conn);
                sp.AddParameter("@map", SqlDbType.Int, 4, ParameterDirection.Input, map);
                DataTable szTable = sp.ExecuteDataTable();
                foreach (DataRow dr in szTable.Rows)
                {
                    zones.Add(new SpawnZone(dr));
                }
            }
            return zones;
        }
        internal static ArrayList loadBannedIPList()
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    ArrayList bannedIPList = new ArrayList();
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_BannedIP_Select", conn);
                    DataTable dtBannedIPList = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtBannedIPList.Rows)
                    {
                        bannedIPList.Add(dr["bannedIP"].ToString());
                    }
                    return bannedIPList;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }
        internal static int SaveLottery(Land land)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Land_Update_Lottery", conn);
                    sp.AddParameter("@landID", SqlDbType.Int, 4, ParameterDirection.Input, land.LandID);
                    sp.AddParameter("@lottery", SqlDbType.BigInt, 8, ParameterDirection.Input, land.Lottery);
                    sp.AddParameter("@lotteryParticipants", SqlDbType.VarChar, 255, ParameterDirection.Input, Utils.ConvertListToString(land.LotteryParticipants).Trim());
                    DataTable dtLand = sp.ExecuteDataTable();
                    if (dtLand == null)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }
        internal static bool CellExistsInDatabase(string mapName, int x, int y, int z)
        {
            try
            {
                foreach (Cell cell in DBWorld.GetCellList(mapName))
                {
                    if (cell.X == x && cell.Y == y && cell.Z == z)
                        return true;
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return true;
            }
            return false;
        }
        internal static int InsertCellWithDefaultValues(string mapName, int x, int y, int z, PC developer)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    if (!CellExistsInDatabase(mapName, x, y, z))
                    {
                        #region Default values.
                        var notes = CELL_INSERT_NOTES_DEFAULT;
                        // no segue is added, NULL value
                        // all other values are bit 0 (false)
                        #endregion
                        var insertCell = new SqlCommand();
                        insertCell.CommandText =
                            "INSERT Cell ([notes], [mapName] ,[xCord], [yCord] ,[zCord], [portal], [pvpEnabled], [singleCustomer], [teleport], [mailbox])" +
                            " VALUES ('" +
                            notes + "', '" + mapName + "', '" + x + "', '" + y + "', '" + z + "', '0', '0', '0','0','0')";
                        insertCell.Connection = conn;
                        insertCell.Connection.Open();
                        var returnValue = insertCell.ExecuteNonQuery();
                        insertCell.Connection.Close();
                        string logString = "Insertion of new default cell for MapName: " + mapName + " Cell X: " + x + " Cell Y: " + y + " Cell Z: " + z + " by " + developer == null ? "Cell Builder" : developer.GetLogString() + ".";
                        Utils.Log(logString, Utils.LogType.DatabaseQuery);
                        return returnValue;
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