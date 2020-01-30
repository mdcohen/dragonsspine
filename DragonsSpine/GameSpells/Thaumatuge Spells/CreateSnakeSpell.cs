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
    [SpellAttribute(GameSpell.GameSpellID.Create_Snake, "createsnake", "Create Snake", "Summon a snake to attack enemies.",
        Globals.eSpellType.Conjuration, Globals.eSpellTargetType.Single, 7, 10, 1300, "", false, true, false, false, false, Character.ClassType.Thaumaturge)]
    public class CreateSnakeSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        private enum SnakePower
        {
            Snake = 1,
            Asp = 2,
            Cobra = 3,
            Boa = 4,
            Serpent = 5,
        };

        public bool OnCast(Character caster, string args)
        {
            /*      Power   Mana    Type        Armor (item IDs)            Skill (base)    Abilities
             *      1       7       snake       none                        6               none
             *      2       12      asp         none                        7               minor poison
             *      3       17      cobra       none                        8               minor poison
             *      4       22      boa         none                        9               major strength
             *      5       27      serpent     none                        10              wields weapons
            */

            #region Determine number of pets. Return false if at or above MAX_PETS.
            short petCount = 0;
            short serpentCount = 0;

            foreach (NPC pet in caster.Pets)
            {
                if (pet.QuestList.Count == 0)
                {
                    petCount++;
                    if (pet.Name == "serpent")
                        serpentCount++;
                }
            }

            if (petCount >= GameSpell.MAX_PETS)
            {
                caster.WriteToDisplay("You may only control " + GameSpell.MAX_PETS + " pets.");
                return false;
            }
            #endregion

            #region Clean up and then split the arguments.
            args = args.Replace(ReferenceSpell.Command, "");

            args = args.Trim();

            string[] sArgs = args.Split(" ".ToCharArray());
            #endregion

            #region Determine power.
            int power = 1; // default power

            if (sArgs.Length > 0)
            {
                try
                {
                    power = Convert.ToInt32(sArgs[0]);

                    if (power > (int)SnakePower.Serpent)
                        power = (int)SnakePower.Serpent;
                }
                catch (Exception)
                {
                    power = 1;
                }
            }
            #endregion

            Autonomy.EntityBuilding.EntityLists.Entity entity = Autonomy.EntityBuilding.EntityLists.Entity.None;
            
            #region Power determines mana cost and entity.
            switch (power)
            {
                case 2:
                    if (caster.Mana < ReferenceSpell.ManaCost + 5)
                    {
                        caster.Mana -= 5;
                        return false;
                    }
                    caster.Mana -= 5;
                    entity = Autonomy.EntityBuilding.EntityLists.Entity.Snake;
                    break;
                case 3:
                    if (caster.Mana < ReferenceSpell.ManaCost + 10)
                    {
                        caster.Mana -= 10;
                        return false;
                    }
                    caster.Mana -= 10;
                    entity = Autonomy.EntityBuilding.EntityLists.Entity.Snake;
                    break;
                case 4:
                    if (caster.Mana < ReferenceSpell.ManaCost + 15)
                    {
                        caster.Mana -= 15;
                        return false;
                    }
                    caster.Mana -= 15;
                    entity = Autonomy.EntityBuilding.EntityLists.Entity.Snake;
                    break;
                case 5:
                    // check mana cost or if caster already has a serpent
                    if (caster.Mana < ReferenceSpell.ManaCost + 20 || serpentCount > 0)
                    {
                        caster.Mana -= 20;
                        if (serpentCount > 0)
                            caster.WriteToDisplay("You may only control one serpent at a time.");
                        return false;
                    }
                    caster.Mana -= 20;
                    entity = Autonomy.EntityBuilding.EntityLists.Entity.Serpent;
                    break;
                default: // 1
                    if (caster.Mana < ReferenceSpell.ManaCost)
                    {
                        return false;
                    }
                    entity = Autonomy.EntityBuilding.EntityLists.Entity.Snake;
                    break;
            }
            #endregion

            int casterSkillLevel = Skills.GetSkillLevel(caster.magic);

            NPC summoned = NPC.LoadNPC(905, caster.FacetID, caster.LandID, caster.MapID, caster.X, caster.Y, caster.Z, -1);

            Autonomy.EntityBuilding.EntityBuilder builder = new Autonomy.EntityBuilding.EntityBuilder();

            summoned.Level = caster.Level + power;
            summoned.species = Globals.eSpecies.Reptile;
            summoned.BaseProfession = Character.ClassType.Fighter;

            summoned.entity = entity;
            builder.SetOnTheFlyVariables(summoned);

            summoned.Alignment = caster.Alignment;
            summoned.Age = 0;
            summoned.special = "despawn";
            summoned.wearing.Clear();
            summoned.WornEffectsList.Clear();
            summoned.WearItem(Item.CopyItemFromDictionary(8114)); // all summoned snakes wear sandwyrm scales, serpent clears from armor list below
            summoned.canCommand = true;
            summoned.IsMobile = true;
            summoned.IsSummoned = true;
            summoned.IsUndead = false;
            summoned.animal = true; // set to false below for serpent
            summoned.classFullName = "Fighter";

            int twoMinutes = Utils.TimeSpanToRounds(new TimeSpan(0, 2, 0));
            // 8 minutes + 2 minutes for every skill level past 10 minus 5 minutes for every power of the spell beyond 1.
            summoned.RoundsRemaining = (twoMinutes * 4) + ((Skills.GetSkillLevel(caster.magic) - ReferenceSpell.RequiredLevel) * twoMinutes) - (((int)power - 1) * twoMinutes);
            summoned.species = Globals.eSpecies.Magical; // this may need to be changed for AI to work properly

            summoned.Hits = summoned.HitsFull;
            summoned.Stamina = summoned.StaminaFull;
            summoned.Mana = summoned.ManaFull;

            long skillToNext = Skills.GetSkillForLevel(Skills.GetSkillLevel(caster.magic) - 2);
            summoned.unarmed = skillToNext;

            #region Name, description, poison -- based on power. Also, items for the serpent (power 5).
            switch (power)
            {
                case 1: // snake
                    summoned.Name = "snake";
                    summoned.shortDesc = "snake";
                    summoned.longDesc = "a large black adder";
                    break;
                case 2: // asp
                    summoned.Name = "asp";
                    summoned.shortDesc = "asp";
                    summoned.longDesc = "a venemous asp";
                    summoned.poisonous = (short)(5 + casterSkillLevel);
                    break;
                case 3: // cobra
                    summoned.Name = "cobra";
                    summoned.shortDesc = "cobra";
                    summoned.longDesc = "a huge king cobra";
                    summoned.poisonous = (short)(10 + casterSkillLevel);
                    break;
                case 4: // boa
                    summoned.Name = "boa";
                    summoned.shortDesc = "boa constrictor";
                    summoned.longDesc = "a massive boa constrictor";
                    summoned.Strength = 25;
                    summoned.strengthAdd = 10;
                    break;
                case 5: // serpent
                    summoned.Name = "serpent";
                    summoned.entity = Autonomy.EntityBuilding.EntityLists.Entity.Yaun__Ti;
                    summoned.shortDesc = "serpent";
                    summoned.longDesc = "a tall, green-scaled serpent with two muscular arms";
                    summoned.animal = false;
                    summoned.visualKey = "serpent";
                    summoned.Strength = 19;
                    summoned.Intelligence = 8;
                    summoned.Wisdom = 7;
                    summoned.strengthAdd = (summoned.Level / 2 ) - 3;
                    summoned.dexterityAdd = (summoned.Level / 2) - 3;
                    summoned.mace = 0;
                    summoned.bow = 0;
                    summoned.twoHanded = skillToNext;
                    summoned.sword = skillToNext;
                    summoned.magic = 0;
                    summoned.shuriken = 0;
                    summoned.staff = 0;
                    summoned.rapier = 0;
                    summoned.dagger = 0;
                    summoned.flail = 0;
                    summoned.halberd = skillToNext;
                    summoned.threestaff = 0;
                    summoned.bash = skillToNext;
                    summoned.wearing.Clear();
                    summoned.baseArmorClass = 5;
                    summoned.THAC0Adjustment = -4;
                    Item shortsword = Item.CopyItemFromDictionary(25020);
                    summoned.talentsDictionary.Add(Talents.GameTalent.GetTalent(Talents.GameTalent.TALENTS.DualWield).Command, DateTime.UtcNow);
                    summoned.EquipRightHand(shortsword);
                    summoned.EquipLeftHand(shortsword);
                    break;
            }
            #endregion

            foreach (Item item in summoned.wearing)
                item.special += " " + Item.EXTRAPLANAR;
            if (summoned.RightHand != null) summoned.RightHand.special += " " + Item.EXTRAPLANAR;
            if (summoned.LeftHand != null) summoned.LeftHand.special += " " + Item.EXTRAPLANAR;

            summoned.PetOwner = caster;
            caster.Pets.Add(summoned);
            return true;
        }
    }
}
