﻿#region 
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
    [SpellAttribute(GameSpell.GameSpellID.Darkness, "darkness", "Darkness", "Shroud an area in darkness.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 5, 1, 400, "", false, true, false, false, false, Character.ClassType.Thief, Character.ClassType.Wizard)]
    public class DarknessSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            ReferenceSpell.SendGenericCastMessage(caster, null, true);
            ReferenceSpell.CastGenericAreaSpell(caster, args, Effect.EffectTypes.Darkness, Skills.GetSkillLevel(caster.magic) * 2, ReferenceSpell.Name);
            return true;
        }
    }
}
