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

namespace DragonsSpine.GameEvents
{
    public interface IGameEventHandler
    {
        GameEvent ReferenceGameEvent { get; set; }

        /// <summary>
        /// Called once when the GameEvent starts.
        /// </summary>
        void OnStart();

        /// <summary>
        /// Called when the GameEvent finishes. General cleanup.
        /// </summary>
        void OnFinish();

        /// <summary>
        /// Called when a GameEvent executes on every tick.
        /// </summary>
        /// <returns></returns>
        void OnTick(object sender, System.Timers.ElapsedEventArgs eventArgs);
    }
}
