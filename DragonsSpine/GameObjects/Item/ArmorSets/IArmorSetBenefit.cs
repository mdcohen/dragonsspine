using System;
using System.Collections.Generic;

namespace DragonsSpine
{
    public interface IArmorSetBenefits
    {
        double GetArmorClassModifier();

        List<Effect> GetEffects();

        List<Spells.GameSpell> GetSpells();
    }
}
