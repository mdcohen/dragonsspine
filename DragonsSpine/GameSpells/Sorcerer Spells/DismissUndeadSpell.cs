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
    [SpellAttribute(GameSpell.GameSpellID.Dismiss_Undead, "dismiss", "Dismiss Undead", "Cause significant damage to an undead being.",
        Globals.eSpellType.Necromancy, Globals.eSpellTargetType.Single, 7, 7, 11500, "0225", false, true, false, false, false, Character.ClassType.Sorcerer)]
    public class DismissUndeadSpell : ISpellHandler
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

            if (!target.IsUndead)
            {
                caster.WriteToDisplay("The " + ReferenceSpell.Name + " spell only works on the undead.");
                return false;
            }

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            int totalDamage = (Skills.GetSkillLevel(caster.magic) * ((caster.IsPC ? GameSpell.DEATH_SPELL_MULTIPLICAND_PC : GameSpell.DEATH_SPELL_MULTIPLICAND_NPC) + 1)) + GameSpell.GetSpellDamageModifier(caster);

            totalDamage += Rules.RollD(1, 2) == 1 ? Rules.RollD(1, 4) : -(Rules.RollD(1, 4));

            Item totem = caster.FindHeldItem(Item.ID_BLOODWOOD_TOTEM);

            if (totem != null && !totem.IsAttunedToOther(caster))
                totalDamage += Rules.RollD(3, 4);

            if (Combat.DoSpellDamage(caster, target, null, totalDamage, ReferenceSpell.Name.ToLower()) == 1)
            {
                Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
                Rules.GiveKillExp(caster, target);
            }

            return true;
        }
    }
}
