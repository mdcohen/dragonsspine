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
    public interface ISpellHandler
    {
        /// <summary>
        /// Holds the GameSpell for the handler.
        /// This is set in the GameSpell constructor when GameSpells are loaded at server startup and is used to reference non-static GameSpell members and variables.
        /// Future, further implementation of this interfaced property include casting GameSpells that are chained to other GameSpells.
        /// </summary>
        GameSpell ReferenceSpell
        { get; set; }

        /// <summary>
        /// Called when an attempt is made to cast a spell.
        /// </summary>
        /// <param name="caster">The Character casting the spell.</param>
        /// <param name="args">Spell arguments.</param>
        /// <returns>True if the spell was successfully cast.</returns>
        bool OnCast(Character caster, string args);
    }
}
