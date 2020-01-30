using System;
using System.Collections.Generic;

namespace DragonsSpine.Autonomy.EntityBuilding
{
    public static class EntityLists
    {
        public static Entity GetUniqueEntity(int npcID)
        {
            switch (npcID)
            {
                #region Unique Entities already existing in the database.
                case 1:
                    return Entity.Kesmai_Red_Dragon;
                case 2:
                    return Entity.Ydnac;
                case 3:
                    return Entity.Trog;
                case 4:
                    return Entity.Kesmai_Crypt_Ghoul;
                case 1001:
                    return Entity.Leng_Red_Dragon;
                case 1002:
                    return Entity.Leng_Vampire;
                case 1003:
                    return Entity.Leng_Sandserpent;
                case 1004:
                    return Entity.Leng_Lightning_Drake;
                case 1006:
                    return Entity.Barbarossa;
                case 2001:
                    return Entity.Axe_Glacier_Blue_Dragon;
                case 2002:
                    return Entity.Axe_Glacier_Lightning_Drake;
                case 2003:
                    return Entity.Axe_Glacier_Yeti;
                case 2004:
                    return Entity.Axe_Glacier_Giant;
                case 2901:
                    return Entity.Alia;
                case 2903:
                    return Entity.King_Wolf;
                case 3007:
                    return Entity.Oakvael_Iron_Lich;
                case 3008:
                    return Entity.Oakvael_Serpent;
                case 3010:
                    return Entity.Oakvael_Doom_Orc;
                case 3311:
                    return Entity.Oakvael_Wind_Dragon;
                case 5005:
                    return Entity.Mutated_Gator;
                case 5006:
                    return Entity.Annwn_Phoenix;
                case 5007:
                    return Entity.Annwn_Red_Dragon;
                case 6032:
                    return Entity.Annwn_Giant;
                case 6070:
                    return Entity.Thisson;
                case 7039:
                    return Entity.Underkingdom_Princess;
                case 7040:
                    return Entity.Troll_King;
                case 7041:
                    return Entity.Overlord;
                case 7042:
                    return Entity.Carfel;
                case 7043:
                    return Entity.Shelob;
                case 7046:
                    return Entity.KooKoo;
                case 7056:
                    return Entity.Wandering_Lightning_Drake;
                case 7100:
                    return Entity.Makon;
                case 9002:
                    return Entity.Broodmother;
                case 20009:
                    return Entity.Confessor_Ghost;
                    #endregion
            }

            return Entity.None;
        }

        public enum Entity
        {
            #region Non Unique Entities (Regular Spawns)
            None, All,
            // A
            Aarakocra, Alia, Alligator, Apparition, Aquatic_Elf, Arbalist, Archer, Audrey,
            // B
            Banshee, Barbarian, Bear, Beetle, Beholder, Berserker, Bighorn, Bloodhulk, Boadkin, Boar, Briarvex, Broodmare,
            Bugbear, Bulette, Burrower,
            // C
            Cat, Centaur, Chimera, Chuul, Cobra, Confessor_Ghost, Crocodile,
            // D
            Dao, Dam, Demon, Dire_Rabbit, Dire_Wolf, Djinn, Doe, Dog, Dragon, Drake, Draugr, Drider,
            Drow_Child, Drow, Drow_Priestess, Druid, Dryad,
            // E
            Eagle, Efreet, Eidolon, Elemental, Ent, Ettin,
            // F
            Fighter, Firbolg, Formicid, Fox,
            // G
            Gargoyle, Gelding, Ghast, Ghost, Ghoul, Giant, Giantkin, Giantlord, Gnome, Gnoll, Goblin, Golem, Goose, Grey_Elf, Griffin,
            // H
            Harpy, Hellhound, High_Elf, Hippogriff, Hobgoblin, Hunter, Hyena,
            // I
            Illithid,
            // J
            Jaguar,
            // K
            Kelpie, Knight, Kobold,
            // L
            Lamassu, Lich, Lindwyrm, Lion, Ice_Lizard, Lizardman, Lurker,
            // M
            Mammoth, Manticore, Marid, Martial_Artist, Merrow, Minotaur, Mummy, Mutated_Gator,
            // N
            Ninja, Nixie, Nymph,
            // O
            Ogre, Oni, Orc, Owlbear,
            // P
            Panda, Panther, Phantasm, Phoenix, Piranha, Pixie, Presence, Priest,
            // R
            Ranger, Rat, Ravager, Raven, Revenant, Rockworm,
            // S
            Sabertooth, Salamander, Sandserpent, Sandwyrm, Savage, Scorpion, Serpent, Shadow, Shambling_Mound, Shark, Skeleton, Smilodon, Snake, Snakeman,
            Sahuagin, Satyr, Sorcerer, Spectre, Spider, Spirit, Sprite, Stalker, Stag, Stallion, Statue,
            // T
            Tengu, Thaumaturge, Thief, Tiger, Troglodyte, Troll,
            // U
            Ursen,
            // V
            Vampire, Velociraptor, Viper,
            // W
            Waft, Wight, Wild_Elf, Will__o___Wisp, Wizard, Wolf, Wood_Elf, Wraith, Wyrm, Wyvern,
            // Y
            Yasai_kyofu, Yaun__Ti, Yeti,
            // Z
            Zombie,
            #endregion
            
            #region Dragons and Drakes
            Amethyst_Dragon, // neutral
            Crystal_Dragon, // neutral
            Emerald_Dragon, // neutral
            Sapphire_Dragon, // neutral
            Topaz_Dragon, // neutral
            Black_Dragon, // chaotic evil
            Blue_Dragon, // chaotic evil
            Brass_Dragon, // lawful
            Bronze_Dragon, // lawful
            Copper_Dragon, // lawful
            Gold_Dragon, // lawful
            Green_Dragon, // chaotic evil
            Red_Dragon, // chaotic evil
            Silver_Dragon, // lawful
            White_Dragon, // chaotic evil

            Lightning_Drake, // chaotic
            #endregion

            #region Unique Entities
            // Island of Kesmai
            Smokey,
            Kesmai_Red_Dragon,
            Laurelena, // druid trainer
            Ydnac,
            Kesmai_Crypt_Ghoul,
            Rhed, // ranger trainer
            Trog,
            // Deep Kesmai
            Broodmother,
            Nightmare,
            Succubus_Prima,
            // Leng
            Leng_Red_Dragon,
            Leng_Lightning_Drake,
            Leng_Sandserpent,
            Leng_Vampire,
            Leng_Dungeon_Wyrm,
            Barbarossa,
            Trambuskar,
            Ianta,
            Cyprial,
            // Axe Glacier
            Axe_Glacier_Blue_Dragon,
            Axe_Glacier_Lightning_Drake,
            Axe_Glacier_Yeti,
            Axe_Glacier_Giant,
            King_Wolf,
            // Oakvael
            Oakvael_Wind_Dragon,
            Oakvael_Iron_Lich, // Lich twins
            Oakvael_Serpent,
            Oakvael_Doom_Orc,
            // Underkingdom
            Overlord,
            Troll_King,
            Shelob,
            Carfel,
            Swordmaster,
            Wandering_Lightning_Drake,
            Underkingdom_Princess,
            KooKoo,
            // Annwn
            Annwn_Red_Dragon,
            Annwn_Giant,
            Annwn_Phoenix,
            Ydmos,
            Ulluvial,
            Anwen,
            Rhiannon,
            Morrigan,
            // Torii
            Gojira,
            Thisson,
            // Shukumei
            Makon,
            Great_Schema,
            // Rift Glacier
            Kraken,
            Titan,
            Rift_Glacier_Lightning_Drake,
            Rift_Glacier_Cloud_Dragon,
            // Innkadi
            The_Lost_One,
            Sea_Hag, // Sartila? TODO: Add NPC random speech for events such as being attacked
            High_Elf_Cdr,
            Spider_Queen,
            Drow_Master,
            Drow_Matriarch,
            Archmage,
            Illithid_Elder,
            Vampire_Lord,
            #endregion

            #region Demons
            // Lesser Demons
            Alu, // female half succubus half human
            Babau,
            Bar__lgura,
            Cambion,
            Chasme,
            Dretch,
            Moldeus,
            Nalfeshnee,
            Rutterkin,
            Succubus,
            Vrock,

            // Non-canonical demons
            Asmodeous,
            Damballa,
            Glamdrang,
            Pazuzu,
            Perdurabo,
            Samael,
            Thamuz,

            // Demon lords
            //Abraxus, // The Unfathomable
            //Adimarchus, // Prince of Madness
            //Ahazu, // The Siezer
            //Ahrimanus, // Chief of Cacodaemons
            Aldinach, // female
            //Alrunes, // female
            //Alvarez,
            Nochlum,
            Asmogorgon,
            #endregion

            #region Merchants
            Animal_Trainer,
            #endregion
        }

        /// <summary>
        /// Unique entities possess many attributes which are more advantageous than non-unique entities.
        /// </summary>
        public static List<Entity> UNIQUE = new List<Entity>
        {
            #region Unique Entities
            // Island of Kesmai
            Entity.Kesmai_Crypt_Ghoul,
            Entity.Kesmai_Red_Dragon,
            Entity.Ydnac,
            Entity.Trog,
            Entity.Laurelena,
            Entity.Rhed,
            // Axe Glacier,
            Entity.Alia,
            Entity.Axe_Glacier_Blue_Dragon,
            Entity.Axe_Glacier_Giant,
            Entity.Axe_Glacier_Lightning_Drake,
            Entity.Axe_Glacier_Yeti,
            Entity.King_Wolf,
            // Annwn
		    Entity.Annwn_Red_Dragon,
            Entity.Annwn_Giant,
            Entity.Annwn_Phoenix,
            Entity.Ydmos,
            // Underkingdom
            Entity.Overlord,
            Entity.Troll_King,
            Entity.Shelob,
            Entity.Carfel,
            Entity.Wandering_Lightning_Drake,
            Entity.KooKoo,
            Entity.Underkingdom_Princess,
            // Torii
            Entity.Thisson,
            // Shukumei
            Entity.Great_Schema,
            Entity.Makon,
            // Rift Glacier
            Entity.Kraken,
            Entity.Titan,
            Entity.Rift_Glacier_Lightning_Drake,
            Entity.Rift_Glacier_Cloud_Dragon,
            // Leng
            Entity.Leng_Dungeon_Wyrm,
            Entity.Leng_Sandserpent,
            // named demons
            Entity.Asmodeous, Entity.Damballa, Entity.Glamdrang, Entity.Pazuzu, Entity.Perdurabo, Entity.Samael, Entity.Thamuz,
            // Innkadi
            Entity.Drow_Master, Entity.Drow_Matriarch,
            Entity.Archmage,
            Entity.The_Lost_One,
            Entity.High_Elf_Cdr,
            Entity.Sea_Hag,
            Entity.Spider_Queen,
            Entity.Illithid_Elder,
            Entity.Vampire_Lord,
            // Deep Kesmai
            Entity.Nightmare,
            Entity.Succubus_Prima,
            Entity.Broodmother
	        #endregion
        };

