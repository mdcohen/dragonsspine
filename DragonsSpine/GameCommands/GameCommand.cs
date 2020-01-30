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

namespace DragonsSpine.Commands
{
    public class GameCommand
    {
        /// <summary>
        /// Key = GameCommand.Command, Value = GameCommand.
        /// </summary>
        public static Dictionary<string, GameCommand> GameCommandDictionary = new Dictionary<string, GameCommand>();

        /// <summary>
        /// Key = GameCommand.Alias, Value = GameCommand.Command.
        /// </summary>
        public static Dictionary<string, string> GameCommandAliases = new Dictionary<string, string>();

        #region Private Data
        /// <summary>
        /// Holds the string game command.
        /// </summary>
        private readonly string m_command;

        /// <summary>
        /// Holds the description of the game command.
        /// </summary>
        private readonly string m_description;

        /// <summary>
        /// Holds the minimum priv level that may use this game command.
        /// </summary>
        private readonly int m_privLevel;

        /// <summary>
        ///  Holds the weight of the command.
        /// </summary>
        private readonly int m_commandWeight;

        /// <summary>
        /// Holds examples of game command usages.
        /// </summary>
        private readonly string[] m_usages;

        /// <summary>
        /// Holds the states the command is allowed to be executed in.
        /// </summary>
        private readonly Globals.ePlayerState[] m_states;

        /// <summary>
        /// Holds the ICommandHandler for the command.
        /// </summary>
        private readonly ICommandHandler m_commandHandler;
        #endregion

        #region Public Properties
        public string Command
        {
            get { return m_command; }
        }

        public string Description
        {
            get { return m_description; }
        }

        public int PrivLevel
        {
            get { return m_privLevel; }
        }

        public int Weight
        {
            get { return m_commandWeight; }
        }

        public string[] Usages
        {
            get { return m_usages; }
        }

        public Globals.ePlayerState[] States
        {
            get { return m_states; }
        }

        public ICommandHandler Handler
        {
            get { return m_commandHandler; }
        }
        #endregion

        public GameCommand(string command, string description, int privLevel, int weight, string[] usages, Globals.ePlayerState[] states, ICommandHandler handler)
        {
            m_command = command;
            m_description = description;
            m_privLevel = privLevel;
            m_commandWeight = weight;
            m_usages = usages;
            m_states = states;
            m_commandHandler = handler;
        }

        public static bool LoadGameCommands()
        {
            try
            {
                foreach(Type t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
                {
                    if(Array.IndexOf(t.GetInterfaces(), typeof(ICommandHandler)) > -1)
                    {
                        CommandAttribute a = (CommandAttribute)t.GetCustomAttributes(typeof(CommandAttribute), true)[0];
                        GameCommand gameCommand = new GameCommand(a.Command, a.Description, a.PrivLevel, a.Weight, a.Usages, a.States, (ICommandHandler)Activator.CreateInstance(t));

                        // Add the GameCommand to a dictionary.
                        if (!GameCommandDictionary.ContainsKey(gameCommand.Command))
                        {
                            GameCommandDictionary.Add(gameCommand.Command, gameCommand);
                        }
                        else
                        {
                            Utils.Log("GameCommand already exists: " + gameCommand.Command, Utils.LogType.SystemWarning);
                        }

                        // Add alternate alias strings to a dictionary paired up with the string command.
                        if (a.Aliases != null)
                        {
                            foreach (string alias in a.Aliases)
                            {
                                if (!GameCommandAliases.ContainsKey(alias))
                                {
                                    GameCommandAliases.Add(alias, gameCommand.Command);
                                }
                                else
                                {
                                    Utils.Log("GameCommand Alias already exists: " + alias, Utils.LogType.SystemWarning);
                                }
                            }
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