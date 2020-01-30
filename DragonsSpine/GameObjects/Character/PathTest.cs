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
using Cell = DragonsSpine.GameWorld.Cell;

namespace DragonsSpine
{
    public class PathTest : NPC
    {
        // Constants used in path testing for determining the cost of cell movement.
        public const string RESERVED_NAME_THROWNOBJECT = "thrown.object";
        public const string RESERVED_NAME_COMMANDSUFFIX = ".command";
        public const string RESERVED_NAME_JUMPKICKCOMMAND = "jk.command"; // in path testing, can move swiftly over water
        public const string RESERVED_NAME_AREAEFFECT = "areaeffect";

        public PathTest(string name, Cell currentCell) : base()
        {
            this.Name = name;
            this.CurrentCell = currentCell;
            this.IsInvisible = true; // added 11/27/2015 Eb
        }

        public bool SuccessfulPathTest(Cell targetCell)
        {
            if (this.CurrentCell == null || targetCell == null) return false;

            if (this.CurrentCell == targetCell) return true;

            return this.BuildMoveList(targetCell.X, targetCell.Y, targetCell.Z);
        }
        
        public static bool SuccessfulPathTest(string name, Cell originCell, Cell targetCell)
        {
            if (originCell == null || targetCell == null) return false;
            if (originCell == targetCell) return true;

            PathTest pathTestDummy = new PathTest(name, originCell);
            pathTestDummy.AddToWorld();

            bool returnVal = pathTestDummy.BuildMoveList(targetCell.X, targetCell.Y, targetCell.Z);

            pathTestDummy.RemoveFromWorld();

            return returnVal;
        }
    }
}
