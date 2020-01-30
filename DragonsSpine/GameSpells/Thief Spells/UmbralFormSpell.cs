namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Umbral_Form, "umbralform", "Umbral Form", "Your form becomes shadowy and incorporeal.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Self, 74, 18, 0, "", true, false, true, false, true, Character.ClassType.Thief)]
    public class UmbralFormSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            if(!caster.IsImmortal && caster is PC && caster.BaseProfession == Character.ClassType.Thief && Skills.GetSkillLevel(caster.thievery) < 17)
            {
                caster.WriteToDisplay("You are not knowledgable enough in thievery to cast " + ReferenceSpell.Name + ".");
                return false;
            }

            // Permanent hide.
            if (caster.EffectsList.ContainsKey(Effect.EffectTypes.Hide_in_Shadows) && !caster.EffectsList[Effect.EffectTypes.Hide_in_Shadows].IsPermanent)
                caster.EffectsList[Effect.EffectTypes.Hide_in_Shadows].StopCharacterEffect();

            if (!caster.EffectsList.ContainsKey(Effect.EffectTypes.Hide_in_Shadows))
                Effect.CreateCharacterEffect(Effect.EffectTypes.Hide_in_Shadows, 1, caster, -1, caster);

            int skillLevel = Skills.GetSkillLevel(caster.magic);

            Effect.CreateCharacterEffect(Effect.EffectTypes.Umbral_Form, skillLevel / 2, caster, skillLevel * skillLevel, caster);
            ReferenceSpell.SendGenericCastMessage(caster, null, false);
            caster.WriteToDisplay("You have become Shadow.");

            return true;
        }
    }
}