        /// <summary>
        /// Antler attack.
        /// </summary>
        public static List<Entity> ANTLERED = new List<Entity>
        {
            Entity.Dire_Rabbit, Entity.Stag, Entity.The_Lost_One
        };

        /// <summary>
        /// Spawn as amoral alignment.
        /// </summary>
        public static List<Entity> AMORAL = new List<Entity>
        {
            Entity.Confessor_Ghost,
            Entity.Samael,
            Entity.Shambling_Mound
        };

        /// <summary>
        /// Creatures dwelling in both water and on land.
        /// </summary>
        public static List<Entity> AMPHIBIOUS = new List<Entity>
        {
            Entity.Aquatic_Elf,
            Entity.Chuul,
            Entity.Gojira,
            Entity.Kelpie,
            Entity.Ice_Lizard, Entity.Lizardman, Entity.Lurker,
            Entity.Merrow,
            Entity.Nixie, Entity.Nymph,
            Entity.Sahuagin, Entity.Sea_Hag
        };

        /// <summary>
        /// Creatures with animal, or basic survival intelligence (excluding drakes and dragons, of course). These entities do not wear armor or wield weapons.
        /// </summary>
        public static List<Entity> ANIMAL = new List<Entity>
        {
            #region Animalian Creatures
		    Entity.Alligator,
            Entity.Bear, Entity.Beetle, Entity.Bighorn, Entity.Boar, Entity.Broodmare, Entity.Bulette, Entity.Burrower,
            Entity.Cat, Entity.Chimera, Entity.Chuul, Entity.Cobra, Entity.Crocodile,
            Entity.Dam, Entity.Dire_Rabbit, Entity.Dire_Wolf, Entity.Doe, Entity.Dog, Entity.Dragon, Entity.Drake, Entity.Dretch,
            Entity.Axe_Glacier_Lightning_Drake, Entity.Leng_Lightning_Drake, Entity.Wandering_Lightning_Drake,
            Entity.Eagle,
            Entity.Fox,
            Entity.Gelding, Entity.Goose, Entity.Griffin,
            Entity.Hellhound, Entity.Hippogriff, Entity.Hyena,
            Entity.Jaguar,
            Entity.Ice_Lizard,
            Entity.Lamassu, Entity.Lion, Entity.Lurker,
            Entity.Makon, Entity.Mammoth, Entity.Manticore, Entity.Mutated_Gator,
            Entity.Nightmare,
            Entity.Owlbear,
            Entity.Panda, Entity.Panther, Entity.Phoenix, Entity.Piranha,
            Entity.Bighorn, Entity.Rat, Entity.Raven, Entity.Rockworm,
            Entity.Sabertooth, Entity.Salamander, Entity.Sandserpent, Entity.Sandwyrm, Entity.Scorpion, Entity.Serpent,
            Entity.Shark, Entity.Snake, Entity.Smilodon, Entity.Stag, Entity.Stallion, Entity.Spider,
            Entity.Tiger,
            Entity.Velociraptor, Entity.Viper,
            Entity.Will__o___Wisp, Entity.Wolf, Entity.Wyrm, Entity.Wyvern,
            Entity.Yeti,

            // dragons
            Entity.Blue_Dragon, Entity.Brass_Dragon, Entity.Bronze_Dragon, Entity.Copper_Dragon, Entity.Gold_Dragon, Entity.Green_Dragon, Entity.Red_Dragon,
            Entity.Silver_Dragon, Entity.White_Dragon,

            // drakes
            Entity.Lightning_Drake,

            // unique entities
            Entity.Broodmother,
            Entity.Kesmai_Red_Dragon,
            Entity.Leng_Red_Dragon,
            Entity.Oakvael_Wind_Dragon,
            Entity.Annwn_Red_Dragon,
            Entity.Annwn_Phoenix,
            Entity.Shelob,
            Entity.Wandering_Lightning_Drake,
            //Entity.Thisson,
            Entity.Gojira,
            Entity.Kraken,
            Entity.Makon,
            Entity.Rift_Glacier_Lightning_Drake,
            Entity.Rift_Glacier_Cloud_Dragon,
            Entity.Leng_Dungeon_Wyrm,
            Entity.Leng_Sandserpent,
            Entity.Leng_Lightning_Drake,
            Entity.King_Wolf,
            Entity.Axe_Glacier_Blue_Dragon,
            Entity.Axe_Glacier_Lightning_Drake
	        #endregion
        };

        /// <summary>
        /// Small, weaker animals typically used as fillers. Might tan into wearable or quest items.
        /// </summary>
        public static List<Entity> ANIMAL_SMALL = new List<Entity>
        {
            Entity.Cat, Entity.Dog, Entity.Fox
        };

        public static List<Entity> AQUAPHOBIC = new List<Entity>
        {
            Entity.Hellhound
        };

        public static List<Entity> ARACHNID = new List<Entity>
        {
            Entity.Spider, Entity.Spider_Queen
        };

        /// <summary>
        /// Currently (6/5/2019) they will be geared with bows. In AI they will search for a bow weapon...
        /// </summary>
        public static List<Entity> ARCHERY_PREFERRED = new List<Entity>()
        {
            Entity.Arbalist,
            Entity.Archer,
            Entity.Ranger
        };

        /// <summary>
        /// Entities with a carapace.
        /// </summary>
        public static List<Entity> ARTHROPOD = new List<Entity>
        {
            Entity.Beetle, Entity.Bulette, Entity.Burrower,
            Entity.Chuul,
            Entity.Kraken,
            Entity.Lindwyrm,
            Entity.Scorpion
        };

        // As of 12/3/2015 these entities display feathers as armor. Perhaps make them all flight capable.
        public static List<Entity> AVIAN = new List<Entity>
        {
            Entity.Aarakocra,
            Entity.Eagle,
            Entity.Goose,
            Entity.Harpy,
            Entity.Lamassu,
            Entity.Annwn_Phoenix,
            Entity.Raven
        };

        // Beak attack and block strings. Piercing attack.
        public static List<Entity> BEAKED = new List<Entity>
        {
            Entity.Aarakocra,
            Entity.Eagle,
            Entity.Griffin,
            Entity.Hippogriff,
            Entity.Owlbear,
            Entity.Raven,
            Entity.Vrock,
        };

        /// <summary>
        /// Entities with a bite attack that is possibly poisonous, paralytic or causes another symptom.
        /// </summary>
        public static List<Entity> BITER = new List<Entity>
        {
            Entity.Alligator,
            Entity.Cat, Entity.Chimera, Entity.Crocodile,
            Entity.Dire_Rabbit, Entity.Dire_Wolf, Entity.Dog, Entity.Dretch,
            Entity.Hellhound,
            Entity.Piranha,
            Entity.Jaguar,
            Entity.Mutated_Gator,
            Entity.Ursen,
            Entity.Shark,
            Entity.Wolf, Entity.King_Wolf,
            Entity.Sabertooth, Entity.Smilodon, Entity.Spider,
             Entity.Viper, Entity.Snake,
            Entity.Moldeus
        };

        public static List<Entity> CANIFORMS = new List<Entity>
        {
            Entity.Bear,
            Entity.Bugbear,
            Entity.Owlbear,
            Entity.Panda,
            Entity.Ursen,
        };

        public static List<Entity> CANINE = new List<Entity>
        {
            Entity.Dire_Wolf, Entity.Dog,
            Entity.Gnoll,
            Entity.Hellhound,
            Entity.Moldeus,
            Entity.Wolf, Entity.King_Wolf
        };

        /// <summary>
        /// Entity names stay capitalized when being formatted in the EntityBuilder.
        /// </summary>
        public static List<Entity> CAPITALIZED = new List<Entity>
        {
            Entity.Alia,
            Entity.Archmage,
            Entity.Axe_Glacier_Yeti,
            Entity.Barbarossa, Entity.Broodmother,
            Entity.Carfel,
            Entity.Cyprial,
            Entity.Drow_Master,
            Entity.Gojira,
            Entity.Great_Schema,
            Entity.High_Elf_Cdr,
            Entity.Ianta,
            Entity.Illithid_Elder,
            Entity.King_Wolf,
            Entity.Laurelena, Entity.Rhed,
            Entity.Overlord,
            Entity.The_Lost_One, Entity.Thisson, Entity.Titan, Entity.Trambuskar, Entity.Troll_King,
            Entity.Vampire_Lord,
            Entity.Ydmos, Entity.Ydnac,
            Entity.Ulluvial, Entity.Anwen, Entity.Rhiannon, Entity.Morrigan,
            Entity.Shelob, Entity.Smokey, Entity.Spider_Queen, Entity.Swordmaster,
            Entity.Sea_Hag, Entity.Spider_Queen, Entity.Succubus_Prima,
            Entity.Drow_Master,
            Entity.Drow_Matriarch,
            Entity.Nightmare,
        };

        public static List<Entity> CASTMODE_NOPREP = new List<Entity>
        {
            Entity.Illithid, Entity.Illithid_Elder,
        };

        public static List<Entity> CASTMODE_UNLIMITED = new List<Entity>
        {
            
        };

        public static List<Entity> CHAOTIC_EVIL_ALIGNMENT = new List<Entity>
        {
            Entity.Kesmai_Red_Dragon, Entity.Ydnac, Entity.Ydmos, Entity.Axe_Glacier_Blue_Dragon, Entity.Succubus, Entity.Nightmare, Entity.Annwn_Red_Dragon,
            Entity.Briarvex, Entity.Overlord, Entity.Zombie, Entity.Mummy, Entity.Carfel, Entity.Lurker, Entity.Makon,
            Entity.Rift_Glacier_Cloud_Dragon, Entity.Leng_Red_Dragon, Entity.Oakvael_Wind_Dragon,

            Entity.Beholder,
            Entity.Illithid, Entity.Illithid_Elder,

            Entity.Chuul, Entity.Sahuagin,

            Entity.Gojira,
            Entity.Thisson,

            Entity.The_Lost_One,

            Entity.Vampire,
            Entity.Vampire_Lord,
            Entity.Leng_Vampire,
            Entity.Zombie,

            Entity.Will__o___Wisp,
            Entity.Sea_Hag,

            Entity.Cyprial,

            Entity.Sea_Hag,
            Entity.Drider,

            // demons
            Entity.Alu, Entity.Asmodeous, Entity.Babau, Entity.Bar__lgura, Entity.Cambion, Entity.Chasme,
            Entity.Damballa, Entity.Dretch, Entity.Glamdrang, Entity.Pazuzu, Entity.Perdurabo, Entity.Rutterkin, Entity.Thamuz,
            Entity.Nalfeshnee, Entity.Moldeus, Entity.Vrock,

            Entity.Demon,

            Entity.Drow,
            Entity.Drow_Matriarch,
            Entity.Drow_Master,
            Entity.Drow_Priestess,
            Entity.Spider_Queen,

            Entity.Broodmother,
            Entity.Succubus_Prima,
            Entity.Nightmare,
        };

