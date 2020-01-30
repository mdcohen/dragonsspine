namespace DragonsSpine.Commands
{
    [CommandAttribute("rub", "Rub a held item.", (int)Globals.eImpLevel.USER, new string[] { }, 1, new string[] { "rub <left | right | item name>" }, Globals.ePlayerState.PLAYING)]
    public class RubCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.CommandWeight > 3)
            {
                chr.WriteToDisplay("Command weight limit exceeded. Rub command not processed.");
                return true;
            }

            Item rubbedItem = chr.FindHeldItem(args);

            if (rubbedItem != null)
            {
                chr.WriteToDisplay("You rub " + rubbedItem.shortDesc + ".");
                chr.SendToAllInSight(chr.GetNameForActionResult() + " rubs " + rubbedItem.shortDesc + ".");

                if (rubbedItem.special.Contains("figurine"))
                    Rules.SpawnFigurine(rubbedItem, chr.CurrentCell, chr);
            }
            else chr.WriteToDisplay("You don't see a " + args + " to rub.");

            return true;
        }
    }
}
