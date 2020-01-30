namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Magic_Missile, "magicmissile", "Magic Missile", "Missiles of magical energy spring forth from your fingertip and strike your target.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Single, 3, 1, 0, "0227", false, true, false, false, false, Character.ClassType.Wizard)]
    public class MagicMissileSpell : ISpellHandler
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

            int numMissiles = 1;

            if (Skills.GetSkillLevel(caster.magic) > 5 && caster.Mana >= ReferenceSpell.ManaCost * 2) numMissiles++; // 2nd missile at magic skill level 6
            if (Skills.GetSkillLevel(caster.magic) > 10 && caster.Mana >= ReferenceSpell.ManaCost * 3) numMissiles++; // 3rd missile at magic skill level 11
            if (Skills.GetSkillLevel(caster.magic) > 15 && caster.Mana >= ReferenceSpell.ManaCost * 4) numMissiles++; // 3rd missile at magic skill level 16
            if (Skills.GetSkillLevel(caster.magic) > 20 && caster.Mana >= ReferenceSpell.ManaCost * 5) numMissiles++; // 3rd missile at magic skill level 21

            while (numMissiles > 0)
            {
                if (target == null || target.IsDead)
                    break;

                if (Combat.DoSpellDamage(caster, target, null, (Skills.GetSkillLevel(caster.magic) * 3) + GameSpell.GetSpellDamageModifier(caster), ReferenceSpell.Name) == 1)
                {
                    Rules.GiveKillExp(caster, target);
                    Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
                }

                if (caster.Mana < ReferenceSpell.ManaCost) return true; // caster cannot cast any more orbs if no mana left
                else if (numMissiles > 1) caster.Mana -= ReferenceSpell.ManaCost; // reduce mana for each orb past the first (first orb mana is reduced before this method is called)

                numMissiles--;
            }
            return true;
        }
    }
}
