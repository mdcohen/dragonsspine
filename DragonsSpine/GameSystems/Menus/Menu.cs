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
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using DragonsSpine.GameWorld;
using DragonsSpine.Mail.GameMail;

namespace DragonsSpine
{
    public class Menu
    {
        public static void PrintMainMenu(PC ch)
        {
            DateTime lastOnline = Account.GetLastOnline(ch.Account.accountID);

            #region If using APP_PROTOCOL
            if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
            {
                ch.Write(ProtocolYuusha.DETECT_CLIENT);

                ProtocolYuusha.sendWorldNews(ch);

                if (!ch.sentWorldInformation)
                {
                    ch.Write(ProtocolYuusha.WORLD_INFORMATION);
                    ProtocolYuusha.SendWorldVersion(ch);  // stored client side, also has round delay
                    ProtocolYuusha.SendWorldSpells(ch); // stored client side
                    ProtocolYuusha.SendWorldTalents(ch); // stored client side
                    //ProtocolYuusha.sendWorldLands(ch);
                    //ProtocolYuusha.sendWorldMaps(ch);
                    //ProtocolYuusha.sendWorldCharGen(ch);
                    ProtocolYuusha.SendAccountInfo(ch);
                    ProtocolYuusha.SendCharacterList(ch);

                    // World info is not currently used in Yuusha as of 12/28/2016 -Eb

                    ch.sentWorldInformation = true;
                }

                if (ch.PCState == Globals.ePlayerState.MAINMENU)
                {
                    ch.Write(ProtocolYuusha.MENU_MAIN);
                }

                ProtocolYuusha.SendCurrentCharacterID(ch);

                //ProtocolYuusha.SendUserList(ch);
            }
            #endregion
            else
            {
                if (ch.protocol != "old-kesmai")
                    ch.Write(ProtocolYuusha.DETECT_PROTOCOL);

                Map.ClearMap(ch);
                ch.WriteLine(DragonsSpineMain.Instance.Settings.ServerName + " (" + DragonsSpineMain.Instance.Settings.ServerVersion + ") Main Menu");

                if (lastOnline.ToString() == "1/1/1900 12:00:00 AM")
                    ch.WriteLine("Welcome, " + ch.Account.accountName + "!                                         ");
                else
                {
                    ch.WriteLine("Welcome back, " + ch.Account.accountName + "!                                    ");
                    ch.WriteLine("Your last visit to " + DragonsSpineMain.Instance.Settings.ServerName + " was on " + lastOnline.ToShortDateString() + " at " + lastOnline.ToShortTimeString() + ".");
                }

                // NOTE: SPINEL DETECTS THE BELOW LINE
                ch.WriteLine("Current Character: " + ch.Name + " Level: " + ch.Level + " Class: " + ch.classFullName + " Land: " + ch.Land.Name + " Map: " + ch.Map.Name);

                // new mail message
                if ((ch as PC).Mailbox.HasUnreadMail)
                {
                    string plural = (ch as PC).Mailbox.UnreadMessages.Count > 1 ? "messages" : "message";
                    ch.WriteLine(Map.CLRLN + Map.BWHT + "You have " + (ch as PC).Mailbox.UnreadMessages.Count + " unread " + plural + " in your mailbox." + Map.CEND);
                }
                ch.WriteLine("");
                ch.WriteLine("1. Enter Game");
                ch.WriteLine("2. Enter Conference Room");
                ch.WriteLine("3. Disconnect");
                ch.WriteLine("4. View Account");
                ch.WriteLine("5. Change Protocol (" + ch.protocol + ")");
                ch.WriteLine("6. Change/Create/Delete Character");
                ch.WriteLine("7. Mail Menu");
                ch.WriteLine("");
                ch.Write("Command: ");
            }

            Account.SetLastOnline(ch.Account.accountID);
        }

        public static void PrintCharMenu(Character ch)
        {
            if(ch.protocol != DragonsSpineMain.Instance.Settings.DefaultProtocol)
            {
                ch.WriteLine(DragonsSpineMain.Instance.Settings.ServerName + " (" + DragonsSpineMain.Instance.Settings.ServerVersion + ") Character Menu");
                ch.WriteLine("Current Character: " + ch.Name + " Level: " + ch.Level + " Class: " + ch.classFullName + " Land: " + ch.Land.Name + " Map: " + ch.Map.Name);
                ch.WriteLine("");
                ch.WriteLine("1. Create New Character");
                ch.WriteLine("2. Change Character");
                ch.WriteLine("3. Delete Character");
                ch.WriteLine("4. Return to Main Menu");
                ch.WriteLine("");
                ch.Write("Command: ");
            }
        }

