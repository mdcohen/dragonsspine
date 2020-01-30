using System;
using System.Collections.Generic;

namespace DragonsSpine
{
    [ArmorSetAttribute(ArmorSet.FULL_STEEL, Item.ID_STEEL_BREASTPLATE, Item.ID_STEEL_GREAVES, Item.ID_STEEL_BASINET_WITH_VISOR, Item.ID_STEEL_GAUNTLETS, Item.ID_STEEL_BRACER, 0)]
    public class FullSteelArmorSet : IArmorSetBenefits
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
