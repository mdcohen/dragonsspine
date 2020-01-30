using System;
using System.Collections.Generic;
using Entity = DragonsSpine.Autonomy.EntityBuilding.EntityLists.Entity;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Summon_Nature__s_Ally, "summonnaturesally", "Summon Nature's Ally", "Summon a nearby ally of nature to aid you.",
        Globals.eSpellType.Conjuration, Globals.eSpellTargetType.Self, 10, 3, 45000, "0284", true, true, true, false, false, Character.ClassType.Druid, Character.ClassType.Ranger)]
    public class SummonNaturesAllySpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        /// <summary>
        /// Item 1 = Skill level.
        /// Item 2 = Entity.
        /// Item 3 = Outdoors only.
        /// </summary>
        private readonly List<Tuple<int, Entity, bool>> AlliesAvailable = new List<Tuple<int, Entity, bool>>()
        {
            // Magic Skill Level 1 (Ranger Skill Level 3)
            new Tuple<int, Entity, bool>(1, Entity.Beetle, false),
            new Tuple<int, Entity, bool>(1, Entity.Fox, false),
            new Tuple<int, Entity, bool>(1, Entity.Rat, false),
            // 2
            new Tuple<int, Entity, bool>(2, Entity.Cat, false),
            new Tuple<int, Entity, bool>(2, Entity.Raven, true),
            // 3
            new Tuple<int, Entity, bool>(3, Entity.Dog, false),
            new Tuple<int, Entity, bool>(3, Entity.Hyena, true),
            // 4
            new Tuple<int, Entity, bool>(4, Entity.Boar, true),
            new Tuple<int, Entity, bool>(4, Entity.Eagle, true),
            new Tuple<int, Entity, bool>(4, Entity.Wolf, false),
            // 5
            new Tuple<int, Entity, bool>(5, Entity.Bighorn, true),
            new Tuple<int, Entity, bool>(5, Entity.Broodmare, true),
            new Tuple<int, Entity, bool>(5, Entity.Salamander, false),
            // 6
            new Tuple<int, Entity, bool>(6, Entity.Alligator, true),
            new Tuple<int, Entity, bool>(6, Entity.Stallion, true),
            new Tuple<int, Entity, bool>(6, Entity.Tiger, false),
            // 7
            new Tuple<int, Entity, bool>(7, Entity.Jaguar, false),
            new Tuple<int, Entity, bool>(7, Entity.Lion, false),
            new Tuple<int, Entity, bool>(7, Entity.Panther, false),
            // 8
            new Tuple<int, Entity, bool>(8, Entity.Bear, true),
            new Tuple<int, Entity, bool>(8, Entity.Smilodon, true),
            new Tuple<int, Entity, bool>(8, Entity.Ice_Lizard, true),
            // 9
            new Tuple<int, Entity, bool>(9, Entity.Griffin, true),
            new Tuple<int, Entity, bool>(9, Entity.Owlbear, true),
            new Tuple<int, Entity, bool>(9, Entity.Sabertooth, false),
            // 10
            new Tuple<int, Entity, bool>(10, Entity.Bulette, true),
            new Tuple<int, Entity, bool>(10, Entity.Dire_Wolf, false),
            new Tuple<int, Entity, bool>(10, Entity.Hippogriff, true),
            // 11
            new Tuple<int, Entity, bool>(11, Entity.Chimera, true),
            new Tuple<int, Entity, bool>(11, Entity.Manticore, true),
            new Tuple<int, Entity, bool>(11, Entity.Velociraptor, true),
            new Tuple<int, Entity, bool>(11, Entity.Minotaur, false),
            // 12
            new Tuple<int, Entity, bool>(12, Entity.Elemental, false),
            new Tuple<int, Entity, bool>(12, Entity.Satyr, true),
            new Tuple<int, Entity, bool>(12, Entity.Centaur, true),
            // 13
            new Tuple<int, Entity, bool>(13, Entity.Dryad, true),
            new Tuple<int, Entity, bool>(13, Entity.Pixie, true),
            new Tuple<int, Entity, bool>(13, Entity.Sprite, false),
            // 14
            new Tuple<int, Entity, bool>(14, Entity.Ent, true),
            new Tuple<int, Entity, bool>(14, Entity.Firbolg, false),
            new Tuple<int, Entity, bool>(14, Entity.Bulette, true),
            // 15
            new Tuple<int, Entity, bool>(15, Entity.Shambling_Mound, true),
            new Tuple<int, Entity, bool>(15, Entity.Wild_Elf, false),
            new Tuple<int, Entity, bool>(15, Entity.Wood_Elf, true),
            // 16
            new Tuple<int, Entity, bool>(16, Entity.Druid, false),
            new Tuple<int, Entity, bool>(16, Entity.Ranger, false),
            // 17
            new Tuple<int, Entity, bool>(17, Entity.Sapphire_Dragon, true),
            new Tuple<int, Entity, bool>(17, Entity.Topaz_Dragon, true),
            // 18
            new Tuple<int, Entity, bool>(18, Entity.Crystal_Dragon, true),
            new Tuple<int, Entity, bool>(18, Entity.Emerald_Dragon, true),
            // 19
            new Tuple<int, Entity, bool>(19, Entity.Amethyst_Dragon, true),

        };

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

            // Rangers get the spell at skill level 3, acts as skill level 1.
            if (!caster.IsImmortal && caster.BaseProfession != Character.ClassType.Druid) power = power - 3;

            if (power < 1) power = 1;
            #endregion

            List<Entity> availableEntities = new List<Entity>();

            foreach (Tuple<int, Entity, bool> tuple in AlliesAvailable)
            {
                if (tuple.Item1 > power) break; // Allies are in sequential power order.

                if (tuple.Item1 <= power)
                {
                    // Not all allies can be summoned indoors.
                    if (caster.CurrentCell.IsOutdoors || (!caster.CurrentCell.IsOutdoors && !tuple.Item3))
                        availableEntities.Add(tuple.Item2);
                    else continue;
                }
            }

            if (availableEntities.Count <= 0) return false;

            Entity entity = availableEntities[Rules.Dice.Next(0, availableEntities.Count - 1)];

            Autonomy.EntityBuilding.EntityBuilder builder = new Autonomy.EntityBuilding.EntityBuilder();

            string profession = "fighter";

            // If human or humanoid, possibly switch to another profession besides fighter.
            if (Autonomy.EntityBuilding.EntityLists.IsHumanOrHumanoid(entity))
            {
                Character.ClassType[] availableProfessions = new Character.ClassType[] { Character.ClassType.Fighter, Character.ClassType.Thaumaturge, Character.ClassType.Wizard };

                Character.ClassType randomProfession = availableProfessions[Rules.Dice.Next(0, availableProfessions.Length - 1)];

                switch(randomProfession)
                {
                    case Character.ClassType.Fighter:
                        profession = Autonomy.EntityBuilding.EntityBuilder.FIGHTER_SYNONYMS[Rules.Dice.Next(0, Autonomy.EntityBuilding.EntityBuilder.FIGHTER_SYNONYMS.Length - 1)];
                        break;
                    case Character.ClassType.Thaumaturge:
                        profession = Autonomy.EntityBuilding.EntityBuilder.THAUMATURGE_SYNONYMS[Rules.Dice.Next(0, Autonomy.EntityBuilding.EntityBuilder.THAUMATURGE_SYNONYMS.Length - 1)];
                        break;
                    case Character.ClassType.Wizard:
                        profession = Autonomy.EntityBuilding.EntityBuilder.WIZARD_SYNONYMS[Rules.Dice.Next(0, Autonomy.EntityBuilding.EntityBuilder.WIZARD_SYNONYMS.Length - 1)];
                        break;
                }
            }

            NPC ally = builder.BuildEntity("nature allied", entity, caster.Map.ZPlanes[caster.Z], profession);

            // Set level.
            ally.Level = Math.Max(caster.Level, magicSkillLevel) + Rules.Dice.Next(-1, 1); // magic skill should be set to higher skill if using impcast
            if (ally.Level <= 0) ally.Level = 1;           

            builder.SetOnTheFlyVariables(ally);
            ally.Alignment = caster.Alignment;
            builder.SetName(ally, ally.BaseProfession.ToString());
            builder.SetDescriptions("", ally, caster.Map.ZPlanes[caster.Z], ally.BaseProfession.ToString().ToLower());
            Autonomy.EntityBuilding.EntityBuilder.SetGender(ally, caster.Map.ZPlanes[caster.Z]);
            Autonomy.EntityBuilding.EntityBuilder.SetVisualKey(ally.entity, ally);
            if (ally.spellDictionary.Count > 0) ally.magic = Skills.GetSkillForLevel(ally.Level + Rules.Dice.Next(-1, 1));
            GameSpell.FillSpellLists(ally);
            
            if(Autonomy.EntityBuilding.EntityLists.IsHumanOrHumanoid(ally))
            {
                List<int> armorToWear;

                if(power <= 13)
                    armorToWear = Autonomy.ItemBuilding.ArmorSets.ArmorSet.ArmorSetDictionary[Autonomy.ItemBuilding.ArmorSets.ArmorSet.FULL_STUDDED_LEATHER].GetArmorList(ally);
                else if(power < 16) armorToWear = Autonomy.ItemBuilding.ArmorSets.ArmorSet.ArmorSetDictionary[Autonomy.ItemBuilding.ArmorSets.ArmorSet.FULL_CHAINMAIL].GetArmorList(ally);
                else armorToWear = Autonomy.ItemBuilding.ArmorSets.ArmorSet.ArmorSetDictionary[Autonomy.ItemBuilding.ArmorSets.ArmorSet.FULL_BANDED_MAIL].GetArmorList(ally);

                foreach (int id in armorToWear)
                {
                    Item armor = Item.CopyItemFromDictionary(id);
                    // It's basic armor sets only. Label them as ethereal. (They will go back with the phantasm to their home plane. Given items drop.)
                    armor.special += " " + Item.EXTRAPLANAR;
                    ally.WearItem(armor);
                }
            }

            if (Autonomy.EntityBuilding.EntityLists.IsHumanOrHumanoid(ally) || Autonomy.EntityBuilding.EntityLists.ANIMALS_WIELDING_WEAPONS.Contains(ally.entity))
            {
                List<int> weaponsList = Autonomy.ItemBuilding.LootManager.GetBasicWeaponsFromArmory(ally, true);
                if (weaponsList.Count > 0)
                    ally.EquipRightHand(Item.CopyItemFromDictionary(weaponsList[Rules.Dice.Next(weaponsList.Count - 1)]));
                weaponsList = Autonomy.ItemBuilding.LootManager.GetBasicWeaponsFromArmory(ally, false);
                if(weaponsList.Count > 0)
                    ally.EquipLeftHand(Item.CopyItemFromDictionary(weaponsList[Rules.Dice.Next(weaponsList.Count - 1)]));

                if (ally.RightHand != null) ally.RightHand.special += " " + Item.EXTRAPLANAR;
                if (ally.LeftHand != null) ally.LeftHand.special += " " + Item.EXTRAPLANAR;
            }

            ally.Hits = ally.HitsFull;
            ally.Mana = ally.ManaFull;
            ally.Stamina = ally.StaminaFull;

            ally.Age = GameWorld.World.AgeCycles[Rules.Dice.Next(0, GameWorld.World.AgeCycles.Count - 1)];
            ally.special = "despawn summonnaturesally";

            int twoMinutes = Utils.TimeSpanToRounds(new TimeSpan(0, 2, 0));
            // 18 minutes + 2 minutes for every skill level past 3
            ally.RoundsRemaining = (twoMinutes * 9) + ((magicSkillLevel - ReferenceSpell.RequiredLevel) * twoMinutes);
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
            ally.AddToWorld();
            return true;
        }
    }
}
