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
    [CommandAttribute("showresists", "Display character resistances.", (int)Globals.eImpLevel.USER, new string[] { "show resists", "show resist", "resists", "show resistance" },
        0, new string[] { "There are no arguments for the show resists command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowResistsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            chr.WriteToDisplay("Resists:");
            chr.WriteToDisplay("Acid      : " + chr.AcidResistance);
            chr.WriteToDisplay("Fire      : " + chr.FireResistance);
            chr.WriteToDisplay("Cold      : " + chr.ColdResistance);
            chr.WriteToDisplay("Death     : " + chr.DeathResistance);
            chr.WriteToDisplay("Blind     : " + chr.BlindResistance);
            chr.WriteToDisplay("Fear      : " + chr.FearResistance);
            chr.WriteToDisplay("Stun      : " + chr.StunResistance);
            chr.WriteToDisplay("Poison    : " + chr.PoisonResistance);
            chr.WriteToDisplay("Lightning : " + chr.LightningResistance);
            chr.WriteToDisplay("Zonk      : " + chr.ZonkResistance);

            return true;
        }
    }
}
