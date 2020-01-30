namespace DragonsSpine.Commands
{
    [CommandAttribute("impsummon", "Summon a PC or NPC to your current position.", (int)Globals.eImpLevel.DEVJR, new string[] { },
        0, new string[] { "impsummon <full name of player or NPC>" }, Globals.ePlayerState.PLAYING)]
    public class ImpSummonCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                chr.WriteToDisplay("Format: impsummon <full name of player or NPC>");
                return true;
            }

            foreach (Character ch in Character.PCInGameWorld)
            {
                if (ch.Name.ToLower() == args.ToLower())
                {
                    ch.CurrentCell = chr.CurrentCell;
                    if (chr is PC)
                    {
                        chr.WriteToDisplay("You have summoned the " + ((ch is PC) ? "player " : "NPC ") + ch.Name + " from Facet: " + ch.FacetID + " Land: " + ch.LandID +
                        " Map: " + ch.MapID + " X: " + ch.X + " Y: " + ch.Y + " Z: " + ch.Z);

                        ch.WriteToDisplay("You have been summoned by the Ghods.");
                    }
                    else ch.WriteToDisplay(chr.GetNameForActionResult() + " summons you!");
                    return true;
                }
            }

            NPC npc = null;

            foreach (NPC npcInWorld in Character.NPCInGameWorld)
            {
                if (npcInWorld.Name.ToLower() == args.ToLower())
                {
                    npc = npcInWorld;
                    break;
                }
            }

            if (npc == null)
            {
                chr.WriteToDisplay("Did not find a player or NPC with the name " + args + ".");
                return true;
            }

            if(chr is PC)
                chr.WriteToDisplay("You have summoned " + npc.GetLogString() + ".");

            npc.CurrentCell = chr.CurrentCell;

            return true;
        }
    }
}
