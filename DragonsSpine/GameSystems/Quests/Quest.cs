using System;
using System.Linq;
using System.Collections.Generic;

namespace DragonsSpine
{
    public class GameQuest
    {
        public static Dictionary<int, GameQuest> QuestDictionary = new Dictionary<int, GameQuest>();
        public const string QUEST_INSERT_NOTES_DEFAULT = "Default Quest";

        public const string ASPLIT = " "; // attribute delimiter
        public const string ISPLIT = "^"; // item delimiter
        public const string VSPLIT = "~"; // variable delimiter

        public enum QuestRequirement
        {
            /// <summary>
            /// No quest requirement.
            /// </summary>
            None,
            /// <summary>
            /// Specific amount of coin required to complete quest or step.
            /// </summary>
            Coin,
            /// <summary>
            /// Flag required to complete quest or step.
            /// </summary>
            Flag,
            /// <summary>
            /// Item required to complete quest or step.
            /// </summary>
            Item,
            /// <summary>
            /// NPC required to complete quest or step.
            /// </summary>
            NPC,
            /// <summary>
            /// Karma required to complete quest or step.
            /// </summary>
            Karma,
            /// <summary>
            /// Experience level requirement.
            /// </summary>
            Level
        }

        #region Private Data
        int questID = -1; // the database catalog ID of this quest
        string notes = ""; // notes about this quest (meant for DEVs only)
        string name = ""; // the name of this quest
        string description = ""; // the description of this quest (to be used for quest journals later)
        string completedDescription = ""; // the description of the quest after it has been completed (to be used for quest journals later)
        Dictionary<int, QuestRequirement> requirements = new Dictionary<int, QuestRequirement>(); //
        List<string> requiredFlags = new List<string>(); // the flags required to initiate this quest, if any
        string rewardTitle = ""; // #~classFullName, where # is the step at which the classFullName is received
        string rewardClass = ""; // #~classType, where # is the step at which the classType is received
        Dictionary<int, string> rewardFlags = new Dictionary<int, string>(); // the flags that are rewarded and at which step, "" is nullifier
        Dictionary<int, int> requiredItems = new Dictionary<int, int>(); // step #~item ID
        Dictionary<int, double> coinValues = new Dictionary<int, double>(); // step~required or reward coin value
        Dictionary<int, int> rewardItems = new Dictionary<int, int>(); // step #~item ID
        Dictionary<int, int> rewardExperience = new Dictionary<int, int>(); // step #~experience amount
        Dictionary<int, string> rewardStats = new Dictionary<int, string>(); // step #~stat " " stat increase
        // stat abbreviations: (h)its, (s)tamina, (m)ana, str, con, wis, int, dex, chr
        Dictionary<int, string> rewardTeleports = new Dictionary<int, string>(); // step #~land,map,x,y,z (cell locks can be used in conjunction with this, target cell gets the celllock!)
        Dictionary<string, string> responseStrings = new Dictionary<string, string>(); // initiator string = key, response string = value
        List<string> hintStrings = new List<string>(); // the strings displayed to tip off the player that this is a quest NPC
        Dictionary<string, int> flagStrings = new Dictionary<string, int>(); // string detected, flag given
        Dictionary<string, int> stepStrings = new Dictionary<string, int>(); // string detected, at which step (step is then incremented)
        Dictionary<int, string> finishStrings = new Dictionary<int, string>(); // the string displayed when this quest is completed at finishString[x]
        Dictionary<int, string> failStrings = new Dictionary<int, string>(); // the string displayed when this quest is failed at failString[x]
        List<Character.ClassType> classTypes = new List<Character.ClassType>(); // classType restrictions
        List<Globals.eAlignment> alignments = new List<Globals.eAlignment>(); // alignment restrictions
        bool repeatable = false; // true if this quest can be repeated
        bool stepOrder = false;
        int maximumLevel = 0;
        int minimumLevel = 0;
        Dictionary<int, string> soundFiles = new Dictionary<int, string>();
        int totalSteps = 1;
        bool despawnsNPC = false;
        int masterQuestID = 0;
        bool teleportGroup = false;

        int timesCompleted = 0; // how many times this quest has been completed
        List<int> completedSteps = new List<int>();
        int currentStep = 0; // the player's current step in this quest
        string startDate = "";
        string finishDate = "";
        #endregion

        #region Public Properties

