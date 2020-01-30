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
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using DragonsSpine.GameWorld;

namespace DragonsSpine
{
    public class Effect
    {
        protected static EffectTypes[] NoWornOffMessage = new EffectTypes[] 
        {
            EffectTypes.None,
            EffectTypes.Peek,
            EffectTypes.Hide_in_Shadows,
            EffectTypes.Drake_Potion,
            EffectTypes.Balm,
            EffectTypes.Image,
            //EffectTypes.Poison,
            EffectTypes.Stamina_Restore,
            EffectTypes.Mana_Restore,
            EffectTypes.Hello_Immobility
        };

        public enum EffectTypes : int
        {
            #region Effect Types
            None, // 0
            Fire,
            Ice,
            Darkness,
            Poison_Cloud,
            Web, // 5
            Light,
            Concussion,
            Find_Secret_Door,
            Illusion,
            Dragon__s_Breath_Fire, // 10
            Dragon__s_Breath_Ice,
            Turn_Undead,
            Whirlwind,
            Breathe_Water,
            Feather_Fall, // 15
            Night_Vision,
            Wizard_Eye,
            Peek,
            HitsMax,
            Hits, // 20
            Exp,
            Stamina,
            StamLeft, // Not used
            Permanent_Strength,
            Permanent_Dexterity, // 25
            Permanent_Intelligence,
            Permanent_Wisdom,
            Permanent_Constitution,
            Permanent_Charisma,
            Temporary_Strength, // 30
            Temporary_Dexterity,
            Temporary_Intelligence,
            Temporary_Wisdom,
            Temporary_Constitution,
            Temporary_Charisma, // 35
            Shield,
            Regenerate_Health,
            Regenerate_Mana, // 38
            Regenerate_Stamina,
            Protection_from_Fire, // 40
            Protection_from_Cold,
            Protection_from_Poison,
            Protection_from_Fire_and_Ice,
            Protection_from_Blind_and_Fear,
            Protection_from_Stun_and_Death, // 45
            Resist_Fear,
            Resist_Blind,
            Resist_Stun,
            Resist_Lightning,
            Resist_Death, // 50
            Resist_Zonk,
            Sacred_Ring,
            Bless,
            Blind,
            Fear, // 55
            Poison,
            Balm,
            Naphtha,
            Ale,
            Wine, // 60
            Beer,
            Coffee,
            Water,
            Youth_Potion,
            Drake_Potion, // 65
            Blindness_Cure,
            Mana_Restore,
            Stamina_Restore,
            Nitro,
            OrcBalm, // 70
            Permanent_Mana,
            Whiskey,
            Rum,
            Unlocked_Horizontal_Door,
            Unlocked_Vertical_Door, // 75
            Hide_in_Shadows,
            Flight,
            Find_Secret_Rockwall,
            Hide_Door, 
            MindControl, // 80 -- not implemented as of 8/7/2013
            Black_Fog,
            Lightning_Storm,
            Fire_Storm,
            Lava, 
            Lightning, // 85
            Hello_Immobility, // used to keep an NPC still for a random amount of time during PC-NPC interaction
            Animal_Affinity, // when you pet a dog - if player has this effect the player IsLucky
            Dog_Follow,
            Fireball, // only used to send fireball effect to client
            Flesh_to_Stone, // 90 // not currently used
            Blizzard,
            Speed,
            Flame_Shield,
            Image,
            Charm_Animal, // 95
            Protection_from_Undead,
            Command_Undead,
            Silence,
            Acid, // area effect and character effect (acid splash)
            Obfuscation, // 100
            Contagion,
            Ornic_Flame, // when item is thrown into an ornic flame it turns into dazzling tsavorite of approximate item value
            Dragon__s_Breath_Wind,
            Dragon__s_Breath_Storm,
            Radiant_Orb, // 105
            Locust_Swarm, // area effect
            Venom,
            Protection_from_Acid,
            Ferocity, // 109 -- improved spell and physical critical chance
            Savagery, // 110 -- improved weapon combat critical chance and damage multiplier
            Weapon_Proc, // this effect uses spell variables and is basically unlimited charges of a random proc determined in Rules.Combat -- effectDuration is spell level, effectAmount is spell ID 
            Ensnare, // 112
            Dragon__s_Breath_Poison,
            Dragon__s_Breath_Acid,
            Protection_from_Lightning, // 115
            Remove_Lesser_Curse, // 116
            Remove_Greater_Curse, // 117
            Shelter,
            Regeneration,
            Ataraxia, // 120
            Mark_of_Vitality,
            Fog, // area effect
            Thunderwave, // area effect
            Polymorph, // 10/2/2019 unused
            Shapeshift, // 125 10/2/2019 unused
            Hunter__s_Mark,
            Barkskin,
            Stoneskin,
            Detect_Undead, // 129
            Cynosure, // 130 debuff which increases critical damage on target and lowers base armor class depending on magic skill level
            Trochilidae, // 131 increase attack speed
            Protection_from_Hellspawn, // 132
            Bazymon__s_Bounty, // 133 xp gain bonus
            Lagniappe, // 134 skill gain bonus
            The_Withering, // 135 decreased xp gain
            Drudgery, // 136 decreased skill gain
            Gnostikos, // 137 clairvoyance -- see spells being cast, see beyond walls
            Cognoscere,
            Umbral_Form,
            Tempest,
            Faerie_Fire, // 141 lowers base AC by 2
            Juvenis, // regeneration of vitals every round as if meditating or resting
            Acumen, // initiative bonus
            Gumption, // initiative bonus, self only
            Quaesitum, // 145 animal friendship
            Walking_Death, // sorcerer spell ID 144
            #endregion
        }

        public const int HYBRID_MANA_MAX = 3;

        #region Private Data
        /// <summary>
        ///  Holds the EffectTypes enumeration value.
        /// </summary>
        protected EffectTypes m_effectType;

        /// <summary>
        /// Holds the power amount of the Effect.
        /// </summary>
        protected int m_power;

        /// <summary>
        /// Holds the duration, in game rounds, of the Effect.
        /// </summary>
        protected int m_duration;
        #endregion

        #region Public Properties
        /// <summary>
        /// Duration in game rounds using DragonsSpineMain.MasterRoundInterval.
        /// </summary>
        public int Duration
        {
            get { return m_duration; }
            set { m_duration = value; }
        }

