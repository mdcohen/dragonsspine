using System;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Ferocity, "ferocity", "Ferocity", "Target receives improved spell and physical critical chance and critical damage multipliers.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 30, 16, 200000, "0292", true, false, true, false, false, Character.ClassType.None)]
    public class FerocitySpell : ISpellHandler
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

            Effect.CreateCharacterEffect(Effect.EffectTypes.Ferocity, Math.Max(15, skillLevel), target, skillLevel * 30, caster);
            return true;
        }
    }
}
