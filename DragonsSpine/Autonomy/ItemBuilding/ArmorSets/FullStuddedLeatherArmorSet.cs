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

namespace DragonsSpine.Autonomy.ItemBuilding.ArmorSets
{
    [ArmorSetAttribute("Full Studded Leather", Item.ID_STUDDED_LEATHER_TUNIC, Item.ID_STUDDED_LEATHER_LEGGINGS, Item.ID_LEATHER_SKULLCAP, Item.ID_STUDDED_LEATHER_GAUNTLETS, Item.ID_STUDDED_LEATHER_BRACER, Item.ID_STUDDED_LEATHER_ARMBAND )]
    public class FullStuddedLeatherArmorSet : IArmorSetBenefits
    {
        public double GetArmorClassModifier()
        {
            return 0;
        }

        public List<Effect> GetEffects()
        {
            return new List<Effect>();
        }

        public List<Spells.GameSpell> GetSpells()
        {
            return new List<Spells.GameSpell>();
        }
    }
}
