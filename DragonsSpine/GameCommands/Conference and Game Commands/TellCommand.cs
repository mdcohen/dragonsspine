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
    [CommandAttribute("tell", "Send a private tell to a player who is online.", (int)Globals.eImpLevel.USER, new string[] { "whisper" },
        0, new string[] { "tell <name> <message>" }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class TellCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args.Equals(null) || args.Equals(""))
            {
                if ((chr as PC).receivePages)
                {
                    (chr as PC).receivePages = false;
                    chr.WriteToDisplay("You will no longer receive tells.");
                }
                else
                {
                    (chr as PC).receivePages = true;
                    chr.WriteToDisplay("You will now receive tells.");
                }
                PC.SaveField(chr.UniqueID, "receiveTells", (chr as PC).receiveTells, null);
                return true;
            }

            string[] sArgs = args.Split(ProtocolYuusha.ASPLIT.ToCharArray());

            if (sArgs.Length < 1 || sArgs[0] == "")
            {
                chr.WriteToDisplay("Format: tell <name> <message>");
                return true;
            }

            PC pc = PC.GetOnline(sArgs[0]);

            string message = "";

            if (pc == null) // did not find a player online, try adventurers
            {
                foreach (Adventurer adv in Character.AdventurersInGameWorldList)
                {
                    if (adv.Name.ToLower() == sArgs[0].ToLower())
                    {
                        for (int a = 1; a < sArgs.Length; a++)
                            message += sArgs[a] + " ";

                        message = message.Trim();

                        chr.WriteToDisplay("You tell " + adv.Name + ", \"" + message + "\"");
                        return true;
                    }
                }

                chr.WriteToDisplay(sArgs[0] + " is not online.");
                return true;
            }

            for (int a = 1; a < sArgs.Length; a++)
                message += sArgs[a] + " ";

            message = message.Trim();

            if (!pc.IsInvisible || (pc.IsInvisible && (chr as PC).ImpLevel >= pc.ImpLevel))
            {
                if (pc.PCState == Globals.ePlayerState.PLAYING)
                {
                    pc.WriteToDisplay(chr.Name + " tells you, \"" + message + "\"");
                    chr.WriteToDisplay("You tell " + pc.Name + ", \"" + message + "\"");
                }
                else if(pc.PCState == Globals.ePlayerState.CONFERENCE)
                {
                    pc.WriteLine(chr.Name + " tells you, \"" + message + "\"", ProtocolYuusha.TextType.Private);
                    chr.WriteToDisplay("You tell " + pc.Name + ", \"" + message + "\"");
                }
                else
                {
                    chr.WriteToDisplay(pc.Name + " is not in the game world or conference rooms. Try again when they are, or send them a mail message.");
                }
            }

            return true;
        }
    }
}
