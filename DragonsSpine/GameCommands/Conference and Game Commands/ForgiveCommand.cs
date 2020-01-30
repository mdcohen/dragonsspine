namespace DragonsSpine.Commands
{
    [CommandAttribute("forgive", "Forgive a player who has killed you unprovoked. This will remove the karma they received.", (int)Globals.eImpLevel.USER, 
        0, new string[] { "There are no arguments for the show belt command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ForgiveCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null || args == "")
            {
                chr.WriteToDisplay("Who do you want to forgive?", ProtocolYuusha.TextType.Status);
                return true;
            }

            PC pc = PC.GetPC(PC.GetPlayerID(args));

            if (pc == null)
            {
                chr.WriteToDisplay("There was no player found named " + args + ".", ProtocolYuusha.TextType.Error);
                return true;
            }

            if (pc.PlayersKilled.Contains(chr.UniqueID))
            {
                pc.PlayersKilled.Remove(chr.UniqueID);

                if (pc.currentMarks > 0)
                {
                    pc.currentMarks--;
                }

                chr.WriteToDisplay("You have forgiven " + pc.Name + ".", ProtocolYuusha.TextType.Status);

                PC online = PC.GetOnline(pc.UniqueID);

                if (online != null)
                {
                    online.WriteLine(chr.Name + " has forgiven you.", ProtocolYuusha.TextType.Status);
                }
            }
            else
            {
                chr.WriteToDisplay(pc.Name + " has not killed you without provocation.", ProtocolYuusha.TextType.Error);
            }

            return true;
        }
    }
}
