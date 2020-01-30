using Map = DragonsSpine.GameWorld.Map;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Hide_in_Shadows, "hide", "Hide in Shadows", "Use illusion magic to conceal your appearance and aid you in blending into the shadows.",
        Globals.eSpellType.Illusion, Globals.eSpellTargetType.Self, 3, 1, 0, "0285", true, true, false, false, false, Character.ClassType.Thief)]
    public class HideInShadowsSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            if (Map.IsNextToWall(caster))
            {
                if (!Rules.BreakHideSpell(caster))
                {
                    bool giveXP = false;
                    if (!caster.IsHidden && caster.BaseProfession == Character.ClassType.Thief)
                        giveXP = true;

                    Effect.CreateCharacterEffect(Effect.EffectTypes.Hide_in_Shadows, 1, caster, 0, caster);
                    caster.WriteToDisplay("You fade into the shadows.");

                    if (giveXP)
                        Skills.GiveSkillExp(caster, Skills.GetSkillLevel(caster.thievery) * 25, Globals.eSkillType.Thievery);
                }
                else return false;
            }
            else caster.WriteToDisplay("You must be in the shadows to hide.");

            return true;
        }
    }
}
