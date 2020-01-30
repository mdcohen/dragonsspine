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
    [CommandAttribute("macro", "Store commands for rapid use while playing the game.", (int)Globals.eImpLevel.USER, 0, new string[] { "macros", "macro list", "macro #", "macro # <text>" },
        Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class MacroCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            args = args.TrimEnd();

            bool usingMacro = args.Length == 1 || args.Length == 2;

            if (args == null || args == "" || args.StartsWith("list") || (usingMacro && chr.PCState != Globals.ePlayerState.PLAYING))
            {
                #region No arguments or argument is "list" - display list of macros.
                if ((chr as PC).macros.Count == 0)
                {
                    chr.WriteToDisplay("You do not have any macros set.");
                    return true;
                }

                string result = "";

                chr.WriteToDisplay("Macros List:");

                int count = 0;
                foreach(string macro in (chr as PC).macros)
                {
                    chr.WriteToDisplay(count + ": " + macro);
                    count++;
                }
                //for (int a = 0; a < (chr as PC).macros.Count; a++)
                //{
                //    string text = (chr as PC).macros[a].ToString();

                //    if(text.Length > 0) // do not display empty macros
                //        result += (a + " = " + (chr as PC).macros[a].ToString() + "\r\n");
                //}

                chr.WriteToDisplay("" + result);

                if (chr.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                    ProtocolYuusha.SendCharacterMacros(chr as PC, chr as PC);
                return true;
                #endregion
            }

            int macrosIndex = -1;

            if (usingMacro) // using a macro, has to be a macros list index
            {
                if (!Int32.TryParse(args, out macrosIndex))
                {
                    chr.WriteToDisplay("Invalid macro command. View command help for details.");
                    return true;
                }
            }
            else if (args.IndexOf(" ") >= 1) // setting a macro
            {
                if (!Int32.TryParse(args.Substring(0, args.IndexOf(" ")), out macrosIndex))
                {
                    chr.WriteToDisplay("Invalid macro command. View command help for details.");
                    return true;
                }
                else args = args.Substring(args.IndexOf(" ") + 1, args.Length - macrosIndex.ToString().Length - 1);
            }
            else
            {
                chr.WriteToDisplay("Invalid macro command. View command help for details.");
                return true;
            }

            if (macrosIndex >= Character.MAX_MACROS) // 0 through 19
            {
                chr.WriteToDisplay("The current maximum amount of macros is " + Character.MAX_MACROS + ".");
                return true;
            }
            else if (macrosIndex < 0)
            {
                chr.WriteToDisplay("Invalid macro command. View command help for details.");
                return true;
            }
            else if (usingMacro && ((chr as PC).macros.Count < macrosIndex + 1 || (chr as PC).macros[macrosIndex].Length < 1))
            {
                chr.WriteToDisplay("Macro " + macrosIndex + " is not set.");
                return true;
            }

            try
            {
                if (usingMacro)
                {
                    if ((chr as PC).macros[macrosIndex].ToString().IndexOf(" ") != -1)
                    {
                        string command = (chr as PC).macros[macrosIndex].ToString().Substring(0, (chr as PC).macros[macrosIndex].ToString().IndexOf(" "));
                        string newArgs = (chr as PC).macros[macrosIndex].ToString().Substring((chr as PC).macros[macrosIndex].ToString().IndexOf(" ") + 1);
                        return CommandTasker.ParseCommand(chr, command, newArgs);
                    }
                    else
                    {
                        return CommandTasker.ParseCommand(chr, (chr as PC).macros[macrosIndex].ToString(), "");
                    }
                }
                else
                {
                    if (args.IndexOf(ProtocolYuusha.ISPLIT) != -1)
                    {
                        chr.WriteToDisplay("Your macro contains an illegal character. The character " + ProtocolYuusha.ISPLIT + " is reserved.");
                        return true;
                    }

                    if ((chr as PC).macros.Count >= macrosIndex + 1)
                    {
                        (chr as PC).macros[macrosIndex] = args;

                        if (chr.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                            ProtocolYuusha.SendCharacterMacros(chr as PC, chr as PC);

                        chr.WriteToDisplay("Macro " + macrosIndex + " has been set to \"" + args + "\".");
                    }
                    else
                    {
                        (chr as PC).macros.Add(args);

                        if (chr.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                            ProtocolYuusha.SendCharacterMacros(chr as PC, chr as PC);

                        chr.WriteToDisplay("Macro " + macrosIndex + " has been set to \"" + args + "\".");
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return true;
            }
        }
    }
}
