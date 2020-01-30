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
    [CommandAttribute("push", "Push an object in a direction.", (int)Globals.eImpLevel.USER, 1, new string[] { "push <item> <direction>" }, Globals.ePlayerState.PLAYING)]
    public class PushCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null || args == "")
            {
                chr.WriteToDisplay("Push what?");
                return true;
            }

            if (chr.CommandWeight > 3)
            {
                chr.WriteToDisplay("Command weight limit exceeded. Push command not processed.");
                return true;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            if (Item.IsItemOnGround(sArgs[0].ToString().ToLower(), chr.FacetID, chr.LandID, chr.MapID, chr.X, chr.Y, chr.Z))
            {
                if (sArgs.Length < 2 || sArgs[1] == null)
                {
                    chr.WriteToDisplay("Where do you want to push the " + sArgs[0] + "?");
                    return true;
                }

                Item item = Item.RemoveItemFromGround(sArgs[0].ToString(), chr.FacetID, chr.LandID, chr.MapID, chr.X, chr.Y, chr.Z);

                if (item == null)
                {
                    chr.WriteToDisplay(GameSystems.Text.TextManager.NullItemMessage(sArgs[0]));
                    return true;
                }

                GameWorld.Map.MoveCharacter(chr, sArgs[1].ToString(), "");

                if (item.attuneType == Globals.eAttuneType.Take)
                {
                    item.AttuneItem(chr);
                }

                chr.CurrentCell.Add(item);
            }
            else
            {
                chr.WriteToDisplay(GameSystems.Text.TextManager.NullItemMessage(sArgs[1]));
            }

            return true;
        }
    }
}
