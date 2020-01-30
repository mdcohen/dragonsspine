using System;
using System.Collections.Generic;

namespace DragonsSpine
{
    [ArmorSetAttribute(ArmorSet.FULL_SCALEMAIL, Item.ID_SCALEMAIL_TUNIC, Item.ID_SCALEMAIL_LEGGINGS, 0, 0, Item.ID_SCALEMAIL_BRACER, 0)]
    public class FullScalemailArmorSet : IArmorSetBenefits
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
