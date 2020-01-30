namespace DragonsSpine.Commands
{
    [CommandAttribute("displaypetmessages", "Toggle display of information from pets.", (int)Globals.eImpLevel.USER, new string[] { "displaypetmsgs" },
        0, new string[] { "displaypetmsgs", "displaypetmsgs [off | on]" }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class DisplayPetMessagesCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            PC pc = (chr as PC);

            if (string.IsNullOrEmpty(args))
            {
                pc.DisplayPetMessages = !pc.DisplayPetMessages;

                chr.WriteToDisplay("Display Pet Messages set to " + (pc.DisplayPetMessages ? "ON" : "OFF") + ".");
            }
            else
            {
                if (args.ToLower() == "off") { pc.DisplayPetMessages = false; chr.WriteToDisplay("Display Pet Messages set to OFF."); }
                else if (args.ToLower() == "on") { pc.DisplayPetMessages = true; chr.WriteToDisplay("Display Pet Messages set to ON."); }
                else { chr.WriteToDisplay("Format: DisplayPetMessages on | off"); }
            }

            return true;
        }
    }
}
