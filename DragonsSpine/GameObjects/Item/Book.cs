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
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DragonsSpine
{
    /// <summary>
    /// Summary description for Book.
    /// </summary>
    public class Book : Item
    {

        public enum BookTypes { None, Normal, Spellbook, Scroll }

        public Book.BookTypes BookType = Book.BookTypes.None;
        public int MaxPages = 0; // max pages this book has
        public int CurrentPage = 0;
        public string[] Pages = null;
		
        public Book() : base()
        {
            CurrentPage = 0;
        }

        public Book(System.Data.DataRow dr) : base(dr)
        {
            this.BookType = (Book.BookTypes)Enum.Parse(typeof(Book.BookTypes), dr["bookType"].ToString());
            this.MaxPages = Convert.ToInt16(dr["maxPages"]);
            this.Pages = dr["pages"].ToString().Split(ProtocolYuusha.ISPLIT.ToCharArray());
        }

        public string ReadPage()
        {
            //Will read the same page over and over again

            int mp = Pages.Length/2; // do this to filter out the blank pages

            if (this.CurrentPage + 1 > mp)
            {
                this.CurrentPage = 0;
            }

            if (this.CurrentPage < 0)
            {
                this.CurrentPage = 0;
            }

            List<string> bookPages = new List<string>();

            foreach (string text in this.Pages)
            {
                if (text == "")
                {
                    continue;
                }
                bookPages.Add(text);
            }

            return bookPages[this.CurrentPage];
        }
    }
}
