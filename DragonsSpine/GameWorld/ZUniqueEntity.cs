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
using System.Xml.Linq;
using System.Collections.Generic;
using DragonsSpine.Autonomy.EntityBuilding;

namespace DragonsSpine.GameWorld
{
    public class ZUniqueEntity
    {
        public ZUniqueEntity(EntityLists.Entity uniqueEntity, IEnumerable<XElement> elements)
        {
            entity = uniqueEntity;

            foreach (XElement elem in elements)
            {
                if (elem.Name == "x")
                    this.xCord = Convert.ToInt32(elem.Value.ToString());
                else if (elem.Name == "y")
                    this.yCord = Convert.ToInt32(elem.Value.ToString());
                else if (elem.Name == "spawnTimer")
                    this.spawnTimer = Convert.ToInt32(elem.Value);
                else if (elem.Name == "spawnMessage")
                    this.spawnMessage = elem.Value.ToString();
                else if (elem.Name == "description")
                    this.description = elem.Value.ToString();
                else if (elem.Name == "profession")
                    this.profession = elem.Value.ToString();
                else if (elem.Name == "spawnRadius")
                    this.spawnRadius = Convert.ToInt32(elem.Value.ToString());
                else if (elem.Name == "lairCells")
                {
                    this.hasLair = true;
                    this.lairCells = elem.Value.ToString();
                }
                else if (elem.Name == "zRange")
                {
                    this.zRange = elem.Value.ToString();
                }
            }

            if (description == null) description = "";
            if (profession == null || profession == "") profession = Character.ClassType.Fighter.ToString();
            if (spawnMessage == null) spawnMessage = "";
            if (spawnTimer <= 0) spawnTimer = 120;
        }

        public EntityLists.Entity entity;
        public string description;
        public string profession;
        public int xCord;
        public int yCord;
        public string spawnMessage;
        public int spawnTimer;
        public int spawnRadius;
        public bool hasLair;
        public string lairCells;
        public string zRange;
    }
}
