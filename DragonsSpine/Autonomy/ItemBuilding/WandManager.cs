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
    // Random wands consist of a handle, shaft and from 1 to 3 cores. Value and power of the wand are dependent upon construction.
    public static class WandManager
    {
        /// <summary>
        /// Percentage to give a humanoid spell warming profession a wand.
        /// </summary>
        public const int HUMANOID_WAND_CHANCE = 2;

        /// <summary>
        /// Percentage to give a non spell warming profession humanoid a wand.
        /// </summary>
        public const int HUMANOID_NONSPELLWARMER_WAND_CHANCE = 0;

        /// <summary>
        /// Percentage to add a spell scroll to lair loot.
        /// </summary>
        public const int LAIR_LOOT_WAND_CHANCE = 3;

        #region Wand Handles
        // Wand Handles - Common
        private static readonly List<string> WandHandlesCommon = new List<string>()
        {
            "alder",
            "buckeye wood",
            "elm",
            "hazelwood",
            "holly",
            "inkwood",
            "oak",
            "palm wood",
            "poplar",
            "willow",
        };

        // Wand Handles - Uncommon
        private static readonly List<string> WandHandlesUncommon = new List<string>()
        {
            "baywood",
            "blackthorn wood",
            "dogwood",
            "ebony",
            "fiddlewood",
            "ironbark",
            "lancewood",
            "onyx",
            "pawpaw wood",
            "sandalwood",
        };

        // Wand Handles - Rare
        private static readonly List<string> WandHandlesRare = new List<string>()
        {
            "diamond",
            "magnolia",
            "onyx",
            "yew",
        };
        #endregion

        #region Wand Shafts
        // Wand Shafts - Common
        private static readonly List<string> WandShaftsCommon = new List<string>()
        {
            "ashwood",
            "aspen",
            "birch",
            "chestnut",
            "cycad",
            "dogwood",
            "elm",
            "hazelnut",
            "holly",
            "hornbeam",
            "juniper",
            "oak",
            "pear wood",
            "poisonwood",
            "sycamore",
        };

        // Wand Shafts - Uncommon
        private static readonly List<string> WandShaftsUncommon = new List<string>()
        {
            "alder",
            "cypress",
            "ebony",
            "ironbark",
            "mahogany",
            "oleander",
            "recdwood",
            "strongbark",
            "vinewood",
            "yew",
        };

        // Wand Shafts - Rare
        private static readonly List<string> WandShaftsRare = new List<string>()
        {
            "blackthorn wood",
            "diamond",
            "lancewood",
            "onyx",
        };
        #endregion

        #region Wand Cores
        // Wand Cores - Common (Core 3)
        private static readonly List<string> WandCoresCommon = new List<string>()
        {
            "basilisk tail",
            "black bear fur",
            "crow feather",
            "dog whiskers",
            "dwarf beard hair",
            "gnome beard hair",
            "goblin hair",
            "harpy hair",
            "owl feather",
            "rat hair",
            "tortoise shell",
            "troll skin",
            "wolf hair",
        };

        // Wand Cores - Uncommon (Core 2)
        private static readonly List<string> WandCoresUncommon = new List<string>()
        {
            "chimera hair",
            "basilisk scale",
            "ent leaves",
            "fairy wing",
            "gargoyle dust",
            "gray elf hair",
            "griffin claw",
            "griffin feather",
            "hellhound tuft",
            "red dragon wing",
            "vampire fang",
            "werewolf whiskers",
        };

        // Wand Cores - Rare (Core 3)
        private static readonly List<string> WandCoresRare = new List<string>()
        {
            "chimera scale",
            "demon scale",
            "fire elemental ash",
            "living spiders",
            "mermaid scale",
            "nightmare mane",
            "troll heartstring",
            "unicorn tail hair",
            "pegasus mane",
        }; 
        #endregion

        /// <summary>
        /// Create a wand with a spell and all/some/no charges to be given to an NPC. The wand is currently placed in the NPC's sack or in lair loot.
        /// </summary>
        /// <param name="npc">The NPC to receive the spell wand.</param>
        /// <returns></returns>
        public static Item CreateSpellWand(NPC npc)
        {
            Item wand = null;

            try
            {
                var spellChoices =
                    from spell in GameSpell.GameSpellDictionary.Values
                    where npc.Level >= (spell.RequiredLevel - 1) && !spell.ClassTypes.Contains(Character.ClassType.None) && spell.IsFoundForCasting && !spell.OnlyFoundInLairs
                    select spell;

                GameSpell chosenSpell = spellChoices.ElementAt(Rules.Dice.Next(spellChoices.Count()));

                //int count = 0;

                //while (chosenSpell.OnlyFoundInLairs && !npc.lairCritter && count <= 10)
                //{
                //    chosenSpell = spellChoices.ElementAt(Rules.Dice.Next(spellChoices.Count()));
                //    count++;
                //}

                //// This kind of throws off calculations for lair scrolls. But oh well. Feeling a tad lazy. Let's add a check in disbursing lair loot in LootManager.
                //if (chosenSpell.OnlyFoundInLairs && !npc.lairCritter)
                //    return null;

                //if (spellChoices.Count() > 0)
                //    scroll = CreateSpellScroll(chosenSpell);

                if (wand != null)
                {
                    Utils.Log(wand.GetLogString() + " created for NPC: " + npc.GetLogString(), Utils.LogType.LootWand);
                    if (wand.vRandLow > 0) { wand.coinValue = Rules.Dice.Next(wand.vRandLow * 2, wand.vRandHigh * 2); }
                }
            }
            catch (System.Exception e)
            {
                Utils.Log(npc.GetLogString() + " has an issue with WandManager.CreateSpellWand", Utils.LogType.LootWarning);
                Utils.LogException(e);
            }

            return wand;
        }

        public static Item CreateDormantWand(NPC npc)
        {
            // Fifty percent chance to create a wand for starters.
            if (Rules.Dice.Next(1, 100) >= 50)
            {
                // wands need a shaft, handle and 1 to 3 cores
                // Find out if it's going to be a common, uncommon or rare wand.
                //
                // First things first you need a shaft.
               
            }
            return null;
        }

        /// <summary>
        /// Create a random spell scroll. Either for scribing or casting, based on GameSpell variables and random dice rolls.
        /// </summary>
        /// <param name="spell">The GameSpell to be scribed or cast from the scroll.</param>
        /// <returns></returns>
        //public static Item CreateSpellScroll(GameSpell spell)
        //{
        //    var scroll = Item.CopyItemFromDictionary(Item.ID_WORN_BLANK_SCROLL);

        //    scroll.spell = spell.ID;
        //    scroll.spellPower = -1;
        //    scroll.longDesc = ScrollDescriptions[Rules.Dice.Next(ScrollDescriptions.Count)];
        //    scroll.identifiedName = "Scroll of " + spell.Name;
        //    scroll.notes = "Scroll of " + spell.Name + " (scribed)";
        //    scroll.special = "longDesc:" + scroll.longDesc + " scrollSpellID:" + spell.ID;
        //    scroll.coinValue = spell.TrainingPrice;
        //    scroll.charges = 0;

        //    if ((spell.IsFoundForCasting && !spell.IsFoundForScribing) || (spell.IsFoundForCasting && Rules.RollD(1, 100) <= 30))
        //    {
        //        scroll.spellPower = spell.RequiredLevel + (Rules.RollD(1, 6));
        //        scroll.coinValue = scroll.coinValue / 2;
        //        scroll.charges = 1;
        //        scroll.notes = "Scroll of " + spell.Name + " (cast only)";
        //    }

        //    return scroll;
        //}

        //public static Item CreateSpellScroll(string scrollSpecial)
        //{
        //    string longDesc = "";

        //    try
        //    {
        //        if (scrollSpecial.Contains("longDesc:"))
        //        {
        //            scrollSpecial = scrollSpecial.Replace("longDesc:", "");
        //            longDesc = scrollSpecial.Substring(0, scrollSpecial.IndexOf("scrollSpellID:"));
        //            //longDesc = scrollSpecial.Substring(scrollSpecial.IndexOf("longDesc:") + "longDesc:".Length, scrollSpecial.IndexOf("scrollSpellID:") - "longDesc:".Length).Trim();
        //            scrollSpecial = scrollSpecial.Replace(longDesc, "");
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        Utils.LogException(ex);
        //        longDesc = "";
        //    }

        //    int spellID = -1;

        //    //ScrollSpecial at this point is 'longDesc:scrollSpellID:....' So I added the string so that it correctly finds the length  -dw
        //    try
        //    {
        //        if (scrollSpecial.Contains("scrollSpellID:"))
        //        {
        //            scrollSpecial = scrollSpecial.Replace("scrollSpellID:", "");
        //            //spellID = System.Convert.ToInt32(scrollSpecial.Substring(scrollSpecial.IndexOf("longDesc:scrollSpellID:") + "longDesc:scrollSpellID:".Length, scrollSpecial.Length - "longDesc:scrollSpellID:".Length));
        //            spellID = System.Convert.ToInt32(scrollSpecial);
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        Utils.LogException(ex);
        //        spellID = -1;
        //    }

        //    var scroll = Item.CopyItemFromDictionary(Item.ID_WORN_BLANK_SCROLL);

        //    if (longDesc == "" || spellID == -1) return scroll; // failed to recreate the scribed scroll

        //    GameSpell spell = GameSpell.GetSpell(spellID);

        //    scroll.spell = spell.ID;
        //    scroll.spellPower = -1;
        //    scroll.longDesc = longDesc;
        //    scroll.identifiedName = "Scroll of " + spell.Name;
        //    scroll.notes = "Scroll of " + spell.Name + " (scribed)";
        //    scroll.special = "longDesc:" + scroll.longDesc + " scrollSpellID:" + spell.ID;
        //    scroll.coinValue = spell.TrainingPrice;
        //    scroll.charges = 0;

        //    return scroll;
        //}
    }
}