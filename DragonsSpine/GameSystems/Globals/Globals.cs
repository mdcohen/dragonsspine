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

namespace DragonsSpine
{
    static public class Globals
    {
        public const int MAX_EXP_LEVEL = 40;
        public const short EXP_LEVEL_3 = 1600;
        public const long EXP_LEVEL_20 = 209715200;
        public const int SKILL_LOSS_DIVISOR = 5000; // 5/31/2019 not used?
        public const int MAX_STORE_INVENTORY = 12; // maximum store inventory amount
        public const int MIN_SCRIBE_LEVEL = 8; // Level characters have to be above to scribe spells

        public static readonly string[] LETTERS = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        public static readonly string[] ALIGNMENT_SYMBOLS = { "?", " ", "!", "*", "+", " ", "+" }; // matches the Globals.eAlignment enumeration

        /// <summary>
        /// eLightsource is in order of visible spectrum strength.
        /// </summary>
        public enum eLightSource
        {
            None,
            WeakItemBlueglow, // current cell illuminated
            StrongItemBlueglow, // radius of 1 cells around current cell
            AnyFireEffect, // radius of 2 cells around current cell
            TownLimits,
            LightSpell,
            RadiantOrb // radius of 3 cells around current cell, full visibility of map
        }

        public enum eEncumbranceLevel
        {
            Lightly = 0, Moderately = 1, Heavily = 2, Severely = 3
        }

        public enum eMerchantType
        {
            None,
            Pawn,
            Barkeep,
            Weapon,
            Armor,
            Apothecary,
            Book,
            Jewellery,
            General,
            Magic
        }

        public enum eTrainerType
        {
            None,
            Spell,
            Weapon,
            Martial_Arts,
            Knight,
            Sage,
            HP_Doctor
        }

        public enum eInteractiveType
        {
            None,
            Banker,
            Tanner,
            Balm,
            Recall_Ring,
            Mugwort,
            Confessor,
            Mender
        }

        public enum eSpellType
        {
            Abjuration, // protective spells
            Alteration, // alter the properties of spell targets
            Conjuration, // bring manifestations of objects, creatures, or some form of energy to you
            Divination, // enable caster to learn secrets long forgotten, to predict the future, to find hidden things, and to foil deceptive spells
            Evocation, // manipulate energy, or tap into an unseen force, to create a desired effect
            Necromancy, // spells relating to the dead and undead
            Enchantment, // spells that affect the minds of others
            Illusion
        }

        public enum eBookType { None, Normal, Spellbook, Scroll }

        public enum eSpellTargetType
        {
            Area_Effect,
            Group,
            Point_Blank_Area_Effect,
            Self,
            Single,
            Single_or_Group
        }

        public enum ePlayerState
        {
            LOGIN,
            NEWCHAR,
            CONFERENCE,
            PLAYING,
            NEWCHARVERIFY,
            PICKRACE,
            PICKGENDER,
            ROLLSTATS,
            PICKCLASS,
            PICKFIRSTNAME,
            PICKEMAIL,
            VERIFYEMAIL,
            PICKPASSWORD,
            VERIFYPASSWORD,
            MAINMENU,
            PICKACCOUNT,
            CHECKPASSWORD,
            CHANGECHAR,
            CHANGECHAR2,
            ACCOUNTMAINT,
            DELETECHAR,
            DELETECHAR2,
            CHANGEPASSWORD,
            CHANGEPASSWORD2,
            CHANGEPASSWORD3,
            PROTO_CHARGEN,
            MAILMENU,
            MAILREADLISTING, // type in a number and the screen is cleared then the message is displayed in MAILREADMESSAGE
            MAILREADMESSAGE, // message is being displayed, type DELETE (caps sensitive) or BACK
            MAILCONFIRMDELETE, // confirm the deletion of a received message
            MAILSEND_GET_RECIPIENT_NAME, // enter player's name, do checks here
            MAILSEND_GET_SUBJECT_HEADER, // enter subject
            MAILSEND_GET_BODY, // enter body, type QUITNOW
            MAILSEND_REVIEW // enter sendnow or quitnow
        };

        public enum eAbilityStat
        {
            Strength,
            Dexterity,
            Intelligence,
            Wisdom,
            Constitution,
            Charisma
        }

