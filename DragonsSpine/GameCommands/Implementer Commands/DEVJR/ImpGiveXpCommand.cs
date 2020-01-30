using System;

namespace DragonsSpine.Commands
{
    [CommandAttribute("impgivexp", "Give experience to a player.", (int)Globals.eImpLevel.DEVJR, new string[] {},
        0, new string[] { "impgivexp <amount> <target>" }, Globals.ePlayerState.PLAYING)]
    public class ImpGiveXpCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                chr.WriteToDisplay("impgivexp <amount> <target>");
            }
            else
            {
                string[] sArgs = args.Split(" ".ToCharArray());

                int amount = Convert.ToInt32(sArgs[0]);

                Character target = null;

                foreach (PC pc in Character.PCInGameWorld)
                {
                    if (pc.Name.ToLower() == sArgs[1].ToLower())
                        target = pc;
                }

                if (target != null && target.Name != "Nobody")
                {
                    target.Experience += amount;
                }
                else if (target == null)
                {
                    target = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, sArgs[1], false, true);

                    if (target != null)
                        target.Experience += amount;
                    else
                    {
                        chr.WriteToDisplay("Could not find target: " + sArgs[1] + ".");
                    }
                }
            }

            return true;
        }
    }
}