        public static void PrintAccountMenu(PC ch, string message)
        {
            if(ch.protocol != DragonsSpineMain.Instance.Settings.DefaultProtocol)
            {
                Map.ClearMap(ch);
                ch.WriteLine(DragonsSpineMain.Instance.Settings.ServerName + " (" + DragonsSpineMain.Instance.Settings.ServerVersion + ") Account Menu");
                ch.WriteLine("");
                ch.WriteLine("Account: " + ch.Account.accountName + " Current Marks: " + (ch as PC).currentMarks);
                ch.WriteLine("");
                if(message != ""){ch.WriteLine(message); ch.WriteLine("");}
                ch.WriteLine("1. Change Password");
                ch.WriteLine("2. Return to Main Menu");
                ch.WriteLine("");
            }
        }

        public static void PrintMailMenu(Character ch)
        {
            if (ch.protocol != DragonsSpineMain.Instance.Settings.DefaultProtocol)
            {
                GameMailbox mailbox = (ch as PC).Mailbox;
                Map.ClearMap(ch);
                ch.WriteLine(DragonsSpineMain.Instance.Settings.ServerName + " (" + DragonsSpineMain.Instance.Settings.ServerVersion + ") Mail Menu");
                ch.WriteLine("Current Character: " + ch.Name + " Level: " + ch.Level + " Class: " + ch.classFullName + " Land: " + ch.Land.Name + " Map: " + ch.Map.Name);
                //ch.WriteLine("Current Character: " + ch.Name);
                ch.WriteLine("Mail Messages: " + mailbox.ReceivedMessages.Count + " Received, " + mailbox.UnreadMessages.Count + " Unread");
                ch.WriteLine("You have " + ((ch as PC).Mailbox.AllMailAttachments.Count - (ch as PC).Mailbox.NumberOfCoinAttachments) + " of " + GameMailbox.MAX_ATTACHMENTS_PER_PLAYER + " maximum item attachments in your mailbox.");
                ch.WriteLine("");
                ch.WriteLine("1. Read Mail");
                ch.WriteLine("2. Send Mail");
                ch.WriteLine("3. Return to Main Menu");
                ch.WriteLine("");
                if (!ch.usingClient) { ch.Write("Command: "); }
            }
        }

        public static void PrintMailReceivedMessagesList(Character ch)
        {
            if (ch.protocol != DragonsSpineMain.Instance.Settings.DefaultProtocol)
            {
                GameMailbox mailbox = (ch as PC).Mailbox;

                Map.ClearMap(ch);

                ch.WriteLine(DragonsSpineMain.Instance.Settings.ServerName + " (" + DragonsSpineMain.Instance.Settings.ServerVersion + ") Received Messages List");
                ch.WriteLine("");
                ch.WriteLine("Current Character: " + ch.Name);
                ch.WriteLine("");
                ch.WriteLine("Received Messages: " + mailbox.ReceivedMessages.Count);
                ch.WriteLine("");
                ch.WriteLine("Num  From             Subject                                  Date/Time");
                ch.WriteLine("------------------------------------------------------------------------");
                string timestamp = "";
                int counter = 1;
                string ifUnreadStart = Map.BWHT;
                string ifUnreadEnd = Map.CEND;
                foreach (GameMailMessage message in mailbox.ReceivedMessages)
                {
                    if (message.HasBeenReadByReceiver) { ifUnreadStart = ""; ifUnreadEnd = ""; } // highlight unread messages
                    else { ifUnreadStart = Map.BWHT; ifUnreadEnd = Map.CEND; }
                    timestamp = message.TimeSent.Date.ToShortDateString();
                    if(message.TimeSent == DateTime.Today) timestamp = message.TimeSent.Date.ToShortTimeString();
                    ch.WriteLine(ifUnreadStart + counter.ToString() + ".".PadRight(4) + GameMailMessage.GetName(message.SenderID).PadRight(16) + " " + message.Subject.PadRight(GameMailMessage.MAX_SUBJECT_LENGTH + 1) + timestamp + ifUnreadEnd);
                    if (message.HasAttachment)
                    {
                        ch.WriteLine("    " + Map.BGRN + "There is a game item attached to the above message." + Map.CEND);
                        ch.WriteLine("    " + Map.BGRN + "Visit a mailbox in the game world to take it." + Map.CEND);
                    }
                    counter++;
                }
                ch.WriteLine("");
                ch.Write("Enter the message number or 'q' to return to the mail menu: ");
            }
        }

