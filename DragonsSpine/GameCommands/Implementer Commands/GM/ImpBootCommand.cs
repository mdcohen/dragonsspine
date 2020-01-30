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
    [CommandAttribute("impboot", "Boot a player from the server.", (int)Globals.eImpLevel.GM, new string[] { },
        0, new string[] { "impboot <player name>" }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class ImpBootCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Who do you want to boot?");
                return false;
            }

            try
            {

                PC pc = PC.GetOnline(args);

                if (pc == null)
                {
                    chr.WriteToDisplay("Unable to find player named " + args + ".");
                    return false;
                }

                chr.WriteToDisplay("You have booted the player named " + pc.Name + ".");

                pc.DisconnectSocket();

                return true;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                chr.WriteToDisplay("Failed to boot the requested player from the game. See a developer for details.");
                return false;
            }
        }
    }
}
