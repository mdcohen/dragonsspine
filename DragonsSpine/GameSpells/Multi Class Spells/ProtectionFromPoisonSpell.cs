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
    [SpellAttribute(GameSpell.GameSpellID.Protection_from_Poison, "prpoison", "Protection from Poison", "Target receives added protection and saving throw bonus versus poison-based damage.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 21, 14, 12000, "0231", true, false, true, true, false, Character.ClassType.Druid, Character.ClassType.Thaumaturge)]
    public class ProtectionFromPoisonSpell : ISpellHandler
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

            int amount = Skills.GetSkillLevel(caster.magic) * 5;

            if (amount > 25)
                amount = 25;

            Effect.CreateCharacterEffect(Effect.EffectTypes.Protection_from_Poison, amount, target, Skills.GetSkillLevel(caster.magic) * 20, caster);

            return true;
        }
    }
}
