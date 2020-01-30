using TargetAcquisition = DragonsSpine.GameSystems.Targeting.TargetAquisition;

namespace DragonsSpine.Talents
{
    [TalentAttribute("gage", "Gage", "Use your wisdom and experience to evaluate a target in order to determine their strengths, weaknesses and abilities compared to your own.",
        false, 0, 10000, 8, 0, true, new string[] { "gage <target>", "gage # <target>" },
        Character.ClassType.All)]
    public class GageTalent : ITalentHandler
    {
        public bool MeetsRequirements(Character chr, Character target)
        {
            return true;
        }

        public bool OnPerform(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Gage who?");
                return false;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = TargetAcquisition.AcquireTarget(chr, sArgs, GameWorld.Cell.DEFAULT_VISIBLE_DISTANCE, 0);

            // failed to find the target
            if (target == null)
            {
                chr.WriteToDisplay("You don't see a " + (sArgs.Length >= 2 ? sArgs[0] + " " + sArgs[1] : sArgs[0]) + " here.");
                return false;
            }

            string gageMessage = target.GetNameForActionResult();

            if (target.Level > chr.Level)
            {
                if (target.Level - chr.Level >= 5)
                    gageMessage += " appears far more experienced than you.";
                else gageMessage += " appears more experienced than you.";
            }
            else if (target.Level == chr.Level)
                gageMessage += " appears as experienced as you are.";
            else
            {
                if(chr.Level - target.Level >= 5)
                    gageMessage += " appears far less experienced than you.";
                else gageMessage += " appears less experienced than you.";
            }

            if (target.talentsDictionary != null && target.talentsDictionary.Count > 0)
                gageMessage += " " + Character.PRONOUN[(int)target.gender] + " looks talented.";

            // Fighter specialization.
            if (target.RightHand != null && target.RightHand.skillType == target.fighterSpecialization)
                gageMessage += " " + Character.PRONOUN[(int)target.gender] + " appears to really know how to use " + Character.POSSESSIVE[(int)target.gender].ToLower() + " " + target.RightHand.name + ".";

            // spell warming professions of the same profession can gage more info
            if(chr.IsSpellWarmingProfession && target.IsSpellWarmingProfession && chr.BaseProfession == target.BaseProfession)
            {
                if (target.GetSkillExperience(Globals.eSkillType.Magic) > chr.GetSkillExperience(Globals.eSkillType.Magic))
                    gageMessage += " " + Character.PRONOUN[(int)target.gender] + " looks more skilled than you in " + Spells.GameSpell.GetSpellCastingNoun(target.BaseProfession);
                else if (target.GetSkillExperience(Globals.eSkillType.Magic) < chr.GetSkillExperience(Globals.eSkillType.Magic))
                    gageMessage += " " + Character.PRONOUN[(int)target.gender] + " looks less skilled than you in " + Spells.GameSpell.GetSpellCastingNoun(target.BaseProfession);
                else
                    gageMessage += " " + Character.PRONOUN[(int)target.gender] + " looks as skilled as you in " + Spells.GameSpell.GetSpellCastingNoun(target.BaseProfession);
            }

            chr.WriteToDisplay(gageMessage.Trim());

            return true;
        }
    }
}            