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
    [SpellAttribute(GameSpell.GameSpellID.Blind, "blind", "Blind", "Attempt to blind a being with eyes. The being has a chance to resist blindness.",
        Globals.eSpellType.Necromancy, Globals.eSpellTargetType.Single, 4, 3, 50, "0229", false, true, false, false, false, Character.ClassType.Thaumaturge)]
    public class BlindSpell : ISpellHandler
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

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            int modifier = caster.Level - target.Level;

            if (!Combat.DND_CheckSavingThrow(target, Combat.SavingThrow.Spell, modifier - target.BlindResistance))
            {
                target.WriteToDisplay("You have been blinded!");
                Effect.CreateCharacterEffect(Effect.EffectTypes.Blind, 1, target, (Skills.GetSkillLevel(caster.magic) / 2) + Rules.RollD(1, 4), caster);
            }
            else ReferenceSpell.SendGenericResistMessages(caster, target);
            return true;
        }
    }
}
