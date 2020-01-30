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
    [SpellAttribute(GameSpell.GameSpellID.Charm_Animal, "charmanimal", "Charm Animal", "Attempt to charm an animal to become your pet and command it for a limited time.",
        Globals.eSpellType.Enchantment, Globals.eSpellTargetType.Single, 8, 4, 900, "0229", false, true, false, false, false, Character.ClassType.Druid, Character.ClassType.Sorcerer)]
    public class CharmAnimalSpell : ISpellHandler
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
            if (target.IsPC || !target.animal)
            {
                caster.WriteToDisplay("The " + ReferenceSpell.Name + " only works on beings with animal intelligence.");
                return false;
            }

            // a charmed animal counts as a pet and a check is made of how many pets a player has
            if (caster.Pets.Count >= GameSpell.MAX_PETS)
            {
                caster.WriteToDisplay("You do not have the ability to control anymore pets.");
                return false;
            }

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            // some animals cannot be charmed
            if (target is NPC)
            {
                if ((target as NPC).lairCritter)
                {
                    caster.WriteToDisplay(target.GetNameForActionResult() + " cannot be charmed.");
                    return true;
                }

                // specifically for animals that are not lair creatures, such as Smokey
                if(target.entity == Autonomy.EntityBuilding.EntityLists.Entity.Smokey)
                {
                    caster.WriteToDisplay(target.GetNameForActionResult() + " cannot be charmed.");
                    return true;
                }

                if(target.special.Contains("figurine"))
                {
                    caster.WriteToDisplay("You cannot charm animals linked to another plane of existence.");
                    return true;
                }
            }

            // target is NPC, not a lair critter, and saving throw is failed (charisma modifier) then charm
            if (!Combat.DND_CheckSavingThrow(target, Combat.SavingThrow.Spell, Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Charisma - Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Charisma))))
            {
                int skillLevel = Skills.GetSkillLevel(caster.magic);
                // animal is charmed for 5 minutes per skill level
                Effect.CreateCharacterEffect(Effect.EffectTypes.Charm_Animal, 1, target, Utils.TimeSpanToRounds(new TimeSpan(0, 5 * Skills.GetSkillLevel(caster.magic), 0)), caster);
                if (ReferenceSpell.IsClassSpell(caster.BaseProfession))
                    Skills.GiveSkillExp(caster, skillLevel * (ReferenceSpell.ManaCost + target.Level), Globals.eSkillType.Magic);
            }
            else caster.WriteToDisplay("You fail to charm the " + target.Name + ".");

            return true;
        }
    }
}
