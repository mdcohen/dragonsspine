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

namespace DragonsSpine
{
    public class Account
    {
        #region Constants
        public const int ACCOUNT_MIN_LENGTH = 5;
        public const int ACCOUNT_MAX_LENGTH = 12;
        public const int PASSWORD_MIN_LENGTH = 6;
        public const int PASSWORD_MAX_LENGTH = 20; 
        #endregion

        public int accountID;
        public string notes;
        public string accountName;
        public string password;
        public string ipAddress;
        public string[] ipAddressList;
        public string hostName;
        public int currentMarks;
        public int lifetimeMarks;
        public DateTime lastOnline;
        public int banLength;
        public DateTime banDate;
        public string email;
        public DateTime loginTime;

        public string[] players;

        public Account()
        {
            this.accountID = -1;
            this.notes = "";
            this.accountName = "";
            this.password = "";
            this.ipAddress = "";
            this.ipAddressList = new string[1];
            this.hostName = "";
            this.currentMarks = 0;
            this.lifetimeMarks = 0;
            this.lastOnline = new DateTime();
            this.banLength = 0;
            this.banDate = new DateTime();
            this.email = "";
            this.loginTime = new DateTime();
        }

        public Account(System.Data.DataRow dr)
        {
            this.accountID = Convert.ToInt32(dr["accountID"]);
            this.notes = dr["notes"].ToString();
            this.accountName = dr["account"].ToString();
            this.password = dr["password"].ToString();
            this.ipAddress = dr["IPAddress"].ToString();
            this.ipAddressList = dr["IPAddressList"].ToString().Split(",".ToCharArray());
            this.currentMarks = Convert.ToInt32(dr["currentMarks"]);
            this.lifetimeMarks = Convert.ToInt32(dr["lifetimeMarks"]);
            this.lastOnline = Convert.ToDateTime(dr["lastOnline"]);
            this.email = dr["email"].ToString();
            this.players = DAL.DBPlayer.GetCharacterList("name", this.accountID);
        }

        public static bool AccountNameDenied(string name)
        {
            // check if account exists
            if (Account.AccountExists(name))
                return true;

            // check account name length
            if (name.Length < Account.ACCOUNT_MIN_LENGTH || name.Length > Account.ACCOUNT_MAX_LENGTH)
                return true;

            bool deny = false;

            string[] silly = new string[]
            {"pvp","lol","haha","hehe","btw","jeje","rofl","roflmao","lmao","lmfao","lmho","dragonsspine","dragonspine","nobody","somebody","anybody","account"};

            string acceptable = "abcdefghijklmnopqrstuvwxyz0123456789";

            foreach (string nasty in Conference.ProfanityArray)
            {
                if (name.ToLower().IndexOf(nasty) > -1)
                {
                    deny = true;
                }
            }

            foreach (string word in silly)
            {
                if (name.ToLower().IndexOf(word) > -1)
                {
                    deny = true;
                }
            }

            foreach (char charcheck in name)
            {
                if (acceptable.IndexOf(charcheck) == -1)
                {
                    deny = true;
                }
            }

            return deny;
        }

        public static void SaveAccountField(int accountID, String field, Object var, string comments)
        {
            if (comments != null)
            {
                Utils.Log(comments, Utils.LogType.Unknown);
            }

            if (DAL.DBAccount.SaveAccountField(accountID, field, var) != 1)
            {
                Utils.Log("Account.save(" + accountID + ", " + field + ", " + var.ToString() + ", Comments: " + comments + ")", Utils.LogType.SystemFailure);
            }
        }

        public static int InsertAccount(string accountName, string password, string ipAddress, string email)
        {
            return DAL.DBAccount.InsertAccount(accountName, password, ipAddress, email);
        }

        public static bool AccountExists(string accountName) // check if account name exists in the DB
        {
            return DAL.DBAccount.AccountExists(accountName);
        }

        public static int GetAccountID(string accountName) // get an account ID using account name
        {
            return DAL.DBAccount.GetAccountID(accountName);
        }

        public static int GetLastPlayed(int accountID) // get last played character ID
        {
            return DAL.DBAccount.GetLastPlayed(accountID);
        }

