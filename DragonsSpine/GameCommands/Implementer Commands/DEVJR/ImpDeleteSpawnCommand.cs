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
    [CommandAttribute("impdeletespawn", "Delete a SpawnZone of a target in view.", (int)Globals.eImpLevel.DEVJR, new string[] { "impdelspawn" },
        0, new string[] { "impdelspawn <target in view>" }, Globals.ePlayerState.PLAYING)]
    public class ImpDeleteSpawnCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null || args == "")
            {
                chr.WriteToDisplay("Usage: impdelspawn <target in view>");
                return false;
            }

            Character target = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, args, false, true);

            if (target == null)
            {
                chr.WriteToDisplay("Target not found: " + args);
                return false;
            }

            if (!(target is NPC))
            {
                chr.WriteToDisplay("Target is not an NPC.");
                return false;
            }

            int result = DAL.DBEditor.DeleteSpawnZone((target as NPC).SpawnZoneID);

            if (result == -1)
            {
                chr.WriteToDisplay("SpawnZoneID " + (target as NPC).SpawnZoneID + " was NOT deleted.");
            }
            else chr.WriteToDisplay("SpawnZoneID " + (target as NPC).SpawnZoneID + " deleted.");

            return true;
        }
    }
}
