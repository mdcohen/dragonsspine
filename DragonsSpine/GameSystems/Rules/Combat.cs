using System;
using System.Collections.Generic;
using DragonsSpine.GameWorld;
using EntityLists = DragonsSpine.Autonomy.EntityBuilding.EntityLists;
using GameSpell = DragonsSpine.Spells.GameSpell;
using TALENTS = DragonsSpine.Talents.GameTalent.TALENTS;

namespace DragonsSpine
{
    public static class Combat
    {
        private const short NO_FUMBLE_SKILL_LEVEL = 12; // Skill level Expert.

        // If conditions are met, and attack messages contain the words below, a chance to poison exists.
        private static List<string> POISON_ATTACKS = new List<string>
        { "barbed", "fangs", "latches", "pierce", "pincer", "rakes", "spits", "stings", "savages", "tentacle", "thorns" };

        public static double AC_GetDexterityArmorClassBonus(Character attacker, Character target)
        {
            // TODO: dexterity bonus for traps, or non-Character object checks.
            if (attacker != null && (!target.seenList.Contains(attacker) || target.HasEffect(Effect.EffectTypes.Blind)) && !target.HasTalent(TALENTS.BlindFighting))
                return 0;

            if (target.Stunned > 0) return 0;

            double bonus = 0;

            // feared targets actually receive an AC bonus
            if (target.IsFeared)
                bonus += target.EffectsList[Effect.EffectTypes.Fear].Power / 2;

            // blind or stunned targets suffer a penalty
            if ((target.IsBlind && !target.HasTalent(TALENTS.BlindFighting)) || target.Stunned > 0 || target.IsResting)
                bonus -= 2.3;

            // further penalty if meditating
            if (target.IsMeditating)
                bonus -= 4.6;

            switch (Rules.GetEncumbrance(target))
            {
                case Globals.eEncumbranceLevel.Moderately:
                    bonus -= .7;
                    break;
                case Globals.eEncumbranceLevel.Heavily:
                    bonus -= 1.4;
                    break;
                case Globals.eEncumbranceLevel.Severely:
                    bonus -= 2.8;
                    break;
                default: // lightly encumbered
                    if (target.dexterityAdd > 0 && (!target.IsBlind || (target.IsBlind && target.HasTalent(TALENTS.BlindFighting))) && !target.IsFeared && target.Stunned <= 0 && !target.IsMeditating)
                        bonus += target.dexterityAdd / 2;
                    break;
            }

            return bonus;
        }

        public static double AC_GetShieldingArmorClass(Character chr, bool rangedAttack)
        {
            double shielding = 0;

            if (chr.Shielding < 0)
            {
                Utils.Log(chr.GetLogString() + " shielding is " + chr.Shielding, Utils.LogType.Unknown);
                return shielding;
            }
            else if (chr.Shielding == 0) return shielding;
            else shielding = rangedAttack ? Convert.ToDouble(chr.Shielding) : chr.Shielding / 3;

            if (shielding > chr.Land.MaxShielding) shielding = chr.Land.MaxShielding;

            return shielding;
        }

        public static double AC_GetArmorClassRating(Character target)
        {
            double rating = 0;

            foreach (Item armor in target.wearing)
            {
                if(armor.itemType == Globals.eItemType.Wearable ||
                    armor.wearLocation == Globals.eWearLocation.Hands ||
                    armor.wearLocation == Globals.eWearLocation.Feet)
                    rating += armor.armorClass;
            }

            if(target is NPC npc && npc.tanningResult != null)
            {
                foreach(int itemID in npc.tanningResult.Keys)
                {
                    if (!target.wearing.Exists(a => a.itemID == itemID))
                    {
                        Item item = Item.CopyItemFromDictionary(itemID);
                        if (item.itemType == Globals.eItemType.Wearable)
                            rating += item.armorClass;
                    }
                }
            }

            return rating;
        }

        public static double AC_GetUnarmedArmorClassBonus(Character attacker, Character target)
        {
            // TODO: dexterity bonus for traps, or non-Character object checks.
            if (attacker != null && (!target.seenList.Contains(attacker) || target.HasEffect(Effect.EffectTypes.Blind)) && !target.HasTalent(TALENTS.BlindFighting))
                return 0;

            if (target.Stunned > 0) return 0;

            // target is not blind, not feared and not stunned
            if ((!target.IsBlind || (target.IsBlind && target.HasTalent(TALENTS.BlindFighting))) && !target.IsFeared && target.Stunned <= 0)
            {
                // right hand is empty or right hand is weapon that uses unarmed skill, and not heavily/severely encumbered
                if ((target.RightHand == null || (target.RightHand != null && target.RightHand.skillType == Globals.eSkillType.Unarmed)) &&
                            Skills.GetSkillLevel(target.unarmed) > 0)// && // greater than Untrained in skill level
                            //Rules.GetEncumbrance(target) < Globals.eEncumbranceLevel.Severely)
                {
                    double encumbrance = target.GetEncumbrance();
                    double armorClass = 0;

                    #region Martial Artists understand how to distribute carried weight differently than other professions.
                    if (target.BaseProfession == Character.ClassType.Martial_Artist)
                    {
                        double skillMod = 0;

                        int skillLevel = Skills.GetSkillLevel(target.unarmed);

                        // martial artists may carry more weight beginning at 1st dan
                        if (skillLevel > 6)
                            skillMod = (skillLevel - 6) * .8;

                        armorClass = Skills.GetSkillLevel(target.unarmed) * .9;

                        if(encumbrance > Character.MAX_UNARMED_WEIGHT + skillMod)
                        {
                            double overEncumb = encumbrance - (Character.MAX_UNARMED_WEIGHT + skillMod);
                            armorClass -= overEncumb * .2;
                        }

                        if (target.LeftHand != null && target.LeftHand.skillType != Globals.eSkillType.Unarmed)
                            armorClass -= target.LeftHand.weight;
                    }
                    #endregion
                    else
                    {
                        armorClass = Skills.GetSkillLevel(target.unarmed) * .7;

                        if (encumbrance > Character.MAX_UNARMED_WEIGHT)
                        {
                            double overEncumb = encumbrance - Character.MAX_UNARMED_WEIGHT;
                            armorClass -= overEncumb * .4;
                        }

                        if (target.LeftHand != null && target.LeftHand.skillType != Globals.eSkillType.Unarmed)
                            armorClass -= target.LeftHand.weight;
                    }

                    return armorClass;
                }
            }

            return 0;
        }

        public static double AC_GetRightHandArmorClass(Character attacker, Character target)
        {
            // TODO: dexterity bonus for traps, or non-Character object checks.
            if (attacker != null && (!target.seenList.Contains(attacker) || target.HasEffect(Effect.EffectTypes.Blind)) && !target.HasTalent(TALENTS.BlindFighting))
                return 0;

            // Right hand is not empty.
            if (target.RightHand != null)
            {
                // Ensnared, no weapon armor class.
                if (target.HasEffect(Effect.EffectTypes.Ensnare) && target.RightHand.armorClass > 0) return 0;

                if ((target.Stunned > 0 || target.IsFeared) && target.RightHand.armorClass > 0) return 0;

                // if the item in the right hand is not a weapon and not a shield, do not modify armor class unless the item already has a negative armor class stat
                if (target.RightHand.itemType != Globals.eItemType.Weapon && target.RightHand.baseType != Globals.eItemBaseType.Shield)
                {
                    if (target.RightHand.armorClass < 0) return target.RightHand.armorClass;
                    else return 0;
                }

                // Items with negative armorClass always impact negatively.
                if (target.RightHand.armorClass < 0)
                {
                    // If wielding a two-handed weapon that already affects armor class negatively, an item in left hand will make it even worse. (silver greataxe)
                    if (target.RightHand.baseType == Globals.eItemBaseType.TwoHanded && target.LeftHand != null)
                    {
                        return 1.5 * target.RightHand.armorClass;
                    }

                    // Otherwise return the negative armor class of the item.
                    return target.RightHand.armorClass;
                }

                // If weapon in right hand.
                if (target.RightHand.itemType == Globals.eItemType.Weapon)
                {
                    // If two-handed weapon in right then left hand must be empty to get any armor class benefit.
                    if (target.RightHand.baseType == Globals.eItemBaseType.TwoHanded && target.LeftHand == null)
                    {
                        Globals.eEncumbranceLevel encumb = Rules.GetEncumbrance(target);

                        switch (encumb)
                        {
                            case Globals.eEncumbranceLevel.Lightly:
                                return target.RightHand.armorClass;
                            case Globals.eEncumbranceLevel.Moderately:
                                return target.RightHand.armorClass / 2;
                            default:
                                return 0;
                        }
                    }
                    // Threestaff in right & nothing in left hand depends upon skill level, unless Immortal skill level.
                    else if (target.RightHand.baseType == Globals.eItemBaseType.Threestaff)
                    {
                        int skillLevel = Skills.GetSkillLevel(target.threestaff);

                        if (skillLevel >= 19)
                            return target.RightHand.armorClass + 6;

                        if (target.LeftHand == null)
                        {
                            if (skillLevel <= 0) return 0;

                            if (skillLevel > 0 && skillLevel <= 4)
                                return target.RightHand.armorClass + 1;
                            else if (skillLevel >= 5 && skillLevel <= 8)
                                return target.RightHand.armorClass + 2;
                            else if (skillLevel >= 9 && skillLevel <= 12)
                                return target.RightHand.armorClass + 3;
                            else if (skillLevel >= 13 && skillLevel <= 16)
                                return target.RightHand.armorClass + 4;
                            else return target.RightHand.armorClass + 5;
                        }
                        else return Math.Round(target.RightHand.armorClass / 2);

                    }
                    // Staff should be wielded with two hands in order to receive full armor class bonus.
                    //else if (target.RightHand.baseType == Globals.eItemBaseType.Staff)
                    //{
                    //    if (target.LeftHand == null)
                    //    {
                    //        return target.RightHand.armorClass;
                    //    }
                    //    else
                    //    {
                    //        return target.RightHand.armorClass / 2;
                    //    }
                    // get half armor class if wielding two handed weapon with one hand
                    else if (target.RightHand.skillType == Globals.eSkillType.Two_Handed)
                    {
                        if (target.LeftHand != null)
                            return target.RightHand.armorClass / 2;
                        else return target.RightHand.armorClass;
                    }
                    else return target.RightHand.armorClass;
                }
                else
                {
                    return target.RightHand.armorClass;
                }
            }
            return 0;
        }

        public static double AC_GetLeftHandArmorClass(Character attacker, Character target)
        {
            // TODO: dexterity bonus for traps, or non-Character object checks.
            if (attacker != null && target != null && (!target.seenList.Contains(attacker) || target.HasEffect(Effect.EffectTypes.Blind)) && !target.HasTalent(TALENTS.BlindFighting))
                return 0;

            // There is an item in the left hand.
            if (target.LeftHand != null)
            {
                // Ensnared, no weapon armor class.
                if (target.HasEffect(Effect.EffectTypes.Ensnare) && target.LeftHand.armorClass > 0) return 0;

                if ((target.Stunned > 0 || target.IsFeared) && target.LeftHand.armorClass > 0) return 0;

                // TODO: dexterity bonus for traps, or non-Character object checks.
                if (attacker != null && !target.seenList.Contains(attacker) && !target.HasTalent(TALENTS.BlindFighting))
                    return 0;

                // if the item in the left hand is not a weapon and not a shield, do not modify armor class unless the item already has a negative armor class stat
                if (target.LeftHand.itemType != Globals.eItemType.Weapon && target.LeftHand.baseType != Globals.eItemBaseType.Shield)
                {
                    if (target.LeftHand.armorClass < 0) return target.LeftHand.armorClass;
                    else return 0;
                }

                // Negative armor class.
                if (target.LeftHand.armorClass < 0)
                    return target.LeftHand.armorClass;

                // Weapons in the right and left hands.
                if (target.RightHand != null && target.RightHand.itemType == Globals.eItemType.Weapon && target.LeftHand.itemType == Globals.eItemType.Weapon && target.LeftHand.wearLocation == Globals.eWearLocation.None)
                {
                    // must have dual wield talent in order to receive AC bonus from left hand, if right hand is not empty
                    if (target.HasTalent(TALENTS.DualWield) && Talents.GameTalent.GameTalentDictionary["dualwield"].Handler.OnPerform(target, "left"))
                    {
                        Item RHweapon = target.RightHand;

                        switch (RHweapon.baseType)
                        {
                            case Globals.eItemBaseType.Staff:
                            case Globals.eItemBaseType.Threestaff:
                                return target.LeftHand.armorClass / 2;
                            case Globals.eItemBaseType.TwoHanded:
                                return -target.LeftHand.armorClass;
                            default:
                                return target.LeftHand.armorClass;

                        }
                    }
                }
                else if(target.LeftHand.wearLocation == Globals.eWearLocation.None) return target.LeftHand.armorClass; // This should be a shield.
            }

            return 0;
        }

        public static double DND_GetHitLocation(Character attacker, Character target)
        {
            double multiplier = 0.0;

            int location = Rules.Dice.Next(1, Enum.GetValues(typeof(Globals.eWearLocation)).Length);

            switch ((Globals.eWearLocation)location)
            {
                case Globals.eWearLocation.Head:
                    multiplier = 2.2;
                    break;
                case Globals.eWearLocation.Face:
                    multiplier = 2.4;
                    break;
                case Globals.eWearLocation.Ear:
                    multiplier = 1.9;
                    break;
                case Globals.eWearLocation.Neck:
                    multiplier = 2.7;
                    break;
                case Globals.eWearLocation.Back:
                case Globals.eWearLocation.Torso:
                    multiplier = 1.5;
                    break;
                case Globals.eWearLocation.Shoulders:
                    multiplier = 1.8;
                    break;
                case Globals.eWearLocation.Waist:
                    multiplier = 1.7;
                    break;
                case Globals.eWearLocation.Legs:
                    multiplier = 1.9;
                    break;
                default:
                    multiplier = 1.3;
                    break;
            }

            // TODO: Ranger called shot here...

            string critLocation = CriticalHitLocation((Globals.eWearLocation)location, target);

            if (critLocation != "")
            {
                attacker.WriteToDisplay("You have scored a critical hit to your target's " + ((Globals.eWearLocation)location).ToString().ToLower() + "!");
                target.WriteToDisplay("You have suffered a critical hit to your " + ((Globals.eWearLocation)location).ToString().ToLower() + "!");
            }
            else
            {
                attacker.WriteToDisplay("You have scored a critical hit!");
                target.WriteToDisplay("You have suffered a critical hit!");
            }

            // TODO: put these into character object variable(s) 9/13/2019
            if (attacker.HasEffect(Effect.EffectTypes.Ferocity))
                multiplier += .2; // 20%

            if (attacker.HasEffect(Effect.EffectTypes.Savagery))
                multiplier += .2; // 20%

            return multiplier;
        }

        public static string CriticalHitLocation(Globals.eWearLocation location, Character target)
        {
            if (EntityLists.IsHumanOrHumanoid(target) || EntityLists.IsGiantKin(target))
                return Utils.FormatEnumString(location.ToString().ToLower());

            // TODO: based on species and other attributes
            return "";
        }

        public static void DoFlag(Character attacker, Character target)
        {
            // Safety net.
            if (target == null || attacker == null) return;

            // Immortal flag does not flag attacker.
            if (attacker.IsImmortal) return;

            // No flagging for implementors using pets against anything.
            if (attacker.PetOwner != null && target is PC)
            {
                if (attacker.PetOwner.IsImmortal || ((attacker.PetOwner is PC) && (attacker.PetOwner as PC).ImpLevel >= Globals.eImpLevel.AGM))
                    return;
            }

            // Make sure pets don't flag their owners.
            if (target.PetOwner != null && target.PetOwner == attacker)
                return;

            // Flags are never set for players in a boxing ring.
            if (target is PC && target.CurrentCell != null && target.CurrentCell.IsBoxingRing && attacker is PC && attacker.CurrentCell != null && attacker.CurrentCell.IsBoxingRing)
                return;

            // attacker's pets cannot be flagged
            if (attacker != target && attacker.IsPC && (!(target is NPC) || ((target is NPC) && !attacker.Pets.Contains(target as NPC))) &&
                !target.FlaggedUniqueIDs.Contains(attacker.UniqueID))
            {
                target.FlaggedUniqueIDs.Add(attacker.UniqueID);
                target.WriteToDisplay(attacker.GetNameForActionResult() + " has been flagged as hostile. Pet " + Character.PRONOUN_2[(int)attacker.gender].ToLower() + " to remove the flag, or take other action.");
            }

            // even in a coexistant world...? 9/21/2019 Eb
            if(attacker != target && attacker is NPC && target is NPC && !target.FlaggedUniqueIDs.Contains(attacker.UniqueID))
            {
                target.FlaggedUniqueIDs.Add(attacker.UniqueID);
            }

            if(attacker != target && target is PC && target.Pets.Count > 0)
            {
                foreach (NPC pet in target.Pets)
                    if (!pet.FlaggedUniqueIDs.Contains(attacker.UniqueID))
                        pet.FlaggedUniqueIDs.Add(attacker.UniqueID);
            }
        }

        public static int DND_RollToHit(Character attacker, Character target, object attackWeapon, ref int attackRoll)
        {
            if (attacker.IsPC || target.IsPC)
                DoFlag(attacker, target);

            int roll = Rules.RollD(1, 20);

            #region Add 1 to a specialized berserker/fighter/ranger roll
            if (attackWeapon != null && (attackWeapon is Item) && Array.IndexOf(World.WeaponSpecializationProfessions, attacker.BaseProfession) != -1 &&
                attacker.fighterSpecialization == (attackWeapon as Item).skillType) roll++;
            #endregion

            #region Add 1 to the roll of a hidden attacker remaining hidden using a piercing weapon.
            if (attacker.IsHidden && !Rules.DetectHidden(attacker, target) && attackWeapon != null && (attackWeapon is Item) &&
                (attackWeapon as Item).special.Contains("pierce") && !(attackWeapon as Item).special.Contains("nopierce"))
                roll++;
            #endregion

            #region Add 1 to the roll of a Ranger using a bow.
            if (attacker.BaseProfession == Character.ClassType.Ranger && attackWeapon != null && (attackWeapon is Item) && (attackWeapon as Item).baseType == Globals.eItemBaseType.Bow)
                roll++;
            #endregion

            #region Add 1 to the roll of a bow or thrown weapon attack if Hunter's Mark
            if (attacker.EffectsList.ContainsKey(Effect.EffectTypes.Hunter__s_Mark) &&
                attackWeapon != null && attackWeapon is Item && ((attackWeapon as Item).skillType == Globals.eSkillType.Bow || (attackWeapon as Item).skillType == Globals.eSkillType.Shuriken))
                roll++;
            #endregion

            // Anything above this with an add to the roll gaurentees no critical misses. Currently only hidden attackers with piercing weapons and a specialized fighter.
            if (roll <= 1) { return -1; } // automatic critical miss

            if (roll >= 20) { return 2; } // automatic critical hit
            
            #region Add to the roll of a thief using the BackstabTalent
            if (attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Backstab)) roll += Talents.BackstabTalent.IncreasedChanceToHit;
            #endregion

            #region Add to the roll of a thief using the AssassinateTalent
            if (attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Assassinate)) roll += Talents.AssassinateTalent.IncreasedChanceToHit;
            #endregion

            // record attacker's roll, used in other combat related methods
            attackRoll = roll;

            // base armor class
            double targetAC = target.baseArmorClass; // images will stay at 20, easier to hit

