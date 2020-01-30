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
using System.Text;

namespace DragonsSpine.Talents
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TalentAttribute : Attribute
    {
        #region Private Data
        /// <summary>
        /// Holds the string command for the talent.
        /// </summary>
        private string m_command;

        /// <summary>
        /// Holds the formal name of the talent.
        /// </summary>
        private string m_name;

        /// <summary>
        /// Holds the description of the talent.
        /// </summary>
        private string m_description;

        /// <summary>
        /// Holds the minimum level a Character may use the talent.
        /// </summary>
        private int m_minimumLevel;

        /// <summary>
        /// Holds whether the Talent is passive (not activated).
        /// </summary>
        private bool m_passive;

        /// <summary>
        /// Holds the stamina cost of the talent.
        /// </summary>
        private int m_performanceCost;

        /// <summary>
        /// Holds the cost of initial purchase.
        /// </summary>
        private int m_purchasePrice;

        /// <summary>
        /// Holds the amount of time before the Talent may be used again.
        /// </summary>
        private TimeSpan m_downTime;

        /// <summary>
        /// Holds whether the talent may be learned at generic mentors.
        /// </summary>
        private bool m_availableAtMentor;

        /// <summary>
        /// Holds examples of command usages.
        /// </summary>
        private string[] m_usages;

        /// <summary>
        /// Holds the professions that may use this talent. If this is null then all professions may perform the talent.
        /// </summary>
        private Character.ClassType[] m_professions;

        private string m_soundFile;

        /// <summary>
        /// Holds the Entities that may perform the talent. Note these GameTalents do not have a performance cost and instead look only at downTime.
        /// </summary>
        private Autonomy.EntityBuilding.EntityLists.Entity[] m_entities;
        #endregion

        #region Public Properties
        public string Command
        {
            get { return m_command; }
        }

        public string Name
        {
            get { return m_name; }
        }

        public string Description
        {
            get { return m_description; }
        }

        public int MinimumLevel
        {
            get { return m_minimumLevel; }
        }

        public bool IsPassive
        {
            get { return m_passive; }
        }

        public int PerformanceCost
        {
            get { return m_performanceCost; }
        }

        public int PurchasePrice
        {
            get { return m_purchasePrice; }
        }

        public TimeSpan DownTime
        {
            get { return m_downTime; }
        }

        public bool IsAvailableAtMentor
        {
            get { return m_availableAtMentor; }
        }

        public string[] Usages
        {
            get { return m_usages; }
        }

        public Character.ClassType[] Professions
        {
            get { return m_professions; }
        }

        public Autonomy.EntityBuilding.EntityLists.Entity[] Entities
        {
            get { return m_entities; }
        }

        public string SoundFile
        {
            get { return m_soundFile; }
        }
        #endregion

        /// <summary>
        /// Constructs a new TalentAttribute.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="name">Formal name.</param>
        /// <param name="description">Description.</param>
        /// <param name="cost">Stamina cost.</param>
        /// <param name="usages">Examples of command usage.</param>
        public TalentAttribute(string command, string name, string description, bool passive, int performanceCost, int purchasePrice, int minimumLevel, int downTime, bool availableAtMentor,
            string[] usages, params Character.ClassType[] professions)
        {
            m_command = command;
            m_name = name;
            m_description = description;
            m_passive = passive;
            m_performanceCost = performanceCost;
            m_purchasePrice = purchasePrice;
            m_minimumLevel = minimumLevel;
            m_downTime = new TimeSpan(0, 0, downTime);
            m_availableAtMentor = availableAtMentor;
            m_usages = usages;
            m_professions = professions;
            m_soundFile = "";
        }

        /// <summary>
        /// Constructs a new TalentAttribute with a SoundFile.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="name">Formal name.</param>
        /// <param name="description">Description.</param>
        /// <param name="cost">Stamina cost.</param>
        /// <param name="usages">Examples of command usage.</param>
        public TalentAttribute(string command, string name, string description, string soundFile, bool passive, int performanceCost, int purchasePrice, int minimumLevel, int downTime, bool availableAtMentor,
            string[] usages, params Character.ClassType[] professions)
        {
            m_command = command;
            m_name = name;
            m_description = description;
            m_soundFile = "";
            m_passive = passive;
            m_performanceCost = performanceCost;
            m_purchasePrice = purchasePrice;
            m_minimumLevel = minimumLevel;
            m_downTime = new TimeSpan(0, 0, downTime);
            m_availableAtMentor = availableAtMentor;
            m_usages = usages;
            m_professions = professions;
        }

        /// <summary>
        /// Constructs a new TalentAttribute for use by speficied Entities in AI decisions.
        /// Note these are always activated, cost no stamina to activate, and instead utilize downTime (recommended once per game day, etc..).
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="name">Formal name.</param>
        /// <param name="description">Description.</param>
        /// <param name="cost">Stamina cost.</param>
        public TalentAttribute(string command, string name, string description, int minimumLevel, int downTime,
            params Autonomy.EntityBuilding.EntityLists.Entity[] entities)
        {
            m_command = command;
            m_name = name;
            m_description = description;
            m_passive = false;
            m_performanceCost = 0;
            m_purchasePrice = 0;
            m_minimumLevel = minimumLevel;
            m_downTime = new TimeSpan(0, 0, downTime);
            m_availableAtMentor = false;
            m_usages = null;
            m_professions = null;
            m_entities = entities;
        }

        /// <summary>
        /// Constructs a new TalentAttribute with null for usages and professions (all professions may perform this talent).
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="name">Formal name.</param>
        /// <param name="description">Description.</param>
        /// <param name="cost">Stamina cost.</param>
        public TalentAttribute(string command, string name, string description, bool passive, int performanceCost, int purchasePrice, int minimumLevel, int downTime, bool availableAtMentor)
            : this(command, name, description, passive, performanceCost, purchasePrice, minimumLevel, downTime, availableAtMentor, null, null)
        {
        }
    }
}
