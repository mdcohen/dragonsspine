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

namespace DragonsSpine.Autonomy.ItemBuilding
{
    public class BookManager
    {
        /// <summary>
        /// Percentage to give a humanoid spell warming profession a book.
        /// </summary>
        public const int HUMANOID_BOOK_CHANCE = 8;

        /// <summary>
        /// Percentage to give a non spell warming profession humanoid a book.
        /// </summary>
        public const int HUMANOID_NONSPELLWARMER_BOOK_CHANCE = 2;

        /// <summary>
        /// Percentage to add a book to lair loot.
        /// </summary>
        public const int LAIR_LOOT_BOOK_CHANCE = 10;

        /// <summary>
        /// Generic book descriptions which are stored in the special variable and parsed upon loading from a player's saved container Items.
        /// </summary>
        private static readonly List<string> BookDescriptions = new List<string>()
        {
            "a delicately inscribed book",
            "a tattered book",
            "a small book",
            "a light book made from thin, flexible sheets of mithril",
            "a tightly bound book",
            "a book made from tough animal hide and inscribed with runes",
            "an old, brittle book",
            "a frayed book",
            "a freshly bound book",
            "a velvety book",
            "a plain book",
            "a vellum book",
            "an ancient book"
        };

        /// <summary>
        /// Possibly added after book base descriptions. Before a title, if there is one.
        /// </summary>
        private static readonly List<string> BookLongDescriptions = new List<string>()
        {
            "with a cover inscribed by warped red runes",
            "with a cover inscribed by glowing green runes",
            "with a cover inscribed by faint blue runes",
        };

        /// <summary>
        /// Key = book title.
        /// Value = book contents. Each index is a page.
        /// </summary>
        private static readonly Dictionary<string, string[]> BookContents = new Dictionary<string, string[]>()
        {
            //BookContents.Add("The Unleashing of Kur", new string[] {"One", "Two"});
        };
    }
}
