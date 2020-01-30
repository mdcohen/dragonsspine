namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Iceshard, "iceshard", "Iceshard", "Throw a conjured shard of ice at a target. Every five skill levels another shard is conjured. This spell requires a successful ToHit roll per shard using shuriken skill.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Single, 3, 2, 900, "0299", false, true, false, false, false, Character.ClassType.Druid)]
    public class IceshardSpell : ISpellHandler
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
            if (target == caster) { caster.WriteToDisplay("You cannot cast " + ReferenceSpell.Name + " at yourself."); return false; }

            ReferenceSpell.SendGenericCastMessage(caster, target, false);

            int numShards = 1;

            if (Skills.GetSkillLevel(caster.magic) > 4 && caster.Mana >= ReferenceSpell.ManaCost * 2) numShards++; // 2nd shard at magic skill level 5
            if (Skills.GetSkillLevel(caster.magic) > 9 && caster.Mana >= ReferenceSpell.ManaCost * 3) numShards++; // 3rd shard at magic skill level 10
            if (Skills.GetSkillLevel(caster.magic) > 14 && caster.Mana >= ReferenceSpell.ManaCost * 4) numShards++; // 4th shard at magic skill level 15
            if (Skills.GetSkillLevel(caster.magic) > 18 && caster.Mana >= ReferenceSpell.ManaCost * 5) numShards++; // 5th shard at magic skill level 19 (max as of 12/2/2015 Eb)

            // add an orb for magic intensity
            if (caster.Map.HasRandomMagicIntensity && Rules.RollD(1, 100) >= 50)
                numShards++;

            int attackRoll = 0;

            while (numShards > 0)
            {
                caster.EmitSound(ReferenceSpell.SoundFile);

                int toHit = Combat.DND_RollToHit(caster, target, this, ref attackRoll); // 0 is miss, 1 is hit, 2 is critical
                int multiplier = 3;

                Item totem = caster.FindHeldItem(Item.ID_TRUESPIRIT_TOTEM);

                if (totem != null && !totem.IsAttunedToOther(caster))
                    multiplier += Rules.RollD(1, 3) + 1;

                if (toHit > 0)
                {
                    target.WriteToDisplay(caster.GetNameForActionResult() + " hits with an " + ReferenceSpell.Name + "!");

                    if (toHit == 2)
                    {
                        multiplier = 4;
                        caster.WriteToDisplay("Your " + ReferenceSpell.Name + " does critical damage!");
                        target.WriteToDisplay("The " + ReferenceSpell.Name + " does critical damage!");
                    }

                    if (Combat.DoSpellDamage(caster, target, null, (Skills.GetSkillLevel(caster.magic) * multiplier) + GameSpell.GetSpellDamageModifier(caster), ReferenceSpell.Name) == 1)
                    {
                        Rules.GiveKillExp(caster, target);
                        Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
                        Skills.GiveSkillExp(caster, target, Globals.eSkillType.Shuriken);
                        return true; // target is dead, break out of here
                    }
                    else
                    {
                        // magic skill is earned regardless
                        Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
                        // some shuriken, or throwing item skill is earned as well
                        Skills.GiveSkillExp(caster, target, Globals.eSkillType.Shuriken);
                    }
                }
                else
                {
                    caster.WriteToDisplay("Your " + ReferenceSpell.Name + " misses " + target.GetNameForActionResult(true) + ".");
                    target.WriteToDisplay(caster.GetNameForActionResult() + " misses you with " + Character.POSSESSIVE[(int)caster.gender].ToLower() + " " + ReferenceSpell.Name + "!");
                }

                if (caster.Mana < ReferenceSpell.ManaCost) return true; // caster cannot cast any more orbs if no mana left
                else if (numShards > 1) caster.Mana -= ReferenceSpell.ManaCost; // reduce mana for each orb past the first (first orb mana is reduced before this method is called)

                numShards--;
            }
            return true;
        }
    }
}
