namespace DragonsSpine.Commands
{
    [CommandAttribute("showstats", "Display character stats.", (int)Globals.eImpLevel.USER, new string[] { "show stats" },
        0, new string[] { "showstats full" }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowStatsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr is PC && chr.InUnderworld)
            {
                if ((chr as PC).UW_hasIntestines) { chr.WriteToDisplay("You have your intestines."); }
                else { chr.WriteToDisplay("You do not have your intestines."); }

                if ((chr as PC).UW_hasLiver) { chr.WriteToDisplay("You have your liver."); }
                else { chr.WriteToDisplay("You do not have your liver."); }

                if ((chr as PC).UW_hasLungs) { chr.WriteToDisplay("You have your lungs."); }
                else { chr.WriteToDisplay("You do not have your lungs."); }

                if ((chr as PC).UW_hasStomach) { chr.WriteToDisplay("You have your stomach."); }
                else { chr.WriteToDisplay("You do not have your stomach."); }

                chr.WriteToDisplay("You have " + (chr as PC).currentKarma.ToString() + " karma.");
            }
            else
            {
                chr.WriteToDisplay("Strength     : " + GetStatString(chr, Globals.eAbilityStat.Strength).PadRight(9) + "Adds: " + chr.strengthAdd.ToString());
                chr.WriteToDisplay("Dexterity    : " + GetStatString(chr, Globals.eAbilityStat.Dexterity).PadRight(9) + "Adds: " + chr.dexterityAdd.ToString());
                chr.WriteToDisplay("Intelligence : " + GetStatString(chr, Globals.eAbilityStat.Intelligence).PadRight(9) + "Hits Adj: " + chr.HitsAdjustment);
                chr.WriteToDisplay("Wisdom       : " + GetStatString(chr, Globals.eAbilityStat.Wisdom).PadRight(9) + "Stam Adj: " + chr.StaminaAdjustment.ToString());
                if (chr.IsSpellUser)
                    chr.WriteToDisplay("Charisma     : " + GetStatString(chr, Globals.eAbilityStat.Charisma).PadRight(9) + "Mana Adj: " + chr.ManaAdjustment.ToString());
                else chr.WriteToDisplay("Charisma     : " + GetStatString(chr, Globals.eAbilityStat.Charisma));
                chr.WriteToDisplay("Constitution : " + GetStatString(chr, Globals.eAbilityStat.Constitution).PadRight(9) + (chr is PC ? "Karma / Marks: " + (chr as PC).currentKarma + " / " + (chr as PC).currentMarks.ToString() : ""));

                chr.WriteToDisplay("You are " + chr.GetAgeDescription(true) + " level " + chr.Level + " " + chr.Alignment.ToString().ToLower() + " " +
                    Utils.FormatEnumString(chr.BaseProfession.ToString()).ToLower() + " from " + Character.RaceToString(chr) + ".");
                chr.WriteToDisplay("You are carrying " + chr.GetEncumbrance() + " m'na and are " + Rules.GetEncumbrance(chr).ToString().ToLower() + " encumbered.");
                chr.WriteToDisplay("Experience: " + string.Format("{0:n0}", chr.Experience));// + " / " + string.Format("{0:n0}", Rules.GetExperienceRequiredForLevel(chr.Level + 1)));
                if (chr.IsInvisible) chr.WriteToDisplay("** You are invisible. **");
                if (chr.IsHidden) chr.WriteToDisplay("You are hidden.");
            }

            return true;
        }

        private string GetStatString(Character chr, Globals.eAbilityStat stat)
        {
            int statScore = Rules.GetFullAbilityStat(chr, stat);
            int baseScore = 0;

            switch (stat)
            {
                case Globals.eAbilityStat.Strength:
                    baseScore = chr.Strength;
                    break;
                case Globals.eAbilityStat.Dexterity:
                    baseScore = chr.Dexterity;
                    break;
                case Globals.eAbilityStat.Intelligence:
                    baseScore = chr.Intelligence;
                    break;
                case Globals.eAbilityStat.Wisdom:
                    baseScore = chr.Wisdom;
                    break;
                case Globals.eAbilityStat.Charisma:
                    baseScore = chr.Charisma;
                    break;
                case Globals.eAbilityStat.Constitution:
                    baseScore = chr.Constitution;
                    break;
            }

            if (statScore != baseScore)
            {
                return statScore.ToString() + " (" + baseScore.ToString() + ")";
            }
            else return baseScore.ToString();
        }
    }
}
