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
using System.ComponentModel;
using System.Data;
using System.Text;
using DragonsSpine.GameWorld;

using DSPNet;
using System.Collections;

namespace DragonsSpine
{
    public class ProtoClientIO
    {
        public static Server server;
        delegate void OnReceiveDelegate(Client client, Packet packet);
        public void startProtoServer()
        {
            server = DSPNet.Instances.CreateServer(2000, 4000);
            Console.WriteLine("Protocol Server listening on port 4000.");
            server.OnReceive += new ServerDataHandler(server_OnReceive);

        }

        void server_OnReceive(Client client, Packet packet)
        {

            lock (client)
            {
                Console.WriteLine("Client: " + client.Name + " : MessageType: " + (string)packet.messageType.ToString()+" Message: "+ (string)packet.message + "\r\n");
                ProcessMessage(client, packet);
            }

        }
        
        private void ProcessMessage(Client client, Packet packet)
        {
            Packet np = new Packet();
            if (packet.messageType == 98)
            {
                client.UpdateName((string)packet.message);
                return;
            }
            #region MessageType 99 & 100 - Login
            if (packet.messageType == 99) // Set the client name, eg accountName
            {
                if (Account.AccountExists((string)packet.message))
                {
                    
                    client.Account = DAL.DBAccount.GetAccountByName((string)packet.message);
                    client.ActiveCharacter = new PC();
                    np = new Packet();
                    np.messageType = 99;
                    np.message = "1";
                    server.SendToName(client.Name, np);
                }
                else
                {
                    np = new Packet();
                    np.messageType = 99;
                    np.message = "0";
                    server.SendToName(client.Name, np);
                }
                return;
            }
            if (packet.messageType == 100) // Password check for accountName - AuthCheck
            {
                bool auth = Account.VerifyPassword(client.Account, (string)packet.message);
                if (auth)
                {
                    np = new Packet();
                    np.messageType = 100;
                    client.IsAuthenticated = true;
                    client.UpdateName(client.Account.accountName);
                    int lastPlayed = Account.GetLastPlayed(client.Account.accountID);
                    if (lastPlayed > 0)
                    {
                        client.ActiveCharacter.IsPC = true;
                        client.ActiveCharacter.Account = client.Account;
                        PC pc1 = PC.GetPC(lastPlayed);
                        pc1.UniqueID = lastPlayed;
                        client.ActiveCharacter.UniqueID = lastPlayed;
                        PC.LoadCharacter(client.ActiveCharacter, pc1);
                        client.ActiveCharacter.PCState = Globals.ePlayerState.MAINMENU;
                        client.ActiveCharacter.protocol = "Proto";
                        
                        np.message = "1";
                        server.SendToName(client.Name, np);

                        np = new Packet();
                        np.messageType = 7;
                        np.message = client.ActiveCharacter.Name + "|" + client.ActiveCharacter.classFullName + "|" + client.ActiveCharacter.Level + "|" + client.ActiveCharacter.CurrentCell.Map.Name;
                        server.SendToName(client.Name, np);
                        return;
                    }
                    else
                    {
                        client.ActiveCharacter.Account = client.Account;
                        client.ActiveCharacter.PCState = Globals.ePlayerState.NEWCHAR;
                        client.ActiveCharacter.protocol = "Proto";
                        np.message = "2"; // no characters
                    }
                   
                    
                    server.SendToName(client.Name, np);
                }
                else
                {
                    np = new Packet();
                    np.messageType = 100;
                    np.message = "0";
                    server.SendToName(client.Name, np);
                }
                return;
            }
            #endregion
            #region MessageType 102 - 105 - New Account
            if (packet.messageType == 102) // new account
            {
                if (Account.AccountExists((string)packet.message) || Account.AccountNameDenied((string)packet.message))
                {
                    np = new Packet();
                    np.message = "0";
                    np.messageType = 102;
                    server.SendToName(client.Name, np);
                    client.Shutdown();
                    return;
                }
                client.ActiveCharacter = new PC();
                client.ActiveCharacter.Account.accountName = ((string)packet.message).ToLower();
                
                client.ActiveCharacter.PCState = Globals.ePlayerState.PICKEMAIL;
                np = new Packet();
                np.message = "1";
                np.messageType = 102;
                server.SendToName(client.Name, np);
                return;
            }
            if (packet.messageType == 103)
            {
                if (client.ActiveCharacter.PCState == Globals.ePlayerState.PICKEMAIL)
                {
                    client.ActiveCharacter.PCState = Globals.ePlayerState.PICKPASSWORD;
                    client.ActiveCharacter.Account.email = (string)packet.message;
                    np = new Packet();
                    np.message = "1";
                    np.messageType = 103;
                    server.SendToName(client.Name, np);
                }
                return;
            }
            if (packet.messageType == 104)
            {
                if (client.ActiveCharacter.PCState == Globals.ePlayerState.PICKPASSWORD)
                {
                    client.ActiveCharacter.Account.password = Utils.GetSHA((string)packet.message);
                    client.ActiveCharacter.PCState = Globals.ePlayerState.NEWCHARVERIFY;
                    np = new Packet();
                    np.message = "1";
                    np.messageType = 104;
                    server.SendToName(client.Name, np);
                }
                return;
            }
            if (packet.messageType == 105)
            {
                if (client.ActiveCharacter.PCState == Globals.ePlayerState.NEWCHARVERIFY)
                {
                    client.ActiveCharacter.Account.ipAddress = "";
                    if (Account.InsertAccount(client.ActiveCharacter.Account.accountName, client.ActiveCharacter.Account.password, client.ActiveCharacter.Account.ipAddress, client.ActiveCharacter.Account.email) != -1)  // a failed insert returns -1
                    {
                        // expect chargen now
                        np = new Packet();
                        np.messageType = 105;
                        np.message = "1";
                        server.SendToName(client.Name, np);
                        client.Shutdown();
                    }
                    else
                    {
                        np = new Packet();
                        np.messageType = 105;
                        np.message = "0";
                        server.SendToName(client.Name, np);
                    }
                }
                return;
            }
            #endregion

            if (!client.IsAuthenticated) { return; } // Dont allow unauthenticated clients past this point

            // can test the message for the presence of a string regardless of the messageType
            #region Test strings
            if ((string)packet.message == "test")
            {
                Packet newpack = new Packet();
                newpack.message = "Test test test...";
                newpack.messageType = 1;
                server.SendToName(client.Name, newpack);
                return;
            }
            if ((string)packet.message == "status")
            {
                Packet newpack = new Packet();
                newpack.message = "Status: Round: " + DragonsSpineMain.GameRound + " | NPCs: " + Character.NPCInGameWorld.Count;
                newpack.messageType = 1;
                server.SendToName(client.Name, newpack);
                return;
            }
            
            #endregion
            
            switch (packet.messageType)
            {
                #region 4 - Status (Health/Stamina/Mana)
                case 4: // Status - Health / Stamina / Mana
                    np = new Packet();
                    np.message = "|" + client.ActiveCharacter.Hits + "," + client.ActiveCharacter.HitsFull + "|" + client.ActiveCharacter.Stamina + "," + client.ActiveCharacter.StaminaFull + "|" + client.ActiveCharacter.Mana + "," + client.ActiveCharacter.ManaFull;
                    np.messageType = 4;
                    server.SendToName(client.Name, np);
                    return;
                #endregion
                #region 5 - Chargen
                case 5: // Chargen
                    ProcessChargen(client, packet);
                    return;
                #endregion
                #region 6 - Conference Room
                case 6:
                    ProcessConferenceCommand(client, packet);
                    return;
                #endregion
                case 7: // Main Menu
                    ProcessMainMenuCommand(client, packet);
                    return;
                case 8: 
                    return;
                #region 10 - Character Cell Request 
                case 10: // Character Cell Requested
                    if ((string)packet.message == "1")
                    {
                        client.ActiveCharacter.PCState = Globals.ePlayerState.PLAYING;
                        client.ActiveCharacter.AddProtoClientToWorld();
                    }
                    //maploc = client.ActiveCharacter.CurrentCell.X + "|" + client.ActiveCharacter.CurrentCell.Y + "|" + client.ActiveCharacter.CurrentCell.Z + "|" + client.ActiveCharacter.CurrentCell.MapID + "|" + client.ActiveCharacter.CurrentCell.LandID;
                    
                    np = new Packet();
                    np = mapPacket(client, client.ActiveCharacter.CurrentCell);
                    np.messageType = 10;
                    //np.message = maploc;
                    server.SendToName(client.Name, np);                    
                    return;
                #endregion
                #region 11 & 12 - Movement Command / Normal Command
                case 11: // Movement command
                    ProcessTextCommand(client, packet);                    
                    return;
                case 12: // Text Command
                    ProcessTextCommand(client, packet);
                    return;
                #endregion
                #region 13, 14, 15 - Mob/Loot/Effect Cell display
                case 13: // Mob Cells
                    ProcessMobCells(client);
                    return;
                case 14:
                    ProcessLootCells(client);
                    return;
                case 15:
                    ProcessEffectCells(client);
                    return;
                #endregion
                #region 20 - RightHand
                case 20: // Right Hand request
                    np = new Packet();
                    np.messageType = 20;
                    if (client.ActiveCharacter.RightHand != null)
                        np.message = client.ActiveCharacter.RightHand.name + "|" + client.ActiveCharacter.RightHand.itemID +"|"+ client.ActiveCharacter.RightHand.longDesc;
                    else
                        np.message = "0|0";
                    server.SendToName(client.Name, np);
                    return;
                #endregion
                #region 21 - LeftHand
                case 21: // LeftHand request
                    np = new Packet();
                    np.messageType = 21;
                    if (client.ActiveCharacter.LeftHand != null)
                        np.message = client.ActiveCharacter.LeftHand.name + "|" + client.ActiveCharacter.LeftHand.itemID + "|" + client.ActiveCharacter.LeftHand.UniqueID;
                    else
                        np.message = "0|0";
                    server.SendToName(client.Name, np);
                    return;
                #endregion
                #region 101 - Quit
                case 101: // client quit

                    client.ActiveCharacter.CurrentCell = null;
                    client.Account.lastOnline = DateTime.Now;
                    client.ActiveCharacter.RemoveFromWorld();
                    client.ActiveCharacter.protocol = "Normal";
                    client.Shutdown();
                    return;
                #endregion
                default:
                    break;
            }
        }

        private void ProcessMainMenuCommand(Client client, Packet packet)
        {
            string[] data = ((string)packet.message).Split("|".ToCharArray());

        }

