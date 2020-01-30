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
using System.Collections.Generic;
using DragonsSpine.GameWorld;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Chain_Lightning, "clightning", "Chain Lightning", "Caster shoots a lightning bolt at a target and it chains between random targets.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 34, 17, 700000, "0066", false, false, false, false, false, Character.ClassType.None)]
    public class ChainLightningSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            // This spell requires arguments.

            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);

            if (target == null || target.CurrentCell == null) { target = caster; }

            int dmgMultiplier = 6; // Audrey

            if (caster is PC) dmgMultiplier = 8;
            int damage = 0;

            if (Skills.GetSkillLevel(caster.magic) >= 4)
            {
                damage = Skills.GetSkillLevel(caster.magic) * dmgMultiplier + GameSpell.GetSpellDamageModifier(caster);
            }
            else
            {
                damage = 6 * dmgMultiplier + GameSpell.GetSpellDamageModifier(caster);
            }

            if (caster is NPC && caster.species == Globals.eSpecies.LightningDrake)
                damage = Rules.RollD(Skills.GetSkillLevel(caster.magic), 14);

            ReferenceSpell.CastChainSpell(caster, target, Effect.EffectTypes.Lightning, damage, "lightning", "a chain bolt", ReferenceSpell.SoundFile);
                        
            return true;
        }
    }
}