        /// <summary>
        /// Creatures with claws that slash and are possibly poisonous or paralytic.
        /// </summary>
        public static List<Entity> CLAWED = new List<Entity>
        {
            #region Clawed Creatures
		    Entity.Bear,
            Entity.Cat, Entity.Chimera,
            Entity.Dretch,
            Entity.Dire_Rabbit, Entity.Dire_Wolf, Entity.Dragon, Entity.Draugr, Entity.Annwn_Red_Dragon, Entity.Kesmai_Red_Dragon, Entity.Oakvael_Wind_Dragon, Entity.Axe_Glacier_Blue_Dragon,
            Entity.Leng_Red_Dragon, Entity.Rift_Glacier_Cloud_Dragon,
            Entity.Drake, Entity.Rift_Glacier_Lightning_Drake, Entity.Axe_Glacier_Lightning_Drake, Entity.Leng_Lightning_Drake, Entity.Wandering_Lightning_Drake,
            Entity.Eagle,
            Entity.Gargoyle,
            Entity.Hyena,
            Entity.Jaguar,
            Entity.King_Wolf, Entity.Kraken,
            Entity.Ice_Lizard,
            Entity.Lindwyrm, Entity.Lion,
            Entity.Makon, Entity.Manticore,
            Entity.Owlbear,
            Entity.Panda, Entity.Panther,
            Entity.Rat,
            Entity.Sabertooth, Entity.Salamander, Entity.Sea_Hag, Entity.Smilodon,
            Entity.Tiger, Entity.Trog, Entity.Troll, Entity.Troll_King,
            Entity.Ursen,
            Entity.Vampire, Entity.Vampire_Lord, Entity.Leng_Vampire,
            Entity.Wolf, Entity.King_Wolf, Entity.Wyrm, Entity.Leng_Dungeon_Wyrm, Entity.Wyvern,

            // dragons
            Entity.Blue_Dragon, Entity.Brass_Dragon, Entity.Bronze_Dragon, Entity.Copper_Dragon, Entity.Gold_Dragon, Entity.Green_Dragon, Entity.Red_Dragon,
            Entity.Silver_Dragon, Entity.White_Dragon,

            Entity.The_Lost_One
	        #endregion
        };

        /// <summary>
        /// Ability to see any hidden being.
        /// </summary>
        public static List<Entity> COGNIZANT = new List<Entity>
        {
            Entity.Beholder,
            Entity.Illithid, Entity.Illithid_Elder,
            Entity.Nightmare,
            Entity.Pazuzu,
            Entity.Spider_Queen, Entity.Succubus_Prima,
            Entity.The_Lost_One, Entity.Thisson
        };

        /// <summary>
        /// Spawned in Hell. Summoned to the Prime Material Plane by foolish mortals.
        /// </summary>
        public static List<Entity> DEMONS = new List<Entity>
        {
            Entity.Alu, Entity.Asmodeous,
            Entity.Babau, Entity.Bar__lgura,
            Entity.Cambion, Entity.Chasme,
            Entity.Damballa, Entity.Demon, Entity.Dretch,
            Entity.Glamdrang,
            Entity.Nalfeshnee,
            Entity.Oni,
            Entity.Pazuzu, Entity.Perdurabo,
            Entity.Rutterkin,
            Entity.Samael, Entity.Succubus, Entity.Succubus_Prima,
            Entity.Thamuz, Entity.Thisson,
            Entity.Vrock,

            // Named demons, not spawned yet 9/16/2019
            Entity.Aldinach,
            Entity.Moldeus,
            Entity.Nochlum,
            Entity.Asmogorgon
        };

        public static List<Entity> NAMED_DEMONS = new List<Entity>
        {
            Entity.Aldinach,
            Entity.Asmodeous,
            Entity.Pazuzu,
            Entity.Glamdrang,
            Entity.Samael,
            Entity.Damballa,
            Entity.Thamuz,
            Entity.Nochlum,
            Entity.Asmogorgon
        };

        public static List<Entity> ELVES = new List<Entity>
        {
            Entity.Archmage, Entity.Aquatic_Elf,
            Entity.Drider,
            Entity.Drow_Child,
            Entity.Drow,
            Entity.Drow_Master,
            Entity.Drow_Priestess,
            Entity.Drow_Matriarch, Entity.Spider_Queen,
            Entity.Grey_Elf, Entity.High_Elf_Cdr,
            Entity.High_Elf, Entity.Wild_Elf, Entity.Wood_Elf,
        };

        public static List<Entity> EQUINE = new List<Entity>
        {
            Entity.Broodmare, Entity.Centaur, Entity.Dam, Entity.Gelding, Entity.Nightmare, Entity.Stallion
        };

        public static List<Entity> EVIL_ALIGNMENT = new List<Entity>
        {
            Entity.Dire_Wolf,
            Entity.Ravager,
            Entity.Sorcerer
        };

        /// <summary>
        /// Fang attacks which typically poison.
        /// </summary>
        public static List<Entity> FANGED = new List<Entity>
        {
            Entity.Cobra,
            Entity.Spider, Entity.Shelob, Entity.Snake,
            Entity.Vampire, Entity.Vampire_Lord, Entity.Viper,
            Entity.Leng_Vampire,
            Entity.Spider_Queen,
            Entity.Drider,
            
        };

        /// <summary>
        /// Currently assigned similar sound files.
        /// </summary>
        public static List<Entity> FELINE = new List<Entity>
        {
            Entity.Cat, Entity.Chimera,
            Entity.Jaguar,
            Entity.Lamassu, Entity.Lion,
            Entity.Panther,
            Entity.Tiger,
            Entity.Sabertooth,
            Entity.Smilodon
        };

        // Always female gender.
        public static List<Entity> FEMALE = new List<Entity>
        {
            Entity.Alia,

            Entity.Spider_Queen,
            Entity.Axe_Glacier_Blue_Dragon, Entity.Axe_Glacier_Lightning_Drake, Entity.Leng_Lightning_Drake,
            Entity.Alu,
            Entity.Dryad, Entity.Nymph, Entity.Pixie, Entity.Sprite,

            Entity.Laurelena,

            Entity.Broodmother, Entity.Broodmare,
            Entity.Banshee, Entity.Lamassu, Entity.Sea_Hag, Entity.Succubus, Entity.Drow_Matriarch, Entity.Drow_Priestess, Entity.Spider_Queen,
            Entity.Trambuskar, Entity.Ianta, Entity.Underkingdom_Princess,
            Entity.Succubus, Entity.Succubus_Prima,
            Entity.Drider
        };

        /// <summary>
        /// Neutral and lawful creatures of light typically inhabiting forested regions. They ignore each other in Rules.DetectAlignment.
        /// </summary>
        public static List<Entity> FEY = new List<Entity>
        {
            Entity.Wild_Elf, Entity.Wood_Elf,
            Entity.Dryad,
            Entity.Centaur,
            Entity.Pixie,
            Entity.Satyr, Entity.Sprite,
            Entity.Ent,
            Entity.Firbolg, Entity.Nymph,
            Entity.Shambling_Mound,
            Entity.Ursen,
        };

        /// <summary>
        /// Entities with a permanent flame shield.
        /// </summary>
        public static List<Entity> FLAMESHIELDED = new List<Entity>
        {
            Entity.Alu,
            Entity.Hellhound,
            Entity.The_Lost_One,
            Entity.Nightmare,
        };

        /// <summary>
        /// These Creatures either fly or float and thus are able to move over most terrain and do not take falling damage.
        /// </summary>
        public static List<Entity> FLYING = new List<Entity>
        {
            #region Flying Creatures
            Entity.Aarakocra, Entity.Alu, Entity.Apparition,
            Entity.Banshee, Entity.Beholder,
            Entity.Cambion, Entity.Chasme, Entity.Chimera, Entity.Confessor_Ghost,
            Entity.Dao, Entity.Djinn, Entity.Dragon, Entity.Drake,
            Entity.Eagle, Entity.Eidolon, Entity.Efreet,
            Entity.Gargoyle, Entity.Ghost, Entity.Goose, Entity.Griffin,
            Entity.Harpy,
            Entity.Hippogriff,
            Entity.Manticore, Entity.Marid,
            Entity.Nightmare,
            Entity.Phantasm, Entity.Phoenix, Entity.Pixie, Entity.Presence,
            Entity.Raven,
            Entity.Spectre,
            Entity.Vrock,
            Entity.Waft, Entity.Wyrm, Entity.Wyvern, Entity.Leng_Dungeon_Wyrm,

            Entity.Annwn_Red_Dragon,
            Entity.Axe_Glacier_Blue_Dragon,
            Entity.Kesmai_Red_Dragon,
            Entity.Leng_Red_Dragon,
            Entity.Oakvael_Wind_Dragon,
            Entity.Rift_Glacier_Cloud_Dragon,

            Entity.Amethyst_Dragon, // neutral
            Entity.Crystal_Dragon, // neutral
            Entity.Emerald_Dragon, // neutral
            Entity.Sapphire_Dragon, // neutral
            Entity.Topaz_Dragon, // neutral
            Entity.Black_Dragon, // chaotic evil
            Entity.Blue_Dragon, // chaotic evil
            Entity.Brass_Dragon, // lawful
            Entity.Bronze_Dragon, // lawful
            Entity.Copper_Dragon, // lawful
            Entity.Gold_Dragon, // lawful
            Entity.Green_Dragon, // chaotic evil
            Entity.Red_Dragon, // chaotic evil
            Entity.Silver_Dragon, // lawful
            Entity.White_Dragon, // chaotic evil

            Entity.Will__o___Wisp
	        #endregion
        };

        public static List<Entity> GIANT_KIN = new List<Entity>
        {
            Entity.Axe_Glacier_Giant,
            Entity.Ettin,
            Entity.Firbolg,
            Entity.Giant, Entity.Giantkin, Entity.Giantlord,
            Entity.Titan, Entity.The_Lost_One
        };

