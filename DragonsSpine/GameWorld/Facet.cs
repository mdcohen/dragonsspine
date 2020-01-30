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
using System.Text;

namespace DragonsSpine.GameWorld
{
    public class Facet
    {
        #region Private Data
        protected readonly short m_id;
        protected readonly string m_name;
        protected Dictionary<int, Land> m_landDict;
        protected Dictionary<int, SpawnZone> m_spawnsDict;
        #endregion

        #region Public Properties
        public short FacetID
        {
            get { return this.m_id; }
        }

        public string Name
        {
            get { return this.m_name; }
        }

        public Dictionary<int, Land>.ValueCollection Lands
        {
            get { return this.m_landDict.Values; }
        }

        public Dictionary<int, SpawnZone> Spawns
        {
            get { return this.m_spawnsDict; }
        }
        #endregion

        #region Constructor
        public Facet(System.Data.DataRow dr)
        {
            m_id = Convert.ToInt16(dr["facetID"]);
            m_name = dr["name"].ToString();
            m_landDict = new Dictionary<int, Land>();
            m_spawnsDict = new Dictionary<int, SpawnZone>();
        } 
        #endregion

        #region Public Methods
        public bool LoadLands()
        {
            return DAL.DBWorld.LoadLands(this);
        }

        public void Add(Land land)
        {
            this.m_landDict.Add(land.LandID, land);
            //Utils.Log("Added " + land.Name + " to " + this.Name + " Facet.", Utils.LogType.SystemGo);
        }

        public void Add(SpawnZone szl)
        {
            this.m_spawnsDict.Add(szl.ZoneID, szl);
        }

        public Land GetLandByID(int landID) // get land by landID
        {
            if (this.m_landDict.ContainsKey(landID))
            {
                return this.m_landDict[landID];
            }
            Utils.Log("facet.GetLandByID(" + landID + ") returned null. Facet: " + this.Name, Utils.LogType.SystemWarning);
            return null;
        }

        public Land GetLandByIndex(int index) // get land by index in landList
        {
            int a = 0;
            foreach (Land land in this.m_landDict.Values)
            {
                if (a == index)
                {
                    return land;
                }
                a++;
            }
            Utils.Log("facet.GetLandByIndex(" + index + ") returned null. Facet: " + this.Name, Utils.LogType.SystemWarning);
            return null;
        } 
        #endregion

        #region Static Methods
        public static bool EstablishSpawnZones()
        {
            try
            {
                foreach (Facet facet in World.Facets)
                {
                    foreach (SpawnZone sz in SpawnZone.Spawns.Values)
                    {
                        if (sz.Timer != 10000) sz.Timer = 10000;

                        facet.Add(sz);

                        //if (sz.IsEnabled && Cell.GetCell(facet.FacetID, sz.LandID, sz.MapID, sz.X, sz.Y, sz.Z) != null)
                        //{
                        //    Cell.GetCell(facet.FacetID, sz.LandID, sz.MapID, sz.X, sz.Y, sz.Z).Add(sz);
                        //}
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

        public static void ReloadSpawnZones()
        {
            SpawnZone.Spawns.Clear();
            EstablishSpawnZones();
        }
        #endregion
    }
}
