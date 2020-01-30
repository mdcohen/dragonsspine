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
    [SpellAttribute(GameSpell.GameSpellID.Contagion, "contagion", "Contagion", "Inflicts a target with a horrific disease, which possibly spreads to whatever beings the target encounters.",
        Globals.eSpellType.Necromancy, Globals.eSpellTargetType.Single, 21, 10, 28000, "0272", false, false, true, false, false, Character.ClassType.Sorcerer)]
    public class ContagionSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            // Acquire target.
            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            int saveMod = 0;
            int effectPower = Skills.GetSkillLevel(caster.magic);

            Item bloodWoodTotem = caster.FindHeldItem(Item.ID_BLOODWOOD_TOTEM);

            // Bloodwood Totem aids the caster of contagion.
            if (bloodWoodTotem != null && !bloodWoodTotem.IsAttunedToOther(caster))
            {
                saveMod = bloodWoodTotem.combatAdds;
                effectPower += bloodWoodTotem.combatAdds;
            }

            // Saving throw.
            if (!Combat.DND_CheckSavingThrow(target, Combat.SavingThrow.Spell, saveMod))
            {
                Effect.CreateCharacterEffect(Effect.EffectTypes.Contagion, effectPower, target, Utils.TimeSpanToRounds(new TimeSpan(0, 20 * Skills.GetSkillLevel(caster.magic), 0)), caster);
                ReferenceSpell.SendGenericStrickenMessage(caster, target);
            }
            else ReferenceSpell.SendGenericResistMessages(caster, target);
            return true;
        }
    }
}
