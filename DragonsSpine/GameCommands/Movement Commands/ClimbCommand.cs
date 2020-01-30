using Map = DragonsSpine.GameWorld.Map;

namespace DragonsSpine.Commands
{
    [CommandAttribute("climb", "Display a list of belt items.", (int)Globals.eImpLevel.USER, 2, new string[] { "climb up", "climb down", "climb u", "climb d" }, Globals.ePlayerState.PLAYING)]
    public class ClimbCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Climb what?");
            }
            else if (chr is NPC || (chr is PC && (chr as PC).ImpLevel > Globals.eImpLevel.USER) || chr.CommandWeight == 2)
            {
                chr.CommandType = CommandTasker.CommandType.Climb;

                string[] sArgs = args.Split(" ".ToCharArray());
                if (sArgs[0].ToLower() == "up" || sArgs[0].ToLower() == "u")
                {
                    if (chr.CurrentCell.IsOneHandClimbUp)
                    {
                        if (chr.RightHand == null || chr.LeftHand == null)
                        {
                            Map.MoveCharacter(chr, "climb up", "");
                        }
                        else
                        {
                            chr.WriteToDisplay("You have slipped and fallen!");
                        }
                    }
                    else if (chr.CurrentCell.IsTwoHandClimbUp)
                    {
                        if (chr.RightHand == null && chr.LeftHand == null)
                        {
                            Map.MoveCharacter(chr, "climb up", "");
                        }
                        else
                        {
                            chr.WriteToDisplay("You have slipped and fallen!");
                        }
                    }
                    else
                    {
                        chr.WriteToDisplay("There is nothing to climb here.");
                        return true;
                    }
                }
                else if (sArgs[0].ToLower() == "down" || sArgs[0].ToLower() == "d")
                {
                    if (chr.CurrentCell.IsOneHandClimbDown)
                    {
                        if (chr.RightHand == null || chr.LeftHand == null)
                        {
                            Map.MoveCharacter(chr, "climb down", "");
                        }
                        else
                        {
                            chr.WriteToDisplay("You have slipped and fallen!");
                        }
                    }
                    else if (chr.CurrentCell.IsTwoHandClimbDown)
                    {
                        if (chr.RightHand == null && chr.LeftHand == null)
                        {
                            Map.MoveCharacter(chr, "climb down", "");
                        }
                        else
                        {
                            chr.WriteToDisplay("Both hands must be empty to climb down here.");
                        }
                    }
                    else
                    {
                        chr.WriteToDisplay("There is nothing to climb here.");
                    }
                }
                else
                {
                    chr.WriteToDisplay("You can't climb that!");
                }
            }

            return true;
        }
    }
}
