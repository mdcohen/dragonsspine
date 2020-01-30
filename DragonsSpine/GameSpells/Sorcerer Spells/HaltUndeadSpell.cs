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
    [SpellAttribute(GameSpell.GameSpellID.Halt_Undead, "haltundead", "Halt Undead", "Halt an undead in its tracks.",
        Globals.eSpellType.Enchantment, Globals.eSpellTargetType.Single, 6, 6, 2100, "0229", false, true, true, false, false, Character.ClassType.Sorcerer)]
    public class HaltUndeadSpell : ISpellHandler
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

            if (!target.IsUndead)
            {
                caster.WriteToDisplay("Your " + ReferenceSpell.Name + " has no affect on the living.");
                return true;
            }

            if (Combat.DoSpellDamage(caster, target, null, 0, "stun") == 1)
            {
                target.Stunned += Convert.ToInt16(Skills.GetSkillLevel(caster.magic) * 4);
                caster.WriteToDisplay(target.GetNameForActionResult(false) + " is paralyzed.");
            }

            return true;
        }
    }
}
