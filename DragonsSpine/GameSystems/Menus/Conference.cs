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

namespace DragonsSpine
{
    public class Conference
    {
        public static string[] rooms = { "A", "B", "C", "D", "E", "F", "AGM", "GM", "DEVJR", "DEV" };
        public static Globals.eImpLevel[] roomsLevel = { Globals.eImpLevel.USER, Globals.eImpLevel.USER, Globals.eImpLevel.USER, Globals.eImpLevel.USER,
                                                           Globals.eImpLevel.USER, Globals.eImpLevel.USER, Globals.eImpLevel.AGM, Globals.eImpLevel.GM,
                                                           Globals.eImpLevel.DEVJR, Globals.eImpLevel.DEV };

        public static string[] ProfanityArray = {"fuck","shit","dick","cunt","pussy","fag","prick","penis","crap","asshole","butt","tits",
												   "boobs","jackoff","suck","sex","intercourse","anal","masturbate","masterbate",
												   "wtf","bitch","cock"};


        public static void Header(PC ch, bool enterMessage)
        {
            string confroom = rooms[ch.confRoom];
            ArrayList room_players = GetAllInRoom(ch);
            
            if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
            {
                String roomsList = "";
                for (int a = 0; a < rooms.Length; a++)
                {
                    roomsList += rooms[a] + ProtocolYuusha.VSPLIT;
                }
                roomsList = roomsList.Substring(0, roomsList.Length - ProtocolYuusha.VSPLIT.Length);

                if (enterMessage)
                {
                    ch.Write(ProtocolYuusha.CONF_ENTER);
                }
                // send conference info -- current room letter, room players count, room letters list
                ch.Write(ProtocolYuusha.CONF_INFO + "Room " + rooms[ch.confRoom] + ProtocolYuusha.ISPLIT + room_players.Count + ProtocolYuusha.ISPLIT + roomsList + ProtocolYuusha.CONF_INFO_END);
            }
            else if(ch.protocol == "Proto")
            {

            }
            else
            {
                Map.ClearMap(ch);
            }

            ch.WriteLine("You are in Conference Room " + rooms[ch.confRoom] + ".", ProtocolYuusha.TextType.Header);

            switch (room_players.Count)
            {
                case 0:
                    ch.WriteLine("You are the only player present.", ProtocolYuusha.TextType.Header);
                    break;
                case 1:
                    ch.WriteLine("There is one player in the room.", ProtocolYuusha.TextType.Header);
                    break;
                default:
                    ch.WriteLine("There are " + room_players.Count.ToString() + " players in the room.", ProtocolYuusha.TextType.Header);
                    break;
            }

            ch.WriteLine("Type /help for a list of commands.", ProtocolYuusha.TextType.Header);

            if (ch.IsInvisible)
            {
                ch.WriteLine("******* You are invisible. *******", ProtocolYuusha.TextType.System);
            }

            if (!ch.IsInvisible && enterMessage)
            {
                ch.SendToAllInConferenceRoom(GetStaffTitle(ch as PC) + ch.Name + " has entered the room.", ProtocolYuusha.TextType.Enter);
            }

            //Protocol.UpdateUserLists();
        }

