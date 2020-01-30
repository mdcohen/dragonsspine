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
using DragonsSpine.GameWorld;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Wizard_Eye, "wizardeye", "Wizard Eye", "Conjure a familiar and see through its eyes.",
        Globals.eSpellType.Conjuration, Globals.eSpellTargetType.Self, 15, 10, 4000, "", false, true, false, false, false, Character.ClassType.Druid, Character.ClassType.Thief, Character.ClassType.Wizard)]
    public class WizardEyeSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            ReferenceSpell.SendGenericCastMessage(caster, null, false);

            Effect.CreateCharacterEffect(Effect.EffectTypes.Wizard_Eye, 1, caster, 10, caster);
            return true;
        }
    }
}
