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

namespace DragonsSpine.GameSystems
{
    /// <summary>
    /// Damage per second calculator. This must be toggled on to collect data.
    /// </summary>
    public static class DPSCalculator
    {
        public enum DamageType
        {
            Unknown, // safety net

            Spell, // could be venom or other Damage over Time?
            SpellResist,
            MeleeWeapon,
            MeleeMiss,
            MeleeFumble, // not currently logged 11/22/2015 Eb
            RangeWeapon,
            RangeWeaponMiss,
            RangeWeaponFumble, // not currently logged 11/22/2015 Eb
            MartialArts,
            MartialArtsMiss,

            // Pets.
            PetSpell,
            PetSpellResist,
            PetMeleeWeapon,
            PetMeleeMiss,
            PetMeleeFumble,
            PetRangeWeapon,
            PetRangeWeaponMiss,
            PetRangeWeaponFumble,
            PetMartialArts,
            PetMartialArtsMiss
        }

        // need a list of damage over time spells -- only one we have now is venom for direct damage, and contagion for indirect damage

        /// <summary>
        /// Key = Unique ID (NPC or PC), Value = List of Tuples(DamageType, GameRound, ItemID or SpellID or -1 (Martial Arts), Damage Dealt)
        /// </summary>
        public static Dictionary<int, List<Tuple<DamageType, int, int, int>>> MasterDPSDictionary = new Dictionary<int, List<Tuple<DamageType, int, int, int>>>();

