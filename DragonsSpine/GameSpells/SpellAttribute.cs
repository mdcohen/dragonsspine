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

namespace DragonsSpine.Spells
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SpellAttribute : Attribute
    {
        #region Private Data
        /// <summary>
        /// Holds the unique spell ID. This value must be unique.
        /// </summary>
        private int m_id;

        /// <summary>
        /// Holds the command used to cast the spell.
        /// </summary>
        private string m_command;

        /// <summary>
        /// Holds the name of the spell.
        /// </summary>
        private string m_name;

        /// <summary>
        /// Holds the description of the spell.
        /// </summary>
        private string m_description;

        /// <summary>
        /// Holds the professions that may cast this spell.
        /// </summary>
        private Character.ClassType[] m_classTypes;

        /// <summary>
        /// Holds the spell type, which is primarily used by AI.
        /// </summary>
        private Globals.eSpellType m_spellType; // Abjuration, Alteration, Conjuration, Divination, Evocation, Necromancy (primarily used by AI)

        /// <summary>
        /// Holds the target type of the spell.
        /// </summary>
        private Globals.eSpellTargetType m_targetType; // Area_Effect, Group, Point_Blank_Area_Effect, Self, Single

        /// <summary>
        /// Holds the mana cost of the spell.
        /// </summary>
        private int m_manaCost; // mana cost to cast the spell

        /// <summary>
        /// Holds the required level in order to learn the spell.
        /// </summary>
        private int m_requiredLevel; // required casting level

        /// <summary>
        /// Holds the cost of the spell when learning it.
        /// </summary>
        private int m_trainingPrice; // purchase price

        /// <summary>
        /// Holds the sound file information played when the spell is cast.
        /// </summary>
        private string m_soundFile; // sound file info

        /// <summary>
        /// Holds whether the spell is beneficial. Used by AI to assist in determining which spell to cast at a target.
        /// </summary>
        private bool m_beneficial;

        /// <summary>
        /// Holds whether the spell is available at generic spell trainers.
        /// </summary>
        private bool m_availableAtTrainer;

        /// <summary>
        /// Holds whether the spell is available as a random scribed scroll in loot.
        /// </summary>
        private bool m_foundForScribing;

        /// <summary>
        /// Holds whether the spell is available as a scroll with charge(s) to be cast.
        /// </summary>
        private bool m_foundForCasting;

        /// <summary>
        /// Holds whether the spell is available only as lair loot.
        /// </summary>
        private bool m_onlyLairs;
        #endregion

        #region Public Properties
        public int ID
        {
            get { return m_id; }
        }

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

        public Character.ClassType[] ClassTypes
        {
            get { return m_classTypes; }
        }

        public Globals.eSpellType SpellType
        {
            get { return m_spellType; }
        }

        public Globals.eSpellTargetType TargetType
        {
            get { return m_targetType; }
        }

        public int ManaCost
        {
            get { return m_manaCost; }
        }

        public int RequiredLevel
        {
            get { return m_requiredLevel; }
        }

        public int TrainingPrice
        {
            get { return m_trainingPrice; }
        }

        public string SoundFile
        {
            get { return m_soundFile; }
        }

        public bool IsBeneficial
        {
            get { return m_beneficial; }
        }

        public bool IsAvailableAtTrainer
        {
            get { return m_availableAtTrainer; }
        }

        public bool IsFoundForScribing
        {
            get { return m_foundForScribing; }
        }

        public bool IsFoundForCasting
        {
            get { return m_foundForCasting; }
        }

        public bool OnlyFoundInLairs
        {
            get { return m_onlyLairs; }
        }
        #endregion

        //Constructors
        public SpellAttribute(GameSpell.GameSpellID id, string command, string name, string description, Globals.eSpellType spellType, Globals.eSpellTargetType targetType, int manaCost,
            int requiredLevel, int trainingPrice, string soundFile, bool beneficial, bool availableAtTrainer, bool foundForScribing, bool foundForCasting, bool onlyLairs, params Character.ClassType[] classTypes)
        {
            m_id = (int)id;
            m_command = command;
            m_name = name;
            m_description = description;
            m_spellType = spellType;
            m_targetType = targetType;
            m_manaCost = manaCost;
            m_requiredLevel = requiredLevel;
            m_trainingPrice = trainingPrice;
            m_soundFile = soundFile;
            m_beneficial = beneficial;
            m_availableAtTrainer = availableAtTrainer;
            m_classTypes = classTypes;
            m_foundForScribing = foundForScribing;
            m_foundForCasting = foundForCasting;
            m_onlyLairs = onlyLairs;
        }
    }
}
