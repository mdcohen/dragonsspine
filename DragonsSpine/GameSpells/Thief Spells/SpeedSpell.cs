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
    [SpellAttribute(GameSpell.GameSpellID.Speed, "speed", "Speed", "Target is able to move faster.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Single, 20, 11, 8000, "0232", true, true, false, true, false, Character.ClassType.Thief)]
    public class SpeedSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);

            if (target == null) { return false; }

            // currently thieves may only enchant themselves with speed, 9 seconds per skill level
            // as of 9/23/2015 thieves may cast speed on other targets
            ReferenceSpell.SendGenericCastMessage(caster, target, true);
            ReferenceSpell.SendGenericEnchantMessage(caster, target);
            int skillLevel = Skills.GetSkillLevel(caster.magic);
            Effect.CreateCharacterEffect(Effect.EffectTypes.Speed, skillLevel, target, Utils.TimeSpanToRounds(new TimeSpan(0, 0, skillLevel * skillLevel)), caster);
            //target.WriteToDisplay("You feel remarkably fast and amazingly agile.");
            return true;
        }
    }
}
