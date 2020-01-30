namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Hunter__s_Mark, "huntersmark", "Hunter's Mark", "While influenced by the Hunter's Mark all bows and thrown weapons have improved accuracy. All physical weapons have increased damage.",
        Globals.eSpellType.Divination, Globals.eSpellTargetType.Single, 9, 6, 30000, "0231", true, true, false, false, false, Character.ClassType.Ranger)]
    public class HuntersMarkSpell : ISpellHandler
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

            Effect.CreateCharacterEffect(Effect.EffectTypes.Hunter__s_Mark, Skills.GetSkillLevel(caster.magic), target, Skills.GetSkillLevel(caster.magic) * 25, caster);
            return true;
        }
    }
}