        /// <summary>
        /// Display DPS statistics.
        /// </summary>
        /// <param name="chr">The character viewing the statistics.</param>
        /// <param name="uniqueID">The unique ID of the Character for whose stats will be viewed.</param>
        /// <returns>False if the unique ID does not exist, true otherwise.</returns>
        public static bool DisplayDPSOutput(Character chr, int uniqueID)
        {
            // display total rounds of combat
            // display total rounds of melee combat
            // display total rounds of range weapon combat
            // display total rounds of martial arts combat
            // display total rounds of spell combat

            // Character enabled DPS but is yet to cause any damage.
            if (!MasterDPSDictionary.ContainsKey(uniqueID) && chr.DPSLoggingEnabled)
            {
                chr.WriteToDisplay("You have not caused any damage to be calculated.");
                return false;
            }

            if (!MasterDPSDictionary.ContainsKey(uniqueID))
            {
                chr.WriteToDisplay("Your damage output is not being logged. Use the command 'toggledps' to enable logging.");
                return false;
            }

            int numRoundsTallied = 0;
            int totalDamage = 0;
            //int numRoundsTotalMeleeCombat = 0;
            //int numRoundsTotalRangeWeaponCombat = 0;
            //int numRoundsTotalMartialArtsCombat = 0;
            //int numRoundsTotalMeleeMisses = 0;
            //int numRoundsTotalRangeWeaponMisses = 0;
            //int numRoundsTotalSpellCombat = 0;
            ////int numRoundsTotalSpellResistsCombat = 0;
            
            // Key = Unique ID (NPC or PC), Value = List of Tuples(DamageType, GameRound, ItemID or SpellID or -1 (Martial Arts), Damage Dealt)

            List<int> roundsTallied = new List<int>();

            // Key = ItemID, SpellID or -1 for unarmed combat, Value = Tuple(Number of Attacks, Damage Total)
            Dictionary<int, Tuple<int, int>> specificWeaponTally = new Dictionary<int, Tuple<int, int>>();

            // DamageType, GameRound, ItemID or SpellID or -1 (Martial Arts), Damage Dealt
            foreach (Tuple<DamageType, int, int, int> tuple in MasterDPSDictionary[uniqueID])
            {
//#if DEBUG
//                Utils.Log("DamageType: " + tuple.Item1.ToString(), Utils.LogType.Debug);
//                Utils.Log("GameRound: " + tuple.Item2, Utils.LogType.Debug);
//                Utils.Log("ID: " + tuple.Item3, Utils.LogType.Debug);
//                Utils.Log("Damage: " + tuple.Item4, Utils.LogType.Debug);
//#endif
                // Tally damage totals for each GameRound.
                if (!roundsTallied.Contains(tuple.Item2)) // GameRound has not already been tallied.
                {
                    roundsTallied.Add(tuple.Item2);
                    numRoundsTallied++;
                    totalDamage += tuple.Item4;
                }
                else // Round already had damage, increment damage total only.
                {
                    totalDamage += tuple.Item4;
                }

                if(!tuple.Item1.ToString().Contains("Spell"))
                {
                // Keep track of the weapon specific damage.
                if (!specificWeaponTally.ContainsKey(tuple.Item3))
                {
                    Tuple<int, int> weaponTuple = new Tuple<int, int>(1, tuple.Item4);
                    specificWeaponTally.Add(tuple.Item3, weaponTuple);
                }
                else
                {
                    Tuple<int, int> weaponTuple = new Tuple<int, int>(specificWeaponTally[tuple.Item3].Item1 + 1, specificWeaponTally[tuple.Item3].Item2 + tuple.Item4);
                    specificWeaponTally.Remove(tuple.Item3);
                    specificWeaponTally.Add(tuple.Item3, weaponTuple);
                }
                }

                //switch (tuple.Item1)
                //{
                //    case DamageType.MartialArts:
                //    case DamageType.MartialArtsMiss:
                //        break;
                //}

                // declassify with a switch, expand on this in the very near future 11/22/2015 Eb
            }

            if (numRoundsTallied > 0)
            {
                chr.WriteToDisplay("Damage Statistics for " + chr.Name);
                chr.WriteToDisplay("Total Combat Rounds : " + numRoundsTallied);
                //chr.WriteToDisplay("Total Combat Time   : " + Utils.RoundsToTimeSpan(numRoundsTallied).ToString());
                chr.WriteToDisplay("Total Damage Output : " + totalDamage);
                chr.WriteToDisplay("Damage Per Round    : " + totalDamage / numRoundsTallied);
                chr.WriteToDisplay("Damage Per Second   : " + totalDamage / Utils.RoundsToTimeSpan(numRoundsTallied).Seconds);

                Utils.Log("-----", Utils.LogType.DPSCalcs);
                Utils.Log("Damage Statistics for " + chr.Name, Utils.LogType.DPSCalcs);
                Utils.Log("Total Combat Rounds : " + numRoundsTallied, Utils.LogType.DPSCalcs);
                //Utils.Log("Total Time          : " + Utils.RoundsToTimeSpan(numRoundsTallied).ToString(), Utils.LogType.DPSCalcs);
                Utils.Log("Total Damage Output : " + totalDamage, Utils.LogType.DPSCalcs);
                Utils.Log("Damage Per Round    : " + totalDamage / numRoundsTallied, Utils.LogType.DPSCalcs);
                Utils.Log("Damage Per Second   : " + totalDamage / Utils.RoundsToTimeSpan(numRoundsTallied).Seconds, Utils.LogType.DPSCalcs);

                foreach (int id in specificWeaponTally.Keys)
                {
                    if (specificWeaponTally[id].Item1 > 0 && specificWeaponTally[id].Item2 > 0)
                    {
                        string weaponName = "Unarmed Combat";

                        if (id > -1) // -1 is used for unarmed combat attacks
                        {
                            Item weapon = Item.CopyItemFromDictionary(id);
                            if (weapon != null)
                                weaponName = weapon.identifiedName;
                            else weaponName = "Unknown";
                        }

                        chr.WriteToDisplay("** Weapon Details for " + chr.Name + " **");
                        chr.WriteToDisplay("Weapon Used           : " + weaponName);
                        chr.WriteToDisplay("Number of Attacks     : " + specificWeaponTally[id].Item1);
                        chr.WriteToDisplay("Total Damage          : " + specificWeaponTally[id].Item2);
                        chr.WriteToDisplay("Avg Damage Per Attack : " + specificWeaponTally[id].Item2 / specificWeaponTally[id].Item1);

                        Utils.Log("** Weapon Details for " + chr.Name + " **", Utils.LogType.DPSCalcs);
                        Utils.Log("Weapon Used           : " + weaponName, Utils.LogType.DPSCalcs);
                        Utils.Log("Number of Attacks     : " + specificWeaponTally[id].Item1, Utils.LogType.DPSCalcs);
                        Utils.Log("Total Damage          : " + specificWeaponTally[id].Item2, Utils.LogType.DPSCalcs);
                        Utils.Log("Avg Damage Per Attack : " + specificWeaponTally[id].Item2 / specificWeaponTally[id].Item1, Utils.LogType.DPSCalcs);
                    }
                }
                Utils.Log("-----", Utils.LogType.DPSCalcs);
                chr.WriteToDisplay("Please note further breakdown of damage types and output will be available in the near future.");
            }
            else
            {
                chr.WriteToDisplay("There is no data to display for your damage output.");
            }
            
            return true;
        }

        /// <summary>
        /// Add melee DPS value for a held item that damaged a target.
        /// </summary>
        /// <param name="uniqueID">Unique ID of the damager.</param>
        /// <param name="gameRound">GameRound the damage was made.</param>
        /// <param name="itemID">Item ID from the item catalog.</param>
        /// <param name="damage">Damage amount.</param>
        public static void AddMeleeWeaponDPSValue(Character chr, int gameRound, int itemID, int damage)
        {
            int uniqueID = chr.UniqueID;

            if (chr.PetOwner != null)
            {
                if (!chr.PetOwner.DPSLoggingEnabled) return;

                uniqueID = chr.PetOwner.UniqueID;
            }

            Tuple<DamageType, int, int, int> tuple = new Tuple<DamageType, int, int, int>(DamageType.MeleeWeapon, gameRound, itemID, damage);

            if (MasterDPSDictionary.ContainsKey(uniqueID))
                MasterDPSDictionary[uniqueID].Add(tuple);
            else MasterDPSDictionary.Add(uniqueID, new List<Tuple<DamageType, int, int, int>>() { tuple });
        }

