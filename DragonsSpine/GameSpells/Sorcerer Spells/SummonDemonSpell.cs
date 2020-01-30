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
using DragonsSpine.Autonomy.EntityBuilding;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Summon_Demon, "summondemon", "Summon Demon", "Attempt to summon a demon from the pits of hell to do your bidding.",
        Globals.eSpellType.Conjuration, Globals.eSpellTargetType.Self, 30, 17, 3200000, "", false, false, true, false, true, Character.ClassType.Sorcerer)]
    public class SummonDemonSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            // There is no power level to this spell. Only one demon may be controlled, and if the caster has
            // any pets the spell will flail. The demon will be very powerful, this it's why it is a skill 17 spell.

            // First gather a list of unnamed demons.

            if (caster.Pets != null && caster.Pets.Count > 0 && (caster is PC) && (caster as PC).ImpLevel < Globals.eImpLevel.DEVJR)
            {
                caster.WriteToDisplay("Summoning a demon requires your full concentration.");
                return false;
            }

            List<EntityLists.Entity> availableDemons = new List<EntityLists.Entity>();

            foreach(EntityLists.Entity entity in EntityLists.DEMONS)
            {
                if (!EntityLists.NAMED_DEMONS.Contains(entity))
                    availableDemons.Add(entity);
            }

            NPC demon = NPC.LoadNPC(Item.ID_SUMMONEDMOB, caster.FacetID, caster.LandID, caster.MapID, caster.X, caster.Y, caster.Z, -1);
            
            EntityLists.Entity chosenDemon = availableDemons[Rules.Dice.Next(availableDemons.Count)];

            EntityBuilder builder = new EntityBuilder();

            demon.Level = caster.Level + Rules.RollD(3, 4);
            demon.entity = chosenDemon;
            List<Character.ClassType> allowedProfessions = new List<Character.ClassType>
            {
                Character.ClassType.Fighter,
                Character.ClassType.Ravager,
                //Character.ClassType.Sorcerer,
                //Character.ClassType.Thaumaturge,
                Character.ClassType.Thief,
                Character.ClassType.Wizard,
            };

            demon.BaseProfession = allowedProfessions[Rules.Dice.Next(0, allowedProfessions.Count - 1)];

            builder.SetOnTheFlyVariables(demon);
            EntityBuilder.SetVisualKey(demon.entity, demon);
            builder.SetName(demon, demon.BaseProfession.ToString().ToLower());
            builder.SetDescriptions("summoned", demon, caster.Map.ZPlanes[caster.Z], demon.BaseProfession.ToString().ToLower());
            EntityBuilder.SetGender(demon, caster.Map.ZPlanes[caster.Z]);
            EntityBuilder.SetVisualKey(demon.entity, demon);

            if (demon.IsSpellUser)
            {
                NPC.CreateGenericSpellList(demon);
                GameSpell.FillSpellLists(demon);
            }

            if (EntityLists.IsHumanOrHumanoid(demon))
            {
                demon.wearing.Clear();

                List<int> armorToWear = Autonomy.ItemBuilding.ArmorSets.ArmorSet.ArmorSetDictionary[Autonomy.ItemBuilding.ArmorSets.ArmorSet.FULL_STEEL].GetArmorList(demon);

                foreach (int id in armorToWear)
                {
                    Item armor = Item.CopyItemFromDictionary(id);
                    armor.special += " " + Item.EXTRAPLANAR;
                    demon.WearItem(armor);
                }
            }

            if (EntityLists.IsHumanOrHumanoid(demon) || EntityLists.ANIMALS_WIELDING_WEAPONS.Contains(demon.entity))
            {
                List<int> weaponsList = Autonomy.ItemBuilding.LootManager.GetBasicWeaponsFromArmory(demon, true);
                if (weaponsList.Count > 0)
                    demon.EquipRightHand(Item.CopyItemFromDictionary(weaponsList[Rules.Dice.Next(weaponsList.Count - 1)]));
                weaponsList = Autonomy.ItemBuilding.LootManager.GetBasicWeaponsFromArmory(demon, false);
                if (weaponsList.Count > 0)
                    demon.EquipLeftHand(Item.CopyItemFromDictionary(weaponsList[Rules.Dice.Next(weaponsList.Count - 1)]));

                if (demon.RightHand != null) demon.RightHand.special += " " + Item.EXTRAPLANAR;
                if (demon.LeftHand != null) demon.LeftHand.special += " " + Item.EXTRAPLANAR;
            }

            demon.castMode = NPC.CastMode.Limited;

            demon.Hits = demon.HitsFull;
            demon.Mana = demon.ManaFull;
            demon.Stamina = demon.StaminaFull;

            demon.Alignment = Globals.eAlignment.ChaoticEvil;
            demon.race = "Hell";

            //demon.aiType = NPC.AIType.EmptySlot;
            demon.Age = 0;
            demon.special = "despawn";
            int fiveMinutes = Utils.TimeSpanToRounds(new TimeSpan(0, 5, 0));
            // 5 minutes plus 1 minute per magic skill level
            demon.RoundsRemaining = fiveMinutes + Skills.GetSkillLevel(caster.magic) * Utils.TimeSpanToRounds(new TimeSpan(0, 1, 0));
            demon.species = Globals.eSpecies.Demon; // this may need to be changed for AI to work properly
            demon.Alignment = caster.Alignment;
            demon.canCommand = true;
            demon.IsMobile = true;
            demon.IsSummoned = true;
            demon.IsUndead = EntityLists.UNDEAD.Contains(demon.entity);

            demon.FollowID = caster.UniqueID;

            demon.PetOwner = caster;
            caster.Pets.Add(demon);

            if (demon.CurrentCell != caster.CurrentCell)
                demon.CurrentCell = caster.CurrentCell;

            demon.EmitSound(demon.idleSound);

            demon.AddToWorld();

            return true;
        }
    }
}
