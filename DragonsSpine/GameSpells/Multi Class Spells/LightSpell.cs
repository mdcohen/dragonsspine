namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Light, "light", "Light", "Create a magical burst of light.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 3, 3, 50, "", false, true, false, true, false, Character.ClassType.Druid, Character.ClassType.Knight, Character.ClassType.Thaumaturge, Character.ClassType.Wizard)]
    public class LightSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            ReferenceSpell.SendGenericCastMessage(caster, null, true);
            ReferenceSpell.CastGenericAreaSpell(caster, args, Effect.EffectTypes.Light, Skills.GetSkillLevel(caster.magic) * ReferenceSpell.RequiredLevel + GameSpell.GetSpellDamageModifier(caster), ReferenceSpell.Name);
            return true;
        }
    }
}
