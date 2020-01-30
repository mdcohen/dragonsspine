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

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Transmute, "transmute", "Transmute", "Transmute material items into a desired precious metal.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Self, 27, 10, 175000, "0271", false, true, false, true, false, Character.ClassType.Thief)]
    public class TransmuteSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            if (caster.CurrentCell.Items.Count <= 0)
            {
                caster.WriteToDisplay("There is nothing here to transmute.");
                return false;
            }
            
            // args: copper (skill 12), silver (skill 13), gold (skill 14)

            ReferenceSpell.SendGenericCastMessage(caster, null, true);

            List<Item> transmutableItems = new List<Item>();

            double transmuteValue = 0;

            // Calculate transmuted items value.
            foreach (Item groundItem in caster.CurrentCell.Items)
            {
                if (groundItem.itemType == Globals.eItemType.Corpse) continue; // No corpses.
                if (groundItem.coinValue <= 0) continue; // No items with negative coin value.
                if (groundItem.attunedID > 0) continue; // No attuned (bound) items.

                double previousTransmuteValue = transmuteValue;

                // Transmuted value depends on item type.
                if (groundItem.itemID == Item.ID_GOLD_NUGGET) transmuteValue += groundItem.coinValue;
                //else if (groundItem.itemID == Item.ID_GOLDRING || groundItem.itemID == Item.ID_RECALLRING) transmuteValue += groundItem.coinValue * .9;
                else if (groundItem.itemType == Globals.eItemType.Coin) transmuteValue += groundItem.coinValue * .85;
                else transmuteValue += groundItem.coinValue * .92;

                if (previousTransmuteValue < transmuteValue) transmutableItems.Add(groundItem);
            }

            // Remove transmuted items.
            foreach (Item item in transmutableItems)
                if (caster.CurrentCell.Items.Contains(item)) caster.CurrentCell.Remove(item);

            // Create gold nugget of the transmuted value.
            var nugget = Item.CopyItemFromDictionary(Item.ID_GOLD_NUGGET);
            nugget.coinValue = Convert.ToInt32(transmuteValue);

            // Spell failes if transmuted nugget is worthless.
            if (nugget.coinValue <= 0)
                return false;

            // Send message to transmuter.
            caster.WriteToDisplay("You call upon the unseen powers of Shadow to transmute your spoils into gold.");

            // Add transmuted nugget to caster's CurrentCell.
            caster.CurrentCell.Add(nugget);

            // Added skill gain. This may need tweaking as it could possibly be abused.
            if (caster.IsSpellWarmingProfession)
                Skills.GiveSkillExp(caster, (int)transmuteValue, Globals.eSkillType.Magic);

            //string transmuteType = "copper";

            //if(!String.IsNullOrEmpty(args))
            //{
            //    switch(args.ToLower())
            //    {
            //        case "silver":
            //            transmuteType = "silver";
            //            break;
            //        case "gold":
            //            transmuteType = "gold";
            //            break;
            //        default:
            //            break;
            //    }
            //}

            //    if (cell.Items.Count > 0)
            //    {
            //        Item tsavorite = null;
            //        double coinValue = 0;

            //        foreach (Item item in new List<Item>(cell.Items))
            //        {
            //            if (item.itemID == Item.ID_DAZZLING_TSAVORITE && tsavorite == null)
            //            {
            //                tsavorite = item;
            //                if (cell.Items.Count > 1)
            //                {
            //                    cell.SendToAllInSight("The violet flames roar for a moment and then die down.");
            //                }
            //            }
            //            else if (item.itemID == Item.ID_DAZZLING_TSAVORITE && tsavorite != null)
            //            {
            //                World.CollectFeeForLottery(World.FEE_ORNIC_FLAME_USE, cell.LandID, ref item.coinValue);
            //                coinValue += item.coinValue;
            //                cell.Remove(item);
            //            }
            //            else if ((item is Corpse) && !(item as Corpse).IsPlayerCorpse)
            //            {
            //                Corpse.DumpCorpse(item as Corpse, cell);
            //                cell.Remove(item);
            //            }
            //            else if (item.attunedID <= 0)
            //            {
            //                World.CollectFeeForLottery(World.FEE_ORNIC_FLAME_USE, cell.LandID, ref item.coinValue);
            //                coinValue += item.coinValue;
            //                cell.Remove(item);
            //            }
            //        }

            //        // Add value to existing tsavorite gem.
            //        if (tsavorite != null)
            //        {
            //            tsavorite.coinValue += coinValue;
            //        }
            //        else if (coinValue > 0) // Or create a new tsavorite gem if items of value were present.
            //        {
            //            tsavorite = Item.CopyItemFromDictionary(Item.ID_DAZZLING_TSAVORITE);
            //            tsavorite.coinValue = coinValue;
            //            cell.EmitSound(GameSpell.GetSpell((int) GameSpell.GameSpellID.Bonfire).SoundFile);
            //            cell.SendToAllInSight("The violet flames roar for a moment and then die down.");
            //            cell.Add(tsavorite);
            //        }
            //    });));

            return true;
        }
    }
}
