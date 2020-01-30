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
    [SpellAttribute(GameSpell.GameSpellID.Strength, "strength", "Strength", "Target becomes physically stronger.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Single, 3, 1, 0, "0232", true, true, false, true, false,
        Character.ClassType.Druid, Character.ClassType.Knight, Character.ClassType.Ranger, Character.ClassType.Ravager, Character.ClassType.Thaumaturge)]
    public class StrengthSpell : ISpellHandler
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

            int effectAmount = 6;

            if (caster.Level >= 15)// || (caster.BaseProfession == Character.ClassType.Thaumaturge && skillLevel >= 15))
                effectAmount = 12;

            int skillLevel = Skills.GetSkillLevel(caster.magic);

            if (skillLevel >= 1)
            {
                Effect.CreateCharacterEffect(Effect.EffectTypes.Temporary_Strength, effectAmount, target, Utils.TimeSpanToRounds(new TimeSpan(0, 20, 0)) + Utils.TimeSpanToRounds(new TimeSpan(0, 0, skillLevel * 30)), caster);
            }
            else
                Effect.CreateCharacterEffect(Effect.EffectTypes.Temporary_Strength, effectAmount, target, Utils.TimeSpanToRounds(new TimeSpan(0, 20, 0)), caster);

            return true;
        }
    }
}
