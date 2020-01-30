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

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Find_Secret_Door, "fsdoor", "Find Secret Door", "Locate secret doors and panels in view.",
        Globals.eSpellType.Divination, Globals.eSpellTargetType.Point_Blank_Area_Effect, 4, 2, 100, "0232", false, true, false, false, false, Character.ClassType.Ranger, Character.ClassType.Thief, Character.ClassType.Wizard)]
    public class FindSecretDoorSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            ReferenceSpell.SendGenericCastMessage(caster, null, true);

            int bitcount = 0;
            //loop through all visible cells
            for (int ypos = -3; ypos <= 3; ypos += 1)
            {
                for (int xpos = -3; xpos <= 3; xpos += 1)
                {
                    Cell cell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + xpos, caster.Y + ypos, caster.Z);
                    if (caster.CurrentCell.visCells[bitcount] && cell.IsSecretDoor)
                    {
                        if (cell.AreaEffects.ContainsKey(Effect.EffectTypes.Hide_Door))
                        {
                            cell.AreaEffects[Effect.EffectTypes.Hide_Door].StopAreaEffect();
                        }
                        else
                        {
                            Effect.EffectTypes effectType = Effect.EffectTypes.Find_Secret_Door;
                            string soundFile = Sound.GetCommonSound(Sound.CommonSound.OpenDoor);

                            if (cell.DisplayGraphic == Cell.GRAPHIC_MOUNTAIN || cell.CellGraphic == Cell.GRAPHIC_MOUNTAIN)
                            {
                                effectType = Effect.EffectTypes.Find_Secret_Rockwall;
                                soundFile = Sound.GetCommonSound(Sound.CommonSound.SlidingRockDoor);
                            }

                            AreaEffect effect = new AreaEffect(effectType, Cell.GRAPHIC_EMPTY, 0, (Skills.GetSkillLevel(caster.magic) / 2) + 10, caster, cell);
                            cell.EmitSound(soundFile);
                        }
                    }
                    bitcount++;
                }
            }

            return true;
        }
    }
}
