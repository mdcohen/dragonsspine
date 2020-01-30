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
namespace DragonsSpine.Commands
{
    [CommandAttribute("impcellmod", "Modify attributes of a Cell row in the database, or insert a new row for the current cell.", (int)Globals.eImpLevel.DEVJR, new string[] { "impcellm", "impcell" },
        0, new string[] { "impcellmod insert" }, Globals.ePlayerState.PLAYING)]
    public class ImpCellModCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Format: impcast <spell command> <arguments>");
                return false;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            if (sArgs.Length >= 1)
            {
                string mapName = chr.Map.Name;
                int x = chr.CurrentCell.X;
                int y = chr.CurrentCell.Y;
                int z = chr.CurrentCell.Z;

                bool cellRowExists = DAL.DBWorld.CellExistsInDatabase(mapName, x, y, z);

                switch (sArgs[0].ToLower())
                {
                    case "insert":
                    case "ins":
                        if (cellRowExists)
                        {
                            chr.WriteToDisplay("Your current cell already has a row entry in the database. Please use a different argument to modify existing variables.");
                            return true;
                        }
                        else
                        {
                            if (DAL.DBWorld.InsertCellWithDefaultValues(mapName, x, y, z, chr as PC) == 1)
                            {
                                chr.WriteToDisplay("Insertion of new default cell successful. MapName: " + mapName + " Cell X: " + x + " Cell Y: " + y + " Cell Z: " + z);
                                chr.WriteToDisplay("The Cell row currently has default values. It will not be used to assess Cell information until the notes are manually updated.");
                                return true;
                            }
                            else
                            {
                                chr.WriteToDisplay("Insertion of default Cell row failed.");
                                return true;
                            }
                        }
                        // Booleans below. No other arguments are necessary yet as the impdb command may be used easily enough. 12/7/2015 Eb
                    //case "mailbox":
                    //case "pvpenabled":
                    //case "portal":
                    //case "singlecustomer":
                    //case "teleport":
                        //break;
                }
            }

            return true;
        }
    }
}
