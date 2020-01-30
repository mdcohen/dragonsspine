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

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Dispel_Illusion, "dispelillusion", "Dispel Illusion", "Removes an area effect illusion.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Area_Effect, 7, 9, 500, "0271", false, true, false, false, false, Character.ClassType.Wizard)]
    public class DispelIllusionSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            ReferenceSpell.SendGenericCastMessage(caster, null, false);

            int xpos = 0;
            int ypos = 0;

            args = args.Replace(ReferenceSpell.Command, "");
            args = args.Trim();
            string[] sArgs = args.Split(" ".ToCharArray());

            #region Get the direction
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
            #endregion

            Cell cell = Cell.GetCell(caster.FacetID, caster.LandID, caster.MapID, caster.X + xpos, caster.Y + ypos, caster.Z);

            foreach (AreaEffect effect in new List<AreaEffect>(cell.AreaEffects.Values))
            {
                if(effect.EffectType == Effect.EffectTypes.Illusion)
                    effect.StopAreaEffect();
            }

            return true;
        }
    }
}
