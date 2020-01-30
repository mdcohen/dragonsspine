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
    [CommandAttribute("crawl", "Crawl in a direction. This avoids being stunned if moving into a solid structure.", (int)Globals.eImpLevel.USER, 2,
        new string[] { "crawl <direction>" }, Globals.ePlayerState.PLAYING)]
    public class CrawlCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.CommandWeight > 3)
            {
                return true;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            if (sArgs[0] == null)
            {
                chr.WriteToDisplay("Crawl where?");
                return true;
            }

            chr.CommandType = CommandTasker.CommandType.Crawl;

            GameWorld.Map.MoveCharacter(chr, sArgs[0], "crawl");

            return true;
        }
    }
}
