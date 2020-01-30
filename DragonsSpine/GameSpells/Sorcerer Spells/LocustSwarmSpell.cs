using DragonsSpine.GameWorld;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Locust_Swarm, "locustswarm", "Locust Swarm", "Summon a stinging swarm of hungry locusts that possibly blind all beings in their path.",
        Globals.eSpellType.Conjuration, Globals.eSpellTargetType.Area_Effect, 21, 10, 67000, "0228", false, false, true, false, true, Character.ClassType.Sorcerer)]
    public class LocustSwarmSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            ReferenceSpell.SendGenericCastMessage(caster, null, true);

            if (args == null)
                args = "";

            int multiplier = 0;

            Item totem = caster.FindHeldItem(Item.ID_BLOODWOOD_TOTEM);

            if (totem != null && !totem.IsAttunedToOther(caster) && totem.AlignmentCheck(caster))
            {
                multiplier += Rules.RollD(1, 3) + 1;
                caster.WriteToDisplay("Your " + totem.name + " glows brightly for a moment.");
            }

            int skillLevel = Skills.GetSkillLevel(caster, Globals.eSkillType.Magic);

            AreaEffect effect = new AreaEffect(Effect.EffectTypes.Locust_Swarm, Cell.GRAPHIC_LOCUST_SWARM, (skillLevel + multiplier) * (ReferenceSpell.RequiredLevel / 2), Rules.RollD(3, 4) + multiplier, caster, Map.GetCellRelevantToCell(caster.CurrentCell, args, true));

            return true;
        }
    }
}
