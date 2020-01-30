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
using GameSpell = DragonsSpine.Spells.GameSpell;

namespace DragonsSpine.Commands
{
    [CommandAttribute("read", "Read from a book or scroll.", (int)Globals.eImpLevel.USER, 0, new string[] { "read <item>" }, Globals.ePlayerState.PLAYING)]
    public class ReadCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args.IndexOf("book") != -1) // "read book" is the same as "flip book"
            {
                return CommandTasker.ParseCommand(chr, "flip", args);
            }

            bool rightHand = true;

            if (args == null)
            {
                if (chr.RightHand != null && chr.RightHand.baseType == Globals.eItemBaseType.Book)
                {
                    return CommandTasker.ParseCommand(chr, "flip", args);
                }

                if (chr.RightHand != null && chr.RightHand.baseType == Globals.eItemBaseType.Scroll)
                {
                    goto readScroll;
                }

                if (chr.LeftHand != null && chr.LeftHand.baseType == Globals.eItemBaseType.Book)
                {
                    return CommandTasker.ParseCommand(chr, "flip", args);
                }

                if (chr.LeftHand != null && chr.LeftHand.baseType == Globals.eItemBaseType.Scroll)
                {
                    rightHand = false;
                }
            }

        readScroll:
            // verification
            if ((rightHand && (chr.RightHand == null || chr.RightHand.baseType != Globals.eItemBaseType.Scroll)) || (!rightHand &&
                (chr.LeftHand == null || chr.LeftHand.baseType != Globals.eItemBaseType.Scroll)))
            {
                chr.WriteToDisplay("You are unable to read that. Try bringing it to someone who can.");
                return true;
            }

            Item scroll = rightHand ? chr.RightHand : chr.LeftHand;

            if ((scroll as Book).Pages == null || (scroll as Book).Pages[0] == "")
            {
                if (scroll.spell > -1)
                {
                    GameSpell scrollSpell = GameSpell.GetSpell(chr.RightHand.spell);
                    
                    if (!scrollSpell.IsClassSpell(chr.BaseProfession))
                    {
                        chr.WriteToDisplay("The " + scroll.name + " contains runes that are illegible to you.");
                    }
                    else
                    {
                        if (chr.spellDictionary.ContainsKey(scrollSpell.ID))
                        {
                            chr.WriteToDisplay("The " + chr.RightHand.name + " contains the spell of " + scrollSpell.Name + ".");
                            // TODO: Implement the ability to see how many charges and perhaps how powerful the spell is.
                        }
                        else
                        {
                            chr.WriteToDisplay("The " + scroll.name + " contains a " + Utils.FormatEnumString(chr.BaseProfession.ToString()).ToLower() +
                                " spell that is unfamiliar to you." + (Skills.GetSkillLevel(chr.magic) >= scrollSpell.RequiredLevel ? " With enough effort you believe this scroll could be scribed into your spellbook." : ""));
                            // TODO: Should the player be informed that the spell can be scribed or not? -- added 1/25/2019 Eb
                        }
                    }
                }
                else // blank scroll
                {
                    chr.WriteToDisplay("The scroll is blank.");
                    // TODO: Does this mean a spell can be added to the scroll?
                }
            }

            return true;
        }
    }
}
