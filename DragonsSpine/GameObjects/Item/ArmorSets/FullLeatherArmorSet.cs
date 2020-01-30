using System;
using System.Collections.Generic;

namespace DragonsSpine
{
    [ArmorSetAttribute(ArmorSet.FULL_LEATHER, Item.ID_LEATHER_TUNIC, Item.ID_LEATHER_LEGGINGS, Item.ID_LEATHER_SKULLCAP, Item.ID_LEATHER_GAUNTLETS, Item.ID_LEATHER_BRACER, 0)]
    public class FullLeatherArmorSet : IArmorSetBenefits
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
