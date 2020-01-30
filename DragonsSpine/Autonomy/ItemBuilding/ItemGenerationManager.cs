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
using System.Linq;
using System.Text;

namespace DragonsSpine.Autonomy.ItemBuilding
{
    public static class ItemGenerationManager
    {
        public static string ITEM_INSERT_NOTES_DEFAULT = "Default Item";

        /** Default variables for items. 10/11/2015 -Eb
            [notes]
           ,[combatAdds]
           ,[itemID]
           ,[itemType]
           ,[baseType]
           ,[name]
           ,[visualKey]
           ,[unidentifiedName]
           ,[identifiedName]
           ,[shortDesc]
           ,[longDesc]
           ,[wearLocation]
           ,[weight]
           ,[coinValue]
           ,[size]
           ,[effectType]
           ,[effectAmount]
           ,[effectDuration]
           ,[special]
           ,[minDamage]
           ,[maxDamage]
           ,[skillType]
           ,[vRandLow]
           ,[vRandHigh]
           ,[key]
           ,[recall]
           ,[alignment]
           ,[spell]
           ,[spellPower]
           ,[charges]
           ,[attackType]
           ,[blueglow]
           ,[flammable]
           ,[fragile]
           ,[lightning]
           ,[returning]
           ,[silver]
           ,[attuneType]
           ,[figExp]
           ,[armorClass]
           ,[armorType]
           ,[bookType]
           ,[maxPages]
           ,[pages]
           ,[drinkDesc]
           ,[fluidDesc]
           ,[lootTable]
         **/

        public static int GetNextAvailableItemID()
        {
            List<int> idNumbers = DAL.DBItem.GetItemIDList();
            idNumbers.Sort();

            int[] values = Enumerable.Range(idNumbers[0], idNumbers[idNumbers.Count - 1] - idNumbers[0]).ToArray();

            foreach (int num in values)
                if (!idNumbers.Contains(num)) return num;
            return -1;
        }
    }
}
