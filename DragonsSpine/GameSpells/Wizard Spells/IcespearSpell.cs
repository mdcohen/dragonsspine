namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Icespear, "icespear", "Icespear", "Shoot a projectile icespear from your adjoined palms.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Single, 14, 14, 115000, "0222", false, false, true, false, false, Character.ClassType.Wizard)]
    public class IcespearSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            if (args == "" || args == null)
            {
                caster.WriteToDisplay("You must designate a target.");
                return false;
            }

            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);

            if (target == null) { return false; }

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            // a result of 1 means the target is killed
            int numSpears = 1;
            if (Skills.GetSkillLevel(caster.magic) > 16 && caster.Mana >= ReferenceSpell.ManaCost * 2) numSpears++; // 2nd missile at magic skill level 17
            if (Skills.GetSkillLevel(caster.magic) > 19 && caster.Mana >= ReferenceSpell.ManaCost * 3) numSpears++; // 3rd missile at magic skill level 19 RK 1

            while (numSpears > 0)
            {
                if (target == null || target.IsDead)
                    break;

                int skillLevel = Skills.GetSkillLevel(caster.magic);
                if (Combat.DoSpellDamage(caster, target, null, (skillLevel * skillLevel) + GameSpell.GetSpellDamageModifier(caster), "icespear") == 1)
                {
                    Rules.GiveKillExp(caster, target);
                    Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
                }
                else
                {
                    if (Combat.DoSpellDamage(caster, target, null, 0, "stun") == 1)
                    {
                        target.WriteToDisplay("You are stunned!");

                        if (target.preppedSpell != null)
                        {
                            target.preppedSpell = null;
                            target.WriteToDisplay("Your spell has been lost.");
                            target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                        }

                        target.Stunned = (short)(Rules.Dice.Next(1, Skills.GetSkillLevel(caster.magic) / 3) + 1);
                        target.SendToAllInSight(target.GetNameForActionResult() + " is stunned by an " + ReferenceSpell.Name + "!");
                    }
                    else
                    {
                        caster.WriteToDisplay(target.GetNameForActionResult() + " resists being stunned by your " + ReferenceSpell.Name + "!");
                        target.WriteToDisplay("You resist being stunned by the " + ReferenceSpell.Name + "!");
                    }
                }

                numSpears--;
            }

            return true;
        }
    }
}
