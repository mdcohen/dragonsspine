namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Faerie_Fire, "faeriefire", "Faerie Fire", "Target is outlined in colorful, harmless flames giving attackers an advantage.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Single, 4, 2, 2600, "0300", false, true, false, true, false, Character.ClassType.Druid, Character.ClassType.Ranger)]
    public class FaerieFireSpell : ISpellHandler
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

            int modifier = 0;
            if (caster.IsWisdomCaster)
                modifier = Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Wisdom) - Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Dexterity);
            else modifier = Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Intelligence) - Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Dexterity);

            if (!Combat.DND_CheckSavingThrow(target, Combat.SavingThrow.Spell, modifier))
            //if (!Rules.FullStatCheck(target, Globals.eAbilityStat.Dexterity, modifier)) // dexterity check
            {
                Effect.CreateCharacterEffect(Effect.EffectTypes.Faerie_Fire, 2, target, Utils.TimeSpanToRounds(new System.TimeSpan(0, 1, 0)), caster);
                ReferenceSpell.SendGenericStrickenMessage(caster, target);
            }
            else ReferenceSpell.SendGenericResistMessages(caster, target);
            return true;
        }
    }
}
