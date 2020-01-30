namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Lagniappe, "lagniappe", "Lagniappe", "Target receives increased skill gain.",
        Globals.eSpellType.Divination, Globals.eSpellTargetType.Single, 35, 19, 400000, "0231", true, false, false, false, false, Character.ClassType.None)]
    public class LagniappeSpell : ISpellHandler
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

            Effect.CreateCharacterEffect(Effect.EffectTypes.Lagniappe, 2, target, skillLevel * 30, caster);
            return true;
        }
    }
}
