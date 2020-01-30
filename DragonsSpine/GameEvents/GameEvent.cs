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
using System.Linq;
using System.Text;

namespace DragonsSpine.GameEvents
{
    /// <summary>
    /// GameEvents are hardcoded in their respective IGameEventHandler interface. They can be anything from a one time item gift to a full on battle between, say, townsfolk and a raiding orc force.
    /// </summary>
    public class GameEvent
    {
        public static Dictionary<string, GameEvent> GameEventDictionary = new Dictionary<string, GameEvent>();

        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime EventStart { get; set; }
        public DateTime EventFinish { get; set; }

        public System.Timers.Timer EventTimer { get; set; }

        public List<int> ParticipantsUniqueIDList = new List<int>();

        public IGameEventHandler Handler { get; set; }

        public bool IsTriggeredEvent
        {
            get { return EventStart == new DateTime(1999, 1, 1) && EventFinish == new DateTime(1999, 1, 1); }
        }

        public GameEvent(string name, string desc, DateTime start, DateTime finish, double timerInterval, IGameEventHandler handler)
        {
            Name = name;
            Description = desc;
            EventStart = start;
            EventFinish = finish;            

            Handler = handler;
            Handler.ReferenceGameEvent = this;

            EventTimer = new System.Timers.Timer();
            EventTimer.Interval = timerInterval;
            this.EventTimer.Elapsed += new System.Timers.ElapsedEventHandler(Handler.OnTick);
        }

        public bool EventRequirementsMet(Character chr)
        {
            if (EventStart > DateTime.Now)
                return false;

            if (EventFinish < DateTime.Now)
                return false;

            if (ParticipantsUniqueIDList.Contains(chr.UniqueID))
                return false;

            // Check destination.

            return true;
        }

        public void StartEventTimer()
        {
            if (EventTimer == null || EventTimer.Interval <= -1)
                return;

            EventTimer.Start();
        }

        public void SuccessfulExecution(Character chr)
        {
            // Log the unique ID of the character who was a part of the event.
            // Send mail if applicable.
            // Send item if applicable.
        }

        /// <summary>
        /// Called upon server start to load all GameEvents into a master dictionary.
        /// </summary>
        /// <returns>True upon successful load.</returns>
        public static bool LoadEvents()
        {
            try
            {
                foreach (Type t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (Array.IndexOf(t.GetInterfaces(), typeof(IGameEventHandler)) > -1)
                    {
                        GameEventAttribute attr = (GameEventAttribute)t.GetCustomAttributes(typeof(GameEventAttribute), true)[0];

                        GameEvent gameEvent = new GameEvent(attr.Name, attr.Description, attr.EventStart, attr.EventFinish,
                            attr.TimerInterval, (IGameEventHandler)Activator.CreateInstance(t));

                        // Add the GameEvent to a dictionary.
                        if (!GameEventDictionary.ContainsKey(gameEvent.Name))
                            GameEventDictionary.Add(gameEvent.Name, gameEvent);
                        else
                            Utils.Log("GameEvent already exists: " + gameEvent.Name, Utils.LogType.SystemWarning);
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
