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

namespace DragonsSpine.DAL
{
    /// <summary>
    /// Summary description for DSpineDB.
    /// </summary>
    public class DSpineDB
    {
        public DSpineDB()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        internal static int InsertSegue(int Land1,int Map1,int yCord1,int xCord1,int Land2,int Map2,int yCord2,int xCord2,int height)
        {
            try
            {
                SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Segue_Insert", DataAccess.GetSQLConnection());
                sp.AddParameter("@Land1ID", SqlDbType.Int, 4, ParameterDirection.Input, Land1);
                sp.AddParameter("@Map1ID", SqlDbType.Int, 4, ParameterDirection.Input, Map1);
                sp.AddParameter("@Ycord1", SqlDbType.Int, 4, ParameterDirection.Input, yCord1);
                sp.AddParameter("@Xcord1", SqlDbType.Int, 4, ParameterDirection.Input, xCord1);
                sp.AddParameter("@Land2ID", SqlDbType.Int, 4, ParameterDirection.Input, Land2);
                sp.AddParameter("@Map2ID", SqlDbType.Int, 4, ParameterDirection.Input, Map2);
                sp.AddParameter("@Ycord2", SqlDbType.Int, 4, ParameterDirection.Input, yCord2);
                sp.AddParameter("@Xcord2", SqlDbType.Int, 4, ParameterDirection.Input, xCord2);
                sp.AddParameter("@Height", SqlDbType.Int, 4, ParameterDirection.Input, height);
	return sp.ExecuteNonQuery();
            }
            catch
            {
                //Log the error
                return -1;
            }
        }

        internal static int insertQuest(string[] args)
        {
            
            SqlConnection scon = DataAccess.GetSQLConnection();
            string sqlString = "INSERT INTO dbo.Quest(notes, name, description, completedDescription, requirements, " +
                "requiredFlags, coinValues, rewardClass, rewardTitle, requiredItems, rewardItems,rewardExperience, " +
                "rewardStats, rewardTeleports, responseStrings, hintStrings, flagStrings, stepStrings, finishStrings, " +
                "failStrings, classTypes, alignments, maximumLevel, minimumLevel, repeatable, stepOrder, soundFiles, " +
                "totalSteps, despawnsNPC, masterQuestID, teleportGroup) VALUES " +
                "(@notes, @name, @description, @completedDescription, @requirements, " +
                "@requiredFlags, @coinValues, @rewardClass, @rewardTitle, @requiredItems, @rewardItems, @rewardExperience, " +
                "@rewardStats, @rewardTeleports, @responseStrings, @hintStrings, @flagStrings, @stepStrings, @finishStrings, " +
                "@failStrings, @classTypes, @alignments, @maximumLevel, @minimumLevel, @repeatable, @stepOrder, @soundFiles, " +
                "@totalSteps, @despawnsNPC, @masterQuestID, @teleportGroup)";
            SqlCommand cmd = new SqlCommand(sqlString, scon);

            #region SQL Parameters
            SqlParameter _notes = new SqlParameter("@notes", SqlDbType.VarChar);
            SqlParameter _name = new SqlParameter("@name", SqlDbType.VarChar);
            SqlParameter _description = new SqlParameter("@description", SqlDbType.VarChar);
            SqlParameter _completedDescription = new SqlParameter("@completedDescription", SqlDbType.VarChar);
            SqlParameter _requirements = new SqlParameter("@requirements", SqlDbType.VarChar);
            SqlParameter _requiredFlags = new SqlParameter("@requiredFlags", SqlDbType.VarChar);
            SqlParameter _coinValues = new SqlParameter("@coinValues", SqlDbType.VarChar);
            SqlParameter _rewardClass = new SqlParameter("@rewardClass", SqlDbType.VarChar);
            SqlParameter _rewardTitle = new SqlParameter("@rewardTitle", SqlDbType.VarChar);
            SqlParameter _requiredItems = new SqlParameter("@requiredItems", SqlDbType.VarChar);
            SqlParameter _rewardItems = new SqlParameter("@rewardItems", SqlDbType.VarChar);
            SqlParameter _rewardExperience = new SqlParameter("@rewardExperience", SqlDbType.VarChar);
            SqlParameter _rewardStats = new SqlParameter("@rewardStats", SqlDbType.VarChar);
            SqlParameter _rewardTeleports = new SqlParameter("@rewardTeleports", SqlDbType.VarChar);
            SqlParameter _responseStrings = new SqlParameter("@responseStrings", SqlDbType.VarChar);
            SqlParameter _hintStrings = new SqlParameter("@hintStrings", SqlDbType.VarChar);
            SqlParameter _flagStrings = new SqlParameter("@flagStrings", SqlDbType.VarChar);
            SqlParameter _stepStrings = new SqlParameter("@stepStrings", SqlDbType.VarChar);
            SqlParameter _finishStrings = new SqlParameter("@finishStrings", SqlDbType.VarChar);
            SqlParameter _failStrings = new SqlParameter("@failStrings", SqlDbType.VarChar);
            SqlParameter _classTypes = new SqlParameter("@classTypes", SqlDbType.NVarChar);
            SqlParameter _alignments = new SqlParameter("@alignments", SqlDbType.NVarChar);
            SqlParameter _maximumLevel = new SqlParameter("@maximumLevel", SqlDbType.SmallInt);
            SqlParameter _minimumLevel = new SqlParameter("@minimumLevel", SqlDbType.SmallInt);
            SqlParameter _repeatable = new SqlParameter("@repeatable", SqlDbType.Bit);
            SqlParameter _stepOrder = new SqlParameter("@stepOrder", SqlDbType.Bit);
            SqlParameter _soundFiles = new SqlParameter("@soundFiles", SqlDbType.VarChar);
            SqlParameter _totalSteps = new SqlParameter("@totalSteps", SqlDbType.SmallInt);
            SqlParameter _despawnsNPC = new SqlParameter("@despawnsNPC", SqlDbType.Bit);
            SqlParameter _masterQuestID = new SqlParameter("@masterQuestID", SqlDbType.Int);
            SqlParameter _teleportGroup = new SqlParameter("@teleportGroup", SqlDbType.Bit);
            #endregion
            #region Parameter Values
            _notes.Value = args[0];
            _name.Value = args[1];
            _description.Value = args[2];
            _completedDescription.Value = args[3];
            _requirements.Value = args[4];
            _requiredFlags.Value = args[5];
            _coinValues.Value = args[6];
            _rewardClass.Value = args[7];
            _rewardTitle.Value = args[8];
            _requiredItems.Value = args[9];
            _rewardItems.Value = args[10];
            _rewardExperience.Value = args[11];
            _rewardStats.Value = args[12];
            _rewardTeleports.Value = args[13];
            _responseStrings.Value = args[14];
            _hintStrings.Value = args[15];
            _flagStrings.Value = args[16];
            _stepStrings.Value = args[17];
            _finishStrings.Value = args[18];
            _failStrings.Value = args[19];
            _classTypes.Value = args[20];
            _alignments.Value = args[21];
            _maximumLevel.Value = Convert.ToInt32(args[22]);
            _minimumLevel.Value = Convert.ToInt32(args[23]);
            _repeatable.Value = Convert.ToBoolean(args[24]);
            _stepOrder.Value = Convert.ToBoolean(args[25]);
            _soundFiles.Value = args[26];
            _totalSteps.Value = Convert.ToInt32(args[27]);
            _despawnsNPC.Value = Convert.ToBoolean(args[28]);
            _masterQuestID.Value = Convert.ToInt32(args[29]);
            _teleportGroup.Value = Convert.ToBoolean(args[30]);
            cmd.Parameters.Add(_notes);
            cmd.Parameters.Add(_name);
            cmd.Parameters.Add(_description);
            cmd.Parameters.Add(_completedDescription);
            cmd.Parameters.Add(_requirements);
            cmd.Parameters.Add(_requiredFlags);
            cmd.Parameters.Add(_coinValues);
            cmd.Parameters.Add(_rewardClass);
            cmd.Parameters.Add(_rewardTitle);
            cmd.Parameters.Add(_requiredItems);
            cmd.Parameters.Add(_rewardItems);
            cmd.Parameters.Add(_rewardExperience);
            cmd.Parameters.Add(_rewardStats);
            cmd.Parameters.Add(_rewardTeleports);
            cmd.Parameters.Add(_responseStrings);
            cmd.Parameters.Add(_hintStrings);
            cmd.Parameters.Add(_flagStrings);
            cmd.Parameters.Add(_stepStrings);
            cmd.Parameters.Add(_finishStrings);
            cmd.Parameters.Add(_failStrings);
            cmd.Parameters.Add(_classTypes);
            cmd.Parameters.Add(_alignments);
            cmd.Parameters.Add(_maximumLevel);
            cmd.Parameters.Add(_minimumLevel);
            cmd.Parameters.Add(_repeatable);
            cmd.Parameters.Add(_stepOrder);
            cmd.Parameters.Add(_soundFiles);
            cmd.Parameters.Add(_totalSteps);
            cmd.Parameters.Add(_despawnsNPC);
            cmd.Parameters.Add(_masterQuestID);
            cmd.Parameters.Add(_teleportGroup);

            #endregion

            //Open the connection
            scon.Open();

            cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();

            //Close the connection
            scon.Close();
            return 0;
        }

