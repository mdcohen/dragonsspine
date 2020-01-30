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
    [CommandAttribute("showtraining", "Display current training.", (int)Globals.eImpLevel.USER, new string[] { "show training" },
        0, new string[] { "There are no arguments for the show training command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowTrainingCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            string spaces = "...................";
            string skillName = "";
            string skillName2 = "";
            chr.WriteToDisplay("Current Training:");
            skillName = (chr as PC).trainedBow.ToString();
            skillName2 = (chr as PC).trainedShuriken.ToString();
            chr.WriteToDisplay("Bow..........." + spaces.Substring(0, spaces.Length - skillName.Length) + skillName + "  Shuriken...." + spaces.Substring(0, spaces.Length - skillName2.Length) + skillName2);
            skillName = (chr as PC).trainedDagger.ToString();
            skillName2 = (chr as PC).trainedStaff.ToString();
            chr.WriteToDisplay("Dagger........" + spaces.Substring(0, spaces.Length - skillName.Length) + skillName + "  Staff......." + spaces.Substring(0, spaces.Length - skillName2.Length) + skillName2);
            skillName = (chr as PC).trainedFlail.ToString();
            skillName2 = (chr as PC).trainedSword.ToString();
            chr.WriteToDisplay("Flail........." + spaces.Substring(0, spaces.Length - skillName.Length) + skillName + "  Sword......." + spaces.Substring(0, spaces.Length - skillName2.Length) + skillName2);
            skillName = (chr as PC).trainedHalberd.ToString();
            skillName2 = (chr as PC).trainedTwoHanded.ToString();
            chr.WriteToDisplay("Halberd......." + spaces.Substring(0, spaces.Length - skillName.Length) + skillName + "  Two Handed.." + spaces.Substring(0, spaces.Length - skillName2.Length) + skillName2);
            skillName = (chr as PC).trainedMace.ToString();
            skillName2 = (chr as PC).trainedThreestaff.ToString();
            chr.WriteToDisplay("Mace.........." + spaces.Substring(0, spaces.Length - skillName.Length) + skillName + "  Threestaff.." + spaces.Substring(0, spaces.Length - skillName2.Length) + skillName2);
            skillName = (chr as PC).trainedRapier.ToString();
            skillName2 = (chr as PC).trainedUnarmed.ToString();
            chr.WriteToDisplay("Rapier........" + spaces.Substring(0, spaces.Length - skillName.Length) + skillName + "  Martial Arts" + spaces.Substring(0, spaces.Length - skillName2.Length) + skillName2);
            skillName2 = "";

            if (chr.BaseProfession == Character.ClassType.Thief)
            {
                skillName = (chr as PC).trainedThievery.ToString();
                chr.WriteToDisplay("Thievery......" + spaces.Substring(0, spaces.Length - skillName.Length) + skillName);
            }
            if (Array.IndexOf(World.MagicSkillProfessions, chr.BaseProfession) > -1)
            {
                skillName = (chr as PC).trainedMagic.ToString();
                chr.WriteToDisplay("Magic........." + spaces.Substring(0, spaces.Length - skillName.Length) + skillName);
            }
            if (chr.HasTalent(Talents.GameTalent.TALENTS.Bash))
            {
                skillName = (chr as PC).trainedBash.ToString();
                chr.WriteToDisplay("Bash.........." + spaces.Substring(0, spaces.Length - skillName.Length) + skillName);
            }

            return true;
        }
    }
}
