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
    [SpellAttribute(GameSpell.GameSpellID.Lightning_Lance, "lightninglance", "Lightning Lance", "Shoot a crackling lance of electrical energy at a target.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Single, 20, 17, 20000, "0066", false, true, false, false, false, Character.ClassType.Wizard)]
    public class LightningLanceSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            //Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);

            //if (target == null) { return false; }

            ReferenceSpell.SendGenericCastMessage(caster, null, true);
            int damage = 0;
            if (Skills.GetSkillLevel(caster.magic) >= 4)
            {
                damage = Skills.GetSkillLevel(caster.magic) * 16 + GameSpell.GetSpellDamageModifier(caster);
            }
            else
            {
                damage = 6 * 16 + GameSpell.GetSpellDamageModifier(caster);
            }
            ReferenceSpell.CastPathSpell(caster, args, Effect.EffectTypes.Lightning, damage, "lightning", "a bolt", ReferenceSpell.SoundFile);
            return true;
        }
    }
}