        /// <summary>
        /// Currently only used to apply griffin sounds and make visible armor "feathers" 12/12/2015 Eb
        /// </summary>
        public static List<Entity> GRIFFIN_ARCHETYPE = new List<Entity>
        {
            Entity.Lamassu,
            Entity.Griffin,
            Entity.Hippogriff,
            Entity.Vrock,
        };

        /// <summary>
        /// These entities hit harder than is typical. They also possess slightly higher level skills.
        /// </summary>
        public static List<Entity> HARD_HITTERS = new List<Entity>
        {
            Entity.Annwn_Giant,
            Entity.Annwn_Phoenix,
            Entity.Annwn_Red_Dragon,
            Entity.Bar__lgura, Entity.Dretch,
            Entity.Great_Schema,
            Entity.Gojira,
            Entity.High_Elf_Cdr,
            Entity.King_Wolf,
            Entity.Kraken,
            Entity.Lindwyrm,
            Entity.Makon,
            Entity.Overlord,
            Entity.Rift_Glacier_Cloud_Dragon, Entity.Rift_Glacier_Lightning_Drake,
            Entity.Smokey,
            Entity.Thisson, Entity.Titan, Entity.Troll_King,
            Entity.Wandering_Lightning_Drake,
            Entity.Ydmos,
            Entity.The_Lost_One,
            Entity.Vampire_Lord,
            Entity.Kesmai_Crypt_Ghoul,
            Entity.Spider_Queen,
            Entity.Drow_Master,
            Entity.Broodmother,
            Entity.Succubus_Prima,
            Entity.Nightmare
        };

        /// <summary>
        /// Entities that have the ability to hide easily. (permanent effect, though still visible when on current cell)
        /// </summary>
        public static List<Entity> HIDDEN = new List<Entity>
        {
            #region Hidden Entities
            Entity.Aquatic_Elf, Entity.Arbalist, Entity.Archer,
            Entity.Babau,
            Entity.Dire_Rabbit,
            Entity.Fox,
            Entity.Ghast, Entity.Ghost, Entity.Ghoul, Entity.Great_Schema,
            Entity.Hobgoblin,
            Entity.Jaguar,
            Entity.Kraken,
            Entity.Lich, Entity.Lurker,
            Entity.Ninja, Entity.Nixie,
            Entity.Panther, Entity.Piranha, Entity.Pixie, Entity.Presence,
            Entity.Sandwyrm, Entity.Shadow, Entity.Shark, Entity.Spectre, Entity.Spider, Entity.Spirit, Entity.Stalker,
            Entity.Tengu,
            Entity.Vampire, Entity.Vampire_Lord, Entity.Viper, Entity.Leng_Vampire,
            Entity.Waft,

            Entity.Kesmai_Crypt_Ghoul,
            Entity.Spider_Queen,
            Entity.Carfel,
            Entity.Drow_Master
	        #endregion
        };

        public static List<Entity> HOOVED = new List<Entity>
        {
            Entity.Broodmare, Entity.Centaur, Entity.Dam, Entity.Gelding, Entity.Stallion, Entity.Nightmare, // equine
            Entity.Lamassu,
            Entity.Hippogriff,
            Entity.Doe, Entity.Stag,
            Entity.Bighorn
        };

        /// <summary>
        /// Entities with a horn attack.
        /// </summary>
        public static List<Entity> HORNED = new List<Entity>
        {
            Entity.Alu,
            Entity.Cambion,
            Entity.Dragon, Entity.Oni, Entity.Yeti,

            Entity.Annwn_Red_Dragon, Entity.Kesmai_Red_Dragon, Entity.Oakvael_Wind_Dragon, Entity.Axe_Glacier_Blue_Dragon,
            Entity.Leng_Red_Dragon, Entity.Rift_Glacier_Cloud_Dragon,

            Entity.Satyr, Entity.Bighorn,
            Entity.The_Lost_One
        };

        /// <summary>
        /// Humans with a specific purpose. See expansion Classes.
        /// </summary>
        public static List<Entity> HUMAN = new List<Entity>
        {
            Entity.Alu, Entity.Animal_Trainer, Entity.Arbalist, Entity.Archer,
            Entity.Barbarian, Entity.Berserker,
            Entity.Cambion,
            Entity.Druid,
            Entity.Fighter,
            Entity.Hunter,
            Entity.Knight,
            Entity.Martial_Artist,
            Entity.Ranger, Entity.Ravager,
            Entity.Savage, Entity.Sorcerer,
            Entity.Thaumaturge, Entity.Thief,
            Entity.Wizard,

            // unique entities
            Entity.Alia,
            Entity.Barbarossa,
            Entity.Carfel,
            Entity.Great_Schema,
            Entity.Ydmos,
            Entity.Ydnac,

            Entity.Laurelena,
            Entity.Rhed,

            Entity.Ulluvial, Entity.Anwen, Entity.Rhiannon, Entity.Morrigan,
        };

        /// <summary>
        /// Broad category, typically wielding weapons, wearing armor and participate well in Groups.
        /// </summary>
        public static List<Entity> HUMANOID = new List<Entity>
        {
            Entity.Archmage,
            Entity.Aarakocra, Entity.Alu, Entity.Aquatic_Elf,
            Entity.Babau, Entity.Bar__lgura, Entity.Bugbear,
            Entity.Cambion, Entity.Centaur, Entity.Cyprial,
            Entity.Drider,
            Entity.Drow_Child, Entity.Drow, Entity.Drow_Master, Entity.Drow_Matriarch, Entity.Drow_Priestess,
            Entity.Dryad,
            Entity.Ettin,
            Entity.Firbolg, Entity.Formicid,
            Entity.Gargoyle, Entity.Giant, Entity.Giantkin, Entity.Giantlord, Entity.Grey_Elf, Entity.Annwn_Giant, Entity.Axe_Glacier_Giant,
            Entity.Goblin, Entity.Gnoll, Entity.Gnome, Entity.Hobgoblin,
            Entity.Harpy, Entity.High_Elf, Entity.High_Elf_Cdr,
            Entity.Illithid, Entity.Illithid_Elder,
            Entity.Oakvael_Doom_Orc, Entity.Ogre, Entity.Oni, Entity.Orc,
            Entity.Pixie,
            Entity.Kobold,
            Entity.Lizardman,
            Entity.Merrow, Entity.Minotaur,
            Entity.Sahuagin, Entity.Sea_Hag, Entity.Satyr, Entity.Snakeman, Entity.Spider_Queen, Entity.Sprite,
            Entity.Tengu, Entity.The_Lost_One, Entity.Titan, Entity.Trog, Entity.Troglodyte, Entity.Troll, Entity.Troll_King,
            Entity.Ursen,
            Entity.Wild_Elf, Entity.Wood_Elf,
            Entity.Yaun__Ti,

            Entity.Succubus_Prima, Entity.Succubus,

            Entity.Underkingdom_Princess, Entity.KooKoo,
            Entity.Nalfeshnee,
            Entity.Moldeus,

            Entity.Vampire,
            Entity.Vampire_Lord,
        };

        #region Immunities
        public static List<Entity> IMMUNE_ACID = new List<Entity>
        {
            Entity.Broodmother,
            Entity.Black_Dragon,
            Entity.Babau, Entity.Bulette,
            Entity.Chuul,
            Entity.Gojira,
            Entity.Kraken,
            Entity.Rockworm,
            Entity.Statue,
            Entity.The_Lost_One,
            Entity.Will__o___Wisp
        };

        public static List<Entity> IMMUNE_BLINDNESS = new List<Entity>
        {
            Entity.Apparition,
            Entity.Bear, Entity.Briarvex,
            Entity.Chuul,
            Entity.Dao, Entity.Djinn, Entity.Dragon, Entity.Drake,
            Entity.Efreet,
            Entity.Formicid,
            Entity.Ghost, Entity.Gojira, Entity.Great_Schema,
            Entity.Illithid,
            Entity.Kraken,
            Entity.Lich, Entity.Lindwyrm,
            Entity.Makon,
            Entity.Panda,
            Entity.Overlord, Entity.Owlbear,
            Entity.Phoenix, Entity.Presence,
            Entity.Sahuagin, Entity.Sandserpent, Entity.Sandwyrm, Entity.Sea_Hag, Entity.Serpent, Entity.Shadow, Entity.Shambling_Mound, Entity.Statue,
            Entity.The_Lost_One,
            Entity.Vampire, Entity.Vampire_Lord,
            Entity.Wyrm, Entity.Leng_Dungeon_Wyrm,
            Entity.Will__o___Wisp
        };

        public static List<Entity> IMMUNE_COLD = new List<Entity>
        {
            Entity.Axe_Glacier_Blue_Dragon, Entity.Axe_Glacier_Yeti,
            Entity.Eidolon,
            Entity.Illithid,
            Entity.Kraken,
            Entity.Leng_Vampire,
            Entity.Lich, Entity.Ice_Lizard,
            Entity.Makon, Entity.Mammoth,
            Entity.Rift_Glacier_Cloud_Dragon,
            Entity.Sahuagin, Entity.Shadow, Entity.Statue,
            Entity.Thisson, Entity.Titan,
            Entity.Vampire, Entity.Vampire_Lord,
            Entity.Will__o___Wisp
        };

        // Keep in mind Ghod's Hooks is a "curse based spell" as of 10/27/2019
        public static List<Entity> IMMUNE_CURSE = new List<Entity>
        {
            Entity.Archmage,
            Entity.Banshee,
            Entity.Cyprial,
            Entity.Drider, Entity.Dryad, Entity.Drow_Matriarch,
            Entity.Nymph,
            Entity.Ghoul, Entity.Golem, Entity.Great_Schema,
            Entity.High_Elf, Entity.High_Elf_Cdr,
            Entity.Leng_Vampire, Entity.Lich,
            Entity.Makon,
            Entity.Oakvael_Serpent,
            Entity.Phoenix, Entity.Pixie,
            Entity.Sea_Hag, Entity.Skeleton, Entity.Spider_Queen, Entity.Statue,
            Entity.The_Lost_One, Entity.Troll_King, Entity.Oakvael_Iron_Lich,
            Entity.Vampire, Entity.Vampire_Lord,
            Entity.Will__o___Wisp,
            Entity.Kesmai_Crypt_Ghoul
        };

        public static List<Entity> IMMUNE_DEATH = new List<Entity>
        {
            Entity.Dryad,
            Entity.Nymph,
            Entity.Makon,
            Entity.Nixie,
            Entity.Oakvael_Serpent,
            Entity.Oakvael_Iron_Lich,
            Entity.Phoenix,
            Entity.Statue,
            Entity.The_Lost_One, Entity.Troll_King,
            Entity.Vampire, Entity.Vampire_Lord,
            Entity.Viper
        };

