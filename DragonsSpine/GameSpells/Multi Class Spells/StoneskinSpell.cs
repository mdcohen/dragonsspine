namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Stoneskin, "stoneskin", "Stoneskin", "Target's skin becomes rough like bark and their armor class improves. Beware of fire.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 20, 12, 203560, "0232", true, true, false, false, false, Character.ClassType.Druid, Character.ClassType.Ranger)]
    public class StoneskinSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);

            if (target == null) return false;

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            if (target.EffectsList.ContainsKey(Effect.EffectTypes.Barkskin))
                target.EffectsList[Effect.EffectTypes.Barkskin].StopCharacterEffect();

            ReferenceSpell.SendGenericEnchantMessage(caster, target);

            Effect.CreateCharacterEffect(Effect.EffectTypes.Stoneskin, 2, target, Skills.GetSkillLevel(caster.magic) / 2, caster);

            return true;
        }
    }
}
