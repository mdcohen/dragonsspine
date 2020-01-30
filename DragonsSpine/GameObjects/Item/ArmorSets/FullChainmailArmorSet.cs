using System;
using System.Collections.Generic;

namespace DragonsSpine
{
    [ArmorSetAttribute(ArmorSet.FULL_CHAINMAIL, Item.ID_CHAINMAIL_TUNIC, Item.ID_CHAINMAIL_LEGGINGS, Item.ID_CHAINMAIL_COIF, Item.ID_CHAINMAIL_GAUNTLETS, Item.ID_CHAINMAIL_BRACER, 0)]
    public class FullChainmailArmorSet : IArmorSetBenefits
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
