namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Wall_of_Fog, "walloffog", "Wall of Fog", "Create a wall of fog. The caster may choose to surround themselves, others, or make it directional fog.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 5, 4, 1700, "0280", false, true, false, false, false, Character.ClassType.Druid)]
    public class WallOfFogSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            ReferenceSpell.SendGenericCastMessage(caster, null, true);
            ReferenceSpell.CastWallSpell(caster, ReferenceSpell, args, Effect.EffectTypes.Fog, 0);
            return true;
        }
    }
}
