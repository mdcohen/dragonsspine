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

namespace DragonsSpine.GameProfessions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ProfessionAttribute : Attribute
    {
        #region Private Data
        /// <summary>
        /// Holds the unique profession ID. This value must be unique.
        /// </summary>
        private int m_id;

        /// <summary>
        /// Holds the unique name of this profession. This value must be unique.
        /// </summary>
        private int m_name;

        /// <summary>
        /// Holds the description of the profession.
        /// </summary>
        private string m_description;

        private int m_hitDice;

        private int m_staminaDice;

        private int m_manaDice;
        #endregion
    }
}
