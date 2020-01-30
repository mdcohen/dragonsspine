using EntityLists = DragonsSpine.Autonomy.EntityBuilding.EntityLists;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Ensnare, "ensnare", "Ensnare", "Slow or possibly root a being in place.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 5, 3, 150, "0281", false, true, false, false, false, Character.ClassType.Druid, Character.ClassType.Ranger)]
    public class EnsnareSpell : ISpellHandler
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

            int modifier = 0;
            if (caster.IsWisdomCaster)
                modifier = Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Wisdom) - Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Dexterity);
            else modifier = Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Intelligence) - Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Dexterity);

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            if (EntityLists.PLANT.Contains(target.entity) || EntityLists.INCORPOREAL.Contains(target.entity) || EntityLists.IsPhysicallyMassive(target))
            {
                ReferenceSpell.SendGenericUnaffectedMessage(caster, target);
            }
            else if (Combat.DND_CheckSavingThrow(target, Combat.SavingThrow.PetrificationPolymorph, modifier)) // makes saving throw
            {
                ReferenceSpell.SendGenericResistMessages(caster, target);
                target.EmitSound(ReferenceSpell.SoundFile);
            }
            else
            {
                ReferenceSpell.SendGenericStrickenMessage(caster, target);
                Effect.CreateCharacterEffect(Effect.EffectTypes.Ensnare, Skills.GetSkillLevel(caster.magic), target, Rules.RollD(2, 4) + 2, caster);
            }
            return true;
        }
    }
}
