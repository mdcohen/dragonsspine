using System;
using System.Collections.Generic;
using System.Timers;
using System.Net.Sockets;
using System.Text;
using DragonsSpine.GameWorld;
using TextManager = DragonsSpine.GameSystems.Text.TextManager;

namespace DragonsSpine
{
    public class PC : Character
    {
        public Account Account { get; set; }
        int timeout = Character.INACTIVITY_TIMEOUT;
        bool ancestor = false; // is this character an ancestor (disables Underworld quests)
        int ancestorID = 0; // the player ID of this character's ancestor
        private string[] DisplayBuffer = new string[DISPLAY_BUFFER_SIZE];
        public string DisplayText = "";
        public DateTime birthday; // time of player creation
        public DateTime lastOnline; // time of last login
        public int confRoom = 0; // ChatRoom.rooms[]
        int inputPos; //our current position in the input buffer
        public bool corpseIsCarried; // player's corpse is being carried

        //public List<Talents.GameTalent.TALENTS> disabledTalents = new List<Talents.GameTalent.TALENTS>();

        public bool savePlayerSpells;

        // underworld specific
        public int UW_hitsMax = 0;
        public int UW_hitsAdjustment = 0;
        public int UW_staminaMax = 0;
        public int UW_staminaAdjustment = 0;
        public int UW_manaMax = 0;
        public int UW_manaAdjustment = 0;
        public bool UW_hasLiver = false;
        public bool UW_hasLungs = false;
        public bool UW_hasIntestines = false;
        public bool UW_hasStomach = false;

        public double bankGold;

        public long highMace; // highest skill level achieved
        public long highBow;
        public long highFlail;
        public long highDagger;
        public long highRapier;
        public long highTwoHanded;
        public long highStaff;
        public long highShuriken;
        public long highSword;
        public long highThreestaff;
        public long highHalberd;
        public long highUnarmed;
        public long highThievery;
        public long highMagic;
        public long highBash;

        public long trainedMace = 0; // current training amount
        public long trainedBow = 0;
        public long trainedFlail = 0;
        public long trainedDagger = 0;
        public long trainedRapier = 0;
        public long trainedTwoHanded = 0;
        public long trainedStaff = 0;
        public long trainedShuriken = 0;
        public long trainedSword = 0;
        public long trainedThreestaff = 0;
        public long trainedHalberd = 0;
        public long trainedUnarmed = 0;
        public long trainedThievery = 0;
        public long trainedMagic = 0;
        public long trainedBash = 0;

        #region Player vs. Player
        public int currentKarma = 0; // current karma amount
        public long lifetimeKarma = 0; // lifetime karma amount
        public int currentMarks = 0; // account current marks amount		
        public int lifetimeMarks = 0; // character lifetime marks amount (this is different than account lifetime marks)
        public long pvpNumKills = 0; // PvP kills amount
        public long pvpNumDeaths = 0; // PvP deaths amount
        List<int> playersKilled = new List<int>(); // array of player IDs that were killed by this character without self defense flag
        #endregion

        public bool DisplayGameRound = false; // not saved to database as of 10/5/2015
        public bool DisplayPetDamage = false;
        public bool DisplayPetMessages = false;
        public bool DisplayCombatDamage = false;
        public bool DisplayDamageShield = true;
        public int[] ignoreList = new int[MAX_IGNORE]; // array of player IDs that are ignored
        public bool filterProfanity = true; // profanity filter, default true
        public int[] friendsList = new int[MAX_FRIENDS]; // array of player IDs that are friends
        public bool friendNotify = true; // display notification when friend logs on and off
        public bool receivePages = true;
        public bool receiveTells = true;
        public bool receiveGroupInvites = true;
        public bool afk = false; // sets AFK tag and AFK message when sent a private message or paged
        //public string afkMessage = "I am currently A.F.K. (Away From Keyboard).";
        Globals.eImpLevel impLevel = Globals.eImpLevel.USER; // implementor level
        public string colorChoice = "brown";
        public bool IsPossessed = false; // true if being possessed by a DEV

        public Globals.eImpLevel ImpLevel
        {
            get { return this.impLevel; }
            set { this.impLevel = value; }
        }

        public bool showStaffTitle = true; // show implevel title in conference rooms and game [GM] [DEV]


        bool anonymous = false; // if player is anonymous will not be seen on WHO or in scores list
        public bool IsAnonymous
        {
            get { return this.anonymous; }
            set { this.anonymous = value; }
        }
        public bool echo = true;
        public List<string> macros = new List<string>();
        long roundsPlayed;
        public long RoundsPlayed
        {
            get { return this.roundsPlayed; }
            set { this.roundsPlayed = value; }
        }
        public int RoundsPlayedSinceAddedToLottery
        {
            get;
            set;
        }

        #region Public Properties
        public int AncestorID
        {
            get { return this.ancestorID; }
            set { this.ancestorID = value; }
        }

        public bool IsAncestor
        {
            get { return this.ancestor; }
            set { this.ancestor = value; }
        }

        public Mail.GameMail.GameMailbox Mailbox
        {
            get;
            set;
        }

        public int Timeout
        {
            get { return this.timeout; }
            set { this.timeout = value; }
        }

        public List<int> PlayersKilled
        {
            get { return this.playersKilled; }
        }

        public bool IsNewPC
        {
            get;
            set;
        }
        #endregion

        #region Socket
        private Socket socket; //the socket

        public void setSocket(Socket newSocket)
        {
            this.socket = newSocket;
            this.socket.LingerState = new LingerOption(true, 10);
            this.socket.Ttl = 42;
            this.socket.ReceiveTimeout = 1500;
            this.socket.SendTimeout = 1500;
        }

        public Socket getSocket() { return this.socket; }

        public int socketAvailable()
        {
            if (socket == null || !socket.Connected)
            {
                return 0;
            }
            return this.socket.Available;
        }

        public int socketReceive(byte[] buffer, int available, int loc)
        {
            return socket.Receive(buffer, available, 0);
        }

        public bool socketConnected()
        {
            if(protocol == "Proto")
            {
                if(ProtoClientIO.server.GetClient(Account.accountName) != null)
                {
                    return true;
                }
                return false;
            }
            if (socket == null) return false;
            return socket.Connected;
        }

        public int socketSend(byte[] b)
        {
            try
            {
                if (this.socketConnected())
                {
                    return socket.Send(b);
                }
                else
                {
                    return -1;
                }

            }
            catch (Exception)
            {
                return -1;
            }
        }

        public void DisconnectSocket()
        {
            if (socketConnected())
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (ObjectDisposedException)
                {
                    // nothing -- it happens anyway
                }
                catch(Exception)
                {
                    //Utils.LogException(e);
                }
            }
        }
        #endregion

        // since data is received from the player one char at a time, we store the chars in inputBuffer until
        // we receive a linefeed, at which case we know they are done with their command. At that point,
        // we copy the chars in inputBuffer into a String, which is stored in the inputCommandQueue until
        // it is time to process commands
        byte[] inputBuffer; //the input buffer for storing commands

        //Queue<string> outputQueue; // output is stored in a queue also...this is necessary because of network latency
        List<string> outputQueue;

        #region Constructors (2)
        public PC()
            : base()
        {
            this.Account = new Account();
            this.IsPC = true;
            this.inputBuffer = new byte[MAX_INPUT_LENGTH];
            this.inputCommandQueue = new Queue<string>();
            //this.outputQueue = new Queue<string>();
            this.outputQueue = new List<string>();
            this.inputPos = 0;
            this.RoundsPlayedSinceAddedToLottery = 0;
            this.EffectsList = new System.Collections.Concurrent.ConcurrentDictionary<Effect.EffectTypes, Effect>();
            this.WornEffectsList = new List<Effect>();

            // fill the macros list with empty strings
            for (int a = 0; a < PC.MAX_MACROS; a++)
            {
                this.macros.Add("");
            }
        }

