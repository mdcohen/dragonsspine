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
    [SpellAttribute(GameSpell.GameSpellID.Protection_from_Fire_and_Ice, "prfireice", "Protection from Fire and Ice", "Target receives added protection and saving throw bonuses versus cold, ice and fire damage.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 21, 12, 2000, "0231", true, true, false, true, false, Character.ClassType.Druid, Character.ClassType.Thaumaturge, Character.ClassType.Wizard)]
    public class ProtectionFromFireAndIceSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, sArgs[sArgs.Length - 1]);

            if (target == null) { return false; }

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            ReferenceSpell.SendGenericEnchantMessage(caster, target);

            int amount = 1 + (Skills.GetSkillLevel(caster.magic) / 10);

            Effect.CreateCharacterEffect(Effect.EffectTypes.Protection_from_Fire_and_Ice, amount, target, Skills.GetSkillLevel(caster.magic) * 30, caster);
            return true;
        }
    }
}
