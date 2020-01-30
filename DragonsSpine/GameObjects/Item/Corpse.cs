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
using World = DragonsSpine.GameWorld.World;
using EntityLists = DragonsSpine.Autonomy.EntityBuilding.EntityLists;

namespace DragonsSpine
{
    public class Corpse : Item
    {
        public enum AnimateCorpseType { Skeleton, Zombie };  

        public Character Ghost { get; set; }
        public bool IsPlayerCorpse { get { return (this.Ghost is PC); } }
        public List<Item> Contents { get; set; }

        public Corpse(System.Data.DataRow dr) : base(dr)
        {
            Ghost = null;
            Contents = new List<Item>();
        }

        /// <summary>
        /// Dump Corpse.Contents into Cell.Items.
        /// </summary>
        /// <param name="corpse">The Corpse to be dumped.</param>
        /// <param name="cell">The Cell to place the contents of the corpse.</param>
        public static void DumpCorpse(Corpse corpse, GameWorld.Cell cell)
        {
            try
            {
                int z = 0;
                int i = 0;

                z = corpse.Contents.Count - 1;
                if (corpse.Contents.Count > 0)
                {
                    System.Collections.ArrayList templist = new System.Collections.ArrayList();
                    Item[] contents = new Item[corpse.Contents.Count];
                    corpse.Contents.CopyTo(contents);
                    foreach (Item item in contents)
                        templist.Add(item);
                    z = templist.Count - 1;
                    while (z >= 0)
                    {
                        Item item = (Item)templist[z];
                        for (i = templist.Count - 1; i > -1; i--)
                        {
                            Item tmpitem = (Item)templist[i];
                            if (tmpitem.name == item.name)
                            {
                                templist.RemoveAt(i);
                                corpse.Contents.RemoveAt(i);
                                cell.Add(tmpitem);
                                z = templist.Count;
                            }
                        }
                        z--;
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        /// <summary>
        /// A Corpse object becomes an undead NPC object.
        /// </summary>
        /// <param name="corpse">The Corpse to become an undead NPC object. The Corpse contains a Ghost of the NPC.</param>
        /// <param name="type">The type of undead to become. EG: skeleton, zombie</param>
        /// <returns>The undead NPC object.</returns>
        public static NPC BecomeUndead(Corpse corpse, AnimateCorpseType type)
        {
            if (corpse.Ghost == null || corpse.IsPlayerCorpse || corpse.Ghost.IsUndead) return null;

            corpse.Ghost.IsDead = false;

            foreach (Item wearing in new List<Item>(corpse.Ghost.wearing))
                corpse.Ghost.RemoveWornItem(wearing);

            corpse.Ghost.UnequipRightHand(corpse.Ghost.RightHand);
            corpse.Ghost.UnequipLeftHand(corpse.Ghost.LeftHand);
            
            corpse.Ghost.QuestFlags.Clear();
            corpse.Ghost.QuestList.Clear();
            corpse.Ghost.sackList.Clear();
            corpse.Ghost.beltList.Clear();
            corpse.Ghost.pouchList.Clear();

            // Take note if spell casting AnimateCorpseTypes are used later.
            corpse.Ghost.spellDictionary.Clear();
            (corpse.Ghost as NPC).evocationAreaEffectSpells.Clear();
            (corpse.Ghost as NPC).evocationSpells.Clear();
            (corpse.Ghost as NPC).alterationHarmfulSpells.Clear();
            (corpse.Ghost as NPC).alterationSpells.Clear();
            (corpse.Ghost as NPC).abjurationSpells.Clear();
            (corpse.Ghost as NPC).necromancySpells.Clear();
            (corpse.Ghost as NPC).divinationSpells.Clear();
            (corpse.Ghost as NPC).conjurationSpells.Clear();

            (corpse.Ghost as NPC).immuneBlind = false;
            (corpse.Ghost as NPC).immuneCold = false;
            (corpse.Ghost as NPC).immuneCurse = true; // immune to curse
            (corpse.Ghost as NPC).immuneDeath = false;
            (corpse.Ghost as NPC).immuneFear = true; // immune to fear
            (corpse.Ghost as NPC).immuneFire = false;
            (corpse.Ghost as NPC).immuneLightning = false;
            (corpse.Ghost as NPC).immunePoison = true; // immune to poison
            (corpse.Ghost as NPC).immuneStun = false;

            corpse.Ghost.IsUndead = true;
            corpse.Ghost.BaseProfession = Character.ClassType.Fighter;
            corpse.Ghost.classFullName = Character.ClassType.Fighter.ToString();
            corpse.Ghost.Name = corpse.Ghost.Name + "." + Utils.FormatEnumString(type.ToString()).ToLower();
            string oldShortDesc = (corpse.Ghost as NPC).shortDesc;
            (corpse.Ghost as NPC).shortDesc = Utils.FormatEnumString(type.ToString()).ToLower() + " of " + (corpse.Ghost as NPC).longDesc;
            (corpse.Ghost as NPC).longDesc = "an animated " + type.ToString().ToLower().Replace("skeleton", "skeletal") + " " + oldShortDesc;
            corpse.Ghost.gender = Globals.eGender.It;

            if (corpse.Ghost.CurrentCell != null)
            {
                foreach (Item loot in new List<Item>(corpse.Contents))
                {
                    if (!corpse.Ghost.WearItem(loot))
                        corpse.Ghost.CurrentCell.Add(loot);

                    corpse.Contents.Remove(loot);
                }
            }

            switch (type)
            {
                case AnimateCorpseType.Zombie:
                    corpse.Ghost.entity = Autonomy.EntityBuilding.EntityLists.Entity.Zombie;
                    corpse.Ghost.attackSound = "0156";
                    corpse.Ghost.deathSound = "0110";
                    corpse.Ghost.idleSound = "0250";
                    break;
                case AnimateCorpseType.Skeleton:
                default:
                    corpse.Ghost.entity = Autonomy.EntityBuilding.EntityLists.Entity.Skeleton;
                    corpse.Ghost.attackSound = "0118";
                    corpse.Ghost.deathSound = "0001";
                    corpse.Ghost.idleSound = "0111";
                    break;
            }

            return corpse.Ghost as NPC;
        }

        /// <summary>
        /// Called when a Character object has been killed.
        /// </summary>
        /// <param name="target">The Character object becoming a corpse.</param>
        /// <returns>The newly created Corpse object.</returns>
        public static Corpse MakeCorpse(Character target)
        {
            if (target == null) return null;

            if (target.CurrentCell != null && target.CurrentCell.ContainsNPCCorpse(target.UniqueID))
                return null;

            // Create a new Corpse object.
            Corpse corpse = (Corpse)CopyItemFromDictionary(ID_CORPSE);

            // If the dead is immune to fire then the corpse will not be flammable.
            if (target.immuneFire)
                corpse.flammable = false;

            //if (target is PC)
            //    corpse.longDesc = "the corpse of " + target.Name;
            //if ((target as NPC).HasRandomName)
            //    corpse.longDesc = "the corpse of a " + Utils.FormatEnumString(target.entity.ToString()).ToLower() + " " + target.classFullName.ToLower() + " named " + target.Name;
            //else
            //    corpse.longDesc = "the corpse of a " + (target as NPC).longDesc;

            // Corpse weight.
            corpse.weight = target.Strength * 10;

            if (target is PC)
            {
                corpse.Ghost = target;
                corpse.itemID = ID_RESERVED_FOR_PLAYER_CORPSE;
                corpse.special = target.Name;
                corpse.longDesc = "the corpse of " + target.Name;
                target.CurrentCell.Add(corpse);
            }
            else
            {
                NPC npc = target as NPC;

                corpse.Ghost = npc;

                if ((target as NPC).HasRandomName)
                    corpse.longDesc = "the corpse of a " + Utils.FormatEnumString(target.entity.ToString()).ToLower() + " " + target.classFullName.ToLower() + " named " + target.Name;
                else if (target.entity.ToString().ToLower().StartsWith("the_"))
                    corpse.longDesc = "the corpse of " + (target as NPC).longDesc;
                else
                    corpse.longDesc = "the corpse of " + (target as NPC).longDesc;

                bool incorporeal = EntityLists.INCORPOREAL.Contains(npc.entity) || npc.IsSpectral;
                bool summoned = EntityLists.SUMMONED.Contains(npc.entity) || npc.IsSummoned;

                Rules.CorpseSack(npc, corpse);
                Rules.CorpsePouch(npc, corpse);
                Rules.CorpseBelt(npc, corpse);
                
                if (!incorporeal && !summoned)
                {
                    Rules.CorpseWearing(npc, corpse);
                    Rules.CorpseRings(npc, corpse);
                }

                // decrease number of spawns
                if (World.GetFacetByID(npc.FacetID).Spawns.ContainsKey(npc.SpawnZoneID))
                    World.GetFacetByID(npc.FacetID).Spawns[npc.SpawnZoneID].NumberInZone--;

                // place corpse on ground
                if (npc.CurrentCell != null)
                {
                    if (incorporeal || summoned)
                        DumpCorpse(corpse, npc.CurrentCell);
                    else npc.CurrentCell.Add(corpse); // place corpse on ground
                }

                npc.RemoveFromWorld(); // done with the npc - remove it.
            }

            return corpse;
        }
    }
}
