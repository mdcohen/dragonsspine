namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Regeneration, "regeneration", "Regeneration", "Target regenerates health faster.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Single, 13, 9, 400000, "0231", true, true, false, true, true, Character.ClassType.Druid)]
    public class RegenerationSpell : ISpellHandler
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

            Effect.CreateCharacterEffect(Effect.EffectTypes.Regeneration, (int)(Skills.GetSkillLevel(caster.magic) / 2), target, Skills.GetSkillLevel(caster.magic) * 25, caster);
            return true;
        }
    }
}
