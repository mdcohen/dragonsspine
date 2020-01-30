namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Protection_from_Undead, "prundead", "Protection from Undead", "Target gains bonuses to saving throws, increased protection and additional armor class versus undead.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 6, 5, 1450, "0231", true, true, false, true, false, Character.ClassType.Druid, Character.ClassType.Sorcerer, Character.ClassType.Thaumaturge)]
    public class ProtectionFromUndeadSpell : ISpellHandler
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

            Effect.CreateCharacterEffect(Effect.EffectTypes.Protection_from_Undead, 2, target, Skills.GetSkillLevel(caster.magic) * 20, caster);
            return true;
        }
    }
}
