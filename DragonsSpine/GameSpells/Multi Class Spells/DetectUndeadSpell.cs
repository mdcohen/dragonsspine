namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Detect_Undead, "detectundead", "Detect Undead", "Target is able to detect all forms of undead beings.",
        Globals.eSpellType.Divination, Globals.eSpellTargetType.Single, 13, 7, 1450, "0231", true, true, false, true, false, Character.ClassType.Sorcerer, Character.ClassType.Thaumaturge, Character.ClassType.Druid)]
    public class DetectUndeadSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, sArgs[sArgs.Length - 1]);

            if (target == null) { return false; }

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            ReferenceSpell.SendGenericEnchantMessage(caster, target);

            int skillLevel = Skills.GetSkillLevel(caster.magic);

            Effect.CreateCharacterEffect(Effect.EffectTypes.Detect_Undead, 1, target, skillLevel * 20, caster);
            return true;
        }
    }
}
