using System;
using System.Collections.Generic;

namespace DragonsSpine
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
