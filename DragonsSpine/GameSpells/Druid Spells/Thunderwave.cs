namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Thunderwave, "thunderwave", "Thunderwave", "A wave of thunderous force sweeps out from you.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 10, 8, 326000, "0282", false, true, true, false, false, Character.ClassType.Druid)]
    public class ThunderwaveSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            if(caster.CurrentCell != null)
            {
                if (caster.CurrentCell.IsWithinTownLimits)
                {
                    caster.WriteToDisplay("You are standing within town limits.");
                    return false;
                }

                if (caster.CurrentCell.IsMagicDead)
                {
                    caster.WriteToDisplay("You are standing in an area that is magic dead.");
                    return false;
                }
            }

            ReferenceSpell.SendGenericCastMessage(caster, null, true);
            ReferenceSpell.CastGenericAreaSpell(caster, args, Effect.EffectTypes.Thunderwave, (Skills.GetSkillLevel(caster.magic) * 4) + GameSpell.GetSpellDamageModifier(caster), ReferenceSpell.Name);
            return true;
        }
    }
}
