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
    [CommandAttribute("imprename", "Rename a Character object.", (int)Globals.eImpLevel.AGM, new string[] { "impname" },
        0, new string[] { "There are no arguments for this command." }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class ImpRenameCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if(String.IsNullOrEmpty(args) || args.ToLower() == "help" || args.ToLower() == "?")
            {
                chr.WriteLine("Format: /imprename <old name> <new name>", ProtocolYuusha.TextType.Help);
                return true;
            }

            // args 0 = new name, or Character object
            // args 1 = new name if args 0 is target

            string[] sArgs = args.Split(" ".ToCharArray());

            if((chr as PC).ImpLevel >= Globals.eImpLevel.DEVJR && sArgs.Length >= 1 && sArgs[0].ToLower() == "random")
            {
                // rename ourselves
                chr.Name = DragonsSpine.GameSystems.Text.NameGenerator.GenerateRandomName(chr);
                chr.WriteLine("Your name has been changed to " + chr.Name + ".", ProtocolYuusha.TextType.System);
                // no immediate save
                return true;
            }

            // rename implementor
            if (sArgs.Length == 1)
            {
                chr.Name = sArgs[0];
                chr.WriteLine("Your name has been changed to " + chr.Name + ".", ProtocolYuusha.TextType.System);
                PC.SaveField(chr.UniqueID, "name", sArgs[0], null);
                return true;
            }

            // Now must have two arguments. Renaming a player.

            if(sArgs.Length < 2)
            {
                chr.WriteLine("Format: /rename <old name> <new name>", ProtocolYuusha.TextType.Help);
                return true;
            }

            int id = PC.GetPlayerID(sArgs[0]);

            if (id == -1) // cannot find player ID, check for target in sight (NPC)
            {
                chr.WriteLine("Player '" + sArgs[0] + "' was not found.", ProtocolYuusha.TextType.Error);

                if (chr.PCState == Globals.ePlayerState.PLAYING)
                {
                    Character target = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, sArgs[0], 1);

                    if (target != null)
                    {
                        string oldName = target.Name;
                        target.Name = sArgs[1];
                        chr.WriteToDisplay(oldName + " has been renamed to " + target.Name + ".");
                        return true;
                    }
                    else
                    {
                        chr.WriteToDisplay("There is no " + sArgs[0] + " here.");
                    }
                }

                return true;
            }

            if(CharGen.CharacterNameDenied(chr, sArgs[1]))
            {
                chr.WriteLine("Illegal name.", ProtocolYuusha.TextType.Error);
                return true;
            }

            chr.WriteLine(PC.GetName(id) + "'s name has been changed to " + sArgs[1] + ".", ProtocolYuusha.TextType.System);

            PC.SaveField(id, "name", sArgs[1], null);

            Character online = PC.GetOnline(id);

            if (online != null)
            {
                online.Name = sArgs[1];
                online.WriteLine("Your name has been changed to " + online.Name + ".", ProtocolYuusha.TextType.System);
            }

            return true;
        }
    }
}
