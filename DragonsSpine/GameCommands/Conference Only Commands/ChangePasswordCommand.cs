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
    [CommandAttribute("changepassword", "Change your account password.", (int)Globals.eImpLevel.USER, new string[] { "chngpwd", "password" },
        0, new string[] { "The new password and confirmation of the new password." }, Globals.ePlayerState.CONFERENCE)]
    public class ChangePasswordCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (String.IsNullOrEmpty(args))
            {
                chr.WriteToDisplay("You must provide a new password and a confirmation of the new password.");
                return false;
            }
            else
            {
                string[] sArgs = args.Split(" ".ToCharArray());

                if (sArgs.Length != 2)
                {
                    chr.WriteToDisplay("You must provide a new password and a confirmation of the new password.");
                    return true;
                }

                if (sArgs[0] != sArgs[1])
                {
                    chr.WriteToDisplay("The new password and confirmed new password do not match.");
                    return true;
                }

                if (sArgs[0].Length < Account.PASSWORD_MIN_LENGTH || sArgs[0].Length > Account.PASSWORD_MAX_LENGTH)
                {
                    chr.WriteToDisplay("The password you provided is invalid.");
                    return true;
                }

                Account.SetPassword((chr as PC).Account.accountID, sArgs[0]);

                chr.WriteToDisplay("Your password has been changed.");
            }

            return true;
        }
    }
}