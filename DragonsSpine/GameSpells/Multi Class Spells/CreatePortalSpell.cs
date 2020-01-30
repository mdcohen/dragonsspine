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
using ArrayList = System.Collections.ArrayList;
using Cell = DragonsSpine.GameWorld.Cell;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Create_Portal, "portal", "Create Portal", "Create a portal through an otherwise impassible wall.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Area_Effect, 6, 5, 500, "0232", false, true, false, true, false, Character.ClassType.Thief, Character.ClassType.Wizard)]
    public class CreatePortalSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            int xpos = 0;
            int ypos = 0;

            //clean out the command name
            args = args.Replace(ReferenceSpell.Command, "");
            args = args.Trim();
            string[] sArgs = args.Split(" ".ToCharArray());

            switch (sArgs[0])
            {
                case "south":
                case "s":
                    ypos++;
                    break;
                case "southeast":
                case "se":
                    ypos++;
                    xpos++;
                    break;
                case "southwest":
                case "sw":
                    ypos++;
                    xpos--;
                    break;
                case "west":
                case "w":
                    xpos--;
                    break;
                case "east":
                case "e":
                    xpos++;
                    break;
                case "northeast":
                case "ne":
                    ypos--;
                    xpos++;
                    break;
                case "northwest":
                case "nw":
                    ypos--;
                    xpos--;
                    break;
                case "north":
                case "n":
                    ypos--;
                    break;
                default:
                    break;
            }

            Cell cell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + xpos, caster.Y + ypos, caster.Z);

            ArrayList cells = new ArrayList();

            cells.Add(cell);

            if ((cell.DisplayGraphic == Cell.GRAPHIC_WALL && !cell.IsMagicDead) || caster.IsImmortal)
            {
                ReferenceSpell.SendGenericCastMessage(caster, null, true);
                AreaEffect effect = new AreaEffect(Effect.EffectTypes.Illusion, Cell.GRAPHIC_EMPTY, 1, 2, caster, cells);
            }
            else
            {
                //GenericFailMessage(caster, "");
                return false;
            }

            return true;
        }
    }
}
