namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Ghod__s_Hooks, "hooks", "Ghod's Hooks", "Fling energy hooks at a humanoid target to nail them in place. Usually causes the target to be stunned.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Single, 19, 9, 75000, "0229", false, false, true, false, true, Character.ClassType.Sorcerer, Character.ClassType.Thaumaturge)]
    public class GhodsHooksSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            if (args == "" || args == null) return false;

            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);

            if (target == null) { return false; }

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            // Physically massive entities cannot be hooked.
            if (Autonomy.EntityBuilding.EntityLists.IsPhysicallyMassive(target))
            {
                caster.WriteToDisplay(target.GetNameForActionResult() + " is too massive to be hooked.");
                return false;
            }

            // Beings who are already stunned cannot be hooked.
            if (target.Stunned > 1)
            {
                caster.WriteToDisplay(target.GetNameForActionResult() + " is already stunned or paralyzed. The Hooks fizzle out of existence.");
                return false;
            }

            // Amoral beings don't have any concerns about the Ghods.
            if (target.Alignment == Globals.eAlignment.Amoral)
            {
                caster.WriteToDisplay(ReferenceSpell.Name + " does not work on amoral beings.");
                return false;
            }

            // Target must not be the same alignment as the caster.
            if (target.Alignment == caster.Alignment)
            {
                caster.WriteToDisplay(ReferenceSpell.Name + " will only be successful on beings of an opposing alignment.");
                return false;
            }

            // Grappling hooks made of divine energy shoot from your hands and latch onto 
            caster.WriteToDisplay("Grappling hooks made of divine energy shoot from your outstretched hands. They latch onto "
                + target.GetNameForActionResult() + " and pull " + Character.PRONOUN_2[(int)target.gender].ToLower() +
                " off the ground. " + Character.PRONOUN[(int)target.gender] +
                " is spun around in the air repeatedly by the hooks and then slammed back to the earth.");

            caster.SendToAllInSight("Grappling hooks made of divine energy shoot from " + caster.GetNameForActionResult(true) +
                "'s outstretched hands. They latch onto "
                + target.GetNameForActionResult(true) + " and pull " + Character.PRONOUN_2[(int)target.gender].ToLower() +
                " off the ground. " + Character.PRONOUN[(int)target.gender] +
                " is spun around in the air repeatedly by the hooks and then slammed back to the earth.");

            // Automatic stun.
            // Why is the Stunned property/variable still a short? Waste of conversion. 1/12/2017 Eb
            if (!target.immuneStun && !Autonomy.EntityBuilding.EntityLists.IMMUNE_STUN.Contains(target.entity))
            {
                target.Stunned = (short)Rules.RollD(1, 2);

                target.WriteToDisplay("You are stunned!");

                if (target.preppedSpell != null)
                {
                    target.preppedSpell = null;
                    target.WriteToDisplay("Your spell has been lost.");
                    target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                }

                target.SendToAllInSight(target.GetNameForActionResult() + " is stunned.");
            }
            else caster.WriteToDisplay(target.GetNameForActionResult() + " is immune to being stunned.");

            int dmgMultiplier = GameSpell.DEATH_SPELL_MULTIPLICAND_NPC;
            if (caster.IsPC) dmgMultiplier = GameSpell.DEATH_SPELL_MULTIPLICAND_PC; // allow players to do slightly more damage than critters at same skill level

            target.EmitSound(GameSpell.GetSpell((int)GameSpell.GameSpellID.Curse).SoundFile);

            if (Combat.DoSpellDamage(caster, target, null, Skills.GetSkillLevel(caster.magic) * dmgMultiplier + GameSpell.GetSpellDamageModifier(caster), "curse") == 1)
            {
                Rules.GiveKillExp(caster, target);
                Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
            }

            return true;
        }
    }
}
