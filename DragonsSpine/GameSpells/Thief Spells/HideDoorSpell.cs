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
    [SpellAttribute(GameSpell.GameSpellID.Hide_Door, "hidedoor", "Hide Door", "Mask a door to appear as a wall or rock.",
        Globals.eSpellType.Illusion, Globals.eSpellTargetType.Area_Effect, 7, 7, 900, "0232", false, true, false, false, false, Character.ClassType.Thief)]
    public class HideDoorSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            Cell cell = Map.GetCellRelevantToCell(caster.CurrentCell, args, true);
            bool spellSuccess = false;

            switch (cell.CellGraphic)
            {
                case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:
                case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:
                case Cell.GRAPHIC_OPEN_DOOR_VERTICAL:
                case Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL:
                    caster.WriteToDisplay("You use your magic to conceal the door.");
                    spellSuccess = true;
                    break;
                case Cell.GRAPHIC_EMPTY:
                    if (cell.IsSecretDoor &&
                        (cell.AreaEffects.ContainsKey(Effect.EffectTypes.Find_Secret_Door) || cell.AreaEffects.ContainsKey(Effect.EffectTypes.Find_Secret_Rockwall)))
                    {
                        if (cell.AreaEffects.ContainsKey(Effect.EffectTypes.Find_Secret_Door))
                        {
                            caster.WriteToDisplay("You use your magic to close the secret door.");
                            cell.AreaEffects[Effect.EffectTypes.Find_Secret_Door].StopAreaEffect();
                        }

                        if (cell.AreaEffects.ContainsKey(Effect.EffectTypes.Find_Secret_Rockwall))
                        {
                            caster.WriteToDisplay("You use your magic to conceal the secret door.");
                            cell.AreaEffects[Effect.EffectTypes.Find_Secret_Rockwall].StopAreaEffect();
                        }

                        spellSuccess = true;

                    }
                    break;
                default:
                    caster.WriteToDisplay(GameSystems.Text.TextManager.YOUR_SPELL_FAILS);
                    caster.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                    break;
            }

            if (spellSuccess)
            {
                cell.IsSecretDoor = true;
                int skillLevel = Skills.GetSkillLevel(caster.magic);

                AreaEffect effect = new AreaEffect(Effect.EffectTypes.Hide_Door, Cell.GRAPHIC_WALL, 0, skillLevel * skillLevel, caster, cell);

                return true;
            }


            return false;
        }
    }
}
