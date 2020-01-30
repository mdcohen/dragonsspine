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
    [SpellAttribute(GameSpell.GameSpellID.Neutralize_Poison, "neutralize", "Neutralize Poison", "Caster neutralizes poison flowing through the blood of their target.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Single, 4, 5, 450, "0231", true, true, false, true, false, Character.ClassType.Druid, Character.ClassType.Ranger, Character.ClassType.Thaumaturge, Character.ClassType.Thief)]
    public class NeutralizePoisonSpell : ISpellHandler
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

            if (target.Poisoned > 0 || target.EffectsList.ContainsKey(Effect.EffectTypes.Poison))
            {
                target.WriteToDisplay("The poison in your veins has been neutralized.");

                if (target.EffectsList.ContainsKey(Effect.EffectTypes.Poison))
                    target.EffectsList[Effect.EffectTypes.Poison].StopCharacterEffect();

                target.Poisoned = 0;
            }

            if (target.EffectsList.ContainsKey(Effect.EffectTypes.Venom))
            {
                target.WriteToDisplay("The venom in your system has been neutralized.");
                target.EffectsList[Effect.EffectTypes.Venom].StopCharacterEffect();
            }

            return true;
        }
    }
}
