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
using eItemBaseType = DragonsSpine.Globals.eItemBaseType;
using ClassType = DragonsSpine.Character.ClassType;

namespace DragonsSpine.Talents
{
    /// <summary>
    /// The dual wield talent is a passive talent.
    /// </summary>
    [TalentAttribute("dualwield", "Dual Wield", "Use weapons in both hands during combat.", true, 3, 110000, 16, 0, true, new string[] { },
        ClassType.Berserker, ClassType.Fighter, ClassType.Knight, ClassType.Martial_Artist, ClassType.Ranger, ClassType.Ravager, ClassType.Thaumaturge, ClassType.Thief)]
    public class DualWieldTalent : ITalentHandler
    {
        private readonly eItemBaseType[] _allowedItemBaseTypes = new eItemBaseType[]
        {
            eItemBaseType.Dagger,
            eItemBaseType.Fan,
            eItemBaseType.Flail,
            eItemBaseType.Mace, // includes axes and hammers
            eItemBaseType.Rapier,
            eItemBaseType.Sword
        };

        public bool MeetsRequirements(Character chr, Character target)
        {
            return true;
        }

        /// <summary>
        /// This is called from other areas of the code to confirm the Character is properly dual wielding.
        /// </summary>
        /// <param name="chr"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool OnPerform(Character chr, string args)
        {
            if (chr.RightHand == null || chr.LeftHand == null) return false; // not dual wielding if either hand is empty

            if (Rules.GetEncumbrance(chr) > Globals.eEncumbranceLevel.Moderately)
                return false;

            var weapons = new List<Item> { chr.RightHand, chr.LeftHand };

            foreach (var w in weapons)
            {
                if (w.itemType != Globals.eItemType.Weapon) // confirm it is a weapon
                    return false;

                if (Array.IndexOf(_allowedItemBaseTypes, w.baseType) == -1) // confirm base type
                    return false;

                if (w.IsAttunedToOther(chr)) // check if a weapon is attuned to wielder (or not an attunable weapon)
                    return false;

                if (!w.AlignmentCheck(chr)) // check if a weapon has an alignment
                    return false;
            }            

            // return true if properly dual wielding
            return true;
        }
    }
}
