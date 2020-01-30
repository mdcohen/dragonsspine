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
    [SpellAttribute(GameSpell.GameSpellID.Turn_Undead, "turnundead", "Turn Undead", "Halt, drive off or destroy undead targets in sight.",
        Globals.eSpellType.Necromancy, Globals.eSpellTargetType.Point_Blank_Area_Effect, 5, 6, 700, "0229", false, true, false, false, false, Character.ClassType.Thaumaturge)]
    public class TurnUndeadSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            ReferenceSpell.SendGenericCastMessage(caster, null, true);
            ReferenceSpell.CastGenericAreaSpell(caster, args, Effect.EffectTypes.Turn_Undead, Skills.GetSkillLevel(caster.magic) * 10, ReferenceSpell.Name);
            return true;
        }
    }
}
