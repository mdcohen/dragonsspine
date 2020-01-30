using System;
using System.Collections.Generic;

namespace DragonsSpine
{
    [ArmorSetAttribute(ArmorSet.BASIC_CHAINMAIL, Item.ID_CHAINMAIL_TUNIC, Item.ID_CHAINMAIL_LEGGINGS)]
    public class BasicChainmailArmorSet : IArmorSetBenefits
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