        public static List<Entity> IMMUNE_FEAR = new List<Entity>
        {
            Entity.Annwn_Giant, Entity.Apparition, Entity.Axe_Glacier_Giant,
            Entity.Bear,
            Entity.Chuul,
            Entity.Dragon, Entity.Drake, Entity.Drider,
            Entity.Eidolon, Entity.Elemental, Entity.Ent,
            Entity.Giant, Entity.Great_Schema,
            Entity.Illithid,
            Entity.Kesmai_Crypt_Ghoul,
            Entity.Kraken,
            Entity.Lamassu, Entity.Leng_Vampire, Entity.Lich, Entity.Lindwyrm,
            Entity.Makon,
            Entity.Nixie,
            Entity.Oakvael_Serpent, Entity.Oakvael_Iron_Lich, Entity.Overlord,
            Entity.Phoenix,
            Entity.Rat,
            Entity.Presence,
            Entity.Scorpion, Entity.Serpent, Entity.Shambling_Mound, Entity.Spider_Queen, Entity.Statue,
            Entity.The_Lost_One, Entity.Troll_King,
            Entity.Vampire, Entity.Vampire_Lord,
            Entity.Wild_Elf, Entity.Wyrm,
        };

        public static List<Entity> IMMUNE_FIRE = new List<Entity>
        {
            Entity.Dao,
            Entity.Demon,
            Entity.Alu,
            Entity.Cambion,
            Entity.Succubus,
            Entity.Succubus_Prima,
            Entity.Pazuzu,
            Entity.Asmodeous,
            Entity.Thamuz,

            Entity.Annwn_Red_Dragon,
            Entity.Kesmai_Red_Dragon,
            Entity.Leng_Red_Dragon,
            Entity.Efreet, Entity.Elemental,
            Entity.Makon,
            Entity.Nightmare,
            Entity.Oni, Entity.Overlord,
            Entity.Phoenix,
            Entity.Rockworm,
            Entity.Salamander, Entity.Serpent, Entity.Statue,
            Entity.Thisson,
            Entity.Wyrm, Entity.Wyvern
        };

        public static List<Entity> IMMUNE_LIGHTNING = new List<Entity>
        {
            Entity.Axe_Glacier_Lightning_Drake,
            Entity.Rift_Glacier_Lightning_Drake,
            Entity.Rift_Glacier_Cloud_Dragon,
            Entity.Wandering_Lightning_Drake,
            Entity.Elemental,
            Entity.Illithid,
            Entity.Makon,
            Entity.Oakvael_Serpent,
            Entity.Serpent, Entity.Shadow, Entity.Shambling_Mound, Entity.Statue,
            Entity.The_Lost_One, Entity.Thisson, Entity.Troll_King,
            Entity.Wyvern
        };

        public static List<Entity> IMMUNE_POISON = new List<Entity>
        {
            Entity.Broodmother,
            Entity.Green_Dragon,
            Entity.Black_Dragon,
            Entity.Babau,
            Entity.Demon,
            Entity.Dao, Entity.Djinn, Entity.Draugr, Entity.Drider, Entity.Drow_Child, Entity.Drow, Entity.Drow_Master,
            Entity.Efreet,
            Entity.Formicid,
            Entity.Ghoul, Entity.Gojira,
            Entity.Illithid,
            Entity.Kraken,
            Entity.Leng_Vampire, Entity.Lich, Entity.Lindwyrm, Entity.Lurker,
            Entity.Makon, Entity.Marid,
            Entity.Oakvael_Iron_Lich, Entity.Overlord,
            Entity.Phantasm,
            Entity.Rat,
            Entity.Sahuagin, Entity.Scorpion, Entity.Serpent, Entity.Spider, Entity.Spider_Queen, Entity.Statue,
            Entity.The_Lost_One, Entity.Thisson,
            Entity.Vampire, Entity.Vampire_Lord,
            Entity.Wild_Elf, Entity.Wood_Elf, Entity.Wyrm, Entity.Leng_Dungeon_Wyrm,
            Entity.Will__o___Wisp,
            Entity.Kesmai_Crypt_Ghoul
        };

        public static List<Entity> IMMUNE_STUN = new List<Entity>
        {
            Entity.Aquatic_Elf, Entity.Annwn_Phoenix, Entity.Axe_Glacier_Blue_Dragon,
            Entity.Broodmother,
            Entity.Great_Schema,
            Entity.Elemental, Entity.Ent,
            Entity.Kraken,
            Entity.Makon,
            Entity.Oakvael_Iron_Lich, Entity.Oakvael_Wind_Dragon,
            Entity.Spider_Queen, Entity.Sprite,
            Entity.The_Lost_One, Entity.Thisson
        };
        #endregion

        #region Special Immunities
        public static List<Entity> IMMUNE_DRUIDRY = new List<Entity>
        {
            Entity.The_Lost_One
        };

        public static List<Entity> IMMUNE_SORCERY = new List<Entity>
        {
            Entity.Sea_Hag
        };

        public static List<Entity> IMMUNE_THAUMATURGY = new List<Entity>
        {
            Entity.Swordmaster, Entity.Thisson
        };

        public static List<Entity> IMMUNE_WIZARDRY = new List<Entity>
        {
            Entity.Thisson
        };
        #endregion

        /// <summary>
        /// These Entities never leave a corpse and are able to avoid certain attacks.
        /// </summary>
        public static List<Entity> INCORPOREAL = new List<Entity>
        {
            #region Incorporeal Creatures (currently IsSpectral property)
            Entity.Apparition,
            Entity.Banshee,
            Entity.Confessor_Ghost,
            Entity.Draugr,
            Entity.Ghost,
            Entity.Presence,
            Entity.Revenant,
            Entity.Shadow, Entity.Spectre, Entity.Spirit,
            Entity.Wraith,
            Entity.Will__o___Wisp
	        #endregion
        };

        public static List<Entity> INVISIBLE = new List<Entity>
        {
            Entity.Audrey
        };

        /// <summary>
        /// Always lawful alignment unless zPlane alignment overrides.
        /// </summary>
        public static List<Entity> LAWFUL_ALIGNMENT = new List<Entity>
        {
            Entity.Alia, Entity.Archmage, Entity.Aarakocra,
            Entity.Dryad,
            Entity.Eagle, Entity.Ent,
            Entity.Firbolg,
            Entity.High_Elf, Entity.High_Elf_Cdr,
            Entity.Knight,
            Entity.Lamassu,
            Entity.Rhed,
            Entity.Panda,
            Entity.Sprite,
            Entity.Wood_Elf
        };

        public static List<Entity> MAGIC_SNIFFER = new List<Entity>
        {
            Entity.Cyprial,
            Entity.Drake, Entity.Dragon,
            Entity.Ent,
            Entity.Thisson,
            Entity.Ydmos,
            Entity.Kraken,
            Entity.Leng_Dungeon_Wyrm,
            Entity.Rift_Glacier_Cloud_Dragon,
            Entity.Axe_Glacier_Blue_Dragon,
            Entity.Annwn_Red_Dragon,
            Entity.Kesmai_Red_Dragon,
            Entity.Leng_Red_Dragon,
            Entity.Oakvael_Wind_Dragon,
            Entity.Sea_Hag,
            Entity.Spider_Queen,
            Entity.Drow_Matriarch
        };

        // Always male gender.
        public static List<Entity> MALE = new List<Entity>
        {
            Entity.Archmage, Entity.Axe_Glacier_Yeti,
            Entity.Barbarossa,
            Entity.Cambion, Entity.Carfel, Entity.Cyprial,
            Entity.Drow_Master,
            Entity.Great_Schema,
            Entity.King_Wolf,
            Entity.Lizardman,
            Entity.Overlord,
            Entity.Smokey,
            Entity.Thisson, Entity.Titan,
            Entity.Vampire_Lord, Entity.Leng_Vampire,
        };

        /// <summary>
        /// Typically animals with a maul attack that does more damage.
        /// </summary>
        public static List<Entity> MAULER = new List<Entity>
        {
            Entity.Bear, Entity.Bulette,
            Entity.Chimera,
            Entity.Dire_Wolf, Entity.Dog, Entity.Dragon, Entity.Dretch,
            Entity.Gojira,
            Entity.Jaguar,
            Entity.Lindwyrm, Entity.Lion,
            Entity.Manticore,
            Entity.Panda, Entity.Panther,
            Entity.Sabertooth, Entity.Smilodon,
            Entity.Tiger,
            Entity.Wolf, Entity.King_Wolf, Entity.Wyrm, Entity.Leng_Dungeon_Wyrm,

            Entity.Rift_Glacier_Lightning_Drake, Entity.Axe_Glacier_Lightning_Drake, Entity.Leng_Lightning_Drake, Entity.Wandering_Lightning_Drake,

            Entity.Annwn_Red_Dragon, Entity.Kesmai_Red_Dragon, Entity.Oakvael_Wind_Dragon, Entity.Axe_Glacier_Blue_Dragon,
            Entity.Leng_Red_Dragon, Entity.Rift_Glacier_Cloud_Dragon,

            Entity.Amethyst_Dragon, // neutral
            Entity.Crystal_Dragon, // neutral
            Entity.Emerald_Dragon, // neutral
            Entity.Sapphire_Dragon, // neutral
            Entity.Topaz_Dragon, // neutral
            Entity.Black_Dragon, // chaotic evil
            Entity.Blue_Dragon, // chaotic evil
            Entity.Brass_Dragon, // lawful
            Entity.Bronze_Dragon, // lawful
            Entity.Copper_Dragon, // lawful
            Entity.Gold_Dragon, // lawful
            Entity.Green_Dragon, // chaotic evil
            Entity.Red_Dragon, // chaotic evil
            Entity.Silver_Dragon, // lawful
            Entity.White_Dragon, // chaotic evil
        };

        /// <summary>
        /// Typically spawns female entities. May have another use for this in the future. 12/3/2015 Eb
        /// </summary>
        public static List<Entity> MATRIARCHAL = new List<Entity>
        {
            Entity.Drider, Entity.Dryad, Entity.Nymph, Entity.Sprite,
        };

        /// <summary>
        /// Instantiated as a Merchant class vice NPC.
        /// </summary>
        public static List<Entity> MERCHANTS = new List<Entity>
        {
            Entity.Animal_Trainer, Entity.Laurelena, Entity.Rhed
        };

        /// <summary>
        /// NPCs typically immobile are forced into mobility.
        /// </summary>
        public static List<Entity> MOBILE = new List<Entity>()
        {

        };

        public static List<Entity> TRAINER_ANIMAL = new List<Entity>
        {
            Entity.Animal_Trainer,
        };

