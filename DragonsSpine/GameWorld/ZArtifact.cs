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
using System.Xml.Linq;

namespace DragonsSpine.GameWorld
{
    /// <summary>
    /// Contains information for spawning artifacts. Currently artifacts only spawn upon server start.
    /// </summary>
    public class ZArtifact
    {
        public ZArtifact(IEnumerable<XNode> nodes)
        {
            foreach (XNode node in nodes)
            {
                if (node is XElement)
                {
                    if ((node as XElement).Name == "itemID")
                        itemID = Convert.ToInt32((node as XElement).Value.ToString());
                    else if ((node as XElement).Name == "x")
                        this.xCord = Convert.ToInt32((node as XElement).Value.ToString());
                    else if ((node as XElement).Name == "y")
                        this.yCord = Convert.ToInt32((node as XElement).Value.ToString());
                }
            }
        }

        public int itemID;
        public int xCord;
        public int yCord;
    }
}
