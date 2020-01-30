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
using GameSpell = DragonsSpine.Spells.GameSpell;

namespace DragonsSpine.Commands
{
    [CommandAttribute("flip", "Flip through a book.", (int)Globals.eImpLevel.USER, new string[] { "turn" },
        0, new string[] { "flip", "flip (to) #", "flip back", "flip forward" }, Globals.ePlayerState.PLAYING)]
    public class FlipCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            Book book;

            int pagesToFlip = 0;
            int iArgCount = 0;

            if (chr.RightHand != null && chr.RightHand.baseType == Globals.eItemBaseType.Book) // get the book from hand
            {
                book = (Book)chr.RightHand;
            }
            else if (chr.LeftHand != null && chr.LeftHand.baseType == Globals.eItemBaseType.Book)
            {
                book = (Book)chr.LeftHand;
            }
            else
            {
                chr.WriteToDisplay("You are not holding a book.");
                return true;
            }

            try
            {
                if (args == null)
                {
                    args = "1"; // flip all alone should advance one page
                }

                String[] sArgs = args.Split(" ".ToCharArray());
                iArgCount = sArgs.GetUpperBound(0) + 1;
                // Start filtering out too many/wrong args 
                if (iArgCount > 3 || (iArgCount == 3 && (sArgs[0].ToLower() != "book" && sArgs[0].ToLower() != "spellbook"))) // FLIP Book TO #
                {
                    chr.WriteToDisplay("Usage: Flip [back / forward / to] [number]");
                    return true;
                }
                if (iArgCount == 3) // only way this is true is if first arg is "book" or "spellbook", so we eliminate it
                {
                    sArgs[0] = sArgs[1];
                    sArgs[1] = sArgs[2];
                    iArgCount = 2;
                }

                if (iArgCount == 2) // make sure the second argument, if there is one, is numeric
                {
                    try
                    {
                        pagesToFlip = Convert.ToInt32(sArgs[1]);
                    }
                    catch
                    {
                        chr.WriteToDisplay("Format: Flip [back / forward / to] [number]");
                        return true;
                    }

                }
                switch (sArgs[0].ToLower())
                {
                    case "back":
                        if (iArgCount == 1) // FLIP BACK
                        {
                            pagesToFlip = -1;
                        }
                        else // FLIP BACK #
                        {
                            pagesToFlip = pagesToFlip * -1;
                        }
                        break;
                    case "forward":
                        pagesToFlip = pagesToFlip * 1;
                        break;
                    case "to":
                        pagesToFlip = pagesToFlip - book.CurrentPage;//pagesToFlip = pagesToFlip - 1;//pagesToFlip = pagesToFlip - currentPage;
                        break;
                    case "page":  // FLIP PAGE - turn one page
                        pagesToFlip = 1;
                        break;
                    case "book":  // READ Book ends up here.  So just read the current page.
                        break;
                    default: // either it's a number or it's bad input
                        try
                        {
                            pagesToFlip = Convert.ToInt32(sArgs[0]);
                        }
                        catch
                        {
                            chr.WriteToDisplay("Format: Flip [back / forward / to] [number]");
                            return true;
                        }
                        break;
                }
                if (book.CurrentPage + pagesToFlip <= 0) // the book does not have a negative amount of pages, set to 0
                {
                    book.CurrentPage = 1;
                }
                else if (book.BookType != Book.BookTypes.Spellbook && book.CurrentPage + pagesToFlip > (int)book.Pages.Length / 2)
                {
                    book.CurrentPage = (int)(book.Pages.Length / 2) - 1;
                }
                else
                {
                    book.CurrentPage = book.CurrentPage + pagesToFlip;
                }

                if (book.BookType == Book.BookTypes.Spellbook)
                {
                    if (book.attunedID != chr.UniqueID) // do not allow a player to read another player's spellbook
                    {
                        chr.WriteToDisplay("The book is sealed shut.");
                        return true;
                    }
                    else
                    {
                        List<int> knownSpellIDs = new List<int>(chr.spellDictionary.Keys);
                        List<string> knownSpellChants = new List<string>(chr.spellDictionary.Values);

                        if (book.CurrentPage > chr.spellDictionary.Count) // page is blank
                        {
                            chr.WriteToDisplay("That page is blank.");
                            return true;
                        }
                        int spellID = knownSpellIDs[book.CurrentPage - 1];
                        chr.WriteToDisplay("Page " + (book.CurrentPage) + ":");
                        GameSpell spell = GameSpell.GetSpell(spellID);
                        chr.WriteToDisplay("The incantation for " + spell.Name + " (" + spell.Command + ")");
                        chr.WriteToDisplay(knownSpellChants[book.CurrentPage - 1]);
                    }
                }
                else
                {
                    chr.WriteToDisplay(book.ReadPage());
                }
            }
            catch (Exception e)
            {
                Utils.Log("Command.flip(" + args + ") by " + chr.GetLogString(), Utils.LogType.CommandFailure);
                Utils.LogException(e);
                chr.WriteToDisplay("That page was not found.");
                return true;
            }

            return true;
        }
    }
}
