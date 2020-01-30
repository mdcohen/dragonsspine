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
using System.Collections;
using System.Collections.Generic;

namespace DragonsSpine.GameWorld
{
    /// <summary>
    /// This class is used for every map location in the game
    /// the whole idea is to keep this as small as we can since
    /// there will be thousands of these things
    /// </summary>
    public class Cell
    {
        #region Cell graphic constants
        public const string GRAPHIC_WATER = "~~";
        public const string GRAPHIC_AIR = "%%";
        public const string GRAPHIC_WEB = "ww";
        public const string GRAPHIC_DARKNESS = "??";
        public const string GRAPHIC_CLOSED_DOOR_HORIZONTAL = "--";
        public const string GRAPHIC_OPEN_DOOR_HORIZONTAL = "\\ ";
        public const string GRAPHIC_CLOSED_DOOR_VERTICAL = "| ";
        public const string GRAPHIC_OPEN_DOOR_VERTICAL = "/ ";
        public const string GRAPHIC_ICE = "~.";
        public const string GRAPHIC_ICE_WALL = "~,";
        public const string GRAPHIC_FIRE = "**";
        public const string GRAPHIC_FOG = "FF";
        public const string GRAPHIC_WALL = "[]";
        public const string GRAPHIC_WALL_IMPENETRABLE = "DD";
        public const string GRAPHIC_MOUNTAIN = "/\\";
        public const string GRAPHIC_FOREST_IMPASSABLE = "TT";
        public const string GRAPHIC_SECRET_DOOR = "SD";
        public const string GRAPHIC_SECRET_MOUNTAIN = "SM";
        public const string GRAPHIC_LOCKED_DOOR_HORIZONTAL = "HD";
        public const string GRAPHIC_LOCKED_DOOR_VERTICAL = "VD";
        public const string GRAPHIC_COUNTER = "==";
        public const string GRAPHIC_COUNTER_PLACEABLE = "CC";
        public const string GRAPHIC_BOXING_RING = ")(";
        public const string GRAPHIC_ALTAR = "mm";
        public const string GRAPHIC_ALTAR_PLACEABLE = "MM";
        public const string GRAPHIC_REEF = "WW";
        public const string GRAPHIC_GRATE = "##";
        public const string GRAPHIC_EMPTY = ". ";
        public const string GRAPHIC_RUINS_LEFT = "_]";
        public const string GRAPHIC_RUINS_RIGHT = "[_";
        public const string GRAPHIC_SAND = ".\\";
        public const string GRAPHIC_FOREST_LEFT = "@ ";
        public const string GRAPHIC_FOREST_RIGHT = " @";
        public const string GRAPHIC_FOREST_FULL = "@@";
        public const string GRAPHIC_BRIDGE = "::";

        public const string GRAPHIC_FOREST_BURNT_LEFT = "t ";
        public const string GRAPHIC_FOREST_BURNT_RIGHT = " t";
        public const string GRAPHIC_FOREST_BURNT_FULL = "tt";

        public const string GRAPHIC_FOREST_FROSTY_LEFT = "f ";
        public const string GRAPHIC_FOREST_FROSTY_RIGHT = " f";
        public const string GRAPHIC_FOREST_FROSTY_FULL = "ff";

        public const string GRAPHIC_GRASS_THICK = "\"\"";
        public const string GRAPHIC_GRASS_LIGHT = "''";
        public const string GRAPHIC_GRASS_FROZEN = ", ";

        public const string GRAPHIC_UPSTAIRS = "up";
        public const string GRAPHIC_DOWNSTAIRS = "dn";
        public const string GRAPHIC_TRASHCAN = "o ";
        public const string GRAPHIC_UP_AND_DOWNSTAIRS = "ud";

        public const string GRAPHIC_LIGHTNING_STORM = "++";
        public const string GRAPHIC_POISON_CLOUD = ":%";
        public const string GRAPHIC_TEMPEST = "t%";
        public const string GRAPHIC_ICE_STORM = "`,";
        public const string GRAPHIC_ACID_STORM = "``";
        public const string GRAPHIC_WHIRLWIND = "%;";
        public const string GRAPHIC_LOCUST_SWARM = "-.";

        public const string GRAPHIC_BARREN_LEFT = ", ";
        public const string GRAPHIC_BARREN_RIGHT = " ,";
        public const string GRAPHIC_BARREN_FULL = ",,";

        public const string GRAPHIC_LOOT_SYMBOL = "$";
        #endregion

        //public struct Point3D
        //{
        //    public Point3D(string x, string y, string z)
        //    {
        //        this.x = Convert.ToInt32(x);
        //        this.y = Convert.ToInt32(y);
        //        this.z = Convert.ToInt32(z);
        //    }

        //    public Point3D(int x, int y, int z)
        //    {
        //        this.x = x;
        //        this.y = y;
        //        this.z = z;
        //    }

        //    public override string ToString()
        //    {
        //        return this.x + "," + this.y + "," + this.z;
        //    }

        //    public override int GetHashCode()
        //    {
        //        unchecked
        //        {
        //            int hash = 17;
        //            hash = hash * 23 + this.x.GetHashCode();
        //            hash = hash * 23 + this.y.GetHashCode();
        //            hash = hash * 23 + this.z.GetHashCode();
        //            return hash;
        //        }
        //    }

        //    public override bool Equals(object obj)
        //    {
        //        if (!(obj is Point3D)) return false;

        //        Point3D xyz = (Point3D)obj;

        //        return this.x == xyz.x && this.y == xyz.y && this.z == xyz.z;
        //    }

        //    private int x;
        //    private int y;
        //    private int z;
        //}
        public enum TrapType
        {
            None,
            Concussion
        }

        #region Private Data
        int facet = 0;
        int land = 0; // 16 bits
        int map = 0; // 16 bits
        int xcord = 0; // 16 bits
        int ycord = 0; // 16 bits
        int zcord = 0; // 32 bits

        TrapType trapType = TrapType.None;
        int trapPower = 0; // mlt amount of concussion damage from trap,concussion trap work
        Character trapOwner = null;

        bool isIllusioned = false; // to avoid exploits

        Segue segue = null;
        bool alwaysDark = false; // true if cell has permanent darkness
        bool ancestorFinish = false;	// true if cell allows completion of ancestoring process
        bool ancestorStart = false; // true if cell begins ancestoring process
        string cellGraphic = "  ";
        string description = ""; // text description shown to players when entering cell TODO: add description off/on toggle
        string displayGraphic = "  ";
        bool lair = false; // true if the janitor will ignore this cell
        bool lockedHorizontalDoor = false;
        bool lockedVerticalDoor = false;
        bool locker = false; // true if this is a lockers cell
        bool ornicLocker = false; // Ornic, technologically advanced locker (may deposit / withdraw coins)
        bool magicDead = false; // true if magic does not work
        bool mapPortal = false;	// true if this is a map portal cell
        bool mirror = false; // true if this cell has a mirror
        bool scribing = false;
        bool noRecall = false; // true if cell does not allow setting recall or recall from
        bool outOfBounds = false; // true if this cell was marked out of bounds during map load
        bool pvpEnabled = false; // true if PvP enabled (no mark or karma penalties)
        bool secretDoor = false; // true if this cell is a secret door
        bool singleCustomer = false;	// true if only one player is allowed in this cell at a time
        bool teleport = false; // true if this is a teleport cell (uses this cell's CellLock for access)
        bool underworldPortal = false; // true if cell allows access to the Underworld
        bool withinTownLimits = false;	// true if within town limits (terrain spells cast here by lawfuls make you neutral)
        bool balmFountain = false;
        bool outdoors = false; // true if is outdoor
        bool mailbox = false; // true if player can send and receive mail from this cell
        bool stairsUp = false;
        bool stairsDown = false;
        List<int> szList = new List<int>();
        private Dictionary<Effect.EffectTypes, AreaEffect> m_areaEffectsDict = new Dictionary<Effect.EffectTypes, AreaEffect>();
        private System.Collections.Concurrent.ConcurrentDictionary<int, Character> m_characters = new System.Collections.Concurrent.ConcurrentDictionary<int, Character>();
        //private List<Character> m_charactersList = new List<Character>(); // collection of Character objects in this cell
        private List<Item> m_itemsList = new List<Item>(); // collection of Item objects in this cell
        //private List<GameObjects.GameObject> m_objectList = new List<DragonsSpine.GameObjects.GameObject>();
        #endregion

        #region Public Properties
        public int FacetID
        {
            get { return this.facet; }
            set { this.facet = value; }
        }
        public Facet Facet
        {
            get
            {
                return World.GetFacetByID(this.FacetID);
            }
        }
        public int LandID
        {
            get { return this.land; }
            set { this.land = value; }
        }
        public Land Land
        {
            get
            {
                return World.GetFacetByID(this.FacetID).GetLandByID(this.LandID);
            }
        }
        public int MapID
        {
            get { return this.map; }
            set { this.map = value; }
        }
        public Map Map
        {
            get
            {
                return World.GetFacetByID(this.FacetID).GetLandByID(this.LandID).GetMapByID(this.MapID);
            }
        }
        public int X
        {
            get { return this.xcord; }
            set { this.xcord = value; }
        }
        public int Y
        {
            get { return this.ycord; }
            set { this.ycord = value; }
        }
        public int Z
        {
            get { return this.zcord; }
            set { this.zcord = value; }
        }
        public TrapType TrapSet
        {
            get { return this.trapType; }
            set { this.trapType = value; }
        }
        public int TrapPower//mlt concussion trap work
        {
            get { return this.trapPower; }
            set { this.trapPower = value; }
        }
        public Character TrapOwner
        {
            get { return this.trapOwner; }
            set { this.trapOwner = value; }
        }

        public bool IsIllusioned
        {
            get { return this.isIllusioned; }
            set { this.isIllusioned = value; }
        }
        
        public string CellGraphic
        {
            get { return this.cellGraphic; }
            set { this.cellGraphic = value; }
        }
        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }
        public string DisplayGraphic
        {
            get { return this.displayGraphic; }
            set
            {
                this.displayGraphic = value;
                if (DragonsSpineMain.ServerStatus == DragonsSpineMain.ServerState.Running)
                    this.Map.UpdateCellVisible(this);
            }
        }
        public bool HasScribingCrystal
        {
            get { return this.scribing; }
            set { this.scribing = value; }
        }
        public bool HasMirror
        {
            get { return this.mirror; }
            set { this.mirror = value; }
        }
        public bool IsAlwaysDark
        {
            get { return this.alwaysDark; }
            set { this.alwaysDark = value; }
        }
        public bool IsAncestorFinish
        {
            get { return this.ancestorFinish; }
            set { this.ancestorFinish = value; }
        }
        public bool IsAncestorStart
        {
            get { return this.ancestorStart; }
            set { this.ancestorStart = value; }
        }
        public bool IsDownSegue
        {
            get
            {
                return this.IsStairsUp || this.IsOneHandClimbUp || this.IsTwoHandClimbUp;
            }
        }
        public bool IsLair
        {
            get { return this.lair; }
            set { this.lair = value; }
        }
        public bool IsLockedHorizontalDoor
        {
            get { return this.lockedHorizontalDoor; }
            set { this.lockedHorizontalDoor = value; }
        }
        public bool IsLockedVerticalDoor
        {
            get { return this.lockedVerticalDoor; }
            set { this.lockedVerticalDoor = value; }
        }
        public bool IsLocker
        {
            get { return this.locker; }
            set { this.locker = value; }
        }
        public bool IsOrnicLocker
        {
            get { return this.ornicLocker; }
            set { this.ornicLocker = value; }
        }
        public bool IsBalmFountain
        {
            get { return this.balmFountain; }
            set { this.balmFountain = value; }
        }
        public bool IsMagicDead
        {
            get { return this.magicDead; }
            set { this.magicDead = value; }
        }
        public bool IsMapPortal
        {
            get { return this.mapPortal; }
            set { this.mapPortal = value; }
        }
        public bool IsNoRecall
        {
            get { return this.noRecall; }
            set { this.noRecall = value; }
        }
        public bool IsOpenDoor
        {
            get
            {
                if (this.DisplayGraphic == GRAPHIC_OPEN_DOOR_HORIZONTAL || this.CellGraphic == GRAPHIC_OPEN_DOOR_HORIZONTAL)
                {
                    return true;
                }
                else if (this.DisplayGraphic == GRAPHIC_OPEN_DOOR_VERTICAL || this.CellGraphic == GRAPHIC_OPEN_DOOR_VERTICAL)
                {
                    return true;
                }
                return false;
            }
        }
        public bool IsOutOfBounds
        {
            get { return this.outOfBounds; }
            set { this.outOfBounds = value; }
        }
        public bool IsPVPEnabled
        {
            get { return this.pvpEnabled; }
            set { this.pvpEnabled = value; }
        }
        public bool IsSecretDoor
        {
            get { return this.secretDoor; }
            set { this.secretDoor = value; }
        }
        public bool IsSingleCustomer
        {
            get { return this.singleCustomer; }
            set { this.singleCustomer = value; }
        }
        public bool IsTeleport
        {
            get { return this.teleport; }
            set { this.teleport = value; }
        }
        public bool IsUnderworldPortal
        {
            get { return this.underworldPortal; }
            set { this.underworldPortal = value; }
        }
        public bool IsUpSegue
        {
            get
            {
                return this.IsStairsDown || this.IsOneHandClimbDown || this.IsTwoHandClimbDown;
            }
        }
        public bool IsWithinTownLimits
        {
            get { return this.withinTownLimits; }
            set { this.withinTownLimits = value; }
        }
        public bool IsBoxingRing
        {
            get { return this.CellGraphic == Map.RESERVED_GRAPHIC_BOXING_RING || this.CellGraphic == Cell.GRAPHIC_BOXING_RING;  }
        }
        public Segue Segue
        {
            get { return this.segue; }
            set { this.segue = value; }
        }
        public bool IsOutdoors
        {
            get { return outdoors; }
            set { outdoors = value; }
        }
        public bool HasMailbox
        {
            get { return mailbox; }
            set { mailbox = value; }
        }
        public bool IsStairsUp
        {
            get { return stairsUp; }
            set { stairsUp = value; }
        }
        public bool IsStairsDown
        {
            get { return stairsDown; }
            set { stairsDown = value; }
        }
        public Dictionary<Effect.EffectTypes, AreaEffect> AreaEffects
        {
            get { return m_areaEffectsDict; }
        }
        //public List<Character> Characters
        //{
        //    get { return m_charactersList; }
        //}
        public System.Collections.Concurrent.ConcurrentDictionary<int, Character> Characters
        {
            get { return m_characters; }
        }
        public List<Item> Items
        {
            get { return m_itemsList; }
        }
        public bool IsBarren // balm berries do not grow in barren cells
        {
            get
            {
                switch (this.displayGraphic)
                {
                    case GRAPHIC_BARREN_FULL:
                    case GRAPHIC_BARREN_LEFT:
                    case GRAPHIC_BARREN_RIGHT:
                        return true;
                    default:
                        break;
                }

                switch (this.cellGraphic)
                {
                    case GRAPHIC_BARREN_FULL:
                    case GRAPHIC_BARREN_LEFT:
                    case GRAPHIC_BARREN_RIGHT:
                        return true;
                    default:
                        break;
                }

                return false;
            }
        }
        #endregion

