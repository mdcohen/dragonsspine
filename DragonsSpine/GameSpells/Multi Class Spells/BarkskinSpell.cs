namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Barkskin, "barkskin", "Barkskin", "Target's skin becomes rough like bark, improving their armor. Beware of fire.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 5, 1, 5300, "0232", true, true, false, true, false, Character.ClassType.Druid, Character.ClassType.Ranger)]
    public class BarkskinSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);

            if (target == null) return false;

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            if (target.EffectsList.ContainsKey(Effect.EffectTypes.Stoneskin))
            {
                caster.WriteToDisplay(target.GetNameForActionResult(false) + " is already enchanted with Stoneskin.");
                return false;
            }

            ReferenceSpell.SendGenericEnchantMessage(caster, target);

            Effect.CreateCharacterEffect(Effect.EffectTypes.Barkskin, 2, target, Utils.TimeSpanToRounds(new System.TimeSpan(0, 10, 0)) + Utils.TimeSpanToRounds(new System.TimeSpan(0, Skills.GetSkillLevel(caster.magic), 0)), caster);

            return true;
        }
    }
}
