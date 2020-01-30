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
using EntityLists = DragonsSpine.Autonomy.EntityBuilding.EntityLists;
using DataRow = System.Data.DataRow;

namespace DragonsSpine.Autonomy.ItemBuilding
{
    /// <summary>
    /// lootXXXXAmount
    ///    The amount of loot the NPC gets from the lootXXXXArray.
    /// 
    /// lootXXXXArray
    ///    The array of item IDs available as loot for the NPC.
    /// 
    /// lootXXXXOdds
    ///    -1 = NPC gets all lootXXXXArray items put into sack, otherwise npc gets that amount.
    ///    100+ = NPC automatically gets one piece of loot from lootXXXXArray, otherwise if Rules.dice.Next(100) is
    ///    lower than lootXXXXOdds npc gets one piece of loot from lootXXXXArray.
    /// </summary>
    public class LootTable
    {
        #region Public Data
        public int npcID;
        public bool isAutonomous;

        public int lootVeryCommonAmount = 0;
        public List<int> lootVeryCommonList = new List<int>();
        
        public int lootCommonAmount = 0;
        public List<int> lootCommonList = new List<int>();

        public int lootUncommonAmount = 0;
        public List<int> lootUncommonList = new List<int>();
        
        public int lootRareAmount = 0;
        public List<int> lootRareList = new List<int>();
        
        public int lootVeryRareAmount = 0;
        public List<int> lootVeryRareList = new List<int>();

        public int lootUltraRareAmount = 0;
        public List<int> lootUltraRareList = new List<int>();

        public int lootLairAmount = 0; // only the most difficult lair NPCs should have an amount greater than 1 (possible duplicate items)
        public List<int> lootLairList = new List<int>();

        public List<int> lootAlwaysList = new List<int>(); // lootAlways is an array of loot the npc will always have eg: quest items such as the Knight quest agate
        
        public int lootBeltAmount = 0; // In loot dispersement code only one belt item is ever added. This can probably be removed. 2/3/2017 Eb
        public List<int> lootBeltList = new List<int>();

        public List<int> lootArtifactsList = new List<int>(); // These are dished out separately to NPC containers and lair loot.

        public List<int> spawnArmorList = new List<int>();
        public List<int> spawnLeftHandList = new List<int>();
        public List<int> spawnRightHandList = new List<int>();

        public List<int> lootUniqueList = new List<int>(); // unique loot that has a chance to drop on some unique entities

        //public bool hasAlwaysRightHand = false; // an item always held in the right hand has been added to spawnRightHandList
        //public bool hasAlwaysLeftHand = false; // an item always held in the left hand has been added to spawnLeftHandList
        public List<Globals.eWearLocation> alwaysWorn = new List<Globals.eWearLocation>();
        #endregion

        public LootTable(int npcID)
        {
            this.npcID = npcID;
        }

        public LootTable(LootTable table)
        {
            isAutonomous = table.isAutonomous;
            lootVeryCommonList = new List<int>(table.lootVeryCommonList);
            lootCommonList = new List<int>(table.lootCommonList);
            lootUncommonList = new List<int>(table.lootUncommonList);
            lootAlwaysList = new List<int>(table.lootAlwaysList);
            lootBeltList = new List<int>(table.lootBeltList);
            lootLairList = new List<int>(table.lootLairList);
            lootRareList = new List<int>(table.lootRareList);
            lootVeryRareList = new List<int>(table.lootVeryRareList);
            spawnRightHandList = new List<int>(table.spawnRightHandList);
            spawnLeftHandList = new List<int>(table.spawnLeftHandList);
            spawnArmorList = new List<int>(table.spawnArmorList);

            lootVeryCommonAmount = table.lootVeryCommonAmount;
            lootCommonAmount = table.lootCommonAmount;
            lootUncommonAmount = table.lootUncommonAmount;
            lootRareAmount = table.lootRareAmount;
            lootVeryRareAmount = table.lootVeryRareAmount;
            lootUltraRareAmount = table.lootUltraRareAmount;
            lootLairAmount = table.lootLairAmount;
            lootBeltAmount = table.lootBeltAmount;

            lootUniqueList = table.lootUniqueList;

            lootArtifactsList = table.lootArtifactsList;
            alwaysWorn = new List<Globals.eWearLocation>();
        }

        public override string ToString()
        {
            System.Reflection.FieldInfo[] fields = this.GetType().GetFields();

            string returnString = "";

            foreach (System.Reflection.FieldInfo f in fields)
            {
                if (f.GetType() == typeof(Int32))
                    returnString += f.Name + ": " + f.GetValue(this) + ", ";
                else
                {
                    try
                    {
                        List<int> list = (List<int>)f.GetValue(this);

                        returnString += f.Name + ": ";

                        foreach (int val in list)
                        {
                            returnString += "[" + val.ToString() + "] " + Item.ItemDictionary[val]["identifiedName"].ToString() + ", ";
                        }

                        returnString = returnString.Substring(0, returnString.Length - 2);

                        returnString += "/r/n";
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return returnString.Substring(0, returnString.Length - 2);
        }
    }
}
