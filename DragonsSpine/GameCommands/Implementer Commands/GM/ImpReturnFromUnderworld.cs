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
    [CommandAttribute("impreturnfromunderworld", "Return a player in view from the Underworld to the realm of the living.", (int)Globals.eImpLevel.GM, new string[] { "imprtnuw" },
        0, new string[] { "imprtnuw" }, Globals.ePlayerState.PLAYING)]
    public class ImpReturnFromUnderworld : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            try
            {
                Character target = GameSystems.Targeting.TargetAquisition.AcquireTarget(chr, args, 3, 3);

                if(target == null)
                {
                    chr.WriteToDisplay("You do not see " + args + " here.");
                    return false;
                }

                if(target.MapID != GameWorld.Map.ID_PRAETOSEBA)
                {
                    chr.WriteToDisplay(target.GetNameForActionResult() + " is not in the Underworld.");
                    return false;
                }

                if (!(target is PC))
                {
                    chr.WriteToDisplay(target.GetNameForActionResult() + " is not a player.");
                    return false;
                }

                chr.WriteToDisplay("You return " + target.Name + " to the realm of the living.");
                Rules.ReturnFromUnderworld(target);
                return true;

            }
            catch
            {
                chr.WriteToDisplay("Format: imprtnuw <player in view>");
            }

            return false;
        }
    }
}
