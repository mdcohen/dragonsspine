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
using System.Text;
using DragonsSpine.GameWorld;

namespace DragonsSpine
{
    public static class Skills
    {
        public const int MAX_SKILL_LEVEL = 40; // is this even necessary, or can we have unlimited skill levels?
        public const int SKILL_XP_LEVEL_CAP = 19; // this is when skill experience req's remain the same per skill level
        public const int SKILL_GAIN_HARD_CAP_MULTIPLIER = 100;

        #region Skill Level Titles
        public static List<string> unarmedLvlName = new List<string>()
        {
            #region Martial Arts Skill Levels
		    "Untrained", // 0
            "White Belt",
            "Yellow Belt",
            "Green Belt",
            "Blue Belt",
            "Red Belt",
            "Black Belt", // 6
            "1st Dan",
            "2nd Dan",
            "3rd Dan",
            "4th Dan", // 10
            "5th Dan",
            "6th Dan",
            "7th Dan",
            "8th Dan",
            "9th Dan", // 15
            "White Sash", // 16
            "Red Sash",
            "Gold Sash",
            "Master", // 19
            "Shisho",
            "Roshi",
            "Sensei",
	        #endregion
        };

        public static List<string> weaponLvlName = new List<string>()
        {
            #region Weapon Skill Levels
		    "Untrained", // 0
            "Awkward",
            "Mediocre",
            "Capable",
            "Familiar",
            "Practiced", // 5
            "Competent",
            "Experienced",
            "Skillful",
            "Proficient",
            "Exceptional", // 10
            "Brilliant",
            "Expert",
            "Astonishing",
            "Amazing",
            "Incredible", // 15
            "Master",
            "Genius",
            "Unearthly",
            "Immortal" // 19 
	        #endregion
        };

        public static List<string> thievingLvlName = new List<string>()
        {
            #region Thievery Skill Levels
		    "Untrained",
            "Clumsy",
            "Mediocre",
            "Average",
            "Talented",
            "Practiced", // 5
            "Deft",
            "Efficient",
            "Graceful",
            "Professional",
            "Dexterous", // 10
            "Adroit",
            "Expert",
            "Astonishing",
            "Amazing",
            "Incredible", // 15
            "Magician",
            "Peerless",
            "Incomprehensible",
            "Master" // 19 
	        #endregion
        };

        public static List<string> thaumLvlNameFemale = new List<string>()
        {
            #region Thaumaturge (Female) Magic Skill Levels
            "Untrained", // 0
            "Shaman",
            "Apprentice",
            "Initiate",
            "Acolyte",
            "Healer", // 5
            "Canoness",
            "Exorcist",
            "Priestess",
            "Seer",
            "Summoner of Snakes", //10
            "Summoner of Spirits",
            "Mistress of Spirits",
            "Prophetess",
            "Matriarch",
            "High Priestess", // 15
            "Mistress of the Planes",
            "Mistress of the Dead",
            "Mistress of Earth and Sky",
            "Hierophant" // 19 
	        #endregion
        };

        public static List<string> thaumLvlNameMale = new List<string>()
        {
            #region Thaumaturge (Male) Magic Skill Levels
            "Untrained", // 0
            "Shaman",
            "Apprentice",
            "Initiate",
            "Acolyte",
            "Healer", // 5
            "Canon",
            "Exorcist",
            "Priest",
            "Seer",
            "Summoner of Snakes", // 10
            "Summoner of Spirits",
            "Master of Spirits",
            "Prophet",
            "Patriarch",
            "High Priest", // 15
            "Master of the Planes",
            "Master of the Dead",
            "Master of Earth and Sky",
            "Hierophant" // 19 
	        #endregion
        };

        public static List<string> druidLvlNameMale = new List<string>()
        {
            "Untrained", // 0
            "Tenderfoot",
            "Greenhorn",
            "Novitiate",
            "Pupil",
            "Mender", // 5
            "Communer",
            "Grey Robe",
            "Brown Robe",
            "Green Robe",
            "Fire Walker", // 10
            "Water Wielder",
            "Earth Mover",
            "Air Bender",
            "Druid",
            "Master of Fire", // 15
            "Master of Water",
            "Master of Earth",
            "Master of Air",
            "Sylvan Druid" // 19
        };

        public static List<string> druidLvlNameFemale = new List<string>()
        {
            "Untrained", // 0
            "Tenderfoot",
            "Greenhorn",
            "Novitiate",
            "Pupil",
            "Mender", // 5
            "Communer",
            "Grey Robe",
            "Brown Robe",
            "Green Robe",
            "Fire Walker", // 10
            "Water Wielder",
            "Earth Mover",
            "Air Bender",
            "Druidess",
            "Mistress of Fire", // 15
            "Mistress of Water",
            "Mistress of Earth",
            "Mistress of Air",
            "Sylvan Druidess" // 19
        };

        public static List<string> rangerLvlNameMale = new List<string>()
        {
            "Untrained", // 0
            "Initiate",
            "Tyro",
            "Greenhand",
            "Pupil",
            "Mender", // 5
            "Communer",
            "Grey Robe",
            "Brown Robe",
            "Green Robe",
            "Fire Walker", // 10
            "Water Wielder",
            "Earth Mover",
            "Air Bender",
            "Allwarden",
            "Master of Fire", // 15
            "Master of Water",
            "Master of Earth",
            "Master of Air",
            "Sylvan Druid" // 19
        };

