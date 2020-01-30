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
using DragonsSpine.GameWorld;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Firestorm, "firestorm", "Firestorm", "Create a fierce, unpredictable storm of fire in a designated area.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 30, 15, 100000, "0224", false, true, false, false, false, Character.ClassType.Wizard)]
    public class FirestormSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            ReferenceSpell.SendGenericCastMessage(caster, null, true);

            if (args == null) args = "";

            Cell targetCell = Map.GetCellRelevantToCell(caster.CurrentCell, args, true);

            if (targetCell == null) return false;

            int skillLevel = Skills.GetSkillLevel(caster, Globals.eSkillType.Magic);

            AreaEffect effect = new AreaEffect(Effect.EffectTypes.Fire_Storm, Cell.GRAPHIC_FIRE, skillLevel * (ReferenceSpell.RequiredLevel / 2), Rules.RollD(2, 4), caster, Map.GetCellRelevantToCell(caster.CurrentCell, args, true));

            return true;
        }
    }
}
