using System;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Firewall, "firewall", "Firewall", "Create a wall of fire. The caster may choose to surround themselves.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 5, 6, 700, "0069", false, true, false, false, false, Character.ClassType.Wizard)]
    public class FirewallSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            ReferenceSpell.SendGenericCastMessage(caster, null, true);
            ReferenceSpell.CastWallSpell(caster, ReferenceSpell, args, Effect.EffectTypes.Fire, (int)(Skills.GetSkillLevel(caster.magic) * 4));
            return true;
        }
    }
}
