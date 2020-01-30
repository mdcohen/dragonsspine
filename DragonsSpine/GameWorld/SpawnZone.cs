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
    public class SpawnZone
    {
        /// <summary>
        /// SpawnZoneDivisor affects how many critters spawn in an Entity Creation Manager instantiated SpawnZone. The
        /// </summary>
        const int SpawnZoneDivisor = 7; // this affects how many mobs spawn in an Entity Creation Manager instanced SpawnZone

        public const int MAX_SPAWN_ATTEMPTS = 160;

        static Dictionary<int, SpawnZone> spawnsDictionary = new Dictionary<int, SpawnZone>(); // key = zoneID, value = SpawnZone

        #region Private Data
        private bool m_enabled = false;
        protected readonly short m_landID = 0;
        protected readonly short m_mapID = 0;
        protected readonly int m_maxAllowed = 0;
        protected readonly int m_npcID = 0;
        protected readonly List<int> m_npcList;
        private short m_numInZone = 0;
        protected readonly int m_radius = 0;
        protected readonly string m_spawnMessage = "";
        private int m_spawnTimer = 0;
        protected readonly int m_zoneID = 0;
        private int m_timer = 0;
        protected readonly int m_spawnX = 0;
        protected readonly int m_spawnY = 0;
        private int m_spawnZ = 0;
        protected readonly List<Tuple<int,int,int>> m_spawnCellsList = new List<Tuple<int,int,int>>(); // list of keys used to get cells from map.cells
        protected readonly List<int> m_spawnZRangeList = new List<int>(); // list of random z coordinates
        protected Dictionary<int, Tuple<int, int>> m_spawnGroupAmounts = new Dictionary<int, Tuple<int, int>>(); // Key = npcID, Tuple<low, high>
        protected bool m_isAutonomous = false;
        protected bool m_isUniqueEntity = false;
        #endregion

        #region Public Properties
        public bool IsEnabled
        {
            get { return this.m_enabled; }
            set { this.m_enabled = value; }
        }
        public int ZoneID
        {
            get { return this.m_zoneID; }
        }
        public int NPCID
        {
            get { return this.m_npcID; }
        }
        public int SpawnTimer
        {
            get { return this.m_spawnTimer; }
        }
        public int Timer
        {
            get { return this.m_timer; }
            set { this.m_timer = value; }
        }
        public int MaxAllowedInZone
        {
            get { return this.m_maxAllowed; }
        }
        public short NumberInZone
        {
            get { return this.m_numInZone; }
            set { this.m_numInZone = value; }
        }
        public string SpawnMessage
        {
            get { return this.m_spawnMessage; }
        }
        public List<int> NPCList
        {
            get { return this.m_npcList; }
        }
        public short LandID
        {
            get { return this.m_landID; }
        }
        public short MapID
        {
            get { return this.m_mapID; }
        }
        public int X
        {
            get { return this.m_spawnX; }
        }
        public int Y
        {
            get { return this.m_spawnY; }
        }
        public int Z
        {
            get { return m_spawnZ; }
            set { m_spawnZ = value; }
        }
        public int Radius
        {
            get { return this.m_radius; }
        }
        public List<Tuple<int,int,int>> SpawnCells
        {
            get { return this.m_spawnCellsList; }
        }
        public List<int> SpawnZRange
        {
            get { return this.m_spawnZRangeList; }
        }
        public bool IsAutonomous
        {
            get { return m_isAutonomous; }
        }
        public bool IsUniqueEntity
        {
            get { return m_isUniqueEntity; }
        }

        /// <summary>
        /// Whether grouped NPCs will spawn in this SpawnZone.
        /// </summary>
        public bool IsAsocial
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor used to create a SpawnZone from the EntityCreationManager.
        /// </summary>
        public SpawnZone(Facet facet, int npcID, List<int> npcIDList, Dictionary<int, Tuple<int, int>> spawnGroupAmounts, short landID, short mapID, int spawnZ, bool limitToWater)
        {
            m_enabled = true;
            m_zoneID = CreateUniqueSpawnZoneID(facet);

            if (m_zoneID < 0)
            {
                Utils.Log("Failed to create new SpawnZone from EntityCreationManager. Invalid ZoneID.", Utils.LogType.SystemWarning);
                return;
            }

            m_npcID = npcID;
            m_spawnMessage = "";
            m_npcList = new List<int>();
            m_npcList.AddRange(npcIDList);
            m_landID = landID;
            m_mapID = mapID;
            m_spawnZ = spawnZ;

            m_isAutonomous = true;
            m_spawnGroupAmounts = spawnGroupAmounts;

            EstablishSpawnRadius(limitToWater);

            int properSpawnCellsCount = 0;

            foreach (Tuple<int, int, int> tuple in m_spawnCellsList)
            {
                Cell cell = Cell.GetCell(0, m_landID, m_mapID, tuple.Item1, tuple.Item2, tuple.Item3);

                // TODO: Need to move this information into a public array as it is accessed throughout the logic. 12/7/2015 Eb
                // These cells won't count toward determining a spawn amount for the ECM SpawnZone.
                switch (cell.CellGraphic)
                {
                    case Cell.GRAPHIC_WALL:		//Don't spawn in wall
                    case Cell.GRAPHIC_GRATE:    //Don't spawn in a grate
                    case Cell.GRAPHIC_REEF:		//Don't spawn in wall
                    case Cell.GRAPHIC_MOUNTAIN:     //Don't spawn in mountain
                    case Cell.GRAPHIC_FOREST_IMPASSABLE:		//Don't spawn in Impenetrible forest
                    case Cell.GRAPHIC_SECRET_DOOR:		//Don't spawn in secret door
                    case Cell.GRAPHIC_ALTAR:		//Don't spawn in an altar
                    case Cell.GRAPHIC_ALTAR_PLACEABLE:
                    case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:		//Don't spawn in closed doors
                    case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:		//Don't spawn in closed doors
                    case Cell.GRAPHIC_COUNTER:		//Don't spawn in counters
                    case Cell.GRAPHIC_COUNTER_PLACEABLE:
                        continue;
                }

                if (cell != null && !limitToWater && cell.CellGraphic != Cell.GRAPHIC_WATER)
                    properSpawnCellsCount++;
                else if (cell != null && limitToWater && cell.CellGraphic == Cell.GRAPHIC_WATER)
                    properSpawnCellsCount++;
            }

            ZAutonomy zAuto = World.GetFacetByID(facet.FacetID).GetLandByID(landID).GetMapByID(mapID).ZPlanes[spawnZ].zAutonomy;

            int divisor = SpawnZoneDivisor - zAuto.spawnIntensityMod;

            if (divisor <= 0)
            {
                divisor = 1; // max spawns
                Utils.Log("SpawnZoneDivisor less than 0 for " + World.GetFacetByID(facet.FacetID).GetLandByID(landID).GetMapByID(mapID).GetZName(spawnZ) + ". Check spawnIntensityMod.", Utils.LogType.SystemWarning);
            }

            m_maxAllowed = Math.Abs(properSpawnCellsCount / divisor);

            if (m_maxAllowed < 1) m_maxAllowed = Rules.RollD(1, 6);

            m_spawnTimer = 120;
            m_timer = 10000; // ???

            IsAsocial = zAuto.asocial;
        }

        /// <summary>
        /// Constructor used to create a SpawnZone for a Unique Entity in the EntityCreationManager.
        /// </summary>
        public SpawnZone(Facet facet, ZUniqueEntity zUnique, NPC npc, short landID, short mapID, int spawnZ, bool limitToWater)
        {
            m_enabled = true;
            m_zoneID = CreateUniqueSpawnZoneID(facet);

            if (m_zoneID < 0)
            {
                Utils.Log("Failed to create new Unique Entity SpawnZone from EntityCreationManager. Invalid ZoneID.", Utils.LogType.SystemWarning);
                return;
            }

            m_npcID = npc.npcID;
            m_spawnMessage = zUnique.spawnMessage;
            m_npcList = new List<int>();
            m_landID = landID;
            m_mapID = mapID;
            m_spawnX = zUnique.xCord;
            m_spawnY = zUnique.yCord;
            m_spawnZ = spawnZ;
            m_radius = zUnique.spawnRadius;

            if (!string.IsNullOrEmpty(zUnique.zRange))
                m_spawnZRangeList.AddRange(Utils.ConvertStringToIntArray(zUnique.zRange));

            m_isAutonomous = false;

            // Highly unlikely x and y coordinates will both be 0 -- this means coordinates were not set in the map text file, make it random in EstablishSpawnRadius
            if (m_spawnX == 0 && m_spawnX == 0) m_isAutonomous = true;

            m_isUniqueEntity = true;

            EstablishSpawnRadius(limitToWater);

            m_maxAllowed = 1;
            m_spawnTimer = zUnique.spawnTimer;
            m_timer = 10000; // ???
        }

        public SpawnZone(System.Data.DataRow dr)
        {
            try
            {
                this.m_enabled = Convert.ToBoolean(dr["enabled"]);
                this.m_zoneID = Convert.ToInt32(dr["zoneID"]);
                this.m_npcID = Convert.ToInt16(dr["npcID"]);
                this.m_spawnTimer = Convert.ToInt32(dr["spawnTimer"]);
                this.m_spawnMessage = dr["spawnMessage"].ToString();
                this.m_npcList = new List<int>();
                if(dr["npcList"].ToString() != "")
                    this.m_npcList.AddRange(Utils.ConvertStringToIntArray(dr["npcList"].ToString()));
                int minZone = Convert.ToInt32(dr["minZone"]);
                int maxZone = Convert.ToInt32(dr["maxZone"]);
                if (minZone > 0 && maxZone > 0)
                {
                    this.m_maxAllowed = Rules.Dice.Next(minZone, maxZone + 1);
                }
                else
                {
                    this.m_maxAllowed = Convert.ToInt32(dr["maxAllowedInZone"]);
                }
                this.m_landID = Convert.ToInt16(dr["spawnLand"]);
                this.m_mapID = Convert.ToInt16(dr["spawnMap"]);
                this.m_spawnX = Convert.ToInt16(dr["spawnX"]);
                this.m_spawnY = Convert.ToInt16(dr["spawnY"]);
                this.m_spawnZ = Convert.ToInt32(dr["spawnZ"]);
                this.m_radius = Convert.ToInt16(dr["spawnRadius"]);
                if (this.m_radius > 0)
                {
                    this.EstablishSpawnRadius(false);
                }
                if (dr["spawnZRange"].ToString() != "")
                {
                    string[] zRange = dr["spawnZRange"].ToString().Trim().Split(" ".ToCharArray());
                    for(int a = 0; a < zRange.Length; a++)
                    {
                        this.m_spawnZRangeList.Add(Convert.ToInt32(zRange[a]));
                    }
                }

                this.m_isAutonomous = false;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }
        #endregion

        #region Public Methods
        public void EstablishSpawnRadius(bool limitToWater)
        {
            m_spawnCellsList.Clear();

            // Sometimes Unique Entities will not be given a spawnX and spawnY. This means all cells should be viable.
            if (IsAutonomous)
            {
                foreach (Cell cell in World.GetFacetByIndex(0).GetLandByID(this.LandID).GetMapByID(this.MapID).cells.Values)
                {
                    if (cell.IsOutOfBounds) continue;

                    if (!limitToWater)
                    {
                        if (cell.Z == Z && !cell.IsOutOfBounds && !cell.IsWithinTownLimits)
                        {
                            switch (cell.CellGraphic)
                            {
                                case Cell.GRAPHIC_WALL:
                                case Map.RESERVED_GRAPHIC_IMPENETRABLE_WALL:
                                case Cell.GRAPHIC_GRATE:
                                case Cell.GRAPHIC_REEF:
                                case Cell.GRAPHIC_MOUNTAIN:
                                case Cell.GRAPHIC_FOREST_IMPASSABLE:
                                case Cell.GRAPHIC_SECRET_DOOR:
                                case Cell.GRAPHIC_ALTAR:
                                case Cell.GRAPHIC_ALTAR_PLACEABLE:
                                case Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL:
                                case Cell.GRAPHIC_OPEN_DOOR_VERTICAL:
                                case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:
                                case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:
                                case Cell.GRAPHIC_COUNTER:
                                case Cell.GRAPHIC_COUNTER_PLACEABLE:
                                case Cell.GRAPHIC_AIR:
                                case Cell.GRAPHIC_FIRE:
                                    break;
                                default:
                                    m_spawnCellsList.Add(Tuple.Create(cell.X, cell.Y, Z));
                                    break;
                            }
                        }
                    }
                    else if (cell.CellGraphic == Cell.GRAPHIC_WATER)
                    {
                        m_spawnCellsList.Add(Tuple.Create(cell.X, cell.Y, Z));
                    }
                }
            }
            else
            {
                Tuple<int, int, int> key;
                int finalX = 0;
                int finalY = 0;

                for (int x = -Radius; x <= Radius; x++)
                {
                    for (int y = -Radius; y <= Radius; y++)
                    {
                        finalX = X + x;
                        finalY = Y + y;
                        key = new Tuple<int, int, int>(finalX, finalY, Z);
                        if (World.GetFacetByIndex(0).GetLandByID(LandID).GetMapByID(MapID).cells.ContainsKey(key))
                        {
                            Cell cell = Cell.GetCell(World.GetFacetByIndex(0).FacetID, LandID, MapID, finalX, finalY, Z);

                            if (cell != null && !cell.IsOutOfBounds)
                            {
                                switch (cell.CellGraphic)
                                {
                                    case Cell.GRAPHIC_WALL:
                                    case Map.RESERVED_GRAPHIC_IMPENETRABLE_WALL:
                                    case Cell.GRAPHIC_GRATE:
                                    case Cell.GRAPHIC_REEF:
                                    case Cell.GRAPHIC_MOUNTAIN:
                                    case Cell.GRAPHIC_FOREST_IMPASSABLE:
                                    case Cell.GRAPHIC_SECRET_DOOR:
                                    case Cell.GRAPHIC_ALTAR:
                                    case Cell.GRAPHIC_ALTAR_PLACEABLE:
                                    case Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL:
                                    case Cell.GRAPHIC_OPEN_DOOR_VERTICAL:
                                    case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:
                                    case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:
                                    case Cell.GRAPHIC_COUNTER:
                                    case Cell.GRAPHIC_COUNTER_PLACEABLE:
                                    case Cell.GRAPHIC_AIR:
                                    case Cell.GRAPHIC_FIRE:
                                        break;
                                    default:
                                        m_spawnCellsList.Add(Tuple.Create(finalX, finalY, Z));
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void EstablishSpawnRadius(int zCord)
        {
            this.m_spawnCellsList.Clear();

            Map map = World.GetFacetByIndex(0).GetLandByID(LandID).GetMapByID(MapID);

            Tuple<int, int, int> key;

            for (int x = map.ZPlanes[zCord].xcordMin; x <= map.ZPlanes[zCord].xcordMax; x++)
            {
                for (int y = map.ZPlanes[zCord].ycordMin; y <= map.ZPlanes[zCord].ycordMax; y++)
                {
                    key = new Tuple<int, int, int>(x, y, zCord);

                    if (map.cells.ContainsKey(key))
                    {
                        m_spawnCellsList.Add(Tuple.Create(x, y, zCord));
                    }
                }
            }
        }

        public int GetGroupAmount(int npcID)
        {
            if (m_spawnGroupAmounts.ContainsKey(npcID))
            {
                return Rules.Dice.Next(m_spawnGroupAmounts[npcID].Item1, m_spawnGroupAmounts[npcID].Item2);
            }
            else return 0;
        }
        #endregion

        #region Static Methods and Properties
        public static Dictionary<int, SpawnZone> Spawns
        {
            get
            {
                return SpawnZone.spawnsDictionary;
            }
        }

        public static void Add(SpawnZone szl)
        {
            SpawnZone.spawnsDictionary.Add(szl.ZoneID, szl);
        }

        public static void AddSpawnZonesToList(ArrayList zones)
        {
            foreach (SpawnZone zone in zones)
            {
                SpawnZone.spawnsDictionary.Add(zone.ZoneID, zone);
            }
        }
        #endregion

        #region Static Functions
        public static bool LoadSpawnZones()
        {
            return DAL.DBWorld.LoadSpawnZones();
        }

        public static void ReloadSpawnZones()
        {
            Spawns.Clear();

            foreach (Facet facet in World.Facets)
            {
                facet.Spawns.Clear();
            }

            if (SpawnZone.LoadSpawnZones())
            {
                Facet.EstablishSpawnZones();
            }
        }

        private static int CreateUniqueSpawnZoneID(Facet facet)
        {
            for (int a = 500; a < 2000; a++)
            {
                if (!facet.Spawns.ContainsKey(a))
                {
                    return a;
                }
            }

            return -1;
        }
        #endregion
    }
}
