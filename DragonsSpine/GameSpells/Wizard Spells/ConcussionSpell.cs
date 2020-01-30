namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Concussion, "concussion", "Concussion", "Blast an area with a powerful concussion.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 10, 8, 80000, "0068", false, false, true, false, false, Character.ClassType.Wizard)]
    public class ConcussionSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            ReferenceSpell.SendGenericCastMessage(caster, null, false);
            ReferenceSpell.CastGenericAreaSpell(caster, args, Effect.EffectTypes.Concussion, Skills.GetSkillLevel(caster.magic) * 6, ReferenceSpell.Name);
            return true;
        }
    }
}
