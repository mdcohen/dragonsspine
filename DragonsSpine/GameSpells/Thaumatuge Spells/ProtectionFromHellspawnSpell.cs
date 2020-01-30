namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Protection_from_Hellspawn, "prhellspawn", "Protection from Hellspawn", "Good luck. You're going to need it where you're going.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 29, 16, 840000, "0231", true, false, true, false, true, Character.ClassType.Thaumaturge)]
    public class ProtectionFromHellspawnSpell : ISpellHandler
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

            Effect.CreateCharacterEffect(Effect.EffectTypes.Protection_from_Hellspawn, 2, target, Skills.GetSkillLevel(caster.magic) * 30, caster);
            return true;
        }
    }
}
