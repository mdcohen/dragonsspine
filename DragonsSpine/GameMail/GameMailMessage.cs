using System;

namespace DragonsSpine.Mail.GameMail
{
    public class GameMailMessage
    {
        public const int MAX_SUBJECT_LENGTH = 40;
        public const int MAX_BODY_LENGTH = 256;
        public const int AUTO_MESSAGE_ITEM_PAYMENT = -2; // this is an id from PC.GetName for corresponding sender IDs of mail messages
        public const int AUTO_MESSAGE_LOTTERY_WIN = -3;
        public const int DEVELOPER_ID = 1; // Default to First player in the database is a developer.

        #region Private Data
        private long m_mailID;
        private int m_senderID;
        private int m_receiverID;
        private DateTime m_timeSent;
        private string m_subject;
        private string m_body;
        private bool m_attachment;
        private bool m_readByReceiver;
        private bool m_allowReply;
        private bool m_allowForward;

        private GameMailAttachment m_mailAttachment;
        #endregion

        public long MailID
        {
            get { return m_mailID; }
            set { m_mailID = value; }
        }

        public int SenderID
        {
            get { return m_senderID; }
        }

        public int ReceiverID
        {
            get { return m_receiverID; }
            set { m_receiverID = value; }
        }

        public DateTime TimeSent
        {
            get { return m_timeSent; }
            set { m_timeSent = value; }
        }

        public string Subject
        {
            get { return m_subject; }
            set { m_subject = value; }
        }

        public string Body
        {
            get { return m_body; }
            set { m_body = value; }
        }

        public GameMailAttachment MailItemAttachment
        {
            get { return m_mailAttachment; }
            set { m_mailAttachment = value; }
        }

        public bool HasAttachment
        {
            get { return m_attachment; }
            set { m_attachment = value; }
        }

        public bool HasBeenReadByReceiver
        {
            get { return m_readByReceiver; }
            set { m_readByReceiver = value; }
        }

        public bool AllowReply
        {
            get { return m_allowReply; }
            set { m_allowReply = value; }
        }

        public bool AllowForward
        {
            get { return m_allowForward; }
            set { m_allowReply = value; }
        }

        #region Constructors (4)
        public GameMailMessage()
        {
            m_mailID = -1;
            m_senderID = -1;
            m_receiverID = -1;
            m_timeSent = DateTime.Now;
            m_subject = "";
            m_body = "";
            m_attachment = false;
            m_readByReceiver = false;
            m_mailAttachment = null;
            m_allowForward = true;
            m_allowReply = true;
        }

        public GameMailMessage(int senderID) : base()
        {
            m_senderID = senderID;
        }

        public GameMailMessage(System.Data.DataRow dr) : base()
        {
            m_mailID = Convert.ToInt64(dr["mailID"]);
            m_senderID = Convert.ToInt32(dr["senderID"]);
            m_receiverID = Convert.ToInt32(dr["receiverID"]);
            m_timeSent = Convert.ToDateTime(dr["timeSent"]);
            m_subject = dr["subject"].ToString();
            m_body = dr["body"].ToString();
            m_attachment = Convert.ToBoolean(dr["attachment"]);
            m_readByReceiver = Convert.ToBoolean(dr["readByReceiver"]);

            if (HasAttachment)
            {
                m_mailAttachment = DAL.DBMail.GetMailAttachment(MailID);
            }
        }

        public GameMailMessage(int senderID, int receiverID, string subject, string body, bool attachment) : base()
        {
            m_senderID = senderID;
            m_receiverID = receiverID;
            m_subject = subject;
            m_body = body;
            m_attachment = attachment;
        } 
        #endregion

        // returns true if the mail has been validated
        private bool ValidateOutgoingMail()
        {
            // automated messages are automatically validated
            if (SenderID < 0)
                return true;

            PC receiver = PC.GetPC(ReceiverID);
            PC sender = PC.GetOnline(SenderID);

            // this should not occur, as if it is not an automated message these are verified elsewhere -- just in case
            if (receiver == null || sender == null) return false;

            // receiver is ignored by sender
            if (Array.IndexOf(receiver.ignoreList, sender.UniqueID) != -1)
            {
                sender.WriteToDisplay("The receiver of your mail currently has you on their ignore list.");
                return false;
            }

            // receiver is on sender's ignore list
            if (Array.IndexOf(sender.ignoreList, receiver.UniqueID) != -1)
            {
                sender.WriteToDisplay("The receiver of this mail is on your ignore list. You must remove them in order to send them mail.");
                return false;
            }

            // filter profanity
            if (receiver.filterProfanity)
            {
                //sender.WriteToDisplay("The receiver of your mail has their profanity filter turned on. The mail is being scanned for profanity...");

                Subject = Conference.FilterProfanity(Subject);
                Body = Conference.FilterProfanity(Body);
            }

            return true;
        }

        public void Send()
        {
            PC sender = PC.GetOnline(SenderID);

            // if mail is validated, send it (insert into database)
            // note that attachments have been validated by this point
            if (ValidateOutgoingMail())
            {
                TimeSent = DateTime.Now;

                MailID = DAL.DBMail.InsertMailMessage(this);

                if (HasAttachment)
                {
                    MailItemAttachment.MailID = MailID;
                    DAL.DBMail.InsertMailAttachment(MailItemAttachment);
                }

                PC receiver = PC.GetOnline(GetName(ReceiverID));

                if (receiver != null)
                {
                    if (receiver.PCState == Globals.ePlayerState.CONFERENCE || receiver.PCState == Globals.ePlayerState.PLAYING) // send a message
                    {
                        if (receiver.protocol != DragonsSpineMain.Instance.Settings.DefaultProtocol)
                            receiver.WriteToDisplay(GameWorld.Map.BWHT + "You have received a new mail message." + GameWorld.Map.CEND);
                        else receiver.WriteToDisplay("You have received a new mail message.");
                    }
                    else if (receiver.PCState == Globals.ePlayerState.MAILREADLISTING) // refresh mailbox list menu
                    {
                        Menu.PrintMailReceivedMessagesList(receiver);
                    }
                    else if (receiver.PCState == Globals.ePlayerState.MAINMENU) // refresh main menu
                    {
                        Menu.PrintMainMenu(receiver);
                    }

                    receiver.Mailbox.AddReceivedMessage(this);
                }

                if (sender != null)
                {
                    sender.WriteToDisplay("Your mail has been sent.");
                    sender.Mailbox.SentMessages.Add(this);
                }
            }
            else if (sender != null)
            {
                sender.WriteToDisplay("There was a problem with your mail message and it has not been sent. Please report this to the developers.");
            }            
        }

        public Item GetItemAttachment()
        {
            if (HasAttachment)
            {
                return MailItemAttachment.GetAttachedItem();
            }
            else return null;
        }

        public static string GetName(int id)
        {
            // all players have identification numbers greater than 0
            if (id > 0) return PC.GetName(id);

            // this could be added to later as more automated message types are conceived
            switch (id)
            {
                case GameMailMessage.AUTO_MESSAGE_ITEM_PAYMENT:
                    return "Item Payment";
                default:
                    return "System Message";
            }
        }

        public static string GetAutoItemPaymentSubject(string name)
        {
            return "Payment From " + name;
        }

        public static string GetAutoItemPaymentBody(string name)
        {
            return "This is an automated message. " + name + " has paid the requested amount for an item.";
        }
    }
}
