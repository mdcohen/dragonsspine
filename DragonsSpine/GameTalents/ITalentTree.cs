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

namespace DragonsSpine.Talents
{
    public interface ITalentTree
    {
        /// <summary>
        /// Used internally to validate various requirements to purchase a talent or talent line. Not implemented as of 12/6/2015 Eb.
        /// </summary>
        /// <param name="chr">The Character attempting to purchase the GameTalent.</param>
        /// <returns>True if validated.</returns>
        bool ValidateTreeRequirements(Character chr);
    }
}
