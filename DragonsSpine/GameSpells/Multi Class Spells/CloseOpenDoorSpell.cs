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
    [SpellAttribute(GameSpell.GameSpellID.Close_and_Open_Door, "codoor", "Close Or Open Door", "Close or open a visible door.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Area_Effect, 3, 2, 50, "0232", false, true, false, false, false, Character.ClassType.Thief, Character.ClassType.Wizard)]
    public class CloseOpenDoorSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            Cell cell = Map.GetCellRelevantToCell(caster.CurrentCell, args, true);
            string newGraphic = cell.DisplayGraphic;
            bool spellSuccess = false;
            string soundFile = "";

            switch (cell.CellGraphic)
            {
                case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:
                    ReferenceSpell.SendGenericCastMessage(caster, null, true);
                    newGraphic = Cell.GRAPHIC_OPEN_DOOR_VERTICAL;
                    soundFile = Sound.GetCommonSound(Sound.CommonSound.OpenDoor);
                    spellSuccess = true;
                    break;
                case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:
                    ReferenceSpell.SendGenericCastMessage(caster, null, true);
                    newGraphic = Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL;
                    soundFile = Sound.GetCommonSound(Sound.CommonSound.OpenDoor);
                    spellSuccess = true;
                    break;
                case Cell.GRAPHIC_OPEN_DOOR_VERTICAL:
                    ReferenceSpell.SendGenericCastMessage(caster, null, true);
                    newGraphic = Cell.GRAPHIC_CLOSED_DOOR_VERTICAL;
                    soundFile = Sound.GetCommonSound(Sound.CommonSound.CloseDoor);
                    spellSuccess = true;
                    break;
                case Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL:
                    ReferenceSpell.SendGenericCastMessage(caster, null, true);
                    newGraphic = Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL;
                    soundFile = Sound.GetCommonSound(Sound.CommonSound.CloseDoor);
                    spellSuccess = true;
                    break;
                default:
                    caster.WriteToDisplay("There is no door there.");
                    break;
            }

            caster.EmitSound(ReferenceSpell.SoundFile);

            if (spellSuccess)
            {
                cell.CellGraphic = newGraphic;
                cell.DisplayGraphic = newGraphic;
                cell.EmitSound(soundFile);
                return true;
            }

            return false;
        }
    }
}
