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
    [SpellAttribute(GameSpell.GameSpellID.Flame_Shield, "flameshield", "Flame Shield", "The target of this spell is surrounded by a shield of flames.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 3, 8, 0, "0224", true, true, false, true, false, Character.ClassType.Ravager)]
    public class FlameShieldSpell : ISpellHandler
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

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            ReferenceSpell.SendGenericEnchantMessage(caster, target);

            target.SendToAllInSight(target.GetNameForActionResult() + " is surrounded by a shield of flames.");

            // Flame shield cancels out Ensnare spell. (Ensnare may be cast again to snuff out the new flame shield...)
            if (target.EffectsList.ContainsKey(Effect.EffectTypes.Ensnare))
            {
                target.WriteToDisplay("The flames burn off the ensnaring vines!");
                target.EffectsList[Effect.EffectTypes.Ensnare].StopCharacterEffect();
            }

            int effectAmount = Skills.GetSkillLevel(caster.magic) * 2; // magic skill users and scroll/item users (spellPower attribute on Items)

            if (caster.IsHybrid && (caster.Level * 3) > effectAmount) effectAmount = caster.Level * 3; // Ravagers use their level, if it's greater than the item level...

            Effect.CreateCharacterEffect(Effect.EffectTypes.Flame_Shield, effectAmount, target, caster.Level * 8, caster);
            return true;
        }
    }
}
