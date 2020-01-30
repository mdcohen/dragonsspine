namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Juvenis, "juvenis", "Juvenis", "Target gains improved regeneration of all vitals.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Single, 47, 17, 1852500, "0231", true, false, true, false, true, Character.ClassType.Druid)]
    public class JuvenisSpell : ISpellHandler
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

            Effect.CreateCharacterEffect(Effect.EffectTypes.Juvenis, 1, target, skillLevel * 15, caster);
            return true;
        }
    }
}
