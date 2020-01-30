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
using System.Collections.Generic;
using System.Linq;
using GameSpell = DragonsSpine.Spells.GameSpell;

namespace DragonsSpine.Autonomy.ItemBuilding
{
    public static class ScrollManager
    {
        /// <summary>
        /// Percentage to give a humanoid spell warming profession a spell scroll.
        /// </summary>
        public const int HUMANOID_SCROLL_CHANCE = 8;

        /// <summary>
        /// Percentage to give a non spell warming profession humanoid a spell scroll.
        /// </summary>
        public const int HUMANOID_NONSPELLWARMER_SCROLL_CHANCE = 2;

        /// <summary>
        /// Percentage to add a spell scroll to lair loot.
        /// </summary>
        public const int LAIR_LOOT_SCROLL_CHANCE = 10;

        /// <summary>
        /// Generic scroll descriptions which are stored in the special variable and parsed upon loading from a player's saved container Items.
        /// </summary>
        private static readonly List<string> ScrollDescriptions = new List<string>()
        {
            "a delicately inscribed scroll",
            "a tattered scroll inscribed with runes",
            "a small scroll inscribed with runes",
            "a rune-inscribed scroll made from a thin, flexible sheet of mithril",
            "a smooth scroll inscribed with runes",
            "a scroll made from tough animal hide and inscribed with runes",
            "an old, brittle parchment inscribed with runes",
            "a frayed scroll inscribed with runes",
            "a fresh scroll inscribed with runes",
            "a velvety scroll inscribed with runes",
            "a plain scroll inscribed with runes",
            "a scroll made of an unknown and unfortunate being's flesh",
            "a thin silver leaf scroll with runes",
            "a vellum scroll inscribed with runes",
            "an ancient scroll inscribed with runes"
        };

        /// <summary>
        /// Create a scroll (scribeable) to be given to an NPC. The scroll is currently placed in the NPC's sack or in lair loot.
        /// </summary>
        /// <param name="npc">The NPC to receive the spell scroll.</param>
        /// <returns></returns>
        public static Item CreateUnavailableSpellScroll(NPC npc)
        {
            Item scroll = null;

            try
            {
                var spellChoices =
                    from spell in GameSpell.GameSpellDictionary.Values
                    where !spell.OnlyFoundInLairs && (spell.IsFoundForCasting || spell.IsFoundForScribing) && npc.Level >= (spell.RequiredLevel - 1)
                    && !spell.ClassTypes.Contains(Character.ClassType.None)
                    select spell;

                if (npc.lairCritter)
                {
                    spellChoices =
                    from spell in GameSpell.GameSpellDictionary.Values
                    where (spell.IsFoundForCasting || spell.IsFoundForScribing) && npc.Level >= (spell.RequiredLevel - 1)
                    && !spell.ClassTypes.Contains(Character.ClassType.None) && spell.OnlyFoundInLairs
                    select spell;
                }

                if (spellChoices.Count() <= 0) // no spells were found
                    return null;

                GameSpell chosenSpell = spellChoices.ElementAt(Rules.Dice.Next(spellChoices.Count()));

                int loopCount = 0;

                while (chosenSpell.OnlyFoundInLairs && !npc.lairCritter && loopCount < spellChoices.Count())
                {
                    chosenSpell = spellChoices.ElementAt(Rules.Dice.Next(0, spellChoices.Count() - 1));
                    loopCount++;
                }

                // This kind of throws off calculations for lair scrolls. But oh well. Feeling a tad lazy. Let's add a check in disbursing lair loot in LootManager.
                if (chosenSpell.OnlyFoundInLairs && !npc.lairCritter)
                    return null;

                if (spellChoices.Count() > 0)
                    scroll = CreateSpellScroll(chosenSpell);

                if (scroll != null)
                {
                    Utils.Log(scroll.GetLogString() + " created for NPC: " + npc.GetLogString(), Utils.LogType.LootScroll);
                    if (scroll.vRandLow > 0) { scroll.coinValue = Rules.Dice.Next(scroll.vRandLow * 2, scroll.vRandHigh * 2); }
                }
            }
            catch(System.Exception e)
            {
                Utils.Log(npc.GetLogString() + " has an issue with ScrollManager.CreateUnavailableSpellScroll", Utils.LogType.LootWarning);
                Utils.LogException(e);
            }

            return scroll;
        }

