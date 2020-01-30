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
    [CommandAttribute("impgotonpc", "Go to a specific NPC using their name or unique ID.", (int)Globals.eImpLevel.DEVJR, new string[] { "gnpc" },
        0, new string[] { "impgotonpc <name>", "impgotonpc <id>", "impgotonpc <name> <profession>" }, Globals.ePlayerState.PLAYING)]
    public class ImpGotoNPCCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("impgotonpc <name> or impgotonpc <id>");
                return false;
            }
            else
            {
                chr.BreakFollowMode();

                string[] sArgs = args.Split(" ".ToCharArray());

                if (sArgs.Length == 1)
                {
                    if (sArgs[0].ToLower() != "adventurer")
                    {
                        Character.ClassType baseProfession = Character.ClassType.None;

                        #region Base Profession
                        if (Enum.TryParse<Character.ClassType>(sArgs[0], true, out baseProfession))
                        {
                            foreach (NPC npc in Character.NPCInGameWorld)
                            {
                                if (npc.MapID == chr.MapID && npc.BaseProfession == baseProfession)
                                {
                                    chr.CurrentCell = npc.CurrentCell;
                                    chr.WriteToDisplay("You have teleported to " + npc.GetLogString() + ".");
                                    return true;
                                }
                            }

                            foreach (NPC npc in Character.NPCInGameWorld)
                            {
                                if (npc.BaseProfession == baseProfession)
                                {
                                    chr.CurrentCell = npc.CurrentCell;
                                    chr.WriteToDisplay("You have teleported to " + npc.GetLogString() + ".");
                                    return true;
                                }
                            }
                        } 
                        #endregion

                        int id = -1;

                        #region NPC Unique ID or NPCID.
                        // check if the argument provided is an integer, then match up an ID -- if no match, return false
                        if (Int32.TryParse(sArgs[0], out id))
                        {
                            foreach (NPC npc in Character.NPCInGameWorld)
                            {
                                if (id < 0 ? npc.UniqueID == id : npc.npcID == id)
                                {
                                    chr.CurrentCell = npc.CurrentCell;
                                    chr.WriteToDisplay("You have teleported to " + npc.GetLogString() + ".");
                                    return true;
                                }
                            }

                            chr.WriteToDisplay("Could not find NPC matching ID number: " + args + ".");
                            return false;
                        } 
                        #endregion

                        #region NPC name.
                        // check current Map first
                        foreach (Character npc in Character.NPCInGameWorld)
                        {
                            if (npc.Name.ToLower() == args.ToLower() && npc.MapID == chr.MapID)
                            {
                                chr.CurrentCell = npc.CurrentCell;
                                chr.WriteToDisplay("You have teleported to " + npc.GetLogString() + ".");
                                return true;
                            }

                        }

                        // if not npc with name is found on current map, check entire world
                        foreach (Character npc in Character.NPCInGameWorld)
                        {
                            if (npc.Name.ToLower() == args.ToLower())
                            {
                                chr.CurrentCell = npc.CurrentCell;
                                chr.WriteToDisplay("You have teleported to " + npc.GetLogString() + ".");
                                return true;
                            }
                        } 
                        #endregion
                    }
                    else
                    {
                        foreach (Character npc in Character.NPCInGameWorld)
                        {
                            if (npc is Adventurer && npc.MapID == chr.MapID)
                            {
                                chr.CurrentCell = npc.CurrentCell;
                                chr.WriteToDisplay("You have teleported to the Adventurer " + npc.GetLogString() + ".");
                                return true;
                            }
                        }

                        foreach (Character npc in Character.NPCInGameWorld)
                        {
                            if (npc is Adventurer)
                            {
                                chr.CurrentCell = npc.CurrentCell;
                                chr.WriteToDisplay("You have teleported to the Adventurer " + npc.GetLogString() + ".");
                                return true;
                            }
                        }
                    }
                }
                else if (sArgs.Length == 2) // impgotonpc <npc name> <npc class>
                {
                    foreach (Character npc in Character.NPCInGameWorld)
                    {
                        if (npc.Name.ToLower() == sArgs[0].ToLower() && npc.BaseProfession.ToString().ToLower() == sArgs[1].ToLower())
                        {
                            chr.CurrentCell = npc.CurrentCell;
                            chr.WriteToDisplay("You have teleported to " + npc.GetLogString() + ".");
                            return true;
                        }

                        if (npc.Name.ToLower() == sArgs[0].ToLower() + " " + sArgs[1].ToLower())
                        {
                            chr.CurrentCell = npc.CurrentCell;
                            chr.WriteToDisplay("You have teleported to " + npc.GetLogString() + ".");
                            return true;
                        }
                    }
                }

                chr.WriteToDisplay("Could not find NPC matching args: " + args + ".");

                return false;
            }
        }
    }
}
