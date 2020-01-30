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
    [SpellAttribute(GameSpell.GameSpellID.Create_Lava, "createlava", "Create Lava", "Create a bubbling stream of lava that flows randomly.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 33, 19, 30000, "0224", false, false, true, false, true, Character.ClassType.Wizard)]
    public class CreateLavaSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            ReferenceSpell.SendGenericCastMessage(caster, null, true);
            
            // clean up the args
            args = args.Replace(ReferenceSpell.Command, "");
            args = args.Replace("lava", "");
            args = args.Trim();

            string[] sArgs = args.Split(" ".ToCharArray());

            // Path is blocked. Spell failure.
            if (Map.IsSpellPathBlocked(Map.GetCellRelevantToCell(caster.CurrentCell, args, true)))
                return false;

            if (Map.GetCellRelevantToCell(caster.CurrentCell, args, true) != null)
            {
                AreaEffect effect = new AreaEffect(Effect.EffectTypes.Lava, Cell.GRAPHIC_FIRE, Skills.GetSkillLevel(caster.magic) * 20, Skills.GetSkillLevel(caster.magic) * 2, caster, Map.GetCellRelevantToCell(caster.CurrentCell, args, false));
                Map.GetCellRelevantToCell(caster.CurrentCell, args, true).SendShout("a bubbling hiss.");
            }

            caster.WriteToDisplay("A geyser of molton hot lava blasts through the surface and begins to spread rapidly.");
            return true;
        }
    }
}
