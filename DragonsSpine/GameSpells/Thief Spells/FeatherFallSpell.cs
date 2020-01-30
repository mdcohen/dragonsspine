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
    [SpellAttribute(GameSpell.GameSpellID.Feather_Fall, "featherfall", "Feather Fall", "Become nearly weightless and float like a feather.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Single, 12, 9, 1500, "0232", false, true, false, true, false, Character.ClassType.Thief)]
    public class FeatherFallSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);
            
            if (target == null)
                return false;

            ReferenceSpell.SendGenericCastMessage(caster, target, true);
            ReferenceSpell.SendGenericEnchantMessage(caster, target);
            int skillLevel = Skills.GetSkillLevel(caster.magic);
            Effect.CreateCharacterEffect(Effect.EffectTypes.Feather_Fall, 1, target, skillLevel * 25, caster);

            return true;
        }
    }
}
