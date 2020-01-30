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

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Acid_Rain, "acidrain", "Acid Rain", "Creates a torrential downpour of acid rain that destroys items and terrain as well as damaging unprotected creatures.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 15, 9, 22000, "0228", false, true, false, false, false, Character.ClassType.Sorcerer)]
    public class AcidRainSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            int multiplier = 4;

            Item totem = caster.FindHeldItem(Item.ID_BLOODWOOD_TOTEM);

            if (totem != null && !totem.IsAttunedToOther(caster))
                multiplier += (Rules.RollD(1, 3) + 1);

            ReferenceSpell.SendGenericCastMessage(caster, null, true);
            ReferenceSpell.CastGenericAreaSpell(caster, args, Effect.EffectTypes.Acid, (Skills.GetSkillLevel(caster.magic) * multiplier) + GameSpell.GetSpellDamageModifier(caster), ReferenceSpell.Name);
            return true;
        }
    }
}