        public CellLock cellLock = new CellLock(); // string matching key needed to access cell

        private static object lockObject = new object();

        #region Constructors (3)
        public Cell(int facetID, int landID, int mapID, int x, int y, int z, string graphic)
        {
            this.LandID = landID;
            this.MapID = mapID;
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.CellGraphic = graphic;
            this.DisplayGraphic = graphic;

            // Tab characters in rapid map building.
            if (graphic.StartsWith(" ") || graphic.Length > 2 || graphic.Contains("\t"))
            {
                Utils.Log("(CELL) Land: " + landID + " Map: " + mapID + " X: " + x + " Y: " + y + " Z: " + z + " contains illegal cell graphic: '" + graphic + "'", Utils.LogType.Posterity);
                if (graphic.StartsWith(" "))
                {
                    this.cellGraphic = Cell.GRAPHIC_MOUNTAIN;
                    this.DisplayGraphic = Cell.GRAPHIC_MOUNTAIN;
                    Utils.Log("(CELL) Land: " + landID + " Map: " + mapID + " X: " + x + " Y: " + y + " Z: " + z + " contains illegal cell graphic: '" + graphic + "' -- Was changed to a mountain cell. Note this is also logged in the Posterity Log.", Utils.LogType.SystemWarning);
                }
                else this.IsOutOfBounds = true;
            }
        }

        public Cell(System.Data.DataRow dr)
        {
            this.X = Convert.ToInt32(dr["xCord"]);
            this.Y = Convert.ToInt32(dr["yCord"]);
            this.Z = Convert.ToInt32(dr["zCord"]);
            if (dr["segue"].ToString() != "")
            {
                string[] segueInfo = dr["segue"].ToString().Split("|".ToCharArray());
                // Land, Map, X, Y, Z, Height
                this.segue = new Segue(Convert.ToInt16(segueInfo[0]), Convert.ToInt16(segueInfo[1]), Convert.ToInt16(segueInfo[2]),
                    Convert.ToInt16(segueInfo[3]), Convert.ToInt32(segueInfo[4]), Convert.ToInt32(segueInfo[5]));
            }
            this.description = dr["description"].ToString();
            if (dr["lock"].ToString() == "")
            {
                this.cellLock = new Cell.CellLock();
            }
            else
            {
                this.cellLock = new Cell.CellLock(dr["lock"].ToString());
            }
            this.mapPortal = Convert.ToBoolean(dr["portal"]);
            this.teleport = Convert.ToBoolean(dr["teleport"]);
            this.singleCustomer = Convert.ToBoolean(dr["singleCustomer"]);
            this.pvpEnabled = Convert.ToBoolean(dr["pvpEnabled"]);
            this.mailbox = Convert.ToBoolean(dr["mailbox"]);
        } 
        #endregion

        public class CellLock // a class that restricts movement into a Cell object
        {
            public enum LockType { None, Door, ClassType, DailyCycle, LunarCycle, Level, Key, Flag, Alignment }
            public string CellLockString = "";

            #region Constructors (2)
            public CellLock()
            {

            }

            // lock types | class types | daily cycles | lunar cycles | level requirements
            // EXAMPLE DATA: Alignment|-1|-1|-1|-1|-1||False|False|As both of your feet settle on the platform it begins to vibrate. Suddenly you are thrown up and to the west, sailing through the air for several seconds before your descent slows and you splash into salty ocean water.|An unseen force shoves you off the platform.|failx|faily|fail effect|Evil|failz
            public CellLock(string cellLockInfo)
            {
                this.CellLockString = cellLockInfo;

                int a;
                string[] lockInfo = cellLockInfo.Split("|".ToCharArray());
                #region Lock Types
                string[] lockTypesArray = lockInfo[0].Split(",".ToCharArray());
                this.lockTypes = new LockType[lockTypesArray.Length];
                for (a = 0; a < lockTypesArray.Length; a++)
                {
                    this.lockTypes[a] = (LockType)Enum.Parse(typeof(LockType), lockTypesArray[a], true);
                }
                #endregion
                #region Class Types Allowed
                if (lockInfo.Length > 1 && lockInfo[1] != "-1") // -1 means all classes allowed
                {
                    string[] classesAllowed = lockInfo[1].Split(",".ToCharArray());
                    this.classTypes = new Character.ClassType[classesAllowed.Length];
                    for (a = 0; a < classesAllowed.Length; a++)
                    {
                        this.classTypes[a] = (Character.ClassType)Enum.Parse(typeof(Character.ClassType), classesAllowed[a], true);
                    }
                }
                #endregion
                #region Daily Cycles Allowed
                if (lockInfo.Length > 2 && lockInfo[2] != "-1") // -1 means all daily cycles
                {
                    string[] dailyCyclesAllowed = lockInfo[2].Split(",".ToCharArray());
                    this.dailyCycles = new World.DailyCycle[dailyCyclesAllowed.Length];
                    for (a = 0; a < dailyCyclesAllowed.Length; a++)
                    {
                        this.dailyCycles[a] = (World.DailyCycle)Enum.Parse(typeof(World.DailyCycle), dailyCyclesAllowed[a], true);
                    }
                }
                #endregion
                #region Lunar Cycles Allowed
                if (lockInfo.Length > 3 && lockInfo[3] != "-1")
                {
                    string[] lunarCyclesAllowed = lockInfo[3].Split(",".ToCharArray());
                    this.lunarCycles = new World.LunarCycle[lunarCyclesAllowed.Length];
                    for (a = 0; a < lunarCyclesAllowed.Length; a++)
                    {
                        this.lunarCycles[a] = (World.LunarCycle)Enum.Parse(typeof(World.LunarCycle), lunarCyclesAllowed[a], true);
                    }
                }
                #endregion
                #region Level Requirements
                if (lockInfo.Length > 4 && Convert.ToInt32(lockInfo[4]) > 0)
                { this.minimumLevel = Convert.ToInt32(lockInfo[4]); }
                if (lockInfo.Length > 5 && Convert.ToInt32(lockInfo[5]) > 0)
                { this.maximumLevel = Convert.ToInt32(lockInfo[5]); }
                #endregion
                #region Other Optional Requirements
                if (lockInfo.Length > 6)
                {
                    this.key = lockInfo[6]; // also used to hold flag information
                }
                if (lockInfo.Length > 7)
                {
                    this.heldKey = Convert.ToBoolean(lockInfo[7]);
                }
                if (lockInfo.Length > 8)
                {
                    this.wornKey = Convert.ToBoolean(lockInfo[8]);
                }
                if (lockInfo.Length > 9)
                {
                    this.lockSuccess = lockInfo[9];
                }
                if (lockInfo.Length > 10)
                {
                    this.lockFailureString = lockInfo[10];
                }
                if (lockInfo.Length > 11)
                {
                    this.lockFailureXCord = Convert.ToInt16(lockInfo[11]);
                }
                if (lockInfo.Length > 12)
                {
                    this.lockFailureYCord = Convert.ToInt16(lockInfo[12]);
                }
                if (lockInfo.Length > 13)
                {
                    this.lockFailureEffect = lockInfo[13];
                }
                if (lockInfo.Length > 14)
                {
                    string[] alignments = lockInfo[14].Split(",".ToCharArray());
                    this.alignmentTypes = new Globals.eAlignment[alignments.Length];
                    for (a = 0; a < alignments.Length; a++)
                    {
                        this.alignmentTypes[a] = (Globals.eAlignment)Enum.Parse(typeof(Globals.eAlignment), alignments[a], true);
                    }
                }
                if (lockInfo.Length > 15)
                {
                    this.lockFailureZCord = Convert.ToInt32(lockInfo[15]);
                }
                #endregion
            } 
            #endregion

            #region Public Data
            public LockType[] lockTypes = new LockType[] { LockType.None };
            public Character.ClassType[] classTypes = new Character.ClassType[] { Character.ClassType.None };
            public World.DailyCycle[] dailyCycles = new World.DailyCycle[] { World.DailyCycle.Morning, World.DailyCycle.Afternoon,
                World.DailyCycle.Evening, World.DailyCycle.Night };
            public World.LunarCycle[] lunarCycles = new World.LunarCycle[] { World.LunarCycle.Full, World.LunarCycle.New,
                World.LunarCycle.Waning_Crescent, World.LunarCycle.Waxing_Crescent };
            public int minimumLevel = 0;
            public int maximumLevel = 10000;
            public string key = "";
            public bool heldKey = false;
            public bool wornKey = false;
            public string lockSuccess = "";
            public string lockFailureString = "";
            public int lockFailureXCord = -1;
            public int lockFailureYCord = -1;
            public string lockFailureEffect = ""; //TODO
            public Globals.eAlignment[] alignmentTypes = new Globals.eAlignment[] { Globals.eAlignment.ChaoticEvil };
            public int lockFailureZCord = -1;
            #endregion
        }
        
        public bool balmBerry = false; // true if balm berries spawn on this cell
        public bool manaBerry = false; // true if mana berries spawn on this cell
        public bool poisonBerry = false; // true if poison berries spawn on this cell
        public bool stamBerry = false; // true if stamina berries spawn on this cell
        public bool growsSprigs = false; // true if neutralize poison springs spawn on this cell
        public int dailyFruit = 1; // the amount of berries that will drop here each game day
        public int droppedFruit = 0; // the number of times in the day fruit has been picked

        #region Climb Properties
        bool oneHandClimbDown = false;
        public bool IsOneHandClimbDown
        {
            get { return this.oneHandClimbDown; }
            set { this.oneHandClimbDown = value; }
        }
        bool oneHandClimbUp = false;
        public bool IsOneHandClimbUp
        {
            get { return this.oneHandClimbUp; }
            set { this.oneHandClimbUp = value; }
        }
        bool twoHandClimbDown = false;
        public bool IsTwoHandClimbDown
        {
            get { return this.twoHandClimbDown; }
            set { this.twoHandClimbDown = value; }
        }
        bool twoHandClimbUp = false;
        public bool IsTwoHandClimbUp
        {
            get { return this.twoHandClimbUp; }
            set { this.twoHandClimbUp = value; }
        } 
        #endregion

        public bool LootDraw
        {
            get
            {
                bool result = true;

                switch (this.DisplayGraphic)
                {
                    case GRAPHIC_FIRE:
                    case GRAPHIC_LIGHTNING_STORM:
                    case GRAPHIC_ACID_STORM:
                    case GRAPHIC_POISON_CLOUD:
                    case GRAPHIC_WEB:
                    case GRAPHIC_ICE_STORM:
                    case GRAPHIC_WHIRLWIND:
                        result = false;
                        break;
                }

                return result;
            }
        }

        public BitArray visCells = new BitArray(49, true);	// create our visible cells array, default to true

        public void Add(Character ch)
        {
            if (ch == null) return;

            if (Characters.ContainsKey(ch.UniqueID))
            {
                Utils.Log("Attempted to add Character " + ch.GetLogString() + " multiple times to Cell: " + this.GetLogString(true), Utils.LogType.SystemWarning);
                return;
            }

            lock (lockObject)
            {
                m_characters.TryAdd(ch.UniqueID, ch);
            }

            //ch.LandID = this.LandID;
        }

        public void Add(Item item)
        {
            if (item == null || (item.itemType == Globals.eItemType.Coin && item.coinValue <= 0)) return;
            if (item.special.Contains(Item.EXTRAPLANAR)) return;

            lock (m_itemsList)
            {
                if (item == null || item != null && m_itemsList.Exists(i => i != null && i.UniqueID == item.UniqueID))
                {
#if DEBUG
                    Utils.Log("Attempted to add Item " + item.GetLogString() + " multiple times to Cell: " + this.GetLogString(true), Utils.LogType.Debug);
#endif
                    return;
                }
            }

            if (item.coinValue < 0)
            {
                item.coinValue = Math.Abs(item.coinValue);
            }

            // Check if we are on a garbage can cell. Corpses can't be thrown away.
            if (CellGraphic == GRAPHIC_TRASHCAN && item.itemType != Globals.eItemType.Corpse)
            {
                World.CollectFeeForLottery(item.itemType == Globals.eItemType.Coin ? World.FEE_JANITORIAL_COIN_REMOVAL : World.FEE_JANITORIAL_ITEM_REMOVAL,
                    LandID, ref item.coinValue);
                return;
            }
            else if(CellGraphic == GRAPHIC_AIR)
            {
                #region items falling through air, currently only and all fragile items will break and poof
                if (item.fragile) return; // it broke
                //if(item is Corpse corpse)
                //{
                //    foreach(Item corpseItem in new List<Item>(corpse.Contents))
                //    {
                //        if(corpseItem.fragile)
                //    }
                //}

                Cell landingCell;
                Segue segue = null;
                int totalHeight = 0;
                int currentHeight = zcord;
                int countLoop = 0;
                int curx = xcord;
                int cury = ycord;
                int curz = zcord;
                do
                {
                    segue = Segue.GetDownSegue(GetCell(FacetID, LandID, MapID, curx, cury, curz));

                    countLoop++;

                    if (segue != null)
                    {
                        landingCell = GetCell(FacetID, segue.LandID, segue.MapID, segue.X, segue.Y, segue.Z);
                        curx = segue.X;
                        cury = segue.Y;
                        curz = segue.Z;
                        totalHeight += segue.Height;
                    }
                    else return;
                }
                while (GetCell(FacetID, segue.LandID, segue.MapID, segue.X, segue.Y, segue.Z).CellGraphic == GRAPHIC_AIR && countLoop < 100);

                // failed to get segue then karma res spot is returned -- don't drop everything there heh heh
                if (segue != null && segue != new Segue(LandID, MapID, Map.KarmaResX, Map.KarmaResY, Map.KarmaResZ, 0))
                {
                    landingCell = GetCell(FacetID, segue.LandID, segue.MapID, segue.X, segue.Y, segue.Z);

                    if (landingCell != null && landingCell.DisplayGraphic != GRAPHIC_AIR)
                    {
                        landingCell.Add(item);
                    }
                }

                return; 
                #endregion
            }

            // Override to prevent figurine duping.
            if (item.baseType == Globals.eItemBaseType.Figurine)
            {
                if(Items.Contains(item))
                {
                    Utils.Log("Possible duplication of figurine prevented. Attempted to add Item " + item.GetLogString() + " to Cell: " + this.GetLogString(true), Utils.LogType.Debug);
                    return;
                }
            }

            if (item.itemType == Globals.eItemType.Coin)
            {
                // check cell for coins that already exist
                foreach (Item addItem in Items)
                {
                    if (addItem.itemType == Globals.eItemType.Coin)
                    {
                        addItem.coinValue += item.coinValue;
                        if (addItem.coinValue > Merchant.MAX_BANK_GOLD)
                        {
                            addItem.coinValue = Merchant.MAX_BANK_GOLD;
                            Utils.Log("There was an attempt to drop coins in an amount over the set limit of " + Merchant.MAX_BANK_GOLD + ".", Utils.LogType.SystemWarning);
                            SendToAllInSight("The ground beneath your feet shakes violently.");
                            Remove(addItem);
                            return;
                        }
                        addItem.dropRound = DragonsSpineMain.GameRound;
                        return;
                    }
                }

                if (item.coinValue > Merchant.MAX_BANK_GOLD) item.coinValue = Merchant.MAX_BANK_GOLD;
            }

            item.dropRound = DragonsSpineMain.GameRound;
            item.facet = FacetID;
            item.land = LandID;
            item.map = MapID;
            item.xcord = X;
            item.ycord = Y;
            item.zcord = Z;

            m_itemsList.Add(item);
        }