        public int QuestID
        {
            get { return this.questID; }
        }
        public string Notes
        {
            get { return this.notes; }
        }
        public string Name
        {
            get { return this.name; }
        }
        public string Description
        {
            get { return this.description; }
        }
        public string CompletedDescription
        {
            get { return this.completedDescription; }
        }
        public List<string> RequiredFlags
        {
            get { return this.requiredFlags; }
        }
        public Dictionary<int, string> RewardFlags
        {
            get { return this.rewardFlags; }
        }
        public string RewardTitle
        {
            get { return this.rewardTitle; }
        }
        public string RewardClass
        {
            get { return this.rewardClass; }
        }
        public Dictionary<int, int> RequiredItems
        {
            get { return this.requiredItems; }
        }
        public Dictionary<int, double> CoinValues
        {
            get { return this.coinValues; }
        }
        public Dictionary<int, int> RewardItems
        {
            get { return this.rewardItems; }
        }
        public Dictionary<int, int> RewardExperience
        {
            get { return this.rewardExperience; }
        }
        public Dictionary<int, string> RewardStats
        {
            get { return this.rewardStats; }
        }
        public Dictionary<int, string> RewardTeleports
        {
            get { return this.rewardTeleports; }
        }
        public Dictionary<string, string> ResponseStrings
        {
            get { return this.responseStrings; }
        }
        public List<string> HintStrings
        {
            get { return this.hintStrings; }
        }
        public Dictionary<string, int> FlagStrings
        {
            get { return this.flagStrings; }
        }
        public Dictionary<string, int> StepStrings
        {
            get { return this.stepStrings; }
        }
        public Dictionary<int, string> FinishStrings
        {
            get { return this.finishStrings; }
        }
        public Dictionary<int, string> FailStrings
        {
            get { return this.failStrings; }
        }
        public List<Character.ClassType> ClassTypes
        {
            get { return this.classTypes; }
        }
        public bool IsClassRestricted
        {
            get
            {
                if (this.classTypes.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }
        public Dictionary<int, QuestRequirement> Requirements
        {
            get { return this.requirements; }
        }
        public List<Globals.eAlignment> Alignments
        {
            get { return this.alignments; }
        }
        public bool IsAlignmentRestricted
        {
            get
            {
                if (this.alignments.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }
        public int MaximumLevel
        {
            get
            {
                return this.maximumLevel;
            }
        }
        public int MinimumLevel
        {
            get
            {
                return this.minimumLevel;
            }
        }
        public bool IsLevelRestricted
        {
            get
            {
                if (this.minimumLevel > 0 || this.maximumLevel > 0)
                {
                    return true;
                }
                return false;
            }
        }
        public bool IsRepeatable
        {
            get { return this.repeatable; }
        }
        public bool StepOrder
        {
            get { return this.stepOrder; }
        }
        public Dictionary<int, string> SoundFiles
        {
            get
            {
                return this.soundFiles;
            }
        }
        public int TotalSteps
        {
            get
            {
                return this.totalSteps;
            }
        }
        public bool DespawnsNPC
        {
            get { return this.despawnsNPC; }
        }
        public int MasterQuestID
        {
            get { return this.masterQuestID; }
        }
        public bool TeleportGroup
        {
            get { return this.teleportGroup; }
        }

        public bool IsSubquest
        {
            get
            {
                return this.MasterQuestID > 0;
            }
        }

        public int TimesCompleted
        {
            get { return this.timesCompleted; }
            set { this.timesCompleted = value; }
        }
        public List<int> CompletedSteps
        {
            get { return this.completedSteps; }
        }
        public int CurrentStep
        {
            get { return this.currentStep; }
            set { this.currentStep = value; }
        }
        public string StartDate
        {
            get { return this.startDate; }
            set { this.startDate = value; }
        }
        public string FinishDate
        {
            get { return this.finishDate; }
            set { this.finishDate = value; }
        } 
        #endregion

        #region Constructors
        public GameQuest()
        {
        }

        public GameQuest(System.Data.DataRow dr)
        {
            string[] s = null;
            string[] t = null;
            int a = 0;

            this.questID = Convert.ToInt32(dr["questID"]);
            this.notes = dr["notes"].ToString();
            this.name = dr["name"].ToString();
            this.description = dr["description"].ToString();
            this.completedDescription = dr["completedDescription"].ToString();
            s = dr["requirements"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    t = s[a].Split(VSPLIT.ToCharArray());
                    this.requirements.Add(Convert.ToInt32(t[0]), (QuestRequirement)(Globals.eItemBaseType)Enum.Parse(typeof(GameQuest.QuestRequirement), t[1]));
                }
            }

            this.rewardTitle = dr["rewardTitle"].ToString();

            this.rewardClass = dr["rewardClass"].ToString();

            s = dr["requiredFlags"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    this.requiredFlags.Add(s[a]);
                }
            }
            s = dr["rewardFlags"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    t = s[a].Split(VSPLIT.ToCharArray());
                    this.rewardFlags.Add(Convert.ToInt32(t[0]), t[1]);
                }
            }
            s = dr["requiredItems"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    t = s[a].Split(VSPLIT.ToCharArray());
                    this.requiredItems.Add(Convert.ToInt32(t[0]), Convert.ToInt32(t[1]));
                }
            }
            s = dr["coinValues"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    t = s[a].Split(VSPLIT.ToCharArray());
                    this.coinValues.Add(Convert.ToInt32(t[0]), Convert.ToDouble(t[1]));
                }
            }
            s = dr["rewardItems"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    t = s[a].Split(VSPLIT.ToCharArray());
                    this.rewardItems.Add(Convert.ToInt32(t[0]), Convert.ToInt32(t[1]));
                }
            }
            s = dr["rewardExperience"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    t = s[a].Split(VSPLIT.ToCharArray());
                    this.rewardExperience.Add(Convert.ToInt32(t[0]), Convert.ToInt32(t[1]));
                }
            }
            s = dr["rewardStats"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    t = s[a].Split(VSPLIT.ToCharArray());
                    this.rewardStats.Add(Convert.ToInt32(t[0]), t[1]);
                }
            }
            s = dr["responseStrings"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    t = s[a].Split(VSPLIT.ToCharArray());
                    this.responseStrings.Add(t[0], t[1]);
                }
            }
            s = dr["rewardTeleports"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    t = s[a].Split(VSPLIT.ToCharArray());
                    this.rewardTeleports.Add(Convert.ToInt32(t[0]), t[1]);
                }
            }
            s = dr["hintStrings"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    this.hintStrings.Add(s[a]);
                }
            }
            s = dr["flagStrings"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    t = s[a].Split(VSPLIT.ToCharArray());
                    this.flagStrings.Add(t[0], Convert.ToInt32(t[1]));
                }
            }
            s = dr["stepStrings"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    t = s[a].Split(VSPLIT.ToCharArray());
                    this.stepStrings.Add(t[0], Convert.ToInt32(t[1]));
                }
            }
            s = dr["finishStrings"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    t = s[a].Split(VSPLIT.ToCharArray());
                    this.finishStrings.Add(Convert.ToInt32(t[0]), t[1]);
                }
            }
            s = dr["failStrings"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    t = s[a].Split(VSPLIT.ToCharArray());
                    this.failStrings.Add(Convert.ToInt32(t[0]), t[1]);
                }
            }
            s = dr["classTypes"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    this.classTypes.Add((Character.ClassType)Enum.Parse(typeof(Character.ClassType), s[a]));
                }
            }
            s = dr["alignments"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    this.alignments.Add((Globals.eAlignment)Enum.Parse(typeof(Globals.eAlignment), s[a]));
                }
            }
            s = dr["soundFiles"].ToString().Split(ISPLIT.ToCharArray());
            for (a = 0; a < s.Length; a++)
            {
                if (s[a].Length > 0)
                {
                    t = s[a].Split(VSPLIT.ToCharArray());
                    this.soundFiles.Add(Convert.ToInt32(t[0]), t[1]);
                }
            }
            this.maximumLevel = Convert.ToInt32(dr["maximumLevel"]);
            this.minimumLevel = Convert.ToInt32(dr["minimumLevel"]);
            this.repeatable = Convert.ToBoolean(dr["repeatable"]);
            this.stepOrder = Convert.ToBoolean(dr["stepOrder"]);
            this.totalSteps = Convert.ToInt32(dr["totalSteps"]);
            this.despawnsNPC = Convert.ToBoolean(dr["despawnsNPC"]);
            this.masterQuestID = Convert.ToInt32(dr["masterQuestID"]);
            this.teleportGroup = Convert.ToBoolean(dr["teleportGroup"]);
        } 
        #endregion

        public static bool LoadQuests()
        {
            return DAL.DBWorld.LoadQuests();
        }

        public static void Add(GameQuest quest)
        {
            QuestDictionary.Add(quest.QuestID, quest);
        }

        public static GameQuest GetQuest(int questID)
        {
            if (QuestDictionary.ContainsKey(questID))
            {
                return QuestDictionary[questID];
            }
            return null;
        }

        public static GameQuest CopyQuest(int questID)
        {
            if(QuestDictionary.ContainsKey(questID))
            {
                return (GameQuest)QuestDictionary[questID].MemberwiseClone();
            }
            return null;
        }

        public string GetLogString()
        {
            return "[" + this.QuestID + "] " + this.Name;
        }

        public bool PlayerMeetsRequirements(PC player, bool inform)
        {
            if (!this.IsRepeatable)
            {
                GameQuest qCheck = player.GetQuest(this.QuestID);

                if (qCheck != null && qCheck.TimesCompleted >= 1)
                {
                    if (inform)
                    {
                        player.WriteToDisplay("You have already completed the quest \"" + this.Name + "\".");
                    }
                    return false;
                }
            }

            foreach (string reqFlag in this.RequiredFlags)
            {
                if (player.QuestFlags.IndexOf(reqFlag) == -1)
                {
                    if (inform)
                    {
                        player.WriteToDisplay("You do not have the required flags for the quest \"" + this.Name + "\".");
                    }
                    return false;
                }
            }

            if (this.IsAlignmentRestricted)
            {
                if (!this.Alignments.Contains(player.Alignment))
                {
                    if (inform)
                    {
                        player.WriteToDisplay("You do not meet the alignment requirement for the quest \"" + this.Name + "\".");
                    }
                    return false;
                }
            }

            if (this.IsClassRestricted)
            {
                // TODO: search sub classes
                if (!this.ClassTypes.Contains(player.BaseProfession))
                {
                    if (inform)
                    {
                        player.WriteToDisplay("You do not meet the profession requirement for the quest \"" + this.Name + "\".");
                    }
                    return false;
                }
            }

            if (this.IsLevelRestricted)
            {
                if (this.minimumLevel > 0)
                {
                    if (Rules.GetExpLevel(player.Experience) < this.minimumLevel)
                    {
                        if (inform)
                        {
                            player.WriteToDisplay("You do not meet the minimum experience level requirement for the quest \"" + this.Name + "\".");
                        }
                        return false;
                    }
                }

                if (this.MaximumLevel > 0)
                {
                    if (Rules.GetExpLevel(player.Experience) > this.maximumLevel)
                    {
                        if (inform)
                        {
                            player.WriteToDisplay("Your experience level is too high for the quest \"" + this.Name + "\".");
                        }
                        return false;
                    }
                }
                
            }

            if (!this.PlayerMeetsKarmaRequirement(player))
            {
                if (inform)
                {
                    player.WriteToDisplay("You do not meet the karma requirements for the quest \"" + this.Name + "\".");
                }
                return false;
            }

            return true;
        }

        public bool PlayerMeetsKarmaRequirement(PC player)
        {
            if (this.Requirements.ContainsValue(QuestRequirement.Karma))
            {
                foreach (int step in this.Requirements.Keys)
                {
                    if (this.Requirements[step] == QuestRequirement.Karma)
                    {
                        if (this.RequiredItems[step] > player.currentKarma)
                        {
                            return false;
                        }
                        else break;
                    }
                }
            }

            return true;
        }

        public bool BeginQuest(PC questor, bool inform)
        {
            GameQuest newQuest = questor.GetQuest(this.QuestID);
            if (newQuest != null)
            {
                DateTime start;
                DateTime.TryParse(newQuest.StartDate, out start);
                if (start < DateTime.Now && !newQuest.CompletedSteps.Contains(newQuest.TotalSteps) && !newQuest.IsRepeatable)
                {
                    if (inform)
                    {
                        questor.WriteToDisplay("You have already started this quest \"" + this.Name + "\".");
                        return false;
                    }
                }
                else
                {
                    if (newQuest.IsRepeatable)
                    {
                        goto verifyQuestRequirements;
                    }

                    if (inform)
                    {
                        questor.WriteToDisplay("You have already completed the quest " + this.Name + ".");
                    }
                }
                return false;
            }

        verifyQuestRequirements:

            if (!PlayerMeetsRequirements(questor, true))
            {
                return false;
            }

            if (questor.QuestList.Contains(newQuest))
            {
                for (int qc = 0;qc < questor.QuestList.Count;qc++)
                {
                    if (questor.QuestList[qc].questID == newQuest.questID)
                    {
                        questor.QuestList[qc].StartDate = DateTime.Now.ToString();
                        questor.QuestList[qc].CurrentStep = 1;
                    }
                }
            }
            else
            {
                newQuest = (GameQuest)this.MemberwiseClone();
                newQuest.StartDate = DateTime.Now.ToString();
                newQuest.CurrentStep = 1;
                questor.QuestList.Add(newQuest);
            }
            return true;
        }

        public void FinishStep(NPC questGiver, PC questor, int step)
        {
            try
            {
                // precautionary
                if (step <= 0) { step = 1; }

                // flag check
                if (!PlayerMeetsRequirements(questor, true))
                {
                    return;
                }

                // make the quest giver stand still for a period of time to conclude interaction
                if (questGiver != null)
                {
                    Effect.CreateCharacterEffect(Effect.EffectTypes.Hello_Immobility, 0, questGiver, Rules.RollD(5, 6), null);
                }

                // the quest giver tells the player that the quest is complete
                if (this.FinishStrings.ContainsKey(step))
                {
                    if (questGiver != null)
                    {
                        string emote = Utils.ParseEmote(this.FinishStrings[step]);
                        string finish = this.FinishStrings[step];
                        if (emote != "")
                        {
                            finish = finish.Replace("{" + emote + "}", "");
                            questor.WriteToDisplay(questGiver.Name + " " + emote);
                        }
                        if (finish.Length > 0)
                        {
                            questor.WriteToDisplay(questGiver.Name + ": " + finish);
                        }
                    }
                    else
                    {
                        questor.WriteToDisplay(this.FinishStrings[step]);
                    }
                }

                // play quest sound file if applicable
                if (this.SoundFiles.ContainsKey(step))
                {
                    if (questGiver != null)
                    {
                        questGiver.EmitSound(this.SoundFiles[step]);
                    }
                    else
                    {
                        questor.CurrentCell.EmitSound(this.SoundFiles[step]);
                    }
                }

                // give reward title
                if (this.RewardTitle != "")
                {
                    string[] s = this.RewardTitle.Split(VSPLIT.ToCharArray());
                    if (s.Length > 0 && Convert.ToInt16(s[0]) == this.CurrentStep)
                    {
                        string oldTitle = questor.classFullName; // store old title
                        questor.classFullName = s[1];
                        questor.WriteToDisplay("Your title has been changed from " + oldTitle + " to " + questor.classFullName + ".");
                    }
                }

                // give reward class
                if (this.RewardClass != "")
                {
                    string[] s = this.RewardClass.Split(VSPLIT.ToCharArray());

                    if (s.Length > 0 && Convert.ToInt16(s[0]) == this.CurrentStep)
                    {
                        string oldClass = "";
                        bool classMatch = false;
                        foreach (Character.ClassType classType in Enum.GetValues(typeof(Character.ClassType)))
                        {
                            if (classType.ToString().ToLower() == s[1].ToLower())
                            {
                                oldClass = questor.BaseProfession.ToString(); // store old class
                                questor.BaseProfession = (Character.ClassType)Enum.Parse(typeof(Character.ClassType), s[1], true);
                                questor.classFullName = Utils.FormatEnumString(questor.BaseProfession.ToString());
                                questor.WriteToDisplay("Your profession has been changed from " + oldClass + " to " + questor.BaseProfession.ToString() + ".");
                                if (questor.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                                {
                                    ProtocolYuusha.SendCharacterStats(questor, questor);
                                }
                                classMatch = true;
                                break;
                            }
                        }

                        if (!classMatch)
                        {
                            // search subclasses here for a match then change subclass... TODO
                        }
                    }
                }
                
                // give reward item
                if (this.RewardItems.ContainsKey(step))
                {
                    Item reward = Item.CopyItemFromDictionary(this.RewardItems[step]);

                    if (reward != null)
                    {
                        if (this.CoinValues.ContainsKey(step)) // set coin value of reward if necessary
                        {
                            reward.coinValue = this.CoinValues[step];
                        }

                        if (reward.attuneType == Globals.eAttuneType.Quest)
                        {
                            reward.AttuneItem(questor);
                        }
                        if (reward.coinValue == 0 && reward.vRandLow > 0) //mlt fix worthless gem reward v
                        {
                            reward.coinValue = Rules.Dice.Next(reward.vRandLow,reward.vRandHigh);
                        }                                                                                     //mlt^
                        questor.EquipEitherHandOrDrop(reward);
                    }

                    if (this.TotalSteps == 1 && this.RewardItems.Count > 1) // used for simple escort quests with multiple rewards
                    {
                        foreach (short s in this.RewardItems.Keys)
                        {
                            if (s > 1) // already rewarded the first item
                            {
                                reward = Item.CopyItemFromDictionary(this.RewardItems[s]);
                                if (reward != null)
                                {
                                    if (this.CoinValues.ContainsKey(s)) // set coin value of reward if necessary
                                    {
                                        reward.coinValue = this.CoinValues[s];
                                    }
                                    if (reward.coinValue == 0 && reward.vRandLow > 0) //mlt fix worthless gem reward v
                                    {
                                        reward.coinValue = Rules.Dice.Next(reward.vRandLow,reward.vRandHigh);
                                    }                                                                                //mlt ^
                                    if (reward.attuneType == Globals.eAttuneType.Quest)
                                    {
                                        reward.AttuneItem(questor);
                                    }

                                    questor.EquipEitherHandOrDrop(reward);
                                }
                            }
                        }
                    }
                }

                // give quest flag
                if (this.RewardFlags.ContainsKey(step))
                {
                    if (!questor.QuestFlags.Contains(this.RewardFlags[step]))
                    {
                        questor.QuestFlags.Add(this.RewardFlags[step]);
                        if (this.RewardFlags[step].IndexOf("_C") != -1) // add a permanent content flag
                        {
                            // remove the _C at the end of the flag
                            questor.ContentFlags.Add(this.RewardFlags[step].Substring(0, this.RewardFlags[step].IndexOf("_C")));
                        }
                        //questor.WriteToDisplay("You have received a quest flag!");
                    }
                }

                // give quest experience
                if (this.RewardExperience.ContainsKey(step))
                {
                    long expGain = this.RewardExperience[step];

                    // Accelerated experience gain option.
                    if (DragonsSpineMain.Instance.Settings.AcceleratedExperienceGain)
                        expGain = Convert.ToInt64(expGain * DragonsSpineMain.Instance.Settings.AcceleratedExperienceGainMultiplier);

                    questor.WriteToDisplay("You earn " + expGain + " experience.");

                    questor.Experience += expGain;
                }

                // give reward stats
                if (this.RewardStats.ContainsKey(step))
                {
                    // Stats: stat #
                    // Talent: 
                    // TODO: Faction: faction # #
                    
                    string[] stat = this.RewardStats[step].Split(ASPLIT.ToCharArray());

                    switch (stat[0].ToLower())
                    {
                        case "h":
                            questor.HitsAdjustment += Convert.ToInt32(stat[1]);
                            questor.WriteToDisplay("Your maximum hits have increased by " + Convert.ToInt32(stat[1]) + ".");
                            questor.Hits = questor.HitsFull;
                            break;
                        case "s":
                            questor.StaminaAdjustment += Convert.ToInt32(stat[1]);
                            questor.WriteToDisplay("Your maximum stamina has increased by " + Convert.ToInt32(stat[1]) + ".");
                            questor.Stamina = questor.StaminaFull;
                            break;
                        case "m":
                            questor.ManaAdjustment += Convert.ToInt32(stat[1]);
                            questor.WriteToDisplay("Your maximum mana has increased by " + Convert.ToInt32(stat[1]) + ".");
                            questor.Mana = questor.ManaFull;
                            break;
                        case "q":
                            questor.UW_hasStomach = true;
                            questor.WriteToDisplay("You have gained your stomach!");
                            break;
                        case "w":
                            questor.UW_hasIntestines = true;
                            questor.WriteToDisplay("You have gained your intestines!");
                            break;
                        case "e":
                            questor.UW_hasLiver = true;
                            questor.WriteToDisplay("You have gained your liver!");
                            break;
                        case "r":
                            questor.UW_hasLungs = true;
                            questor.WriteToDisplay("You have gained your lungs!");
                            break;
                        case "t":
                        case "tal":
                        case "talent":
                            DragonsSpine.Talents.GameTalent talent = null;
                            foreach (string tSearch in DragonsSpine.Talents.GameTalent.GameTalentDictionary.Keys)
                            {
                                if (tSearch == stat[1])
                                {
                                    talent = DragonsSpine.Talents.GameTalent.GameTalentDictionary[tSearch];
                                    break;
                                }
                            }

                            if (talent != null)
                            {
                                questor.talentsDictionary.Add(talent.Command, DateTime.Now - talent.DownTime);
                                questor.WriteToDisplay(questGiver.GetNameForActionResult() + " teaches you how to perform " + talent.Description.ToLower());
                                DAL.DBPlayer.InsertPlayerTalent(questor.UniqueID, talent.Command);
                            }
                            break;
                        default:
                            // TODO: add faction here
                            break;
                    }
                }

                // reward teleport
                if (this.RewardTeleports.ContainsKey(step))
                {
                    string[] coords = this.RewardTeleports[step].Split(",".ToCharArray());
                    questor.CurrentCell = GameWorld.Cell.GetCell(questor.FacetID, Convert.ToInt16(coords[0]), Convert.ToInt16(coords[1]),
                        Convert.ToInt32(coords[2]), Convert.ToInt32(coords[3]), Convert.ToInt32(coords[4]));

                    if (coords.Length >= 6 && coords[5].Length > 0)
                    {
                            questor.WriteToDisplay(coords[5]);
                    }

                    // teleport the group with the questor
                    if (this.TeleportGroup && questor.Group != null)
                    {
                        string reason = "";
                        if (coords.Length >= 6 && coords[5].Length > 1)
                        {
                            reason = coords[5];
                        }
                        questor.Group.TeleportGroup(questor, questor.CurrentCell, reason);
                    }
                }

                if (!this.CompletedSteps.Contains(this.CurrentStep))
                {
                    this.CompletedSteps.Add(this.CurrentStep);
                }

                if (this.TotalSteps > this.CurrentStep)
                {
                    this.CurrentStep++;
                    // log the quest step completion
                    Utils.Log(this.GetLogString() + ", quest step " + step + ", was completed by " + questor.GetLogString(), Utils.LogType.QuestCompletion);
                }

                if (this.CurrentStep >= this.TotalSteps || this.TotalSteps == 1)
                {
                    this.CompleteQuest(questor);

                    if (this.DespawnsNPC)
                    {
                        NPC npc = (NPC)questGiver;
                        npc.RoundsRemaining = 0;
                        npc.special = npc.special + " despawn";
                        //npc.special += " despawn";
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public void CompleteQuest(PC questor)
        {
            try
            {
                bool inlist = false;
                for (int qc = 0;qc < questor.QuestList.Count;qc++)
                {
                    if (questor.QuestList[qc].questID == this.questID)
                    {
                        questor.QuestList[qc].timesCompleted++;
                        questor.QuestList[qc].FinishDate = DateTime.Now.ToString();
                        questor.QuestList[qc].CurrentStep = 0;
                        inlist = true;
                    }
                }
                if (!inlist)
                {
                    // increment times completed
                    this.TimesCompleted++;
                    // stamp the finish date
                    this.FinishDate = DateTime.Now.ToString();
                    //reset current step to 0 for repeatable quest,others dont matter since cant do again
                    this.CurrentStep = 0;
                    // clear out the completed steps if repeatable
                }
                if (this.IsRepeatable)
                {
                    this.completedSteps = new List<int>();
                }

                // clear out old flags that were needed for this quest
                foreach (string flag in this.RequiredFlags)
                {
                    if (questor.QuestFlags.Contains(flag))
                    {
                        questor.QuestFlags.Remove(flag);
                    }
                }

                if (this.name != "" && (this.MasterQuestID <= 0 && !this.responseStrings.ContainsKey("follow")) && questor.LandID != GameWorld.Land.ID_UNDERWORLD)
                {
                    questor.WriteToDisplay("Quest completed: " + this.Name);
                }

                // log the quest completion
                Utils.Log(this.GetLogString() + " was completed by " + questor.GetLogString(), Utils.LogType.QuestCompletion);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static int GetNextAvailableQuestID()
        {
            List<int> idNumbers = DAL.DBQuest.GetQuestIDList();

            idNumbers.Sort();
            int[] values = Enumerable.Range(idNumbers[0], idNumbers[idNumbers.Count - 1] - idNumbers[0]).ToArray();


            foreach (int num in values)
                if (!idNumbers.Contains(num)) return num;
            return -1;
        }
    }
}
