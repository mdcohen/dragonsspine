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
using System.Xml;
using System.Diagnostics;

namespace DragonsSpine.DAL
{
    /// <summary>
    /// 
    /// SqlStoredProcedure class 
    /// 
    /// The generic SqlStoredProcedure class is a SqlCommand wrapper for SQL Server 
    /// which offers next to all normal 'execute' mehods some extra features: 
    /// - The type is set to 'StoredProcedure'. 
    /// - The Timeout is set to 0. 
    /// - A 'non-open' Connection is Opened and Closed automatically. 
    /// - ReturnValue support. 
    /// - ExecuteDataSet and ExecuteDataTable methods 
    /// - Advanced Error handling is provided. 
    /// 
    /// </summary>
    public class SqlStoredProcedure : IDisposable
    {
        protected const string RERUN_TRANSACTION = "Rerun the transaction.";
        protected const string mReturnValue = "ReturnValue";
        protected SqlCommand mCommand;
        protected bool mConnectionOpened = false;
 
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Stored Procedure name</param>
        /// <param name="connection">A Data.SqlClients.SqlConnection that represents the connection to an instance of SQL Server.</param>
        public SqlStoredProcedure(string name, SqlConnection connection) : this(name, connection, null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Stored Procedure name</param>
        /// <param name="connection">A Data.SqlClients.SqlConnection that represents the connection to an instance of SQL Server.</param>
        /// <param name="transaction">The SqlTransaction in which the SqlCommand executes.</param>
        public SqlStoredProcedure(string name, SqlConnection connection, SqlTransaction transaction)
        {
            mCommand = new SqlCommand(name, connection, transaction);
            mCommand.CommandTimeout = 0;
            mCommand.CommandType = CommandType.StoredProcedure;
            AddReturnValue();
        }

        /// <summary>
        /// Close the Command
        /// </summary>
        public void Dispose()
        {
            if ( mCommand != null )
            {
                mCommand.Dispose();
                mCommand = null;
            }
        }

        /// <summary>
        /// Gets or sets the name of the StoredProcedure
        /// </summary>
        virtual	public string Name
        {
            get { return mCommand.CommandText; }
            set { mCommand.CommandText = value;	}
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
        /// </summary>
        virtual	public int Timeout 
        {
            get { return mCommand.CommandTimeout; }
            set { mCommand.CommandTimeout = value; }
        }

        /// <summary>
        /// Gets the SqlCommand encapsulated in this object
        /// </summary>
        virtual	public SqlCommand Command 
        {
            get { return mCommand; }
        }

        /// <summary>
        /// Gets or sets the SqlConnection used by this instance of the Stored Procedure.
        /// </summary>
        virtual	public SqlConnection Connection 
        {
            get { return mCommand.Connection; }
            set { mCommand.Connection = value; }
        }

        /// <summary>
        /// Gets or sets the SqlTransaction used by this instance of the Stored Procedure.
        /// </summary>
        virtual	public SqlTransaction Transaction 
        {
            get { return mCommand.Transaction; }
            set { mCommand.Transaction = value; }
        }
		
        /// <summary>
        /// Gets the SqlParameterCollection.
        /// </summary>
        virtual	public SqlParameterCollection Parameters
        {
            get { return mCommand.Parameters; }
        }

        /// <summary>
        /// Gets the ReturnValue.
        /// </summary>
        /// <returns>The ReturnValue value.</returns>
        virtual	public int ReturnValue
        {
            get { return (int)mCommand.Parameters[mReturnValue].Value; }
        }

        /// <summary>
        /// Add a parameter to the collection.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="dbType">One of the SqlDbType values.</param>
        /// <param name="size">The width of the parameter, less or equal to 0 will autodetect it using the dbType</param>
        /// <param name="direction">One of the ParameterDirection values. </param>
        /// <returns>The SqlParameter object added to the Parameters collection.</returns>
        virtual	public SqlParameter AddParameter(string parameterName,
				SqlDbType dbType,
				int size,
				ParameterDirection direction)
        {
            SqlParameter p;
            if (size > 0)
            {
                p = new SqlParameter(parameterName, dbType, size);
            }
            else
            {
                // size is automacally detected using dbType
                p = new SqlParameter(parameterName, dbType);
            }
            p.Direction = direction;
            Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Add a parameter to the collection.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="dbType">One of the SqlDbType values.</param>
        /// <param name="size">The width of the parameter, less than or equal to 0 will autodetect it using the dbType</param>
        /// <param name="direction">One of the ParameterDirection values. </param>
        /// <param name="value">An Object that is the value of the SqlParameter. </param>
        /// <returns>The SqlParameter object added to the Parameters collection.</returns>
        virtual	public SqlParameter AddParameter(string parameterName,
				SqlDbType dbType,
				int size,
				ParameterDirection direction,
				object value)
        {
            SqlParameter p = this.AddParameter(parameterName, dbType, size, direction);
            if (value == null)
                p.IsNullable = true;
            p.Value = value;
            return p;
        }

        /// <summary>
        /// Add the ReturnValue parameter
        /// </summary>
        /// <returns></returns>
        virtual	protected SqlParameter AddReturnValue()
        {
            SqlParameter p = Parameters.Add( 
            new SqlParameter( mReturnValue,
            SqlDbType.Int,
            /* int size */ 4,
            ParameterDirection.ReturnValue,
            /* bool isNullable */ false,
            /* byte precision */ 0,
            /* byte scale */ 0,
            /* string srcColumn */ string.Empty,
            DataRowVersion.Default,
            /* value */ null));
            return p;
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the Connection and 
        /// returns the number of rows affected.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        virtual	public int ExecuteNonQuery()
        {
            int rowsAffected = -1;

            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    mCommand.Connection = conn;
                    OpenConnection();
                    rowsAffected = mCommand.ExecuteNonQuery();
                }
                catch (SqlException e)
                {
                    throw TranslateException(e);
                }
                finally
                {
                    CloseConnection();
                }
            }

            return rowsAffected;			
        }

        /// <summary>
        /// Executes the StoredProcedure on the Connection and builds a SqlDataReader object.
        /// </summary>
        /// <returns>The SqlDataReader</returns>
        virtual	public SqlDataReader ExecuteReader()
        {
            SqlDataReader reader;
            try 
            {
                OpenConnection();
                reader = mCommand.ExecuteReader();
            }
            catch (SqlException e)
            {
                throw TranslateException(e);
            }
            return reader;			
        }

        /// <summary>
        /// Executes the StoredProcedure on the Connection and builds a SqlDataReader using one of the CommandBehavior values.
        /// </summary>
        /// <param name="behavior">One of the CommandBehavior values.</param>
        /// <returns>A SqlDataReader object.</returns>
        virtual	public SqlDataReader ExecuteReader(CommandBehavior behavior)
        {
            SqlDataReader reader;
            try 
            {
                OpenConnection();
                reader = mCommand.ExecuteReader(behavior);
            }
            catch (SqlException e)
            {
                throw TranslateException(e);
            }
            finally 
            {
                CloseConnection();
            }
            return reader;			
        }

        /// <summary>
        /// Executes the Stored Procedure, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <returns>The first column of the first row in the resultset.</returns>
        virtual	public object ExecuteScalar()
        {
            object val;
            try 
            {
                OpenConnection();
                val = mCommand.ExecuteScalar();
            }
            catch (SqlException e)
            {
                throw TranslateException(e);
            }
            finally 
            {
                CloseConnection();
            }
            return val;			
        }

        /// <summary>
        /// Executes the StoredProcedure on the Connection and builds a XmlReader object.
        /// </summary>
        /// <returns>A XmlReader object.</returns>
        virtual	public XmlReader ExecuteXmlReader()
        {
            XmlReader reader;
            try 
            {
                OpenConnection();
                reader = mCommand.ExecuteXmlReader();
            }
            catch (SqlException e)
            {
                throw TranslateException(e);
            }
            finally 
            {
                CloseConnection();
            }
            return reader;			
        }

        /// <summary>
        /// Executes the StoredProcedure on the Connection and builds a DataSet object.
        /// </summary>
        /// <returns>A DataSet object.</returns>
        virtual	public DataSet ExecuteDataSet()
        {
            DataSet dataset = new DataSet();
            this.ExecuteDataSet(dataset);			
            return dataset;			
        }

        /// <summary>
        /// Executes the StoredProcedure on the Connection and fill the given DataSet object.
        /// </summary>
        /// <param name="dataset">The DataSet to be filled</param>
        /// <returns>A DataSet object.</returns>
        virtual	public DataSet ExecuteDataSet(DataSet dataset)
        {
            try 
            {
                OpenConnection();
                SqlDataAdapter a = new SqlDataAdapter(this.Command);
                a.Fill(dataset);
            }
            catch (SqlException e)
            {
                throw TranslateException(e);
            }
            finally 
            {
                CloseConnection();
            }
            return dataset;			
        }

        /// <summary>
        /// Executes the StoredProcedure on the Connection and builds a DataTable object.
        /// </summary>
        /// <returns>A DataTable object.</returns>
        virtual	public DataTable ExecuteDataTable()
        {
            DataTable dt = null;

            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    this.Connection = conn;
                    OpenConnection();
                    SqlDataAdapter a = new SqlDataAdapter(this.Command);
                    dt = new DataTable();
                    a.Fill(dt);
                }
                catch (SqlException e)
                {
                    if (e.Message.Contains(RERUN_TRANSACTION))
                    {
                        try
                        {
                            SqlDataAdapter a = new SqlDataAdapter(this.Command);
                            dt = new DataTable();
                            a.Fill(dt);
                        }
                        catch (Exception ex)
                        {
                            Utils.LogException(ex);
                            Utils.Log("Deadlocked transaction failed to rerun transaction. Check exception log for more details.", Utils.LogType.SystemFailure);
                        }
                    }
                    throw TranslateException(e);
                }
                finally
                {
                    CloseConnection();
                }
            }
            return dt;			
        }

        /// <summary>
        /// Open the Connection when the state is not already open.
        /// </summary>
        protected void OpenConnection()
        {
            if (mCommand.Connection.State != ConnectionState.Open)
            {
                mCommand.Connection.Open();
                mConnectionOpened = true;
            }
        }


        /// <summary>
        /// Close the Connection when the state is open.
        /// </summary>
        protected void CloseConnection()
        {
            if ((mCommand.Connection.State == ConnectionState.Open) & mConnectionOpened)
                mCommand.Connection.Close();
        }

        /// <summary>
        /// Translate a SqlException to the correct DALException
        /// </summary>
        /// <param name="e">SqlException to be translated</param>
        /// <returns>An DALException</returns>
        protected Exception TranslateException(SqlException e)
        {
            // uses SQLServer 2000 ErrorCodes
            switch (e.Number)
            {
                default:
                // throw a general DAL Exception
                    break;
            }			
			
            // return the error
            return e;
        }

    }
}
