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
    [SpellAttribute(GameSpell.GameSpellID.Shield, "shield", "Shield", "Target receives increased armor class.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 4, 2, 200, "0231", true, true, false, true, false, Character.ClassType.Sorcerer, Character.ClassType.Wizard)]
    public class ShieldSpell : ISpellHandler
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

            ReferenceSpell.SendGenericCastMessage(caster, target, false);

            int shieldEffect = 0;
            int shieldDuration = 0;
            int skillLevel = Skills.GetSkillLevel(caster.magic);
            if (skillLevel < 5) { shieldEffect = 1; shieldDuration = 30; }
            else if (skillLevel >= 5 && skillLevel <= 10) { shieldEffect = 3; shieldDuration = 60; }
            else if (skillLevel > 10 && skillLevel <= 15) { shieldEffect = 6; shieldDuration = 90; }
            else { shieldEffect = 9; shieldDuration = 120; }
            Effect.CreateCharacterEffect(Effect.EffectTypes.Shield, shieldEffect, target, Skills.GetSkillLevel(caster.magic) * shieldDuration, caster);
            target.EmitSound(ReferenceSpell.SoundFile);
            target.WriteToDisplay("You are surrounded by the blue glow of a " + ReferenceSpell.Name + " spell.");
            return true;
        }
    }
}
