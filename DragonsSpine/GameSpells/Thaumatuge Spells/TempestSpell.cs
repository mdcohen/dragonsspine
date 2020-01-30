using DragonsSpine.GameWorld;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Tempest, "tempest", "Tempest", "A divine wind sweeps through the lands.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 97, 18, 8000, "0228", false, false, true, false, true, Character.ClassType.Thaumaturge)]
    public class TempestSpell : ISpellHandler
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

            AreaEffect effect = new AreaEffect(Effect.EffectTypes.Tempest, Cell.GRAPHIC_TEMPEST, skillLevel * ReferenceSpell.RequiredLevel, Rules.RollD(2, 4), caster, targetCell);

            return true;
        }
    }
}
