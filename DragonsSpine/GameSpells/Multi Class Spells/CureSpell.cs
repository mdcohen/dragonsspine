namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Cure, "cure", "Cure", "Heal yourself or a target.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Single, 3, 5, 150, "0233", true, true, false, false, false, Character.ClassType.Druid, Character.ClassType.Knight, Character.ClassType.Thaumaturge)]
    public class CureSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);

            if (target == null)
            {
                if (args == null || args.Length == 0)
                {
                    target = caster;
                }
                else return false;
            }

            // Undead can never be healed by a Ghodly (divine) cure spell. There are other spells to heal pets and undead beings.
            if (target.IsUndead)
            {
                caster.WriteToDisplay("The " + ReferenceSpell.Name + " does not work on the undead.");
                return false; // spell fails
            }

            int cureAmount = 0;
            int pctHitsLeft = (int)(((float)target.Hits / (float)target.HitsFull) * 100);

            // Knights and Rangers also have Cure, though slightly less powerful.
            if (caster.BaseProfession != Character.ClassType.Thaumaturge && caster.BaseProfession != Character.ClassType.Druid)
            {
                if (pctHitsLeft < 75)
                {
                    cureAmount = (int)((target.HitsFull - target.Hits) * .70);
                }
                else { target.Hits = target.HitsFull; }
            }
            else
            {
                if (pctHitsLeft < 50)
                {
                    int criticalHeal = Rules.RollD(1, 100);
                    if (criticalHeal < Skills.GetSkillLevel(caster.magic))
                    {
                        Skills.GiveSkillExp(caster, (caster.Level - criticalHeal) * 10, Globals.eSkillType.Magic);
                        target.Hits = target.HitsFull;
                    }
                    else { cureAmount = (int)((target.HitsFull - target.Hits) * .80); }
                }
                else { target.Hits = target.HitsFull; }
            }

            // Halve the cure amount if diseased.
            if (target.EffectsList.ContainsKey(Effect.EffectTypes.Contagion))
                cureAmount = cureAmount / 2;

            // Thaumaturges and druids gain wisdom amount toward heal.
            if (!caster.IsHybrid && caster.IsWisdomCaster)
                cureAmount += Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Wisdom);

            // Make sure a cured target never goes over their max health.
            if (target.Hits + cureAmount < target.HitsFull)
                target.Hits += cureAmount;
            else target.Hits = target.HitsFull;

            // Send casting message to the caster with the associated sound file.
            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            // Humans and humanoids see the pale glow text message.
            if (Autonomy.EntityBuilding.EntityLists.IsHumanOrHumanoid(caster) && target != caster)
                target.SendToAllInSight(target.GetNameForActionResult() + " is surrounded by a pale blue glow from " + caster.GetNameForActionResult(true) + "'s outstretched hand.");

            target.WriteToDisplay("You have been healed.");

            // Last check to check for a random chance to cure the Sorcerer spell Contagion upon cure cast. (added this because players were complaining about the detrimental effects of Contagion)
            if (target.HasEffect(Effect.EffectTypes.Contagion))
            {
                int chanceToCureContagion = 90 - Skills.GetSkillLevel(caster.magic);

                if (Rules.RollD(1, 100) > chanceToCureContagion)
                {
                    target.EffectsList[Effect.EffectTypes.Contagion].StopCharacterEffect();
                    target.WriteToDisplay("You have recovered from a contagion.");
                    if (target != caster)
                        caster.WriteToDisplay("You have cured " + target.GetNameForActionResult(true) + " of " + Character.POSSESSIVE[(int)target.gender].ToLower() + " contagion.");
                }
            }

            return true;
        }
    }
}
