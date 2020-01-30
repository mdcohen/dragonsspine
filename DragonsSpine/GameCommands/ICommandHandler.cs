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
using DragonsSpine;

namespace DragonsSpine.Commands
{
    public interface ICommandHandler
    {
        /// <summary>
        /// Called when a command should be executed.
        /// </summary>
        /// <param name="chr">The Character executing the command.</param>
        /// <param name="args">Command arguments.</param>
        /// <returns>True if successful.</returns>
        bool OnCommand(Character chr, string args);
    }
}
