namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Walking_Death, "walkingdeath", "Walking Death", "You move, act and look like the walking dead.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Single, 37, 17, 755000, "0229", false, false, true, false, true, Character.ClassType.Sorcerer)]
    public class WalkingDeathSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            if (string.IsNullOrEmpty(args)) return false;

            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);

            if (target == null) { return false; }

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            // Physically massive entities cannot be hooked.
            if (Autonomy.EntityBuilding.EntityLists.IsPhysicallyMassive(target))
            {
                caster.WriteToDisplay(target.GetNameForActionResult() + " is too massive to be strangled.");
                return false;
            }

            // Automatic stun.
            // Why is the Stunned property/variable still a short? Waste of conversion. 1/12/2017 Eb
            if (!target.immuneStun && !Autonomy.EntityBuilding.EntityLists.IMMUNE_STUN.Contains(target.entity))
            {
                target.Stunned = (short)Rules.RollD(2, 4);

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

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            if (Combat.DoSpellDamage(caster, target, null, Skills.GetSkillLevel(caster.magic) * 2 * dmgMultiplier + GameSpell.GetSpellDamageModifier(caster), "stranglehold") == 1)
            {
                Rules.GiveKillExp(caster, target);
                Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
            }

            return true;
        }
    }
}
