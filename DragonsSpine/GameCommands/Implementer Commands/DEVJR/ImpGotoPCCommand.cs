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
    [CommandAttribute("impgotopc", "Go to a player in the world.", (int)Globals.eImpLevel.DEVJR, new string[] { "gpc" },
        0, new string[] { "impgotopc <name>" }, Globals.ePlayerState.PLAYING)]
    public class ImpGotoPCCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("impgotopc <player name>");
                return false;
            }
            else
            {
                chr.BreakFollowMode();

                foreach (Character ch in Character.PCInGameWorld)
                {
                    if (ch.Name.ToLower() == args.ToLower())
                    {
                        chr.CurrentCell = ch.CurrentCell;
                        chr.WriteToDisplay("You have teleported to " + ch.GetLogString() + " on map " + GameWorld.World.GetFacetByID(ch.FacetID).GetLandByID(ch.LandID).GetMapByID(ch.MapID).Name + ".");
                        return true;
                    }
                }

                chr.WriteToDisplay("Could not find character.");

                return false;
            }
        }
    }
}
