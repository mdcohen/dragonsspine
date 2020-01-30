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
    [SpellAttribute(GameSpell.GameSpellID.Bonfire, "bonfire", "Bonfire", "Create a roaring bonfire.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 4, 1, 50, "0069", false, true, false, false, false, Character.ClassType.Wizard)]
    public class BonfireSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            // clean up the args
            args = args.Replace("bonfire", "");
            args = args.Trim();

            string[] sArgs = args.Split(" ".ToCharArray());

            if (Map.IsSpellPathBlocked(Map.GetCellRelevantToCell(caster.CurrentCell, args, true))) return false;

            Cell targetCell = Map.GetCellRelevantToCell(caster.CurrentCell, args, true);

            if (targetCell != null)
            {
                AreaEffect effect = new AreaEffect(Effect.EffectTypes.Fire, Cell.GRAPHIC_FIRE, (int)(Skills.GetSkillLevel(caster.magic) * 2.5), (int)(Skills.GetSkillLevel(caster.magic) * 1.5), caster, Map.GetCellRelevantToCell(caster.CurrentCell, args, false));
                targetCell.SendShout("a roaring fire.");
                targetCell.EmitSound(ReferenceSpell.SoundFile);
            }

            caster.WriteToDisplay("You create a magical " + ReferenceSpell.Name.ToLower() + ".");

            return true;
        }
    }
}
