#region 
/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
#endregion
using System;
using World = DragonsSpine.GameWorld.World;

namespace DragonsSpine.Commands
{
    [CommandAttribute("showskills", "Display character skills.", (int)Globals.eImpLevel.USER, new string[] { "show skills", "show skill" },
        0, new string[] { "There are no arguments for the show skills command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowSkillsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            DisplayFormattedSkillLevelTwoPerLine(chr, chr, Globals.eSkillType.Bow, Globals.eSkillType.Shuriken);
            DisplayFormattedSkillLevelTwoPerLine(chr, chr, Globals.eSkillType.Dagger, Globals.eSkillType.Staff);
            DisplayFormattedSkillLevelTwoPerLine(chr, chr, Globals.eSkillType.Flail, Globals.eSkillType.Sword);
            DisplayFormattedSkillLevelTwoPerLine(chr, chr, Globals.eSkillType.Polearm, Globals.eSkillType.Two_Handed);
            DisplayFormattedSkillLevelTwoPerLine(chr, chr, Globals.eSkillType.Mace, Globals.eSkillType.Threestaff);
            DisplayFormattedSkillLevelTwoPerLine(chr, chr, Globals.eSkillType.Rapier, Globals.eSkillType.Unarmed);
            if (Array.IndexOf(World.ThieverySkillProfessions, chr.BaseProfession) > -1)
                DisplayFormattedSkillLevelTwoPerLine(chr, chr, Globals.eSkillType.Thievery, Globals.eSkillType.None);
            if (Array.IndexOf(World.MagicSkillProfessions, chr.BaseProfession) > -1)
                DisplayFormattedSkillLevelTwoPerLine(chr, chr, Globals.eSkillType.Magic, Globals.eSkillType.None);
            if (chr.HasTalent(Talents.GameTalent.TALENTS.Bash))
                DisplayFormattedSkillLevelTwoPerLine(chr, chr, Globals.eSkillType.Bash, Globals.eSkillType.None);
            if (chr.fighterSpecialization != Globals.eSkillType.None ||
                (Array.IndexOf(World.WeaponSpecializationProfessions, chr.BaseProfession) != -1 && chr.Level >= Character.WARRIOR_SPECIALIZATION_LEVEL))
                chr.WriteToDisplay("Weapon Specialization: " + Utils.FormatEnumString(chr.fighterSpecialization.ToString()));

            return true;
        }

        /// <summary>
        /// Displays skills formatted for the "Show Skills" command
        /// </summary>
        /// <param name="chr">The character whose skills shall be displayed.</param>
        /// <param name="viewer">The character viewing the skills. (implementors)</param>
        /// <param name="Skill1">First skill to be displayed on this line.</param>
        /// <param name="Skill2">Second skill to be displayed.  Use Globals.eSkillType.None if no 2nd skill</param>
        public static void DisplayFormattedSkillLevelTwoPerLine(Character chr, Character viewer, DragonsSpine.Globals.eSkillType skill1, DragonsSpine.Globals.eSkillType skill2)
        {
            string skill1Title = "";
            string skill1Level = " [0]";

            try
            {
                string dots = ".................................";
                string skill1Name = Skills.GetSkillName(skill1);

                skill1Title = Skills.GetSkillTitle(skill1, chr.BaseProfession, chr.GetSkillExperience(skill1), chr.gender);
                skill1Level = " [" + Skills.GetSkillLevel(chr, skill1) + "]";

                if (skill2 != Globals.eSkillType.None) // requested 2 skills to be displayed?
                {
                    string skill2Name = Skills.GetSkillName(skill2);
                    string skill2Title = Skills.GetSkillTitle(skill2, chr.BaseProfession, chr.GetSkillExperience(skill2), chr.gender);
                    string skill2Level = " [" + Skills.GetSkillLevel(chr, skill2) + "]";

                    viewer.WriteToDisplay(skill1Name +
                                       dots.Substring(0, dots.Length - skill1Name.Length - skill1Title.Length) +
                                       skill1Title + skill1Level.PadRight(8, ' ') + 
                                       skill2Name +
                                       dots.Substring(0, dots.Length - skill2Name.Length - skill2Title.Length) +
                                       skill2Title + skill2Level);
                }
                else // only 1 skill to be displayed
                    viewer.WriteToDisplay(skill1Name +
                                       dots.Substring(0, dots.Length - skill1Name.Length - skill1Title.Length) +
                                       skill1Title + skill1Level + 
                                       "   ");
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }
    }
}
