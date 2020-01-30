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
using ClassType = DragonsSpine.Character.ClassType;

namespace DragonsSpine.Talents
{
    /// <summary>
    /// Blind fighting allows attacks in darkness and when blinded, as well as riposting in either condition.
    /// </summary>
    [TalentAttribute("blindfighting", "Blind Fighting", "Use your gut instincts to defend yourself when your eyes cannot see.", true, 3, 500000, 17, 0, true, new string[] { },
        ClassType.Berserker, ClassType.Fighter, ClassType.Knight, ClassType.Martial_Artist, ClassType.Ranger, ClassType.Ravager, ClassType.Thief)]
    public class BlindFightingTalent : ITalentHandler
    {
        public bool MeetsRequirements(Character chr, Character target)
        {
            return true;
        }

        public bool OnPerform(Character chr, string args)
        {
            Item weapon = null;

            switch (args)
            {
                case "left":
                    weapon = chr.LeftHand;
                    break;
                case "right":
                    weapon = chr.RightHand;
                    break;
            }

            if (weapon == null)
                return false;

            if (weapon.itemType != Globals.eItemType.Weapon)
                return false;

            if (weapon.IsAttunedToOther(chr)) // check if a weapon is attuned
                return false;

            if (!weapon.AlignmentCheck(chr)) // check if a weapon has an alignment
                return false;

            return true;
        }
    }
}