        public void Add(AreaEffect effect)
        {
            if (effect == null) return;

            lock (m_areaEffectsDict)
            {
                if (!m_areaEffectsDict.ContainsKey(effect.EffectType))
                {
                    m_areaEffectsDict.Add(effect.EffectType, effect);
                }
                //else
                //{
                //    m_areaEffectsDict[effect.EffectType].Power += effect.Power;
                //    m_areaEffectsDict[effect.EffectType].Duration += effect.Duration;
                //    m_areaEffectsDict[effect.EffectType].Caster = effect.Caster;

                //    //Utils.Log("Attempted to add same Effect " + effect.EffectType.ToString() + " to Cell: " + this.GetLogString(true), Utils.LogType.SystemWarning);
                //}
            }
        }

        public void Remove(Character ch)
        {
            lock (lockObject)
            {
                m_characters.TryRemove(ch.UniqueID, out ch); 
            }
        }

        public void Remove(Item item)
        {
            m_itemsList.Remove(item);
        }

        public void Remove(Effect.EffectTypes effectType)
        {
            m_areaEffectsDict.Remove(effectType);
        }

        public bool ContainsNPCCorpse(int npcUniqueID)
        {
            foreach(Item item in Items)
            {
                if (item is Corpse && (item as Corpse).Ghost.UniqueID == npcUniqueID)
                    return true;
            }
            return false;
        }

        public bool ContainsPlayerCorpse(string playerName)
        {
            foreach (Item item in Items)
            {
                if (item.itemType == Globals.eItemType.Corpse && item.special == playerName)
                    return true;
            }
            return false;
        }