            // grant armor class bonuses to actual characters, not images
            if (!target.IsImage)
            {
                // armor and unarmed blocking do not aid against a ranged spell attack
                // only base armor class, dexterity and items in each hand aid against a ranged spell attack (acid orb)
                if (!(attackWeapon is Spells.ISpellHandler))
                {
                    // armor class rating
                    targetAC -= Combat.AC_GetArmorClassRating(target);

                    // unarmed armor class bonus
                    targetAC -= Combat.AC_GetUnarmedArmorClassBonus(attacker, target);
                }

                #region Shield spell armor class adjustments
                /*
                 * If the attacker shot, threw or jumpkicked the target then we adjust the targetAC by the full shield value.
                 * Otherwise we divide the shield value in half. Keep in mind a negative shield value will affect the targetAC...
                 * (eg: future debuff spells)
                 */

                if (attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Cast) || attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot) ||
                    attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Throw) || attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Jumpkick))
                {
                    targetAC -= Combat.AC_GetShieldingArmorClass(target, true);
                }
                else targetAC -= Combat.AC_GetShieldingArmorClass(target, false);
                #endregion

                if (!(attackWeapon is Spells.ISpellHandler))
                {
                    #region Right hand and left hand armor class adjustments
                    if (target.RightHand != null && !target.RightHand.IsAttunedToOther(target) &&
                            target.RightHand.AlignmentCheck(target) && target.RightHand.wearLocation != Globals.eWearLocation.Hands)
                    {
                        // right hand AC adjustment
                        targetAC -= Combat.AC_GetRightHandArmorClass(attacker, target);
                    }

                    if (target.LeftHand != null && !target.LeftHand.IsAttunedToOther(target) &&
                        target.LeftHand.AlignmentCheck(target))
                    {
                        // left hand AC adjustment
                        targetAC -= Combat.AC_GetLeftHandArmorClass(attacker, target);
                    }
                    #endregion
                }

                // skill AC adjustment
                //if (target.GetWeaponSkillLevel(target.RightHand) > attacker.GetWeaponSkillLevel(attacker.RightHand))
                //    targetAC -= target.GetWeaponSkillLevel(target.RightHand) - attacker.GetWeaponSkillLevel(attacker.RightHand);

                targetAC -= Combat.AC_GetDexterityArmorClassBonus(attacker, target);

                // 8/5/2013 this type of armor class bonus does not require another function to be written yet -Eb
                if (attacker.IsUndead && target.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Undead))
                    targetAC -= target.EffectsList[Effect.EffectTypes.Protection_from_Undead].Power;

                if(EntityLists.IsHellspawn(attacker) && target.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Hellspawn))
                    targetAC -= target.EffectsList[Effect.EffectTypes.Protection_from_Hellspawn].Power;

            }
            else if (Rules.CheckPerception(attacker)) // target is an image and the attacker makes a successful perception check
                targetAC = 20;

            int modifiedTHAC0 = Combat.DND_GetModifiedTHAC0(attacker, attackWeapon);
            int calculatedArmorClass = (int)Math.Round(targetAC);
            int rollNeeded = modifiedTHAC0 - calculatedArmorClass;

            #region if DEBUG (currently commented out)
#if DEBUG
            if (attacker.IsPC)
            {
                //attacker.WriteToDisplay("roll = " + roll + ", rollNeeded = (" + rollNeeded +"), DND_GetModifiedTHAC0(attacker, weapon) [" +
                //    modifiedTHAC0 + "] + CalculateArmorClass(targetAC = " + targetAC + ") [" + calculatedArmorClass + "]");

                try
                {
                    Utils.Log("DND_RollToHit(attacker, target, obj attackWeapon)", Utils.LogType.CombatTesting);
                    Utils.Log("ATTACKER: " + attacker.GetLogString(), Utils.LogType.CombatTesting);
                    if (target != null) Utils.Log("TARGET: " + target.GetLogString(), Utils.LogType.CombatTesting);
                    else Utils.Log("TARGET: DEAD", Utils.LogType.CombatTesting);
                    if (attackWeapon != null && attackWeapon is Item)
                        Utils.Log("WEAPON: " + (attackWeapon as Item).GetLogString(), Utils.LogType.CombatTesting);
                    else if (attackWeapon != null && attackWeapon is Spells.ISpellHandler) Utils.Log("SPELL: " + (attackWeapon as Spells.ISpellHandler).ReferenceSpell.Name, Utils.LogType.CombatTesting);
                    Utils.Log("Roll (1d20) = " + roll + ", rollNeeded = " + rollNeeded, Utils.LogType.CombatTesting);
                    Utils.Log("rollNeeded = DND_GetModifiedTHAC0(attacker, weapon | spell) + CalculateArmorClass(targetAC)", Utils.LogType.CombatTesting);
                    Utils.Log("DND_GetModifiedThac0(attacker, weapon) = " + modifiedTHAC0, Utils.LogType.CombatTesting);
                    Utils.Log("targetAC = " + targetAC, Utils.LogType.CombatTesting);
                    Utils.Log("CalculateArmorClass(targetAC) = " + calculatedArmorClass, Utils.LogType.CombatTesting);
                    Utils.Log("*********************************", Utils.LogType.CombatTesting);
                }
                catch (Exception) { }

            }

            if (target.IsPC)
            {
                //target.WriteToDisplay("roll = " + roll + ", rollNeeded = (" + rollNeeded + "), DND_GetModifiedTHAC0(attacker, weapon) [" +
                //    modifiedTHAC0 + "] + CalculateArmorClass(targetAC = " + targetAC + ") [" + calculatedArmorClass + "]");
                try
                {
                    Utils.Log("DND_RollToHit(attacker, target, obj attackWeapon)", Utils.LogType.CombatTesting);
                    if (attacker != null) Utils.Log("ATTACKER: " + attacker.GetLogString(), Utils.LogType.CombatTesting);
                    else Utils.Log("ATTACKER: DEAD" + attacker.GetLogString(), Utils.LogType.CombatTesting);
                    Utils.Log("TARGET: " + target.GetLogString(), Utils.LogType.CombatTesting);
                    if (attackWeapon != null && attackWeapon is Item)
                        Utils.Log("WEAPON: " + (attackWeapon as Item).GetLogString(), Utils.LogType.CombatTesting);
                    else if (attackWeapon != null && attackWeapon is Spells.ISpellHandler) Utils.Log("SPELL: " + (attackWeapon as Spells.ISpellHandler).ReferenceSpell.Name, Utils.LogType.CombatTesting);
                    Utils.Log("Roll (1d20) = " + roll + ", rollNeeded = " + rollNeeded, Utils.LogType.CombatTesting);
                    Utils.Log("rollNeeded = DND_GetModifiedTHAC0(attacker, weapon | spell) + CalculateArmorClass(targetAC)", Utils.LogType.CombatTesting);
                    Utils.Log("DND_GetModifiedThac0(attacker, weapon) = " + modifiedTHAC0, Utils.LogType.CombatTesting);
                    Utils.Log("targetAC = " + targetAC, Utils.LogType.CombatTesting);
                    Utils.Log("CalculateArmorClass(targetAC) = " + calculatedArmorClass, Utils.LogType.CombatTesting);
                    Utils.Log("*********************************", Utils.LogType.CombatTesting);
                }
                catch (Exception) { }
            }
