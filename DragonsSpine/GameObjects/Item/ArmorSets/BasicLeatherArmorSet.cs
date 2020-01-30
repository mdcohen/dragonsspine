using System;
using System.Collections.Generic;

namespace DragonsSpine
{
    [ArmorSetAttribute(ArmorSet.BASIC_LEATHER, Item.ID_LEATHER_TUNIC, Item.ID_LEATHER_LEGGINGS)]
    public class BasicLeatherArmorSet : IArmorSetBenefits
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
