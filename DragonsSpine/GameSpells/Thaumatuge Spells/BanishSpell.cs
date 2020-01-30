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
    [SpellAttribute(GameSpell.GameSpellID.Banish, "banish", "Banish", "Call upon your Ghods to banish an entity not indigenous to the Prime Material Plane of existence.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 7, 7, 6400, "0229", false, true, false, false, false, Character.ClassType.Thaumaturge)]
    public class BanishSpell : ISpellHandler
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

            // Only NPCs are summoned and thus only they are affected by this spell.
            if ((target is NPC) && (target as NPC).IsSummoned)
            {
                ReferenceSpell.SendGenericCastMessage(caster, target, true);

                if (!Combat.DND_CheckSavingThrow(target, Combat.SavingThrow.Spell, 0))
                {
                    target.SendToAllInSight(target.GetNameForActionResult() + " has been banished by " + caster.GetNameForActionResult().Replace("The ", "the ") + "!");
                    Rules.UnsummonCreature(target as NPC);
                }
                else
                {
                    caster.WriteToDisplay("You fail to banish " + target.GetNameForActionResult(true) + ".");
                    int totalDamage = (Skills.GetSkillLevel(caster.magic) * (caster.IsPC ? GameSpell.BANISH_SPELL_MULTIPLICAND_PC : GameSpell.BANISH_SPELL_MULTIPLICAND_NPC)) + GameSpell.GetSpellDamageModifier(caster);

                    totalDamage += Rules.RollD(1, 2) == 1 ? Rules.RollD(1, 4) : -(Rules.RollD(1, 4));

                    if (Combat.DoSpellDamage(caster, target, null, totalDamage, ReferenceSpell.Name.ToLower()) == 1)
                    {
                        Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
                        Rules.GiveKillExp(caster, target);
                    }
                }
            }
            else
            {
                caster.WriteToDisplay("Your target cannot be banished.");
                return false;
            }

            return true;
        }
    }
}
