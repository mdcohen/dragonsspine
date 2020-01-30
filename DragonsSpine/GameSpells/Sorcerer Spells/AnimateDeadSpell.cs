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

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Animate_Dead, "animatedead", "Animate Dead", "Animates a corpse to become a skeleton or zombie pet.",
        Globals.eSpellType.Necromancy, Globals.eSpellTargetType.Single, 24, 11, 70000, "0272", false, false, true, false, false, Character.ClassType.Sorcerer)]
    public class AnimateDeadSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            #region Determine number of pets.
            int petCount = 0;
            foreach (NPC pet in caster.Pets)
            {
                if (pet.QuestList.Count == 0)
                {
                    petCount++;
                }
            }

            if (petCount >= GameSpell.MAX_PETS)
            {
                caster.WriteToDisplay("You may only control " + GameSpell.MAX_PETS + " pets.");
                return false;
            }
            #endregion

            try
            {
                Corpse corpseWithGem = null;
                Item gemComponent = null;

                #region Find corpse in cell with inserted material component.
                foreach (Item item in caster.CurrentCell.Items)
                {
                    if (item is Corpse)
                    {
                        foreach (Item placeable in (item as Corpse).Contents)
                        {
                            if (placeable.itemID == Item.ID_UNCUTPAINITE)
                            {
                                corpseWithGem = item as Corpse;
                                gemComponent = placeable;
                                break;
                            }
                        }
                    }

                    if (corpseWithGem != null)
                        break;
                }
                #endregion

                if (corpseWithGem == null)
                {
                    caster.WriteToDisplay("You must first prepare a corpse before you cast " + ReferenceSpell.Name + ".");
                    return false; // spell failure
                }
                else if (corpseWithGem.IsPlayerCorpse)
                {
                    // catch a player corpse being animated (should not have been able to place the material component in the corpse)
                    caster.WriteToDisplay("Sadly, player corpses cannot currently be animated.");
                    if (corpseWithGem.Contents.Contains(gemComponent))
                        corpseWithGem.Contents.Remove(gemComponent);
                    return false;
                }
                else
                {
                    //NPC animated = NPC.CreateNPC((corpseWithGem.Ghost as NPC).npcID, caster.FacetID, caster.LandID, caster.MapID, caster.X, caster.Y, caster.Z);
                    //TODO Make static method in Corpse class to return animated dead NPC.

                    if (!(corpseWithGem.Ghost is NPC animated))
                    {
                        caster.WriteToDisplay("There was a problem with your " + ReferenceSpell.Name + " spell. Please report this to the developers.");
                        return false;
                    }

                    if (corpseWithGem.Contents.Contains(gemComponent))
                        corpseWithGem.Contents.Remove(gemComponent);

                    if (!caster.IsImmortal && (animated.Level > caster.Level || animated.lairCritter))
                    {
                        caster.WriteToDisplay("You are unable to animate the corpse properly.");
                        //TODO destroy corpse here?
                        return false;
                    }

                    // get minutes to corpse decay, if over half way to decay then make skeleton otherwise make zombie
                    bool isZombie = false;
                    int remainingRoundsUntilDecomposition = GameWorld.World.NPCCorpseDecayTimer - (DragonsSpineMain.GameRound - corpseWithGem.dropRound);
                    if (remainingRoundsUntilDecomposition >= (GameWorld.World.NPCCorpseDecayTimer / 2))
                        isZombie = true;

                    animated.CurrentCell = caster.CurrentCell;

                    animated = Corpse.BecomeUndead(corpseWithGem, isZombie ? Corpse.AnimateCorpseType.Zombie : Corpse.AnimateCorpseType.Skeleton);

                    animated.Alignment = caster.Alignment;
                    //TODO message that the corpse has been animated sent to all
                    animated.canCommand = true;

                    animated.PetOwner = caster;
                    caster.Pets.Add(animated);

                    animated.AddToWorld();
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }

            return true;
        }
    }
}
