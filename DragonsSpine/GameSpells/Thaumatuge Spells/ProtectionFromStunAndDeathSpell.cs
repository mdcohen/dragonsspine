namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Protection_from_Stun_and_Death, "prstundeath", "Protection from Stun and Death", "Target receives added protection and saving throw bonuses versus stun and death magic.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 34, 19, 2200000, "0231", true, true, false, false, false, Character.ClassType.Thaumaturge)]
    public class ProtectionFromStunAndDeathSpell : ISpellHandler
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

            Effect.CreateCharacterEffect(Effect.EffectTypes.Protection_from_Stun_and_Death, Skills.GetSkillLevel(caster.magic) * 3, target, Skills.GetSkillLevel(caster.magic) * 30, caster);
            return true;
        }
    }
}
