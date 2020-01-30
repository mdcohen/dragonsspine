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
using System.Data;
using System.Data.SqlClient;
using DragonsSpine.Mail.GameMail;

namespace DragonsSpine.DAL
{
    public static class DBMail
    {
        internal static List<GameMailMessage> GetMailByReceiverID(int receiverID) // retrieves locker record from PlayerLocker table
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    List<GameMailMessage> mailList = new List<GameMailMessage>();
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Mail_Select_By_ReceiverID", conn);
                    sp.AddParameter("@receiverID", SqlDbType.Int, 4, ParameterDirection.Input, receiverID);
                    DataTable dtMail = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtMail.Rows)
                    {
                        mailList.Add(new GameMailMessage(dr));
                    }
                    return mailList;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        internal static List<GameMailMessage> GetMailBySenderID(int senderID) // retrieves locker record from PlayerLocker table
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    List<GameMailMessage> mailList = new List<GameMailMessage>();
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Mail_Select_By_SenderID", conn);
                    sp.AddParameter("@senderID", SqlDbType.Int, 4, ParameterDirection.Input, senderID);
                    DataTable dtMail = sp.ExecuteDataTable();
                    foreach (DataRow dr in dtMail.Rows)
                    {
                        mailList.Add(new GameMailMessage(dr));
                    }
                    return mailList;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        internal static GameMailMessage GetMailByMailID(long mailID)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Mail_Select_By_MailID", conn);
                    sp.AddParameter("@receiverID", SqlDbType.BigInt, 8, ParameterDirection.Input, mailID);
                    DataTable dtMail = sp.ExecuteDataTable();
                    if (dtMail.Rows.Count > 0)
                    {
                        return new GameMailMessage(dtMail.Rows[0]);
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

        internal static GameMailAttachment GetMailAttachment(long mailID)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_MailAttachment_Select_By_MailID", conn);
                    sp.AddParameter("@mailID", SqlDbType.BigInt, 8, ParameterDirection.Input, mailID);
                    DataTable dtMailAtt = sp.ExecuteDataTable();
                    if (dtMailAtt.Rows.Count > 0)
                    {
                        return new GameMailAttachment(dtMailAtt.Rows[0]);
                    }
                    else return null;
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return null;
                }
            }
        }

        internal static long InsertMailMessage(GameMailMessage mail)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Mail_Insert", conn);
                    sp.AddParameter("@senderID", SqlDbType.Int, 4, ParameterDirection.Input, mail.SenderID);
                    sp.AddParameter("@receiverID", SqlDbType.Int, 4, ParameterDirection.Input, mail.ReceiverID);
                    sp.AddParameter("@timeSent", SqlDbType.DateTime, 8, ParameterDirection.Input, mail.TimeSent);
                    sp.AddParameter("@subject", SqlDbType.NVarChar, 50, ParameterDirection.Input, mail.Subject);
                    sp.AddParameter("@body", SqlDbType.VarChar, 4000, ParameterDirection.Input, mail.Body);
                    sp.AddParameter("@attachment", SqlDbType.Bit, 1, ParameterDirection.Input, mail.HasAttachment);
                    sp.AddParameter("@readByReceiver", SqlDbType.Bit, 1, ParameterDirection.Input, false);
                    sp.AddParameter("@mailID", SqlDbType.BigInt, 8, ParameterDirection.Output);
                    return Convert.ToInt64(sp.ExecuteScalar());
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }

        internal static int InsertMailAttachment(GameMailAttachment attachment)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_MailAttachment_Insert", conn);
                    sp.AddParameter("@mailID", SqlDbType.BigInt, 8, ParameterDirection.Input, attachment.MailID);
                    sp.AddParameter("@itemID", SqlDbType.Int, 4, ParameterDirection.Input, attachment.ItemID);
                    sp.AddParameter("@attunedID", SqlDbType.Int, 4, ParameterDirection.Input, attachment.AttunedID);
                    sp.AddParameter("@special", SqlDbType.VarChar, 255, ParameterDirection.Input, attachment.Special);
                    sp.AddParameter("@coinValue", SqlDbType.Float, 8, ParameterDirection.Input, attachment.CoinValue);
                    sp.AddParameter("@charges", SqlDbType.Int, 4, ParameterDirection.Input, attachment.Charges);
                    sp.AddParameter("@attuneType", SqlDbType.Int, 4, ParameterDirection.Input, (int)attachment.AttuneType);
                    sp.AddParameter("@figExp", SqlDbType.BigInt, 8, ParameterDirection.Input, attachment.FigExp);
                    sp.AddParameter("@timeCreated", SqlDbType.DateTime, 8, ParameterDirection.Input, attachment.TimeCreated);
                    sp.AddParameter("@whoCreated", SqlDbType.NVarChar, 100, ParameterDirection.Input, attachment.WhoCreated);
                    sp.AddParameter("@paymentRequested", SqlDbType.Float, 8, ParameterDirection.Input, attachment.PaymentRequested);
                    return sp.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }

        internal static int DeleteMailMessage(long mailID)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Mail_Delete", conn);
                    sp.AddParameter("@mailID", SqlDbType.BigInt, 8, ParameterDirection.Input, mailID);
                    return sp.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }

        // Currently only one attachment per message.
        internal static int DeleteMailAttachment(long mailID)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_MailAttachment_Delete", conn);
                    sp.AddParameter("@mailID", SqlDbType.BigInt, 4, ParameterDirection.Input, mailID);
                    return sp.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return -1;
                }
            }
        }

        internal static int UpdateMailMessage(GameMailMessage mail)
        {
            using (var conn = DataAccess.GetSQLConnection())
            {
                try
                {
                    SqlStoredProcedure sp = new SqlStoredProcedure("prApp_Mail_Update", conn);
                    sp.AddParameter("@mailID", SqlDbType.BigInt, 8, ParameterDirection.Input, mail.MailID);
                    sp.AddParameter("@attachment", SqlDbType.Int, 4, ParameterDirection.Input, mail.HasAttachment);
                    sp.AddParameter("@readByReceiver", SqlDbType.Int, 4, ParameterDirection.Input, mail.HasBeenReadByReceiver);
                    DataTable dtMail = sp.ExecuteDataTable();
                    if (dtMail == null)
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
    }
}
