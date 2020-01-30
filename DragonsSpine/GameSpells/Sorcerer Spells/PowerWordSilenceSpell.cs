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

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Power_Word___Silence, "silence", "Power Word: Silence", "Caster silences a target or group of targets.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single_or_Group, 9, 13, 125000, "0221", false, false, true, false, false, Character.ClassType.Sorcerer)]
    public class PowerWordSilenceSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, sArgs[sArgs.Length - 1]);

            if (target == null) { return false; }

            // no saving throw

            int magicSkillLevel = Skills.GetSkillLevel(caster.magic);

            int duration = 10; // start at 10 rounds

            if (magicSkillLevel > target.Level) // higher skill level than target level adds rounds
                duration += magicSkillLevel - target.Level;
            else if (magicSkillLevel < target.Level) // lower magic skill level decreases rounds by half per lower level
                duration -= Convert.ToInt32((target.Level - magicSkillLevel) * .5);

            if (duration < 10) duration = 5; // will always do at least 5 rounds of silence

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            // no saving throw...
            Effect.CreateCharacterEffect(Effect.EffectTypes.Silence, 1, target, duration, caster);

            return true;
        }
    }
}
