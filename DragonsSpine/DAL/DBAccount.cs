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
    public static class DBAccount
    {
        internal static int InsertAccount(string account, string password, string ipAddress, string email) // insert a new account
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Account_Insert", conn);
                    sp.AddParameter("@account", SqlDbType.NVarChar, 20, ParameterDirection.Input, account);
                    sp.AddParameter("@password", SqlDbType.NVarChar, 100, ParameterDirection.Input, password);
                    sp.AddParameter("@ipAddress", SqlDbType.NVarChar, 16, ParameterDirection.Input, ipAddress);
                    sp.AddParameter("@ipAddressList", SqlDbType.NVarChar, 16, ParameterDirection.Input, ipAddress); // initial IPAddressList is current IPAddress only
                    sp.AddParameter("@email", SqlDbType.NVarChar, 50, ParameterDirection.Input, email);
                    return sp.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }
        internal static Object GetAccountField(int accountID, String field, Type objectType)
        {
            // objectTypes: "System.Int32", "System.String", "System.Boolean", "System.Char", "System.DateTme"
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Account_Select_Field", conn);
                    sp.AddParameter("@accountID", SqlDbType.Int, 4, ParameterDirection.Input, accountID);
                    sp.AddParameter("@field", SqlDbType.NVarChar, 50, ParameterDirection.Input, field);
                    DataTable dtAccount = sp.ExecuteDataTable();
                    if (dtAccount == null || dtAccount.Rows.Count < 1)
                    {
                        return null;
                    }
                    foreach (DataRow dr in dtAccount.Rows)
                    {
                        switch (objectType.ToString())
                        {
                            case "System.Int32":
                                return Convert.ToInt32(dr[field]);
                            case "System.String":
                                return dr[field].ToString();
                            case "System.Boolean":
                                return Convert.ToBoolean(dr[field]);
                            case "System.Char":
                                return Convert.ToChar(dr[field]);
                            case "System.DateTime":
                                return Convert.ToDateTime(dr[field]);
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

        internal static int SaveAccountField(int accountID, String field, Object var)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Account_Update_Field", conn);
                    sp.AddParameter("@AccountID", SqlDbType.Int, 4, ParameterDirection.Input, accountID);
                    sp.AddParameter("@field", SqlDbType.NVarChar, 50, ParameterDirection.Input, field);
                    sp.AddParameter("@type", SqlDbType.NVarChar, 50, ParameterDirection.Input, var.GetType().ToString());
                    if (var.GetType().Equals(Type.GetType("System.Int32")))
                    {
                        sp.AddParameter("@int", SqlDbType.Int, 4, ParameterDirection.Input, var);
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
                        sp.AddParameter("@int", SqlDbType.Char, 1, ParameterDirection.Input, var);
                    }
                    else if (var.GetType().Equals(Type.GetType("System.DateTime")))
                    {
                        sp.AddParameter("@dateTime", SqlDbType.DateTime, 8, ParameterDirection.Input, var);
                    }
                    else
                    {
                        Utils.Log("DBAccount.saveAccountProperty(" + accountID + ", " + field + ", " + var.GetType().ToString() + ") *Type Not Recognized*, " + var.GetType().ToString(), Utils.LogType.SystemFailure);
                        return -1;
                    }

                    DataTable dtAccount = sp.ExecuteDataTable();
                    if (dtAccount == null)
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

        internal static bool AccountExists(string account) // searches DB to see if account exists
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Account_Check", conn);
                    sp.AddParameter("@account", SqlDbType.NVarChar, 20, ParameterDirection.Input, account);
                    DataTable dtAccountItem = sp.ExecuteDataTable();
                    if (dtAccountItem == null || dtAccountItem.Rows.Count < 1)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return false;
                }
            }
        }

        internal static int GetAccountID(string account) // get an account ID (generated by db) using account name
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    int i = 0;
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Account_Select", conn);
                    sp.AddParameter("@account", SqlDbType.NVarChar, 20, ParameterDirection.Input, account);
                    DataTable dtAccountItem = sp.ExecuteDataTable();
                    if (dtAccountItem == null || dtAccountItem.Rows.Count < 1)
                    {
                        return -1;
                    }
                    foreach (DataRow dr in dtAccountItem.Rows)
                    {
                        i = Convert.ToInt16(dr["AccountID"]);
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

        internal static int GetLastPlayed(int accountID) // get last played finds the last player saved on an account
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    int i = 0;
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Player_Select_By_Account", conn);
                    sp.AddParameter("@accountID", SqlDbType.Int, 4, ParameterDirection.Input, accountID);
                    DataTable dtPlayerItem = sp.ExecuteDataTable();
                    if (dtPlayerItem == null || dtPlayerItem.Rows.Count < 1)
                    {
                        return -1;
                    }
                    DateTime db = DateTime.MinValue;
                    DateTime dt = DateTime.MinValue;
                    foreach (DataRow dr in dtPlayerItem.Rows)
                    {
                        dt = Convert.ToDateTime(dr["lastOnline"]);
                        if (DateTime.Compare(dt, db) > 0)  // In Compare, > 0 means param 1 > param 2
                        {
                            db = dt;
                            i = Convert.ToInt32(dr["playerID"]);
                        }
                    }
                    return i;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }

        internal static Account GetAccountByName(string name)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Account_Select", conn);
                sp.AddParameter("@account", SqlDbType.NVarChar, 20, ParameterDirection.Input, name);
                DataTable dt = sp.ExecuteDataTable();
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    return new Account(dr);
                }
                return null;
            }
        }

        internal static List<Account> GetAllAccounts()
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                List<Account> accounts = new List<Account>();
                SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Account_Select_All", conn);
                DataTable dt = sp.ExecuteDataTable();
                foreach (DataRow dr in dt.Rows)
                {
                    accounts.Add(new Account(dr));
                }
                return accounts;
            }
        }

        internal static int SaveAccount(Account account)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Account_Update", conn);
                    sp.AddParameter("@accountID", SqlDbType.Int, 4, ParameterDirection.Input, account.accountID);
                    sp.AddParameter("@notes", SqlDbType.Text, System.Int32.MaxValue, ParameterDirection.Input, account.notes);
                    sp.AddParameter("@account", SqlDbType.NVarChar, 20, ParameterDirection.Input, account.accountName);
                    sp.AddParameter("@password", SqlDbType.NVarChar, 100, ParameterDirection.Input, account.password);
                    sp.AddParameter("@currentMarks", SqlDbType.Int, 4, ParameterDirection.Input, account.currentMarks);
                    sp.AddParameter("@email", SqlDbType.NVarChar, 50, ParameterDirection.Input, account.email);
                    DataTable dtPlayerStats = sp.ExecuteDataTable();
                    if (dtPlayerStats == null)
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

        internal static int DeleteAccount(int accountID)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Account_Delete", conn);
                sp.AddParameter("@accountID", SqlDbType.Int, 4, ParameterDirection.Input, accountID);
                return sp.ExecuteNonQuery();
            }
        }
    }
}
