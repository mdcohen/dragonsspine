namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Obfuscation, "obfuscation", "Obfuscation", "The caster is able to completely disguise their alignment and profession.",
        Globals.eSpellType.Illusion, Globals.eSpellTargetType.Self, 34, 14, 115000, "0231", false, false, true, false, true, Character.ClassType.Thief)]
    public class ObfuscationSpell : ISpellHandler
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

            int skillLevel = Skills.GetSkillLevel(caster.magic);

            Effect.CreateCharacterEffect(Effect.EffectTypes.Obfuscation, skillLevel / 2, target, (int)(skillLevel * (DragonsSpineMain.MasterRoundInterval / 1000)), caster);

            if (caster.BaseProfession == Character.ClassType.Thief)
                Skills.GiveSkillExp(caster, Skills.GetSkillLevel(caster.magic) * ReferenceSpell.ManaCost, Globals.eSkillType.Magic);
            return true;
        }
    }
}