        /// <summary>
        /// Profession is determined in EntityBuilder.DetermineProfession.
        /// </summary>
        public static List<Entity> TRAINER_SPELLS = new List<Entity>
        {
            Entity.Laurelena, Entity.Rhed,
        };

        public static List<Entity> MENTOR = new List<Entity>
        {
            Entity.Laurelena, Entity.Rhed,
        };

        /// <summary>
        /// This will override the coexistent variable of a ZPlane. Natural enemies attack each other no matter what other factors are involved.
        /// </summary>
        public static List<Tuple<Entity, Entity>> NATURAL_ENEMIES = new List<Tuple<Entity, Entity>>
        {
            // List of tupled enemies.
            new Tuple<Entity, Entity>(Entity.Aarakocra, Entity.Harpy),
            new Tuple<Entity, Entity>(Entity.Eagle, Entity.Raven),
            new Tuple<Entity, Entity>(Entity.Dire_Rabbit, Entity.Dire_Wolf),
            new Tuple<Entity, Entity>(Entity.Wild_Elf, Entity.High_Elf)
        };

        /// <summary>
        /// Always spawn as neutral alignment unless zPlane is set to override.
        /// </summary>
        public static List<Entity> NEUTRAL_ALIGNMENT = new List<Entity>
        {
            Entity.Animal_Trainer,
            Entity.Drow_Child,
            Entity.Dire_Rabbit, Entity.Fox,
            Entity.Gnome, Entity.Great_Schema,
            Entity.Laurelena,
            Entity.Pixie, Entity.Nixie, Entity.Satyr,
            Entity.Wild_Elf
        };

        public static List<Entity> PINCERED = new List<Entity>
        {
            Entity.Beetle, Entity.Chuul, Entity.Formicid, Entity.Kraken, Entity.Scorpion, Entity.The_Lost_One
        };

        /// <summary>
        /// Typically immobile, and susceptible to specific attacks.
        /// </summary>
        public static List<Entity> PLANT = new List<Entity>
        {
            Entity.Audrey,
            Entity.Briarvex,
            Entity.Yasai_kyofu
        };

        /// <summary>
        /// These Entities have possibly poison attacks, either via special attacks or otherwise.
        /// </summary>
        public static List<Entity> POISONOUS = new List<Entity>
        {
            #region Poisonous Creatures
            Entity.Axe_Glacier_Lightning_Drake,
		    Entity.Babau, Entity.Beetle, Entity.Briarvex, Entity.Broodmother,
            Entity.Chasme, Entity.Chuul,
            Entity.Drider, Entity.Drake,
            Entity.Gargoyle, Entity.Ghoul, Entity.Green_Dragon,
            Entity.Kesmai_Crypt_Ghoul,
            Entity.Kraken,
            Entity.Leng_Dungeon_Wyrm, Entity.Leng_Lightning_Drake, Entity.Lich, Entity.Lindwyrm, Entity.Lurker,
            Entity.Manticore, Entity.Mutated_Gator,
            Entity.Rat, Entity.Rift_Glacier_Lightning_Drake,
            Entity.Scorpion, Entity.Serpent, Entity.Snake, Entity.Snakeman, Entity.Spider, Entity.Spider_Queen,
            Entity.The_Lost_One,
            Entity.Viper,
            Entity.Wandering_Lightning_Drake, Entity.Wyrm, 
            Entity.Yasai_kyofu,
	        #endregion
        };

        public static List<Entity> RANDOM_NAME = new List<Entity>
        {
            Entity.Animal_Trainer,
        };

        /// <summary>
        /// Cold-blooded Entities able to move through water. Currently use the same sound files (Yuusha client).
        /// </summary>
        public static List<Entity> REPTILIAN = new List<Entity>
        {
            #region Reptilian Creatures
		    Entity.Chuul, Entity.Cobra, Entity.Crocodile,
            Entity.Ice_Lizard, Entity.Lizardman,
            Entity.Mutated_Gator,
            Entity.Oakvael_Serpent,
            Entity.Serpent, Entity.Snake, Entity.Snakeman,
            Entity.Thisson,
            Entity.Viper,
            Entity.Yaun__Ti
	        #endregion
        };

        /// <summary>
        /// Entities with a smash attack, and typically superior strength, that can stun and/or move an enemy.
        /// </summary>
        public static List<Entity> SMASHER = new List<Entity>
        {
            Entity.Bear, Entity.Bighorn, Entity.Bloodhulk, Entity.Boadkin,
            Entity.Chuul,
            Entity.Ent, Entity.Ettin, Entity.Firbolg,
            Entity.Giant, Entity.Giantkin, Entity.Giantlord, Entity.Gojira, Entity.Golem,
            Entity.Kraken,
            Entity.Makon,
            Entity.Nalfeshnee,
            Entity.Owlbear,
            Entity.Shambling_Mound,
            Entity.Axe_Glacier_Giant, Entity.Axe_Glacier_Yeti,
            Entity.The_Lost_One
        };

        /// <summary>
        /// These Entities typically work together to hunt prey or take down an enemy.
        /// </summary>
        public static List<Entity> SOCIAL = new List<Entity>
        {
            #region Social Creatures
            Entity.Aarakocra, Entity.Aquatic_Elf,
            Entity.Banshee, Entity.Barbarian, Entity.Beetle, Entity.Berserker, Entity.Boar, Entity.Bugbear,
            Entity.Centaur,
            Entity.Dire_Rabbit, Entity.Dire_Wolf, Entity.Drow_Child, Entity.Drow, Entity.Dryad,
            Entity.Firbolg, Entity.Formicid,
            Entity.Gargoyle, Entity.Gnoll, Entity.Goblin, Entity.Grey_Elf, Entity.Griffin,
            Entity.Harpy, Entity.Hellhound, Entity.High_Elf, Entity.Hobgoblin, Entity.Hyena,
            Entity.Kobold,
            Entity.Lamassu, Entity.Lion, Entity.Lizardman,
            Entity.Moldeus,
            Entity.Nalfeshnee,
            Entity.Orc,
            Entity.Piranha,
            Entity.Bighorn, Entity.Rat, Entity.Raven,
            Entity.Sahuagin, Entity.Satyr, Entity.Scorpion, Entity.Skeleton, Entity.Smilodon,
            Entity.Tengu, Entity.Tiger, Entity.Troglodyte, Entity.Troll,
            Entity.Velociraptor,
            Entity.Wild_Elf, Entity.Wolf, Entity.Wood_Elf, Entity.Wraith, Entity.Wyvern,
            Entity.Zombie
	        #endregion
        };

        public static List<Entity> STINGER = new List<Entity>
        {
            Entity.Chasme, Entity.Drake, Entity.Formicid, Entity.Scorpion
        };

        /// <summary>
        /// Summoned from a plane other than the Prime Material. Affected by the Banish spell.
        /// Drop all items acquires on the Prime
        /// Material Plane.
        /// </summary>
        public static List<Entity> SUMMONED = new List<Entity>
        {
            //Entity.Demon,
            Entity.Dao, Entity.Djinn,
            Entity.Eidolon,
            Entity.Efreet, Entity.Elemental,
            Entity.Hellhound,
            //Entity.Cambion,
            //Entity.Alu,
            //Entity.Babau,
            //Entity.Rutterkin,
            //Entity.Dretch,
            //Entity.Asmodeous,
            //Entity.Pazuzu,
            //Entity.Glamdrang,
            //Entity.Samael,
            //Entity.Damballa,
            //Entity.Perdurabo,
            Entity.Marid,
            Entity.Phantasm,
            //Entity.Thamuz,
            //Entity.Chasme,
            //Entity.Bar__lgura,
            Entity.Will__o___Wisp,
            Entity.Lamassu,
            Entity.Golem
        };

        /// <summary>
        /// These entities summon players/targets if they are in their flagged list.
        /// </summary>
        public static List<Entity> SUMMONER = new List<Entity>
        {
            Entity.Archmage, Entity.Cyprial, Entity.Drow_Matriarch, Entity.Great_Schema, Entity.Sea_Hag, Entity.Thisson, Entity.Ydmos
        };

        public static List<Entity> SUPERIOR_HEALTH = new List<Entity>
        {
            Entity.Archmage,
            Entity.Annwn_Phoenix,
            Entity.Annwn_Red_Dragon,
            Entity.Broodmother,
            Entity.Cyprial,
            Entity.Drow_Master,
            Entity.Drow_Matriarch,
            Entity.Gojira, Entity.Great_Schema,
            Entity.High_Elf_Cdr,
            Entity.Illithid_Elder,
            Entity.Kesmai_Crypt_Ghoul,
            Entity.King_Wolf,
            Entity.Kraken,
            Entity.Makon,
            Entity.Rift_Glacier_Cloud_Dragon, Entity.Rift_Glacier_Lightning_Drake,
            Entity.Leng_Dungeon_Wyrm,
            Entity.Wandering_Lightning_Drake,
            Entity.Sea_Hag,
            Entity.Spider_Queen,
            Entity.The_Lost_One,
            Entity.Titan,
            Entity.Vampire_Lord,

            Entity.Nightmare
        };

        public static List<Entity> SUPERIOR_MANA = new List<Entity>
        {
            Entity.Archmage,
            Entity.Cyprial,
            Entity.Drow_Matriarch,
            Entity.Sea_Hag,
            Entity.Spider_Queen,
            Entity.Ydnac,
            Entity.Ydmos
        };

        public static List<Entity> SUS = new List<Entity>
        {
            Entity.Boar, Entity.Nalfeshnee
        };

        public static List<Entity> SUSCEPTIBLE_FIRE = new List<Entity>
        {
            Entity.Audrey,
            Entity.Axe_Glacier_Giant,
            Entity.Axe_Glacier_Yeti,
            Entity.Briarvex,
            Entity.Formicid,
            Entity.Ice_Lizard,
            Entity.Troll,
            Entity.Troll_King,
            Entity.Yasai_kyofu
        };

        public static List<Entity> SUSCEPTIBLE_ICE = new List<Entity>
        {
            Entity.Elemental,
            Entity.Salamander,
        };

        /// <summary>
        /// Attack with a tail that may be used as a piercing weapon (typically poisoned).
        /// </summary>
        public static List<Entity> TAILPIERCER = new List<Entity>
        {
            Entity.Broodmother,
            Entity.Drake,
            Entity.Manticore
        };

