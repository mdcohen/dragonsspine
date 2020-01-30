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

namespace DragonsSpine.Commands
{
    [CommandAttribute("impgetitemstats", "All proceeding text is sent to every player in your current map.", (int)Globals.eImpLevel.DEVJR, new string[] { "impitemstats", "itemstats" },
        0, new string[] { "impgetitemstats <item on ground or in hands>" }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class ImpGetItemStatsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Usage: impitemstats <name of item on ground or in hand>, impitemstats <name> all, impitemstats <name> field, impitemstats <itemID>");
                return true;
            }

            //string[] sArgs = args.Split(" ".ToCharArray());

            //TODO: other usage implementation 10/18/2015 Eb

            Item tmpItem = chr.FindHeldItem(args);

            if (tmpItem == null)
                tmpItem = Item.FindItemOnGround(args, chr.FacetID, chr.LandID, chr.MapID, chr.X, chr.Y, chr.Z);

            if (tmpItem == null)
            {
                chr.WriteToDisplay("Unable to find target item \"" + args + "\".");
                return false;
            }

            string effectNames = "none";

            if (tmpItem.effectType != "")
            {
                effectNames = "";

                string[] effectsList = tmpItem.effectType.Split(" ".ToCharArray());
                string[] effectsAmt = tmpItem.effectAmount.Split(" ".ToCharArray());

                for (int a = 0; a < effectsList.Length; a++)
                    effectNames += "[" + effectsList[a] + "]" + Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(effectsList[a])) + "(" + effectsAmt[a] + ") ";
            }

            chr.WriteToDisplay(tmpItem.GetLogString());
            chr.WriteToDisplay("attunedID: " + tmpItem.attunedID);
            if (tmpItem.attunedID > 0) // it's a player
                chr.WriteToDisplay((string)DAL.DBPlayer.GetPlayerField(tmpItem.attunedID, "name", Type.GetType("System.String")));
            chr.WriteToDisplay("alignment: " + tmpItem.alignment.ToString());
            chr.WriteToDisplay("itemType: " + tmpItem.itemType.ToString());
            chr.WriteToDisplay("baseType: " + tmpItem.baseType.ToString());
            chr.WriteToDisplay("visualKey: " + tmpItem.visualKey);
            chr.WriteToDisplay("shortDesc: " + tmpItem.shortDesc);
            chr.WriteToDisplay("longDesc: " + tmpItem.longDesc);
            chr.WriteToDisplay("weight: " + tmpItem.weight);
            chr.WriteToDisplay("effects: " + effectNames);
            chr.WriteToDisplay("coinValue: " + tmpItem.coinValue);
            chr.WriteToDisplay("vRandLow: " + tmpItem.vRandLow);
            chr.WriteToDisplay("vRandHigh: " + tmpItem.vRandHigh);
            chr.WriteToDisplay("size: " + tmpItem.size);
            chr.WriteToDisplay("minDamage: " + tmpItem.minDamage);
            chr.WriteToDisplay("maxDamage: " + tmpItem.maxDamage);
            chr.WriteToDisplay("dropRound: " + tmpItem.dropRound);
            chr.WriteToDisplay("armorClass: " + tmpItem.armorClass);
            chr.WriteToDisplay("special: " + tmpItem.special);
            chr.WriteToDisplay("lootTable: " + tmpItem.lootTable);

            if (tmpItem.spell >= 0)
            {
                chr.WriteToDisplay("spell: " + Spells.GameSpell.GetSpell(tmpItem.spell).Name);
                chr.WriteToDisplay("charges: " + tmpItem.charges);
                chr.WriteToDisplay("spellPower: " + tmpItem.spellPower);
            }

            return true;
        }
    }
}
