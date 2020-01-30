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
using System.Collections.Generic;

namespace DragonsSpine.DAL
{
    /// <summary>
    /// All the various database routines for saving/loading the account and player information.
    /// If you're tracking calling functions, all come from either IO.cs, PC.cs and CharGen.cs- Chip
    /// </summary>
    internal static class DBPlayer
    {
        // ***********************
        // CHARACTER/PLAYER ROUTINES
        // ***********************
        internal static string GetPlayerTableName(int playerID, string field) // gets the table name that contains player field
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    string[] tableNames = { "Player", "PlayerSettings" };

                    for (int a = 0; a < tableNames.Length; a++)
                    {
                        SqlStoredProcedure sp = new SqlStoredProcedure("prApp_" + tableNames[a] + "_Select", conn);
                        sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                        DataTable dt = sp.ExecuteDataTable();

                        foreach (DataColumn dc in dt.Columns)
                        {
                            if (dc.ColumnName == field) { return tableNames[a]; }
                        }
                    }
                    Utils.Log("DBPlayer.GetPlayerTableName(" + playerID + ", " + field + ")  Field was not found in any player table.", Utils.LogType.SystemFailure);
                    return "error";

                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return "error";
                }
            }
        }

        internal static Object GetPlayerField(int playerID, string field, Type objectType) // return player field object
        {
            // objectTypes: "System.Int32", "System.String", "System.Boolean", "System.Char" , "System.DateTime"
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Player_Select_Field", conn);
                    sp.AddParameter("@table", SqlDbType.NVarChar, 50, ParameterDirection.Input, GetPlayerTableName(playerID, field));
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    sp.AddParameter("@field", SqlDbType.NVarChar, 50, ParameterDirection.Input, field);
                    DataTable dtPlayer = sp.ExecuteDataTable();

                    if (dtPlayer == null || dtPlayer.Rows.Count < 1)
                        return null;

                    foreach (DataRow dr in dtPlayer.Rows)
                    {
                        if (objectType == null)
                            return dr[field].ToString();

                        switch (objectType.ToString())
                        {
                            case "System.Int16":
                                return Convert.ToInt16(dr[field]);
                            case "System.Int32":
                                return Convert.ToInt32(dr[field]);
                            case "System.Int64":
                                return Convert.ToInt64(dr[field]);
                            case "System.String":
                                return dr[field].ToString();
                            case "System.Boolean":
                                return Convert.ToBoolean(dr[field]);
                            case "System.Char":
                                return Convert.ToChar(dr[field]);
                            case "System.DateTime":
                                return Convert.ToDateTime(dr[field]);
                            case "System.Double":
                                return Convert.ToDouble(dr[field]);
                            case "Character.SkillType":
                                return dr[field].ToString();
                            default:
                                Utils.Log("DBPlayer.getPlayerField(" + playerID + ", " + field + ", " + objectType.ToString() + ") Unable to find " + objectType.ToString() + " in switch.", Utils.LogType.SystemFailure);
                                break;
                        }
                    }
                    return null;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        internal static bool PlayerExists(string name) // searches DB to see if character exists
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Player_Check", conn);
                    sp.AddParameter("@name", SqlDbType.NVarChar, 20, ParameterDirection.Input, name);
                    DataTable dtAccountItem = sp.ExecuteDataTable();

                    if (dtAccountItem == null || dtAccountItem.Rows.Count < 1)
                        return false;
                    else
                        return true;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return false;
                }
            }
        }

        internal static int GetPlayerID(string name) // gets the PlayerID (generated by DB) from a player's name
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Player_Select_By_Name", conn);
                    sp.AddParameter("@name", SqlDbType.NVarChar, 255, ParameterDirection.Input, name);
                    DataTable dtAccountItem = sp.ExecuteDataTable();

                    if (dtAccountItem == null || dtAccountItem.Rows.Count < 1)
                        return -1;

                    int i = 0;
                    foreach (DataRow dr in dtAccountItem.Rows)
                    {
                        i = Convert.ToInt16(dr["PlayerID"]);
                        return i;
                    }
                    return -1;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }

        internal static bool DeletePlayerFromDatabase(int playerID) // delete a character
        {
            try
            {
                if (!new Mail.GameMail.GameMailbox(playerID).DeleteMailbox())
                    return false;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                Utils.Log("Failure while deleting Mailbox for playerID " + playerID + ".", Utils.LogType.SystemFailure);
                return false;
            }

            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    bool flag = false;
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_PlayerSack_Delete", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    if (sp.ExecuteNonQuery() == -1)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                    sp = new SqlStoredProcedure("prApp_PlayerLocker_Delete", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    if (sp.ExecuteNonQuery() == -1)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                    sp = new SqlStoredProcedure("prApp_PlayerWearing_Delete", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    if (sp.ExecuteNonQuery() == -1)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                    sp = new SqlStoredProcedure("prApp_PlayerRings_Delete", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    if (sp.ExecuteNonQuery() == -1)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                    sp = new SqlStoredProcedure("prApp_PlayerSpells_Delete", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    if (sp.ExecuteNonQuery() == -1)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                    sp = new SqlStoredProcedure("prApp_PlayerBelt_Delete", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    if (sp.ExecuteNonQuery() == -1)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                    sp = new SqlStoredProcedure("prApp_PlayerEffects_Delete", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    if (sp.ExecuteNonQuery() == -1)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                    sp = new SqlStoredProcedure("prApp_PlayerHeld_Delete", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    if (sp.ExecuteNonQuery() == -1)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                    sp = new SqlStoredProcedure("prApp_PlayerFlags_Delete", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    if (sp.ExecuteNonQuery() == -1)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                    sp = new SqlStoredProcedure("prApp_PlayerQuests_Delete", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    if (sp.ExecuteNonQuery() == -1)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                    sp = new SqlStoredProcedure("prApp_PlayerSkills_Delete", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    if (sp.ExecuteNonQuery() == -1)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                    sp = new SqlStoredProcedure("prApp_PlayerSettings_Delete", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    if (sp.ExecuteNonQuery() == -1)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                    sp = new SqlStoredProcedure("prApp_Player_Delete", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    if (sp.ExecuteNonQuery() == -1)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                    return flag;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return false;
                }
            }
        }

        internal static int GetCharactersCount(int accountID) // returns how many characters the account has in the DB
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Player_Select_By_Account", conn);
                    sp.AddParameter("@accountID", SqlDbType.Int, 4, ParameterDirection.Input, accountID);
                    DataTable dtPlayerItem = sp.ExecuteDataTable();
                    return dtPlayerItem.Rows.Count;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }

        internal static void p_sendCharacterList(int accountID, Character currentCharacter) // protocol send character list
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Player_Select_By_Account", conn);
                    sp.AddParameter("@accountID", SqlDbType.Int, 4, ParameterDirection.Input, accountID);
                    DataTable dtPlayerItem = sp.ExecuteDataTable();
                    PC pc;
                    DataRow dr;

                    currentCharacter.Write(ProtocolYuusha.CHARACTER_LIST);
                    for (int i = 0; i < dtPlayerItem.Rows.Count; i++)
                    {
                        dr = dtPlayerItem.Rows[i];
                        pc = PC.GetPC((Convert.ToInt16(dr["playerID"])));
                        ProtocolYuusha.SendCharacterStats(pc, currentCharacter);
                        ProtocolYuusha.SendCharacterRightHand(pc, currentCharacter);
                        ProtocolYuusha.SendCharacterLeftHand(pc, currentCharacter);
                        ProtocolYuusha.SendCharacterMacros(pc, currentCharacter as PC);
                        ProtocolYuusha.SendCharacterSpells(pc, currentCharacter);
                        ProtocolYuusha.SendCharacterTalents(pc, currentCharacter);
                        if (i < dtPlayerItem.Rows.Count - 1) { currentCharacter.Write(ProtocolYuusha.CHARACTER_LIST_SPLIT); }
                    }
                    currentCharacter.Write(ProtocolYuusha.CHARACTER_LIST_END);
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
            }
        }

        internal static string[] GetCharacterList(string field, int accountID) // build a list of playerID's on an account
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    int i = 0;
                    string[] charlist = new string[Character.MAX_CHARS];
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Player_Select_By_Account", conn);
                    sp.AddParameter("@accountID", SqlDbType.Int, 4, ParameterDirection.Input, accountID);
                    DataTable dtPlayerItem = sp.ExecuteDataTable();

                    foreach (DataRow dr in dtPlayerItem.Rows)
                    {
                        charlist[i] = dr[field].ToString();
                        i++;
                    }
                    return charlist;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        internal static string[] GetPlayerTableColumnNames(int playerID)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    string[] columns;
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Player_Select", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    DataTable dtPlayer = sp.ExecuteDataTable();
                    columns = new string[dtPlayer.Columns.Count];
                    int a = 0;
                    foreach (DataColumn dc in dtPlayer.Columns)
                    {
                        columns[a] = dc.ColumnName;
                        a++;
                    }
                    return columns;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        // ***********************
        // SAVE CHARACTER ROUTINES
        // ***********************
        internal static int SavePlayerField(int playerID, string field, object var)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Player_Update_Field", conn);
                    sp.AddParameter("@table", SqlDbType.NVarChar, 50, ParameterDirection.Input, GetPlayerTableName(playerID, field));
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    sp.AddParameter("@field", SqlDbType.NVarChar, 50, ParameterDirection.Input, field);
                    sp.AddParameter("@type", SqlDbType.NVarChar, 50, ParameterDirection.Input, var.GetType().ToString());

                    if (var.GetType().Equals(Type.GetType("System.Int16")))
                    {
                        sp.AddParameter("@short", SqlDbType.SmallInt, 2, ParameterDirection.Input, var);
                    }
                    else if (var.GetType().Equals(Type.GetType("System.Double")))
                    {
                        sp.AddParameter("@float", SqlDbType.Float, 0, ParameterDirection.Input, var);
                    }
                    else if (var.GetType().Equals(Type.GetType("System.Int32")))
                    {
                        sp.AddParameter("@int", SqlDbType.Int, 4, ParameterDirection.Input, var);
                    }
                    else if (var.GetType().Equals(Type.GetType("System.Int64")))
                    {
                        sp.AddParameter("@long", SqlDbType.BigInt, 8, ParameterDirection.Input, var);
                    }
                    else if (var.GetType().Equals(Type.GetType("System.String")))
                    {
                        sp.AddParameter("@string", SqlDbType.NVarChar, 4000, ParameterDirection.Input, var);
                    }
                    else if (var.GetType().Equals(Type.GetType("System.Boolean")))
                    {
                        sp.AddParameter("@bit", SqlDbType.Bit, 1, ParameterDirection.Input, var);
                    }
                    else if (var.GetType().Equals(Type.GetType("System.Char")))
                    {
                        sp.AddParameter("@char", SqlDbType.Char, 1, ParameterDirection.Input, var);
                    }
                    else if (var.GetType().Equals(Type.GetType("System.DateTime")))
                    {
                        sp.AddParameter("@dateTime", SqlDbType.DateTime, 8, ParameterDirection.Input, var);
                    }
                    else
                    {
                        Utils.Log("DBPlayer.savePlayerProperty(" + playerID + ", " + field + ", " + var.GetType().ToString() + ") *Type Not Recognized*, " + var.GetType().ToString(), Utils.LogType.SystemFailure);
                        return -1;
                    }

                    DataTable dtPlayer = sp.ExecuteDataTable();

                    if (dtPlayer == null)
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

        // save a single PC field
        internal static int SavePlayerStats(PC pc)
        {
            //string sptouse = "";

            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    //if (pc.IsNewPC)
                    //    sptouse = "prApp_Player_Insert";  // If new char, use INSERT for a new row.
                    //else
                    //    sptouse = "prApp_Player_Update";  // If saving old character, UPDATE an existing row.

                    SqlStoredProcedure sp = new SqlStoredProcedure(pc.IsNewPC ? "prApp_Player_Insert" : "prApp_Player_Update", conn);

                    if (pc.IsNewPC)
                        sp.AddParameter("@accountID", SqlDbType.Int, 4, ParameterDirection.Input, pc.Account.accountID);
                    else
                        sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);

                    sp.AddParameter("@notes", SqlDbType.Text, System.Int32.MaxValue, ParameterDirection.Input, pc.Notes);
                    sp.AddParameter("@account", SqlDbType.NVarChar, 20, ParameterDirection.Input, pc.Account.accountName);
                    sp.AddParameter("@name", SqlDbType.NVarChar, 255, ParameterDirection.Input, pc.Name);
                    sp.AddParameter("@gender", SqlDbType.Int, 4, ParameterDirection.Input, pc.gender);
                    sp.AddParameter("@race", SqlDbType.NVarChar, 20, ParameterDirection.Input, pc.race);
                    sp.AddParameter("@classFullName", SqlDbType.NVarChar, 15, ParameterDirection.Input, pc.classFullName);
                    sp.AddParameter("@classType", SqlDbType.VarChar, 50, ParameterDirection.Input, pc.BaseProfession.ToString());
                    sp.AddParameter("@visualKey", SqlDbType.VarChar, 50, ParameterDirection.Input, pc.visualKey);
                    sp.AddParameter("@alignment", SqlDbType.Int, 4, ParameterDirection.Input, (int)pc.Alignment);
                    sp.AddParameter("@confRoom", SqlDbType.Int, 4, ParameterDirection.Input, pc.confRoom);
                    sp.AddParameter("@impLevel", SqlDbType.Int, 4, ParameterDirection.Input, (int)pc.ImpLevel);
                    sp.AddParameter("@ancestor", SqlDbType.Bit, 1, ParameterDirection.Input, pc.IsAncestor);
                    sp.AddParameter("@ancestorID", SqlDbType.Bit, 1, ParameterDirection.Input, pc.AncestorID);
                    //sp.AddParameter("@facet", SqlDbType.SmallInt, 0, ParameterDirection.Input, pc.FacetID);
                    sp.AddParameter("@land", SqlDbType.Int, 4, ParameterDirection.Input, pc.LandID);
                    sp.AddParameter("@map", SqlDbType.Int, 4, ParameterDirection.Input, pc.MapID);
                    sp.AddParameter("@xCord", SqlDbType.Int, 4, ParameterDirection.Input, pc.X);
                    sp.AddParameter("@yCord", SqlDbType.Int, 4, ParameterDirection.Input, pc.Y);
                    sp.AddParameter("@zCord", SqlDbType.Int, 4, ParameterDirection.Input, pc.Z);
                    sp.AddParameter("@dirPointer", SqlDbType.Char, 1, ParameterDirection.Input, pc.dirPointer);
                    sp.AddParameter("@stunned", SqlDbType.Int, 4, ParameterDirection.Input, pc.Stunned);
                    sp.AddParameter("@floating", SqlDbType.Int, 4, ParameterDirection.Input, pc.floating);
                    sp.AddParameter("@dead", SqlDbType.Bit, 1, ParameterDirection.Input, pc.IsDead);
                    sp.AddParameter("@fighterSpecialization", SqlDbType.VarChar, 50, ParameterDirection.Input, pc.fighterSpecialization.ToString());
                    sp.AddParameter("@level", SqlDbType.Int, 4, ParameterDirection.Input, pc.Level);
                    sp.AddParameter("@exp", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.Experience);
                    sp.AddParameter("@hits", SqlDbType.Int, 4, ParameterDirection.Input, pc.Hits);
                    sp.AddParameter("@hitsMax", SqlDbType.Int, 4, ParameterDirection.Input, pc.HitsMax);
                    sp.AddParameter("@hitsAdjustment", SqlDbType.Int, 4, ParameterDirection.Input, pc.HitsAdjustment);
                    sp.AddParameter("@hitsDoctored", SqlDbType.Int, 4, ParameterDirection.Input, pc.HitsDoctored);
                    sp.AddParameter("@stamina", SqlDbType.Int, 4, ParameterDirection.Input, pc.StaminaMax);
                    sp.AddParameter("@stamLeft", SqlDbType.Int, 4, ParameterDirection.Input, pc.Stamina);
                    sp.AddParameter("@staminaAdjustment", SqlDbType.Int, 4, ParameterDirection.Input, pc.StaminaAdjustment);
                    sp.AddParameter("@mana", SqlDbType.Int, 4, ParameterDirection.Input, pc.Mana);
                    sp.AddParameter("@manaMax", SqlDbType.Int, 4, ParameterDirection.Input, pc.ManaMax);
                    sp.AddParameter("@manaAdjustment", SqlDbType.Int, 4, ParameterDirection.Input, pc.ManaAdjustment);
                    sp.AddParameter("@age", SqlDbType.Int, 4, ParameterDirection.Input, pc.Age);
                    sp.AddParameter("@roundsPlayed", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.RoundsPlayed);
                    sp.AddParameter("@numKills", SqlDbType.Int, 4, ParameterDirection.Input, pc.Kills);
                    sp.AddParameter("@numDeaths", SqlDbType.Int, 4, ParameterDirection.Input, pc.Deaths);
                    sp.AddParameter("@bankGold", SqlDbType.Float, 8, ParameterDirection.Input, pc.bankGold);
                    sp.AddParameter("@strength", SqlDbType.Int, 4, ParameterDirection.Input, pc.Strength);
                    sp.AddParameter("@dexterity", SqlDbType.Int, 4, ParameterDirection.Input, pc.Dexterity);
                    sp.AddParameter("@intelligence", SqlDbType.Int, 4, ParameterDirection.Input, pc.Intelligence);
                    sp.AddParameter("@wisdom", SqlDbType.Int, 4, ParameterDirection.Input, pc.Wisdom);
                    sp.AddParameter("@constitution", SqlDbType.Int, 4, ParameterDirection.Input, pc.Constitution);
                    sp.AddParameter("@charisma", SqlDbType.Int, 4, ParameterDirection.Input, pc.Charisma);
                    sp.AddParameter("@strengthAdd", SqlDbType.Int, 4, ParameterDirection.Input, pc.strengthAdd);
                    sp.AddParameter("@dexterityAdd", SqlDbType.Int, 4, ParameterDirection.Input, pc.dexterityAdd);
                    if (pc.IsNewPC)
                    {
                        pc.birthday = DateTime.UtcNow;
                        sp.AddParameter("@birthday", SqlDbType.DateTime, 8, ParameterDirection.Input, pc.birthday);
                    }
                    pc.lastOnline = DateTime.UtcNow;
                    sp.AddParameter("@lastOnline", SqlDbType.DateTime, 8, ParameterDirection.Input, pc.lastOnline);
                    // Underworld specific
                    sp.AddParameter("@UW_hitsMax", SqlDbType.Int, 4, ParameterDirection.Input, pc.UW_hitsMax);
                    sp.AddParameter("@UW_hitsAdjustment", SqlDbType.Int, 4, ParameterDirection.Input, pc.UW_hitsAdjustment);
                    sp.AddParameter("@UW_staminaMax", SqlDbType.Int, 4, ParameterDirection.Input, pc.UW_staminaMax);
                    sp.AddParameter("@UW_staminaAdjustment", SqlDbType.Int, 4, ParameterDirection.Input, pc.UW_staminaAdjustment);
                    sp.AddParameter("@UW_manaMax", SqlDbType.Int, 4, ParameterDirection.Input, pc.UW_manaMax);
                    sp.AddParameter("@UW_manaAdjustment", SqlDbType.Int, 4, ParameterDirection.Input, pc.UW_manaAdjustment);
                    sp.AddParameter("@UW_intestines", SqlDbType.Bit, 1, ParameterDirection.Input, pc.UW_hasIntestines);
                    sp.AddParameter("@UW_liver", SqlDbType.Bit, 1, ParameterDirection.Input, pc.UW_hasLiver);
                    sp.AddParameter("@UW_lungs", SqlDbType.Bit, 1, ParameterDirection.Input, pc.UW_hasLungs);
                    sp.AddParameter("@UW_stomach", SqlDbType.Bit, 1, ParameterDirection.Input, pc.UW_hasStomach);
                    // Player vs. Player
                    sp.AddParameter("@currentKarma", SqlDbType.Int, 4, ParameterDirection.Input, pc.currentKarma);
                    sp.AddParameter("@lifetimeKarma", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.lifetimeKarma);
                    sp.AddParameter("@lifetimeMarks", SqlDbType.Int, 4, ParameterDirection.Input, pc.lifetimeMarks);
                    sp.AddParameter("@pvpDeaths", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.pvpNumDeaths);
                    sp.AddParameter("@pvpKills", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.pvpNumKills);
                    sp.AddParameter("@playersFlagged", SqlDbType.NVarChar, 1000, ParameterDirection.Input, Utils.ConvertListToString(pc.FlaggedUniqueIDs));
                    sp.AddParameter("@playersKilled", SqlDbType.NVarChar, 4000, ParameterDirection.Input, Utils.ConvertListToString(pc.PlayersKilled));

                    if (pc.IsNewPC)
                    {
                        Utils.Log("Inserting new player, Account: " + pc.Account.accountName + "  Player: " + pc.Name, Utils.LogType.NewPlayerCreation);

                        return sp.ExecuteNonQuery();
                    }
                    else
                    {
                        DataTable dtPlayerStats = sp.ExecuteDataTable();

                        if (dtPlayerStats == null)
                            return -1;
                        else
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

        // save the main part of a PC record
        // save the player's settings
        internal static int SavePlayerSettings(PC pc)
        {
            //string sptouse;

            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    //if (pc.IsNewPC)
                    //    sptouse = "prApp_PlayerSettings_Insert";  // if new char, use INSERT for a new row
                    //else
                    //    sptouse = "prApp_PlayerSettings_Update";  // if saving old character, UPDATE an existing row

                    SqlStoredProcedure sp = new SqlStoredProcedure(pc.IsNewPC ? "prApp_PlayerSettings_Insert" : "prApp_PlayerSettings_Update", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                    sp.AddParameter("@anonymous", SqlDbType.Bit, 1, ParameterDirection.Input, pc.IsAnonymous);
                    sp.AddParameter("@echo", SqlDbType.Bit, 1, ParameterDirection.Input, pc.echo);
                    sp.AddParameter("@filterProfanity", SqlDbType.Bit, 1, ParameterDirection.Input, pc.filterProfanity);
                    sp.AddParameter("@friendsList", SqlDbType.NVarChar, 4000, ParameterDirection.Input, Utils.ConvertIntArrayToString(pc.friendsList));
                    sp.AddParameter("@friendNotify", SqlDbType.Bit, 1, ParameterDirection.Input, pc.friendNotify);
                    sp.AddParameter("@ignoreList", SqlDbType.NVarChar, 4000, ParameterDirection.Input, Utils.ConvertIntArrayToString(pc.ignoreList));
                    sp.AddParameter("@immortal", SqlDbType.Bit, 1, ParameterDirection.Input, pc.IsImmortal);
                    sp.AddParameter("@invisible", SqlDbType.Bit, 1, ParameterDirection.Input, pc.IsInvisible);
                    sp.AddParameter("@receiveGroupInvites", SqlDbType.Bit, 1, ParameterDirection.Input, pc.receiveGroupInvites);
                    sp.AddParameter("@receivePages", SqlDbType.Bit, 1, ParameterDirection.Input, pc.receivePages);
                    sp.AddParameter("@receiveTells", SqlDbType.Bit, 1, ParameterDirection.Input, pc.receiveTells);
                    sp.AddParameter("@showStaffTitle", SqlDbType.Bit, 1, ParameterDirection.Input, pc.showStaffTitle);
                    string macrosString = "";
                    if (pc.macros.Count > 0)
                    {
                        for (int a = 0; a < pc.macros.Count; a++)
                        {
                            macrosString += pc.macros[a].ToString() + ProtocolYuusha.ISPLIT;
                        }
                        macrosString = macrosString.Substring(0, macrosString.Length - ProtocolYuusha.ISPLIT.Length);
                    }
                    sp.AddParameter("@macros", SqlDbType.NVarChar, 4000, ParameterDirection.Input, macrosString);
                    sp.AddParameter("@displayCombatDamage", SqlDbType.Bit, 1, ParameterDirection.Input, pc.DisplayCombatDamage);
                    sp.AddParameter("@displayPetDamage", SqlDbType.Bit, 1, ParameterDirection.Input, pc.DisplayPetDamage);
                    sp.AddParameter("@displayPetMessages", SqlDbType.Bit, 1, ParameterDirection.Input, pc.DisplayPetMessages);
                    sp.AddParameter("@displayDamageShield", SqlDbType.Bit, 1, ParameterDirection.Input, pc.DisplayDamageShield);
                    if (pc.IsNewPC)
                    {
                        return sp.ExecuteNonQuery();
                    }
                    else
                    {
                        DataTable dtPlayerSettings = sp.ExecuteDataTable();

                        if (dtPlayerSettings == null)
                            return -1;
                        else
                            return 1;
                    }
                }
                catch (Exception e)
                {
                    conn.Close();
                    Utils.LogException(e);
                    return -1;
                }
            }
        }

        // save the player's skills
        internal static int SavePlayerSkills(PC pc)
        {
            //String sptouse;
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    //if (pc.IsNewPC)
                    //{
                    //    sptouse = "prApp_PlayerSkills_Insert";  // if new char, use INSERT for a new row
                    //}
                    //else
                    //{
                    //    sptouse = "prApp_PlayerSkills_Update";  // if saving old character, UPDATE an existing row
                    //}

                    SqlStoredProcedure sp = new SqlStoredProcedure(pc.IsNewPC ? "prApp_PlayerSkills_Insert" : "prApp_PlayerSkills_Update", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                    sp.AddParameter("@mace", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.mace);
                    sp.AddParameter("@bow", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.bow);
                    sp.AddParameter("@flail", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.flail);
                    sp.AddParameter("@dagger", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.dagger);
                    sp.AddParameter("@rapier", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.rapier);
                    sp.AddParameter("@twoHanded", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.twoHanded);
                    sp.AddParameter("@staff", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.staff);
                    sp.AddParameter("@shuriken", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.shuriken);
                    sp.AddParameter("@sword", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.sword);
                    sp.AddParameter("@threestaff", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.threestaff);
                    sp.AddParameter("@halberd", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.halberd);
                    sp.AddParameter("@unarmed", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.unarmed);
                    sp.AddParameter("@thievery", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.thievery);
                    sp.AddParameter("@magic", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.magic);
                    sp.AddParameter("@bash", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.bash);

                    sp.AddParameter("@highMace", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.highMace);
                    sp.AddParameter("@highBow", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.highBow);
                    sp.AddParameter("@highFlail", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.highFlail);
                    sp.AddParameter("@highDagger", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.highDagger);
                    sp.AddParameter("@highRapier", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.highRapier);
                    sp.AddParameter("@highTwoHanded", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.highTwoHanded);
                    sp.AddParameter("@highStaff", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.highStaff);
                    sp.AddParameter("@highShuriken", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.highShuriken);
                    sp.AddParameter("@highSword", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.highSword);
                    sp.AddParameter("@highThreestaff", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.highThreestaff);
                    sp.AddParameter("@highHalberd", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.highHalberd);
                    sp.AddParameter("@highUnarmed", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.highUnarmed);
                    sp.AddParameter("@highThievery", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.highThievery);
                    sp.AddParameter("@highMagic", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.highMagic);
                    sp.AddParameter("@highBash", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.highBash);

                    sp.AddParameter("@trainedMace", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.trainedMace);
                    sp.AddParameter("@trainedBow", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.trainedBow);
                    sp.AddParameter("@trainedFlail", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.trainedFlail);
                    sp.AddParameter("@trainedDagger", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.trainedDagger);
                    sp.AddParameter("@trainedRapier", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.trainedRapier);
                    sp.AddParameter("@trainedTwoHanded", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.trainedTwoHanded);
                    sp.AddParameter("@trainedStaff", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.trainedStaff);
                    sp.AddParameter("@trainedShuriken", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.trainedShuriken);
                    sp.AddParameter("@trainedSword", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.trainedSword);
                    sp.AddParameter("@trainedThreestaff", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.trainedThreestaff);
                    sp.AddParameter("@trainedHalberd", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.trainedHalberd);
                    sp.AddParameter("@trainedUnarmed", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.trainedUnarmed);
                    sp.AddParameter("@trainedThievery", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.trainedThievery);
                    sp.AddParameter("@trainedMagic", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.trainedMagic);
                    sp.AddParameter("@trainedBash", SqlDbType.BigInt, 8, ParameterDirection.Input, pc.trainedBash);
                    if (pc.IsNewPC)
                    {
                        int val = sp.ExecuteNonQuery();
                        return val;
                    }
                    else
                    {
                        DataTable dtPlayerSkills = sp.ExecuteDataTable();

                        if (dtPlayerSkills == null)
                            return -1;
                        else
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

        internal static int SavePlayerHeld(PC pc) // save the player's held items
        {
            //string sptouse;
            int result = 0;
            bool rightHand = false;
            int itemID = 0;
            int attunedID = 0;
            string special = "";
            double coinValue = 0;
            int venom = 0;
            bool nocked = false;
            int charges = -1;
            int attuneType = (int)Globals.eAttuneType.None;
            double figExp = 0;
            DateTime timeCreated = DateTime.UtcNow;
            string whoCreated = "SYSTEM";
            //SqlStoredProcedure sp;

            //if (pc.IsNewPC)
            //{
            //    sptouse = "prApp_PlayerHeld_Insert";  // If new char, we insert a new row.
            //}
            //else
            //{
            //    sptouse = "prApp_PlayerHeld_Update";  // If saving old character, we update an existing row.
            //}

            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    for (int a = 0; a < 2; a++)
                    {
                        if (!rightHand && pc.LeftHand != null)
                        {
                            itemID = pc.LeftHand.itemID;
                            attunedID = pc.LeftHand.attunedID;
                            special = pc.LeftHand.special;
                            coinValue = pc.LeftHand.coinValue;
                            charges = pc.LeftHand.charges;
                            venom = pc.LeftHand.venom;
                            attuneType = (int)pc.LeftHand.attuneType;
                            figExp = pc.LeftHand.figExp;
                            nocked = pc.LeftHand.IsNocked;
                            timeCreated = pc.LeftHand.timeCreated;
                            whoCreated = pc.LeftHand.whoCreated;
                        }
                        else if (!rightHand && pc.LeftHand == null)
                        {
                            itemID = 0;
                            attunedID = 0;
                            special = "";
                            coinValue = 0;
                            venom = 0;
                            charges = -1;
                            attuneType = (int)Globals.eAttuneType.None;
                            figExp = 0;
                            nocked = false;
                            timeCreated = DateTime.UtcNow;
                            whoCreated = "SYSTEM";
                        }
                        else if (rightHand && pc.RightHand != null)
                        {
                            itemID = pc.RightHand.itemID;
                            attunedID = pc.RightHand.attunedID;
                            special = pc.RightHand.special;
                            coinValue = pc.RightHand.coinValue;
                            venom = pc.RightHand.venom;
                            charges = pc.RightHand.charges;
                            attuneType = (int)pc.RightHand.attuneType;
                            figExp = pc.RightHand.figExp;
                            nocked = pc.RightHand.IsNocked;
                            timeCreated = pc.RightHand.timeCreated;
                            whoCreated = pc.RightHand.whoCreated;
                        }
                        else if (rightHand && pc.RightHand == null)
                        {
                            itemID = 0;
                            attunedID = 0;
                            special = "";
                            coinValue = 0;
                            venom = 0;
                            charges = -1;
                            attuneType = (int)Globals.eAttuneType.None;
                            figExp = 0;
                            nocked = false;
                            timeCreated = DateTime.UtcNow;
                            whoCreated = "SYSTEM";
                        }

                        SqlStoredProcedure sp = new SqlStoredProcedure(pc.IsNewPC ? "prApp_PlayerHeld_Insert" : "prApp_PlayerHeld_Update", conn);
                        sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                        sp.AddParameter("@rightHand", SqlDbType.Int, 4, ParameterDirection.Input, rightHand);
                        sp.AddParameter("@itemID", SqlDbType.Int, 4, ParameterDirection.Input, itemID);
                        sp.AddParameter("@attunedID", SqlDbType.Int, 4, ParameterDirection.Input, attunedID);
                        sp.AddParameter("@special", SqlDbType.NVarChar, 4000, ParameterDirection.Input, special);
                        sp.AddParameter("@coinValue", SqlDbType.Float, 8, ParameterDirection.Input, coinValue);
                        sp.AddParameter("@charges", SqlDbType.Int, 4, ParameterDirection.Input, charges);
                        sp.AddParameter("@venom", SqlDbType.Int, 4, ParameterDirection.Input, venom);
                        sp.AddParameter("@attuneType", SqlDbType.Int, 4, ParameterDirection.Input, attuneType);
                        sp.AddParameter("@figExp", SqlDbType.BigInt, 8, ParameterDirection.Input, figExp);
                        sp.AddParameter("@nocked", SqlDbType.Bit, 1, ParameterDirection.Input, nocked);
                        sp.AddParameter("@timeCreated", SqlDbType.DateTime, 8, ParameterDirection.Input, timeCreated);
                        sp.AddParameter("@whoCreated", SqlDbType.NVarChar, 100, ParameterDirection.Input, whoCreated);

                        if (pc.IsNewPC)  // insert 
                        {
                            result += sp.ExecuteNonQuery();
                        }
                        else  // update 
                        {
                            DataTable dtPlayerHeld = sp.ExecuteDataTable();
                            if (dtPlayerHeld == null)
                            {
                                result--;
                            }
                            else
                            {
                                result++;
                            }
                        }

                        rightHand = true;
                    }
                    return result;
                }
                catch (Exception e)
                {
                    Utils.Log("Failure at savePlayerHeld rightHand = " + Convert.ToString(rightHand), Utils.LogType.SystemFailure);
                    Utils.LogException(e);
                    return result;
                }
            }
        }

        internal static int SavePlayerSack(PC pc) // save the player's sack contents and carried gold
        {
            //String sptouse = "";  // Holds the special procedure to use (insert or update)
            int sacksize = pc.sackList.Count;  // number of items in sack
            int itemtosave = 0;
            int attunedtosave = 0;
            double goldtosave = 0;
            string specialtosave = "";
            double coinvaluetosave = 0;
            int chargestosave = 0;
            int venomtosave = 0;
            Globals.eAttuneType attunetypeToSave = Globals.eAttuneType.None;
            long figexptosave = 0;
            DateTime timecreatedToSave =  DateTime.UtcNow;
            string whocreatedToSave = "SYSTEM";
            int err = 0;

            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    for (int slot = 1; slot <= Character.MAX_SACK + 1; ++slot) //items max plus gold
                    {
                        //if(pc.IsNewPC)
                        //{
                        //    sptouse = "prApp_PlayerSack_Insert";  // If new char, we insert a new row.
                        //}
                        //else
                        //{
                        //    sptouse = "prApp_PlayerSack_Update";  // If saving old character, we update an existing row.
                        //}
                        if (sacksize == 0) // Regardless of amount of items in sack, we save all 21 rows, inserting 0's for no item.
                        {
                            itemtosave = 0;
                            attunedtosave = 0;
                            goldtosave = 0;
                            specialtosave = "";
                            coinvaluetosave = 0;
                            chargestosave = 0;
                            venomtosave = 0;
                            attunetypeToSave = Globals.eAttuneType.None;
                            figexptosave = 0;
                            timecreatedToSave = DateTime.UtcNow;
                            whocreatedToSave = "SYSTEM";
                        }
                        else
                        {
                            Item item = pc.sackList[sacksize - 1];  // Now set the the itemID, attuned, and coinvalue if needed.
                            itemtosave = item.itemID;
                            if (item.itemType == Globals.eItemType.Coin)
                            {
                                goldtosave = item.coinValue;
                            }
                            else
                            {
                                goldtosave = 0;
                            }
                            attunedtosave = item.attunedID;
                            specialtosave = item.special;
                            coinvaluetosave = item.coinValue;
                            chargestosave = item.charges;
                            venomtosave = item.venom;
                            attunetypeToSave = item.attuneType;
                            figexptosave = item.figExp;
                            timecreatedToSave = item.timeCreated;
                            whocreatedToSave = item.whoCreated;
                        }
                        SqlStoredProcedure sp = new SqlStoredProcedure(pc.IsNewPC ? "prApp_PlayerSack_Insert" : "prApp_PlayerSack_Update", conn);
                        sp.AddParameter("@PlayerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                        sp.AddParameter("@SackSlot", SqlDbType.Int, 4, ParameterDirection.Input, slot);
                        sp.AddParameter("@SackItem", SqlDbType.Int, 4, ParameterDirection.Input, itemtosave);
                        sp.AddParameter("@Attuned", SqlDbType.Int, 4, ParameterDirection.Input, attunedtosave);
                        sp.AddParameter("@SackGold", SqlDbType.Float, 8, ParameterDirection.Input, goldtosave);
                        sp.AddParameter("@Special", SqlDbType.NVarChar, 255, ParameterDirection.Input, specialtosave);
                        sp.AddParameter("@CoinValue", SqlDbType.Float, 8, ParameterDirection.Input, coinvaluetosave);
                        sp.AddParameter("@Charges", SqlDbType.Int, 4, ParameterDirection.Input, chargestosave);
                        sp.AddParameter("@Venom", SqlDbType.Int, 4, ParameterDirection.Input, venomtosave);
                        sp.AddParameter("@WillAttune", SqlDbType.Int, 4, ParameterDirection.Input, (int)attunetypeToSave);
                        sp.AddParameter("@FigExp", SqlDbType.BigInt, 8, ParameterDirection.Input, figexptosave);
                        sp.AddParameter("@TimeCreated", SqlDbType.DateTime, 8, ParameterDirection.Input, timecreatedToSave);
                        sp.AddParameter("@whoCreated", SqlDbType.NVarChar, 100, ParameterDirection.Input, whocreatedToSave);

                        if (pc.IsNewPC)  // Insert 
                        {
                            err = sp.ExecuteNonQuery();
                        }
                        else  // Update 
                        {
                            DataTable dtPlayerSack = sp.ExecuteDataTable();
                            if (dtPlayerSack == null)
                                err = -1;
                            else
                                err = 1;
                        }
                        --sacksize;
                        if (sacksize <= 0) { sacksize = 0; }
                    }
                    return err;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }

        internal static int SavePlayerPouch(PC pc) // save the player's pouch contents, should be no coins in the pouch
        {
            int pouchsize = pc.pouchList.Count;  // number of items in pouch
            int itemtosave = 0;
            int attunedtosave = 0;
            string specialtosave = "";
            double coinvaluetosave = 0;
            int chargestosave = 0;
            int venomtosave = 0;
            Globals.eAttuneType attunetypeToSave = Globals.eAttuneType.None;
            long figexptosave = 0;
            DateTime timecreatedToSave = DateTime.UtcNow;
            string whocreatedToSave = "SYSTEM";
            int err = 0;

            using (var conn = DataAccess.GetSQLConnection())
            {
                string spToUse = "prApp_PlayerPouch_Update";

                if (pc.IsNewPC) spToUse = "prApp_PlayerPouch_Insert";

                try
                {
                    // already an insert sp
                    if (!pc.IsNewPC)
                    {
                        SqlCommand CountRows = new SqlCommand();

                        SqlParameter id = new SqlParameter();
                        SqlParameter command = new SqlParameter();

                        CountRows.Parameters.AddWithValue("@PlayerID", pc.UniqueID);

                        CountRows.CommandText = "SELECT COUNT(PlayerID) FROM PlayerPouch WHERE PlayerID = '" + pc.UniqueID + "'";

                        CountRows.Connection = conn;
                        CountRows.Connection.Open();
                        int rowCount = Convert.ToInt32(CountRows.ExecuteScalar());
                        CountRows.Connection.Close();

                        if (rowCount <= 0) spToUse = "prApp_PlayerPouch_Insert";
                    }

                    for (int slot = 1; slot <= Character.MAX_POUCH; ++slot) //
                    {
                        if (pouchsize == 0) // Regardless of amount of items in pouch, we save all 20 rows, inserting 0's for no item.
                        {
                            itemtosave = 0;
                            attunedtosave = 0;
                            specialtosave = "";
                            coinvaluetosave = 0;
                            chargestosave = 0;
                            venomtosave = 0;
                            attunetypeToSave = Globals.eAttuneType.None;
                            figexptosave = 0;
                            timecreatedToSave = DateTime.UtcNow;
                            whocreatedToSave = "SYSTEM";
                        }
                        else
                        {
                            Item item = pc.pouchList[pouchsize - 1];  // Now set the the itemID, attuned, and coinvalue if needed.
                            itemtosave = item.itemID;
                            attunedtosave = item.attunedID;
                            specialtosave = item.special;
                            coinvaluetosave = item.coinValue;
                            chargestosave = item.charges;
                            venomtosave = item.venom;
                            attunetypeToSave = item.attuneType;
                            figexptosave = item.figExp;
                            timecreatedToSave = item.timeCreated;
                            whocreatedToSave = item.whoCreated;
                        }
                        SqlStoredProcedure sp = new SqlStoredProcedure(spToUse, conn);
                        sp.AddParameter("@PlayerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                        sp.AddParameter("@PouchSlot", SqlDbType.Int, 4, ParameterDirection.Input, slot);
                        sp.AddParameter("@PouchItem", SqlDbType.Int, 4, ParameterDirection.Input, itemtosave);
                        sp.AddParameter("@Attuned", SqlDbType.Int, 4, ParameterDirection.Input, attunedtosave);
                        sp.AddParameter("@Special", SqlDbType.NVarChar, 255, ParameterDirection.Input, specialtosave);
                        sp.AddParameter("@CoinValue", SqlDbType.Float, 8, ParameterDirection.Input, coinvaluetosave);
                        sp.AddParameter("@Charges", SqlDbType.Int, 4, ParameterDirection.Input, chargestosave);
                        sp.AddParameter("@Venom", SqlDbType.Int, 4, ParameterDirection.Input, venomtosave);
                        sp.AddParameter("@WillAttune", SqlDbType.Int, 4, ParameterDirection.Input, (int)attunetypeToSave);
                        sp.AddParameter("@FigExp", SqlDbType.BigInt, 8, ParameterDirection.Input, figexptosave);
                        sp.AddParameter("@TimeCreated", SqlDbType.DateTime, 8, ParameterDirection.Input, timecreatedToSave);
                        sp.AddParameter("@WhoCreated", SqlDbType.NVarChar, 100, ParameterDirection.Input, whocreatedToSave);

                        if (pc.IsNewPC)  // Insert 
                        {
                            err = sp.ExecuteNonQuery();
                        }
                        else  // Update 
                        {
                            DataTable dtPlayerPouch = sp.ExecuteDataTable();
                            if (dtPlayerPouch == null)
                                err = -1;
                            else
                                err = 1;
                        }
                        --pouchsize;
                        if (pouchsize <= 0) { pouchsize = 0; }
                    }
                    return err;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }

        internal static int SavePlayerLocker(PC pc) // save the player's locker contents
        {
            //string sptouse = "";  // Holds the special procedure to use (insert or update)
            int lockersize = pc.lockerList.Count;  // number of items in locker
            int itemtosave = 0;
            int attunedtosave = 0;
            string specialtosave = "";
            double coinvaluetosave = 0;
            int chargestosave = 0;
            Globals.eAttuneType attunetypeToSave = Globals.eAttuneType.None;
            long figexptosave = 0;
            bool nocked = false;
            DateTime timecreatedToSave =  DateTime.UtcNow;
            string whocreatedToSave = "SYSTEM";
            int err = 0;

            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    for (int slot = 1; slot <= Character.MAX_LOCKER; ++slot) //20 items max
                    {
                        //if(pc.IsNewPC)
                        //{
                        //    sptouse = "prApp_PlayerLocker_Insert";  // If new char, we insert a new row.
                        //}
                        //else
                        //{
                        //    sptouse = "prApp_PlayerLocker_Update";  // If saving old character, we update an existing row.
                        //}
                        if (lockersize == 0) // Regardless of amount of items in locker, we save all 20 rows, inserting 0's for no item.
                        {
                            itemtosave = 0;
                            attunedtosave = 0;
                            specialtosave = "";
                            coinvaluetosave = 0;
                            chargestosave = 0;
                            attunetypeToSave = Globals.eAttuneType.None;
                            figexptosave = 0;
                            nocked = false;
                            timecreatedToSave = DateTime.UtcNow;
                            whocreatedToSave = "SYSTEM";
                        }
                        else
                        {
                            Item item = (Item)pc.lockerList[lockersize - 1];  // Now set the the itemID, attuned, and coinvalue if needed.
                            itemtosave = item.itemID;
                            attunedtosave = item.attunedID;
                            specialtosave = item.special;
                            coinvaluetosave = item.coinValue;
                            chargestosave = item.charges;
                            attunetypeToSave = item.attuneType;
                            figexptosave = item.figExp;
                            nocked = item.IsNocked;
                            timecreatedToSave = item.timeCreated;
                            whocreatedToSave = item.whoCreated;
                        }
                        SqlStoredProcedure sp = new SqlStoredProcedure(pc.IsNewPC ? "prApp_PlayerLocker_Insert" : "prApp_PlayerLocker_Update", conn);
                        sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                        sp.AddParameter("@lockerSlot", SqlDbType.Int, 4, ParameterDirection.Input, slot);
                        sp.AddParameter("@itemID", SqlDbType.Int, 4, ParameterDirection.Input, itemtosave);
                        sp.AddParameter("@attunedID", SqlDbType.Int, 4, ParameterDirection.Input, attunedtosave);
                        sp.AddParameter("@special", SqlDbType.NVarChar, 255, ParameterDirection.Input, specialtosave);
                        sp.AddParameter("@coinValue", SqlDbType.Float, 8, ParameterDirection.Input, coinvaluetosave);
                        sp.AddParameter("@charges", SqlDbType.Int, 4, ParameterDirection.Input, chargestosave);
                        sp.AddParameter("@attuneType", SqlDbType.Int, 4, ParameterDirection.Input, (int)attunetypeToSave);
                        sp.AddParameter("@figExp", SqlDbType.BigInt, 8, ParameterDirection.Input, figexptosave);
                        sp.AddParameter("@nocked", SqlDbType.Bit, 1, ParameterDirection.Input, nocked);
                        sp.AddParameter("@timeCreated", SqlDbType.DateTime, 8, ParameterDirection.Input, timecreatedToSave);
                        sp.AddParameter("@whoCreated", SqlDbType.NVarChar, 50, ParameterDirection.Input, whocreatedToSave);

                        if (pc.IsNewPC)  // Insert 
                        {
                            err = sp.ExecuteNonQuery();
                        }
                        else  // Update 
                        {
                            DataTable dtPlayerLocker = sp.ExecuteDataTable();
                            if (dtPlayerLocker == null)
                                err = -1;
                            else
                                err = 1;
                        }
                        --lockersize;
                        if (lockersize <= 0) { lockersize = 0; }
                    }
                    return err;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }

        internal static int SavePlayerWearing(PC pc) // save the player's worn items
        {
            //string sptouse = "";  // holds the special procedure to use (insert or update)
            int wearinglist = pc.wearing.Count;  // number of items player is wearing
            int itemToSave = 0;
            int attunedToSave = 0;
            int orientationToSave = 0;
            string specialToSave = "";
            double coinvalueToSave = 0;
            int chargesToSave = 0;
            Globals.eAttuneType attunetypeToSave = Globals.eAttuneType.None;
            DateTime timecreatedToSave =  DateTime.UtcNow;
            string whocreatedToSave = "SYSTEM";
            int err = 0;

            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    for (int slot = 1; slot <= 20; ++slot) // 20 worn items max
                    {
                        if (wearinglist == 0) // Regardless of amount of items player is wearing, we save all 20 rows, inserting 0's for no item.
                        {
                            itemToSave = 0;
                            attunedToSave = 0;
                            orientationToSave = 0;
                            specialToSave = "";
                            coinvalueToSave = 0;
                            chargesToSave = 0;
                            attunetypeToSave = Globals.eAttuneType.None;
                            timecreatedToSave = DateTime.UtcNow;
                            whocreatedToSave = "SYSTEM";
                        }
                        else
                        {
                            Item item = (Item)pc.wearing[wearinglist - 1];  // Set the the itemID, attuned, and locationcode.
                            itemToSave = item.itemID;
                            attunedToSave = item.attunedID;
                            orientationToSave = (int)item.wearOrientation;
                            specialToSave = item.special;
                            coinvalueToSave = item.coinValue;
                            chargesToSave = item.charges;
                            attunetypeToSave = item.attuneType;
                            timecreatedToSave = item.timeCreated;
                            whocreatedToSave = item.whoCreated;
                        }
                        SqlStoredProcedure sp = new SqlStoredProcedure(pc.IsNewPC ? "prApp_PlayerWearing_Insert" : "prApp_PlayerWearing_Update", conn);
                        sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                        sp.AddParameter("@wearingSlot", SqlDbType.Int, 4, ParameterDirection.Input, slot);
                        sp.AddParameter("@itemID", SqlDbType.Int, 4, ParameterDirection.Input, itemToSave);
                        sp.AddParameter("@attunedID", SqlDbType.Int, 4, ParameterDirection.Input, attunedToSave);
                        sp.AddParameter("@wearOrientation", SqlDbType.Int, 4, ParameterDirection.Input, orientationToSave);
                        sp.AddParameter("@special", SqlDbType.NVarChar, 255, ParameterDirection.Input, specialToSave);
                        sp.AddParameter("@coinValue", SqlDbType.Float, 8, ParameterDirection.Input, coinvalueToSave);
                        sp.AddParameter("@charges", SqlDbType.Int, 4, ParameterDirection.Input, chargesToSave);
                        sp.AddParameter("@attuneType", SqlDbType.Int, 4, ParameterDirection.Input, (int)attunetypeToSave);
                        sp.AddParameter("@timeCreated", SqlDbType.DateTime, 8, ParameterDirection.Input, timecreatedToSave);
                        sp.AddParameter("@whoCreated", SqlDbType.NVarChar, 100, ParameterDirection.Input, whocreatedToSave);

                        if (pc.IsNewPC)  // Insert 
                        {
                            err = sp.ExecuteNonQuery();
                        }
                        else  // Update 
                        {
                            DataTable dtPlayerWearing = sp.ExecuteDataTable();
                            if (dtPlayerWearing == null)
                                err = -1;
                            else
                                err = 1;
                        }
                        --wearinglist;
                        if (wearinglist <= 0) { wearinglist = 0; }
                    }
                    return err;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }

        internal static int SavePlayerBelt(PC pc) // save the player's belt contents
        {
            //string sptouse = "";  // Holds the special procedure to use (insert or update)
            int beltsize = pc.beltList.Count;  // number of items in locker
            int itemtosave = 0;
            int attunedtosave = 0;
            string specialtosave = "";
            double coinvaluetosave = 0;
            int chargestosave = 0;
            int venomtosave = 0;
            Globals.eAttuneType attunetypeToSave = Globals.eAttuneType.None;
            long figexptosave = 0;
            bool nocked = false;
            DateTime timecreatedToSave =  DateTime.UtcNow;
            string whocreatedToSave = "SYSTEM";
            int err = 0;

            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    for (int slot = 1; slot <= 8; ++slot) //8 items max
                    {
                        //if (pc.IsNewPC)
                        //    sptouse = "prApp_PlayerBelt_Insert";  // If new char, we insert a new row.
                        //else sptouse = "prApp_PlayerBelt_Update";  // If saving old character, we update an existing row.

                        if (beltsize == 0)
                        {
                            #region Save defaults if no item.
                            itemtosave = 0;
                            attunedtosave = 0;
                            specialtosave = "";
                            coinvaluetosave = 0;
                            chargestosave = 0;
                            venomtosave = 0;
                            attunetypeToSave = Globals.eAttuneType.None;
                            figexptosave = 0;
                            nocked = false;
                            timecreatedToSave = DateTime.UtcNow;
                            whocreatedToSave = "SYSTEM";
                            #endregion
                        }
                        else
                        {
                            Item item = (Item)pc.beltList[beltsize - 1];  // Now set the the itemID and attuned field
                            itemtosave = item.itemID;
                            attunedtosave = item.attunedID;
                            specialtosave = item.special;
                            coinvaluetosave = item.coinValue;
                            chargestosave = item.charges;
                            venomtosave = item.venom;
                            attunetypeToSave = item.attuneType;
                            figexptosave = item.figExp;
                            nocked = item.IsNocked;
                            timecreatedToSave = item.timeCreated;
                            whocreatedToSave = item.whoCreated;
                        }

                        #region Parameters.
                        SqlStoredProcedure sp = new SqlStoredProcedure(pc.IsNewPC ? "prApp_PlayerBelt_Insert" : "prApp_PlayerBelt_Update", conn);
                        sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                        sp.AddParameter("@beltSlot", SqlDbType.Int, 4, ParameterDirection.Input, slot);
                        sp.AddParameter("@itemID", SqlDbType.Int, 4, ParameterDirection.Input, itemtosave);
                        sp.AddParameter("@attunedID", SqlDbType.Int, 4, ParameterDirection.Input, attunedtosave);
                        sp.AddParameter("@special", SqlDbType.NVarChar, 255, ParameterDirection.Input, specialtosave);
                        sp.AddParameter("@coinValue", SqlDbType.Float, 8, ParameterDirection.Input, coinvaluetosave);
                        sp.AddParameter("@charges", SqlDbType.Int, 4, ParameterDirection.Input, chargestosave);
                        sp.AddParameter("@venom", SqlDbType.Int, 4, ParameterDirection.Input, venomtosave);
                        sp.AddParameter("@attuneType", SqlDbType.Int, 4, ParameterDirection.Input, (int)attunetypeToSave);
                        sp.AddParameter("@figExp", SqlDbType.BigInt, 8, ParameterDirection.Input, figexptosave);
                        sp.AddParameter("@nocked", SqlDbType.Bit, 1, ParameterDirection.Input, nocked);
                        sp.AddParameter("@timeCreated", SqlDbType.DateTime, 8, ParameterDirection.Input, timecreatedToSave);
                        sp.AddParameter("@whoCreated", SqlDbType.NVarChar, 100, ParameterDirection.Input, whocreatedToSave);
                        #endregion

                        if (pc.IsNewPC)  // Insert
                        {
                            err = sp.ExecuteNonQuery();
                        }
                        else  // Update 
                        {
                            DataTable dtPlayerBelt = sp.ExecuteDataTable();
                            if (dtPlayerBelt == null)
                                err = -1;
                            else
                                err = 1;
                        }
                        --beltsize;
                        if (beltsize <= 0) { beltsize = 0; }
                    }
                    return err;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    pc.beltList.Reverse();
                    return -1;
                }
            }
        }

        internal static int SavePlayerRings(PC pc) // save the player's rings
        {
            //string sptouse = "";  // Holds the special procedure to use (insert or update)
            int itemtosave = 0;
            int attunedtosave = 0;
            bool isRecall = false;
            bool wasRecall = false;
            int recallLand = 0;
            int recallMap = 0;
            int recallX = 0;
            int recallY = 0;
            int recallZ = 0;
            string specialtosave = "";
            double coinvaluetosave = 0;
            int chargestosave = 0;
            Globals.eAttuneType attunetypeToSave = Globals.eAttuneType.None;
            DateTime timecreatedToSave = DateTime.UtcNow;
            string whocreatedToSave = "SYSTEM";
            Item ringtosave = null;
            int err = 0;

            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    for (int ringnum = 1; ringnum <= 8; ++ringnum)
                    {
                        //if (pc.IsNewPC)
                        //{
                        //    sptouse = "prApp_PlayerRings_Insert";  // If new char, we insert a new row.
                        //}
                        //else
                        //{
                        //    sptouse = "prApp_PlayerRings_Update";  // If saving old character, we update an existing row.
                        //}

                        switch (ringnum)
                        {
                            case 1:
                                ringtosave = pc.RightRing1;
                                break;
                            case 2:
                                ringtosave = pc.RightRing2;
                                break;
                            case 3:
                                ringtosave = pc.RightRing3;
                                break;
                            case 4:
                                ringtosave = pc.RightRing4;
                                break;
                            case 5:
                                ringtosave = pc.LeftRing1;
                                break;
                            case 6:
                                ringtosave = pc.LeftRing2;
                                break;
                            case 7:
                                ringtosave = pc.LeftRing3;
                                break;
                            case 8:
                                ringtosave = pc.LeftRing4;
                                break;
                        }
                        if (ringtosave == null)
                        {
                            itemtosave = 0;
                            attunedtosave = 0;
                            isRecall = false;
                            wasRecall = false;
                            recallLand = 0;
                            recallMap = 0;
                            recallX = 0;
                            recallY = 0;
                            recallZ = 0;
                            specialtosave = "";
                            coinvaluetosave = 0;
                            chargestosave = 0;
                            attunetypeToSave = Globals.eAttuneType.None;
                            timecreatedToSave = DateTime.UtcNow;
                            whocreatedToSave = "SYSTEM";
                        }
                        else
                        {
                            Item item = ringtosave;
                            itemtosave = item.itemID;
                            attunedtosave = item.attunedID;
                            isRecall = item.isRecall;
                            wasRecall = item.wasRecall;
                            recallLand = item.recallLand;
                            recallMap = item.recallMap;
                            recallX = item.recallX;
                            recallY = item.recallY;
                            recallZ = item.recallZ;
                            specialtosave = item.special;
                            coinvaluetosave = item.coinValue;
                            chargestosave = item.charges;
                            attunetypeToSave = item.attuneType;
                            timecreatedToSave = item.timeCreated;
                            whocreatedToSave = item.whoCreated;
                        }

                        SqlStoredProcedure sp = new SqlStoredProcedure(pc.IsNewPC ? "prApp_PlayerRings_Insert" : "prApp_PlayerRings_Update", conn);
                        sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                        sp.AddParameter("@ringFinger", SqlDbType.Int, 4, ParameterDirection.Input, ringnum);
                        sp.AddParameter("@itemID", SqlDbType.Int, 4, ParameterDirection.Input, itemtosave);
                        sp.AddParameter("@attunedID", SqlDbType.Int, 4, ParameterDirection.Input, attunedtosave);
                        sp.AddParameter("@isRecall", SqlDbType.Bit, 1, ParameterDirection.Input, isRecall);
                        sp.AddParameter("@wasRecall", SqlDbType.Bit, 1, ParameterDirection.Input, wasRecall);
                        sp.AddParameter("@recallLand", SqlDbType.Int, 4, ParameterDirection.Input, recallLand);
                        sp.AddParameter("@recallMap", SqlDbType.Int, 4, ParameterDirection.Input, recallMap);
                        sp.AddParameter("@recallX", SqlDbType.Int, 4, ParameterDirection.Input, recallX);
                        sp.AddParameter("@recallY", SqlDbType.Int, 4, ParameterDirection.Input, recallY);
                        sp.AddParameter("@recallZ", SqlDbType.Int, 4, ParameterDirection.Input, recallZ);
                        sp.AddParameter("@special", SqlDbType.NVarChar, 255, ParameterDirection.Input, specialtosave);
                        sp.AddParameter("@coinValue", SqlDbType.Float, 8, ParameterDirection.Input, coinvaluetosave);
                        sp.AddParameter("@charges", SqlDbType.Int, 4, ParameterDirection.Input, chargestosave);
                        sp.AddParameter("@attuneType", SqlDbType.Int, 4, ParameterDirection.Input, (int)attunetypeToSave);
                        sp.AddParameter("@timeCreated", SqlDbType.DateTime, 8, ParameterDirection.Input, timecreatedToSave);
                        sp.AddParameter("@whoCreated", SqlDbType.NVarChar, 100, ParameterDirection.Input, whocreatedToSave);

                        if (pc.IsNewPC)  // Insert 
                        {
                            err = sp.ExecuteNonQuery();
                        }
                        else  // Update 
                        {
                            DataTable dtPlayerRing = sp.ExecuteDataTable();
                            if (dtPlayerRing == null)
                                err = -1;
                            else
                                err = 1;
                        }
                    }
                    return err;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }

        internal static int SavePlayerSpells(PC pc) // save the player's SpellList contents
        {
            //string sptouse = "";  // Holds the special procedure to use (insert or update)
            int spellListSize = pc.spellDictionary.Count;  // number of spells in list
            int spelltosave = -1;
            string chanttosave = null;
            int err = 0;

            List<int> knownSpellIDs = new List<int>(pc.spellDictionary.Keys);
            List<string> knownSpellChants = new List<string>(pc.spellDictionary.Values);

            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    for (int slot = 1; slot <= 35; ++slot) //35 spells maximum
                    {
                        //if (pc.IsNewPC)
                        //{
                        //    sptouse = "prApp_PlayerSpells_Insert";  // If new char, we insert a new row.
                        //}
                        //else
                        //{
                        //    sptouse = "prApp_PlayerSpells_Update";  // If saving old character, we update an existing row.
                        //}
                        if (slot > spellListSize)
                        {
                            spelltosave = -1; // Save empty slots as -1 (as there is such a thing as spell 0)
                            chanttosave = null;
                        }
                        else
                        {
                            spelltosave = knownSpellIDs[slot - 1];
                            chanttosave = knownSpellChants[slot - 1];
                        }
                        SqlStoredProcedure sp = new SqlStoredProcedure(pc.IsNewPC ? "prApp_PlayerSpells_Insert" : "prApp_PlayerSpells_Update", conn);
                        sp.AddParameter("@PlayerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                        sp.AddParameter("@SpellSlot", SqlDbType.Int, 4, ParameterDirection.Input, slot);
                        sp.AddParameter("@SpellID", SqlDbType.Int, 4, ParameterDirection.Input, spelltosave);
                        sp.AddParameter("@ChantString", SqlDbType.NVarChar, 255, ParameterDirection.Input, chanttosave);

                        if (pc.IsNewPC)  // Insert 
                        {
                            err = sp.ExecuteNonQuery();
                        }
                        else  // Update 
                        {
                            conn.Open();
                            DataTable dtPlayerSpell = sp.ExecuteDataTable();
                            conn.Close();
                            if (dtPlayerSpell == null)
                                err = -1;
                            else
                                err = 1;
                        }
                    }
                    return err;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    Utils.Log("Failed DBPlayer.SavePlayerSpells for Player: " + pc.GetLogString(), Utils.LogType.ExceptionDetail);
                    return -1;
                }
            }
        }

        // saves player quest flags
        internal static int SavePlayerFlags(PC pc)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    if (pc.UniqueID < 1)
                        return -1;

                    string sptouse = "";
                    if (LoadPlayerFlags(pc, false))
                    {
                        sptouse = "prApp_PlayerFlags_Update";
                    }
                    else
                    {
                        sptouse = "prApp_PlayerFlags_Insert";
                    }

                    SqlStoredProcedure sp = new SqlStoredProcedure(sptouse, conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                    sp.AddParameter("@questFlags", SqlDbType.NVarChar, 4000, ParameterDirection.Input, Utils.ConvertListToString(pc.QuestFlags));
                    sp.AddParameter("@contentFlags", SqlDbType.NVarChar, 4000, ParameterDirection.Input, Utils.ConvertListToString(pc.ContentFlags));

                    return sp.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }

        // saves player quest information
        internal static bool SavePlayerQuests(PC pc)
        {
            if (pc.QuestList.Count <= 0)
                return true;
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    string sptouse = "prApp_PlayerQuests_Delete";
                    SqlStoredProcedure sp = new SqlStoredProcedure(sptouse, conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                    sp.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return false;
                }
            }

            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    string sptouse = "";

                    foreach (GameQuest q in pc.QuestList)
                    {
                        if (PlayerQuestExists(pc.UniqueID, q.QuestID))
                            sptouse = "prApp_PlayerQuests_Update";
                        else sptouse = "prApp_PlayerQuests_Insert";

                        SqlStoredProcedure sp = new SqlStoredProcedure(sptouse, conn);
                        sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                        sp.AddParameter("@questID", SqlDbType.Int, 4, ParameterDirection.Input, q.QuestID);
                        sp.AddParameter("@timesCompleted", SqlDbType.Int, 4, ParameterDirection.Input, q.TimesCompleted);
                        sp.AddParameter("@startDate", SqlDbType.VarChar, 50, ParameterDirection.Input, q.StartDate);
                        sp.AddParameter("@finishDate", SqlDbType.VarChar, 50, ParameterDirection.Input, q.FinishDate);
                        sp.AddParameter("@currentStep", SqlDbType.SmallInt, 2, ParameterDirection.Input, q.CurrentStep);
                        //sp.AddParameter("@completedSteps", SqlDbType.NVarChar, 255, ParameterDirection.Input, Utils.ConvertListToString(q.CompletedSteps));
                        sp.ExecuteNonQuery();
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

        internal static int SavePlayerEffects(PC pc) // save the player's non-worn effects
        {
            //string sptouse = "";  // Holds the special procedure to use (insert or update)
            int numEffects = pc.EffectsList.Count;  // number of effects on player
            int effecttosave = 0;
            int amounttosave = 0;
            int durationtosave = 0;
            int err = 0;
            Effect[] effects = new Effect[pc.EffectsList.Count];
            pc.EffectsList.Values.CopyTo(effects, 0);

            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    for (int slot = 1; slot <= 20; ++slot) //20 effects max
                    {
                        //if (pc.IsNewPC)
                        //{
                        //    sptouse = "prApp_PlayerEffects_Insert";  // If new char, we insert a new row.
                        //}
                        //else
                        //{
                        //    sptouse = "prApp_PlayerEffects_Update";  // If saving old character, we update an existing row.
                        //}
                        if (numEffects == 0) // We've reached end of effects on player - fill up rest with 0's
                        {
                            effecttosave = 0;
                            amounttosave = 0;
                            durationtosave = 0;
                        }
                        else
                        {
                            Effect effect = effects[numEffects - 1];
                            effecttosave = (int)effect.EffectType;
                            amounttosave = effect.Power;
                            durationtosave = effect.Duration;
                        }
                        SqlStoredProcedure sp = new SqlStoredProcedure(pc.IsNewPC ? "prApp_PlayerEffects_Insert" : "prApp_PlayerEffects_Update", conn);
                        sp.AddParameter("@PlayerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                        sp.AddParameter("@EffectSlot", SqlDbType.Int, 4, ParameterDirection.Input, slot);
                        sp.AddParameter("@EffectID", SqlDbType.Int, 4, ParameterDirection.Input, effecttosave);
                        sp.AddParameter("@Amount", SqlDbType.Int, 4, ParameterDirection.Input, amounttosave);
                        sp.AddParameter("@Duration", SqlDbType.Int, 4, ParameterDirection.Input, durationtosave);

                        if (pc.IsNewPC)  // Insert 
                        {
                            err = sp.ExecuteNonQuery();
                        }
                        else  // Update 
                        {
                            DataTable dtPlayerEffect = sp.ExecuteDataTable();
                            if (dtPlayerEffect == null)
                            {
                                err = -1;
                            }
                            else
                            {
                                err = 1;
                            }
                        }
                        --numEffects;
                        if (numEffects <= 0) { numEffects = 0; }
                    }
                    return err;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }

        // ***********************
        // LOAD CHARACTER ROUTINES
        //************************
        internal static PC GetPCByID(int playerID) // get a player using their ID
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Player_Select", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    DataTable dtPlayer = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtPlayer.Rows)
                    {
                        PC pc = new PC(dr);
                        return pc;
                    }
                    return null;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        internal static DataRow GetPlayerSettingsDataRow(int playerID)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_PlayerSettings_Select", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    DataTable dtPlayerSettings = sp.ExecuteDataTable();
                    return dtPlayerSettings.Rows[0];
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        /// <summary>
        /// ERASE UPON CODE CLEANUP COMPLETION
        /// </summary>
        /// <param name="pc"></param>
        internal static void LoadPlayerSettings(PC pc)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_PlayerSettings_Select", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                    DataTable dtPlayerSettings = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtPlayerSettings.Rows)
                    {
                        ConvertRowToSettings(pc, dr);
                    }
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
            }
        }

        /// <summary>
        /// ERASE UPON CODE CLEANUP COMPLETION
        /// </summary>
        /// <param name="pc"></param>
        /// <param name="drItem"></param>
        internal static void ConvertRowToSettings(PC pc, DataRow drItem)
        {
            try
            {
                pc.DisplayCombatDamage = Convert.ToBoolean(drItem["displayCombatDamage"]);
                pc.DisplayPetDamage = Convert.ToBoolean(drItem["displayPetDamage"]);
                pc.DisplayPetMessages = Convert.ToBoolean(drItem["displayPetMessages"]);
                pc.DisplayDamageShield = Convert.ToBoolean(drItem["displayDamageShield"]);
                pc.IsAnonymous = Convert.ToBoolean(drItem["anonymous"]);
                pc.echo = Convert.ToBoolean(drItem["echo"]);
                pc.filterProfanity = Convert.ToBoolean(drItem["filterProfanity"]);
                int[] array = Utils.ConvertStringToIntArray(drItem["friendsList"].ToString());
                array.CopyTo(pc.friendsList, 0);
                pc.friendNotify = Convert.ToBoolean(drItem["friendNotify"]);
                array = Utils.ConvertStringToIntArray(drItem["ignoreList"].ToString());
                array.CopyTo(pc.ignoreList, 0);
                pc.IsImmortal = Convert.ToBoolean(drItem["immortal"]);
                pc.IsInvisible = Convert.ToBoolean(drItem["invisible"]);
                pc.receiveGroupInvites = Convert.ToBoolean(drItem["receiveGroupInvites"]);
                pc.receivePages = Convert.ToBoolean(drItem["receivePages"]);
                pc.receiveTells = Convert.ToBoolean(drItem["receiveTells"]);
                pc.showStaffTitle = Convert.ToBoolean(drItem["showStaffTitle"]);

                // fill the macros list with empty strings
                for (int a = 0; a < PC.MAX_MACROS; a++)
                {
                    pc.macros.Add("");
                }

                if (drItem["macros"].ToString() != "")
                {
                    string[] savedMacros = drItem["macros"].ToString().Split(ProtocolYuusha.ISPLIT.ToCharArray());

                    for (int a = 0; a < savedMacros.Length; a++)
                    {
                        pc.macros[a] = savedMacros[a];
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        internal static DataRow GetPlayerSkillsDataRow(int playerID)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_PlayerSkills_Select", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    DataTable dtPlayerSkills = sp.ExecuteDataTable();
                    return dtPlayerSkills.Rows[0];
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        /// <summary>
        /// ERASE UPON CODE CLEANUP COMPLETION
        /// </summary>
        /// <param name="pc"></param>
        internal static void LoadPlayerSkills(PC pc)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_PlayerSkills_Select", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                    DataTable dtPlayerSkills = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtPlayerSkills.Rows)
                    {
                        ConvertRowToSkills(pc, dr);
                    }
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
            }
        }

        /// <summary>
        /// ERASE UPON CODE CLEANUP COMPLETION
        /// </summary>
        /// <param name="pc"></param>
        /// <param name="drItem"></param>
        internal static void ConvertRowToSkills(PC pc, DataRow drItem)
        {
            try
            {
                pc.mace = Convert.ToInt64(drItem["mace"]);
                pc.bow = Convert.ToInt64(drItem["bow"]);
                pc.flail = Convert.ToInt64(drItem["flail"]);
                pc.dagger = Convert.ToInt64(drItem["dagger"]);
                pc.rapier = Convert.ToInt64(drItem["rapier"]);
                pc.twoHanded = Convert.ToInt64(drItem["twoHanded"]);
                pc.staff = Convert.ToInt64(drItem["staff"]);
                pc.shuriken = Convert.ToInt64(drItem["shuriken"]);
                pc.sword = Convert.ToInt64(drItem["sword"]);
                pc.threestaff = Convert.ToInt64(drItem["threestaff"]);
                pc.halberd = Convert.ToInt64(drItem["halberd"]);
                pc.unarmed = Convert.ToInt64(drItem["unarmed"]);
                pc.thievery = Convert.ToInt64(drItem["thievery"]);
                pc.magic = Convert.ToInt64(drItem["magic"]);
                pc.bash = Convert.ToInt64(drItem["bash"]);
                pc.highMace = Convert.ToInt64(drItem["highMace"]);
                pc.highBow = Convert.ToInt64(drItem["highBow"]);
                pc.highFlail = Convert.ToInt64(drItem["highFlail"]);
                pc.highDagger = Convert.ToInt64(drItem["highDagger"]);
                pc.highRapier = Convert.ToInt64(drItem["highRapier"]);
                pc.highTwoHanded = Convert.ToInt64(drItem["highTwohanded"]);
                pc.highStaff = Convert.ToInt64(drItem["highStaff"]);
                pc.highShuriken = Convert.ToInt64(drItem["highShuriken"]);
                pc.highSword = Convert.ToInt64(drItem["highSword"]);
                pc.highThreestaff = Convert.ToInt64(drItem["highThreestaff"]);
                pc.highHalberd = Convert.ToInt64(drItem["highHalberd"]);
                pc.highUnarmed = Convert.ToInt64(drItem["highUnarmed"]);
                pc.highThievery = Convert.ToInt64(drItem["highThievery"]);
                pc.highMagic = Convert.ToInt64(drItem["highMagic"]);
                pc.highBash = Convert.ToInt64(drItem["highBash"]);
                pc.trainedMace = Convert.ToInt64(drItem["trainedMace"]);
                pc.trainedBow = Convert.ToInt64(drItem["trainedBow"]);
                pc.trainedFlail = Convert.ToInt64(drItem["trainedFlail"]);
                pc.trainedDagger = Convert.ToInt64(drItem["trainedDagger"]);
                pc.trainedRapier = Convert.ToInt64(drItem["trainedRapier"]);
                pc.trainedTwoHanded = Convert.ToInt64(drItem["trainedTwoHanded"]);
                pc.trainedStaff = Convert.ToInt64(drItem["trainedStaff"]);
                pc.trainedShuriken = Convert.ToInt64(drItem["trainedShuriken"]);
                pc.trainedSword = Convert.ToInt64(drItem["trainedSword"]);
                pc.trainedThreestaff = Convert.ToInt64(drItem["trainedThreestaff"]);
                pc.trainedHalberd = Convert.ToInt64(drItem["trainedHalberd"]);
                pc.trainedUnarmed = Convert.ToInt64(drItem["trainedUnarmed"]);
                pc.trainedThievery = Convert.ToInt64(drItem["trainedThievery"]);
                pc.trainedMagic = Convert.ToInt64(drItem["trainedMagic"]);
                pc.trainedBash = Convert.ToInt64(drItem["trainedBash"]);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        internal static Item LoadPlayerHeld(int playerID, bool rightHand)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                Item heldItem = null;
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_PlayerHeld_Select", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    sp.AddParameter("@rightHand", SqlDbType.Bit, 1, ParameterDirection.Input, rightHand);
                    DataTable dtPlayer = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtPlayer.Rows)
                    {
                        heldItem = ConvertRowToHeldItem(dr);
                    }
                    return heldItem;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        internal static Item ConvertRowToHeldItem(DataRow drItem)
        {
            try
            {
                int id = Convert.ToInt32(drItem["itemID"]);
                if (id == 0) { return null; }
                Item heldItem = Item.CopyItemFromDictionary(id);

                // Quick fix to store scroll information.
                if (heldItem.baseType == Globals.eItemBaseType.Scroll && drItem["Special"].ToString().Contains("longDesc:"))
                    heldItem = Autonomy.ItemBuilding.ScrollManager.CreateSpellScroll(drItem["Special"].ToString());
                else heldItem.special = drItem["special"].ToString();
                
                heldItem.attunedID = Convert.ToInt32(drItem["attunedID"]);
                heldItem.coinValue = Convert.ToInt64(drItem["coinValue"]);
                heldItem.charges = Convert.ToInt32(drItem["charges"]);
                heldItem.attuneType = (Globals.eAttuneType)Convert.ToInt32(drItem["attuneType"]);
                heldItem.figExp = Convert.ToInt64(drItem["figExp"]);
                heldItem.IsNocked = Convert.ToBoolean(drItem["nocked"]);
                heldItem.timeCreated = Convert.ToDateTime(drItem["timeCreated"]);
                heldItem.whoCreated = drItem["whoCreated"].ToString();
                heldItem.venom = Convert.ToInt32(drItem["venom"]);
                return heldItem;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return null;
            }
        }

        internal static List<Item> LoadPlayerSack(int playerID) // retrieves sack record from PlayerSack table.
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    List<Item> buildSack = new List<Item>();
                    for (int slot = 1; slot <= Character.MAX_SACK + 1; slot++) // plus one for coins
                    {
                        SqlStoredProcedure sp = new SqlStoredProcedure("prApp_PlayerSack_Select", conn);
                        sp.AddParameter("@PlayerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                        sp.AddParameter("@SackSlot", SqlDbType.Int, 4, ParameterDirection.Input, slot);
                        DataTable dtPlayer = sp.ExecuteDataTable();
                        foreach (DataRow dr in dtPlayer.Rows)
                        {
                            Item item = ConvertRowToSackItem(dr);
                            if (item != null)
                            {
                                buildSack.Add(item);
                            }
                        }
                    }
                    return buildSack;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        internal static List<Item> LoadPlayerPouch(int playerID) // retrieves sack record from PlayerSack table.
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    List<Item> buildPouch = new List<Item>();
                    for (int slot = 1; slot <= Character.MAX_POUCH; slot++)
                    {
                        SqlStoredProcedure sp = new SqlStoredProcedure("prApp_PlayerPouch_Select", conn);
                        sp.AddParameter("@PlayerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                        sp.AddParameter("@PouchSlot", SqlDbType.Int, 4, ParameterDirection.Input, slot);
                        DataTable dtPlayer = sp.ExecuteDataTable();
                        foreach (DataRow dr in dtPlayer.Rows)
                        {
                            Item item = ConvertRowToPouchItem(dr);
                            if (item != null)
                            {
                                buildPouch.Add(item);
                            }
                        }
                    }
                    return buildPouch;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        internal static Item ConvertRowToSackItem(DataRow drItem) // convert PlayerSack table results
        {
            try
            {
                int id = Convert.ToInt32(drItem["SackItem"]);
                if (id == 0) { return null; }
                Item sItem = Item.CopyItemFromDictionary(id);
                if (sItem.name == "coins")
                {
                    sItem.coinValue = Convert.ToInt64(drItem["SackGold"]);
                }

                // Quick fix to store scroll information.
                if (sItem.baseType == Globals.eItemBaseType.Scroll && drItem["Special"].ToString().Contains("longDesc:"))
                    sItem = Autonomy.ItemBuilding.ScrollManager.CreateSpellScroll(drItem["Special"].ToString());
                else sItem.special = drItem["Special"].ToString();
                
                sItem.attunedID = Convert.ToInt32(drItem["Attuned"]);
                sItem.coinValue = Convert.ToInt64(drItem["CoinValue"]);
                sItem.charges = Convert.ToInt32(drItem["Charges"]);
                sItem.venom = Convert.ToInt32(drItem["Venom"]);
                sItem.attuneType = (Globals.eAttuneType)Convert.ToInt32(drItem["attuneType"]);
                sItem.figExp = Convert.ToInt64(drItem["FigExp"]);
                sItem.timeCreated = Convert.ToDateTime(drItem["TimeCreated"]);
                sItem.whoCreated = drItem["WhoCreated"].ToString();
                return sItem;
            }
            catch(Exception e)
            {
                Utils.LogException(e);
                return null;
            }
        }

        internal static Item ConvertRowToPouchItem(DataRow drItem) // convert PlayerSack table results
        {
            try
            {
                int id = Convert.ToInt32(drItem["PouchItem"]);
                if (id == 0) { return null; }
                Item pouchItem = Item.CopyItemFromDictionary(id);
                
                // Quick fix to store scroll information.
                if (pouchItem.baseType == Globals.eItemBaseType.Scroll && drItem["Special"].ToString().Contains("longDesc:"))
                    pouchItem = Autonomy.ItemBuilding.ScrollManager.CreateSpellScroll(drItem["Special"].ToString());
                else pouchItem.special = drItem["Special"].ToString();

                pouchItem.attunedID = Convert.ToInt32(drItem["Attuned"]);
                pouchItem.coinValue = Convert.ToInt64(drItem["CoinValue"]);
                pouchItem.charges = Convert.ToInt32(drItem["Charges"]);
                pouchItem.venom = Convert.ToInt32(drItem["Venom"]);
                pouchItem.attuneType = (Globals.eAttuneType)Convert.ToInt32(drItem["attuneType"]);
                pouchItem.figExp = Convert.ToInt64(drItem["FigExp"]);
                pouchItem.timeCreated = Convert.ToDateTime(drItem["TimeCreated"]);
                pouchItem.whoCreated = drItem["WhoCreated"].ToString();
                return pouchItem;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return null;
            }
        }

        internal static List<Item> LoadPlayerLocker(int playerID) // retrieves locker record from PlayerLocker table
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    List<Item> buildLocker = new List<Item>();
                    for (int slot = 1; slot <= 20; slot++)
                    {
                        SqlStoredProcedure sp = new SqlStoredProcedure("prApp_PlayerLocker_Select", conn);
                        sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                        sp.AddParameter("@lockerSlot", SqlDbType.Int, 4, ParameterDirection.Input, slot);
                        DataTable dtPlayer = sp.ExecuteDataTable();
                        foreach (DataRow dr in dtPlayer.Rows)
                        {
                            Item item = ConvertRowToLockerItem(dr);
                            if (item != null)
                            {
                                buildLocker.Add(item);
                            }
                        }
                    }
                    return buildLocker;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        internal static Item ConvertRowToLockerItem(DataRow drItem) // convert PlayerLocker table results
        {
            try
            {
                int id = Convert.ToInt32(drItem["itemID"]);
                if (id == 0) { return null; }
                Item lItem = Item.CopyItemFromDictionary(id);

                // Quick fix to restore scroll information.
                if (lItem.baseType == Globals.eItemBaseType.Scroll && drItem["Special"].ToString().Contains("longDesc:"))
                    lItem = Autonomy.ItemBuilding.ScrollManager.CreateSpellScroll(drItem["Special"].ToString());
                else lItem.special = drItem["special"].ToString();

                lItem.attunedID = Convert.ToInt32(drItem["attunedID"]);
                lItem.coinValue = Convert.ToInt64(drItem["coinValue"]);
                lItem.charges = Convert.ToInt32(drItem["charges"]);
                lItem.attuneType = (Globals.eAttuneType)Convert.ToInt32(drItem["attuneType"]);
                lItem.figExp = Convert.ToInt64(drItem["figExp"]);
                lItem.IsNocked = Convert.ToBoolean(drItem["nocked"]);
                lItem.timeCreated = Convert.ToDateTime(drItem["timeCreated"]);
                lItem.whoCreated = drItem["whoCreated"].ToString();
                return lItem;
            }
            catch(Exception e)
            {
                Utils.LogException(e);
                return null;
            }
        }

        internal static List<Item> LoadPlayerWearing(int playerID) // retrieves wearing record from PlayerWearing table
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    List<Item> buildWearing = new List<Item>();
                    for (int slot = 1; slot <= 20; slot++)
                    {
                        SqlStoredProcedure sp = new SqlStoredProcedure("prApp_PlayerWearing_Select", conn);
                        sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                        sp.AddParameter("@wearingSlot", SqlDbType.Int, 4, ParameterDirection.Input, slot);
                        DataTable dtPlayer = sp.ExecuteDataTable();
                        foreach (DataRow dr in dtPlayer.Rows)
                        {
                            Item item = ConvertRowToWearingItem(dr);
                            if (item != null)
                            {
                                int inventoryCount = 0;
                                foreach (Item wornItem in buildWearing)
                                {
                                    if (wornItem.wearLocation == item.wearLocation)
                                    {
                                        inventoryCount++;
                                    }
                                }

                                if (inventoryCount < Globals.Max_Wearable[(int)item.wearLocation])
                                    buildWearing.Add(item);
                            }
                        }
                    }
                    return buildWearing;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        internal static Item ConvertRowToWearingItem(DataRow drItem) // convert PlayerWearing table results
        {
            try
            {
                int id = Convert.ToInt32(drItem["itemID"]);
                if (id == 0) { return null; }
                Item wItem = Item.CopyItemFromDictionary(id);
                wItem.attunedID = Convert.ToInt32(drItem["attunedID"]);
                wItem.wearOrientation = (Globals.eWearOrientation)Convert.ToInt32(drItem["wearOrientation"]);
                wItem.special = drItem["special"].ToString();
                wItem.coinValue = Convert.ToInt32(drItem["coinValue"]);
                wItem.charges = Convert.ToInt32(drItem["charges"]);
                wItem.attuneType = (Globals.eAttuneType)Convert.ToInt32(drItem["attuneType"]);
                wItem.timeCreated = Convert.ToDateTime(drItem["timeCreated"]);
                wItem.whoCreated = drItem["whoCreated"].ToString();
                return wItem;
            }
            catch(Exception e)
            {
                Utils.LogException(e);
                return null;
            }
        }

        internal static List<Item> LoadPlayerBelt(int playerID) // retrieves belt record from PlayerBelt table
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    List<Item> buildBelt = new List<Item>();
                    for (int slot = 1; slot <= 8; slot++)
                    {
                        SqlStoredProcedure sp = new SqlStoredProcedure("prApp_PlayerBelt_Select", conn);
                        sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                        sp.AddParameter("@beltSlot", SqlDbType.Int, 4, ParameterDirection.Input, slot);
                        DataTable dtPlayer = sp.ExecuteDataTable();
                        foreach (DataRow dr in dtPlayer.Rows)
                        {
                            Item item = ConvertRowToBeltItem(dr);
                            if (item != null)
                            {
                                buildBelt.Add(item);
                            }
                        }
                    }
                    return buildBelt;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        internal static Item ConvertRowToBeltItem(DataRow drItem) // convert PlayerBelt table results
        {
            try
            {
                int id = Convert.ToInt32(drItem["itemID"]);
                if (id == 0) { return null; }
                Item bItem = Item.CopyItemFromDictionary(id);

                // Quick fix to store scroll information.
                if (bItem.baseType == Globals.eItemBaseType.Scroll && drItem["Special"].ToString().Contains("longDesc:"))
                    bItem = Autonomy.ItemBuilding.ScrollManager.CreateSpellScroll(drItem["Special"].ToString());
                else bItem.special = drItem["special"].ToString();

                bItem.attunedID = Convert.ToInt32(drItem["attunedID"]);
                bItem.coinValue = Convert.ToInt64(drItem["coinValue"]);
                bItem.charges = Convert.ToInt32(drItem["charges"]);
                bItem.venom = Convert.ToInt32(drItem["venom"]);
                bItem.attuneType = (Globals.eAttuneType)Convert.ToInt32(drItem["attuneType"]);
                bItem.figExp = Convert.ToInt64(drItem["figExp"]);
                bItem.IsNocked = Convert.ToBoolean(drItem["nocked"]);
                bItem.timeCreated = Convert.ToDateTime(drItem["timeCreated"]);
                bItem.whoCreated = drItem["whoCreated"].ToString();
                return bItem;
            }
            catch(Exception e)
            {
                Utils.LogException(e);
                return null;
            }
        }

        internal static Item LoadPlayerRings(int playerID, int ringnum) // retrieves rings record from PlayerRings table
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                Item ring = null;
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_PlayerRings_Select", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    sp.AddParameter("@ringFinger", SqlDbType.Int, 4, ParameterDirection.Input, ringnum);
                    DataTable dtPlayer = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtPlayer.Rows)
                    {
                        ring = ConvertRowToRingItem(dr);
                    }
                    return ring;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        internal static Item ConvertRowToRingItem(DataRow drItem) // convert PlayerRings table results
        {
            try
            {
                int id = Convert.ToInt32(drItem["itemID"]);
                if (id == 0) { return null; }
                Item item = Item.CopyItemFromDictionary(id);
                item.attunedID = Convert.ToInt32(drItem["attunedID"]);
                item.isRecall = Convert.ToBoolean(drItem["isRecall"]);
                item.wasRecall = Convert.ToBoolean(drItem["wasRecall"]);
                item.recallLand = Convert.ToInt16(drItem["recallLand"]);
                item.recallMap = Convert.ToInt16(drItem["recallMap"]);
                item.recallX = Convert.ToInt32(drItem["recallX"]);
                item.recallY = Convert.ToInt32(drItem["recallY"]);
                item.recallZ = Convert.ToInt32(drItem["recallZ"]);
                item.special = drItem["special"].ToString();
                item.coinValue = Convert.ToInt64(drItem["coinValue"]);
                item.charges = Convert.ToInt32(drItem["charges"]);
                item.attuneType = (Globals.eAttuneType)Convert.ToInt32(drItem["attuneType"]);
                item.timeCreated = Convert.ToDateTime(drItem["timeCreated"]);
                item.whoCreated = drItem["whoCreated"].ToString();
                return item;
            }
            catch(Exception e)
            {
                Utils.LogException(e);
                return null;
            }
        }

        internal static Dictionary<int, string> LoadPlayerSpells(Character.ClassType baseProfession, int playerID) // retrieves spell record from PlayerSpells table
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    Dictionary<int, string> buildSpells = new Dictionary<int, string>();
                    for (int slot = 1; slot <= 35; slot++)
                    {
                        SqlStoredProcedure sp = new SqlStoredProcedure("prApp_PlayerSpells_Select", conn);
                        sp.AddParameter("@PlayerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                        sp.AddParameter("@SpellSlot", SqlDbType.Int, 4, ParameterDirection.Input, slot);
                        DataTable dtPlayerSpells = sp.ExecuteDataTable();
                        foreach (DataRow dr in dtPlayerSpells.Rows)
                        {
                            int id = Convert.ToInt32(dr["SpellID"]);
                            Spells.GameSpell spell = Spells.GameSpell.GetSpell(id);

                            // Added in case spell profession availability is changed in the hard code. 1/13/17 Eb
                            // Hopefully this doesn't occur often. Put in place because Summon Demon is no longer a thaum spell, now sorcerer.
                            if (spell != null && !spell.IsClassSpell(baseProfession))
                                continue;

                            string chant = Convert.ToString(dr["ChantString"]);
                            // failsafe to prevent players from having spells their profession cannot cast
                            if (id >= 0 && DragonsSpine.Spells.GameSpell.GetSpell(id).IsClassSpell(baseProfession))
                            {
                                //added to fix null spell chants
                                if (chant.Length < 2) { chant = DragonsSpine.Spells.GameSpell.GenerateMagicWords(); }
                                while (buildSpells.ContainsValue(chant))
                                {
                                    chant = DragonsSpine.Spells.GameSpell.GenerateMagicWords();
                                }
                                // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                                buildSpells.Add(id, chant);
                            }
                        }
                    }
                    return buildSpells;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        internal static bool PlayerQuestExists(int playerID, int questID)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_PlayerQuests_Select_Single", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    sp.AddParameter("@questID", SqlDbType.Int, 4, ParameterDirection.Input, questID);
                    DataTable dt = sp.ExecuteDataTable();

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        return false;
                    }
                    else if (dt.Rows.Count == 1)
                    {
                        return true;
                    }
                    else if (dt.Rows.Count > 1)
                    {
                        Utils.Log("Error: More than 1 row exists with playerID " + playerID + " and questID " + questID + " in PlayerQuests.", Utils.LogType.SystemWarning);
                        return true;
                    }
                    else { return true; }
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    Utils.Log("Exception thrown while saving in PlayerQuests.", Utils.LogType.SystemWarning);
                    return true;
                }
            }
        }

        internal static bool LoadPlayerFlags(PC pc, bool load)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_PlayerFlags_Select", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                    DataTable dt = sp.ExecuteDataTable();
                    if (!load)
                    {
                        if (dt == null || dt.Rows.Count <= 0)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }

                    foreach (DataRow dr in dt.Rows)
                    {
                        string[] s = dr["questFlags"].ToString().Split(ProtocolYuusha.ASPLIT.ToCharArray());

                        for (int a = 0; a < s.Length; a++)
                        {
                            if(!pc.QuestFlags.Contains(s[a]))
                                pc.QuestFlags.Add(s[a]);
                        }

                        s = dr["contentFlags"].ToString().Split(ProtocolYuusha.ASPLIT.ToCharArray());

                        for (int a = 0; a < s.Length; a++)
                        {
                            if (!pc.ContentFlags.Contains(s[a]))
                                pc.ContentFlags.Add(s[a]);
                        }
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

        internal static List<GameQuest> LoadPlayerQuests(int playerID)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    List<GameQuest> questsList = new List<GameQuest>();
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_PlayerQuests_Select", conn);
                    sp.AddParameter("@playerID", SqlDbType.Int, 4, ParameterDirection.Input, playerID);
                    DataTable dtPlayerQuests = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtPlayerQuests.Rows)
                    {
                        GameQuest quest = GameQuest.CopyQuest(Convert.ToInt32(dr["questID"]));

                        quest.TimesCompleted = Convert.ToInt32(dr["timesCompleted"]);
                        quest.CurrentStep = Convert.ToInt16(dr["currentStep"]);
                        quest.StartDate = dr["startDate"].ToString();

                        if (dr["finishDate"].ToString().Length > 0)
                        {
                            quest.FinishDate = dr["finishDate"].ToString();
                        }

                        bool questFound = false;

                        foreach (GameQuest q in questsList)
                        {
                            if (q.QuestID == quest.QuestID)
                            {
                                questFound = true;
                                if (q.IsRepeatable) q.TimesCompleted++; // changed to mod the quest (q) in the questlist instead of 'quest' which isnt saved on match.

                            }
                        }

                        if(!questFound)
                            questsList.Add(quest);
                    }
                    return questsList;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        internal static void LoadPlayerEffects(PC pc) // retrieves effect record from PlayerEffects table
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    for (int slot = 1; slot <= 20; slot++)
                    {
                        SqlStoredProcedure sp = new SqlStoredProcedure("prApp_PlayerEffects_Select", conn);
                        sp.AddParameter("@PlayerID", SqlDbType.Int, 4, ParameterDirection.Input, pc.UniqueID);
                        sp.AddParameter("@EffectSlot", SqlDbType.Int, 4, ParameterDirection.Input, slot);
                        DataTable dtPlayerEffect = sp.ExecuteDataTable();
                        foreach (DataRow dr in dtPlayerEffect.Rows)
                        {
                            if (Convert.ToInt32(dr["EffectID"]) > 0)
                            {
                                AddPlayerEffect(dr, pc);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
            }
        }

        internal static void AddPlayerEffect(DataRow drItem, PC pc) // convert PlayerEffects table results
        {
            try
            {
                Effect.CreateCharacterEffect((Effect.EffectTypes)Convert.ToInt32(drItem["EffectID"]),
                Convert.ToInt32(drItem["Amount"]), pc, Convert.ToInt32(drItem["Duration"]), null);
            }
            catch(Exception e)
            {
                Utils.LogException(e);
            }
        }

        internal static void InsertPlayerTalent(int playerID, string talentCommand)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlCommand InsertTalent = new SqlCommand();

                    SqlParameter id = new SqlParameter();
                    SqlParameter command = new SqlParameter();

                    InsertTalent.Parameters.AddWithValue("@playerID", playerID);
                    InsertTalent.Parameters.AddWithValue("@talentCommand", talentCommand);
                    InsertTalent.Parameters.AddWithValue("@lastUse", (DateTime.UtcNow - Talents.GameTalent.GameTalentDictionary[talentCommand].DownTime).ToString());

                    InsertTalent.CommandText =
                        "INSERT PlayerTalents (playerID, talentCommand, lastUse)" +
                        " VALUES(@playerID, @talentCommand, @lastUse)";

                    InsertTalent.Connection = conn;
                    InsertTalent.Connection.Open();
                    InsertTalent.ExecuteNonQuery();
                    InsertTalent.Connection.Close();
                }
                catch (Exception e)
                {
                    conn.Close();
                    Utils.LogException(e);
                }
            }
        }

        internal static void UpdatePlayerTalent(int playerID, string talentCommand, DateTime lastUse)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlCommand UpdateTalent = new SqlCommand();

                    UpdateTalent.CommandText =
                        "UPDATE PlayerTalents" +
                        " SET lastUse = '" + lastUse.ToString() + "'" +
                        " WHERE playerID = " + playerID + " AND talentCommand = '" + talentCommand + "'";

                    UpdateTalent.Connection = conn;
                    UpdateTalent.Connection.Open();
                    UpdateTalent.ExecuteNonQuery();
                    UpdateTalent.Connection.Close();
                }
                catch (Exception e)
                {
                    conn.Close();
                    Utils.LogException(e);
                }
            }
        }

        internal static void DeletePlayerTalent(int playerID, string talentCommand)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlCommand DeleteTalent = new SqlCommand();

                    DeleteTalent.CommandText =
                        "DELETE FROM PlayerTalents" +
                        " WHERE playerID = " + playerID + " AND talentCommand = '" + talentCommand + "'";

                    DeleteTalent.Connection = conn;
                    DeleteTalent.Connection.Open();
                    DeleteTalent.ExecuteNonQuery();
                    DeleteTalent.Connection.Close();
                }
                catch (Exception e)
                {
                    conn.Close();
                    Utils.LogException(e);
                }
            }
        }

        internal static void LoadPlayerTalents(PC pc)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlCommand LoadTalents = new SqlCommand();
                    LoadTalents.Connection = conn;

                    LoadTalents.CommandText =
                        "SELECT * FROM PlayerTalents" +
                        " WHERE playerID = " + pc.UniqueID;

                    LoadTalents.Connection.Open();
                    
                    SqlDataReader reader = LoadTalents.ExecuteReader();
                    var table = new DataTable();
                    table.Load(reader);

                    LoadTalents.Connection.Close();

                    pc.talentsDictionary.Clear();

                    foreach (DataRow dr in table.Rows)
                        pc.talentsDictionary.Add(dr["talentCommand"].ToString(), Convert.ToDateTime(dr["lastUse"].ToString()));
                }
                catch (Exception e)
                {
                    conn.Close();
                    Utils.LogException(e);
                }
            }
        }
    }
}