        public EffectTypes EffectType
        {
            get { return m_effectType; }
        }

        public int Power
        {
            get { return m_power; }
            set { m_power = value; }
        }
        public Character Target
        { get; set; }
        public Character Caster // who cast this spell if the effect came from a caster, if not leave null
        { get; set; }
        protected System.Timers.Timer EffectTimer
        { get; set; }

        public bool IsPermanent
        {
            get { return Duration == -1; }
        }
        #endregion

        #region Constructors
        public Effect()
        {
        }

        // character effect
        public Effect(EffectTypes effectType, int amount, int duration, Character target, Character caster)
        {
            // duration 0 = instantaneous effect, already handled. -1 = permanent effect
            if (duration == 0 && effectType != EffectTypes.Hide_in_Shadows) return;

            m_effectType = effectType;
            m_power = amount;
            Duration = duration;
            Target = target;
            Caster = caster;
            lock (Target.EffectsList)
            {
                Target.EffectsList.TryAdd(EffectType, this);
            }
            EffectTimer = new System.Timers.Timer(DragonsSpineMain.MasterRoundInterval);
            EffectTimer.Elapsed += new System.Timers.ElapsedEventHandler(CharacterEffectEvent);
            EffectTimer.Start();
        }

        // worn effect
        public Effect(EffectTypes effectType, int power, Character ch)
        {
            m_effectType = effectType;
            m_power = power;
            Target = ch;
            Caster = ch;
        }
        #endregion

