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
    public class Segue
    {
        #region Private Data
        private readonly int m_landID;
        private readonly int m_mapID;
        private readonly int m_xCord;
        private readonly int m_yCord;
        private readonly int m_zCord;
        private readonly int m_height;
        #endregion

        #region Public Properties
        public int LandID
        {
            get { return this.m_landID; }
        }
        public int MapID
        {
            get { return this.m_mapID; }
        }
        public int X
        {
            get { return this.m_xCord; }
        }
        public int Y
        {
            get { return this.m_yCord; }
        }
        public int Z
        {
            get { return this.m_zCord; }
        }
        public int Height
        {
            get { return this.m_height; }
        }
        #endregion

        #region Constructor
        public Segue(int landID, int mapID, int xCord, int yCord, int zCord, int height)
        {
            this.m_landID = landID;
            this.m_mapID = mapID;
            this.m_xCord = xCord;
            this.m_yCord = yCord;
            this.m_zCord = zCord;
            this.m_height = height;
        } 
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Search for a segueway that leads UP when a game object is moving DOWN.
        /// TODO: This code can be simplified by accessing adjacent cell function. 11/19/2015 Eb
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static Segue GetUpSegue(Cell cell)
        {
            try
            {
                //int[] zPlanes = new int[cell.Map.ZPlanes.Count];
                var zPlanes = new List<int>(cell.Map.ZPlanes.Keys);

                //cell.Map.ZPlanes.Keys.CopyTo(zPlanes, 0);
                zPlanes.Sort();

                //Array.Sort(zPlanes); // sorts in ascending order
                int a = 0;
                Cell targetCell = null;

                do
                {
                    if (zPlanes[a] > cell.Z)
                    {
                        targetCell = Cell.GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X, cell.Y, zPlanes[a]);

                        if (targetCell != null)
                        {
                            if (targetCell.IsUpSegue) // found up segue directly above
                                return new Segue(targetCell.LandID, targetCell.MapID, targetCell.X, targetCell.Y, targetCell.Z, Math.Abs(targetCell.Z - cell.Z));

                            // search for a suitable segue
                            for (int xpos = 0; xpos <= 1; xpos++)
                            {
                                for (int ypos = 0; ypos <= 1; ypos++)
                                {
                                    Cell searchCell = Cell.GetCell(targetCell.FacetID, targetCell.LandID, targetCell.MapID, targetCell.X + xpos, targetCell.Y + ypos, targetCell.Z);
                                    if (searchCell != null && searchCell.IsUpSegue)
                                        return new Segue(searchCell.LandID, searchCell.MapID, searchCell.X, searchCell.Y, searchCell.Z, Math.Abs(searchCell.Z - cell.Z));

                                    searchCell = Cell.GetCell(targetCell.FacetID, targetCell.LandID, targetCell.MapID, targetCell.X - xpos, targetCell.Y - ypos, targetCell.Z);
                                    
                                    if (searchCell != null && searchCell.IsUpSegue)
                                        return new Segue(searchCell.LandID, searchCell.MapID, searchCell.X, searchCell.Y, searchCell.Z, Math.Abs(searchCell.Z - cell.Z));
                                }
                            }
                        }
                    }

                    a++;
                }
                while (a < zPlanes.Count);

                if (targetCell != null) // this is the fail safe
                    return new Segue(targetCell.LandID, targetCell.MapID, targetCell.X, targetCell.Y, targetCell.Z, Math.Abs(targetCell.Z - cell.Z));

                Utils.Log("Failed to find a suitable Segue at Segue.GetUpSegue(Cell: " + cell.LandID + " " + cell.MapID + " " + cell.X + " " + cell.Y + " " + cell.Z + ")", Utils.LogType.SystemFailure);
                Utils.Log("Returned new Segue (KarmaRes)", Utils.LogType.SystemFailure);

                return new Segue(cell.LandID, cell.MapID, cell.Map.KarmaResX, cell.Map.KarmaResY, cell.Map.KarmaResZ, 0);
                //return null;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return new Segue(cell.LandID, cell.MapID, cell.Map.KarmaResX, cell.Map.KarmaResY, cell.Map.KarmaResZ, 0);
                //return null;
            }
        }

        /// <summary>
        /// Search for a segueway that leads DOWN when a game object is moving UP.
        /// TODO: This code can be simplified by accessing adjacent cell function. 11/19/2015 Eb
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static Segue GetDownSegue(Cell cell)
        {
            try
            {
               //int[] zPlanes = new int[cell.Map.ZPlanes.Count];

                var zPlanes = new List<int>(cell.Map.ZPlanes.Keys);

                //cell.Map.ZPlanes.Keys.CopyTo(zPlanes, 0);
                //Array.Sort(zPlanes); // sorts in ascending order
                //Array.Reverse(zPlanes); // reverse the array

                zPlanes.Sort();
                zPlanes.Reverse(); // start looking from lower zPlanes

                int a = 0;
                Cell targetCell = null;
                do
                {
                    if (zPlanes[a] < cell.Z)
                    {
                        targetCell = Cell.GetCell(cell.FacetID, cell.LandID, cell.MapID, cell.X, cell.Y, zPlanes[a]);

                        if (targetCell != null)
                        {
                            // down segue cell is directly above
                            if (targetCell.IsDownSegue && targetCell.DisplayGraphic != Cell.GRAPHIC_WALL && targetCell.DisplayGraphic != Cell.GRAPHIC_MOUNTAIN)
                                return new Segue(targetCell.LandID, targetCell.MapID, targetCell.X, targetCell.Y, targetCell.Z, cell.Z - targetCell.Z);

                            if (cell.DisplayGraphic == Cell.GRAPHIC_AIR)
                            {
                                #region Falling through the air.
                                if ((targetCell.DisplayGraphic != Cell.GRAPHIC_WALL && targetCell.DisplayGraphic != Cell.GRAPHIC_MOUNTAIN) || targetCell.IsDownSegue)
                                    return new Segue(targetCell.LandID, targetCell.MapID, targetCell.X, targetCell.Y, targetCell.Z, cell.Z - targetCell.Z);

                                // search for a suitable segue for air drop if straight down didnt work
                                for (int xpos = -1; xpos <= 1; xpos++)
                                {
                                    for (int ypos = -1; ypos <= 1; ypos++)
                                    {
                                        Cell searchCell = Cell.GetCell(targetCell.FacetID, targetCell.LandID, targetCell.MapID, targetCell.X + xpos, targetCell.Y + ypos, targetCell.Z);

                                        if (searchCell != null && searchCell.DisplayGraphic != Cell.GRAPHIC_WALL && searchCell.DisplayGraphic != Cell.GRAPHIC_MOUNTAIN)
                                            return new Segue(searchCell.LandID, searchCell.MapID, searchCell.X, searchCell.Y, searchCell.Z, cell.Z - searchCell.Z);

                                        searchCell = Cell.GetCell(targetCell.FacetID, targetCell.LandID, targetCell.MapID, targetCell.X - xpos, targetCell.Y - ypos, targetCell.Z);

                                        if (searchCell != null && searchCell.DisplayGraphic != Cell.GRAPHIC_WALL && searchCell.DisplayGraphic != Cell.GRAPHIC_MOUNTAIN)
                                            return new Segue(searchCell.LandID, searchCell.MapID, searchCell.X, searchCell.Y, searchCell.Z, cell.Z - searchCell.Z);
                                    }
                                }
                                Utils.Log("Failed to find a suitable Segue for air cell at Segue.GetDownSegue(Cell: " + cell.LandID + " " + cell.MapID + " " + cell.X + " " + cell.Y + " " + cell.Z + ")", Utils.LogType.SystemWarning);
                                Utils.Log("Returned new Segue (KarmaRes)", Utils.LogType.SystemWarning);
                                return new Segue(cell.LandID, cell.MapID, cell.Map.KarmaResX, cell.Map.KarmaResY, cell.Map.KarmaResZ, 0); 
                                #endregion
                            }

                            //foreach (Cell appCell in Map.GetAdjacentCells(targetCell))
                            //{
                            //    if (appCell.IsUpSegue)
                            //        return new Segue(appCell.LandID, appCell.MapID, appCell.X, appCell.Y, appCell.Z, appCell.Z - cell.Z);
                            //}

                            // search for a suitable segue -- old code that worked, not going to modify it for now 11/20/2015 Eb
                            for (int xpos = -1; xpos <= 1; xpos++)
                            {
                                for (int ypos = -1; ypos <= 1; ypos++)
                                {
                                    Cell searchCell = Cell.GetCell(targetCell.FacetID, targetCell.LandID, targetCell.MapID, targetCell.X + xpos, targetCell.Y + ypos, targetCell.Z);

                                    if (searchCell != null && searchCell.IsDownSegue && searchCell.DisplayGraphic != Cell.GRAPHIC_WALL && searchCell.DisplayGraphic != Cell.GRAPHIC_MOUNTAIN)
                                        return new Segue(searchCell.LandID, searchCell.MapID, searchCell.X, searchCell.Y, searchCell.Z, cell.Z - searchCell.Z);
                                    
                                    searchCell = Cell.GetCell(targetCell.FacetID, targetCell.LandID, targetCell.MapID, targetCell.X - xpos, targetCell.Y - ypos, targetCell.Z);

                                    if (searchCell != null && searchCell.IsDownSegue && searchCell.DisplayGraphic != Cell.GRAPHIC_WALL && searchCell.DisplayGraphic != Cell.GRAPHIC_MOUNTAIN)
                                        return new Segue(searchCell.LandID, searchCell.MapID, searchCell.X, searchCell.Y, searchCell.Z, cell.Z - searchCell.Z);
                                }
                            }
                        }
                    }
                    a++;
                }
                while (a < zPlanes.Count);

                if (targetCell != null && targetCell.DisplayGraphic != Cell.GRAPHIC_WALL && targetCell.DisplayGraphic != Cell.GRAPHIC_MOUNTAIN)
                    return new Segue(targetCell.LandID, targetCell.MapID, targetCell.X, targetCell.Y, targetCell.Z, targetCell.Z - cell.Z);

                Utils.Log("Failed to find a suitable Segue at Segue.GetDownSegue(Cell: " + cell.LandID + " " + cell.MapID + " " + cell.X + " " + cell.Y + " " + cell.Z + ")", Utils.LogType.SystemFailure);
                Utils.Log("Returned new Segue (KarmaRes)", Utils.LogType.SystemFailure);

                return new Segue(cell.LandID, cell.MapID, cell.Map.KarmaResX, cell.Map.KarmaResY, cell.Map.KarmaResZ, 0);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return new Segue(cell.LandID, cell.MapID, cell.Map.KarmaResX, cell.Map.KarmaResY, cell.Map.KarmaResZ, 0);
                //return null;
            }
        }
        #endregion
    }
}