#endif 
            #endregion

            if (roll >= rollNeeded)
            {
                int critChance = 0;

                // Immproved critical works for spells and physical combat.
                if (attacker.HasEffect(Effect.EffectTypes.Ferocity, out Effect ferocityEffect))
                    critChance += ferocityEffect.Power;

                if (attackWeapon is Spells.ISpellHandler)
                {
                    critChance += Skills.GetSkillLevel(attacker.magic);
                }
                else if (attackWeapon is Item)
                {
                    critChance += attacker.GetWeaponSkillLevel(attackWeapon as Item);

                    // specialized fighters gain 1% per level after specialization level
                    if (attacker.fighterSpecialization == (attackWeapon as Item).skillType) critChance += attacker.Level - Character.WARRIOR_SPECIALIZATION_LEVEL;

                    // savage critical effect
                    if(attacker.HasEffect(Effect.EffectTypes.Savagery, out Effect savageryEffect))
                        critChance += savageryEffect.Power;
                }

                if (Rules.Dice.Next(1, 100) <= critChance) // chance of crit increased with skill level - 1% per level up to 20%
                    return 2;

                return 1; // hit
            }

            return 0; // miss
        }

        /// <summary>
        /// Only called on a successful ToHit roll.
        /// </summary>
        /// <param name="ch">Character performing the attack.</param>
        /// <param name="target">Target of the attack.</param>
        /// <param name="attackWeapon">Attack weapon.</param>
        /// <param name="critical">Whether or not the attack is a critical hit.</param>
        public static void DND_Attack(Character ch, Character target, Item attackWeapon, bool critical)
        {
            Globals.eSkillType skillType = Globals.eSkillType.Unarmed;
            int damage = 0;
            int totalDamage = 0;
            int damageBonus = 0;
            int criticalDamage = 0;
            int hardHitterDamage = 0;
            string dmgAdjective = "fatal";

            // attacker is wielding a weapon, or using a worn weapon (ie: gauntlets, boots)
            if (attackWeapon != null)
            {
                #region Proc Effects
                // Check for a proc first. If the proc (spell damage) kills the target, award exp AND skill gain.
                //Dictionary<GameSpell, int> procEffects = null;

                //if ((critical || Rules.RollD(1, 100) >= 15 + ch.Level) && attackWeapon.HasProcEffects(out procEffects))
                //{
                //    long savedMagicSkill = ch.magic;
                //    GameSpell savedPreppedSpell = ch.preppedSpell;

                //    foreach (GameSpell spell in procEffects.Keys)
                //    {
                //        if (Rules.RollD(1, 100) >= 50)
                //        {
                //            // Should a WeaponProc child of Character be instantiated here?
                //            ch.preppedSpell = spell;
                //            ch.magic = Skills.GetSkillToMax(procEffects[spell]);
                //            spell.CastSpell(ch, target.UniqueID.ToString());
                //            if (target.IsDead)
                //            {
                //                ch.magic = savedMagicSkill;
                //                ch.preppedSpell = savedPreppedSpell;
                //                return;
                //            }
                //        }
                //    }
                //    ch.magic = savedMagicSkill;
                //    ch.preppedSpell = savedPreppedSpell;
                //} 
                #endregion

                skillType = attackWeapon.skillType;
                damageBonus += ch.GetWeaponSkillLevel(attackWeapon);
                damage = CalculateWeaponDamage(ch, attackWeapon);

                // "unarmed" attack, but gauntlets or boots were sent here as attackWeapon
                if (attackWeapon == ch.GetInventoryItem(Globals.eWearLocation.Hands) || attackWeapon == ch.GetInventoryItem(Globals.eWearLocation.Feet))
                    damage += CalculateUnarmedDamage(ch);
            }
            else // using pure unarmed combat
            {
                damageBonus += ch.GetWeaponSkillLevel(null);
                damage += CalculateUnarmedDamage(ch);
            }

            #region Critical Hits
            // critical damage
            if (critical)
            {
                double multiplier = Combat.DND_GetHitLocation(ch, target);

                criticalDamage = (int)(damage * multiplier) - damage;

                // TODO increase stun chance for various reasons...

                // below 1.8 will not cause a stun
                if (attackWeapon != null)
                {
                    if (multiplier < 1.8 && attackWeapon.itemID != Item.ID_SMOKEYS_SHOVEL) critical = false;
                    else if (multiplier < 1.5 && attackWeapon.itemID == Item.ID_SMOKEYS_SHOVEL) critical = false; //shovel stuns a little more often
                }
            }
            #endregion

            // tally damage
            totalDamage += damage; // add damage to total damage
            totalDamage += damageBonus; // add damage bonus to total damage
            totalDamage += Rules.GetFullAbilityStat(ch, Globals.eAbilityStat.Strength) / 2; // add strength damage
            totalDamage += ch.strengthAdd; // add strength add to total damage
            // range shots add damage for dexterity
            if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Throw) || ch.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot))
            {
                totalDamage += Rules.GetFullAbilityStat(ch, Globals.eAbilityStat.Dexterity) / 2;
                totalDamage += ch.dexterityAdd;
            }
            

            // constitution soak
            if (!critical) totalDamage -= Rules.GetFullAbilityStat(ch, Globals.eAbilityStat.Constitution) / 2;

            // talent damage modifiers
            if (!ch.CommandsProcessed.Contains(CommandTasker.CommandType.Attack))
            {
                if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Assassinate))
                {
                    totalDamage = target.Hits;
                }
                else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Backstab))
                {
                    totalDamage = Convert.ToInt32(totalDamage * Talents.BackstabTalent.DamageMultiplier) + Rules.RollD(ch.Level, 6);
                }
                else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.BattleCharge))
                {
                    totalDamage = totalDamage * Talents.BattleChargeTalent.DamageMultiplier + Rules.RollD(ch.Level, 4);
                }
                else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.RoundhouseKick))
                {
                    totalDamage = totalDamage * Talents.RoundhouseKickTalent.DamageMultiplier + Rules.RollD(ch.Level, 4);
                }
                else if(ch.CommandsProcessed.Contains(CommandTasker.CommandType.Cleave))
                {
                    totalDamage = Convert.ToInt32(totalDamage * Talents.CleaveTalent.DamageMultiplier) + Rules.RollD(ch.Level, 3);
                }
            }

            totalDamage += criticalDamage; // add critical damage
            
            if (EntityLists.HARD_HITTERS.Contains(ch.entity)) // or berserkers ability?
                totalDamage = (int)(totalDamage * 2.1);

            // confirm total damage is atleast 1
            if (totalDamage <= 0) totalDamage = Rules.RollD(1, 4);

            // determine damage adjective
            int percentage = (int)(((float)totalDamage / (float)target.Hits) * 100); // get the damage adjective

            if (percentage >= 0 && percentage < 15) dmgAdjective = "light";
            else if (percentage >= 15 && percentage < 31) dmgAdjective = "moderate";
            else if (percentage >= 31 && percentage < 70) dmgAdjective = "heavy";
            else if (percentage >= 70 && percentage < 100) dmgAdjective = "severe";
            else if (percentage >= 100) dmgAdjective = "fatal";

            #region Display pet damage if enabled
            if (target != null && !target.IsDead && ch != null && !ch.IsDead)
            {
                if (ch.PetOwner != null && ch.PetOwner.IsPC && (ch.PetOwner as PC).DisplayPetDamage)
                    ch.PetOwner.WriteToDisplay("Your pet " + ch.Name + " does " + totalDamage + " damage to " + target.Name + ".");
                if (target.PetOwner != null && target.PetOwner.IsPC && (target.PetOwner as PC).DisplayPetDamage)
                    target.PetOwner.WriteToDisplay("Your pet " + target.Name + " took " + totalDamage + " from " + ch.Name + ".");
            }
            #endregion

            // emit sound
            target.EmitSound(Sound.GetSoundForDamage(dmgAdjective));

            if (ch is PC)
            {
                #region Give skill experience
                if (!target.IsPC) // player vs. environment
                    Skills.GiveSkillExp(ch, target, skillType); // give skill experience for player vs. environment
                else // pvp and sparring
                    Skills.GiveSkillExp(ch, ch.Level + target.Level, skillType); // give skill experience for player vs. player
                #endregion

                Combat.SendSuccessfulAttackResults(ch as PC, target, attackWeapon, dmgAdjective, totalDamage);
            }
            else if (!Combat.SendSuccessfulAttackResults(ch as NPC, target, attackWeapon, dmgAdjective))
            {
                target.WriteToDisplay(ch.GetNameForActionResult() + " hits you!");                
            }

            // check special talents attack for damage modification
            //if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Cleave))
            //    totalDamage += (Rules.RollD(1, 10) + 5);

            //if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Leg_Sweep))
            //    totalDamage -= (Rules.RollD(1, 12) + 6);

            #region DPS Logging
            // Log DPS
            if (ch.DPSLoggingEnabled)
            {
                // Martial Arts DPS logging.
                if (attackWeapon == null || (attackWeapon == ch.GetInventoryItem(Globals.eWearLocation.Hands) || attackWeapon == ch.GetInventoryItem(Globals.eWearLocation.Feet)))
                {
                    GameSystems.DPSCalculator.AddMartialArtsDPSValue(ch, DragonsSpineMain.GameRound, totalDamage);
                }
                // Range weapon DPS logging.
                else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot) || ch.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot))
                {
                    GameSystems.DPSCalculator.AddRangeWeaponDPSValue(ch, DragonsSpineMain.GameRound, attackWeapon.itemID, totalDamage);
                }
                // Melee weapon DPS logging.
                else GameSystems.DPSCalculator.AddMeleeWeaponDPSValue(ch, DragonsSpineMain.GameRound, attackWeapon.itemID, totalDamage);
            } 
            #endregion

            #region Combat Logging
            if (DragonsSpineMain.Instance.Settings.DetailedCombatLogging)
            {
                if (target.IsPC)
                {
                    if (critical)
                    {
                        if (attackWeapon == null)
                        {
                            Utils.Log(target.GetLogString() + " took (Damage: " + damage + " + Damage Bonus: " + damageBonus + " + Temp Strength: " + ch.TempStrength + " + Strength Add: " + ch.strengthAdd + " + Critical Damage: " + criticalDamage + " + Hard Hitter Damage: " + hardHitterDamage + ") = " + totalDamage + " from " + ch.GetLogString() + " Unarmed (" + ch.LastCommand + ") Skill: " +
                                Skills.GetSkillTitle(Globals.eSkillType.Unarmed, ch.BaseProfession, ch.GetSkillExperience(Globals.eSkillType.Unarmed), ch.gender), Utils.LogType.CriticalCombatDamageToPlayer);
                        }
                        else
                        {
                            Utils.Log(target.GetLogString() + " took (Damage: " + damage + " + Damage Bonus: " + damageBonus + " + Temp Strength: " + ch.TempStrength + " + Strength Add: " + ch.strengthAdd + " + Critical Damage: " + criticalDamage + " + Hard Hitter Damage: " + hardHitterDamage + ") = " + totalDamage + " from " + ch.GetLogString() + " " + attackWeapon.GetLogString() + " (" + ch.LastCommand + ") Skill: " +
                                Skills.GetSkillTitle(attackWeapon.skillType, ch.BaseProfession, ch.GetSkillExperience(attackWeapon.skillType), ch.gender), Utils.LogType.CriticalCombatDamageToPlayer);
                        }
                    }
                    else
                    {
                        if (attackWeapon == null)
                        {
                            Utils.Log(target.GetLogString() + " took (Damage: " + damage + " + Damage Bonus: " + damageBonus + " + Temp Strength: " + ch.TempStrength + " + Strength Add: " + ch.strengthAdd + " + Hard Hitter Damage: " + hardHitterDamage + ") = " + totalDamage + " from " + ch.GetLogString() + " Unarmed (" + ch.LastCommand + ") Skill: " +
                                Skills.GetSkillTitle(Globals.eSkillType.Unarmed, ch.BaseProfession, ch.GetSkillExperience(Globals.eSkillType.Unarmed), ch.gender), Utils.LogType.CombatDamageToPlayer);
                        }
                        else
                        {
                            Utils.Log(target.GetLogString() + " took (Damage: " + damage + " + Damage Bonus: " + damageBonus + " + Temp Strength: " + ch.TempStrength + " + Strength Add: " + ch.strengthAdd + " + Hard Hitter Damage: " + hardHitterDamage + ") = " + totalDamage + " from " + ch.GetLogString() + " " + attackWeapon.GetLogString() + " (" + ch.LastCommand + ") Skill: " +
                                Skills.GetSkillTitle(attackWeapon.skillType, ch.BaseProfession, ch.GetSkillExperience(attackWeapon.skillType), ch.gender), Utils.LogType.CombatDamageToPlayer);
                        }
                    }
                }
                else
                {
                    if (critical)
                    {
                        if (attackWeapon == null)
                        {
                            Utils.Log(target.GetLogString() + " took (Damage: " + damage + " + Damage Bonus: " + damageBonus + " + Temp Strength: " + ch.TempStrength + " + Strength Add: " + ch.strengthAdd + " + Critical Damage: " + criticalDamage + " + Hard Hitter Damage: " + hardHitterDamage + ") = " + totalDamage + " from " + ch.GetLogString() + " Unarmed (" + ch.LastCommand + ") Skill: " +
                                Skills.GetSkillTitle(Globals.eSkillType.Unarmed, ch.BaseProfession, ch.GetSkillExperience(Globals.eSkillType.Unarmed), ch.gender), Utils.LogType.CriticalCombatDamageToCreature);
                        }
                        else
                        {
                            Utils.Log(target.GetLogString() + " took (Damage: " + damage + " + Damage Bonus: " + damageBonus + " + Temp Strength: " + ch.TempStrength + " + Strength Add: " + ch.strengthAdd + " + Critical Damage: " + criticalDamage + " + Hard Hitter Damage: " + hardHitterDamage + ") = " + totalDamage + " from " + ch.GetLogString() + " " + attackWeapon.GetLogString() + " (" + ch.LastCommand + ") Skill: " +
                                Skills.GetSkillTitle(attackWeapon.skillType, ch.BaseProfession, ch.GetSkillExperience(attackWeapon.skillType), ch.gender), Utils.LogType.CriticalCombatDamageToCreature);
                        }
                    }
                    else
                    {
                        if (attackWeapon == null)
                        {
                            Utils.Log(target.GetLogString() + " took (Damage: " + damage + " + Damage Bonus: " + damageBonus + " + Temp Strength: " + ch.TempStrength + " + Strength Add: " + ch.strengthAdd + " + Hard Hitter Damage: " + hardHitterDamage + ") = " + totalDamage + " from " + ch.GetLogString() + " Unarmed (" + ch.LastCommand + ") Skill: " +
                                Skills.GetSkillTitle(Globals.eSkillType.Unarmed, ch.BaseProfession, ch.GetSkillExperience(Globals.eSkillType.Unarmed), ch.gender), Utils.LogType.CombatDamageToCreature);
                        }
                        else
                        {
                            Utils.Log(target.GetLogString() + " took (Damage: " + damage + " + Damage Bonus: " + damageBonus + " + Temp Strength: " + ch.TempStrength + " + Strength Add: " + ch.strengthAdd + " + Hard Hitter Damage: " + hardHitterDamage + ") = " + totalDamage + " from " + ch.GetLogString() + " " + attackWeapon.GetLogString() + " (" + ch.LastCommand + ") Skill: " +
                                Skills.GetSkillTitle(attackWeapon.skillType, ch.BaseProfession, ch.GetSkillExperience(attackWeapon.skillType), ch.gender), Utils.LogType.CombatDamageToCreature);
                        }
                    }
                }
            }

            if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Shield_Bash))
                critical = true;

            if (attackWeapon != null)
            {
                if (attackWeapon.venom > 0)
                {
                    int savemod = Convert.ToInt32(attackWeapon.venom / 1.5) - target.PoisonResistance;

                    // Fails saving throw, or is being assassinated.
                    if (!Combat.DND_CheckSavingThrow(target, Combat.SavingThrow.ParalyzationPoisonDeath, savemod) || ch.CommandsProcessed.Contains(CommandTasker.CommandType.Assassinate))
                    {
                        int venomAmount = attackWeapon.venom;

                        if (target.PoisonProtection > 0)
                        {
                            int protection = target.PoisonProtection;
                            if (protection > 100) { protection = 100; }
                            double dPercent = 108 - protection;//need 8% protection to start reducing
                            dPercent = .01 * dPercent;
                            venomAmount = (int)(attackWeapon.venom * dPercent);
                        }

                        Effect.CreateCharacterEffect(Effect.EffectTypes.Venom, venomAmount, target, -1, ch);
                        target.WriteToDisplay("You have been poisoned by " + ch.GetNameForActionResult(true) + "!");
                        ch.WriteToDisplay(target.GetNameForActionResult() + " has been poisoned" + (attackWeapon == null ? "" : " by your " + attackWeapon.name) + ".");
                    }
                    else
                    {
                        ch.WriteToDisplay(target.GetNameForActionResult() + " resists your poison.");
                        target.WriteToDisplay("You resist " + ch.GetNameForActionResult(true) + "'s poison!");
                    }
                }

                // check for venom charges (wristbow gloves) 1/24/2017 Eb
                if (attackWeapon != null &&
                    attackWeapon.spell == (int)GameSpell.GameSpellID.Venom && attackWeapon.charges > 0)
                {
                    if ((attackWeapon.returning && attackWeapon.baseType == Globals.eItemBaseType.Bow) ||
                        attackWeapon.baseType != Globals.eItemBaseType.Bow)
                    {
                        attackWeapon.venom = (int)(attackWeapon.spellPower * Spells.VenomSpell.VenomDamageMultiplier);
                        // Venom is a skill 13 spell. The power of the spell, currently, is skill level * 1.5. So, 19.5 for thieves
                        // that first learn the spell. Make sure the spellPower of an auto nocking, envenomed bow stays at least 19.
                        if (attackWeapon.venom < 19) attackWeapon.venom = 19;
                    }

                    if (attackWeapon.charges < Item.NUM_FOR_UNLIMITED_CHARGES)
                        attackWeapon.charges--;
                }
                else
                    attackWeapon.venom = 0;
            }
            #endregion

            // Main call to deal the damage. All adjustments must be made before this method call.
            Combat.DoDamage(target, ch, totalDamage, critical);

            // Riposte is only checked if the attacker is lunging with a weapon. Animals, dragons, Characters using unarmed combat are not included...
            if(attackWeapon != null &&
                attackWeapon != ch.GetInventoryItem(Globals.eWearLocation.Feet) &&
                attackWeapon != ch.GetInventoryItem(Globals.eWearLocation.Hands))
                Combat.CheckRiposte(target, ch);
        }

        public static int DND_GetModifiedTHAC0(Character ch, object attackWeapon)
        {
            try
            {
                int modifier = 0;

                if (attackWeapon == null)
                {
                    #region Pure unarmed combat; no gauntlets or boots interfering with martial arts.
                    // bonus after red belt for martial artists
                    if (ch.BaseProfession == Character.ClassType.Martial_Artist && Skills.GetSkillLevel(ch.GetSkillExperience(Globals.eSkillType.Unarmed)) > 5)
                        modifier += Skills.GetSkillLevel(ch.GetSkillExperience(Globals.eSkillType.Unarmed)) - 5;

                    if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Kick))
                        modifier -= 2;
                    else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Jumpkick))
                        modifier -= 4;
                    #endregion
                }
                else
                {
                    // Using gauntlets or other unarmed skill type, martial artists get bonus after red belt.
                    if (attackWeapon is Item && (attackWeapon as Item).skillType == Globals.eSkillType.Unarmed && ch.BaseProfession == Character.ClassType.Martial_Artist &&
                        Skills.GetSkillLevel(ch.GetSkillExperience(Globals.eSkillType.Unarmed)) > 5)
                        modifier += Skills.GetSkillLevel(ch.GetSkillExperience(Globals.eSkillType.Unarmed)) - 5;

                    // More difficult to attack if wearing boots while kicking or jumpkicking.
                    if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Kick))
                        modifier -= 3;
                    else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Jumpkick))
                        modifier -= 5;

                    // Weapon bonus base is attack weapon combat adds.
                    if (attackWeapon is Item)
                        modifier += (attackWeapon as Item).combatAdds;
                    else if (attackWeapon is Spells.ISpellHandler) // if caster of spell has a staff or item that uses magic skill, combat adds grant bonus
                    {
                        #region Check for magic focus items if attack is a GameSpell.
                        int itemID = -1; // only allow one of same item to give bonus (for now)

                        if (ch.RightHand != null && ch.RightHand.skillType == Globals.eSkillType.Magic)
                        {
                            modifier += ch.RightHand.combatAdds;
                        }

                        if (ch.LeftHand != null && ch.LeftHand.skillType == Globals.eSkillType.Magic)
                        {
                            if (itemID == -1 || (itemID > -1 && itemID != ch.LeftHand.itemID))
                            {
                                modifier += ch.LeftHand.combatAdds;
                            }
                        }

                        foreach (Item item in ch.wearing)
                        {
                            if (item.skillType == Globals.eSkillType.Magic)
                            {
                                if (itemID == -1 || (itemID > -1 && itemID != ch.LeftHand.itemID))
                                {
                                    modifier += item.combatAdds;
                                }
                            }
                        }
                        #endregion
                    }

                    #region Bonus based on skill level for missile attacks while hidden.
                    if ((attackWeapon is Item) && (attackWeapon as Item).skillType == Globals.eSkillType.Bow ||
                                (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot) || ch.CommandsProcessed.Contains(CommandTasker.CommandType.Throw)) &&
                                ch.IsHidden)
                        modifier += (int)(Skills.GetSkillLevel(ch.GetSkillExperience((attackWeapon as Item).skillType)) / 3);
                    #endregion
                }

                if (attackWeapon is Spells.ISpellHandler)
                {
                    return DND_GetBaseTHAC0(ch.BaseProfession, Skills.GetSkillLevel(ch.magic)) - modifier + ch.THAC0Adjustment;
                }
                else return DND_GetBaseTHAC0(ch.BaseProfession, ch.GetWeaponSkillLevel(attackWeapon as Item)) - modifier + ch.THAC0Adjustment;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return 10;
            }
        }

        public static int DND_GetBaseTHAC0(Character.ClassType classType, int skillLevel) // returns THAC0 based on class and skill level
        {
            int thac0 = 20;
            int[] Fighter = new int[]
            { 19, 18, 17, 16, 15, 14, 13, 12, 11, 10,
                9, 8, 7, 6, 5, 4, 3, 2, 1, 0,
                -1, -2, -3, -4, -5, -6, -7, -8, -9, -10,
                -11, -12, -13, -14, -15, -16, -17 };
            int[] Thaumaturge = new int[] // and druids
            { 20, 19, 18, 18, 17, 16, 16, 15, 14, 14,
                13, 12, 12, 11, 10, 10, 9, 8, 8, 7,
                6, 6, 5, 4, 4, 3, 2, 2, 1, 0,
                0, -1, -2, -2, -3, -4, -4 };
            int[] MartialArtist = new int[] // and rangers
            { 19, 19, 18, 17, 16, 15, 14, 13, 12, 11,
                11, 10, 10, 9, 9, 8, 8, 7, 6, 5,
                5, 4, 3, 2, 2, 1, 0, 0, -1, -1,
                -2, -3, -3, -4, -5, -6, -6  };
            int[] Wizard = new int[] // and sorcerers
            { 20, 20, 19, 19, 19, 18, 18, 18, 17, 17,
                17, 16, 16, 16, 15, 15, 15, 14, 14, 14,
                13, 13, 13, 12, 12, 12, 11, 11, 11, 10,
                10, 10, 9, 9, 9, 8, 8 };
            int[] Thief = new int[]
            { 19, 19, 18, 17, 17, 16, 15, 15, 14, 13,
                13, 12, 11, 11, 10, 9, 9, 8, 7, 7,
                6, 5, 5, 4, 3, 3, 2, 1, 1, 0,
                -1, -1, -2, -3, -3, -4, -4 };
            int[] Knight = new int[] // and ravagers
            { 20, 19, 18, 17, 16, 15, 14, 13, 12, 11,
                10, 9, 8, 7, 6, 5, 4, 3, 2, 1,
                0, -1, -2, -3, -4, -5, -6, -7, -8, -9,
                -10, -11, -12, -13, -14, -15, -16 };
            try
            {
                switch (classType)
                {
                    case Character.ClassType.Fighter:
                        if (skillLevel > Fighter.Length || skillLevel < 0)
                            skillLevel = Fighter.Length - 1;
                        thac0 = Fighter[skillLevel];
                        break;
                    case Character.ClassType.Ravager:
                    case Character.ClassType.Knight:
                        if (skillLevel > Knight.Length || skillLevel < 0)
                            skillLevel = Knight.Length - 1;
                        thac0 = Knight[skillLevel];
                        break;
                    case Character.ClassType.Berserker:
                    case Character.ClassType.Ranger:
                    case Character.ClassType.Martial_Artist:
                        if (skillLevel > MartialArtist.Length || skillLevel < 0)
                            skillLevel = MartialArtist.Length - 1;
                        thac0 = MartialArtist[skillLevel];
                        break;
                    case Character.ClassType.Druid:
                    case Character.ClassType.Thaumaturge:
                        if (skillLevel > Thaumaturge.Length || skillLevel < 0)
                            skillLevel = Thaumaturge.Length - 1;
                        thac0 = Thaumaturge[skillLevel];
                        break;
                    case Character.ClassType.Thief:
                        if (skillLevel > Thief.Length || skillLevel < 0)
                            skillLevel = Thief.Length - 1;
                        thac0 = Thief[skillLevel];
                        break;
                    case Character.ClassType.Sorcerer:
                    case Character.ClassType.Wizard:
                        if (skillLevel > Wizard.Length || skillLevel < 0)
                            skillLevel = Wizard.Length - 1;
                        thac0 = Wizard[skillLevel];
                        break;
                    default:
                        if (skillLevel > Thaumaturge.Length || skillLevel < 0)
                            skillLevel = Thaumaturge.Length - 1;
                        thac0 = Thaumaturge[skillLevel];
                        break;
                }
            }
            catch(Exception e)
            {
                Utils.LogException(e);
                Utils.Log("Failed Combat.DND_GetBaseThac0 for ClassType." + classType.ToString() + " at skillLevel " + skillLevel + ".", Utils.LogType.ExceptionDetail);
                return MartialArtist[MartialArtist.Length - (MartialArtist.Length / 2)];
            }

            return thac0;
        }

        public enum SavingThrow
        {
            ParalyzationPoisonDeath,
            PetrificationPolymorph,
            RodStaffWand,
            BreathWeapon,
            Spell
        }

        /// <summary>
        /// Make a saving throw check. The GREATER the modifier is the more difficult the saving throw is to make.
        /// Protection is a POSITIVE, yet it should be negated when calling this method. Some saving throws use Rules.GetGenericStatModifer (eg: dexterity helps get out of webs).
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="savingThrow"></param>
        /// <param name="modifier">Outside modification to the saving throw roll.</param>
        /// <returns>False if the saving throw failed.</returns>
        public static bool DND_CheckSavingThrow(Character ch, SavingThrow savingThrow, int modifier)
        {
            // Types:
            // PPD (Paralyzation, Poison, Death Magic)
            // PP (Petrification or Polymorph)
            // RSW (Rod, Staff, Wand)
            // S (Spells) (Concussion)
            // BW (Breath Weapon)

            #region TODO: further work on item saving throws
            if (ch == null)
            {
                switch (savingThrow)
                {
                    case SavingThrow.BreathWeapon:
                        modifier -= 2;
                        break;
                    case SavingThrow.ParalyzationPoisonDeath:
                        break;
                    case SavingThrow.PetrificationPolymorph:
                        modifier += 2;
                        break;
                    case SavingThrow.RodStaffWand:
                        modifier += 1;
                        break;
                    case SavingThrow.Spell:
                        modifier -= 1;
                        break;
                }
                return Rules.RollD(1, 20) + modifier >= 10;
            }
            #endregion

            int rollNeeded = Commands.ShowSavesCommand.GetSavingThrow(ch, savingThrow) + modifier;

            int roll = Rules.RollD(1, 20);
#if DEBUG
            ch.SendToAllDEVInSight(ch.GetNameForActionResult() + " " + savingThrow.ToString() + " result: rollNeeded: " + rollNeeded + " -- roll: " + roll);
#endif
            return roll >= rollNeeded;
            //return Rules.RollD(1, 20) >= rollNeeded;
        }

        public static int CalculateUnarmedDamage(Character ch)
        {
            if (ch.BaseProfession == Character.ClassType.Martial_Artist) // attacker is a martial artist
            {
                if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Jumpkick))
                    return Rules.RollD(ch.GetWeaponSkillLevel(null), 5);
                else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Kick))
                    return Rules.RollD(ch.GetWeaponSkillLevel(null), 4);
                else // all other martial artist attacks -- includes talents not previously placed in conditional statements
                    return Rules.RollD(ch.GetWeaponSkillLevel(null), 3);
            }
            else if (!ch.IsPC) // attacker is not a player
            {
                NPC npc = (NPC)ch;

                if (npc.lairCritter)
                {
                    if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Jumpkick))
                        return Rules.RollD(ch.GetWeaponSkillLevel(null), 7);
                    else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Kick))
                        return Rules.RollD(ch.GetWeaponSkillLevel(null), 6);
                    else
                        return Rules.RollD(ch.GetWeaponSkillLevel(null), 5);
                }
                else
                {
                    if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Jumpkick))
                        return Rules.RollD(ch.GetWeaponSkillLevel(null), 3);
                    else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Kick))
                        return Rules.RollD(ch.GetWeaponSkillLevel(null), 2);
                    else
                        return (int)(Rules.RollD(ch.GetWeaponSkillLevel(null), 2) / 1.5);
                }
            }
            else // attacker is a non martial artist player using unarmed skill
            {
                if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Jumpkick))
                    return Rules.RollD(ch.GetWeaponSkillLevel(null), 3);
                else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Kick))
                    return Rules.RollD(ch.GetWeaponSkillLevel(null), 2);
                else
                    return (int)(Rules.RollD(ch.GetWeaponSkillLevel(null), 2) / 1.5);
            }
        }

        public static int CalculateWeaponDamage(Character ch, Item weapon) // weapon min and max damage random, plus strength add
        {
            int minDamage = weapon.minDamage;
            int maxDamage = weapon.maxDamage;

            if (maxDamage <= 0) { maxDamage = Convert.ToInt32(Rules.GetFullAbilityStat(ch, Globals.eAbilityStat.Strength) / 4); }
            if (minDamage <= 0) { minDamage = 1; }

            if (minDamage > maxDamage)
            {
                minDamage = 1;
                maxDamage = Convert.ToInt32(Rules.GetFullAbilityStat(ch, Globals.eAbilityStat.Strength) / 4);
            }

            // Flails (including whips) wielded as two handed weapon (still using flail skill) and at Proficient skill level will do more damage.
            if (weapon.skillType == Globals.eSkillType.Flail && ch.LeftHand == null && Skills.GetSkillLevel(ch.flail) >= 9)
            {
                minDamage = minDamage * 2;
                maxDamage = maxDamage * 2;
            }

            // Hunter's Mark effect raises min and max damage for all weapons. Moreso for thrown and shot weapons.
            if(ch.EffectsList.ContainsKey(Effect.EffectTypes.Hunter__s_Mark))
            {
                int bonus = ch.EffectsList[Effect.EffectTypes.Hunter__s_Mark].Power;

                if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot) || ch.CommandsProcessed.Contains(CommandTasker.CommandType.Throw))
                    bonus = bonus * 2; // or should this be 1.5? 9/29/2019

                minDamage += bonus;
                maxDamage += bonus;
            }

            if (weapon.skillType == Globals.eSkillType.Unarmed)
                return CalculateUnarmedDamage(ch) + Rules.Dice.Next(minDamage, maxDamage + 1) + weapon.combatAdds;

            return Rules.Dice.Next(minDamage, maxDamage + 1) + weapon.combatAdds;
        }

        /// <summary>
        /// All non spell damage is routed through this method.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="damager"></param>
        /// <param name="damage"></param>
        /// <param name="critical"></param>
        public static void DoDamage(Character target, Character damager, int damage, bool critical)
        {
            if(target.PetOwner != null && target.PetOwner == damager)
            {
                if (target.EffectsList.ContainsKey(Effect.EffectTypes.Charm_Animal))
                    target.EffectsList[Effect.EffectTypes.Charm_Animal].StopCharacterEffect();

                if (target.EffectsList.ContainsKey(Effect.EffectTypes.Command_Undead))
                    target.EffectsList[Effect.EffectTypes.Command_Undead].StopCharacterEffect();

                if (target is NPC && target.special.Contains("figurine"))
                {
                    Rules.DespawnFigurine(target as NPC);
                    return;
                }
            }
            
            #region Automatic death to targets using Peek GameSpell or Wizard Eyes.
            if (target.IsPeeking || target.IsWizardEye)
            {
                Rules.DoDeath(target, null);
                return;
            }
            #endregion

            // Hide always breaks when taking damage.
            if (target.IsHidden)
                target.IsHidden = false;

            // damage shields
            if (!target.IsDead && target.EffectsList.ContainsKey(Effect.EffectTypes.Flame_Shield))
            {
                if (DoSpellDamage(target, damager, null, target.EffectsList[Effect.EffectTypes.Flame_Shield].Power, Effect.EffectTypes.Flame_Shield.ToString().ToLower()) == 1)
                {
                    Rules.GiveAEKillExp(target, damager);
                    Rules.DoDeath(damager, target);
                    return;
                }
            }

            if (damage > 0)
            {
                // if spell is prepped, and more than 50% of remaining hits are taken in damage, clear the preppedspell
                if (target.preppedSpell != null && damage >= target.Hits * .50 && damage < target.Hits)
                {
                    target.preppedSpell = null;
                    target.WriteToDisplay("Your spell has been lost.");
                    target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                }

                // if target has a memorized spell chant and more than 75% of remaining hits are taken
                if (!string.IsNullOrEmpty(target.MemorizedSpellChant) && damage >= target.Hits * .75)
                {
                    target.MemorizedSpellChant = "";
                    target.WriteToDisplay("You have forgotten your memorized spell chant!");
                    target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.MemorizedChantLoss));
                }

                target.Hits -= damage;
                target.DamageRound = DragonsSpineMain.GameRound;
                target.IsMeditating = false; // break meditation
                target.IsResting = false; // break resting
            }

            if (target != null && target.Stunned <= 0 && critical && !target.immuneStun)
            {
                int saveMod = -(target.ZonkResistance);

                // damager used a bash
                if (damager.CommandsProcessed.Contains(CommandTasker.CommandType.Shield_Bash))
                {
                    saveMod += Math.Max(target.Level, Skills.GetSkillLevel(damager.GetSkillExperience(Globals.eSkillType.Bash))) -
                        Math.Min(target.Level, Skills.GetSkillLevel(damager.GetSkillExperience(Globals.eSkillType.Bash)));
                }
                else if (damager.CommandsProcessed.Contains(CommandTasker.CommandType.RoundhouseKick))
                {
                    saveMod += Math.Max(target.Level, Skills.GetSkillLevel(damager.GetSkillExperience(Globals.eSkillType.Unarmed))) -
                        Math.Min(target.Level, Skills.GetSkillLevel(damager.GetSkillExperience(Globals.eSkillType.Unarmed)));
                }
                else if (damager.CommandsProcessed.Contains(CommandTasker.CommandType.Leg_Sweep))
                {
                    saveMod += Math.Max(target.Level, Skills.GetSkillLevel(damager.GetSkillExperience(Globals.eSkillType.Unarmed))) -
                        Math.Min(target.Level, Skills.GetSkillLevel(damager.GetSkillExperience(Globals.eSkillType.Unarmed)));
                }

                if (!DND_CheckSavingThrow(target, SavingThrow.PetrificationPolymorph, saveMod))
                {
                    // stun for 1 round if bash save failed, 1 - 2 for others
                    if (damager.CommandsProcessed.Contains(CommandTasker.CommandType.Shield_Bash))
                        target.Stunned = 1;
                    else target.Stunned += (short)Rules.RollD(1, 2);

                    if (target.Hits > 0)
                    {
                        target.SendToAllInSight(target.GetNameForActionResult() + " is stunned!");
                        target.WriteToDisplay("You have been stunned by the blow!");
                    }

                    #region If the stunned creature is leader of a group, choose a new leader.
                    if (!target.IsPC && target.Group != null && (target is NPC) && target.Group.GroupLeaderID == target.UniqueID && target.Group.GroupNPCList.Count > 1)
                    {
                        foreach (NPC npc in target.Group.GroupNPCList)
                        {
                            if (npc != target && npc.Stunned <= 0)
                            {
                                target.Group.GroupLeaderID = npc.UniqueID;
                                break;
                            }
                        }
                    }
                    #endregion
                }
            }
            else if (target.Stunned <= 0 && critical && target.immuneStun)
            {
                if (damager.CommandsProcessed.Contains(CommandTasker.CommandType.Shield_Bash))
                    damager.WriteToDisplay(target.GetNameForActionResult() + " is not affected by your bash stun.");
                else damager.WriteToDisplay(target.GetNameForActionResult() + " is not affected by your physical stun.");
            }

            if (target.Hits <= 0)
            {
                #region Display pet damage if enabled
                if (damager.PetOwner != null && damager.PetOwner.IsPC && (damager.PetOwner as PC).DisplayPetDamage)
                {
                    damager.PetOwner.WriteToDisplay("Your pet " + damager.Name + " does " + damage + " damage to " + target.Name + ".");
                }

                if (target.PetOwner != null && target.PetOwner.IsPC && (target.PetOwner as PC).DisplayPetDamage)
                {
                    target.PetOwner.WriteToDisplay("Your pet " + target.Name + " took " + damage + " from " + damager.Name + ".");
                }
                #endregion

                if (damager is PC && !damager.IsImmortal && !damager.CurrentCell.IsBoxingRing) // if damager is a PC
                {
                    Rules.GiveKillExp(damager, target); // give kill experience
                }
                else if (damager.PetOwner != null && damager.PetOwner is PC) // if the damager is a pet with a PC owner
                {
                    Rules.GiveKillExp(damager, target);
                }

                Rules.DoDeath(target, damager);

                if (target.IsPC)
                    target.updateHits = true;
            }
        }

        /// <summary>
        /// Returns 1 if the target is skilled by spell damage. 0 otherwise.
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="target"></param>
        /// <param name="item"></param>
        /// <param name="damage"></param>
        /// <param name="spellType"></param>
        /// <returns></returns>
        public static int DoSpellDamage(Character caster, Character target, Item item, int damage, string spellType)
        {
            // Immune to thaumaturgy.
            if (target != null && EntityLists.IMMUNE_DRUIDRY.Contains(target.entity) && caster != null &&
                caster.BaseProfession == Character.ClassType.Druid)
            {
                caster.WriteToDisplay(target.GetNameForActionResult() + " is immune to " + caster.BaseProfession.ToString().ToLower() + " spells.");
                return 0;
            }

            // Immune to thaumaturgy.
            if (target != null && EntityLists.IMMUNE_SORCERY.Contains(target.entity) && caster != null &&
                caster.BaseProfession == Character.ClassType.Sorcerer)
            {
                caster.WriteToDisplay(target.GetNameForActionResult() + " is immune to " + caster.BaseProfession.ToString().ToLower() + " spells.");
                return 0;
            }

            // Immune to thaumaturgy.
            if (target != null && EntityLists.IMMUNE_THAUMATURGY.Contains(target.entity) && caster != null &&
                caster.BaseProfession == Character.ClassType.Thaumaturge)
            {
                caster.WriteToDisplay(target.GetNameForActionResult() + " is immune to " + caster.BaseProfession.ToString().ToLower() + " spells.");
                return 0;
            }

            // Immune to wizardry.
            if (target != null && EntityLists.IMMUNE_WIZARDRY.Contains(target.entity) && caster != null &&
                caster.BaseProfession == Character.ClassType.Wizard)
            {
                caster.WriteToDisplay(target.GetNameForActionResult() + " is immune to " + caster.BaseProfession.ToString().ToLower() + " spells.");
                return 0;
            }

            // Nothing happens from illusions.
            if (!string.IsNullOrEmpty(spellType) && spellType.ToLower() == "illusion")
                return 0;

            // Damage shields do no damage when it is a range attack
            if(spellType.ToLower().EndsWith("shield") &&
                (target.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot) || target.CommandsProcessed.Contains(CommandTasker.CommandType.Throw)))
            {
                return 0;
            }

            #region Automatic death to targets peeking, wizardeyes, and images...
            if (target.IsPeeking || target.IsWizardEye)// || target.IsImage)
            {
                Rules.DoDeath(target, null);
                return 0; // no experience or skill gain
            }
            #endregion

            #region Spell damage to pet by owner. Break charm or command undead. Despawn figurine and return 0.
            if (target.PetOwner != null && target.PetOwner == caster)
            {
                if (target.EffectsList.ContainsKey(Effect.EffectTypes.Charm_Animal))
                    target.EffectsList[Effect.EffectTypes.Charm_Animal].StopCharacterEffect();

                if (target.EffectsList.ContainsKey(Effect.EffectTypes.Command_Undead))
                    target.EffectsList[Effect.EffectTypes.Command_Undead].StopCharacterEffect();

                if (target is NPC && target.special.Contains("figurine"))
                {
                    Rules.DespawnFigurine(target as NPC);
                    return 0;
                }
            } 
            #endregion

            if ((target != null && target.IsDead) || (caster != null && caster.IsDead)) { return 0; }
            //if (target != null && target.Alignment == Globals.eAlignment.Amoral) { return 0; }
            //if (target != null && target.merchantType != Merchant.MerchantType.None) { return 0; }
            //if (target != null && target.trainerType != Merchant.TrainerType.None) { return 0; }
            //if (target != null && !target.IsPC && !(target as NPC).IsMobile) { return 0; }

            int protection = 0;
            int lvlDiff = 0;
            int conDiff = 0;
            int strDiff = 0;
            bool stunChance = false;
            int totalDamage = 0;
            string damageType = spellType;

            SavingThrow savingThrow = SavingThrow.Spell;

            if (item != null)
                savingThrow = SavingThrow.RodStaffWand;

            if (caster != null && caster.IsPC || target != null && target.IsPC)
            {
                switch (spellType.ToLower())
                {
                    case "light": // light spell will not set flag
                        break;
                    default:
                        Combat.DoFlag(caster, target);
                        break;
                }
            }

            int savingThrowModifier = 0;

            // Protection from Undead saving throw modifier versus undead.
            if (caster != null && caster.IsUndead && target != null && target.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Undead))
                savingThrowModifier -= target.EffectsList[Effect.EffectTypes.Protection_from_Undead].Power;

            if (caster != null && EntityLists.IsHellspawn(target) && target != null && target.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Hellspawn))
                savingThrowModifier -= target.EffectsList[Effect.EffectTypes.Protection_from_Hellspawn].Power;

            try
            {
                #region Saving Throws and Damage Adjustments
                switch (spellType.ToLower())
                {
                    #region Acid & Acid Splash
                    case "acid":
                    case "acid orb":
                        if (target.immuneAcid || EntityLists.IMMUNE_ACID.Contains(target.entity))
                        {
                            if (caster != null && target != caster)
                            {
                                caster.WriteToDisplay(target.GetNameForActionResult() + " is immune to acid damage.");
                            }
                            return 0;
                        }
                        else if (DND_CheckSavingThrow(target, savingThrow, savingThrowModifier - target.AcidResistance) || target.HasEffect(Effect.EffectTypes.Stoneskin))
                        { damage = damage / 2; }
                        if (caster != null)
                        {
                            protection = target.AcidProtection - Skills.GetSkillLevel(caster.magic);
                        }
                        else
                        {
                            protection = target.AcidProtection;
                        }
                        damageType = "";
                        // acid continues to do damage as an acid splash (except vs. hard shelled/scaled entities -- effect is created nonetheless)
                        Effect.CreateCharacterEffect(Effect.EffectTypes.Acid, damage / 2, target, Rules.RollD(1, 4), caster);                        
                        break;
                    case "acid splash":
                        if (target.immuneAcid ||
                            EntityLists.IMMUNE_ACID.Contains(target.entity) ||
                            EntityLists.ARTHROPOD.Contains(target.entity) ||
                            EntityLists.WYRMKIN.Contains(target.entity))
                        {
                            if (caster != null && target != caster)
                            {
                                caster.WriteToDisplay(target.GetNameForActionResult() + " is immune to acid damage.");
                            }
                            return 0;
                        }
                        else if (target.HasEffect(Effect.EffectTypes.Stoneskin)) damage = 0; // no acid splash damage on stoneskin
                        // this is a result of an Acid effect
                        break;
                    #endregion
                    case "banish":
                        if (target != null)
                        {
                            if (target is NPC && !(target as NPC).IsSummoned)
                                damage = 0;
                        }
                        break;
                    #region Curse
                    case "curse":
                        #region Immune to Curse
                        if (target.immuneCurse || EntityLists.IMMUNE_CURSE.Contains(target.entity))
                        {
                            if (caster != null)
                                caster.WriteToDisplay(target.GetNameForActionResult() + " is immune to curse damage.");

                            damage = 0;
                        }
                        damageType = "a curse spell";
                        #endregion
                        // no saving throw against curse
                        break;
                    #endregion
                    #region Death
                    case "death":
                        #region Immune to Death
                        if (target.immuneDeath || EntityLists.IMMUNE_DEATH.Contains(target.entity))
                        {
                            if (caster != null && target != caster)
                            {
                                caster.WriteToDisplay(target.GetNameForActionResult() + " is immune to death magic.");
                            }
                            damage = 0;
                        }
                        #endregion
                        else
                        {
                            // lower saving throws for non player targets dependent if target is thaum and based on caster magic skill
                            if (caster != null && caster.IsPC && target != null && target.BaseProfession != caster.BaseProfession)
                                savingThrowModifier -= Skills.GetSkillLevel(caster.magic) - 7; // 7 is skill level death spell is learned
                            savingThrowModifier -= target.DeathResistance;
                            if (DND_CheckSavingThrow(target, savingThrow, savingThrowModifier)) { damage = (int)(damage / 1.5); }
                        }
                        protection = target.DeathProtection;
                        damageType = "a death spell";
                        break;
                    #endregion
                    #region Dismiss Undead
                    case "dismiss undead":
                        if (DND_CheckSavingThrow(target, savingThrow, savingThrowModifier)) { damage = (int)(damage / 1.5); }
                        break;
                    #endregion
                    #region Lifeleech
                    case "lifeleech":
                        if (DND_CheckSavingThrow(target, savingThrow, savingThrowModifier)) { damage = (int)(damage / 2); }
                        break;
                    #endregion
                    #region Stun
                    case "stun":
                        // set stunresist based off of the total value
                        int stunres = (int)(target.StunResistance * .33);
                        #region Immune to Stun
                        if (target.immuneStun)
                        {
                            if (caster != null && target != caster)
                            {
                                caster.WriteToDisplay(target.GetNameForActionResult() + " is immune to stun magic.");
                            }
                            return 0;
                        }
                        #endregion
                        else if (DND_CheckSavingThrow(target, savingThrow, savingThrowModifier - (stunres)))
                        {
                            return 0;
                        }
                        return 1;
                    #endregion
                    #region Fear
                    case "fear":
                        // damage is used as a saving throw modifier
                        #region Immune to Fear
                        if (target.immuneFear)
                        {
                            if (caster != null && target != caster)
                            {
                                caster.WriteToDisplay(target.GetNameForActionResult() + " is immune to fear.");
                            }
                            return 0;
                        }
                        #endregion
                        else if (DND_CheckSavingThrow(target, savingThrow, savingThrowModifier - target.FearResistance)) { return 0; }
                        return 1;
                    #endregion
                    #region Dragon Fear
                    case "dragon fear":
                        if (target.Level > caster.Level)
                        {
                            return 0;
                        } // dragon fear won't work on targets higher level than the dragon - they're heros.
                        // otherwise there's a good chance it will work
                        lvlDiff = caster.Level - target.Level;
                        conDiff = (caster.Constitution + caster.TempConstitution) - (target.Constitution + target.TempConstitution);
                        if (Rules.Dice.Next(3, caster.Constitution + 1) + lvlDiff + conDiff > target.Constitution + target.TempConstitution)
                        {
                            return 1;
                        }
                        return 0;
                    #endregion
                    #region Giant Stomp
                    case "giant stomp":
                        if (target.Level > caster.Level) { return 0; } // giant stomp won't work in this situation
                        // otherwise there's a good chance it will work
                        lvlDiff = caster.Level - target.Level;
                        strDiff = Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Strength) - Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Strength);
                        if (Rules.Dice.Next(3, Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Strength) + 1) + lvlDiff + strDiff > Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Strength)) { return 1; }
                        return 0;
                    #endregion
                    #region Turn Undead
                    case "turnundead":
                        #region Turn Undead
                        if (target.IsUndead)
                        {
                            //chance of undead being turned based on target level and caster magic level
                            if (target.Level >= Skills.GetSkillLevel(caster.magic))
                            {
                                switch (target.Level - Skills.GetSkillLevel(caster.magic))
                                {
                                    case 0:
                                        if (Rules.RollD(1, 100) >= 90)
                                        { damage = (int)(damage / 2); }
                                        else { damage = target.Hits; }
                                        break;
                                    case 1:
                                        if (Rules.RollD(1, 100) >= 80)
                                        { damage = (int)(damage / 2); }
                                        else { damage = target.Hits; }
                                        break;
                                    case 2:
                                        if (Rules.RollD(1, 100) >= 70)
                                        { damage = (int)(damage / 2); }
                                        else { damage = target.Hits; }
                                        break;
                                    case 3:
                                        if (Rules.RollD(1, 100) >= 60)
                                        { damage = (int)(damage / 2); }
                                        else { damage = target.Hits; }
                                        break;
                                    case 4:
                                        if (Rules.RollD(1, 100) >= 50)
                                        { damage = (int)(damage / 2); }
                                        else { damage = target.Hits; }
                                        break;
                                    default:
                                        damage = 0;
                                        if (target.race != "")
                                        { caster.WriteToDisplay(target.Name + " is unaffected by your prayers."); }
                                        else { caster.WriteToDisplay("The " + target.Name + " is unaffected by your prayers."); }
                                        break;
                                }
                            }
                            else
                            {
                                damage = target.Hits;
                            }
                        }
                        else
                        {
                            damage = 0;
                        }
                        break;
                    #endregion
                    #endregion
                    #region Light
                    case "light":
                        int lightSpellMultiplier = 0;
                        if (target.IsUndead || target.HasEffect(Effect.EffectTypes.Umbral_Form))
                        {
                            switch (caster.BaseProfession)
                            {
                                case Character.ClassType.Thaumaturge:
                                    lightSpellMultiplier = Rules.RollD(2, 6);
                                    break;
                                case Character.ClassType.Knight: // undead knight
                                    lightSpellMultiplier = Rules.RollD(2, 4);
                                    break;
                                default:
                                    lightSpellMultiplier = Rules.RollD(2, 3);
                                    break;
                            }
                            damage = damage * lightSpellMultiplier;
                        }
                        else
                        {
                            damage = 0;
                        }
                        break;
                    #endregion
                    #region Lightning or Lightning Storm
                    case "lightning storm":
                    case "lightning":
                    case "lightning lance":
                    case "lightninglance":
                        #region Immune to Lightning
                        if (target.immuneLightning || EntityLists.IMMUNE_LIGHTNING.Contains(target.entity))
                        {
                            if (caster != null && target != caster)
                            {
                                caster.WriteToDisplay(target.GetNameForActionResult() + " is immune to electric damage.");
                            }
                            damage = 0;
                        }
                        #endregion
                        else if (DND_CheckSavingThrow(target, savingThrow, savingThrowModifier - target.LightningResistance)) { damage = damage / 2; }
                        else { stunChance = true; }
                        if (caster != null)
                        {
                            protection = target.LightningProtection - Skills.GetSkillLevel(caster.magic);
                        }
                        else
                        {
                            protection = target.LightningProtection;
                        }
                        damageType = "a lightning bolt";
                        break;
                    #endregion
                    #region Locust Swarm
                    case "locust swarm":
                        if ((target is NPC && (target as NPC).IsSpectral) || EntityLists.INCORPOREAL.Contains(target.entity) || target.HasEffect(Effect.EffectTypes.Umbral_Form))
                            damage = 0;
                        if (target == caster) damage = 0; // locusts avoid the one who summoned them.
                        break; 
                    #endregion
                    #region All fire based damage
                    case "flameshield":
                    case "flame_shield":
                    case "firebolt":
                    case "fire":
                    case "fireball":
                    case "firestorm":
                    case "fire storm":
                    case "firewall":
                        #region Immune to Fire
                        if (target.immuneFire || EntityLists.IMMUNE_FIRE.Contains(target.entity))
                        {
                            if (caster != null && target != caster)
                            {
                                caster.WriteToDisplay(target.GetNameForActionResult() + " is immune to fire damage.");
                            }
                            damage = 0;
                        }
                        #endregion
                        else if (DND_CheckSavingThrow(target, savingThrow, savingThrowModifier - target.FireResistance)) { damage = damage / 2; }
                        if (caster != null)
                            protection = target.FireProtection - Skills.GetSkillLevel(caster.magic);
                        else protection = target.FireProtection;
                        if (EntityLists.SUSCEPTIBLE_FIRE.Contains(target.entity) && damage > 0)
                            damage += damage / 2;
                        damageType = "";
                        break;
                    #endregion
                    #region Ornic Flame - hurts only non evil
                    case "ornic flame":
                        if (target.Alignment.ToString().ToLower().Contains("evil"))
                        {
                            damage = 0;
                        }
                        else
                        {
                            if (DND_CheckSavingThrow(target, savingThrow, savingThrowModifier - target.FireResistance)) { damage = damage / 2; }
                            protection = target.FireProtection;
                        }
                        break; 
                    #endregion
                    #region Dragon's Breath Fire
                    case "dragon's breath fire":
                        //damage = Rules.RollD(Skills.GetSkillLevel(caster.magic), 14); // Breathweapon more powerful
                        if (target == caster || target.species == Globals.eSpecies.FireDragon)
                        {
                            damage = 0;
                        } // Fire dragons are the only species immune to dragon's breath fire
                        // protection from dragon's breath is lessened - it's a more powerful element
                        else if (DND_CheckSavingThrow(target, SavingThrow.BreathWeapon, savingThrowModifier))
                        {
                            damage = damage / 2;
                        }
                        protection = target.FireProtection - Skills.GetSkillLevel(caster.magic);
                        if (EntityLists.SUSCEPTIBLE_FIRE.Contains(target.entity) && damage > 0)
                            damage += damage / 2;
                        break;
                    #endregion
                    #region Dragon's Breath Ice
                    case "dragon's breath ice":
                        //damage = Rules.RollD(Skills.GetSkillLevel(caster.magic), 16); // Breathweapon more powerful
                        if (target == caster || target.species == Globals.eSpecies.IceDragon)
                        { damage = 0; }
                        else if (DND_CheckSavingThrow(target, SavingThrow.BreathWeapon, savingThrowModifier))
                        {
                            damage = damage / 2;
                        }
                        protection = target.ColdProtection - Skills.GetSkillLevel(caster.magic);
                        if (EntityLists.SUSCEPTIBLE_ICE.Contains(target.entity) && damage > 0)
                            damage += damage / 2;
                        break;
                    #endregion
                    #region Ice / Iceshard / Icespear / Blizzard
                    case "blizzard":
                    case "iceshard":
                    case "icespear":
                    case "ice":
                        #region Immune to Cold
                        if (target.immuneCold || EntityLists.IMMUNE_COLD.Contains(target.entity))
                        {
                            if (caster != null && target != caster)
                            {
                                caster.WriteToDisplay(target.GetNameForActionResult() + " is immune to cold damage.");
                            }
                            damage = 0;
                        }
                        #endregion
                        else if (DND_CheckSavingThrow(target, savingThrow, savingThrowModifier - target.ColdResistance)) { damage = damage / 2; }
                        if (caster != null)
                            protection = target.ColdProtection - Skills.GetSkillLevel(caster.magic);
                        else
                            protection = target.ColdProtection;
                        if (EntityLists.SUSCEPTIBLE_ICE.Contains(target.entity) && damage > 0)
                            damage += damage / 2;
                        break;
                    #endregion
                    #region Concussion
                    case "concussion":
                        if (DND_CheckSavingThrow(target, SavingThrow.Spell, savingThrowModifier - target.ZonkResistance))
                        {
                            stunChance = false;
                        }
                        else
                        {
                            stunChance = true;
                        }

                        if (EntityLists.IsGiantKin(target))
                        {
                            damage = damage / 2;
                            stunChance = false;
                        }
                        else if (target.entity == EntityLists.Entity.Thisson || EntityLists.ARTHROPOD.Contains(target.entity))
                        {
                            damage = damage / 3;
                            stunChance = false;
                        }
                        break;
                    #endregion
                    #region Illusion
                    case "illusion":
                        if (!Rules.CheckPerception(target))
                        {
                            break;
                        }
                        return 0;
                    #endregion
                    #region Disintegrate
                    case "disintegrate":
                        if (DND_CheckSavingThrow(target, SavingThrow.PetrificationPolymorph, savingThrowModifier))
                        {
                            damage = damage / 2;
                        }
                        break;
                    #endregion
                    #region Web
                    case "web":
                        return 0;
                    #endregion
                    #region Darkness
                    case "darkness":
                        return 0;
                    #endregion
                    #region Whirlwind
                    case "whirlwind":
                        if (target.species == Globals.eSpecies.WindDragon || target.entity == EntityLists.Entity.Gold_Dragon)
                            damage = 0;
                        else
                            stunChance = true;
                        break;
                    #endregion
                    #region Poison and Poison Cloud
                    case "poison cloud":
                        #region Immune to Poison
                        if (target.immunePoison || EntityLists.IMMUNE_POISON.Contains(target.entity))
                        {
                            if (caster != null && target != caster)
                            {
                                caster.WriteToDisplay(target.GetNameForActionResult() + " is immune to poison.");
                            }
                            return 0;
                        }
                        if (DND_CheckSavingThrow(target, SavingThrow.ParalyzationPoisonDeath, savingThrowModifier))
                            damage = damage / 2;
                        damageType = "a poison cloud";
                        Effect.CreateCharacterEffect(Effect.EffectTypes.Poison, damage / 2, target, Rules.RollD(1, 4) + 1, caster);
                        #endregion
                        break;
                    case "poison"://mlt poison work
                        #region Immune to Poison
                        if (target.immunePoison)
                        {
                            if (caster != null)
                            {
                                caster.WriteToDisplay(target.GetNameForActionResult() + " is immune to poison.");
                            }
                            return 0;
                        }
                        #endregion
                        else if (DND_CheckSavingThrow(target, SavingThrow.ParalyzationPoisonDeath, savingThrowModifier - target.PoisonResistance))
                        {
                            caster.WriteToDisplay(target.GetNameForActionResult() + " resisted your poison.");
                            return 0;
                        }
                        else
                        {
                            if (caster != null)
                                protection = target.PoisonProtection - Skills.GetSkillLevel(caster.magic);
                            else
                                protection = target.PoisonProtection;
                            damageType = "poison";
                        }
                        break;
                    #endregion
                    case "tempest":
                        if (caster != null && target.Alignment == caster.Alignment)
                        {
                            damage = 0;
                        }
                        break;
                    default:
                        break;
                }
                #endregion

                // At this point the total damage is tallied. Next, damage will be reduced for various reasons (protection/absorption).
                totalDamage = damage;

                // Venom and poison comes through here, as should all damage over time from non-melee.
                if (caster != null && caster.DPSLoggingEnabled)
                    GameSystems.DPSCalculator.AddSpellDPSValue(caster, DragonsSpineMain.GameRound, spellType, totalDamage);

                #region Evaluate protection and spell damage.
                // TODO: fix protection - works as a damage soak
                //Protections based on a percentage of the total damage.

                if (caster != null && caster.IsUndead && target != null && target.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Undead))
                    protection += target.EffectsList[Effect.EffectTypes.Protection_from_Undead].Power * 10;

                if (caster != null && EntityLists.IsHellspawn(caster) && target != null && target.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Hellspawn))
                    protection += target.EffectsList[Effect.EffectTypes.Protection_from_Hellspawn].Power * 10;

                if (protection > 0)
                {
                    //if (protection > 100) { protection = 100; }

                    //double dPercent = 100 - protection;

                    //dPercent = .01 * dPercent;

                    //totalDamage = (int)(totalDamage * dPercent);

                    totalDamage -= protection;

                    if (totalDamage < 0)
                        totalDamage = 0;
                }
                else if (protection < 0) // take into account negative protection (in the form of debuffs and creature limitations?)
                {
                    totalDamage += Math.Abs(protection);
                }
                #endregion

                #region mlt poison work
                if (damageType == "poison" || damageType == "poison cloud")// mlt poison work
                {
                    if (target.Poisoned < totalDamage)
                    {
                        Effect.CreateCharacterEffect(Effect.EffectTypes.Poison, totalDamage, target, 1, caster);
                        caster.WriteToDisplay(target.GetNameForActionResult()+ " has been poisoned");
                        target.WriteToDisplay("You have been poisoned!");
                    }
                    else
                    {
                        caster.WriteToDisplay(target.GetNameForActionResult() + " is already suffering from a stronger poison.");
                        target.WriteToDisplay("You are already suffering from a stronger poison.");
                    }
                    totalDamage = 0;
                }
                #endregion

                #region Handling Critical Spell Damage
                // Caster must be a spell warming profession (has magic skill) and damage shields do not have critical damage.
                if (caster != null && target != null && totalDamage > 0 && !spellType.ToLower().EndsWith("shield"))
                {
                    var criticalChance = Rules.RollD(1, 100);
                    
                    if (caster.IsWisdomCaster)
                        criticalChance += Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Wisdom);
                    else
                        criticalChance += Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Intelligence);

                    int addedMultiplier = 0;

                    // Improved Critical Effect.
                    if (caster.HasEffect(Effect.EffectTypes.Ferocity, out Effect ferocityAmplifier))
                    {
                        criticalChance += ferocityAmplifier.Power;
                        addedMultiplier += 1;
                    }

                    if (criticalChance >= 100 - Rules.RollD(1, 10))
                    {
                        caster.WriteToDisplay("Your " + spellType + " does critical damage!");
                        target.WriteToDisplay("You have suffered critical spell damage!");
                        totalDamage += GameSpell.GetSpellDamageModifier(caster) + Rules.Dice.Next(Skills.GetSkillLevel(caster.magic) * (2 + addedMultiplier), Skills.GetSkillLevel(caster.magic) * (3 + addedMultiplier));

                        // Extra skill gain for those with magic skill.
                        if(caster.IsSpellWarmingProfession)
                            Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);
                    }
                }
                #endregion

                #region Send Damage Shield Message
                if (caster != null && target != null && caster is PC)
                {
                    if (spellType.ToLower().EndsWith("shield"))
                    {
                        if ((caster as PC).DisplayDamageShield && (System.Configuration.ConfigurationManager.AppSettings["DisplayDamageShield"].ToLower() == "true" ||
                            (caster as PC).ImpLevel >= Globals.eImpLevel.DEV) && totalDamage > 0)
                        {
                            caster.WriteToDisplay("Your " + spellType + " does " + totalDamage + " damage to " + target.GetNameForActionResult(true) + ".");
                       }
                    }
                    else
                    {
                        // display combat damage if applicable
                        if ((caster as PC).DisplayCombatDamage && (System.Configuration.ConfigurationManager.AppSettings["DisplayCombatDamage"].ToLower() == "true" ||
                            (caster as PC).ImpLevel >= Globals.eImpLevel.DEV) && totalDamage > 0)
                        {
                            caster.WriteToDisplay("Your " + spellType + " does " + totalDamage + " damage to " + target.GetNameForActionResult(true) + ".");//, Protocol.TextType.SpellHit);
                        }
                    }
                }
                #endregion

                #region Total Damage > 0
                if (totalDamage > 0)
                {
                    #region Display pet damage if enabled
                    if (caster != null && caster.PetOwner != null && caster.PetOwner.IsPC && (caster.PetOwner as PC).DisplayPetDamage)
                        caster.PetOwner.WriteToDisplay("Your pet " + caster.Name + " DOES " + damage + " damage to " + target.GetNameForActionResult(true) + ".");

                    if (target != null && target.PetOwner != null && target.PetOwner.IsPC && (target.PetOwner as PC).DisplayPetDamage)
                        target.PetOwner.WriteToDisplay("Your pet " + target.Name + " TOOK " + damage + " from " + caster.GetNameForActionResult(true) + ".");
                    #endregion

                    if (!string.IsNullOrEmpty(damageType))
                        target.WriteToDisplay("You have been hit by " + damageType + "!");

                    #region More than 50% damage will cause a prepped spell to be lost.
                    if (target.preppedSpell != null && totalDamage >= (int)target.Hits * .50 && totalDamage < target.Hits)
                    {
                        target.preppedSpell = null;
                        target.WriteToDisplay("Your spell has been lost.");
                        target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                    }
                    #endregion

                    // if target has a memorized spell chant and more than 75% of remaining hits are taken
                    if (!string.IsNullOrEmpty(target.MemorizedSpellChant) && damage >= target.Hits * .75)
                    {
                        target.MemorizedSpellChant = "";
                        target.WriteToDisplay("You have forgotten your memorized spell chant!");
                        target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.MemorizedChantLoss));
                    }

                    target.Hits -= totalDamage;

                    #region Lifeleech grants hits in return.
                    if (spellType.ToLower() == "lifeleech" && caster.Hits < caster.HitsFull)
                    {
                        caster.Hits += Convert.ToInt32(totalDamage / 3); // lifeleech graints 33% heal to caster of total damage done to the target
                        if (caster.Hits > caster.HitsFull) caster.Hits = caster.HitsFull;
                        // TODO: allow lifeleech casters to gain more hits than their max in certain conditions?
                        caster.updateHits = true;
                    } 
                    #endregion

                    target.updateHits = true; // update hits (used in displaying map)
                    target.DamageRound = DragonsSpineMain.GameRound; // update damage round
                    target.IsResting = false; // cancel resting
                    target.IsMeditating = false; // cancel meditating

                    // grant extra skill experience for damage done by a spell the caster knows
                    // modified on 9/19/2013 to set up some requirements -Eb
                    if (caster != null && totalDamage > 0 && caster.IsSpellWarmingProfession && caster != target) //mlt added,a little skill xp for damage done
                    {
                        GameSpell spell = GameSpell.GetSpell(spellType);

                        if (spell != null && caster.spellDictionary != null && caster.spellDictionary.ContainsKey(spell.ID))
                        {
                            switch (spellType)
                            {
                                case "flameshield":
                                    // no extra skill for flame shield damage
                                    break;
                                default:
                                    Skills.GiveSkillExp(caster, totalDamage, Globals.eSkillType.Magic);
                                    break;
                            }
                        }
                    }

                    #region Spell Damage Logging
                    if (DragonsSpineMain.Instance.Settings.DetailedSpellLogging)
                    {
                        if (!target.IsPC && caster != null)
                            Utils.Log(target.GetLogString() + " took (Damage: " + damage + " - Protection: " + protection + ") = " + totalDamage + " from " + caster.GetLogString() + " " + spellType.ToUpper() + " Health: " + target.Hits, Utils.LogType.SpellDamageToCreature);
                        else if (target.IsPC && caster != null)
                            Utils.Log(target.GetLogString() + " took (Damage: " + damage + " - Protection: " + protection + ") = " + totalDamage + " from " + caster.GetLogString() + " " + spellType.ToUpper() + " Health: " + target.Hits, Utils.LogType.SpellDamageToPlayer);
                        else if (damage > 1)
                                Utils.Log(target.GetLogString() + " took (Damage: " + damage + " - Protection: " + protection + ") = " + totalDamage + " from " + spellType.ToUpper() + " at (" + target.X + ", " + target.Y + ") Health: " + target.Hits, Utils.LogType.SpellDamageFromMapEffect);
                    }
                    #endregion

                    if (target.Hits <= 0)
                    {
                        Rules.DoDeath(target, caster);

                        if (caster != null && caster.IsPC) { return 1; } // 4/15/2013 should return 1 to NPC as well...

                        return 0;
                    }

                    if (target.IsHidden) // if target is hidden break the hide spell
                        target.IsHidden = false;

                    // if the target is stunned by this spell damage
                    if (stunChance && target.Stunned <= 0)
                    {
                        int modifier = -target.ZonkResistance;

                        if (spellType.ToLower().IndexOf("lightning") != -1)
                            modifier = -target.LightningResistance;

                        if(target.HasEffect(Effect.EffectTypes.Cynosure, out Effect cynosureEffect))
                            modifier += cynosureEffect.Power;

                        if (!DND_CheckSavingThrow(target, SavingThrow.PetrificationPolymorph, modifier))
                        {
                            target.Stunned = (short)Rules.RollD(1, 2);
                            target.SendToAllInSight(target.GetNameForActionResult() + " is stunned!");
                            target.WriteToDisplay("You are stunned!");

                            if (target.preppedSpell != null)
                            {
                                target.preppedSpell = null;
                                target.WriteToDisplay("Your spell has been lost.");
                                target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                            }
                        }
                    }
                    return 0;
                }
                #endregion

                #region Total Damage <= 0
                else
                {
                    //if (!string.IsNullOrEmpty(damageType) && target != null)
                    //    target.WriteToDisplay("You have taken no damage from " + damageType + ".");

                    if (DragonsSpineMain.Instance.Settings.DetailedSpellLogging)
                    {
                        if (!target.IsPC && caster != null)
                            Utils.Log(target.GetLogString() + " took (Damage: " + damage + " - Protection: " + protection + ") = " + totalDamage + " from " + caster.GetLogString() + " " + spellType.ToUpper() + " Health: " + target.Hits, Utils.LogType.SpellDamageToCreature);
                        else if (target.IsPC && caster != null)
                            Utils.Log(target.GetLogString() + " took (Damage: " + damage + " - Protection: " + protection + ") = " + totalDamage + " from " + caster.GetLogString() + " " + spellType.ToUpper() + " Health: " + target.Hits, Utils.LogType.SpellDamageToPlayer);
                        else if (damage > 1)
                            Utils.Log(target.GetLogString() + " took (Damage: " + damage + " - Protection: " + protection + ") = " + totalDamage + " from " + spellType.ToUpper() + " at (" + target.X + ", " + target.Y + ") Health: " + target.Hits, Utils.LogType.SpellDamageFromMapEffect);
                    }
                }
                return 0;
                #endregion
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return 0;
            }
        }

        public static void DoConcussionDamage(Cell cell, int damage, Character cause)
        {
            try
            {
                foreach (Character chr in cell.Characters.Values)
                {
                    chr.WriteToDisplay("You have been hit by an explosion!");
                    if (Combat.DoSpellDamage(cause, chr, null, damage, "concussion") == 1)
                    {
                        Rules.GiveAEKillExp(cause, chr);
                    }
                }

            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static void DoCombat(Character attacker, Character target, Item weapon)
        {
            // safety nets
            if (target == null || target.IsDead)
            {
                if (attacker != null && !attacker.IsDead)
                    attacker.WriteToDisplay("You do not see your target.");
                return;
            }

            if (attacker == null || attacker.IsDead) return;

            if (weapon != null)
            {
                if (weapon.IsAttunedToOther(attacker)) // check if a weapon is attuned
                {
                    attacker.CurrentCell.Add(weapon);
                    attacker.WriteToDisplay("The " + weapon.name + " leaps from your hand!");
                    if (attacker.wearing.Contains(weapon))
                        attacker.RemoveWornItem(weapon);
                    else if (weapon == attacker.RightHand)
                        attacker.UnequipRightHand(weapon);
                    else if (weapon == attacker.LeftHand)
                        attacker.UnequipLeftHand(weapon);
                    return;
                }

                if (!weapon.AlignmentCheck(attacker)) // check if a weapon has an alignment
                {
                    attacker.WriteToDisplay("The " + weapon.name + " singes your hand and falls to the ground!");
                    attacker.CurrentCell.Add(weapon);

                    if (attacker.wearing.Contains(weapon))
                        attacker.RemoveWornItem(weapon);
                    else if (attacker.RightHand == weapon)
                        attacker.UnequipRightHand(weapon);
                    else if (attacker.LeftHand == weapon)
                        attacker.UnequipLeftHand(weapon);

                    DoDamage(attacker, attacker, Rules.RollD(1, 4), false);
                    return;
                }
            }

            bool specialBlock = false;

            try
            {
                #region Special Block
                if (Combat.CheckSpecialBlock(attacker, target, weapon))
                {
                    // give some skill experience for learning this target has a special block
                    // also gives some skill to those sparring amoral demons and merchants (temporary)

                    if (((target is Merchant) && ((target as Merchant).merchantType > Merchant.MerchantType.None ||
                        (target as Merchant).trainerType > Merchant.TrainerType.None)) ||
                        target.Alignment == Globals.eAlignment.Amoral || target.IsImmortal)
                    {
                        attacker.WriteToDisplay("You miss!");
                        target.WriteToDisplay(attacker.GetNameForActionResult() + " misses you.");
                    }
                    else
                    {
                        string attackType = "Swing";

                        // List of attackType translations.
                        List<CommandTasker.CommandType> cmds = new List<CommandTasker.CommandType>()
                        {
                            CommandTasker.CommandType.Shield_Bash, CommandTasker.CommandType.Jumpkick, CommandTasker.CommandType.Kick, CommandTasker.CommandType.Poke,
                            CommandTasker.CommandType.Leg_Sweep, CommandTasker.CommandType.Cleave
                        };

                        if (attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot) || attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Throw))
                            attackType = "Shot";
                        else
                        {
                            foreach (CommandTasker.CommandType cmdType in cmds)
                            {
                                if (attacker.CommandsProcessed.Contains(cmdType))
                                    attackType = Utils.FormatEnumString(cmdType.ToString());
                            }
                        }

                        attacker.WriteToDisplay(attackType + " hits for little effect.");
                        target.WriteToDisplay(attacker.GetNameForActionResult() + " hits with little effect.");

                    }

                    target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.MeleeMiss));

                    specialBlock = true;
                }
                #endregion
            }
            catch (Exception e)
            {
                Utils.Log("Failed to check special block portion of Rules.doCombat.", Utils.LogType.SystemFailure);
                Utils.LogException(e);
            }

            // Going into combat will drop the obfuscation effect.
            if(attacker.EffectsList.ContainsKey(Effect.EffectTypes.Obfuscation))
                attacker.EffectsList[Effect.EffectTypes.Obfuscation].StopCharacterEffect();

            int hitcheck = 0;

            if (!specialBlock)
            {
                try
                {
                    int attackRoll = 0;

                    try
                    {
                        hitcheck = DND_RollToHit(attacker, target, weapon, ref attackRoll);
                    }
                    catch (Exception e)
                    {
                        Utils.Log("Failed Rules.DNDrollToHit portion of Rules.doCombat.", Utils.LogType.SystemFailure);
                        Utils.LogException(e);
                    }

                    // Backstabs never critically fail. Removed on 1/1/2017 -Eb
                    //if (attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Backstab) && hitcheck == -1) hitcheck++;

                    bool damageShieldDeath = false;

                    switch (hitcheck)
                    {
                        case -1: // critical miss, possible fumble, always breaks non permanent hide spell
                            #region critical miss
                            attacker.IsHidden = false;

                            // damage shield if critical miss

                            if (!target.IsDead && target.EffectsList.ContainsKey(Effect.EffectTypes.Flame_Shield))
                            {
                                if (DoSpellDamage(target, attacker, null, target.EffectsList[Effect.EffectTypes.Flame_Shield].Power * 2, "flameshield") == 1)
                                {
                                    damageShieldDeath = true;
                                }
                            }

                            if (weapon == null 
                                || weapon != null &&
                                                (weapon == attacker.GetInventoryItem(Globals.eWearLocation.Hands) && !attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot))  ||
                                                weapon == attacker.GetInventoryItem(Globals.eWearLocation.Feet))
                            {
                                if (!attacker.animal)
                                {
                                    attacker.EmitSound(Sound.GetCommonSound(Sound.CommonSound.UnarmedMiss));
                                }
                                else
                                {
                                    attacker.EmitSound(Sound.GetCommonSound(Sound.CommonSound.MeleeMiss));
                                }

                                // critical miss jumpkick
                                if (attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Jumpkick))
                                {
                                    if (Skills.GetSkillLevel(attacker.GetSkillExperience(Globals.eSkillType.Unarmed)) < 7)
                                    {
                                        attacker.WriteToDisplay("You have slipped and fallen.");
                                        if (Rules.RollD(1, 20) < 10)
                                        {
                                            attacker.Stunned = (short)Rules.RollD(1, 2); // stun the character
                                            attacker.WriteToDisplay("You are stunned!");
                                        }
                                        if (attacker.Hits > 4) // falling damage - don't kill the character on a critically failed jumpkick. tempting though.
                                            Combat.DoDamage(attacker, attacker, Rules.RollD(1, 4), false);
                                        else
                                            Combat.DoDamage(attacker, attacker, Rules.RollD(1, attacker.Hits - 1), false);
                                    }
                                    else attacker.WriteToDisplay("You miss!");
                                }
                                // critical miss kick
                                else if (attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Kick))
                                {
                                    attacker.WriteToDisplay("You have pulled a muscle.");
                                    if (attacker.Hits > 2) // again, don't kill the character on a critically failed kick
                                        Combat.DoDamage(attacker, attacker, Rules.RollD(1, 2), false);
                                    else
                                        Combat.DoDamage(attacker, attacker, Rules.RollD(1, attacker.Hits - 1), false);
                                }
                                else
                                {
                                    attacker.WriteToDisplay("You miss!");
                                    target.WriteToDisplay(attacker.GetNameForActionResult() + " misses you.");
                                }
                            }
                            else
                            {
                                if (Combat.CheckFumble(attacker, weapon))
                                {
                                    Combat.DoFumble(attacker, weapon);
                                }
                                else
                                {
                                    attacker.WriteToDisplay("You miss!");
                                    target.WriteToDisplay(attacker.GetNameForActionResult() + " misses you.");
                                }
                            }

                            // attacker died due to damage shield
                            if (damageShieldDeath)
                            {
                                Rules.GiveAEKillExp(target, attacker);
                                Rules.DoDeath(attacker, target);
                                return;
                            }

                            break;
                            #endregion
                        case 0: // miss
                            #region miss
                            // damage shields
                            if (!target.IsDead && target.EffectsList.ContainsKey(Effect.EffectTypes.Flame_Shield))
                            {
                                if (DoSpellDamage(target, attacker, null, target.EffectsList[Effect.EffectTypes.Flame_Shield].Power / 2, "flameshield") == 1)
                                    damageShieldDeath = true;
                            }

                            if (!Combat.SendMissedAttackMessage(attacker, target, weapon, attackRoll)) // writes out the miss message
                            {
                                if (weapon == null || (weapon != null && ((weapon.wearLocation == Globals.eWearLocation.Hands) && !attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot)) || weapon.wearLocation == Globals.eWearLocation.Feet))
                                {
                                    if (!attacker.animal)
                                        attacker.EmitSound(Sound.GetCommonSound(Sound.CommonSound.UnarmedMiss));
                                    else
                                        attacker.EmitSound(Sound.GetCommonSound(Sound.CommonSound.MeleeMiss));
                                }
                                else if (weapon.RangePreferred() && target != null)
                                {
                                    target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.ThrownWeapon));
                                }
                                else
                                {
                                    attacker.EmitSound(Sound.GetCommonSound(Sound.CommonSound.MeleeMiss));
                                }

                                attacker.WriteToDisplay("You miss!");
                                target.WriteToDisplay(attacker.GetNameForActionResult() + " misses you.");
                            }

                            // attacker died due to damage shield
                            if (damageShieldDeath)
                            {
                                Rules.GiveAEKillExp(target, attacker);
                                Rules.DoDeath(attacker, target);
                            }

                            // Be careful adding logic after a death... 11/22/2015 Eb
                            break;
                            #endregion
                        case 1: // normal hit
                            DND_Attack(attacker, target, weapon, false);
                            break;
                        case 2: // critical hit
                            if (attacker.GetWeaponSkillLevel(weapon) < 6) // critical hits (including stuns) not possible below skill level 5
                                DND_Attack(attacker, target, weapon, false);
                            else DND_Attack(attacker, target, weapon, true);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return;
                }
            }

            // DPS log a miss. Successful attacks are logged elsewhere.
            if (attacker.DPSLoggingEnabled && (specialBlock || hitcheck <= 0))
            {
                int itemID = -1;
                GameSystems.DPSCalculator.DamageType dmgType = GameSystems.DPSCalculator.DamageType.MartialArtsMiss;

                if (attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot) || attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot))
                {
                    itemID = weapon.itemID;
                    dmgType = GameSystems.DPSCalculator.DamageType.RangeWeaponMiss;
                }
                else if (weapon != null && weapon.wearLocation != Globals.eWearLocation.Hands && weapon.wearLocation != Globals.eWearLocation.Feet)
                {
                    itemID = weapon.itemID;
                    dmgType = GameSystems.DPSCalculator.DamageType.MeleeMiss;
                }

                GameSystems.DPSCalculator.AddMissedAttack(attacker, DragonsSpineMain.GameRound, itemID, dmgType);
            }
        }

        /// <summary>
        /// Perform a weapon fumble. Check for fragile items, bottles, special properties of items (Thor hammer).
        /// </summary>
        /// <param name="ch">The Character doing the fumbling.</param>
        /// <param name="weapon">The weapon being fumbled.</param>
        public static void DoFumble(Character ch, Item weapon)
        {
            if (ch == null) return;

            // Why is an NPC removed from a group when it fumbles? -Eb
            if (ch is NPC && ch.Group != null)
            {
                ch.Group.Remove(ch as NPC);
            }

            ch.WriteToDisplay("You fumble!");

            if (ch.IsPC) // fumble sounds
            {
                if (ch.gender == Globals.eGender.Male)
                    ch.EmitSound(Sound.GetCommonSound(Sound.CommonSound.MaleFumble));
                else if (ch.gender == Globals.eGender.Female)
                    ch.EmitSound(Sound.GetCommonSound(Sound.CommonSound.FemaleFumble));
            }

            ch.SendToAllInSight(ch.GetNameForActionResult() + " fumbles.");

            if (weapon == null) return;

            if (weapon.itemID == Item.ID_THOR_HAMMER)
            {
                GameSpell.GetSpell((int)GameSpell.GameSpellID.Lightning_Bolt).CastSpell(ch, "");
                if (ch.IsDead) return;
                GameSpell.GetSpell((int)GameSpell.GameSpellID.Chain_Lightning).CastSpell(ch, "");
                if (ch.IsDead) return;
            }

            if (ch is NPC)
            {
                // NPC should want to pick up this item. Store the item temporarily.
                if ((ch as NPC).PivotItem == null) { (ch as NPC).PivotItem = weapon; }
            }


            if (weapon.skillType == Globals.eSkillType.Bow)
            {
                weapon.IsNocked = false;
            }
            else
            {
                if (weapon == ch.RightHand) // weapon from character's right hand
                {
                    // mlt? These ideas match chaotic rhammer and evil rdagger.
                    if (weapon.itemID == Item.ID_RHAMMER_CHAOTIC || weapon.itemID == Item.ID_RDAGGER_EVIL) { return; }

                    if (!ch.CommandsProcessed.Contains(CommandTasker.CommandType.Throw))
                    {
                        ch.UnequipRightHand(weapon);
                        if(ch.CurrentCell != null) ch.CurrentCell.Add(weapon);
                    }
                    else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Throw) && !ch.RightHand.special.Contains("figurine"))
                    {
                        ch.UnequipRightHand(weapon);
                        ch.CurrentCell.Add(weapon);
                    }
                    else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Throw) && ch.RightHand.special.Contains("figurine"))
                    {
                        ch.UnequipRightHand(weapon);
                    }
                }
                else if (weapon == ch.LeftHand) // weapon from character's left hand
                {
                    if (weapon.itemID == Item.ID_RHAMMER_CHAOTIC || weapon.itemID == Item.ID_RDAGGER_EVIL) { return; }

                    if (!ch.CommandsProcessed.Contains(CommandTasker.CommandType.Throw))
                    {
                        ch.UnequipLeftHand(weapon);
                        ch.CurrentCell.Add(weapon);
                    }
                    else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Throw) && !ch.LeftHand.special.Contains("figurine"))
                    {
                        ch.UnequipLeftHand(weapon);
                        ch.CurrentCell.Add(weapon);
                    }
                    else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Throw) && ch.LeftHand.special.Contains("figurine"))
                    {
                        ch.UnequipLeftHand(weapon);
                    }
                }
                else // weapon from character's belt
                {
                    for (int a = 0; a < ch.beltList.Count; a++)
                    {
                        if (weapon == ch.beltList[a])
                        {
                            ch.beltList.Remove(weapon);
                            ch.CurrentCell.Add(weapon);
                            break;
                        }
                    }
                }
            }

            #region Weapon with lightning is fumbled.
            if (weapon.lightning)  // bad things can happen when certain items are fumbled
            {
                // Special damage for Thor hammer here.
                //GameSpell.GetSpell((int)GameSpell.GameSpellID.Lightning_Bolt).CastSpell(chr, "");
                //GameSpell.GetSpell((int)GameSpell.GameSpellID.Chain_Lightning).CastSpell(chr, "");


                if (Rules.RollD(1, 100) >= 50) // 50 percent chance a fumbled lightning weapon will cast
                {
                    GameSpell oldPrepped = ch.preppedSpell;
                    long oldMagic = ch.magic;

                    ch.preppedSpell = GameSpell.GetSpell("lightning");
                    ch.magic = Rules.Dice.Next((int)((weapon.combatAdds + 1) * 100000 / 2), weapon.combatAdds + 1 * 100000);

                    ch.preppedSpell.CastSpell(ch, "");

                    ch.preppedSpell = oldPrepped;
                    ch.magic = oldMagic;

                    if (ch.IsDead) { return; }
                }
            }
            #endregion

            #region Bottles and fragile items may break when fumbled.
            if (weapon.baseType == Globals.eItemBaseType.Bottle || weapon.fragile) // certain items break when fumbled
            {
                // 50% chance to break.
                if (Rules.RollD(1, 100) >= 50)
                {
                    ch.SendToAllInSight("You hear something shatter at " + ch.Name + "'s feet.");
                    ch.WriteToDisplay("The " + weapon.name + " shatters at your feet.");
                    ch.CurrentCell.Remove(weapon);
                }
            }
            #endregion
        }

        /// <summary>
        /// Determine if a jumpkick is successful dependent upon unarmed skill level and encumbrance. If successful, DoCombat is called.
        /// </summary>
        /// <param name="ch">The jumpkicker.</param>
        /// <param name="target">Target being jumpkicked.</param>
        public static void DoJumpKick(Character ch, Character target)
        {
            if (target == null || target.IsDead) return;
            if (ch == null || ch.IsDead) return;

            Globals.eEncumbranceLevel EncumbDesc = Rules.GetEncumbrance(ch);
            int fallChance = 0;
            int fallDmg = 1;
            int roll = 0;

            if (ch.GetWeaponSkillLevel(null) < 6) // if character is below black belt there is a chance to fall when jumpkicking
            {
                fallChance += 20 - ch.GetWeaponSkillLevel(null); // add more of a chance to fall, and damage according to encumbrance
                fallDmg += Rules.RollD(1, 2);

                if (EncumbDesc == Globals.eEncumbranceLevel.Moderately)
                {
                    fallChance += 5;
                    fallDmg += Rules.RollD(1, 2);
                }
                else if (EncumbDesc == Globals.eEncumbranceLevel.Heavily)
                {
                    fallChance += 10;
                    fallDmg += Rules.RollD(1, 4);
                }
                else if (EncumbDesc == Globals.eEncumbranceLevel.Severely)
                {
                    fallChance += 15;
                    fallDmg += Rules.RollD(1, 6);
                }

                roll = Rules.RollD(1, 100);

                if (roll < (60 + fallChance) - ch.GetWeaponSkillLevel(null))
                {
                    ch.WriteToDisplay("You have slipped and fallen.");
                    if (roll < (50 + fallChance) - ch.GetWeaponSkillLevel(null))
                    { ch.Stunned = (short)Rules.RollD(1, 2); }
                    DoDamage(ch, ch, fallDmg, false);
                    return;
                }
                else if (!target.IsDead)
                {
                    DoCombat(ch, target, ch.GetInventoryItem(Globals.eWearLocation.Feet));
                }
            }
            else // at black belt level there is no chance to fall when jumpkicking, unless encumbered
            {
                if (EncumbDesc > Globals.eEncumbranceLevel.Lightly)
                {
                    if (EncumbDesc == Globals.eEncumbranceLevel.Moderately) { fallChance += 2; fallDmg += Rules.RollD(1, 2); }
                    else if (EncumbDesc == Globals.eEncumbranceLevel.Heavily) { fallChance += 5; fallDmg += Rules.RollD(1, 3); }
                    else if (EncumbDesc == Globals.eEncumbranceLevel.Severely) { fallChance += 8; fallDmg += Rules.RollD(1, 6); }

                    roll = Rules.RollD(1, 100);

                    if (roll < (20 + fallChance) - ch.GetWeaponSkillLevel(null))
                    {
                        ch.WriteToDisplay("You have slipped and fallen.");
                        if (roll < (10 + fallChance) - ch.GetWeaponSkillLevel(null))
                        {
                            ch.Stunned = (short)Rules.RollD(1, fallChance);
                        }
                        Combat.DoDamage(ch, ch, fallDmg, false);
                    }
                    else if (!target.IsDead)
                    {
                        Combat.DoCombat(ch, target, ch.GetInventoryItem(Globals.eWearLocation.Feet));
                    }
                }
                else if (!target.IsDead)
                {
                    Combat.DoCombat(ch, target, ch.GetInventoryItem(Globals.eWearLocation.Feet));
                }
            }
        }

        /// <summary>
        /// Hard code determination if a weapon can be fumbled.
        /// </summary>
        /// <param name="ch">Character using the weapon.</param>
        /// <param name="weapon">The weapon being used.</param>
        /// <returns>False if weapon will not be fumbled.</returns>
        public static bool CheckFumble(Character ch, Item weapon)
        {
            // Not wielding a weapon. Impossible to fumble?
            if (weapon == null) { return false; }

            // Special check for the Thor hammer.
            if(weapon.itemID == Item.ID_THOR_HAMMER && Rules.RollD(1, 100) <= (Rules.RollD(1, 100) - Rules.RollD(2, 20)))
            {
                return false;
            }

            // gauntlets and boots cannot be fumbled -- 2/5/17 added skilltype check for the new wristbow item
            if ((weapon == ch.GetInventoryItem(Globals.eWearLocation.Hands) && weapon.skillType != Globals.eSkillType.Bow) || weapon == ch.GetInventoryItem(Globals.eWearLocation.Feet))
                return false;

            // prevent a weapon wielded by an npc that will be attuned from fumbling
            if (!ch.IsPC && weapon.attuneType != Globals.eAttuneType.None) { return false; }

            // allow for a 1% chance to fumble across the board
            if (Rules.RollD(1,100) == 1)
            {
                return true;
            }

            // no fumbles for fighters using their specialized skill
            if (ch.fighterSpecialization == weapon.skillType) { return false; }

            // chance to fumble decreases the higher the skill is, no chance after skill 12
            if (NO_FUMBLE_SKILL_LEVEL - ch.GetWeaponSkillLevel(weapon) - ch.dexterityAdd < Rules.RollD(1, 12))
                return false;

            return true;
        }

        /// <summary>
        /// Checks for a special (automatic) block. IE: blueglow, silver, blunt/pierce/slash, special weapon requirement
        /// </summary>
        /// <param name="ch">The attacker using the weapon.</param>
        /// <param name="target">Target of the weapon.</param>
        /// <param name="weapon">Weapon being used.</param>
        /// <returns>Returns true if a special block occurred.</returns>
        public static bool CheckSpecialBlock(Character ch, Character target, Item weapon)
        {
            if (target is NPC && (target as NPC).entity == EntityLists.Entity.Overlord)
            {
                if (weapon == null) return true;

                if(!ch.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot) && !ch.CommandsProcessed.Contains(CommandTasker.CommandType.Throw)) return true;
            }

            // all merchants, trainers, and amoral creatures are always blocked
            if (target is Merchant || target.Alignment == Globals.eAlignment.Amoral || target.IsImmortal)
                return true;

            // special weapon requirement field
            if (EntityLists.WEAPON_REQUIREMENT.ContainsKey(target.entity))
            {
                if (weapon == null) return true;

                if (EntityLists.WEAPON_REQUIREMENT.ContainsKey(target.entity))
                {
                    bool notWieldingSpecialWeapon = true;

                    foreach (string specialAttack in EntityLists.WEAPON_REQUIREMENT[target.entity])
                    {
                        if (weapon.special.ToLower().Contains(specialAttack))
                            notWieldingSpecialWeapon = false; // they ARE wielding the correct weapon
                    }

                    if (notWieldingSpecialWeapon) return true;
                }
            }

            bool blocked = false;

            // Stoneskin blocks all non magical weapons (currently combatAdds <= 0, non blueglow), and piercing weapons.
            if(target.EffectsList.ContainsKey(Effect.EffectTypes.Stoneskin))
            {
                blocked = true;
                if (weapon == null)
                {
                    Globals.eWearLocation wearLoc = Globals.eWearLocation.Hands;
                    if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Kick) || ch.CommandsProcessed.Contains(CommandTasker.CommandType.Jumpkick))
                        wearLoc = Globals.eWearLocation.Feet;

                    foreach (Item wItem in ch.wearing)
                        if (wItem.wearLocation == wearLoc && (wItem.combatAdds > 0 || wItem.blueglow)) { blocked = false; break; }

                    //goto piercers;
                }
                else if (weapon.combatAdds > 0 || weapon.blueglow) { blocked = false; }
            }

            #region Blueglow Requirement
            if (target.special.Contains(Item.BLUEGLOW) || ((target is NPC) && EntityLists.BLUEGLOW_REQUIRED.Contains((target as NPC).entity)))
            {
                if (weapon == null)
                {
                    Globals.eWearLocation wearLoc = Globals.eWearLocation.Hands;
                    if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Kick) || ch.CommandsProcessed.Contains(CommandTasker.CommandType.Jumpkick))
                        wearLoc = Globals.eWearLocation.Feet;

                    foreach (Item wItem in ch.wearing)
                        if (wItem.wearLocation == wearLoc && wItem.blueglow) { blocked = false; break; }

                    //goto piercers;
                }
                else if (weapon.blueglow) { blocked = false; }
            }
            #endregion

            #region Silver Required
            if (target.special.Contains(Item.SILVER) || ((target is NPC) && EntityLists.SILVER_REQUIRED.Contains((target as NPC).entity)))
            {
                if (weapon == null)
                {
                    blocked = true;

                    Globals.eWearLocation wearLoc = Globals.eWearLocation.Hands;

                    if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Kick) || ch.CommandsProcessed.Contains(CommandTasker.CommandType.Jumpkick))
                        wearLoc = Globals.eWearLocation.Feet;

                    foreach (Item wItem in ch.wearing)
                        if (wItem.wearLocation == wearLoc && wItem.silver) { blocked = false; break; }

                    //goto piercers;
                }
                else if (!weapon.silver) { blocked = true; }
            }
            #endregion

            #region Blueglow & Silver Required
            if ((target.special.Contains(Item.BLUEGLOW) && target.special.Contains(Item.SILVER)) ||
                (target is NPC && EntityLists.BLUEGLOW_REQUIRED.Contains((target as NPC).entity) && EntityLists.SILVER_REQUIRED.Contains((target as NPC).entity)))
            {
                if (weapon == null)
                {
                    Globals.eWearLocation wearLoc = Globals.eWearLocation.Hands;
                    if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Kick) || ch.CommandsProcessed.Contains(CommandTasker.CommandType.Jumpkick))
                        wearLoc = Globals.eWearLocation.Feet;
                    foreach (Item wItem in ch.wearing)
                    {
                        if (wItem.wearLocation == wearLoc)
                        {
                            if (!wItem.blueglow || !wItem.silver) { blocked = true; break; }
                        }
                    }
                    //goto piercers;
                }
                if (!weapon.blueglow || !weapon.silver) { blocked = true; }
            }
            #endregion

        piercers:
            #region Non Piercing Weapon Required (eg: skeletons)
            if (target.EffectsList.ContainsKey(Effect.EffectTypes.Stoneskin) || target.special.Contains(Item.NOPIERCE) || (target is NPC && EntityLists.NOPIERCE_REQUIRED.Contains((target as NPC).entity)))
            {
                if (weapon != null && weapon.special.Contains("pierce") && !weapon.special.Contains("slash") && !weapon.special.Contains("blunt")) { blocked = true; }
            }
            #endregion

            #region Blunt Required
            if (target.special.Contains(Item.ONLYBLUNT) || (target is NPC && EntityLists.ONLYBLUNT_REQUIRED.Contains((target as NPC).entity)))
            {
                if (weapon == null) { blocked = false; }
                else if (weapon.baseType == Globals.eItemBaseType.Shield) { blocked = false; }
                else if (!weapon.special.Contains("blunt")) { blocked = true; }
            }
            #endregion

            //#region Slash Required
            //if (target.special.Contains("onlyslash"))
            //{
            //    if (weapon == null) { blocked = true; }
            //    else if (!weapon.special.Contains("slash")) { blocked = true; }
            //}
            //#endregion
            //#region Pierce Required
            //if (target.special.Contains("onlypierce"))
            //{
            //    if (weapon == null) { blocked = true; }
            //    else if (!weapon.special.Contains("pierce")) { blocked = true; }
            //}
            //#endregion
            //#region Only Normal weapons
            //if (-1 != target.special.IndexOf("onlynormal"))
            //{
            //    if (weapon == null) { blocked = true; }
            //    if (weapon.blueglow) { blocked = true; }
            //    if (weapon.silver) { blocked = true; }
            //}
            //#endregion
            return blocked;
        }

        /// <summary>
        /// This is only called upon a succecssful attack, dealing damage, on the target (riposter) if the attacker is wielding a physical (non natural) weapon.
        /// </summary>
        /// <param name="riposter">The target of the initial attack. The check for a riposte is completed in this method.</param>
        /// <param name="attacker">The attacker causing damage, via a physically wielded weapon (read: no natural attacks), to the riposter.</param>
        private static void CheckRiposte(Character riposter, Character attacker)
        {
            if (riposter == null || riposter.IsDead) return;
            if (attacker == null || attacker.IsDead) return;

            // no riposting if stunned
            if (riposter.Stunned > 0)
                return;

            // riposter is blind, and does not have blind fightning talent **4/10/2014** Blind Fightning not currently available at mentors, or in game. This should be a quest.
            if (riposter.IsBlind && !riposter.HasTalent(TALENTS.BlindFighting))
                return;

            // redundancy checks, verify existence of riposte talent and neither combatant is already dead
            if (riposter.HasTalent(TALENTS.Riposte) && !riposter.IsDead && !attacker.IsDead)
            {
                Talents.GameTalent gtRiposte = Talents.GameTalent.GetTalent(TALENTS.Riposte); // RiposteTalent instance

                if (riposter.RightHand != null && gtRiposte.MeetsPerformanceCost(riposter) && gtRiposte.WeaponPassFailCheck(riposter, riposter.RightHand.skillType))
                {
                    if (gtRiposte.Handler.OnPerform(riposter, "right " + attacker.UniqueID.ToString()))
                    {
                        gtRiposte.SuccessfulPerformance(riposter);
                    }
                }

                // if target has dual wield passive talent they can attempt a riposte with a left hand weapon
                if (riposter.LeftHand != null && riposter.HasTalent(TALENTS.DualWield) && gtRiposte.WeaponPassFailCheck(riposter, riposter.LeftHand.skillType))
                {
                    Talents.GameTalent gtDualWield = Talents.GameTalent.GetTalent(TALENTS.DualWield); // DualWieldTalent instance

                    if (gtDualWield.MeetsPerformanceCost(riposter) && gtDualWield.Handler.OnPerform(riposter, null)) // redundancy checks done in DualWieldTalent.Handler.OnPerform
                    {
                        gtDualWield.SuccessfulPerformance(riposter); // initial stamina use

                        if (gtRiposte.MeetsPerformanceCost(riposter) && gtRiposte.Handler.OnPerform(riposter, "left " + attacker.UniqueID.ToString()))
                        {
                            gtRiposte.SuccessfulPerformance(riposter); // secondary stamina use
                        }
                    }
                }
            }
        }

        public static void CheckDoubleAttack(Character attacker, Character target, Item attackWeapon)
        {
            if (target == null || target.IsDead) return;
            if (attacker == null || attacker.IsDead) return;

            if (attackWeapon != null)
            {
                if (attackWeapon.IsAttunedToOther(attacker)) // check if a weapon is attuned
                {
                    attacker.CurrentCell.Add(attackWeapon);
                    attacker.WriteToDisplay("The " + attackWeapon.name + " leaps from your hand!");
                    if (attacker.wearing.Contains(attackWeapon))
                        attacker.RemoveWornItem(attackWeapon);
                    else if (attacker.RightHand == attackWeapon)
                        attacker.UnequipRightHand(attackWeapon);
                    else if (attacker.LeftHand == attackWeapon)
                        attacker.UnequipLeftHand(attackWeapon);
                    return;
                }

                if (!attackWeapon.AlignmentCheck(attacker)) // check if a weapon has an alignment
                {
                    attacker.CurrentCell.Add(attackWeapon);
                    attacker.WriteToDisplay("The " + attackWeapon.name + " singes your hand and falls to the ground!");
                    if (attacker.wearing.Contains(attackWeapon))
                        attacker.RemoveWornItem(attackWeapon);
                    else if (attacker.RightHand == attackWeapon)
                        attacker.UnequipRightHand(attackWeapon);
                    else if (attacker.LeftHand == attackWeapon)
                        attacker.UnequipLeftHand(attackWeapon);
                    DoDamage(attacker, attacker, Rules.RollD(1, 4), false);
                    return;
                }
            }

            if (attacker != null && target != null && attacker.HasTalent(TALENTS.DoubleAttack) && !attacker.IsDead && !target.IsDead)
            {
                Talents.GameTalent gtDoubleAttack = Talents.GameTalent.GetTalent(TALENTS.DoubleAttack);

                // meets performance cost and passes the basic check
                if(gtDoubleAttack.PassiveTalentAvailable(attacker) && gtDoubleAttack.MeetsPerformanceCost(attacker) && gtDoubleAttack.WeaponPassFailCheck(attacker, attackWeapon == null ? Globals.eSkillType.Unarmed : attackWeapon.skillType))
                {
                    string arg = attackWeapon == attacker.RightHand ? "right" : "left";

                    if (gtDoubleAttack.Handler.OnPerform(attacker, arg + " " + target.UniqueID.ToString()))
                    {
                        gtDoubleAttack.SuccessfulPerformance(attacker);

                        Combat.DoCombat(attacker, target, attackWeapon);
                    }
                }
            }
        }

        public static void CheckDualWield(Character attacker, Character target, Item attackWeapon)
        {
            if (target == null || target.IsDead) return;
            if (attacker == null || attacker.IsDead) return;

            if (attackWeapon != null)
            {
                if (attackWeapon.IsAttunedToOther(attacker)) // check if a weapon is attuned
                {
                    attacker.CurrentCell.Add(attackWeapon);
                    attacker.WriteToDisplay("The " + attackWeapon.name + " leaps from your hand!");
                    if (attacker.wearing.Contains(attackWeapon))
                        attacker.RemoveWornItem(attackWeapon);
                    else if (attacker.RightHand == attackWeapon)
                        attacker.UnequipRightHand(attackWeapon);
                    else if (attacker.LeftHand == attackWeapon)
                        attacker.UnequipLeftHand(attackWeapon);
                    return;
                }

                if (!attackWeapon.AlignmentCheck(attacker)) // check if a weapon has an alignment
                {
                    attacker.CurrentCell.Add(attackWeapon);
                    attacker.WriteToDisplay("The " + attackWeapon.name + " singes your hand and falls to the ground!");
                    if (attacker.wearing.Contains(attackWeapon))
                        attacker.RemoveWornItem(attackWeapon);
                    else if (attacker.RightHand == attackWeapon)
                        attacker.UnequipRightHand(attackWeapon);
                    else if (attacker.LeftHand == attackWeapon)
                        attacker.UnequipLeftHand(attackWeapon);
                    DoDamage(attacker, attacker, Rules.RollD(1, 4), false);
                    return;
                }
            }

            // Attacker has dual wield and left hand is not empty. (Martial arts is not yet supported) -Eb 9/16/2015
            if (attacker.HasTalent(TALENTS.DualWield) && attackWeapon != null && attackWeapon.skillType != Globals.eSkillType.Unarmed && target != null && !target.IsDead)
            {
                Talents.GameTalent gtDualWield = Talents.GameTalent.GetTalent(TALENTS.DualWield); // DualWieldTalent instance

                if (gtDualWield.PassiveTalentAvailable(attacker) && gtDualWield.MeetsPerformanceCost(attacker) && gtDualWield.Handler.OnPerform(attacker, null)) // redundancy checks done in DualWieldTalent.Handler.OnPerform
                {
                    gtDualWield.SuccessfulPerformance(attacker); // initial stamina use

                    Combat.DoCombat(attacker, target, attackWeapon);

                    Combat.CheckDoubleAttack(attacker, target, attackWeapon);
                }

                Combat.CheckSpecialWeaponAttack(attacker, target, attackWeapon);
            }
        }

        public static void CheckSpecialWeaponAttack(Character attacker, Character target, Item attackWeapon)
        {
            if (target == null || target.IsDead) return;
            if (attacker == null || attacker.IsDead) return;

            if (attackWeapon != null)
            {
                if (attackWeapon.IsAttunedToOther(attacker)) // check if a weapon is attuned
                {
                    attacker.CurrentCell.Add(attackWeapon);
                    attacker.WriteToDisplay("The " + attackWeapon.name + " leaps from your hand!");
                    if (attacker.wearing.Contains(attackWeapon))
                        attacker.RemoveWornItem(attackWeapon);
                    else if (attackWeapon == attacker.RightHand)
                        attacker.UnequipRightHand(attackWeapon);
                    else if (attackWeapon == attacker.LeftHand)
                        attacker.UnequipLeftHand(attackWeapon);
                    return;
                }

                if (!attackWeapon.AlignmentCheck(attacker)) // check if a weapon has an alignment
                {
                    attacker.CurrentCell.Add(attackWeapon);
                    attacker.WriteToDisplay("The " + attackWeapon.name + " singes your hand and falls to the ground!");
                    if (attacker.wearing.Contains(attackWeapon))
                        attacker.RemoveWornItem(attackWeapon);
                    else if (attackWeapon == attacker.RightHand)
                        attacker.UnequipRightHand(attackWeapon);
                    else if (attackWeapon == attacker.LeftHand)
                        attacker.UnequipLeftHand(attackWeapon);
                    DoDamage(attacker, attacker, Rules.RollD(1, 4), false);
                    return;
                }
            }

            // Trochilidae effect.
            if (attacker != null && attacker.HasEffect(Effect.EffectTypes.Trochilidae) && target != null && attacker != target && !attacker.IsDead && !target.IsDead)
                DoCombat(attacker, target, attackWeapon);

            // Hummingbird special.
            if (attackWeapon != null && attackWeapon.special.ToLower().Contains("hummingbird") && attacker != null && target != null && attacker != target && !attacker.IsDead && !target.IsDead)
                DoCombat(attacker, target, attackWeapon);
        }

        /// <summary>
        /// Tally AC and determine if a combat miss is a true miss or a miss as a result of various AC components.
        /// </summary>
        /// <param name="attacker">The attacker who missed.</param>
        /// <param name="target">The target who was missed.</param>
        /// <param name="attackWeapon">The weapon used by the attacker. This value may be null for unarmed combat.</param>
        /// <param name="attackRoll">The attack roll that was used to determine the missed attack.</param>
        /// <returns>Returns true if the miss results in a simple "You miss." message.</returns>
        public static bool SendMissedAttackMessage(Character attacker, Character target, Item attackWeapon, int attackRoll)
        {
            try
            {
                if (attacker == null || target == null)
                    return false;

                if (attacker.DPSLoggingEnabled)
                {
                    if (attackWeapon == null || (attackWeapon.wearLocation == Globals.eWearLocation.Feet || attackWeapon.wearLocation == Globals.eWearLocation.Hands))
                    {
                        GameSystems.DPSCalculator.AddMissedAttack(attacker, DragonsSpineMain.GameRound, -1, GameSystems.DPSCalculator.DamageType.MartialArtsMiss);
                    }
                    else if (attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot) || attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Throw))
                        GameSystems.DPSCalculator.AddMissedAttack(attacker, DragonsSpineMain.GameRound, attackWeapon.itemID, GameSystems.DPSCalculator.DamageType.RangeWeaponMiss);
                    else GameSystems.DPSCalculator.AddMissedAttack(attacker, DragonsSpineMain.GameRound, attackWeapon.itemID, GameSystems.DPSCalculator.DamageType.MeleeMiss);
                }

                // ranks of defense -- base armor class, dexterity, shield spell, held item(s), armor

                int thac0 = DND_GetBaseTHAC0(attacker.BaseProfession, attacker.GetWeaponSkillLevel(attackWeapon)); //DND_GetModifiedTHAC0(attacker, attackWeapon);

                double targetAC = target.baseArmorClass;

                #region True Miss
                if (thac0 - targetAC >= attackRoll)
                    return false;
                #endregion

                #region Dexterity Miss
                targetAC -= Combat.AC_GetDexterityArmorClassBonus(attacker, target);

                if (thac0 - targetAC >= attackRoll)
                    return false;
                #endregion

                #region Shield Spell Block
                if (attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Cast) || attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot) ||
                    attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Throw) || attacker.CommandsProcessed.Contains(CommandTasker.CommandType.Jumpkick))
                {
                    targetAC -= Combat.AC_GetShieldingArmorClass(target, true);
                }
                else targetAC -= Combat.AC_GetShieldingArmorClass(target, false);

                if (thac0 - targetAC >= attackRoll)
                {
                    attacker.WriteToDisplay("You are blocked by a shield spell.");
                    target.WriteToDisplay(attacker.GetNameForActionResult() + " is blocked by your shield spell.");
                    return true;
                }
                #endregion

                #region Held Item (WEAPON, MISCELLANEOUS) Armor Class Block (non skill related)
                if (target.RightHand != null && !target.RightHand.IsAttunedToOther(target) &&
                    target.RightHand.AlignmentCheck(target))
                {
                    targetAC -= Combat.AC_GetRightHandArmorClass(attacker, target);

                    if (thac0 - targetAC >= attackRoll)
                    {
                        attacker.WriteToDisplay("You are blocked by a " + target.RightHand.name + ".");
                        target.WriteToDisplay(attacker.GetNameForActionResult() + " is blocked by your " + target.RightHand.name + ".");
                        target.EmitSound(Sound.GetWeaponBlockSound(attacker.RightHand, target.RightHand));
                        return true;
                    }
                }

                if (target.LeftHand != null && !target.LeftHand.IsAttunedToOther(target) &&
                    target.LeftHand.AlignmentCheck(target))
                {
                    targetAC -= Combat.AC_GetLeftHandArmorClass(attacker, target);

                    if (thac0 - targetAC >= attackRoll)
                    {
                        attacker.WriteToDisplay("You are blocked by a " + target.LeftHand.name + ".");
                        target.WriteToDisplay(attacker.GetNameForActionResult() + " is blocked by your " + target.LeftHand.name + ".");
                        target.EmitSound(Sound.GetWeaponBlockSound(attacker.RightHand, target.LeftHand));
                        return true;
                    }
                }
                #endregion

                #region Armor Block
                targetAC -= Combat.AC_GetArmorClassRating(target);

                if (thac0 - targetAC >= attackRoll)
                {
                    if (target.baseArmorClass < 10) // TODO: evaluate this statement
                    {
                        if (target is NPC && target.RightHand == null)
                            goto npcUnarmedReturnMessage;

                        return true; // return a true miss
                    }

                    attacker.WriteToDisplay("You are blocked by the armor.");
                    target.WriteToDisplay(attacker.GetNameForActionResult() + " is blocked by your armor.");
                    target.EmitSound(Sound.GetArmorBlockSound(attacker.RightHand));
                    return true;
                }
                #endregion

                #region Unarmed Block
                targetAC -= Combat.AC_GetUnarmedArmorClassBonus(attacker, target);

                if (thac0 - targetAC >= attackRoll)
                {
                    if (target.RightHand == null)
                    {
                        if (target is NPC)
                        {
                            goto npcUnarmedReturnMessage;
                        }
                        else
                        {
                            attacker.WriteToDisplay("You are blocked by a hand.");
                            target.WriteToDisplay(attacker.GetNameForActionResult() + " is blocked by your hand.");
                        }
                        target.EmitSound(Sound.GetWeaponBlockSound(null, null));
                    }
                    else if (target.RightHand.skillType == Globals.eSkillType.Unarmed)
                    {
                        attacker.WriteToDisplay("You are blocked by a " + target.RightHand.name + ".");
                        target.WriteToDisplay(attacker.GetNameForActionResult() + " is blocked by your " + target.RightHand.name + ".");
                    }
                    else if (target.LeftHand != null && target.LeftHand.skillType == Globals.eSkillType.Unarmed)
                    {
                        attacker.WriteToDisplay("You are blocked by a " + target.LeftHand.name + ".");
                        target.WriteToDisplay(attacker.GetNameForActionResult() + " is blocked by your " + target.LeftHand.name + ".");
                    }

                    target.EmitSound(Sound.GetWeaponBlockSound(null, null));

                    return true;
                }
                #endregion

                #region If all other block types do not complete, then do a hand block in most instances.
                else if ((!target.IsBlind || target.IsBlind && target.HasTalent(TALENTS.BlindFighting)) && !target.IsFeared && target.Stunned <= 0)
                {
                    if (target is NPC && target.RightHand == null)
                        goto npcUnarmedReturnMessage;
                    else
                    {
                        if (target.RightHand != null && target.RightHand.name != "greataxe") // greataxe does not block, ever
                        {
                            attacker.WriteToDisplay("You are blocked by a " + target.RightHand.name + ".");
                            target.WriteToDisplay(attacker.GetNameForActionResult() + " is blocked by your " + target.RightHand.name + ".");
                        }
                        else
                        {
                            attacker.WriteToDisplay("You are blocked by the armor.");
                            target.WriteToDisplay(attacker.GetNameForActionResult() + " is blocked by your armor.");
                        }
                    }

                    target.EmitSound(Sound.GetWeaponBlockSound(null, null));

                    return true;
                }
                else return false;

            npcUnarmedReturnMessage:
                NPC npc = (NPC)target;

                if (npc.blockStrings.Count > 0)
                {
                    string blockString = npc.blockStrings[Rules.Dice.Next(0, npc.blockStrings.Count - 1)];

                    if (!char.IsUpper(blockString[0]))
                        attacker.WriteToDisplay(npc.GetNameForActionResult() + " " + blockString + ".");
                    else
                        attacker.WriteToDisplay(blockString);
                }
                else if (EntityLists.IsHumanOrHumanoid(npc))
                {
                    attacker.WriteToDisplay("You are blocked by a hand.");
                    target.WriteToDisplay(attacker.GetNameForActionResult() + " is blocked by your hand.");
                }
                else
                {
                    attacker.WriteToDisplay(target.GetNameForActionResult() + " blocks your attack.");
                    target.WriteToDisplay("You block " + attacker.GetNameForActionResult(true) + "'s attack.");
                }

                target.EmitSound(Sound.GetWeaponBlockSound(null, null));

                return true;

                #endregion
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return false;
            }
        }

        /// <summary>
        /// All successful attack results for a PC in combat are routed through this method.
        /// </summary>
        /// <param name="npc">The attacking NPC.</param>
        /// <param name="target">The target being attacked.</param>
        /// <param name="attackWeapon">The attack weapon used. Null, hands or boots for martial arts.</param>
        /// <param name="dmgAdjective">Light, heavy, severe or fatal.</param>
        /// <returns></returns>
        public static bool SendSuccessfulAttackResults(NPC npc, Character target, Item attackWeapon, string dmgAdjective)
        {
            string attackString = "";

            try
            {
                if ((attackWeapon == null || (attackWeapon.wearLocation == Globals.eWearLocation.Hands || attackWeapon.wearLocation == Globals.eWearLocation.Feet)) &&
                    EntityLists.IsHumanOrHumanoid(npc))
                {
                    if (npc.CommandsProcessed.Contains(CommandTasker.CommandType.Kick))
                        target.WriteToDisplay(npc.GetNameForActionResult() + " kicks you!");
                    else if (npc.CommandsProcessed.Contains(CommandTasker.CommandType.Jumpkick))
                        target.WriteToDisplay(npc.GetNameForActionResult() + " jumpkicks you!");
                    else
                    {
                        // trolls are humanoid, and have claws
                        if (EntityLists.CLAWED.Contains(npc.entity) && Rules.RollD(1, 6) <= 3)
                            target.WriteToDisplay(npc.GetNameForActionResult() + " hits with a claw!");
                        else target.WriteToDisplay(npc.GetNameForActionResult() + " punches you!");
                    }
                }
                else if (npc.CommandsProcessed.Contains(CommandTasker.CommandType.Shield_Bash))
                {
                    string shortDesc = "a shield";

                    if (attackWeapon != null)
                        shortDesc = attackWeapon.shortDesc;
                    else if (npc.RightHand != null && npc.RightHand.baseType == Globals.eItemBaseType.Shield)
                        shortDesc = npc.RightHand.shortDesc;
                    else if (npc.LeftHand != null && npc.LeftHand.baseType == Globals.eItemBaseType.Shield)
                        shortDesc = npc.LeftHand.shortDesc;

                    target.WriteToDisplay(npc.GetNameForActionResult() + " bashes you with " + shortDesc + "!");
                }
                else if (attackWeapon != null)
                {
                    npc.WriteToDisplay((npc.CommandsProcessed.Contains(CommandTasker.CommandType.Throw) || npc.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot) ? "Shot" : "Swing") + " hits with " + dmgAdjective + " damage!");
                    target.WriteToDisplay(npc.GetNameForActionResult() + " hits with " + attackWeapon.shortDesc + "!");
                }
                else
                {
                    if (npc.attackStrings.Count > 0)
                    {
                        attackString = npc.attackStrings[Rules.Dice.Next(0, npc.attackStrings.Count - 1)];

                        // TODO: resolve why there is an empty attackString in the npc's list -- first seen 4/11/2014
                        if (attackString.Trim() == "") attackString = "hits you!";

                        target.WriteToDisplay(npc.GetNameForActionResult() + " " + attackString + "!");

                        // Poisonous attacks.
                        if (npc.poisonous > 0 || EntityLists.POISONOUS.Contains(npc.entity))
                        {
                            if (npc.poisonous <= 0) npc.poisonous = (short)(npc.Level * 2);

                            foreach (string attackWord in POISON_ATTACKS)
                            {
                                if (attackString.ToLower().Contains(attackWord))
                                {
                                    if (!Combat.DND_CheckSavingThrow(target, Combat.SavingThrow.ParalyzationPoisonDeath, -target.PoisonResistance))
                                    {
                                        int poisonAmount = npc.poisonous - target.PoisonProtection;

                                        if (poisonAmount > 0)
                                        {
                                            target.Poisoned += poisonAmount;
                                            target.WriteToDisplay("You have been poisoned.");
                                            Effect.CreateCharacterEffect(Effect.EffectTypes.Poison, poisonAmount, target, 0, npc);
                                        }
                                        else target.WriteToDisplay("You are protected from " + npc.GetNameForActionResult(true) + "'s poison.");
                                    }
                                    else target.WriteToDisplay("You resist " + npc.GetNameForActionResult(true) + "'s poison.");

                                    break;
                                }
                            }
                        }
                    }
                    else target.WriteToDisplay(npc.GetNameForActionResult() + " hits you!");
                }

                // Attacks that move a target.
                if (EntityLists.SMASHER.Contains(npc.entity) && Rules.RollD(1, 6) == 4 ||
                    (attackString.ToLower().Contains("smash") || attackString.ToLower().Contains("buffets")))
                    NPC.AIRandomlyMoveCharacter(target);

                return true;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return false;
            }
        }

        // TODO: Move stun and knock down logic to a separate method. 12/1/2015 Eb

        /// <summary>
        /// All successful attack results for a PC in combat are routed through this method.
        /// </summary>
        /// <param name="ch">The attacking PC.</param>
        /// <param name="target">The target being attacked.</param>
        /// <param name="attackWeapon">The attack weapon used. Null, hands or boots for martial arts.</param>
        /// <param name="dmgAdjective">Light, heavy, severe or fatal.</param>
        /// <param name="totalDamage">Total damage.</param>
        public static void SendSuccessfulAttackResults(PC ch, Character target, Item attackWeapon, string dmgAdjective, int totalDamage)
        {
            string dmgDisplay = "";

            // Display total damage.
            if ((ch as PC).DisplayCombatDamage && (System.Configuration.ConfigurationManager.AppSettings["DisplayCombatDamage"].ToLower() == "true" ||
                (ch as PC).ImpLevel >= Globals.eImpLevel.DEV))
                dmgDisplay = " (" + totalDamage + ")";

            // Martial arts attacks.
            if (attackWeapon == null || (attackWeapon != null && (attackWeapon.wearLocation == Globals.eWearLocation.Hands || attackWeapon.wearLocation == Globals.eWearLocation.Feet))) // handle unarmed attack messages
            {
                #region Martial Arts attacks.
                if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Leg_Sweep))
                {
                    ch.WriteToDisplay("Leg sweep hits for " + dmgAdjective + " damage!" + dmgDisplay);
                    target.WriteToDisplay(ch.GetNameForActionResult() + " sweeps " + Character.PRONOUN[(int)ch.gender].ToLower() + " legs under yours!");

                    if (Rules.RollD(1, 100) + Skills.GetSkillLevel(ch.unarmed) >= 50 && target != null && !target.IsDead && ch != null &&
                        !ch.IsDead && EntityLists.IsHumanOrHumanoid(target) && !EntityLists.IsGiantKin(target))
                    {
                        ch.WriteToDisplay(target.GetNameForActionResult() + " has been knocked down!");
                        target.WriteToDisplay("You have been knocked down!");

                        if (target.preppedSpell != null)
                        {
                            target.preppedSpell = null;
                            target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                        }
                    }
                }
                else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Kick))
                {
                    ch.WriteToDisplay("Kick hits for " + dmgAdjective + " damage!" + dmgDisplay);
                    target.WriteToDisplay(ch.GetNameForActionResult() + " kicks you!");
                }
                else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.RoundhouseKick))
                {
                    ch.WriteToDisplay("Roundhouse kick hits for " + dmgAdjective + " damage!" + dmgDisplay);
                    target.WriteToDisplay(ch.GetNameForActionResult() + " roundhouse kicks you!");
                    if (Rules.RollD(1, 100) + Skills.GetSkillLevel(ch.unarmed) >= 50 && target != null && !target.IsDead && ch != null && !ch.IsDead && EntityLists.IsHumanOrHumanoid(target) && !EntityLists.IsGiantKin(target))
                    {
                        ch.WriteToDisplay(target.GetNameForActionResult() + " has been knocked down!");
                        target.WriteToDisplay("You have been knocked down!");
                        if (target.preppedSpell != null)
                        {
                            target.preppedSpell = null;
                            target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                        }

                        if (target != null)
                            Character.AIRandomlyMoveCharacter(target);
                    }
                }
                else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Jumpkick))
                {
                    ch.WriteToDisplay("Jumpkick hits for " + dmgAdjective + " damage!" + dmgDisplay);
                    target.WriteToDisplay(ch.GetNameForActionResult() + " jumpkicks you!");
                }
                else
                {
                    ch.WriteToDisplay("Swing hits for " + dmgAdjective + " damage!" + dmgDisplay);
                    target.WriteToDisplay(ch.GetNameForActionResult() + " punches you!");
                } 
                #endregion

            }// range weapon attacks
            else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot) || ch.CommandsProcessed.Contains(CommandTasker.CommandType.Throw))
            {
                #region Range Weapon Attacks.
                ch.WriteToDisplay("Shot hits for " + dmgAdjective + " damage!" + dmgDisplay);
                target.WriteToDisplay(ch.GetNameForActionResult() + " hits with " + attackWeapon.shortDesc + "!");

                // If shot does fatal damage then remove the item from hand and deposit it in target's cell here.
                if (dmgAdjective == "fatal")
                {
                    if (ch.RightHand != null && attackWeapon == ch.RightHand && !attackWeapon.returning && (attackWeapon.skillType != Globals.eSkillType.Bow && !ch.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot)))
                    {
                        if (target.CurrentCell != null)
                            target.CurrentCell.Add(attackWeapon);
                        ch.UnequipRightHand(attackWeapon);

                    }
                    else if (ch.LeftHand != null && attackWeapon == ch.LeftHand && !attackWeapon.returning && (attackWeapon.skillType != Globals.eSkillType.Bow && !ch.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot)))
                    {
                        if (target.CurrentCell != null)
                            target.CurrentCell.Add(attackWeapon);
                        ch.UnequipLeftHand(attackWeapon);
                    }
                } 
                #endregion
            }
            else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Backstab))
            {
                ch.WriteToDisplay("Backstab hits for " + dmgAdjective + " damage!" + dmgDisplay);
                target.WriteToDisplay(ch.GetNameForActionResult() + " backstabs you with " + attackWeapon.shortDesc + "!");
            }
            else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Assassinate))
            {
                ch.WriteToDisplay("You have assassinated your target!" + dmgDisplay);
                target.WriteToDisplay("You have been assassinated by " + ch.GetNameForActionResult(true) + "!");
            }
            else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Shield_Bash))
            {
                ch.WriteToDisplay("Bash hits for " + dmgAdjective + " damage!" + dmgDisplay);
                target.WriteToDisplay(ch.GetNameForActionResult() + " bashes you with " + attackWeapon.shortDesc + "!");
            }
            else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Cleave))
            {
                ch.WriteToDisplay("Cleave hits for " + dmgAdjective + " damage!" + dmgDisplay);
                target.WriteToDisplay(ch.GetNameForActionResult() + " cleaves you with " + attackWeapon.shortDesc + "!");
            }
            else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.BattleCharge))
            {
                ch.WriteToDisplay("Battle Charge hits for " + dmgAdjective + " damage!" + dmgDisplay);
                target.WriteToDisplay(ch.GetNameForActionResult() + " battle charges you with " + attackWeapon.shortDesc + "!");
            }
            else if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Poke))
            {
                ch.WriteToDisplay("Poke hits for " + dmgAdjective + " damage!" + dmgDisplay);
                target.WriteToDisplay(ch.GetNameForActionResult() + " pokes you with " + attackWeapon.shortDesc + "!");
            }
            else
            {
                ch.WriteToDisplay("Swing hits for " + dmgAdjective + " damage!" + dmgDisplay);

                string weaponHitDesc = " hits with " + attackWeapon.shortDesc + "!";

                if (attackWeapon == ch.GetInventoryItem(Globals.eWearLocation.Hands))
                    weaponHitDesc = " punches you!";
                else if (attackWeapon == ch.GetInventoryItem(Globals.eWearLocation.Feet))
                {
                    if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Jumpkick))
                        weaponHitDesc = " jumpkicks you!";
                    if (ch.CommandsProcessed.Contains(CommandTasker.CommandType.Kick))
                        weaponHitDesc = "kicks you!";
                }

                target.WriteToDisplay(ch.GetNameForActionResult() + weaponHitDesc);
            }
        }
    }
}
