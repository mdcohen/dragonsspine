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

namespace DragonsSpine
{
    public class Point
    {
        public int x;
        public int y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return "{" + this.x.ToString() + "," + this.y.ToString() + "}";
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point)) return false;

            return this == (Point)obj;
        }

        public static bool operator ==(Point lhs, Point rhs)
        {
            if ((object)lhs == null && (object)rhs == null)
                return true;

            if ((object)lhs == null || (object)rhs == null)
                return false;

            if (lhs.x == rhs.x && lhs.y == rhs.y)
                return true;
            else
                return false;
        }

        public static bool operator !=(Point lhs, Point rhs)
        {
            if ((object)lhs == null && (object)rhs == null)
                return false;

            if ((object)lhs == null || (object)rhs == null)
                return true;

            if (lhs.x == rhs.x && lhs.y == rhs.y)
                return false;
            else
                return true;
        }

        public static Point operator -(Point lhs, Point rhs)
        {
            return new Point(lhs.x - rhs.x, lhs.y - rhs.y);
        }

    }
}
