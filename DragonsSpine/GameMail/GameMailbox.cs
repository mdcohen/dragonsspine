using System;
using System.Collections.Generic;

namespace DragonsSpine.Mail.GameMail
{
    public class GameMailbox
    {
        public const int MAX_ATTACHMENTS_PER_PLAYER = 5;

        private int m_mailboxOwnerID = -1;
        private List<GameMailMessage> m_allMailMessages = null; // holds all messages the mailbox owner has sent and received
        private List<GameMailMessage> m_receivedMessages = null; // holds only received messages
        private List<GameMailMessage> m_unreadMessages = null; // holds only received messages that are unread
        private List<GameMailMessage> m_sentMessages = null; // holds only sent messages

        public GameMailMessage MessageDraft { get; set; } // used to hold information for a message being composed
        public GameMailMessage MessageCurrentlyReading { get; set; }
        public List<GameMailAttachment> AllMailAttachments { get; set; }
        public short NumberOfCoinAttachments { get; set; } // coins do not count toward MAX_ATTACHMENTS_PER_PLAYER

        public bool HasUnreadMail
        {
            get { return m_unreadMessages.Count > 0; }
        }

        public List<GameMailMessage> UnreadMessages
        {
            get { return m_unreadMessages; }
        }

        public List<GameMailMessage> ReceivedMessages
        {
            get { return m_receivedMessages; }
        }

        public List<GameMailMessage> SentMessages
        {
            get { return m_sentMessages; }
        }

        public int TotalAttachmentsInMailbox
        {
            get
            {
                if (m_receivedMessages == null || m_receivedMessages.Count <= 0) return 0;

                int count = 0;

                foreach (GameMailMessage message in ReceivedMessages)
                    if (message.HasAttachment) count++;

                return count;
            }
        }

        #region Constructor (1)
        public GameMailbox(int playerID)
        {
            m_mailboxOwnerID = playerID;
            m_receivedMessages = DAL.DBMail.GetMailByReceiverID(playerID);
            m_sentMessages = DAL.DBMail.GetMailBySenderID(playerID);
            m_allMailMessages = new List<GameMailMessage>(m_receivedMessages);
            m_allMailMessages.AddRange(m_sentMessages);
            m_unreadMessages = new List<GameMailMessage>();
            AllMailAttachments = new List<GameMailAttachment>();

            LoadUnreadMessagesAndMailAttachments();
        } 
        #endregion

        private void LoadUnreadMessagesAndMailAttachments()
        {
            m_unreadMessages.Clear();
            AllMailAttachments.Clear();
            NumberOfCoinAttachments = 0;

            foreach (GameMailMessage message in m_allMailMessages)
            {
                if (!message.HasBeenReadByReceiver && message.ReceiverID == m_mailboxOwnerID)
                    m_unreadMessages.Add(message);

                if (message.HasAttachment)
                {
                    GameMailAttachment attachment = DAL.DBMail.GetMailAttachment(message.MailID);
                    AllMailAttachments.Add(attachment);
                    if (attachment.ItemID == Item.ID_COINS) NumberOfCoinAttachments++;
                }
            }
        }

        public void AddReceivedMessage(GameMailMessage message)
        {
            m_allMailMessages.Insert(0, message);
            m_receivedMessages.Insert(0, message);

            LoadUnreadMessagesAndMailAttachments();
        }

        public void AddSentMessage(GameMailMessage message)
        {
            m_allMailMessages.Insert(0, message);
            m_sentMessages.Insert(0, message);
        }

        public void ClearMessageDraft()
        {
            MessageDraft = null;
        }

        public void ClearMessageCurrentlyReading()
        {
            MessageCurrentlyReading = null;
        }

        public void DeleteReceivedMessage(GameMailMessage message)
        {
            if (m_allMailMessages.Contains(message))
                m_allMailMessages.Remove(message);

            if (m_receivedMessages.Contains(message))
                m_receivedMessages.Remove(message);

            if (m_unreadMessages.Contains(message))
                m_unreadMessages.Remove(message);

            // typically before this method is called the owner of the message is told to remove the attachment first
            if (message.HasAttachment)
            {
                DeleteMailAttachment(message.MailItemAttachment);
            }

            DAL.DBMail.DeleteMailMessage(message.MailID); // will no longer show up in someone's sent messages...            
        }

        public void DeleteMailAttachment(GameMailAttachment attachment)
        {
            if (AllMailAttachments.Contains(attachment))
                AllMailAttachments.Remove(attachment);

            if (attachment.ItemID == Item.ID_COINS)
                NumberOfCoinAttachments--;

            DAL.DBMail.DeleteMailAttachment(attachment.MailID);
        }

        public bool DeleteMailbox()
        {
            try
            {
                // Delete all received messages. Messages sent to another player will remain in their inbox.
                foreach (GameMailMessage message in ReceivedMessages)
                {
                    if (message.HasAttachment)
                    {
                        DAL.DBMail.DeleteMailAttachment(message.MailID);
                    }

                    DAL.DBMail.DeleteMailMessage(message.MailID);
                }
                return true;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return false;
            }
        }

        public static bool ValidateItemAttachment(int itemID, int receiverID, int senderID)
        {
            bool senderIsOnline = false;

            PC sender = PC.GetOnline(senderID);

            if (sender == null)
                sender = PC.GetPC(senderID);
            else senderIsOnline = true;

            if (sender == null) return false;

            PC receiver = PC.GetOnline(receiverID);

            if (receiver == null)
                receiver = PC.GetPC(receiverID);

            if (receiver == null) return false;

            // check if item is a corpse
            if (itemID == Item.ID_CORPSE)
            {
                if (senderIsOnline) sender.WriteToDisplay("You cannot attach a corpse to a message. That's disgusting.");
                return false;
            }

            // check max attachments
            if (itemID != Item.ID_COINS && receiver.Mailbox.AllMailAttachments.Count >= MAX_ATTACHMENTS_PER_PLAYER)
            {
                if (senderIsOnline) sender.WriteToDisplay(receiver.Name + " has no more room for attachments in their inbox.");
                return false;
            }

            // ** Removed AG/BG limitation for sending GameMailAttachment objects on 1/21/2017 -Eb
            // verify receiver is in same land as sender for attached items
            //if (receiver.LandID != sender.LandID && sender.ImpLevel < Globals.eImpLevel.GM)
            //{
            //    if (senderIsOnline)
            //    {
            //        sender.WriteToDisplay("Since the receiver of your mail does not reside in " + sender.Land.LongDesc + " you may not send an item attachment.");
            //    }
            //    return false;
            //}

            return true;
        }
    }
}
