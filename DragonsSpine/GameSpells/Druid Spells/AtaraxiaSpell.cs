namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Ataraxia, "ataraxia", "Ataraxia", "Target regenerates mana faster.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Single, 21, 11, 900000, "0231", true, true, false, true, true, Character.ClassType.Druid)]
    public class AtaraxiaSpell : ISpellHandler
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

            Effect.CreateCharacterEffect(Effect.EffectTypes.Ataraxia, Skills.GetSkillLevel(caster.magic) / 2, target, Skills.GetSkillLevel(caster.magic) * 25, caster);
            return true;
        }
    }
}