        internal static int insertAccount(string account, string password, string ipaddress, string ipaddresslist, string email)
        {
            try
            {
                SqlConnection scon = DataAccess.GetSQLConnection();
                string sqlString = "INSERT INTO dbo.Account(account, password, ipAddress, ipAddressList, email) VALUES "+
                    "(@account, @password, @ipAddress, @ipAddressList, @email)";
                SqlCommand cmd = new SqlCommand(sqlString, scon);

                #region SQL Parameters
                SqlParameter theAccount = new SqlParameter("@account", SqlDbType.NVarChar, 20);
                SqlParameter thePassword = new SqlParameter("@password", SqlDbType.NVarChar, 20);
                SqlParameter theIpaddress = new SqlParameter("@ipAddress", SqlDbType.NVarChar, 16);
                SqlParameter theIPAddressList = new SqlParameter("@ipAddressList", SqlDbType.NVarChar, 16);
                SqlParameter theEmail = new SqlParameter("@email", SqlDbType.NVarChar, 50);
                #endregion
                #region Parameter Values
                theAccount.Value = account;
                thePassword.Value = password;
                theIpaddress.Value = ipaddress;
                theIPAddressList.Value = ipaddresslist;
                theEmail.Value = email;

                cmd.Parameters.Add(theAccount);
                cmd.Parameters.Add(thePassword);
                cmd.Parameters.Add(theIpaddress);
                cmd.Parameters.Add(theIPAddressList);
                cmd.Parameters.Add(theEmail);
                #endregion

                //Open the connection
                scon.Open();

                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();

                //Close the connection
                scon.Close();
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
            return 0;
        }
    }
}