        /// <summary>
        /// Create a random spell scroll. Either for scribing or casting, based on GameSpell variables and random dice rolls.
        /// </summary>
        /// <param name="spell">The GameSpell to be scribed or cast from the scroll.</param>
        /// <returns></returns>
        public static Item CreateSpellScroll(GameSpell spell)
        {
            var scroll = Item.CopyItemFromDictionary(Item.ID_WORN_BLANK_SCROLL);

            scroll.spell = spell.ID;
            scroll.spellPower = -1;
            scroll.longDesc = ScrollDescriptions[Rules.Dice.Next(ScrollDescriptions.Count)];
            scroll.identifiedName = "Scroll of " + spell.Name;
            scroll.notes = "Scroll of " + spell.Name + " (scribed)";
            scroll.special = "longDesc:" + scroll.longDesc + " scrollSpellID:" + spell.ID;
            scroll.coinValue = spell.TrainingPrice / 4;
            scroll.charges = 0;

            if ((spell.IsFoundForCasting && !spell.IsFoundForScribing) || (spell.IsFoundForCasting && Rules.RollD(1, 100) <= 30))
            {
                scroll.spellPower = spell.RequiredLevel + Rules.RollD(1, 6);
                scroll.coinValue = scroll.coinValue / 2;
                scroll.charges = 1;
                scroll.notes = "Scroll of " + spell.Name + " (cast only)";
            }

            return scroll;
        }

        /// <summary>
        /// Spell scrolls loaded from saved player item data.
        /// </summary>
        /// <param name="scrollSpecial"></param>
        /// <returns></returns>
        public static Item CreateSpellScroll(string scrollSpecial)
        {
            string longDesc = "";

            try
            {
                if (scrollSpecial.Contains("longDesc:"))
                {
                    scrollSpecial = scrollSpecial.Replace("longDesc:", "");
                    longDesc = scrollSpecial.Substring(0, scrollSpecial.IndexOf("scrollSpellID:"));
                    //longDesc = scrollSpecial.Substring(scrollSpecial.IndexOf("longDesc:") + "longDesc:".Length, scrollSpecial.IndexOf("scrollSpellID:") - "longDesc:".Length).Trim();
                    scrollSpecial = scrollSpecial.Replace(longDesc, "");
                }
            }
            catch (System.Exception ex)
            {
                Utils.LogException(ex);
                longDesc = "";
            }

            //scrollSpecial = scrollSpecial.Replace(longDesc, ""); 
            //scrollSpecial = scrollSpecial.Trim();

            int spellID = -1;

            //ScrollSpecial at this point is 'longDesc:scrollSpellID:....' So I added the string so that it correctly finds the length  -dw
            try
            {
                if (scrollSpecial.Contains("scrollSpellID:"))
                {
                    scrollSpecial = scrollSpecial.Replace("scrollSpellID:", "");
                    //spellID = System.Convert.ToInt32(scrollSpecial.Substring(scrollSpecial.IndexOf("longDesc:scrollSpellID:") + "longDesc:scrollSpellID:".Length, scrollSpecial.Length - "longDesc:scrollSpellID:".Length));
                    spellID = System.Convert.ToInt32(scrollSpecial);
                }
            }
            catch(System.Exception ex)
            {
                Utils.LogException(ex);
                spellID = -1;
            }

            var scroll = Item.CopyItemFromDictionary(Item.ID_WORN_BLANK_SCROLL);

            if (longDesc == "" || spellID == -1) return scroll; // failed to recreate the scribed scroll

            GameSpell spell = GameSpell.GetSpell(spellID);

            scroll.spell = spell.ID;
            scroll.spellPower = -1;
            scroll.longDesc = longDesc;
            scroll.identifiedName = "Scroll of " + spell.Name;
            scroll.notes = "Scroll of " + spell.Name + " (scribed)";
            scroll.special = "longDesc:" + scroll.longDesc + " scrollSpellID:" + spell.ID;
            scroll.coinValue = spell.TrainingPrice;
            scroll.charges = 0;

            return scroll;
        }
    }
}