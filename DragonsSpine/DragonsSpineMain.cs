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
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Configuration;
using DragonsSpine.Config;
using Wintellect.PowerCollections;
using DragonsSpine.GameWorld;
using System.Threading;

namespace DragonsSpine
{
    public class DragonsSpineMain
    {
        public enum ServerState : int
        {
            Starting, Running, Locked, ShuttingDown, Restarting
        };

        #region Private Static Data
        private static int _lastProcessResponseTimerEvent = 0; // records the game round when this timer activates
        private static System.Timers.Timer _roundTimer;
        private static System.Timers.Timer _saveTimer;
        private static System.Timers.Timer _janitorTimer;
        private static System.Timers.Timer _chronoTimer;
        private static System.Timers.Timer _lunarTimer;
        private static System.Timers.Timer _inactivityTimer;
        private static System.Timers.Timer _displayTimer;
        private static System.Timers.Timer _responseTimer; // for detecting if the Dragon's Spine process is still responding
        #endregion

        #region Private Data
        //private GameServerSettings m_settings;
        private IO m_io;
        #endregion

        #region Public Static Properties
        /// <summary>
        /// Holds the current server status.
        /// </summary>
        public static ServerState ServerStatus { get; set; }

        /// <summary>
        /// Holds the only instance of the game server.
        /// </summary>
        public static DragonsSpineMain Instance { get; set; }

        /// <summary>
        /// Holds the current game round.
        /// </summary>
        public static int GameRound { get; set; }

        /// <summary>
        /// Holds the master round interval. Usually 5000, or 5 seconds.
        /// </summary>
        public static double MasterRoundInterval { get; set; }

        /// <summary>
        /// Dictionary of objects by initiative.
        /// </summary>
        public static MultiDictionary<int, object> InitiativeList = new MultiDictionary<int, object>(true);
        #endregion

        #region Public Instance Data
        /// <summary>
        ///  Holds the server settings.
        /// </summary>
        public GameServerSettings Settings { get; set; }
        #endregion

        //public static ThreadStart kesSpawner;
        //public static Thread KesSpawnThread;

        public static void KesSpawnerTask()
        {
            #region Spawn NPCs
            Utils.Log("Spawning NPCs and Creating LootTables.", Utils.LogType.SystemGo);
            NPC.DoSpawn();
            #endregion

            Utils.Log("Loot Tables Count: " + Autonomy.ItemBuilding.LootManager.NPCLootTablesCount, Utils.LogType.SystemGo);
            Utils.Log("Spawned Adventurers: " + Adventurer.AdventurerCount, Utils.LogType.SystemGo);

            _janitorTimer = new System.Timers.Timer();
            _janitorTimer.Elapsed += new ElapsedEventHandler(JanitorEvent);  // janitor timer
            _janitorTimer.Interval = MasterRoundInterval * 3;
            _janitorTimer.Start();
            Utils.Log("Janitor round timer started.", Utils.LogType.SystemGo);
        }

