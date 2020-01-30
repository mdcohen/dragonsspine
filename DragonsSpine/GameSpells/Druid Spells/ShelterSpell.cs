using System;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Shelter, "shelter", "Shelter", "Create a reinforced, impenetrable shelter of thick oak trees and thorny brambles.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 7, 6, 8700, "0279", false, false, true, false, false, Character.ClassType.Druid)]
    public class ShelterSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            if(caster.CurrentCell == null)
            {
                return false;
            }

            if(!caster.CurrentCell.IsOutdoors)
            {
                caster.WriteToDisplay("You must be outdoors to cast " + ReferenceSpell.Name + ".");
                return false;
            }

            ReferenceSpell.SendGenericCastMessage(caster, null, true);
            ReferenceSpell.CastWallSpell(caster, ReferenceSpell, args, Effect.EffectTypes.Shelter, Skills.GetSkillLevel(caster.magic) * Rules.RollD(1, 4));
            return true;
        }
    }
}
