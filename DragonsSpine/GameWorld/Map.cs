using System;
using System.Collections.Generic;
using System.IO;

namespace DragonsSpine.GameWorld
{
    public class Map
    {
        public enum ClimateType { Temperate, Subtropical, Tropical, Desert, Tundra, Frozen, Subterranean }
        public enum Direction { None, North, South, East, West, Northeast, Southeast, Northwest, Southwest }
        public enum ForestedLevel { None, Barren, Very_Light, Light, Medium, Heavy, Very_Heavy, Random }
        public enum LightLevel { None, Dim, Low, Normal } // 0, 1, 2, 3
        public enum FogLevel { Heavy, Medium, Light, None } // 0, 1, 2, 3

        #region Map ID Constants
        public const int ID_KESMAI = 0;
        public const int ID_LENG = 1;
        public const int ID_AXEGLACIER = 2;
        public const int ID_OAKVAEL = 3;
        public const int ID_PRAETOSEBA = 4;
        public const int ID_ANNWN = 5;
        public const int ID_TORII = 6;
        public const int ID_SHUKUMEI = 7;
        public const int ID_RIFTGLACIER = 8;
        public const int ID_UNDERKINGDOM = 11;
        public const int ID_HELL = 12;
        #endregion       

        #region Reserved Map Graphics - Used when reading maps only.
        public const string RESERVED_GRAPHIC_ANCESTOR_START = "a0";
        public const string RESERVED_GRAPHIC_ANCESTOR_FINISH = "a1";
        public const string RESERVED_GRAPHIC_ONE_HAND_CLIMB_DOWN = "c0";
        public const string RESERVED_GRAPHIC_ONE_HAND_CLIMB_UP = "c1";
        public const string RESERVED_GRAPHIC_TWO_HAND_CLIMB_DOWN = "c2";
        public const string RESERVED_GRAPHIC_TWO_HAND_CLIMB_UP = "c3";
        public const string RESERVED_GRAPHIC_ONE_HAND_CLIMB_DOWN_IN_WATER = "~0";
        public const string RESERVED_GRAPHIC_ONE_HAND_CLIMB_UP_IN_WATER = "~1";
        public const string RESERVED_GRAPHIC_TWO_HAND_CLIMB_DOWN_IN_WATER = "~2";
        public const string RESERVED_GRAPHIC_TWO_HAND_CLIMB_UP_IN_WATER = "~3";
        public const string RESERVED_GRAPHIC_NO_RECALL_IN_WATER = "~r";
        public const string RESERVED_GRAPHIC_TOWN_LIMITS_IN_WATER = "~t";
        public const string RESERVED_GRAPHIC_BALM_FOUNTAIN = "~b";
        public const string RESERVED_GRAPHIC_LOCKERS = "LK";
        public const string RESERVED_GRAPHIC_ORNIC_LOCKERS = "EK";
        public const string RESERVED_GRAPHIC_IMPENETRABLE_WALL = "DD";
        public const string RESERVED_GRAPHIC_IMPENETRABLE_WALL_TOWN_LIMITS = "Dt";
        public const string RESERVED_GRAPHIC_WALL_TOWN_LIMITS = "[t";
        public const string RESERVED_GRAPHIC_SCRIBING_CRYSTAL = "SC";
        public const string RESERVED_GRAPHIC_MIRROR = "MR";
        public const string RESERVED_GRAPHIC_BOXING_RING = "BR";
        public const string RESERVED_GRAPHIC_ORNIC_FLAME = "*O";
        #endregion

        #region Private Data
        short facetID;
        short landID;
        short mapID;
        string name;
        string shortDesc;
        string longDesc;
        int suggestedMaximumLevel;
        int suggestedMinimumLevel;
        bool pvpEnabled;
        double experienceModifier; // multiplier for modification to NPC experience value
        int difficulty; // not currently used
        ClimateType climate; // map climate - not currently used
        bool balmBushes = false;
        bool manaBushes = false;
        bool poisonBushes = false;
        bool staminaBushes = false;
        short resX = -1; // normal respawn xcord
        short resY = -1; // normal respawn ycord
        int resZ = -1; // normal respawn zcord
        short thiefResX = -1; // thief respawn xcord
        short thiefResY = -1; // thief respawn ycord
        int thiefResZ = -1; // thief respawn zcord
        short karmaResX = -1; // karma respawn xcord
        short karmaResY = -1; // karma respawn ycord
        int karmaResZ = -1; // karma respawn zcord
        short evilResX = -1;
        short evilResY = -1;
        int evilResZ = -1;
        Dictionary<int, ZPlane> zPlaneInfos = new Dictionary<int, ZPlane>(); // int = z
        bool randomMagicIntensity = false;
        #endregion

        #region Public Properties
        public short FacetID
        {
            get { return this.facetID; }
        }
        public short LandID
        {
            get { return this.landID; }
        }
        public short MapID
        {
            get { return this.mapID; }
        }
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        public string ShortDesc
        {
            get { return this.shortDesc; }
            set { this.shortDesc = value; }
        }
        public string LongDesc
        {
            get { return this.longDesc; }
            set { this.longDesc = value; }
        }
        public int SuggestedMaximumLevel
        {
            get { return this.suggestedMaximumLevel; }
            set { this.suggestedMaximumLevel = value; }
        }
        public int SuggestedMinimumLevel
        {
            get { return this.suggestedMinimumLevel; }
            set { this.suggestedMinimumLevel = value; }
        }
        public bool IsPVPEnabled
        {
            get { return this.pvpEnabled; }
            set { this.pvpEnabled = value; }
        }
        public double ExperienceModifier
        {
            get { return this.experienceModifier; }
            set { this.experienceModifier = value; }
        }
        public int Difficulty
        {
            get { return this.difficulty; }
            set { this.difficulty = value; }
        }
        public ClimateType Climate
        {
            get { return this.climate; }
            set { this.climate = value; }
        }
        public bool HasBalmBushes
        {
            get { return this.balmBushes; }
            set { this.balmBushes = value; }
        }
        public bool HasManaBushes
        {
            get { return this.manaBushes; }
            set { this.manaBushes = value; }
        }
        public bool HasPoisonBushes
        {
            get { return this.poisonBushes; }
            set { this.poisonBushes = value; }
        }
        public bool HasStaminaBushes
        {
            get { return this.staminaBushes; }
            set { this.staminaBushes = value; }
        }
        public short ResX
        {
            get { return this.resX; }
            set { this.resX = value; }
        }
        public short ResY
        {
            get { return this.resY; }
            set { this.resY = value; }
        }
        public int ResZ
        {
            get { return this.resZ; }
            set { this.resZ = value; }
        }
        public short ThiefResX
        {
            get { return this.thiefResX; }
            set { this.thiefResX = value; }
        }
        public short ThiefResY
        {
            get { return this.thiefResY; }
            set { this.thiefResY = value; }
        }
        public int ThiefResZ
        {
            get { return this.thiefResZ; }
            set { this.thiefResZ = value; }
        }
        public short KarmaResX
        {
            get { return this.karmaResX; }
            set { this.karmaResX = value; }
        }
        public short KarmaResY
        {
            get { return this.karmaResY; }
            set { this.karmaResY = value; }
        }
        public int KarmaResZ
        {
            get { return this.karmaResZ; }
            set { this.karmaResZ = value; }
        }
        public short EvilResX
        {
            get { return this.evilResX; }
            set { this.evilResX = value; }
        }
        public short EvilResY
        {
            get { return this.evilResY; }
            set { this.evilResY = value; }
        }
        public int EvilResZ
        {
            get { return this.evilResZ; }
            set { this.evilResZ = value; }
        }
        public Dictionary<int, ZPlane> ZPlanes
        {
            get { return this.zPlaneInfos; }
        }
        public bool HasRandomMagicIntensity
        {
            get { return this.randomMagicIntensity; }
        }
        #endregion

        public System.Collections.Concurrent.ConcurrentDictionary<Tuple<int, int, int>, Cell> cells = new System.Collections.Concurrent.ConcurrentDictionary<Tuple<int, int, int>, Cell>(); // Tuple = x, y, z

        #region Constructor
        public Map(short facetID, System.Data.DataRow dr)
        {
            this.facetID = facetID;
            this.landID = Convert.ToInt16(dr["landID"]);
            this.mapID = Convert.ToInt16(dr["mapID"]);
            this.Name = dr["name"].ToString();
            this.ShortDesc = dr["shortDesc"].ToString();
            this.LongDesc = dr["longDesc"].ToString();
            this.SuggestedMaximumLevel = Convert.ToInt32(dr["suggestedMaximumLevel"]);
            this.SuggestedMinimumLevel = Convert.ToInt32(dr["suggestedMinimumLevel"]);
            this.IsPVPEnabled = Convert.ToBoolean(dr["pvpEnabled"]);
            this.ExperienceModifier = Convert.ToDouble(dr["expModifier"]);
            this.Difficulty = Convert.ToInt16(dr["difficulty"]);
            this.Climate = (Map.ClimateType)Convert.ToInt16(dr["climateType"]);
            this.HasBalmBushes = Convert.ToBoolean(dr["balmBushes"]);
            this.HasPoisonBushes = Convert.ToBoolean(dr["poisonBushes"]);
            this.HasManaBushes = Convert.ToBoolean(dr["manaBushes"]);
            this.HasStaminaBushes = Convert.ToBoolean(dr["staminaBushes"]);
            this.ResX = Convert.ToInt16(dr["resX"]);
            this.ResY = Convert.ToInt16(dr["resY"]);
            this.ResZ = Convert.ToInt16(dr["resZ"]);
            this.ThiefResX = Convert.ToInt16(dr["thiefResX"]);
            this.ThiefResY = Convert.ToInt16(dr["thiefResY"]);
            this.ThiefResZ = Convert.ToInt32(dr["thiefResZ"]);
            this.KarmaResX = Convert.ToInt16(dr["karmaResX"]);
            this.KarmaResY = Convert.ToInt16(dr["karmaResY"]);
            this.KarmaResZ = Convert.ToInt32(dr["karmaResZ"]);
            this.EvilResX = Convert.ToInt16(dr["evilResX"]);
            this.EvilResY = Convert.ToInt16(dr["evilResY"]);
            this.EvilResZ = Convert.ToInt32(dr["evilResZ"]);
            this.randomMagicIntensity = Convert.ToBoolean(dr["randomMagicIntensity"]);
        }
        #endregion

        #region ANSI Codes
        public const string CLS = "\x1b[2J\x1b[1;1f";
        public const string CLRLN = "\x1b[K";
        public const string CEND = "\x1b[0m";
        public const string SAVECUR = "\x1b[s";
        public const string LOADCUR = "\x1b[u";

        public const string CGRY = "\x1b[37m";
        public const string CNRM = "\x1B[0;0m";
        public const string CRED = "\x1B[31m";
        public const string CGRN = "\x1B[32m";
        public const string CYEL = "\x1B[33m";
        public const string CBLU = "\x1B[34m";
        public const string CMAG = "\x1B[35m";
        public const string CCYN = "\x1B[36m";
        public const string CWHT = "\x1B[37m";
        public const string CBLK = "\x1B[30m";

        /*              Bold Colors            */
        public const string BGRY = "\x1b[1;30m";
        public const string BRED = "\x1B[1;31m";
        public const string BGRN = "\x1B[1;32m";
        public const string BYEL = "\x1B[1;33m";
        public const string BBLU = "\x1B[1;34m";
        public const string BMAG = "\x1B[1;35m";
        public const string BCYN = "\x1B[1;36m";
        public const string BWHT = "\x1B[1;37m";
        public const string BBLK = "\x1B[1;30m";

        /*             Backgrounds             */
        public const string BKRED = "\x1B[41m";
        public const string BKGRN = "\x1B[42m";
        public const string BKYEL = "\x1B[43m";
        public const string BKBLU = "\x1B[44m";
        public const string BKMAG = "\x1B[45m";
        public const string BKCYN = "\x1B[46m";
        public const string BKWHT = "\x1B[47m";
        public const string BKBLK = "\x1B[40m";
        #endregion

