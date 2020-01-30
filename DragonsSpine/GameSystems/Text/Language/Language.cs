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

namespace DragonsSpine.GameSystems.Text
{
    public abstract class Language
    {
        // Key = English, Value = Language translation
        public Dictionary<string, string> Lexicon = new Dictionary<string, string>();

        public List<string> EnglishWords = new List<string>();

        public List<string> LanguageTranslation = new List<string>();
    }
}