        /// <summary>
        /// Attack with their tails as a typically blunt weapon.
        /// </summary>
        public static List<Entity> TAILWHIPPER = new List<Entity>
        {
            Entity.Chimera, Entity.Crocodile,
            Entity.Dragon, Entity.Drake,
            Entity.Manticore, Entity.Mutated_Gator,
            Entity.Wyrm, Entity.Leng_Dungeon_Wyrm,

            Entity.Blue_Dragon, Entity.Brass_Dragon, Entity.Bronze_Dragon, Entity.Copper_Dragon, Entity.Gold_Dragon, Entity.Green_Dragon, Entity.Red_Dragon,
            Entity.Silver_Dragon, Entity.White_Dragon,

            Entity.Annwn_Red_Dragon, Entity.Axe_Glacier_Blue_Dragon, Entity.Kesmai_Red_Dragon, Entity.Leng_Red_Dragon, Entity.Oakvael_Wind_Dragon, Entity.Rift_Glacier_Cloud_Dragon,

            Entity.Annwn_Phoenix,

            Entity.Gojira,
        };

        public static List<Entity> TENTACLED = new List<Entity>
        {
            Entity.Chuul, Entity.Illithid, Entity.Kraken, Entity.Lurker
        };

        /// <summary>
        /// Attack with their talons as a slashing or piercing weapon.
        /// </summary>
        public static List<Entity> TALONED = new List<Entity>
        {
            Entity.Griffin, Entity.Hippogriff,
            Entity.Eagle,
            Entity.Drake, Entity.Axe_Glacier_Lightning_Drake, Entity.Leng_Lightning_Drake, Entity.Wandering_Lightning_Drake,
            Entity.Phoenix,
            Entity.Raven,
            Entity.Velociraptor,
            Entity.Vrock,
        };

        /// <summary>
        /// Attack with their tusks as blunt and piercing weapons.
        /// </summary>
        public static List<Entity> TUSKED = new List<Entity>
        {
            Entity.Mammoth, Entity.Nalfeshnee, Entity.The_Lost_One
        };

        /// <summary>
        /// Spawn without weapons. Could be humanoids, they just prefer to fight with their appendages.
        /// </summary>
        public static List<Entity> UNARMED_PREFERRED = new List<Entity>
        {
            Entity.Elemental, Entity.Ent, Entity.Golem, Entity.Martial_Artist, Entity.Ninja, Entity.Shambling_Mound, Entity.Yeti, Entity.Axe_Glacier_Yeti
        };

        /// <summary>
        /// Undead entities are more prone to certain spells and effects.
        /// </summary>
        public static List<Entity> UNDEAD = new List<Entity>
        { 
            #region Undead Creatures
		    Entity.Apparition,
            Entity.Banshee, Entity.Bloodhulk,
            Entity.Confessor_Ghost,
            Entity.Draugr,
            Entity.Ghast, Entity.Ghost, Entity.Ghoul,
            Entity.Lich,
            Entity.Mummy,
            Entity.Presence,
            Entity.Revenant,
            Entity.Shadow, Entity.Skeleton, Entity.Spectre, Entity.Spirit, Entity.Stalker, Entity.Swordmaster,
            Entity.Vampire, Entity.Vampire_Lord, Entity.Leng_Vampire,
            Entity.Waft, Entity.Wight, Entity.Wraith,
            Entity.Zombie,

            Entity.Kesmai_Crypt_Ghoul
	        #endregion
        };

        /// <summary>
        /// Entities that will never leave water.
        /// </summary>
        public static List<Entity> WATER_DWELLER = new List<Entity>
        {
            Entity.Aquatic_Elf,
            Entity.Kraken,
            Entity.Lurker,
            Entity.Piranha,
            Entity.Shark
        };

        public static List<Entity> WEAK = new List<Entity>
        {
            Entity.Drow_Child, Entity.Fox
        };

        // Entities with the abilitiy to cast/create webs.
        public static List<Entity> WEB_DWELLERS = new List<Entity>
        {
            Entity.Drider,
            Entity.Spider,
            Entity.Spider_Queen,
            Entity.Shelob
        };

        /// <summary>
        /// Entities that have a wing buffeting attack (moves target on a successful attack).
        /// </summary>
        public static List<Entity> WING_BUFFETER = new List<Entity>
        {
            Entity.Cambion,
            Entity.Vrock,
            Entity.Dragon, Entity.Annwn_Red_Dragon, Entity.Axe_Glacier_Blue_Dragon, Entity.Kesmai_Red_Dragon, Entity.Leng_Red_Dragon, Entity.Oakvael_Wind_Dragon,
            Entity.Rift_Glacier_Cloud_Dragon,

            Entity.Blue_Dragon, Entity.Brass_Dragon, Entity.Bronze_Dragon, Entity.Copper_Dragon, Entity.Gold_Dragon, Entity.Green_Dragon, Entity.Red_Dragon,
            Entity.Silver_Dragon, Entity.White_Dragon,

            Entity.Annwn_Phoenix
        };

        /// <summary>
        /// Entities that can block with a wing and spawn in the air.
        /// </summary>
        public static List<Entity> WINGED = new List<Entity>
        {
            Entity.Aarakocra, Entity.Alu,
            Entity.Cambion, Entity.Chasme,
            Entity.Eagle,
            Entity.Dragon, Entity.Annwn_Red_Dragon, Entity.Axe_Glacier_Blue_Dragon, Entity.Kesmai_Red_Dragon, Entity.Leng_Red_Dragon, Entity.Oakvael_Wind_Dragon,
            Entity.Harpy,
            Entity.Griffin, Entity.Hippogriff,
            Entity.Manticore,
            Entity.Nalfeshnee,
            Entity.Raven, Entity.Rift_Glacier_Cloud_Dragon,
            Entity.Succubus, Entity.Succubus_Prima,
            Entity.Vrock,

            Entity.Blue_Dragon, Entity.Brass_Dragon, Entity.Bronze_Dragon, Entity.Copper_Dragon, Entity.Gold_Dragon, Entity.Green_Dragon, Entity.Red_Dragon,
            Entity.Silver_Dragon, Entity.White_Dragon,

            Entity.Annwn_Phoenix
        };

        public static List<Entity> BLUEGLOW_REQUIRED = new List<Entity>
        {
            Entity.Annwn_Phoenix, Entity.Annwn_Red_Dragon, Entity.Asmodeous,
            Entity.Axe_Glacier_Lightning_Drake, Entity.Axe_Glacier_Yeti,
            Entity.Carfel,
            Entity.Elemental, Entity.Ent,
            Entity.Hellhound, Entity.Illithid, Entity.Illithid_Elder,
            Entity.Gojira,
            Entity.Kraken,
            Entity.Lamassu, Entity.Leng_Lightning_Drake, Entity.Leng_Dungeon_Wyrm, Entity.Leng_Red_Dragon,
            Entity.Oakvael_Iron_Lich, Entity.Oakvael_Serpent, Entity.Oakvael_Wind_Dragon,
            Entity.Rift_Glacier_Lightning_Drake,
            Entity.Succubus, Entity.Succubus_Prima, Entity.Swordmaster,
            Entity.Will__o___Wisp,
            Entity.Spider_Queen
        };

        // Do not pick anything up or wield weapons. Simply an abberation or beast that has higher than animal intelligence.
        public static List<Entity> NO_HANDS = new List<Entity> { Entity.Beholder };

        // Do not wear armor. Not animals. Simply an abberation or beast that has higher than animal intelligence.
        public static List<Entity> NO_ARMOR = new List<Entity> { Entity.Beholder, Entity.Ent, Entity.Shambling_Mound };

        // Only blunt weapons harm these entities.
        public static List<Entity> ONLYBLUNT_REQUIRED = new List<Entity> { Entity.Gargoyle };

        // Blunt weapons do not harm these entities.
        public static List<Entity> NOBLUNT_REQUIRED = new List<Entity>
        { Entity.Beholder, Entity.Ent, Entity.Gojira, Entity.Kraken, Entity.Lurker, Entity.Mummy, Entity.Shambling_Mound };

        // Piercing weapons do not harm these entities.
        public static List<Entity> NOPIERCE_REQUIRED = new List<Entity> { Entity.Skeleton, Entity.Swordmaster };

        // Silver weapon required to hit. Includes mithril silver.
        public static List<Entity> SILVER_REQUIRED = new List<Entity>
        {
            Entity.King_Wolf, Entity.Leng_Vampire, Entity.Pazuzu, Entity.Succubus,
            Entity.Vampire, Entity.Vampire_Lord,
            Entity.Wyrm
        };

        // Mithril silver weapons required to hit. Does not include regular silver weapons.
        public static List<Entity> MITHRIL_SILVER_REQUIRED = new List<Entity>
        { };

        // Lawful weapon required to hit.
        public static List<Entity> LAWFUL_REQUIRED = new List<Entity>
        {
            Entity.The_Lost_One,
        };

        /// <summary>
        /// Entities with a specific weapon requirement. Only one weapon will harm them.
        /// </summary>
        public static Dictionary<Entity, List<string>> WEAPON_REQUIREMENT = new Dictionary<Entity, List<string>>()
        {
            {Entity.Axe_Glacier_Blue_Dragon, new List<string> { "greataxe"} },
            {Entity.Makon, new List<string> { "menuki"} },
            {Entity.Ydmos, new List<string> { "swordoflight"} },
            {Entity.Overlord, new List<string> { "nomelee"} },
            {Entity.Rift_Glacier_Cloud_Dragon, new List<string> { "ulfang"} },
            //{Entity.Thisson, new List<string> {"unnamedweapon"} }
        };

        public static List<Entity> WYRMKIN = new List<Entity>
        {
            Entity.Amethyst_Dragon, // neutral
            Entity.Crystal_Dragon, // neutral
            Entity.Emerald_Dragon, // neutral
            Entity.Sapphire_Dragon, // neutral
            Entity.Topaz_Dragon, // neutral
            Entity.Black_Dragon, // chaotic evil
            Entity.Blue_Dragon, // chaotic evil
            Entity.Brass_Dragon, // lawful
            Entity.Bronze_Dragon, // lawful
            Entity.Copper_Dragon, // lawful
            Entity.Gold_Dragon, // lawful
            Entity.Green_Dragon, // chaotic evil
            Entity.Red_Dragon, // chaotic evil
            Entity.Silver_Dragon, // lawful
            Entity.White_Dragon, // chaotic evil

            Entity.Broodmother,

            Entity.Dragon,
            Entity.Annwn_Red_Dragon, Entity.Axe_Glacier_Blue_Dragon, Entity.Kesmai_Red_Dragon, Entity.Leng_Red_Dragon, Entity.Oakvael_Wind_Dragon, Entity.Rift_Glacier_Cloud_Dragon,
            Entity.Drake, Entity.Axe_Glacier_Lightning_Drake, Entity.Leng_Lightning_Drake, Entity.Rift_Glacier_Lightning_Drake, Entity.Wandering_Lightning_Drake,
            Entity.Wyrm, Entity.Leng_Dungeon_Wyrm,

            Entity.Lindwyrm,
        };

