namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Drudgery, "drudgery", "Drudgery", "Target suffers from decreased skill gain.",
        Globals.eSpellType.Necromancy, Globals.eSpellTargetType.Single, 35, 19, 400000, "0272", true, false, false, false, false, Character.ClassType.None)]
    public class DrudgerySpell : ISpellHandler
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

            ReferenceSpell.SendGenericStrickenMessage(caster, target);

            int skillLevel = Skills.GetSkillLevel(caster.magic);

            Effect.CreateCharacterEffect(Effect.EffectTypes.Drudgery, 2, target, skillLevel * 30, caster);
            return true;
        }
    }
}
