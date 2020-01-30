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
    [SpellAttribute(GameSpell.GameSpellID.Create_Illusion, "illusion", "Create Illusion", "Create illusionary terrain.",
        Globals.eSpellType.Illusion, Globals.eSpellTargetType.Area_Effect, 9, 9, 2000, "0271", false, true, false, false, false, Character.ClassType.Wizard)]
    public class CreateIllusionSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            //clean out the command name
            args = args.Replace(ReferenceSpell.Command, "");
            args = args.Trim();

            string[] sArgs = args.Split(" ".ToCharArray());

            Cell cell = Map.GetCellRelevantToCell(caster.CurrentCell, args, false);

            if (cell == null)
            {
                caster.WriteToDisplay("Illusion spell format: CAST <illusion type> <direction>");
                return false;
            }

            if (cell.IsMagicDead)
            {
                //caster.WriteToDisplay("Your spell fails.");
                //caster.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                return false;
            }

            AreaEffect effect;

            switch (sArgs[0].ToLower())
            {
                case "wall":
                    ReferenceSpell.SendGenericCastMessage(caster, null, true);
                    effect = new AreaEffect(Effect.EffectTypes.Illusion, Cell.GRAPHIC_WALL, 0, Skills.GetSkillLevel(caster.magic) * 10, caster, cell);
                    break;
                case "fire":
                    ReferenceSpell.SendGenericCastMessage(caster, null, true);
                    effect = new AreaEffect(Effect.EffectTypes.Illusion, Cell.GRAPHIC_FIRE, 0, Skills.GetSkillLevel(caster.magic) * 10, caster, cell);
                    break;
                case "bridge":
                    ReferenceSpell.SendGenericCastMessage(caster, null, true);
                    effect = new AreaEffect(Effect.EffectTypes.Illusion, Cell.GRAPHIC_BRIDGE, 0, Skills.GetSkillLevel(caster.magic) * 10, caster, cell);
                    break;
                case "empty":
                    ReferenceSpell.SendGenericCastMessage(caster, null, true);
                    effect = new AreaEffect(Effect.EffectTypes.Illusion, Cell.GRAPHIC_EMPTY, 0, Skills.GetSkillLevel(caster.magic) * 10, caster, cell);
                    break;
                case "water":
                    ReferenceSpell.SendGenericCastMessage(caster, null, true);
                    effect = new AreaEffect(Effect.EffectTypes.Illusion, Cell.GRAPHIC_WATER, 0, Skills.GetSkillLevel(caster.magic) * 10, caster, cell);
                    break;
                case "ice":
                    ReferenceSpell.SendGenericCastMessage(caster, null, true);
                    effect = new AreaEffect(Effect.EffectTypes.Illusion, Cell.GRAPHIC_ICE, 0, Skills.GetSkillLevel(caster.magic) * 10, caster, cell);
                    break;
                case "forest":
                    ReferenceSpell.SendGenericCastMessage(caster, null, true);
                    effect = new AreaEffect(Effect.EffectTypes.Illusion, Cell.GRAPHIC_FOREST_FULL, 0, Skills.GetSkillLevel(caster.magic) * 10, caster, cell);
                    break;
                case "tree":
                    ReferenceSpell.SendGenericCastMessage(caster, null, true);
                    effect = new AreaEffect(Effect.EffectTypes.Illusion, Cell.GRAPHIC_FOREST_RIGHT, 0, Skills.GetSkillLevel(caster.magic) * 10, caster, cell);
                    break;
                case "mountain":
                    ReferenceSpell.SendGenericCastMessage(caster, null, true);
                    effect = new AreaEffect(Effect.EffectTypes.Illusion, Cell.GRAPHIC_MOUNTAIN, 0, 200, caster, cell);
                    break;
                default:
                    caster.WriteToDisplay(sArgs[0] + " is not a valid illusion type.");
                    caster.WriteToDisplay("Valid Illusions: wall | fire | bridge | empty | water | ice | forest | tree");
                    break;
            }
            return true;
        }
    }
}
