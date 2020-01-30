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
using System.Security.Cryptography;
using DragonsSpine.GameWorld;
using DragonsSpine.Mail.GameMail;

namespace DragonsSpine
{
    /// <summary>
    /// This is the equivalent of our Socks class, but, since it's really a lot more than just socks stuff,
    /// I called it IO
    /// </summary>
    public class IO
    {
        public TcpListener listener;
        protected int port;

        #region Static lists holding PC and Character objects waiting to be moved to processing lists.
        public static List<PC> AddToLogin = new List<PC>();
        public static List<PC> RemoveFromLogin = new List<PC>();
        public static List<PC> AddToCharGen = new List<PC>();
        public static List<PC> RemoveFromCharGen = new List<PC>();
        public static List<PC> AddToMenu = new List<PC>();
        public static List<PC> RemoveFromMenu = new List<PC>();
        public static List<PC> AddToConf = new List<PC>();
        public static List<PC> RemoveFromConf = new List<PC>();
        public static List<Character> AddToWorld = new List<Character>();
        public static List<Character> RemoveFromWorld = new List<Character>(); 
        #endregion

        #region Static Booleans to notify game loop PC and Character objects are waiting to be moved to a processing list.
        public static bool pplToAddToLogin = false;
        public static bool pplToRemoveFromLogin = false;
        public static bool pplToAddToCharGen = false;
        public static bool pplToRemoveFromCharGen = false;
        public static bool pplToAddToMenu = false;
        public static bool pplToRemoveFromMenu = false;
        public static bool pplToAddToConf = false;
        public static bool pplToRemoveFromConf = false;
        public static bool pplToAddToWorld = false;
        public static bool pplToRemoveFromWorld = false; 
        #endregion

        public IO(int port)
        {
            this.port = port;
        }

        public bool Open()
        {
            try
            {
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                listener = new TcpListener(IPAddress.Any, port);
                Utils.Log("Listening for connections on port " + port + ".", Utils.LogType.SystemGo);
                listener.Start();
                return true;
            }
            catch (Exception e)
            {
                Utils.Log("Listener Error: " + e.Message, Utils.LogType.SystemFatalError);
                return false;
            }
        }

        public void HandleNewConnections()
        {
            if (DragonsSpineMain.ServerStatus == DragonsSpineMain.ServerState.Running ||
                DragonsSpineMain.ServerStatus == DragonsSpineMain.ServerState.Locked)
            {
                while (listener.Pending())
                { //for as long as we have ppl waiting to connect...
                    
                    Socket newSock; //a socket to store their connection in
                    newSock = listener.AcceptSocket(); //accept the connection

                    PC pc = new PC();
                    pc.setSocket(newSock);
                    pc.IsPC = true;
                    pc.PCState = Globals.ePlayerState.LOGIN;
                    string hostName;

                    string clientAddr = "" + IPAddress.Parse(((IPEndPoint)newSock.RemoteEndPoint).Address.ToString());

                    try
                    {
                        hostName = Dns.GetHostEntry(clientAddr).HostName;
                        pc.Account.hostName = hostName;
                    }
                    catch (Exception e)
                    {
                        hostName = "No Host Name: " + e.Message + "";
                        pc.Account.hostName = "Unknown Host Name";
                    }

                    try
                    {
                        clientAddr = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                    }
                    catch
                    {
                        clientAddr = "DNS Failed!";
                    }

                    #region Check banned IP addresses.
                    if (World.BannedIPList.Count > 0)
                    {
                        for (int a = 0; a < World.BannedIPList.Count; a++)
                        {
                            if (World.BannedIPList.IndexOf(clientAddr) != -1)
                            {
                                Map.WriteAtXY(pc, 5, 3, "Unable to access the login server.");
                                Utils.Log("BANNED LOGIN ATTEMPT: " + clientAddr, Utils.LogType.SystemWarning);
                                newSock.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                                newSock.Close();
                                return;
                            }
                        }
                    } 
                    #endregion

                    // Log the connection information.
                    Utils.Log(hostName + " (" + clientAddr + ")", Utils.LogType.Connection);

                    #region Check if IP address is already sitting at login. If multiple IPs are not allowed, disconnect IP address already connected.
                    if (!DragonsSpineMain.Instance.Settings.AllowMultipleLoginFromSameIP)
                    {
                        foreach (PC connected in Character.LoginList)
                        {
                            if (connected.Account.ipAddress == clientAddr)
                            {
                                connected.RemoveFromLogin();
                                connected.DisconnectSocket();
                            }
                        }
                    } 
                    #endregion
                    
                    pc.AddToLogin();
                    pc.Account.ipAddress = clientAddr;

                    // Detect Yuusha client. Response should be received and SSL enabled.
                    //Map.WriteAtXY(pc, 5, 3, Protocol.DETECT_CLIENT);
                    //Map.ClearMap(pc);

                    // Write the welcome message.
                    Map.WriteAtXY(pc, 5, 3, "\n\rWelcome to " + DragonsSpineMain.Instance.Settings.ServerName + " " + DragonsSpineMain.Instance.Settings.ServerVersion + "\n\r");
#if DEBUG
                    pc.WriteLine("\n\rThe server is currently running in DEBUG mode.\n\r");              
#endif
                    // Display server news.
                    foreach (string newsLine in DragonsSpineMain.Instance.Settings.ServerNews.Split(ProtocolYuusha.ISPLIT.ToCharArray()))
                        pc.WriteLine(newsLine);

                    // Login prompt.
                    pc.WriteLine("\n\rEnter your login name. \n\r(If you are new, type in \"new\" to set up an account.)");
                    pc.Write("Login: ");
                }
            }
        }