        private void ProcessConferenceCommand(Client client, Packet packet)
        {

            string[] data = ((string)packet.message).Split("|".ToCharArray());
            string commandType = data[0];
            string command = data[1];
            string args = "";

            switch (commandType)
            {
                case "0": // Enter conference
                    client.ActiveCharacter.PCState = Globals.ePlayerState.CONFERENCE;
                    client.ActiveCharacter.AddToConf();
                    Conference.Header(client.ActiveCharacter, true);                    
                    break;
                case "1": // conference command
                    ChatCommands(client, command, args);
                    break;
                default:
                    break;
            }
            
        }

        private void ProcessChargen(Client client, Packet packet)
        {
            Packet np = new Packet();
            np.messageType = 5;

            //Step 1: Gender
            //Step 2: HomeLand
            //Step 3: Class
            //Step 4: Stats
            //Step 5: Name

            // step|data
            string[] data = ((string)packet.message).Split("|".ToCharArray());
            switch(data[0])
            {
                #region Gender Selection
                case "1":
                    
                    if (data[1].ToLower() == "m")
                    {
                        client.ActiveCharacter.gender = Globals.eGender.Male;
                    }
                    else
                    {
                        client.ActiveCharacter.gender = Globals.eGender.Female;
                    }
                    np.message = "1|1";

                    break;
                #endregion
                #region HomeLand Selection
                case "2":
                    if (data[1] == "0")
                    {
                        client.ActiveCharacter.race = "Illyria";
                    }
                    if (data[1] == "1")
                    {
                        client.ActiveCharacter.race = "Mu";
                    }
                    if (data[1] == "2")
                    {
                        client.ActiveCharacter.race = "Lemuria";
                    }
                    if (data[1] == "3")
                    {
                        client.ActiveCharacter.race = "Leng";
                    }
                    if (data[1] == "4")
                    {
                        client.ActiveCharacter.race = "Draznia";
                    }
                    if (data[1] == "5")
                    {
                        client.ActiveCharacter.race = "Hovath";
                    }
                    if (data[1] == "6")
                    {
                        client.ActiveCharacter.race = "Mnar";
                    }
                    if (data[1] == "7")
                    {
                        client.ActiveCharacter.race = "Barbarian";
                    }
                    np.message = "2|1";
                    break;
                #endregion
                #region Class Selection
                case "3":
                    Character.ClassType myclass = CharGen.VerifyClass(data[1]);
                    if (myclass != Character.ClassType.None)
                    {
                        client.ActiveCharacter.BaseProfession = myclass;
                        client.ActiveCharacter.classFullName = Utils.FormatEnumString(client.ActiveCharacter.BaseProfession.ToString());
                    }
                    np.message = "3|1";
                    break;
                #endregion
                #region Stats Rolling
                case "4":
                    client.ActiveCharacter.CurrentCell = Cell.GetCell(0, 0, 0, 41, 34, 0);
                    if (client.ActiveCharacter.Level != 3) { client.ActiveCharacter.Level = 3; } // set level to 3
                    client.ActiveCharacter.Strength = CharGen.AdjustMinMaxStat(CharGen.RollStat() + CharGen.GetRacialBonus(client.ActiveCharacter, Globals.eAbilityStat.Strength));
                    client.ActiveCharacter.Dexterity = CharGen.AdjustMinMaxStat(CharGen.RollStat() + CharGen.GetRacialBonus(client.ActiveCharacter, Globals.eAbilityStat.Dexterity));
                    client.ActiveCharacter.Intelligence = CharGen.AdjustMinMaxStat(CharGen.RollStat() + CharGen.GetRacialBonus(client.ActiveCharacter, Globals.eAbilityStat.Intelligence));
                    client.ActiveCharacter.Wisdom = CharGen.AdjustMinMaxStat(CharGen.RollStat() + CharGen.GetRacialBonus(client.ActiveCharacter, Globals.eAbilityStat.Wisdom));
                    client.ActiveCharacter.Constitution = CharGen.AdjustMinMaxStat(CharGen.RollStat() + CharGen.GetRacialBonus(client.ActiveCharacter, Globals.eAbilityStat.Constitution));
                    client.ActiveCharacter.Charisma = CharGen.AdjustMinMaxStat(CharGen.RollStat() + CharGen.GetRacialBonus(client.ActiveCharacter, Globals.eAbilityStat.Charisma));

                    if (client.ActiveCharacter.Strength >= 16)
                        client.ActiveCharacter.strengthAdd = 1;
                    else client.ActiveCharacter.strengthAdd = 0;

                    if (client.ActiveCharacter.Dexterity >= 16)
                        client.ActiveCharacter.dexterityAdd = 1;
                    else client.ActiveCharacter.dexterityAdd = 0;

                    client.ActiveCharacter.HitsMax = Rules.GetHitsGain(client.ActiveCharacter, client.ActiveCharacter.Level) + (int)(client.ActiveCharacter.Land.StatCapOperand / 1.5);
                    client.ActiveCharacter.StaminaMax = Rules.GetStaminaGain(client.ActiveCharacter, 1) + (int)(client.ActiveCharacter.Land.StatCapOperand / 8);
                    client.ActiveCharacter.ManaMax = 0;
                    if (client.ActiveCharacter.IsSpellUser)
                    {
                        client.ActiveCharacter.ManaMax = Rules.GetManaGain(client.ActiveCharacter, 1) + (int)(client.ActiveCharacter.Land.StatCapOperand / 8);
                    }
                    if (client.ActiveCharacter.HitsMax > Rules.GetMaximumHits(client.ActiveCharacter))
                        client.ActiveCharacter.HitsMax = Rules.GetMaximumHits(client.ActiveCharacter);
                    np.message = "4|1," + client.ActiveCharacter.Strength + "," + client.ActiveCharacter.Dexterity + "," + client.ActiveCharacter.Intelligence +
                        "," + client.ActiveCharacter.Wisdom + "," + client.ActiveCharacter.Constitution + "," + client.ActiveCharacter.Charisma +
                        "," + client.ActiveCharacter.HitsMax + "," + client.ActiveCharacter.StaminaMax + "," + client.ActiveCharacter.ManaMax;
                    break;
                #endregion
                #region Name Selection
                case "5":
                    if (!CharGen.CharacterNameDenied(client.ActiveCharacter, data[1]))
                    {
                        client.ActiveCharacter.Name = data[1];
                        client.ActiveCharacter.PCState = Globals.ePlayerState.MAINMENU;
                        client.ActiveCharacter.IsNewPC = true;  // This char hasn't been saved to DB yet. This tells Save to insert, rather than update.
                        CharGen.SetupNewCharacter(client.ActiveCharacter);  // This routine saves char before return.
                        client.ActiveCharacter.IsNewPC = false; // So now we set newchar to false - future saves will update, not insert.
                        client.ActiveCharacter.UniqueID = DAL.DBPlayer.GetPlayerID(client.ActiveCharacter.Name); // Plug the new PlayerID (generated by the db) into character.
                        client.ActiveCharacter.Account.players = DAL.DBPlayer.GetCharacterList("name", client.ActiveCharacter.Account.accountID);
                        Utils.Log(client.ActiveCharacter.GetLogString(), Utils.LogType.Login);
                        np.message = "5|1"; // everything is good
                    }
                    else
                    {
                        np.message = "5|0"; // bad name
                    }
                    break;
                #endregion
                default:
                    break;
            }

            server.SendToName(client.Name, np);
        }
        private Packet mapPacket(Client client, Cell centerCell)
        {
            Packet mapPak = new Packet();
            mapPak.messageType = 10;
            string mapstring = "";
            mapstring = client.ActiveCharacter.CurrentCell.X + "|" + client.ActiveCharacter.CurrentCell.Y + "|" + client.ActiveCharacter.CurrentCell.Z + "|" + client.ActiveCharacter.CurrentCell.MapID + "|" + client.ActiveCharacter.CurrentCell.LandID;
                    
            Cell[] cellArray = Cell.GetApplicableCellArray(centerCell, 3);
            for (int x = 0; x < 49; x++)
            {
                if (cellArray[x] == null)
                {
                    mapstring += "|0";
                }
                else if (client.ActiveCharacter.CurrentCell.visCells[x] == false)
                {
                    mapstring += "|0";
                }
                else
                {
                    mapstring += "|1"; //convertMapString(cellArray[x]);
                }
            }
            mapPak.message = mapstring;
            return mapPak;
        }
        private void ProcessTextCommand(Client client, Packet packet)
        {
            if (client.ActiveCharacter.PCState == Globals.ePlayerState.CONFERENCE)
            {
                ProcessConferenceCommand(client, packet);
                return;
            }
            string command = "";
            string args = "";
            string all = "";
            int pos;
            all =(string) packet.message;
            // break out the command based on the first comma, or space, send the rest in args
            if (all.IndexOf(",") != -1 && (all.IndexOf(' ') == -1 || all.IndexOf(' ') > all.IndexOf(",")) && client.ActiveCharacter.PCState == Globals.ePlayerState.PLAYING)
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
                if (client.ActiveCharacter.PCState == Globals.ePlayerState.PLAYING && char.IsNumber(all, 0))
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
            if (!(client.ActiveCharacter is PC) || (client.ActiveCharacter as PC).PCState == Globals.ePlayerState.PLAYING)
            {
                CommandTasker.ParseCommand(client.ActiveCharacter, command, args);

                if (client.ActiveCharacter is PC)
                {
                    bool fullRoundCommand = false;

                    foreach (CommandTasker.CommandType cmd in client.ActiveCharacter.CommandsProcessed)
                    {
                        if (CommandTasker.FullRoundCommands.Contains(cmd))
                        {
                            fullRoundCommand = true;
                            break;
                        }
                    }

                    if (client.ActiveCharacter.IsImmortal ||
                        (client.ActiveCharacter.HasSpeed && client.ActiveCharacter.CommandsProcessed.Contains(CommandTasker.CommandType.Movement)) ||
                        !fullRoundCommand)
                    {
                        if (client.ActiveCharacter.CurrentCell != null)
                            client.ActiveCharacter.Map.UpdateCellVisible(client.ActiveCharacter.CurrentCell);

                    }

                    Character.ValidatePlayer(client.ActiveCharacter as PC);
                }
            }
            
            Packet np = new Packet();
            np = mapPacket(client, client.ActiveCharacter.CurrentCell);
            np.messageType = 10;
            server.SendToName(client.Name, np);
            client.ActiveCharacter.CommandWeight = 0;
            client.ActiveCharacter.CommandsProcessed.Clear();
        }
        private void ProcessMobCells(Client client)
        {
            Packet mapPak = new Packet();
            mapPak.messageType = 13;
            // |<ALIGN>,<LETTER>,<NUMBER_OF>,<NAME>

            string[] LETTER = new string[]{"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q",
											  "R","S","T","U","V","W","X","Y","Z"};
            string[] Align = Globals.ALIGNMENT_SYMBOLS;
            
            string critstring = "";
            int alignnum = 0;
            int mobnum = 0;
            int counter = 0;
            Cell[] cellArray = Cell.GetApplicableCellArray(client.ActiveCharacter.CurrentCell, 3);
            for (int x = 0; x < 49; x++)
            {
                if (cellArray[x] == null || client.ActiveCharacter.CurrentCell.visCells[x] == false)
                {
                    //critstring += "|0";
                }
                else
                {
                    if (cellArray[x].Characters.Count > 0)
                    {
                        if (cellArray[x] != client.ActiveCharacter.CurrentCell)
                        {
                            #region Not on the character's cell
                            if (cellArray[x].Characters.Count == 1)
                            {
                                #region Only 1 mob in the cell
                                foreach (Character mob in cellArray[x].Characters.Values)
                                {
                                    alignnum = (int)mob.Alignment;
                                    if (mob.BaseProfession == Character.ClassType.Thief) { alignnum = 0; }
                                    if (mob.BaseProfession == Character.ClassType.Thief && client.ActiveCharacter.BaseProfession != Character.ClassType.Knight)
                                    {
                                        if (client.ActiveCharacter.Level > Skills.GetSkillLevel(mob.magic)) { alignnum = (int)mob.Alignment; }
                                    }
                                    if (mob.IsHidden)
                                    {
                                        #region Mob is hidden
                                        if (Skills.GetSkillLevel(mob.magic) < client.ActiveCharacter.Level - 3)
                                        {
                                            if (GameSystems.Targeting.TargetAquisition.FindTargetInNextCells(mob, client.ActiveCharacter.Name) != null && Skills.GetSkillLevel(mob.magic) < client.ActiveCharacter.Level)
                                            {                                                
                                                critstring += "|"+Align[alignnum] +","+ LETTER[counter] + ",1," + mob.Name+","+cellArray[x].X+","+cellArray[x].Y;
                                                mobnum++;
                                            }
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        #region Mob is visible
                                        if (!mob.IsInvisible)
                                        {

                                            critstring += "|" + Align[alignnum] + "," + LETTER[counter] + ",1," + mob.Name + "," + cellArray[x].X + "," + cellArray[x].Y;
                                            mobnum++;
                                        }
                                        #endregion
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                #region More than 1 mob in the cell
                                ArrayList npcs = new ArrayList();
                                ArrayList nums = new ArrayList();
                                ArrayList aligns = new ArrayList();
                                int num = 1;
                                string name = "";

                                foreach (Character mob in cellArray[x].Characters.Values)
                                //for (int x = 0; x < cellArray[j].Characters.Count; x++)
                                {
                                    alignnum = (int)mob.Alignment;
                                    if (mob.BaseProfession == Character.ClassType.Thief) { alignnum = 0; }
                                    if (mob.BaseProfession == Character.ClassType.Thief && client.ActiveCharacter.BaseProfession != Character.ClassType.Knight)
                                    {
                                        if (client.ActiveCharacter.Level > Skills.GetSkillLevel(mob.magic)) { alignnum = (int)mob.Alignment; }
                                    }
                                    if (mob.IsHidden)
                                    {
                                        if (Skills.GetSkillLevel(mob.magic) < client.ActiveCharacter.Level - 3)
                                        {
                                            if (GameSystems.Targeting.TargetAquisition.FindTargetInNextCells(mob, client.ActiveCharacter.Name) != null && Skills.GetSkillLevel(mob.magic) < client.ActiveCharacter.Level)
                                            {

                                                name = mob.Name;
                                                num = 1;
                                                if (npcs.Count > 0)
                                                {
                                                    int v = 0;
                                                    bool match = false;
                                                    while (v < npcs.Count)
                                                    {
                                                        if (npcs[v].ToString() == name)
                                                        {
                                                            nums[v] = (int)nums[v] + 1;
                                                            match = true;
                                                        }
                                                        v++;
                                                    }
                                                    if (match == false)
                                                    {
                                                        npcs.Add(name);
                                                        nums.Add(num);
                                                        aligns.Add(mob.Alignment);
                                                    }
                                                }
                                                else
                                                {
                                                    npcs.Add(name);
                                                    nums.Add(num);
                                                    aligns.Add(mob.Alignment);
                                                }

                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!mob.IsInvisible)
                                        {
                                            //
                                            name = mob.Name;
                                            num = 1;
                                            if (npcs.Count > 0)
                                            {
                                                int v = 0;
                                                bool match = false;
                                                while (v < npcs.Count)
                                                {
                                                    if (npcs[v].ToString() == name)
                                                    {
                                                        nums[v] = (int)nums[v] + 1;
                                                        match = true;
                                                    }
                                                    v++;
                                                }
                                                if (match == false)
                                                {
                                                    npcs.Add(name);
                                                    nums.Add(num);
                                                    aligns.Add(mob.Alignment);
                                                }
                                            }
                                            else
                                            {
                                                npcs.Add(name);
                                                nums.Add(num);
                                                aligns.Add(mob.Alignment);
                                            }
                                        }
                                    }
                                }
                                for (int i = 0; i < npcs.Count; i++)
                                {


                                    if ((int)nums[i] > 1)
                                        critstring += "|" + Align[(int)aligns[i]] + "," + LETTER[counter] + "," + nums[i] + "," + GameSystems.Text.TextManager.Multinames((string)npcs[i]) + "," + cellArray[x].X + "," + cellArray[x].Y;
                                    else
                                        critstring += "|" + Align[(int)aligns[i]] + "," + LETTER[counter] + "," + npcs[i] + "," + cellArray[x].X + "," + cellArray[x].Y;
                                    mobnum++;
                                }
                                #endregion
                            }
                            #endregion
                        }
                        else
                        {
                            #region On the character's cell
                            int linenum = counter;
                            foreach (Character crit in cellArray[x].Characters.Values)
                            {
                                if (crit != client.ActiveCharacter)
                                {
                                    if (!crit.IsInvisible)
                                    {
                                        alignnum = (int)crit.Alignment;
                                        if (crit.BaseProfession == Character.ClassType.Thief) { alignnum = 0; }
                                        if (crit.BaseProfession == Character.ClassType.Thief && client.ActiveCharacter.BaseProfession != Character.ClassType.Knight)
                                        {
                                            if (client.ActiveCharacter.Level > Skills.GetSkillLevel(crit.magic)) { alignnum = (int)crit.Alignment; }
                                        }

                                        critstring += "|";
                                        critstring += Align[alignnum] +","+ crit.Name; // Align + name
                                        critstring += ","+crit.RightHandItemName(); // Col + righthand
                                        critstring += ","+crit.LeftHandItemName(); // Col + lefthand
                                        critstring += ","+crit.GetVisibleArmorName(); // Col + Armor
                                        critstring += "," + cellArray[x].X + "," + cellArray[x].Y;
                                        linenum++;
                                        mobnum++;
                                    }
                                }
                            }
                            #endregion
                        }
                        if (client.ActiveCharacter.CurrentCell != cellArray[x])
                        {
                            counter++;
                        }
                    }
                }
                
            }
            mapPak.message = critstring;
            server.SendToName(client.Name, mapPak);
        }
        private void ProcessLootCells(Client client)
        {
            string lootstring = "";

            Packet np = new Packet();
            np.messageType = 14;

            Cell[] cellArray = Cell.GetApplicableCellArray(client.ActiveCharacter.CurrentCell, 3);
            for (int x = 0; x < 49; x++)
            {
                if (cellArray[x] == null || client.ActiveCharacter.CurrentCell.visCells[x] == false)
                {
                    lootstring += "|0";
                }
                else
                {
                    if (cellArray[x].Items.Count > 0 && cellArray[x].LootDraw)
                    {
                        lootstring += "|1";
                    }
                    else
                    {
                        lootstring += "|0";
                    }
                }
            }



            np.message = lootstring;
            server.SendToName(client.Name, np);
        }
        private void ProcessEffectCells(Client client)
        {
            string effectstring = "";

            Packet np = new Packet();
            np.messageType = 15;

            Cell[] cellArray = Cell.GetApplicableCellArray(client.ActiveCharacter.CurrentCell, 3);
            for (int x = 0; x < 49; x++)
            {
                if (cellArray[x] == null || client.ActiveCharacter.CurrentCell.visCells[x] == false)
                {
                    effectstring += "|0";
                }
                else
                {
                    if (cellArray[x].AreaEffects.Count > 0)
                    {
                        effectstring += "|1," + cellArray[x].DisplayGraphic;
                    }
                }
            }

            np.message = effectstring;
            server.SendToName(client.Name, np);
        }

        public static void WriteLine(string clientName, string message)
        {
            Packet np = new Packet();
            np.messageType = 1;
            np.message = message;
            server.SendToName(clientName, np);
        }
        public void ChatCommands(Client ch, string command, string args)
        {
            string all = command + " " + args;

            all = all.Replace(",  ", " ");
            all = all.Trim();
            Packet np = new Packet();
            bool match = false;

            if (ch != null)
            {
                if (all.ToLower() == "/echo on") { ch.ActiveCharacter.echo = true; ch.ActiveCharacter.WriteLine("Echo Enabled."); return; }
                if (all.ToLower() == "/echo off") { ch.ActiveCharacter.echo = false; ch.ActiveCharacter.WriteLine("Echo disabled."); return; }                
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
                    if (!DragonsSpine.Commands.GameCommand.GameCommandDictionary[command].Handler.OnCommand(ch.ActiveCharacter, args))
                        SendInvalidCommand(ch);
                    return;
                }
                else
                {
                    if (!command.StartsWith("/")) command = "/" + command;

                    switch (command.ToLower())
                    {
                        #region /time
                        case "/time":
                            ch.ActiveCharacter.WriteLine("The time is now " + DateTime.Now.ToString() + ".");
                            break;
                        #endregion
                        #region /list
                        case "/characters":
                        case "/characterlist":
                        case "/list":
                            ch.ActiveCharacter.WriteLine("");
                            //string characterListing = PC.FormatCharacterList(ch.accountID, false, ch);
                            string[] protoCharacterListing = PC.FormatCharacterList(ch.Account.players, false, ch.ActiveCharacter).Replace("\n\r", "|").Split("|".ToCharArray());
                            foreach (string line in protoCharacterListing)
                            {
                                ch.ActiveCharacter.WriteLine(line);
                            }
                            
                            ch.ActiveCharacter.WriteLine("");
                            break;
                        #endregion
                        #region /lottery
                        case "/lottery":
                        case "/lotto":
                            ch.ActiveCharacter.WriteLine("");
                            foreach (Land land in World.GetFacetByIndex(0).Lands)
                            {
                                if (land.LandID != Land.ID_UNDERWORLD)
                                {
                                    ch.ActiveCharacter.WriteLine(land.Name + " Lottery");
                                    ch.ActiveCharacter.WriteLine("Amount: " + land.Lottery);
                                    //ch.WriteLine(land.Name + " lottery jackpot is currently " + land.Lottery + " gold coins.", Protocol.TextType.Listing);
                                    if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.AGM && land.LotteryParticipants.Count > 0)
                                    {
                                        Dictionary<string, int> lotto = new Dictionary<string, int>(); // Player name, # chances

                                        land.LotteryParticipants.ForEach(delegate (int id)
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

                                        ch.ActiveCharacter.WriteLine("Participants: " + names);
                                    }
                                    else if (ch.ActiveCharacter.ImpLevel == Globals.eImpLevel.USER && land.LotteryParticipants.Contains(ch.ActiveCharacter.UniqueID))
                                    {
                                        ch.ActiveCharacter.WriteLine("You are a participant in " + land.LongDesc + " lottery.");
                                    }
                                    ch.ActiveCharacter.WriteLine("");
                                }

                            }
                            break;
                        #endregion
                        #region /switch
                        case "/switch":
                            if (args == null) { ch.ActiveCharacter.WriteLine("Usage: /switch #   Use /list to see a list of your characters."); break; }
                            else
                            {
                                string[] sArgs = args.Split(" ".ToCharArray());

                                string oldName = ch.Name;
                                bool wasInvisible = ch.ActiveCharacter.IsInvisible;

                                string oldLogString = ch.ActiveCharacter.GetLogString();
                                if (PC.SelectNewCharacter(ch.ActiveCharacter, Convert.ToInt32(sArgs[0])))
                                {
                                    Utils.Log(oldLogString, Utils.LogType.Logout);
                                    Utils.Log(ch.ActiveCharacter.GetLogString(), Utils.LogType.Login);
                                    if (!wasInvisible && !ch.ActiveCharacter.IsInvisible) { ch.ActiveCharacter.SendToAllInConferenceRoom(oldName + " has switched to " + ch.Name + ".", ProtocolYuusha.TextType.Enter); } // do not send if switched player was invisible
                                    else if (wasInvisible && !ch.ActiveCharacter.IsInvisible) // but instead send a message that the new character entered (if visible)
                                    {
                                        ch.ActiveCharacter.SendToAllInConferenceRoom(Conference.GetStaffTitle(ch.ActiveCharacter as PC) + ch.Name + " has entered the room.", ProtocolYuusha.TextType.Enter);
                                    }
                                    else if (!wasInvisible && ch.ActiveCharacter.IsInvisible)
                                    {
                                        ch.ActiveCharacter.SendToAllInConferenceRoom(Conference.GetStaffTitle(ch.ActiveCharacter as PC) + ch.Name + " has left the world.", ProtocolYuusha.TextType.Exit);
                                    }
                                    ch.ActiveCharacter.WriteLine("You have switched to your character named " + ch.Name + ".", ProtocolYuusha.TextType.Status);
                                    (ch.ActiveCharacter as PC).lastOnline = DateTime.UtcNow;// set last online
                                    PC.SaveField(ch.ActiveCharacter.UniqueID, "lastOnline", (ch.ActiveCharacter as PC).lastOnline, null);

                                    
                                }
                                else
                                {
                                    ch.ActiveCharacter.WriteLine("Invalid character.");
                                }
                            }
                            break;
                        #endregion
                        #region /filter
                        case "/filter":
                            if (ch.ActiveCharacter.filterProfanity)
                            {
                                ch.ActiveCharacter.filterProfanity = false;
                                ch.ActiveCharacter.WriteLine("Your profanity filter is now OFF.");
                            }
                            else
                            {
                                ch.ActiveCharacter.filterProfanity = true;
                                ch.ActiveCharacter.WriteLine("Your profanity filter is now ON.");
                            }
                            PC.SaveField(ch.ActiveCharacter.UniqueID, "filterProfanity", ch.ActiveCharacter.filterProfanity, null);
                            break;
                        #endregion
                        #region /afk
                        case "/away":
                        case "/afk":
                            if (ch.ActiveCharacter.afk)
                            {
                                ch.ActiveCharacter.afk = false;
                                ch.ActiveCharacter.WriteLine("You are no longer AFK.");
                                if (!ch.ActiveCharacter.IsInvisible)
                                    ch.ActiveCharacter.SendToAllInConferenceRoom(ch.ActiveCharacter.Name + " is no longer AFK.", ProtocolYuusha.TextType.Status);
                            }
                            else
                            {
                                ch.ActiveCharacter.afk = true;                               
                                ch.ActiveCharacter.WriteLine("You are now AFK.", ProtocolYuusha.TextType.Status);                             
                                if (!ch.ActiveCharacter.IsInvisible)
                                    ch.ActiveCharacter.SendToAllInConferenceRoom(ch.ActiveCharacter.Name + " is now AFK.", ProtocolYuusha.TextType.Status);
                            }
                            break;
                        #endregion
                        #region /echo
                        case "/echo":
                            if (ch.ActiveCharacter.echo)
                            {
                                ch.ActiveCharacter.echo = false;
                                ch.ActiveCharacter.WriteLine("Echo disabled.");
                            }
                            else
                            {
                                ch.ActiveCharacter.echo = true;
                                ch.ActiveCharacter.WriteLine("Echo enabled.");
                            }
                            PC.SaveField(ch.ActiveCharacter.UniqueID, "echo", ch.ActiveCharacter.echo, null);
                            break;
                        #endregion
                        #region /anon
                        case "/anon":
                        case "/anonymous":
                            if (ch.ActiveCharacter.IsAnonymous)
                            {
                                ch.ActiveCharacter.IsAnonymous = false;
                                ch.ActiveCharacter.WriteLine("You are no longer anonymous.");
                            }
                            else
                            {
                                ch.ActiveCharacter.IsAnonymous = true;
                                ch.ActiveCharacter.WriteLine("You are now anonymous.");
                            }
                            PC.SaveField(ch.ActiveCharacter.UniqueID, "anonymous", ch.ActiveCharacter.IsAnonymous, null);
                            break;
                        #endregion
                        #region /enter
                        case "/ent":
                        case "/play":
                        case "/enter":
                            if (DragonsSpineMain.ServerStatus == DragonsSpineMain.ServerState.Running || ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.GM)
                            {
                                ch.ActiveCharacter.PCState = Globals.ePlayerState.PLAYING;
                                ch.ActiveCharacter.RemoveFromConf();
                                ch.ActiveCharacter.AddToWorld();
                                                                
                                if (ch.ActiveCharacter.IsAnonymous && !ch.ActiveCharacter.IsInvisible)
                                {
                                    ch.ActiveCharacter.SendToAllInConferenceRoom(Conference.GetStaffTitle(ch.ActiveCharacter as PC) + ch.ActiveCharacter.Name + " has left for the lands.", ProtocolYuusha.TextType.Exit);
                                }
                                else if (!ch.ActiveCharacter.IsAnonymous && !ch.ActiveCharacter.IsInvisible)
                                {
                                    ch.ActiveCharacter.SendToAllInConferenceRoom(Conference.GetStaffTitle(ch.ActiveCharacter as PC) + ch.ActiveCharacter.Name + " has left for " + ch.ActiveCharacter.Map.ShortDesc + ".", ProtocolYuusha.TextType.Exit);
                                }

                                // showing the map removes round with blank screen
                                np = new Packet();
                                np.message = "1|0";
                                np.messageType = 6;
                                server.SendToName(ch.Name, np);

                                np = new Packet();
                                np = mapPacket(ch, ch.ActiveCharacter.CurrentCell);
                                server.SendToName(ch.Name, np);

                            }
                            else
                            {
                                
                                 ch.ActiveCharacter.WriteLine("The game world is currently locked. Please try again later.");
                                
                            }
                            break;
                        #endregion
                        #region /exit
                        case "/quit":
                        case "/exit":
                        case "/ex":
                        case "/q":
                        case "/logout":
                            if (!ch.ActiveCharacter.IsInvisible) // send exit message if character is not invisible
                            {
                                ch.ActiveCharacter.SendToAllInConferenceRoom(Conference.GetStaffTitle(ch.ActiveCharacter as PC) + ch.ActiveCharacter.Name + " has left the world.", ProtocolYuusha.TextType.Exit);
                            }

                            Utils.Log(ch.ActiveCharacter.GetLogString(), Utils.LogType.Logout); // log the logout                            

                            ch.ActiveCharacter.RemoveFromConf();
                            ch.ActiveCharacter.RemoveFromServer();
                            ch.Shutdown();
                            break;
                        #endregion
                        #region /room
                        case "/room":
                            if (args == "" || args == null)
                            {
                                Conference.Header(ch.ActiveCharacter, false);
                                break;
                            }

                            for (int a = 0; a < Conference.rooms.Length; a++)
                            {
                                if (args.ToUpper() == Conference.rooms[a].ToUpper())
                                {
                                    if (ch.ActiveCharacter.ImpLevel >= (Globals.eImpLevel)Conference.roomsLevel[a])
                                    {
                                        match = true;
                                        if (!ch.ActiveCharacter.IsInvisible)
                                        {
                                            ch.ActiveCharacter.SendToAllInConferenceRoom(Conference.GetStaffTitle(ch.ActiveCharacter as PC) + ch.ActiveCharacter.Name + " has left the room.", ProtocolYuusha.TextType.Exit);
                                        }
                                        ch.ActiveCharacter.confRoom = a;
                                        PC.SaveField(ch.ActiveCharacter.UniqueID, "confRoom", ch.ActiveCharacter.confRoom, null);
                                        break;
                                    }
                                }
                            }
                            if (match)
                            {
                                Conference.Header(ch.ActiveCharacter, true);
                            }
                            else
                            {
                                ch.ActiveCharacter.WriteLine("Invalid Room.");
                            }
                            break;
                        #endregion
                        #region /notify
                        case "/notify":
                            if (ch.ActiveCharacter.friendNotify)
                            {
                                ch.ActiveCharacter.friendNotify = false;
                                ch.ActiveCharacter.WriteLine("You will now be notified when your friends list log on and off.");
                            }
                            else
                            {
                                ch.ActiveCharacter.friendNotify = true;
                                ch.ActiveCharacter.WriteLine("You will no longer be notified when your friends log on and off.");
                            }
                            PC.SaveField(ch.ActiveCharacter.UniqueID, "friendNotify", ch.ActiveCharacter.friendNotify, null);
                            break;
                        #endregion
                        #region /friend
                        case "/friend":
                        case "/friends":
                            if (args != null)
                            {
                                int friendsCount;
                                for (friendsCount = 0; friendsCount <= Character.MAX_FRIENDS; friendsCount++)
                                {
                                    if (ch.ActiveCharacter.friendsList[friendsCount] == 0) { break; }
                                }
                                if (friendsCount >= Character.MAX_FRIENDS) // friends list has a maximum length
                                {
                                    ch.ActiveCharacter.WriteLine("Your friends list is full.");
                                    break;
                                }
                                int friendID = DAL.DBPlayer.GetPlayerID(args); // attempt to retrieve the friend's playerID
                                if (friendID == -1) // player ID not found
                                {
                                    ch.ActiveCharacter.WriteLine("That player was not found.");
                                    break;
                                }
                                if (friendID == ch.ActiveCharacter.UniqueID) // players cannot add themselves to their friends list
                                {
                                    ch.ActiveCharacter.WriteLine("You cannot add your own name to your friend's list.");
                                    break;
                                }
                                string friend = PC.GetName(friendID);
                                for (int a = 0; a < ch.ActiveCharacter.friendsList.Length; a++) // check if player ID is already on friends list
                                {
                                    if (ch.ActiveCharacter.friendsList[a] == 0)
                                    {
                                        break;
                                    }
                                    else if (ch.ActiveCharacter.friendsList[a] == friendID) // if player ID exists, remove from friends list
                                    {
                                        match = true;
                                        ch.ActiveCharacter.friendsList[a] = 0;
                                        ch.ActiveCharacter.WriteLine(friend + " has been removed from your friends list.");
                                        PC.SaveField(ch.ActiveCharacter.UniqueID, "friendsList", Utils.ConvertIntArrayToString(ch.ActiveCharacter.friendsList), null);
                                        break;
                                    }
                                }
                                if (!match) // add friend's player ID to first available array location
                                {
                                    ch.ActiveCharacter.friendsList[friendsCount] = friendID;
                                    ch.ActiveCharacter.WriteLine(friend + " has been added to your friends list.");
                                    PC.SaveField(ch.ActiveCharacter.UniqueID, "friendsList", Utils.ConvertIntArrayToString(ch.ActiveCharacter.friendsList), null);
                                    break;
                                }

                            }
                            else // if args are null display ignore list
                            {
                                ch.ActiveCharacter.WriteLine("");
                                ch.ActiveCharacter.WriteLine("Friends List");
                                ch.ActiveCharacter.WriteLine("-----------");
                                for (int a = 0; a < ch.ActiveCharacter.friendsList.Length; a++)
                                {
                                    if (ch.ActiveCharacter.friendsList[a] == 0)
                                    {
                                        break;
                                    }
                                    ch.ActiveCharacter.WriteLine(a + 1 + ". " + PC.GetName(ch.ActiveCharacter.friendsList[a]));
                                }
                                ch.ActiveCharacter.WriteLine("");
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
                                    if (ch.ActiveCharacter.ignoreList[ignoreCount] == 0)
                                    {
                                        break;
                                    }
                                }
                                if (ignoreCount >= Character.MAX_IGNORE) // ignore list has a maximum length
                                {
                                    ch.ActiveCharacter.WriteLine("Your ignore list is full.");
                                    break;
                                }

                                int ignoreID = DAL.DBPlayer.GetPlayerID(args); // attempt to retrieve the ignored player's ID
                                if (ignoreID == -1) // player ID not found
                                {
                                    ch.ActiveCharacter.WriteLine("That player was not found.");
                                    break;
                                }
                                if (ignoreID == ch.ActiveCharacter.UniqueID) // players cannot ignore themselves
                                {
                                    ch.ActiveCharacter.WriteLine("You cannot ignore yourself.");
                                    break;
                                }
                                string ignored = PC.GetName(ignoreID);
                                //string ignored = DAL.DBPlayer.GetPlayerNameByID(ignoreID); // ignored player's name
                                if ((Globals.eImpLevel)DAL.DBPlayer.GetPlayerField(ignoreID, "ImpLevel", ch.ActiveCharacter.ImpLevel.GetType()) >= Globals.eImpLevel.GM) // cannot ignore staff member
                                {
                                    ch.ActiveCharacter.WriteLine("You cannot ignore a " + DragonsSpineMain.Instance.Settings.ServerName + " staff member.");
                                    break;
                                }
                                for (int a = 0; a < ch.ActiveCharacter.ignoreList.Length; a++) // check if player ID is already ignored
                                {
                                    if (ch.ActiveCharacter.ignoreList[a] == 0)
                                    {
                                        break;
                                    }
                                    else if (ch.ActiveCharacter.ignoreList[a] == ignoreID) // if player ID exists, remove from ignore list
                                    {
                                        match = true;
                                        ch.ActiveCharacter.ignoreList[a] = 0;
                                        ch.ActiveCharacter.WriteLine(ignored + " has been removed from your ignore list.");
                                        PC.SaveField(ch.ActiveCharacter.UniqueID, "ignoreList", Utils.ConvertIntArrayToString(ch.ActiveCharacter.ignoreList), null);
                                        break;
                                    }
                                }
                                if (!match) // add ignored player ID to first available array location
                                {
                                    ch.ActiveCharacter.ignoreList[ignoreCount] = ignoreID;
                                    ch.ActiveCharacter.WriteLine(ignored + " has been added to your ignore list.");
                                    PC.SaveField(ch.ActiveCharacter.UniqueID, "ignoreList", Utils.ConvertIntArrayToString(ch.ActiveCharacter.ignoreList), null);
                                    break;
                                }
                            }
                            else // if args are null display ignore list
                            {
                                ch.ActiveCharacter.WriteLine("");
                                ch.ActiveCharacter.WriteLine("Ignore List");
                                ch.ActiveCharacter.WriteLine("-----------");
                                for (int a = 0; a < ch.ActiveCharacter.ignoreList.Length; a++)
                                {
                                    if (ch.ActiveCharacter.ignoreList[a] == 0)
                                    {
                                        break;
                                    }
                                    ch.ActiveCharacter.WriteLine(a + 1 + ". " + PC.GetName(ch.ActiveCharacter.ignoreList[a]));
                                }
                                ch.ActiveCharacter.WriteLine("");
                            }
                            break;
                        #endregion
                        #region /page
                        case "/page":
                            // if args are null then turn paging off or on
                            if (args == null)
                            {
                                if (ch.ActiveCharacter.receivePages)
                                {
                                    ch.ActiveCharacter.receivePages = false;
                                    ch.ActiveCharacter.WriteLine("You will no longer receive pages.");
                                }
                                else
                                {
                                    ch.ActiveCharacter.receivePages = true;
                                    ch.ActiveCharacter.WriteLine("You will now receive pages.");
                                }
                                PC.SaveField(ch.ActiveCharacter.UniqueID, "receivePages", ch.ActiveCharacter.receivePages, null);
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
                                        ch.ActiveCharacter.WriteLine("That player was not found.");
                                        return;
                                    }

                                    if (receiver.receivePages)
                                    {
                                        if (Array.IndexOf(receiver.ignoreList, ch.ActiveCharacter.UniqueID) == -1)
                                        {
                                            receiver.WriteLine(Conference.GetStaffTitle(ch.ActiveCharacter as PC) + ch.ActiveCharacter.Name + " would like to speak to you in Conference " + Conference.GetUserLocation(ch.ActiveCharacter) + ".");
                                        }
                                        ch.ActiveCharacter.WriteLine(Conference.GetStaffTitle(receiver) + receiver.Name + " has been paged.");
                                    }
                                    else
                                    {
                                        ch.ActiveCharacter.WriteLine(Conference.GetStaffTitle(receiver) + receiver.Name + " has disabled their pager.");
                                    }
                                }
                                else
                                {
                                    // Check adventurers.
                                    foreach (Adventurer adv in Character.AdventurersInGameWorldList)
                                    {
                                        if (adv.Name.ToLower() == args.ToLower())
                                        {
                                            ch.ActiveCharacter.WriteLine(adv.Name + " has been paged.");
                                            return;
                                        }
                                    }

                                    ch.ActiveCharacter.WriteLine("That player was not found.");
                                }
                            }
                            catch
                            {
                                ch.ActiveCharacter.WriteLine("An error occured while processing your page request.");
                                Utils.Log("ChatCommands(" + ch.ActiveCharacter.GetLogString() + ", " + command + ", " + args + ")", Utils.LogType.CommandFailure);
                            }
                            break;
                        #endregion
                        #region /displaycombatdamage
                        case "/displaycombatdamage":
                            if (System.Configuration.ConfigurationManager.AppSettings["DisplayCombatDamage"].ToLower() == "false")
                            {
                                ch.ActiveCharacter.WriteLine("** Combat damage statistic information is currently disabled.");
                                ch.ActiveCharacter.DisplayCombatDamage = false;
                            }
                            if (ch.ActiveCharacter.DisplayCombatDamage)
                            {
                                ch.ActiveCharacter.DisplayCombatDamage = false;
                                ch.ActiveCharacter.WriteLine("You will no longer see combat damage statistics." );
                            }
                            else
                            {
                                ch.ActiveCharacter.DisplayCombatDamage = true;
                                ch.ActiveCharacter.WriteLine("You will now see combat damage statistics." );
                            }
                            break;
                        #endregion
                        #region /help
                        case "/help":
                            ch.ActiveCharacter.WriteLine("");
                            ch.ActiveCharacter.WriteLine("Conference Room Commands");
                            ch.ActiveCharacter.WriteLine("------------------------");
                            ch.ActiveCharacter.WriteLine("  /play - Enter the game.");
                            ch.ActiveCharacter.WriteLine("  /exit - Disconnect from " + DragonsSpineMain.Instance.Settings.ServerName + ".");
                            ch.ActiveCharacter.WriteLine("  /list - List your current characters.");
                            ch.ActiveCharacter.WriteLine("  /switch # - Switch to character #.");
                            ch.ActiveCharacter.WriteLine("  /scores or /topten - Get player rankings.");
                            ch.ActiveCharacter.WriteLine("     /scores me - Your current score.");
                            ch.ActiveCharacter.WriteLine("     /scores <class> <amount>");
                            ch.ActiveCharacter.WriteLine("     /scores <class>");
                            ch.ActiveCharacter.WriteLine("     /scores <player>");
                            ch.ActiveCharacter.WriteLine("	   /scores all <amount>");
                            ch.ActiveCharacter.WriteLine("  /page - Toggle paging.");
                            ch.ActiveCharacter.WriteLine("     /page <name> - Page someone in the game.");
                            ch.ActiveCharacter.WriteLine("  /tell or /t - Toggle private tells.");
                            ch.ActiveCharacter.WriteLine("     /tell <name> <message> - Send a private tell.");
                            ch.ActiveCharacter.WriteLine("  /friend - View your friends list.");
                            ch.ActiveCharacter.WriteLine("     /friend <name> - Add or remove a player from your friends list.");
                            ch.ActiveCharacter.WriteLine("  /notify - Toggle friend notification.");
                            ch.ActiveCharacter.WriteLine("  /ignore - View your ignore list.");
                            ch.ActiveCharacter.WriteLine("     /ignore <name> - Add or remove a player from your ignore list.");
                            ch.ActiveCharacter.WriteLine("  /users - Shows everyone in the game.");
                            ch.ActiveCharacter.WriteLine("  /menu - Return to the main menu.");
                            ch.ActiveCharacter.WriteLine("  /afk - Toggle A.F.K. (Away From Keyboard).");
                            ch.ActiveCharacter.WriteLine("  /anon - Toggle anonymous.");
                            ch.ActiveCharacter.WriteLine("  /filter - Toggle profanity filter.");
                            ch.ActiveCharacter.WriteLine("  /rename <new name> - Change your character's name.");
                            ch.ActiveCharacter.WriteLine("  /echo - Toggle command echo.");
                            ch.ActiveCharacter.WriteLine("  /macro - View your macros list.");
                            ch.ActiveCharacter.WriteLine("     /macro # <text> - Set # macro, where # macro is between 0 and " + Character.MAX_MACROS + ".");
                            ch.ActiveCharacter.WriteLine("  /lottery - View current lottery jackpots.");
                            ch.ActiveCharacter.WriteLine("  /commands - View a full list of available game commands.");
                            ch.ActiveCharacter.WriteLine("  /displaycombatdamage - Toggle combat damage statistics.");
                            ch.ActiveCharacter.WriteLine("  /help - This help list.");
                            ch.ActiveCharacter.WriteLine("");
                            if (ch.ActiveCharacter.ImpLevel > Globals.eImpLevel.USER)
                            {
                                ch.ActiveCharacter.WriteLine("Staff Commands");
                                ch.ActiveCharacter.WriteLine("  /stafftitle - Toggle staff title.");
                                ch.ActiveCharacter.WriteLine("");
                            }
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.GM)
                            {
                                ch.ActiveCharacter.WriteLine("GM Commands");
                                ch.ActiveCharacter.WriteLine("  /invis - Toggle invisibility.");
                                ch.ActiveCharacter.WriteLine("  /announce - Send announcement to all, anonymously.");
                                ch.ActiveCharacter.WriteLine("  /selfannounce - Send announcement to all, includes your name.");
                                ch.ActiveCharacter.WriteLine("  /immortal - Toggle immortality.");
                                ch.ActiveCharacter.WriteLine("  /ban <name> <# of days> - Ban a player (includes their account)");
                                ch.ActiveCharacter.WriteLine("  /boot <name> - Disconnects a player from the server.");
                                ch.ActiveCharacter.WriteLine("  /rename <old> <new> - Change a player's name.");
                                ch.ActiveCharacter.WriteLine("  /restock - Restock all merchant inventory items.");
                                ch.ActiveCharacter.WriteLine("  /clearstores - Clears all non original merchant inventory items.");
                                ch.ActiveCharacter.WriteLine("");
                            }
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                ch.ActiveCharacter.WriteLine("DEV Commands");
                                ch.ActiveCharacter.WriteLine("  /bootplayers - Force all players to quit.");
                                ch.ActiveCharacter.WriteLine("  /lockserver - Locks the server.");
                                ch.ActiveCharacter.WriteLine("  /unlockserver - Unlocks the server.");
                                ch.ActiveCharacter.WriteLine("  /implevel <name> <impLevel#> - Set a player's implevel.");
                                ch.ActiveCharacter.WriteLine("  /listf - Lists the Player table columns in the database.");
                                ch.ActiveCharacter.WriteLine("  /getf <name> <field name> - Get a player's field value.");
                                ch.ActiveCharacter.WriteLine("  /setf <name> <field name> <value> <notify> - Set a player's field value.");
                                ch.ActiveCharacter.WriteLine("  /processemptyworld <on|off> - No argument displays status of this attribute.");
                                ch.ActiveCharacter.WriteLine("  /purgeaccount <account> - Purge a players account.");
                                //ch.ActiveCharacter.WriteLine("  /searchnpcloot <itemID> - Search for an item on an NPC currently in the game.", Protocol.TextType.Help);
                                ch.ActiveCharacter.WriteLine("  /getskill <name> <skill> - Get a PC's skill level.");
                                ch.ActiveCharacter.WriteLine("  /setskill <name> <skill> <skill level> - Set a PC's skill level.");
                                ch.ActiveCharacter.WriteLine("  /restartserver - Forces a hard shutdown, with no PC saves, and restarts the DragonsSpine.exe process.");
                                ch.ActiveCharacter.WriteLine("  /deleteplayer | /dplayer <name> - Delete a player from the database.");
                                ch.ActiveCharacter.WriteLine("");
                            }
                            ch.ActiveCharacter.WriteLine("");
                            break;
                        #endregion
                        #region /menu
                        case "/menu":
                            ch.ActiveCharacter.RemoveFromConf();

                            if (!ch.ActiveCharacter.IsInvisible)
                            {
                                ch.ActiveCharacter.SendToAllInConferenceRoom(Conference.GetStaffTitle(ch.ActiveCharacter as PC) + ch.ActiveCharacter.Name + " has exited to the menu.", ProtocolYuusha.TextType.Exit);
                            }
                            ch.ActiveCharacter.PCState = Globals.ePlayerState.MAINMENU;
                            np = new Packet();
                            np.message = "0|0";
                            np.messageType = 6;
                            server.SendToName(ch.Name, np);
                            break;
                        #endregion
                        // Staff Commands
                        #region /stafftitle
                        case "/stafftitle":
                            if (ch.ActiveCharacter.ImpLevel > Globals.eImpLevel.USER)
                            {
                                if (ch.ActiveCharacter.showStaffTitle)
                                {
                                    ch.ActiveCharacter.showStaffTitle = false;
                                    ch.ActiveCharacter.WriteLine("Your staff title is now OFF.");

                                }
                                else
                                {
                                    ch.ActiveCharacter.showStaffTitle = true;
                                    ch.ActiveCharacter.WriteLine("Your staff title is now ON.");
                                }
                                //Protocol.UpdateUserLists();
                                PC.SaveField(ch.ActiveCharacter.UniqueID, "showStaffTitle", ch.ActiveCharacter.showStaffTitle, null);
                            }
                            else { Conference.SendInvalidCommand(ch.ActiveCharacter); }
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
                                        chr.WriteLine("SYSTEM: " + args);
                                    }
                                }
                                Utils.Log("LOCAL SYSTEM ANNOUNCEMENT: " + args, Utils.LogType.Announcement);
                                return;
                            }
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.GM)
                            {
                                Utils.Log(Conference.GetStaffTitle(ch.ActiveCharacter as PC) + ch.ActiveCharacter.GetLogString() + " announced '" + args + "'", Utils.LogType.Announcement);
                                ch.ActiveCharacter.SendToAll("SYSTEM: " + args); // send to all in game
                                ch.ActiveCharacter.SendToAllInConferenceRoom("SYSTEM: " + args, ProtocolYuusha.TextType.System); // send to all in chat room....
                            }
                            else { Conference.SendInvalidCommand(ch.ActiveCharacter); }
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
                                        chr.WriteLine("SYSTEM: " + args);
                                    }
                                }
                                Utils.Log("LOCAL SYSTEM ANNOUNCEMENT: " + args, Utils.LogType.Announcement);
                                return;
                            }
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.GM)
                            {
                                Utils.Log(Conference.GetStaffTitle(ch.ActiveCharacter as PC) + ch.ActiveCharacter.GetLogString() + " announced '" + args + "'", Utils.LogType.Announcement);
                                ch.ActiveCharacter.SendToAll("SYSTEM: " + ch.ActiveCharacter.Name + ": " + args); // send to all in game
                                ch.ActiveCharacter.SendToAllInConferenceRoom("SYSTEM: " + ch.ActiveCharacter.Name + ": " + args, ProtocolYuusha.TextType.System); // send to all in chat room....
                            }
                            else { Conference.SendInvalidCommand(ch.ActiveCharacter); }
                            break;
                        #endregion
                        #region /restock
                        case "/restock":
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.GM)
                            {
                                StoreItem.RestockStores();
                                ch.ActiveCharacter.WriteLine("You have restocked all stores with their original stock items.");
                            }
                            else
                            {
                                SendInvalidCommand(ch);
                            }
                            break;
                        #endregion
                        #region /clearstores
                        case "/clearstores":
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.GM)
                            {
                                StoreItem.ClearStores();
                                ch.ActiveCharacter.WriteLine("You have cleared all stores of their non original stock items.");
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
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                DragonsSpineMain.ServerStatus = DragonsSpineMain.ServerState.ShuttingDown;
                            }
                            else SendInvalidCommand(ch);
                            break;
                        #endregion
                        #region /processemptyworld <on|off>
                        case "/processemptyworld":
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                if (args == "on")
                                {
                                    DragonsSpineMain.Instance.Settings.ProcessEmptyWorld = true;
                                    ch.ActiveCharacter.WriteLine("Empty world processing enabled.");
                                }
                                else if (args == "off")
                                {
                                    DragonsSpineMain.Instance.Settings.ProcessEmptyWorld = false;
                                    ch.ActiveCharacter.WriteLine("Empty world processing disabled.");
                                }
                                else
                                {
                                    ch.ActiveCharacter.WriteLine("Empty world processing is currently set to " + DragonsSpineMain.Instance.Settings.ProcessEmptyWorld.ToString());
                                }
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /purgeaccount
                        case "/purgeaccount":
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                Account account = DAL.DBAccount.GetAccountByName(args);
                                if (account != null)
                                {
                                    ch.ActiveCharacter.WriteLine("Purging account " + account.accountName + "...");
                                    foreach (string chrName in account.players)
                                    {
                                        if (chrName.Length > 0)
                                        {
                                            if (DAL.DBPlayer.DeletePlayerFromDatabase(PC.GetPlayerID(chrName)))
                                            {
                                                ch.ActiveCharacter.WriteLine("Deleted player " + chrName + ".");
                                            }
                                            else
                                            {
                                                ch.ActiveCharacter.WriteLine("Failed to delete player " + chrName + ".");
                                            }
                                        }
                                    }
                                    DAL.DBAccount.DeleteAccount(account.accountID);
                                    ch.ActiveCharacter.WriteLine("Purge completed of account " + account.accountName + ".");
                                }
                                else
                                {
                                    ch.ActiveCharacter.WriteLine("Failed to find account with the name " + args + ".");
                                }
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /deleteplayer or /dplayer
                        case "/dplayer":
                        case "/deleteplayer":
                            {
                                if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.DEV)
                                {
                                    if (args == null || args.Length < GameSystems.Text.NameGenerator.NAME_MIN_LENGTH)
                                    {
                                        ch.ActiveCharacter.WriteLine("Invalid player name.");
                                        return;
                                    }

                                    PC pc = PC.GetOnline(args);

                                    if (pc != null)
                                    {
                                        ch.ActiveCharacter.WriteLine("That player cannot be deleted because they are online.");
                                        return;
                                    }
                                    else pc = PC.GetPC(PC.GetPlayerID(args));

                                    if (pc == null)
                                    {
                                        ch.ActiveCharacter.WriteLine("That player does not exist.");
                                        return;
                                    }

                                    if (DAL.DBPlayer.DeletePlayerFromDatabase(pc.UniqueID))
                                    {
                                        ch.ActiveCharacter.WriteLine("The player " + pc.Name + " has been deleted from the database.");
                                    }
                                    else
                                    {
                                        ch.ActiveCharacter.WriteLine("Failed to delete player " + pc.Name + " from the database.");
                                    }
                                }
                                else SendInvalidCommand(ch);
                            }
                            break;
                        #endregion
                        #region /getskill
                        case "/getskill":
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                // args = <name> <skill>
                                string[] getSkillArgs = args.Split(" ".ToCharArray());

                                int getSkillID = PC.GetPlayerID(getSkillArgs[0]);

                                if (getSkillID <= 0)
                                {
                                    ch.ActiveCharacter.WriteLine("The player " + getSkillArgs[0] + " does not exist.");
                                    return;
                                }

                                PC getSkillPC = PC.GetOnline(getSkillID);

                                // check if PC is legit
                                if (getSkillPC == null)
                                {
                                    getSkillPC = PC.GetPC(getSkillID);

                                    if (getSkillPC == null)
                                    {
                                        ch.ActiveCharacter.WriteLine("Player not found.");
                                        return;
                                    }
                                }

                                Globals.eSkillType getSkillType = (Globals.eSkillType)Enum.Parse(typeof(Globals.eSkillType), getSkillArgs[1], true);

                                ch.ActiveCharacter.WriteLine(getSkillPC.Name + "'s " + getSkillArgs[1] + " skill is level " +
                                    Skills.GetSkillLevel(getSkillPC, getSkillType) +
                                    " (" + Skills.GetSkillTitle(getSkillType, getSkillPC.BaseProfession, getSkillPC.GetSkillExperience(getSkillType), getSkillPC.gender) + ").");
                            }
                            else SendInvalidCommand(ch);
                            break;
                        #endregion
                        #region /setskill
                        case "/setskill":
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                // args = <name> <skill> <skill level between 0 and 19>
                                string[] setSkillArgs = args.Split(" ".ToCharArray());

                                int setSkillID = PC.GetPlayerID(setSkillArgs[0]);

                                if (setSkillID <= 0)
                                {
                                    ch.ActiveCharacter.WriteLine("The player " + setSkillArgs[0] + " does not exist.");
                                    return;
                                }

                                PC setSkillPC = PC.GetOnline(setSkillID);

                                // check if PC is legit
                                if (setSkillPC == null)
                                {
                                    setSkillPC = PC.GetPC(setSkillID);

                                    if (setSkillPC == null)
                                    {
                                        ch.ActiveCharacter.WriteLine("Player not found.");
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
                                    ch.ActiveCharacter.WriteLine("Invalid arguments. Command usage: /setskill <name of player> <skill type> <skill level>.");
                                }

                                setSkillPC.Save();

                                ch.ActiveCharacter.WriteLine(setSkillPC.Name + "'s " + setSkillArgs[1] + " skill has been set to level " + setSkillArgs[2] +
                                    " (" + Skills.GetSkillTitle(setSkillType, setSkillPC.BaseProfession, setSkillPC.GetSkillExperience(setSkillType), setSkillPC.gender) + ").");

                            }
                            else SendInvalidCommand(ch);

                            break;
                        #endregion
                        #region /searchnpcloot (search all NPCs for an itemID)
                        case "/searchnpcloot":
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                Item loot;

                                try
                                {
                                    loot = Item.CopyItemFromDictionary(Convert.ToInt32(args));
                                }
                                catch (FormatException)
                                {
                                    ch.ActiveCharacter.WriteLine("Please use an item ID number to search for NPC loot.");
                                    break;
                                }

                                if (loot == null)
                                {
                                    ch.ActiveCharacter.WriteLine("No item with ID '" + args + "' exists.");
                                    break;
                                }

                                foreach (NPC npc in Character.NPCInGameWorld)
                                {
                                    if (npc.RightHand != null && npc.RightHand.itemID == loot.itemID)
                                        ch.ActiveCharacter.WriteLine(npc.GetLogString() + " >> RIGHT HAND");

                                    if (npc.LeftHand != null && npc.LeftHand.itemID == loot.itemID)
                                        ch.ActiveCharacter.WriteLine(npc.GetLogString() + " >> LEFT HAND");

                                    foreach (Item armor in npc.wearing)
                                    {
                                        if (armor.itemID == loot.itemID)
                                            ch.ActiveCharacter.WriteLine(npc.GetLogString() + " >> WEARING");
                                    }

                                    foreach (Item sackItem in npc.sackList)
                                    {
                                        if (sackItem.itemID == loot.itemID)
                                            ch.ActiveCharacter.WriteLine(npc.GetLogString() + " >> SACK");
                                    }

                                    foreach (Item beltItem in npc.beltList)
                                    {
                                        if (beltItem.itemID == loot.itemID)
                                            ch.ActiveCharacter.WriteLine(npc.GetLogString() + " >> BELT");
                                    }

                                    if (npc.lairCritter)
                                    {
                                        foreach (Cell cell in npc.lairCellsList)
                                        {
                                            if (Item.IsItemOnGround(loot.itemID, cell))
                                                ch.ActiveCharacter.WriteLine(npc.GetLogString() + " >> LAIR");
                                        }
                                    }
                                }
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /getstats (display CPU usage)
                        case "/getstats":
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                ch.ActiveCharacter.WriteLine("CPU Usage: " + Utils.GetCpuUsage() + "%");
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /listf (list Player table columns in the database)
                        case "/listf": // short for "list fields"
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                try
                                {
                                    string[] columns = DAL.DBPlayer.GetPlayerTableColumnNames(ch.ActiveCharacter.UniqueID);
                                    //string output = "";

                                    ch.ActiveCharacter.WriteLine("");
                                    ch.ActiveCharacter.WriteLine("Player Table Columns");
                                    ch.ActiveCharacter.WriteLine("--------------------");

                                    string col1 = "";
                                    string col2 = "";

                                    for (int a = 0; a < columns.Length; a++)
                                    {
                                        if (a % 2 == 0)
                                        {
                                            col1 = columns[a].PadRight(22);
                                            if (a == columns.Length - 1)
                                            {
                                                if (ch.ActiveCharacter.usingClient)
                                                    ch.ActiveCharacter.WriteLine(col1);
                                                else ch.ActiveCharacter.WriteLine(col1);
                                            }
                                        }
                                        else
                                        {
                                            col2 = columns[a];
                                            if (!ch.ActiveCharacter.usingClient)
                                                ch.ActiveCharacter.WriteLine(col1 + " " + col2);
                                            else ch.ActiveCharacter.WriteLine(col1 + " " + col2);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Utils.Log("ERROR: ChatRoom.ChatCommands(" + ch.ActiveCharacter.GetLogString() + ", " + command + ", " + args + ")", Utils.LogType.CommandFailure);
                                    Utils.LogException(e);
                                }
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /getf
                        case "/getf": // short for "get field"
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                // /getf <name> <field name>
                                if (args == null || args.ToLower() == "help" || args == "?")
                                {
                                    ch.ActiveCharacter.WriteLine("Format: /getf <name> <field name>");
                                    ch.ActiveCharacter.WriteLine("<name> is the full name of the player (not case sensitive)");
                                    ch.ActiveCharacter.WriteLine("<field name> is the number of the field from the /listf output");
                                    break;
                                }
                                try
                                {
                                    string[] getfArgs = args.Split(" ".ToCharArray());
                                    int id = PC.GetPlayerID(getfArgs[0]);
                                    if (id == -1)
                                    {
                                        ch.ActiveCharacter.WriteLine("Player '" + getfArgs[0] + "' was not found in the database.");
                                        break;
                                    }
                                    //int num = Convert.ToInt32(getfArgs[1]);
                                    string fieldValue = fieldValue = Convert.ToString(DAL.DBPlayer.GetPlayerField(id, getfArgs[1], null));
                                    //string[] columns = DAL.DBPlayer.getPlayerTableColumnNames(id);
                                    //fieldValue = Convert.ToString(DAL.DBPlayer.getPlayerField(id, getfArgs[1], null)); // subtract one because the /listpcol display added 1
                                    ch.ActiveCharacter.WriteLine(PC.GetName(id) + "'s \"" + getfArgs[1] + "\" is set to \"" + fieldValue + "\".");

                                }
                                catch (Exception e)
                                {
                                    Utils.Log("ChatRoom.ChatCommands(" + ch.ActiveCharacter.GetLogString() + ", " + command + ", " + args + ")", Utils.LogType.CommandFailure);
                                    Utils.LogException(e);
                                }
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /setf
                        case "/setf":
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                // /setf <name> <field name> <value> <notify: true | false>
                                if (args == null || args == "" || args.ToLower() == "help" || args == "?")
                                {
                                    ch.ActiveCharacter.WriteLine("Format: /setf <name> <field name> <value> <notify: true | false>");
                                    ch.ActiveCharacter.WriteLine("<name> is the full name of the player (not case sensitive)");
                                    ch.ActiveCharacter.WriteLine("<field name> is the field from the /listf output");
                                    ch.ActiveCharacter.WriteLine("<value> to set the field to");
                                    ch.ActiveCharacter.WriteLine("<notify> true will notify the user that the value has been changed");
                                    ch.ActiveCharacter.WriteLine("**Please note that booleans must be 'true or 'false', not 1 or 0.");
                                    break;
                                }

                                try
                                {
                                    // split the arguments
                                    // /setf <name> <field name> <value> [true | false]
                                    string[] setfArgs = args.Split(" ".ToCharArray());

                                    if (setfArgs.Length < 3)
                                    {
                                        ch.ActiveCharacter.WriteLine("Invalid arguments. Format: /setf <name> <field name> <value> <notify: true | false>");
                                        break;
                                    }

                                    #region Determine if player exists
                                    int id = PC.GetPlayerID(setfArgs[0]);

                                    if (id == -1)
                                    {
                                        ch.ActiveCharacter.WriteLine("Player '" + setfArgs[0] + "' was not found in the database.");
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
                                        ch.ActiveCharacter.WriteLine(pc.Name + "'s \"" + fieldName + "\" has been saved in the database.");
                                    }
                                    else
                                    {
                                        ch.ActiveCharacter.WriteLine(pc.Name + "'s \"" + fieldName + "\" was NOT saved in the database.");
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
                                                ch.ActiveCharacter.WriteLine("Found property: " + prop.Name);
                                                break;
                                            }
                                        }                                       
                                    }

                                    ch.ActiveCharacter.WriteLine(pc.Name + "'s \"" + fieldName + "\" has been changed from \"" + fieldValue_old +
                                        "\" to \"" + fieldValue_new + "\".");

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
                                                pcOnline.WriteLine("Your " + fieldName + " has been changed from \"" + fieldValue_old + "\" to \"" + fieldValue_new + "\".");
                                            }

                                            ch.ActiveCharacter.WriteLine(pcOnline.Name + " has been notified of the change.");
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Utils.Log("ChatRoom.ChatCommands(" + ch.ActiveCharacter.GetLogString() + ", " + command + ", " + args + ")", Utils.LogType.CommandFailure);
                                    Utils.LogException(e);
                                }
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /implevel
                        case "/implevel":
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                if (args == null || args.ToLower() == "help" || args == "?")
                                {
                                    ch.ActiveCharacter.WriteLine("Format: /implevel <name> <impLevel#>");
                                    ch.ActiveCharacter.WriteLine("<name> is the full name of the player (not case sensitive)");
                                    ch.ActiveCharacter.WriteLine("<impLevel#> is the new impLevel, ranging from 0 to " + Enum.GetValues(ch.ActiveCharacter.ImpLevel.GetType()).Length + ".");
                                    break;
                                }
                                try
                                {
                                    String[] impArgs = args.Split(" ".ToCharArray());
                                    int id = PC.GetPlayerID(impArgs[0]);
                                    if (id == -1)
                                    {
                                        ch.ActiveCharacter.WriteLine("Player '" + impArgs[0] + "' was not found.");
                                        break;
                                    }
                                    PC online = PC.GetOnline(id);
                                    Globals.eImpLevel oldImpLevel = (Globals.eImpLevel)PC.GetField(id, "impLevel", (int)ch.ActiveCharacter.ImpLevel, null);
                                    Globals.eImpLevel newImpLevel = (Globals.eImpLevel)Convert.ToInt32(impArgs[1]);
                                    if (online != null) // if character is online, set their new implevel and alert them of the change
                                    {
                                        online.ImpLevel = newImpLevel;
                                        //Protocol.UpdateUserLists(); // send new user lists to protocol users
                                        online.WriteLine("Your impLevel has been changed from " + oldImpLevel + " to " + newImpLevel + ".");
                                    }
                                    else
                                    {
                                        ch.ActiveCharacter.WriteLine(PC.GetName(id) + "'s impLevel has been changed from " + oldImpLevel + " to " + newImpLevel + ".");
                                    }
                                    PC.SaveField(id, "impLevel", (int)online.ImpLevel, null); // save new implevel to Player table
                                    Utils.Log(online.GetLogString() + " impLevel was changed from " + oldImpLevel + " to " + newImpLevel + " by " + ch.ActiveCharacter.GetLogString() + ".", Utils.LogType.Unknown);
                                }
                                catch (Exception e)
                                {
                                    Utils.Log("ChatRoom.ChatCommands(" + ch.ActiveCharacter.GetLogString() + ", " + command + ", " + args + ")", Utils.LogType.CommandFailure);
                                    Utils.LogException(e);
                                    Conference.ChatCommands(ch.ActiveCharacter, "/implevel", null);
                                }
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /lockserver
                        case "/lockserver":
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                DragonsSpineMain.ServerStatus = DragonsSpineMain.ServerState.Locked;
                                ch.ActiveCharacter.WriteLine("Game world has been locked.");
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /unlockserver
                        case "/unlockserver":
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                DragonsSpineMain.ServerStatus = DragonsSpineMain.ServerState.Running;
                                ch.ActiveCharacter.WriteLine("Game world has been unlocked.");
                            }
                            else { SendInvalidCommand(ch); }
                            break;
                        #endregion
                        #region /bootplayers
                        case "/bootplayers":
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                DragonsSpineMain.ServerStatus = DragonsSpineMain.ServerState.Locked;
                                foreach (PC chr in new List<PC>(Character.PCInGameWorld))
                                {
                                    CommandTasker.ParseCommand(chr, "forcequit", "");
                                }
                                ch.ActiveCharacter.WriteLine("Game World locked, players booted.");
                            }
                            else SendInvalidCommand(ch);
                            break;
                        #endregion
                        #region /restartserver
                        case "/restartserver":
                            if (ch.ActiveCharacter.ImpLevel >= Globals.eImpLevel.DEV)
                            {
                                DragonsSpineMain.Instance.RestartServerWithoutSave(ch.ActiveCharacter.GetLogString() + " performed a hard server restart from the conference room.");
                            }
                            else SendInvalidCommand(ch);
                            break;
                        #endregion                        
                        default:
                            Conference.SendInvalidCommand(ch.ActiveCharacter);
                            break;
                    }
                }
            }
            else
            {
                if (ch.ActiveCharacter.IsInvisible)
                {
                    ch.ActiveCharacter.WriteLine("Please become visible if you wish to chat in the conference room.");
                }
                else
                {
                    ch.ActiveCharacter.SendToAllInConferenceRoom(Conference.GetStaffTitle(ch.ActiveCharacter as PC) + ch.ActiveCharacter.Name + ": " + all, ProtocolYuusha.TextType.PlayerChat);
                }
            }
            return;
        }
        public static void SendInvalidCommand(Client client)
        {
            string invalid = "Invalid Command. Type /help for a list of commands.";
            WriteLine(client.Name, invalid);
        }
        
        
        public Client GetClientByName(string name)
        {
            return server.GetClient(name);
        }

        #region Convert Cell DisplayGraphic to single character
        private string convertMapString(Cell cell)
        {
            switch (cell.DisplayGraphic)
            {
                case Cell.GRAPHIC_WALL:
                    return "a";
                case Cell.GRAPHIC_EMPTY:
                    return "b";
                case Cell.GRAPHIC_FOREST_LEFT:
                    return "c";
                case Cell.GRAPHIC_FOREST_RIGHT:
                    return "d";
                case Cell.GRAPHIC_FOREST_FULL:
                    return "e";
                case Cell.GRAPHIC_ACID_STORM:
                    return "f";
                case Cell.GRAPHIC_AIR:
                    return "g";
                case Cell.GRAPHIC_ALTAR:
                    return "h";
                case Cell.GRAPHIC_ALTAR_PLACEABLE:
                    return "i";
                case Cell.GRAPHIC_BARREN_FULL:
                    return "j";
                case Cell.GRAPHIC_BARREN_LEFT:
                    return "k";
                case Cell.GRAPHIC_BARREN_RIGHT:
                    return "l";
                case Cell.GRAPHIC_BOXING_RING:
                    return "m";
                case Cell.GRAPHIC_BRIDGE:
                    return "n";
                case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:
                    return "o";
                case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:
                    return "p";
                case Cell.GRAPHIC_COUNTER:
                    return "q";
                case Cell.GRAPHIC_COUNTER_PLACEABLE:
                    return "r";
                case Cell.GRAPHIC_DARKNESS:
                    return "s";
                case Cell.GRAPHIC_DOWNSTAIRS:
                    return "t";
                case Cell.GRAPHIC_FIRE:
                    return "u";
                case Cell.GRAPHIC_FOREST_BURNT_FULL:
                    return "v";
                case Cell.GRAPHIC_FOREST_BURNT_LEFT:
                    return "w";
                case Cell.GRAPHIC_FOREST_BURNT_RIGHT:
                    return "x";
                case Cell.GRAPHIC_FOREST_FROSTY_FULL:
                    return "y";
                case Cell.GRAPHIC_FOREST_FROSTY_LEFT:
                    return "z";
                case Cell.GRAPHIC_FOREST_FROSTY_RIGHT:
                    return "1";
                case Cell.GRAPHIC_FOREST_IMPASSABLE:
                    return "2";
                case Cell.GRAPHIC_GRASS_LIGHT:
                    return "3";
                case Cell.GRAPHIC_GRASS_THICK:
                    return "4";
                case Cell.GRAPHIC_GRATE:
                    return "5";
                case Cell.GRAPHIC_ICE:
                    return "6";
                case Cell.GRAPHIC_ICE_STORM:
                    return "7";
                case Cell.GRAPHIC_ICE_WALL:
                    return "8";
                case Cell.GRAPHIC_LIGHTNING_STORM:
                    return "9";
                case Cell.GRAPHIC_LOCKED_DOOR_HORIZONTAL:
                    return "0";
                case Cell.GRAPHIC_LOCKED_DOOR_VERTICAL:
                    return "!";
                case Cell.GRAPHIC_LOCUST_SWARM:
                    return "@";
                case Cell.GRAPHIC_LOOT_SYMBOL:
                    return "#";
                case Cell.GRAPHIC_MOUNTAIN:
                    return "$";
                case Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL:
                    return "%";
                case Cell.GRAPHIC_OPEN_DOOR_VERTICAL:
                    return "^";
                case Cell.GRAPHIC_POISON_CLOUD:
                    return "&";
                case Cell.GRAPHIC_REEF:
                    return "*";
                case Cell.GRAPHIC_RUINS_LEFT:
                    return "(";
                case Cell.GRAPHIC_RUINS_RIGHT:
                    return ")";
                case Cell.GRAPHIC_SAND:
                    return "A";
                case Cell.GRAPHIC_SECRET_DOOR:
                    return "B";
                case Cell.GRAPHIC_SECRET_MOUNTAIN:
                    return "C";
                case Cell.GRAPHIC_TRASHCAN:
                    return "D";
                case Cell.GRAPHIC_UP_AND_DOWNSTAIRS:
                    return "E";
                case Cell.GRAPHIC_UPSTAIRS:
                    return "F";
                case Cell.GRAPHIC_WATER:
                    return "G";
                case Cell.GRAPHIC_WEB:
                    return "H";
                case Cell.GRAPHIC_WHIRLWIND:
                    return "I";
                default:
                    return "?";
            }
        }
        #endregion

    }
}