        /// <summary>
        /// These nasties eat those they kill.
        /// </summary>
        public static List<Entity> EATERS = new List<Entity>
        {
            Entity.Axe_Glacier_Blue_Dragon, Entity.Rift_Glacier_Cloud_Dragon, Entity.Gojira, Entity.Kraken, Entity.Leng_Red_Dragon, Entity.Annwn_Red_Dragon
        };

        // Lists containing lists of Entities that will not flee in battle.
        public static List<List<Entity>> FEARLESS = new List<List<Entity>>
        {
            EATERS, WYRMKIN, UNDEAD, SUMMONED, PLANT, MAGIC_SNIFFER, DEMONS, NAMED_DEMONS, CHAOTIC_EVIL_ALIGNMENT, UNIQUE, new List<Entity>() { Entity.Ent, Entity.Sea_Hag, }
        };


        // Lists containing lists of Entities that can attack from 1 cell away.
        public static List<List<Entity>> LONGARMED = new List<List<Entity>>
        {
            PLANT, TENTACLED, GIANT_KIN,
            new List<Entity>() { Entity.Ent, Entity.Gojira, Entity.Golem, Entity.Merrow, Entity.Ogre, Entity.Shambling_Mound, Entity.Yeti, Entity.Axe_Glacier_Yeti, Entity.The_Lost_One }
        };

        // Movement speed of 2.
        public static List<Entity> LUMBERING = new List<Entity>
        {
            Entity.Kobold,
            Entity.Shambling_Mound,
        };

        // Animals allowed to wield weapons.
        public static List<Entity> ANIMALS_WIELDING_WEAPONS = new List<Entity>
        {
            Entity.Gargoyle, Entity.Smokey
        };

        #region Talents
        #region Martial Artist Talents
        public static List<Entity> TALENT_FLYINGFURY = new List<Entity>
        {
            Entity.Great_Schema,
            Entity.Ninja,
            Entity.Succubus_Prima,
        };

        public static List<Entity> TALENT_MEMORIZE = new List<Entity>
        {
            Entity.Archmage, Entity.Cyprial, Entity.Drow_Master, Entity.Drow_Matriarch, Entity.Illithid_Elder, Entity.Sea_Hag, Entity.Spider_Queen, Entity.Ydmos
        };

        public static List<Entity> TALENT_LEGSWEEP = new List<Entity>
        {
            Entity.Great_Schema,
            Entity.Ninja,
            Entity.Martial_Artist,
            Entity.Succubus_Prima,
        };

        public static List<Entity> TALENT_RAPIDKICKS = new List<Entity>
        {
            Entity.Great_Schema,
            Entity.Ninja,
            Entity.Succubus_Prima,
        };

        public static List<Entity> TALENT_ROUNDHOUSEKICK = new List<Entity>
        {
            Entity.Great_Schema,
            Entity.Ninja,
            Entity.Succubus_Prima,
        };
        #endregion

        #region Multi Class (Profession) Talents
        public static List<Entity> TALENT_CHARGE = new List<Entity>
        {
            Entity.Barbarossa,
            Entity.Berserker,
            Entity.High_Elf_Cdr,
            Entity.Fighter,
            Entity.Knight,
            Entity.Ravager,
            Entity.The_Lost_One,
        };

        public static List<Entity> TALENT_CLEAVE = new List<Entity>
        {
            Entity.Barbarossa,
            Entity.Barbarian,
            Entity.Berserker,
            Entity.High_Elf_Cdr,
            Entity.Fighter,
            Entity.Knight,
            Entity.Ravager,
            Entity.The_Lost_One,
            Entity.Troll_King,
            Entity.Titan,
        };

        public static List<Entity> TALENT_DUALWIELD = new List<Entity>
        {
            Entity.Barbarossa,
            Entity.Carfel,
            Entity.Great_Schema,
            Entity.High_Elf_Cdr,
            Entity.Swordmaster,
            Entity.Drow_Master,
            Entity.Drow_Matriarch,
            Entity.Fighter,
            Entity.The_Lost_One,
        };

        public static List<Entity> TALENT_DOUBLEATTACK = new List<Entity>
        {
            Entity.Carfel,
            Entity.Barbarossa,
            Entity.Great_Schema,
            Entity.High_Elf_Cdr,
            Entity.Swordmaster,
            Entity.Drow_Master,
            Entity.Thisson,
            Entity.Yeti,
            Entity.Axe_Glacier_Giant,
            Entity.Annwn_Giant,
            Entity.King_Wolf,
            Entity.Broodmother,
            Entity.Nightmare,
            Entity.Makon,
            Entity.Succubus_Prima,
            Entity.The_Lost_One,
        };

        public static List<Entity> TALENT_RIPOSTE = new List<Entity>
        {
            Entity.Carfel,
            Entity.Barbarossa,
            Entity.Great_Schema,
            Entity.High_Elf_Cdr,
            Entity.Thief,
            Entity.Swordmaster,
            Entity.Fighter,
            Entity.Drow_Master,
            Entity.Troll_King,
            Entity.Thisson,
        };
        #endregion

        #region Fighter Talent
        public static List<Entity> TALENT_SHIELDBASH = new List<Entity>
        {
            Entity.Berserker,
            Entity.Fighter,
        };
        #endregion

        #region Thief Talents
        public static List<Entity> TALENT_ASSASSINATE = new List<Entity>
        {
            Entity.Drow_Master
        };

        public static List<Entity> TALENT_BACKSTAB = new List<Entity>
        {
            Entity.Babau,
            Entity.Carfel,
            Entity.Drow_Master,
        };

        public static List<Entity> TALENT_DAGGERSTORM = new List<Entity>
        {
            Entity.Carfel,
            Entity.Drow_Master,
        };

        public static List<Entity> TALENT_STEAL = new List<Entity>
        {
            Entity.Archer,
            Entity.Thief,
            Entity.Babau,
            Entity.Demon,
        };
        #endregion

        #endregion

        #region Static Methods
        /// <summary>
        /// Determine broad groups of EntityLists such as EATERS (death of target results in Underworld), FEARLESS (do not run from enemies) and LONGARMED (reaching into nearby Cells).
        /// </summary>
        /// <param name="entityList">The list of entities to iterate through.</param>
        /// <param name="entity">The entity being searched for in the entityList.</param>
        /// <returns></returns>
        public static bool EntityListContains(List<List<Entity>> entityList, Entity entity)
        {
            foreach (List<Entity> eList in entityList)
            {
                if (eList.Contains(entity))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Used for comparing one entity to another. Notably whether an entity is a NATURAL_ENEMY of another entity. First example of this is harpy and aarakocra.
        /// </summary>
        /// <param name="entityList">The list of tupled entities to compare.</param>
        /// <param name="entity1">First entity.</param>
        /// <param name="entity2">Second entity.</param>
        /// <returns></returns>
        public static bool TupledEntityListContains(List<Tuple<Entity, Entity>> entityList, Entity entity1, Entity entity2)
        {
            // Looking for paired entities. If the entities are the same it should not be intended. Return false.
            if (entity1 == entity2) return false;

            foreach (Tuple<Entity, Entity> pairedEntities in entityList)
            {
                if ((pairedEntities.Item1 == entity1 || pairedEntities.Item1 == entity2) && (pairedEntities.Item2 == entity1 || pairedEntities.Item2 == entity2))
                    return true;
            }

            return false;
        }

        public static bool IsAnimal(Entity entity)
        {
            return ANIMAL.Contains(entity) || ANIMAL_SMALL.Contains(entity) || ANIMALS_WIELDING_WEAPONS.Contains(entity);
        }

        /// <summary>
        /// Will wear armor and wield weapons. Assigned a gender. As of March 2018 demons spawn with armor and drop it.
        /// The gates of hell are open.
        /// </summary>
        /// <returns></returns>
        public static bool IsHumanOrHumanoid(Character chr)
        {
            return chr is PC || HUMAN.Contains(chr.entity) || HUMANOID.Contains(chr.entity) || FEY.Contains(chr.entity)
                || chr.species == Globals.eSpecies.Human || ELVES.Contains(chr.entity) || DEMONS.Contains(chr.entity) || IsGiantKin(chr);
        }

        /// <summary>
        /// Will wear armor and wield weapons. Assigned a gender. As of March 2018 demons spawn with armor and drop it.
        /// The gates of hell are open.
        /// </summary>
        /// <returns></returns>
        public static bool IsHumanOrHumanoid(Entity entity)
        {
            return HUMAN.Contains(entity) || HUMANOID.Contains(entity) || FEY.Contains(entity) || ELVES.Contains(entity) ||
                DEMONS.Contains(entity) || GIANT_KIN.Contains(entity);
        }

        public static bool IsHuman(Character chr)
        {
            return HUMAN.Contains(chr.entity) || chr.species == Globals.eSpecies.Human;
        }

        public static bool IsElvish(Character chr)
        {
            return ELVES.Contains(chr.entity) || chr.species == Globals.eSpecies.Elvish;
        }

        public static bool IsHellspawn(Character chr)
        {
            return DEMONS.Contains(chr.entity) || NAMED_DEMONS.Contains(chr.entity) || chr.species == Globals.eSpecies.Demon;
        }

        public static bool IsMerchant(Entity entity)
        {
            return TRAINER_ANIMAL.Contains(entity) || TRAINER_SPELLS.Contains(entity) || MENTOR.Contains(entity);
        }

        /// <summary>
        /// Predominately male gender. Addition to MATRIARCHAL EntityList would trump this. Will wear armor and wield weapons.
        /// Perceived strength in AI assessments is higher (more dangerous). Cannot be knocked down due to physical attacks, as of 12/4/2015 Eb.
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        public static bool IsGiantKin(Character chr)
        {
            return GIANT_KIN.Contains(chr.entity) || chr.species == Globals.eSpecies.Giantkin;
        }

        public static bool IsFullBloodedWyrmKin(Character chr)
        {
            return WYRMKIN.Contains(chr.entity) || chr.entity.ToString().ToLower().EndsWith("dragon") || chr.entity.ToString().ToLower().EndsWith("drake");
        }

        public static bool HasNightvision(Entity entity)
        {
            return WYRMKIN.Contains(entity) || UNDEAD.Contains(entity) || ELVES.Contains(entity);
        }

        // not affected by Ghod's Hooks or Ensnare
        public static bool IsPhysicallyMassive(Character chr)
        {
            return WYRMKIN.Contains(chr.entity) || IsFullBloodedWyrmKin(chr)
                || GIANT_KIN.Contains(chr.entity) || GRIFFIN_ARCHETYPE.Contains(chr.entity)
                || SMASHER.Contains(chr.entity) || chr.entity == Entity.The_Lost_One;
        }
        #endregion
    }
}
