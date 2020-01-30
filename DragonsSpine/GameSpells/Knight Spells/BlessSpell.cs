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
    [SpellAttribute(GameSpell.GameSpellID.Blessing_of_the_Faithful, "bless", "Blessing of the Faithful", "Call upon your Ghod to aid you or your group in battle.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single_or_Group, 3, 8, 0, "0231", true, true, false, true, false, Character.ClassType.Knight)]
    public class BlessSpell : ISpellHandler
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

            // the Bless spell can only be cast by knights on those that are lawful
            if (!caster.IsImmortal && target.Alignment != caster.Alignment)
            {
                caster.WriteToDisplay("You may only bless other " + caster.Alignment.ToString().ToLower() + " beings.");
                return false;
            }

            // TODO: add group cast results (Bless is an atypical enchantment that requires members to be of the same alignment as the caster)

            ReferenceSpell.SendGenericCastMessage(caster, target, true);
            ReferenceSpell.SendGenericEnchantMessage(caster, target);
            // 5/29/2019 Bless adds to shielding, temp dexterity, temp constituion, hits regen, mana regen, stamina regen
            Effect.CreateCharacterEffect(Effect.EffectTypes.Bless, 2, target, Utils.TimeSpanToRounds(new TimeSpan(0, 15, 0)), caster);
            caster.SendToAllInSight(caster.GetNameForActionResult() + " is briefly surrounded by a golden hue.");
            return true;
        }
    }
}
