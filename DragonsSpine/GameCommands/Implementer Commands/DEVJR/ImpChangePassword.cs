#region 
/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
#endregion
using System;

namespace DragonsSpine.Commands
{
    [CommandAttribute("impchangepassword", "Display all commands.", (int)Globals.eImpLevel.DEVJR, new string[] { "impchangepswd" },
        0, new string[] { "impchangepassword <account> <new password>" }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ImpChangePasswordCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (String.IsNullOrEmpty(args))
            {
                chr.WriteToDisplay("You must provide a valid account name and new password as arguments. impchangepassword <account> <new password>");
                return false;
            }
            else
            {
                string[] sArgs = args.Split(" ".ToCharArray());

                if (sArgs.Length != 2)
                {
                    chr.WriteToDisplay("Invalid arguments.");
                    return true;
                }

                string accountName = sArgs[0];

                if (!DAL.DBAccount.AccountExists(accountName))
                {
                    chr.WriteToDisplay("The account " + accountName + " does not exist.");
                    return true;
                }

                if (sArgs[1].Length < Account.PASSWORD_MIN_LENGTH || sArgs[1].Length > Account.PASSWORD_MAX_LENGTH)
                {
                    chr.WriteToDisplay("The password was invalid.");
                    return true;
                }

                Account.SetPassword(Account.GetAccountID(accountName), sArgs[1]);

                chr.WriteToDisplay("Account " + accountName + " password set to " + sArgs[1]);
            }

            return true;
        }
    }
}
