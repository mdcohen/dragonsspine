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
    [CommandAttribute("showspells", "Display a list of known spells.", (int)Globals.eImpLevel.USER, new string[] { "show spells", "show spell", "spellbook", "show spellbook" },
        0, new string[] { "There are no arguments for the show spells command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowSpellsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            System.Collections.Generic.List<GameSpell> spells = new System.Collections.Generic.List<GameSpell>();

            foreach (int id in chr.spellDictionary.Keys)
                spells.Add(GameSpell.GetSpell(id));

            if (chr.IsHybrid) // Currently only Knights and Ravagers
            {
                if (!chr.HasKnightRing)
                {
                    chr.WriteToDisplay("You are not equipped with the proper item to cast spells.");
                    return true;
                }

                foreach (GameSpell gamespell in GameSpell.GameSpellDictionary.Values)
                {
                    if (gamespell.IsClassSpell(chr.BaseProfession) && gamespell.IsAvailableAtTrainer && !spells.Contains(gamespell))
                        spells.Add(gamespell);
                }
            }

            spells.Sort(delegate(GameSpell spell1, GameSpell spell2)
            {
                int compareLevel = spell1.RequiredLevel.CompareTo(spell2.RequiredLevel);
                if (compareLevel == 0)
                    return spell2.Name.CompareTo(spell2.Name);
                return compareLevel;
            });

            if (spells.Count > 0)
            {
                chr.WriteToDisplay("Known Spells:");
                foreach (GameSpell spell in spells)
                {
                    if (chr.spellDictionary.ContainsKey(spell.ID))
                        chr.WriteToDisplay(spell.Name + " (" + spell.Command + ") " + chr.spellDictionary[spell.ID] + (chr.spellDictionary[spell.ID] == chr.MemorizedSpellChant ? " [MEMORIZED]":""));
                    else chr.WriteToDisplay(spell.Name + " (" + spell.Command + ")");
                }
            }
            else chr.WriteToDisplay("You do not know any spells.");            

            return true;
        }
    }
}
