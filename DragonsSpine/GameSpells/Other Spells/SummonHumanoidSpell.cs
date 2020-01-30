using System;
using System.Collections.Generic;
using Entity = DragonsSpine.Autonomy.EntityBuilding.EntityLists.Entity;
using DragonsSpine.Autonomy.EntityBuilding;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Summon_Humanoid, "summonhumanoid", "Summon Humanoid", "Summon a humanoid ally to aid you.",
        Globals.eSpellType.Conjuration, Globals.eSpellTargetType.Self, 0, 1, 100, "0285", true, false, false, false, false, Character.ClassType.All)]
    public class SummonHumanoidSpell : ISpellHandler
    {
        public static List<Entity> SummonHumanoidAvailability = new List<Entity>()
        {
            Entity.Archmage, Entity.Drow_Master, Entity.Drow_Matriarch
        };

        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            #region Determine number of pets. Return false if at or above MAX_PETS.
            if (!caster.IsImmortal)
            {
                int petCount = 0;

                foreach (NPC pet in caster.Pets)
                {
                    if (pet.QuestList.Count == 0)
                        petCount++;
                }

                if (petCount >= GameSpell.MAX_PETS)
                {
                    caster.WriteToDisplay("You do not possess the mental fortitude to summon an ally.");
                    return false;
                }
            }
            #endregion

            args = args.Replace(ReferenceSpell.Command, "");
            args = args.Trim();
            string[] sArgs = args.Split(" ".ToCharArray());

            #region Determine Power
            int magicSkillLevel = Skills.GetSkillLevel(caster.magic);
            int power = magicSkillLevel;

            if (sArgs.Length > 0)
            {
                try
                {
                    power = Convert.ToInt32(sArgs[0]);

                    if (power > magicSkillLevel && !caster.IsImmortal)
                        power = magicSkillLevel;
                }
                catch (Exception)
                {
                    power = magicSkillLevel;
                }
            }

            if (power < 1) power = caster.Level;
            #endregion

            Entity entity = Entity.Fighter;

            Enum.TryParse(caster.BaseProfession.ToString(), true, out entity);

            EntityBuilder builder = new EntityBuilder();

            string profession = entity.ToString().ToLower();

            if(!EntityLists.IsHuman(caster) || Rules.RollD(1, 100) < 50)
            {
                string entityLowerCase = caster.entity.ToString().ToLower();
                if (entityLowerCase.StartsWith("drow"))
                {
                    entity = Entity.Drow;
                    // Drow.Master summons drow thieves, Drow.Matriarch summons drow priestesses unless uncomment below lines
                    //List<string> availableProfessions = new List<string>() {"anathema", "cleric", "ravager", "rogue", "sorcerer" };
                    //profession = availableProfessions[Rules.Dice.Next(0, availableProfessions.Count - 1)];
                }
                else if (EntityLists.ELVES.Contains(caster.entity))
                {
                    List<Entity> allyEntities = new List<Entity>()
                    {
                        Entity.Grey_Elf,
                        Entity.High_Elf,
                        Entity.Wood_Elf
                    };
                }
                else
                {
                    List<Entity> allyEntities = new List<Entity>()
                    {
                        Entity.Gnome, Entity.Goblin,
                        Entity.Kobold,
                        Entity.Orc,
                        Entity.Tengu,
                    };

                    entity = allyEntities[Rules.Dice.Next(0, allyEntities.Count - 1)];
                }

                //profession = EntityBuilder.THIEF_SYNONYMS[Rules.Dice.Next(0, EntityBuilder.THIEF_SYNONYMS.Length - 1)];
            }

            NPC ally = builder.BuildEntity("allied", entity, caster.Map.ZPlanes[caster.Z], profession);

            // Set level.
            ally.Level = Math.Max(caster.Level, magicSkillLevel) + Rules.Dice.Next(-1, 1); // magic skill should be set to higher skill if using impcast
            if (ally.Level <= 0) ally.Level = 1;

            builder.SetOnTheFlyVariables(ally);
            ally.Alignment = caster.Alignment;
            builder.SetName(ally, profession);
            builder.SetDescriptions("allied", ally, caster.Map.ZPlanes[caster.Z], ally.BaseProfession.ToString().ToLower());
            EntityBuilder.SetGender(ally, caster.Map.ZPlanes[caster.Z]);
            EntityBuilder.SetVisualKey(ally.entity, ally);
            if (ally.spellDictionary.Count > 0) ally.magic = Skills.GetSkillForLevel(ally.Level + Rules.Dice.Next(-1, 1));
            GameSpell.FillSpellLists(ally);

            if (EntityLists.IsHumanOrHumanoid(ally))
            {
                List<int> armorToWear;

                if (power <= 13)
                    armorToWear = Autonomy.ItemBuilding.ArmorSets.ArmorSet.ArmorSetDictionary[Autonomy.ItemBuilding.ArmorSets.ArmorSet.FULL_STUDDED_LEATHER].GetArmorList(ally);
                else if (power < 16) armorToWear = Autonomy.ItemBuilding.ArmorSets.ArmorSet.ArmorSetDictionary[Autonomy.ItemBuilding.ArmorSets.ArmorSet.FULL_CHAINMAIL].GetArmorList(ally);
                else armorToWear = Autonomy.ItemBuilding.ArmorSets.ArmorSet.ArmorSetDictionary[Autonomy.ItemBuilding.ArmorSets.ArmorSet.FULL_BANDED_MAIL].GetArmorList(ally);

                foreach (int id in armorToWear)
                {
                    Item armor = Item.CopyItemFromDictionary(id);
                    // It's basic armor sets only. Label them as ethereal. (They will go back with the phantasm to their home plane. Given items drop.)
                    armor.special += " " + Item.EXTRAPLANAR;
                    ally.WearItem(armor);
                }
            }

            if (EntityLists.IsHumanOrHumanoid(ally) || EntityLists.ANIMALS_WIELDING_WEAPONS.Contains(ally.entity))
            {
                List<int> weaponsList = Autonomy.ItemBuilding.LootManager.GetBasicWeaponsFromArmory(ally, true);
                if (weaponsList.Count > 0)
                    ally.EquipRightHand(Item.CopyItemFromDictionary(weaponsList[Rules.Dice.Next(weaponsList.Count - 1)]));
                weaponsList = Autonomy.ItemBuilding.LootManager.GetBasicWeaponsFromArmory(ally, false);
                if (weaponsList.Count > 0)
                    ally.EquipLeftHand(Item.CopyItemFromDictionary(weaponsList[Rules.Dice.Next(weaponsList.Count - 1)]));

                if (ally.RightHand != null) ally.RightHand.special += " " + Item.EXTRAPLANAR;
                if (ally.LeftHand != null) ally.LeftHand.special += " " + Item.EXTRAPLANAR;
            }

            ally.Hits = ally.HitsFull;
            ally.Mana = ally.ManaFull;
            ally.Stamina = ally.StaminaFull;

            ally.Age = GameWorld.World.AgeCycles[Rules.Dice.Next(0, GameWorld.World.AgeCycles.Count - 1)];
            ally.special = "despawn summonthief";

            int oneMinute = Utils.TimeSpanToRounds(new TimeSpan(0, 1, 0));
            // 10 minutes + 2 minutes for every skill level past 3
            ally.RoundsRemaining = (oneMinute * 5) + ((power - ReferenceSpell.RequiredLevel) * oneMinute);
            //ally.species = Globals.eSpecies.Magical; // this may need to be changed for AI to work properly

            ally.canCommand = true;
            ally.IsMobile = true;
            ally.IsSummoned = true;
            ally.IsUndead = false;

            ally.FollowID = caster.UniqueID;

            ally.PetOwner = caster;
            caster.Pets.Add(ally);

            if (ally.CurrentCell != caster.CurrentCell)
                ally.CurrentCell = caster.CurrentCell;

            ReferenceSpell.SendGenericCastMessage(caster, null, true);

            ally.EmitSound(ally.idleSound);
            caster.WriteToDisplay((ally.longDesc.StartsWith("evil") ? "An " : "A ") + ally.longDesc + " answers your call for assistance.");
            caster.SendToAllInSight(caster.GetNameForActionResult() + " summons an ally.");
            if(caster is NPC)
                ally.MostHated = (caster as NPC).MostHated;
            ally.AddToWorld();
            return true;
        }
    }
}
