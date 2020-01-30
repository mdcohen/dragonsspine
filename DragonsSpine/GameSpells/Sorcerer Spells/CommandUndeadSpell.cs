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
    [SpellAttribute(GameSpell.GameSpellID.Command_Undead, "commandundead", "Command Undead", "Attempt to command an undead being.",
        Globals.eSpellType.Necromancy, Globals.eSpellTargetType.Single, 12, 8, 42000, "0229", false, true, false, false, false, Character.ClassType.Sorcerer)]
    public class CommandUndeadSpell : ISpellHandler
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

            // auto spell failure on PC targets and non animal
            if (target.IsPC || !target.IsUndead)
            {
                caster.WriteToDisplay("The " + ReferenceSpell.Name + " only works on the undead.");
                return false;
            }

            // a commanded undead counts as a pet and a check is made of how many pets a player has
            if (caster.Pets.Count >= GameSpell.MAX_PETS)
            {
                caster.WriteToDisplay("You do not have the ability to control anymore pets.");
                return false;
            }

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            int savingThrowMod = Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Charisma) - Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Charisma);

            // target is NPC, not a lair critter, and saving throw is failed then command
            if ((target is NPC) && !(target as NPC).lairCritter &&
                !Combat.DND_CheckSavingThrow(target, Combat.SavingThrow.Spell, Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Charisma) - Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Intelligence)))
            {
                // undead is commanded for 7 minutes per skill level
                Effect.CreateCharacterEffect(Effect.EffectTypes.Command_Undead, 1, target, Utils.TimeSpanToRounds(new TimeSpan(0, 7 * Skills.GetSkillLevel(caster.magic), 0)), caster);
            }
            else caster.WriteToDisplay("You fail to command the " + target.Name + ".");

            return true;
        }
    }
}