        public PC(System.Data.DataRow dr)
            : base()
        {
            this.IsPC = true;
            this.Account = DAL.DBAccount.GetAccountByName(dr["account"].ToString());
            this.inputBuffer = new byte[MAX_INPUT_LENGTH];
            this.inputCommandQueue = new Queue<string>();
            //this.outputQueue = new Queue<string>();
            this.outputQueue = new List<string>();
            this.inputPos = 0;
            this.RoundsPlayedSinceAddedToLottery = 0;
            this.UniqueID = Convert.ToInt32(dr["playerID"]);
            this.Notes = dr["notes"].ToString();
            this.Name = dr["name"].ToString();
            this.gender = (Globals.eGender)Convert.ToInt16(dr["gender"]);
            this.race = dr["race"].ToString();
            this.classFullName = dr["classFullName"].ToString();
            this.BaseProfession = (ClassType)Enum.Parse(typeof(ClassType), dr["classType"].ToString());
            this.visualKey = dr["visualKey"].ToString();
            // temporary
            if (this.visualKey == null || this.visualKey == "")
                PC.SetCharacterVisualKey(this);
            this.Alignment = (Globals.eAlignment)Convert.ToInt32(dr["alignment"]);
            this.confRoom = Convert.ToInt32(dr["confRoom"]);
            this.ImpLevel = (Globals.eImpLevel)Convert.ToInt32(dr["impLevel"]);
            this.IsAncestor = Convert.ToBoolean(dr["ancestor"]);
            this.AncestorID = Convert.ToInt32(dr["ancestorID"]);
            this.FacetID = Convert.ToInt16(dr["facet"]);
            this.LandID = Convert.ToInt16(dr["land"]);
            this.MapID = Convert.ToInt16(dr["map"]);
            this.X = Convert.ToInt16(dr["xCord"]);
            this.Y = Convert.ToInt16(dr["yCord"]);
            this.Z = Convert.ToInt32(dr["zCord"]);
            this.CurrentCell = Cell.GetCell(this.FacetID, this.LandID, this.MapID, this.X, this.Y, this.Z);
            this.dirPointer = dr["dirPointer"].ToString();
            this.Stunned = Convert.ToInt16(dr["stunned"]);
            this.floating = Convert.ToInt16(dr["floating"]);
            this.IsDead = Convert.ToBoolean(dr["dead"]);
            this.fighterSpecialization = (Globals.eSkillType)Enum.Parse(typeof(Globals.eSkillType), dr["fighterSpecialization"].ToString());
            this.Level = Convert.ToInt16(dr["level"]);
            this.Experience = Convert.ToInt64(dr["exp"]);
            this.Hits = Convert.ToInt32(dr["hits"]);
            this.HitsAdjustment = Convert.ToInt32(dr["hitsAdjustment"]);
            this.HitsMax = Convert.ToInt32(dr["hitsMax"]);
            this.HitsDoctored = Convert.ToInt32(dr["hitsDoctored"]);
            this.StaminaMax = Convert.ToInt32(dr["stamina"]);
            this.StaminaAdjustment = Convert.ToInt32(dr["staminaAdjustment"]);
            this.Stamina = Convert.ToInt32(dr["stamLeft"]);
            this.Mana = Convert.ToInt32(dr["mana"]);
            this.ManaMax = Convert.ToInt32(dr["manaMax"]);
            this.ManaAdjustment = Convert.ToInt32(dr["manaAdjustment"]);
            this.Age = Convert.ToInt32(dr["age"]);
            this.RoundsPlayed = Convert.ToInt64(dr["roundsPlayed"]);
            this.Deaths = Convert.ToInt32(dr["numDeaths"]);
            this.Kills = Convert.ToInt32(dr["numKills"]);
            this.bankGold = Convert.ToInt64(dr["bankGold"]);
            this.Strength = Convert.ToInt32(dr["strength"]);
            this.Dexterity = Convert.ToInt32(dr["dexterity"]);
            this.Intelligence = Convert.ToInt32(dr["intelligence"]);
            this.Wisdom = Convert.ToInt32(dr["wisdom"]);
            this.Constitution = Convert.ToInt32(dr["constitution"]);
            this.Charisma = Convert.ToInt32(dr["charisma"]);
            this.strengthAdd = Convert.ToInt32(dr["strengthAdd"]);
            this.dexterityAdd = Convert.ToInt32(dr["dexterityAdd"]);
            //
            this.birthday = Convert.ToDateTime(dr["birthday"]);
            this.lastOnline = Convert.ToDateTime(dr["lastOnline"]);

            // Underworld specific
            this.UW_hitsMax = Convert.ToInt32(dr["UW_hitsMax"]);
            this.UW_hitsAdjustment = Convert.ToInt32(dr["UW_hitsAdjustment"]);
            this.UW_staminaMax = Convert.ToInt32(dr["UW_staminaMax"]);
            this.UW_staminaAdjustment = Convert.ToInt32(dr["UW_staminaAdjustment"]);
            this.UW_manaMax = Convert.ToInt32(dr["UW_manaMax"]);
            this.UW_manaAdjustment = Convert.ToInt32(dr["UW_manaAdjustment"]);
            this.UW_hasIntestines = Convert.ToBoolean(dr["UW_intestines"]);
            this.UW_hasLiver = Convert.ToBoolean(dr["UW_liver"]);
            this.UW_hasLungs = Convert.ToBoolean(dr["UW_lungs"]);
            this.UW_hasStomach = Convert.ToBoolean(dr["UW_stomach"]);
            //Player vs. Player
            this.currentKarma = Convert.ToInt32(dr["currentKarma"]);
            this.lifetimeKarma = Convert.ToInt64(dr["lifetimeKarma"]);
            this.pvpNumDeaths = Convert.ToInt64(dr["pvpDeaths"]);
            this.pvpNumKills = Convert.ToInt64(dr["pvpKills"]);
            this.currentMarks = Account.GetCurrentMarks(this.Account.accountID);
            this.lifetimeMarks = Convert.ToInt32(dr["lifetimeMarks"]);
            int a = 0;
            string[] pFlagged = dr["playersFlagged"].ToString().Split(ProtocolYuusha.ASPLIT.ToCharArray());
            if (dr["playersFlagged"].ToString() != "")
            {
                for (a = 0; a < pFlagged.Length; a++)
                {
                    int flaggedID = Convert.ToInt32(pFlagged[a]);
                    if (!FlaggedUniqueIDs.Contains(flaggedID))
                        FlaggedUniqueIDs.Add(flaggedID);
                }
            }
            if (dr["playersKilled"].ToString() != "")
            {
                pFlagged = dr["playersKilled"].ToString().Split(ProtocolYuusha.ASPLIT.ToCharArray());
                for (a = 0; a < pFlagged.Length; a++)
                {
                    this.PlayersKilled.Add(Convert.ToInt32(pFlagged[a]));
                }
            }

            DAL.DBPlayer.LoadPlayerSettings(this); // add the player's settings
            DAL.DBPlayer.LoadPlayerSkills(this); // add skills to the player

            try
            {
                this.LeftHand = DAL.DBPlayer.LoadPlayerHeld(this.UniqueID, false); // load left hand item
            }
            catch(Exception e)
            {
                this.LeftHand = null;
                Utils.LogException(e);
            }
            try
            {
                this.RightHand = DAL.DBPlayer.LoadPlayerHeld(this.UniqueID, true); // load right hand item
            }
            catch(Exception e)
            {
                this.RightHand = null;
                Utils.LogException(e);
            }

            this.sackList = DAL.DBPlayer.LoadPlayerSack(this.UniqueID);
            this.pouchList = DAL.DBPlayer.LoadPlayerPouch(this.UniqueID);
            this.wearing = DAL.DBPlayer.LoadPlayerWearing(this.UniqueID);
            this.lockerList = DAL.DBPlayer.LoadPlayerLocker(this.UniqueID);
            this.beltList = DAL.DBPlayer.LoadPlayerBelt(this.UniqueID);
            this.RightRing1 = DAL.DBPlayer.LoadPlayerRings(this.UniqueID, (int)Globals.eWearOrientation.RightRing1);
            this.RightRing2 = DAL.DBPlayer.LoadPlayerRings(this.UniqueID, (int)Globals.eWearOrientation.RightRing2);
            this.RightRing3 = DAL.DBPlayer.LoadPlayerRings(this.UniqueID, (int)Globals.eWearOrientation.RightRing3);
            this.RightRing4 = DAL.DBPlayer.LoadPlayerRings(this.UniqueID, (int)Globals.eWearOrientation.RightRing4);
            this.LeftRing1 = DAL.DBPlayer.LoadPlayerRings(this.UniqueID, (int)Globals.eWearOrientation.LeftRing1);
            this.LeftRing2 = DAL.DBPlayer.LoadPlayerRings(this.UniqueID, (int)Globals.eWearOrientation.LeftRing2);
            this.LeftRing3 = DAL.DBPlayer.LoadPlayerRings(this.UniqueID, (int)Globals.eWearOrientation.LeftRing3);
            this.LeftRing4 = DAL.DBPlayer.LoadPlayerRings(this.UniqueID, (int)Globals.eWearOrientation.LeftRing4);
            this.spellDictionary = DAL.DBPlayer.LoadPlayerSpells(this.BaseProfession, this.UniqueID);
            DAL.DBPlayer.LoadPlayerEffects(this);
            this.QuestList = DAL.DBPlayer.LoadPlayerQuests(this.UniqueID);
            DAL.DBPlayer.LoadPlayerFlags(this, true);
            DAL.DBPlayer.LoadPlayerTalents(this);

            this.Mailbox = new Mail.GameMail.GameMailbox(this.UniqueID);
        }
        #endregion

        #region Static Methods
        public static Object GetField(int playerID, string field, object var, string comments)
        {
            if (comments != null)
            {
                Utils.Log(comments, Utils.LogType.Unknown);
            }

            Object obj = DAL.DBPlayer.GetPlayerField(playerID, field, var.GetType());

            if (obj == null)
            {
                Utils.Log("FAILURE: PC.getField(" + playerID + ", " + field + ", " + var.ToString() + ", Comments: " + comments + ")", Utils.LogType.SystemFailure);
                return null;
            }
            return obj;
        }

        public static void SaveField(int playerID, string field, object var, string comments) // save a single PC field
        {
            if (comments != null)
            {
                Utils.Log(comments, Utils.LogType.Unknown);
            }

            int result = DAL.DBPlayer.SavePlayerField(playerID, field, var);

            if (result != 1)
            {
                Utils.Log("FAILURE: PC.saveField(" + playerID + ", " + field + ", " + var.ToString() + ", Comments: " + comments + ")", Utils.LogType.SystemFailure);
            }
        }

