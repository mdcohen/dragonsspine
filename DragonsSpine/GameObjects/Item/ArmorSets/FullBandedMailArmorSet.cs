using System;
using System.Collections.Generic;

namespace DragonsSpine
{
    [ArmorSetAttribute(ArmorSet.FULL_BANDED_MAIL, Item.ID_BANDED_MAIL_TUNIC, Item.ID_IRON_GREAVES, Item.ID_CHAINMAIL_COIF, Item.ID_BANDED_GAUNTLETS, Item.ID_BANDED_MAIL_BRACER, 0)]
    public class FullBandedMailArmorSet : IArmorSetBenefits
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
