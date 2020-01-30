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
    public static class DataAccess
    {
        public static SqlConnection GetSQLConnection()
        {
            try
            {
                var sCon = new SqlConnection(DragonsSpineMain.Instance.Settings.SQLConnection);
                return sCon;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return new SqlConnection();
            }
        }

        /// <summary>
        /// Executes a database query using query text submitted via a command while connected to the server.
        /// </summary>
        /// <param name="queryText">The query text to be executed.</param>
        /// <returns>Number of rows affected. -1 is returned for an invalid query.</returns>
        public static int ExecuteQuery(string queryText)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    var query = new SqlCommand();
                    query.Connection = conn;
                    query.CommandText = queryText;
                    conn.Open();
                    var returnValue = query.ExecuteNonQuery();
                    conn.Close();

                    return returnValue;
                }
                catch(Exception e)
                {
                    Utils.Log("Failed DB Query in DataAccess.ExecuteQuery. Query text: " + queryText, Utils.LogType.DatabaseQuery);
                    Utils.LogException(e);
                    return -1;
                }
            }
        }
    }
}
