namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Breathe_Water, "brwater", "Breathe Water", "Grant a target the ability to breathe normally while submerged in water.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 8, 8, 1100, "0232", true, true, false, false, false, Character.ClassType.Druid, Character.ClassType.Ranger, Character.ClassType.Thief, Character.ClassType.Wizard)]
    public class BreatheWaterSpell : ISpellHandler
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

            ReferenceSpell.SendGenericEnchantMessage(caster, target);
            int skillLevel = Skills.GetSkillLevel(caster.magic);
            Effect.CreateCharacterEffect(Effect.EffectTypes.Breathe_Water, 1, target, skillLevel * 30, caster);

            return true;
        }
    }
}
