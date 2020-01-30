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
    [SpellAttribute(GameSpell.GameSpellID.Firebolt, "firebolt", "Firebolt", "Caster sends a bolt of fire in a direction. The fire bolt engulfs all targets in its path if they fail their saving throw.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Single, 10, 12, 5000, "0224", false, true, false, false, false, Character.ClassType.Wizard)]
    public class FireboltSpell : ISpellHandler
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

            target.WriteToDisplay("You have been hit by a " + ReferenceSpell.Name + "!");

            if (Combat.DoSpellDamage(caster, target, null, (Skills.GetSkillLevel(caster.magic) * 12) + GameSpell.GetSpellDamageModifier(caster), ReferenceSpell.Name) == 1)
            {
                Rules.GiveKillExp(caster, target);
                Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
            }
            return true;
        }
    }
}