        /// <summary>
        /// TODO: This enumeration, and related code, needs some work.
        /// </summary>
        public enum eSpecies
        {
            Arachnid, Avian,
            Centaur, CloudDragon,
            Demon, DomesticAnimal, Dwarvish,
            Elvish,
            FireDragon, Fish,
            Giantkin, Goblin,
            Hobgoblin, Human,
            IceDragon, Insect,
            Kobold,
            LightningDrake,
            Magical, Makon, Minotaur,
            Ogre, Orc, Overlord,
            Pheonix, Plant,
            Reptile,
            Sandwyrm, Smokey, Snakeman,
            Tengu, Thisson, Titan, Troll, TrollKing,
            TundraYeti,
            Unknown, Unnatural,
            WildAnimal, WindDragon, Wyvern,
            Ydmos
        }

        public enum eIconColor
        {
            Green,
            Yellow,
            Brown,
            Gray,
            Red,
            Purple
        }

        public enum eGender
        {
            It,
            Male,
            Female,
            Random
        }

        public enum eHomeland
        {
            Illyria,
            Mu,
            Lemuria,
            Leng,
            Draznia,
            Hovath,
            Mnar,
            Barbarian
        }

        public enum eSkillType
        {
            None,
            Bow,
            Dagger,
            Flail,
            Polearm, // Halberd
            Mace,
            Rapier,
            Shuriken, // Range weapons. Includes sling. TODO: Rename?
            Staff,
            Sword,
            Threestaff,
            Two_Handed,
            Unarmed,
            Thievery,
            Magic,
            Bash,
            // Tradeskills
            Alchemy, // potions, poisons
            Blacksmithing, // metal armor, weapons
            Carpentry, // larger wooden structures and items, necessary for construction
            Construction, // includes shipbuilding, building structures
            Cooking,
            Fishing,
            Gathering, // similar to foraging in other MMOs
            Handicraft, // fletching, small woodworking (totems)
            Husbandry, // includes animal training
            Jewelcrafting, // gemstones and minerals work, jewelery creation
            Logging, // harvest type: wood
            Metallurgy, // working with metal, combined with jewelcrafting
            Mining, // hargest type: ore, gemstones, minerals
            Refining, // breakdown of materials and refining larger items
            Runesmithing, // creation and implementation of runes
            Scribing, // create new spellbooks, scrolls
            Tailoring // animal skinning included, crafting items from animal hides
        }

        public enum eTradeSkillType
        {
            Alchemy, // potions, poisons
            Blacksmithing, // metal armor, weapons
            Carpentry, // larger wooden structures and items, necessary for construction
            Construction, // includes shipbuilding, building structures
            Cooking,
            Fishing,
            Gathering, // similar to foraging in other MMOs
            Handicraft, // fletching, small woodworking (totems)
            Husbandry, // includes animal training
            Jewelcrafting, // gemstones and minerals work, jewelery creation
            Logging, // harvest type: wood
            Metallurgy, // working with metal, combined with jewelcrafting
            Mining, // hargest type: ore, gemstones, minerals
            Refining, // breakdown of materials and refining larger items
            Runesmithing, // creation and implementation of runes
            Scribing, // create new spellbooks, scrolls
            Tailoring // animal skinning included, crafting items from animal hides
        }

        public enum eWeaponAttackType
        {
            None, // cannot be used to attack, or cannot be attacked
            Slash, // slash weapon required
            Pierce, // piercing weapon required
            Blunt, // blunt weapon required (includes unarmed combat)
            All
        }

        public enum eImpLevel
        {
            USER,
            AGM,
            GM,
            DEVJR,
            DEV
        }

        public enum eAlignment
        {
            None,
            Lawful,
            Neutral,
            Chaotic,
            Evil,
            Amoral,
            ChaoticEvil // used in Rules.DetectAlignment -- this alignment attacks everything on sight
        }

        public enum eAttuneType
        {
            /// <summary>
            /// Item is already attuned or will never attune.
            /// </summary>
            None,
            /// <summary>
            /// Item will bind to the first player that attacks with it.
            /// </summary>
            Attack,
            /// <summary>
            /// Item will bind to the player that killed the current item's owner.
            /// </summary>
            Slain,
            /// <summary>
            ///  Item will bind when taken for the first time.
            /// </summary>
            Take,
            /// <summary>
            /// Item will bind when worn for the first time.
            /// </summary>
            Wear,
            /// <summary>
            /// Item will bind when given as a quest reward.
            /// </summary>
            Quest
        }

