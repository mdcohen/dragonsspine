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
    [CommandAttribute("rename", "Rename your character.", (int)Globals.eImpLevel.USER, new string[] { },
        0, new string[] { "The single argument for the rename command is the new name you wish to have." }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class RenameCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (string.IsNullOrEmpty(args) || args.ToLower() == "help" || args.ToLower() == "?")
            {
                chr.WriteLine("Format: /rename <new name>", ProtocolYuusha.TextType.Help);
            }

            if (CharGen.CharacterNameDenied(chr, args))
            {
                chr.WriteLine("Illegal name.", ProtocolYuusha.TextType.Error);
                return true;
            }
            else
            {
                if (!char.IsUpper(args, 0)) // capitalize the first letter if necessary
                    args = args[0].ToString().ToUpper() + args.Substring(1);

                chr.Name = args;

                PC.SaveField(chr.UniqueID, "name", args, null);

                chr.WriteLine("Your name has been changed to " + chr.Name + ".", ProtocolYuusha.TextType.System);
            }

            return true;
        }
    }
}
