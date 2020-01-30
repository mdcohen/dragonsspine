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
using System.Linq;
using System.Text;

namespace DragonsSpine.Commands
{
    [CommandAttribute("impgetloottable", "Display NPC's loot table.", (int)Globals.eImpLevel.DEVJR, new string[] { "impgetloottable", "getlt", "getloottable" },
        0, new string[] { "impgetloottable <target>" }, Globals.ePlayerState.PLAYING)]
    public class ImpGetLootTableCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null || args == "")
            {
                chr.WriteToDisplay("Usage: impgetloottable <target>");
                return false;
            }

            var target = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, args, false, true);

            if (target == null)
            {
                chr.WriteToDisplay("Target not found: " + args);
                return false;
            }

            if(!(target is NPC))
            {
                chr.WriteToDisplay("Target is not an NPC.");
                return false;
            }

            try
            {
                chr.WriteToDisplay(Autonomy.ItemBuilding.LootManager.GetLootTable(target as NPC, target.Map.ZPlanes[target.Z]).ToString());
            }
            catch(Exception e)
            {
                Utils.LogException(e);
                chr.WriteToDisplay("There was an exception.");
                return false;
            }

            return true;
        }
    }
}