        public static int GetCurrentMarks(int accountID) // get current account marks
        {
            return (int)DAL.DBAccount.GetAccountField(accountID, "CurrentMarks", Type.GetType("System.Int32"));
        }

        public static string GetPassword(string account)  // get account password
        {
            return (string)DAL.DBAccount.GetAccountField(DAL.DBAccount.GetAccountID(account), "Password", Type.GetType("System.String"));
        }

        public static void SetPassword(int accountID, string password)
        {
            string shapw = Utils.GetSHA(password);                      // Compute password hash.
            DAL.DBAccount.SaveAccountField(accountID, "Password", shapw);
        }

        public static string GetEmail(int accountID)
        {
            return (string)DAL.DBAccount.GetAccountField(accountID, "email", Type.GetType("System.String"));
        }

        public static void SaveEmail(int accountID, string email)
        {
            DAL.DBAccount.SaveAccountField(accountID, "email", email);
        }

        public static DateTime GetLastOnline(int accountID) // get last online date and time
        {
            return (DateTime)DAL.DBAccount.GetAccountField(accountID, "LastOnline", Type.GetType("System.DateTime"));
        }

        public static void SetLastOnline(int accountID) // set new last online
        {
            DAL.DBAccount.SaveAccountField(accountID, "LastOnline", DateTime.UtcNow);//.AddSeconds(DateTime.Now.Second));
        }

        public static void SetIPAddress(int accountID, string ipAddress) // saves the current (ie: latest) IP address used to connect
        {
            if (GetIPAddressList(accountID).IndexOf(ipAddress) == -1) { StoreIPAddress(accountID, ipAddress); }
        }

        public static string GetIPAddressList(int accountID) // get IP address list
        {
            return (string)DAL.DBAccount.GetAccountField(accountID, "IPAddressList", Type.GetType("System.String"));
        }

        public static void StoreIPAddress(int accountID, string ipAddress) // stores all IPAddresses this account has connected from
        {
            try
            {
                string ipAddressList = (string)DAL.DBAccount.GetAccountField(accountID, "IPAddressList", Type.GetType("System.String"));

                if (ipAddressList == "")
                {
                    ipAddressList = ipAddress;
                }
                else
                {
                    if (ipAddressList.Length + ipAddress.Length + 1 >= 4000)
                    {
                        ipAddressList = ipAddress;
                    }
                    else
                    {
                        ipAddressList = ipAddress + "," + ipAddressList;
                    }
                }
                DAL.DBAccount.SaveAccountField(accountID, "IPAddressList", ipAddressList);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static bool VerifyPassword(Account account, string pw)
        {
            string dbpw = account.password;

            if (dbpw.Length > PASSWORD_MAX_LENGTH)
            {
                #region Verify SHA hashed/encrypted password.
                string shapw = Utils.GetSHA(pw);

                if (shapw.Equals(dbpw))
                    return true;
                else
                    return false;

                #endregion
            }
            else
            {
                #region Verify normal password, and encrypted it after.

                if (pw.Equals(dbpw))
                {
                    string dbshapw = Utils.GetSHA(dbpw);
                    int aid = account.accountID;
                    SetPassword(aid, pw);           // SetPassword has been modified to SHA the password itself.

                    return true;
                }
                else
                {
                    return false;
                }

                #endregion
            }
        }

        public static bool VerifyPassword(string account, string pw)
        {
            string dbpw = GetPassword(account);

            if (dbpw.Length > PASSWORD_MAX_LENGTH)
            {
                #region Verify SHA hashed/encrypted password.
                string shapw = Utils.GetSHA(pw);

                if (shapw.Equals(dbpw))
                    return true;
                else
                    return false;

                #endregion
            }
            else
            {
                #region Verify normal password, and encrypted it after.

                if (pw.Equals(dbpw))
                {
                    string dbshapw = Utils.GetSHA(dbpw);
                    int aid = GetAccountID(account);
                    SetPassword(aid, pw);           // SetPassword has been modified to SHA the password itself.

                    return true;
                }
                else
                {
                    return false;
                }

                #endregion
            }
        }

        public string GetLogString()
        {
            return "[" + this.accountID + "] " + this.accountName;
        }

        public int Save()
        {
            return DAL.DBAccount.SaveAccount(this);
        }
    }
}
