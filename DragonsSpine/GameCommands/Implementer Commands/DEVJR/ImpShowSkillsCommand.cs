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
    [CommandAttribute("impshowskills", "Display a target's skill levels.", (int)Globals.eImpLevel.DEVJR, new string[] { "impshowskill" },
        0, new string[] { "impshowskills <target in view>" }, Globals.ePlayerState.PLAYING)]
    public class ImpShowSkillsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            Character target = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, args, true, true);

            if (target == null)
            {
                chr.WriteToDisplay("You do not see " + args + " here.");
                return true;
            }

            ShowSkillsCommand.DisplayFormattedSkillLevelTwoPerLine(target, chr, Globals.eSkillType.Bow, Globals.eSkillType.Shuriken);
            ShowSkillsCommand.DisplayFormattedSkillLevelTwoPerLine(target, chr, Globals.eSkillType.Dagger, Globals.eSkillType.Staff);
            ShowSkillsCommand.DisplayFormattedSkillLevelTwoPerLine(target, chr, Globals.eSkillType.Flail, Globals.eSkillType.Sword);
            ShowSkillsCommand.DisplayFormattedSkillLevelTwoPerLine(target, chr, Globals.eSkillType.Polearm, Globals.eSkillType.Two_Handed);
            ShowSkillsCommand.DisplayFormattedSkillLevelTwoPerLine(target, chr, Globals.eSkillType.Mace, Globals.eSkillType.Threestaff);
            ShowSkillsCommand.DisplayFormattedSkillLevelTwoPerLine(target, chr, Globals.eSkillType.Rapier, Globals.eSkillType.Unarmed);

            if (Array.IndexOf(World.ThieverySkillProfessions, target.BaseProfession) > -1)
                ShowSkillsCommand.DisplayFormattedSkillLevelTwoPerLine(target, chr, Globals.eSkillType.Thievery, Globals.eSkillType.None);
            if (Array.IndexOf(World.MagicSkillProfessions, target.BaseProfession) > -1)
                ShowSkillsCommand.DisplayFormattedSkillLevelTwoPerLine(target, chr, Globals.eSkillType.Magic, Globals.eSkillType.None);
            if (chr.HasTalent(Talents.GameTalent.TALENTS.Bash))
                ShowSkillsCommand.DisplayFormattedSkillLevelTwoPerLine(target, chr, Globals.eSkillType.Bash, Globals.eSkillType.None);

            if (target.fighterSpecialization != Globals.eSkillType.None ||
                (Array.IndexOf(World.WeaponSpecializationProfessions, target.BaseProfession) != -1 && target.Level >= Character.WARRIOR_SPECIALIZATION_LEVEL))
                chr.WriteToDisplay("Weapon Specialization: " + Utils.FormatEnumString(target.fighterSpecialization.ToString()));

            return true;
        }
    }
}
