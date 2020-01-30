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

namespace DragonsSpine.Talents
{
    public class GameTalent
    {
        /// <summary>
        /// Key = command, Value = GameTalent object
        /// </summary>
        public static Dictionary<string, GameTalent> GameTalentDictionary = new Dictionary<string, GameTalent>();

        // This enumeration, when *lowercase*, is EXACTLY the command of the GameTalent. The GameTalent.Command is the Key in the GameTalentDictionary.
        // The order of this enumeration is inconsequential.
        public enum TALENTS
        {
            Riposte,
            DualWield,
            BlindFighting,
            Steal,
            Peek,
            Bash,
            DoubleAttack,
            Charge,
            PickLocks,
            RapidKicks,
            FlyingFury,
            LegSweep,
            Backstab,
            Cleave,
            Memorize,
            Assassinate
        }

        #region Private Data
        /// <summary>
        /// Holds the string command for the talent.
        /// </summary>
        private readonly string m_command;

        /// <summary>
        /// Holds the formal name of the talent.
        /// </summary>
        private readonly string m_name;

        /// <summary>
        /// Holds the description of the talent.
        /// </summary>
        private readonly string m_description;

        /// <summary>
        /// Holds the minimum experience level a Character must have to use the talent.
        /// </summary>
        private readonly int m_minimumLevel;

        /// <summary>
        /// Holds whether the GameTalent is passive (not activated).
        /// </summary>
        private readonly bool m_passive;

        /// <summary>
        /// Holds the stamina cost of the talent. If this is less than 0 then this GameTalent is passive, otherwise it must be activated.
        /// </summary>
        private readonly int m_performanceCost;

        /// <summary>
        /// Holds the initial purchase price of the talent.
        /// </summary>
        private readonly int m_purchasePrice;

        /// <summary>
        /// Holds the amount of time before the Talent may be used again.
        /// </summary>
        private readonly TimeSpan m_downTime;

        /// <summary>
        /// Holds whether this GameTalent is available to learn at generic mentors.
        /// </summary>
        private readonly bool m_availableAtMentor;

        /// <summary>
        /// Holds the rank information array for this GameTalent, if one exists.
        /// </summary>
        private readonly int[,,,,] m_rank_level_price_stamina_array;

        /// <summary>
        /// Holds examples of command usages.
        /// </summary>
        private readonly string[] m_usages;

        /// <summary>
        /// Holds the professions that may perform the talent. If this is null then it is available to all professions.
        /// </summary>
        private readonly Character.ClassType[] m_professions;

        /// <summary>
        /// Holds the Entities that may perform the talent. Note these GameTalents do not have a performance cost and instead look only at downTime.
        /// </summary>
        private readonly Autonomy.EntityBuilding.EntityLists.Entity[] m_entities;

        /// <summary>
        /// Holds the handler called when the talent is executed.
        /// </summary>
        private readonly ITalentHandler m_handler;

        /// <summary>
        /// Holds the sound filename played when the talent is performed.
        /// </summary>
        private readonly string m_soundFile;
        #endregion

        #region Public Properties
        /// <summary>
        /// Access the string command for the talent.
        /// </summary>
        public string Command
        {
            get { return m_command; }
        }

        /// <summary>
        /// Access the formal name of the talent.
        /// </summary>
        public string Name
        {
            get { return m_name; }
        }

        /// <summary>
        /// Access the description of the talent.
        /// </summary>
        public string Description
        {
            get { return m_description; }
        }

        /// <summary>
        /// Access the minimum experience level a Character must have to use the talent.
        /// </summary>
        public int MinimumLevel
        {
            get { return m_minimumLevel; }
        }

        /// <summary>
        /// Access whether or not this is a passive GameTalent. 
        /// </summary>
        public bool IsPassive
        {
            get { return m_passive; }
        }

        /// <summary>
        /// Access the stamina cost of the talent. If this is less than 0 then this GameTalent is passive, otherwise it must be activated.
        /// </summary>
        public int PerformanceCost
        {
            get { return m_performanceCost; }
        }

        /// <summary>
        /// Access the initial purchase price of the talent, if the talent is available at mentor NPCs.
        /// </summary>
        public int PurchasePrice
        {
            get { return m_purchasePrice; }
        }

        /// <summary>
        /// Access the amount of time before the talent may be used again. Note that this value is converted to rounds in the Utils class.
        /// </summary>
        public TimeSpan DownTime
        {
            get { return m_downTime; }
        }

        /// <summary>
        /// Access whether this GameTalent is available to learn at generic mentors.
        /// </summary>
        public bool IsAvailableAtMentor
        {
            get { return m_availableAtMentor; }
        }

        /// <summary>
        /// Access the rank information array for this talent, if one exists. Note if this returns null the talent has only one rank.
        /// </summary>
        public int[,,,,] RLPSArray
        {
            get { return m_rank_level_price_stamina_array; }
        }

        /// <summary>
        /// Access examples of command usages.
        /// </summary>
        public string[] Usages
        {
            get { return m_usages; }
        }

        /// <summary>
        /// Access the OnPerform handler called when the talent is executed.
        /// </summary>
        public ITalentHandler Handler
        {
            get { return m_handler; }
        }

        /// <summary>
        /// The sound file info string sent when the Talent is initially performed.
        /// </summary>
        public string SoundFile
        {
            get { return m_soundFile; }
        }
        #endregion

