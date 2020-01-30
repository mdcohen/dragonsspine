﻿#region 
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
using System.Collections.Generic;
using DragonsSpine.Commands;

namespace DragonsSpine.Commands
{
    [CommandAttribute("impshowcommands", "Display all commands.", (int)Globals.eImpLevel.DEVJR, new string[] { "impshowcmds" },
        0, new string[] { "impshowcmds # - where # is command weight" }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ImpShowCommandsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null || args == "")
            {
                List<GameCommand> commands = new List<GameCommand>(GameCommand.GameCommandDictionary.Values);

                commands.Sort((s1, s2) => s1.Weight.CompareTo(s2.Weight));

                foreach (GameCommand cmd in commands)
                    chr.WriteToDisplay("COMMAND:" + cmd.Command.PadRight(25) + " WEIGHT: " + cmd.Weight);
            }
            else
            {
                int weightChoice = 0;

                if (Int32.TryParse(args.Split(" ".ToCharArray())[0], out weightChoice) && weightChoice >= 0 && weightChoice <= 3)
                {
                    List<GameCommand> commands = new List<GameCommand>(GameCommand.GameCommandDictionary.Values);

                    commands.Sort((s1, s2) => s1.Weight.CompareTo(s2.Weight));

                    foreach (GameCommand cmd in commands)
                    {
                        if(cmd.Weight == weightChoice)
                            chr.WriteToDisplay("COMMAND:" + cmd.Command.PadRight(25) + " WEIGHT: " + cmd.Weight);
                    }
                }
                else
                {
                    // check character or string, display commands starting with character or string argument

                    chr.WriteToDisplay("Invalid command weight argumemt.");
                    return false;
                }

            }

            return true;
        }
    }
}
