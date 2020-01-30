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
    [SpellAttribute(GameSpell.GameSpellID.Minor_Protection_from_Fire, "mprfire", "Minor Protection from Fire", "Slightly less powerful version of Protection from Fire.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single_or_Group, 3, 2, 50, "0231", true, true, false, false, false, Character.ClassType.Ranger, Character.ClassType.Ravager)]
    public class MinorProtectionFromFireSpell : ISpellHandler
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

            int amount = 0;
            int duration = 0;

            int skillLevel = Skills.GetSkillLevel(caster.magic);

            if (skillLevel == 0)
            {
                amount = Convert.ToInt32(caster.Level * 2.5);
                duration = caster.Level * 20;
            }
            else
            {
                amount = Convert.ToInt32(caster.Level * 2.5);
                duration = skillLevel * 20;
            }

            if (amount > 15) { amount = 15; }

            Effect.CreateCharacterEffect(Effect.EffectTypes.Protection_from_Fire, amount, target, duration, caster);

            return true;
        }
    }
}
