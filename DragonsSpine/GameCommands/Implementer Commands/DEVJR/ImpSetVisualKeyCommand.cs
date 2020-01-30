using System;
using DragonsSpine.GameSystems.Targeting;

namespace DragonsSpine.Commands
{
    [CommandAttribute("impsetvisualkey", "Set a visual key override. This variable is not stored in the database.", (int)Globals.eImpLevel.DEVJR, new string[] { "impsetvk" },
         0, new string[] { "impsetvk <target> <visualKey> | <name of online player> <visualKey>" }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ImpSetVisualKeyommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                chr.WriteToDisplay("impsetvk requires a target or the name of a player who is online. Argument 2 is the desired visualKey or 'clear' to remove the overidden visual key.");
                return true;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            if(sArgs.Length < 2)
            {
                chr.WriteToDisplay("impsetvk requires a target or the name of a player who is online. Argument 2 is the desired visualKey or 'clear' to remove the overidden visual key.");
                return true;
            }

            if(sArgs[0].ToLower() == "me")
            {
                if (sArgs[1].ToLower() == "clear")
                {
                    chr.visualKeyOverride = "";
                    chr.WriteToDisplay("Your visual key override has been cleared.");
                }
                else
                {
                    chr.visualKeyOverride = sArgs[1].ToLower();
                    chr.WriteToDisplay("Your visual key override set to " + sArgs[1].ToLower() + ".");
                }
                return true;
            }

            Character target = TargetAquisition.AcquireTarget(chr, sArgs[0], GameWorld.Cell.DEFAULT_VISIBLE_DISTANCE, 0);

            // no way to validate the visual key now, as this data is not sent to the server from the client 9/13/2019

            if(target != null)
            {
                if (sArgs[1].ToLower() == "clear")
                {
                    target.visualKeyOverride = "";
                    chr.WriteToDisplay(target.GetNameForActionResult() + "'s visual key override has been cleared.");
                }
                else
                {
                    target.visualKeyOverride = sArgs[1].ToLower();
                    chr.WriteToDisplay(target.GetNameForActionResult() + "'s visual key override set to " + sArgs[1].ToLower() + ".");
                }
            }
            else if(PC.GetOnline(sArgs[0]) is PC pc)
            {
                if (sArgs[1].ToLower() == "clear")
                {
                    pc.visualKeyOverride = "";
                    chr.WriteToDisplay(pc.GetNameForActionResult() + "'s visual key override has been cleared.");
                }
                else
                {
                    pc.visualKeyOverride = sArgs[1].ToLower();
                    chr.WriteToDisplay(pc.GetNameForActionResult() + "'s visual key override set to " + sArgs[1].ToLower() + ".");
                }
            }

            return true;
        }
    }
}
