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
using DragonsSpine.Autonomy.EntityBuilding;

namespace DragonsSpine.GameWorld
{
    /// <summary>
    /// Stored in a ZPlane object. Used by the EntityCreationManager to spawn creatures and loot based off Z plane specific variables.
    /// </summary>
    public class ZAutonomy
    {
        public ZAutonomy(string zPlaneName, string info)
        {
            this.m_zPlaneName = zPlaneName;
            this.uniqueEntities = new List<ZUniqueEntity>();
            this.artifacts = new List<ZArtifact>();
            this.guardZone = false;
            this.genderExclusive = Globals.eGender.Random;
            this.spawnIntensityMod = 0;
            this.allowChaosPortal = true;

            foreach (XElement element in XDocument.Parse(info).Descendants())
            {
                if (element.Name == "minimumSuggestedLevel")
                    this.minimumSuggestedLevel = Convert.ToInt32(element.Value);
                else if (element.Name == "maximumSuggestedLevel")
                    this.maximumSuggestedLevel = Convert.ToInt32(element.Value);
                else if (element.Name == "entities")
                    this.entities = element.Value;
                else if (element.Name == "uniqueEntity")
                {
                    foreach (XElement ele in element.Elements())
                    {
                        if (ele.Name == "entity")
                        {
                            EntityLists.Entity entity;

                            if (Enum.TryParse(ele.Value.ToString(), true, out entity))
                            {
                                ZUniqueEntity zUnique = new ZUniqueEntity(entity, ele.ElementsAfterSelf());

                                this.uniqueEntities.Add(zUnique);
                            }
                        }
                    }
                }
                else if (element.Name == "artifact")
                {
                    ZArtifact zArtifact = new ZArtifact(element.DescendantNodes());

                    this.artifacts.Add(zArtifact);
                }
                else if (element.Name.ToString().ToLower() == "spawnintensitymod")
                    this.spawnIntensityMod = Convert.ToInt32(element.Value);
                else if (element.Name == "asocial")
                    this.asocial = Convert.ToBoolean(element.Value);
                else if (element.Name == "guardzone")
                    this.guardZone = Convert.ToBoolean(element.Value);
                else if (element.Name.ToString().ToLower() == "gender")
                {
                    Enum.TryParse<Globals.eGender>(element.Name.ToString(), out this.genderExclusive);
                }
                else if (element.Name == "chaosportal")
                    this.allowChaosPortal = Convert.ToBoolean(element.Value);
            }
        }

        private string m_zPlaneName;
        public int maximumSuggestedLevel;
        public int minimumSuggestedLevel;
        public string entities; // used in EntityBuilder
        public List<ZUniqueEntity> uniqueEntities; // used in EntityBuilder
        public List<ZArtifact> artifacts;
        public int spawnIntensityMod; // negative or positive, affects # of crits spawned in auto calculation SpawnZone Constructor (negative numbers reduce critter spawn numbers)
        public bool asocial; // typically social critters will not be social
        public bool guardZone; // spawns will not wander, they are on guard (first used in the Wuil Abyss Watchtower) -- also, they will not flee in fear
        public Globals.eGender genderExclusive; // if spawns have a gender they will be either male, female or it (random isn't an option as this is the default)
        public bool allowChaosPortal;

        public override string ToString()
        {
            return "ZPlaneName: " + this.m_zPlaneName + ", minLevel = " + this.minimumSuggestedLevel + ", maxLevel = " + this.maximumSuggestedLevel + ", entities = " + this.entities;
        }
    }
}
