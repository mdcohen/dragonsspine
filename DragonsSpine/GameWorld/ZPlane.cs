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

namespace DragonsSpine.GameWorld
{
    /// <summary>
    /// ZPlane is currently setup in the Map.LoadMaps method using flags in the map .txt files.
    /// </summary>
    public class ZPlane
    {
        /// <summary>
        /// Default values for ZPlanes.
        /// </summary>
        /// <param name="z">The Z plane.</param>
        /// <param name="map">The Map the Z plane is contained in.</param>
        //[Serializable]
        public ZPlane(int z, Map map, string landShortDesc)
        {
            this.mapName = map.Name;
            this.landShortDesc = landShortDesc;
            zCord = z;
            name = "";
            isOutdoors = false;
            balmBushes = map.HasBalmBushes;
            manaBushes = map.HasManaBushes;
            poisonBushes = map.HasPoisonBushes;
            staminaBushes = map.HasStaminaBushes;
            shortDesc = "";
            longDesc = "";
            pvpEnabled = map.IsPVPEnabled;
            experienceModifier = map.ExperienceModifier;
            difficulty = map.Difficulty;
            climate = map.Climate;
            forestry = Map.ForestedLevel.Medium;
            lightLevel = Map.LightLevel.Normal;
            withinTownLimits = false;
            alwaysDark = false;
            isNoRecall = false;
            xcordMax = 0;
            xcordMin = 0;
            ycordMax = 0;
            ycordMin = 0;
            zAutonomy = null;
            spawnAlignment = Globals.eAlignment.None;
            hasSandyBeaches = false;
            isCoexistent = false;
            swampy = false;
            icy = false;
        }

        public int zCord;
        public string name;
        public bool isOutdoors;
        public bool balmBushes;
        public bool manaBushes;
        public bool poisonBushes;
        public bool staminaBushes;
        public string shortDesc;
        public string longDesc;
        public bool pvpEnabled;
        public double experienceModifier;
        public int difficulty;
        public Map.ClimateType climate;
        public Map.ForestedLevel forestry;
        public Map.LightLevel lightLevel;
        public bool withinTownLimits;
        public bool alwaysDark;
        public bool isNoRecall;
        public int xcordMax;
        public int xcordMin;
        public int ycordMax;
        public int ycordMin;
        public Globals.eAlignment spawnAlignment; // all spawns on the zPlane will this alignment
        public string mapName; // the name of the map this zPlane is in
        public string landShortDesc; // the short description of the land this zPlane is in
        public bool hasSandyBeaches;
        public bool isCoexistent; // all mobs work together regardless of alignment
        public bool swampy;
        public bool icy;

        public ZAutonomy zAutonomy = null;

        public override string ToString()
        {
            return this.name;
        }
    }
}