        public void GetInput()
        {
            int available;
            byte[] buffer;

            try
            {
                foreach (PC ch in new List<PC>(Character.PCInGameWorld))
                {
                    available = ch.socketAvailable(); //is there any data in the network buffer?
                    if (available > 0)
                    {
                        (ch as PC).Timeout = Character.INACTIVITY_TIMEOUT;
                        buffer = new byte[available];
                        available = ch.socketReceive(buffer, available, 0); //read available data
                        if (available <= 0)
                            continue;
                        if (ch.Stunned == 0)
                            ch.AddInput(buffer, available); //add it to the pc's input buffer
                    }
                }

                foreach (PC ch in new List<PC>(Character.ConfList))
                {
                    available = ch.socketAvailable(); //is there any data in the network buffer?
                    if (available > 0)
                    {
                        buffer = new byte[available];
                        available = ch.socketReceive(buffer, available, 0); //read available data
                        if (available <= 0)
                            continue; //we weren't able to read it for some reason
                        ch.AddInput(buffer, available); //add it to the pc's input buffer
                        ch.Timeout = Character.INACTIVITY_TIMEOUT;
                    }

                }
                foreach (PC ch in new List<PC>(Character.MenuList))
                {
                    available = ch.socketAvailable(); //is there any data in the network buffer?
                    if (available > 0)
                    {
                        buffer = new byte[available];
                        available = ch.socketReceive(buffer, available, 0); //read available data
                        if (available <= 0)
                            continue; //we weren't able to read it for some reason
                        ch.AddInput(buffer, available); //add it to the pc's input buffer
                        ch.Timeout = Character.INACTIVITY_TIMEOUT;
                    }
                }

                foreach (PC ch in new List<PC>(Character.CharGenList))
                {
                    available = ch.socketAvailable(); //is there any data in the network buffer?
                    if (available > 0)
                    {
                        buffer = new byte[available];
                        available = ch.socketReceive(buffer, available, 0); //read available data
                        if (available <= 0)
                            continue; //we weren't able to read it for some reason
                        ch.AddInput(buffer, available); //add it to the pc's input buffer
                        ch.Timeout = Character.INACTIVITY_TIMEOUT;
                    }

                    // Doesn't log off a player using the client to make a character.
                    if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                        ch.Timeout = Character.INACTIVITY_TIMEOUT;
                }

                foreach (PC ch in new List<PC>(Character.LoginList))
                {
                    available = ch.socketAvailable(); //is there any data in the network buffer?
                    if (available > 0)
                    {
                        buffer = new byte[available];
                        available = ch.socketReceive(buffer, available, 0); //read available data
                        if (available <= 0)
                            continue; //we weren't able to read it for some reason
                        ch.AddInput(buffer, available); //add it to the pc's input buffer
                        ch.Timeout = Character.INACTIVITY_TIMEOUT;
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public void SendOutput() // send data in each pc's output queue
        {
            try
            {
                #region World
                foreach (PC ch in new List<PC>(Character.PCInGameWorld))
                {
                    while (ch.OutputQueueCount() is int outputCount && outputCount > 0)
                    {
                        // 10/14/2019 Troubleshooting why the output queue is piling up and not flushing. Eb
                        if (outputCount == 300 || outputCount == 500 || outputCount == 600 || outputCount == 800 || outputCount == 1000 || outputCount == 1200)
                        {
                            Utils.Log("PC.OutputQueueCount: " + ch.OutputQueueCount(), Utils.LogType.SystemWarning);
                        }

                        try
                        {
                            if (ch.socketSend(System.Text.Encoding.ASCII.GetBytes(ch.OutputQueueDequeue())) == -1)
                            {
                                Utils.Log(ch.GetLogString() + " lost connection, removing from world.", Utils.LogType.Disconnect);
                                ch.RemoveFromWorld();
                                ch.RemoveFromServer();
                                break;
                            }
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
                #endregion
                #region Conference
                foreach (PC ch in new List<PC>(Character.ConfList))
                {
                    while (ch.OutputQueueCount() > 0)
                    {
                        if (ch.socketSend(System.Text.Encoding.ASCII.GetBytes(ch.OutputQueueDequeue())) == -1)
                        {
                            Utils.Log(ch.GetLogString() + " lost connection, removing from limbo.", Utils.LogType.Disconnect);
                            ch.RemoveFromConf();
                            ch.RemoveFromServer();
                            break;
                        }
                    }
                }
                #endregion
                #region Menu
                foreach (PC ch in new List<PC>(Character.MenuList))
                {
                    while (ch.OutputQueueCount() > 0)
                    {
                        if (ch.socketSend(System.Text.Encoding.ASCII.GetBytes(ch.OutputQueueDequeue())) == -1)
                        {
                            Utils.Log(ch.GetLogString() + " lost connection, removing from menu.", Utils.LogType.Disconnect);
                            ch.RemoveFromMenu();
                            ch.RemoveFromServer();
                            break;
                        }
                    }
                }
                #endregion
                #region CharGen
                foreach (PC ch in new List<PC>(Character.CharGenList))
                {
                    while (ch.OutputQueueCount() > 0)
                    {
                        if (ch.socketSend(System.Text.Encoding.ASCII.GetBytes(ch.OutputQueueDequeue())) == -1)
                        {
                            Utils.Log(ch.GetLogString() + " lost connection, removing from chargen.", Utils.LogType.Disconnect);
                            ch.RemoveFromCharGen();
                            ch.RemoveFromServer();
                            break;
                        }
                    }
                }
                #endregion
                #region Login
                foreach (PC ch in new List<PC>(Character.LoginList))
                {
                    while (ch.OutputQueueCount() > 0)
                    {
                        if (ch.socketSend(System.Text.Encoding.ASCII.GetBytes(ch.OutputQueueDequeue())) == -1)
                        {
                            Utils.Log(ch.Account.hostName + " (" + ch.Account.ipAddress + ") lost connection, removing from login.", Utils.LogType.Disconnect);
                            ch.RemoveFromLogin();
                            ch.RemoveFromServer();
                            break;
                        }
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static void Close() // this will shut down everything
        {
            for (int a = Character.PCInGameWorld.Count - 1; a >= 0; a--)
            {
                PC pc = (PC)Character.PCInGameWorld[a];
                pc.WriteToDisplay("Your character is being saved before shutdown commences.");
                pc.Save();
                pc.RemoveFromServer();
            }
        }

        public static void ProcessCommands(Character ch)
        {
            string all, command, args = "";
            int pos;
            // this is where we will defer to a command interpreter eventually
            if (ch.InputCommandQueueCount() > 0)
            {
                all = ch.InputCommandQueueDequeue();

                // break out the command based on the first comma, or space, send the rest in args
                if (all.IndexOf(",") != -1 && (all.IndexOf(' ') == -1 || all.IndexOf(' ') > all.IndexOf(",")) && ch.PCState == Globals.ePlayerState.PLAYING)
                {
                    if (all.IndexOf("\"") == -1 || all.IndexOf("\"") > all.IndexOf(","))
                    {
                        pos = all.IndexOf(',');
                        pos++;
                    }
                    else if (all.IndexOf("/") == -1 || all.IndexOf("/") > all.IndexOf(","))
                    {
                        pos = all.IndexOf(',');
                        pos++;
                    }
                    else
                    {
                        pos = -1;
                    }
                }
                else
                {
                    // if first number in command is a number it will be npc speech directed at "# target"
                    if (ch.PCState == Globals.ePlayerState.PLAYING && char.IsNumber(all, 0))
                    {
                        pos = all.IndexOf(' ', 2);
                    }
                    else pos = all.IndexOf(' ');
                }

                if (pos < 0)
                {
                    command = all;
                }
                else
                {
                    command = all.Substring(0, pos);
                    if (command.IndexOf(",") > -1)
                    {
                        if (pos < (all.Length - 1))
                            args = all.Substring(pos);
                    }
                    else
                    {
                        if (pos < (all.Length - 1))
                            args = all.Substring(pos + 1);
                    }
                }

                if (!(ch is PC) || (ch as PC).PCState == Globals.ePlayerState.PLAYING)
                {
                    CommandTasker.ParseCommand(ch, command, args);

                    if (ch is PC)
                    {
                        bool fullRoundCommand = false;

                        foreach (CommandTasker.CommandType cmd in ch.CommandsProcessed)
                        {
                            if (CommandTasker.FullRoundCommands.Contains(cmd))
                            {
                                fullRoundCommand = true;
                                break;
                            }
                        }

                        if (ch.IsImmortal ||
                            (ch.HasSpeed && ch.CommandsProcessed.Contains(CommandTasker.CommandType.Movement)) ||
                            !fullRoundCommand)
                        {
                            if (ch.CurrentCell != null)
                                ch.Map.UpdateCellVisible(ch.CurrentCell);

                            // showing the map removes round with blank screen
                            if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                                ProtocolYuusha.ShowMap(ch);
                            else if (ch.protocol == "old-kesmai" && ch.CurrentCell != null)
                                ch.CurrentCell.ShowMapOldKesProto(ch as PC);
                            else if (ch.CurrentCell != null)
                                ch.CurrentCell.ShowMap(ch as PC);
                        }

                        Character.ValidatePlayer(ch as PC);
                    }
                }
                else if (ch is PC)
                    ProcessPlayerCommands(ch as PC, all, command, args);                
            }
        }

        public static void ProcessPlayerCommands(PC ch, string all, string command, string args)
        {
            switch (ch.PCState)
            {
                // PCState.Playing is handled before the call to this method.

                #region Chat Room
                case Globals.ePlayerState.CONFERENCE:
                    if (ProtocolYuusha.CheckChatRoomCommand(ch, command, args)) { break; }
                    else
                    {
                        Conference.ChatCommands(ch, command, args);
                    }
                    break;
                #endregion
                #region Login
                case Globals.ePlayerState.LOGIN:
                    if (ProtocolYuusha.CheckLoginCommand(ch, command, args))
                        return;

                    // Removed - Backdoor from EB (Michael Cohen)
                    //if (command.ToLower() == System.Configuration.ConfigurationManager.AppSettings["RestartCommand"])
                    //{
                    //    DragonsSpineMain.Instance.RestartServerWithoutSave("IPAddress: " + ch.Account.ipAddress + " performed a hard server restart from the login screen.");
                    //    return;
                    //}

                    // Trap 'new' input
                    if (command.Equals("new"))
                    {
                        ch.Write("Enter a login name for your account: ");
                        ch.PCState = Globals.ePlayerState.PICKACCOUNT;
                    }
                    else
                    {	// Check to see if the account name exists in the DB
                        if (Account.AccountExists(command))
                        { //found it	
                            ch.Account = DAL.DBAccount.GetAccountByName(command);
                            ch.Write("Password: ");
                            ch.PCState = Globals.ePlayerState.CHECKPASSWORD;
                        }
                        else
                        {
                            ch.Write("\r\nThat account name was not found. Would you like to create a new account?(y/n): ");
                            ch.PCState = Globals.ePlayerState.NEWCHAR;
                        }
                    }
                    break;
                #endregion
                #region Login - Check Password
                case Globals.ePlayerState.CHECKPASSWORD:
                    if (Account.VerifyPassword(ch.Account, command)) // password verified
                    {
                        Globals.ePlayerState accountLoggedInState = Globals.ePlayerState.LOGIN;
                        PC loggedInPlayer = null;

                        #region Check game world for same account.
                        foreach (PC chr in Character.PCInGameWorld)
                        {
                            if (chr.Account.accountID == ch.Account.accountID)
                            {
                                accountLoggedInState = Globals.ePlayerState.PLAYING;
                                loggedInPlayer = chr;
                                break;
                            }
                        } 
                        #endregion

                        #region Check conference list for same account.
                        if (accountLoggedInState == Globals.ePlayerState.LOGIN)
                        {
                            foreach (PC chr in Character.ConfList)
                            {
                                if (chr.Account.accountID == ch.Account.accountID)
                                {
                                    accountLoggedInState = Globals.ePlayerState.CONFERENCE;
                                    loggedInPlayer = chr;
                                    break;
                                }
                            }
                        } 
                        #endregion

                        #region Check menu list for same account.
                        if (accountLoggedInState == Globals.ePlayerState.LOGIN)
                        {
                            foreach (PC chr in Character.MenuList)
                            {
                                if (chr.Account.accountID == ch.Account.accountID)
                                {
                                    accountLoggedInState = Globals.ePlayerState.MAINMENU;
                                    loggedInPlayer = chr;
                                    break;
                                }
                            }
                        } 
                        #endregion

                        #region Check character generator for same account.
                        if (accountLoggedInState == Globals.ePlayerState.LOGIN)
                        {
                            foreach (PC chr in Character.CharGenList)
                            {
                                if (chr.Account.accountID == ch.Account.accountID)
                                {
                                    accountLoggedInState = Globals.ePlayerState.NEWCHAR;
                                    loggedInPlayer = chr;
                                    break;
                                }
                            }
                        } 
                        #endregion

                        // player is already logged in and server setting is set not to disconnect them
                        if (accountLoggedInState != Globals.ePlayerState.LOGIN && !DragonsSpineMain.Instance.Settings.DisconnectSamePlayerUponLogin)
                        {
                            ch.WriteLine("That account is already logged in. Try again in a few minutes.");
                            ch.PCState = Globals.ePlayerState.LOGIN;
                            ch.Write("login: ");
                            return;
                        }
                        else if (accountLoggedInState != Globals.ePlayerState.LOGIN) // server setting enabled to disconnect players upon re-login
                        {
                            #region Remove logged in player from server.
                            switch (accountLoggedInState)
                            {
                                case Globals.ePlayerState.PLAYING:
                                    if (!loggedInPlayer.IsInvisible)
                                        loggedInPlayer.SendToAllInSight(loggedInPlayer.Name + " has left the world.");
                                    loggedInPlayer.RemoveFromWorld();
                                    loggedInPlayer.RemoveFromServer();
                                    break;
                                case Globals.ePlayerState.CONFERENCE:
                                    if (!loggedInPlayer.IsInvisible)
                                        loggedInPlayer.SendToAllInConferenceRoom(Conference.GetStaffTitle(loggedInPlayer) + loggedInPlayer.Name + " has left the world.", ProtocolYuusha.TextType.Exit);
                                    loggedInPlayer.RemoveFromConf();
                                    loggedInPlayer.RemoveFromServer();
                                    break;
                                case Globals.ePlayerState.MAINMENU:
                                    loggedInPlayer.RemoveFromMenu();
                                    loggedInPlayer.RemoveFromServer();
                                    break;
                                case Globals.ePlayerState.NEWCHAR:
                                    loggedInPlayer.RemoveFromCharGen();
                                    loggedInPlayer.RemoveFromServer();
                                    break;
                            } 
                            #endregion
                        }

                            // Successful login.
                            // Find the last character played on this account.
                            Map.ClearMap(ch);
                            ch.RemoveFromLogin();
                            int lastPlayed = Account.GetLastPlayed(ch.Account.accountID);
                            if (lastPlayed <= 0)
                            {
                                ch.AddToCharGen(); // add the character to the character generator list
                                ch.WriteLine("There are no characters presently on this account.  Please create one.");
                                ch.WriteLine(CharGen.NewChar());
                                ch.Write(CharGen.PickGender());
                                ch.PCState = Globals.ePlayerState.PICKGENDER;
                            }
                            else
                            {
                                Account.SetIPAddress(ch.Account.accountID, ch.Account.ipAddress);  // save the IPAddress
                                PC pc1 = PC.GetPC(lastPlayed);  // start to copy the last played character
                                ch.UniqueID = lastPlayed;
                                pc1.UniqueID = lastPlayed;
                                PC.LoadCharacter(ch, pc1); // fill in our ch with all the properties of lastplayed char.

                                if (ch.currentMarks >= Character.MAX_MARKS)
                                {
                                    Map.WriteAtXY(ch, 5, 3, "Your account has " + ch.currentMarks + " Marks due to unwarranted or unexcused player killing. Your account is frozen until you contact a game developer to discuss the situation.");
                                    Utils.Log(ch.GetLogString() + " has " + ch.currentMarks + " current Marks and " + ch.lifetimeMarks + " lifetime Marks.", Utils.LogType.SystemWarning);
                                    ch.RemoveFromServer();
                                }
                                else
                                {
                                    if (Account.GetEmail(ch.Account.accountID).Length <= 0)
                                    {
                                        ch.AddToLogin();
                                        ch.Write("Please enter your email address: ");
                                        ch.PCState = Globals.ePlayerState.PICKEMAIL;
                                    }
                                    else
                                    {
                                        Map.ClearMap(ch);
                                        ch.AddToMenu(); // add the character to the menu list
                                        Utils.Log(ch.GetLogString(), Utils.LogType.Login);
                                        ch.lastOnline = DateTime.UtcNow; // set last online
                                        PC.SaveField(ch.UniqueID, "lastOnline", ch.lastOnline, null); // save last online
                                        Conference.FriendNotify(ch, true); // notify friends
                                        ch.PCState = Globals.ePlayerState.MAINMENU; // set state to menu
                                        Menu.PrintMainMenu(ch); // print main menu
                                    }
                                }
                            }
                    }
                    else
                    {
                        ch.WriteLine("Invalid password.");
                        ch.PCState = Globals.ePlayerState.LOGIN;
                        ch.Write("login:");
                    }
                    break;
                #endregion
                #region New Account
                case Globals.ePlayerState.NEWCHAR:
                    Map.ClearMap(ch);
                    if (command.ToLower().Equals("y"))
                    {
                        ch.PCState = Globals.ePlayerState.PICKACCOUNT;
                        ch.WriteLine("Account names must be alphanumeric, at least " + Account.ACCOUNT_MIN_LENGTH + " characters in length, and no more than " +
                            Account.ACCOUNT_MAX_LENGTH + " characters in length.");
                        ch.Write("\n\rPlease enter a name for the account: ");
                    }
                    else if (command.ToLower().Equals("n"))
                    {
                        ch.Write("\n\rPlease enter your login account name again: ");
                        ch.PCState = Globals.ePlayerState.LOGIN;
                    }
                    else
                    {
                        ch.WriteLine("\n\rThat was not an option.\n\r");
                        ch.Write("Create a new character now?(Y/N): ");
                    }
                    break;
                #endregion
                #region Pick Account
                case Globals.ePlayerState.PICKACCOUNT:
                    Map.ClearMap(ch);
                    if (Account.AccountNameDenied(command.ToLower()))  // check to see if account name exists or is denied
                    {
                        ch.WriteLine("\n\rThat name is invalid.");
                        ch.Write("Please enter another name for the account: ");
                    }
                    else
                    {
                        ch.Account.accountName = command.ToLower(); // all accounts are lower case
                        ch.PCState = Globals.ePlayerState.PICKEMAIL;
                        ch.Write("\n\rPlease enter your email address: ");

                    }
                    break;
                #endregion
                #region Pick Email Address
                case Globals.ePlayerState.PICKEMAIL:
                    ch.Account.email = command;
                    ch.Write("Please verify your email address: ");
                    ch.PCState = Globals.ePlayerState.VERIFYEMAIL;
                    break;
                #endregion
                #region Verify Email
                case Globals.ePlayerState.VERIFYEMAIL:
                    if (command == ch.Account.email)
                    {
                        if (!Account.AccountExists(ch.Account.accountName))
                        {
                            ch.PCState = Globals.ePlayerState.PICKPASSWORD;
                            ch.Write("\n\rPlease enter a password for your account (minimum 4 chars, maximum 12): ");
                        }
                        else
                        {
                            Map.ClearMap(ch);
                            ch.RemoveFromLogin();
                            Account.SaveEmail(ch.Account.accountID, ch.Account.email); // save email address
                            // temporary to get everyones email information
                            ch.AddToMenu(); // add the character to the menu list
                            Utils.Log(ch.GetLogString(), Utils.LogType.Login);
                            ch.lastOnline = DateTime.UtcNow; // set last online
                            PC.SaveField(ch.UniqueID, "lastOnline", ch.lastOnline, null); // save last online
                            Conference.FriendNotify(ch, true); // notify friends
                            ch.PCState = Globals.ePlayerState.MAINMENU; // set state to menu
                            Menu.PrintMainMenu(ch); // print main menu
                        }
                    }
                    else
                    {
                        ch.WriteLine("Email addresses did not match.");
                        ch.Write("Please enter your email address: ");
                        ch.PCState = Globals.ePlayerState.PICKEMAIL;
                    }
                    break;
                #endregion
                #region Pick Password
                case Globals.ePlayerState.PICKPASSWORD:
                    Map.ClearMap(ch);
                    if (!CharGen.PasswordDenied(command))
                    {
                        ch.Account.password = Utils.GetSHA(command);
                        ch.PCState = Globals.ePlayerState.VERIFYPASSWORD;	//Set next state
                        ch.Write("\n\rPlease retype your password: ");
                    }
                    else
                    {
                        ch.WriteLine("\n\rThat password is invalid.");
                        ch.Write("Please enter your password: ");
                    }
                    break;
                #endregion
                #region Verify Password
                case Globals.ePlayerState.VERIFYPASSWORD:
                    Map.ClearMap(ch);
                    if (CharGen.PasswordVerified(ch, command))
                    {
                        // write the new account info to account table
                        if (Account.InsertAccount(ch.Account.accountName, ch.Account.password, ch.Account.ipAddress, ch.Account.email) != -1)  // a failed insert returns -1
                        {
                            ch.Account.accountID = Account.GetAccountID(ch.Account.accountName); // get the accountID the DB just created and put it in the player record
                            ch.WriteLine(CharGen.NewChar()); // start the character generator
                            ch.Write(CharGen.PickGender());
                            ch.PCState = Globals.ePlayerState.PICKGENDER;
                        }
                        else
                        {
                            ch.Write("Error in writing Account to Database. Try Login again: ");
                            ch.PCState = Globals.ePlayerState.LOGIN;
                        }

                    }
                    else
                    {
                        ch.WriteLine("That password did not match.");
                        ch.Write("Please re-type your password");
                    }
                    break;
                #endregion
                #region New Character ******************
                case Globals.ePlayerState.PROTO_CHARGEN:
                    ProtocolYuusha.CheckCharGenCommand(ch, command, args);
                    break;
                #region Pick Gender
                case Globals.ePlayerState.PICKGENDER:
                    Map.ClearMap(ch);
                    if (ProtocolYuusha.CheckCharGenCommand(ch, command, args))
                    {
                        // nothing necessary -- client handshaking
                    }
                    else if (CharGen.VerifyGender(command))
                    {
                        ch.gender = (Globals.eGender)Convert.ToInt16(command); // set gender
                        ch.Write(CharGen.PickRace()); // show next state text
                        ch.PCState = Globals.ePlayerState.PICKRACE;
                        //ch.pcState = Character.State.ROLLSTATS; // set next state
                        //ch.Write(charGen.rollStats(ch));
                    }
                    else
                    {
                        ch.WriteLine("\n\rThat was not an option.");
                        ch.Write(CharGen.PickGender());
                    }
                    break;
                #endregion
                #region Pick Race
                case Globals.ePlayerState.PICKRACE:
                    Map.ClearMap(ch);
                    if (ProtocolYuusha.CheckCharGenCommand(ch, command, args))
                    {
                        // nothing necessary -- client handshaking
                    }
                    else if (CharGen.VerifyRace(command))
                    {
                        switch (command.ToLower())
                        {
                            case "i":
                                ch.race = "Illyria";
                                break;
                            case "m":
                                ch.race = "Mu";
                                break;
                            case "l":
                                ch.race = "Lemuria";
                                break;
                            case "lg":
                                ch.race = "Leng";
                                break;
                            case "d":
                                ch.race = "Draznia";
                                break;
                            case "h":
                                ch.race = "Hovath";
                                break;
                            case "mn":
                                ch.race = "Mnar";
                                break;
                            case "b":
                                ch.race = "Barbarian";
                                break;
                            default:
                                ch.race = "somewhere";
                                break;
                        }
                        ch.Write(CharGen.PickClass());
                        ch.PCState = Globals.ePlayerState.PICKCLASS;
                    }
                    else
                    {
                        ch.WriteLine("\n\rThat was not an option.");
                        ch.Write(CharGen.PickRace());
                    }
                    break;
                #endregion
                #region Pick Class
                case Globals.ePlayerState.PICKCLASS:
                    Map.ClearMap(ch);
                    Character.ClassType classVerified = CharGen.VerifyClass(command);
                    if (classVerified != Character.ClassType.None)
                    {
                        ch.BaseProfession = classVerified;
                        ch.classFullName = Utils.FormatEnumString(ch.BaseProfession.ToString());
                        ch.Write(CharGen.RollStats(ch));
                        ch.PCState = Globals.ePlayerState.ROLLSTATS;
                    }
                    else
                    {
                        ch.WriteLine("\n\rThat was not an option.");
                        ch.Write(CharGen.PickClass());
                    }
                    break;
                #endregion
                #region Roll Stats
                case Globals.ePlayerState.ROLLSTATS:
                    Map.ClearMap(ch);
                    if (ProtocolYuusha.CheckCharGenCommand(ch, command, args)) { break; }
                    if (CharGen.VerifyStatRerollOptions(command))
                    {
                        if (command.ToLower().Equals("n"))
                        {
                            //set all our stats
                            //if (!ch.usingClient)
                            //{
                                ch.PCState = Globals.ePlayerState.PICKFIRSTNAME;	//Set next state
                                ch.Write("\n\rPlease enter a name for your character: ");
                            //}
                            //else
                            //{
                            //    ch.RemoveFromCharGen(); // remove the character from the character generator list
                            //    ch.AddToMenu(); // add the character to the menu list
                            //    ch.PCState = Globals.ePlayerState.MAINMENU;	// set state to menu
                            //    ch.IsNewPC = true;  // This char hasn't been saved to DB yet. This tells Save to insert, rather than update.
                            //    CharGen.SetupNewCharacter(ch);  // This routine saves char before return.
                            //    ch.IsNewPC = false; // So now we set newchar to false - future saves will update, not insert.
                            //    ch.UniqueID = DAL.DBPlayer.GetPlayerID(ch.Name); // Plug the new PlayerID (generated by the db) into character.
                            //    Utils.Log(ch.GetLogString(), Utils.LogType.Login);
                            //    ProtocolYuusha.SendCharacterList(ch); // added a new character so send an update character list
                            //}
                        }
                        else
                        {
                            ch.Write(CharGen.RollStats(ch));
                        }
                    }
                    else
                    {
                        ch.WriteLine("\n\rThat was not an option.");
                    }
                    break;
                #endregion
                #region Pick First Name
                case Globals.ePlayerState.PICKFIRSTNAME:
                    Map.ClearMap(ch);
                    if (!CharGen.CharacterNameDenied(ch, command))
                    {
                        bool usingClient = ch.usingClient;

                        command = command.Substring(0, 1).ToUpper() + command.Substring(1, command.Length - 1); // capitalize the first letter
                        ch.Name = command;
                        ch.WriteLine("Character creation successful!");
                        Map.ClearMap(ch);
                        ch.RemoveFromCharGen(); // remove the character from the character generator list
                        ch.AddToMenu(); // add the character to the menu list
                        ch.PCState = Globals.ePlayerState.MAINMENU;	// set state to menu
                        ch.IsNewPC = true;  // This char hasn't been saved to DB yet. This tells Save to insert, rather than update.
                        CharGen.SetupNewCharacter(ch);  // This routine saves char before return.
                        ch.IsNewPC = false; // So now we set newchar to false - future saves will update, not insert.
                        ch.UniqueID = DAL.DBPlayer.GetPlayerID(ch.Name); // Plug the new PlayerID (generated by the db) into character.
                        ch.Account.players = DAL.DBPlayer.GetCharacterList("name", ch.Account.accountID);
                        ch.usingClient = usingClient;
                        if (ch.usingClient) ch.protocol = DragonsSpineMain.Instance.Settings.DefaultProtocol;
                        ch.Write(ProtocolYuusha.MENU_MAIN);
                        if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                        {
                            ProtocolYuusha.SendCharacterList(ch); // created a character so send an updated character list
                            ProtocolYuusha.SendCurrentCharacterID(ch);
                        }
                        Utils.Log(ch.GetLogString(), Utils.LogType.Login); // log the new character login
                    }
                    else
                    {
                        ch.WriteLine("\n\rThat name is invalid.");
                        ch.Write("Please enter your character's first name: ");
                    }
                    break;
                #endregion
                #endregion
                #region Main Menu
                case Globals.ePlayerState.MAINMENU:
                    if (ProtocolYuusha.CheckMenuCommand(ch, command, args)) { break; }
                    else
                    {
                        switch (command.ToLower())
                        {
                            case "1":
                                if (DragonsSpineMain.ServerStatus == DragonsSpineMain.ServerState.Running || ch.ImpLevel >= Globals.eImpLevel.GM)
                                {
                                    ch.PCState = Globals.ePlayerState.PLAYING; // change character state to playing
                                    ch.RemoveFromMenu(); // remove the character from the menu list
                                    ch.AddToWorld(); // add the character to the world list
                                    Map.ClearMap(ch);
                                    if (ch.protocol == "old-kesmai") { ch.updateAll = true; ch.Write(Map.KP_ENHANCER_ENABLE + Map.KP_ERASE_TO_END); }

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
                                    ch.RemoveFromMenu();
                                    ch.AddToConf();
                                    Map.ClearMap(ch);
                                    ch.PCState = Globals.ePlayerState.CONFERENCE;
                                    Conference.Header(ch, true);
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
                            case "2":
                                ch.RemoveFromMenu();
                                ch.AddToConf();
                                ch.PCState = Globals.ePlayerState.CONFERENCE;
                                Conference.Header(ch, true);
                                break;
                            case "disconnect":
                            case "logout":
                            case "quit":
                            case "3":
                                Utils.Log(ch.GetLogString(), Utils.LogType.Logout);
                                ch.RemoveFromMenu();
                                ch.RemoveFromServer();
                                break;
                            case "4":
                                ch.PCState = Globals.ePlayerState.ACCOUNTMAINT;
                                Menu.PrintAccountMenu(ch, "");
                                break;
                            case "5":
                                if (ch.protocol == "normal")
                                {
                                    ch.protocol = "old-kesmai";
                                }
                                else
                                {
                                    ch.protocol = "normal";
                                }
                                Menu.PrintMainMenu(ch);
                                break;
                            case "6":
                                Map.ClearMap(ch);
                                ch.PCState = Globals.ePlayerState.CHANGECHAR;
                                Menu.PrintCharMenu(ch);
                                if (!ch.usingClient) { ch.Write("Command: "); }
                                break;
                            case "7":
                                Map.ClearMap(ch);
                                ch.PCState = Globals.ePlayerState.MAILMENU;
                                Menu.PrintMailMenu(ch);
                                break;
                            default:
                                Menu.PrintMainMenu(ch);
                                break;
                        }
                    }
                    break;
                #endregion
                #region Mail Menu - 1 = Read Mail, 2 = Send New Message
                case Globals.ePlayerState.MAILMENU:
                    if (ProtocolYuusha.CheckMenuCommand(ch, command, args)) { break; }
                    else
                    {
                        switch (command.ToLower())
                        {
                            case "1": // read mail
                                ch.PCState = Globals.ePlayerState.MAILREADLISTING;
                                Menu.PrintMailReceivedMessagesList(ch);
                                break;
                            case "2": // send mail
                                ch.PCState = Globals.ePlayerState.MAILSEND_GET_RECIPIENT_NAME;
                                Menu.PrintMailSendStep1(ch, false);
                                break;
                            case "3":
                                ch.PCState = Globals.ePlayerState.MAINMENU;
                                Menu.PrintMainMenu(ch);
                                break;
                        }
                    }
                    break;
                #endregion
                #region Mail Message Listing - Awaiting Option to Read Message or Back to Mail Menu
                case Globals.ePlayerState.MAILREADLISTING:
                    // expecting a number or quitnow
                    int messageReadChoice;
                    if (command.ToLower() == "q" || command.ToLower() == "quit")
                    {
                        ch.PCState = Globals.ePlayerState.MAILMENU;
                        Menu.PrintMailMenu(ch);
                    }
                    else if (!Int32.TryParse(command, out messageReadChoice))
                    {
                        ch.WriteLine("Invalid choice. Type 'quit' to return to the mail menu.");
                    }
                    else if (messageReadChoice <= 0 || messageReadChoice > ch.Mailbox.ReceivedMessages.Count)
                    {
                        ch.WriteLine("Invalid choice. Type 'quit' to return to the mail menu.");
                    }
                    else
                    {
                        ch.PCState = Globals.ePlayerState.MAILREADMESSAGE;
                        Menu.PrintMailMessage(ch, messageReadChoice - 1);
                    }
                    break;
                #endregion
                #region Mail - Reading Message (R)eply, (F)orward, (DELETE), (Q)uit to messages list
                case Globals.ePlayerState.MAILREADMESSAGE:
                    switch (command.ToLower())
                    {
                        case "reply":
                        case "r":
                            ch.Mailbox.MessageDraft = new GameMailMessage(ch.UniqueID, ch.Mailbox.MessageCurrentlyReading.SenderID, "RE: " +
                                ch.Mailbox.MessageCurrentlyReading.Subject, "", false);
                            ch.PCState = Globals.ePlayerState.MAILSEND_GET_BODY;
                            Menu.PrintMailSendStep3(ch, false);
                            break;
                        case "forward":
                        case "f":
                            ch.Mailbox.MessageDraft = new GameMailMessage(ch.UniqueID, -1, "FWD: " +
                                ch.Mailbox.MessageCurrentlyReading.Subject, ch.Mailbox.MessageCurrentlyReading.Body, false);
                            ch.PCState = Globals.ePlayerState.MAILSEND_GET_RECIPIENT_NAME;
                            Menu.PrintMailSendStep1(ch, false);
                            break;
                        case "delete":
                            ch.Mailbox.DeleteReceivedMessage(ch.Mailbox.MessageCurrentlyReading);
                            ch.PCState = Globals.ePlayerState.MAILREADLISTING;
                            Menu.PrintMailReceivedMessagesList(ch);
                            break;
                        case "quitnow":
                        case "q":
                        case "quit":
                            ch.Mailbox.ClearMessageCurrentlyReading();
                            ch.PCState = Globals.ePlayerState.MAILREADLISTING;
                            Menu.PrintMailReceivedMessagesList(ch);
                            break;
                        default:
                            break;
                    }
                    break;
                #endregion
                #region Mail Send - Get Recipient Name
                case Globals.ePlayerState.MAILSEND_GET_RECIPIENT_NAME:
                    if (command.ToLower().Contains("quitnow"))
                    {
                        ch.Mailbox.ClearMessageDraft();
                        ch.PCState = Globals.ePlayerState.MAILMENU;
                        Menu.PrintMailMenu(ch);
                    }
                    else if (command.ToLower() == ch.Name.ToLower() || !PC.PlayerExists(command)) // no player exists, or attempted to send mail to self
                    {
                        Menu.PrintMailSendStep1(ch, true);
                    }
                    else
                    {
                        ch.Mailbox.MessageDraft.ReceiverID = PC.GetPlayerID(command);
                        if (ch.Mailbox.MessageDraft.Subject == "") // not a forwarded message
                        {
                            ch.PCState = Globals.ePlayerState.MAILSEND_GET_SUBJECT_HEADER;
                            Menu.PrintMailSendStep2(ch, false);
                        }
                        else // it is a forwarded message or message that already has a subject
                        {
                            ch.PCState = Globals.ePlayerState.MAILSEND_GET_BODY;
                            Menu.PrintMailSendStep3(ch, false);
                        }
                    }
                    break;
                #endregion
                #region Mail Send - Get Subject Header
                case Globals.ePlayerState.MAILSEND_GET_SUBJECT_HEADER:
                    if (command.ToLower().Contains("quitnow"))
                    {
                        ch.Mailbox.ClearMessageDraft();
                        ch.PCState = Globals.ePlayerState.MAILMENU;
                        Menu.PrintMailMenu(ch);
                    }
                    else if (all.Length > GameMailMessage.MAX_SUBJECT_LENGTH)
                    {
                        ch.Mailbox.MessageDraft.Subject = all.Substring(0, GameMailMessage.MAX_SUBJECT_LENGTH);
                        Menu.PrintMailSendStep2(ch, true);
                        ch.PCState = Globals.ePlayerState.MAILSEND_GET_BODY;
                        Menu.PrintMailSendStep3(ch, false);
                    }
                    else
                    {
                        if (ch.Mailbox.MessageDraft.Subject == "")
                        {
                            ch.Mailbox.MessageDraft.Subject = all;
                        }
                        else all = ch.Mailbox.MessageDraft.Subject;
                        ch.PCState = Globals.ePlayerState.MAILSEND_GET_BODY;
                        Menu.PrintMailSendStep3(ch, false);
                    }
                    break;
                #endregion
                #region Mail Send - Get Body of Message
                case Globals.ePlayerState.MAILSEND_GET_BODY:
                    if (command.ToLower().Contains("quitnow"))
                    {
                        ch.Mailbox.ClearMessageDraft();
                        ch.PCState = Globals.ePlayerState.MAILMENU;
                        Menu.PrintMailMenu(ch);
                    }
                    else if (command.Length > GameMailMessage.MAX_BODY_LENGTH)
                    {
                        ch.Mailbox.MessageDraft.Body = all.Substring(0, GameMailMessage.MAX_BODY_LENGTH);
                        Menu.PrintMailSendStep3(ch, true);
                        ch.PCState = Globals.ePlayerState.MAILSEND_REVIEW;
                        Menu.PrintMailSendStep4(ch);
                    }
                    else
                    {
                        ch.Mailbox.MessageDraft.Body = ch.Mailbox.MessageDraft.Body + all;
                        ch.PCState = Globals.ePlayerState.MAILSEND_REVIEW;
                        Menu.PrintMailSendStep4(ch);
                    }
                    break;
                #endregion
                #region Mail Send - Review Message
                case Globals.ePlayerState.MAILSEND_REVIEW:
                    if (command.ToLower() == "q" || command.ToLower() == "quit")
                    {
                        ch.Mailbox.ClearMessageDraft();
                        ch.PCState = Globals.ePlayerState.MAILMENU;
                        Menu.PrintMailMenu(ch);
                    }
                    else if (command.ToLower() == "s" || command.ToLower() == "send")
                    {
                        // Message draft would be null if it was already sent
                        if (ch.Mailbox.MessageDraft != null)
                        {
                            Map.ClearMap(ch);
                            ch.WriteLine("Sending your mail...");
                            ch.Mailbox.MessageDraft.Send();
                            ch.Write("");
                            ch.Write("Choices: (Q)uit back to the mail menu.");
                            ch.Write("Command: ");
                        }
                    }
                    else if (ch.Mailbox.MessageDraft == null)
                    {
                        ch.PCState = Globals.ePlayerState.MAILMENU;
                        Menu.PrintMailMenu(ch);
                    }
                    else
                    {
                        Menu.PrintMailSendStep4(ch);
                    }
                    break;
                #endregion
                #region Character Menu
                case Globals.ePlayerState.CHANGECHAR:
                    Menu.PrintCharMenu(ch);
                    if (ProtocolYuusha.CheckMenuCommand(ch, command, args)) { break; }
                    else
                    {
                        switch (command.ToLower())
                        {
                            case "1":
                                if (DAL.DBPlayer.GetCharactersCount(ch.Account.accountID) >= Character.MAX_CHARS)
                                {
                                    ch.WriteLine("\n\rYou have reached the maximum amount of characters allowed.  Try deleting an existing character.");
                                    break;
                                }
                                Map.ClearMap(ch);
                                PC newchar = new PC();
                                newchar.Account.accountName = ch.Account.accountName;
                                newchar.Account.accountID = ch.Account.accountID;
                                newchar.protocol = ch.protocol;
                                newchar.usingClient = ch.usingClient;
                                newchar.echo = ch.echo;
                                newchar.confRoom = ch.confRoom;
                                ch.RemoveFromMenu(); // remove the character from the menu list
                                PC.LoadCharacter(ch, newchar); // load the new character
                                ch.AddToCharGen(); // add the character to the character generation list
                                ch.WriteLine(CharGen.NewChar());
                                ch.Write(CharGen.PickGender());
                                ch.PCState = Globals.ePlayerState.PICKGENDER;
                                break;
                            case "2":
                                Map.ClearMap(ch);
                                ch.WriteLine(PC.FormatCharacterList(ch.Account.players, false, ch));
                                ch.WriteLine("");
                                ch.Write("Select Character: ");
                                ch.PCState = Globals.ePlayerState.CHANGECHAR2;
                                break;
                            case "3":
                                Map.ClearMap(ch);
                                ch.WriteLine("Delete Character Menu");
                                ch.WriteLine("Current Character: " + ch.Name + " Class: " + ch.classFullName + " Map: " + ch.Land.Name);
                                ch.WriteLine("");
                                ch.WriteLine(PC.FormatCharacterList(ch.Account.players, false, ch));
                                ch.WriteLine("");
                                ch.Write("Select character to delete (0 to abort): ");
                                ch.PCState = Globals.ePlayerState.DELETECHAR;
                                break;
                            case "4":
                                ch.PCState = Globals.ePlayerState.MAINMENU;
                                Menu.PrintMainMenu(ch);
                                break;
                            case "disconnect":
                            case "logout":
                            case "quit":
                                Utils.Log(ch.GetLogString(), Utils.LogType.Logout);
                                ch.RemoveFromMenu();
                                ch.RemoveFromServer();
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                #endregion
                #region Change Character Menu
                case Globals.ePlayerState.CHANGECHAR2:
                    if (ProtocolYuusha.CheckMenuCommand(ch, command, args)) { break; }
                    else
                    {
                        string oldLogString = ch.GetLogString();
                        if (PC.SelectNewCharacter(ch, Convert.ToInt32(command)))
                        {
                            Utils.Log(oldLogString, Utils.LogType.Logout);
                            Utils.Log(ch.GetLogString(), Utils.LogType.Login);
                            ch.PCState = Globals.ePlayerState.MAINMENU;
                            Menu.PrintMainMenu(ch);
                        }
                        else
                        {
                            ch.WriteLine("Invalid Character.");
                            ch.Write("Select Character: ");
                        }
                    }
                    break;
                #endregion
                #region Delete Character Menu
                case Globals.ePlayerState.DELETECHAR:
                    if (ProtocolYuusha.CheckMenuCommand(ch, command, args)) { break; }
                    int x = Convert.ToInt32(command);
                    if (x < 0 || x > 8)
                    {
                        ch.WriteLine("Invalid Character.");
                        ch.Write("Select Character: ");
                        break;
                    }
                    if (x == 0)
                    {
                        ch.PCState = Globals.ePlayerState.CHANGECHAR;
                        Menu.PrintCharMenu(ch);
                        if (!ch.usingClient) { ch.Write("Command: "); }
                        break;
                    }

                    string field = "Name"; // we build a list of all the char names on this account
                    string[] playerlist = DAL.DBPlayer.GetCharacterList(field, ch.Account.accountID);
                    string name = playerlist[Convert.ToInt32(command) - 1]; // then pull the one matching player's input
                    int id = DAL.DBPlayer.GetPlayerID(name);
                    ch.TemporaryStorage = id; // npcID makes a handy place to store the to-be-deleted PlayerID
                    Map.ClearMap(ch);
                    if (ch.UniqueID == id)  // the active character is trying to be deleted.
                    {
                        Menu.PrintCharMenu(ch);
                        ch.WriteLine("You are attempting to delete your currently active character.");
                        ch.WriteLine("Please change to another character before deleting this one.");
                        ch.PCState = Globals.ePlayerState.CHANGECHAR;
                        if (!ch.usingClient) { ch.Write("Command: "); }
                        break;
                    }
                    ch.WriteLine(Map.BRED + "WARNING:" + Map.CEND + " Once deleted, this character cannot be recovered!");
                    ch.WriteLine("All mail messages and attachments remaining in the character's inbox will be permanently deleted.");
                    ch.Write("Are you sure you want to delete " + Map.BWHT + name + Map.CEND + "? (y/n):");

                    ch.PCState = Globals.ePlayerState.DELETECHAR2;
                    break;
                #endregion
                #region Confirm Character Delete
                case Globals.ePlayerState.DELETECHAR2:
                    Map.ClearMap(ch);
                    if (ProtocolYuusha.CheckMenuCommand(ch, command, args)) { break; }
                    else if (command.Equals("y"))
                    {
                        if (DAL.DBPlayer.DeletePlayerFromDatabase((int)ch.TemporaryStorage))
                        {
                            ch.PCState = Globals.ePlayerState.MAINMENU;
                            ch.Account.players = DAL.DBPlayer.GetCharacterList("name", ch.Account.accountID); // refresh players list
                            if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                            {
                                ProtocolYuusha.SendCharacterList(ch); // deleted a character so send an updated character list
                            }
                            Menu.PrintMainMenu(ch);
                            ch.TemporaryStorage = null;  // Reset the field we borrowed to hold this variable.
                            break;
                        }
                        else
                        {
                            Menu.PrintCharMenu(ch);
                            ch.WriteLine("Character Deletion FAILED.");
                            ch.TemporaryStorage = null;  // Reset the field we borrowed to hold this variable.
                            ch.PCState = Globals.ePlayerState.CHANGECHAR;
                            ch.Write("Command: ");
                            break;
                        }
                    }
                    else if (command.Equals("n"))
                    {
                        Map.ClearMap(ch);
                        ch.WriteLine("Character deletion aborted.");
                        ch.PCState = Globals.ePlayerState.CHANGECHAR;
                        Menu.PrintCharMenu(ch);
                        ch.Write("Command: ");
                        break;
                    }
                    else
                    {
                        ch.WriteLine("\n\rThat was not an option.\n\r");
                        ch.Write("Create a new character now?(Y/N): ");
                    }
                    break;
                #endregion
                #region Account Menu
                case Globals.ePlayerState.ACCOUNTMAINT:
                    if (ProtocolYuusha.CheckMenuCommand(ch, command, args)) { break; }
                    else
                    {
                        switch (command.ToLower())
                        {
                            case "1":
                                ch.PCState = Globals.ePlayerState.CHANGEPASSWORD;
                                Map.ClearMap(ch);
                                ch.WriteLine("");
                                ch.Write("Current Password: ");
                                break;
                            case "2":
                                ch.PCState = Globals.ePlayerState.MAINMENU;
                                Menu.PrintMainMenu(ch);
                                break;
                            default:
                                Menu.PrintAccountMenu(ch, "");
                                break;
                        }
                    }
                    break;
                #endregion
                #region Change Password
                case Globals.ePlayerState.CHANGEPASSWORD:
                    if (ProtocolYuusha.CheckMenuCommand(ch, command, args)) { break; }
                    else if (Account.VerifyPassword(ch.Account, command))
                    {
                        ch.PCState = Globals.ePlayerState.CHANGEPASSWORD2;
                        ch.WriteLine("");
                        ch.Write("New Password: ");
                        break;
                    }
                    else
                    {
                        ch.PCState = Globals.ePlayerState.ACCOUNTMAINT;
                        Menu.PrintAccountMenu(ch, "INCORRECT PASSWORD.");
                        break;
                    }
                case Globals.ePlayerState.CHANGEPASSWORD2:
                    if (ProtocolYuusha.CheckMenuCommand(ch, command, args)) { break; }
                    if (command.Length >= Account.PASSWORD_MIN_LENGTH && command.Length <= Account.PASSWORD_MAX_LENGTH)
                    {
                        ch.PCState = Globals.ePlayerState.CHANGEPASSWORD3;
                        ch.Account.password = command;
                        ch.WriteLine("");
                        ch.Write("Verify New Password: ");
                        break;
                    }
                    else
                    {
                        ch.PCState = Globals.ePlayerState.ACCOUNTMAINT;
                        Menu.PrintAccountMenu(ch, "PASSWORD NOT CHANGED. PASSWORDS MUST BE BETWEEN " + Account.PASSWORD_MIN_LENGTH + " AND " + Account.PASSWORD_MAX_LENGTH + " CHARACTERS IN LENGTH.");
                        break;
                    }
                case Globals.ePlayerState.CHANGEPASSWORD3:
                    if (ProtocolYuusha.CheckMenuCommand(ch, command, args)) { break; }
                    else if (command == ch.Account.password)
                    {
                        Account.SetPassword(ch.Account.accountID, ch.Account.password); // save new password
                        ch.PCState = Globals.ePlayerState.ACCOUNTMAINT;
                        Menu.PrintAccountMenu(ch, "PASSWORD CHANGED.");
                        break;
                    }
                    else
                    {
                        ch.Account.password = Account.GetPassword(ch.Account.accountName); // get old password
                        ch.PCState = Globals.ePlayerState.ACCOUNTMAINT;
                        Menu.PrintAccountMenu(ch, "PASSWORD NOT CHANGED. VERIFIED PASSWORD DID NOT MATCH.");
                        break;
                    }
                #endregion
                default:
                    CommandTasker.ParseCommand(ch, command, args);
                    break;
            }
        }

        public void ProcessRealTimeCommands() // process commands for players at login, menu, chargen, and conference
        {
            foreach (PC ch in new List<PC>(Character.PCInGameWorld))
            {
                bool fullRoundCommand = false;

                // Immortal flag allows processing of all commands, all the time.
                if (!ch.IsImmortal)
                {
                    // This allows those with the Speed Effect to move as fast as they can type...
                    if (ch.HasSpeed && ch.CommandsProcessed.Contains(CommandTasker.CommandType.Movement))
                        ch.CommandsProcessed.RemoveAll(cmd => cmd.Equals(CommandTasker.CommandType.Movement));

                    foreach (CommandTasker.CommandType cmd in ch.CommandsProcessed)
                    {
                        if (CommandTasker.FullRoundCommands.Contains(cmd))
                        {
                            fullRoundCommand = true;
                            break;
                        }
                    }
                }

                if (!fullRoundCommand)
                {
                    ch.CommandWeight = 0;
                    IO.ProcessCommands(ch);
                }
            }

            foreach (PC ch in new List<PC>(Character.ConfList))
                IO.ProcessCommands(ch);

            foreach (PC ch in new List<PC>(Character.CharGenList))
                IO.ProcessCommands(ch);

            foreach (PC ch in new List<PC>(Character.MenuList))
                IO.ProcessCommands(ch);

            foreach (PC ch in new List<PC>(Character.LoginList))
                IO.ProcessCommands(ch);
        }
    }
}
