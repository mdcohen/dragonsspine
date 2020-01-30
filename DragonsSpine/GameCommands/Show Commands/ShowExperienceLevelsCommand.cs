using System;

namespace DragonsSpine.Commands
{
    [CommandAttribute("showexperiencelevels", "Display a list of experience level values.", (int)Globals.eImpLevel.USER, new string[] { "showexp", "showxp", "show xp", "show exp", "show experience", "show levels" },
        0, new string[] { "You may use 'showxp [level]' to display experience needed for [level]." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowExperienceLevelsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            //long experienceNeeded = Globals.EXP_LEVEL_3;
            //int levelRequested = 0;

            for (int a = 3; a <= Globals.MAX_EXP_LEVEL; a++)
            {
                if (args == null || args == "") // Display level and the long value with commas.
                    chr.WriteToDisplay("Level " + a + ": " + string.Format("{0:n0}", Rules.GetExperienceRequiredForLevel(a)));
                else if(int.TryParse(args, out int levelRequested) && a == levelRequested)
                {
                    chr.WriteToDisplay("Level " + a + ": " + string.Format("{0:n0}", Rules.GetExperienceRequiredForLevel(a)));
                    return true;
                }
            }

            return true;
        }
    }
}