        public static int Main(string[] args)
        {
            SetInstance(new DragonsSpineMain());

            MasterRoundInterval = Convert.ToDouble(ConfigurationManager.AppSettings["MasterRoundInterval"]);

            Instance.Settings = GameServerSettings.Load();

            #region Initialize Console
            WinConsole.Initialize();
            Console.SetError(new ConsoleWriter(Console.Error, ConsoleColor.Red | ConsoleColor.Intensified | ConsoleColor.WhiteBG, ConsoleFlashMode.FlashUntilResponse, true));
            WinConsole.Flash(true);
            #endregion

            ServerStatus = ServerState.Starting;

            Utils.Log(Instance.Settings.ServerName + " " + Instance.Settings.ServerVersion, Utils.LogType.SystemGo);

            #region Load Commands
            if (!Commands.GameCommand.LoadGameCommands())
            {
                Utils.Log("GameCommand.LoadCommands() failed.", Utils.LogType.SystemFatalError);
                return -1;
            }
            else
                Utils.Log("Loaded " + DragonsSpine.Commands.GameCommand.GameCommandDictionary.Count + " commands and " +
                DragonsSpine.Commands.GameCommand.GameCommandAliases.Count + " aliases.", Utils.LogType.SystemGo);
            #endregion

            #region Load Talents
            if (!DragonsSpine.Talents.GameTalent.LoadTalents())
            {
                Utils.Log("GameTalent.LoadTalents() failed.", Utils.LogType.SystemFatalError);
                return -1;
            }
            else
                Utils.Log("Loaded " + DragonsSpine.Talents.GameTalent.GameTalentDictionary.Count + " talents.", Utils.LogType.SystemGo);
            #endregion

            #region Load World
            if (!World.LoadWorld()) // load World
            {
                Utils.Log("World.loadWorld() failed.", Utils.LogType.SystemFatalError);
                return -1;
            }
            #endregion

            #region Load Banned IP List
            if (!World.LoadBannedIPList()) // load banned IP list - not a fatal error on fail
            {
                Utils.Log("World.LoadBannedIPList() failed.", Utils.LogType.SystemFailure);
            } 
            #endregion
            
            #region Load Items
            if (!DAL.DBItem.LoadItems())
            {
                Utils.Log("Item.loadItems() failed.", Utils.LogType.SystemFatalError);
                // Notify developers there was a fatal error via the new DBAdmin class, and a GameMail.
                //return -1;
            } 
            #endregion

            #region Load Armor Sets
            if (!Autonomy.ItemBuilding.ArmorSets.ArmorSet.LoadArmorSets())
            {
                Utils.Log("ArmorSet.LoadArmorSets() failed.", Utils.LogType.SystemFatalError);
                return -1;
            }
            else Utils.Log("Loaded " + Autonomy.ItemBuilding.ArmorSets.ArmorSet.ArmorSetDictionary.Count + " armor sets.", Utils.LogType.SystemGo); 
            #endregion

            #region Build Loot Dictionary
            if (Autonomy.ItemBuilding.LootManager.BuildLootDictionaries())
            {
                Utils.Log("LootManager has " + Autonomy.ItemBuilding.LootManager.GetLootCount(false) + " items for loot.", Utils.LogType.SystemGo);
                Utils.Log("LootManager has " + Autonomy.ItemBuilding.LootManager.GetLootCount(true) + " specified item entries for loot.", Utils.LogType.SystemGo);
            }
            else
            {
                Utils.Log("LootManager.BuildLootDictionary() failed.", Utils.LogType.SystemFailure);
                return -1;
            } 
            #endregion

            #region Load Quests
            if (!GameQuest.LoadQuests())
            {
                Utils.Log("Quest.LoadQuests() failed.", Utils.LogType.SystemFatalError);
                return -1;
            }
            else Utils.Log("Loaded " + GameQuest.QuestDictionary.Count + " quests.", Utils.LogType.SystemGo);
            #endregion

            #region Load Spawn Zones
            if (!SpawnZone.LoadSpawnZones())
            {
                Utils.Log("SpawnZoneLink.LoadSpawnZones() failed.", Utils.LogType.SystemFatalError);
                return -1;
            } 
            #endregion

            #region Load Newbies
            if (!CharGen.LoadNewbies())
            {
                Utils.Log("CharGen.LoadNewbies failed.", Utils.LogType.SystemFatalError);
                return -1;
            } 
            #endregion

            #region Load Spells
            if (!Spells.GameSpell.LoadGameSpells())
            {
                Utils.Log("DragonsSpine.Spells.LoadGameSpells() failed.", Utils.LogType.SystemFatalError);
                return -1;
            }
            else Utils.Log("Loaded " + Spells.GameSpell.GameSpellDictionary.Count + " spells.", Utils.LogType.SystemGo);
            #endregion

            #region Establish Spawn Zones
            if (!Facet.EstablishSpawnZones())
            {
                Utils.Log("Facet.EstablishSpawnZones() failed.", Utils.LogType.SystemFatalError);
                return -1;
            } 
            #endregion

            #region Create NPC Catalog
            Utils.Log("Creating NPC Catalog.", Utils.LogType.SystemGo);
            DAL.DBNPC.LoadNPCDictionary();
            #endregion

            #region Build New Entities
            try
            {
                var spawnZonesCount = 0;

                if (Autonomy.EntityBuilding.EntityCreationManager.BuildNewEntities(out spawnZonesCount))
                {
                    var autoDictionaryCount = Autonomy.EntityBuilding.EntityCreationManager.AutoCreatedNPCDictionary.Count;
                    Utils.Log("ECM created " + autoDictionaryCount + " new Entit" + (autoDictionaryCount == 1 ? "y" : "ies") + ".", Utils.LogType.SystemGo);
                    Utils.Log("ECM created " + spawnZonesCount + " new SpawnZone" + (spawnZonesCount == 1 ? "" : "s") + ".", Utils.LogType.SystemGo);
                }
                else
                {
                    Utils.Log("Failed EntityCreationManager.BuildNewEntities().", Utils.LogType.SystemFailure);
                    return -1;
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }

            #endregion

            #region Spawn Artifacts
            if (Autonomy.ItemBuilding.LootManager.SpawnArtifacts(out int artifactsCount))
                Utils.Log("ECM spawned " + artifactsCount + " artifact" + (artifactsCount == 1 ? "." : "s."), Utils.LogType.SystemGo);
            #endregion

            System.Threading.Tasks.Task spawnerTask = new System.Threading.Tasks.Task(() => KesSpawnerTask());
            spawnerTask.Start();

            #region Clear Stores / Restock stores);
            if (ConfigurationManager.AppSettings["ClearStores"].ToLower() == "true") // clear all store items that are not original store items
                StoreItem.ClearStores();

            if (ConfigurationManager.AppSettings["RestockStores"].ToLower() == "true") // restock store merchants
                StoreItem.RestockStores();
            #endregion

            #region Load GameEvents - Disabled until the week of 11/30/2015 for further work.
            //if (!DragonsSpine.GameEvents.GameEvent.LoadEvents())
            //{
            //    Utils.Log("GameEvents.LoadEvents() failed.", Utils.LogType.SystemFatalError);
            //    return -1;
            //}
            //else
            //    Utils.Log("Loaded " + DragonsSpine.GameEvents.GameEvent.GameEventDictionary.Count + " GameEvents.", Utils.LogType.SystemGo);
            #endregion

            Instance.StartTimers();

            var io = new IO(Instance.Settings.ServerPort); // get our network stuff ready

            if (!io.Open())
            {
                Utils.Log("Failed to start network services.", Utils.LogType.SystemFatalError);
                return -1;
            }
            try
            {
                Instance.RunGame(io);
            }
            catch (StackOverflowException soE)
            {
                Utils.LogException(soE);
                Instance.RestartServerWithoutSave("Process is terminated due to StackOverflowException.");
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                Instance.RestartServerWithoutSave("Process is terminated due to unhandled exception in Instance.RunGame.");
            }
            finally
            {
                //Instance.RestartServerWithoutSave("Process is terminated due to escape of main game loop in Instance.RunGame.");
                // TODO: Clean up of resources before restarting server due to escape of main game loop. 11/25/2015 Eb
            }

            Instance.RestartServerWithoutSave("Process is terminated due to escape of main game loop in Instance.RunGame.");

            return 0;
        }

        private void StartTimers()
        {
            _responseTimer = new System.Timers.Timer();
            _responseTimer.Elapsed += new ElapsedEventHandler(DetectProcessResponse);
            _responseTimer.Interval = 60000;
            _responseTimer.Start();
            Utils.Log("Detect process response timer started.", Utils.LogType.SystemGo);

            _roundTimer = new System.Timers.Timer();
            _roundTimer.Elapsed += new ElapsedEventHandler(RoundEvent); // Character rounds (5 seconds)
            _roundTimer.Interval = MasterRoundInterval;
            _roundTimer.Start();
            Utils.Log("Master round timer started.", Utils.LogType.SystemGo);

            _displayTimer = new System.Timers.Timer();
            _displayTimer.Elapsed += new ElapsedEventHandler(UpdateServerStatus);
            _displayTimer.Interval = 30000;
            //_displayTimer.Interval = MasterRoundInterval;
            _displayTimer.Start();
            Utils.Log("Server status display timer started.", Utils.LogType.SystemGo);

            _saveTimer = new System.Timers.Timer();
            _saveTimer.Elapsed += new ElapsedEventHandler(SaveEvent);
            _saveTimer.Interval = MasterRoundInterval + 1000;
            _saveTimer.Start();
            Utils.Log("Player save timer started.", Utils.LogType.SystemGo);

            

            _chronoTimer = new System.Timers.Timer();
            _chronoTimer.Elapsed += new ElapsedEventHandler(World.ShiftDailyCycle); // time change (30 minutes)
            _chronoTimer.Interval = new TimeSpan(0, 30, 0).TotalMilliseconds;
            _chronoTimer.Start();
            Utils.Log("Chronology timer started.", Utils.LogType.SystemGo);

            _lunarTimer = new System.Timers.Timer();
            _lunarTimer.Elapsed += new ElapsedEventHandler(World.ShiftLunarCycle); // moon phases timer (90 minutes)
            _lunarTimer.Interval = new TimeSpan(0, 90, 0).TotalMilliseconds;
            _lunarTimer.Start();
            Utils.Log("Lunar cycle timer started.", Utils.LogType.SystemGo);

            _inactivityTimer = new System.Timers.Timer();
            _inactivityTimer.Elapsed += new ElapsedEventHandler(InactivityEvent);
            _inactivityTimer.Interval = 10000;
            _inactivityTimer.Start();
            Utils.Log("Inactivity timer started.", Utils.LogType.SystemGo);

            foreach(GameEvents.GameEvent gameEvent in GameEvents.GameEvent.GameEventDictionary.Values)
                gameEvent.StartEventTimer();
        }

        private static DateTime LastMainLoopStart = DateTime.Now;

        protected void RunGame(IO io) // the primary loop function for the game
        {
            m_io = io;

            int exceptionCount = 0;

            DragonsSpineMain.ServerStatus = DragonsSpineMain.ServerState.Running;

            DragonsSpineMain.Instance.Settings.ServerNews = DragonsSpineMain.Instance.Settings.ServerNews + "^^Server Restarted: " + DragonsSpineMain.Instance.Settings.ServerStartTime.ToUniversalTime() + " UTC";

            start: Utils.Log("Starting main game loop.", Utils.LogType.SystemGo);

            try
            {
                while (DragonsSpineMain.ServerStatus <= DragonsSpineMain.ServerState.Locked)
                {
                    LastMainLoopStart = DateTime.Now;
                    CleanupLists();
                    io.HandleNewConnections();
                    io.GetInput();
                    io.ProcessRealTimeCommands();
                    io.SendOutput();
                    Thread.Sleep(100);
                }
            }
            catch (Exception e)
            {
                Utils.Log("Exception Data: " + e.Data + " Source: " + e.Source + " TargetSite: " + e.TargetSite, Utils.LogType.ExceptionDetail);
                Utils.LogException(e);
                exceptionCount++;

                if (exceptionCount <= 3)
                {
                    goto start;
                }
                else
                {
                    RestartServerWithoutSave("Exception count exceeded in main server loop.");
                }
            }

            IO.Close();
            System.Environment.Exit(0);
        }

        private void CleanupLists()
        {
            #region Remove
            try
            {
                if (IO.pplToRemoveFromLogin)
                {
                    foreach (PC ch in new ArrayList(IO.RemoveFromLogin))
                    {
                        Character.LoginList.Remove(ch);
                    }
                    IO.RemoveFromLogin.Clear();
                    IO.pplToRemoveFromLogin = false;
                }
                if (IO.pplToRemoveFromCharGen)
                {
                    foreach (PC ch in new ArrayList(IO.RemoveFromCharGen))
                    {
                        Character.CharGenList.Remove(ch);
                    }
                    IO.RemoveFromCharGen.Clear();
                    IO.pplToRemoveFromCharGen = false;
                }
                if (IO.pplToRemoveFromMenu)
                {
                    foreach (PC ch in new ArrayList(IO.RemoveFromMenu))
                    {
                        Character.MenuList.Remove(ch);
                    }
                    IO.RemoveFromMenu.Clear();
                    IO.pplToRemoveFromMenu = false;
                }
                if (IO.pplToRemoveFromConf)
                {
                    foreach (PC ch in new ArrayList(IO.RemoveFromConf))
                    {
                        Character.ConfList.Remove(ch);
                    }
                    IO.RemoveFromConf.Clear();
                    IO.pplToRemoveFromConf = false;
                }
                if (IO.pplToRemoveFromWorld)
                {
                    try
                    {
                        foreach (Character ch in new ArrayList(IO.RemoveFromWorld))
                        {
                            Character.AllCharList.Remove(ch);
                            if (ch is PC)
                            {
                                Character.PCInGameWorld.Remove(ch as PC);
                            }
                            else if(ch is NPC)
                            {
                                Character.NPCInGameWorld.Remove(ch as NPC);

                                if (ch is Adventurer)
                                    Character.AdventurersInGameWorldList.Remove(ch as Adventurer);
                            }
                        }
                        IO.RemoveFromWorld.Clear();
                        IO.pplToRemoveFromWorld = false;
                    }
                    catch (Exception e)
                    {
                        Utils.LogException(e);
                    }
                }
            }
            catch (Exception e)
            {
                Utils.Log("Error in Cleanup Lists <REMOVE>", Utils.LogType.SystemFailure);
                Utils.LogException(e);
                foreach (PC ch in new ArrayList(Character.PCInGameWorld))
                {
                    Character.AllCharList.Remove(ch);
                    Character.PCInGameWorld.Remove(ch);
                }
            }
            #endregion
            #region Add
            try
            {
                if (IO.pplToAddToLogin)
                {
                    foreach (PC ch in new List<PC>(IO.AddToLogin))
                    {
                        Character.LoginList.Add(ch);
                    }
                    IO.AddToLogin.Clear();
                    IO.pplToAddToLogin = false;
                }
                if (IO.pplToAddToCharGen)
                {
                    foreach (PC ch in new List<PC>(IO.AddToCharGen))
                    {
                        Character.CharGenList.Add(ch);
                    }
                    IO.AddToCharGen.Clear();
                    IO.pplToAddToCharGen = false;
                }
                if (IO.pplToAddToMenu)
                {
                    foreach (PC ch in new List<PC>(IO.AddToMenu))
                    {
                        Character.MenuList.Add(ch);
                        ch.afk = false;
                    }
                    //Protocol.UpdateUserLists(); // send updated user lists to protocol users
                    IO.AddToMenu.Clear();
                    IO.pplToAddToMenu = false;
                }
                if (IO.pplToAddToConf)
                {
                    foreach (PC ch in new List<PC>(IO.AddToConf))
                    {
                        Character.ConfList.Add(ch);
                    }
                    //Protocol.UpdateUserLists(); // send updated user lists to protocol users
                    IO.AddToConf.Clear();
                    IO.pplToAddToConf = false;
                }
                if (IO.pplToAddToWorld)
                {
                    foreach (Character ch in new List<Character>(IO.AddToWorld))
                    {
                        Character.AllCharList.Add(ch);
                        if (ch.IsPC)
                        {
                            Character.PCInGameWorld.Add(ch as PC);
                            (ch as PC).afk = false;
                            //Protocol.UpdateUserLists(); // send updated user lists to protocol users
                        }
                        else if(ch is NPC)
                        {
                            Character.NPCInGameWorld.Add(ch as NPC);
                        }

                        if(ch is Adventurer)
                            Character.AdventurersInGameWorldList.Add(ch as Adventurer);
                    }
                    IO.AddToWorld.Clear();
                    IO.pplToAddToWorld = false;
                }
            }
            catch (Exception e)
            {
                Utils.Log("Error in Cleanuplists <ADD>", Utils.LogType.SystemFailure);
                Utils.LogException(e);
            }
            #endregion
        }

        #region Timer Events

        private void PostRoundEvent(MultiDictionary<int, object> initlist)
        {
            for (int n = 25; n >= 0; n--)
            {
                try
                {
                    ICollection<object> objlist = initlist[n];
                    foreach (object myobj in objlist)
                    {
                        if (myobj is PC)
                            (myobj as PC).RoundEvent();
                        if (myobj is NPC)
                            (myobj as NPC).RoundEvent();
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        private void RoundEvent(object sender, ElapsedEventArgs eventArgs)
        {
            lock (this)
            {
                foreach (PC obj in new List<PC>(Character.PCInGameWorld))
                    InitiativeList.Add(Rules.RollD(1, 20) + obj.InitiativeModifier, obj);

                foreach (NPC obj in new List<NPC>(NPC.NPCInGameWorld))
                    InitiativeList.Add(Rules.RollD(1, 20) + obj.InitiativeModifier, obj);
                
                PostRoundEvent(InitiativeList);
            }

            //World.magicCordLastRound.Clear();

            //foreach (String cord in new List<string>(World.magicCordThisRound))
            //    World.magicCordLastRound.Add(cord);

            //World.magicCordThisRound.Clear();

            InitiativeList.Clear();

            DragonsSpineMain.GameRound++;

            
        }

        private static void SaveEvent(object sender, ElapsedEventArgs eventArgs)
        {
            foreach (PC pc in new List<PC>(Character.PCInGameWorld))
            {
                pc.Save();
            }
            
        }

        private static void JanitorEvent(object sender, ElapsedEventArgs eventArgs)
        {
            try
            {
                foreach (Facet facet in World.Facets)
                {
                    #region Increment all SpawnZone.Timers
                    foreach (SpawnZone szl in facet.Spawns.Values)
                        if (szl.NumberInZone < szl.MaxAllowedInZone)
                            szl.Timer++; 
                    #endregion

                    foreach (Land land in facet.Lands)
                    {
                        foreach (Map map in land.Maps)
                        {
                            if (map != null)
                            {
                                foreach (Cell cell in map.cells.Values)
                                {
                                    if (cell != null)
                                    {
                                        if (!cell.IsLair && cell.CellGraphic != Cell.GRAPHIC_ALTAR_PLACEABLE && cell.CellGraphic != Cell.GRAPHIC_COUNTER_PLACEABLE && cell.Items.Count > 0)
                                        {
                                            #region Handle items not in a lair cell, or on placeable altars and counters.
                                            foreach(Item item in new List<Item>(cell.Items))
                                            {
                                                // decay for corpses
                                                if (item is Corpse)
                                                {
                                                    try
                                                    {
                                                        if ((item as Corpse).IsPlayerCorpse)
                                                        {
                                                            #region Janitor handles player corpses.
                                                            if (item.dropRound < DragonsSpineMain.GameRound - World.PlayerCorpseDecayTimer)
                                                            {
                                                                PC pc = PC.GetOnline(item.special);
                                                                if (pc == null)
                                                                {
                                                                    Corpse.DumpCorpse(item as Corpse, cell);
                                                                    cell.Remove(item);
                                                                }
                                                                else if (!pc.IsDead)
                                                                {
                                                                    // TODO?
                                                                }
                                                                else
                                                                {
                                                                    Rules.DeadRest(pc);
                                                                }
                                                            }
                                                            #endregion
                                                        }
                                                        else
                                                        {
                                                            #region Janitor handles NPC corpses.
                                                            if (item.dropRound < DragonsSpineMain.GameRound - World.NPCCorpseDecayTimer)
                                                            {
                                                                Corpse.DumpCorpse(item as Corpse, cell);
                                                                (item as Corpse).Ghost = null;
                                                                cell.Remove(item);
                                                            }
                                                            #endregion
                                                        }
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Utils.LogException(e);
                                                        Utils.Log("Error in JanitorEvent while handling Corpse decay. Corpse: " + item.GetLogString(), Utils.LogType.SystemWarning);
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        if (item == null) continue;

                                                        //decay for items that aren't attuned or artifacts
                                                        if (item.attunedID <= 0 && !item.IsArtifact())
                                                        {
                                                            if (item.dropRound < DragonsSpineMain.GameRound - World.ItemDecayTimer)
                                                            {
                                                                if (item.itemType == Globals.eItemType.Coin)
                                                                {
                                                                    // all coins go into lottery for particular land
                                                                    World.CollectFeeForLottery(World.FEE_JANITORIAL_COIN_REMOVAL, cell.LandID, ref item.coinValue);
                                                                }
                                                                else World.CollectFeeForLottery(World.FEE_JANITORIAL_ITEM_REMOVAL, cell.LandID, ref item.coinValue);
                                                                cell.Remove(item);
                                                            }
                                                        }
                                                        //decay for attuned items
                                                        else
                                                        {
                                                            if (item.dropRound < DragonsSpineMain.GameRound - World.AttunedOrArtifactItemDecayTimer)
                                                            {
                                                                if (!item.IsArtifact())
                                                                    Utils.Log("Janitor removed attuned item: " + item.GetLogString() + " PlayerID: " + item.attunedID + ".", Utils.LogType.JanitorWarning);
                                                                else if (item.attunedID >= 0) Utils.Log("Janitor removed attuned artifact " + item.GetLogString() + " PlayerID: " + item.attunedID + ".", Utils.LogType.JanitorWarning);
                                                                else Utils.Log("Janitor removed artifact " + item.GetLogString() + ".", Utils.LogType.JanitorWarning);
                                                                cell.Remove(item);
                                                            }
                                                        }
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Utils.LogException(e);
                                                        if(item != null)
                                                            Utils.Log("Error in JanitorEvent while handling non Corpse item decay. Item: " + item.GetLogString(), Utils.LogType.SystemWarning);
                                                        continue;
                                                    }
                                                }
                                            } 
                                            #endregion
                                        }

                                        if (cell.IsOrnicLocker)
                                        {
                                            try
                                            {
                                                #region Spawn common use items on altar near Ornic lockers.
                                                Cell altarCell = Map.GetNearestCounterOrAltarCell(cell);

                                                if (altarCell != null && altarCell.CellGraphic == Cell.GRAPHIC_ALTAR_PLACEABLE)
                                                {
                                                    int numRecallRings = 0;

                                                    foreach (Item item in altarCell.Items)
                                                    {
                                                        if (item.itemID == Item.ID_RECALLRING)
                                                            numRecallRings++;
                                                    }

                                                    if (numRecallRings < 2)
                                                    {
                                                        altarCell.SendToAllInSight("The altar next to the Ornic lockers emits a low humming sound and is briefly surrounded by a pale violet luminescence.");

                                                        while (numRecallRings < 2)
                                                        {
                                                            Item recallRing = Item.CopyItemFromDictionary(Item.ID_RECALLRING);
                                                            recallRing.coinValue = 0; // to avoid uing this to exploit coin gain
                                                            altarCell.Add(recallRing);
                                                            numRecallRings++;
                                                        }
                                                    }
                                                }
                                                #endregion
                                            }
                                            catch (Exception e)
                                            {
                                                Utils.LogException(e);
                                                Utils.Log("Error in JanitorEvent while handling Ornic Flame cell. Cell: " + cell.GetLogString(true), Utils.LogType.SystemWarning);
                                                continue;
                                            }
                                        }

                                        if (cell.IsBalmFountain && cell.Items.Count > 0)
                                        {
                                            try
                                            {
                                                Merchant.ExchangeCoinsForBalm(cell);
                                            }
                                            catch (Exception e)
                                            {
                                                Utils.LogException(e);
                                                Utils.Log("Error in JanitorEvent while handling balm fountain exchange. Cell: " + cell.GetLogString(true), Utils.LogType.SystemWarning);
                                                continue;
                                            }
                                        }

                                        if (!cell.IsBarren && (cell.balmBerry || cell.manaBerry || cell.poisonBerry || cell.stamBerry || cell.growsSprigs) && (cell.droppedFruit < cell.dailyFruit))
                                        {
                                            try
                                            {
                                                #region The janitor is also a gardener, making sure berry bushes are producing.
                                                bool hasBalmBerries = false;
                                                bool hasManaBerries = false;
                                                bool hasPoisonBerries = false;
                                                bool hasStamBerries = false;
                                                bool hasSprigs = false;

                                                foreach (Item berryItem in cell.Items)
                                                {
                                                    if (berryItem.itemID == Item.ID_BALMBERRY && cell.balmBerry) { hasBalmBerries = true; break; }
                                                    else if (berryItem.itemID == Item.ID_POISONBERRY && cell.poisonBerry) { hasPoisonBerries = true; break; }
                                                    else if (berryItem.itemID == Item.ID_MANABERRY && cell.manaBerry) { hasManaBerries = true; break; }
                                                    else if (berryItem.itemID == Item.ID_STAMINABERRY && cell.stamBerry) { hasStamBerries = true; break; }
                                                    else if (berryItem.itemID == Item.ID_GREENSPRIG && cell.growsSprigs) { hasSprigs = true; break; }
                                                }

                                                if (!hasBalmBerries && !hasManaBerries && !hasPoisonBerries && !hasStamBerries)
                                                {
                                                    int berriesID = -1;

                                                    if (cell.balmBerry && !hasBalmBerries) { berriesID = Item.ID_BALMBERRY; }
                                                    else if (cell.poisonBerry && !hasPoisonBerries) { berriesID = Item.ID_POISONBERRY; }
                                                    else if (cell.manaBerry && !hasManaBerries) { berriesID = Item.ID_MANABERRY; }
                                                    else if (cell.stamBerry && !hasStamBerries) { berriesID = Item.ID_STAMINABERRY; }
                                                    else if (cell.growsSprigs && !hasSprigs) { berriesID = Item.ID_GREENSPRIG; }
                                                    if (berriesID != -1)
                                                    {
                                                        cell.Add(Item.CopyItemFromDictionary(berriesID));
                                                        cell.droppedFruit++;
                                                    }
                                                }

                                                //if (!hasSprigs && cell.growsSprigs)
                                                //{
                                                //    cell.Add(Item.CopyItemFromDictionary(Item.ID_GREENSPRIG));
                                                //}
                                                #endregion
                                            }
                                            catch (Exception e)
                                            {
                                                Utils.LogException(e);
                                                Utils.Log("Error in JanitorEvent while handling berry regrowth. Cell: " + cell.GetLogString(true), Utils.LogType.SystemWarning);
                                                continue;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                Utils.Log("Failure in JanitorEvent while pruning cells.", Utils.LogType.SystemFailure);
            }

            
            
            NPC.DoSpawn();
        }

        private static void InactivityEvent(object sender, ElapsedEventArgs eventArgs)
        {
            int a;

            #region Check inactivity in game world.
            for (a = 0; a < Character.PCInGameWorld.Count; a++) // loop through all characters in the game and do maintenence
            {
                PC ch = Character.PCInGameWorld[a];
                if (ch.afk || (ch.corpseIsCarried && ch.socketConnected()) || ch.IsResting) continue;
                ch.Timeout--;
                if (ch.Timeout < 0 || !ch.socketConnected())
                {
                    if (ch.IsDead)
                    {
                       CommandTasker.ParseCommand(ch, "rest", "");
                    }

                    ch.RemoveFromWorld();
                    ch.RemoveFromServer();
                    Utils.Log(ch.GetLogString() + " disconnected from the world for inactivity.", Utils.LogType.Timeout);
                }
            } 
            #endregion

            #region Check inactivity at login.
            for (a = 0; a < Character.LoginList.Count; a++) // check for inactivity at the login prompt
            {
                PC ch = Character.LoginList[a] as PC;
                ch.Timeout--;
                if (ch.Timeout < 0 || !ch.socketConnected())
                {
                    if (!ch.socketConnected())
                    {
                        Utils.Log(ch.Account.hostName + " (" + ch.Account.ipAddress + ") lost connection, removing from login.", Utils.LogType.Disconnect);
                    }
                    else
                    {
                        Utils.Log(ch.Account.hostName + " (" + ch.Account.ipAddress + ") disconnected from login for inactivity.", Utils.LogType.Timeout);
                    }
                    ch.RemoveFromLogin();
                    ch.RemoveFromServer();
                }
            } 
            #endregion

            #region Check inactivity in conference rooms.
            for (a = 0; a < Character.ConfList.Count; a++) // check for inactivity in limbo
            {
                PC ch = Character.ConfList[a] as PC;
                if (ch.afk) continue;

                ch.Timeout--;

                if (ch.Timeout < 0 || !ch.socketConnected())
                {
                    if (!ch.socketConnected())
                    {
                        if (!ch.IsInvisible)
                        {
                            ch.SendToAllInConferenceRoom(Conference.GetStaffTitle(ch as PC) + ch.Name + " has left the world.", ProtocolYuusha.TextType.Exit);
                        }
                        Utils.Log(ch.GetLogString() + " lost connection, removing from limbo.", Utils.LogType.Disconnect);
                    }
                    else
                    {
                        if (!ch.IsInvisible)
                        {
                            ch.SendToAllInConferenceRoom(Conference.GetStaffTitle(ch as PC) + ch.Name + " has left the world.", ProtocolYuusha.TextType.Exit);
                        }
                        Utils.Log(ch.GetLogString() + " disconnected from limbo for inactivity.", Utils.LogType.Timeout);
                    }
                    ch.RemoveFromConf();
                    ch.RemoveFromServer();
                }
            } 
            #endregion

            #region Check inactivity in character generation.
            for (a = 0; a < Character.CharGenList.Count; a++)
            {
                PC ch = Character.CharGenList[a];
                ch.Timeout--;
                if (ch.Timeout < 0 || !ch.socketConnected())
                {
                    if (!ch.socketConnected())
                    {
                        if (ch.Name != "Nobody")
                        {
                            Utils.Log(ch.GetLogString() + " lost connection, removing from chargen.", Utils.LogType.Disconnect);
                        }
                        else
                        {
                            Utils.Log(ch.Account.hostName + " (" + ch.Account.ipAddress + ") lost connection, removing from chargen.", Utils.LogType.Disconnect);
                        }
                    }
                    else
                    {
                        if (ch.Name != "Nobody")
                        {
                            Utils.Log(ch.GetLogString() + " disconnected from chargen for inactivity.", Utils.LogType.Timeout);
                        }
                        else
                        {
                            Utils.Log(ch.Account.hostName + " (" + ch.Account.ipAddress + ") disconnected from chargen for inactivity.", Utils.LogType.Timeout);
                        }
                    }
                    ch.RemoveFromCharGen();
                    ch.RemoveFromServer();
                }
            }
            #endregion

            #region Check inactivity for players at a menu.
            for (a = 0; a < Character.MenuList.Count; a++)
            {
                PC ch = Character.MenuList[a] as PC;
                ch.Timeout--;
                if (ch.Timeout < 0 || !ch.socketConnected())
                {
                    if (!ch.socketConnected())
                    {
                        Utils.Log(ch.GetLogString() + " lost connection, removing from menu.", Utils.LogType.Disconnect);
                    }
                    else
                    {
                        Utils.Log(ch.GetLogString() + " disconnected from the menu for inactivity.", Utils.LogType.Timeout);
                    }
                    ch.RemoveFromMenu();
                    ch.RemoveFromServer();
                }
            } 
            #endregion
        }

        private static void UpdateServerStatus(object sender, ElapsedEventArgs eventArgs)
        {
            int playerCount = Character.PCInGameWorld.Count + Character.MenuList.Count + Character.ConfList.Count + Character.CharGenList.Count;

            string outputString = DateTime.Now.ToLocalTime() + ": NPCs: [" + Character.NPCInGameWorld.Count
                + "] | Players: [" + playerCount + "] | Rnd: [" + DragonsSpineMain.GameRound + "]";

            Console.WriteLine(outputString);
            //Utils.Log(outputString, Utils.LogType.SystemGo);
        }

        private static void DetectProcessResponse(object sender, ElapsedEventArgs eventArgs)
        {
            if (DateTime.Now - LastMainLoopStart > TimeSpan.FromMinutes(1))
            {
                DragonsSpineMain.Instance.RestartServerWithoutSave("More than one minute since start of the most recent game loop iteration. The server is now restarting without player saves.");
                return;
            }

            System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();
           
            if (!p.Responding)
            {
                DragonsSpineMain.Instance.RestartServerWithoutSave("The server process stopped responding.");
                return;
            }

            // 60000 / 5000 = 12 rounds
            int roundsBetweenChecks = Convert.ToInt32(_responseTimer.Interval / MasterRoundInterval);

            // If more than 12 + 4 rounds go by without a GameRound update, restart the server.
            if (DragonsSpineMain.GameRound - _lastProcessResponseTimerEvent > roundsBetweenChecks + (roundsBetweenChecks / 3))
            {
                DragonsSpineMain.Instance.RestartServerWithoutSave("GameRound has not incremented sufficiently; the server is frozen and is restarting.");
                return;
            }

            _lastProcessResponseTimerEvent = DragonsSpineMain.GameRound;
        }
        #endregion

        public static void SetInstance(DragonsSpineMain server)
        {
            Instance = server;
        }

        public void RestartServerWithoutSave(string reason)
        {
            try
            {
                Utils.Log("The server is being restarted without a player save. Reason: " + reason, Utils.LogType.SystemRestart);
                try
                {
                    m_io.listener.Stop();
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
                DragonsSpineMain.ServerStatus = ServerState.ShuttingDown;
                DragonsSpineMain.ServerStatus = ServerState.Restarting;
                System.Diagnostics.ProcessStartInfo Info = new System.Diagnostics.ProcessStartInfo();
                Info.Arguments = "/C ping 127.0.0.1 -n 1 -w 3000 > Nul & \"" + Application.ExecutablePath;
                Info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                Info.CreateNoWindow = false;
                Info.FileName = "cmd.exe";
                System.Diagnostics.Process.Start(Info);
                Application.Exit();
                
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }
    }
}
