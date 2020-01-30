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
    [SpellAttribute(GameSpell.GameSpellID.Icestorm, "icestorm", "Icestorm", "Create an icestorm of pounding hailstones at a specified area.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 6, 7, 800, "0070", false, true, false, true, false, Character.ClassType.Wizard)]
    public class IcestormSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            ReferenceSpell.SendGenericCastMessage(caster, null, true);
            ReferenceSpell.CastGenericAreaSpell(caster, args, Effect.EffectTypes.Ice, Skills.GetSkillLevel(caster.magic) * 5, ReferenceSpell.Name);
            return true;
        }
    }
}
