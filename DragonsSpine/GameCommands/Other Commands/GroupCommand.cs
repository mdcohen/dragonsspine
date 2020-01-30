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
    [CommandAttribute("group", "Create or join a player group.", (int)Globals.eImpLevel.USER, 0, new string[] { "There are no arguments for the show belt command." }, Globals.ePlayerState.PLAYING)]
    public class GroupCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null || args == "")
            {
                chr.WriteToDisplay("Group Commands (Format: group <command> <argument>)");
                chr.WriteToDisplay("group accept | join - Accepts an invitiation to join a group.");
                chr.WriteToDisplay("group invite <name> - Invites <name> to join your group or creates a group.");
                chr.WriteToDisplay("group decline | reject - Decline an invitation to join a group.");
                chr.WriteToDisplay("group status - View current group member information.");
                chr.WriteToDisplay("group say <message> - Send a message to all group members.");
                chr.WriteToDisplay("group disband - Leave your current group.");
                chr.WriteToDisplay("group follow | fol <name> - Follow a visible group member.");
                chr.WriteToDisplay("group unfollow | unfol - Stop following a group member.");
                return true;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            switch (sArgs[0])
            {
                case "accept":
                case "join":
                    #region accept
                    if (chr.Group == null)
                    {
                        if (chr.GroupInviter >= 0)
                        {
                            if (!CharacterGroup.AcceptPlayerGroupInvite(chr, chr.GroupInviter))
                            {
                                CharacterGroup.CreatePlayerGroup(chr.GroupInviter, chr.UniqueID);
                            }
                        }
                        else
                        {
                            chr.WriteToDisplay("You have not been invited to a group.");
                        }
                    }
                    else
                    {
                        chr.WriteToDisplay("You are already a member of a group.");
                    }
                    break;
                    #endregion
                case "invite":
                    #region invite
                    if (chr.Group == null)
                    {
                        if (sArgs.Length < 2)
                        {
                            chr.WriteToDisplay("Format: group invite <player name>");
                            return true;
                        }

                        if (CharacterGroup.IsInviteEligible(sArgs[1], chr))
                        {
                            chr.Group = new CharacterGroup(chr.UniqueID);
                            chr.WriteToDisplay(chr.Group.InvitePlayer(sArgs[1]));
                        }
                    }
                    else
                    {
                        if (sArgs.Length < 2)
                        {
                            chr.WriteToDisplay("Format: group invite <player name>");
                            return true;
                        }

                        if (CharacterGroup.IsInviteEligible(sArgs[1], chr))
                        {
                            chr.WriteToDisplay(chr.Group.InvitePlayer(sArgs[1]));
                        }
                    }
                    break;
                    #endregion
                case "decline":
                case "reject":
                    #region decline | reject
                    if (chr.Group == null)
                    {
                        if (chr.GroupInviter >= 0)
                        {
                            CharacterGroup group = CharacterGroup.GetPlayerGroupByGroupLeaderID(chr.GroupInviter);
                            if (group != null)
                            {
                                PC pc = PC.GetOnline(chr.GroupInviter);
                                if (pc != null)
                                {
                                    pc.WriteToDisplay(chr.Name + " declines your group invite.");
                                    chr.WriteToDisplay("You decline the invite into " + pc.Name + "'s group.");
                                    if (pc.Group.GroupMemberIDList == null)
                                    {
                                        pc.Group = null;
                                    }
                                }
                            }
                            chr.GroupInviter = -1;
                        }
                    }
                    else
                    {
                        chr.WriteToDisplay("You are already the member of a group. Use \"group disband\" to leave the group.");
                    }
                    break;
                    #endregion
                case "disband":
                    #region disband
                    if (chr.Group != null)
                    {
                        chr.Group.DisbandPlayerGroupMember(chr.UniqueID, true);
                    }
                    else
                    {
                        if (chr.GroupInviter >= 0)
                        {
                            return new CommandTasker(chr)["group", "decline"];
                        }
                        else
                        {
                            chr.WriteToDisplay("You are not a member of a group.");
                        }
                    }
                    break;
                    #endregion
                case "follow":
                case "fol":
                    #region follow | fol
                    if (chr.Group == null)
                    {
                        chr.WriteToDisplay("You are not in a group.");
                        break;
                    }
                    if (sArgs.Length > 1)
                    {
                        Character leader = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, sArgs[1], false, true);
                        if (leader != null && leader.IsPC && leader.Group == chr.Group)
                        {
                            chr.FollowID = leader.UniqueID;
                            chr.WriteToDisplay("You follow " + leader.Name + ".");
                            leader.WriteToDisplay(chr.Name + " is now following you.");
                        }
                        else
                        {
                            chr.WriteToDisplay(sArgs[1] + " is an invalid follow target.");
                        }
                    }
                    else
                    {
                        chr.WriteToDisplay("Format: group follow <name> - where <name> is a player in your group");
                    }
                    break;
                    #endregion
                case "unfollow":
                case "unfol":
                    if (chr.Group == null)
                    {
                        chr.WriteToDisplay("You are not in a group.");
                        break;
                    }
                    else if (chr.FollowID != 0)
                    {
                        chr.BreakFollowMode();
                        break;
                    }
                    else
                    {
                        chr.WriteToDisplay("You are not following anyone.");
                    }
                    break;
                case "status":
                case "who":
                    #region status | who
                    if (chr.Group == null)
                    {
                        chr.WriteToDisplay("You are not in a group.");
                        break;
                    }
                    if (chr.Group != null && chr.Group.GroupMemberIDList == null)
                    {
                        chr.WriteToDisplay("Your group is not formed yet.");
                        break;
                    }
                    chr.WriteToDisplay("Group Status");
                    string groupMemberInfo = "";
                    foreach (int id in chr.Group.GroupMemberIDList)
                    {
                        PC pc = PC.GetOnline(id);
                        if (pc != null)
                        {
                            groupMemberInfo = pc.Name;
                            groupMemberInfo = groupMemberInfo.PadRight(15);
                            groupMemberInfo += "[" + pc.Level + "] " + pc.classFullName;
                            groupMemberInfo = groupMemberInfo.PadRight(32);
                            groupMemberInfo += "H: " + pc.Hits + "/" + pc.HitsFull + " S: " + pc.Stamina + "/" + pc.StaminaFull;
                            if (pc.IsSpellUser)
                            {
                                groupMemberInfo += " M: " + pc.Mana + "/" + pc.ManaFull;
                            }
                            if (pc.Group.GroupLeaderID == pc.UniqueID)
                            {
                                groupMemberInfo += " (Leader)";
                            }
                            chr.WriteToDisplay(groupMemberInfo);
                        }
                    }
                    break;
                    #endregion
                case "say":
                    #region group chat
                    if (chr.Group != null)
                    {
                        string groupChat = "";
                        for (int a = 0; a < sArgs.Length; a++)
                        {
                            groupChat += sArgs[a] + " ";
                        }
                        groupChat = groupChat.Substring(0, groupChat.Length - 1);
                        chr.Group.SendGroupMessage(chr.Name + ": " + groupChat);
                    }
                    else
                    {
                        chr.WriteToDisplay("You are not in a group.");
                    }
                    break;
                    #endregion
                default:
                    chr.WriteToDisplay("Invalid group command. Type group for a list of commands.");
                    break;
            }

            return true;
        }
    }
}
