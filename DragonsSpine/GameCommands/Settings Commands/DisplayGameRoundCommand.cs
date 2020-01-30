namespace DragonsSpine.Commands
{
    [CommandAttribute("displaygameround", "Toggle display of game rounds.", (int)Globals.eImpLevel.USER, new string[] { "displayround" },
        0, new string[] { "displaygameround", "displaygameround [off | on]" }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class DisplayGameRoundCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                (chr as PC).DisplayGameRound = !(chr as PC).DisplayGameRound;
            }
            else
            {
                string[] sArgs = args.Split(" ".ToCharArray());

                if (sArgs[0].ToLower() == "off") { (chr as PC).DisplayGameRound = false; }
                else if (sArgs[0].ToLower() == "on") { (chr as PC).DisplayGameRound = true; }
                else { (chr as PC).DisplayGameRound = !(chr as PC).DisplayGameRound; }
            }

            if ((chr as PC).DisplayGameRound)
                chr.WriteToDisplay("Display game round set to ON.");
            else chr.WriteToDisplay("Display game round set to OFF.");

            return true;
        }
    }
}
