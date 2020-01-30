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

namespace DragonsSpine.Commands
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CommandAttribute : Attribute
    {
        #region Private Data
        /// <summary>
        /// Holds the string command.
        /// </summary>
        private string m_command;

        /// <summary>
        /// Holds the description of the command.
        /// </summary>
        private string m_description;

        /// <summary>
        /// Holds the minimum priv level that may use this command.
        /// </summary>
        private int m_privLevel;

        /// <summary>
        /// Holds command aliases.
        /// </summary>
        private string[] m_aliases;

        /// <summary>
        /// Holds the weight of the command.
        /// </summary>
        private int m_commandWeight;

        /// <summary>
        /// Holds examples of command usages.
        /// </summary>
        private string[] m_usages;

        /// <summary>
        /// Holds the states the game is allowed to be executed in.
        /// </summary>
        private Globals.ePlayerState[] m_states;
        #endregion

        #region Public Properties
        public string Command
        {
            get { return m_command; }
        }

        public int Weight
        {
            get { return m_commandWeight; }
        }

        public string Description
        {
            get { return m_description; }
        }

        public int PrivLevel
        {
            get { return m_privLevel; }
        }

        public string[] Aliases
        {
            get { return m_aliases; }
        }

        public string[] Usages
        {
            get { return m_usages; }
        }

        public Globals.ePlayerState[] States
        {
            get { return m_states; }
        }
        #endregion

        /// <summary>
        /// Constructs a new CommandAttribute.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="description">Description.</param>
        /// <param name="privLevel">Privilege Level.</param>
        /// <param name="aliases">Aliases.</param>
        /// <param name="weight">Command weight.</param>
        /// <param name="usages">Examples of command usage.</param>
        /// <param name="states">Which player states the command may be executed.</param>
        public CommandAttribute(string command, string description, int privLevel, string[] aliases, int weight, string[] usages, params Globals.ePlayerState[] states)
        {
            m_command = command;
            m_description = description;
            m_privLevel = privLevel;
            m_aliases = aliases;
            m_commandWeight = weight;
            m_usages = usages;
            m_states = states;            
        }

        /// <summary>
        /// Constructs a new CommandAttribute with null for aliases.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="description">Description.</param>
        /// <param name="privLevel">Privilege Level.</param>
        /// <param name="weight">Command weight.</param>
        /// <param name="usages">Examples of command usage.</param>
        /// <param name="states">Which player states the command may be executed.</param>
        public CommandAttribute(string command, string description, int privLevel, int weight, string[] usages, params Globals.ePlayerState[] states)
            : this(command, description, privLevel, null, weight, usages, states)
        {
        }
    }
}
