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
using System.IO;

namespace DragonsSpine.GameWorld
{
    /// <summary>
    /// A Land is a collection of Map objects. Some rules are in place for Lands eg: one way portal to the Advanced Game map from the Beginner's Game.
    /// </summary>
    public class Land
    {
        public const int ID_BEGINNERSGAME = 0;
        public const int ID_ADVANCEDGAME = 1;
        public const int ID_UNDERWORLD = 2;

        #region Private Data
        protected readonly short m_facetID;
        protected readonly short m_landID;
        protected readonly string m_name; // name of this land (ie: Beginner's Game)
        protected readonly string m_shortDesc; // short description (ie: BG)
        protected readonly string m_longDesc; // a detailed description of this land
        protected readonly int[] m_hitDice; // none, fighter, thaum, wizard, ma, thief, knight
        protected readonly int[] m_manaDice;
        protected readonly int[] m_staminaDice;
        protected readonly int m_statCapOperand; // added to the stat cap formula (eg: hits cap = hitDice * level + statCapOperand)
        protected readonly int m_maximumAbilityScore; // the maximum a player may raise an ability score to in this land
        protected readonly int m_maximumShielding; // the maximum amount of shielding a player may have in this land
        protected readonly int m_maximumTempAbilityScore; // the maximum amount of temporary stat a player may have in this land
        protected Dictionary<int, Map> m_mapDict; // array of Map objects in this Land
        protected long m_lottery; // holds the lottery amount for the land
        protected List<int> m_lotteryParticipants; // holds the list of lotto participants
        #endregion

        #region Properties
        public short FacetID
        {
            get { return this.m_facetID; }
        }
        public short LandID
        {
            get { return this.m_landID; }
        }
        public string Name
        {
            get { return this.m_name; }
        }
        public string ShortDesc
        {
            get { return this.m_shortDesc; }
        }
        public string LongDesc
        {
            get { return this.m_longDesc; }
        }
        public int[] HitDice
        {
            get { return this.m_hitDice; }
        }
        public int[] ManaDice
        {
            get { return this.m_manaDice; }
        }
        public int[] StaminaDice
        {
            get { return this.m_staminaDice; }
        }
        public int StatCapOperand
        {
            get { return this.m_statCapOperand; }
        }
        public int MaxAbilityScore
        {
            get { return this.m_maximumAbilityScore; }
        }
        public int MaxShielding
        {
            get { return this.m_maximumShielding; }
        }
        public int MaxTempAbilityScore // unused as of 10/17/2019
        {
            get { return this.m_maximumTempAbilityScore; }
        }
        public Dictionary<int, Map> MapDictionary
        {
            get { return this.m_mapDict; }
        }
        public Dictionary<int, Map>.ValueCollection Maps
        {
            get { return this.m_mapDict.Values; }
        }
        public long Lottery
        {
            get { return m_lottery; }
            set { m_lottery = value; }
        }
        public List<int> LotteryParticipants
        {
            get { return m_lotteryParticipants; }
        }
        #endregion

        #region Constructor
        public Land(short facetID, System.Data.DataRow dr)
        {
            m_facetID = facetID;
            m_landID = Convert.ToInt16(dr["landID"]);
            m_name = dr["name"].ToString();
            m_shortDesc = dr["shortDesc"].ToString();
            m_longDesc = dr["longDesc"].ToString();
            //int len = Enum.GetNames(typeof(Character.ClassType)).Length;
            //m_hitDice = new int[len];
            //string[] s = dr["hitDice"].ToString().Split(" ".ToCharArray());
            //for (int a = 0; a < m_hitDice.Length; a++)
            //{
            //    m_hitDice[a] = Convert.ToInt32(s[a]);
            //}
            //m_manaDice = new int[len];
            //s = dr["manaDice"].ToString().Split(" ".ToCharArray());
            //for (int a = 0; a < m_manaDice.Length; a++)
            //{
            //    m_manaDice[a] = Convert.ToInt32(s[a]);
            //}
            //m_staminaDice = new int[len];
            //s = dr["staminaDice"].ToString().Split(" ".ToCharArray());
            //for (int a = 0; a < m_manaDice.Length; a++)
            //{
            //    m_staminaDice[a] = Convert.ToInt32(s[a]);
            //}
            m_statCapOperand = Convert.ToInt32(dr["statCapOperand"]);
            m_maximumAbilityScore = Convert.ToInt32(dr["maximumAbilityScore"]);
            m_maximumShielding = Convert.ToInt32(dr["maximumShielding"]);
            m_maximumTempAbilityScore = Convert.ToInt32(dr["maximumTempAbilityScore"]);
            m_mapDict = new Dictionary<int, Map>();
            m_lottery = Convert.ToInt64(dr["lottery"]);
            m_lotteryParticipants = new List<int>();

            string[] lottoParts = dr["lotteryParticipants"].ToString().Split(ProtocolYuusha.ASPLIT.ToCharArray());

            if (dr["lotteryParticipants"].ToString() != "")
            {
                for (int a = 0; a < lottoParts.Length; a++)
                {
                    LotteryParticipants.Add(Convert.ToInt32(lottoParts[a]));
                }
            }
        }
        #endregion

        public bool FillLand() // fill this land with the appropriate maps
        {
            try
            {
                DAL.DBWorld.LoadMaps(this);

                string mapBase = "..\\..\\maps";

                foreach (Map map in this.Maps)
                {
                    if (!map.LoadMap(mapBase + "\\" + map.Name + ".txt", this.FacetID, this.LandID, map.MapID))
                    {
                        Utils.Log("Land.FillLand() failed while loading map file for " + map.Name + ".", Utils.LogType.SystemFailure);
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

        public void Add(Map map)
        {
            m_mapDict.Add(map.MapID, map);
            //Utils.Log("Added " + map.Name + " to " + this.Name + " in " + World.GetFacetByID(this.FacetID).Name + " Facet.", Utils.LogType.SystemGo);
        }

        public Map GetMapByID(int mapID)
        {
            if (this.MapDictionary.ContainsKey(mapID))
                return this.MapDictionary[mapID];
            Utils.Log("Land.GetMapByID(" + mapID + ") returned null (Land = " + this.Name + "[" + this.LandID + "]).", Utils.LogType.SystemWarning);
            return null;
        }

        public Map GetMapByIndex(int index)
        {
            int a = 0;
            foreach (Map map in this.Maps)
            {
                if (a == index)
                {
                    return map;
                }
                a++;
            }
            Utils.Log("Land.GetMapByIndex(" + index + ") returned null (Land = " + this.Name + "[" + this.LandID + "]).", Utils.LogType.SystemWarning);
            return null;
        }
    }
}
