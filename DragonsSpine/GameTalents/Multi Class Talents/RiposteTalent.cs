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
using eItemBaseType = DragonsSpine.Globals.eItemBaseType;

namespace DragonsSpine.Talents
{
    /// <summary>
    /// The riposte talent is a passive talent.
    /// </summary>
    [TalentAttribute("riposte", "Riposte", "Make a quick return thrust to parry an incoming attack.", true, 2, 55000, 14, 0, true, new string[] { },
        Character.ClassType.Fighter, Character.ClassType.Ranger, Character.ClassType.Thief)]
    public class RiposteTalent : ITalentHandler
    {
        public bool MeetsRequirements(Character chr, Character target)
        {
            return true;
        }

        private eItemBaseType[] allowedItemBaseTypes = new eItemBaseType[]
        {
            eItemBaseType.Dagger,
            eItemBaseType.Fan,
            eItemBaseType.Rapier,
            eItemBaseType.Sword
        };

        // args: left/right targetID
        public bool OnPerform(Character chr, string args)
        {
            if (Rules.GetEncumbrance(chr) > Globals.eEncumbranceLevel.Moderately)
                return false;

            string[] sArgs = args.Split(" ".ToCharArray());

            Item weapon = sArgs[0] == "left" ? chr.LeftHand : chr.RightHand;

            if (weapon == null)
                return false;

            if (weapon.itemType != Globals.eItemType.Weapon)
                return false;

            if (Array.IndexOf(allowedItemBaseTypes, weapon.baseType) == -1)
                return false;

            if (weapon.IsAttunedToOther(chr)) // check if a weapon is attuned
                return false;

            if (!weapon.AlignmentCheck(chr)) // check if a weapon has an alignment
                return false;

            int id = Int32.Parse(sArgs[1]);

            Character target = null;

            if (chr.CurrentCell != null && chr.CurrentCell.Characters.ContainsKey(id))
                target = chr.CurrentCell.Characters[id];

            // safety net
            if (target == null)
                return false;

            chr.WriteToDisplay("You riposte the attack with your " + weapon.name + "!");
            target.WriteToDisplay(chr.Name + " ripostes your attack!");

            //chr.CommandType = CommandTasker.CommandType.Attack;
            Combat.DoCombat(chr, target, weapon);

            return true;
        }
    }
}
