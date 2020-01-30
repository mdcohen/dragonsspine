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
    [CommandAttribute("impinvisible", "Toggle invisibility.", (int)Globals.eImpLevel.GM, new string[] { "invis", "invisible", "impinvis" },
        0, new string[] { "There are no arguments for the impinvisible command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ImpInvisibleCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            try
            {
                chr.IsInvisible = !chr.IsInvisible;
                if (!chr.IsInvisible)
                {
                    chr.WriteLine("You are no longer invisible.", ProtocolYuusha.TextType.Status);

                    if (chr is PC)
                    {
                        Conference.FriendNotify(chr as PC, true);
                        if (chr.PCState == Globals.ePlayerState.CONFERENCE) (chr as PC).SendToAllInConferenceRoom(Conference.GetStaffTitle(chr as PC) + chr.Name + " has entered the room.", ProtocolYuusha.TextType.Enter);
                    }
                }
                else
                {
                    chr.WriteLine("You are now invisible.", ProtocolYuusha.TextType.Status);
                    if (chr is PC)
                    {
                        (chr as PC).SendToAllInConferenceRoom(Conference.GetStaffTitle(chr as PC) + chr.Name + " has left the world.", ProtocolYuusha.TextType.Exit);
                        Conference.FriendNotify(chr as PC, false);
                    }
                }

                if(chr is PC)
                    PC.SaveField(chr.UniqueID, "invisible", chr.IsInvisible, null);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }

            return true;
        }
    }
}
