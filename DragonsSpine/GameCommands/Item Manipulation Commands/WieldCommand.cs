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
    [CommandAttribute("wield", "Wield an item from your belt.", (int)Globals.eImpLevel.USER, new string[] { "draw" },
        1, new string[] { "wield <item>" }, Globals.ePlayerState.PLAYING)]
    public class WieldCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            try
            {
                //Check if they passed any args
                if (args == null || args == "")
                {
                    chr.WriteToDisplay("Wield what?");
                    return true;
                }

                if (chr.RightHand != null && chr.LeftHand != null)
                {
                    chr.WriteToDisplay("Your hands are full.");
                }
                else
                {
                    string[] sArgs = args.Split(" ".ToCharArray());
                    
                    Item tItem = null;

                    if (Int32.TryParse(sArgs[0], out int worldItemID))
                        tItem = chr.RemoveFromBelt(worldItemID);
                    else tItem = chr.RemoveFromBelt(sArgs[0]);

                    if (tItem != null)
                    {
                        if (chr.RightHand == null)
                            chr.EquipRightHand(tItem);
                        else if (chr.LeftHand == null)
                            chr.EquipLeftHand(tItem);
                    }
                    else
                    {
                        chr.WriteToDisplay("You don't have a " + sArgs[0] + " on your belt.");
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return false;
            }

            return true;
        }
    }
}