        #region Kesmai Protocol
        #region Tiles
        public const char ESC = '\x1B';
        public const char KPDARKNESS = (char)32;
        public const char KPEMPTY = (char)33;
        public const char KPROOM = (char)34;
        public const char KPAIR = (char)35;
        public const char KPWEB = (char)36;
        public const char KPHCDOOR = (char)37;
        public const char KPVCDOOR = (char)38;
        public const char KPHODOOR = (char)39;
        public const char KPVODOOR = (char)40;
        public const char KPFIRE = (char)41;
        public const char KPICE = (char)42;
        public const char KPRUIN1 = (char)43;
        public const char KPRUIN2 = (char)44;
        public const char KPSTAIRUP = (char)45;
        public const char KPSTAIRDN = (char)46;
        public const char KPREEF = (char)47;
        public const char KPWATER = (char)48;
        public const char KPTREE1 = (char)49;
        public const char KPTREE2 = (char)50;
        public const char KPFOREST1 = (char)51;
        public const char KPFOREST2 = (char)52;
        public const char KPDOCK = (char)53;
        public const char KPBRIDGE = (char)54;
        public const char KPIMPTREE = (char)55;
        public const char KPBURNT1 = (char)56;
        public const char KPBURNT2 = (char)57;
        public const char KPPATH = (char)58;
        public const char KPMOUNTAIN = (char)59;
        public const char KPIMPMOUNT = (char)60;
        public const char KPALTAR = (char)61;
        public const char KPSHOPCOUNTER = (char)62;
        public const char KPPIT = (char)63;
        public const char KPGRATE = (char)64;
        public const char KPRING = (char)65;
        public const char KPTRASH = (char)66;
        public const char KPGRASS = (char)67;
        public const char KPSAND = (char)68;
        public const char KPTILE = (char)69;
        public const char KPWALL = (char)70;
        public const char KPGRAVE = (char)71;
        public const char KPIMPDARK = (char)73;
        public const char KPFIELD = (char)82;
        public const char KPLIGHT = (char)83;
        public const char KPLINECOUNTER = (char)84;
        public const char KPNOVIS = (char)85;
        public const char KPEND = (char)127;
        #endregion
        #region KP Strings
        /// <summary>
        /// Cursor Up - (L 1)
        /// </summary>
        public const string KP_MOVE_CURS_UP = "\x1B" + "A";
        /// <summary>
        /// Cursor Down - (L 1)
        /// </summary>
        public const string KP_MOVE_CURS_DN = "\x1B" + "B";
        /// <summary>
        /// Cursor Right - (L 1)
        /// </summary>
        public const string KP_MOVE_CURS_RT = "\x1B" + "C";
        /// <summary>
        /// Cursor Left - (L 1)
        /// </summary>
        public const string KP_MOVE_CURS_LT = "\x1B" + "D";
        /// <summary>
        /// Enter Enhancer Mode - (L 1)
        /// </summary>
        public const string KP_ENHANCER_ENABLE = "\x1B" + "E";
        /// <summary>
        /// Exit Enhancer Mode - (L 1) 
        /// </summary>
        public const string KP_ENHANCER_DISABLE = "\x1B" + "G";
        /// <summary>
        /// Move the cursor to the home position (1,1). - (L 1)
        /// </summary>
        public const string KP_MOVE_CURS_HOME = "\x1B" + "H";
        /// <summary>
        /// Reverse Line Feed - (L 1)
        /// </summary>
        public const string KP_REV_LINE_FEED = "\x1B" + "I";
        /// <summary>
        /// Erase to End of Screen - (L 1)
        /// </summary>
        public const string KP_ERASE_TO_END = "\x1B" + "J";
        /// <summary>
        /// Erase to end of current line - (L 1)
        /// </summary>
        public const string KP_ERASE_END_LINE = "\x1B" + "K";
        /// <summary>
        /// Clear all the lines of text in the display window that 
        /// have not been written on since the last cleanup.
        /// </summary>
        public const string KP_CLEAN_ACTIVE = "\x1B" + "L";
        /// <summary>
        /// Position the text cursor to the next available position in
        /// the text display window and clear the line of text.
        /// </summary>
        public const string KP_NEXT_LINE = "\x1B" + "M";
        /// <summary>
        /// [f][num] [,[f][num]] ; Set Numeric field
        /// Set the numeric field specified by [f] to the value specified by
        /// [num].  The field id is a single ASCII character whose value is
        /// the desired field plus 31.  You can use more than 1 of the pairs
        /// each separated by commas and terminated by a semicolon.
        /// </summary>
        public const string KP_SET_NUM_FIELD = "\x1B" + "N";
        /// <summary>
        /// [tstr] Picture Terrrain Update String
        /// The first byte of [tstr] corresponds
        /// to picture position 1,1; the second byte corresponds to 1,2.
        /// </summary>
        public const string KP_PICTURE_TERRAIN_UPDATE = "\x1B" + "P";
        /// <summary>
        /// Play the sound effect specified by [snd]
        /// </summary>
        public const string KP_PLAY_SOUND = "\x1B" + "S";
        /// <summary>
        /// [dir] [istr] [end] Picture Icon Update String
        /// This function is used to send the critters and treasure data
        /// for the picture.  The [dir] bytes is the player course arrow.
        /// [istr] is composed of an arbitary number of three character
        /// packets that are:
        /// byte 1 : picture position 31 + (1..49)
        /// byte 2 : icon number : (treasure pile = 32) (a critter = 33) (several critters = 34) 
        /// byte 3 : display character A..L
        /// The end of the string [end] is an ascii 127
        /// </summary>
        public const string KP_PICTURE_ICON_UPDATE = "\x1B" + "U";
        /// <summary>
        /// [eff][p] Display Visual Effect (L 4)
        /// Display the specified visual effect [eff] centered at the specified
        /// map position [p].  The effect and position numbers are sent
        /// as ASCII codes whose whose values are the desired number plus
        /// 31.  The position number ranges from 1 to 49 corresponding
        /// to position (1,1) and (7,7) in the picture map.  The numbers
        /// are in row major order.
        /// </summary>
        public const string KP_DISPLAY_VISUAL_EFFECT = "\x1B" + "V";
        /// <summary>
        /// Direct cursor access [l][c]
        /// Move the cursor to the specified line [l] and column [c].  The
        /// line and column numbers are sent as ASCII codes whose values
        /// are the desired position plus 31. e.g. 32 refers to the first
        /// line or column.
        /// </summary>
        public const string KP_DIRECT_CURS = "\x1B" + "Y";
        #endregion
        #endregion