        #region Max Wearable
        /// <summary>
        /// This array should always match the eWearLocation enumeration in length.
        /// Fingers are an exception because they are not only right and left.
        /// Some code will have to be modified if more than a Right and Left wear orientation is added.
        /// </summary>
        public static int[] Max_Wearable = {
            0, // None
            1, // Head
            1, // Neck            
            2, // Ear
            1, // Face
            1, // Shoulders
            1, // Back
            1, // Torso
            2, // Bicep            
            2, // Wrist
            8, // Finger
            1, // Waist
            1, // Legs            
            1, // Feet
            1  // Hands
            //1, // Nose
            //1, // Forearms
            //1, // Calves
            //1, // Shins
        };
        #endregion


        /// <summary>
        /// The order of the WearLocation enum corresponds with the Max_Wearable array
        /// </summary>
        public enum eWearLocation
        {
            /// <summary>
            /// Item cannot be worn.
            /// </summary>
            None,
            Head,
            Neck,
            Ear, // x 2
            Face,
            Shoulders,
            Back,
            Torso,
            Bicep, // x 2
            Wrist, // x 2
            Finger,
            Waist,
            Legs,
            Feet,
            Hands, // 17 total (not including None) - db accepts 20
            //Nose,
            //Forearms,
            //Calves,
            //Shins,
        };

        public enum eWearOrientation : int
        {
            None, RightRing1, RightRing2, RightRing3, RightRing4, LeftRing1, LeftRing2, LeftRing3, LeftRing4, Left = 9, Right = 10
        }

        public enum eItemType
        { 
            Weapon,
            Wearable,
            Container,
            Miscellaneous,
            Edible,
            Potable,
            Corpse, // Corpse Item child type
            Bauble, // gems
            Literature, // scrolls, books
            Coin // only coins
        }

        public enum eItemBaseType
        {
            Unknown, // 0
            Bow,
            Dagger,
            Flail,
            Halberd, // rename this polearm? not necessary, yet.
            Mace, // 5
            Rapier,
            Shuriken,
            Staff,
            Sword,
            Threestaff, // 10
            TwoHanded,
            Thievery,
            Magic,
            Unarmed,
            Armor, // 15
            Helm,
            Bracelet,
            Shield,
            Boots,
            Bottle, // 20
            Amulet,
            Ring,
            Book,
            Food,
            Gem, // 25
            Figurine,
            Scroll,
            Boomerang,
            Fan,
            Taxidermic, // makon paws
            Torch,
            Whip,
            Sling,
            Earring
        }

        /// <summary>
        /// The eItemSize enumeration is in order of smallest to largest.
        /// </summary>
        public enum eItemSize
        {
            Pouch_Only,
            Sack_Or_Pouch,
            Sack_Only,
            Belt_Or_Sack,
            Belt_Only,
            Belt_Large_Slot_Only,
            No_Container
        }

        public enum eArmorType
        {
            None,
            Feathers,
            Fur,
            Bone,
            Cloth,
            Leather,
            Studded,
            Hide,
            Chainmail,
            Scalemail,
            Banded,
            Platemail,
            Steel,
            Scales,
            Rock,
            Stone,
            Mithril,
            Chitin
        }

        public enum eAttackType
        {
            None,
            Blunt,
            Pierce,
            Slash,
            BluntPierce,
            BluntSlash,
            BluntPierceSlash,
            PierceSlash,
        };

        public enum eMaterials
        {
            Gold,
            Copper,
            Bronze,
            Silver,
            Mithril,
            Iron,
            Steel, // alloy
            Platinum,
            Blue_Crystal,
            Jade,
            Diamond,
            Sapphire,
            Amethyst,
            Onyx,
            Blue_Diamond,
            Yttril,
            Malachite,
            Aquamarine,
            Silk,
            Ruby,
            Pink_Pearl,
            Black_Pearl,
            White_Pearl,
            Yellow_Pearl,
            Lead,
            Emerald,
            Moonstone,
            Mother_of_Pearl,
            Tsavorite,
            Glass,
            Griffin_Feathers,
            Pheonix_Feathers,
            Leather,
            Bone,
            Black_Diamond,
            Stone,
            Tourmaline
        }

        public enum eUniqueMaterials
        {
            Capril,
            Setrium,
            Tustine,
            Ucryx
        }
    }
}
