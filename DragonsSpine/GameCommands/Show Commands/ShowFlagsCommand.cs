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
    [CommandAttribute("showflags", "Display current armor class information.", (int)Globals.eImpLevel.USER, new string[] { "show flags", "show flag"},
        0, new string[] { "There are no arguments for the show flags command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowFlagsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            // Content Flags
            chr.WriteToDisplay("Content Flags: " + chr.ContentFlags.Count);
            foreach(string flag in chr.ContentFlags)
                chr.WriteToDisplay(flag);
            // Players flagged as hostile
            chr.WriteToDisplay("Players Flagged: " + chr.FlaggedUniqueIDs.Count);
            foreach (int flag in chr.FlaggedUniqueIDs)
                chr.WriteToDisplay((string)DAL.DBPlayer.GetPlayerField(flag, "name", Type.GetType("System.String")));
            // Quest Flags
            chr.WriteToDisplay("Quest Flags: " + chr.QuestFlags.Count);
            foreach (string flag in chr.QuestFlags)
                chr.WriteToDisplay(flag);
            // NPCs with chr flagged as hostile
            chr.WriteToDisplay("NPCs with you flagged as hostile...");
            foreach (NPC npc in Character.NPCInGameWorld)
            {
                if (npc.FlaggedUniqueIDs.Contains(chr.UniqueID))
                    chr.WriteToDisplay(npc.Name + " in " + npc.Map.Name + ", " + npc.Map.ZPlanes[npc.Z]);
            }
            return true;
        }
    }
}
