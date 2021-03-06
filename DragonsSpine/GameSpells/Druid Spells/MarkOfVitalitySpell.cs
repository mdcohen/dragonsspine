﻿namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Mark_of_Vitality, "vitality", "Mark of Vitality", "Target regenerates stamina faster.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Single, 10, 8, 300000, "0231", true, true, false, true, true, Character.ClassType.Druid)]
    public class MarkOfVitalitySpell : ISpellHandler
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

            Effect.CreateCharacterEffect(Effect.EffectTypes.Mark_of_Vitality, Skills.GetSkillLevel(caster.magic) / 2, target, Skills.GetSkillLevel(caster.magic) * 25, caster);
            return true;
        }
    }
}
