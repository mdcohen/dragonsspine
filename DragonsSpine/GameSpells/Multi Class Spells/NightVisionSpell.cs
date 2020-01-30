namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Night_Vision, "nightvision", "Night Vision", "Target gains the ability to see in darkness.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Single, 10, 6, 1000, "0232", true, true, false, true, false, Character.ClassType.Thief,
        Character.ClassType.Ranger, Character.ClassType.Druid)]
    public class NightVisionSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);

            if (target == null)
                return false;

            ReferenceSpell.SendGenericCastMessage(caster, target, true);
            ReferenceSpell.SendGenericEnchantMessage(caster, target);

            Effect.CreateCharacterEffect(Effect.EffectTypes.Night_Vision, 1, target, Skills.GetSkillLevel(caster.magic) * (ReferenceSpell.ManaCost * 2), caster);

            return true;
        }
    }
}
