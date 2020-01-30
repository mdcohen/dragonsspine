using DragonsSpine.GameWorld;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Lightning_Storm, "lightningstorm", "Lightning Storm", "Create a fierce and unpredictable lightning storm in an area.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 35, 18, 11000, "0066", false, true, false, false, false, Character.ClassType.Druid, Character.ClassType.Thaumaturge, Character.ClassType.Wizard)]
    public class LightningStormSpell : ISpellHandler
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

            AreaEffect effect = new AreaEffect(Effect.EffectTypes.Lightning_Storm, Cell.GRAPHIC_LIGHTNING_STORM, skillLevel * (ReferenceSpell.RequiredLevel / 2), Rules.RollD(2, 4), caster, targetCell);
            return true;
        }
    }
}