        public void ShowMapOldKesProto(PC ch)
        {
            
            string mapstring = Map.KP_PICTURE_TERRAIN_UPDATE;
            string updatestring = Map.KP_CLEAN_ACTIVE + Map.KP_PICTURE_ICON_UPDATE + ch.dirPointer;
            string[] LETTER = new string[]{"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q",
											  "R","S","T","U","V","W","X","Y","Z"};
            string[] Align = Globals.ALIGNMENT_SYMBOLS;
            int counter = 0;

            #region Client Layout
            //
            // On a redraw everything is sent
            // EX:
            // Y4  Y  JY5=Hits       : Y5J   153Y5RHits Taken   : Y5a     0Y6=Experience : Y6I8669670
            // Y6RMagic Points : Y6a    47Y7=Stamina    : Y7J    69Y6 R Y7 L Uv1!A1!A1!A1!A8!E
            // P)!1a012a2!a!!!a724127b747d747Y 9A djinnY!9A WhumperY"9A Y.Ironuzak.SeaY#9A demon
            // Y$9djinnY$fplateMTo the south you hear a toad croaking. LUv1!A1!A1!A1!A8!EPY4 >K s; l up" "!
            //
            // Otherwise only the changed items are sent.
            // EX:
            // Y4  MYou hear a toad croaking. Y6a    50LUv1!A1!A1!APx6fFeUUUY 9A djinnY @KY!9A djinn
            // Y!@KY"9A toadY"?KY4 >Kl up

            // Clear screen = ESC-Y(char)32(char)32 ESC-J (set pos 1,1 and send clear)

            // ESC-Y <l><c> = (char)31+<line/col> (char)32+(char)32 = pos 1,1
            // As you can see, ESC-Y+(char)52+(char)32 is sent every round in the begining.
            // this places the active line at 21,1
            //
            // Hits: Line is at (char)53(char)61 (53-31=22, 61-31=30 == 22,30)
            // #ofHP: Line is at (char)53(char)74 (53-31=22, 74-31=43 == 22,43) (6 chars long)
            // Hits Taken line is at (char)53(char)82 (53-31=22, 82-31=51 == 22,51)
            // #ofHP taken line is at (char)53(char)97 (53-31=22, 97-31=66 == 22,66) (6 chars long)
            // ExpText Line is at (char)54(char)61 (54-31=23, 61-31=30 == 23,30)
            // Exp# Line is at (char)54(char)73 (54-31=23, 73-31=42 == 23,42) (7 chars long)
            // MPText Line is at (char)54(char)82 (54-31=23, 82-31=51 == 23,51)
            // MP# Line is at (char)54(char)97 (54-31=23, 97-31=66 == 23,66) (6 chars long)
            // StamText Line is at (char)55(char)61 (55-31=24, 61-31=30 == 24,30)
            // Stam# Line is at (char)55(char)74 (55-31=24, 74-31=43 == 24,43) (6 chars long)
            // Rtext Line is at (char)54(char)32 (54-31=23, 32-31=1 == 23,1)
            // LText Line is at (char)55(char)32 (55-31=24, 32-31=1 == 24,1)
            // Update String: ESC-U<pointer><string><(char)127>
            // Map String: ESC-P<chars for pos 1,1 - 7,7> string is trunc'd at 7,7 --- P's
            // Contact lines (critter lines): start at (char)32(char)57 (32-31=1, 57-31=26 == 1,26)
            // col 71 for armor of mob on same cell as player
            // col 41 for right hand item
            // col 56 for left hand item
            // 
            // Message Line - ESC-L at end of all messages or on blank message
            // Set pos 21,1 ESC-Y(char)52(char)32 > ESC-K

            // Client Screen Layout
            // 0.........1.........2.........3.........4.........5.........6.........7.........8.........9 
            // 1PPPPPPPPPPPPPP.....2.....A djinn.......4.........5.........6.........7.........8.........9
            // 2PPPPPPPPPPPPPP.....2.....A Whumper.....4.........5.........6.........7.........8.........9
            // 3PPPPPPPPPPPPPP.....2.....A Y.Ironuzak.Sea........5.........6.........7.........8.........9
            // 4PPPPPPPPPPPPPP.....2.....A demon.......4.........5.........6.........7.........8.........9
            // 5PPPPPPPPPPPPPP.....2.....djinn.........4RhandItem5.....LeftHandItem..7plate....8.........9
            // 6PPPPPPPPPPPPPP.....2.........3.........4.........5.........6.........7.........8.........9
            // 7PPPPPPPPPPPPPP.....2.........3.........4.........5.........6.........7.........8.........9
            // 8.........1.........2.........3.........4.........5.........6.........7.........8.........9
            // 9.........1.........2.........3.........4.........5.........6.........7.........8.........9
            //10.........1.........2.........3.........4.........5.........6.........7.........8.........9
            //11.........1.........2.........3.........4.........5.........6.........7.........8.........9
            //12.........1.........2.........3.........4.........5.........6.........7.........8.........9
            //13.........1.........2.........3.........4.........5.........6.........7.........8.........9
            //14.........1.........2.........3.........4.........5.........6.........7.........8.........9
            //15.........1.........2.........3.........4.........5.........6.........7.........8.........9
            //16.........1.........2.........3.........4.........5.........6.........7.........8.........9
            //17.........1.........2.........3.........4.........5.........6.........7.........8.........9
            //18.........1.........2.........3.........4.........5.........6.........7.........8.........9
            //19.........1.........2.........3.........4.........5.........6.........7.........8.........9
            //20.........1.........2.........3.........4.........5.........6.........7.........8.........9
            //21> First Message Line (Active line set at each new round)...6.........7.........8.........9
            //22.........1.........2.........Hits       : ######.5Hits Taken   : ######........8.........9
            //23R .......1.........2.........Experience :#######.5Magic Points : ######........8.........9
            //24L .......1.........2.........Stamina    : ######.5.........6.........7.........8.........9
            //25.........1.........2.........3.........4.........5.........6.........7.........8.........9
            //26.........1.........2.........3.........4.........5.........6.........7.........8.........9
            //27.........1.........2.........3.........4.........5.........6.........7.........8.........9
            #endregion

            if (ch.IsPeeking)
            {
                Character target = ch.EffectsList[Effect.EffectTypes.Peek].Target;

                if (target != null && target.CurrentCell != null && ch.CurrentCell != target.CurrentCell)
                    ch.CurrentCell = target.CurrentCell;
            }

            var cellArray = Cell.GetApplicableCellArray(ch.CurrentCell, ch.GetVisibilityDistance());
            // TODO this will have to be modified to add cells that have light sources

            #region Create the Map String <ESC> P <tstr>
            //int skip = 0;
            if (ch.updateMap)
            {
                for (int j = 0; j < cellArray.Length; j++)
                {
                    if (cellArray[j] == null || this.visCells[j] == false)
                    {
                        mapstring += Map.KPNOVIS;
                        //skip++;
                    }
                    else
                    {
                        //if(skip !=0)
                        //{
                        //	mapstring += (char)(96+skip);
                        //}
                        //skip = 0;
                        if (cellArray[j].AreaEffects.Count > 0)
                        {
                            if (cellArray[j].IsAlwaysDark)
                            {
                                if (ch.HasNightVision)
                                {
                                    mapstring += Map.kesProtoConvertGraphic(cellArray[j].DisplayGraphic);
                                }
                                else
                                {
                                    mapstring += Map.KPDARKNESS;
                                }
                            }
                            else if (ch.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) ||
                                    ch.CurrentCell.IsAlwaysDark)
                            {
                                if (ch.HasNightVision)
                                {
                                    mapstring += Map.kesProtoConvertGraphic(cellArray[j].DisplayGraphic);
                                }
                                else
                                {
                                    mapstring += Map.KPDARKNESS;
                                }
                            }
                            else
                            {
                                mapstring += Map.kesProtoConvertGraphic(cellArray[j].DisplayGraphic);
                            }
                        }
                        else
                        {
                            if (cellArray[j].IsAlwaysDark)
                            {
                                if (ch.HasNightVision)
                                {
                                    mapstring += Map.kesProtoConvertGraphic(cellArray[j].CellGraphic);
                                }
                                else
                                {
                                    mapstring += Map.KPDARKNESS;
                                }
                            }
                            else if (cellArray[j].AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) || cellArray[j].IsAlwaysDark)
                            {
                                if (ch.HasNightVision)
                                {
                                    mapstring += Map.kesProtoConvertGraphic(cellArray[j].CellGraphic);
                                }
                                else
                                {
                                    mapstring += Map.KPDARKNESS;
                                }
                            }
                            else
                            {
                                mapstring += Map.kesProtoConvertGraphic(cellArray[j].CellGraphic);
                            }
                        }
                    }

                }
            }
            mapstring += (char)127;
            #endregion
            #region Create the Message String (ESC-M+<Message>)
            string curMessage = ch.DisplayText;
            
            string messageString = "";
            string newText = "";
            if (curMessage == "")
            {
                messageString = Map.KP_NEXT_LINE + Map.KP_CLEAN_ACTIVE;
            }
            else
            {
                string[] messageArray = curMessage.Split("\n\r".ToCharArray());
                foreach (string mText in messageArray)
                {
                    newText = "";
                    if (mText != "")
                    {
                        if (mText.Length > 65)
                        {
                            newText = mText.Insert(mText.LastIndexOf(" ", 65), Map.KP_NEXT_LINE);
                            newText = newText.Insert(0, Map.KP_NEXT_LINE);
                        }
                        else
                        {
                            newText = mText.Insert(0, Map.KP_NEXT_LINE);
                        }
                    }
                    messageString += newText;                    
                }
                messageString += Map.KP_NEXT_LINE + Map.KP_CLEAN_ACTIVE;
                ch.ClearDisplay(); 
            }
            #endregion
            #region Create the Update String <ESC> U <dir> <istr> <end> Picture Icon Update String
            //
            // Update Map String (ESC-U <dir> <iStr> <end>
            // This function is used to send the critters and treasure data
            // for the picture.  The <dir> bytes is the player course arrow.
            // <istr> is composed of an arbitary number of three character
            // packets that are:
            // byte 1 : picture position 31 + (1..49)
            // byte 2 : icon number
            //          treasure pile    = 32
            //          a critter        = 33
            //          several critters = 34
            // (this will be expanded in the future to indicate
            // the body type of the critter.  The number can be
            // taken out of the display list)
            // byte 3 : display character A..L
            // The end of the string <end> is an ascii 127
            //
            int mobnum = 0;
            for (int j = 0; j < 49; j++)
            {
                if (cellArray[j] == null || this.visCells[j] == false)
                {
                    // do nothing
                }
                else
                {
                    if (cellArray[j].Characters.Count == 1)
                    {
                        if (cellArray[j] != ch.CurrentCell)
                        {
                            updatestring += (char)(32 + j);
                            updatestring += (char)33;
                            updatestring += LETTER[mobnum];
                            mobnum++;
                        }
                    }
                    else if (cellArray[j].Characters.Count > 1)
                    {
                        if (cellArray[j] != ch.CurrentCell)
                        {
                            updatestring += (char)(32 + j);
                            updatestring += (char)34;
                            updatestring += LETTER[mobnum];
                            mobnum++;
                        }


                    }
                    if (cellArray[j].Items.Count > 0 && cellArray[j].LootDraw)
                    {
                        updatestring += (char)(32 + j);
                        updatestring += (char)32 + "$";
                    }
                }
            }
            updatestring += (char)127;
            #endregion
            #region Create the Critter Listing <ESC>Y
            mobnum = 0;
            string critstring = "";
            int alignnum = 0;
            counter = 0;
            for (int x = 1; x < 10; x++)
            {
                critstring += Map.KP_DIRECT_CURS;
                critstring += (char)(31 + x);
                critstring += (char)56;
                critstring += Map.KP_ERASE_END_LINE;
            }
            for (int j = 0; j < 49; j++)
            {
                if (cellArray[j] == null || this.visCells[j] == false)
                {
                    // do nothing
                }
                else
                {
                    if (cellArray[j].Characters.Count > 0)
                    {
                        if (cellArray[j] != ch.CurrentCell)
                        {
                            #region Not on the character's cell
                            if (cellArray[j].Characters.Count == 1)
                            {
                                #region Only 1 mob in the cell
                                foreach (Character mob in cellArray[j].Characters.Values)
                                {
                                    alignnum = (int)mob.Alignment;
                                    if (mob.BaseProfession == Character.ClassType.Thief) { alignnum = 0; }
                                    if (mob.BaseProfession == Character.ClassType.Thief && ch.BaseProfession != Character.ClassType.Knight)
                                    {
                                        if (ch.Level > Skills.GetSkillLevel(mob.magic)) { alignnum = (int)mob.Alignment; }
                                    }
                                    if (mob.IsHidden)
                                    {
                                        #region Mob is hidden
                                        if (Skills.GetSkillLevel(mob.magic) < ch.Level - 3)
                                        {
                                            if (GameSystems.Targeting.TargetAquisition.FindTargetInNextCells(mob, ch.Name) != null && Skills.GetSkillLevel(mob.magic) < ch.Level)
                                            {
                                                critstring += Map.KP_DIRECT_CURS;
                                                critstring += (char)(32 + mobnum);
                                                critstring += (char)56;
                                                critstring += Align[alignnum] + LETTER[counter] + " " + mob.Name + Map.KP_ERASE_END_LINE;
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
                                            critstring += Map.KP_DIRECT_CURS;
                                            critstring += (char)(32 + mobnum);
                                            critstring += (char)56;
                                            critstring += Align[alignnum] + LETTER[counter] + " " + mob.Name + Map.KP_ERASE_END_LINE;
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

                                foreach(Character mob in cellArray[j].Characters.Values)
                                //for (int x = 0; x < cellArray[j].Characters.Count; x++)
                                {
                                    alignnum = (int)mob.Alignment;
                                    if (mob.BaseProfession == Character.ClassType.Thief) { alignnum = 0; }
                                    if (mob.BaseProfession == Character.ClassType.Thief && ch.BaseProfession != Character.ClassType.Knight)
                                    {
                                        if (ch.Level > Skills.GetSkillLevel(mob.magic)) { alignnum = (int)mob.Alignment; }
                                    }
                                    if (mob.IsHidden)
                                    {
                                        if (Skills.GetSkillLevel(mob.magic) < ch.Level - 3)
                                        {
                                            if (GameSystems.Targeting.TargetAquisition.FindTargetInNextCells(mob, ch.Name) != null && Skills.GetSkillLevel(mob.magic) < ch.Level)
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

                                    critstring += Map.KP_DIRECT_CURS;
                                    critstring += (char)(32 + mobnum);
                                    critstring += (char)56;
                                    if ((int)nums[i] > 1)
                                        critstring += Align[(int)aligns[i]] + LETTER[counter] + " " + nums[i] + " " + GameSystems.Text.TextManager.Multinames((string)npcs[i]) + Map.KP_ERASE_END_LINE;
                                    else
                                        critstring += Align[(int)aligns[i]] + LETTER[counter] + " " + npcs[i] + Map.KP_ERASE_END_LINE;
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
                            foreach (Character crit in cellArray[j].Characters.Values)
                            {
                                if (crit != ch)
                                {
                                    if (!crit.IsInvisible)
                                    {
                                        alignnum = (int)crit.Alignment;
                                        if (crit.BaseProfession == Character.ClassType.Thief) { alignnum = 0; }
                                        if (crit.BaseProfession == Character.ClassType.Thief && ch.BaseProfession != Character.ClassType.Knight)
                                        {
                                            if (ch.Level > Skills.GetSkillLevel(crit.magic)) { alignnum = (int)crit.Alignment; }
                                        }
                                        critstring += Map.KP_DIRECT_CURS;
                                        critstring += (char)(32 + mobnum);
                                        critstring += (char)56;
                                        critstring += Align[alignnum] + crit.Name + Map.KP_ERASE_END_LINE; // Align + name
                                        critstring += Map.KP_DIRECT_CURS + ((char)(32 + linenum)); // ESC-Y <line>
                                        critstring += (char)(31 + 41) + crit.RightHandItemName() + Map.KP_ERASE_END_LINE; // Col + righthand
                                        critstring += Map.KP_DIRECT_CURS + ((char)(32 + linenum)); // ESC-Y <line>
                                        critstring += (char)(31 + 56) + crit.LeftHandItemName() + Map.KP_ERASE_END_LINE; // Col + lefthand
                                        critstring += Map.KP_DIRECT_CURS + ((char)(32 + linenum)); // Esc-Y <Line>
                                        critstring += (char)(31 + 71) + crit.GetVisibleArmorName() + Map.KP_ERASE_END_LINE; // Col + Armor
                                        linenum++;
                                        mobnum++;
                                    }
                                }
                            }
                            #endregion
                        }
                        if (ch.CurrentCell != cellArray[j])
                        {
                            counter++;
                        }
                    }
                }

            }
            #endregion
            #region Create the Status Line strings
            // HitText Line 'Hits       : '
            string hitsTextLine = Map.KP_DIRECT_CURS + (char)53 + (char)61 + "Hits       : ";
            // HitsTakenText Line 'Hits Taken   : '
            string hitsTakenTextLine = Map.KP_DIRECT_CURS + (char)53 + (char)82 + "Hits Taken   : ";
            // ExpText Line 'Experience : '
            string ExpTextLine = Map.KP_DIRECT_CURS + (char)54 + (char)61 + "Experience : ";
            // MPText Line 'Magic Points : '
            string MPTextLine = Map.KP_DIRECT_CURS + (char)54 + (char)82 + "Magic Points : ";
            // StamText Line 'Stamina    : '
            string stamTextLine = Map.KP_DIRECT_CURS + (char)55 + (char)61 + "Stamina    : ";
            #region Setup the desired lengths
            // Max hits
            string hitsMax = ch.HitsFull.ToString();
            while (hitsMax.Length < 6)
            {
                hitsMax = hitsMax.Insert(0, " ");
            }
            //hitsLeft
            int hTn = ch.HitsFull - ch.Hits;
            string hitsLeft = hTn.ToString();
            while (hitsLeft.Length < 6)
            {
                hitsLeft = hitsLeft.Insert(0, " ");
            }
            //stamMax
            string stamMax = ch.StaminaFull.ToString();
            while (stamMax.Length < 6)
            {
                stamMax = stamMax.Insert(0, " ");
            }
            //stamLeft
            string stamLeft = ch.Stamina.ToString();
            while (stamLeft.Length < 6)
            {
                stamLeft = stamLeft.Insert(0, " ");
            }
            //manaMax
            string manaMax = ch.ManaFull.ToString();
            while (manaMax.Length < 6)
            {
                manaMax = manaMax.Insert(0, " ");
            }
            //manaLeft
            string manaLeft = ch.Mana.ToString();
            while (manaLeft.Length < 6)
            {
                manaLeft = manaLeft.Insert(0, " ");
            }
            //exp
            string chexp = UpdateExp(ch);
            while (chexp.Length < 7)
            {
                chexp = chexp.Insert(0, " ");
            }

            #endregion
            // numhits
            string numHits = Map.KP_DIRECT_CURS + (char)53 + (char)74 + hitsMax;
            // numhitstaken
            string numHitsTaken = Map.KP_DIRECT_CURS + (char)53 + (char)97 + hitsLeft;
            // numExp
            string numExp = Map.KP_DIRECT_CURS + (char)54 + (char)73 + chexp;
            // numMP
            string numMP = Map.KP_DIRECT_CURS + (char)54 + (char)97 + manaLeft;
            // numStam 
            string numStam = Map.KP_DIRECT_CURS + (char)55 + (char)74 + stamLeft;
            // Right Hand
            string rHandText = Map.KP_DIRECT_CURS + (char)54 + (char)32 + "R ";
            // Left Hand
            string lHandText = Map.KP_DIRECT_CURS + (char)55 + (char)32 + "L ";
            string rHandItem = Map.KP_DIRECT_CURS + (char)54;
            rHandItem += (char)34 + ch.RightHandItemName();
            string lHandItem = Map.KP_DIRECT_CURS + (char)55;
            lHandItem += (char)34 + ch.LeftHandItemName();

            #endregion
            #region Build the Kproto String
            string redraw = Map.KP_DIRECT_CURS + (char)32 + (char)32 + Map.KP_ERASE_TO_END;
            string setCur = Map.KP_DIRECT_CURS + (char)52 + (char)32 + " ";
            string chstatus = "";
            if (ch.Stunned > 0) { chstatus = " You are stunned."; }
            if (ch.IsBlind) { chstatus = " You are blind."; }
            if (ch.IsFeared) { chstatus = " You are afraid."; }
            string cmdline = Map.KP_DIRECT_CURS + ((char)(52)) + " >" + chstatus + Map.KP_ERASE_END_LINE;

            string kProtoString = setCur;

            if (ch.updateAll)
            {
                ch.updateExp = true;
                ch.updateLeft = true;
                ch.updateMap = true;
                ch.updateMP = true;
                ch.updateRight = true;
                ch.updateStam = true;
                kProtoString = setCur + redraw + hitsTextLine + hitsTakenTextLine + ExpTextLine +
                    MPTextLine + stamTextLine + rHandText + lHandText;
                ch.updateAll = false;
            }
            if (ch.updateExp)
            {
                kProtoString += numExp;
                //ch.updateExp = false;
            }
            if (ch.updateHits)
            {
                kProtoString += numHits;
                kProtoString += numHitsTaken;
                //ch.updateHits = false;
            }
            if (ch.updateMP)
            {
                kProtoString += numMP;
                //ch.updateMP = false;
            }
            if (ch.updateStam)
            {
                kProtoString += numStam;
                //ch.updateStam = false;
            }
            if (ch.updateRight)
            {
                kProtoString += rHandItem;
                //ch.updateRight = false;
            }
            if (ch.updateLeft)
            {
                kProtoString += lHandItem;
                //ch.updateLeft = false;
            }

            kProtoString += updatestring + mapstring + critstring + messageString + cmdline;
            #endregion

            #region Send the character the Kproto String

            ch.Write(kProtoString);
            //ch.clearDisplay();
            #endregion
            return;
        }

        public void ShowMap(PC ch)
        {
            try
            {
                //IF this is an NPC, break outa here right away
                if (!ch.IsPC) { return; }

                // handle darkness and blindness
                if(ch.IsBlind && !ch.HasTalent(Talents.GameTalent.TALENTS.BlindFighting))
                {
                    Map.ClearMap(ch);
                }
                else if(ch.IsMeditating)
                {
                    Map.ClearMap(ch);
                }
                else if((ch.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) || ch.CurrentCell.IsAlwaysDark) && !ch.HasNightVision)
                {
                    if (ch.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Light))
                    {
                        ShowNormalDisplayRewrite(ch);
                    }
                    else
                    {
                        Map.ClearMap(ch);
                    }
                }
                else
                {
                    ShowNormalDisplayRewrite(ch);
                }

                Map.WriteAtXY(ch, 12, 6, ch.dirPointer);

                // Show Displays
                UpdateRightHand(ch); // update the player status display
                UpdateLeftHand(ch);
                UpdateHits(ch);
                UpdateHitsLeft(ch);
                UpdateStam(ch);
                UpdateExp(ch);
                UpdateMagicPoints(ch);

                if (ch.Stunned == 0)
                {
                    if (ch.IsFeared)
                    {
                        Map.WriteAtXY(ch, 1, 11, ch.DisplayText);
                        ch.ClearDisplay();
                        Map.WriteAtXY(ch, 1, 21, " -> You are scared!");
                    }
                    else
                    {
                        if (ch.IsBlind)
                        {
                            string message = "You are blind!";

                            if(ch.HasTalent(Talents.GameTalent.TALENTS.BlindFighting))
                                message = "You are blind, yet acutely aware of your surroundings.";

                            ch.WriteToDisplay(message);
                        }
                        Map.WriteAtXY(ch, 1, 11, ch.DisplayText);
                        ch.ClearDisplay();
                        if(ch.IsResting && !ch.IsMeditating)
                            Map.WriteAtXY(ch, 1, 21, " -> (resting) ");
                        else if(ch.IsMeditating)
                            Map.WriteAtXY(ch, 1, 21, " -> (meditating) ");
                        else Map.WriteAtXY(ch, 1, 21, " ->");
                        ch.Write(ch.GetInputBuffer());
                    }
                }
                else
                {
                    Map.WriteAtXY(ch, 1, 11, ch.DisplayText);
                    ch.ClearDisplay();
                    Map.WriteAtXY(ch, 1, 21, " -> You are stunned!");
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public void ShowNormalDisplayRewrite(Character ch)
        {
            try
            {
                string mapstring = "";
                string updatestring = "";
                string[] LETTER = Globals.LETTERS;
                string[] Align = Globals.ALIGNMENT_SYMBOLS;
                Globals.eAlignment visibleAlign = Globals.eAlignment.Lawful; //used to determine if thieves are seen as neutral by knights
                int counter = 0;
                int writeX = 6;
                int writeY = 3;
                ch.Write(Map.CLS);
                ch.seenList.Clear();

                int visibleDistance = ch.GetVisibilityDistance();

                #region Peeking 
                if (ch.IsPeeking)
                {
                    Character target = ch.EffectsList[Effect.EffectTypes.Peek].Target;

                    if (target != null && target.CurrentCell != null)
                    {
                        ch.CurrentCell = target.CurrentCell;
                        visibleDistance = target.GetVisibilityDistance();
                    }
                }
                #endregion

                var cellArray = Cell.GetApplicableCellArray(ch.CurrentCell, visibleDistance);
                var fullCellArray = Cell.GetApplicableCellArray(ch.CurrentCell, Cell.DEFAULT_VISIBLE_DISTANCE);

                #region  Print out the map
                if (!ch.IsBlind)
                {
                    for (int j = 0; j < cellArray.Length; j++)
                    {
                        if (counter == 7)
                        {
                            writeY++;
                            writeX = 6;
                            counter = 0;
                        }

                        if (cellArray[j] == null && ch.CurrentCell.visCells[j] && fullCellArray.Length >= j + 1 && fullCellArray[j] != null)
                        {
                            Globals.eLightSource lightsource; // no use for this yet

                            if (fullCellArray[j].HasLightSource(out lightsource) && !AreaEffect.CellContainsLightAbsorbingEffect(fullCellArray[j]))
                            {
                                cellArray[j] = fullCellArray[j];
                            }
                        }

                        // Cell is null or cell is not visible.
                        if (cellArray[j] == null || this.visCells[j] == false)
                        {
                            mapstring = "  ";
                        }
                        else
                        {
                            if (cellArray[j].AreaEffects.Count > 0)
                            {
                                #region Check for effects
                                mapstring = Map.returnTile(cellArray[j].DisplayGraphic);

                                if (ch.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) ||
                                            ch.CurrentCell.IsAlwaysDark)
                                {
                                    if (!ch.HasNightVision)
                                    {
                                        //mlt tweak,show fire or lightning even if no nv
                                        if (cellArray[j].AreaEffects.ContainsKey(Effect.EffectTypes.Fire) || cellArray[j].AreaEffects.ContainsKey(Effect.EffectTypes.Lightning) || cellArray[j].AreaEffects.ContainsKey(Effect.EffectTypes.Lightning_Storm))
                                        {
                                            mapstring = Map.returnTile(cellArray[j].DisplayGraphic);
                                        }
                                        if(cellArray[j].AreaEffects.ContainsKey(Effect.EffectTypes.Light))
                                        {
                                            mapstring = Map.returnTile(cellArray[j].DisplayGraphic);
                                        }
                                        else { mapstring = "  "; }
                                    }
                                    else
                                    {
                                        if (cellArray[j].AreaEffects.Count >= 1 && !cellArray[j].LootDraw)//mlt tweak to show spell effects if nv
                                        {
                                            mapstring = Map.returnTile(cellArray[j].DisplayGraphic);
                                        }
                                        else
                                        {
                                            mapstring = Map.returnTile(cellArray[j].CellGraphic);
                                        }
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                mapstring = Map.returnTile(cellArray[j].CellGraphic);

                                if ((ch.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) ||
                                            ch.CurrentCell.IsAlwaysDark) && !ch.HasNightVision)
                                {
                                    if (ch.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Light))
                                    {
                                        mapstring = Map.returnTile(cellArray[j].DisplayGraphic);
                                    }
                                    else
                                    {
                                        mapstring = "  ";
                                    }
                                }
                            }
                        }
                        Map.WriteMapStringAtXY(ch, writeX, writeY, mapstring, cellArray[j]);
                        writeX += 2;
                        counter++;
                    }
                }
                #endregion

                #region Print out any creatures in the cell
                int mobletter = 0;
                writeX = 6;
                writeY = 3;
                counter = 0;
                int line = 0;

                // Display mobs in current cell first.
                foreach (Character target in ch.CurrentCell.Characters.Values)
                {
                    if (target != ch && Rules.DetectInvisible(target, ch))
                    {
                        visibleAlign = target.Alignment;
                        if (target.BaseProfession == Character.ClassType.Thief && !Rules.DetectAlignment(target, ch))
                        {
                            visibleAlign = ch.Alignment;
                        }

                        Map.WriteAtXY(ch, 25, 2 + counter, Align[(int)visibleAlign] + " " + target.Name);
                        Map.WriteAtXY(ch, 45, 2 + counter, target.RightHandItemName());
                        Map.WriteAtXY(ch, 57, 2 + counter, target.LeftHandItemName());
                        Map.WriteAtXY(ch, 68, 2 + counter, target.GetVisibleArmorName());

                        counter++;
                        ch.seenList.Add(target);
                    }
                }

                for (int j = 0; j < 49; j++)
                {
                    updatestring = "";

                    if (line == 7)
                    {
                        writeX = 6;
                        writeY++;
                        line = 0;
                    }

                    if (cellArray[j] == null || this.visCells[j] == false)
                    {
                        updatestring = "";
                    }
                    else
                    {
                        if (cellArray[j].Characters.Count == 1)
                        {
                            #region Single Mob in cell
                            if (ch.CurrentCell != cellArray[j])
                            {
                                foreach (Character critter in cellArray[j].Characters.Values)
                                {
                                    visibleAlign = critter.Alignment;

                                    if (critter.BaseProfession == Character.ClassType.Thief)
                                    {
                                        if (!Rules.DetectAlignment(critter, ch))
                                        {
                                            visibleAlign = Globals.eAlignment.Lawful;
                                        }
                                    }

                                    if (Rules.DetectHidden(critter, ch) && Rules.DetectInvisible(critter, ch))
                                    {
                                        updatestring = LETTER[mobletter];
                                        Map.WriteAtXY(ch, 24, 2 + counter, Align[(int)visibleAlign] + LETTER[mobletter] + " " + critter.Name);
                                        counter++;
                                        mobletter++;
                                        ch.seenList.Add(critter);
                                    }
                                }
                            }
                            #endregion
                        }

                        if (cellArray[j].Characters.Count > 1)
                        {
                            #region More than one mob in the cell
                            if (ch.CurrentCell != cellArray[j])
                            {
                                #region Mobs not on characters cell
                                foreach (Character critter in cellArray[j].Characters.Values)
                                {
                                    if (critter.IsPC || critter.Group == null ||
                                        ((critter is NPC) && critter.Group != null && critter.Group.GroupLeaderID == critter.UniqueID))
                                    {
                                        visibleAlign = critter.Alignment;
                                        if (critter.BaseProfession == Character.ClassType.Thief)
                                        {
                                            if (!Rules.DetectAlignment(critter, ch))
                                            {
                                                visibleAlign = Globals.eAlignment.Lawful;
                                            }
                                        }

                                        if (Rules.DetectHidden(critter, ch) && Rules.DetectInvisible(critter, ch))
                                        {
                                            updatestring = LETTER[mobletter];
                                            if (critter.Group == null || critter.IsPC)
                                            {
                                                Map.WriteAtXY(ch, 24, 2 + counter, Align[(int)visibleAlign] + LETTER[mobletter] + " " + critter.Name);
                                                ch.seenList.Add(critter);
                                            }
                                            else
                                            {
                                                Map.WriteAtXY(ch, 24, 2 + counter, Align[(int)visibleAlign] + LETTER[mobletter] + " " + critter.Group.GroupNPCList.Count.ToString() + " " + GameSystems.Text.TextManager.Multinames(critter.Name));
                                                ch.seenList.Add(critter);
                                            }
                                            counter++;
                                        }
                                    }
                                }
                                #endregion
                                mobletter++;
                            }
                            #endregion
                        }
                    }
                    if (updatestring != "")
                    {
                        Map.WriteMapStringAtXY(ch, writeX, writeY, updatestring, cellArray[j]);
                    }
                    writeX += 2;
                    line++;
                }
                #endregion

                #region Display any items in the cells
                if (!ch.IsBlind)
                {
                    try
                    {
                        counter = 0;
                        writeX = 7;
                        writeY = 3;

                        for (int j = 0; j < 49; j++)
                        {
                            if (counter == 7)
                            {
                                writeY++;
                                writeX = 7;
                                counter = 0;
                            }
                            if (cellArray[j] == null || this.visCells[j] == false)
                            {
                                updatestring = "";
                            }
                            else if (cellArray[j].Items.Count > 0)
                            {
                                // Always draw loot if in the current cell.
                                if (ch.CurrentCell == cellArray[j])
                                {
                                    updatestring = "$";
                                }
                                else if ((cellArray[j].CellGraphic != GRAPHIC_WATER ||
                                    cellArray[j].DisplayGraphic != GRAPHIC_WATER ||
                                    cellArray[j].CellGraphic != GRAPHIC_WALL ||
                                    cellArray[j].DisplayGraphic != GRAPHIC_WALL) && cellArray[j].LootDraw)
                                {
                                    updatestring = "$";
                                }
                            }
                            else
                            {
                                updatestring = "";
                            }
                            if (cellArray[j] != null)
                            {
                                if (!cellArray[j].LootDraw && cellArray[j].Items.Count > 0 && cellArray[j].AreaEffects.Count > 0)
                                {
                                    updatestring = "";
                                    Map.WriteMapStringAtXY(ch, writeX, writeY, updatestring, cellArray[j]);
                                }

                                else if (updatestring != "")
                                {
                                    Map.WriteMapStringAtXY(ch, writeX, writeY, updatestring, cellArray[j]);
                                }
                            }
                            else
                            {
                                if (updatestring != "")
                                    Map.WriteMapStringAtXY(ch, writeX, writeY, updatestring, cellArray[j]);
                            }
                            counter++;
                            writeX += 2;
                        }
                    }
                    catch (Exception ex3)
                    {
                        Utils.Log("Error in Print out Items", Utils.LogType.ExceptionDetail);
                        Utils.LogException(ex3);
                        return;
                    }
                }
                #endregion
                return;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return;
            }
        }

        public void UpdateRightHand(Character ch)
        {
            string clr = "                             ";
            Map.WriteAtXY(ch, 1, 23, clr);
            Map.WriteAtXY(ch, 1, 23, " " + Map.BGRN + "R " + ch.RightHandItemName() + Map.CEND);
            return;
        }

        public void UpdateLeftHand(Character ch)
        {
            string clr = "                             ";
            Map.WriteAtXY(ch, 1, 24, clr);
            Map.WriteAtXY(ch, 1, 24, " " + Map.BGRN + "L " + ch.LeftHandItemName() + Map.CEND);
            return;
        }

        public void UpdateHits(Character ch)
        {
            string clr = "      ";
            Map.WriteAtXY(ch, 42, 23, clr);
            Map.WriteAtXY(ch, 30, 23, Map.CMAG + "Hits       : " + ch.Hits + "/" + ch.HitsFull + "    " + Map.CEND);
            return;
        }

        public void UpdateHitsLeft(Character ch)
        {
            string clr = "      ";
            Map.WriteAtXY(ch, 72, 23, clr);
            Map.WriteAtXY(ch, 60, 23, Map.CMAG + "Hits Taken : " + (int)(ch.HitsFull - ch.Hits) + Map.CEND);
            return;
        }

        public string UpdateExp(Character ch)
        {
            string clr = "        ";
            string chexp = "0";

            if (ch.Experience > 999999)
            {
                long expK = ch.Experience / 1000;
                chexp = Convert.ToString(expK) + "K";
            }
            else
            {
                chexp = Convert.ToString(ch.Experience);
            }

            if (ch.protocol == "normal")
            {
                // Spinel bug where xp at or over a high value causes the Spinel client to crash.
                // Added 10/27/2019 Eb
                if (ch.Experience > 1600000)
                    chexp = "0";

                Map.WriteAtXY(ch, 42, 24, clr);
                Map.WriteAtXY(ch, 30, 24, Map.CMAG + "Experience : " + chexp + "  " + Map.CEND);
                return "";
            }
            else if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol || ch.protocol == "old-kesmai")
            {
                return chexp;
            }
            else
            {
                Map.WriteAtXY(ch, 42, 24, clr);
                Map.WriteAtXY(ch, 30, 24, Map.CMAG + "Experience : " + chexp + "  " + Map.CEND);
                return "";
            }
        }

        public void UpdateStam(Character ch)
        {
            string clr = "     ";
            Map.WriteAtXY(ch, 42, 25, clr);
            Map.WriteAtXY(ch, 30, 25, Map.CMAG + "Stamina    : " + ch.Stamina + Map.CEND);
            return;
        }

        public void UpdateMagicPoints(Character ch)
        {
            string clr = "     ";
            if (ch.ManaMax > 0)
            {
                Map.WriteAtXY(ch, 72, 24, clr);
                Map.WriteAtXY(ch, 60, 24, Map.CMAG + "Magic Points: " + ch.Mana + Map.CEND);
            }
            return;
        }

        public void ClearDisplayWindow(Character ch) //change to a real buffer that gets displayed each round, clearing at the end.
        {
            int x;

            string clr = "                                                                                             ";
            for (x = 22; x > 9; x--)
            {
                Map.WriteAtXY(ch, 1, x, clr);
            }
        }

        public void SendShout(string text)
        {
            Cell cell = null;

            for (int ypos = -6; ypos <= 6; ypos += 1)
            {
                for (int xpos = -6; xpos <= 6; xpos += 1)
                {
                    try
                    {
                        if (Cell.CellRequestInBounds(this, xpos, ypos))
                        {
                            cell = Cell.GetCell(this.FacetID, this.LandID, this.MapID, this.X + xpos, this.Y + ypos, this.Z);
                            
                            if (cell != null)
                            {
                                foreach(Character chr in cell.Characters.Values)
                                {
                                    chr.WriteToDisplay(Character.GetTextDirection(chr, this.X, this.Y) + text);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Utils.LogException(e);
                    }
                }
            }
        }

        public void SendToAllInSight(string text)
        {
            Cell cell = null;

            for (int ypos = -3; ypos <= 3; ypos += 1)
            {
                for (int xpos = -3; xpos <= 3; xpos += 1)
                {
                    try
                    {
                        if (Cell.CellRequestInBounds(this, xpos, ypos))
                        {
                            cell = Cell.GetCell(this.FacetID, this.LandID, this.MapID, this.X + xpos, this.Y + ypos, this.Z);

                            if (cell != null)
                            {
                                foreach(Character chr in cell.Characters.Values)
                                {
                                    chr.WriteToDisplay(text);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Utils.LogException(e);
                    }
                }
            }
        }

        public static void Move(Character ch, string[] dir, int landID, int mapID, int xcord, int ycord, int zcord)
        {
            if (dir == null)
                dir = new string[] { "", "", "" };

            if(ch != null && ch.EffectsList != null)
            {
                if(ch.EffectsList.ContainsKey(Effect.EffectTypes.Ensnare))
                {
                    ch.WriteToDisplay("You are ensnared and cannot move!");
                    return;
                }
            }

            string tile;

            ch.updateMap = true;

            Cell tCell = Cell.GetCell(ch.FacetID, landID, mapID, xcord, ycord, zcord);

            if (tCell == null)
            {
                Utils.Log("Cell.Move cell is null land = " + landID + " map = " + mapID + " x = " + xcord + " y = " + ycord + " z = " + zcord, Utils.LogType.SystemWarning);
                return;
            }

            if (tCell.IsOutOfBounds)
            {
                Utils.Log("Cell.Move cell is out of bounds and a move will not be completed.", Utils.LogType.SystemWarning);
                return;
            }
            
            if (tCell.AreaEffects.Count > 0)
                tile = tCell.DisplayGraphic;
            else
                tile = tCell.CellGraphic;

            #region Handle shark moving
            if (ch is NPC) // mlt shark work v
            {
                NPC npc = (NPC)ch;

                if (npc.IsWaterDweller)
                {
                    if (tCell.CellGraphic == Cell.GRAPHIC_WATER)
                    {
                        npc.CurrentCell = tCell;
                        return;
                    }
                    else if (tCell.CellGraphic == Cell.GRAPHIC_REEF)
                    {
                        dir[0] = "";
                        dir[1] = "";
                        dir[2] = "";
                        return;
                    }
                    else
                    {
                        dir[0] = "";
                        dir[1] = "";
                        dir[2] = "";
                        return;
                    }
                }
            }
            #endregion

            try
            {
                #region Handle moving TO a cell (tCell)

                bool isTeleporting = false;

                try
                {
                    if (ch.TemporaryStorage != null)
                        isTeleporting = Convert.ToBoolean(ch.TemporaryStorage);
                }
                catch
                {
                    // do nothing
                }

                if (tCell.IsTeleport && isTeleporting == false) // prevents loop of teleportation
                {
                    #region Handle moving into a teleport
                    if (!ch.IsPC && ch.PetOwner == null)
                    {
                        // perform a normal move for mobs, since we dont want them to use teleporters
                        ch.CurrentCell = tCell;

                        if (ch.floating < 2)
                            ch.floating = 2;

                        return;
                    }


                    Segue segue = Cell.GetCell(ch.FacetID, ch.LandID, ch.MapID, xcord, ycord, zcord).Segue;// Segue.GetSegue1(ch.LandID, ch.MapID, xcord, ycord);

                    //bool found = false;

                    try
                    {
                        if (segue != null)
                        {
                            ch.TemporaryStorage = true; // temporarily store that the character is moving through a segue / teleport

                            if (Cell.PassesCellLock(ch, tCell.cellLock, false)) // teleport the character if there is a match
                            {
                                #region Hard coded by Inkler at some point.
                                //if (tCell == hbCell1)
                                //{
                                //    //mlt first make sure noone else in room,anti grief player                                
                                //    Cell cell = null;
                                //    try
                                //    {
                                //        //loop through all altar room cells
                                //        for (int ypos = 29; ypos <= 33; ypos += 1)
                                //        {
                                //            for (int xpos = 82; xpos <= 91; xpos += 1)
                                //            {
                                //                cell = Cell.GetCell(0, 0, 11, xpos, ypos, -240);
                                //                if (cell.CellGraphic == GRAPHIC_ALTAR_PLACEABLE) { continue; }
                                //                // look for any character in the cell
                                //                foreach (Character chr in cell.Characters.Values)
                                //                {
                                //                    if (chr != null && chr is PC && (chr as PC).ImpLevel < Globals.eImpLevel.DEV)
                                //                    {
                                //                        ch.WriteToDisplay("Someone else is in there,the ghods have prevented you from losing your amulet.");
                                //                        ch.WriteToDisplay("Only 1 person at a time is allowed.");
                                //                        found = true;
                                //                        ch.CurrentCell = Cell.GetCell(ch.FacetID, ch.LandID, ch.MapID, 80, 32, -240);
                                //                        dir[0] = "";
                                //                        dir[1] = "";
                                //                        dir[2] = "";
                                //                        return;
                                //                    }
                                //                }
                                //            }
                                //        }
                                //        if (!found)
                                //        {
                                //            //no char found so delete ammy key
                                //            if (ch.RightHand != null && ch.RightHand.itemID == 1205)
                                //            {
                                //                ch.UnequipRightHand(ch.RightHand);
                                //            }
                                //            else if (ch.LeftHand != null && ch.LeftHand.itemID == 1205)
                                //            {
                                //                ch.UnequipLeftHand(ch.LeftHand);
                                //            }
                                //        }
                                //    }
                                //    catch (Exception e)
                                //    {
                                //        Utils.LogException(e);
                                //    }
                                //}
                                #endregion

                                // success message
                                if (tCell.cellLock.lockSuccess.Length > 0)
                                {
                                    ch.WriteToDisplay(tCell.cellLock.lockSuccess);
                                }

                                Move(ch, null, segue.LandID, segue.MapID, segue.X, segue.Y, segue.Z);
                                ch.TemporaryStorage = null;

                            }
                            else if (tCell.cellLock.lockFailureXCord != -1 && tCell.cellLock.lockFailureXCord >= 1)// place the character on the teleport cell only
                            {
                                if (tCell.cellLock.lockFailureString.Length > 0)
                                {
                                    ch.WriteToDisplay(tCell.cellLock.lockFailureString);
                                }

                                if (ch.floating < 2)
                                {
                                    ch.floating = 2;
                                }

                                Move(ch, null, ch.LandID, ch.MapID, tCell.cellLock.lockFailureXCord, tCell.cellLock.lockFailureYCord, tCell.cellLock.lockFailureZCord);
                                ch.TemporaryStorage = null;
                                return;
                            }
                            else
                            {
                                ch.CurrentCell = tCell;
                                if (tCell.cellLock.lockFailureString.Length > 0)
                                {
                                    ch.WriteToDisplay(tCell.cellLock.lockFailureString);
                                }
                                if (ch.floating < 2)
                                {
                                    ch.floating = 2;
                                }
                                ch.TemporaryStorage = null;
                                return;
                            }
                        }
                        dir[0] = "";
                        dir[1] = "";
                        dir[2] = "";
                        return;
                    }
                    catch (Exception e)
                    {
                        Utils.LogException(e);
                        dir[0] = "";
                        dir[1] = "";
                        dir[2] = "";
                    }
                    #endregion
                }

                if (Array.IndexOf(tCell.cellLock.lockTypes, Cell.CellLock.LockType.None) == -1) // there is a CellLock
                {
                    if (Array.IndexOf(tCell.cellLock.lockTypes, Cell.CellLock.LockType.Door) != -1)
                    {
                        if (ch.IsImmortal || (tCell.AreaEffects.ContainsKey(Effect.EffectTypes.Unlocked_Horizontal_Door) || tCell.AreaEffects.ContainsKey(Effect.EffectTypes.Unlocked_Vertical_Door)))
                        {
                            ch.CurrentCell = tCell;
                            if (ch.floating < 2)
                            {
                                ch.floating = 2;
                            }
                            return;
                        }
                        else
                        {
                            ch.WriteToDisplay("The door is locked.");
                            dir[0] = "";
                            dir[1] = "";
                            dir[2] = "";
                            return;
                        }
                    }

                    if (tCell.IsSecretDoor && !tCell.AreaEffects.ContainsKey(Effect.EffectTypes.Find_Secret_Door))
                    {
                        goto checkTileType;
                    }

                    if (!tCell.IsTeleport) // NOT a teleport
                    {
                        if (PassesCellLock(ch, tCell.cellLock, true)) // passes the cellLock
                        {
                            ch.CurrentCell = tCell;
                            if (ch.floating < 2)
                            {
                                ch.floating = 2;
                            }
                            if (tCell.cellLock.lockSuccess != "") { ch.WriteToDisplay(tCell.cellLock.lockSuccess); }
                            #region Hard coded by Inkler.
                            //if (tCell == hbCell2)
                            //{
                            //    //mlt hummingbird work,the whirlwind and items spawner
                            //    for (int y = 29; y <= 33; y += 1)
                            //    {
                            //        for (int x = 82; x <= 91; x += 1)
                            //        {
                            //            Cell wCell = Cell.GetCell(0, 0, 11, x, y, -240);
                            //            if (wCell == null) { ch.WriteToDisplay("getting a null cell"); }
                            //            if (wCell.CellGraphic != GRAPHIC_ALTAR_PLACEABLE && wCell.CellGraphic != GRAPHIC_WALL)
                            //            {
                            //                Effect effect = new Effect(Effect.EffectType.Whirlwind, Cell.GRAPHIC_WHIRLWIND, 15, 4, null, wCell);
                            //            }
                            //        }
                            //    }
                            //    Item hb1 = null;
                            //    Item hb3 = null;
                            //    int randomitem = 0;
                            //    if (ch.Alignment == Globals.eAlignment.Lawful) { hb1 = Item.CopyItemFromDictionary(1209); }
                            //    else if (ch.Alignment == Globals.eAlignment.Neutral) { hb1 = Item.CopyItemFromDictionary(1213); }
                            //    else if (ch.Alignment == Globals.eAlignment.Chaotic) { hb1 = Item.CopyItemFromDictionary(1214); }
                            //    ch.CurrentCell.Add(hb1);
                            //    Item hb2 = Item.CopyItemFromDictionary(20040);//drake potion
                            //    ch.CurrentCell.Add(hb2);
                            //    randomitem = Rules.dice.Next(1, 100);
                            //    if (randomitem < 50) { hb3 = Item.CopyItemFromDictionary(1204); }//perm wis pot
                            //    else if (randomitem < 75) { hb3 = Item.CopyItemFromDictionary(13090); }//f&i ring
                            //    else if (randomitem < 95) { hb3 = Item.CopyItemFromDictionary(30010); }//tiger figurine
                            //    else { hb3 = Item.CopyItemFromDictionary(19020); }//ff boots
                            //    ch.CurrentCell.Add(hb3);
                            //    PC chr = PC.GetOnline(ch.Name);
                            //    Quest qwest = null;
                            //    foreach (Quest q in chr.QuestList)
                            //    {
                            //        if (q.QuestID == 45)
                            //        {
                            //            qwest = chr.GetQuest(q.QuestID);
                            //            break;
                            //        }
                            //        else { continue; }
                            //    }
                            //    qwest.FinishStep(null, chr, 1);
                            //} 
                            #endregion
                            return;
                        }
                        else
                        {
                            if (tCell.cellLock.lockFailureString.Length > 0)
                            {
                                ch.WriteToDisplay(tCell.cellLock.lockFailureString);
                            }
                            // this is where we do CellLock failure effects
                            dir[0] = "";
                            dir[1] = "";
                            dir[2] = "";
                            return;
                        }
                    }
                }
                #endregion

                if (tCell.IsWithinTownLimits && !ch.IsPC && ch.Alignment != Globals.eAlignment.Lawful)
                {
                    #region Prevent non lawful mobs from entering a lawful town
                    //ch.WriteToDisplay("You are not allowed to enter the town limits.");
                    dir[0] = "";
                    dir[1] = "";
                    dir[2] = "";
                    return;
                    #endregion
                }

                if (tCell.TrapPower > 0)//mlt concussion trap work
                {
                    #region Activate trap if cell contains concussion trap
                    ch.CurrentCell = tCell;
                    if (ch.floating < 2)
                    {
                        ch.floating = 2;
                    }
                    ch.WriteToDisplay("You feel the floor shift slightly under your feet.");
                    DragonsSpine.Spells.GameSpell.CastGenericAreaSpell(tCell, "2", Effect.EffectTypes.Concussion, tCell.TrapPower, "Concussion");
                    dir[0] = "";
                    dir[1] = "";
                    dir[2] = "";
                    return;
                    #endregion
                }

                if ((tCell.IsLockedHorizontalDoor || tCell.IsLockedVerticalDoor) && !tCell.IsOpenDoor)
                {
                    #region Locked Door
                    ch.WriteToDisplay("The door is locked.");
                    dir[0] = "";
                    dir[1] = "";
                    dir[2] = "";
                    return;
                    #endregion
                }

                if (tCell.Characters.Count > 0 && tCell.IsSingleCustomer && ch.IsPC && !tCell.IsPVPEnabled) // moving into a single customer cell
                {
                    #region Handle moving into a single customer cell
                    foreach (Character customer in tCell.Characters.Values)
                    {
                        // if the customer isn't an implementor, and the character moving isn't an implementor
                        if (customer is PC && ch is PC && (customer as PC).ImpLevel < Globals.eImpLevel.GM && (ch as PC).ImpLevel < Globals.eImpLevel.GM)
                        {
                            ch.WriteToDisplay("Please wait your turn.");
                            dir[0] = "";
                            dir[1] = "";
                            dir[2] = "";
                            return;
                        }
                    }
                    #endregion
                }

                // On initial spawning when server is starting allow crowded cells.
                if (DragonsSpineMain.ServerStatus == DragonsSpineMain.ServerState.Running)
                {
                    int chrCount = tCell.Characters.Count;

                    foreach (Character chrObj in tCell.Characters.Values)
                        if (chrObj is PathTest) chrCount--;

                    // Note this limits moving and attacking with certain combat talents such as Leg Sweep, Cleave and Backstab.
                    if (chrCount > 10 && !ch.IsImmortal) // dont let a pc enter a cell with more than 5 ppl/mobs in it
                    {
                        #region Handle PC's moving into crowded cells
                        ch.WriteToDisplay(GameSystems.Text.TextManager.CROWD_BLOCK);
                        dir[0] = "";
                        dir[1] = "";
                        dir[2] = "";
                        return;
                        #endregion
                    }

                    if (chrCount > 7 && !ch.IsPC) // dont let a mob enter a cell with more than 7 ppl/mobs in it
                    {
                        #region Handle Mobs moving into crowded cells
                        //ch.WriteToDisplay("You are blocked by the crowd.");
                        dir[0] = "";
                        dir[1] = "";
                        dir[2] = "";
                        return;
                        #endregion
                    }
                }

                if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Crawl))//mlt cut this to fix crawl stun (ch.LastCommand != null && 
                {
                    #region Handle a Crawl command that would run into a wall
                    switch (tile)
                    {
                        case GRAPHIC_WATER:
                            ch.WriteToDisplay("You are stopped by water.");
                            dir[0] = "";
                            dir[1] = "";
                            dir[2] = "";
                            return;
                        case GRAPHIC_AIR:
                            ch.WriteToDisplay("You detect open air.");
                            dir[0] = "";
                            dir[1] = "";
                            dir[2] = "";
                            return;
                        case GRAPHIC_WALL:
                            ch.WriteToDisplay("You are stopped by a wall.");
                            dir[0] = "";
                            dir[1] = "";
                            dir[2] = "";
                            return;
                        case GRAPHIC_ALTAR_PLACEABLE:
                        case GRAPHIC_ALTAR:
                            ch.WriteToDisplay("You are stopped by an altar.");
                            dir[0] = "";
                            dir[1] = "";
                            dir[2] = "";
                            return;
                        case GRAPHIC_COUNTER_PLACEABLE:
                        case GRAPHIC_COUNTER:
                            ch.WriteToDisplay("You are stopped by a counter.");
                            dir[0] = "";
                            dir[1] = "";
                            dir[2] = "";
                            return;
                        case GRAPHIC_MOUNTAIN:
                            ch.WriteToDisplay("You are stopped by solid earth.");
                            dir[0] = "";
                            dir[1] = "";
                            dir[2] = "";
                            return;
                        case GRAPHIC_GRATE:
                            ch.WriteToDisplay("You are stopped by a grate.");
                            dir[0] = "";
                            dir[1] = "";
                            dir[2] = "";
                            return;
                        case GRAPHIC_FIRE:
                            ch.WriteToDisplay("You are stopped by fire.");
                            dir[0] = "";
                            dir[1] = "";
                            dir[2] = "";
                            return;
                    }
                    #endregion
                }

                checkTileType:
                // moves into a wall
                if (tile.Equals(GRAPHIC_WALL) || tile.Equals(GRAPHIC_GRATE) || tile.Equals(GRAPHIC_MOUNTAIN) || tile.Equals(GRAPHIC_ALTAR) ||
                    tile.Equals(GRAPHIC_COUNTER) || tile.Equals(GRAPHIC_COUNTER_PLACEABLE) || tile.Equals(GRAPHIC_ALTAR_PLACEABLE) && !ch.IsImmortal)
                {
                    #region Handle moving into a Wall
                    if (tCell.AreaEffects.Count > 0)
                    {
                        if (tCell.DisplayGraphic.Equals(GRAPHIC_EMPTY))
                            ch.CurrentCell = tCell;

                        return;
                    }

                    if (!ch.IsImmortal && !ch.IsInvisible && !ch.CommandsProcessed.Contains(CommandTasker.CommandType.Crawl)) // let's keep the service department happy
                    {
                        ch.WriteToDisplay("WHUMP! You are stunned!");

                        //if (!ch.IsPC && !ch.IsBlind && !ch.IsFeared)
                        //{
                        //    Utils.Log(ch.GetLogString() + " ran into a wall with CellGraphic " + tile + " and DisplayGraphic " + tCell.DisplayGraphic, Utils.LogType.Unknown);
                        //}

                        //if (ch.race != "")
                        //    ch.SendToAllInSight(ch.Name + " runs into a wall.");
                        //else ch.SendToAllInSight("The " + ch.Name + " runs into a wall.");
                        if (dir[2] != "") { ch.Stunned = (short)(Rules.RollD(1, 3) + 1); }
                        else if (dir[1] != "") { ch.Stunned = (short)2; }
                        else { ch.Stunned = (short)1; }
                    }

                    if (!ch.IsImmortal)
                    {
                        dir[0] = "";
                        dir[1] = "";
                        dir[2] = "";
                        return;
                    }
                    #endregion
                }
                else if (tile.Equals(GRAPHIC_AIR) && !ch.CommandsProcessed.Contains(CommandTasker.CommandType.Crawl) && !ch.CommandsProcessed.Contains(CommandTasker.CommandType.Climb))
                {
                    #region Handle moving into Air

                    if (ch.CanFly)
                    {
                        ch.CurrentCell = tCell;
                        return;
                    }

                    Segue segue = null;
                    int totalHeight = 0;
                    int currentHeight = ch.CurrentCell.Z;
                    int countLoop = 0;
                    int curx = xcord;
                    int cury = ycord;
                    int curz = zcord;
                    do
                    {
                        segue = Segue.GetDownSegue(Cell.GetCell(ch.FacetID, landID, mapID, curx, cury, curz));

                        countLoop++;

                        if (segue != null)
                        {
                            ch.CurrentCell = Cell.GetCell(ch.FacetID, segue.LandID, segue.MapID, segue.X, segue.Y, segue.Z);
                            curx = segue.X;
                            cury = segue.Y;
                            curz = segue.Z;
                            totalHeight += segue.Height;
                        }
                        else return;
                    }
                    while (Cell.GetCell(ch.FacetID, segue.LandID, segue.MapID, segue.X, segue.Y, segue.Z).CellGraphic == GRAPHIC_AIR && countLoop < 100);

                    if (segue != null)
                    {
                        ch.CurrentCell = Cell.GetCell(ch.FacetID, landID, mapID, segue.X, segue.Y, segue.Z);

                        if (segue.Height != 0)
                            ch.WriteToDisplay("You have fallen " + Math.Abs(totalHeight) + " feet!");
                        else if (segue.Height == 0)
                        {
                            totalHeight = Math.Abs(Rules.Dice.Next(3, 10));
                            ch.WriteToDisplay("You have fallen " + totalHeight + " feet into an inaccessible fissure or cave. A passing Ghod, seeing your plight, has teleported you to a safe spot.");
                        }
                        // there will be damage if character does not have featherfall
                        if (!ch.HasFeatherFall)
                        {
                            Globals.eEncumbranceLevel EncumbDesc = Rules.GetEncumbrance(ch);
                            int stunMod = 0;
                            int fallDmg = totalHeight;
                            // there will be more damage and stun if encumbered
                            if (EncumbDesc > Globals.eEncumbranceLevel.Lightly)
                            {
                                int maxEncumb = Rules.Formula_MaxEncumbrance(ch);
                                fallDmg += (int)(ch.encumbrance - maxEncumb);

                                stunMod = 3 * (int)EncumbDesc;
                            }

                            if (totalHeight >= 10 || stunMod > 0)
                            {
                                ch.Stunned += (short)(Rules.Dice.Next(stunMod) + stunMod);
                            }

                            #region Emit Falling Sound
                            if (ch.gender == Globals.eGender.Female)
                            {
                                ch.EmitSound(Sound.GetCommonSound(Sound.CommonSound.FallingFemale));
                            }
                            else if (ch.gender == Globals.eGender.Male)
                            {
                                ch.EmitSound(Sound.GetCommonSound(Sound.CommonSound.FallingMale));
                            }
                            #endregion
                            Combat.DoDamage(ch, ch, fallDmg, false);
                        }
                    }

                    dir[0] = "";
                    dir[1] = "";
                    dir[2] = "";
                    return;
                    #endregion
                }
                else if (tile.Equals(GRAPHIC_REEF) && !ch.CommandsProcessed.Contains(CommandTasker.CommandType.Crawl))
                {
                    #region Handle moving into a Reef
                    ch.WriteToDisplay(GameSystems.Text.TextManager.PATH_IS_BLOCKED);
                    dir[0] = "";
                    dir[1] = "";
                    dir[2] = "";
                    return;
                    #endregion
                }
                else if (tile.Equals(GRAPHIC_FOREST_IMPASSABLE) && (!tCell.AreaEffects.ContainsKey(Effect.EffectTypes.Shelter) ||
                    (tCell.AreaEffects.ContainsKey(Effect.EffectTypes.Shelter) && tCell.AreaEffects[Effect.EffectTypes.Shelter].Caster != ch)))
                {
                    #region Handle moving into a Solid Tree
                    ch.WriteToDisplay(GameSystems.Text.TextManager.PATH_IS_BLOCKED);
                    dir[0] = "";
                    dir[1] = "";
                    dir[2] = "";
                    #endregion
                }
                else if (tile.Equals(GRAPHIC_WATER))
                {
                    #region Handle moving into Water
                    if (ch.CanFly)
                    {
                        ch.CurrentCell = tCell;
                        return;
                    }

                    //if (ch.IsPC)
                    //{
                    //    ch.WriteToDisplay("Splash!");
                    //}

                    ch.CurrentCell = tCell;

                    if ((!(ch is NPC) || !(ch as NPC).IsSpectral) &&
                        !Autonomy.EntityBuilding.EntityLists.WATER_DWELLER.Contains(ch.entity) &&
                        !Autonomy.EntityBuilding.EntityLists.AMPHIBIOUS.Contains(ch.entity) &&
                        (!ch.IsInvisible))
                    {
                        ch.EmitSound(Sound.GetCommonSound(Sound.CommonSound.Splash));
                    }

                    if (!ch.IsImmortal)
                    {
                        dir[0] = "";
                        dir[1] = "";
                        dir[2] = "";
                    }

                    if (ch.IsPC && !ch.IsImmortal && !ch.CanFly && !ch.CanBreatheWater && !IsNextToLand(tCell)) // IsNextToLand = shallow water
                    {
                        if (ch.floating >= 4)
                        {
                            ch.floating -= 1;
                        }
                        else if (ch.floating == 3)
                        {
                            ch.floating -= 1;
                            ch.WriteToDisplay("You are sinking below the surface!");
                        }
                        else if (ch.floating == 2)
                        {
                            ch.floating -= 1;
                            ch.WriteToDisplay("You are submerged!");
                        }
                        else if (ch.floating == 1)
                        {
                            ch.floating -= 1;
                            ch.WriteToDisplay("You are submerged under water and cannot breathe!");

                        }
                        else if (ch.floating < 1)
                        {
                            ch.WriteToDisplay("You have drowned!");
                            ch.SendToAllInSight(ch.Name + " has drowned to death.");
                            if (!ch.IsDead)
                            {
                                Rules.DoDeath(ch, null);
                            }
                        }
                        else
                        {
                            ch.floating = 3; // just in case.
                        }

                    }
                    #endregion
                }
                else if (tile.Equals(GRAPHIC_CLOSED_DOOR_HORIZONTAL))
                {
                    #region Handle moving into a Door
                    ch.CurrentCell = tCell;
                    ch.CurrentCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.OpenDoor));
                    ch.CurrentCell.DisplayGraphic = GRAPHIC_OPEN_DOOR_HORIZONTAL;
                    ch.CurrentCell.CellGraphic = GRAPHIC_OPEN_DOOR_HORIZONTAL;
                    #endregion
                }
                else if (tile.Equals(Cell.GRAPHIC_CLOSED_DOOR_VERTICAL))
                {
                    #region Handle moving into a Door
                    ch.CurrentCell = tCell;
                    ch.CurrentCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.OpenDoor));
                    ch.CurrentCell.DisplayGraphic = GRAPHIC_OPEN_DOOR_VERTICAL;
                    ch.CurrentCell.CellGraphic = GRAPHIC_OPEN_DOOR_VERTICAL;
                    #endregion
                }
                else if ((tile.Equals(GRAPHIC_RUINS_RIGHT) || tile.Equals(GRAPHIC_RUINS_LEFT)) && !ch.IsImmortal)
                {
                    #region Handle moving into ruins
                    ch.CurrentCell = tCell;
                    if (dir[2] != "")
                    {
                        ch.WriteToDisplay("Your movement is slowed by the rubble.");
                        dir[2] = "";
                    }
                    #endregion
                }
                else if (tile.Equals(GRAPHIC_SAND) && !ch.IsImmortal)
                {
                    ch.CurrentCell = tCell;
                    if (dir[2] != "")
                    {
                        ch.WriteToDisplay("Your movement is slowed by the sand.");
                        dir[2] = "";
                    }
                }
                else if ((tile.Equals(GRAPHIC_UPSTAIRS) || tile.Equals(GRAPHIC_DOWNSTAIRS)) && !ch.IsImmortal &&
                    (Array.IndexOf(dir, "up") > -1 || Array.IndexOf(dir, "dn") > -1 || Array.IndexOf(dir, "u") > -1 || Array.IndexOf(dir, "d") > -1 ||
                    Array.IndexOf(dir, "down") > -1))
                {
                    // character can move to stairs, must move up in a different round
                    ch.CurrentCell = tCell;

                    dir[0] = "";
                    dir[1] = "";
                    dir[2] = "";
                }
                else
                {
                    ch.CurrentCell = tCell;

                    if (ch.floating < 2)
                        ch.floating = 2;
                    return;
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public void EmitSound(string soundFile)
        {
            if (soundFile == null || soundFile == "")
            {
                return;
            }
            else if (soundFile.Length < 4)
            {
                Utils.Log("Sound file length less than 4 at cell.EmitSound(" + soundFile + ")", Utils.LogType.SystemWarning);
                return;
            }
            Cell cell = null;
            for (int ypos = -6; ypos <= 6; ypos++)
            {
                for (int xpos = -6; xpos <= 6; xpos++)
                {
                    if (CellRequestInBounds(this, xpos, ypos))
                    {
                        cell = GetCell(FacetID, LandID, MapID, X + xpos, Y + ypos, Z);

                        if (cell != null && cell.Characters.Count > 0)
                        {
                            foreach (Character chr in cell.Characters.Values)
                            {
                                if ((chr is PC) && chr.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                                {
                                    int distance = GetCellDistance(X, Y, cell.X, cell.Y);
                                    int direction = Convert.ToInt32(Map.GetDirection(cell, this)); // the sound is <direction> from the cell emitting sound (this) 
                                    chr.Write(ProtocolYuusha.SOUND + soundFile + ProtocolYuusha.VSPLIT + distance + ProtocolYuusha.VSPLIT + direction + ProtocolYuusha.SOUND_END);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static int DEFAULT_VISIBLE_CELLS = 49;
        public static int DEFAULT_VISIBLE_DISTANCE = 3;
        public static int CENTER_VISIBLE_CELL = 24;
        private static int[] DISTANCE_ONE_NULL_CELLS = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 19, 20, 21, 22, 26, 27, 28, 29, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48 };
        private static int[] DISTANCE_ONE_VISIBLE_CELLS = new[] {16, 17, 18, 23, 24, 25, 30, 31, 32};
        private static int[] DISTANCE_TWO_NULL_CELLS = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 13, 14, 20, 21, 27, 28, 34, 35, 41, 42, 43, 44, 45, 46, 47, 48 };
        private static int[] DISTANCE_TWO_VISIBLE_CELLS = new[] { 8, 9, 10, 11, 12, 15, 16, 17, 18, 19, 22, 23, 24, 25, 26, 29, 30, 31, 32, 33, 36, 37, 38, 39, 40 };

        public static Cell[] GetApplicableCellArray(Cell cell, int distanceFromCell)
        {
            Cell[] cellArray = new Cell[DEFAULT_VISIBLE_CELLS];
            int bitcount = 0;

            try
            {
                // not currently used as of 11/6/2013 -- looking for a use
                if (distanceFromCell < 0)
                {
                    for (bitcount = 0; bitcount < cellArray.Length; bitcount++)
                    {
                        cellArray[bitcount] = null;
                    }

                    return cellArray;
                }

                int limitDistance = distanceFromCell;

                //if (distanceFromCell <= DEFAULT_VISIBLE_DISTANCE) distanceFromCell = DEFAULT_VISIBLE_DISTANCE;
                //else cellArray = new Cell[(distanceFromCell + distanceFromCell) * (distanceFromCell + distanceFromCell)]; // increase the size of the array for perimeter larger than 3

                if (distanceFromCell > DEFAULT_VISIBLE_DISTANCE) cellArray = new Cell[distanceFromCell * 3 * distanceFromCell * 3];

                for (int ypos = -distanceFromCell; ypos <= distanceFromCell; ypos++)
                {
                    for (int xpos = -distanceFromCell; xpos <= distanceFromCell; xpos++)
                    {
                        if (!CellRequestInBounds(cell, xpos, ypos)) cellArray[bitcount] = null;
                        else
                        {
                            //cellArray[bitcount] = GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X + xpos, cell.Y + ypos, cell.Z);
                            bool nonNullCell = true;

                            //if (limitDistance < DEFAULT_VISIBLE_DISTANCE)
                            //{
                            //    switch (limitDistance)
                            //    {
                            //        case 0:
                            //            if (bitcount != CENTER_VISIBLE_CELL)
                            //            {
                            //                cellArray[bitcount] = null; nonNullCell = false;
                            //            }
                            //            break;
                            //        case 1:
                            //            if (Array.IndexOf(DISTANCE_ONE_NULL_CELLS, bitcount) > -1)
                            //            {
                            //                cellArray[bitcount] = null; nonNullCell = false;
                            //            }
                            //            break;
                            //        case 2:
                            //            if (Array.IndexOf(DISTANCE_TWO_NULL_CELLS, bitcount) > -1)
                            //            {
                            //                cellArray[bitcount] = null; nonNullCell = false;
                            //            }
                            //            break;
                            //    }
                            //}

                            if (nonNullCell) cellArray[bitcount] = Cell.GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X + xpos, cell.Y + ypos, cell.Z);
                        }
                        bitcount++;
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                Utils.Log("CellArray.Length = " + cellArray.Length + ", bitcount = " + bitcount, Utils.LogType.Unknown);
            }

            return cellArray;
        }

        public static bool CellRequestInBounds(Cell cell, int xAdd, int yAdd)
        {
            if (cell == null || cell.Map == null)
            {
                return false;
            }

            int xMax = cell.Map.ZPlanes[cell.Z].xcordMax;
            int xMin = cell.Map.ZPlanes[cell.Z].xcordMin;
            int yMax = cell.Map.ZPlanes[cell.Z].ycordMax;
            int yMin = cell.Map.ZPlanes[cell.Z].ycordMin;

            if (xAdd >= 0)
            {
                if (cell.X + xAdd > xMax)
                {
                    return false;
                }
            }

            if (xAdd < 0)
            {
                if (cell.X + xAdd < xMin)
                {
                    return false;
                }
            }

            if (yAdd >= 0)
            {
                if (cell.Y + yAdd > yMax)
                {
                    return false;
                }
            }

            if (yAdd < 0)
            {
                if (cell.Y + yAdd < yMin)
                {
                    return false;
                }
            }
            
            return true;
        }

        public static Cell GetCell(int facetID, int landID, int mapID, int x, int y, int z)
        {
            try
            {
                //string key = "0,0,0";
                //key = x.ToString() + "," + y.ToString() + "," + z.ToString(); // crashing on too many nested calls
                //key = x + "," + y + "," + z; // same as above without the nested calls to ToString() - 1 crash here so far
                Tuple<int, int, int> key = new Tuple<int, int, int>(x, y, z);
                if (World.GetFacetByID(facetID).GetLandByID(landID).GetMapByID(mapID).cells.ContainsKey(key))
                {
                    return World.GetFacetByID(facetID).GetLandByID(landID).GetMapByID(mapID).cells[key];
                }
                //Utils.Log("Failure in Cell.GetCell(" + facetID + ", " + landID + ", " + mapID + ", " + x + ", " + y + ", " + z + ") returned null.", Utils.LogType.SystemFailure);
                return null;
            }
            catch (Exception e)
            {
                Utils.Log("Failure in Cell.GetCell(" + facetID + ", " + landID + ", " + mapID + ", " + x + ", " + y + ", " + z + ").", Utils.LogType.SystemFailure);
                Utils.LogException(e);
                return null;
            }
        }

        public static int GetCellDistance(int startXCord, int startYCord, int goalXCord, int goalYCord) // determine distance between two cells
        {
            try
            {
                if (startXCord == goalXCord && startYCord == goalYCord)
                {
                    return 0;
                }

                if (goalXCord <= Int32.MinValue || goalYCord <= Int32.MinValue)
                {
                    return 0;
                }

                int xAbsolute = Math.Abs(startXCord - goalXCord); // get absolute value of StartX - GoalX
                int yAbsolute = Math.Abs(startYCord - goalYCord); // get absolute value of StartY - GoalY

                if (xAbsolute > yAbsolute)
                {
                    return xAbsolute;
                }
                else
                {
                    return yAbsolute;
                }
            }
            catch (Exception e)
            {
                Utils.Log("Failure at getCellDistance(" + startXCord + ", " + startYCord + ", " + goalXCord + ", " + goalYCord + ")", Utils.LogType.SystemFailure);
                Utils.LogException(e);
                return 0;
            }
        }

        public static bool PassesCellLock(Character ch, Cell.CellLock cellLock, bool inform)
        {
            try
            {
                if (ch.IsImmortal) { return true; } // if immortal the ch may pass freely
                if (!ch.IsPC) { return false; } // NPCs fail the check automatically

                if (Array.IndexOf(cellLock.lockTypes, CellLock.LockType.None) != -1) // check if there is no cellLock on this teleport
                {
                    return true;
                }
                else
                {
                    // check Character.classType restrictions
                    if (Array.IndexOf(cellLock.lockTypes, CellLock.LockType.ClassType) != -1)
                    {
                        if (Array.IndexOf(cellLock.classTypes, ch.BaseProfession) == -1)
                        {
                            if (inform)
                            {
                                ch.WriteToDisplay("You failed to meet the profession restriction of a cell lock.");
                            }
                            return false;
                        }
                    }
                    // check World.Alignment restrictions
                    {
                        if (Array.IndexOf(cellLock.lockTypes, CellLock.LockType.Alignment) != -1)
                        {
                            if (Array.IndexOf(cellLock.alignmentTypes, ch.Alignment) == -1)
                            {
                                if (inform)
                                {
                                    ch.WriteToDisplay("You failed to meet the alignment restriction of a cell lock.");
                                }
                                return false;
                            }
                        }
                    }
                    // check World.dailyCycle restrictions
                    if (Array.IndexOf(cellLock.lockTypes, CellLock.LockType.DailyCycle) != -1)
                    {
                        if (Array.IndexOf(cellLock.dailyCycles, World.CurrentDailyCycle) == -1)
                        {
                            if (inform)
                            {
                                ch.WriteToDisplay("You failed to meet the daily cycle restriction of a cell lock.");
                            }
                            return false;
                        }
                    }
                    // check World.lunarCycle restrictions
                    if (Array.IndexOf(cellLock.lockTypes, CellLock.LockType.LunarCycle) != -1)
                    {
                        if (Array.IndexOf(cellLock.lunarCycles, World.CurrentLunarCycle) == -1)
                        {
                            if (inform)
                            {
                                ch.WriteToDisplay("You failed to meet the lunar cycle restriction of a cell lock.");
                            }
                            return false;
                        }
                    }
                    // check level requirements for teleport
                    if (Array.IndexOf(cellLock.lockTypes, CellLock.LockType.Level) != -1)
                    {
                        if (ch.Level < cellLock.minimumLevel)
                        {
                            if (inform)
                            {
                                ch.WriteToDisplay("You failed to meet the minimum level restriction of a cell lock.");
                            }
                            return false; } // minimumLevel
                        if (ch.Level > cellLock.maximumLevel)
                        {
                            if (inform)
                            {
                                ch.WriteToDisplay("You failed to meet the maximum level restriction of a cell lock.");
                            }
                            return false; } // maximumLevel
                    }
                    // check key requirements - item.name or item.key
                    if (Array.IndexOf(cellLock.lockTypes, CellLock.LockType.Key) != -1)
                    {
                        if (cellLock.heldKey)
                        {
                            if (ch.RightHand == null && ch.LeftHand == null)
                            {
                                if (inform)
                                {
                                    ch.WriteToDisplay("You failed to meet the key restriction of a cell lock.");
                                }
                                return false;
                            }
                            else if (ch.RightHand != null && ch.LeftHand == null)
                            {
                                if (!ch.RightHand.key.Contains(cellLock.key) && ch.RightHand.name != cellLock.key)
                                {
                                    if (inform)
                                    {
                                        ch.WriteToDisplay("You failed to meet the key restriction of a cell lock.");
                                    }
                                    return false;
                                }
                            }
                            else if (ch.RightHand == null && ch.LeftHand != null)
                            {
                                if (!ch.LeftHand.key.Contains(cellLock.key) && ch.LeftHand.name != cellLock.key)
                                {
                                    if (inform)
                                    {
                                        ch.WriteToDisplay("You failed to meet the key restriction of a cell lock.");
                                    }
                                    return false;
                                }
                            }
                            else if (ch.RightHand != null && ch.LeftHand != null)
                            {
                                if (!ch.RightHand.key.Contains(cellLock.key) && ch.RightHand.name != cellLock.key &&
                                    !ch.LeftHand.key.Contains(cellLock.key) && ch.LeftHand.name != cellLock.key)
                                {
                                    if (inform)
                                    {
                                        ch.WriteToDisplay("You failed to meet the key restriction of a cell lock.");
                                    }
                                    return false;
                                }
                            }
                        }
                        else if (cellLock.wornKey) // worn item key can be item.key or item.name
                        {
                            bool wornMatch = false;
                            
                            foreach (Item item in ch.wearing)
                            {
                                if (item.key.Contains(cellLock.key) || item.name == cellLock.key)
                                {
                                    wornMatch = true; break;
                                }
                            }
                            if (!wornMatch)
                            {
                                foreach (Item item in ch.GetRings())
                                {
                                    if (item.key.Contains(cellLock.key) || item.name == cellLock.key)
                                    {
                                        wornMatch = true; break;
                                    }
                                }
                            }
                            if (!wornMatch)
                            {
                                if (inform)
                                {
                                    ch.WriteToDisplay("You failed to meet the key restriction of a cell lock.");
                                }
                                return false;
                            }
                        }
                    }
                    // check Character.flagType restrictions
                    if (Array.IndexOf(cellLock.lockTypes, CellLock.LockType.Flag) != -1)
                    {
                        foreach (String qflag in ch.QuestFlags)
                        {
                            if (qflag != cellLock.key)
                            {
                                continue;
                            }
                            else if (qflag == cellLock.key) { return true; }
                        }
                        if (inform)
                        {
                            ch.WriteToDisplay("You failed to meet the flag restriction of a cell lock.");
                        }
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return true;
            }
        }

        public static bool IsNextToLand(Cell cell)
        {
            Cell[] cArray = GetApplicableCellArray(cell, 1);

            foreach(Cell c in cArray)
            {
                if (c != null && c.DisplayGraphic != GRAPHIC_WATER && c.DisplayGraphic != GRAPHIC_REEF)
                    return true;
            }

            return false;
        }

        public string GetLogString(bool verbose)
        {
            if(!verbose) return "(CELL) " + this.Land.ShortDesc + " - " + this.Map.Name + " " + this.X + ", " + this.Y + ", " + this.Z;
            else return "(CELL) " + this.Land.ShortDesc + " - " + this.Map.Name + " " + this.X + ", " + this.Y + ", " + this.Z + " Characters Count: " + Characters.Count + " Items Count: " + Items.Count;
        }

        public bool HasLightSource(out Globals.eLightSource lightsource)
        {
            Globals.eLightSource lightsourceToReturn = Globals.eLightSource.None;
            bool result = false;

            foreach (Character chr in this.Characters.Values)
            {
                Globals.eLightSource characterLightsource = Globals.eLightSource.None;
                if (chr.HasLightSource(out characterLightsource))
                {
                    lightsourceToReturn = (Globals.eLightSource)Math.Max((int)characterLightsource, (int)lightsourceToReturn);
                    result = true;
                }
            }

            if (this.AreaEffects.ContainsKey(Effect.EffectTypes.Light))
            {
                lightsourceToReturn = (Globals.eLightSource)Math.Max((int)Globals.eLightSource.LightSpell, (int)lightsourceToReturn);
                result = true;
            }

            if (this.AreaEffects.ContainsKey(Effect.EffectTypes.Fire) || this.AreaEffects.ContainsKey(Effect.EffectTypes.Dragon__s_Breath_Fire) ||
                this.AreaEffects.ContainsKey(Effect.EffectTypes.Fire_Storm) || this.AreaEffects.ContainsKey(Effect.EffectTypes.Ornic_Flame))
            {
                lightsourceToReturn = (Globals.eLightSource)Math.Max((int)Globals.eLightSource.AnyFireEffect, (int)lightsourceToReturn);
                result = true;
            }

            if (this.IsWithinTownLimits)
            {
                lightsourceToReturn = (Globals.eLightSource)Math.Max((int)Globals.eLightSource.TownLimits, (int)lightsourceToReturn);
                result = true;
            }

            foreach (Item item in this.Items)
            {
                if (item.blueglow)
                {
                    lightsourceToReturn = (Globals.eLightSource)Math.Max((int)Globals.eLightSource.WeakItemBlueglow, (int)lightsourceToReturn);
                    result = true;
                }
            }

            lightsource = lightsourceToReturn;
            return result;
        }
    }
}
