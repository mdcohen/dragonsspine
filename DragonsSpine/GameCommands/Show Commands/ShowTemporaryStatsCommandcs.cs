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
    [CommandAttribute("showtempstats", "Display a list of bonuses to stats.", (int)Globals.eImpLevel.USER, new string[] { "show tempstats", "show temporary stats", "show temp stats" },
        0, new string[] { "There are no arguments for the show temporary stats command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowTemporaryStatsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            chr.WriteToDisplay("Temporary Stat Bonuses:");
            chr.WriteToDisplay("Strength     : " + chr.TempStrength);
            chr.WriteToDisplay("Dexterity    : " + chr.TempDexterity);
            chr.WriteToDisplay("Intelligence : " + chr.TempIntelligence);
            chr.WriteToDisplay("Wisdom       : " + chr.TempWisdom);
            chr.WriteToDisplay("Constitution : " + chr.TempConstitution);
            chr.WriteToDisplay("Charisma     : " + chr.TempCharisma);

            return true;
        }
    }
}
