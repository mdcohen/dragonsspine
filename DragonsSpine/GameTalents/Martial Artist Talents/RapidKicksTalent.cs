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

namespace DragonsSpine.Talents
{
    [TalentAttribute("rapidkicks", "Rapid Kicks", "Perform rapid kicks based on skill level in the Martial Arts.", false, 5, 121000, 15, 15, true, new string[] { },
        Character.ClassType.Martial_Artist)]
    public class RapidKicksTalent : ITalentHandler
    {
        public bool MeetsRequirements(Character chr, Character target)
        {
            return true;
        }

        public bool OnPerform(Character chr, string args)
        {
            if (Rules.GetEncumbrance(chr) > Globals.eEncumbranceLevel.Moderately)
            {
                chr.WriteToDisplay("You are too encumbered to perform Rapid Kicks.");
                return false;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            // Check if boots are worn.
            Item weapon = chr.GetInventoryItem(Globals.eWearLocation.Feet);

            if (weapon != null)
            {
                // This may need to be modified later to allow single wielded weapons in left hand, with right hand empty, to double attack.
                // Otherwise, this is a secondary weapon attack so check if the attacker has dual wield.
                if (weapon.IsAttunedToOther(chr)) // check if a weapon is attuned
                    return false;

                if (!weapon.AlignmentCheck(chr)) // check if a weapon has an alignment
                    return false;
            }

            Character target = GameSystems.Targeting.TargetAquisition.AcquireTarget(chr, args, 0, 0);

            // safety net
            if (target == null)
            {
                int id = 0;

                if (args.StartsWith("-") || Int32.TryParse(args, out id))
                    chr.WriteToDisplay("You do not see your target here.");
                else chr.WriteToDisplay("You do not see a " + args + " here.");
                return false;
            }

            // for now a successful double attack sends a sound
            chr.SendSound(chr.gender == Globals.eGender.Female ? Sound.GetCommonSound(Sound.CommonSound.FemaleGrunt) : Sound.GetCommonSound(Sound.CommonSound.MaleGrunt));

            if (weapon != null)
                chr.CommandType = CommandTasker.CommandType.Kick;

            int kickCount = (int)(Skills.GetSkillLevel(chr.unarmed) / 4);

            while (chr != null && target != null && !chr.IsDead && !target.IsDead && kickCount > 0)
            {
                Combat.DoCombat(chr, target, weapon);
                kickCount--;
            }

            chr.CommandsProcessed.RemoveAll(cmdType => cmdType == CommandTasker.CommandType.Kick);

            return true;
        }
    }
}