        public static void AddRangeWeaponDPSValue(Character chr, int gameRound, int itemID, int damage)
        {
            int uniqueID = chr.UniqueID;

            if (chr.PetOwner != null)
            {
                if (!chr.PetOwner.DPSLoggingEnabled) return;

                uniqueID = chr.PetOwner.UniqueID;
            }

            Tuple<DamageType, int, int, int> tuple = new Tuple<DamageType, int, int, int>(DamageType.RangeWeapon, gameRound, itemID, damage);

            if (MasterDPSDictionary.ContainsKey(uniqueID))
                MasterDPSDictionary[uniqueID].Add(tuple);
            else MasterDPSDictionary.Add(uniqueID, new List<Tuple<DamageType, int, int, int>>() { tuple });
        }

        public static void AddSpellDPSValue(Character chr, int gameRound, int spellID, int damage)
        {
            int uniqueID = chr.UniqueID;

            if (chr.PetOwner != null)
            {
                if (!chr.PetOwner.DPSLoggingEnabled) return;

                uniqueID = chr.PetOwner.UniqueID;
            }

            Tuple<DamageType, int, int, int> tuple = new Tuple<DamageType, int, int, int>(DamageType.Spell, gameRound, spellID, damage);

            if (MasterDPSDictionary.ContainsKey(uniqueID))
                MasterDPSDictionary[uniqueID].Add(tuple);
            else MasterDPSDictionary.Add(uniqueID, new List<Tuple<DamageType, int, int, int>>() { tuple });
        }

        public static void AddMartialArtsDPSValue(Character chr, int gameRound, int damage)
        {
            int uniqueID = chr.UniqueID;

            if (chr.PetOwner != null)
            {
                if (!chr.PetOwner.DPSLoggingEnabled) return;

                uniqueID = chr.PetOwner.UniqueID;
            }

            Tuple<DamageType, int, int, int> tuple = new Tuple<DamageType, int, int, int>(DamageType.MeleeWeapon, gameRound, -1, damage);

            if (MasterDPSDictionary.ContainsKey(uniqueID))
                MasterDPSDictionary[uniqueID].Add(tuple);
            else MasterDPSDictionary.Add(uniqueID, new List<Tuple<DamageType, int, int, int>>() { tuple });
        }

        /// <summary>
        /// Log a missed attack or spell resist as it counts in DPS calculation.
        /// </summary>
        /// <param name="uniqueID"></param>
        /// <param name="gameRound"></param>
        /// <param name="itemOrSpellID">-1 for martial arts missed attack.</param>
        /// <param name="damageType"></param>
        public static void AddMissedAttack(Character chr, int gameRound, int itemOrSpellID, DamageType damageType)
        {
            int uniqueID = chr.UniqueID;

            if (chr.PetOwner != null)
            {
                if (!chr.PetOwner.DPSLoggingEnabled) return;

                uniqueID = chr.PetOwner.UniqueID;
            }

            Tuple<DamageType, int, int, int> tuple = new Tuple<DamageType, int, int, int>(damageType, gameRound, itemOrSpellID, 0);

            if (MasterDPSDictionary.ContainsKey(uniqueID))
                MasterDPSDictionary[uniqueID].Add(tuple);
            else MasterDPSDictionary.Add(uniqueID, new List<Tuple<DamageType, int, int, int>>() { tuple });
        }

        public static void AddSpellDPSValue(Character chr, int gameRound, string spellType, int totalDamage)
        {
            int uniqueID = chr.UniqueID;

            if (chr.PetOwner != null)
            {
                if (!chr.PetOwner.DPSLoggingEnabled) return;

                uniqueID = chr.PetOwner.UniqueID;
            }

            Spells.GameSpell.GameSpellID id = Spells.GameSpell.GameSpellID.None;

            // TODO: Combat.DoSpellDamage needs rewriting to use GameSpell.GameSpellID or another "spell/effect type enum" 11/22/2015 Eb
            Enum.TryParse<Spells.GameSpell.GameSpellID>(spellType, true, out id);

            Tuple<DamageType, int, int, int> tuple = new Tuple<DamageType, int, int, int>(DamageType.Spell, gameRound, (int)id, totalDamage);

            if (MasterDPSDictionary.ContainsKey(uniqueID))
                MasterDPSDictionary[uniqueID].Add(tuple);
            else MasterDPSDictionary.Add(uniqueID, new List<Tuple<DamageType, int, int, int>>() { tuple });
        }
    }
}
