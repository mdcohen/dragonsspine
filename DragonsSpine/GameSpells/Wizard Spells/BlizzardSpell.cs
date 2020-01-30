using DragonsSpine.GameWorld;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Blizzard, "blizzard", "Blizzard", "Create a mighty blizzard of scathing ice and wind.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 32, 18, 125000, "0070", false, false, true, false, false, Character.ClassType.Wizard)]
    public class BlizzardSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            caster.CurrentCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.Whirlwind));
            ReferenceSpell.SendGenericCastMessage(caster, null, true);
            if (args == null) args = "";

            Cell targetCell = Map.GetCellRelevantToCell(caster.CurrentCell, args, true);

            if (targetCell == null) return false;

            int skillLevel = Skills.GetSkillLevel(caster, Globals.eSkillType.Magic);

            AreaEffect effect = new AreaEffect(Effect.EffectTypes.Blizzard, Cell.GRAPHIC_ICE_STORM, skillLevel * (ReferenceSpell.RequiredLevel / 2), Rules.RollD(4, 6), caster, targetCell);
            return true;
        }
    }
}
