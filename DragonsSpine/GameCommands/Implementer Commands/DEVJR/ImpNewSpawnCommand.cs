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
    [CommandAttribute("impnewspawn", "Create a new spawn point and insert a row into the SpawnZone table.", (int)Globals.eImpLevel.DEVJR, new string[] { "impaddspawn" },
        0, new string[] { "impnewspawn <arguments>" }, Globals.ePlayerState.PLAYING)]
    public class ImpNewSpawnCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (String.IsNullOrEmpty(args))
            {
                chr.WriteToDisplay("impnewspawn <notes(use + instead of space)> <enabled> <NPCID> <SpawnTimer> <MaxAllowedInZone> <SpawnMessage> <MinZone> <MaxZone> <NPCList> <spawnLand> <spawnMap> <spawnXcord> <spawnYcord> <spawnZcord> <spawnRadius> <spawnZRange>");
                chr.WriteToDisplay("Use a vertical bar | in between arguments, instead of a space.");
                chr.WriteToDisplay("You may also use 'impnewspawn [armory | weapons | pawn | bar | apothecary | jeweler]' to create a generic spawn zone at current cell.");
                return true;
            }

            // armory, weapons, pawn, barkeep, apothecary, jeweler

            switch (args)
            {
                case "armor":
                case "armory":
                    GameCommand.GameCommandDictionary["impnewspawn"].Handler.OnCommand(chr, chr.Land.Name + "+Generic+Armor+Shop|1|" + Merchant.GENERIC_ARMORER_NPCID + "|" + Merchant.DEFAULT_MERCHANT_SPAWNTIMER + "|1||1|1|" + Merchant.GENERIC_ARMORER_NPCID + "|" + chr.LandID + "|" + chr.MapID + "|" + chr.X + "|" + chr.Y + "|" + chr.Z + "|0|");
                    return true;
                case "weapon":
                case "weapons":
                    GameCommand.GameCommandDictionary["impnewspawn"].Handler.OnCommand(chr, chr.Land.Name + "+Generic+Weapon+Shop|1|" + Merchant.GENERIC_WEAPON_NPCID + "|" + Merchant.DEFAULT_MERCHANT_SPAWNTIMER + "|1||1|1|" + Merchant.GENERIC_WEAPON_NPCID + "|" + chr.LandID + "|" + chr.MapID + "|" + chr.X + "|" + chr.Y + "|" + chr.Z + "|0|");
                    return true;
                case "pawn":
                    GameCommand.GameCommandDictionary["impnewspawn"].Handler.OnCommand(chr, chr.Land.Name + "+Generic+Pawn+Shop|1|" + Merchant.GENERIC_PAWN_NPCID + "|" + Merchant.DEFAULT_MERCHANT_SPAWNTIMER + "|1||1|1|" + Merchant.GENERIC_PAWN_NPCID + "|" + chr.LandID + "|" + chr.MapID + "|" + chr.X + "|" + chr.Y + "|" + chr.Z + "|0|");
                    return true;
                case "bar":
                case "barkeep":
                    GameCommand.GameCommandDictionary["impnewspawn"].Handler.OnCommand(chr, chr.Land.Name + "+Generic+Barkeep|1|" + Merchant.GENERIC_BARKEEP_NPCID + "|" + Merchant.DEFAULT_MERCHANT_SPAWNTIMER + "|1||1|1|" + Merchant.GENERIC_BARKEEP_NPCID + "|" + chr.LandID + "|" + chr.MapID + "|" + chr.X + "|" + chr.Y + "|" + chr.Z + "|0|");
                    return true;
                case "apoth":
                case "apothecary":
                    GameCommand.GameCommandDictionary["impnewspawn"].Handler.OnCommand(chr, chr.Land.Name + "+Generic+Apothecary|1|" + Merchant.GENERIC_APOTHECARY_NPCID + "|" + Merchant.DEFAULT_MERCHANT_SPAWNTIMER + "|1||1|1|" + Merchant.GENERIC_APOTHECARY_NPCID + "|" + chr.LandID + "|" + chr.MapID + "|" + chr.X + "|" + chr.Y + "|" + chr.Z + "|0|");
                    return true;
                case "jewel":
                case "jewels":
                case "jewelery":
                case "jeweler":
                    GameCommand.GameCommandDictionary["impnewspawn"].Handler.OnCommand(chr, chr.Land.Name + "+Generic+Jeweler|1|" + Merchant.GENERIC_JEWELER_NPCID + "|" + Merchant.DEFAULT_MERCHANT_SPAWNTIMER + "|1||1|1|" + Merchant.GENERIC_JEWELER_NPCID + "|" + chr.LandID + "|" + chr.MapID + "|" + chr.X + "|" + chr.Y + "|" + chr.Z + "|0|");
                    return true;
                // As of 2/10/2017 Zplanes with ZAutonomy will utilize available maximumDifficultyLevel (Character object Level variable) info and set skill levels for trainer with that info (currently +5 to max difficulty level) -Eb
                case "highsorc":
                    GameCommand.GameCommandDictionary["impnewspawn"].Handler.OnCommand(chr, chr.Land.Name + "+Generic+High+Sorc+Trainer|1|" + Merchant.GENERIC_HIGH_SORC_TRAINER_NPCID + "|" + Merchant.DEFAULT_MERCHANT_SPAWNTIMER + "|1||1|1|" + Merchant.GENERIC_HIGH_SORC_TRAINER_NPCID + "|" + chr.LandID + "|" + chr.MapID + "|" + chr.X + "|" + chr.Y + "|" + chr.Z + "|0|");
                    return true;
                case "evilweapstrainer":
                    GameCommand.GameCommandDictionary["impnewspawn"].Handler.OnCommand(chr, chr.Land.Name + "+Generic+Evil+Weapons+Trainer|1|" + Merchant.GENERIC_EVIL_WEAPONS_TRAINER_NPCID + "|" + Merchant.DEFAULT_MERCHANT_SPAWNTIMER + "|1||1|1|" + Merchant.GENERIC_EVIL_WEAPONS_TRAINER_NPCID + "|" + chr.LandID + "|" + chr.MapID + "|" + chr.X + "|" + chr.Y + "|" + chr.Z + "|0|");
                    return true;
                case "weapstrainer":
                case "weaponstrainer":
                    GameCommand.GameCommandDictionary["impnewspawn"].Handler.OnCommand(chr, chr.Land.Name + "+Generic+Lawful+High+Weapons+Trainer|1|" + Merchant.GENERIC_LAWFUL_HIGH_WEAPONS_TRAINER_NPCID + "|" + Merchant.DEFAULT_MERCHANT_SPAWNTIMER + "|1||1|1|" + Merchant.GENERIC_LAWFUL_HIGH_WEAPONS_TRAINER_NPCID + "|" + chr.LandID + "|" + chr.MapID + "|" + chr.X + "|" + chr.Y + "|" + chr.Z + "|0|");
                    return true;
                case "matrainer":
                case "martialartist":
                    GameCommand.GameCommandDictionary["impnewspawn"].Handler.OnCommand(chr, chr.Land.Name + "+Generic+Lawful+High+MA+Trainer|1|" + Merchant.GENERIC_LAWFUL_HIGH_MA_TRAINER + "|" + Merchant.DEFAULT_MERCHANT_SPAWNTIMER + "|1||1|1|" + Merchant.GENERIC_LAWFUL_HIGH_MA_TRAINER + "|" + chr.LandID + "|" + chr.MapID + "|" + chr.X + "|" + chr.Y + "|" + chr.Z + "|0|");
                    return true;
                case "thaumtrainer":
                    GameCommand.GameCommandDictionary["impnewspawn"].Handler.OnCommand(chr, chr.Land.Name + "+Generic+Lawful+High+Thaum+Trainer|1|" + Merchant.GENERIC_LAWFUL_HIGH_THAUM_TRAINER + "|" + Merchant.DEFAULT_MERCHANT_SPAWNTIMER + "|1||1|1|" + Merchant.GENERIC_LAWFUL_HIGH_THAUM_TRAINER + "|" + chr.LandID + "|" + chr.MapID + "|" + chr.X + "|" + chr.Y + "|" + chr.Z + "|0|");
                    return true;
                case "wiztrainer":
                    GameCommand.GameCommandDictionary["impnewspawn"].Handler.OnCommand(chr, chr.Land.Name + "+Generic+Lawful+High+Wiz+Trainer|1|" + Merchant.GENERIC_LAWFUL_HIGH_WIZ_TRAINER + "|" + Merchant.DEFAULT_MERCHANT_SPAWNTIMER + "|1||1|1|" + Merchant.GENERIC_LAWFUL_HIGH_WIZ_TRAINER + "|" + chr.LandID + "|" + chr.MapID + "|" + chr.X + "|" + chr.Y + "|" + chr.Z + "|0|");
                    return true;
                case "thieftrainer":
                    GameCommand.GameCommandDictionary["impnewspawn"].Handler.OnCommand(chr, chr.Land.Name + "+Generic+Neutral+High+Thief+Trainer|1|" + Merchant.GENERIC_NEUTRAL_HIGH_THIEF_TRAINER + "|" + Merchant.DEFAULT_MERCHANT_SPAWNTIMER + "|1||1|1|" + Merchant.GENERIC_NEUTRAL_HIGH_THIEF_TRAINER + "|" + chr.LandID + "|" + chr.MapID + "|" + chr.X + "|" + chr.Y + "|" + chr.Z + "|0|");
                    return true;
                case "tanner":
                    GameCommand.GameCommandDictionary["impnewspawn"].Handler.OnCommand(chr, chr.Land.Name + "+Generic+Lawful+Tanner|1|" + Merchant.GENERIC_TANNER_NPCID + "|" + Merchant.DEFAULT_MERCHANT_SPAWNTIMER + "|1||1|1|" + Merchant.GENERIC_TANNER_NPCID + "|" + chr.LandID + "|" + chr.MapID + "|" + chr.X + "|" + chr.Y + "|" + chr.Z + "|0|");
                    return true;
                case "ranger":
                    GameCommand.GameCommandDictionary["impnewspawn"].Handler.OnCommand(chr, chr.Land.Name + "+Generic+Lawful+High+Ranger+Trainer|1|" + Merchant.GENERIC_LAWFUL_HIGH_RANGER_TRAINER + "|" + Merchant.DEFAULT_MERCHANT_SPAWNTIMER + "|1||1|1|" + Merchant.GENERIC_LAWFUL_HIGH_RANGER_TRAINER + "|" + chr.LandID + "|" + chr.MapID + "|" + chr.X + "|" + chr.Y + "|" + chr.Z + "|0|");
                    return true;
                case "druid":
                    GameCommand.GameCommandDictionary["impnewspawn"].Handler.OnCommand(chr, chr.Land.Name + "+Generic+Neutral+High+Druid+Trainer|1|" + Merchant.GENERIC_NEUTRAL_HIGH_DRUID_TRAINER + "|" + Merchant.DEFAULT_MERCHANT_SPAWNTIMER + "|1||1|1|" + Merchant.GENERIC_NEUTRAL_HIGH_DRUID_TRAINER + "|" + chr.LandID + "|" + chr.MapID + "|" + chr.X + "|" + chr.Y + "|" + chr.Z + "|0|");
                    return true;
                default:
                    // continue on with arguments breakdown -- old code, but still works if strongly typed 2/10/2017 Eb
                    break;
            }

            string[] sArgs = args.Split("|".ToCharArray());

            if (sArgs.Length != 16)
            {
                chr.WriteToDisplay("impnewspawn <notes(use + instead of space)> <enabled> <NPCID> <SpawnTimer> <MaxAllowedInZone> <SpawnMessage> <MinZone> <MaxZone> <NPCList> <spawnLand> <spawnMap> <spawnXcord> <spawnYcord> <spawnZcord> <spawnRadius> <spawnZRange>");
                chr.WriteToDisplay("Use a vertical bar | in between arguments, instead of a space.");
                return true;
            }

            #region arguments list
            // Args:
            // 0 : notes
            // 1 : enabled
            // 2 : NPCID
            // 3 : SpawnTimer
            // 4 : MaxAllowedInZone
            // 5 : SpawnMessage
            // 6 : MinZone
            // 7 : MaxZone
            // 8 : NPCList
            // 9 : spawnLand
            // 10: spawnMap
            // 11: spawnX
            // 12: spawnY
            // 13: spawnZ
            // 14: spawnRadius
            // 15: spawnZRange 
            #endregion

            string notes = sArgs[0].Replace("+", " ");
            bool enabled = false;
            int NPCID = Convert.ToInt32(sArgs[2]);
            int SpawnTimer = Convert.ToInt32(sArgs[3]);
            int MaxAllowedInZone = Convert.ToInt32(sArgs[4]);
            string SpawnMessage = sArgs[5];
            int minZone = Convert.ToInt32(sArgs[6]);
            int maxZone = Convert.ToInt32(sArgs[7]);
            string NPCList = sArgs[8];
            int spawnLand = Convert.ToInt32(sArgs[9]);
            int spawnMap = Convert.ToInt32(sArgs[10]);
            int spawnXcord = Convert.ToInt32(sArgs[11]);
            int spawnYcord = Convert.ToInt32(sArgs[12]);
            int spawnZcord = Convert.ToInt32(sArgs[13]);
            int spawnRadius = Convert.ToInt32(sArgs[14]);
            string spawnZRange = sArgs[15];

            if (sArgs[1] == "1" || sArgs[1].ToLower() == "true")
            {
                enabled = true;
            }

            if (DAL.DBEditor.UpdateSpawnZone(true, notes, enabled, NPCID, SpawnTimer, MaxAllowedInZone, SpawnMessage, NPCList, minZone, maxZone, spawnLand, spawnMap,
                spawnXcord, spawnYcord, spawnZcord, spawnRadius, spawnZRange) > 0)
            {
                chr.WriteToDisplay("SpawnZone created at M: " + spawnMap + " X: " + spawnXcord + " Y: " + spawnYcord + " Z: " + spawnZcord);
            }
            else chr.WriteToDisplay("SpawnZone insert failed.");

            return true;
        }
    }
}
