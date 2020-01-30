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
    [CommandAttribute("impattune", "Attune (bind) an item on the ground or in a hand to a player.", (int)Globals.eImpLevel.GM, new string[] { },
        0, new string[] { "impattune <item name> <player name>" }, Globals.ePlayerState.PLAYING)]
    public class ImpAttuneCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            try
            {
                string[] sArgs = args.Split(" ".ToCharArray());

                Item item = Item.FindItemOnGround(sArgs[0], chr.FacetID, chr.LandID, chr.MapID, chr.X, chr.Y, chr.Z);

                if (item == null)
                {
                    if (chr.RightHand != null && chr.RightHand.name == sArgs[0].ToLower())
                    {
                        item = chr.RightHand;
                    }
                    else if (chr.LeftHand != null && chr.LeftHand.name == sArgs[0].ToLower())
                    {
                        item = chr.LeftHand;
                    }
                }

                if (item != null)
                {
                    PC pc = PC.GetPC(PC.GetPlayerID(sArgs[1]));

                    if (pc != null)
                    {
                        item.AttuneItem(pc.UniqueID, chr.GetLogString() + " used impattune to attune " + item.GetLogString() + " to " + pc.GetLogString() + ".");
                        chr.WriteToDisplay("You have successfully attuned " + sArgs[0] + " to " + pc.Name + ".");
                        return true;
                    }
                    else
                    {
                        chr.WriteToDisplay("Unable to find the player named " + sArgs[1] + " in the database.");
                    }
                }
                else
                {
                    chr.WriteToDisplay("Unable to find an item named " + sArgs[0] + " on the ground or in either of your hands.");
                }
            }
            catch
            {
                chr.WriteToDisplay("Format: impattune <item name> <player name>");
            }

            return false;
        }
    }
}
