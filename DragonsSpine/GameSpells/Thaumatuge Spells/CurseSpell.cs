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
    [SpellAttribute(GameSpell.GameSpellID.Curse, "curse", "Curse", "Curses a target causing immediate, unavoidable physical damage.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Single, 3, 1, 25, "0225", false, true, false, false, false, Character.ClassType.Thaumaturge)]
    public class CurseSpell : ISpellHandler
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

            int dmgMultiplier = GameSpell.CURSE_SPELL_MULTIPLICAND_NPC;
            if (caster.IsPC) dmgMultiplier = GameSpell.CURSE_SPELL_MULTIPLICAND_PC; // allow players to do slightly more damage than critters at same skill level

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            if (Combat.DoSpellDamage(caster, target, null, Skills.GetSkillLevel(caster.magic) * dmgMultiplier + GameSpell.GetSpellDamageModifier(caster), "curse") == 1)
            {
                Rules.GiveKillExp(caster, target);
                Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
            }

            return true;
        }
    }
}