        public static List<string> rangerLvlNameFemale = new List<string>()
        {
            "Untrained", // 0
            "Initiate",
            "Tyro",
            "Greenhand",
            "Pupil",
            "Mender", // 5
            "Communer",
            "Grey Robe",
            "Brown Robe",
            "Green Robe",
            "Fire Walker", // 10
            "Water Wielder",
            "Earth Mover",
            "Air Bender",
            "Allwarden",
            "Mistress of Fire", // 15
            "Mistress of Water",
            "Mistress of Earth",
            "Mistress of Air",
            "Sylvan Druidess" // 19
        };

        public static List<string> wizardLvlName = new List<string>()
        {
            "Untrained","Aspirant","Apprentice","Apprentice to Fire","Apprentice to Ice","Apprentice to Illusions","Shaper of Fire",
            "Shaper of Ice","Wizard","Shaper of Illusions","Illusionist","Master of Earth","Master of Illusions","Master of Air",
            "Mage","Lord of Fire","Lord of Illusions","Lord of Air","Archmage","Magus"
        };

        public static List<string> thiefLvlNameMale = new List<string>()
        {
            "Untrained","Skulker in Shadows","Master of Mischief","Diviner of Magics","Knight of Darkness","Opener of Ways",
            "Lurker in Darkness","Obscurer of Ways","Master of Water","Master of Air","Master of Secrets","Master Thief",
            "Shadow Thief","Shadow Mage","Shadow Stalker","Shadow Lord","Thief of Wands","Thief of Cups","Thief of Pentacles",
            "Thief of Souls"
        };

        public static List<string> thiefLvlNameFemale = new List<string>()
        {
            "Untrained","Skulker in Shadows","Mistress of Mischief","Diviner of Magics","Dame of Darkness","Opener of Ways",
            "Lurker in Darkness","Obscurer of Ways","Mistress of Water","Mistress of Air","Mistress of Secrets","Mistress Thief",
            "Shadow Thief","Shadow Witch","Shadow Stalker","Shadow Lady","Thief of Wands","Thief of Cups","Thief of Pentacles",
            "Thief of Souls"
        };

        public static List<string> sorcererLvlNameMale = new List<string>()
        {
            "Untrained",
            "Aspirant",
            "Apprentice",
            "Prestidigitator", // 3
            "Beguiler",
            "Neophyte of Undeath",
            "Recondite of Undeath", // 6
            "Commander of Undead",
            "Despoiler",
            "Maker of Pestilence",
            "Viceroy of Pestilence", // 10
            "Sorcerer",
            "Caretaker of Undeath",
            "Cager of Souls",
            "Master of Pestilence", // 14
            "Master of Undeath",
            "Lord of Pestilence",
            "Warlock",
            "Arch Sorcerer", // 18
            "Lich"
        };

        public static List<string> sorcererLvlNameFemale = new List<string>()
        {
            "Untrained",
            "Aspirant",
            "Apprentice",
            "Prestidigitator", // 3
            "Beguiler",
            "Neophyte of Undeath",
            "Recondite of Undeath", // 6
            "Commander of Undead",
            "Despoiler",
            "Maker of Pestilence",
            "Viceroy of Pestilence", // 10
            "Sorceress",
            "Caretaker of Undeath",
            "Cager of Souls",
            "Mistress of Pestilence", // 14
            "Mistress of Undeath",
            "Lady of Pestilence",
            "Witch",
            "Arch Sorcereress", // 18
            "Lich"
        };
        #endregion

        /// <summary>
        /// Retrieve the best skillType out of a designated skillTypes array.
        /// </summary>
        /// <param name="chr">The Character whose skills are to be reviewed.</param>
        /// <param name="skillTypes">Array of skillTypes to review.</param>
        /// <returns>The most experienced skillType.</returns>
        public static Globals.eSkillType GetBestSkill(Character chr, List<Globals.eSkillType> skillTypes)
        {
            Globals.eSkillType bestSkill = Globals.eSkillType.None;

            foreach (Globals.eSkillType skillType in skillTypes)
            {
                if (Skills.GetSkillLevel(chr, skillType) > Skills.GetSkillLevel(chr, bestSkill))
                    bestSkill = skillType;
            }
            return bestSkill;
        }

        /// <summary>
        /// Get the amount of skill experience needed for a skill level.
        /// </summary>
        /// <param name="level">The level of skill experience.</param>
        /// <returns>The amount of experience needed for a level.</returns>
        public static long GetSkillForLevel(int level)
        {
            if (level <= 0) return 0;

            long m = 1600;

            for (int a = 1; a <= level; a++)
            {
                if (a < SKILL_XP_LEVEL_CAP)
                    m = m * 2;
                else m = m + m;
            }

            return m;
        }

        /// <summary>
        /// Get the amount of skill experience needed for a skill level.
        /// </summary>
        /// <param name="level">The level of skill experience.</param>
        /// <returns>The amount of experience needed for a level.</returns>
        public static long GetSkillToMax(int level)
        {
            long m = 1600;
            for (int a = 1; a < level; a++)
            {
                if (a < SKILL_XP_LEVEL_CAP)
                    m = m * 2;
                else m = m + m;
            }
            return m;
        }