        public static void PrintMailSendStep1(Character ch, bool invalidName)
        {
            if (invalidName)
                ch.WriteLine("The name you entered is invalid.");
            else
            {
                (ch as PC).Mailbox.MessageDraft = new GameMailMessage(ch.UniqueID);
                Map.ClearMap(ch);
                ch.WriteLine("You are now composing a mail message.");
                ch.WriteLine("Messages cannot be addressed to yourself.");
                ch.WriteLine("Items can be attached to a message when using a mailbox in the game world.");
                ch.WriteLine("Type \"quitnow\" at any time to return to the mail menu.");
            }
            ch.Write("Recipient: ");
        }

        public static void PrintMailSendStep2(Character ch, bool invalidSubject)
        {
            if (invalidSubject)
            {
                ch.WriteLine("The subject you entered is invalid. There is a " + GameMailMessage.MAX_SUBJECT_LENGTH + " character limit.");
                ch.WriteLine("Subject: " + (ch as PC).Mailbox.MessageDraft.Subject);
            }
            else ch.Write("Subject: " + (ch as PC).Mailbox.MessageDraft.Subject);
        }

        public static void PrintMailSendStep3(Character ch, bool invalidBody)
        {
            if (invalidBody)
            {
                ch.WriteLine("The body you entered was too long. There is a " + GameMailMessage.MAX_BODY_LENGTH + " character limit.");
            }
            else
            {
                ch.WriteLine("Type the body of your message below. Press enter when you are ready to review and send.");
                ch.WriteLine("");
            }
            ch.Write((ch as PC).Mailbox.MessageDraft.Body);
        }

        public static void PrintMailSendStep4(Character ch)
        {
            GameMailMessage reading = (ch as PC).Mailbox.MessageDraft;

            //Map.clearMap(ch);
            ch.WriteLine("");
            ch.WriteLine("To: " + GameMailMessage.GetName(reading.ReceiverID));
            ch.WriteLine("From: " + GameMailMessage.GetName(reading.SenderID));
            ch.WriteLine("Subject: " + reading.Subject);
            //ch.WriteLine("Date/Time: " + reading.TimeSent.ToShortDateString() + " " + reading.TimeSent.ToShortTimeString());
            ch.WriteLine("");
            ch.WriteLine(reading.Body);
            ch.WriteLine("");
            ch.WriteLine("Choices: (S)end or (Q)uit");
            ch.Write("Command: ");
        }

        public static void PrintMailMessage(Character ch, int messageIndex)
        {
            (ch as PC).Mailbox.MessageCurrentlyReading = (ch as PC).Mailbox.ReceivedMessages[messageIndex];
            (ch as PC).Mailbox.MessageCurrentlyReading.HasBeenReadByReceiver = true;
            (ch as PC).Mailbox.UnreadMessages.Remove((ch as PC).Mailbox.MessageCurrentlyReading);
            // attachments cannot be removed from messages at the menus, must be at a mailbox in the game world
            DAL.DBMail.UpdateMailMessage((ch as PC).Mailbox.MessageCurrentlyReading);

            Map.ClearMap(ch);
            ch.WriteLine("");
            ch.WriteLine("From: " + GameMailMessage.GetName((ch as PC).Mailbox.MessageCurrentlyReading.SenderID));
            ch.WriteLine("Subject: " + (ch as PC).Mailbox.MessageCurrentlyReading.Subject);
            ch.WriteLine((ch as PC).Mailbox.MessageCurrentlyReading.TimeSent.ToUniversalTime().ToShortDateString() + " " + (ch as PC).Mailbox.MessageCurrentlyReading.TimeSent.ToUniversalTime().ToShortTimeString() + " UTC");
            if ((ch as PC).Mailbox.MessageCurrentlyReading.HasAttachment)
            {
                ch.WriteLine(Map.BGRN + "There is a game item attached to this message. Visit a mailbox in the game world to take it." + Map.CEND);
            }
            ch.WriteLine("");
            ch.WriteLine((ch as PC).Mailbox.MessageCurrentlyReading.Body);
            ch.WriteLine("");
            ch.WriteLine("Choices: (R)eply, (F)orward, (DELETE), or (Q)uit back to your messages list.");
            ch.Write("Command: ");
        }
    }
}