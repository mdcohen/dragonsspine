using System.Collections.Generic;

namespace DragonsSpine.Commands
{
    [CommandAttribute("comms", "Display a verbose list of game commands.", (int)Globals.eImpLevel.USER, new string[] { "commands" },
        0, new string[] { "You may input an argument to look for commands that start with specified letters. eg: commands show" }, Globals.ePlayerState.CONFERENCE)]
    public class CommandsListCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            var cmdsList = new List<GameCommand>();
            var privLevel = (int)(chr as PC).ImpLevel;
            if (args.ToLower() == "user")
                privLevel = (int)Globals.eImpLevel.USER;

            // Add GameCommand to list if user priveledge level meets requirements.
            foreach (DragonsSpine.Commands.GameCommand gameCommand in GameCommand.GameCommandDictionary.Values)
            {
                if (gameCommand.PrivLevel <= privLevel && !cmdsList.Contains(gameCommand))
                {
                    if(args == "" || args == null ||
                        (args != null && gameCommand.Command.StartsWith(args.ToLower())))
                    cmdsList.Add(gameCommand);
                }
            }

            if (args == "" || args == null)
                chr.WriteLine("You have " + cmdsList.Count + " commands available.", ProtocolYuusha.TextType.Help);
            else chr.WriteLine("You have " + cmdsList.Count + " commands available starting with '" + args + "'.");

            // Sorts alphabetically by Command. (attack, cast, etc)
            cmdsList.Sort((s1, s2) => s1.Command.CompareTo(s2.Command));

            foreach (GameCommand sortedCommand in cmdsList)
            {
                var usageList = "  Usage List: ";
                var aliasList = "  Alias List: ";

                chr.WriteLine(sortedCommand.Command + " - " + sortedCommand.Description, ProtocolYuusha.TextType.Help);

                foreach (string usage in sortedCommand.Usages)
                {
                    usageList += usage + ", ";
                }

                if (usageList.Length > 14)
                {
                    usageList = usageList.Substring(0, usageList.Length - 2);
                    chr.WriteLine(usageList, ProtocolYuusha.TextType.Help);
                }

                foreach (string alias in Commands.GameCommand.GameCommandAliases.Keys)
                {
                    if (GameCommand.GameCommandAliases[alias] == sortedCommand.Command)
                    {
                        aliasList += alias + ", ";
                    }
                }

                if (aliasList.Length > 14)
                {
                    aliasList = aliasList.Substring(0, aliasList.Length - 2);
                    chr.WriteLine(aliasList, ProtocolYuusha.TextType.Help);
                }

                chr.WriteLine("-----");
            }

            return true;
        }
    }
}