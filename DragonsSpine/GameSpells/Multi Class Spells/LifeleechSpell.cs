using System;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Lifeleech, "lifeleech", "Lifeleech", "Caster leeches life from a target.",
        Globals.eSpellType.Necromancy, Globals.eSpellTargetType.Single, 3, 8, 16000, "0270", false, true, false, false, false, Character.ClassType.Ravager, Character.ClassType.Sorcerer)]
    public class LifeleechSpell : ISpellHandler
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

            if(target == caster) { return false; }

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            // Cannot drain life from undead or illusions/images.
            if (target.IsUndead || target.IsImage)
            {
                ReferenceSpell.SendGenericUnaffectedMessage(caster, target);
                return true;
            }

            int damageLevel = caster.Level * 3;

            if (!caster.IsHybrid) damageLevel = Skills.GetSkillLevel(caster.magic) + 3;

            if (Combat.DoSpellDamage(caster, target, null, Convert.ToInt32(damageLevel * ((caster is PC) ? GameSpell.CURSE_SPELL_MULTIPLICAND_PC : GameSpell.CURSE_SPELL_MULTIPLICAND_NPC)) + GameSpell.GetSpellDamageModifier(caster), ReferenceSpell.Name.ToLower()) == 1)
            {
                Rules.GiveKillExp(caster, target);

                if (caster.IsSpellWarmingProfession)
                    Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
            }
            return true;
        }
    }
}