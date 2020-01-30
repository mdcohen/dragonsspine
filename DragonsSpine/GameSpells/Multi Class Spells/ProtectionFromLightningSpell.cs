namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Protection_from_Lightning, "prlightning", "Protection from Lightning", "Target receives added protection and saving throw bonuses versus electricity based attacks.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 36, 19, 2600000, "0231", true, true, false, false, false, Character.ClassType.Druid, Character.ClassType.Thaumaturge)]
    public class ProtectionFromLightningSpell : ISpellHandler
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

            Effect.CreateCharacterEffect(Effect.EffectTypes.Protection_from_Lightning, Skills.GetSkillLevel(caster.magic) * 3, target, Skills.GetSkillLevel(caster.magic) * 30, caster);
            return true;
        }
    }
}
