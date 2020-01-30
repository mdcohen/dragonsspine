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
    [SpellAttribute(GameSpell.GameSpellID.Chaos_Portal, "chaosportal", "Chaos Portal", "Attempt to teleport yourself and group members to a foreign land.",
        Globals.eSpellType.Conjuration, Globals.eSpellTargetType.Single_or_Group, 50, 13, 205000, "0283", false, false, true, true, true, Character.ClassType.Wizard)]
    public class ChaosPortalSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            // Determine map with args.

            if (args == null || args == "")
            {
                caster.WriteToDisplay("You must determine where you will teleport to.");
                return false;
            }

            // Basically planes of existence are offlimits, for now, or perhaps there will be a planar projection spell in the future.
            string[] offlimitMaps = new string[] { "eridu", "praetoseba", "hell" };

            if(Array.IndexOf(offlimitMaps, args.ToLower()) > -1 && !caster.IsImmortal)
            {
                caster.WriteToDisplay("You cannot travel to some planes of existence with the " + ReferenceSpell.Name + " spell.");
                return false;
            }

            GameWorld.Map destinationMap = null;

            foreach(GameWorld.Land land in GameWorld.World.GetFacetByID(caster.FacetID).Lands)
            {
                foreach(GameWorld.Map map in land.Maps)
                {
                    if(map.Name.ToLower() == args.ToLower())
                    {
                        destinationMap = map;
                        break;
                    }
                }

                if (destinationMap != null) break;
            }

            if(destinationMap == null)
            {
                caster.WriteToDisplay("There is no such place in this reality.");
                return false;
            }

            // Get a random cell. Must not be lair, out of bounds, or path blocked.

            List<Tuple<int,int,int>> cellsList = new List<Tuple<int, int, int>>(destinationMap.cells.Keys);

            GameWorld.Cell destinationCell = null;

            do
            {
                destinationCell = destinationMap.cells[cellsList[Rules.Dice.Next(cellsList.Count)]];
            }
            while (!CellMeetsTeleportRequirements(destinationCell));

            ReferenceSpell.SendGenericCastMessage(caster, null, true);

            caster.SendToAllInSight("A portal of chaotic energy opens up before you and " + caster.GetNameForActionResult(true) + " steps inside.");
            caster.WriteToDisplay("A portal of chaotic energy opens up before you and you step inside.");

            // Whirlwind sound...
            //caster.CurrentCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.Whirlwind));

            // fail to teleport if holding a corpse or drop it
            // check caster first, nobody teleports
            if (!CharacterMeetsTeleportRequirements(caster))
            {
                caster.SendToAllInSight("The chaos portal immediately collapses and " + caster.GetNameForActionResult(true) + " is slammed into the ground.");
                AreaEffect knockdown = new AreaEffect(Effect.EffectTypes.Concussion, "", 50 + Rules.Dice.Next(-5, +5), 0, caster, caster.CurrentCell);
                NPC.AIRandomlyMoveCharacter(caster);
                return false;
            }

            List<Character> teleportedCharacters = null;

            if (caster.Group != null && caster.Group.GroupMemberIDList != null)
            {
                teleportedCharacters = new List<Character>();

                foreach (int uniqueID in caster.Group.GroupMemberIDList)
                {
                    if (uniqueID != caster.UniqueID)
                    {
                        PC pc = PC.GetOnline(uniqueID);
                        if (CharacterMeetsTeleportRequirements(pc))
                        {
                            teleportedCharacters.Add(pc);
                            pc.WriteToDisplay("You follow " + caster.GetNameForActionResult(true) + " through the chaos portal. You are now in " + pc.Map.Name + ".");
                        }
                        else
                        {
                            pc.WriteToDisplay("You are thrown back out of the chaos portal!");
                            NPC.AIRandomlyMoveCharacter(pc);
                        }
                    }
                }
            }

            // make the move, clear recall rings. possible stun or blindness and/or damage?
            GameWorld.Cell chaosPortalCell = caster.CurrentCell; // stored for emote and sound upon portal collapse
            destinationCell.EmitSound(ReferenceSpell.SoundFile);
            //destinationCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.Whirlwind));
            SendPortalEntranceEmote(caster);
            //caster.SendSoundToAllInRange(Sound.GetCommonSound(Sound.CommonSound.MapPortal));
            caster.CurrentCell = destinationCell;
            SendPortalExitEmote(caster);
            ResetRecallRings(caster);
            TeleportPets(caster, destinationCell);

            if (caster.Group != null && teleportedCharacters != null)
            {
                foreach (Character chr in teleportedCharacters)
                {
                    SendPortalEntranceEmote(chr);
                    //chr.SendSoundToAllInRange(Sound.GetCommonSound(Sound.CommonSound.MapPortal));
                    chr.CurrentCell = destinationCell;
                    SendPortalExitEmote(chr);
                    ResetRecallRings(chr);
                    TeleportPets(chr, destinationCell);
                }
            }

            chaosPortalCell.SendToAllInSight("With a thunder clap, the chaos portal winks out of existence.");
            chaosPortalCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.ThunderClap));
            
            return true;
        }

        public static bool CellMeetsTeleportRequirements(GameWorld.Cell cell)
        {
            if (cell == null) return false;
            if (cell.IsOutOfBounds) return false; // out of bounds
            if (GameWorld.Map.IsSpellPathBlocked(cell)) return false; // spell path is blocked
            //if (cell.IsNoRecall) return false; // no recall
            if(cell.Map.ZPlanes[cell.Z].zAutonomy != null && !cell.Map.ZPlanes[cell.Z].zAutonomy.allowChaosPortal) return false; // z plane does not allow chaos portal

            return true;
        }

        private bool CharacterMeetsTeleportRequirements(Character chr)
        {
            //if (chr.WhichHand("corpse") > (int)Globals.eWearOrientation.None)
            //{
            //    chr.WriteToDisplay("You cannot teleport while holding a corpse.");
            //    return false;
            //}

            return true;
        }

        private void ResetRecallRings(Character chr)
        {
            int recallReset = 0;

            // reset recall rings when portal used
            foreach (Item ring in chr.GetRings())
            {
                if (ring.isRecall)
                {
                    ring.isRecall = false;
                    ring.wasRecall = true;
                    recallReset++;
                }
            }

            if (recallReset > 0)
            {
                chr.SendSound(Sound.GetCommonSound(Sound.CommonSound.RecallReset));

                if (recallReset == 1)
                {
                    chr.WriteToDisplay("Your recall ring has been cleared! You must remove and reset it.");
                }
                else if (recallReset > 1)
                {
                    chr.WriteToDisplay("Your recall rings have been cleared! You must remove and reset them.");
                }
            }
        }
        
        private void TeleportPets(Character chr, GameWorld.Cell destinationCell)
        {
            if(chr.Pets != null)
            {
                foreach(NPC pet in chr.Pets)
                {
                    // No sounds...
                    SendPortalEntranceEmote(pet);
                    pet.CurrentCell = destinationCell;
                    SendPortalExitEmote(pet);
                    ResetRecallRings(pet); // in case phantasms have been commanded to wear a recall ring??
                }
            }
        }

        private void SendPortalEntranceEmote(Character chr)
        {
            chr.SendToAllInSight(chr.GetNameForActionResult() + " enters a chaos portal.");
        }

        private void SendPortalExitEmote(Character chr)
        {
            chr.SendToAllInSight(chr.GetNameForActionResult() + " arrives via a chaos portal.");
        }
    }
}
