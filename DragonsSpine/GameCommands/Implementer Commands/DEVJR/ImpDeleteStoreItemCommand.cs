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
    [CommandAttribute("impdeletestoreitem", "Remove an item from a merchant's stock.", (int)Globals.eImpLevel.DEVJR, new string[] { "impdelstore" },
        0, new string[] { "impdelstore <stock ID>." }, Globals.ePlayerState.PLAYING)]
    public class ImpDeleteStoreItemCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (String.IsNullOrEmpty(args))
            {
                chr.WriteToDisplay("Usage: impdelstore <stock ID>");
                return false;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            if (sArgs.Length != 1)
            {
                chr.WriteToDisplay("Incorrect number of arguments. Usage: impdelstore <stock ID>");
                return false;
            }

            var stockID = -1;

            if (!Int32.TryParse(sArgs[0], out stockID) || stockID < 1)
            {
                chr.WriteToDisplay("Invalid stock ID: " + sArgs[0]);
                return false;
            }

            if (DAL.DBWorld.DeleteStoreItem(stockID) != -1)
            {
                chr.WriteToDisplay("Stock ID " + stockID + " has been deleted from the database.");
                return true;
            }

            return false;
        }
    }
}
