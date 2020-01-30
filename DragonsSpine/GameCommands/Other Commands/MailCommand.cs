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
using Map = DragonsSpine.GameWorld.Map;
using DragonsSpine.Mail.GameMail;

namespace DragonsSpine.Commands
{
    [CommandAttribute("mail", "Access the mail system at a mailbox.", (int)Globals.eImpLevel.USER, new string[] { },
        0, new string[] { "mail help" }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class MailCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            // character is not at a mailbox (all regular users must be at a mailbox)
            if (chr.PCState != Globals.ePlayerState.CONFERENCE && (!chr.CurrentCell.HasMailbox && (chr as PC).ImpLevel == Globals.eImpLevel.USER))
            {
                chr.WriteToDisplay("You are not standing next to a mailbox.");
                return true;
            }

            // args are null or empty
            if (args == null || args == "")
            {
                chr.WriteToDisplay("Type \"mail help\" for more information.");
                return true;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            if (sArgs.Length == 1)
            {
                string arg0 = sArgs[0].ToLower();
                // only "mail help" - "mail send" - "mail list" - "mail cancel" are single argument mail options
                switch (arg0)
                {
                    case "help":
                    case "h":
                    case "send":
                    case "s":
                    case "list":
                    case "l":
                    case "cancel":
                    case "ca":
                        break;
                    default:
                        chr.WriteToDisplay("Invalid mail command. Type \"mail help\" for more information.");
                        return true;
                }
            }

            switch (sArgs[0].ToLower())
            {
                case "help":
                    chr.WriteToDisplay("Mailbox Help:");
                    chr.WriteToDisplay("mail (h)elp - displays this help message");
                    chr.WriteToDisplay("mail (l)ist - list of all your received messages");
                    chr.WriteToDisplay("mail (l)ist (a)ttachments - list of all attachments in your mailbox");
                    chr.WriteToDisplay("mail (r)ead # - the # in the list to read");
                    chr.WriteToDisplay("mail (d)elete # - the # in the list to delete");
                    chr.WriteToDisplay("mail (cr)eate <receipient> <subject> - step 1 of message drafting");
                    chr.WriteToDisplay("mail (cr)eate <body> - step 2 of message drafting");
                    chr.WriteToDisplay("mail (a)ttach <left | right> <payment request amount> - step 3 of message drafting, optional");
                    chr.WriteToDisplay("mail (s)end - send your drafted message");
                    chr.WriteToDisplay("mail (ca)ncel - cancels a message draft, returning any item attachments");
                    chr.WriteToDisplay("mail (t)ake # - take # attachment in the attachments list");
                    chr.WriteToDisplay("** Warning: Message drafts with items must be sent or cancelled before you log out or the item will be lost.");
                    break;
                #region attach
                case "a":
                case "attach": // mail attach <left | right> <payment request amount>
                    {
                        if ((chr as PC).Mailbox.MessageDraft == null)
                        {
                            chr.WriteToDisplay("You do not have a message drafted. Type \"mail help\" for more information.");
                            return true;
                        }

                        if ((chr as PC).Mailbox.MessageDraft.HasAttachment)
                        {
                            chr.WriteToDisplay("Your message draft already has an attachment. Currently there is a limit of one attachment per message.");
                            return true;
                        }

                        // must have appropriate arguments
                        if (sArgs.Length < 2 || (sArgs[1].ToLower() != "left" && sArgs[1].ToLower() != "right"))
                        {
                            chr.WriteToDisplay("Invalid mail attach command. Type \"mail help\" for more information.");
                            return true;
                        }

                        // no item in left or right hand
                        if ((sArgs[1].ToLower() == "left" && chr.LeftHand == null) || (sArgs[1].ToLower() == "right" && chr.RightHand == null))
                        {
                            chr.WriteToDisplay("You do not have an item in your " + sArgs[1].ToLower() + " hand.");
                            return true;
                        }

                        double paymentRequest = 0;

                        if (sArgs.Length >= 3 && !Double.TryParse(sArgs[2], out paymentRequest))
                        {
                            chr.WriteToDisplay("The payment you requested is in the wrong format. Use numbers only.");
                            return true;
                        }

                        GameMailMessage message = (chr as PC).Mailbox.MessageDraft;

                        if (sArgs[1].ToLower() == "left")
                        {
                            if (GameMailbox.ValidateItemAttachment(chr.LeftHand.itemID, message.ReceiverID, message.SenderID))
                            {
                                string itemName = chr.LeftHand.name;

                                (chr as PC).Mailbox.MessageDraft.MailItemAttachment = new GameMailAttachment(chr.LeftHand, paymentRequest);
                                (chr as PC).Mailbox.MessageDraft.HasAttachment = true;

                                if (!chr.UnequipLeftHand(chr.LeftHand)) // backup measure just in case the item could not be unequipped
                                {
                                    (chr as PC).Mailbox.MessageDraft.MailItemAttachment = null;
                                    (chr as PC).Mailbox.MessageDraft.HasAttachment = false;
                                }
                                else chr.WriteToDisplay("You have attached your " + itemName + " to the message and requested " + paymentRequest + " coins as payment.");
                            }
                        }
                        else if (sArgs[1].ToLower() == "right")
                        {
                            if (GameMailbox.ValidateItemAttachment(chr.RightHand.itemID, message.ReceiverID, message.SenderID))
                            {
                                string itemName = chr.RightHand.name;

                                (chr as PC).Mailbox.MessageDraft.MailItemAttachment = new GameMailAttachment(chr.RightHand, paymentRequest);
                                (chr as PC).Mailbox.MessageDraft.HasAttachment = true;

                                if (!chr.UnequipRightHand(chr.RightHand)) // backup measure just in case the item could not be unequipped
                                {
                                    (chr as PC).Mailbox.MessageDraft.MailItemAttachment = null;
                                    (chr as PC).Mailbox.MessageDraft.HasAttachment = false;
                                }
                                else chr.WriteToDisplay("You have attached your " + itemName + " to the message and requested " + paymentRequest + " coins as payment.");
                            }
                        }
                        break;
                    }
                #endregion
                #region cancel
                case "ca":
                case "cancel":
                    {
                        GameMailbox mailbox = (chr as PC).Mailbox;

                        if (mailbox.MessageDraft != null)
                        {
                            if (mailbox.MessageDraft.HasAttachment) // remove attachment from a message draft, equip it or drop it on the ground
                            {
                                chr.EquipEitherHandOrDrop(mailbox.MessageDraft.MailItemAttachment.GetAttachedItem());
                                chr.WriteToDisplay("An item attachment has been removed from your message draft.");
                            }

                            mailbox.ClearMessageDraft();

                            chr.WriteToDisplay("Your message draft has been canceled.");
                        }
                        else chr.WriteToDisplay("There is no message draft to cancel.");
                        break;
                    }
                #endregion
                #region create
                case "cr":
                case "create": // mail create <recipient> <subject> OR mail create <body>
                    {
                        try
                        {
                            GameMailbox mailbox = (chr as PC).Mailbox;

                            if (mailbox.MessageDraft == null) // the message draft is null at login, and after a message draft is sent successfully
                            {
                                if (sArgs.Length < 3)
                                {
                                    chr.WriteToDisplay("Invalid format to create a new mail message. Type \"mail help\" for more information.");
                                    return true;
                                }

                                if (sArgs[1].ToLower() == chr.Name.ToLower() || !PC.PlayerExists(sArgs[1]))
                                {
                                    chr.WriteToDisplay("Invalid recipient.");
                                    return true;
                                }

                                mailbox.MessageDraft = new GameMailMessage(chr.UniqueID);
                                mailbox.MessageDraft.ReceiverID = PC.GetPlayerID(sArgs[1]);
                                // mail create <recipient> <subject>
                                mailbox.MessageDraft.Subject = args.Substring(args.IndexOf(sArgs[1]) + sArgs[1].Length + 1);
                                mailbox.MessageDraft.Subject.Trim();
                                // check subject length, automatically shorten it if too long
                                if (mailbox.MessageDraft.Subject.Length > GameMailMessage.MAX_SUBJECT_LENGTH)
                                {
                                    mailbox.MessageDraft.Subject = mailbox.MessageDraft.Subject.Substring(0, GameMailMessage.MAX_SUBJECT_LENGTH);
                                    mailbox.MessageDraft.Subject.Trim();
                                }

                                chr.WriteToDisplay("You have created a new mail message draft.");
                                chr.WriteToDisplay("To: " + GameMailMessage.GetName(mailbox.MessageDraft.ReceiverID));
                                chr.WriteToDisplay("Subject: " + mailbox.MessageDraft.Subject);
                                chr.WriteToDisplay("Use \"mail create <body>\" to add the message body then \"mail send\" to send it.");
                            }
                            else
                            {
                                mailbox.MessageDraft.Body = args.Substring(args.IndexOf(' ') + 1);
                                if (mailbox.MessageDraft.Body.Length > GameMailMessage.MAX_BODY_LENGTH)
                                {
                                    mailbox.MessageDraft.Body = mailbox.MessageDraft.Body.Substring(0, GameMailMessage.MAX_SUBJECT_LENGTH);
                                    mailbox.MessageDraft.Body.Trim();
                                }

                                chr.WriteToDisplay("Your new mail message draft is ready to be sent.");
                                chr.WriteToDisplay("To: " + GameMailMessage.GetName(mailbox.MessageDraft.ReceiverID));
                                chr.WriteToDisplay("Subject: " + mailbox.MessageDraft.Subject);
                                chr.WriteToDisplay(mailbox.MessageDraft.Body);
                                chr.WriteToDisplay("Use \"mail attach <right | left> <payment request amount>\" to attach an item now, or \"mail send\" now to send without an attachment");
                            }
                        }
                        catch (Exception e)
                        {
                            Utils.LogException(e);
                        }
                    }
                    break;
                #endregion
                #region send
                case "s":
                case "send": // send the currently drafted message in the player's mailbox
                    if ((chr as PC).Mailbox.MessageDraft == null)
                    {
                        chr.WriteToDisplay("You do not have a message drafted. Type \"mail help\" for more information.");
                        return true;
                    }
                    else
                    {
                        chr.WriteToDisplay("Sending your mail...");
                        (chr as PC).Mailbox.MessageDraft.Send();
                        (chr as PC).Mailbox.ClearMessageDraft();
                    }
                    break;
                #endregion
                #region list
                case "l":
                case "list":
                    {
                        if (sArgs.Length > 1 && sArgs[1].ToLower().StartsWith("a")) // list of attachments
                        {
                            #region list (a)ttachments
                            if ((chr as PC).Mailbox.AllMailAttachments.Count > 0)
                            {
                                chr.WriteToDisplay("Num  From            Item                      Payment        Date/Time");
                                chr.WriteToDisplay("-----------------------------------------------------------------------");
                                string timeStamp, itemName = "";
                                int counter = 1;
                                foreach (GameMailMessage message in (chr as PC).Mailbox.ReceivedMessages)
                                {
                                    if (message.HasAttachment)
                                    {
                                        timeStamp = message.TimeSent.Date.ToShortDateString();

                                        if (message.TimeSent == DateTime.Today) timeStamp = message.TimeSent.Date.ToShortTimeString();

                                        if (message.MailItemAttachment.ItemID != Item.ID_COINS)
                                        {
                                            itemName = Item.ItemDictionary[message.MailItemAttachment.ItemID]["name"].ToString();
                                        }
                                        else itemName = message.MailItemAttachment.CoinValue > 1 ? message.MailItemAttachment.CoinValue + " coins" : message.MailItemAttachment.CoinValue + " coin";

                                        chr.WriteToDisplay(counter + ".".PadRight(4) + GameMailMessage.GetName(message.SenderID).PadRight(16) +
                                            itemName.PadRight(26) + message.MailItemAttachment.PaymentRequested.ToString().PadRight(15) + timeStamp);

                                        counter++;
                                    }
                                }
                            }
                            else chr.WriteToDisplay("There are no item attachments in your mailbox.");
                            #endregion
                        }
                        else
                        {
                            chr.WriteToDisplay("Num  From             Subject                                  Date/Time");
                            chr.WriteToDisplay("------------------------------------------------------------------------");
                            string timeStamp = "";
                            int counter = 1;
                            string ifUnreadStart = "";
                            string ifUnreadEnd = "";
                            foreach (GameMailMessage message in (chr as PC).Mailbox.ReceivedMessages)
                            {
                                if (message.HasBeenReadByReceiver) { ifUnreadStart = ""; ifUnreadEnd = ""; } // highlight unread messages
                                else if (chr.protocol != DragonsSpineMain.Instance.Settings.DefaultProtocol)
                                {
                                    ifUnreadStart = Map.BWHT;
                                    ifUnreadEnd = Map.CEND;
                                }

                                timeStamp = message.TimeSent.Date.ToShortDateString();

                                if (message.TimeSent == DateTime.Today)
                                    timeStamp = message.TimeSent.Date.ToShortTimeString();

                                chr.WriteToDisplay(ifUnreadStart + counter.ToString() + ".".PadRight(4) + GameMailMessage.GetName(message.SenderID).PadRight(16) + " " + message.Subject.PadRight(GameMailMessage.MAX_SUBJECT_LENGTH + 1) + timeStamp + ifUnreadEnd);

                                if (message.HasAttachment)
                                {
                                    if(chr.protocol != DragonsSpineMain.Instance.Settings.DefaultProtocol)
                                        chr.WriteToDisplay(Map.BGRN + "     The above message has an item attachment." + Map.CEND);
                                    else chr.WriteToDisplay("     The above message has an item attachment.");
                                }
                                counter++;
                            }
                        }
                    }
                    break;
                #endregion
                #region read
                case "r":
                case "read":
                    {
                        int messageReadChoice;
                        if (!Int32.TryParse(sArgs[1], out messageReadChoice))
                        {
                            chr.WriteToDisplay("Invalid mail message number.");
                            return true;
                        }
                        else if (messageReadChoice <= 0 || messageReadChoice > (chr as PC).Mailbox.ReceivedMessages.Count)
                        {
                            chr.WriteToDisplay("Invalid mail message number.");
                        }
                        else
                        {
                            GameMailbox mailbox = (chr as PC).Mailbox;
                            mailbox.MessageCurrentlyReading = mailbox.ReceivedMessages[messageReadChoice - 1];
                            mailbox.MessageCurrentlyReading.HasBeenReadByReceiver = true;
                            mailbox.UnreadMessages.Remove(mailbox.MessageCurrentlyReading);
                            // attachments cannot be removed from messages at the menus, must be at a mailbox in the game world
                            DAL.DBMail.UpdateMailMessage(mailbox.MessageCurrentlyReading);

                            chr.WriteToDisplay("From: " + GameMailMessage.GetName(mailbox.MessageCurrentlyReading.SenderID));
                            chr.WriteToDisplay("Subject: " + mailbox.MessageCurrentlyReading.Subject);
                            chr.WriteToDisplay(mailbox.MessageCurrentlyReading.TimeSent.ToUniversalTime().ToShortDateString() + " " + mailbox.MessageCurrentlyReading.TimeSent.ToUniversalTime().ToShortTimeString() + " UTC");
                            if (mailbox.MessageCurrentlyReading.HasAttachment)
                            {
                                Item attachment = Item.CopyItemFromDictionary(mailbox.MessageCurrentlyReading.MailItemAttachment.ItemID);
                                chr.WriteToDisplay("Item Attachment: " + attachment.name);
                            }
                            chr.WriteToDisplay("Body:");
                            chr.WriteToDisplay(mailbox.MessageCurrentlyReading.Body);
                            mailbox.ClearMessageCurrentlyReading();
                        }
                    }
                    break;
                #endregion
                #region delete
                case "d":
                case "delete":
                    {
                        if (sArgs.Length < 2)
                        {
                            chr.WriteToDisplay("Invalid mail message number. Type \"mail help\" for more information.");
                            return true;
                        }

                        int messageReadChoice;

                        if (!Int32.TryParse(sArgs[1], out messageReadChoice))
                        {
                            chr.WriteToDisplay("Invalid mail message number.");
                            return true;
                        }
                        else if (messageReadChoice <= 0 || messageReadChoice > (chr as PC).Mailbox.ReceivedMessages.Count)
                        {
                            chr.WriteToDisplay("Invalid mail message number.");
                        }
                        else
                        {
                            if ((chr as PC).Mailbox.ReceivedMessages[messageReadChoice - 1].HasAttachment)
                            {
                                chr.WriteToDisplay("The message you chose to delete has an item attachment. Take the item and then try again.");
                                return true;
                            }
                            else
                            {
                                (chr as PC).Mailbox.DeleteReceivedMessage((chr as PC).Mailbox.ReceivedMessages[messageReadChoice - 1]);
                                chr.WriteToDisplay("The message has been deleted.");
                            }
                        }
                    }
                    break;
                #endregion
                #region take
                case "t":
                case "take":
                    {
                        if (sArgs.Length < 2)
                        {
                            chr.WriteToDisplay("Invalid mail message number. Type \"mail help\" for more information.");
                            return true;
                        }

                        int attachmentTakeChoice;

                        if (!Int32.TryParse(sArgs[1], out attachmentTakeChoice))
                        {
                            chr.WriteToDisplay("Invalid attachment number.");
                            return true;
                        }
                        else if (attachmentTakeChoice <= 0 || attachmentTakeChoice > (chr as PC).Mailbox.AllMailAttachments.Count + (chr as PC).Mailbox.NumberOfCoinAttachments)
                        {
                            chr.WriteToDisplay("Invalid attachment number.");
                        }
                        else
                        {
                            int counter = 1;

                            foreach (GameMailMessage message in (chr as PC).Mailbox.ReceivedMessages)
                            {
                                if (message.HasAttachment && counter == attachmentTakeChoice)
                                {
                                    // coins are put directly into the bank account, then the mail attachment is deleted
                                    if (message.MailItemAttachment.ItemID == Item.ID_COINS)
                                    {
                                        (chr as PC).bankGold += message.MailItemAttachment.CoinValue;
                                        chr.WriteToDisplay(message.MailItemAttachment.CoinValue + " coins have been deposited into your bank account.");
                                        (chr as PC).Mailbox.DeleteMailAttachment(message.MailItemAttachment);
                                        message.MailItemAttachment = null;
                                        message.HasAttachment = false;
                                        DAL.DBMail.UpdateMailMessage(message);
                                        return true;
                                    }

                                    // check for coins available -- auto banking
                                    if (message.MailItemAttachment.PaymentRequested > 0 && (chr as PC).bankGold >= message.MailItemAttachment.PaymentRequested)
                                    {
                                        (chr as PC).bankGold -= message.MailItemAttachment.PaymentRequested;

                                        PC paymentRequestor = PC.GetOnline(message.SenderID);

                                        if (paymentRequestor != null)
                                        {
                                            paymentRequestor.bankGold += message.MailItemAttachment.PaymentRequested;
                                            paymentRequestor.WriteToDisplay(chr.Name + " has paid the requested amount for an item.");
                                            paymentRequestor.WriteToDisplay(message.MailItemAttachment.PaymentRequested + " coins have been deposited into your bank account.");
                                        }
                                        else
                                        {
                                            GameMailMessage autoMessage = new GameMailMessage(GameMailMessage.AUTO_MESSAGE_ITEM_PAYMENT,
                                                message.SenderID, GameMailMessage.GetAutoItemPaymentSubject(chr.Name), GameMailMessage.GetAutoItemPaymentBody(chr.Name), true);
                                            Item coins = Item.CopyItemFromDictionary(Item.ID_COINS);
                                            coins.coinValue = message.MailItemAttachment.PaymentRequested;
                                            autoMessage.MailItemAttachment = new GameMailAttachment(coins);
                                            autoMessage.Send();
                                        }

                                    }
                                    else if (message.MailItemAttachment.PaymentRequested > 0) // not enough coins in the bank
                                    {
                                        chr.WriteToDisplay("You do not have enough coins in the bank for the requested payment amount.");
                                        return true;
                                    }

                                    GameMailbox mailbox = (chr as PC).Mailbox;
                                    mailbox.MessageCurrentlyReading = message;
                                    if (mailbox.UnreadMessages.Contains(mailbox.MessageCurrentlyReading))
                                        mailbox.UnreadMessages.Remove(mailbox.MessageCurrentlyReading);
                                    chr.WriteToDisplay("From: " + GameMailMessage.GetName(mailbox.MessageCurrentlyReading.SenderID));
                                    chr.WriteToDisplay("Subject: " + mailbox.MessageCurrentlyReading.Subject);
                                    chr.WriteToDisplay(mailbox.MessageCurrentlyReading.TimeSent.ToUniversalTime().ToShortDateString() + " " + mailbox.MessageCurrentlyReading.TimeSent.ToUniversalTime().ToShortTimeString() + " UTC");
                                    chr.WriteToDisplay(mailbox.MessageCurrentlyReading.Body);
                                    message.HasBeenReadByReceiver = true;

                                    Item item = message.MailItemAttachment.GetAttachedItem();

                                    if (item.itemType != Globals.eItemType.Coin)
                                    {
                                        if (!chr.EquipEitherHandOrDrop(item))
                                        {
                                            chr.WriteToDisplay(Map.BGRN + "The " + item.name + " has been removed from the message and placed at your feet." + Map.CEND);
                                        }
                                        else chr.WriteToDisplay(Map.BGRN + "You remove the " + item.name + " from the message." + Map.CEND);
                                        message.HasAttachment = false;
                                        message.MailItemAttachment = null;
                                        DAL.DBMail.DeleteMailAttachment(message.MailID);
                                    }
                                    else
                                    {
                                        (chr as PC).bankGold += item.coinValue;
                                        chr.WriteToDisplay(item.coinValue + " coins have been deposited directly into your bank account.");
                                        message.HasAttachment = false;
                                        message.MailItemAttachment = null;
                                        DAL.DBMail.DeleteMailAttachment(message.MailID);
                                    }

                                    DAL.DBMail.UpdateMailMessage(mailbox.MessageCurrentlyReading);
                                }
                            }
                        }
                        // verify number
                        // check payment - look in hands for coins, then sack, then bank
                        // create item, delete attachment (don't delete the mail)
                        break;
                    }
                #endregion
                default:
                    break;
            }

            return true;
        }
    }
}
