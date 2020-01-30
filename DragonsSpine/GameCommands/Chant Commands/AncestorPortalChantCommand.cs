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

namespace DragonsSpine.Commands
{
    [CommandAttribute("ashakashtugnushiilani", "Your character will permanetly die. An ancestor will take their place, upon your spirit's return from from the Underworld.",
        (int)Globals.eImpLevel.USER, new string[] { "ashak ashtug nushi ilani" }, 3, new string[] { "There are no arguments for the Ceremony of Ancestors command." },
        Globals.ePlayerState.PLAYING)]
    public class AncestorPortalChantCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.CommandWeight <= 3)
            {
                if (chr.CurrentCell.IsAncestorStart)
                {
                    chr.SendToAllInSight(chr.Name + ": " + GameSystems.Text.TextManager.CHANT_ANCESTOR_START);
                    (chr as PC).IsAncestor = true; // Flag this character as an ancestor (disables Underworld quests).
                    Rules.EnterUnderworld(chr as PC);
                }
                else chr.WriteToDisplay(GameSystems.Text.TextManager.VISION_BLUR);
            }
            else
            {
                chr.WriteToDisplay("Performing death rites requires your full concentration.");
            }
            return true;
        }
    }
}