        /// <summary>
        /// Get how much experience is needed to be earned from the starting point of a new skill level.
        /// </summary>
        /// <param name="level">Current skill level.</param>
        /// <returns>The total amount of experience needed for the next level.</returns>
        public static long GetSkillToNext(int level)
        {
            if (level == 0)
                return 0;

            if (level == 1)
                return 1569;

            long m = 1600;

            for (int a = 2; a < level; a++)
            {
                if (a < SKILL_XP_LEVEL_CAP)
                    m = m * 2;
                else m = m + m;
            }

            return m;
        }

        public static int GetSkillLevel(Character ch, Globals.eSkillType skillType)
        {
            //Globals.eSkillType skillType = (Globals.eSkillType)Enum.Parse(typeof(Globals.eSkillType), skill, true);

            switch (skillType)
            {
                case Globals.eSkillType.Bash:
                    return GetSkillLevel(ch.bash);
                case Globals.eSkillType.Bow:
                    return GetSkillLevel(ch.bow);
                case Globals.eSkillType.Dagger:
                    return GetSkillLevel(ch.dagger);
                case Globals.eSkillType.Flail:
                    return GetSkillLevel(ch.flail);
                case Globals.eSkillType.Polearm:
                    return GetSkillLevel(ch.halberd);
                case Globals.eSkillType.Mace:
                    return GetSkillLevel(ch.mace);
                case Globals.eSkillType.Magic:
                    return GetSkillLevel(ch.magic);
                case Globals.eSkillType.Rapier:
                    return GetSkillLevel(ch.rapier);
                case Globals.eSkillType.Shuriken:
                    return GetSkillLevel(ch.shuriken);
                case Globals.eSkillType.Staff:
                    return GetSkillLevel(ch.staff);
                case Globals.eSkillType.Sword:
                    return GetSkillLevel(ch.sword);
                case Globals.eSkillType.Thievery:
                    return GetSkillLevel(ch.thievery);
                case Globals.eSkillType.Threestaff:
                    return GetSkillLevel(ch.threestaff);
                case Globals.eSkillType.Two_Handed:
                    return GetSkillLevel(ch.twoHanded);
                case Globals.eSkillType.Unarmed:
                    return GetSkillLevel(ch.unarmed);
                default:
                    return 0;
            }
        }

        public static int GetSkillLevel(long skillExp)
        {
            if (skillExp < 31)
                return 0;

            long low = 31;

            long high = 1600;

            for (int a = 1; a <= Skills.MAX_SKILL_LEVEL; a++)
            {
                if (skillExp >= low && skillExp < high)
                    return a;

                low = high;

                if (a < SKILL_XP_LEVEL_CAP)
                    high = high * 2;
                else high = high + high;
            }

            return 1;
        }

        public static string GetSkillTitle(Globals.eSkillType skillType, Character.ClassType classType, long skillExp, Globals.eGender gender)
        {
            int skillLevel = Skills.GetSkillLevel(skillExp);

            string post19rank = "";

            if (skillLevel > SKILL_XP_LEVEL_CAP)
            {
                post19rank = " Rk" + (skillLevel - MAX_SKILL_LEVEL + 1).ToString();
                skillLevel = SKILL_XP_LEVEL_CAP;
            }

            switch (skillType)
            {
                case Globals.eSkillType.Thievery:
                    return thievingLvlName[skillLevel] + post19rank;
                case Globals.eSkillType.Magic:
                    switch (classType)
                    {
                        case Character.ClassType.Druid:
                            if (gender == Globals.eGender.Female)
                                return druidLvlNameFemale[skillLevel] + post19rank;
                            else return druidLvlNameMale[skillLevel] + post19rank;
                        case Character.ClassType.Ranger:
                            if (gender == Globals.eGender.Female)
                                return rangerLvlNameFemale[skillLevel] + post19rank;
                            else return rangerLvlNameMale[skillLevel] + post19rank;
                        case Character.ClassType.Sorcerer:
                            if (gender == Globals.eGender.Female)
                                return sorcererLvlNameFemale[skillLevel] + post19rank;
                            else return sorcererLvlNameMale[skillLevel] + post19rank;
                        case Character.ClassType.Thaumaturge:
                            if (gender == Globals.eGender.Female)
                                return thaumLvlNameFemale[skillLevel] + post19rank;
                            else return thaumLvlNameMale[skillLevel] + post19rank;
                        case Character.ClassType.Thief:
                            if (gender == Globals.eGender.Female)
                                return thiefLvlNameFemale[skillLevel] + post19rank;
                            else return thiefLvlNameMale[skillLevel] + post19rank;
                        case Character.ClassType.Wizard:
                            return wizardLvlName[skillLevel] + post19rank;
                        default:
                            return weaponLvlName[0];
                    }
                case Globals.eSkillType.Unarmed:
                    return unarmedLvlName[skillLevel] + post19rank;
                default:
                    return weaponLvlName[skillLevel] + post19rank;
            }
        }

        public static String GetSkillName(Globals.eSkillType skillType)
        {
            return Utils.FormatEnumString(skillType.ToString());
        }
        