        #region Static Methods
        public static Direction GetDirection(Cell from, Cell to)
        {
            if (from == null || to == null) return Direction.None;

            try
            {
                if (from.X < to.X && from.Y < to.Y)
                {
                    return Direction.Southeast;
                }
                else if (from.X < to.X && from.Y > to.Y)
                {
                    return Direction.Northeast;
                }
                else if (from.X > to.X && from.Y < to.Y)
                {
                    return Direction.Southwest;
                }
                else if (from.X > to.X && from.Y > to.Y)
                {
                    return Direction.Northwest;
                }
                else if (from.X == to.X && from.Y > to.Y)
                {
                    return Direction.North;
                }
                else if (from.X == to.X && from.Y < to.Y)
                {
                    return Direction.South;
                }
                else if (from.X < to.X && from.Y == to.Y)
                {
                    return Direction.East;
                }
                else if (from.X > to.X && from.Y == to.Y)
                {
                    return Direction.West;
                }
                else
                {
                    return Direction.None; // same cell
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return Direction.None;
            }
        }

        public static void WriteMapStringAtXY(Character ch, int y, int x, string str, Cell cell)
        {
            ch.Write("\x1b[" + x + ";" + y + "H");
            PrintMapString(ch, str, cell);
        }

        public static void WriteAtXY(Character ch, int y, int x, string str)
        {
            ch.Write("\x1b[" + x + ";" + y + "H");
            ch.Write(str);
            //printTile(ch, str);
        }

        public static void PrintMapString(Character ch, string mapTile, Cell cell)
        {
            switch (mapTile)
            {
                case Cell.GRAPHIC_WALL:
                    ch.Write(CYEL + Cell.GRAPHIC_WALL + CEND);
                    break;
                case Cell.GRAPHIC_RUINS_LEFT:
                case Cell.GRAPHIC_RUINS_RIGHT:
                    ch.Write(CMAG + mapTile + CEND);
                    break;
                case Cell.GRAPHIC_MOUNTAIN:
                    ch.Write(BBLK + Cell.GRAPHIC_MOUNTAIN + CEND);
                    break;
                case Cell.GRAPHIC_AIR:
                    ch.Write(BWHT + Cell.GRAPHIC_AIR + CEND);
                    break;
                case Cell.GRAPHIC_WATER:
                    if (cell.IsBalmFountain) ch.Write(BKWHT + BBLU + Cell.GRAPHIC_WATER + CEND);
                    else ch.Write(BKBLU + BWHT + Cell.GRAPHIC_WATER + CEND);
                    break;
                case Cell.GRAPHIC_WEB:
                    ch.Write(BKWHT + BBLK + Cell.GRAPHIC_WEB + CEND);
                    break;
                case Cell.GRAPHIC_REEF:
                    ch.Write(BKBLU + BBLK + Cell.GRAPHIC_WATER + CEND);
                    break;
                case Cell.GRAPHIC_ICE_WALL:
                case Cell.GRAPHIC_ICE_STORM:
                case Cell.GRAPHIC_ICE:
                    ch.Write(BKCYN + BCYN + mapTile + CEND);
                    break;
                case Cell.GRAPHIC_GRASS_THICK:
                case Cell.GRAPHIC_GRASS_LIGHT:
                    ch.Write(CGRN + mapTile + CEND);
                    break;
                case Cell.GRAPHIC_FOREST_FROSTY_LEFT:
                case Cell.GRAPHIC_FOREST_FROSTY_RIGHT:
                case Cell.GRAPHIC_FOREST_FROSTY_FULL:
                    ch.Write(BWHT + mapTile + CEND);
                    break;
                case Cell.GRAPHIC_FOREST_LEFT:
                case Cell.GRAPHIC_FOREST_RIGHT:
                case Cell.GRAPHIC_FOREST_FULL:
                    ch.Write(BGRN + mapTile + CEND);
                    break;
                case Cell.GRAPHIC_FOREST_IMPASSABLE:
                    ch.Write(BKGRN + BBLK + mapTile + CEND);
                    break;
                case Cell.GRAPHIC_BARREN_LEFT:
                case Cell.GRAPHIC_BARREN_RIGHT:
                case Cell.GRAPHIC_BARREN_FULL:
                    ch.Write(BBLK + mapTile + CEND);
                    break;
                case Cell.GRAPHIC_FIRE:
                case "*0":
                case "*1":
                case "*2":
                case "*3":
                case "*4":
                case "*5":
                case "*6":
                case "*7":
                case "*8":
                case "*9":
                    if (cell.AreaEffects.ContainsKey(Effect.EffectTypes.Ornic_Flame)) ch.Write(BKMAG + BWHT + Cell.GRAPHIC_FIRE + CEND);
                    else ch.Write(BKRED + BYEL + Cell.GRAPHIC_FIRE + CEND);
                    break;
                case Cell.GRAPHIC_ALTAR:
                case Cell.GRAPHIC_ALTAR_PLACEABLE:
                    ch.Write(CMAG + Cell.GRAPHIC_ALTAR + CEND);
                    break;
                case Cell.GRAPHIC_SAND:	//Sand
                    ch.Write(BYEL + Cell.GRAPHIC_SAND + CEND);
                    break;
                case Cell.GRAPHIC_LOOT_SYMBOL:
                    ch.Write(BYEL + Cell.GRAPHIC_LOOT_SYMBOL + CEND);
                    break;
                case Cell.GRAPHIC_BRIDGE:
                    ch.Write(CRED + Cell.GRAPHIC_BRIDGE + CEND);
                    break;
                case Map.RESERVED_GRAPHIC_BOXING_RING:
                case Cell.GRAPHIC_BOXING_RING:
                    ch.Write(BKYEL + CGRY + Cell.GRAPHIC_BOXING_RING + CEND);
                    break;

                case Cell.GRAPHIC_ACID_STORM:
                    ch.Write(BGRN + Cell.GRAPHIC_ACID_STORM + CEND);
                    break;
                case Cell.GRAPHIC_LIGHTNING_STORM:
                    ch.Write(BBLU + Cell.GRAPHIC_LIGHTNING_STORM + CEND);
                    break;
                case Cell.GRAPHIC_LOCUST_SWARM:
                    ch.Write(BYEL + Cell.GRAPHIC_LOCUST_SWARM + CEND);
                    break;
                case Cell.GRAPHIC_POISON_CLOUD:
                    ch.Write(BGRN + Cell.GRAPHIC_POISON_CLOUD + CEND);
                    break;
                default:
                    ch.Write(mapTile);
                    break;
            }
        }

        public static char kesProtoConvertGraphic(string cellgraphic)
        {
            char kGraphic;
            switch (cellgraphic)
            {
                case Cell.GRAPHIC_TRASHCAN:
                    kGraphic = Map.KPTRASH;
                    break;
                case Cell.GRAPHIC_EMPTY:
                    kGraphic = Map.KPEMPTY;
                    break;
                case Cell.GRAPHIC_WALL:
                    kGraphic = Map.KPWALL;
                    break;
                case Cell.GRAPHIC_RUINS_LEFT:
                    kGraphic = Map.KPRUIN1;
                    break;
                case Cell.GRAPHIC_RUINS_RIGHT:
                    kGraphic = Map.KPRUIN2;
                    break;
                case "ZZ":
                    kGraphic = Map.KPSTAIRUP;
                    break;
                case "XX":
                    kGraphic = Map.KPSTAIRDN;
                    break;
                case Cell.GRAPHIC_AIR: //Air
                    kGraphic = Map.KPAIR;
                    break;
                case Cell.GRAPHIC_WATER:
                    kGraphic = Map.KPWATER;
                    break;
                case Cell.GRAPHIC_REEF: //Unpassable water
                    kGraphic = Map.KPREEF;
                    break;
                case Cell.GRAPHIC_ICE:
                    kGraphic = Map.KPICE;
                    break;
                case Cell.GRAPHIC_FOREST_BURNT_LEFT:
                case Cell.GRAPHIC_FOREST_BURNT_FULL: //Burnt tree
                    kGraphic = Map.KPBURNT1;
                    break;
                case Cell.GRAPHIC_FOREST_BURNT_RIGHT: //Burnt tree
                    kGraphic = Map.KPBURNT2;
                    break;
                case "t*": //Burning tree
                    kGraphic = Map.KPFIRE;
                    break;
                case "*t": //Burning tree
                    kGraphic = Map.KPFIRE;
                    break;
                case Cell.GRAPHIC_FOREST_LEFT:	//Trees
                    kGraphic = Map.KPTREE1;
                    break;
                case Cell.GRAPHIC_FOREST_RIGHT:
                    kGraphic = Map.KPTREE2;
                    break;
                case Cell.GRAPHIC_FOREST_FULL: //Thick trees
                    kGraphic = Map.KPFOREST1;
                    break;
                case Cell.GRAPHIC_FOREST_IMPASSABLE:
                    kGraphic = Map.KPIMPTREE;
                    break;
                case "()":
                    kGraphic = Map.KPPIT;
                    break;
                case Cell.GRAPHIC_BOXING_RING:
                    kGraphic = Map.KPRING;
                    break;
                case ", ": //Ashes
                    kGraphic = Map.KPEMPTY;
                    break;
                case "@$":
                    kGraphic = Map.KPTREE1;
                    break;
                case Cell.GRAPHIC_FIRE:
                    kGraphic = Map.KPFIRE;
                    break;
                case "v*":
                    kGraphic = Map.KPFIRE;
                    break;
                case "PS":
                    kGraphic = Map.KPEMPTY;
                    break;
                case "LK":
                    kGraphic = Map.KPROOM;
                    break;
                case Cell.GRAPHIC_DARKNESS:
                    kGraphic = Map.KPDARKNESS;
                    break;
                case ".$":
                    kGraphic = Map.KPEMPTY;
                    break;
                case "cd":
                    kGraphic = Map.KPEMPTY;
                    break;
                case "cu":
                    kGraphic = Map.KPEMPTY;
                    break;
                case "tp": //Two way teleporter
                    kGraphic = Map.KPTILE;
                    break;
                case "pb":
                    kGraphic = Map.KPEMPTY;
                    break;
                case "MP": //Map Portal
                    kGraphic = Map.KPTILE;
                    break;
                case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:
                case Cell.GRAPHIC_LOCKED_DOOR_VERTICAL:
                    kGraphic = Map.KPVCDOOR;
                    break;
                case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:
                case Cell.GRAPHIC_LOCKED_DOOR_HORIZONTAL:
                    kGraphic = Map.KPHCDOOR;
                    break;
                case Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL:
                    kGraphic = Map.KPHODOOR;
                    break;
                case Cell.GRAPHIC_OPEN_DOOR_VERTICAL:
                    kGraphic = Map.KPVODOOR;
                    break;
                case Cell.GRAPHIC_ALTAR:
                case Cell.GRAPHIC_ALTAR_PLACEABLE:
                    kGraphic = Map.KPALTAR;
                    break;
                case Cell.GRAPHIC_COUNTER:
                case Cell.GRAPHIC_COUNTER_PLACEABLE:
                    kGraphic = Map.KPSHOPCOUNTER;
                    break;
                case Cell.GRAPHIC_SAND:
                    kGraphic = Map.KPSAND;
                    break;
                case Cell.GRAPHIC_MOUNTAIN:
                    kGraphic = Map.KPIMPMOUNT;
                    break;
                case Cell.GRAPHIC_GRATE:
                    kGraphic = Map.KPGRATE;
                    break;
                case Cell.GRAPHIC_BRIDGE:
                    kGraphic = Map.KPBRIDGE;
                    break;
                case Cell.GRAPHIC_UPSTAIRS:
                    kGraphic = Map.KPSTAIRUP;
                    break;
                case Cell.GRAPHIC_DOWNSTAIRS:
                    kGraphic = Map.KPSTAIRDN;
                    break;
                case "\"":
                    kGraphic = Map.KPGRASS;
                    break;
                case "gg":
                    kGraphic = Map.KPGRAVE;
                    break;
                default:
                    kGraphic = Map.KPNOVIS;
                    break;
            }
            return kGraphic;
        }

        public static void ClearMap(Character ch)
        {
            if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                ch.Write(Map.KP_DIRECT_CURS + (char)32 + (char)32 + Map.KP_ERASE_TO_END);
            if (ch.protocol == "old-kesmai")
                ch.Write(Map.KP_DIRECT_CURS + (char)32 + (char)32 + Map.KP_ERASE_TO_END);
            else
                ch.Write(Map.CLS);
        }

        public static string returnTile(string mapTile)
        {
            switch (mapTile)
            {
                case "  ":	//Vision blocked
                case Cell.GRAPHIC_WALL:	//Impenetrable Wall
                case Cell.GRAPHIC_RUINS_LEFT:	//Ruins
                case Cell.GRAPHIC_RUINS_RIGHT:	//Ruins
                case Cell.GRAPHIC_GRATE:  //Grate
                case Cell.GRAPHIC_FOREST_LEFT:	//Tree
                case Cell.GRAPHIC_FOREST_RIGHT:	//Tree
                case Cell.GRAPHIC_FOREST_FULL:  //Trees (vision blocked)
                case Cell.GRAPHIC_FOREST_IMPASSABLE:	//Impassable trees
                case Cell.GRAPHIC_AIR:  //Air
                case Cell.GRAPHIC_WATER:  //Water
                case Cell.GRAPHIC_REEF: //Impassable water
                case Cell.GRAPHIC_ICE:	//Ice
                case Cell.GRAPHIC_BRIDGE:	//Bridge or Dock
                case Cell.GRAPHIC_SAND:	//Sand
                case Cell.GRAPHIC_GRASS_THICK:
                case Cell.GRAPHIC_GRASS_LIGHT:	//Grass
                    return mapTile;
                case Cell.GRAPHIC_ALTAR_PLACEABLE: //Altar
                    return Cell.GRAPHIC_ALTAR;
                case Cell.GRAPHIC_FIRE:	//Fire
                case "*0":
                case "*1":
                case "*2":
                case "*3":
                case "*4":
                case "*5":
                case "*6":
                case "*7":
                case "*8":
                case "*9":
                    return Cell.GRAPHIC_FIRE;
                case "LK":
                    return Cell.GRAPHIC_EMPTY;
                case "$$":
                    return Cell.GRAPHIC_EMPTY;
                case "ZZ":	//Segueway up
                case "RU":
                    return Cell.GRAPHIC_UPSTAIRS;
                case "XX":  //Segueway down
                case "RD":
                    return Cell.GRAPHIC_DOWNSTAIRS;
                case "cd":	//Climb down
                case "cu":	//Climb up
                case "pb":	//Pit bottom
                case "gg":
                    return Cell.GRAPHIC_EMPTY;
                case Cell.GRAPHIC_LOCKED_DOOR_VERTICAL:
                    return Cell.GRAPHIC_CLOSED_DOOR_VERTICAL;
                case Cell.GRAPHIC_LOCKED_DOOR_HORIZONTAL:
                    return Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL;
                case "CC":
                    return Cell.GRAPHIC_COUNTER;
                case Cell.GRAPHIC_BOXING_RING:
                case Map.RESERVED_GRAPHIC_BOXING_RING:
                    return Cell.GRAPHIC_BOXING_RING;
                case "MP":
                    return Cell.GRAPHIC_BRIDGE;
                case Cell.GRAPHIC_DARKNESS:
                    //return "  ";
                    return Cell.GRAPHIC_EMPTY;
                case "ee"://concussion traps
                case "e1"://concussion traps
                case "e2":
                case "e3":
                case "e4":
                case "e5":
                case "e6":
                case "e7":
                case "e8":
                case "e9":
                case "e0":
                    return Cell.GRAPHIC_EMPTY;
                case "wb"://web
                case Cell.GRAPHIC_WEB:
                case "w1":
                case "w2":
                case "w3":
                case "w4":
                case "w5":
                case "w6":
                case "w7":
                case "w8":
                case "w9":
                case "w0":
                    return Cell.GRAPHIC_WEB;
                default:
                    return mapTile;
            }
        }

        public static bool IsVisionBlocked(Cell cell)
        {
            // I believe this was causing a map display issue. 1/20/2017 Eb
            //if (cell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) || cell.IsAlwaysDark)
            //    return true;

            string cellGraphic = cell.DisplayGraphic;

            switch (cell.DisplayGraphic)
            {
                case Cell.GRAPHIC_WALL:
                case Cell.GRAPHIC_MOUNTAIN:
                case Cell.GRAPHIC_FOG:
                case Cell.GRAPHIC_FOREST_IMPASSABLE:
                case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:
                case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:
                case Cell.GRAPHIC_REEF:
                case Cell.GRAPHIC_FOREST_FULL: //TODO: future, if Ranger then vision is not blocked
                case Cell.GRAPHIC_DARKNESS:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSpellPathBlocked(Cell cell)
        {
            if (cell == null) return true;
            //if (cell.IsMagicDead) return true;
            //if (cell.IsWithinTownLimits) return true;

            switch (cell.DisplayGraphic)
            {
                case Cell.GRAPHIC_WALL:
                case Cell.GRAPHIC_MOUNTAIN:
                case Cell.GRAPHIC_FOREST_IMPASSABLE:
                //case Cell.GRAPHIC_WATER:
                //case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:
                //case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:
                //case Cell.GRAPHIC_AIR:
                case Cell.GRAPHIC_REEF:
                //case Cell.GRAPHIC_GRATE:  mlt we are going to allow spells and range weaps through grates
                //case Cell.GRAPHIC_ALTAR:
                //case Cell.GRAPHIC_COUNTER:
                //case Cell.GRAPHIC_ALTAR_PLACEABLE:
                //case Cell.GRAPHIC_COUNTER_PLACEABLE:
                    return true;
                default:
                    return false;
            }
        }

        public static void MoveCharacter(Character ch, string direction, string args)
        {
            if (ch is PC && !ch.IsImmortal)
            {
                foreach (CommandTasker.CommandType cmd in ch.CommandsProcessed)
                {
                    // Currently only for when attacks force a target (ch) to move. 1/29/2019 Eb
                    try
                    {
                        if ((string)ch.TemporaryStorage == "override movement")
                            continue;
                    }
                    catch { continue; }

                    if (CommandTasker.FullRoundCommands.Contains(cmd) && cmd != CommandTasker.CommandType.Crawl && cmd != CommandTasker.CommandType.Sweep)
                    {
                        ch.WriteToDisplay("You cannot " + Utils.FormatEnumString(cmd.ToString()).ToLower() + " and move in the same round.");
                        
                        string commandsProcessed = "";
                        
                        foreach (CommandTasker.CommandType command in ch.CommandsProcessed)
                            commandsProcessed += command.ToString() + " ";

                        return;
                    }
                }
            }

            //switch (ch.FirstJoinedCommand) // commands that cannot be done simultaneously while moving
            //{
            //    case CommandTasker.CommandType.Attack:
            //    case CommandTasker.CommandType.ShieldBash:
            //    case CommandTasker.CommandType.Cast:
            //    case CommandTasker.CommandType.Chant:
            //    case CommandTasker.CommandType.Crawl:
            //    case CommandTasker.CommandType.Jumpkick:
            //    case CommandTasker.CommandType.Kick:
            //    case CommandTasker.CommandType.Movement:
            //    case CommandTasker.CommandType.Poke:
            //    case CommandTasker.CommandType.Rest:
            //    case CommandTasker.CommandType.Meditate:
            //    case CommandTasker.CommandType.Shoot:
            //    case CommandTasker.CommandType.Throw:
            //    case CommandTasker.CommandType.Climb:
            //        if (!ch.IsImmortal && !ch.EffectsList.ContainsKey(Effect.EffectTypes.Speed))
            //        {
            //            ch.WriteToDisplay("You cannot " + Utils.FormatEnumString(ch.FirstJoinedCommand.ToString()) + " and move in the same round.");
            //            return;
            //        }
            //        break;
            //    default:
            //        break;

            //}

            //TODO: fix this again 7/5/2014
            //if (ch.CommandType != CommandTasker.CommandType.Crawl)//mlt added if to fix crawl stun
            //    ch.CommandType = CommandTasker.CommandType.Movement;

            // don't kill a character this way?
            if (ch.Hits == 1 && ch.Stamina == 0 && Rules.GetEncumbrance(ch) > Globals.eEncumbranceLevel.Lightly)
            {
                ch.WriteToDisplay("You are too exhausted to move.");
                return;
            }

            ch.CommandType = CommandTasker.CommandType.Movement;

            int i = 0;
            int landnum = ch.CurrentCell.LandID;
            int mapnum = ch.CurrentCell.MapID;
            int xcord = ch.X;
            int ycord = ch.Y;
            int zcord = ch.Z;
            int count = 0;

            string[] dir = new string[] { "", "", "" };

            dir[0] = direction;

            if (ch.RightHand != null && ch.RightHand.IsNocked && !ch.RightHand.name.Contains("crossbow") && !ch.RightHand.returning)
            {
                ch.RightHand.IsNocked = false;
            }
            else if (ch.LeftHand != null && ch.LeftHand.IsNocked && !ch.LeftHand.name.Contains("crossbow") && !ch.LeftHand.returning)
            {
                ch.LeftHand.IsNocked = false;
            }

            if (args != null)
            {
                string[] sArgs = args.Split(" ".ToCharArray());

                if (sArgs[0] != null)
                {
                    dir[1] = sArgs[0];
                }

                if (sArgs.GetUpperBound(0) > 0)
                {
                    if (sArgs[1] != null)
                    {
                        dir[2] = sArgs[1];
                    }
                }
            }

            #region Moving through a web.
            if (ch.CurrentCell.DisplayGraphic == Cell.GRAPHIC_WEB || ch.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Web))
            {
                int webEffectAmount = 1;

                if (ch.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Web))
                {
                    webEffectAmount = ch.CurrentCell.AreaEffects[Effect.EffectTypes.Web].Power;
                }

                // Non arachnids, non web dwellers, non incorporeal must make a saving throw vs. spell with a modifier of the web effect amount.
                if (ch.species != Globals.eSpecies.Arachnid && !Autonomy.EntityBuilding.EntityLists.WEB_DWELLERS.Contains(ch.entity)
                    && !Autonomy.EntityBuilding.EntityLists.INCORPOREAL.Contains(ch.entity))
                {
                    if(ch.EffectsList.ContainsKey(Effect.EffectTypes.Flame_Shield))
                    {
                        if (!Combat.DND_CheckSavingThrow(ch, Combat.SavingThrow.Spell, webEffectAmount - Rules.GetGenericStatModifier(ch, Globals.eAbilityStat.Dexterity) - ch.EffectsList[Effect.EffectTypes.Flame_Shield].Power))
                        {
                            if (ch.IsPC)
                            {
                                ch.WriteToDisplay("You are stuck in a web!");
                            }
                            else
                            {
                                NPC npc = (NPC)ch;
                                npc.MoveList.Clear();
                            }
                            return;
                        }
                        else ch.WriteToDisplay("Your flameshield helps you move through the web.");
                    }
                    else if (!Combat.DND_CheckSavingThrow(ch, Combat.SavingThrow.Spell, webEffectAmount - Rules.GetGenericStatModifier(ch, Globals.eAbilityStat.Dexterity)))
                    {
                        if (ch.IsPC)
                        {
                            ch.WriteToDisplay("You are stuck in a web!");
                        }
                        else
                        {
                            NPC npc = (NPC)ch;
                            npc.MoveList.Clear();
                        }
                        return;
                    }
                }
            }
            #endregion

            #region Moving through fire.
            if ((ch.CurrentCell.DisplayGraphic == Cell.GRAPHIC_FIRE) && !ch.CanFly)
            {
                if (!ch.immuneFire && !ch.IsImmortal)
                {
                    if (ch.IsPC || Rules.CheckPerception(ch))
                    {
                        if (!Rules.FullStatCheck(ch, Globals.eAbilityStat.Dexterity))
                        {
                            if (ch.IsPC)
                            {
                                ch.WriteToDisplay("You are confused by the fire!");
                            }
                            else
                            {
                                NPC npc = (NPC)ch;
                                npc.MoveList.Clear();
                            }
                            return;
                        }
                    }
                }
            }
            #endregion

            #region Moving through a gas cloud.
            if ((ch.CurrentCell.DisplayGraphic == Cell.GRAPHIC_POISON_CLOUD) && !ch.CanFly)
            {
                if (!ch.immuneFire && !ch.IsImmortal)
                {
                    if (ch.IsPC || Rules.CheckPerception(ch))
                    {
                        if (!Rules.FullStatCheck(ch, Globals.eAbilityStat.Dexterity))
                        {
                            if (ch.IsPC)
                            {
                                ch.WriteToDisplay("You are confused by the gas!");
                            }
                            else
                            {
                                NPC npc = (NPC)ch;
                                npc.MoveList.Clear();
                            }
                            return;
                        }
                    }
                }
            }
            #endregion

            #region Moving through ice or snow/frost.
            if ((ch.CurrentCell.DisplayGraphic == Cell.GRAPHIC_ICE || ch.CurrentCell.DisplayGraphic == Cell.GRAPHIC_ICE_STORM ||
                ch.CurrentCell.DisplayGraphic == Cell.GRAPHIC_FOREST_FROSTY_LEFT || ch.CurrentCell.DisplayGraphic == Cell.GRAPHIC_FOREST_FROSTY_RIGHT ||
                ch.CurrentCell.DisplayGraphic == Cell.GRAPHIC_FOREST_FROSTY_FULL) && !ch.CanFly)
            {
                if (!ch.immuneCold && !ch.animal && !ch.IsImmortal)
                {
                    if (!Rules.FullStatCheck(ch, Globals.eAbilityStat.Dexterity))
                    {
                        if (ch.IsPC)
                        {
                            ch.WriteToDisplay("You have slipped on the ice!");
                        }
                        else
                        {
                            NPC npc = (NPC)ch;
                            npc.MoveList.Clear();
                        }
                        return;
                    }
                }
            }
            #endregion

            while (i < 3)
            {
                switch (dir[i].ToLower())
                {
                    case "n":
                    case "north":
                        Cell.Move(ch, dir, landnum, mapnum, xcord, ycord -= 1, zcord);
                        ch.dirPointer = "^";
                        count++;
                        CheckHiddenStatus(ch);
                        break;
                    case "ne":
                    case "northeast":
                        Cell.Move(ch, dir, landnum, mapnum, xcord += 1, ycord -= 1, zcord);
                        ch.dirPointer = ">";
                        count++;
                        CheckHiddenStatus(ch);
                        break;
                    case "e":
                    case "east":
                        Cell.Move(ch, dir, landnum, mapnum, xcord += 1, ycord, zcord);
                        ch.dirPointer = ">";
                        count++;
                        CheckHiddenStatus(ch);
                        break;
                    case "se":
                    case "southeast":
                        Cell.Move(ch, dir, landnum, mapnum, xcord += 1, ycord += 1, zcord);
                        ch.dirPointer = ">";
                        count++;
                        CheckHiddenStatus(ch);
                        break;
                    case "s":
                    case "south":
                        Cell.Move(ch, dir, landnum, mapnum, xcord, ycord += 1, zcord);
                        ch.dirPointer = "v";
                        count++;
                        CheckHiddenStatus(ch);
                        break;
                    case "sw":
                    case "southwest":
                        Cell.Move(ch, dir, landnum, mapnum, xcord -= 1, ycord += 1, zcord);
                        ch.dirPointer = "<";
                        count++;
                        CheckHiddenStatus(ch);
                        break;
                    case "w":
                    case "west":
                        Cell.Move(ch, dir, landnum, mapnum, xcord -= 1, ycord, zcord);
                        ch.dirPointer = "<";
                        count++;
                        CheckHiddenStatus(ch);
                        break;
                    case "nw":
                    case "northwest":
                        Cell.Move(ch, dir, landnum, mapnum, xcord -= 1, ycord -= 1, ch.Z);
                        ch.dirPointer = "<";
                        count++;
                        CheckHiddenStatus(ch);
                        break;
                    case "d":
                    case "down":
                    case "climb down":

                        Segue segue = ch.CurrentCell.Segue;

                        if (segue == null)
                        {
                            segue = Segue.GetDownSegue(ch.CurrentCell);
                        }

                        if (segue != null)
                        {
                            Cell.Move(ch, dir, segue.LandID, segue.MapID, segue.X, segue.Y, segue.Z);
                            Map.CheckHiddenStatus(ch);
                        }
                        break;
                    case "u":
                    case Cell.GRAPHIC_UPSTAIRS:
                    case "climb up":

                        segue = ch.CurrentCell.Segue;

                        if (segue == null)
                        {
                            segue = Segue.GetUpSegue(ch.CurrentCell);
                        }

                        if (segue != null)
                        {
                            Cell.Move(ch, dir, segue.LandID, segue.MapID, segue.X, segue.Y, segue.Z);
                            Map.CheckHiddenStatus(ch);
                        }
                        break;
                    default:
                        break;
                }
                i++;
            }

            // Move a Peeker.
            if (ch.EffectsList.ContainsKey(Effect.EffectTypes.Peek))
                ch.EffectsList[Effect.EffectTypes.Peek].Caster.CurrentCell = ch.CurrentCell;

            // Thieves gain a little thievery skill while moving around hidden, and while using their disguise magic.
            if (ch.BaseProfession == Character.ClassType.Thief)
            {
                if(ch.IsHidden)
                    Skills.GiveSkillExp(ch, Skills.GetSkillLevel(ch.thievery) * 3, Globals.eSkillType.Thievery);

                if(ch.EffectsList.ContainsKey(Effect.EffectTypes.Obfuscation))
                    Skills.GiveSkillExp(ch, Skills.GetSkillLevel(ch.thievery) * 25, Globals.eSkillType.Thievery);
            }

            try
            {
                if (ch.IsPC) // descriptions are sent to PCs only
                {
                    #region return description messages sent to players
                    if (ch.CurrentCell.Description != "") // 4/19/2006 discovered this was being sent every move.. was != null instead of != ""
                    {
                        if (ch.CurrentCell.Description.StartsWith("[")) { ch.WriteToDisplay(ch.CurrentCell.Description.Substring(8, ch.CurrentCell.Description.Length)); }
                        else { ch.WriteToDisplay(ch.CurrentCell.Description); }

                        string descriptionToSend = "";

                        if (ch.CurrentCell.IsLocker && !ch.CurrentCell.Description.Contains("Your name is inscribed in glowing blue letters on the drawer's handle."))
                        {
                            descriptionToSend = "As you approach the wall a large drawer slides out from what was an empty wall. Your name is inscribed in glowing blue letters on the drawer's handle.";
                        }

                        if (ch.CurrentCell.HasMailbox)
                        {
                            string leadingSpace = "";

                            if (descriptionToSend.Length > 0)
                                leadingSpace = " ";

                            descriptionToSend += leadingSpace + "Protruding from the wall here is a flat, marble tablet with a depression slightly larger than your hand. You recognize the new technology as a method of sending and receiving messages, as well as physical objects, from distant locations.";
                        }

                        if (descriptionToSend != "")
                            ch.WriteToDisplay(descriptionToSend);
                    }
                    else
                    {
                        string descriptionToSend = "";

                        if (ch.CurrentCell.IsLocker && !ch.CurrentCell.Description.Contains("Your name is inscribed in glowing blue letters on the drawer's handle."))
                        {
                            descriptionToSend = "As you approach the wall a large drawer slides out from what was an empty wall. Your name is inscribed in glowing blue letters on the drawer's handle.";
                        }

                        string leadingSpace = "";

                        if (ch.CurrentCell.HasMailbox)
                        {
                            if (descriptionToSend.Length > 0)
                                leadingSpace = " ";

                            descriptionToSend += leadingSpace + "Protruding from the wall is a flat, marble tablet with a depression slightly larger than your hand. You recognize the new technology as a method of sending and receiving messages, as well as physical objects, from distant locations.";
                        }

                        if (ch.CurrentCell.IsOneHandClimbUp || ch.CurrentCell.IsTwoHandClimbUp)
                        {
                            if (descriptionToSend.Length > 0)
                                leadingSpace = " ";
                            else leadingSpace = "";
                            string addedDesc = "It looks possible to climb up here.";
                            if (ch.CurrentCell.IsTwoHandClimbUp) addedDesc = "It looks possible to climb up here with both hands.";

                            descriptionToSend += leadingSpace + addedDesc;
                        }

                        if (ch.CurrentCell.IsOneHandClimbDown || ch.CurrentCell.IsTwoHandClimbDown)
                        {
                            if (descriptionToSend.Length > 0)
                                leadingSpace = " ";
                            else leadingSpace = "";

                            string addedDesc = "It looks possible to climb down here.";
                            if (ch.CurrentCell.IsTwoHandClimbDown) addedDesc = "It looks possible to climb down here with both hands.";
                            descriptionToSend += leadingSpace + addedDesc;
                        }

                        if (ch.CurrentCell.IsBalmFountain)
                        {
                            if (descriptionToSend.Length > 0)
                                leadingSpace = " ";
                            else leadingSpace = "";

                            descriptionToSend += leadingSpace + "There is fountain of cloudy fluid here with a young balm tree at its center.";
                        }

                        if (descriptionToSend != "")
                            ch.WriteToDisplay(descriptionToSend);
                    }

                    switch (ch.CurrentCell.DisplayGraphic)
                    {
                        case Cell.GRAPHIC_TRASHCAN:
                            ch.WriteToDisplay("You are standing next to a garbage can.");
                            break;
                        case Cell.GRAPHIC_DOWNSTAIRS:
                            ch.WriteToDisplay("You are standing at the top of stairs leading down.");
                            break;
                        case Cell.GRAPHIC_UPSTAIRS:
                            ch.WriteToDisplay("You are standing at the bottom of stairs leading up.");
                            break;
                        case Cell.GRAPHIC_FIRE:
                            ch.WriteToDisplay("You are standing in fire.");
                            break;
                        case Cell.GRAPHIC_LIGHTNING_STORM:
                            ch.WriteToDisplay("You are standing in a lightning storm.");
                            break;
                        case Cell.GRAPHIC_POISON_CLOUD:
                            ch.WriteToDisplay("You are standing in a poison cloud.");
                            break;
                        case Cell.GRAPHIC_ICE_STORM:
                            ch.WriteToDisplay("You are standing in an ice storm.");
                            break;
                        default:
                            break;
                    }

                    if ((ch.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) || ch.CurrentCell.IsAlwaysDark) && !ch.HasNightVision)
                    {
                        ch.WriteToDisplay("You are standing in darkness.");
                    }

                    #endregion
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }

            #region encumbrance

            Globals.eEncumbranceLevel encumbrance = Rules.GetEncumbrance(ch);

            if (count == 1)
            {
                if (encumbrance == Globals.eEncumbranceLevel.Severely)
                {
                    ch.Stamina -= 5;
                    ch.updateStam = true;
                    if (ch.Stamina < 1)
                    {
                        ch.Stamina = 0;
                        if (ch.Hits > 1)
                        {
                            Combat.DoDamage(ch, ch, 5, false);
                        }
                        else
                        {
                            ch.Hits = 1;
                        }
                    }
                }
                else if (encumbrance == Globals.eEncumbranceLevel.Heavily)
                {
                    ch.Stamina -= 1;
                    ch.updateStam = true;
                    if (ch.Stamina < 1)
                    {
                        ch.Stamina = 0;
                        if (ch.Hits > 1)
                        {
                            Combat.DoDamage(ch, ch, 1, false);
                        }
                        else
                        {
                            ch.Hits = 1;
                        }
                    }
                }
            }
            else if (count == 2)
            {
                if (encumbrance == Globals.eEncumbranceLevel.Severely)
                {
                    ch.Stamina -= 10;
                    ch.updateStam = true;
                    if (ch.Stamina < 1)
                    {
                        ch.Stamina = 0;
                        if (ch.Hits > 1)
                        {
                            Combat.DoDamage(ch, ch, 10, false);
                        }
                        else
                        {
                            ch.Hits = 1;
                        }
                    }
                }

                else if (encumbrance == Globals.eEncumbranceLevel.Heavily)
                {
                    ch.Stamina -= 2;
                    ch.updateStam = true;
                    if (ch.Stamina < 1)
                    {
                        ch.Stamina = 0;
                        if (ch.Hits > 2)
                        {
                            Combat.DoDamage(ch, ch, 2, false);
                        }
                        else
                        {
                            ch.Hits = 1;
                        }
                    }
                }
                else if (encumbrance == Globals.eEncumbranceLevel.Moderately)
                {
                    ch.Stamina -= 1;
                    ch.updateStam = true;
                    if (ch.Stamina < 1)
                    {
                        ch.Stamina = 0;
                        if (ch.Hits > 1)
                        {
                            Combat.DoDamage(ch, ch, 1, false);
                        }
                        else
                        {
                            ch.Hits = 1;
                        }
                    }
                }
            }
            else if (count == 3)
            {
                if (encumbrance == Globals.eEncumbranceLevel.Severely)
                {
                    ch.Stamina -= 15;
                    ch.updateStam = true;
                    if (ch.Stamina < 1)
                    {
                        ch.Stamina = 0;
                        if (ch.Hits > 1)
                        {
                            Combat.DoDamage(ch, ch, 15, false);
                        }
                        else
                        {
                            ch.Hits = 1;
                        }
                    }
                }
                else if (encumbrance == Globals.eEncumbranceLevel.Heavily)
                {
                    ch.Stamina -= 3;
                    ch.updateStam = true;
                    if (ch.Stamina < 1)
                    {
                        ch.Stamina = 0;
                        if (ch.Hits > 3)
                        {
                            Combat.DoDamage(ch, ch, 3, false);
                        }
                        else
                        {
                            ch.Hits = 1;
                        }
                    }
                }
                else if (encumbrance == Globals.eEncumbranceLevel.Moderately)
                {
                    ch.Stamina -= 2;
                    ch.updateStam = true;
                    if (ch.Stamina < 1)
                    {
                        ch.Stamina = 0;
                        if (ch.Hits > 2)
                        {
                            Combat.DoDamage(ch, ch, 2, false);
                        }
                        else
                        {
                            ch.Hits = 1;
                        }
                    }
                }
            }
            #endregion
        }

        public static void CheckHiddenStatus(Character ch)
        {
            if (ch.IsHidden) // returns true if Hide_In_Shadows effect exists
            {
                // character is not next to a wall, an hide spell is not permanent
                if (!Map.IsNextToWall(ch) && !ch.EffectsList[Effect.EffectTypes.Hide_in_Shadows].IsPermanent)
                {
                    ch.IsHidden = false;
                }
            }
        }

        public static Cell GetCellRelevantToCell(Cell cell, string args, bool includeSpellPathBlocked)
        {
            int argCount = 0;
            int i = 0, x = 0, y = 0;

            args = args.Trim();

            string[] sArgs = args.Split(" ".ToCharArray());

            argCount = sArgs.GetUpperBound(0);

            for (i = 0; i <= argCount; i++)
            {
                switch (sArgs[i].ToLower())
                {
                    case "north":
                    case "n":
                        if (!includeSpellPathBlocked && IsSpellPathBlocked(Cell.GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X + x, cell.Y + y - 1, cell.Z)))
                        {
                            i = argCount + 1;
                            break;
                        }
                        else
                            y--;
                        break;
                    case "northeast":
                    case "ne":
                        if (!includeSpellPathBlocked && IsSpellPathBlocked(Cell.GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X + x + 1, cell.Y + y - 1, cell.Z)))
                        {
                            i = argCount + 1;
                            break;
                        }
                        else
                        {
                            y--;
                            x++;
                        }
                        break;
                    case "northwest":
                    case "nw":
                        if (!includeSpellPathBlocked && IsSpellPathBlocked(Cell.GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X + x - 1, cell.Y + y - 1, cell.Z)))
                        {
                            i = argCount + 1;
                            break;
                        }
                        else
                        {
                            y--;
                            x--;
                        }
                        break;
                    case "west":
                    case "w":
                        if (!includeSpellPathBlocked && IsSpellPathBlocked(Cell.GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X + x - 1, cell.Y + y, cell.Z)))
                        {
                            i = argCount + 1;
                            break;
                        }
                        else
                            x--;
                        break;
                    case "east":
                    case "e":
                        if (!includeSpellPathBlocked && IsSpellPathBlocked(Cell.GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X + x + 1, cell.Y + y, cell.Z)))
                        {
                            i = argCount + 1;
                            break;
                        }
                        else
                            x++;
                        break;
                    case "southwest":
                    case "sw":
                        if (!includeSpellPathBlocked && IsSpellPathBlocked(Cell.GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X + x - 1, cell.Y + y + 1, cell.Z)))
                        {
                            i = argCount + 1;
                            break;
                        }
                        else
                        {
                            y++;
                            x--;
                        }
                        break;
                    case "southeast":
                    case "se":
                        if (!includeSpellPathBlocked && IsSpellPathBlocked(Cell.GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X + x + 1, cell.Y + y + 1, cell.Z)))
                        {
                            i = argCount + 1;
                            break;
                        }
                        else
                        {
                            y++;
                            x++;
                        }
                        break;
                    case "south":
                    case "s":
                        if (!includeSpellPathBlocked && IsSpellPathBlocked(Cell.GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X + x, cell.Y + y + 1, cell.Z)))
                        {
                            i = argCount + 1;
                            break;
                        }
                        else
                        {
                            y++;
                        }
                        break;
                    default:
                        break;
                }
            }

            // should never be able to do something off screen
            if (y > 3)
                y = 3;
            if (y < -3)
                y = -3;
            if (x > 3)
                x = 3;
            if (x < -3)
                x = -3;

            // return the cell that is pointed to by the args
            return Cell.GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X + x, cell.Y + y, cell.Z);
        }

        public static Item GetItemCopyFromCounter(Character ch, int count, string itemName)
        {
            int a = 0;

            Cell counterCell = Map.GetNearestCounterOrAltarCell(ch.CurrentCell);

            foreach (Item item in new List<Item>(counterCell.Items))
            {
                if (item.name.ToLower() == itemName.ToLower())
                {
                    if (++a == count)
                        return item;
                }
            }
            return null;
        }

        private static List<string> ShadowedCells = new List<string>()
        {
            Cell.GRAPHIC_WALL, Cell.GRAPHIC_ALTAR_PLACEABLE,Cell.GRAPHIC_ALTAR,Cell.GRAPHIC_COUNTER_PLACEABLE,Cell.GRAPHIC_COUNTER,Cell.GRAPHIC_MOUNTAIN,
            Cell.GRAPHIC_FOREST_RIGHT,Cell.GRAPHIC_FOREST_LEFT,Cell.GRAPHIC_FOREST_FULL,Cell.GRAPHIC_FOREST_IMPASSABLE,
            Cell.GRAPHIC_FOREST_FROSTY_FULL,Cell.GRAPHIC_FOREST_FROSTY_LEFT,Cell.GRAPHIC_FOREST_FROSTY_RIGHT,Cell.GRAPHIC_RUINS_LEFT,Cell.GRAPHIC_RUINS_RIGHT,
            Cell.GRAPHIC_WALL_IMPENETRABLE, Cell.GRAPHIC_DARKNESS
        };

        public static bool IsNextToWall(Character ch)
        {
            Cell cell = null;

            if (ch != null)
            {
                for (int ypos = -1; ypos <= 1; ypos += 1)
                {
                    for (int xpos = -1; xpos <= 1; xpos += 1)
                    {
                        cell = Cell.GetCell(ch.FacetID, ch.LandID, ch.MapID, ch.X + xpos, ch.Y + ypos, ch.Z);

                        if (cell != null)
                        {
                            if (cell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) || cell.IsAlwaysDark)
                            {
                                return true;
                            }
                            else if (ShadowedCells.Contains(cell.CellGraphic) || ShadowedCells.Contains(cell.DisplayGraphic))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static List<Item> GetCopyOfAllItemsFromCounter(Character ch)
        {
            Cell counterCell = GetNearestCounterOrAltarCell(ch.CurrentCell);

            if (counterCell != null)
            {
                List<Item> itemsList = new List<Item>();
                Item[] counterItems = new Item[counterCell.Items.Count];
                counterCell.Items.CopyTo(counterItems);
                foreach (Item item in counterItems)
                {
                    itemsList.Add(item);
                }
                return itemsList;
            }
            return null;
        }

        public static Item RemoveItemFromCounter(Character ch, long worldID)
        {
            Cell counterCell = GetNearestCounterOrAltarCell(ch.CurrentCell);

            if (counterCell != null)
            {
                foreach (Item item in new List<Item>(counterCell.Items))
                {
                    if (item.IsArtifact() && ch.PossessesItem(item.itemID))
                    {
                        ch.WriteToDisplay("You already possess this artifact.");
                        return null;
                    }

                    if (item.UniqueID == worldID)
                    {
                        counterCell.Remove(item);
                        return item;
                    }
                }
            }
            return null;
        }

        public static Item RemoveItemFromCounter(Character ch, string itemName)
        {
            try
            {
                Cell counterCell = GetNearestCounterOrAltarCell(ch.CurrentCell);

                if (counterCell != null)
                {
                    foreach (Item item in new List<Item>(counterCell.Items))
                    {
                        if (item.name.ToLower().StartsWith(itemName.ToLower()) || (int.TryParse(itemName, out int uniqueID) && uniqueID == item.UniqueID))
                        {
                            if (item.IsArtifact() && ch.PossessesItem(item.itemID))
                            {
                                ch.WriteToDisplay("You already possess this artifact.");
                                return null;
                            }

                            counterCell.Remove(item);
                            return item;
                        }
                    }
                }
            }
            catch(Exception ex)
            { Utils.LogException(ex); }
            return null;
        }

        /// <summary>
        /// Determine if a Character is next to a placeable counter or altar.
        /// </summary>
        /// <param name="ch">The Character to test.</param>
        /// <returns>True if next to a placeable counter or altar.</returns>
        public static bool IsNextToCounter(Character ch)
        {
            for (int ypos = -1; ypos <= 1; ypos += 1)
            {
                for (int xpos = -1; xpos <= 1; xpos += 1)
                {
                    Cell curCell = Cell.GetCell(ch.FacetID, ch.LandID, ch.MapID, ch.X + xpos, ch.Y + ypos, ch.Z);

                    if (curCell != null && !string.IsNullOrEmpty(curCell.CellGraphic) && (curCell.CellGraphic == Cell.GRAPHIC_COUNTER_PLACEABLE || curCell.CellGraphic == Cell.GRAPHIC_ALTAR_PLACEABLE))
                        return true;
                }
            }
            return false;
        }

        public static bool IsNextToScribingCrystal(Character ch)
        {
            for (int ypos = -1; ypos <= 1; ypos += 1)
            {
                for (int xpos = -1; xpos <= 1; xpos += 1)
                {
                    Cell curCell = Cell.GetCell(ch.FacetID, ch.LandID, ch.MapID, ch.X + xpos, ch.Y + ypos, ch.Z); ;
                    if (curCell != null && curCell.HasScribingCrystal)
                        return true;
                }
            }
            return false;
        }

        public static bool IsNextToBalmFountain(Character ch)
        {
            for (int ypos = -1; ypos <= 1; ypos += 1)
            {
                for (int xpos = -1; xpos <= 1; xpos += 1)
                {
                    Cell curCell = Cell.GetCell(ch.FacetID, ch.LandID, ch.MapID, ch.X + xpos, ch.Y + ypos, ch.Z); ;
                    if (curCell != null && curCell.IsBalmFountain)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void PutItemOnCounter(Character ch, Item item)
        {
            //loop through all visable cells
            for (int ypos = -1; ypos <= 1; ypos += 1)
            {
                for (int xpos = -1; xpos <= 1; xpos += 1)
                {
                    Cell curCell = Cell.GetCell(ch.FacetID, ch.LandID, ch.MapID, ch.X + xpos, ch.Y + ypos, ch.Z);
                    if (curCell != null && curCell.CellGraphic == Cell.GRAPHIC_COUNTER_PLACEABLE || curCell.CellGraphic == Cell.GRAPHIC_ALTAR_PLACEABLE)
                    {
                        // coins should accumulate value; avoid redundant coins items
                        if (item.itemType == Globals.eItemType.Coin)
                        {
                            foreach (Item itm in curCell.Items)
                            {
                                if (itm.itemType == Globals.eItemType.Coin)
                                {
                                    itm.coinValue += item.coinValue;
                                    return;
                                }
                            }
                        }

                        curCell.Add(item);
                        return;
                    }

                }
            }
        }

        public static bool CheckSecretDoor(Cell cell)
        {
            if (cell.IsSecretDoor)
            {
                if (cell.DisplayGraphic == Cell.GRAPHIC_WALL)
                {
                    AreaEffect effect = new AreaEffect(Effect.EffectTypes.Find_Secret_Door, Cell.GRAPHIC_EMPTY, 0, 8, null, cell);
                    cell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.OpenDoor));
                    return true;
                }
                else if (cell.DisplayGraphic == Cell.GRAPHIC_MOUNTAIN)
                {
                    AreaEffect effect = new AreaEffect(Effect.EffectTypes.Find_Secret_Rockwall, Cell.GRAPHIC_EMPTY, 0, 8, null, cell);
                    cell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SlidingRockDoor));
                    return true;
                }
            }
            return false;
        }

        public static bool UnlockDoor(Cell cell, string key)
        {
            if (key == cell.cellLock.key)
            {
                if (cell.IsLockedHorizontalDoor)
                {
                    AreaEffect effect = new AreaEffect(Effect.EffectTypes.Unlocked_Horizontal_Door, Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL, 0, 5, null, cell);
                    return true;
                }
                else if (cell.IsLockedVerticalDoor)
                {
                    AreaEffect effect = new AreaEffect(Effect.EffectTypes.Unlocked_Vertical_Door, Cell.GRAPHIC_OPEN_DOOR_VERTICAL, 0, 5, null, cell);
                    return true;
                }
            }
            return false;
        }

        public static bool UnlockDoor(Character chr, Cell cell, Item key)
        {
            if (key.key == cell.cellLock.key || key.itemID == Item.ID_LOCKPICK)
            {
                if (key.itemID == Item.ID_LOCKPICK && !chr.HasTalent(Talents.GameTalent.TALENTS.PickLocks))
                {
                    chr.WriteToDisplay("You do not know how to pick locks.");
                    return false;
                }

                if (cell.IsLockedHorizontalDoor)
                {
                    AreaEffect effect = new AreaEffect(Effect.EffectTypes.Unlocked_Horizontal_Door, Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL, 0, 5, null, cell);
                    return true;
                }
                else if (cell.IsLockedVerticalDoor)
                {
                    AreaEffect effect = new AreaEffect(Effect.EffectTypes.Unlocked_Vertical_Door, Cell.GRAPHIC_OPEN_DOOR_VERTICAL, 0, 5, null, cell);
                    return true;
                }
            }
            return false;
        }

        public static bool LockDoor(Cell cell, string key)
        {
            if (key == cell.cellLock.key)
            {
                if (cell.AreaEffects.ContainsKey(Effect.EffectTypes.Unlocked_Horizontal_Door))
                {
                    cell.AreaEffects[Effect.EffectTypes.Unlocked_Horizontal_Door].StopAreaEffect();
                    return true;
                }
                else if (cell.AreaEffects.ContainsKey(Effect.EffectTypes.Unlocked_Vertical_Door))
                {
                    cell.AreaEffects[Effect.EffectTypes.Unlocked_Vertical_Door].StopAreaEffect();
                    return true;
                }
            }
            return false;
        }

        public static Cell GetNearestCounterOrAltarCell(Cell cell)
        {
            for (int ypos = -1; ypos <= 1; ypos += 1)
            {
                for (int xpos = -1; xpos <= 1; xpos += 1)
                {
                    Cell counterCell = Cell.GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X + xpos, cell.Y + ypos, cell.Z);
                    if (counterCell != null && counterCell.CellGraphic == Cell.GRAPHIC_COUNTER_PLACEABLE || counterCell.CellGraphic == Cell.GRAPHIC_ALTAR_PLACEABLE)
                    {
                        return counterCell;
                    }
                }
            }
            return null;
        }

        public static List<Cell> GetAdjacentCells(Cell cell, Character ch, int radius, Effect.EffectTypes effectType)
        {
            List<Cell> cellList = new List<Cell>();
            for (int ypos = -radius; ypos <= radius; ypos++)
            {
                for (int xpos = -radius; xpos <= radius; xpos++)
                {
                    Cell newcell = Cell.GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X + xpos, cell.Y + ypos, cell.Z);
                    if (newcell != null)
                    {
                        if (newcell.AreaEffects.ContainsKey(effectType))
                        {
                            cellList.Add(newcell);
                        }
                    }
                }
            }
            if (cellList.Count > 0)
            {
                return cellList;
            }
            else
            {
                return null;
            }
        }

        public static List<Cell> GetAdjacentCells(Cell cell, Character ch)
        {
            try
            {
                List<Cell> cellList = new List<Cell>();
                for (int ypos = -1; ypos <= 1; ypos++)
                {
                    for (int xpos = -1; xpos <= 1; xpos++)
                    {
                        Cell newcell = Cell.GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X + xpos, cell.Y + ypos, cell.Z);
                        if (newcell != null)
                        {
                            if (ch.GetCellCost(newcell) <= 2)
                            {
                                cellList.Add(newcell);
                            }
                        }
                    }
                }
                if (cellList.Count > 0)
                {
                    return cellList;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Utils.Log("Error in Map.GetAdjacentCells - Name: " + ch.GetLogString(), Utils.LogType.SystemFailure);
                Utils.LogException(e);
                return null;
            }
        }

        public static List<Cell> GetAdjacentCells(Cell cell)
        {
            List<Cell> cellList = new List<Cell>();
            for (int ypos = -1; ypos <= 1; ypos++)
            {
                for (int xpos = -1; xpos <= 1; xpos++)
                {
                    Cell newcell = Cell.GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X + xpos, cell.Y + ypos, cell.Z);
                    if (newcell != null)
                    {
                        if (!IsSpellPathBlocked(newcell))
                        {
                            cellList.Add(newcell);
                        }
                    }
                }
            }
            if (cellList.Count > 0)
            {
                return cellList;
            }
            else
            {
                return null;
            }
        } 
        #endregion

        public void Add(Tuple<int, int, int> key, Cell cell)
        {
            if (!this.cells.TryAdd(Tuple.Create(key.Item1, key.Item2, key.Item3), cell))
            {
                Utils.Log("Failed to add cell to Map.Cells ConcurrentDictionary! Cell: " + cell.ToString(), Utils.LogType.SystemFailure);
            }
        }

        private void BuildZPlane(string s, ZPlane zInfo, ref int xOffset, ref int y)
        {
            // entire z coord is outdoor
            if (s.IndexOf("outdoor=true") != -1)
                zInfo.isOutdoors = true;

            // entire z coord is no recall
            if (s.IndexOf("norecall=true") != -1)
                zInfo.isNoRecall = true;

            // entire z coord is within town limits
            if (s.IndexOf("townlimits=true") != -1)
                zInfo.withinTownLimits = true;

            // entire z coord is darkness tiles
            if (s.IndexOf("dark=true") != -1)
                zInfo.alwaysDark = true;

            if (s.IndexOf("sandybeaches=true") != -1)
                zInfo.hasSandyBeaches = true;

            if (s.IndexOf("coexistent=true") != -1)
                zInfo.isCoexistent = true;

            if (s.IndexOf("icy=true") != -1)
                zInfo.icy = true;

            if (s.IndexOf("swampy=true") != -1)
                zInfo.swampy = true;

            if (s.IndexOf("alignment=") != -1)
            {
                if (s.Contains("alignment=lawful"))
                    zInfo.spawnAlignment = Globals.eAlignment.Lawful;
                else if (s.Contains("alignment=evil"))
                    zInfo.spawnAlignment = Globals.eAlignment.Evil;
                else if (s.ToLower().Contains("alignment=chaoticevil"))
                    zInfo.spawnAlignment = Globals.eAlignment.ChaoticEvil;
                else if (s.Contains("alignment=chaotic"))
                    zInfo.spawnAlignment = Globals.eAlignment.Chaotic;
                else if (s.Contains("alignment=neutral"))
                    zInfo.spawnAlignment = Globals.eAlignment.Neutral;
                else if (s.Contains("alignment=amoral"))
                    zInfo.spawnAlignment = Globals.eAlignment.Amoral;
            }

            //// minimum level of creatures in the z plane
            //if (s.ToLower().IndexOf("<minlevel>") != -1)
            //    zInfo.suggestedMinimumLevel = Convert.ToInt32(s.Substring(s.ToLower().IndexOf("<minlevel>") + 10, s.ToLower().IndexOf("</minlevel>") - (s.IndexOf("<minlevel>") + 10)));

            //// maximum level of creatures in the z plane
            //if (s.ToLower().IndexOf("<maxlevel>") != -1)
            //    zInfo.suggestedMaximumLevel = Convert.ToInt32(s.Substring(s.ToLower().IndexOf("<maxlevel>") + 10, s.ToLower().IndexOf("</maxlevel>") - (s.IndexOf("<maxlevel>") + 10)));

            // forestry level
            if (s.IndexOf("<f>") != -1 && s.IndexOf("</f>") != -1)
                zInfo.forestry = (ForestedLevel)Enum.Parse(typeof(ForestedLevel), s.Substring(s.IndexOf("<f>") + 3, s.IndexOf("</f>") - (s.IndexOf("<f>") + 3)), true);

            if(zInfo.forestry == ForestedLevel.Random)
            {
                ForestedLevel[] array = (ForestedLevel[])Enum.GetValues(typeof(ForestedLevel));
                zInfo.forestry = array[Rules.Dice.Next(array.Length - 1)];
            }

            // light level
            if (s.IndexOf("<l>") != -1 && s.IndexOf("</l>") != -1)
                zInfo.lightLevel = (LightLevel)Enum.Parse(typeof(LightLevel), s.Substring(s.IndexOf("<l>") + 3, s.IndexOf("</l>") - (s.IndexOf("<l>") + 3)), true);

            // name of the z plane
            if (s.IndexOf("<n>") != -1 && s.IndexOf("</n>") != -1)
                zInfo.name = s.Substring(s.IndexOf("<n>") + 3, s.IndexOf("</n>") - (s.IndexOf("<n>") + 3));

            // x offset
            if (s.IndexOf("<x>") != -1 && s.IndexOf("</x>") != -1)
                xOffset = Convert.ToInt16(s.Substring(s.IndexOf("<x>") + 3, s.IndexOf("</x>") - (s.IndexOf("<x>") + 3)));
            else xOffset = 0;

            // y offset
            if (s.IndexOf("<y>") != -1 && s.IndexOf("</y>") != -1)
                y = Convert.ToInt16(s.Substring(s.IndexOf("<y>") + 3, s.IndexOf("</y>") - (s.IndexOf("<y>") + 3)));
            else y = 0;

            zInfo.xcordMax = 0;
            zInfo.xcordMin = xOffset;
            zInfo.ycordMax = 0;
            zInfo.ycordMin = y;
        }

        public bool LoadMap(string fileName, int facetID, int landID, int mapID)
        {
            int xOffset = 0;
            int x = 0;
            int y = 0;
            int z = 0;
            int a = 0;
            int effectLevel = 0;
            ZPlane zInfo;
            Cell cell = null; // the cell that will be added to this map's dictionary

            try
            {
                if (!File.Exists(fileName)) return false;

                StreamReader sr = File.OpenText(fileName);

                string[] mapLines = sr.ReadToEnd().Split("\n".ToCharArray());

                bool viewingAutonomy = false;
                Dictionary<int, string> autonomyLines = new Dictionary<int, string>();
                List<string> mapLinesList = new List<string>();

                // iterate through all lines in the map .txt file, separate actual z plane info and autonomy information
                for (a = 0; a < mapLines.Length; a++)
                {
                    mapLines[a] = mapLines[a].Replace("\r", "");

                    if (mapLines[a].Contains("<z>"))
                    {
                        z = Convert.ToInt32(mapLines[a].Substring(mapLines[a].IndexOf("<z>") + 3, mapLines[a].IndexOf("</z>") - (mapLines[a].IndexOf("<z>") + 3)));
                    }

                    if (viewingAutonomy)
                    {
                        autonomyLines[z] = autonomyLines[z] + mapLines[a].Trim();
                    }

                    if (mapLines[a].StartsWith("<autonomy>")) // start to collect autonomy information
                    {
                        try
                        {
                            autonomyLines.Add(z, mapLines[a].Trim());
                            viewingAutonomy = true;
                            continue;
                        }
                        catch (Exception e)
                        {
                            Utils.LogException(e);
                            Utils.Log("Error in LoadMap when adding new <autonomy>. Z: " + z + " Line: " + mapLines[a], Utils.LogType.ExceptionDetail);
                        }
                    }

                    if (mapLines[a].StartsWith("</autonomy>"))
                    {
                        viewingAutonomy = false;
                        continue;
                    }

                    // does not start with a comment, is not a header line, and is odd in length
                    if (!mapLines[a].StartsWith("//") && !mapLines[a].StartsWith("<") && !viewingAutonomy && mapLines[a].Length % 2 == 1)
                    {
                        mapLines[a] = mapLines[a] + " ";
                        Utils.Log("Map line " + (a + 1) + " was odd in length for " + this.Name + ".", Utils.LogType.SystemWarning);
                    }

                    if (!viewingAutonomy) mapLinesList.Add(mapLines[a]);
                }

                foreach (string s in mapLinesList)
                {
                    if (s.Length > 0 && !s.StartsWith("//")) // is comment in map file
                    {
                        a = 0;
                        x = xOffset;

                        if (s.StartsWith("<")) // look for z plane attributes
                        {
                            // z coord (height)
                            if (s.IndexOf("<z>") != -1 && s.IndexOf("</z>") != -1)
                            {
                                z = Convert.ToInt32(s.Substring(s.IndexOf("<z>") + 3, s.IndexOf("</z>") - (s.IndexOf("<z>") + 3)));
                                zInfo = new ZPlane(z, this, World.GetFacetByIndex(0).GetLandByID(landID).ShortDesc);
                            }
                            else
                            {
                                z = 0;
                                zInfo = new ZPlane(z, this, World.GetFacetByIndex(0).GetLandByID(landID).ShortDesc);
                            }

                            BuildZPlane(s, zInfo, ref xOffset, ref y);

                            // add z plane to the dictionary
                            if (!this.zPlaneInfos.ContainsKey(z))
                                this.zPlaneInfos.Add(z, zInfo);

                            if (autonomyLines.ContainsKey(z))
                            {
                                zInfo.zAutonomy = new ZAutonomy(zInfo.name, autonomyLines[z]);
                            }
                        }
                        else
                        {
                            #region Read the length of the line and create the cells.
                            while (a < s.Length)
                            {
                                if (a < s.Length)
                                {
                                    if (s.Substring(a, 2) != "  ")
                                    {
                                        cell = new Cell(facetID, landID, mapID, x, y, z, s.Substring(a, 2));

                                        Tuple<int, int, int> key = new Tuple<int, int, int>(x, y, z);

                                        if (!this.cells.ContainsKey(key))
                                            this.Add(key, cell);
                                        else
                                        {
                                            Utils.Log("Failed to add Cell(first) to " + this.Name + " " + x + "," + y + "," + z, Utils.LogType.SystemFailure);
                                            throw new Exception();
                                        }

                                        cell.IsPVPEnabled = ZPlanes[z].pvpEnabled;
                                        cell.IsNoRecall = ZPlanes[z].isNoRecall;
                                        cell.IsOutdoors = ZPlanes[z].isOutdoors;
                                        cell.IsWithinTownLimits = ZPlanes[z].withinTownLimits;
                                        cell.IsAlwaysDark = ZPlanes[z].alwaysDark;

                                        if (cell.CellGraphic == Cell.GRAPHIC_UPSTAIRS)
                                            cell.IsStairsUp = true;
                                        if (cell.CellGraphic == Cell.GRAPHIC_DOWNSTAIRS)
                                            cell.IsStairsDown = true;
                                        if (cell.CellGraphic == Cell.GRAPHIC_UP_AND_DOWNSTAIRS)
                                        {
                                            cell.IsStairsDown = true;
                                            cell.IsStairsUp = true;
                                        }

                                        if (cell.CellGraphic == Cell.GRAPHIC_EMPTY && ZPlanes[z].icy && Rules.RollD(1, 100) >= 50)
                                        {
                                            cell.CellGraphic = Cell.GRAPHIC_ICE;
                                        }

                                        // swampy
                                        if (cell.CellGraphic == Cell.GRAPHIC_EMPTY && ZPlanes[z].swampy && Rules.RollD(1, 100) >= 50)
                                        {
                                            cell.CellGraphic = Cell.GRAPHIC_WATER;
                                        }

                                        if (cell.CellGraphic == Cell.GRAPHIC_EMPTY && ZPlanes[z].hasSandyBeaches)
                                        {
                                            var adjacentCells = Map.GetAdjacentCells(cell);
                                            foreach (Cell adjCell in adjacentCells)
                                            {
                                                if (adjCell != null && adjCell.CellGraphic == Cell.GRAPHIC_WATER && Rules.RollD(1, 100) >= 50)
                                                    cell.CellGraphic = Cell.GRAPHIC_SAND;
                                            }
                                        }

                                        #region If fire Effect
                                        if (cell.CellGraphic[0].ToString() == "*")
                                        {
                                            if (cell.CellGraphic == Cell.GRAPHIC_FIRE || cell.CellGraphic == "*t")
                                            {
                                                effectLevel = 1;

                                                if (cell.CellGraphic == "*t")
                                                {
                                                    cell.IsWithinTownLimits = true;
                                                }
                                            }
                                            else if (cell.CellGraphic == Map.RESERVED_GRAPHIC_ORNIC_FLAME)
                                            {
                                                effectLevel = 0; // flames will not burn, Ornic Flame damage is handled in DoAreaEffect (non evil receive damage)
                                                AreaEffect ornicFlame = new AreaEffect(Effect.EffectTypes.Ornic_Flame, Cell.GRAPHIC_FIRE, Rules.RollD(3, 20), -1, null, cell);
                                            }
                                            else effectLevel = Convert.ToInt32(cell.CellGraphic.Substring(1, 1));

                                            cell.CellGraphic = Cell.GRAPHIC_FIRE;

                                            if (effectLevel > 0)
                                            {
                                                AreaEffect effect = new AreaEffect(Effect.EffectTypes.Fire, Cell.GRAPHIC_FIRE, effectLevel * 10, -1, null, cell);
                                            }
                                        }
                                        #endregion

                                        #region if Concussion Trap"ee"
                                        else if (cell.CellGraphic[0].ToString() == "e")
                                        {
                                            if (cell.CellGraphic == "ee" || cell.CellGraphic == "e]")
                                            {
                                                effectLevel = 60;
                                            }
                                            else
                                            {
                                                effectLevel = Convert.ToInt32((cell.CellGraphic[1] * 2) * 7);
                                            }

                                            if (cell.CellGraphic == "e]")
                                            {
                                                effectLevel = 60;
                                                cell.IsNoRecall = true;
                                                cell.CellGraphic = Cell.GRAPHIC_RUINS_RIGHT;
                                            }
                                            else
                                            {
                                                cell.CellGraphic = Cell.GRAPHIC_EMPTY;
                                            }
                                            cell.TrapPower = effectLevel;
                                        }
                                        #endregion

                                        else if (cell.CellGraphic[0].ToString() == ":")
                                        {
                                            switch (cell.CellGraphic)
                                            {
                                                case ":r":
                                                    cell.IsNoRecall = true;
                                                    break;
                                                case ":t":
                                                    cell.IsWithinTownLimits = true;
                                                    break;
                                                case ":0":
                                                    cell.IsOneHandClimbDown = true;
                                                    break;
                                                case ":1":
                                                    cell.IsOneHandClimbUp = true;
                                                    break;
                                                case ":2":
                                                    cell.IsTwoHandClimbDown = true;
                                                    break;
                                                case ":3":
                                                    cell.IsTwoHandClimbUp = true;
                                                    break;
                                                case ":K":
                                                    cell.IsLocker = true;
                                                    cell.HasMailbox = true;
                                                    break;
                                                case ":C":
                                                    if (cell.Description.Length > 0 && !cell.Description.Contains("scribing"))
                                                        cell.Description += " There is a scribing crystal here.";
                                                    else cell.Description = "There is a scribing crystal here.";
                                                    cell.HasScribingCrystal = true;
                                                    break;
                                                case ":b":
                                                case ":B":
                                                    if (cell.Description.Length > 0 && !cell.Description.ToLower().Contains("ship") && !cell.Description.ToLower().Contains("boat") &&
                                                        !cell.Description.ToLower().Contains("schooner"))
                                                        cell.Description += " You are standing on the deck of a schooner.";
                                                    else cell.Description = "You are standing on the deck of a schooner.";
                                                    cell.IsWithinTownLimits = true;
                                                    break;

                                            }
                                            cell.CellGraphic = Cell.GRAPHIC_BRIDGE;
                                        }

                                        else if (cell.CellGraphic[0].ToString() == "|")
                                        {
                                            switch (cell.CellGraphic)
                                            {
                                                case "|r":
                                                    cell.IsNoRecall = true;
                                                    break;
                                                case "|t":
                                                    cell.IsWithinTownLimits = true;
                                                    break;
                                            }
                                            cell.CellGraphic = Cell.GRAPHIC_CLOSED_DOOR_VERTICAL;
                                        }

                                        else if (cell.CellGraphic[0].ToString() == "-")
                                        {
                                            switch (cell.CellGraphic)
                                            {
                                                case "-r":
                                                    cell.IsNoRecall = true;
                                                    break;
                                                case "-t":
                                                    cell.IsWithinTownLimits = true;
                                                    break;
                                            }
                                            cell.CellGraphic = Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL;
                                        }
                                        else if (cell.CellGraphic[0].ToString() == "o")
                                        {
                                            switch (cell.CellGraphic)
                                            {
                                                case "or":
                                                    cell.IsNoRecall = true;
                                                    break;
                                                case "ot":
                                                    cell.IsWithinTownLimits = true;
                                                    break;
                                            }
                                            cell.CellGraphic = Cell.GRAPHIC_TRASHCAN;
                                        }

                                        else if (cell.CellGraphic[0].ToString() == "i")
                                        {
                                            switch (cell.CellGraphic)
                                            {
                                                case "i0":
                                                    cell.IsOneHandClimbDown = true;
                                                    break;
                                                case "i1":
                                                    cell.IsOneHandClimbUp = true;
                                                    break;
                                                case "i2":
                                                    cell.IsTwoHandClimbDown = true;
                                                    break;
                                                case "i3":
                                                    cell.IsTwoHandClimbUp = true;
                                                    break;
                                            }
                                            cell.CellGraphic = Cell.GRAPHIC_ICE;
                                        }

                                        #region else if ancestoring related "a0" "a1"
                                        else if (cell.CellGraphic[0].ToString() == "a")
                                        {
                                            switch (cell.CellGraphic)
                                            {
                                                case RESERVED_GRAPHIC_ANCESTOR_START:
                                                    cell.IsAncestorStart = true;
                                                    break;
                                                case RESERVED_GRAPHIC_ANCESTOR_FINISH:
                                                    cell.IsAncestorFinish = true;
                                                    break;
                                            }
                                            cell.CellGraphic = Cell.GRAPHIC_EMPTY;
                                        }
                                        #endregion

                                        #region else if cellGraphic[0] == "c" set as climb
                                        else if (cell.CellGraphic[0].ToString() == "c")
                                        {
                                            switch (cell.CellGraphic)
                                            {
                                                case RESERVED_GRAPHIC_ONE_HAND_CLIMB_DOWN:
                                                    cell.IsOneHandClimbDown = true;
                                                    break;
                                                case RESERVED_GRAPHIC_ONE_HAND_CLIMB_UP:
                                                    cell.IsOneHandClimbUp = true;
                                                    break;
                                                case RESERVED_GRAPHIC_TWO_HAND_CLIMB_DOWN:
                                                    cell.IsTwoHandClimbDown = true;
                                                    break;
                                                case RESERVED_GRAPHIC_TWO_HAND_CLIMB_UP:
                                                    cell.IsTwoHandClimbUp = true;
                                                    break;
                                            }
                                            cell.CellGraphic = Cell.GRAPHIC_EMPTY;
                                        }
                                        #endregion

                                        #region else if cellGraphic[0] == "~" "{" "." - checks for CellGraphic[1] == "t" for townlimits
                                        else if (cell.CellGraphic[0].ToString() == "~" && cell.CellGraphic[1].ToString() != "~")
                                        {
                                            switch (cell.CellGraphic)
                                            {
                                                case RESERVED_GRAPHIC_ONE_HAND_CLIMB_DOWN_IN_WATER:
                                                    cell.IsOneHandClimbDown = true;
                                                    cell.CellGraphic = Cell.GRAPHIC_WATER;
                                                    break;
                                                case RESERVED_GRAPHIC_ONE_HAND_CLIMB_UP_IN_WATER:
                                                    cell.IsOneHandClimbUp = true;
                                                    cell.CellGraphic = Cell.GRAPHIC_WATER;
                                                    break;
                                                case RESERVED_GRAPHIC_TWO_HAND_CLIMB_DOWN_IN_WATER:
                                                    cell.IsTwoHandClimbDown = true;
                                                    cell.CellGraphic = Cell.GRAPHIC_WATER;
                                                    break;
                                                case RESERVED_GRAPHIC_TWO_HAND_CLIMB_UP_IN_WATER:
                                                    cell.IsTwoHandClimbUp = true;
                                                    cell.CellGraphic = Cell.GRAPHIC_WATER;
                                                    break;
                                                case RESERVED_GRAPHIC_NO_RECALL_IN_WATER:
                                                    cell.IsNoRecall = true;
                                                    cell.CellGraphic = Cell.GRAPHIC_WATER;
                                                    break;
                                                case RESERVED_GRAPHIC_TOWN_LIMITS_IN_WATER:
                                                    cell.IsWithinTownLimits = true;
                                                    cell.CellGraphic = Cell.GRAPHIC_WATER;
                                                    break;
                                                case RESERVED_GRAPHIC_BALM_FOUNTAIN:
                                                    cell.IsBalmFountain = true;
                                                    cell.CellGraphic = Cell.GRAPHIC_WATER;
                                                    break;
                                            }
                                        }
                                        else if (cell.CellGraphic[0].ToString() == "{" && cell.CellGraphic[1].ToString() != "}")
                                        {

                                            switch (cell.CellGraphic)
                                            {
                                                case "{0":
                                                    cell.IsOneHandClimbDown = true;
                                                    cell.CellGraphic = "{}";
                                                    break;
                                                case "{1":
                                                    cell.IsOneHandClimbUp = true;
                                                    cell.CellGraphic = "{}";
                                                    break;
                                                case "{2":
                                                    cell.IsTwoHandClimbDown = true;
                                                    cell.CellGraphic = "{}";
                                                    break;
                                                case "{3":
                                                    cell.IsTwoHandClimbUp = true;
                                                    cell.CellGraphic = "{}";
                                                    break;
                                                case "{r":
                                                    cell.IsNoRecall = true;
                                                    cell.CellGraphic = "{}";
                                                    break;
                                                case "{t":
                                                    cell.IsWithinTownLimits = true;
                                                    cell.CellGraphic = "{}";
                                                    break;
                                            }
                                            int randomTree = Rules.RollD(1, 300);
                                            int randomFruit = Rules.RollD(1, 400);

                                            if (randomTree >= 0 && randomTree < 41)
                                            {
                                                cell.CellGraphic = Cell.GRAPHIC_FOREST_RIGHT;
                                            }
                                            else if (randomTree > 40 && randomTree < 81)
                                            {
                                                cell.CellGraphic = Cell.GRAPHIC_FOREST_LEFT;
                                            }
                                            else
                                            {
                                                cell.CellGraphic = Cell.GRAPHIC_FOREST_FULL;
                                            }

                                            // 1 to 4
                                            if (randomFruit >= 1 && randomFruit <= 4 && HasBalmBushes)
                                            {
                                                cell.balmBerry = true;
                                                cell.dailyFruit = Rules.RollD(3, 20);
                                                if (cell.Description.Length > 0)
                                                {
                                                    cell.Description += " There is a bush covered with bright red berries here.";
                                                }
                                                else
                                                {
                                                    cell.Description = "There is a bush covered with bright red berries here.";
                                                }
                                            }
                                            // 10 or 11
                                            else if (randomFruit == 10 || randomFruit == 11 && this.HasPoisonBushes)
                                            {
                                                cell.poisonBerry = true;
                                                cell.dailyFruit = Rules.RollD(2, 20);
                                                if (cell.Description.Length > 0)
                                                {
                                                    cell.Description += " There is a bush with pale yellow berries here.";
                                                }
                                                else
                                                {
                                                    cell.Description = "There is a bush with pale yellow berries here.";
                                                }
                                            }
                                            // 42
                                            else if (randomFruit == 42 && this.HasManaBushes)
                                            {
                                                cell.manaBerry = true;
                                                cell.dailyFruit = Rules.RollD(1, 6);
                                                if (cell.Description.Length > 0)
                                                {
                                                    cell.Description += " There is a bush with light blue berries here.";
                                                }
                                                else
                                                {
                                                    cell.Description = "There is a bush with light blue berries here.";
                                                }
                                            }
                                            else if (randomFruit >= 60 && randomFruit <= 63)
                                            {
                                                cell.growsSprigs = true;
                                                cell.dailyFruit = Rules.RollD(2, 12);
                                                if (cell.Description.Length > 0)
                                                {
                                                    cell.Description += " There is a bush with small green sprigs here.";
                                                }
                                                else
                                                {
                                                    cell.Description = "There is a bush with small green sprigs here.";
                                                }
                                            }
                                            // 98
                                            else if (randomFruit == 98 && this.HasStaminaBushes)
                                            {
                                                cell.stamBerry = true;
                                                cell.dailyFruit = Rules.RollD(1, 10);
                                                if (cell.Description.Length > 0)
                                                {
                                                    cell.Description += " There is a bush with dark green berries here.";
                                                }
                                                else
                                                {
                                                    cell.Description = "There is a bush with dark green berries here.";
                                                }
                                            }
                                        }
                                        else if (cell.CellGraphic[0].ToString() == "." && cell.CellGraphic[1].ToString() != " ")
                                        {
                                            switch (cell.CellGraphic)
                                            {
                                                case ".r":
                                                    cell.IsNoRecall = true;
                                                    break;
                                                case ".t":
                                                    cell.IsWithinTownLimits = true;
                                                    break;
                                                case ".0":
                                                    cell.IsOneHandClimbDown = true;
                                                    break;
                                                case ".1":
                                                    cell.IsOneHandClimbUp = true;
                                                    break;
                                                case ".2":
                                                    cell.IsTwoHandClimbDown = true;
                                                    break;
                                                case ".3":
                                                    cell.IsTwoHandClimbUp = true;
                                                    break;
                                                case ".f": // Black Fog
                                                    AreaEffect effect = new AreaEffect(Effect.EffectTypes.Black_Fog, Cell.GRAPHIC_EMPTY, 0, -1, null, cell);
                                                    break;
                                            }

                                            if (cell.CellGraphic != Cell.GRAPHIC_SAND)
                                                cell.CellGraphic = Cell.GRAPHIC_EMPTY;

                                        }
                                        #endregion

                                        #region else if Pit "()" set IsTwoHandClimbDown
                                        else if (cell.CellGraphic == "()")
                                        {
                                            cell.IsTwoHandClimbDown = true;
                                        }
                                        #endregion

                                        #region else if Pit Bottom "pb" set IsTwoHandClimbUp
                                        else if (cell.CellGraphic == "pb")
                                        {
                                            cell.IsTwoHandClimbUp = true;
                                            cell.CellGraphic = Cell.GRAPHIC_EMPTY;
                                        }
                                        #endregion

                                        #region else if Graveyard "gg" set IsUnderworldPortal
                                        else if (cell.CellGraphic == "gg")
                                        {
                                            cell.IsUnderworldPortal = true;
                                        }
                                        #endregion

                                        #region else if permanent Darkness Effect "??"
                                        else if (cell.CellGraphic == Cell.GRAPHIC_DARKNESS)
                                        {
                                            cell.IsAlwaysDark = true;
                                            cell.CellGraphic = Cell.GRAPHIC_DARKNESS;
                                        }
                                        #endregion

                                        #region else if Web Effect "wb" "w1" "w9"
                                        else if (cell.CellGraphic[0].ToString() == "w")
                                        {
                                            if (cell.CellGraphic == "wb" || cell.CellGraphic == Cell.GRAPHIC_WEB)
                                                effectLevel = Rules.RollD(1, 6);
                                            else if (cell.CellGraphic == "wr")
                                            {
                                                effectLevel = Rules.RollD(2, 4);
                                                cell.IsNoRecall = true;
                                            }
                                            else
                                            {
                                                if (!Int32.TryParse(cell.CellGraphic.Substring(1, 1), out effectLevel))
                                                {
                                                    Utils.Log("Illegal web cell in " + fileName + " at " + cell.GetLogString(false), Utils.LogType.SystemWarning);
                                                    effectLevel = Rules.RollD(1, 6);
                                                }
                                            }

                                            cell.CellGraphic = Cell.GRAPHIC_WEB; // permanent web

                                            AreaEffect effect = new AreaEffect(Effect.EffectTypes.Web, Cell.GRAPHIC_WEB, effectLevel, -1, null, cell);
                                        }
                                        #endregion

                                        #region else if Magic Dead wall "DD"
                                        else if (cell.CellGraphic == Map.RESERVED_GRAPHIC_IMPENETRABLE_WALL)
                                        {
                                            cell.CellGraphic = Cell.GRAPHIC_WALL;
                                            cell.IsMagicDead = true;
                                        }
                                        #endregion

                                        else if (cell.CellGraphic == Map.RESERVED_GRAPHIC_WALL_TOWN_LIMITS)
                                        {
                                            cell.CellGraphic = Cell.GRAPHIC_WALL;
                                            cell.IsWithinTownLimits = true;
                                        }
                                        else if (cell.CellGraphic == Map.RESERVED_GRAPHIC_IMPENETRABLE_WALL_TOWN_LIMITS)
                                        {
                                            cell.CellGraphic = Cell.GRAPHIC_WALL;
                                            cell.IsWithinTownLimits = true;
                                            cell.IsMagicDead = true;
                                        }

                                        #region else if Locker "LK"
                                        else if (cell.CellGraphic == Map.RESERVED_GRAPHIC_LOCKERS || cell.CellGraphic == Map.RESERVED_GRAPHIC_ORNIC_LOCKERS)
                                        {
                                            if (cell.CellGraphic == Map.RESERVED_GRAPHIC_ORNIC_LOCKERS) cell.IsOrnicLocker = true;

                                            cell.CellGraphic = Cell.GRAPHIC_EMPTY;
                                            cell.IsLocker = true;
                                            cell.HasMailbox = true;

                                            foreach (Cell next in Cell.GetApplicableCellArray(cell, 1))
                                                if (next != null && next.IsWithinTownLimits) cell.IsWithinTownLimits = true;
                                        }
                                        #endregion
                                        else if (cell.CellGraphic == Map.RESERVED_GRAPHIC_BOXING_RING)
                                        {
                                            if (cell.Description.Length > 0 && !cell.Description.Contains("boxing"))
                                                cell.Description += " You are standing in a boxing ring.";
                                            else cell.Description = "You are standing in a boxing ring.";
                                        }
                                        else if (cell.CellGraphic == Map.RESERVED_GRAPHIC_SCRIBING_CRYSTAL)
                                        {
                                            cell.CellGraphic = Cell.GRAPHIC_EMPTY;
                                            cell.HasScribingCrystal = true;
                                            if (cell.Description.Length > 0 && !cell.Description.Contains("scribing"))
                                                cell.Description += " There is a scribing crystal here.";
                                            else cell.Description = "There is a scribing crystal here.";
                                        }
                                        else if (cell.CellGraphic == Map.RESERVED_GRAPHIC_MIRROR)
                                        {
                                            cell.CellGraphic = Cell.GRAPHIC_EMPTY;
                                            cell.HasMirror = true;
                                            if (cell.Description.Length > 0)
                                                cell.Description += " There is a wall mirror here.";
                                            else cell.Description = "There is a wall mirror here.";
                                        }
                                        else if (cell.CellGraphic == Cell.GRAPHIC_LOCKED_DOOR_HORIZONTAL)
                                        {
                                            cell.CellGraphic = Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL;
                                            cell.IsLockedHorizontalDoor = true;
                                        }

                                        else if (cell.CellGraphic == Cell.GRAPHIC_LOCKED_DOOR_VERTICAL)
                                        {
                                            if (MapID == Map.ID_UNDERKINGDOM) { cell.CellGraphic = Cell.GRAPHIC_GRATE; }//mlt here
                                            else { cell.CellGraphic = Cell.GRAPHIC_CLOSED_DOOR_VERTICAL; }
                                            cell.IsLockedVerticalDoor = true;
                                        }
                                        else if (cell.CellGraphic == "ut")
                                        {
                                            cell.IsStairsUp = true;
                                            cell.CellGraphic = Cell.GRAPHIC_UPSTAIRS;
                                            cell.IsWithinTownLimits = true;
                                        }
                                        else if (cell.CellGraphic == "dt")
                                        {
                                            cell.IsStairsDown = true;
                                            cell.CellGraphic = Cell.GRAPHIC_DOWNSTAIRS;
                                            cell.IsWithinTownLimits = true;
                                        }
                                        #region Secret Door
                                        else if (cell.CellGraphic == Cell.GRAPHIC_SECRET_DOOR)
                                        {
                                            cell.IsSecretDoor = true;
                                            cell.CellGraphic = Cell.GRAPHIC_WALL;
                                        }
                                        else if (cell.CellGraphic == "Sr")
                                        {
                                            cell.IsSecretDoor = true;
                                            cell.IsNoRecall = true;
                                            cell.CellGraphic = Cell.GRAPHIC_WALL;
                                        }
                                        #endregion

                                        #region Secret Rockwall
                                        else if (cell.CellGraphic == Cell.GRAPHIC_SECRET_MOUNTAIN)
                                        {
                                            cell.IsSecretDoor = true;
                                            cell.CellGraphic = Cell.GRAPHIC_MOUNTAIN;
                                        }
                                        #endregion

                                        #region Random Trees / Berries "{}" GRAPHIC_FOREST_FULL
                                        else if (cell.CellGraphic == "{}" || cell.CellGraphic == Cell.GRAPHIC_FOREST_FULL)
                                        {
                                            int randomTree = Rules.RollD(1, 200);
                                            int randomBerries = Rules.RollD(1, 300);

                                            switch (ZPlanes[z].forestry)
                                            {
                                                case ForestedLevel.None:
                                                    randomBerries = 0;
                                                    break;
                                                case ForestedLevel.Barren:
                                                    randomBerries = 0;
                                                    randomTree -= 225; // either empty ground or light grass
                                                    break;
                                                case ForestedLevel.Very_Light:
                                                    randomTree -= 175;
                                                    randomBerries = 0;
                                                    break;
                                                case ForestedLevel.Light:
                                                    randomTree -= 100;
                                                    randomBerries -= 50;
                                                    break;
                                                case ForestedLevel.Medium:
                                                    randomTree -= 50;
                                                    break;
                                                case ForestedLevel.Heavy:
                                                    randomTree += 100;
                                                    break;
                                                case ForestedLevel.Very_Heavy:
                                                    randomTree += 175;
                                                    break;
                                            }

                                            if (randomTree <= -150)
                                            {
                                                if (ZPlanes[z].forestry > ForestedLevel.Barren)
                                                    cell.CellGraphic = Cell.GRAPHIC_GRASS_LIGHT;
                                                else cell.CellGraphic = Cell.GRAPHIC_EMPTY;
                                            }
                                            else if (randomTree < 0 && randomTree > -150)
                                                cell.CellGraphic = Cell.GRAPHIC_GRASS_THICK;
                                            else if (randomTree >= 0 && randomTree <= 200)
                                            {
                                                if (Rules.RollD(1, 20) <= 10)
                                                    cell.CellGraphic = Cell.GRAPHIC_FOREST_RIGHT;
                                                else cell.CellGraphic = Cell.GRAPHIC_FOREST_LEFT;
                                            }
                                            else
                                                cell.CellGraphic = Cell.GRAPHIC_FOREST_FULL;

                                            if (randomTree >= 0)
                                            {
                                                if (randomBerries >= 1 && randomBerries <= 4 && this.HasBalmBushes)
                                                {
                                                    cell.balmBerry = true;
                                                    cell.dailyFruit = Rules.Dice.Next(3, 60);
                                                    if (cell.Description.Length > 0)
                                                        cell.Description += " There is a bush covered with bright red berries here.";
                                                    else cell.Description = "There is a bush covered with bright red berries here.";
                                                }
                                                else if (randomBerries == 10 && randomBerries == 11 && HasPoisonBushes)
                                                {
                                                    cell.poisonBerry = true;
                                                    cell.dailyFruit = Rules.Dice.Next(2, 20);
                                                    if (cell.Description.Length > 0)
                                                    {
                                                        cell.Description += " There is a bush with pale yellow berries here.";
                                                    }
                                                    else
                                                    {
                                                        cell.Description = "There is a bush with pale yellow berries here.";
                                                    }
                                                }
                                                else if (randomBerries == 42 && HasManaBushes)
                                                {
                                                    cell.manaBerry = true;
                                                    cell.dailyFruit = Rules.Dice.Next(1, 6);
                                                    if (cell.Description.Length > 0)
                                                    {
                                                        cell.Description += " There is a bush with light blue berries here.";
                                                    }
                                                    else
                                                    {
                                                        cell.Description = "There is a bush with light blue berries here.";
                                                    }
                                                }
                                                else if (randomBerries == 98 && HasStaminaBushes)
                                                {
                                                    cell.stamBerry = true;
                                                    cell.dailyFruit = Rules.Dice.Next(1, 10);
                                                    if (cell.Description.Length > 0)
                                                    {
                                                        cell.Description += " There is a bush with dark green berries here.";
                                                    }
                                                    else
                                                    {
                                                        cell.Description = "There is a bush with dark green berries here.";
                                                    }
                                                }
                                            }
                                        }
                                        #endregion

                                        #region Random Dead Trees "{_"
                                        else if (cell.CellGraphic == "{_")
                                        {
                                            switch (Rules.RollD(1, 3))
                                            {
                                                case 1:
                                                    cell.CellGraphic = Cell.GRAPHIC_FOREST_BURNT_RIGHT;
                                                    break;
                                                case 2:
                                                    cell.CellGraphic = Cell.GRAPHIC_FOREST_BURNT_LEFT;
                                                    break;
                                                case 3:
                                                    cell.CellGraphic = Cell.GRAPHIC_FOREST_BURNT_FULL;
                                                    break;
                                            }
                                        }
                                        #endregion

                                        switch (cell.CellGraphic)
                                        {
                                            case Cell.GRAPHIC_MOUNTAIN:
                                            case Cell.GRAPHIC_REEF:
                                            case Cell.GRAPHIC_COUNTER:
                                            case Cell.GRAPHIC_ALTAR:
                                            case Cell.GRAPHIC_FOREST_IMPASSABLE:
                                            case Cell.GRAPHIC_ALTAR_PLACEABLE:
                                            case Cell.GRAPHIC_COUNTER_PLACEABLE:
                                                cell.IsMagicDead = true;
                                                foreach (Cell next in Cell.GetApplicableCellArray(cell, 1))
                                                    if (next != null && next.IsWithinTownLimits) cell.IsWithinTownLimits = true;
                                                break;
                                        }

                                        cell.DisplayGraphic = Map.returnTile(cell.CellGraphic);
                                    }
                                    else
                                    {
                                        cell = new Cell(facetID, landID, mapID, x, y, z, Cell.GRAPHIC_MOUNTAIN);
                                        cell.IsOutOfBounds = true;
                                        Tuple<int, int, int> key = new Tuple<int, int, int>(x, y, z);
                                        if (!this.cells.ContainsKey(key))
                                        {
                                            this.Add(key, cell);
                                        }
                                        else
                                        {
                                            Utils.Log("Failed to add Cell(second) to " + this.Name + " " + x + "," + y + "," + z, Utils.LogType.SystemFailure);
                                            throw new Exception();
                                        }
                                    }
                                }
                                a += 2;
                                x++;
                                if (x > ZPlanes[z].xcordMax)
                                {
                                    ZPlanes[z].xcordMax = x;
                                    //Utils.Log("Map: " + cell.Map.name + " ZPlane: " + ZPlanes[z].name + " xcordMax: " + ZPlanes[z].xcordMax, Utils.LogType.Debug);
                                }
                            }
                            y++;
                            if (y > ZPlanes[z].ycordMax)
                            {
                                ZPlanes[z].ycordMax = y;
                                //Utils.Log("Map: " + cell.Map.name + " ZPlane: " + ZPlanes[z].name + " ycordMax: " + ZPlanes[z].ycordMax, Utils.LogType.Debug);
                            }
                            #endregion
                        }
                    }
                }

                // now that the cells are all created, we need to calculate the display on each one
                this.UpdateMapCellVisible();

                // load the cell information from the Cell table
                List<Cell> cellsList = DAL.DBWorld.GetCellList(this.Name);

                //Tuple<int, int, int> key;

                foreach (Cell iCell in cellsList)
                {
                    Tuple<int, int, int> key = new Tuple<int, int, int>(iCell.X, iCell.Y, iCell.Z);

                    if (this.cells.ContainsKey(key))
                    {
                        this.cells[key].Segue = iCell.Segue;
                        this.cells[key].Description = this.cells[key].Description.Trim();
                        this.cells[key].Description = iCell.Description + " " + this.cells[key].Description;
                        this.cells[key].Description = this.cells[key].Description.Trim();
                        this.cells[key].cellLock = iCell.cellLock;
                        this.cells[key].IsMapPortal = iCell.IsMapPortal;
                        this.cells[key].IsTeleport = iCell.IsTeleport;
                        this.cells[key].IsSingleCustomer = iCell.IsSingleCustomer;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return false;
            }
        }

        public void UpdateMapCellVisible()
        {
            //Now that the cells are all created, we need to calculate the display on each one
            int y = 0;
            int x = 0;

            try
            {
                foreach (int z in this.ZPlanes.Keys)//z = 0; z < this.zPlanes.Count; z++)
                {
                    y = this.ZPlanes[z].ycordMin;

                    while (y <= this.ZPlanes[z].ycordMax)
                    {
                        while (x <= this.ZPlanes[z].xcordMax)
                        {
                            Tuple<int, int, int> key = new Tuple<int, int, int>(x, y, z);

                            if (this.cells.ContainsKey(key))
                            {
                                this.UpdateCellVisible(this.cells[key]);
                            }
                            x++;
                        }
                        y++;
                        x = this.ZPlanes[z].xcordMin;
                    }
                }
            }
            catch (Exception e)
            {
                Utils.Log("UpdateDisplay Failed: " + e.Message + "  Map = " + this.Name + "  Y = " + y + "  X = " + x, Utils.LogType.SystemFailure);
            }
        }

        public void UpdateVisionCellVisible(Cell cell)
        {
            if (cell == null)
                return;

            int y = cell.Y - 3;
            int x = cell.X - 3;

            try
            {
                while (y <= cell.Y + 3)
                {
                    while (x <= cell.X + 3)
                    {
                        Tuple<int, int, int> key = new Tuple<int, int, int>(x, y, cell.Z);

                        if (cells.ContainsKey(key))
                        {
                            UpdateCellVisible(cells[key]);
                        }
                        x++;
                    }
                    y++;
                    x = cell.X - 3;
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public bool VisBlockedXYZ(int x, int y, int z)
        {
            Tuple<int, int, int> key = new Tuple<int, int, int>(x, y, z);

            if (cells.ContainsKey(key))
                return IsVisionBlocked(cells[key]);

            return false;
        }

        public string GetZName(int zCord)
        {
            if (zPlaneInfos.ContainsKey(zCord))
                return zPlaneInfos[zCord].name;

            return "";
        }

        #region Erinoc LOS Code
        /* Map position reminder:
             * 0001xx03xx0506
             * 07080910111213
             * xx1516171819xx
             * 212223==252627
             * xx2930313233xx
             * 35363738494041
             * 4243xx45xx4748
             */

        bool ApplyViewRules(Cell c, int dx, int dy)
        {
            int x = c.X, y = c.Y, z = c.Z;
            int mx = dx + 3, my = dy + 3;
            int ax = Math.Abs(dx), ay = Math.Abs(dy);
            int sx = Math.Sign(dx), sy = Math.Sign(dy);
            int dx2, dy2;

            if (dx == 0 || dy == 0 || ax == ay)
            {
                // Rule 1 - If on direct cross or diag, check the one closer on the same axis
                if (ax <= 1 && ay <= 1)
                {
                    return true; // this fixes the trees around the character
                }
                if (c.visCells[(mx - sx) + 7 * (my - sy)])
                {
                    return !VisBlockedXYZ(x + dx - sx, y + dy - sy, z);
                }
                return false;

            }

            dx2 = ax > ay ? sx : 0;
            dy2 = ay > ax ? sy : 0;

            if (ax == 1 || ay == 1)
            {
                // Rule 2 - If one off of horizontal or vertical, check the two closer via move to diag and move to cross
                if (VisBlockedXYZ(x + sx, y + sy, z) && VisBlockedXYZ(x + dx2, y + dy2, z))
                {
                    return false;
                }
                // Rule 2a - if three away, match the one two away
                if (Math.Max(ax, ay) == 3)
                {
                    if (VisBlockedXYZ(x + dx - sx, y + dy - sy, z) && VisBlockedXYZ(x + dx - dx2, y + dy - dy2, z))
                    {
                        return false;
                    }
                    return true;
                }
                return true;
            }
            // Rule 3 - If at 3,2 (with reflection) then false if either 2,1 or 1,1 is blocked
            // Double the dx's.  They start as +-1, 0 or 0, +-1, so this gives the relative move to 1,1
            dx2 += sx;
            dy2 += sy;
            if (c.visCells[(mx - sx) + 7 * (my - sy)] && c.visCells[(mx - dx2) + 7 * (my - dy2)])
            {
                if (!VisBlockedXYZ(x + dx - sx, y + dy - sy, z) && !VisBlockedXYZ(x + dx - dx2, y + dy - dy2, z))
                {
                    return true;
                }
            }
            return false;
        }

        public void UpdateCellVisible(Cell cell)
        {
            for (int round = 1; round <= 3; round++)
            {
                for (int i = -round; i < round; i++)
                {
                    cell.visCells[(3 - round) + 7 * (3 - i)] = ApplyViewRules(cell, -round, -i);
                    cell.visCells[(3 + round) + 7 * (3 + i)] = ApplyViewRules(cell, round, i);
                    cell.visCells[(3 + i) + 7 * (3 - round)] = ApplyViewRules(cell, i, -round);
                    cell.visCells[(3 - i) + 7 * (3 + round)] = ApplyViewRules(cell, -i, round);
                }
            }

            cell.visCells[Cell.CENTER_VISIBLE_CELL] = true; // cell a Character is in is always visible?
        }
        #endregion
    }
}
