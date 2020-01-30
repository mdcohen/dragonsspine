namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Cynosure, "cynosure", "Cynosure", "Target becomes more susceptible to all forms of physical attacks, and stuns.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 31, 15, 1280000, "0229", false, false, true, false, true, Character.ClassType.Druid)]
    public class CynosureSpell : ISpellHandler
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

            if (!Combat.DND_CheckSavingThrow(target, Combat.SavingThrow.Spell, Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Wisdom - Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Wisdom))))
            //if (!Combat.DND_CheckSavingThrow(target, Combat.SavingThrow.Spell, 0))
            {
                Effect.CreateCharacterEffect(Effect.EffectTypes.Cynosure, Skills.GetSkillLevel(caster.magic) / 2, target, Skills.GetSkillLevel(caster.magic) * 10, caster);
                ReferenceSpell.SendGenericStrickenMessage(caster, target);
            }
            else ReferenceSpell.SendGenericResistMessages(caster, target);
            return true;
        }
    }
}