        /// <summary>
        /// Give skill for stealing and casting, and as a result of special block in combat
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="amount"></param>
        /// <param name="skillType"></param>
        public static void GiveSkillExp(Character ch, int amount, Globals.eSkillType skillType)
        {
            if (ch == null) return;

            try
            {
                // Break out of here right away if the server is not set to allow NPCs to gain skill.
                if (System.Configuration.ConfigurationManager.AppSettings["NPCSkillGain"].ToLower() == "false" && ch != null && !ch.IsPC)
                    return;

                string currentSkillTitle = Skills.GetSkillTitle(skillType, ch.BaseProfession, ch.GetSkillExperience(skillType), ch.gender);

                double bonus = 1;

                int skillExpGained = amount;
                int skillGainHardCap = Skills.GetSkillLevel(ch.GetSkillExperience(skillType)) * SKILL_GAIN_HARD_CAP_MULTIPLIER;
                if (skillGainHardCap < SKILL_GAIN_HARD_CAP_MULTIPLIER) skillGainHardCap = SKILL_GAIN_HARD_CAP_MULTIPLIER;

                #region Verify hard cap for amount of skill gained not exceeded (skill level * 100)
                if (skillExpGained > skillGainHardCap)
                    skillExpGained = skillGainHardCap;
                #endregion

                #region Set skill gain bonus for spell casters with high wisdom or intelligence
                if (skillType == Globals.eSkillType.Magic)
                {
                    if (ch.IsWisdomCaster)
                    {
                        double statBonus = Rules.GetGenericStatModifier(ch, Globals.eAbilityStat.Wisdom) * .5;
                        if (statBonus > 0) bonus += statBonus;
                    }
                    else if (ch.IsIntelligenceCaster)
                    {
                        double statBonus = Rules.GetGenericStatModifier(ch, Globals.eAbilityStat.Intelligence) * .5;
                        if (statBonus > 0) bonus += statBonus;
                    }
                }
                #endregion

                #region Set skill gain bonus for thieves with high dexterity using thievery skill
                if (ch.BaseProfession == Character.ClassType.Thief && skillType == Globals.eSkillType.Thievery)
                {
                    double statBonus = Rules.GetGenericStatModifier(ch, Globals.eAbilityStat.Dexterity) * .5;
                    if (statBonus > 0) bonus += statBonus;
                }
                #endregion

                #region Set skill gain bonus for specialized fighters
                if (ch.fighterSpecialization == skillType)
                    bonus += 1;
                #endregion

                #region Set skill gain bonus for martial artists using unarmed skill
                if (ch.BaseProfession == Character.ClassType.Martial_Artist && skillType == Globals.eSkillType.Unarmed)
                {
                    bonus += 1;
                }
                #endregion

                #region Calculate and add training bonus, if character is trained.
                if ((ch is PC) && (ch as PC).GetTrainedSkillExperience(skillType) > 0)
                {
                    int skillTrainingBonus = (int)((float)(ch as PC).GetTrainedSkillExperience(skillType) * (.02 * bonus));
                    if (skillTrainingBonus < 1) skillTrainingBonus = 1;                 // Make sure we deplete training.

                    if ((skillExpGained + skillTrainingBonus) > skillGainHardCap)
                    {
                        if (skillExpGained >= skillGainHardCap)
                        {
                            skillTrainingBonus = 0;                                     // Already over hard cap, no training.
                        }
                        else
                        {
                            skillTrainingBonus = skillGainHardCap - skillExpGained;     // Allow training up to the hard cap, then stop.
                        }
                    }

                    skillExpGained += skillTrainingBonus;                               // This shouldn't exceed hardcap, but we'll check later anyway.

                    (ch as PC).SetTrainedSkillExperience(skillType, (ch as PC).GetTrainedSkillExperience(skillType) - skillTrainingBonus);

                    if ((ch as PC).GetTrainedSkillExperience(skillType) < 0)
                    {
                        (ch as PC).SetTrainedSkillExperience(skillType, 0);
                    }
                }
                #endregion

                #region Verify hard cap for amount of skill gained not exceeded (skill level * 100)
                if (skillExpGained > skillGainHardCap)
                    skillExpGained = skillGainHardCap;
                #endregion

                // Accelerated skill gain.
                if (DragonsSpineMain.Instance.Settings.AcceleratedSkillGain)
                    skillExpGained = Convert.ToInt32(skillExpGained * DragonsSpineMain.Instance.Settings.AcceleratedSkillGainMultiplier);

                // Lagniappe -- more skill gain
                if (ch.HasEffect(Effect.EffectTypes.Lagniappe, out Effect lagniappeEffect))
                {
                    if (lagniappeEffect.Power <= 2)
                        skillExpGained += skillExpGained / 2;
                    else skillExpGained = skillExpGained * lagniappeEffect.Power;
                }

                // Drudgery -- less skill gain
                if (ch.HasEffect(Effect.EffectTypes.Drudgery, out Effect drudgeEffect))
                {
                    if (drudgeEffect.Power <= 2)
                        skillExpGained -= skillExpGained / 2;
                    else skillExpGained = skillExpGained / drudgeEffect.Power;
                }

                if (ch is PC && ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol && ch.PCState == Globals.ePlayerState.PLAYING)
                    ch.Write(ProtocolYuusha.CHARACTER_SKILLEXPCHANGE + skillType + ProtocolYuusha.VSPLIT + skillExpGained + ProtocolYuusha.CHARACTER_SKILLEXPCHANGE_END);

                ch.SetSkillExperience(skillType, ch.GetSkillExperience(skillType) + skillExpGained);

                #region If character has risen a skill level send skill up sound and message
                if (Skills.GetSkillTitle(skillType, ch.BaseProfession, ch.GetSkillExperience(skillType), ch.gender) != currentSkillTitle)
                {
                    ch.SendSound(Sound.GetCommonSound(Sound.CommonSound.SkillUp));
                    ch.WriteToDisplay("You have risen from " + currentSkillTitle + " to " + Utils.FormatEnumString(Skills.GetSkillTitle(skillType, ch.BaseProfession, ch.GetSkillExperience(skillType), ch.gender) + " in your " + Utils.FormatEnumString(skillType.ToString()).ToLower()) + " skill.");
                }
                #endregion

                #region Log as non combat skill gain if applicable
                Utils.Log(ch.GetLogString() + " gained " + skillExpGained.ToString() + " " + skillType.ToString() + " skill.", Utils.LogType.SkillGainNonCombat);
                #endregion
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        /// <summary>
        /// Give skill experience as a result of any type of combat in player vs. environment.
        /// </summary>
        /// <param name="ch">The Character to gain skill experience.</param>
        /// <param name="target">The target to use in the calculation for amount of skill experience to give.</param>
        /// <param name="skillType">The type of skill used.</param>
        public static void GiveSkillExp(Character ch, Character target, Globals.eSkillType skillType)
        {
            if (ch == null || target == null)
                return;

            if (target.IsImage) return;

            if (!ch.IsPC && System.Configuration.ConfigurationManager.AppSettings["NPCSkillGain"].ToLower() == "false")
                return;

            string currentSkillLevelName = GetSkillTitle(skillType, ch.BaseProfession, ch.GetSkillExperience(skillType), ch.gender);

            double skillRisk = CalculateSkillRisk(ch, target);

            if (ch is PC && ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol && ch.PCState == Globals.ePlayerState.PLAYING)
                ch.Write(ProtocolYuusha.CHARACTER_SKILLRISK + skillRisk + ProtocolYuusha.CHARACTER_SKILLRISK_END);

            int skillExpGained = (int)((float)target.Experience * skillRisk);
#if DEBUG
            target.SendToAllDEVInSight(target.GetNameForActionResult() + ": " + target.Experience + "xp numAttackers: " + ch.NumAttackers + " SkillRisk: " + skillRisk);
#endif

            double bonus = 1;

            int skillGainHardCap = GetSkillLevel(ch.GetSkillExperience(skillType)) * SKILL_GAIN_HARD_CAP_MULTIPLIER;

            if (skillGainHardCap < SKILL_GAIN_HARD_CAP_MULTIPLIER) skillGainHardCap = SKILL_GAIN_HARD_CAP_MULTIPLIER;

            #region Verify hard cap for amount of skill gained not exceeded (skill level * 100)
            if (skillExpGained > skillGainHardCap)
                skillExpGained = skillGainHardCap;
            #endregion

            #region Set skill gain bonus for spell casters with high wisdom or intelligence
            if (skillType == Globals.eSkillType.Magic)
            {
                if (ch.IsWisdomCaster)
                {
                    double statBonus = Rules.GetGenericStatModifier(ch, Globals.eAbilityStat.Wisdom) * .5;
                    if (statBonus > 0) bonus += statBonus;
                }
                else if (ch.IsIntelligenceCaster)
                {
                    double statBonus = Rules.GetGenericStatModifier(ch, Globals.eAbilityStat.Intelligence) * .5;
                    if (statBonus > 0) bonus += statBonus;
                }
            }
            #endregion

            #region Set skill gain bonus for thieves with high dexterity using thievery skill
            if (ch.BaseProfession == Character.ClassType.Thief && skillType == Globals.eSkillType.Thievery)
            {
                double statBonus = Rules.GetGenericStatModifier(ch, Globals.eAbilityStat.Dexterity) * .5;
                if (statBonus > 0) bonus += statBonus;
            }
            #endregion

            #region Set skill gain bonus for specialized fighters
            if (ch.fighterSpecialization == skillType)
            {
                bonus += 1;
            }
            #endregion

            #region Set skill gain bonus for martial artists using unarmed skill
            if (ch.BaseProfession == Character.ClassType.Martial_Artist && skillType == Globals.eSkillType.Unarmed)
            {
                bonus += 1;
            }
            #endregion

            if (skillType == Globals.eSkillType.Bow)//mlt adding ,nock shoot = 1/2 gain as was
            {
                bonus += 1;
            }

            #region Calculate and add training bonus, if character is trained.
            if ((ch is PC) && (ch as PC).GetTrainedSkillExperience(skillType) > 0)
            {
                // get skill training bonus
                int skillTrainingBonus = (int)((float)(ch as PC).GetTrainedSkillExperience(skillType) * (.02 * bonus));

                // make sure the training pool is going to be depleted
                if (skillTrainingBonus < 1) skillTrainingBonus = 1;

                // if hard cap is enabled, and if the skill experience plus the training bonus is greater than the hard cap
                if ((skillExpGained + skillTrainingBonus) > skillGainHardCap)
                {
                    if (skillExpGained >= skillGainHardCap)
                    {
                        skillTrainingBonus = 0;                                     // Already over hard cap, no training.
                    } 
                    else 
                    {
                        skillTrainingBonus = skillGainHardCap - skillExpGained;     // Allow training up to the hard cap.
                    }
                }

                // add skill training bonnus to total skill experience gained
                skillExpGained += skillTrainingBonus;

                // decrease skill training amount
                (ch as PC).SetTrainedSkillExperience(skillType, (ch as PC).GetTrainedSkillExperience(skillType) - skillTrainingBonus);

                // make sure skill trained amount is not less than 0
                if ((ch as PC).GetTrainedSkillExperience(skillType) < 0) (ch as PC).SetTrainedSkillExperience(skillType, 0);
            }
            #endregion
            
            #region Verify hard cap for amount of skill gained not exceeding (skill level * 100)
            if (skillExpGained > skillGainHardCap)
            {
                skillExpGained = skillGainHardCap;
            }
            #endregion

            // Accelerated skill gain.
            if (DragonsSpineMain.Instance.Settings.AcceleratedSkillGain)
                skillExpGained = Convert.ToInt32(skillExpGained * DragonsSpineMain.Instance.Settings.AcceleratedSkillGainMultiplier);

            // Lagniappe -- more skill gain
            if (ch.HasEffect(Effect.EffectTypes.Lagniappe, out Effect lagniappeEffect))
            {
                if (lagniappeEffect.Power <= 2)
                    skillExpGained += skillExpGained / 2;
                else skillExpGained = skillExpGained * lagniappeEffect.Power;
            }

            // Drudgery -- less skill gain
            if (ch.HasEffect(Effect.EffectTypes.Drudgery, out Effect drudgeEffect))
            {
                if (drudgeEffect.Power <= 2)
                    skillExpGained -= skillExpGained / 2;
                else skillExpGained = skillExpGained / drudgeEffect.Power;
            }

            // Send skill gained to client.
            if (ch is PC && ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol && ch.PCState == Globals.ePlayerState.PLAYING)
                ch.Write(ProtocolYuusha.CHARACTER_SKILLEXPCHANGE + skillType + ProtocolYuusha.VSPLIT + skillExpGained + ProtocolYuusha.CHARACTER_SKILLEXPCHANGE_END);

            ch.SetSkillExperience(skillType, ch.GetSkillExperience(skillType) + skillExpGained); // add the amount of experience gained

            #region If character has risen a skill level send skill up sound and message
            if (GetSkillTitle(skillType, ch.BaseProfession, ch.GetSkillExperience(skillType), ch.gender) != currentSkillLevelName)
            {
                ch.SendSound(Sound.GetCommonSound(Sound.CommonSound.SkillUp));
                ch.WriteToDisplay("You have risen from " + currentSkillLevelName + " to " + GetSkillTitle(skillType, ch.BaseProfession, ch.GetSkillExperience(skillType), ch.gender) + " in your " + Utils.FormatEnumString(skillType.ToString()).ToLower() + " skill.");
            }
            #endregion

            Utils.Log(ch.GetLogString() + " +" + skillExpGained + " " + skillType.ToString().ToLower() + " vs. " + target.GetLogString() + ".", Utils.LogType.SkillGainCombat);

        }

        public static bool SkillTrain(PC chr, Character trainer, Item gold)
        {
            string skillName = "";

            Globals.eSkillType skillType = Globals.eSkillType.None;

            #region Determine skillName, which is part of the string that will be displayed to Character upon training.
            if (chr.RightHand == null)
            {
                if(!(trainer is Merchant) || (trainer is Merchant && (trainer as Merchant).trainerType != Merchant.TrainerType.Martial_Arts))
                {
                    chr.WriteToDisplay(trainer.GetNameForActionResult(false) + ": I am not skilled in training the martial arts.");
                    return false;
                }

                skillType = Globals.eSkillType.Unarmed;
                skillName = "in the martial arts";
            }
            else
            {
                skillType = chr.RightHand.skillType;
                if (skillType == Globals.eSkillType.Magic)
                {
                    switch (chr.BaseProfession)
                    {
                        case Character.ClassType.Thaumaturge:
                            skillName = "in the art of thaumaturgy";
                            break;
                        case Character.ClassType.Thief:
                            skillName = "in the art of shadow magic";
                            break;
                        case Character.ClassType.Wizard:
                            skillName = "in the art of wizardry";
                            break;
                        case Character.ClassType.Druid:
                            skillName = "in the ways of druidic magic";
                            break;
                        case Character.ClassType.Ranger:
                            skillName = "in the ways of warden magic";
                            break;
                        default:
                            skillName = "in magic";
                            break;
                    }

                    if(chr.BaseProfession != trainer.BaseProfession)
                    {
                        chr.WriteToDisplay(trainer.GetNameForActionResult(false) + ": I am not skilled " + skillName + ".");
                        return false;
                    }
                }
                else if (skillType == Globals.eSkillType.Thievery)
                {
                    skillName = "in the ways of thievery";

                    if (trainer.BaseProfession != Character.ClassType.Thief)
                    {
                        chr.WriteToDisplay(trainer.GetNameForActionResult(false) + ": I am not skilled " + skillName + ".");
                        return false;
                    }
                    else if(chr.BaseProfession != Character.ClassType.Thief)
                    {
                        chr.WriteToDisplay(trainer.GetNameForActionResult(false) + ": You do not have what it takes to train " + skillName + ".");
                        return false;
                    }
                }
                else
                {
                    // TODO: add tradeskill check here

                    if ((chr.BaseProfession == Character.ClassType.Thief && trainer.BaseProfession == Character.ClassType.Thief) ||
                        (trainer is Merchant && (trainer as Merchant).trainerType == Merchant.TrainerType.Weapon))
                    {
                        skillName = "with " + chr.RightHand.shortDesc;
                    }
                    else
                    {
                        if (chr.RightHand != null) skillName = " with " + chr.RightHand.shortDesc;
                        else skillName = "";

                        chr.WriteToDisplay(trainer.GetNameForActionResult(false) + ": You'll have to look elsewhere for training" + skillName + ".");
                        return false;
                    }
                }
            } 
            #endregion

            // trainer's current skill experience
            long trainerSkill = trainer.GetSkillExperience(skillType);

            #region Return false if the trainer is not skilled.
            if (trainerSkill <= 0) // if the trainer is not skilled
            {
                chr.WriteToDisplay(trainer.Name + ": I am not skilled " + skillName + ".");
                return false;
            }
            #endregion

            #region Return false if training bash skill and player does not have the Shield Bash talent.
            if (skillType == Globals.eSkillType.Bash && !chr.HasTalent(Talents.GameTalent.TALENTS.Bash))
            {
                chr.WriteToDisplay(trainer.Name + ": You have not learned the " + Talents.GameTalent.GetTalent(Talents.GameTalent.TALENTS.Bash).Name + " talent.");
                return false;
            } 
            #endregion

            // player's current skill experience
            long skillExp = chr.GetSkillExperience(skillType);

            #region Return false if the player is more skilled than the trainer.
            if (skillExp > trainerSkill)
            {
                chr.WriteToDisplay(trainer.Name + ": You are more skilled " + skillName.ToLower() + " than I.");
                return false;
            }
            #endregion

            // player's current trained amount with the skill
            long currentTrained = chr.GetTrainedSkillExperience(skillType);

            // total skill experience needed for this skill level
            long skillMax = Skills.GetSkillForLevel(Skills.GetSkillLevel(skillExp));

            // how far the player is into the skill level
            long skillIntoLevel = skillMax - skillExp;

            // how much skill the player has remaining in current skill level
            long skillRemain = skillMax - skillIntoLevel; // Skills.GetSkillToNext(Skills.GetSkillLevel(skillExp)) - skillIntoLevel;

            // current skill rank
            int skillRank = Skills.GetSkillRank(skillExp);

            // cost per rank for current skill level
            int skillRankCost = Skills.GetTrainingCostPerRank(Skills.GetSkillLevel(skillExp));

            int nextLevelRankCost = Skills.GetTrainingCostPerRank(Skills.GetSkillLevel(skillExp) + 1);

            long maxTrainAmount = 0;

            if (skillRank <= 5)
            {
                maxTrainAmount = 5 * skillRankCost;
            }
            else
            {
                int difference = 10 - skillRank;
                maxTrainAmount = (difference * skillRankCost) + (nextLevelRankCost * (5 - difference));
            }

            if (currentTrained >= maxTrainAmount) // if the player's already trained to the maximum then send them off to practice
            {
                chr.WriteToDisplay(trainer.Name + ": You must practice more before I can train you again.");
                return false;
            }

            // subtract the amount this character is already trained from the maximum to set new maximum
            if (currentTrained > 0)
                maxTrainAmount = maxTrainAmount - currentTrained;

            long trainAmount = 0;

            // figure the amount of gold we'll take and set the training amount
            if (gold.coinValue > maxTrainAmount)
            {
                trainAmount = maxTrainAmount;
                gold.coinValue = gold.coinValue - trainAmount;

                switch (gold.land)
                {
                    case -1:
                        Map.PutItemOnCounter(chr, gold);
                        break;
                    case -2:
                        chr.bankGold = gold.coinValue;
                        break;
                    case -3:
                        chr.bankGold -= trainAmount;
                        break;
                    default:
                        trainer.CurrentCell.Add(gold);
                        break;
                }
            }
            else
            {
                trainAmount = Convert.ToInt64(gold.coinValue);
                // using up all bank gold for max training
                if (gold.land == -2)
                    chr.bankGold = 0;
                else if (gold.land == -3)
                    chr.bankGold -= trainAmount;
            }

            chr.SetTrainedSkillExperience(skillType, chr.GetTrainedSkillExperience(skillType) + trainAmount);

            if (skillType == Globals.eSkillType.Magic)
                skillName = "in magic";

            Utils.Log(chr.GetLogString() + " trains " + skillType + ". current: " + skillExp + " trainAmount: " + trainAmount + " maxTrainAmount: " + maxTrainAmount +
                " skillMaxXp: " + skillMax + " skillRemain: " + skillRemain + " skillRank: " + skillRank + " skillRankCost: " + skillRankCost +
                " nextLevelRankCost: " + nextLevelRankCost + " trainedSkillExp: " + currentTrained, Utils.LogType.SkillTraining);

            chr.WriteToDisplay(trainer.Name + ": You have been trained " + skillName + ", go now and practice.");

            chr.Experience += trainAmount; // give training experience

            Utils.Log(chr.GetLogString() + " earns " + trainAmount + " experience from training.", Utils.LogType.ExperienceTraining);

            return true;
        }

        private static double CalculateSkillRisk(Character ch, Character target)
        {
            int levelDifference = ch.IsPC && !target.IsPC ? target.Level - Rules.GetExpLevel(ch.Experience) : 0;

            double baseM = .04;

            double riskM = .01;

            double finalM = .04;

            int pctHits = (int)(((float)ch.Hits / (float)ch.HitsFull) * 100);

            if (levelDifference < 0) // the character is engaging a lower level critter
            {
                finalM = ((ch.NumAttackers + levelDifference) * riskM) + baseM;
            }
            else if (levelDifference == 0) // the character is engaging a critter of equal level
            {
                if (pctHits >= 50) // character's health is at or above 50%
                {
                    finalM = (ch.NumAttackers * riskM) + baseM;
                }
                else if (pctHits < 50 && pctHits > 25) // character's health is below 50% and above 25%
                {
                    finalM = (ch.NumAttackers * (riskM * 2)) + baseM;
                }
                else // character's health is at or below 25%
                {
                    finalM = (ch.NumAttackers * (riskM * 3)) + baseM;
                }
            }
            else // the character is engaging a higher level critter
            {
                if (pctHits >= 75) // character's health is at or above 75%
                {
                    finalM = ((ch.NumAttackers + levelDifference) * riskM * 2) + baseM;
                }
                else if (pctHits < 75 && pctHits > 35) // character's health is below 75% and above 35%
                {
                    finalM = ((ch.NumAttackers + levelDifference) * (riskM * 3)) + baseM;
                }
                else // character's health is at or below 40%
                {
                    finalM = ((ch.NumAttackers + levelDifference) * (riskM * 4)) + baseM;
                }
            }

            // look at entity lists here for additional skill risk

            if (finalM < 0) finalM = .01;

            Utils.Log("Final Multiplier = " + finalM + " " + ch.GetLogString() + " H: " + pctHits + "% #Atkrs: " + ch.NumAttackers + " vs. " + target.GetLogString() + ".", Utils.LogType.SkillGainRisk);
            
            return finalM;
        }

        public static int GetTrainingCostPerRank(int skillLevel)
        {
            // get cost to train the entire skill level, divided by 10 ranks
            return (int)(Rules.Formula_TrainingCostForLevel(skillLevel) / 10);
        }

        private static int GetSkillRank(long currentSkill)
        {
            long max = Skills.GetSkillForLevel(Skills.GetSkillLevel(currentSkill)); // get skill to max
            long into = max - currentSkill; // get skill into current skill level
            long remaining = Skills.GetSkillToNext(Skills.GetSkillLevel(currentSkill)) - into; // get skill remaining in this skill level

            int rank = (int)(((float)remaining / (float)Skills.GetSkillToNext(Skills.GetSkillLevel(currentSkill))) * 10);

            return rank;
        }

        public static void SkillLoss(Character ch, Globals.eSkillType skillType, long amount)
        {
            if (skillType == Globals.eSkillType.None) return;

            long currSkillExp = ch.GetSkillExperience(skillType);
            string currSkillTitle = Skills.GetSkillTitle(skillType, ch.BaseProfession, currSkillExp, ch.gender);
            int currSkillLevel = Skills.GetSkillLevel(currSkillExp);

            if (currSkillExp > 31 + amount) { currSkillExp -= amount; }

            if (ch is PC && ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol && ch.PCState == Globals.ePlayerState.PLAYING)
                ch.Write(ProtocolYuusha.CHARACTER_SKILLEXPCHANGE + skillType + ProtocolYuusha.VSPLIT + -amount + ProtocolYuusha.CHARACTER_SKILLEXPCHANGE_END);

            ch.SetSkillExperience(skillType, currSkillExp);

            if (GetSkillLevel(currSkillExp) != currSkillLevel)
                ch.WriteToDisplay("You have fallen from " + currSkillTitle + " to " + GetSkillTitle(skillType, ch.BaseProfession, currSkillExp, ch.gender) + " in your " + Utils.FormatEnumString(skillType.ToString()) + " skill.");
        }

        public static void SkillLossOverTime(Character ch)
        {
            foreach (Globals.eSkillType skillType in Enum.GetValues(typeof(Globals.eSkillType)))
            {
                if (skillType != Globals.eSkillType.None)
                {
                    Skills.SkillLoss(ch, skillType, Skills.GetSkillLevel(ch.GetSkillExperience(skillType)));
                }
            }
        }

        public static Globals.eSkillType[] GetXHighestSkills(Character ch, int x)
        {

            try
            {
                Globals.eSkillType[] skills = (Globals.eSkillType[])Enum.GetValues(typeof(Globals.eSkillType));
                long[] values = new long[skills.Length];

                for (int count = 0; count < skills.Length; count++)
                {
                    values[count] = ch.GetSkillExperience(skills[count]);
                }

                // sorted in ascending order
                Array.Sort(values, skills);

                Globals.eSkillType[] xHighest = new Globals.eSkillType[x];

                for (int count = 0, at = skills.Length - 1; count < x; count++, at--)
                {
                    xHighest[count] = skills[at];
                }

                return xHighest;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return null;
            }
        }
    }
}