using DragonsSpine.GameWorld;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Whirlwind, "whirlwind", "Whirlwind", "Create a powerful and unpredictable whirlwind.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 20, 13, 75000, "0072", false, true, false, false, false,
        Character.ClassType.Druid, Character.ClassType.Wizard)]
    public class WhirlwindSpell : ISpellHandler
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

            AreaEffect effect = new AreaEffect(Effect.EffectTypes.Whirlwind, Cell.GRAPHIC_WHIRLWIND, skillLevel * (ReferenceSpell.RequiredLevel / 2), skillLevel / 2, caster, targetCell);

            return true;
        }
    }
}
