using DragonsSpine.GameWorld;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Poison_Cloud, "poisoncloud", "Poison Cloud", "Create an unpredictable cloud of choking poison.",
        Globals.eSpellType.Necromancy, Globals.eSpellTargetType.Area_Effect, 24, 14, 8000, "0228", false, true, false, false, false, Character.ClassType.Thaumaturge)]
    public class PoisonCloudSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            ReferenceSpell.SendGenericCastMessage(caster, null, true);

            if (args == null) args = "";

            Cell targetCell = Map.GetCellRelevantToCell(caster.CurrentCell, args, true);

            if (targetCell == null) return false;

            int skillLevel = Skills.GetSkillLevel(caster, Globals.eSkillType.Magic);

            AreaEffect effect = new AreaEffect(Effect.EffectTypes.Poison_Cloud, Cell.GRAPHIC_POISON_CLOUD, skillLevel * (ReferenceSpell.RequiredLevel / 2), Rules.RollD(2, 4), caster, targetCell);

            return true;
        }
    }
}
