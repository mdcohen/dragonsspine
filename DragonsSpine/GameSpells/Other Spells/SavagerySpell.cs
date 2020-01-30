using System;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Savagery, "savagery", "Savagery", "Target receives improved melee and ranged combat critical chance and critical damage multipliers.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 40, 18, 400000, "0293", true, false, true, false, false, Character.ClassType.None)]
    public class SavagerySpell : ISpellHandler
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

            Effect.CreateCharacterEffect(Effect.EffectTypes.Savagery, Math.Max(15, skillLevel), target, skillLevel * 30, caster);
            return true;
        }
    }
}
