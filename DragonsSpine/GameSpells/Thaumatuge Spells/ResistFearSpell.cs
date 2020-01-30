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
    [SpellAttribute(GameSpell.GameSpellID.Resist_Fear, "resfear", "Resist Fear", "Target receives saving throw bonus versus fear magic.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 9, 9, 1000, "0231", true, true, false, true, false, Character.ClassType.Thaumaturge)]
    public class ResistFearSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);

            if (target == null) { return false; }

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            ReferenceSpell.SendGenericEnchantMessage(caster, target);

            Effect.CreateCharacterEffect(Effect.EffectTypes.Resist_Fear, 1 + (Skills.GetSkillLevel(caster.magic) / 10), target, Skills.GetSkillLevel(caster.magic) * 20, caster);

            return true;
        }
    }
}
