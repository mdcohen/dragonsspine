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
using eItemBaseType = DragonsSpine.Globals.eItemBaseType;

namespace DragonsSpine.Talents
{
    /// <summary>
    /// Perform a second attack upon a successful initial attack. Currently called from AttackCommand, KickCommand and PokeCommand classes.
    /// </summary>
    [TalentAttribute("doubleattack", "Double Attack", "Evaluate your opponent's defenses for a second attack opening.", true, 2, 95000, 15, 5, true, new string[] { },
        Character.ClassType.Berserker, ClassType.Fighter, ClassType.Knight, ClassType.Martial_Artist, ClassType.Ranger, ClassType.Ravager, ClassType.Thief)]
    public class DoubleAttackTalent : ITalentHandler
    {
        private readonly eItemBaseType[] allowedItemBaseTypes = new eItemBaseType[]
        {
            eItemBaseType.Dagger,
            eItemBaseType.Flail,
            eItemBaseType.Halberd,
            eItemBaseType.Mace,
            eItemBaseType.Rapier,
            eItemBaseType.Staff,
            eItemBaseType.Sword,
            eItemBaseType.Threestaff,
            eItemBaseType.TwoHanded,
            eItemBaseType.Thievery,
            eItemBaseType.Magic,
            eItemBaseType.Unarmed,
            eItemBaseType.Fan,
            eItemBaseType.Whip
        };

        public bool MeetsRequirements(Character chr, Character target)
        {
            return true;
        }

        public bool OnPerform(Character chr, string args)
        {
            // No double attack if above moderately encumbered.
            if (Rules.GetEncumbrance(chr) > Globals.eEncumbranceLevel.Moderately)
                return false;

            // Currently non martial artists do not get a double attack chance using true unarmed combat.
            //if (chr.BaseProfession != ClassType.Martial_Artist && chr.LeftHand == null && chr.RightHand == null && !(chr is NPC))
            //    return false;

            string[] sArgs = args.Split(" ".ToCharArray());

            Item weapon = sArgs[0] == "left" ? chr.LeftHand : chr.RightHand;

            if (weapon != null)
            {
                // This may need to be modified later to allow single wielded weapons in left hand, with right hand empty, to double attack.
                // Otherwise, this is a secondary weapon attack so check if the attacker has dual wield.
                if (weapon == chr.LeftHand && !chr.HasTalent(GameTalent.TALENTS.DualWield))
                    return false;

                if (weapon.itemType != Globals.eItemType.Weapon)
                    return false;

                if (Array.IndexOf(allowedItemBaseTypes, weapon.baseType) == -1)
                    return false;

                if (weapon.IsAttunedToOther(chr)) // check if a weapon is attuned
                    return false;

                if (!weapon.AlignmentCheck(chr)) // check if a weapon has an alignment
                    return false;
            }

            int id = Int32.Parse(sArgs[1]);

            Character target = null;

            if (chr.CurrentCell != null && chr.CurrentCell.Characters.ContainsKey(id))
                target = chr.CurrentCell.Characters[id];

            // safety net
            if (target == null)
                return false;

            // for now a successful double attack sends a sound
            chr.SendSound(chr.gender == Globals.eGender.Female ? Sound.GetCommonSound(Sound.CommonSound.FemaleGrunt) : Sound.GetCommonSound(Sound.CommonSound.MaleGrunt));

            if(weapon != null)
                chr.CommandType = CommandTasker.CommandType.Attack;

            Combat.DoCombat(chr, target, weapon);

            return true;
        }
    }
}