        public GameTalent(string command, string name, string description, bool passive, int minimumLevel, int performanceCost, int purchasePrice, TimeSpan downTime, bool availableAtMentor,
            string[] usages, Character.ClassType[] professions, Autonomy.EntityBuilding.EntityLists.Entity[] entities,
            ITalentHandler handler)
        {
            m_command = command;
            m_name = name;
            m_description = description;
            m_passive = passive;
            m_minimumLevel = minimumLevel;
            m_performanceCost = performanceCost;
            m_purchasePrice = purchasePrice;
            m_downTime = downTime;
            m_availableAtMentor = availableAtMentor;
            m_rank_level_price_stamina_array = null;
            m_usages = usages;
            m_professions = professions;
            m_entities = entities;
            m_handler = handler;
        }

        public static GameTalent GetTalent(TALENTS talent)
        {
            return GameTalentDictionary[talent.ToString().ToLower()];
        }

        /// <summary>
        /// Determine if a base profession may use this GameTalent.
        /// </summary>
        /// <param name="profession"></param>
        /// <returns></returns>
        public bool IsProfessionElgible(Character.ClassType profession)
        {
            return m_professions == null || Array.IndexOf(m_professions, (Character.ClassType)profession) > -1 ||
                Array.IndexOf(m_professions, Character.ClassType.All) > -1;
        }

        /// <summary>
        /// If the RLPS (Rank Level Price Stamina) is null then this GameTalent has only one rank.
        /// </summary>
        /// <returns>False if the GameTalent has one rank.</returns>
        public bool IsRankedTalent()
        {
            return RLPSArray != null;
        }

        /// <summary>
        /// Verify enough stamina is available to perform the GameTalent.
        /// </summary>
        /// <param name="chr">The Character object performing this GameTalent.</param>
        /// <returns>True if enough stamina is available.</returns>
        public bool MeetsPerformanceCost(Character chr)
        {
            return chr.Stamina >= this.PerformanceCost;
        }

        public virtual bool MeetsAdditionalRequirements(Character chr)
        {
            return true;
        }

        public bool PassiveTalentAvailable(Character chr)
        {
            if (chr.DisabledPassiveTalents.Contains(Command)) return false;

            if (!this.IsPassive || (this.IsPassive && (DateTime.UtcNow - this.DownTime >= chr.talentsDictionary[this.Command])))
                return true;

            return false;
        }

        /// <summary>
        /// Current mechanics are rather simple. Roll d100, if result is less than or equal to (skill level of Weapon) PLUS (experience level of Character) = success.
        /// </summary>
        /// <returns></returns>
        public bool WeaponPassFailCheck(Character chr, Globals.eSkillType skillType)
        {
            return Rules.RollD(1, 100) <= (Skills.GetSkillLevel(chr, skillType) + chr.Level);
        }

        /// <summary>
        /// Final adjustments are made to a Character upon successful performance of this GameTalent.
        /// </summary>
        /// <param name="chr">The Character object that successfully performed this GameTalent.</param>
        public void SuccessfulPerformance(Character chr)
        {
            if (!chr.IsImmortal) // for testing purposes
            {
                if(!string.IsNullOrEmpty(SoundFile))
                    chr.EmitSound(SoundFile);

                chr.talentsDictionary[Command] = DateTime.UtcNow;

                if (chr is PC pc)
                {
                    DAL.DBPlayer.UpdatePlayerTalent(pc.UniqueID, Command, chr.talentsDictionary[Command]);

                    // Send talent usage to client.
                    if (pc.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                        pc.Write(ProtocolYuusha.CHARACTER_TALENT_USE + Name + ProtocolYuusha.VSPLIT + chr.talentsDictionary[Command] + ProtocolYuusha.CHARACTER_TALENT_USE_END);
                }

                chr.Stamina -= PerformanceCost;

                if (chr.Stamina < 0)
                    chr.Stamina = 0;

                
            }

#if DEBUG
            chr.WriteToDisplay("DEBUG >> Talent Success: " + this.Name);
#endif
        }

        /// <summary>
        /// Called upon server start to load all GameTalents into a master dictionary.
        /// </summary>
        /// <returns>True upon successful load.</returns>
        public static bool LoadTalents()
        {
            try
            {
                foreach (Type t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (Array.IndexOf(t.GetInterfaces(), typeof(ITalentHandler)) > -1)
                    {
                        TalentAttribute attr = (TalentAttribute)t.GetCustomAttributes(typeof(TalentAttribute), true)[0];
                        GameTalent talent = new GameTalent(attr.Command, attr.Name, attr.Description, attr.IsPassive, attr.MinimumLevel, attr.PerformanceCost, attr.PurchasePrice,
                            attr.DownTime, attr.IsAvailableAtMentor, attr.Usages, attr.Professions, attr.Entities, (ITalentHandler)Activator.CreateInstance(t));

                        // Add the GameTalent to a dictionary.
                        if (!GameTalentDictionary.ContainsKey(talent.Command))
                        {
                            GameTalentDictionary.Add(talent.Command, talent);
                        }
                        else
                        {
                            Utils.Log("GameTalent already exists: " + talent.Command, Utils.LogType.SystemWarning);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return false;
            }
            return true;
        }
    }
}
