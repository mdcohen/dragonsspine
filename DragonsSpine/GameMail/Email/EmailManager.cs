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
using System.Collections.Generic;
using System.Net.Mail;

namespace DragonsSpine.GameSystems.Mail.Email
{
    public static class EmailManager
    {
        public static MailMessage CreateNewEmailMessage(string from, string to, string subject, string body)
        {
            MailMessage message = new MailMessage(from, to, subject, body);

            return message;
        }

        public static void SendEmailMessage(MailMessage message)
        {
            SmtpClient client = new SmtpClient();
        }
    }
}
