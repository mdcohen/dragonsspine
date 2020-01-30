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

namespace DragonsSpine.GameEvents
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class GameEventAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime EventStart { get; set; }
        public DateTime EventFinish { get; set; }
        public double TimerInterval { get; set; }

        public GameEventAttribute()
        {
            Name = "";
            Description = "";
            EventStart = new DateTime(1999, 1, 1);
            EventFinish = new DateTime(1999, 1, 1);
            TimerInterval = DragonsSpineMain.MasterRoundInterval; // default to game round timer interval
        }

        public GameEventAttribute(string name, string desc) : base()
        {
            Name = name;
            Description = desc;
        }

        public GameEventAttribute(string name, string desc, string startDateTime, string finishDateTime)
        {
            Name = name;
            Description = desc;

            DateTime eStart = new DateTime(1999, 1, 1);

            if (!DateTime.TryParse(startDateTime, out eStart))
            {
                Utils.Log("Failed to parse start DateTime for GameEvent: " + Name, Utils.LogType.SystemWarning);
            }
            else
            {
                EventStart = eStart;

                DateTime eFinish = new DateTime(1999, 1, 1);

                if (!DateTime.TryParse(startDateTime, out eStart))
                {
                    Utils.Log("Failed to parse finish DateTime for GameEvent: " + Name, Utils.LogType.SystemWarning);
                }
                else
                {
                    EventFinish = eFinish;
                    TimerInterval = DragonsSpineMain.MasterRoundInterval;
                }
            }
        }

        public GameEventAttribute(string name, string desc, string startDateTime, string finishDateTime, double timerInterval)
            : base()
        {
            Name = name;
            Description = desc;

            DateTime eStart = new DateTime(1999, 1, 1);

            if (!DateTime.TryParse(startDateTime, out eStart))
            {
                Utils.Log("Failed to parse start DateTime for GameEvent: " + Name, Utils.LogType.SystemWarning);
            }
            else
            {
                EventStart = eStart;

                DateTime eFinish = new DateTime(1999, 1, 1);

                if (!DateTime.TryParse(startDateTime, out eStart))
                {
                    Utils.Log("Failed to parse finish DateTime for GameEvent: " + Name, Utils.LogType.SystemWarning);
                }
                else
                {
                    EventFinish = eFinish;
                    TimerInterval = timerInterval;
                }
            }
        }
    }
}
