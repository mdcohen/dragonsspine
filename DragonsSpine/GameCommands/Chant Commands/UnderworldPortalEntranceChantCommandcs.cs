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
    [CommandAttribute("urrukuyazixul", "Enter the Underworld at a specified location. You will drop all of your gear.",
        (int)Globals.eImpLevel.USER, new string[] {"urruku ya zi xul" }, 3, new string[] { "There are no arguments for the Underworld entrance portal chant." },
        Globals.ePlayerState.PLAYING)]
    public class UnderworldPortalEntranceCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            chr.CommandType = DragonsSpine.CommandTasker.CommandType.Chant;

            if (chr.CommandWeight <= 3)
            {
                if (chr.CurrentCell.IsUnderworldPortal)
                {
                    chr.SendToAllInSight(chr.Name + ": " + DragonsSpine.GameSystems.Text.TextManager.CHANT_UNDERWORLD_PORTAL);
                    Rules.EnterUnderworld(chr as PC);
                }
                else chr.WriteToDisplay(DragonsSpine.GameSystems.Text.TextManager.VISION_BLUR);
            }
            else
            {
                chr.WriteToDisplay("Beckoning the spirits of the Underworld requires your full concentration.");
            }
            return true;
        }
    }
}