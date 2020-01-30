namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Trochilidae, "trochilidae", "Trochilidae", "Move with the mein of a hummingbird.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Single, 28, 13, 1900000, "0231", true, true, true, false, true, Character.ClassType.Druid)]
    public class TrochilidaeSpell : ISpellHandler
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

            ReferenceSpell.SendGenericEnchantMessage(caster, target);

            Effect.CreateCharacterEffect(Effect.EffectTypes.Trochilidae, Skills.GetSkillLevel(caster.magic), target, Skills.GetSkillLevel(caster.magic) * 25, caster);
            return true;
        }
    }
}
