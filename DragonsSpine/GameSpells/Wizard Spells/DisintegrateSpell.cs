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
using System.Collections.Generic;
using Cell = DragonsSpine.GameWorld.Cell;
using Map = DragonsSpine.GameWorld.Map;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Disintegrate, "disintegrate", "Disintegrate", "Destroy a wall or items, and cause damage to living creatures.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 12, 10, 6000, "0271", false, true, false, false, false, Character.ClassType.Wizard)]
    public class DisintegrateSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            args = args.Replace(ReferenceSpell.Command, "");
            args = args.Trim();

            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);
            Cell dCell = null;
            if (target == null) dCell = Map.GetCellRelevantToCell(caster.CurrentCell, args, false);
            else dCell = target.CurrentCell;

            if (dCell == null && target == null)
                return false;

            //var tCell = Map.GetCellRelevantToCell(caster.CurrentCell, args, true);

            if (dCell != null)
            {
                // destroy all but attuned items
                foreach (var item in new List<Item>(dCell.Items))
                {
                    if (item.attunedID <= 0 && !item.IsArtifact() && !((item is Corpse) && (item as Corpse).IsPlayerCorpse))
                    {
                        dCell.Remove(item);
                    }
                }

                // do spell damage
                foreach (Character chr in dCell.Characters.Values)
                {
                    if (Combat.DoSpellDamage(caster, chr, null, Skills.GetSkillLevel(caster.magic) * 10, ReferenceSpell.Name.ToLower()) == 1)
                        Rules.GiveAEKillExp(caster, chr);
                }

                // destroy walls for a while
                if (!dCell.IsMagicDead)
                {
                    if (dCell.DisplayGraphic == Cell.GRAPHIC_WALL)
                    {
                        var newDispGraphic = Rules.RollD(1, 20) >= 10 ? Cell.GRAPHIC_RUINS_LEFT : Cell.GRAPHIC_RUINS_RIGHT;

                        var effect = new AreaEffect(Effect.EffectTypes.Illusion, newDispGraphic, 0, (int)Skills.GetSkillLevel(caster.magic) * 6, caster, dCell);
                        dCell.SendShout("a wall crumbling.");
                    }
                    else if(dCell.DisplayGraphic == Cell.GRAPHIC_RUINS_LEFT || dCell.DisplayGraphic == Cell.GRAPHIC_RUINS_RIGHT)
                    {
                        var newDispGraphic = Rules.RollD(1, 20) >= 10 ? Cell.GRAPHIC_BARREN_LEFT : Cell.GRAPHIC_BARREN_RIGHT;

                        var effect = new AreaEffect(Effect.EffectTypes.Illusion, newDispGraphic, 0, (int)Skills.GetSkillLevel(caster.magic) * 6, caster, dCell);
                        dCell.SendShout("stone crumbling to dust.");
                    }
                }
            }

            ReferenceSpell.SendGenericCastMessage(caster, null, true);

            return true;
        }
    }
}