        public static void PossessCharacter(Character ch, PC pc1)
        {
            ch.UniqueID = pc1.UniqueID;
            //ch.accountID = pc1.accountID;
            //ch.account = pc1.account;
            ch.Name = pc1.Name;
            ch.gender = pc1.gender;
            ch.race = pc1.race;
            ch.BaseProfession = pc1.BaseProfession;
            ch.classFullName = pc1.classFullName;
            ch.BaseProfession = pc1.BaseProfession;
            ch.visualKey = pc1.visualKey;
            ch.Alignment = pc1.Alignment;
            if (ch.PCState != Globals.ePlayerState.CONFERENCE)
                (ch as PC).confRoom = pc1.confRoom;
            //(ch as PC).ImpLevel = pc1.ImpLevel;
            //(ch as PC).IsImmortal = pc1.IsImmortal;
            (ch as PC).friendsList = pc1.friendsList;
            (ch as PC).friendNotify = pc1.friendNotify;
            (ch as PC).ignoreList = pc1.ignoreList;
            (ch as PC).receivePages = pc1.receivePages;
            (ch as PC).receiveTells = pc1.receiveTells;
            (ch as PC).filterProfanity = pc1.filterProfanity;
            (ch as PC).showStaffTitle = pc1.showStaffTitle;
            (ch as PC).macros = pc1.macros;
            (ch as PC).DisplayCombatDamage = pc1.DisplayCombatDamage;
            (ch as PC).IsAncestor = pc1.IsAncestor;
            (ch as PC).AncestorID = pc1.AncestorID;
            (ch as PC).echo = pc1.echo;
            (ch as PC).IsAnonymous = pc1.IsAnonymous;
            ch.LandID = pc1.LandID;
            ch.MapID = pc1.MapID;
            ch.X = pc1.X;
            ch.Y = pc1.Y;
            ch.CurrentCell = Cell.GetCell(pc1.FacetID, pc1.LandID, pc1.MapID, pc1.X, pc1.Y, pc1.Z);
            ch.dirPointer = pc1.dirPointer;
            ch.Stunned = pc1.Stunned;
            ch.floating = pc1.floating;
            ch.IsDead = pc1.IsDead;
            ch.IsHidden = pc1.IsHidden;
            //ch.IsInvisible = pc1.IsInvisible;
            ch.Level = pc1.Level;
            ch.Experience = pc1.Experience;
            ch.Hits = pc1.Hits;
            ch.HitsAdjustment = pc1.HitsAdjustment;
            ch.HitsMax = pc1.HitsMax;
            ch.HitsDoctored = pc1.HitsDoctored;
            ch.StaminaAdjustment = pc1.StaminaAdjustment;
            ch.StaminaMax = pc1.StaminaMax;
            ch.Stamina = pc1.Stamina;
            ch.Mana = pc1.Mana;
            ch.ManaAdjustment = pc1.ManaAdjustment;
            ch.ManaMax = pc1.ManaMax;
            ch.hitsRegen = pc1.hitsRegen;
            ch.manaRegen = pc1.manaRegen;
            ch.staminaRegen = pc1.staminaRegen;
            ch.Age = pc1.Age;
            (ch as PC).RoundsPlayed = pc1.RoundsPlayed;
            ch.Deaths = pc1.Deaths;
            ch.Kills = pc1.Kills;
            (ch as PC).bankGold = pc1.bankGold;
            ch.Strength = pc1.Strength;
            ch.Dexterity = pc1.Dexterity;
            ch.Intelligence = pc1.Intelligence;
            ch.Wisdom = pc1.Wisdom;
            ch.Constitution = pc1.Constitution;
            ch.Charisma = pc1.Charisma;
            ch.strengthAdd = pc1.strengthAdd;
            ch.dexterityAdd = pc1.dexterityAdd;

            ch.mace = pc1.mace;
            ch.bow = pc1.bow;
            ch.flail = pc1.flail;
            ch.dagger = pc1.dagger;
            ch.rapier = pc1.rapier;
            ch.twoHanded = pc1.twoHanded;
            ch.staff = pc1.staff;
            ch.shuriken = pc1.shuriken;
            ch.sword = pc1.sword;
            ch.threestaff = pc1.threestaff;
            ch.halberd = pc1.halberd;
            ch.unarmed = pc1.unarmed;
            ch.thievery = pc1.thievery;
            ch.magic = pc1.magic;
            ch.bash = pc1.bash;

            (ch as PC).highMace = pc1.highMace;
            (ch as PC).highBow = pc1.highBow;
            (ch as PC).highFlail = pc1.highFlail;
            (ch as PC).highDagger = pc1.highDagger;
            (ch as PC).highRapier = pc1.highRapier;
            (ch as PC).highTwoHanded = pc1.highTwoHanded;
            (ch as PC).highStaff = pc1.highStaff;
            (ch as PC).highShuriken = pc1.highShuriken;
            (ch as PC).highSword = pc1.highSword;
            (ch as PC).highThreestaff = pc1.highThreestaff;
            (ch as PC).highHalberd = pc1.highHalberd;
            (ch as PC).highUnarmed = pc1.highUnarmed;
            (ch as PC).highThievery = pc1.highThievery;
            (ch as PC).highMagic = pc1.highMagic;
            (ch as PC).highBash = pc1.highBash;

            (ch as PC).trainedMace = pc1.trainedMace;
            (ch as PC).trainedBow = pc1.trainedBow;
            (ch as PC).trainedFlail = pc1.trainedFlail;
            (ch as PC).trainedDagger = pc1.trainedDagger;
            (ch as PC).trainedRapier = pc1.trainedRapier;
            (ch as PC).trainedTwoHanded = pc1.trainedTwoHanded;
            (ch as PC).trainedStaff = pc1.trainedStaff;
            (ch as PC).trainedShuriken = pc1.trainedShuriken;
            (ch as PC).trainedSword = pc1.trainedSword;
            (ch as PC).trainedThreestaff = pc1.trainedThreestaff;
            (ch as PC).trainedHalberd = pc1.trainedHalberd;
            (ch as PC).trainedUnarmed = pc1.trainedUnarmed;
            (ch as PC).trainedThievery = pc1.trainedThievery;
            (ch as PC).trainedMagic = pc1.trainedMagic;
            (ch as PC).trainedBash = pc1.trainedBash;

            (ch as PC).birthday = pc1.birthday;
            (ch as PC).lastOnline = pc1.lastOnline;

            ch.Shielding = pc1.Shielding;

            #region Resistances
            ch.FireResistance = pc1.FireResistance;
            ch.ColdResistance = pc1.ColdResistance;
            ch.LightningResistance = pc1.LightningResistance;
            ch.DeathResistance = pc1.DeathResistance;
            ch.BlindResistance = pc1.BlindResistance;
            ch.FearResistance = pc1.FearResistance;
            ch.StunResistance = pc1.StunResistance;
            ch.PoisonResistance = pc1.PoisonResistance;
            ch.ZonkResistance = pc1.ZonkResistance;
            #endregion

            #region Protections
            ch.AcidProtection = pc1.AcidProtection;
            ch.FireProtection = pc1.FireProtection;
            ch.ColdProtection = pc1.ColdProtection;
            ch.LightningProtection = pc1.LightningProtection;
            ch.DeathProtection = pc1.DeathProtection;
            ch.PoisonProtection = pc1.PoisonProtection;
            #endregion

            ch.RightHand = pc1.RightHand;

            ch.LeftHand = pc1.LeftHand;

            //Underworld specific
            (ch as PC).UW_hitsMax = pc1.UW_hitsMax;
            (ch as PC).UW_staminaMax = pc1.UW_staminaMax;
            (ch as PC).UW_manaMax = pc1.UW_manaMax;
            (ch as PC).UW_hasIntestines = pc1.UW_hasIntestines;
            (ch as PC).UW_hasLiver = pc1.UW_hasLiver;
            (ch as PC).UW_hasLungs = pc1.UW_hasLungs;
            (ch as PC).UW_hasStomach = pc1.UW_hasStomach;

            //Player vs. Player
            (ch as PC).currentKarma = pc1.currentKarma;
            (ch as PC).lifetimeKarma = pc1.lifetimeKarma;
            (ch as PC).currentMarks = Account.GetCurrentMarks((ch as PC).Account.accountID);
            (ch as PC).lifetimeMarks = pc1.lifetimeMarks;
            (ch as PC).pvpNumDeaths = pc1.pvpNumDeaths;
            (ch as PC).pvpNumKills = pc1.pvpNumKills;

            //ch.Timeout = Character.INACTIVITY_TIMEOUT;
            ch.spellDictionary = pc1.spellDictionary;
            ch.wearing = pc1.wearing;
            ch.sackList = pc1.sackList;
            ch.pouchList = pc1.pouchList;
            ch.lockerList = pc1.lockerList;
            ch.pouchList = pc1.pouchList;
            ch.beltList = pc1.beltList;
            ch.RightRing1 = pc1.RightRing1;
            ch.RightRing2 = pc1.RightRing2;
            ch.RightRing3 = pc1.RightRing3;
            ch.RightRing4 = pc1.RightRing4;
            ch.LeftRing1 = pc1.LeftRing1;
            ch.LeftRing2 = pc1.LeftRing2;
            ch.LeftRing3 = pc1.LeftRing3;
            ch.LeftRing4 = pc1.LeftRing4;

            //reset temp stats
            ch.TempStrength = 0;
            ch.TempDexterity = 0;
            ch.TempIntelligence = 0;
            ch.TempWisdom = 0;
            ch.TempConstitution = 0;
            ch.TempCharisma = 0;

            ch.EffectsList = new System.Collections.Concurrent.ConcurrentDictionary<Effect.EffectTypes, Effect>();
            ch.WornEffectsList = new List<Effect>();

            foreach (Effect effect in pc1.EffectsList.Values)
                Effect.CreateCharacterEffect(effect.EffectType, effect.Power, ch, effect.Duration, null);


            // store a knight's relevant stats before adding ring effects (knight ring)
            int savedKnightMana = 0;
            int savedKnightManaMax = 0;
            if (ch.BaseProfession == ClassType.Knight || ch.BaseProfession == ClassType.Ravager)
            {
                savedKnightMana = ch.Mana;
                savedKnightManaMax = ch.ManaMax;
            }

            #region Add Held Item Effects
            if (ch.RightHand != null && ch.RightHand.effectType.Length > 0 && ch.RightHand.wearLocation == Globals.eWearLocation.None)
            {
                Effect.AddWornEffectToCharacter(ch, ch.RightHand);
            }
            if (ch.LeftHand != null && ch.LeftHand.effectType.Length > 0 && ch.LeftHand.wearLocation == Globals.eWearLocation.None)
            {
                Effect.AddWornEffectToCharacter(ch, ch.LeftHand);
            }
            #endregion

            #region Add Ring Effects
            if (ch.RightRing1 != null && ch.RightRing1.effectType.Length > 0)
            {
                Effect.AddWornEffectToCharacter(ch, ch.RightRing1);
            }
            if (ch.RightRing2 != null && ch.RightRing2.effectType.Length > 0)
            {
                Effect.AddWornEffectToCharacter(ch, ch.RightRing2);
            }
            if (ch.RightRing3 != null && ch.RightRing3.effectType.Length > 0)
            {
                Effect.AddWornEffectToCharacter(ch, ch.RightRing3);
            }
            if (ch.RightRing4 != null && ch.RightRing4.effectType.Length > 0)
            {
                Effect.AddWornEffectToCharacter(ch, ch.RightRing4);
            }

            if (ch.LeftRing1 != null && ch.LeftRing1.effectType.Length > 0)
            {
                Effect.AddWornEffectToCharacter(ch, ch.LeftRing1);
            }
            if (ch.LeftRing2 != null && ch.LeftRing2.effectType.Length > 0)
            {
                Effect.AddWornEffectToCharacter(ch, ch.LeftRing2);
            }
            if (ch.LeftRing3 != null && ch.LeftRing3.effectType.Length > 0)
            {
                Effect.AddWornEffectToCharacter(ch, ch.LeftRing3);
            }
            if (ch.LeftRing4 != null && ch.LeftRing4.effectType.Length > 0)
            {
                Effect.AddWornEffectToCharacter(ch, ch.LeftRing4);
            }
            #endregion

            // revert a knight's related stats after adding ring effects
            if (ch.BaseProfession == ClassType.Knight || ch.BaseProfession == ClassType.Ravager)
            {
                if (ch.HasKnightRing)
                {
                    ch.Mana = savedKnightMana;
                    ch.ManaMax = savedKnightManaMax;
                }
                else
                {
                    ch.Mana = 0;
                    ch.ManaMax = 0;
                }
            }

            #region Add Inventory Effects
            foreach (Item wItem in ch.wearing)
            {
                try
                {
                    if (wItem.effectType.Length > 0)
                    {
                        Effect.AddWornEffectToCharacter(ch, wItem);
                    }
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
            }
            #endregion

            ch.QuestList = pc1.QuestList;
            ch.QuestFlags = pc1.QuestFlags;
            ch.ContentFlags = pc1.ContentFlags;

            ch.encumbrance = ch.GetEncumbrance();
            ch.talentsDictionary = pc1.talentsDictionary;

            (ch as PC).Mailbox = pc1.Mailbox;

            (ch as PC).IsPossessed = true;
        }

        public static void LoadCharacter(Character ch, PC pc1) // load character
        {
            ch.UniqueID = pc1.UniqueID;
            ch.Name = pc1.Name;
            ch.gender = pc1.gender;
            ch.race = pc1.race;
            ch.BaseProfession = pc1.BaseProfession;
            ch.classFullName = pc1.classFullName;
            ch.BaseProfession = pc1.BaseProfession;
            ch.visualKey = pc1.visualKey;
            ch.Alignment = pc1.Alignment;
            if (ch is PC)
            {
                if (ch.PCState != Globals.ePlayerState.CONFERENCE)
                    (ch as PC).confRoom = pc1.confRoom;
                (ch as PC).ImpLevel = pc1.ImpLevel;
                (ch as PC).IsImmortal = pc1.IsImmortal;
                (ch as PC).friendsList = pc1.friendsList;
                (ch as PC).friendNotify = pc1.friendNotify;
                (ch as PC).ignoreList = pc1.ignoreList;
                (ch as PC).receivePages = pc1.receivePages;
                (ch as PC).receiveTells = pc1.receiveTells;
                (ch as PC).filterProfanity = pc1.filterProfanity;
                (ch as PC).showStaffTitle = pc1.showStaffTitle;
                (ch as PC).macros = pc1.macros;
                (ch as PC).DisplayCombatDamage = pc1.DisplayCombatDamage;
                (ch as PC).IsAncestor = pc1.IsAncestor;
                (ch as PC).AncestorID = pc1.AncestorID;
                (ch as PC).echo = pc1.echo;
                (ch as PC).IsAnonymous = pc1.IsAnonymous;
                (ch as PC).RoundsPlayed = pc1.RoundsPlayed;
                (ch as PC).bankGold = pc1.bankGold;

                (ch as PC).highMace = pc1.highMace;
                (ch as PC).highBow = pc1.highBow;
                (ch as PC).highFlail = pc1.highFlail;
                (ch as PC).highDagger = pc1.highDagger;
                (ch as PC).highRapier = pc1.highRapier;
                (ch as PC).highTwoHanded = pc1.highTwoHanded;
                (ch as PC).highStaff = pc1.highStaff;
                (ch as PC).highShuriken = pc1.highShuriken;
                (ch as PC).highSword = pc1.highSword;
                (ch as PC).highThreestaff = pc1.highThreestaff;
                (ch as PC).highHalberd = pc1.highHalberd;
                (ch as PC).highUnarmed = pc1.highUnarmed;
                (ch as PC).highThievery = pc1.highThievery;
                (ch as PC).highMagic = pc1.highMagic;
                (ch as PC).highBash = pc1.highBash;

                (ch as PC).trainedMace = pc1.trainedMace;
                (ch as PC).trainedBow = pc1.trainedBow;
                (ch as PC).trainedFlail = pc1.trainedFlail;
                (ch as PC).trainedDagger = pc1.trainedDagger;
                (ch as PC).trainedRapier = pc1.trainedRapier;
                (ch as PC).trainedTwoHanded = pc1.trainedTwoHanded;
                (ch as PC).trainedStaff = pc1.trainedStaff;
                (ch as PC).trainedShuriken = pc1.trainedShuriken;
                (ch as PC).trainedSword = pc1.trainedSword;
                (ch as PC).trainedThreestaff = pc1.trainedThreestaff;
                (ch as PC).trainedHalberd = pc1.trainedHalberd;
                (ch as PC).trainedUnarmed = pc1.trainedUnarmed;
                (ch as PC).trainedThievery = pc1.trainedThievery;
                (ch as PC).trainedMagic = pc1.trainedMagic;
                (ch as PC).trainedBash = pc1.trainedBash;

                (ch as PC).birthday = pc1.birthday;
                (ch as PC).lastOnline = pc1.lastOnline;

                //Underworld specific
                (ch as PC).UW_hitsMax = pc1.UW_hitsMax;
                (ch as PC).UW_staminaMax = pc1.UW_staminaMax;
                (ch as PC).UW_manaMax = pc1.UW_manaMax;
                (ch as PC).UW_hasIntestines = pc1.UW_hasIntestines;
                (ch as PC).UW_hasLiver = pc1.UW_hasLiver;
                (ch as PC).UW_hasLungs = pc1.UW_hasLungs;
                (ch as PC).UW_hasStomach = pc1.UW_hasStomach;

                //Player vs. Player
                (ch as PC).currentKarma = pc1.currentKarma;
                (ch as PC).lifetimeKarma = pc1.lifetimeKarma;
                (ch as PC).currentMarks = Account.GetCurrentMarks((ch as PC).Account.accountID);
                (ch as PC).lifetimeMarks = pc1.lifetimeMarks;
                (ch as PC).pvpNumDeaths = pc1.pvpNumDeaths;
                (ch as PC).pvpNumKills = pc1.pvpNumKills;

            }
            ch.LandID = pc1.LandID;
            ch.MapID = pc1.MapID;
            ch.X = pc1.X;
            ch.Y = pc1.Y;
            ch.CurrentCell = Cell.GetCell(pc1.FacetID, pc1.LandID, pc1.MapID, pc1.X, pc1.Y, pc1.Z);
            ch.dirPointer = pc1.dirPointer;
            ch.Stunned = pc1.Stunned;
            ch.floating = pc1.floating;
            ch.IsDead = pc1.IsDead;
            ch.IsHidden = pc1.IsHidden;
            ch.IsInvisible = pc1.IsInvisible;
            ch.Level = pc1.Level;
            ch.Experience = pc1.Experience;
            ch.Hits = pc1.Hits;
            ch.HitsAdjustment = pc1.HitsAdjustment;
            ch.HitsMax = pc1.HitsMax;
            ch.HitsDoctored = pc1.HitsDoctored;
            ch.StaminaAdjustment = pc1.StaminaAdjustment;
            ch.StaminaMax = pc1.StaminaMax;
            ch.Stamina = pc1.Stamina;
            ch.Mana = pc1.Mana;
            ch.ManaAdjustment = pc1.ManaAdjustment;
            ch.ManaMax = pc1.ManaMax;
            ch.hitsRegen = pc1.hitsRegen;
            ch.manaRegen = pc1.manaRegen;
            ch.staminaRegen = pc1.staminaRegen;
            ch.Age = pc1.Age;

            ch.Deaths = pc1.Deaths;
            ch.Kills = pc1.Kills;
            ch.Strength = pc1.Strength;
            ch.Dexterity = pc1.Dexterity;
            ch.Intelligence = pc1.Intelligence;
            ch.Wisdom = pc1.Wisdom;
            ch.Constitution = pc1.Constitution;
            ch.Charisma = pc1.Charisma;
            ch.strengthAdd = pc1.strengthAdd;
            ch.dexterityAdd = pc1.dexterityAdd;

            ch.mace = pc1.mace;
            ch.bow = pc1.bow;
            ch.flail = pc1.flail;
            ch.dagger = pc1.dagger;
            ch.rapier = pc1.rapier;
            ch.twoHanded = pc1.twoHanded;
            ch.staff = pc1.staff;
            ch.shuriken = pc1.shuriken;
            ch.sword = pc1.sword;
            ch.threestaff = pc1.threestaff;
            ch.halberd = pc1.halberd;
            ch.unarmed = pc1.unarmed;
            ch.thievery = pc1.thievery;
            ch.magic = pc1.magic;
            ch.bash = pc1.bash;

            ch.Shielding = pc1.Shielding;

            #region Resistances
            ch.FireResistance = pc1.FireResistance;
            ch.ColdResistance = pc1.ColdResistance;
            ch.LightningResistance = pc1.LightningResistance;
            ch.DeathResistance = pc1.DeathResistance;
            ch.BlindResistance = pc1.BlindResistance;
            ch.FearResistance = pc1.FearResistance;
            ch.StunResistance = pc1.StunResistance;
            ch.PoisonResistance = pc1.PoisonResistance;
            ch.ZonkResistance = pc1.ZonkResistance;
            #endregion

            #region Protections
            ch.AcidProtection = pc1.AcidProtection;
            ch.FireProtection = pc1.FireProtection;
            ch.ColdProtection = pc1.ColdProtection;
            ch.LightningProtection = pc1.LightningProtection;
            ch.DeathProtection = pc1.DeathProtection;
            ch.PoisonProtection = pc1.PoisonProtection;
            #endregion

            ch.RightHand = pc1.RightHand;

            ch.LeftHand = pc1.LeftHand;

            //ch.Timeout = Character.INACTIVITY_TIMEOUT;
            ch.spellDictionary = pc1.spellDictionary;
            ch.wearing = pc1.wearing;
            ch.sackList = pc1.sackList;
            ch.lockerList = pc1.lockerList;
            ch.beltList = pc1.beltList;
            ch.pouchList = pc1.pouchList;
            ch.RightRing1 = pc1.RightRing1;
            ch.RightRing2 = pc1.RightRing2;
            ch.RightRing3 = pc1.RightRing3;
            ch.RightRing4 = pc1.RightRing4;
            ch.LeftRing1 = pc1.LeftRing1;
            ch.LeftRing2 = pc1.LeftRing2;
            ch.LeftRing3 = pc1.LeftRing3;
            ch.LeftRing4 = pc1.LeftRing4;

            //reset temp stats
            ch.TempStrength = 0;
            ch.TempDexterity = 0;
            ch.TempIntelligence = 0;
            ch.TempWisdom = 0;
            ch.TempConstitution = 0;
            ch.TempCharisma = 0;

            ch.EffectsList = new System.Collections.Concurrent.ConcurrentDictionary<Effect.EffectTypes, Effect>();
            ch.WornEffectsList = new List<Effect>();

            foreach (Effect effect in pc1.EffectsList.Values)
                Effect.CreateCharacterEffect(effect.EffectType, effect.Power, ch, effect.Duration, null);


            // store a knight's relevant stats before adding ring effects (knight ring)
            int savedKnightMana = 0;
            int savedKnightManaMax = 0;
            if (ch.BaseProfession == ClassType.Knight || ch.BaseProfession == ClassType.Ravager)
            {
                savedKnightMana = ch.Mana;
                savedKnightManaMax = ch.ManaMax;
            }

            #region Add Held Item Effects
            if (ch.RightHand != null && ch.RightHand.effectType.Length > 0 && ch.RightHand.wearLocation == Globals.eWearLocation.None)
            {
                Effect.AddWornEffectToCharacter(ch, ch.RightHand);
            }
            if (ch.LeftHand != null && ch.LeftHand.effectType.Length > 0 && ch.LeftHand.wearLocation == Globals.eWearLocation.None)
            {
                Effect.AddWornEffectToCharacter(ch, ch.LeftHand);
            }
            #endregion

            #region Add Ring Effects
            if (ch.RightRing1 != null && ch.RightRing1.effectType.Length > 0)
            {
                Effect.AddWornEffectToCharacter(ch, ch.RightRing1);
            }
            if (ch.RightRing2 != null && ch.RightRing2.effectType.Length > 0)
            {
                Effect.AddWornEffectToCharacter(ch, ch.RightRing2);
            }
            if (ch.RightRing3 != null && ch.RightRing3.effectType.Length > 0)
            {
                Effect.AddWornEffectToCharacter(ch, ch.RightRing3);
            }
            if (ch.RightRing4 != null && ch.RightRing4.effectType.Length > 0)
            {
                Effect.AddWornEffectToCharacter(ch, ch.RightRing4);
            }

            if (ch.LeftRing1 != null && ch.LeftRing1.effectType.Length > 0)
            {
                Effect.AddWornEffectToCharacter(ch, ch.LeftRing1);
            }
            if (ch.LeftRing2 != null && ch.LeftRing2.effectType.Length > 0)
            {
                Effect.AddWornEffectToCharacter(ch, ch.LeftRing2);
            }
            if (ch.LeftRing3 != null && ch.LeftRing3.effectType.Length > 0)
            {
                Effect.AddWornEffectToCharacter(ch, ch.LeftRing3);
            }
            if (ch.LeftRing4 != null && ch.LeftRing4.effectType.Length > 0)
            {
                Effect.AddWornEffectToCharacter(ch, ch.LeftRing4);
            }
            #endregion

            // revert a knight's related stats after adding ring effects
            if (ch.BaseProfession == ClassType.Knight || ch.BaseProfession == ClassType.Ravager)
            {
                if (ch.HasKnightRing)
                {
                    ch.Mana = savedKnightMana;
                    ch.ManaMax = savedKnightManaMax;
                }
                else
                {
                    ch.Mana = 0;
                    ch.ManaMax = 0;
                }
            }

            #region Add Inventory Effects
            foreach (Item wItem in ch.wearing)
            {
                try
                {
                    if (wItem.effectType.Length > 0)
                    {
                        Effect.AddWornEffectToCharacter(ch, wItem);
                    }
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
            }
            #endregion

            ch.QuestList = pc1.QuestList;
            ch.QuestFlags = pc1.QuestFlags;
            ch.ContentFlags = pc1.ContentFlags;
            ch.talentsDictionary = pc1.talentsDictionary;

            if(ch.Pets != null && ch.Pets.Count > 0)
            {
                foreach(NPC npc in new List<NPC>(ch.Pets))
                    AI.Command_Begone(npc, ch);
            }

            ch.encumbrance = ch.GetEncumbrance();

            if (ch is PC)
                (ch as PC).Mailbox = pc1.Mailbox;
        }

        public static bool SwitchCharacter(PC ch, int id)
        {
            PC pc = PC.GetPC(id);

            if (pc != null && pc.Account.accountID == ch.Account.accountID)
            {
                PC.LoadCharacter(ch, pc);
                //Protocol.UpdateUserLists();
                return true;
            }

            return false;
        }

        public static bool SelectNewCharacter(PC ch, int charnum) // select a new character
        {
            string[] playerlist = DAL.DBPlayer.GetCharacterList("playerID", ch.Account.accountID);
            int id = Convert.ToInt32(playerlist[charnum - 1]);

            PC pc = PC.GetPC(id); // get new character from the database
            if (pc != null && pc.UniqueID != ch.UniqueID)
            {
                PC.LoadCharacter(ch, pc);
                return true;
            }
            else return false;
        }

        public static bool PlayerExists(string playerName) // return if player name exists in the Player table
        {
            return DAL.DBPlayer.PlayerExists(playerName);
        }

        public static string FormatCharacterList(string[] characterList, bool protocol, Character currentCharacter)
        {
            string list = "";
            int i;
            //string[] characterList = DAL.DBPlayer.GetCharacterList("name", accountID);
            i = 1;
            list = "Character Listing\n\r";
            list += "-----------------\n\r";
            foreach (String name in characterList)
            {
                if (name == null) break;
                list += "\n\r" + i + ". " + name;
                i++;
            }
            return list;
        }

        public static PC GetPC(int playerID) // return a PC
        {
            return DAL.DBPlayer.GetPCByID(playerID);
        }

        /// <summary>
        /// Get a player who is online.
        /// </summary>
        /// <param name="playerID">Player's unique ID.</param>
        /// <returns>The PC object.</returns>
        public static PC GetOnline(int playerID) // return a PC that is online
        {
            foreach (Character chr in Character.MenuList)
            {
                if (chr.UniqueID == playerID) { return (PC)chr; }
            }
            foreach (Character chr in Character.ConfList)
            {
                if (chr.UniqueID == playerID) { return (PC)chr; }
            }
            foreach (Character chr in Character.PCInGameWorld)
            {
                if (chr.UniqueID == playerID) { return (PC)chr; }
            }
            return null;
        }

        public static bool IsOnline(int playerID)
        {
            foreach (Character chr in Character.MenuList)
                if (chr.UniqueID == playerID) return true;

            foreach (Character chr in Character.ConfList)
                if (chr.UniqueID == playerID) return true;

            foreach (Character chr in Character.PCInGameWorld)
                if (chr.UniqueID == playerID) return true;

            return false;
        }

        /// <summary>
        /// Gets a player online by player name.
        /// </summary>
        /// <param name="playerName">The player's name.</param>
        /// <returns>The PC object.</returns>
        public static PC GetOnline(string playerName) // return a PC that is online
        {
            foreach (Character chr in Character.MenuList)
            {
                if (chr.Name.ToLower() == playerName.ToLower()) { return (PC)chr; }
            }
            foreach (Character chr in Character.ConfList)
            {
                if (chr.Name.ToLower() == playerName.ToLower()) { return (PC)chr; }
            }
            foreach (Character chr in Character.PCInGameWorld)
            {
                if (chr.Name.ToLower() == playerName.ToLower()) { return (PC)chr; }
            }
            return null;
        }

        public static string GetName(int playerID) // return player name
        {
            return (string)DAL.DBPlayer.GetPlayerField(playerID, "name", Type.GetType("System.String"));
        }

        public static int GetPlayerID(string playerName)
        {
            return DAL.DBPlayer.GetPlayerID(playerName);
        }
        #endregion

        public override void RoundEvent()
        {
            if (!this.IsPC) return;

            this.CommandsProcessed.Clear();

            if (Character.PCInGameWorld.Contains(this))
            {
                #region Player

                if (this.CurrentCell != null)
                {
                    this.Map.UpdateCellVisible(this.CurrentCell);
                }

                Character.ValidatePlayer(this);

                // If the character is dead...
                if (IsDead)
                {
                    #region Character is dead
                    // Confirm that stats are at 0.
                    this.Hits = 0;
                    this.Mana = 0;
                    this.Stamina = 0;

                    if (!this.CurrentCell.ContainsPlayerCorpse(this.Name))
                    {
                        // Check if the corpse is being carried (to a priest).
                        foreach (PC chold in new List<PC>(Character.PCInGameWorld))
                        {
                            if (chold.RightHand != null && chold.RightHand.itemType == Globals.eItemType.Corpse &&
                                chold.RightHand.special == this.Name)
                            {
                                this.CurrentCell = chold.CurrentCell;
                                this.corpseIsCarried = true;
                            }
                            else if (chold.LeftHand != null && chold.LeftHand.itemType == Globals.eItemType.Corpse &&
                                chold.LeftHand.special == this.Name)
                            {
                                this.CurrentCell = chold.CurrentCell;
                                this.corpseIsCarried = true;
                            }
                        }

                        // When corpses are destroyed by fire or other spells it is an automatic Ghod-rez, so this shouldn't be a problem...
                        if (!this.corpseIsCarried)
                            Corpse.MakeCorpse(this);
                    }
                    #endregion
                }
                else corpseIsCarried = false;

                CommandWeight = 0; // reset the number of commands entered
                InitiativeModifier = 0;

                if (!InUnderworld && !IsDead) // age the character if they are not in the Underworld and not dead
                {
                    Age++;
                    Rules.DoAgingEffect(this);
                }

                RoundsPlayed++; // add to the total roundsPlayed
                RoundsPlayedSinceAddedToLottery++;

                // for every hour played chances increase to win the lottery
                if (ImpLevel == Globals.eImpLevel.USER && Utils.RoundsToTimeSpan(RoundsPlayedSinceAddedToLottery) >= TimeSpan.FromHours(1))
                {
                    Land land = Land;

                    if (Land != null)
                    {
                        Land.LotteryParticipants.Add(UniqueID);
                        DAL.DBWorld.SaveLottery(Land);
                        //this.WriteToDisplay("You have been added to " + land.LongDesc + " list of lottery participants. Good luck!");
                        this.RoundsPlayedSinceAddedToLottery = 0;
                    }
                }

                if (DragonsSpineMain.Instance.Settings.SkillLossOverTime && !IsDead && !InUnderworld && RoundsPlayed % Globals.SKILL_LOSS_DIVISOR == 0)
                    Skills.SkillLossOverTime(this);

                if (preppedSpell != null)
                {
                    if (DragonsSpineMain.GameRound - PreppedRound > 1)
                    {
                        Mana--;
                        updateMP = true;
                        if (Mana <= 0)
                        {
                            Mana = 0;
                            preppedSpell = null;
                            WriteToDisplay("Your spell has been lost.");
                        }
                    }
                }

                if (Stunned == 0 && !IsFeared)
                {
                    IO.ProcessCommands(this);

                    int hitsGain = 0;
                    int staminaGain = 0;
                    int manaGain = 0;

                    // If has effect Juvenis (currently skill level 17 Druid spell), gains regeneration as if resting and meditating.
                    // Resting. Not diseased. Less than 3 rounds since last damage.
                    if ((IsResting || HasEffect(Effect.EffectTypes.Juvenis)) && !EffectsList.ContainsKey(Effect.EffectTypes.Contagion) &&  DamageRound < DragonsSpineMain.GameRound - 3)
                    {
                        #region Regenerate hits, stamina and mana.
                        if (this.Hits < this.HitsFull)  // increase stats
                        {
                            hitsGain++;
                            hitsGain += this.hitsRegen;
                        }

                        if (this.Stamina < this.StaminaFull)
                        {
                            staminaGain++;
                            staminaGain += this.staminaRegen;
                        }

                        if (this.Mana < this.ManaFull)
                        {
                            manaGain++;
                            manaGain += this.manaRegen;
                        }
                        #endregion
                    }

                    // more regeneration if meditating
                    if ((IsMeditating || HasEffect(Effect.EffectTypes.Juvenis)) && !EffectsList.ContainsKey(Effect.EffectTypes.Contagion) && DamageRound < DragonsSpineMain.GameRound - 3)
                    {
                        #region Regenerate hits, stamina and mana.
                        if (this.Hits < this.HitsFull)  // increase stats
                        {
                            hitsGain++;
                            hitsGain += this.hitsRegen;
                        }

                        if (this.Stamina < this.StaminaFull)
                        {
                            staminaGain++;
                            staminaGain += this.staminaRegen;
                        }

                        if (this.Mana < this.ManaFull)
                        {
                            manaGain++;
                            manaGain += this.manaRegen;
                        }
                        #endregion
                    }

                    // Confirm we're not going over full. (Full = Adjustment + Max)
                    if (hitsGain > 0)
                    {
                        if (Hits + hitsGain > HitsFull)
                            Hits = HitsFull;
                        else Hits += hitsGain;
                    }

                    if (staminaGain > 0)
                    {
                        if (Stamina + staminaGain > StaminaFull)
                            Stamina = StaminaFull;
                        else Stamina += staminaGain;
                    }

                    if (manaGain > 0)
                    {
                        if (Mana + manaGain > ManaFull)
                            Mana = ManaFull;
                        else Mana += manaGain;
                    }

                    #region Follow Mode for Players (including Developers)
                    if (this.IsPC)
                    {
                        if (this.FollowID != 0)
                        {
                            foreach (CommandTasker.CommandType cmd in this.CommandsProcessed)
                            {
                                if (CommandTasker.BreakFollowCommands.Contains(cmd))
                                {
                                    this.BreakFollowMode();
                                    break;
                                }
                            }
                        }
                        else if (this.FollowID != 0 && this.LastCommand != "chase")//mlt chase command work
                        // mlt we will want a check for follower and followee on ea others friends list for follow command
                        //so a player can follow a hidden friend,but not use for pvp
                        {

                            Character target = GameSystems.Targeting.TargetAquisition.FindTargetInView(this, this.FollowID, false, true);

                            if (target == null)
                            {
                                this.BreakFollowMode();
                            }
                            else if (target.IsDead)
                            {
                                this.BreakFollowMode();
                            }
                            else if (this.CurrentCell != null && target.CurrentCell != null && this.CurrentCell != target.CurrentCell)
                            {
                                if (this.ImpLevel >= Globals.eImpLevel.DEV)
                                {
                                    this.CurrentCell = target.CurrentCell;
                                    this.Timeout = Character.INACTIVITY_TIMEOUT;
                                }
                                else
                                {
                                    PathTest pathTest = new PathTest(PathTest.RESERVED_NAME_COMMANDSUFFIX, this.CurrentCell);

                                    if (!pathTest.SuccessfulPathTest(target.CurrentCell))
                                    {
                                        // Work done 4/11/2013. -Eb
                                        PathTest follower = new PathTest(PathTest.RESERVED_NAME_COMMANDSUFFIX, this.CurrentCell);
                                        follower.AIGotoXYZ(target.CurrentCell.X, target.CurrentCell.Y, target.CurrentCell.Z);
                                        this.CurrentCell = follower.CurrentCell;
                                        follower.RemoveFromWorld();
                                    }
                                    else
                                    {
                                        this.CurrentCell = target.CurrentCell;
                                    }

                                    pathTest.RemoveFromWorld();

                                    this.Timeout = Character.INACTIVITY_TIMEOUT;
                                }
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    if (Stunned > 0)
                    {
                        Stunned -= 1;
                        while (InputCommandQueueCount() != 0)
                            InputCommandQueueDequeue();  // empty the command buffer when stunned.
                    }
                    else if (IsFeared)
                    {
                        if (IsBlind) NPC.AIRandomlyMoveCharacter(this);
                        else
                        {
                            Character fearer = this.EffectsList[Effect.EffectTypes.Fear].Caster;

                            if (fearer != null && fearer.CurrentCell != null)
                            {
                                AI.BackAwayFromCell(this, fearer.CurrentCell);
                            }
                            else if (this.CurrentCell != null)
                            {
                                AI.BackAwayFromCell(this, this.CurrentCell);
                            }
                            else NPC.AIRandomlyMoveCharacter(this);
                        }

                        EmitSound(Sound.GetCommonSound(Sound.CommonSound.Feared));
                    }
                }

                NumAttackers = 0; // reset the number of attackers after commands are processed

                // update the visible cells
                if (CurrentCell != null)
                    //this.CurrentCell.DoSwitch(this.CurrentCell);
                    //if (this.CurrentCell.LootDraw)
                    Map.UpdateCellVisible(this.CurrentCell);

                if (protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                    ProtocolYuusha.ShowMap(this);
                else if (protocol == "old-kesmai")
                    CurrentCell.ShowMapOldKesProto(this);
                else if(protocol == "Proto")
                {
                    
                }
                else if (CurrentCell != null)
                    CurrentCell.ShowMap(this);

                if (CurrentCell != null)
                {
                    #region Check current cell for effects / water.
                    string tile = "";
                    if (CurrentCell.AreaEffects.Count > 0)
                    {
                        tile = CurrentCell.DisplayGraphic;
                    }
                    else
                    {
                        tile = CurrentCell.CellGraphic;
                    }

                    // always reset floating here
                    if (!tile.Equals(Cell.GRAPHIC_WATER))
                    {
                        floating = 4;
                    }
                    else if (tile.Equals(Cell.GRAPHIC_WATER) && !CanBreatheWater && !CanFly && !IsDead && !IsImmortal && !IsUndead && !Cell.IsNextToLand(CurrentCell))
                    {
                        if (floating >= 4)
                        {
                            floating -= 1;
                        }
                        else if (floating == 3)
                        {
                            floating -= 1;
                            WriteToDisplay("You are sinking below the surface!");
                        }
                        else if (floating == 2)
                        {
                            floating -= 1;
                            WriteToDisplay("You are submerged!");
                        }
                        else if (floating == 1)
                        {
                            floating -= 1;
                            WriteToDisplay("You are submerged under water and cannot breathe!");

                        }
                        else if (floating < 1)
                        {
                            WriteToDisplay("You have drowned!");
                            SendToAllInSight(GetNameForActionResult(false) + " has drowned to death.");
                            if (!IsDead)
                            {
                                Rules.DoDeath(this, null);
                            }
                        }
                        else
                        {
                            floating = 3; // just in case.
                        }
                    }
                    #endregion
                }

                #endregion
            }
        }

        protected override void ThirdRoundEvent(object obj, ElapsedEventArgs e)
        {
            if (PCState != Globals.ePlayerState.PLAYING)
                return;

            base.ThirdRoundEvent(obj, e);
        }

        public void AddInput(byte[] buf, int length) // add characters to a pc's input buffer
        {
            if (inputPos + length >= MAX_INPUT_LENGTH) //too many chars, it will overrun our inputBuffer
            {
                return;
            }

            for (int a = 0; a < length; a++)
            {
                if (buf[a] == '\r' || buf[a] == '\0' || buf[a] == '\n')
                {
                    if (inputPos > 0)
                    {
                        string str = Encoding.ASCII.GetString(inputBuffer, 0, inputPos);

                        inputCommandQueue.Enqueue(str); //add it to the command queue
//#if DEBUG
//                        Utils.Log("InputCommandQueueCount: " + InputCommandQueueCount(), Utils.LogType.Debug);
//#endif

                        if (str.ToLower() != "a" && str.ToLower() != "ag" && str.ToLower() != "aga" && str.ToLower() != "agai" && str.ToLower() != "again")
                        {
                            LastCommand = str;
                        }
                    }
                    inputPos = 0; //start over in the inputBuffer
                    continue;
                }

                if (inputPos > 0 && inputBuffer[inputPos - 1] == ' ' && buf[a] == ' ')
                    continue; //in case they put extra spaces in a command

                // handle backspaces
                if (buf[a] == 8)
                {
                    if (inputPos > 0)
                        inputPos--;
                }
                else inputBuffer[inputPos++] = buf[a]; //add this char to the inputBuffer
            }

            if (this.echo && this.PCState == Globals.ePlayerState.PLAYING)
                EchoBytes(buf, length);
        }

        public void ClearDisplay()
        {
            int ctr;
            if (!this.IsPC)
            {
                return;
            }

            for (ctr = DISPLAY_BUFFER_SIZE - 1; ctr > 0; ctr--)
                DisplayBuffer[ctr] = "";

            DisplayBuffer[0] = "";
            DisplayText = "";
            for (ctr = 1; ctr < DISPLAY_BUFFER_SIZE; ctr++)
                DisplayText += "";
        }

        public int OutputQueueCount()
        {
            try
            {
                return outputQueue.Count;
            }
            catch (Exception e)
            {
                Utils.LogException(e);

                return 0;
            }
        }

        public string OutputQueueDequeue()
        {
            try
            {
                string a = outputQueue[0];
                outputQueue.RemoveAt(0);
                return a;
                //return outputQueue.Dequeue();
            }
            catch(Exception e)
            {
                Utils.LogException(e);
                string a = outputQueue[0];
                outputQueue.RemoveAt(0);
                return a;
                //return outputQueue.Dequeue();
            }
            finally
            {
                //outputQueue.Clear();
            }
        }

        public void ClearOutputQueue()
        {
            outputQueue.Clear();
        }

        public string GetInputBuffer()
        {
            return Encoding.ASCII.GetString(inputBuffer, 0, inputPos);
        }

        public void EchoBytes(byte[] b, int count)
        {
            socket.Send(b, count, 0);
        }

        public void Save() // save entire PC
        {
            if (this.IsPossessed || this.IsImage || this.IsWizardEye || this.IsPeeking) return; // do not save
            //return;
            try
            {
                var pc = (PC)this.MemberwiseClone();

                // store a knight's relevant stats before adding worn effects (knight ring)
                var saveKnightMana = -1;
                var saveKnightManaMax = -1;

                if (pc.HasKnightRing)
                {
                    saveKnightMana = pc.Mana;
                    saveKnightManaMax = pc.ManaMax;
                }

                //revert a knight's related stats back for player save
                if (saveKnightMana != -1 && saveKnightManaMax != -1)
                {
                    pc.Mana = saveKnightMana;
                    pc.ManaMax = saveKnightManaMax;
                }

                // Now save all the stats and arraylists
                try
                {
                    DAL.DBPlayer.SavePlayerStats(pc);
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }

                if (pc.IsNewPC)
                {
                    pc.UniqueID = DAL.DBPlayer.GetPlayerID(pc.Name);
                    pc.savePlayerSpells = true;
                }

                try
                {
                    DAL.DBPlayer.SavePlayerSettings(pc); // save settings
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
                try
                {
                    DAL.DBPlayer.SavePlayerSkills(pc); // save skills
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
                try
                {
                    DAL.DBPlayer.SavePlayerHeld(pc); // save held items
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
                try
                {
                    DAL.DBPlayer.SavePlayerPouch(pc); // save pouch items
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
                try
                {
                    DAL.DBPlayer.SavePlayerSack(pc); // save sack items
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
                try
                {
                    DAL.DBPlayer.SavePlayerWearing(pc); // save worn items
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
                try
                {
                    DAL.DBPlayer.SavePlayerLocker(pc); // save locker items
                }
                catch(Exception e)
                {
                    Utils.LogException(e);
                }
                try
                {
                    DAL.DBPlayer.SavePlayerBelt(pc); // save belt items
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
                try
                {
                    DAL.DBPlayer.SavePlayerRings(pc); // save rings worn
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }

                if (pc.savePlayerSpells)
                {
                    try
                    {
                        DAL.DBPlayer.SavePlayerSpells(pc); // save spells known
                        pc.savePlayerSpells = false;
                    }
                    catch (Exception e)
                    {
                        Utils.LogException(e);
                    }
                }

                try
                {
                    DAL.DBPlayer.SavePlayerEffects(pc); // save non item effects
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
                try
                {
                    DAL.DBPlayer.SavePlayerQuests(pc); // save player quests
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
                try
                {
                    DAL.DBPlayer.SavePlayerFlags(pc); // save player flags (quests flags, content flags)
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }

                pc.IsNewPC = false;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public override void Write(string message) // sends a text message to the player
        {
            switch (protocol)
            {
                case "Proto":
                    ProtoClientIO.WriteLine(Account.accountName, message);
                    return;
                default:
                    break;
            }

            //outputQueue.Enqueue(message);
            outputQueue.Add(message);
        }

        public override void WriteLine(string message, ProtocolYuusha.TextType textType) // write a line with a carriage return, include protocol
        {
            try
            {
                message = message + "\r\n"; // add a carriage return line feed to message

                switch (this.protocol)
                {
                    case "Proto":
                        ProtoClientIO.WriteLine(Account.accountName, message);
                        return;
                    default:
                        break;
                }

                if (protocol != DragonsSpineMain.Instance.Settings.DefaultProtocol || PCState == Globals.ePlayerState.PLAYING) // if not using the protocol send the message
                {
                    if (PCState == Globals.ePlayerState.PLAYING)
                    {
                        WriteToDisplay(message);
                    }
                    else
                    {
                        Write(message);
                    }
                    return;
                }

                // Format the protocol message.
                message = ProtocolYuusha.GetTextProtocolString(textType, true) + message + ProtocolYuusha.GetTextProtocolString(textType, false);

                // Send the protocol message.
                Write(message);

            }
            catch (Exception e)
            {
                Write(message);
                Utils.LogException(e);
            }
        }

        public override void WriteLine(string message) // write a line with a carriage return line feed, exclude protocol
        {
            Write(message + "\r\n");
        }

        public override void WriteToDisplay(string message, ProtocolYuusha.TextType textType) // write a line with a carriage return, include protocol
        {
            try
            {
                message = message + "\r\n"; // add a carriage return line feed to message
                switch (this.protocol)
                {
                    case "Proto":
                        ProtoClientIO.WriteLine(Account.accountName, message);
                        return;
                    default:
                        break;
                }
                if (this.protocol != DragonsSpineMain.Instance.Settings.DefaultProtocol || this.PCState == Globals.ePlayerState.PLAYING) // if not using the protocol send the message
                {
                    if (this.PCState == Globals.ePlayerState.PLAYING)
                        this.WriteToDisplay(message);
                    else
                        this.Write(message);
                    return;
                }

                message = ProtocolYuusha.GetTextProtocolString(textType, true) + message + ProtocolYuusha.GetTextProtocolString(textType, false);
                this.Write(message); // send the protocol message

            }
            catch (Exception e)
            {
                this.Write(message);
                Utils.LogException(e);
            }
        }

        public override void WriteToDisplay(string message)
        {
            base.WriteToDisplay(message);

            // filter message by replacing strings
            try
            {
                if (message.Contains("%"))
                {
                    message = FilterDisplayText(message);
                }
            }
            catch (StackOverflowException)
            {
                Utils.Log("Message = " + message, Utils.LogType.Unknown);
            }

            // if player is not playing then use WriteLine method
            if (this.PCState != Globals.ePlayerState.PLAYING)
            {
                this.WriteLine(message, ProtocolYuusha.TextType.Status);
                return;
            }

            message = this.DisplayGameRound == true ? "[" + DragonsSpineMain.GameRound.ToString() + "] " + message : message;
            switch (this.protocol)
            {
                case "Proto":
                    ProtoClientIO.WriteLine(Account.accountName, message);
                    return;
                default:
                    break;
            }
            if (this.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
            {
                this.Write(ProtocolYuusha.GAME_TEXT + message + ProtocolYuusha.GAME_TEXT_END);
                return;
            }

            int ctr;

            for (ctr = DISPLAY_BUFFER_SIZE - 1; ctr > 0; ctr--)
                DisplayBuffer[ctr] = DisplayBuffer[ctr - 1];
            //message = "[" + DragonsSpineMain.GameRound.ToString() + "] " + message;
            DisplayBuffer[0] = message + "\n\r";
            DisplayText = message + "\n\r";
            for (ctr = 1; ctr < DISPLAY_BUFFER_SIZE; ctr++)
                DisplayText = DisplayBuffer[ctr] + DisplayText;

        }

        public void SendToAllInConferenceRoom(string text, ProtocolYuusha.TextType textType)
        {
            for (int a = 0; a < Character.ConfList.Count; a++)
            {
                PC chr = ConfList[a] as PC;

                if (chr.PCState == Globals.ePlayerState.CONFERENCE)
                {
                    if (this.confRoom == chr.confRoom && Array.IndexOf((chr as PC).ignoreList, this.UniqueID) == -1) // in same room and not ignored
                    {
                        switch (textType)
                        {
                            case ProtocolYuusha.TextType.Enter:
                                if (this.Name == chr.Name) { break; } // do not send enter messages to the player that is entering
                                chr.WriteLine(text, ProtocolYuusha.TextType.Enter); break;
                            case ProtocolYuusha.TextType.Exit:
                                if (this.Name == chr.Name) { break; } // do not send exit messages to the player that is exiting
                                chr.WriteLine(text, ProtocolYuusha.TextType.Exit); break;
                            case ProtocolYuusha.TextType.PlayerChat:
                                if (chr.filterProfanity) { text = Conference.FilterProfanity(text); } // send the text through the filter
                                chr.WriteLine(text, ProtocolYuusha.TextType.PlayerChat);
                                break;
                            case ProtocolYuusha.TextType.Status:
                                chr.WriteLine(text, ProtocolYuusha.TextType.Status); break;
                            case ProtocolYuusha.TextType.System:
                                chr.WriteLine(text, ProtocolYuusha.TextType.System); break;
                        } // end switch
                    } // end if
                } // end if
            } // end foreach
        }

        public long GetTrainedSkillExperience(Globals.eSkillType skillType)
        {
            switch (skillType)
            {
                case Globals.eSkillType.Bow:
                    return this.trainedBow;
                case Globals.eSkillType.Sword:
                    return this.trainedSword;
                case Globals.eSkillType.Two_Handed:
                    return this.trainedTwoHanded;
                case Globals.eSkillType.Unarmed:
                    return this.trainedUnarmed;
                case Globals.eSkillType.Staff:
                    return this.trainedStaff;
                case Globals.eSkillType.Dagger:
                    return this.trainedDagger;
                case Globals.eSkillType.Polearm:
                    return this.trainedHalberd;
                case Globals.eSkillType.Rapier:
                    return this.trainedRapier;
                case Globals.eSkillType.Shuriken:
                    return this.trainedShuriken;
                case Globals.eSkillType.Magic:
                    return this.trainedMagic;
                case Globals.eSkillType.Mace:
                    return this.trainedMace;
                case Globals.eSkillType.Flail:
                    return this.trainedFlail;
                case Globals.eSkillType.Threestaff:
                    return this.trainedThreestaff;
                case Globals.eSkillType.Thievery:
                    return this.trainedThievery;
                case Globals.eSkillType.Bash:
                    return this.trainedBash;
                default:
                    return -1;
            }
        }

        public void SetTrainedSkillExperience(Globals.eSkillType skillType, long amount)
        {
            switch (skillType)
            {
                case Globals.eSkillType.Bow:
                    this.trainedBow = amount;
                    break;
                case Globals.eSkillType.Sword:
                    this.trainedSword = amount;
                    break;
                case Globals.eSkillType.Two_Handed:
                    this.trainedTwoHanded = amount;
                    break;
                case Globals.eSkillType.Unarmed:
                    this.trainedUnarmed = amount;
                    break;
                case Globals.eSkillType.Staff:
                    this.trainedStaff = amount;
                    break;
                case Globals.eSkillType.Dagger:
                    this.trainedDagger = amount;
                    break;
                case Globals.eSkillType.Polearm:
                    this.trainedHalberd = amount;
                    break;
                case Globals.eSkillType.Rapier:
                    this.trainedRapier = amount;
                    break;
                case Globals.eSkillType.Shuriken:
                    this.trainedShuriken = amount;
                    break;
                case Globals.eSkillType.Magic:
                    this.trainedMagic = amount;
                    break;
                case Globals.eSkillType.Mace:
                    this.trainedMace = amount;
                    break;
                case Globals.eSkillType.Flail:
                    this.trainedFlail = amount;
                    break;
                case Globals.eSkillType.Threestaff:
                    this.trainedThreestaff = amount;
                    break;
                case Globals.eSkillType.Thievery:
                    this.trainedThievery = amount;
                    break;
                case Globals.eSkillType.Bash:
                    this.trainedBash = amount;
                    break;
            }
        }

        public long GetHighSkillExperience(Globals.eSkillType skillType)
        {
            switch (skillType)
            {
                case Globals.eSkillType.Bow:
                    return this.highBow;
                case Globals.eSkillType.Sword:
                    return this.highSword;
                case Globals.eSkillType.Two_Handed:
                    return this.highTwoHanded;
                case Globals.eSkillType.Unarmed:
                    return this.highUnarmed;
                case Globals.eSkillType.Staff:
                    return this.highStaff;
                case Globals.eSkillType.Dagger:
                    return this.highDagger;
                case Globals.eSkillType.Polearm:
                    return this.highHalberd;
                case Globals.eSkillType.Rapier:
                    return this.highRapier;
                case Globals.eSkillType.Shuriken:
                    return this.highShuriken;
                case Globals.eSkillType.Magic:
                    return this.highMagic;
                case Globals.eSkillType.Mace:
                    return this.highMace;
                case Globals.eSkillType.Flail:
                    return this.highFlail;
                case Globals.eSkillType.Threestaff:
                    return this.highThreestaff;
                case Globals.eSkillType.Thievery:
                    return this.highThievery;
                case Globals.eSkillType.Bash:
                    return this.highBash;
                default:
                    return -1;
            }
        }

        public static void SetCharacterVisualKey(Character ch)
        {
            if (ch is PC)
            {
                if (ch.IsPeeking)
                    ch.visualKey = "";
                else if (ch.IsDead)
                    ch.visualKey = "ghost";
                else if (ch.IsWizardEye || ch.IsPolymorphed || ch.IsShapeshifted)
                    ch.visualKey = ch.Name.ToLower();
                else if (string.IsNullOrEmpty(ch.visualKeyOverride))
                    ch.visualKey = ch.gender.ToString().ToLower() + "_" + ch.BaseProfession.ToString().ToLower() + "_pc_" + (ch as PC).colorChoice.ToLower();
                else ch.visualKey = ch.visualKeyOverride;
            }
            else if(ch is NPC && string.IsNullOrEmpty(ch.visualKey))
            {
                if (!string.IsNullOrEmpty(ch.visualKeyOverride))
                    ch.visualKey = ch.visualKeyOverride;
                else
                {
                    if (ch.IsWizardEye || ch.IsPolymorphed || ch.IsShapeshifted)
                    {
                        ch.visualKey = ch.Name.ToLower();
                    }
                    else if ((ch as NPC).HasRandomName || Autonomy.EntityBuilding.EntityLists.IsHuman(ch as NPC))
                    {
                        ch.visualKey = ch.gender.ToString().ToLower() + "_" + ch.BaseProfession.ToString().ToLower() + "_npc_";
                        List<string> colorOptions = new List<string>() { "red", "gray", "green", "purple", "yellow", "brown" };
                        ch.visualKey += colorOptions[new Random(Guid.NewGuid().GetHashCode()).Next(0, colorOptions.Count)];
                        return;
                    }
                    else ch.visualKey = ch.Name.ToLower().Replace(".", "_");

                    // By default genderless or male. If female, and not ALWAYS female (nymphs, sprites, dryads etc)
                    if (Autonomy.EntityBuilding.EntityLists.IsHumanOrHumanoid(ch as NPC) &&
                        ch.gender == Globals.eGender.Female && !Autonomy.EntityBuilding.EntityLists.FEMALE.Contains(ch.entity))
                        ch.visualKey = ch.visualKey + "_female";
                }
            }
        }

        public void AddToLogin()
        {
            foreach (PC ch in Character.LoginList)
            {
                if (ch.Account.ipAddress == this.Account.ipAddress)
                {
                    ch.RemoveFromLogin();
                }
            }
            IO.AddToLogin.Add(this);
            IO.pplToAddToLogin = true;
        }

        public void RemoveFromLogin()
        {
            IO.RemoveFromLogin.Add(this);
            IO.pplToRemoveFromLogin = true;
        }

        public void AddToCharGen()
        {
            IO.AddToCharGen.Add(this);
            IO.pplToAddToCharGen = true;
        }

        public void RemoveFromCharGen()
        {
            IO.RemoveFromCharGen.Add(this);
            IO.pplToRemoveFromCharGen = true;
        }

        public void AddToMenu()
        {
            IO.AddToMenu.Add(this);
            IO.pplToAddToMenu = true;
        }

        public void RemoveFromMenu()
        {
            IO.RemoveFromMenu.Add(this);
            IO.pplToRemoveFromMenu = true;
        }

        public void AddToConf()
        {
            IO.AddToConf.Add(this);
            IO.pplToAddToConf = true;
        }
        public void RemoveFromConf()
        {
            IO.RemoveFromConf.Add(this);
            IO.pplToRemoveFromConf = true;
        }

        public override string GetLogString()
        {
            try
            {
                return "(PC) [ID: " + this.UniqueID + "] " + this.Name + " [" + Utils.FormatEnumString(this.Alignment.ToString()) + " " + Utils.FormatEnumString(this.BaseProfession.ToString()) + "(" + this.Level + ")] (" + (this as PC).Account.accountName + ") (" +
                            (this.CurrentCell != null ? this.CurrentCell.GetLogString(false) : "Current Cell = null") + ")";
            }
            catch (Exception e)
            {
                Utils.LogException(e);

                return "(PC) [Exception in GetLogString()]";
            }
        }
    }
}