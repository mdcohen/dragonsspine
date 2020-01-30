namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Fireball, "fireball", "Fireball", "Create a fireball to engulf a designated area in flames.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 5, 6, 600, "0069", false, true, false, true, false, Character.ClassType.Wizard)]
    public class FireballSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            ReferenceSpell.SendGenericCastMessage(caster, null, true);
            ReferenceSpell.CastGenericAreaSpell(caster, args, Effect.EffectTypes.Fire, Skills.GetSkillLevel(caster.magic) * 4, ReferenceSpell.Name);
            return true;
        }
    }
}
