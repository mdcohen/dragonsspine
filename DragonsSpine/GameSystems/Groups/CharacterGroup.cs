using System;
using System.Collections.Generic;
using DragonsSpine.GameWorld;
using TargetAcquisition = DragonsSpine.GameSystems.Targeting.TargetAquisition;

namespace DragonsSpine
{
    /// <summary>
    /// A Group consists of 2 or more players or creatures that work together in the World.
    /// Player groups share experience if within view of a group member's kill, and can chat as a group using the group chat command.
    /// Creature groups move together and attack the same targets.
    /// </summary>
    public class CharacterGroup
    {
        public const int MAX_GROUP_SIZE = 4; // maximum amount of players that may be in a group
        public const int LEVEL_DIFFERENCE_LIMIT = 5; // maximum average level difference of a prospective invite
        public static Dictionary<int, CharacterGroup> playerGroupDictionary = new Dictionary<int, CharacterGroup>();
        public static Dictionary<int, CharacterGroup> creatureGroupDictionary = new Dictionary<int, CharacterGroup>();
        public enum GroupType { Player, NPC }
        #region Private Data
        int m_groupID; // the identifier ID of this Group object
        GroupType groupType; // player or creature group
        int groupLeaderID; // the leader of this group (playerID or worldNPCID)
        List<int> groupMemberIDList; // list of group member IDs (player IDs or worldNPCIDs)
        DateTime groupCreationTime; // the date and time this group was created
        List<NPC> groupNPCList;
        int initialSize;
        List<Tuple<int, int>> groupMemberWarmedSpells; // tuple1 = spell ID, tuple2 = target ID
        #endregion
        public int GroupLeaderID
        {
            get { return this.groupLeaderID; }
            set { this.groupLeaderID = value; }
        }
        public int InitialSize
        {
            get { return this.initialSize; }
        }
        public List<int> GroupMemberIDList
        {
            get { return this.groupMemberIDList; }
        }
        public List<NPC> GroupNPCList
        {
            get { return this.groupNPCList; }
        }
        public List<Tuple<int, int>> GroupMemberWarmedSpells
        {
            get { return this.groupMemberWarmedSpells; }
        }
        public int HighestLevel
        {
            get
            {
                int highestLevel = 0;
                for (int a = 0; a < this.groupMemberIDList.Count; a++)
                {
                    PC groupMember = PC.GetOnline((int)this.groupMemberIDList[a]);
                    if (groupMember != null && groupMember.Level > highestLevel)
                    {
                        highestLevel = groupMember.Level;
                    }
                }
                return highestLevel;
            }
        }
        public CharacterGroup(int groupLeaderID)
        {
            this.groupLeaderID = groupLeaderID;
            //m_pauseToBuff = false;
        }
        private CharacterGroup(GroupType groupType, int groupLeaderID, int firstMemberID) // constructor to create a group of players
        {
            this.groupType = groupType; // set the type of group
            this.groupLeaderID = groupLeaderID; // set the group owner ID
            this.groupMemberIDList = new List<int>
            {
                groupLeaderID, // add the group owner
                firstMemberID // add the first member
            }; // initialize the group member ID list
            this.groupCreationTime = DateTime.Now; // the date and time this group was created
            if (groupType == GroupType.NPC)
            {
                for (int a = 0; a < 25600; a++)
                {
                    if (!CharacterGroup.creatureGroupDictionary.ContainsKey(a))
                    {
                        this.m_groupID = a;
                        break;
                    }
                }
                this.groupNPCList = new List<NPC>();
                creatureGroupDictionary.Add(this.m_groupID, this);
            }
            else if (groupType == GroupType.Player)
            {
                for (int a = 0; a < 25600; a++)
                {
                    if (!CharacterGroup.playerGroupDictionary.ContainsKey(a))
                    {
                        this.m_groupID = a;
                        break;
                    }
                }
                playerGroupDictionary.Add(this.m_groupID, this);
            }
        }
        #region Static Methods
        public static void CreatePlayerGroup(int groupLeaderID, int firstMemberID) // creates a group of players
        {
            CharacterGroup group = new CharacterGroup(GroupType.Player, groupLeaderID, firstMemberID);
            group.SendGroupMessage("You have formed a group.");
            PC pc = PC.GetOnline(groupLeaderID);
            if (pc != null)
                pc.Group = group;
            pc = PC.GetOnline(firstMemberID);
            if (pc != null)
                pc.Group = group;
        }
        public static void CreateCreatureGroup(NPC npc1, NPC npc2, int initialSize)
        {
            CharacterGroup group = new CharacterGroup(GroupType.NPC, npc1.UniqueID, npc2.UniqueID)
            {
                initialSize = initialSize
            };
            group.groupNPCList.Add(npc1);
            group.groupNPCList.Add(npc2);
            npc1.Group = group;
            npc2.Group = group;
            if (npc1.IsSpellUser || npc2.IsSpellUser)
                group.groupMemberWarmedSpells = new List<Tuple<int, int>>();
        }
        public static bool AcceptPlayerGroupInvite(Character ch, int groupLeaderID)
        {
            CharacterGroup group = GetPlayerGroupByGroupLeaderID(groupLeaderID);
            if (group != null)
            {
                if (group.GroupMemberIDList.Count >= MAX_GROUP_SIZE)
                {
                    ch.WriteToDisplay("The group is full.");
                    return true;
                }
                else
                {
                    group.GroupMemberIDList.Add(ch.UniqueID);
                    group.SendGroupMessage(ch.Name + " has joined the group.", ch.UniqueID);
                    ch.Group = group;
                    ch.WriteToDisplay("You have joined the group.");
                    return true;
                }
            }
            return false;
        }
        public static CharacterGroup GetPlayerGroupByGroupLeaderID(int groupLeaderID)
        {
            foreach (CharacterGroup group in playerGroupDictionary.Values)
            {
                if (group.GroupLeaderID == groupLeaderID)
                {
                    return group;
                }
            }
            return null;
        }
        public static bool IsInviteEligible(string playerName, Character inviter)
        {
            PC pc = PC.GetOnline(playerName);
            if (pc == null)
            {
                inviter.WriteToDisplay("The player \"" + playerName + "\" is not online.");
                return false;
            }
            if (Array.IndexOf(pc.ignoreList, inviter.UniqueID) != -1)
            {
                inviter.WriteToDisplay(pc.Name + " has placed you on their ignore list.");
                return false;
            }
            if (pc.LandID != inviter.LandID)
            {
                inviter.WriteToDisplay(pc.Name + " is hunting in a different Land.");
                return false;
            }
            if (pc.Group != null)
            {
                inviter.WriteToDisplay(pc.Name + " is already a member of a group.");
                return false;
            }
            if (pc.GroupInviter != -1 && pc.GroupInviter != inviter.UniqueID)
            {
                inviter.WriteToDisplay(pc.Name + " has already been invited to a group. They must decline or accept their current invitation.");
                pc.WriteToDisplay(inviter.Name + " invites you to join a group but there is another group awaiting your invite decision.");
                return false;
            }
            if (pc.GroupInviter != -1 && pc.GroupInviter == inviter.UniqueID)
            {
                inviter.WriteToDisplay("You have already invited " + pc.Name + " to join your group.");
                return false;
            }
            if (inviter.Group != null && inviter.Group.GroupMemberIDList.Count >= MAX_GROUP_SIZE)
            {
                inviter.WriteToDisplay("Your group is full.");
                return false;
            }
            int highestLevel = inviter.Level;
            int lowestLevel = inviter.Level;
            if (inviter.Group != null)
            {
                foreach (int playerID in inviter.Group.GroupMemberIDList)
                {
                    PC member = PC.GetOnline(playerID);
                    if (member != null)
                    {
                        highestLevel = Math.Max(member.Level, highestLevel);
                        lowestLevel = Math.Min(member.Level, lowestLevel);
                    }
                }
            }
            if (highestLevel > pc.Level + LEVEL_DIFFERENCE_LIMIT)
            {
                inviter.WriteToDisplay(pc.Name + " is too low in experience level to join your group.");
                pc.WriteToDisplay("You are too low in experience level to join " + inviter.Name + "'s group.");
                return false;
            }
            if (lowestLevel < pc.Level - LEVEL_DIFFERENCE_LIMIT)
            {
                inviter.WriteToDisplay(pc.Name + " is too high in experience level to join your group.");
                pc.WriteToDisplay("You are too high in experience level to join " + inviter.Name + "'s group.");
                return false;
            }
            return true;
        }
        public static bool IsInviteEligible(NPC npc, NPC inviter)
        {
            return true;
        }
        #endregion
        #region Public Methods
        public void Add(NPC npc)
        {
            this.GroupMemberIDList.Add(npc.UniqueID);
            this.GroupNPCList.Add(npc);
            npc.Group = this;
        }
        public void Remove(NPC npc)
        {
            this.GroupMemberIDList.Remove(npc.UniqueID);
            this.GroupNPCList.Remove(npc);
            // removing group leader from the group, assign a new group leader
            if (this.GroupLeaderID == npc.UniqueID && this.groupMemberIDList.Count > 1)
            {
                this.GroupMemberIDList.Sort();
                this.groupLeaderID = this.groupMemberIDList[0]; // assign a new group leader
            }
            else if (this.GroupMemberIDList.Count <= 1)
            {
                this.DisbandCreatureGroup();
            }
            if (npc.Group != null)
            {
                npc.Group = null;
            }
        }
        public void DisbandCreatureGroup()
        {
            foreach (NPC npc in this.GroupNPCList)
            {
                npc.Group = null;
            }
            if (CharacterGroup.creatureGroupDictionary.ContainsKey(this.m_groupID))
            {
                CharacterGroup.creatureGroupDictionary.Remove(this.m_groupID);
            }
        }
        public void DisbandPlayerGroup()
        {
            this.SendGroupMessage("Your group has been disbanded.");
            foreach (int groupMemberID in groupMemberIDList)
            {
                PC pc = PC.GetOnline(groupMemberID);
                if (pc != null)
                {
                    pc.Group = null;
                }
            }
            if (CharacterGroup.playerGroupDictionary.ContainsKey(this.m_groupID))
            {
                CharacterGroup.playerGroupDictionary.Remove(this.m_groupID);
            }
        }
        public void DisbandPlayerGroupMember(int groupMemberID, bool displayDisbandMessage)
        {
            this.groupMemberIDList.Remove(groupMemberID);
            PC pc = null;
            if (displayDisbandMessage)
            {
                pc = PC.GetOnline(groupMemberID);
                if (pc != null)
                {
                    pc.WriteToDisplay("You have left your group.");
                    pc.Group = null;
                    pc = null;
                }
            }
            if (this.groupMemberIDList.Count < 2)
            {
                this.DisbandPlayerGroup();
                return;
            }
            if (groupMemberID == this.groupLeaderID)
            {
                this.groupLeaderID = (int)this.groupMemberIDList[0];
                pc = PC.GetOnline(this.groupLeaderID);
                if (pc != null)
                {
                    this.SendGroupMessage(pc.Name + " is now the leader of your group.", this.groupLeaderID);
                    pc.WriteToDisplay("You are now the leader of your group.");
                }
            }
        }
        public void SendGroupMessage(string message)
        {
            for (int a = 0; a < this.groupMemberIDList.Count; a++)
            {
                PC pc = PC.GetOnline((int)groupMemberIDList[a]);
                if (pc != null)
                {
                    pc.WriteToDisplay(message);
                }
            }
        }
        public void SendGroupMessage(string message, int groupMemberIDToExclude)
        {
            for (int a = 0; a < this.groupMemberIDList.Count; a++)
            {
                PC pc = PC.GetOnline((int)groupMemberIDList[a]);
                if (pc != null && pc.UniqueID != groupMemberIDToExclude)
                {
                    if (pc.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                    {
                        pc.WriteToDisplay("(Group) " + message);
                    }
                    else
                    {
                        pc.WriteToDisplay("(Group) " + message);
                    }
                }
            }
        }
        public void GiveGroupFlag(Character expEarner, string flag, string reason)
        {
            if (this.GroupMemberIDList != null)
            {
                for (int a = 0; a < this.GroupMemberIDList.Count; a++)
                {
                    Character groupMember = PC.GetOnline((int)groupMemberIDList[a]);
                    if (groupMember != null)
                    {
                        if (TargetAcquisition.FindTargetInView(expEarner, groupMember.Name, true, true) != null)
                        {
                            string[] s = flag.Split(ProtocolYuusha.VSPLIT.ToCharArray());
                            int questID = Convert.ToInt32(s[0]);
                            if (questID <= 0 || groupMember.GetQuest(questID) != null)
                            {
                                if (!groupMember.QuestFlags.Contains(flag))
                                {
                                    groupMember.QuestFlags.Add(flag);
                                    groupMember.WriteToDisplay("You have received a quest flag!");
                                }
                            }
                        }
                    }
                }
            }
        }
        public void GiveGroupExperience(Character expEarner, long expGained, string reason)
        {
            if (this.groupMemberIDList != null)
            {
                int membersInView = 0;
                for (int a = 0; a < this.groupMemberIDList.Count; a++)
                {
                    if (TargetAcquisition.FindTargetInView(expEarner, PC.GetOnline((int)groupMemberIDList[a]).Name, true, true) != null)
                    {
                        membersInView++;
                    }
                }
                for (int a = 0; a < this.groupMemberIDList.Count; a++)
                {
                    Character groupMember = PC.GetOnline((int)groupMemberIDList[a]);
                    if (Math.Abs(this.HighestLevel - groupMember.Level) <= LEVEL_DIFFERENCE_LIMIT)
                    {
                        if (TargetAcquisition.FindTargetInView(expEarner, groupMember.Name, true, true) != null)
                        {
                            groupMember.Experience += Convert.ToInt64(expGained / membersInView);
                        }
                    }
                }
            }
            else
            {
                expEarner.Experience += expGained;
            }
        }
        public void TeleportGroup(Character teleporter, Cell cell, string reason)
        {
            // cellLock can be used in conjunction with this, check the cell.cellLock
            try
            {
                if (this.GroupMemberIDList != null)
                {
                    for (int a = 0; a < this.GroupMemberIDList.Count; a++)
                    {
                        Character groupMember = TargetAcquisition.FindTargetInView(teleporter, PC.GetOnline((int)GroupMemberIDList[a]).Name, true, true);
                        if (groupMember != null)
                        {
                            if (cell.cellLock == null || !Cell.PassesCellLock(groupMember, cell.cellLock, true))
                            {
                                groupMember.CurrentCell = cell;
                                if (reason != null && reason != "")
                                {
                                    groupMember.WriteToDisplay(reason);
                                }
                            }
                            else
                            {
                                if (cell.cellLock != null && !Cell.PassesCellLock(groupMember, cell.cellLock, false))
                                {
                                    if (cell.cellLock.lockFailureString != "")
                                    {
                                        groupMember.WriteToDisplay(cell.cellLock.lockFailureString);
                                    }
                                }
                            }
                        }
                        else
                        {
                            PC pc = PC.GetOnline((int)GroupMemberIDList[a]);
                            if (pc != null)
                            {
                                this.SendGroupMessage(pc.Name + " was not in view of " + teleporter.Name + " and failed to teleport with the group.", pc.UniqueID);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }
        public bool GroupMemberWarmedSpell(int spellID, int targetID)
        {
            return this.groupMemberWarmedSpells.Contains(Tuple.Create(spellID, targetID));
        }
        #endregion
        #region Public Functions
        public string InvitePlayer(string playerName)
        {
            PC player = PC.GetOnline(playerName);
            player.WriteToDisplay(PC.GetName(this.groupLeaderID) + " invites you to join a group.");
            player.GroupInviter = this.groupLeaderID;
            return "You have invited " + player.Name + " to join your group.";
        }
        public bool IsGroupMember(string playerName)
        {
            for (int a = 0; a < groupMemberIDList.Count; a++)
            {
                if (playerName.ToLower() == PC.GetOnline((int)groupMemberIDList[a]).Name.ToLower())
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}