        public static void ChatCommands(PC ch, string command, string args)
        {
            string all = command + " " + args;

            all = all.Replace(",  ", " ");
            all = all.Trim();

            bool match = false;

            if (ch != null)
            {
                if (all.ToLower() == "/echo on") { ch.echo = true; ch.WriteLine("Echo Enabled.", ProtocolYuusha.TextType.Status); return; }
                if (all.ToLower() == "/echo off") { ch.echo = false; ch.WriteLine("Echo disabled.", ProtocolYuusha.TextType.Status); return; }

                if (command.StartsWith(Convert.ToString((char)27)) || command.IndexOf((char)27) != -1)
                {
                    if (!ProtocolYuusha.CheckChatRoomCommand(ch, command, args))
                    {
                        //to prevent users from sending protocol messages
                        Utils.Log(ch.GetLogString() + " attempted to send conference room message with protocol. Command: " + command + args, Utils.LogType.SystemWarning);
                        SendInvalidCommand(ch);
                    }
                }
            }

            if (command.StartsWith("/") || all.StartsWith("/"))
            {
                command = command.Trim();
                args = args.Trim();

                if (DragonsSpine.Commands.GameCommand.GameCommandAliases.ContainsKey(command.Replace("/", "")))
                    command = DragonsSpine.Commands.GameCommand.GameCommandAliases[command.Replace("/", "")];
                else if (DragonsSpine.Commands.GameCommand.GameCommandAliases.ContainsKey(all.Replace("/", "")))
                    command = DragonsSpine.Commands.GameCommand.GameCommandAliases[all.Replace("/", "")];
                else if (DragonsSpine.Commands.GameCommand.GameCommandDictionary.ContainsKey(all.Replace("/", "")))
                    command = all.Replace("/", "");
                else if (DragonsSpine.Commands.GameCommand.GameCommandDictionary.ContainsKey(command.Replace("/", "")))
                    command = command.Replace("/", "");

                if (DragonsSpine.Commands.GameCommand.GameCommandDictionary.ContainsKey(command))
                {
                    if(!DragonsSpine.Commands.GameCommand.GameCommandDictionary[command].Handler.OnCommand(ch, args))
                        Conference.SendInvalidCommand(ch);
                    return;
                }
                else
                {
                    if (!command.StartsWith("/")) command = "/" + command;

                    switch (command.ToLower())
                    {
                        #region /time
                        case "/time":
                            ch.WriteLine("The time is now " + DateTime.Now.ToString() + ".", ProtocolYuusha.TextType.Status);
                            break;
                        #endregion
                        #region /list
                        case "/characters":
                        case "/characterlist":
                        case "/list":
                            ch.WriteLine("", ProtocolYuusha.TextType.Listing);
                            //string characterListing = PC.FormatCharacterList(ch.accountID, false, ch);
                            if (ch.protocol != DragonsSpineMain.Instance.Settings.DefaultProtocol)
                                ch.WriteLine(PC.FormatCharacterList(ch.Account.players, false, ch));
                            else
                            {
                                string[] protoCharacterListing = PC.FormatCharacterList(ch.Account.players, false, ch).Replace("\n\r", "|").Split("|".ToCharArray());
                                foreach (string line in protoCharacterListing)
                                {
                                    ch.WriteLine(line, ProtocolYuusha.TextType.Listing);
                                }
                            }
                            ch.WriteLine("", ProtocolYuusha.TextType.Listing);
                            break;
                        #endregion
                        #region /lottery
                        case "/lottery":
                        case "/lotto":
                            ch.WriteLine("", ProtocolYuusha.TextType.Listing);
                            foreach (Land land in World.GetFacetByIndex(0).Lands)
                            {
                                if (land.LandID != Land.ID_UNDERWORLD)
                                {
                                    ch.WriteLine(land.Name + " Lottery", ProtocolYuusha.TextType.Listing);
                                    ch.WriteLine("Amount: " + land.Lottery, ProtocolYuusha.TextType.Listing);
                                    //ch.WriteLine(land.Name + " lottery jackpot is currently " + land.Lottery + " gold coins.", Protocol.TextType.Listing);
                                    if (ch.ImpLevel >= Globals.eImpLevel.AGM && land.LotteryParticipants.Count > 0)
                                    {
                                        Dictionary<string, int> lotto = new Dictionary<string, int>(); // Player name, # chances

                                        land.LotteryParticipants.ForEach(delegate(int id)
                                        {
                                            string name = PC.GetName(id);
                                            if (!lotto.ContainsKey(name))
                                            {
                                                lotto.Add(name, 1);
                                            }
                                            else { lotto[name]++; }
                                        });

                                        string names = "";

                                        foreach (string name in lotto.Keys)
                                            names += name + " x " + lotto[name] + ", ";

                                        names = names.Substring(0, names.Length - 2);

                                        ch.WriteLine("Participants: " + names, ProtocolYuusha.TextType.Listing);
                                    }
                                    else if (ch.ImpLevel == Globals.eImpLevel.USER && land.LotteryParticipants.Contains(ch.UniqueID))
                                    {
                                        ch.WriteLine("You are a participant in " + land.LongDesc + " lottery.", ProtocolYuusha.TextType.Listing);
                                    }
                                    ch.WriteLine("", ProtocolYuusha.TextType.Listing);
                                }

                            }
                            break;
                        #endregion
                        #region /switch
                        case "/switch":
                            if (args == null) { ch.WriteLine("Usage: /switch #   Use /list to see a list of your characters.", ProtocolYuusha.TextType.Error); break; }
                            else
                            {
                                string[] sArgs = args.Split(" ".ToCharArray());

                                string oldName = ch.Name;
                                bool wasInvisible = ch.IsInvisible;

                                string oldLogString = ch.GetLogString();
                                if (PC.SelectNewCharacter(ch, Convert.ToInt32(sArgs[0])))
                                {
                                    Utils.Log(oldLogString, Utils.LogType.Logout);
                                    Utils.Log(ch.GetLogString(), Utils.LogType.Login);
                                    if (!wasInvisible && !ch.IsInvisible) { ch.SendToAllInConferenceRoom(oldName + " has switched to " + ch.Name + ".", ProtocolYuusha.TextType.Enter); } // do not send if switched player was invisible
                                    else if (wasInvisible && !ch.IsInvisible) // but instead send a message that the new character entered (if visible)
                                    {
                                        ch.SendToAllInConferenceRoom(GetStaffTitle(ch as PC) + ch.Name + " has entered the room.", ProtocolYuusha.TextType.Enter);
                                    }
                                    else if (!wasInvisible && ch.IsInvisible)
                                    {
                                        ch.SendToAllInConferenceRoom(GetStaffTitle(ch as PC) + ch.Name + " has left the world.", ProtocolYuusha.TextType.Exit);
                                    }
                                    ch.WriteLine("You have switched to your character named " + ch.Name + ".", ProtocolYuusha.TextType.Status);
                                    (ch as PC).lastOnline = DateTime.UtcNow; // set last online
                                    PC.SaveField(ch.UniqueID, "lastOnline", (ch as PC).lastOnline, null);

                                    if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                                        ProtocolYuusha.SendCurrentCharacterID(ch);
                                }
                                else
                                {
                                    ch.WriteLine("Invalid character.", ProtocolYuusha.TextType.Error);
                                }
                            }
                            break;
                        #endregion
                        #region /filter
                        case "/filter":
                            if (ch.filterProfanity)
                            {
                                ch.filterProfanity = false;
                                ch.WriteLine("Your profanity filter is now OFF.", ProtocolYuusha.TextType.Status);
                            }
                            else
                            {
                                ch.filterProfanity = true;
                                ch.WriteLine("Your profanity filter is now ON.", ProtocolYuusha.TextType.Status);
                            }
                            PC.SaveField(ch.UniqueID, "filterProfanity", ch.filterProfanity, null);
                            break;
                        #endregion
                        #region /afk
                        case "/away":
                        case "/afk":
                            if (ch.afk)
                            {
                                ch.afk = false;
                                ch.WriteLine("You are no longer AFK.", ProtocolYuusha.TextType.Status);
                                if(!ch.IsInvisible)
                                    ch.SendToAllInConferenceRoom(ch.Name + " is no longer AFK.", ProtocolYuusha.TextType.Status);
                            }
                            else
                            {
                                ch.afk = true;
                                //if (args != null && args != "" && args.IndexOf((char)27) == -1) // do not allow escape characters
                                //{
                                //    if (args.Length > 255) { args = args.Substring(0, 255); } // limit length of afk message
                                //    ch.afkMessage = args;
                                //    ch.WriteLine("You are now AFK.", Protocol.TextType.Status);
                                //}
                                //else
                                //{
                                ch.WriteLine("You are now AFK.", ProtocolYuusha.TextType.Status);
                                //}

                                if(!ch.IsInvisible)
                                    ch.SendToAllInConferenceRoom(ch.Name + " is now AFK.", ProtocolYuusha.TextType.Status);
                            }
                            break;
                        #endregion
                        #region /echo
                        case "/echo":
                            if (ch.echo)
                            {
                                ch.echo = false;
                                ch.WriteLine("Echo disabled.", ProtocolYuusha.TextType.Status);
                            }
                            else
                            {
                                ch.echo = true;
                                ch.WriteLine("Echo enabled.", ProtocolYuusha.TextType.Status);
                            }
                            PC.SaveField(ch.UniqueID, "echo", ch.echo, null);
                            break;
                        #endregion
                        #region /anon
                        case "/anon":
                        case "/anonymous":
                            if (ch.IsAnonymous)
                            {
                                ch.IsAnonymous = false;
                                ch.WriteLine("You are no longer anonymous.", ProtocolYuusha.TextType.Status);
                            }
                            else
                            {
                                ch.IsAnonymous = true;
                                ch.WriteLine("You are now anonymous.", ProtocolYuusha.TextType.Status);
                            }
                            PC.SaveField(ch.UniqueID, "anonymous", ch.IsAnonymous, null);
                            break;
                        #endregion
                        #region /enter
                        case "/ent":
                        case "/play":
                        case "/enter":
                            if (DragonsSpineMain.ServerStatus == DragonsSpineMain.ServerState.Running || ch.ImpLevel >= Globals.eImpLevel.GM)
                            {
                                ch.PCState = Globals.ePlayerState.PLAYING;
                                ch.RemoveFromConf();
                                ch.AddToWorld();
                                Map.ClearMap(ch);
                                if (ch.protocol == "old-kesmai") { ch.Write(Map.KP_ENHANCER_ENABLE + Map.KP_ERASE_TO_END); ch.updateAll = true; }

                                if (ch.IsAnonymous && !ch.IsInvisible)
                                {
                                    ch.SendToAllInConferenceRoom(GetStaffTitle(ch as PC) + ch.Name + " has left for the lands.", ProtocolYuusha.TextType.Exit);
                                }
                                else if (!ch.IsAnonymous && !ch.IsInvisible)
                                {
                                    ch.SendToAllInConferenceRoom(GetStaffTitle(ch as PC) + ch.Name + " has left for " + ch.Map.ShortDesc + ".", ProtocolYuusha.TextType.Exit);
                                }

                                // showing the map removes round with blank screen
                                if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                                    ProtocolYuusha.ShowMap(ch);
                                else if (ch.protocol == "old-kesmai" && ch.CurrentCell != null)
                                    ch.CurrentCell.ShowMapOldKesProto(ch);
                                else if (ch.CurrentCell != null)
                                    ch.CurrentCell.ShowMap(ch);
                            }
                            else
                            {
                                if (ch.usingClient)
                                {
                                    ProtocolYuusha.sendMessageBox(ch, "The game world is currently locked.", "World Locked", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                                }
                                else
                                {
                                    ch.WriteLine("The game world is currently locked. Please try again later.", ProtocolYuusha.TextType.System);
                                }
                            }
                            break;
                        #endregion
                        #region /exit
                        case "/quit":
                        case "/exit":
                        case "/ex":
                        case "/q":
                        case "/logout":
                            if (!ch.IsInvisible) // send exit message if character is not invisible
                            {
                                ch.SendToAllInConferenceRoom(GetStaffTitle(ch as PC) + ch.Name + " has left the world.", ProtocolYuusha.TextType.Exit);
                            }

                            Utils.Log(ch.GetLogString(), Utils.LogType.Logout); // log the logout

                            if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                                ch.Write(ProtocolYuusha.LOGOUT);

                            ch.RemoveFromConf();
                            ch.RemoveFromServer();
                            break;
                        #endregion
                        #region /room
                        case "/room":
                            if (args == "" || args == null)
                            {
                                Header(ch, false);
                                break;
                            }

                            for (int a = 0; a < rooms.Length; a++)
                            {
                                if (args.ToUpper() == rooms[a].ToUpper())
                                {
                                    if (ch.ImpLevel >= (Globals.eImpLevel)roomsLevel[a])
                                    {
                                        match = true;
                                        if (!ch.IsInvisible)
                                        {
                                            ch.SendToAllInConferenceRoom(GetStaffTitle(ch as PC) + ch.Name + " has left the room.", ProtocolYuusha.TextType.Exit);
                                        }
                                        ch.confRoom = a;
                                        PC.SaveField(ch.UniqueID, "confRoom", ch.confRoom, null);
                                        break;
                                    }
                                }
                            }
                            if (match)
                            {
                                Header(ch, true);
                            }
                            else
                            {
                                ch.WriteLine("Invalid Room.", ProtocolYuusha.TextType.Error);
                            }
                            break;
                        #endregion
                        #region /notify
                        case "/notify":
                            if (ch.friendNotify)
                            {
                                ch.friendNotify = false;
                                ch.WriteLine("You will now be notified when your friends list log on and off.", ProtocolYuusha.TextType.Status);
                            }
                            else
                            {
                                ch.friendNotify = true;
                                ch.WriteLine("You will no longer be notified when your friends log on and off.", ProtocolYuusha.TextType.Status);
                            }
                            PC.SaveField(ch.UniqueID, "friendNotify", ch.friendNotify, null);
                            break;
                        #endregion
                        #region /macro (GameCommand)
                        //case "/macro":
                        //case "/macros":
                        //    if (args == null)
                        //    {
                        //        if (ch.macros.Count == 0)
                        //        {
                        //            ch.WriteLine("You do not have any macros set.", Protocol.TextType.Error);
                        //            break;
                        //        }
                        //        else
                        //        {
                        //            ch.WriteLine("", Protocol.TextType.Listing);
                        //            ch.WriteLine("Macro List", Protocol.TextType.Listing);
                        //            ch.WriteLine("----------", Protocol.TextType.Listing);
                        //            for (int a = 0; a < ch.macros.Count; a++)
                        //            {
                        //                ch.WriteLine(a + " = " + ch.macros[a].ToString(), Protocol.TextType.Listing);
                        //            }
                        //        }
                        //    }
                        //    else
                        //    {
                        //        int macrosIndex;

                        //        try
                        //        {
                        //            macrosIndex = Convert.ToInt32(args.Substring(0, args.IndexOf(' ')));
                        //        }
                        //        catch (Exception e)
                        //        {
                        //            Utils.LogException(e);
                        //            ch.WriteLine("Format: /macro # <text>, where # can range from 0 to " + Character.MAX_MACROS + ".", Protocol.TextType.Status);
                        //            break;
                        //        }

                        //        if (macrosIndex > Character.MAX_MACROS)
                        //        {
                        //            ch.WriteLine("The current maximum amount of macros each character may have is " + (Character.MAX_MACROS + 1) + ".", Protocol.TextType.Status);
                        //            break;
                        //        }

                        //        if (macrosIndex < 0)
                        //        {
                        //            ch.WriteLine("Format: /macro # <text>, where # can range from 0 to " + Character.MAX_MACROS + ".", Protocol.TextType.Status);
                        //            break;
                        //        }

                        //        string newMacro = args.Substring(args.IndexOf(' ') + 1);

                        //        if (ch.macros.Count >= macrosIndex + 1)
                        //        {
                        //            if (newMacro.IndexOf(Protocol.ISPLIT) != -1)
                        //            {
                        //                ch.WriteLine("Your macro contains an illegal character. The character " + Protocol.ISPLIT + " is reserved.", Protocol.TextType.Status);
                        //                break;
                        //            }
                        //            ch.macros[macrosIndex] = newMacro;
                        //            ch.WriteLine("Macro " + macrosIndex + " has been set to \"" + newMacro + "\".", Protocol.TextType.Status);
                        //        }
                        //        else
                        //        {
                        //            if (newMacro.IndexOf(Protocol.ISPLIT) != -1)
                        //            {
                        //                ch.WriteLine("Your macro contains an illegal character. The character " + Protocol.ISPLIT + " is reserved.", Protocol.TextType.Status);
                        //                break;
                        //            }
                        //            ch.macros.Add(newMacro);
                        //            ch.WriteLine("Macro " + macrosIndex + " has been set to \"" + newMacro + "\".", Protocol.TextType.Status);
                        //        }

                        //        if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                        //            Protocol.SendCharacterMacros((ch as PC), ch);
                        //    }
                        //    break;
                        #endregion
                        #region /friend
                        case "/friend":
                        case "/friends":
                            if (args != null)
                            {
                                int friendsCount;
                                for (friendsCount = 0; friendsCount <= Character.MAX_FRIENDS; friendsCount++)
                                {
                                    if (ch.friendsList[friendsCount] == 0) { break; }
                                }
                                if (friendsCount >= Character.MAX_FRIENDS) // friends list has a maximum length
                                {
                                    ch.WriteLine("Your friends list is full.", ProtocolYuusha.TextType.Error);
                                    break;
                                }
                                int friendID = DAL.DBPlayer.GetPlayerID(args); // attempt to retrieve the friend's playerID
                                if (friendID == -1) // player ID not found
                                {
                                    ch.WriteLine("That player was not found.", ProtocolYuusha.TextType.Error);
                                    break;
                                }
                                if (friendID == ch.UniqueID) // players cannot add themselves to their friends list
                                {
                                    ch.WriteLine("You cannot add your own name to your friend's list.", ProtocolYuusha.TextType.Error);
                                    break;
                                }
                                string friend = PC.GetName(friendID);
                                for (int a = 0; a < ch.friendsList.Length; a++) // check if player ID is already on friends list
                                {
                                    if (ch.friendsList[a] == 0)
                                    {
                                        break;
                                    }
                                    else if (ch.friendsList[a] == friendID) // if player ID exists, remove from friends list
                                    {
                                        match = true;
                                        ch.friendsList[a] = 0;
                                        ch.WriteLine(friend + " has been removed from your friends list.", ProtocolYuusha.TextType.Status);
                                        PC.SaveField(ch.UniqueID, "friendsList", Utils.ConvertIntArrayToString(ch.friendsList), null);
                                        break;
                                    }
                                }
                                if (!match) // add friend's player ID to first available array location
                                {
                                    ch.friendsList[friendsCount] = friendID;
                                    ch.WriteLine(friend + " has been added to your friends list.", ProtocolYuusha.TextType.Status);
                                    PC.SaveField(ch.UniqueID, "friendsList", Utils.ConvertIntArrayToString(ch.friendsList), null);
                                    break;
                                }

                                //if (ch.protocol == DragonsSpineMain.Instance.Settings.ServerProtocol)
                                //{
                                //    Protocol.SendCharacterStats((PC)ch, ch);
                                //    Protocol.SendUserList(ch);
                                //}
                            }
                            else // if args are null display ignore list
                            {
                                ch.WriteLine("", ProtocolYuusha.TextType.Listing);
                                ch.WriteLine("Friends List", ProtocolYuusha.TextType.Listing);
                                ch.WriteLine("-----------", ProtocolYuusha.TextType.Listing);
                                for (int a = 0; a < ch.friendsList.Length; a++)
                                {
                                    if (ch.friendsList[a] == 0)
                                    {
                                        break;
                                    }
                                    ch.WriteLine(a + 1 + ". " + PC.GetName(ch.friendsList[a]), ProtocolYuusha.TextType.Listing);
                                }
                                ch.WriteLine("", ProtocolYuusha.TextType.Listing);
                            }
                            break;
                        #endregion
                        #region /ignore
                        case "/ignore":
                            if (args != null)
                            {
                                int ignoreCount;
                                for (ignoreCount = 0; ignoreCount <= Character.MAX_IGNORE; ignoreCount++) // get ignore list count
                                {
                                    if (ch.ignoreList[ignoreCount] == 0)
                                    {
                                        break;
                                    }
                                }
                                if (ignoreCount >= Character.MAX_IGNORE) // ignore list has a maximum length
                                {
                                    ch.WriteLine("Your ignore list is full.", ProtocolYuusha.TextType.Error);
                                    break;
                                }

                                int ignoreID = DAL.DBPlayer.GetPlayerID(args); // attempt to retrieve the ignored player's ID
                                if (ignoreID == -1) // player ID not found
                                {
                                    ch.WriteLine("That player was not found.", ProtocolYuusha.TextType.Error);
                                    break;
                                }
                                if (ignoreID == ch.UniqueID) // players cannot ignore themselves
                                {
                                    ch.WriteLine("You cannot ignore yourself.", ProtocolYuusha.TextType.Error);
                                    break;
                                }
                                string ignored = PC.GetName(ignoreID);
                                //string ignored = DAL.DBPlayer.GetPlayerNameByID(ignoreID); // ignored player's name
                                if ((Globals.eImpLevel)DAL.DBPlayer.GetPlayerField(ignoreID, "ImpLevel", ch.ImpLevel.GetType()) >= Globals.eImpLevel.GM) // cannot ignore staff member
                                {
                                    ch.WriteLine("You cannot ignore a " + DragonsSpineMain.Instance.Settings.ServerName + " staff member.", ProtocolYuusha.TextType.Error);
                                    break;
                                }
                                for (int a = 0; a < ch.ignoreList.Length; a++) // check if player ID is already ignored
                                {
                                    if (ch.ignoreList[a] == 0)
                                    {
                                        break;
                                    }
                                    else if (ch.ignoreList[a] == ignoreID) // if player ID exists, remove from ignore list
                                    {
                                        match = true;
                                        ch.ignoreList[a] = 0;
                                        ch.WriteLine(ignored + " has been removed from your ignore list.", ProtocolYuusha.TextType.Status);
                                        PC.SaveField(ch.UniqueID, "ignoreList", Utils.ConvertIntArrayToString(ch.ignoreList), null);
                                        break;
                                    }
                                }
                                if (!match) // add ignored player ID to first available array location
                                {
                                    ch.ignoreList[ignoreCount] = ignoreID;
                                    ch.WriteLine(ignored + " has been added to your ignore list.", ProtocolYuusha.TextType.Status);
                                    PC.SaveField(ch.UniqueID, "ignoreList", Utils.ConvertIntArrayToString(ch.ignoreList), null);
                                    break;
                                }
                            }
                            else // if args are null display ignore list
                            {
                                ch.WriteLine("", ProtocolYuusha.TextType.Listing);
                                ch.WriteLine("Ignore List", ProtocolYuusha.TextType.Listing);
                                ch.WriteLine("-----------", ProtocolYuusha.TextType.Listing);
                                for (int a = 0; a < ch.ignoreList.Length; a++)
                                {
                                    if (ch.ignoreList[a] == 0)
                                    {
                                        break;
                                    }
                                    ch.WriteLine(a + 1 + ". " + PC.GetName(ch.ignoreList[a]), ProtocolYuusha.TextType.Listing);
                                }
                                ch.WriteLine("", ProtocolYuusha.TextType.Listing);
                            }
                            break;
                        #endregion
                        #region /page
                        case "/page":
                            // if args are null then turn paging off or on
                            if (args == null)
                            {
                                if (ch.receivePages)
                                {
                                    ch.receivePages = false;
                                    ch.WriteLine("You will no longer receive pages.", ProtocolYuusha.TextType.Status);
                                }
                                else
                                {
                                    ch.receivePages = true;
                                    ch.WriteLine("You will now receive pages.", ProtocolYuusha.TextType.Status);
                                }
                                PC.SaveField(ch.UniqueID, "receivePages", ch.receivePages, null);
                                break;
                            }
                            // if args are not null, search for the player and send them a page
                            try
                            {
                                PC receiver = PC.GetOnline(PC.GetPlayerID(args));

                                if (receiver != null)
                                {
                                    if (receiver.IsInvisible)
                                    {
                                        ch.WriteLine("That player was not found.", ProtocolYuusha.TextType.Status);
                                        return;
                                    }

                                    if (receiver.receivePages)
                                    {
                                        if (Array.IndexOf(receiver.ignoreList, ch.UniqueID) == -1)
                                        {
                                            receiver.WriteLine(GetStaffTitle(ch as PC) + ch.Name + " would like to speak to you in Conference " + GetUserLocation(ch) + ".", ProtocolYuusha.TextType.Page);
                                        }
                                        ch.WriteLine(GetStaffTitle(receiver) + receiver.Name + " has been paged.", ProtocolYuusha.TextType.Status);
                                    }
                                    else
                                    {
                                        ch.WriteLine(GetStaffTitle(receiver) + receiver.Name + " has disabled their pager.", ProtocolYuusha.TextType.Status);
                                    }
                                }
                                else
                                {
                                    // Check adventurers.
                                    foreach (Adventurer adv in Character.AdventurersInGameWorldList)
                                    {
                                        if (adv.Name.ToLower() == args.ToLower())
                                        {
                                            ch.WriteLine(adv.Name + " has been paged.", ProtocolYuusha.TextType.Status);
                                            return;
                                        }
                                    }

                                    ch.WriteLine("That player was not found.", ProtocolYuusha.TextType.Status);
                                }
                            }
                            catch
                            {
                                ch.WriteLine("An error occured while processing your page request.", ProtocolYuusha.TextType.Error);
                                Utils.Log("ChatCommands(" + ch.GetLogString() + ", " + command + ", " + args + ")", Utils.LogType.CommandFailure);
                            }
                            break;
                        #endregion
                        #region /displaycombatdamage
                        case "/displaycombatdamage":
                            if (System.Configuration.ConfigurationManager.AppSettings["DisplayCombatDamage"].ToLower() == "false")
                            {
                                ch.WriteLine("** Combat damage statistic information is currently disabled.", ProtocolYuusha.TextType.Status);
                                ch.DisplayCombatDamage = false;
                            }
                            if (ch.DisplayCombatDamage)
                            {
                                ch.DisplayCombatDamage = false;
                                ch.WriteLine("You will no longer see combat damage statistics.", ProtocolYuusha.TextType.Status);
                            }
                            else
                            {
                                ch.DisplayCombatDamage = true;
                                ch.WriteLine("You will now see combat damage statistics.", ProtocolYuusha.TextType.Status);
                            }
                            break;
                        #endregion
                        #region /help
                        case "/help":
                            ch.WriteLine("", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("Conference Room Commands", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("------------------------", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /play - Enter the game.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /exit - Disconnect from " + DragonsSpineMain.Instance.Settings.ServerName + ".", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /list - List your current characters.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /switch # - Switch to character #.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /scores or /topten - Get player rankings.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("     /scores me - Your current score.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("     /scores <class> <amount>", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("     /scores <class>", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("     /scores <player>", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("	   /scores all <amount>", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /page - Toggle paging.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("     /page <name> - Page someone in the game.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /tell or /t - Toggle private tells.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("     /tell <name> <message> - Send a private tell.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /friend - View your friends list.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("     /friend <name> - Add or remove a player from your friends list.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /notify - Toggle friend notification.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /ignore - View your ignore list.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("     /ignore <name> - Add or remove a player from your ignore list.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /users - Shows everyone in the game.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /menu - Return to the main menu.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /afk - Toggle A.F.K. (Away From Keyboard).", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /anon - Toggle anonymous.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /filter - Toggle profanity filter.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /rename <new name> - Change your character's name.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /echo - Toggle command echo.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /macro - View your macros list.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("     /macro # <text> - Set # macro, where # macro is between 0 and " + Character.MAX_MACROS + ".", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /lottery - View current lottery jackpots.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /commands - View a full list of available game commands.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /displaycombatdamage - Toggle combat damage statistics.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("  /help - This help list.", ProtocolYuusha.TextType.Help);
                            ch.WriteLine("", ProtocolYuusha.TextType.Help);
                            if (ch.ImpLevel > Globals.eImpLevel.USER)
                            {
                                ch.WriteLine("Staff Commands", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /stafftitle - Toggle staff title.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("", ProtocolYuusha.TextType.Help);
                            }
                            if (ch.ImpLevel >= Globals.eImpLevel.GM)
                            {
                                ch.WriteLine("GM Commands", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /invis - Toggle invisibility.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /announce - Send announcement to all, anonymously.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /selfannounce - Send announcement to all, includes your name.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /immortal - Toggle immortality.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /ban <name> <# of days> - Ban a player (includes their account)", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /boot <name> - Disconnects a player from the server.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /rename <old> <new> - Change a player's name.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /restock - Restock all merchant inventory items.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /clearstores - Clears all non original merchant inventory items.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("", ProtocolYuusha.TextType.Help);
                            }
                            if (ch.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                ch.WriteLine("DEV Commands", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /bootplayers - Force all players to quit.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /lockserver - Locks the server.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /unlockserver - Unlocks the server.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /implevel <name> <impLevel#> - Set a player's implevel.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /listf - Lists the Player table columns in the database.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /getf <name> <field name> - Get a player's field value.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /setf <name> <field name> <value> <notify> - Set a player's field value.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /processemptyworld <on|off> - No argument displays status of this attribute.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /purgeaccount <account> - Purge a players account.", ProtocolYuusha.TextType.Help);
                                //ch.WriteLine("  /searchnpcloot <itemID> - Search for an item on an NPC currently in the game.", Protocol.TextType.Help);
                                ch.WriteLine("  /getskill <name> <skill> - Get a PC's skill level.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /setskill <name> <skill> <skill level> - Set a PC's skill level.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /restartserver - Forces a hard shutdown, with no PC saves, and restarts the DragonsSpine.exe process.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("  /deleteplayer | /dplayer <name> - Delete a player from the database.", ProtocolYuusha.TextType.Help);
                                ch.WriteLine("", ProtocolYuusha.TextType.Help);
                            }
                            ch.WriteLine("", ProtocolYuusha.TextType.Help);
                            break;
                        #endregion
                        #region /menu
                        case "/menu":
                            ch.RemoveFromConf();
                            ch.AddToMenu();
                            if (!ch.IsInvisible)
                            {
                                ch.SendToAllInConferenceRoom(GetStaffTitle(ch as PC) + ch.Name + " has exited to the menu.", ProtocolYuusha.TextType.Exit);
                            }
                            ch.PCState = Globals.ePlayerState.MAINMENU;
                            Menu.PrintMainMenu(ch);
                            break;
                        #endregion
                        // Staff Commands
                        #region /stafftitle
                        case "/stafftitle":
                            if (ch.ImpLevel > Globals.eImpLevel.USER)
                            {
                                if (ch.showStaffTitle)
                                {
                                    ch.showStaffTitle = false;
                                    ch.WriteLine("Your staff title is now OFF.", ProtocolYuusha.TextType.Status);

                                }
                                else
                                {
                                    ch.showStaffTitle = true;
                                    ch.WriteLine("Your staff title is now ON.", ProtocolYuusha.TextType.Status);
                                }
                                //Protocol.UpdateUserLists();
                                PC.SaveField(ch.UniqueID, "showStaffTitle", ch.showStaffTitle, null);
                            }
                            else { Conference.SendInvalidCommand(ch); }
                            break;
                        #endregion
                        // GM Commands
                        #region /announce
                        case "/announce":
                            if (ch == null) // coming from local server window
                            {
                                int a;
                                for (a = 0; a < Character.PCInGameWorld.Count; a++)
                                {
                                    Character chr = Character.PCInGameWorld[a];
                                    if (chr != null)
                                    {
                                        chr.WriteToDisplay("SYSTEM: " + args);
                                    }
                                }
                                for (a = 0; a < Character.ConfList.Count; a++)
                                {
                                    Character chr = Character.ConfList[a];
                                    if (chr.PCState == Globals.ePlayerState.CONFERENCE)
                                    {
                                        chr.WriteLine("SYSTEM: " + args, ProtocolYuusha.TextType.System);
                                    }
                                }
                                Utils.Log("LOCAL SYSTEM ANNOUNCEMENT: " + args, Utils.LogType.Announcement);
                                return;
                            }
                            if (ch.ImpLevel >= Globals.eImpLevel.GM)
                            {
                                Utils.Log(GetStaffTitle(ch as PC) + ch.GetLogString() + " announced '" + args + "'", Utils.LogType.Announcement);
                                ch.SendToAll("SYSTEM: " + args); // send to all in game
                                ch.SendToAllInConferenceRoom("SYSTEM: " + args, ProtocolYuusha.TextType.System); // send to all in chat room....
                            }
                            else { Conference.SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /selfannounce
                        case "/selfannounce":
                            if (ch == null) // coming from local server window
                            {
                                int a;
                                for (a = 0; a < Character.PCInGameWorld.Count; a++)
                                {
                                    Character chr = Character.PCInGameWorld[a];
                                    if (chr != null)
                                    {
                                        chr.WriteToDisplay("SYSTEM: " + args);
                                    }
                                }
                                for (a = 0; a < Character.ConfList.Count; a++)
                                {
                                    Character chr = Character.ConfList[a];
                                    if (chr.PCState == Globals.ePlayerState.CONFERENCE)
                                    {
                                        chr.WriteLine("SYSTEM: " + args, ProtocolYuusha.TextType.System);
                                    }
                                }
                                Utils.Log("LOCAL SYSTEM ANNOUNCEMENT: " + args, Utils.LogType.Announcement);
                                return;
                            }
                            if (ch.ImpLevel >= Globals.eImpLevel.GM)
                            {
                                Utils.Log(GetStaffTitle(ch as PC) + ch.GetLogString() + " announced '" + args + "'", Utils.LogType.Announcement);
                                ch.SendToAll("SYSTEM: " + ch.Name + ": " + args); // send to all in game
                                ch.SendToAllInConferenceRoom("SYSTEM: " + ch.Name + ": " + args, ProtocolYuusha.TextType.System); // send to all in chat room....
                            }
                            else { Conference.SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /restock
                        case "/restock":
                            if (ch.ImpLevel >= Globals.eImpLevel.GM)
                            {
                                StoreItem.RestockStores();
                                ch.WriteLine("You have restocked all stores with their original stock items.", ProtocolYuusha.TextType.System);
                            }
                            else
                            {
                                SendInvalidCommand(ch);
                            }
                            break;
                        #endregion
                        #region /clearstores
                        case "/clearstores":
                            if (ch.ImpLevel >= Globals.eImpLevel.GM)
                            {
                                StoreItem.ClearStores();
                                ch.WriteLine("You have cleared all stores of their non original stock items.", ProtocolYuusha.TextType.System);
                            }
                            else
                            {
                                SendInvalidCommand(ch);
                            }
                            break;
                        #endregion
                        // DEV Commands
                        #region /shutdown
                        case "/shutdown":
                            if (ch.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                DragonsSpineMain.ServerStatus = DragonsSpineMain.ServerState.ShuttingDown;
                            }
                            else SendInvalidCommand(ch);
                            break;
                        #endregion
                        #region /processemptyworld <on|off>
                        case "/processemptyworld":
                            if (ch.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                if (args == "on")
                                {
                                    DragonsSpineMain.Instance.Settings.ProcessEmptyWorld = true;
                                    ch.WriteLine("Empty world processing enabled.", ProtocolYuusha.TextType.System);
                                }
                                else if (args == "off")
                                {
                                    DragonsSpineMain.Instance.Settings.ProcessEmptyWorld = false;
                                    ch.WriteLine("Empty world processing disabled.", ProtocolYuusha.TextType.System);
                                }
                                else
                                {
                                    ch.WriteLine("Empty world processing is currently set to " + DragonsSpineMain.Instance.Settings.ProcessEmptyWorld.ToString(), ProtocolYuusha.TextType.System);
                                }
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /purgeaccount
                        case "/purgeaccount":
                            if (ch.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                Account account = DAL.DBAccount.GetAccountByName(args);
                                if (account != null)
                                {
                                    ch.WriteLine("Purging account " + account.accountName + "...", ProtocolYuusha.TextType.System);
                                    foreach (string chrName in account.players)
                                    {
                                        if (chrName.Length > 0)
                                        {
                                            if (DAL.DBPlayer.DeletePlayerFromDatabase(PC.GetPlayerID(chrName)))
                                            {
                                                ch.WriteLine("Deleted player " + chrName + ".", ProtocolYuusha.TextType.System);
                                            }
                                            else
                                            {
                                                ch.WriteLine("Failed to delete player " + chrName + ".", ProtocolYuusha.TextType.System);
                                            }
                                        }
                                    }
                                    DAL.DBAccount.DeleteAccount(account.accountID);
                                    ch.WriteLine("Purge completed of account " + account.accountName + ".", ProtocolYuusha.TextType.System);
                                }
                                else
                                {
                                    ch.WriteLine("Failed to find account with the name " + args + ".", ProtocolYuusha.TextType.Error);
                                }
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /deleteplayer or /dplayer
                        case "/dplayer":
                        case "/deleteplayer":
                            {
                                if (ch.ImpLevel >= Globals.eImpLevel.DEV)
                                {
                                    if (args == null || args.Length < GameSystems.Text.NameGenerator.NAME_MIN_LENGTH)
                                    {
                                        ch.WriteLine("Invalid player name.");
                                        return;
                                    }

                                    PC pc = PC.GetOnline(args);

                                    if (pc != null)
                                    {
                                        ch.WriteLine("That player cannot be deleted because they are online.", ProtocolYuusha.TextType.System);
                                        return;
                                    }
                                    else pc = PC.GetPC(PC.GetPlayerID(args));

                                    if (pc == null)
                                    {
                                        ch.WriteLine("That player does not exist.", ProtocolYuusha.TextType.System);
                                        return;
                                    }

                                    if (DAL.DBPlayer.DeletePlayerFromDatabase(pc.UniqueID))
                                    {
                                        ch.WriteLine("The player " + pc.Name + " has been deleted from the database.");
                                    }
                                    else
                                    {
                                        ch.WriteLine("Failed to delete player " + pc.Name + " from the database.");
                                    }
                                }
                                else SendInvalidCommand(ch);
                            }
                            break;
                        #endregion
                        #region /getskill
                        case "/getskill":
                            if (ch.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                // args = <name> <skill>
                                string[] getSkillArgs = args.Split(" ".ToCharArray());

                                int getSkillID = PC.GetPlayerID(getSkillArgs[0]);

                                if (getSkillID <= 0)
                                {
                                    ch.WriteLine("The player " + getSkillArgs[0] + " does not exist.", ProtocolYuusha.TextType.System);
                                    return;
                                }

                                PC getSkillPC = PC.GetOnline(getSkillID);

                                // check if PC is legit
                                if (getSkillPC == null)
                                {
                                    getSkillPC = PC.GetPC(getSkillID);

                                    if (getSkillPC == null)
                                    {
                                        ch.WriteLine("Player not found.", ProtocolYuusha.TextType.System);
                                        return;
                                    }
                                }

                                Globals.eSkillType getSkillType = (Globals.eSkillType)Enum.Parse(typeof(Globals.eSkillType), getSkillArgs[1], true);

                                ch.WriteLine(getSkillPC.Name + "'s " + getSkillArgs[1] + " skill is level " +
                                    Skills.GetSkillLevel(getSkillPC, getSkillType) +
                                    " (" + Skills.GetSkillTitle(getSkillType, getSkillPC.BaseProfession, getSkillPC.GetSkillExperience(getSkillType), getSkillPC.gender) + ").", ProtocolYuusha.TextType.System);
                            }
                            else SendInvalidCommand(ch);
                            break;
                        #endregion
                        #region /setskill
                        case "/setskill":
                            if (ch.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                // args = <name> <skill> <skill level between 0 and 19>
                                string[] setSkillArgs = args.Split(" ".ToCharArray());

                                int setSkillID = PC.GetPlayerID(setSkillArgs[0]);

                                if (setSkillID <= 0)
                                {
                                    ch.WriteLine("The player " + setSkillArgs[0] + " does not exist.");
                                    return;
                                }

                                PC setSkillPC = PC.GetOnline(setSkillID);

                                // check if PC is legit
                                if (setSkillPC == null)
                                {
                                    setSkillPC = PC.GetPC(setSkillID);

                                    if (setSkillPC == null)
                                    {
                                        ch.WriteLine("Player not found.");
                                        return;
                                    }
                                }

                                Globals.eSkillType setSkillType = (Globals.eSkillType)Enum.Parse(typeof(Globals.eSkillType), setSkillArgs[1], true);

                                try
                                {
                                    setSkillPC.SetSkillExperience(setSkillType, Skills.GetSkillToNext(Convert.ToInt32(setSkillArgs[2])));
                                }
                                catch (Exception)
                                {
                                    ch.WriteLine("Invalid arguments. Command usage: /setskill <name of player> <skill type> <skill level>.");
                                }

                                setSkillPC.Save();

                                ch.WriteLine(setSkillPC.Name + "'s " + setSkillArgs[1] + " skill has been set to level " + setSkillArgs[2] +
                                    " (" + Skills.GetSkillTitle(setSkillType, setSkillPC.BaseProfession, setSkillPC.GetSkillExperience(setSkillType), setSkillPC.gender) + ").");

                            }
                            else SendInvalidCommand(ch);

                            break;
                        #endregion
                        #region /searchnpcloot (search all NPCs for an itemID)
                        case "/searchnpcloot":
                            if (ch.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                Item loot;

                                try
                                {
                                    loot = Item.CopyItemFromDictionary(Convert.ToInt32(args));
                                }
                                catch (FormatException)
                                {
                                    ch.WriteLine("Please use an item ID number to search for NPC loot.", ProtocolYuusha.TextType.Error);
                                    break;
                                }

                                if (loot == null)
                                {
                                    ch.WriteLine("No item with ID '" + args + "' exists.", ProtocolYuusha.TextType.Error);
                                    break;
                                }

                                foreach (NPC npc in Character.NPCInGameWorld)
                                {
                                    if (npc.RightHand != null && npc.RightHand.itemID == loot.itemID)
                                        ch.WriteLine(npc.GetLogString() + " >> RIGHT HAND", ProtocolYuusha.TextType.System);

                                    if (npc.LeftHand != null && npc.LeftHand.itemID == loot.itemID)
                                        ch.WriteLine(npc.GetLogString() + " >> LEFT HAND", ProtocolYuusha.TextType.System);

                                    foreach (Item armor in npc.wearing)
                                    {
                                        if (armor.itemID == loot.itemID)
                                            ch.WriteLine(npc.GetLogString() + " >> WEARING", ProtocolYuusha.TextType.System);
                                    }

                                    foreach (Item sackItem in npc.sackList)
                                    {
                                        if (sackItem.itemID == loot.itemID)
                                            ch.WriteLine(npc.GetLogString() + " >> SACK", ProtocolYuusha.TextType.System);
                                    }

                                    foreach (Item beltItem in npc.beltList)
                                    {
                                        if (beltItem.itemID == loot.itemID)
                                            ch.WriteLine(npc.GetLogString() + " >> BELT", ProtocolYuusha.TextType.System);
                                    }

                                    if (npc.lairCritter)
                                    {
                                        foreach (Cell cell in npc.lairCellsList)
                                        {
                                            if (Item.IsItemOnGround(loot.itemID, cell))
                                                ch.WriteLine(npc.GetLogString() + " >> LAIR", ProtocolYuusha.TextType.System);
                                        }
                                    }
                                }
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /getstats (display CPU usage)
                        case "/getstats":
                            if (ch.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                ch.WriteLine("CPU Usage: " + Utils.GetCpuUsage() + "%", ProtocolYuusha.TextType.Help);
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /listf (list Player table columns in the database)
                        case "/listf": // short for "list fields"
                            if (ch.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                try
                                {
                                    string[] columns = DAL.DBPlayer.GetPlayerTableColumnNames(ch.UniqueID);
                                    //string output = "";

                                    ch.WriteLine("", ProtocolYuusha.TextType.Listing);
                                    ch.WriteLine("Player Table Columns", ProtocolYuusha.TextType.Listing);
                                    ch.WriteLine("--------------------", ProtocolYuusha.TextType.Listing);

                                    string col1 = "";
                                    string col2 = "";

                                    for (int a = 0; a < columns.Length; a++)
                                    {
                                        if (a % 2 == 0)
                                        {
                                            col1 = columns[a].PadRight(22);
                                            if (a == columns.Length - 1)
                                            {
                                                if (ch.usingClient)
                                                    ch.WriteLine(col1);
                                                else ch.WriteLine(col1, ProtocolYuusha.TextType.Help);
                                            }
                                        }
                                        else
                                        {
                                            col2 = columns[a];
                                            if (!ch.usingClient)
                                                ch.WriteLine(col1 + " " + col2);
                                            else ch.WriteLine(col1 + " " + col2, ProtocolYuusha.TextType.Help);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Utils.Log("ERROR: ChatRoom.ChatCommands(" + ch.GetLogString() + ", " + command + ", " + args + ")", Utils.LogType.CommandFailure);
                                    Utils.LogException(e);
                                }
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /getf
                        case "/getf": // short for "get field"
                            if (ch.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                // /getf <name> <field name>
                                if (args == null || args.ToLower() == "help" || args == "?")
                                {
                                    ch.WriteLine("Format: /getf <name> <field name>", ProtocolYuusha.TextType.Help);
                                    ch.WriteLine("<name> is the full name of the player (not case sensitive)", ProtocolYuusha.TextType.Help);
                                    ch.WriteLine("<field name> is the number of the field from the /listf output", ProtocolYuusha.TextType.Help);
                                    break;
                                }
                                try
                                {
                                    string[] getfArgs = args.Split(" ".ToCharArray());
                                    int id = PC.GetPlayerID(getfArgs[0]);
                                    if (id == -1)
                                    {
                                        ch.WriteLine("Player '" + getfArgs[0] + "' was not found in the database.", ProtocolYuusha.TextType.Error);
                                        break;
                                    }
                                    //int num = Convert.ToInt32(getfArgs[1]);
                                    string fieldValue = fieldValue = Convert.ToString(DAL.DBPlayer.GetPlayerField(id, getfArgs[1], null));
                                    //string[] columns = DAL.DBPlayer.getPlayerTableColumnNames(id);
                                    //fieldValue = Convert.ToString(DAL.DBPlayer.getPlayerField(id, getfArgs[1], null)); // subtract one because the /listpcol display added 1
                                    ch.WriteLine(PC.GetName(id) + "'s \"" + getfArgs[1] + "\" is set to \"" + fieldValue + "\".", ProtocolYuusha.TextType.System);

                                }
                                catch (Exception e)
                                {
                                    Utils.Log("ChatRoom.ChatCommands(" + ch.GetLogString() + ", " + command + ", " + args + ")", Utils.LogType.CommandFailure);
                                    Utils.LogException(e);
                                }
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /setf
                        case "/setf":
                            if (ch.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                // /setf <name> <field name> <value> <notify: true | false>
                                if (args == null || args == "" || args.ToLower() == "help" || args == "?")
                                {
                                    ch.WriteLine("Format: /setf <name> <field name> <value> <notify: true | false>", ProtocolYuusha.TextType.Help);
                                    ch.WriteLine("<name> is the full name of the player (not case sensitive)", ProtocolYuusha.TextType.Help);
                                    ch.WriteLine("<field name> is the field from the /listf output", ProtocolYuusha.TextType.Help);
                                    ch.WriteLine("<value> to set the field to", ProtocolYuusha.TextType.Error);
                                    ch.WriteLine("<notify> true will notify the user that the value has been changed", ProtocolYuusha.TextType.Help);
                                    ch.WriteLine("**Please note that booleans must be 'true or 'false', not 1 or 0.", ProtocolYuusha.TextType.Help);
                                    break;
                                }

                                try
                                {
                                    // split the arguments
                                    // /setf <name> <field name> <value> [true | false]
                                    string[] setfArgs = args.Split(" ".ToCharArray());

                                    if (setfArgs.Length < 3)
                                    {
                                        ch.WriteLine("Invalid arguments. Format: /setf <name> <field name> <value> <notify: true | false>");
                                        break;
                                    }

                                    #region Determine if player exists
                                    int id = PC.GetPlayerID(setfArgs[0]);

                                    if (id == -1)
                                    {
                                        ch.WriteLine("Player '" + setfArgs[0] + "' was not found in the database.", ProtocolYuusha.TextType.Error);
                                        break;
                                    }
                                    #endregion

                                    //int num = Convert.ToInt32(setfArgs[1]);

                                    string[] columns = DAL.DBPlayer.GetPlayerTableColumnNames(id);
                                    string fieldName = setfArgs[1];
                                    string fieldValue_old = Convert.ToString(DAL.DBPlayer.GetPlayerField(id, fieldName, null)); // subtract one because the /listf display added 1
                                    string fieldValue_new = setfArgs[2];

                                    System.Reflection.PropertyInfo[] propertyInfo = typeof(DragonsSpine.PC).GetProperties(); // get property info array
                                    System.Reflection.FieldInfo[] fieldInfo = typeof(DragonsSpine.PC).GetFields(); // get field info array

                                    //Character online = PC.GetOnline(id);

                                    PC pc = PC.GetPC(id);

                                    // Set the value in the database.
                                    if (DAL.DBPlayer.SavePlayerField(id, fieldName, fieldValue_new) != -1)
                                    {
                                        ch.WriteLine(pc.Name + "'s \"" + fieldName + "\" has been saved in the database.", ProtocolYuusha.TextType.System);
                                    }
                                    else
                                    {
                                        ch.WriteLine(pc.Name + "'s \"" + fieldName + "\" was NOT saved in the database.", ProtocolYuusha.TextType.System);
                                        return;
                                    }

                                    PC pcOnline = PC.GetOnline(id);

                                    if (pcOnline != null)
                                    {
                                        //bool foundProperty = false;

                                        foreach (System.Reflection.PropertyInfo prop in propertyInfo)
                                        {
                                            if (prop.Name.ToLower() == fieldName.ToLower())
                                            {
                                                prop.SetValue(pcOnline, Convert.ChangeType(setfArgs[2], Convert.GetTypeCode(prop.GetType())), null);
                                                //foundProperty = true;
                                                ch.WriteLine("Found property: " + prop.Name, ProtocolYuusha.TextType.System);
                                                break;
                                            }
                                        }

                                        //if (!foundProperty)
                                        //{
                                        //    System.Reflection.Assembly assembly = AppDomain.CurrentDomain.Load(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

                                        //    foreach (System.Reflection.FieldInfo fInfo in fieldInfo)
                                        //    {
                                        //        if (fInfo.Name.ToLower() == fieldName.ToLower())
                                        //        {
                                        //            try
                                        //            {
                                        //                fInfo.SetValue(pcOnline, Convert.ChangeType(fieldValue_new, fInfo.FieldType));
                                        //            }
                                        //            catch (InvalidCastException)
                                        //            {
                                        //                Type enumType = assembly.GetType(fInfo.FieldType.ToString());
                                        //                fInfo.SetValue(pcOnline, Convert.ChangeType(Enum.Parse(enumType, fieldValue_new), fInfo.FieldType));
                                        //            }
                                        //            break;
                                        //        }
                                        //    }
                                        //}
                                    }

                                    ch.WriteLine(pc.Name + "'s \"" + fieldName + "\" has been changed from \"" + fieldValue_old +
                                        "\" to \"" + fieldValue_new + "\".", ProtocolYuusha.TextType.System);

                                    if (pcOnline != null)
                                    {
                                        if (setfArgs.Length >= 4 && (setfArgs[3].ToLower() == "true" || setfArgs[3] == "1"))
                                        {
                                            if (pcOnline.PCState == Globals.ePlayerState.PLAYING)
                                            {
                                                pcOnline.WriteLine("Your " + fieldName + " has been changed from \"" + fieldValue_old + "\" to \"" + fieldValue_new + "\".");
                                            }
                                            else
                                            {
                                                pcOnline.WriteLine("Your " + fieldName + " has been changed from \"" + fieldValue_old + "\" to \"" + fieldValue_new + "\".", ProtocolYuusha.TextType.System);
                                            }

                                            ch.WriteLine(pcOnline.Name + " has been notified of the change.", ProtocolYuusha.TextType.System);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Utils.Log("ChatRoom.ChatCommands(" + ch.GetLogString() + ", " + command + ", " + args + ")", Utils.LogType.CommandFailure);
                                    Utils.LogException(e);
                                }
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /implevel
                        case "/implevel":
                            if (ch.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                if (args == null || args.ToLower() == "help" || args == "?")
                                {
                                    ch.WriteLine("Format: /implevel <name> <impLevel#>", ProtocolYuusha.TextType.Help);
                                    ch.WriteLine("<name> is the full name of the player (not case sensitive)", ProtocolYuusha.TextType.Help);
                                    ch.WriteLine("<impLevel#> is the new impLevel, ranging from 0 to " + Enum.GetValues(ch.ImpLevel.GetType()).Length + ".", ProtocolYuusha.TextType.Help);
                                    break;
                                }
                                try
                                {
                                    String[] impArgs = args.Split(" ".ToCharArray());
                                    int id = PC.GetPlayerID(impArgs[0]);
                                    if (id == -1)
                                    {
                                        ch.WriteLine("Player '" + impArgs[0] + "' was not found.", ProtocolYuusha.TextType.Error);
                                        break;
                                    }
                                    PC online = PC.GetOnline(id);
                                    Globals.eImpLevel oldImpLevel = (Globals.eImpLevel)PC.GetField(id, "impLevel", (int)ch.ImpLevel, null);
                                    Globals.eImpLevel newImpLevel = (Globals.eImpLevel)Convert.ToInt32(impArgs[1]);
                                    if (online != null) // if character is online, set their new implevel and alert them of the change
                                    {
                                        online.ImpLevel = newImpLevel;
                                        //Protocol.UpdateUserLists(); // send new user lists to protocol users
                                        online.WriteLine("Your impLevel has been changed from " + oldImpLevel + " to " + newImpLevel + ".", ProtocolYuusha.TextType.System);
                                    }
                                    else
                                    {
                                        ch.WriteLine(PC.GetName(id) + "'s impLevel has been changed from " + oldImpLevel + " to " + newImpLevel + ".", ProtocolYuusha.TextType.System);
                                    }
                                    PC.SaveField(id, "impLevel", (int)online.ImpLevel, null); // save new implevel to Player table
                                    Utils.Log(online.GetLogString() + " impLevel was changed from " + oldImpLevel + " to " + newImpLevel + " by " + ch.GetLogString() + ".", Utils.LogType.Unknown);
                                }
                                catch (Exception e)
                                {
                                    Utils.Log("ChatRoom.ChatCommands(" + ch.GetLogString() + ", " + command + ", " + args + ")", Utils.LogType.CommandFailure);
                                    Utils.LogException(e);
                                    Conference.ChatCommands(ch, "/implevel", null);
                                }
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /lockserver
                        case "/lockserver":
                            if (ch.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                DragonsSpineMain.ServerStatus = DragonsSpineMain.ServerState.Locked;
                                ch.WriteLine("Game world has been locked.", ProtocolYuusha.TextType.System);
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /unlockserver
                        case "/unlockserver":
                            if (ch.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                DragonsSpineMain.ServerStatus = DragonsSpineMain.ServerState.Running;
                                ch.WriteLine("Game world has been unlocked.", ProtocolYuusha.TextType.System);
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /bootplayers
                        case "/bootplayers":
                            if (ch.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                DragonsSpineMain.ServerStatus = DragonsSpineMain.ServerState.Locked;
                                foreach (PC chr in new List<PC>(Character.PCInGameWorld))
                                {
                                    CommandTasker.ParseCommand(chr, "forcequit", "");
                                }
                                ch.WriteLine("Game World locked, players booted.", ProtocolYuusha.TextType.System);
                            }
                            else SendInvalidCommand(ch);
                            break;
                        #endregion
                        #region /restartserver
                        case "/restartserver":
                            if (ch.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                DragonsSpineMain.Instance.RestartServerWithoutSave(ch.GetLogString() + " performed a hard server restart from the conference room.");
                            }
                            else SendInvalidCommand(ch);
                            break;
                        #endregion
//                        #region /possess (DEBUG mode only)
//                        case "/possess":
//                            {
//                                if (ch.ImpLevel < Globals.eImpLevel.DEV)
//                                {
//                                    SendInvalidCommand(ch);
//                                }
//                                else
//                                {
//#if DEBUG
//                                    int id = PC.GetPlayerID(args);

//                                    if (id <= 0)
//                                    {
//                                        ch.WriteLine("The player " + args + " was not found.", Protocol.TextType.System);
//                                        return;
//                                    }

//                                    PC pc = PC.GetPC(id); // get new character from the database

//                                    if (pc != null)
//                                    {
//                                        PC.PossessCharacter(ch, pc);
//                                        ch.WriteLine("You have possessed the player named " + ch.Name + ".", Protocol.TextType.System);
//                                        ch.WriteLine("You must switch characters or log off to end possession.", Protocol.TextType.System);
//                                    }
//                                    else ch.WriteLine("You have failed to possess " + args + ".", Protocol.TextType.System);
//                                }
//#endif
//                            }
                            //break; 
                        //#endregion
                        default:
                            Conference.SendInvalidCommand(ch);
                            break;
                    }
                }
            }
            else
            {
                if (ch.IsInvisible)
                {
                    ch.WriteLine("Please become visible if you wish to chat in the conference room.", ProtocolYuusha.TextType.Help);
                }
                else
                {
                    ch.SendToAllInConferenceRoom(GetStaffTitle(ch as PC) + ch.Name + ": " + all, ProtocolYuusha.TextType.PlayerChat);
                }
            }
            return;
        }

        public static ArrayList GetAllInRoom(PC ch)
        {
            ArrayList a = new ArrayList();
            if (ch.ImpLevel < Globals.eImpLevel.GM)
            {
                foreach (PC chr in Character.ConfList)
                {
                    if (chr.PCState == Globals.ePlayerState.CONFERENCE && chr != ch && !chr.IsInvisible)
                    {
                        if (ch.confRoom == chr.confRoom) a.Add(chr);
                    }
                }
                return a;
            }
            else
            {
                foreach (PC chr in Character.ConfList)
                {
                    if (chr.PCState == Globals.ePlayerState.CONFERENCE && chr != ch)
                    {
                        if (ch.confRoom == chr.confRoom) a.Add(chr);
                    }
                }
                return a;
            }
        }

        public static string GetUserLocation(PC ch)
        {
            if (ch.Name == "Nobody") { return "Character Generator"; }

            switch (ch.PCState)
            {
                case Globals.ePlayerState.PLAYING:
                    string location = ch.Map.Name;
                    if (ch.IsDead)
                    {
                        location = location + " (dead)";
                    }
                    return location;
                case Globals.ePlayerState.CONFERENCE:
                    return "Room " + rooms[ch.confRoom];
                default:
                    return "Menu";
            }
        }

        public static string GetStaffTitle(PC ch)
        {
            if (ch.ImpLevel > Globals.eImpLevel.USER && ch.showStaffTitle)
                return "[" + ch.ImpLevel.ToString() + "]";

            return "";
        }

        public static void SendInvalidCommand(Character ch)
        {
            string invalid = "Invalid Command. Type /help for a list of commands.";
            ch.WriteLine(invalid, ProtocolYuusha.TextType.Error);
        }

        public static string FilterProfanity(string message) // filter profanity and return filtered message
        {
            for (int a = 0; a < ProfanityArray.Length; a++)
            {
                if (message.ToLower().IndexOf(ProfanityArray[a]) != -1)
                {
                    do
                    {
                        int b = message.ToLower().IndexOf(ProfanityArray[a]);
                        message = message.Remove(b, ProfanityArray[a].Length);
                        string replacement = "";
                        for (int c = 0; c < ProfanityArray[a].Length; c++)
                        {
                            replacement = replacement + "*";
                        }
                        message = message.Insert(b, replacement);
                    }
                    while (message.IndexOf(ProfanityArray[a]) != -1);
                }
            }
            return message;
        }

        public static void FriendNotify(PC friend, bool login) // send friend notify messages
        {
            if (!friend.IsInvisible) // only do friend notify if the friend is visible
            {
                // notify users at the menu
                foreach (PC user in Character.MenuList)
                {
                    if (Array.IndexOf(user.friendsList, friend.UniqueID) != -1 && user.friendNotify) // if the player is a friend and notifications are on
                    {
                        if (Array.IndexOf(friend.ignoreList, user.UniqueID) == -1) // ** if the friend has this user ignored the message will not be sent
                        {
                            if (login) { user.WriteLine(GetStaffTitle(friend) + friend.Name + " has logged on.", ProtocolYuusha.TextType.Friend); }
                            else { user.WriteLine(GetStaffTitle(friend) + friend.Name + " has logged off.", ProtocolYuusha.TextType.Friend); }
                        }
                    }
                }

                // notify users in conference
                foreach (PC user in Character.ConfList)
                {
                    if (Array.IndexOf(user.friendsList, friend.UniqueID) != -1 && user.friendNotify) // if the player is a friend and notifications are on
                    {
                        if (Array.IndexOf(friend.ignoreList, user.UniqueID) == -1) // ** if the friend has this user ignored the message will not be sent
                        {
                            if (login) { user.WriteLine(GetStaffTitle(friend) + friend.Name + " has logged on.", ProtocolYuusha.TextType.Friend); }
                            else { user.WriteLine(GetStaffTitle(friend) + friend.Name + " has logged off.", ProtocolYuusha.TextType.Friend); }
                        }
                    }
                }

                // notify users in the game
                foreach (PC user in Character.PCInGameWorld)
                {
                    if (Array.IndexOf(user.friendsList, friend.UniqueID) != -1 && user.friendNotify) // if the player is a friend and notifications are on
                    {
                        if (Array.IndexOf(friend.ignoreList, user.UniqueID) == -1) // ** if the friend has this user ignored the message will not be sent
                        {
                            if (login) { user.WriteLine(GetStaffTitle(friend) + friend.Name + " has logged on.", ProtocolYuusha.TextType.Friend); }
                            else { user.WriteLine(GetStaffTitle(friend) + friend.Name + " has logged off.", ProtocolYuusha.TextType.Friend); }
                        }
                    }
                }
            }
        }
    }
}
