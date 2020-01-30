using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DemonName = DragonsSpine.Commands.DemonSummoningChantCommand.DemonName;

namespace DragonsSpine.Commands
{
    [CommandAttribute("finishuw", "Finish all UW quests and teleport to the end.",
        (int)Globals.eImpLevel.USER, new string[] { "finishuw" }, 3, new string[] { "There are no arguments for this command." },
        Globals.ePlayerState.PLAYING)]
    public class FinishUW : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            chr.CommandType = CommandTasker.CommandType.Chant;
            chr.SendToAllInSight(chr.Name + ": I hate the UW!");

            if(chr.InUnderworld)
            {
                (chr as PC).UW_hasIntestines = true;
                (chr as PC).UW_hasLiver = true;
                (chr as PC).UW_hasLungs = true;
                (chr as PC).UW_hasStomach = true;

                Rules.ReturnFromUnderworld(chr);
                return true;
            }
            else
            {
                chr.WriteToDisplay(GameSystems.Text.TextManager.VISION_BLUR);
                return true;
            }
           
        }
    }
}