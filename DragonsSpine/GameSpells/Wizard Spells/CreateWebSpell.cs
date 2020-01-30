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
using Cell = DragonsSpine.GameWorld.Cell;
using Map = DragonsSpine.GameWorld.Map;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Create_Web, "web", "Create Web", "Create a sticky web to trap intruders.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 4, 5, 100, "0230", false, true, false, true, false, Character.ClassType.Wizard)]
    public class CreateWebSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            args = args.Replace(" at ", "");

            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);

            int duration = 3; // 3 rounds, base

            if (caster.species == Globals.eSpecies.Arachnid || Autonomy.EntityBuilding.EntityLists.ARACHNID.Contains(caster.entity))
                duration += caster.Level / 2;
            else if (caster.IsSpellWarmingProfession && caster.preppedSpell == GameSpell.GetSpell((int)GameSpell.GameSpellID.Create_Web))
            {
                duration += Skills.GetSkillLevel(caster.magic) / 3;
            }

            duration += Rules.Dice.Next(-1, 1);

            if (target == null)
            {
                Cell targetCell = Map.GetCellRelevantToCell(caster.CurrentCell, args, false);

                if (targetCell != null)
                {
                    AreaEffect effect = new AreaEffect(Effect.EffectTypes.Web, Cell.GRAPHIC_WEB, Skills.GetSkillLevel(caster.magic), duration, caster, targetCell);
                    targetCell.EmitSound(ReferenceSpell.SoundFile);
                }
            }
            else
            {
                AreaEffect effect = new AreaEffect(Effect.EffectTypes.Web, Cell.GRAPHIC_WEB, Skills.GetSkillLevel(caster.magic), duration, caster, target.CurrentCell);
                if (target.CurrentCell != null)
                    target.CurrentCell.EmitSound(ReferenceSpell.SoundFile);
            }

            if (target == null)
                caster.WriteToDisplay("You cast " + ReferenceSpell.Name + ".");
            else caster.WriteToDisplay("You cast " + ReferenceSpell.Name + " at " + target.GetNameForActionResult(true).Replace("The ", "the "));
            return true;
        }
    }
}