        private void CharacterEffectEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!Target.IsPC || Target.PCState == Globals.ePlayerState.PLAYING)
            {
                if (Duration > 0)
                    Duration--;

                try
                {
                    if (Duration == 0)
                    {
                        #region Duration is 0. Stop character effect (except Hide In Shadows).
                        if (EffectType == EffectTypes.Hide_in_Shadows)
                        {
                            if (Caster != null && Caster.IsPC)
                            {
                                if (Caster.IsHidden)
                                {
                                    Map.CheckHiddenStatus(Caster);
                                }
                                else
                                {
                                    StopCharacterEffect();
                                }
                            }
                        }
                        else
                        {
                            StopCharacterEffect();
                        } 
                        #endregion
                    }
                    else
                    {
                        if (Target != null && Target.IsDead)
                        {
                            StopCharacterEffect();
                        }
                        else
                        {
                            switch (EffectType)
                            {
                                case EffectTypes.Acid: // acid splash effect
                                    if (Power <= 0) StopCharacterEffect();
                                    else
                                    {
                                        if (Combat.DoSpellDamage(Caster, Target, null, Power + 1, "acid splash") == 1)
                                            Rules.GiveAEKillExp(Caster, Target);
                                        if (Target != null && !Target.IsDead) Power = Power / 2;
                                    }
                                    break;
                                case EffectTypes.Balm:
                                    DoBalmEffect(Target, this);
                                    break;
                                case EffectTypes.Poison:
                                    if (Power <= 0) StopCharacterEffect();
                                    else
                                    {
                                        if (Combat.DoSpellDamage(Caster, Target, null, Power + 1, "poison") == 1)
                                            Rules.GiveAEKillExp(Caster, Target);

                                        if (Target != null && !Target.IsDead) Power = Power / 2;
                                    }
                                    break;
                                case EffectTypes.Venom:
                                    Power--;
                                    Combat.DoDamage(Target, Caster, Power + 1, false);
                                    if (Power <= 0 || Target.IsDead)
                                        StopCharacterEffect();
                                    else
                                    {
                                        Target.SendToAllInSight(Target.GetNameForActionResult() + GameSystems.Text.TextManager.VENOM_PAINED_BROADCAST);
                                        if (Target.attackSound != "")
                                            Target.EmitSound(Target.attackSound);
                                    }
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Duration = 1;
                    Utils.LogException(ex);
                }
            }
        }        

        public void StopCharacterEffect()
        {
            try
            {
                EffectTimer.Stop();

                if (Target.IsImage) return;

                switch (EffectType)
                {
                    case EffectTypes.Stoneskin:
                        Target.baseArmorClass += Power;
                        break;
                    case EffectTypes.Barkskin:
                        Target.baseArmorClass += Power;
                        break;
                    case EffectTypes.Charm_Animal:
                        Target.canCommand = false;
                        Target.Alignment = NPC.CopyNPCFromDictionary((Target as NPC).npcID).Alignment;
                        if(Caster != null)
                        {
                            if (!Caster.IsDead && GameSystems.Targeting.TargetAquisition.FindTargetInView(Target, Caster) != null) Target.TargetID = Caster.UniqueID;
                            else if (Target.PetOwner != null) Target.TargetID = Target.PetOwner.UniqueID;

                            if(Caster.Pets.Contains(Target as NPC)) Caster.Pets.Remove(Target as NPC);
                        }
                        Target.PetOwner = null;
                        break;
                    case EffectTypes.Command_Undead:
                        Target.canCommand = false;
                        Target.Alignment = NPC.CopyNPCFromDictionary((Target as NPC).npcID).Alignment;
                        if (Caster != null)
                        {
                            if (!Caster.IsDead && GameSystems.Targeting.TargetAquisition.FindTargetInView(Target, Caster) != null) Target.TargetID = Caster.UniqueID;
                            else if (Target.PetOwner != null) Target.TargetID = Target.PetOwner.UniqueID;

                            if (Caster.Pets.Contains(Target as NPC)) Caster.Pets.Remove(Target as NPC);
                        }
                        Target.PetOwner = null;
                        break;
                    case EffectTypes.Cynosure:
                        Target.baseArmorClass -= Power;
                        break;
                    case EffectTypes.Speed:
                        break;
                    case EffectTypes.Dog_Follow:
                        this.Target.FollowID = 0;
                        break;
                    case EffectTypes.Peek:
                        if (Caster != null)
                        {
                            Caster.CurrentCell = this.Caster.MyClone.CurrentCell;
                            Caster.visualKey = Caster.MyClone.visualKey;
                            Caster.IsInvisible = this.Caster.MyClone.IsInvisible;
                            Caster.MyClone.RemoveFromWorld();
                            if (Caster.MyClone is PC && Character.ConfList.Contains(Caster.MyClone as PC))
                                (Caster.MyClone as PC).RemoveFromConf();
                            Caster.MyClone = null;
                        }
                        break;
                    case EffectTypes.Wizard_Eye:
                        Cell oldCell = this.Target.CurrentCell;
                        this.Target.CurrentCell = this.Target.MyClone.CurrentCell;
                        this.Target.MyClone.CurrentCell = null;
                        this.Target.Name = this.Target.MyClone.Name;
                        this.Target.RightHand = this.Target.MyClone.RightHand;
                        this.Target.LeftHand = this.Target.MyClone.LeftHand;
                        this.Target.Hits = this.Target.MyClone.Hits;
                        this.Target.HitsMax = this.Target.MyClone.HitsMax;
                        this.Target.HitsAdjustment = this.Target.MyClone.HitsAdjustment;
                        this.Target.Stamina = this.Target.MyClone.Stamina;
                        this.Target.StaminaMax = this.Target.MyClone.StaminaMax;
                        this.Target.StaminaAdjustment = this.Target.MyClone.StaminaAdjustment;
                        this.Target.Mana = this.Target.MyClone.Mana;
                        this.Target.ManaMax = this.Target.MyClone.ManaMax;
                        this.Target.ManaAdjustment = this.Target.MyClone.ManaAdjustment;
                        this.Target.animal = false;
                        PC.SetCharacterVisualKey(this.Target);
                        this.Target.EffectsList[EffectTypes.Hide_in_Shadows].StopCharacterEffect();
                        this.Target.MyClone.RemoveFromWorld();
                        this.Target.MyClone = null;
                        break;
                    case EffectTypes.Protection_from_Acid:
                        this.Target.AcidResistance--;
                        this.Target.AcidProtection -= Power;
                        break;
                    case EffectTypes.Protection_from_Fire:
                        this.Target.FireResistance--;
                        this.Target.FireProtection -= Power;
                        break;
                    case EffectTypes.Protection_from_Cold:
                        this.Target.ColdResistance--;
                        this.Target.ColdProtection -= Power;
                        break;
                    case EffectTypes.Protection_from_Lightning:
                        this.Target.LightningResistance--;
                        this.Target.LightningProtection -= Power;
                        break;
                    case EffectTypes.HitsMax:
                        Target.HitsMax -= Power;
                        break;
                    case EffectTypes.Hits:
                        Target.Hits -= Power;
                        break;
                    case EffectTypes.Stamina:
                        Target.StaminaMax -= Power;
                        break;
                    case EffectTypes.Temporary_Strength:
                        Target.TempStrength -= Power;
                        break;
                    case EffectTypes.Temporary_Dexterity:
                        this.Target.TempDexterity -= Power;
                        break;
                    case EffectTypes.Temporary_Intelligence:
                        this.Target.TempIntelligence -= Power;
                        break;
                    case EffectTypes.Temporary_Wisdom:
                        this.Target.TempWisdom -= Power;
                        break;
                    case EffectTypes.Temporary_Constitution:
                        this.Target.TempConstitution -= Power;
                        break;
                    case EffectTypes.Temporary_Charisma:
                        this.Target.TempCharisma -= Power;
                        break;
                    case EffectTypes.Protection_from_Fire_and_Ice:
                        this.Target.FireResistance--;
                        this.Target.FireProtection -= Power;
                        this.Target.ColdResistance--;
                        this.Target.ColdProtection -= Power;
                        break;
                    case EffectTypes.Shield:
                        this.Target.Shielding -= Power;
                        break;
                    case EffectTypes.Resist_Fear:
                        this.Target.FearResistance--;
                        break;
                    case EffectTypes.Resist_Blind:
                        this.Target.BlindResistance--;
                        break;
                    case EffectTypes.Resist_Lightning:
                        this.Target.LightningResistance--;
                        break;
                    case EffectTypes.Protection_from_Poison:
                        this.Target.PoisonResistance--;
                        this.Target.PoisonProtection -= Power;
                        break;
                    case EffectTypes.Resist_Stun:
                        this.Target.StunResistance--;
                        break;
                    case EffectTypes.Resist_Death:
                        this.Target.DeathProtection -= Power;
                        break;
                    case EffectTypes.Protection_from_Blind_and_Fear:
                        this.Target.BlindResistance--;
                        this.Target.FearResistance--;
                        break;
                    case EffectTypes.Protection_from_Stun_and_Death:
                        this.Target.StunResistance--;
                        this.Target.DeathResistance--;
                        this.Target.DeathProtection -= this.Power;
                        break;
                    case EffectTypes.Bless:
                        if (Target.EffectsList.ContainsKey(EffectTypes.Bless))
                        {
                            int blessPower = Target.EffectsList[EffectTypes.Bless].Power;
                            Target.Shielding -= blessPower;
                            Target.TempDexterity -= blessPower;
                            Target.TempConstitution -= blessPower;
                            Target.hitsRegen -= blessPower;
                            Target.manaRegen -= blessPower;
                            Target.staminaRegen -= blessPower;
                        }
                        else Utils.Log("Error at Effect.StopCharacterEffect for EffectType.Bless. EffectType does not exist in Character's spell effects.", Utils.LogType.SystemWarning);
                        break;
                    case EffectTypes.Poison:
                        if (Target.EffectsList.ContainsKey(Effect.EffectTypes.Poison))
                            Target.EffectsList.TryRemove(Effect.EffectTypes.Poison, out Effect outPoison);
                        Target.Poisoned = 0;
                        break;
                    case EffectTypes.Balm: // healing over time
                        if (this.Target.IsDead)
                            break;
                        if (this.Power > 0)
                        {
                            this.Target.Hits += this.Power;
                            if (this.Target.Hits > this.Target.HitsFull)
                                this.Target.Hits = this.Target.HitsFull;
                        }
                        break;
                    default:
                        break;
                }


                if (Target.EffectsList.ContainsKey(this.EffectType))
                    Target.EffectsList.TryRemove(this.EffectType, out Effect outEffect);

                if (Target.PCState == Globals.ePlayerState.PLAYING || !Target.IsPC)
                {
                    string effectName = GetEffectName(EffectType);

                    if (!Target.IsDead)
                    {
                        //TODO: devise a better way to restrict certain effects from arriving here
                        if (!effectName.StartsWith("temporary") && !effectName.StartsWith("permanent") &&
                            Array.IndexOf(Effect.NoWornOffMessage, (Effect.EffectTypes)this.EffectType) == -1)
                        {
                            if (effectName.StartsWith("The "))
                                Target.WriteToDisplay(effectName + " spell has worn off.");
                           else Target.WriteToDisplay("The " + effectName + " spell has worn off.");

                            if (Caster != null && Caster.IsPC && !Target.IsDead)
                            {
                                PC pc = PC.GetOnline(Caster.UniqueID);

                                if (pc != null && pc.PCState == Globals.ePlayerState.PLAYING && pc.UniqueID != Target.UniqueID && effectName != "acid")
                                {
                                    if (effectName.StartsWith("The "))
                                        pc.WriteToDisplay(effectName + " spell has worn off of " + Target.GetNameForActionResult(true) + ".");
                                    else
                                        pc.WriteToDisplay("Your " + GetEffectName(EffectType) + " spell has worn off of " + Target.GetNameForActionResult(true) + ".");
                                }
                            }
                        }
                    }
                }

                if (Target != null && Target is PC && (Target as PC).protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                    ProtocolYuusha.SendCharacterEffects(Target);
            }
            catch (Exception e)
            {
                Utils.Log("Failure at StopCharacterEffect Effect: " + this.EffectType.ToString(), Utils.LogType.SystemFailure);
                Utils.LogException(e);
            }
        }

        public static void CreateCharacterEffect(EffectTypes effectType, int effectAmount, Character target, int duration, Character caster)
        {
            try
            {
                if (target.Name.StartsWith(Spells.GameSpell.IMAGE_IDENTIFIER) && effectType != EffectTypes.Image)
                    return;

                // currently no stacking of spells like damage over time venom/poison
                if (target.EffectsList.ContainsKey(effectType)) // the target already has this effect type
                {
                    if (target.EffectsList[effectType].Power <= effectAmount)
                    {
                        target.EffectsList[effectType].Power = effectAmount;
                        target.EffectsList[effectType].Duration = duration;
                        target.EffectsList[effectType].Caster = caster;

                        if (target != null && target is PC && (target as PC).protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                            ProtocolYuusha.SendCharacterEffects(target);
                    }
                    return;
                }

                Effect effect = new Effect(effectType, effectAmount, duration, target, caster);

                switch (effectType)
                {
                    case EffectTypes.Stoneskin:
                        target.baseArmorClass -= effectAmount;
                        break;
                    case EffectTypes.Barkskin:
                        target.baseArmorClass -= effectAmount;
                        break;
                    case EffectTypes.Cynosure:
                        target.baseArmorClass += effectAmount;
                        break;
                    case EffectTypes.Dog_Follow:
                        target.FollowID = caster.UniqueID;
                        effect.Caster = null;
                        break;
                    #region Strictly Bottle Effects
                    case EffectTypes.Ale:
                        target.SendToAllInSight(target.Name + " burps loudly.");
                        break;
                    case EffectTypes.Balm: // healing over time
                        Effect.DoBalmEffect(target, target.EffectsList[effectType]);
                        break;
                    case EffectTypes.Beer:
                        target.SendToAllInSight(target.Name + " burps.");
                        break;
                    case EffectTypes.Blindness_Cure:
                        if (target.IsBlind)
                        {
                            if (target.EffectsList.ContainsKey(EffectTypes.Blind))
                            {
                                target.EffectsList[EffectTypes.Blind].StopCharacterEffect();
                                target.WriteToDisplay("Your blindness has been cured!");
                            }
                        }
                        break;
                    case EffectTypes.Charm_Animal:
                        if (target is NPC && target.Group != null)
                            target.Group.Remove(target as NPC);
                        target.canCommand = true;
                        target.PetOwner = caster;
                        target.Alignment = caster.Alignment;
                        target.FlaggedUniqueIDs.Clear(); // clear all flagged player IDs
                        caster.Pets.Add(target as NPC);
                        break;
                    case EffectTypes.Coffee:
                        target.Stamina += effectAmount;
                        break;
                    case EffectTypes.Command_Undead:
                        if (target is NPC && target.Group != null)
                            target.Group.Remove(target as NPC);
                        target.canCommand = true;
                        target.PetOwner = caster;
                        target.Alignment = caster.Alignment;
                        caster.Pets.Add(target as NPC);
                        break;
                    case EffectTypes.Drake_Potion:
                        if (target.Constitution < target.Land.MaxAbilityScore) // add a constitution point
                            target.Constitution++;
                        int hitsLimit = Rules.GetMaximumHits(target); // get hits limit for class type
                        int staminaLimit = Rules.GetMaximumStamina(target); // get stamina limit for class type
                        target.HitsMax += effectAmount; // add to hits
                        target.StaminaMax += effectAmount; // add to stamina
                        if (target.HitsMax > hitsLimit) target.HitsMax = hitsLimit; // adjust to hits limit if necessary
                        if (target.StaminaMax > staminaLimit) target.StaminaMax = staminaLimit; // adjust to stamina limit if necessary
                        target.Hits = target.HitsFull; // hits to full
                        target.Stamina = target.StaminaFull; // stamina to full
                        target.Mana = target.ManaFull; // mana to full
                        break;
                    case EffectTypes.Mana_Restore:
                        target.Mana = target.ManaFull;
                        break;
                    case EffectTypes.Naphtha:
                        target.Poisoned = effectAmount;
                        break;
                    case EffectTypes.Nitro:
                        // TODO  - mlt if you get hit during duration,,or run into a wall, you explode.
                        break;
                    case EffectTypes.OrcBalm:
                        target.Stunned = (short)Rules.RollD(1, effectAmount);
                        break;
                    case EffectTypes.Permanent_Mana:
                        int manaLimit = Rules.GetMaximumMana(target); // get mana limit for class type
                        target.ManaMax += effectAmount; // add to mana
                        if (target.ManaMax > manaLimit)
                        {
                            target.ManaMax = manaLimit; // adjust to mana limit if necessary
                        }
                        target.Mana = target.ManaFull; // mana to full
                        break;
                    case EffectTypes.Stamina_Restore:
                        target.Stamina = target.StaminaFull;
                        break;
                    case EffectTypes.Water:
                        break;
                    case EffectTypes.Wine:
                        break;
                    case EffectTypes.Youth_Potion:
                        if (target.Age > World.AgeCycles[0])
                        {
                            target.Age = World.AgeCycles[0];
                            target.WriteToDisplay("You feel young again!");
                        }
                        else
                        {
                            target.WriteToDisplay("The fluid is extremely bitter.");
                        }
                        break;
                    #endregion
                    case EffectTypes.HitsMax:
                        target.HitsMax += effectAmount;
                        break;
                    case EffectTypes.Hits:
                        target.Hits += effectAmount;
                        break;
                    case EffectTypes.Stamina:
                        target.StaminaMax += effectAmount;
                        break;
                    case EffectTypes.Poison:
                        target.Poisoned = effectAmount;
                        if (target is NPC && target.Group != null)
                        {
                            target.Group.Remove(target as NPC);
                        }
                        break;
                    #region Permanent Stats
                    case EffectTypes.Permanent_Strength:
                        target.Strength += effectAmount;
                        if (target.Strength > target.Land.MaxAbilityScore)
                        {
                            target.Strength = target.Land.MaxAbilityScore;
                        }
                        target.Stunned = 3;
                        break;
                    case EffectTypes.Permanent_Dexterity:
                        target.Dexterity += effectAmount;
                        Effect.CreateCharacterEffect(EffectTypes.Blind, 0, target, 3, null);
                        if (target.Dexterity > target.Land.MaxAbilityScore)
                        {
                            target.Dexterity = target.Land.MaxAbilityScore;
                        }
                        break;
                    case EffectTypes.Permanent_Intelligence:
                        target.Intelligence += effectAmount;
                        if (target.Intelligence > target.Land.MaxAbilityScore)
                        {
                            target.Intelligence = target.Land.MaxAbilityScore;
                        }
                        break;
                    case EffectTypes.Permanent_Wisdom:
                        target.Wisdom += effectAmount;
                        if (target.Wisdom > target.Land.MaxAbilityScore)
                        {
                            target.Wisdom = target.Land.MaxAbilityScore;
                        }
                        break;
                    case EffectTypes.Permanent_Constitution:
                        target.Constitution += effectAmount;
                        if (target.Constitution > target.Land.MaxAbilityScore)
                        {
                            target.Constitution = target.Land.MaxAbilityScore;
                        }
                        break;
                    case EffectTypes.Permanent_Charisma:
                        target.Charisma += effectAmount;
                        if (target.Charisma > target.Land.MaxAbilityScore)
                        {
                            target.Charisma = target.Land.MaxAbilityScore;
                        }
                        break;
                    #endregion
                    #region Temporary Stats
                    case EffectTypes.Temporary_Strength:
                        target.TempStrength += effectAmount;
                        target.EmitSound(DragonsSpine.Spells.GameSpell.GetSpell("strength").SoundFile);
                        break;
                    case EffectTypes.Temporary_Dexterity:
                        target.TempDexterity += effectAmount;
                        break;
                    case EffectTypes.Temporary_Intelligence:
                        target.TempIntelligence += effectAmount;
                        break;
                    case EffectTypes.Temporary_Wisdom:
                        target.TempWisdom += effectAmount;
                        break;
                    case EffectTypes.Temporary_Constitution:
                        target.TempConstitution += effectAmount;
                        break;
                    case EffectTypes.Temporary_Charisma:
                        target.TempCharisma += effectAmount;
                        break;
                    #endregion
                    case EffectTypes.Shield:
                        target.Shielding += effectAmount;
                        break;
                    #region Increase Protection
                    case EffectTypes.Protection_from_Acid:
                        target.AcidResistance++;
                        target.AcidProtection += effectAmount;
                        break;
                    case EffectTypes.Protection_from_Fire:
                        target.FireResistance++;
                        target.FireProtection += effectAmount;
                        break;
                    case EffectTypes.Protection_from_Cold:
                        target.ColdResistance++;
                        target.ColdProtection += effectAmount;
                        break;
                    case EffectTypes.Protection_from_Lightning:
                        target.LightningResistance++;
                        target.LightningProtection += effectAmount;
                        break;
                    case EffectTypes.Protection_from_Fire_and_Ice:
                        target.FireResistance++;
                        target.FireProtection += effectAmount;
                        target.ColdResistance++;
                        target.ColdProtection += effectAmount;
                        break;
                    case EffectTypes.Protection_from_Poison:
                        target.PoisonResistance++;
                        target.PoisonProtection += effectAmount;
                        break;
                    case EffectTypes.Protection_from_Blind_and_Fear:
                        target.BlindResistance++;
                        target.FearResistance++;
                        break;
                    case EffectTypes.Protection_from_Stun_and_Death:
                        target.StunResistance++;
                        target.DeathResistance++;
                        target.DeathProtection += effectAmount;
                        break;
                    #endregion
                    #region Increase Resist
                    case EffectTypes.Resist_Fear:
                        target.FearResistance++;
                        break;
                    case EffectTypes.Resist_Blind:
                        target.BlindResistance++;
                        break;
                    case EffectTypes.Resist_Stun:
                        target.StunResistance++;
                        break;
                    case EffectTypes.Resist_Lightning:
                        target.LightningResistance++;
                        break;
                    case EffectTypes.Resist_Death:
                        target.DeathResistance++;
                        break;
                    #endregion
                    case EffectTypes.Bless:
                        target.Shielding += effectAmount;
                        target.TempStrength += effectAmount;
                        target.TempDexterity += effectAmount;
                        target.TempConstitution += effectAmount;
                        target.hitsRegen += effectAmount;
                        target.manaRegen += effectAmount;
                        target.staminaRegen += effectAmount;
                        break;
                    case EffectTypes.Peek:
                        if (caster != null)
                        {
                            caster.MyClone = caster.CloneCharacter();
                            caster.MyClone.AddToWorld();
                            caster.MyClone.IsPC = false;
                            caster.IsInvisible = true;
                            caster.visualKey = "";

                            if (target != null && target.CurrentCell != null)
                                caster.CurrentCell = target.CurrentCell;

                            //effect.StopCharacterEffect();

                            // the Peek effect does not remain in the target's effectsList
                            //if (target.EffectsList.ContainsKey(EffectTypes.Peek))
                            //    target.EffectsList.TryRemove(EffectTypes.Peek, out effect);

                            //Effect peakEffect = new Effect(effect.EffectType, effect.Power, effect.Duration, caster, caster);
                        }
                        break;
                    #region WizardEye
                    case EffectTypes.Wizard_Eye:
                        target.MyClone = target.CloneCharacter();
                        target.MyClone.IsPC = false;
                        Effect.CreateCharacterEffect(EffectTypes.Wizard_Eye, 0, target.MyClone, duration, target);
                        Effect.CreateCharacterEffect(EffectTypes.Hide_in_Shadows, 0, target, -1, target);
                        target.MyClone.CurrentCell = target.CurrentCell;
                        target.MyClone.AddToWorld();
                        switch(target.BaseProfession)
                        {
                            case Character.ClassType.Thief:
                                target.Name = "rat";
                                break;
                            case Character.ClassType.Druid:
                                target.Name = "beetle";
                                break;
                            default: // wizards
                                target.Name = "toad";
                                break;
                        }
                        PC.SetCharacterVisualKey(target);
                        target.RightHand = null;
                        target.LeftHand = null;
                        target.Hits = 3;
                        target.HitsMax = 3;
                        target.HitsAdjustment = 0;
                        target.Stamina = 3;
                        target.StaminaMax = 3;
                        target.StaminaAdjustment = 0;
                        target.Mana = 0;
                        target.ManaMax = 0;
                        target.ManaAdjustment = 0;
                        target.animal = true;
                        break;
                    #endregion
                    case EffectTypes.Fear:
                        if (target is NPC && target.Group != null)
                        {
                            target.Group.Remove(target as NPC);
                        }

                        if (target.preppedSpell != null) // lose spell
                        {
                            bool keepWarmedSpell = true;

                            if (target.IsSpellWarmingProfession)
                            {
                                if (target.IsIntelligenceCaster) keepWarmedSpell = Rules.FullStatCheck(target, Globals.eAbilityStat.Intelligence);
                                else if (target.IsWisdomCaster) keepWarmedSpell = Rules.FullStatCheck(target, Globals.eAbilityStat.Wisdom);
                            }
                            else keepWarmedSpell = Rules.RollD(1, 100) >= 50; // 50% chance to keep it

                            if (keepWarmedSpell)
                            {
                                target.preppedSpell = null;
                                target.WriteToDisplay("You have lost your warmed spell.");
                                target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                            }
                        }
                        break;
                    case EffectTypes.Blind:
                        if (target is NPC && target.Group != null)
                            target.Group.Remove(target as NPC);
                        break;
                    case EffectTypes.Silence:
                        if(target.PCState == Globals.ePlayerState.PLAYING)
                            target.WriteToDisplay("You have been silenced!");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                if (target != null && caster != null)
                {
                    target.EffectsList.Clear(); // Clear all effects on the target.
                    Utils.Log("Effect.CreateCharacterEffect(): [Character: " + target.Name + "] [Caster: " + caster.Name + "] [Effect Name: " + effectType.ToString() + "] - killed effects", Utils.LogType.SystemWarning);
                }
                Utils.LogException(e);
            }

            if (target != null && target is PC && (target as PC).protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
            {
                if(duration != 0 || effectType == EffectTypes.Hide_in_Shadows)
                    ProtocolYuusha.SendCharacterEffects(target);

                ProtocolYuusha.SendCharacterStats(target as PC, target);
            }
        }

        /// <summary>
        /// Adds worn OR held item effects to a character.
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="item"></param>
        public static void AddWornEffectToCharacter(Character ch, Item item)
        {
            // As of 1/25/17 only spell uses charges. Effects will not use charges.
            if (item.effectType.Length > 0 && item.effectType != "" && item.effectType != "0")// && (item.charges == -1 || item.charges > 0))
            {
                try
                {
                    string[] itemEffectType = item.effectType.Split(" ".ToCharArray());
                    string[] itemEffectAmount = item.effectAmount.Split(" ".ToCharArray());
                    for (int a = 0; a < itemEffectType.Length; a++)
                    {
                        Effect effect = new Effect((EffectTypes)Convert.ToInt32(itemEffectType[a]), Convert.ToInt32(itemEffectAmount[a]), ch);

                        if (effect.EffectType != EffectTypes.None && effect.EffectType != EffectTypes.Weapon_Proc)
                        {
                            switch (effect.EffectType)
                            {
                                #region EffectType Switch
                                case EffectTypes.Mark_of_Vitality:
                                case EffectTypes.Regenerate_Stamina:
                                    ch.staminaRegen += effect.Power;
                                    break;
                                case EffectTypes.Regeneration:
                                case EffectTypes.Regenerate_Health:
                                    ch.hitsRegen += effect.Power;
                                    break;
                                case EffectTypes.Ataraxia:
                                case EffectTypes.Regenerate_Mana:
                                    ch.manaRegen += effect.Power;
                                    break;
                                case EffectTypes.HitsMax:
                                    ch.HitsMax += effect.Power;
                                    break;
                                case EffectTypes.Hits:
                                    ch.Hits += effect.Power;
                                    break;
                                case EffectTypes.Stamina:
                                    ch.StaminaMax += effect.Power;
                                    break;
                                #region Temporary Stats
                                case EffectTypes.Temporary_Strength:
                                    ch.TempStrength += effect.Power;
                                    break;
                                case EffectTypes.Temporary_Dexterity:
                                    ch.TempDexterity += effect.Power;
                                    break;
                                case EffectTypes.Temporary_Intelligence:
                                    ch.TempIntelligence += effect.Power;
                                    break;
                                case EffectTypes.Temporary_Wisdom:
                                    ch.TempWisdom += effect.Power;
                                    break;
                                case EffectTypes.Temporary_Constitution:
                                    ch.TempConstitution += effect.Power;
                                    break;
                                case EffectTypes.Temporary_Charisma:
                                    ch.TempCharisma += effect.Power;
                                    break;
                                #endregion
                                #region Increase Protection
                                case EffectTypes.Protection_from_Acid:
                                    ch.AcidResistance++;
                                    ch.AcidProtection += effect.Power;
                                    break;
                                case EffectTypes.Protection_from_Fire:
                                    ch.FireResistance++;
                                    ch.FireProtection += effect.Power;
                                    break;
                                case EffectTypes.Protection_from_Cold:
                                    ch.ColdResistance++;
                                    ch.ColdProtection += effect.Power;
                                    break;
                                case EffectTypes.Protection_from_Fire_and_Ice:
                                    ch.FireResistance++;
                                    ch.FireProtection += effect.Power;
                                    ch.ColdResistance++;
                                    ch.ColdProtection += effect.Power;
                                    break;
                                case EffectTypes.Protection_from_Poison:
                                    ch.PoisonResistance++;
                                    ch.PoisonProtection += effect.Power;
                                    break;
                                case EffectTypes.Protection_from_Blind_and_Fear:
                                    ch.BlindResistance += effect.Power;
                                    ch.FearResistance += effect.Power;
                                    break;
                                case EffectTypes.Protection_from_Stun_and_Death:
                                    ch.StunResistance++;
                                    ch.DeathResistance++;
                                    ch.DeathProtection += effect.Power;
                                    break;
                                #endregion
                                #region Increase Resist
                                case EffectTypes.Resist_Fear:
                                    ch.FearResistance += effect.Power;
                                    break;
                                case EffectTypes.Resist_Blind:
                                    ch.BlindResistance += effect.Power;
                                    break;
                                case EffectTypes.Resist_Stun:
                                    ch.StunResistance += effect.Power;
                                    break;
                                case EffectTypes.Resist_Lightning:
                                    ch.LightningResistance += effect.Power;
                                    break;
                                case EffectTypes.Resist_Death:
                                    ch.DeathResistance += effect.Power;
                                    break;
                                case EffectTypes.Resist_Zonk:
                                    ch.ZonkResistance += effect.Power;
                                    break;
                                #endregion
                                case EffectTypes.Shield:
                                    ch.Shielding += effect.Power;
                                    break;
                                case EffectTypes.Sacred_Ring:
                                    if (ch.IsPC && item.attunedID == ch.UniqueID)
                                    {
                                        ch.ManaMax = HYBRID_MANA_MAX;
                                        ch.Mana = 0;
                                    }
                                    else if (!ch.IsPC)
                                    {
                                        ch.ManaMax = HYBRID_MANA_MAX;
                                        ch.Mana = 0;
                                    }
                                    break;
                                default:
                                    break;
                                    #endregion
                            }

                            ch.WornEffectsList.Add(effect);
                        }
                    }

                    if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol && ch.PCState == Globals.ePlayerState.PLAYING)
                    {
                        ProtocolYuusha.SendCharacterStats(ch as PC, ch);
                    }

                    //if (item.charges > 0 && item.charges < Item.NUM_FOR_UNLIMITED_CHARGES)
                    //{
                    //    item.charges -= 1;
                    //}
                }
                catch (Exception e)
                {
                    Utils.Log("Failure at Effect.AddWornEffectToCharacter(" + ch.GetLogString() + ", " + item.GetLogString() + ")", Utils.LogType.SystemFailure);
                    Utils.LogException(e);
                }

                if (ch is PC && (ch as PC).protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                    ProtocolYuusha.SendCharacterWornEffects(ch);
            }
        }

        public static void RemoveWornEffectFromCharacter(Character ch, Item item)
        {
            if (item.effectType.Length > 0 && item.effectType != "0")
            {
                try
                {
                    string[] itemEffectType = item.effectType.Split(" ".ToCharArray());
                    string[] itemEffectAmount = item.effectAmount.Split(" ".ToCharArray());
                    for (int a = 0; a < itemEffectType.Length; a++)
                    {
                        foreach (Effect effect in new List<Effect>(ch.WornEffectsList))
                        {
                            if (effect.EffectType == (EffectTypes)Convert.ToInt32(itemEffectType[a]) &&
                                effect.Power == Convert.ToInt32(itemEffectAmount[a]))
                            {
                                ch.WornEffectsList.Remove(effect);
                                break;
                            }
                        }
                        switch ((EffectTypes)Convert.ToInt32(itemEffectType[a]))
                        {
                            case EffectTypes.Resist_Zonk:
                                ch.ZonkResistance -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Mark_of_Vitality:
                            case EffectTypes.Regenerate_Stamina:
                                ch.staminaRegen -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Regeneration:
                            case EffectTypes.Regenerate_Health:
                                ch.hitsRegen -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Ataraxia:
                            case EffectTypes.Regenerate_Mana:
                                ch.manaRegen -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Protection_from_Acid:
                                ch.AcidResistance--;
                                ch.AcidProtection -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Protection_from_Fire:
                                ch.FireResistance--;
                                ch.FireProtection -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Protection_from_Cold:
                                ch.ColdResistance--;
                                ch.ColdProtection -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Protection_from_Lightning:
                                ch.LightningResistance--;
                                ch.LightningProtection -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.HitsMax:
                                ch.HitsMax -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Hits:
                                ch.Hits -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Stamina:
                                ch.StaminaMax -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Temporary_Strength:
                                ch.TempStrength -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Temporary_Dexterity:
                                ch.TempDexterity -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Temporary_Intelligence:
                                ch.TempIntelligence -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Temporary_Wisdom:
                                ch.TempWisdom -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Temporary_Constitution:
                                ch.TempConstitution -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Temporary_Charisma:
                                ch.TempCharisma -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Protection_from_Fire_and_Ice:
                                ch.FireResistance--;
                                ch.FireProtection -= Convert.ToInt32(itemEffectAmount[a]);
                                ch.ColdResistance--;
                                ch.ColdProtection -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Shield:
                                ch.Shielding -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Protection_from_Poison:
                                ch.PoisonResistance--;
                                ch.PoisonProtection -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Protection_from_Blind_and_Fear:
                                ch.BlindResistance -= Convert.ToInt32(itemEffectAmount[a]);
                                ch.FearResistance -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Protection_from_Stun_and_Death:
                                ch.StunResistance--;
                                ch.DeathResistance--;
                                ch.DeathProtection -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Resist_Fear:
                                ch.FearResistance -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Resist_Blind:
                                ch.BlindResistance -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Resist_Stun:
                                ch.StunResistance -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Resist_Lightning:
                                ch.LightningResistance -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Resist_Death:
                                ch.DeathResistance -= Convert.ToInt32(itemEffectAmount[a]);
                                break;
                            case EffectTypes.Sacred_Ring:
                                ch.ManaMax = 0;
                                ch.Mana = 0;
                                break;
                            case EffectTypes.Umbral_Form:
                                foreach(EffectTypes ef in ch.EffectsList.Keys)
                                    ch.EffectsList[ef].StopCharacterEffect();
                                break;
                            default:
                                break;
                        }//end switch
                    }//end for
                }//end try
                catch (Exception e)
                {
                    Utils.Log("Failure at Effect.RemoveWornEffectToCharacter(" + ch.GetLogString() + ", " + item.GetLogString() + ")", Utils.LogType.SystemFailure);
                    Utils.LogException(e);
                }

                if (ch is PC && (ch as PC).protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                    ProtocolYuusha.SendCharacterWornEffects(ch);
            }
        }

        public static string GetEffectName(EffectTypes effectType) // returns the effect name, or an empty string if none
        {
            return Utils.FormatEnumString(effectType.ToString());
        }

        protected static void DoBalmEffect(Character target, Effect effect)
        {
            // break out of here if the target died while drinking a balm
            if (target.IsDead)
            {
                effect.StopCharacterEffect();
                return;
            }

            try
            {
                int balmAmount = effect.Power / 2;
                if (target.EffectsList.ContainsKey(EffectTypes.Contagion))
                    balmAmount = balmAmount / 4;

                effect.Power -= balmAmount;

                target.Hits += balmAmount;

                if (target.Hits >= target.HitsFull || effect.Duration <= 0 || effect.Power <= 0)
                {
                    target.Hits = target.HitsFull;
                    effect.StopCharacterEffect();
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        /// <summary>
        /// This is always called when fire is in a cell. Burn flammable items, destroy webs.
        /// </summary>
        /// <param name="cell">The affected cell.</param>
        /// <param name="effect">The type of fire effect.</param>
        protected static void BurnFlammables(Cell cell, Effect effect)
        {
            if (cell.AreaEffects.ContainsKey(EffectTypes.Web))
                cell.AreaEffects[EffectTypes.Web].StopAreaEffect();

            foreach(Item flammableItem in new List<Item>(cell.Items))
            //for (int a = 0; a < cell.Items.Count; a++)
            {
                //Item flammableItem = cell.Items[a];

                try
                {
                    Combat.SavingThrow itemSavingThrow = Combat.SavingThrow.Spell;
                    int savingThrowMod = 0;

                    if (flammableItem.effectType.Length > 0)
                    {
                        savingThrowMod += flammableItem.effectType.Length;
                    }

                    if (cell.IsLair)
                        savingThrowMod += 5;

                    if (effect.EffectType == EffectTypes.Dragon__s_Breath_Fire)
                        itemSavingThrow = Combat.SavingThrow.BreathWeapon;

                    if (flammableItem.flammable && (flammableItem is Corpse || !Combat.DND_CheckSavingThrow(null, itemSavingThrow, savingThrowMod)))
                    {
                        if (flammableItem is Corpse)
                        {
                            Corpse.DumpCorpse(flammableItem as Corpse, cell);

                            if ((flammableItem as Corpse).IsPlayerCorpse)
                            {
                                Character player = (flammableItem as Corpse).Ghost;
                                player.WriteToDisplay("Your corpse burns and smolders, rapidly turning into naught but ash and a grease stain. You feel a tugging sensation as the world around your ghost form twists and spins wildly.");
                                Rules.EnterUnderworld(player as PC);
                            }
                        }

                        cell.Remove(flammableItem);
                    } // end if flammable
                }
                catch (Exception e)
                {
                    Utils.Log("Failure at Effect.BurnFlammableItems EffectType: " + effect.EffectType.ToString(), Utils.LogType.SystemFailure);
                    Utils.LogException(e);
                }
            }
        }

        /// <summary>
        /// This is always called when acid is in a cell. Melt objects, including gold coins. Disfigure corpses.
        /// </summary>
        /// <param name="cell">The affected cell.</param>
        /// <param name="effect">The type of fire effect.</param>
        protected static void AcidMelting(Cell cell, Effect effect)
        {
            if (cell.AreaEffects.ContainsKey(EffectTypes.Web))
                cell.AreaEffects[EffectTypes.Web].StopAreaEffect();

            double coinCount = 0;

            for (int a = 0; a < cell.Items.Count; a++)
            {
                Item meltedItem = cell.Items[a];

                try
                {
                    Combat.SavingThrow itemSavingThrow = Combat.SavingThrow.Spell;
                    int savingThrowMod = 0;

                    if (meltedItem.effectType.Length > 0)
                        savingThrowMod += meltedItem.effectType.Length;

                    if (cell.IsLair)
                        savingThrowMod += 5;

                    if (meltedItem.flammable && (meltedItem is Corpse || !Combat.DND_CheckSavingThrow(null, itemSavingThrow, savingThrowMod)))
                    {
                        if (meltedItem is Corpse)
                        {
                            Corpse.DumpCorpse(meltedItem as Corpse, cell);

                            if ((meltedItem as Corpse).IsPlayerCorpse)
                            {
                                Character player = (meltedItem as Corpse).Ghost;
                                player.WriteToDisplay("Your corpse sizzles and melts, rapidly turning into naught but a grease stain. You feel a tugging sensation as the world around your ghost form twists and spins wildly.");
                                Rules.EnterUnderworld(player as PC);
                            }
                        }

                        cell.Remove(meltedItem);
                    } // end if flammable

                    if (meltedItem.itemType == Globals.eItemType.Coin)
                    {
                        coinCount += meltedItem.coinValue;
                        cell.Remove(meltedItem);
                    }
                }
                catch (Exception e)
                {
                    Utils.Log("Failure at Effect.BurnFlammableItems EffectType: " + effect.EffectType.ToString(), Utils.LogType.SystemFailure);
                    Utils.LogException(e);
                }
            }

            if (coinCount > 0)
            {

            }
        }
    }
}
