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
    [SpellAttribute(GameSpell.GameSpellID.Death, "death", "Death", "Punish a target with directed damage.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Single, 6, 7, 850, "0223", false, true, false, false, false, Character.ClassType.Thaumaturge)]
    public class DeathSpell : ISpellHandler
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

            int totalDamage = (Skills.GetSkillLevel(caster.magic) * (caster.IsPC ? GameSpell.DEATH_SPELL_MULTIPLICAND_PC : GameSpell.DEATH_SPELL_MULTIPLICAND_NPC)) + GameSpell.GetSpellDamageModifier(caster);

            totalDamage += Rules.RollD(1, 2) == 1 ? Rules.RollD(1, 4) : -(Rules.RollD(1, 4));

            if (Combat.DoSpellDamage(caster, target, null, totalDamage, ReferenceSpell.Name.ToLower()) == 1)
            {
                Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
                Rules.GiveKillExp(caster, target);
            }

            return true;
        }
    }
}
