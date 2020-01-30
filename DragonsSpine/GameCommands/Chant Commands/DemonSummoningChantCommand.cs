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
using System.Linq;
using System.Text;
using DragonsSpine.GameWorld;
using ClassType = DragonsSpine.Character.ClassType;

namespace DragonsSpine.Commands
{
    [CommandAttribute("alsikunushiilani", "Speaking the demon summoning chant may summon a demon or it may place you in Hell.",
        (int)Globals.eImpLevel.USER, new string[] { "alsi ku nushi ilani" }, 3, new string[] { "There are no arguments for the demon summoning chant." },
        Globals.ePlayerState.PLAYING)]
    public class DemonSummoningChantCommand : ICommandHandler
    {
        public enum DemonName { Asmodeus, Pazuzu, Glamdrang, Damballa, Thamuz, Perdurabo, Samael }

        public bool OnCommand(Character chr, string args)
        {
            chr.SendToAllInSight(chr.Name + ": " + GameSystems.Text.TextManager.CHANT_DEMON_SUMMONING);

            if (chr.gender == Globals.eGender.Female)
                chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.FemaleSpellWarm));
            else
                chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.MaleSpellWarm));

            int dNum = Rules.RollD(1, 6);

            switch (dNum)
            {
                case 0:
                    ChantDemonSummon(DemonName.Asmodeus, chr);
                    break;
                case 1:
                    ChantDemonSummon(DemonName.Damballa, chr);
                    break;
                case 2:
                    ChantDemonSummon(DemonName.Glamdrang, chr);
                    break;
                case 3:
                    ChantDemonSummon(DemonName.Pazuzu, chr);
                    break;
                case 4:
                    ChantDemonSummon(DemonName.Perdurabo, chr);
                    break;
                case 5:
                    ChantDemonSummon(DemonName.Thamuz, chr);
                    break;
                case 6:
                default:
                    ChantDemonSummon(DemonName.Samael, chr);
                    break;
            }
            return true;
        }

        public static void ChantDemonSummon(DemonName demonName, Character ch)
        {
            ch.SendToAllInSight(ch.Name + ": alsi ku nushi ilani");

            NPC demon = null;
            Cell hellCell = null;

            foreach (NPC npcInWorld in Character.NPCInGameWorld)
            {
                if (npcInWorld.Name.ToLower() == demonName.ToString().ToLower())
                {
                    demon = npcInWorld;
                    break;
                }
            }

            #region // Check to see if the demon is in the world and if the player is in hell
            if (demon == null || demon.MapID != World.MAP_HELL)
            {
                ch.WriteToDisplay("You hear a voice in your head: 'Not now, I'm busy.'");
                return;
            }
            if (ch.MapID == World.MAP_HELL)
            {
                ch.WriteToDisplay(demonName.ToString() + " refuses your call.");
                return;
            }
            #endregion

            #region // Now we do a wisdom check to see if the demon responds
            if (!Rules.FullStatCheck(ch, Globals.eAbilityStat.Wisdom))
            {
                ch.WriteToDisplay("You feel a sense of dread pass over you.");
                return;
            }
            #endregion

            #region // Now we do a Charisma check to see if the demon tries to pull the summoner to hell
            if (Rules.RollD(1, 20) < demon.Charisma - ch.Charisma)
            {
                // We are going to attempt to pull the summoner to Hell
                int chance = demon.Wisdom; // based on the demons wisdom
                if (Rules.Dice.Next(1, 100) < chance)
                {
                    // move the character to Hell
                    switch (ch.BaseProfession)
                    {
                        case ClassType.Thief:
                            hellCell = Cell.GetCell(ch.FacetID, World.LAND_AG, World.MAP_HELL,
                                World.GetFacetByID(ch.FacetID).GetLandByID(World.LAND_AG).GetMapByID(World.MAP_HELL).ThiefResX,
                                World.GetFacetByID(ch.FacetID).GetLandByID(World.LAND_AG).GetMapByID(World.MAP_HELL).ThiefResY,
                                World.GetFacetByID(ch.FacetID).GetLandByID(World.LAND_AG).GetMapByID(World.MAP_HELL).ThiefResZ);
                            break;
                        default:
                            hellCell = Cell.GetCell(ch.FacetID, World.LAND_AG, World.MAP_HELL,
                                World.GetFacetByID(ch.FacetID).GetLandByID(World.LAND_AG).GetMapByID(World.MAP_HELL).ResX,
                                World.GetFacetByID(ch.FacetID).GetLandByID(World.LAND_AG).GetMapByID(World.MAP_HELL).ResY,
                                World.GetFacetByID(ch.FacetID).GetLandByID(World.LAND_AG).GetMapByID(World.MAP_HELL).ResZ);
                            break;
                    }
                    if (ch.Alignment == Globals.eAlignment.Evil) // override for evil characters
                    {
                        hellCell = Cell.GetCell(ch.FacetID, World.LAND_AG, World.MAP_HELL,
                                World.GetFacetByID(ch.FacetID).GetLandByID(World.LAND_AG).GetMapByID(World.MAP_HELL).KarmaResX,
                                World.GetFacetByID(ch.FacetID).GetLandByID(World.LAND_AG).GetMapByID(World.MAP_HELL).KarmaResY,
                                World.GetFacetByID(ch.FacetID).GetLandByID(World.LAND_AG).GetMapByID(World.MAP_HELL).KarmaResZ);
                    }
                    ch.WriteToDisplay("The world goes black around you.");
                    ch.SendToAllInSight(ch.Name + " is consumed by a pillar of fire!");
                    ch.CurrentCell = hellCell;
                    ch.WriteToDisplay("You hear a voice in your head: 'Welcome to hell, mortal.'");
                    Rules.EnterHell(ch as PC);
                    return;
                }
                ch.WriteToDisplay("You see a river of flames flash before your eyes.");
                return;
            }
            #endregion

            #region // Bring forth the demon to the mortal plane
            demon.lairCells = demon.CurrentCell.X + "|" + demon.CurrentCell.Y + "|" + demon.CurrentCell.Z; // temporary storage
            demon.CurrentCell = ch.CurrentCell;
            //demon.aiType = AIType.EmptySlot;
            demon.species = Globals.eSpecies.Demon;
            demon.RoundsRemaining = Rules.RollD(2, 8) * 2;
            demon.IsSummoned = true;
            demon.special = "despawn";
            demon.Age = 0;
            switch (demonName)
            {
                case DemonName.Perdurabo:
                case DemonName.Samael:
                    demon.Alignment = Globals.eAlignment.Amoral;
                    demon.TemporaryStorage = demon.Name; // temporary storage of the demon's name
                    demon.Name = "demon";
                    break;
                case DemonName.Glamdrang:
                    demon.TemporaryStorage = demon.Name;
                    demon.Name = "ghoul";
                    demon.Alignment = Globals.eAlignment.Chaotic;
                    break;
                default:
                    demon.Alignment = Globals.eAlignment.ChaoticEvil;
                    demon.TemporaryStorage = demon.Name;
                    demon.Name = "demon";
                    break;
            }
            ch.SendToAllInSight("A rift in the ground opens up near " + ch.Name + ", spilling forth fire and ash.");
            ch.WriteToDisplay("A rift in the ground opens up spilling forth fire and ash.");
            #endregion
        }
    }
}

