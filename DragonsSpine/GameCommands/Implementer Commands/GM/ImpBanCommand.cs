using System;

namespace DragonsSpine.Commands
{
    [CommandAttribute("impban", "Ban a player account from the server for a number of days.", (int)Globals.eImpLevel.GM, new string[] { },
        0, new string[] { "impban <player name> <# of days> (-1 for a permanent ban)" }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class ImpBanCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Who do you want to ban?");
                return false;
            }

            try
            {
                string[] sArgs = args.Split(" ".ToCharArray());

                if (sArgs.Length < 2)
                {
                    chr.WriteToDisplay("Format: impban <name> <# of days>");
                    return false;
                }

                int playerID = PC.GetPlayerID(sArgs[0]);

                if (playerID == -1)
                {
                    chr.WriteToDisplay("Unable to find player named " + sArgs[0] + " in the database.");
                    return true;
                }

                if (!int.TryParse(sArgs[1], out int days) || days == 0)
                {
                    chr.WriteToDisplay("Invalid number of days argument.");
                    return true;
                }

                PC pc = PC.GetPC(playerID);

                if(pc.ImpLevel >= (chr as PC).ImpLevel)
                {
                    chr.WriteToDisplay("You cannot ban a greater or equal implementor level.");
                    return true;
                }

                Account.SaveAccountField(pc.Account.accountID, "banLength", pc.Account.accountID, "Account " + pc.Account.accountName + " banned for " + days + " days by " + chr.GetLogString());

                chr.WriteToDisplay("You have banned the account of " + pc.Account.accountName + " for " + days + " days.");

                pc = PC.GetOnline(playerID);

                if (pc != null)
                {
                    //impboot(pc2.Name);
                    // boot the player
                }

                return true;

            }
            catch (Exception e)
            {
                chr.WriteToDisplay("Format: impban <name> <# of days>");
                Utils.LogException(e);
            }

            return false;
        }
    }
}
