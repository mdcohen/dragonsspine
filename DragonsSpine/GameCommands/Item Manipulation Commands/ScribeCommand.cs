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
    [CommandAttribute("scribe", "Scribe a spell from a scroll into your spellbook.", (int)Globals.eImpLevel.USER, 3, new string[] { "scribe <item>" }, Globals.ePlayerState.PLAYING)]
    public class ScribeCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.CommandWeight > 3)
            {
                chr.WriteToDisplay("Command weight limit exceeded. Scribe command not processed.");
                return true;
            }

            // TODO: Implement a scribe skill, ability or tradeskill.
            if (Array.IndexOf(GameWorld.World.SpellWarmingProfessions, chr.BaseProfession) == -1 || chr.Level < Globals.MIN_SCRIBE_LEVEL)
            {
                chr.WriteToDisplay("You do not know how to scribe spells.");
                return true;
            }
            else
            {
                var scroll = chr.FindHeldItem(args);

                if (scroll == null)
                {
                    chr.WriteToDisplay("You are not holding a " + args + ".");
                    return true;
                }

                var spell = GameSpell.GetSpell(scroll.spell);

                if (spell == null || !spell.IsClassSpell(chr.BaseProfession))
                {
                    chr.WriteToDisplay("You cannot scribe this " + scroll.name + ".");
                    return true;
                }

                if (!GameWorld.Map.IsNextToScribingCrystal(chr))
                {
                    chr.WriteToDisplay("You are not in the proper location to scribe a spell.");
                    return true;
                }

                var spellbook = chr.FindHeldItem(Item.ID_SPELLBOOK);

                if (spellbook == null || spellbook.attunedID != chr.UniqueID)
                {
                    chr.WriteToDisplay("You must be holding your spellbook when scribing a spell.");
                    return true;
                }

                if (!spell.IsFoundForScribing)
                {
                    chr.WriteToDisplay("You cannot scribe this " + scroll.name + ". It may be cast by reading it aloud.");
                    return true;
                }

                if (chr.spellDictionary.ContainsKey(spell.ID))
                {
                    chr.WriteToDisplay("You already know the spell " + spell.Name + ".");
                    return true;
                }

                if (Skills.GetSkillLevel(chr.magic) < spell.RequiredLevel)
                {
                    chr.WriteToDisplay("You are not skilled enough to scribe the spell " + spell.Name + ".");
                    return true;
                }

                // success
                GameSpell.ScribeSpell(chr, spell);

                if (scroll == chr.RightHand)
                    chr.UnequipRightHand(scroll);
                else if (scroll == chr.LeftHand)
                    chr.UnequipLeftHand(scroll);

                chr.WriteToDisplay("The " + scroll.name + " disintegrates.");
            }

            return true;
        }
    }
}
