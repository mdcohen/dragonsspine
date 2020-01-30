namespace DragonsSpine.Commands
{
    [CommandAttribute("toggledps", "Disable or enable personal damage per second (DPS) logging.", (int)Globals.eImpLevel.USER, new string[] { "toggle dps", "tdps" },
        0, new string[] { "toggledps" }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ToggleDPSLoggingCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            chr.DPSLoggingEnabled = !chr.DPSLoggingEnabled;

            if (chr.DPSLoggingEnabled)
            {
                chr.WriteToDisplay("Your damage log has been enabled.");

                if (chr.Pets != null && chr.Pets.Count > 0)
                    chr.WriteToDisplay("Please note you have pets and their damage output will be included in calculations.");
            }
            else
            {
                chr.WriteToDisplay("Your damage log has been disabled.");
            }

            return true;
        }
    }
}
