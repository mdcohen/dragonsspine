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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Timers;
using Cell = DragonsSpine.GameWorld.Cell;
using World = DragonsSpine.GameWorld.World;

namespace DragonsSpine
{
    public class Item
    {
        /// <summary>
        /// Item names that can be thrown from a belt using "shoot" or "throw" commands. The order of this array is insignificant.
        /// </summary>
        public static string[] ThrowFromBelt = new string[]
        { "boomerang", "dagger", "dart", "shuriken", "shortsword", "axe", "hammer" };

        public const string BLUEGLOW = "blueglow"; public const string SILVER = "silver"; public const string NOPIERCE = "nopierce";
        public const string ONLYBLUNT = "onlyblunt"; public const string NOBLUNT = "noblunt"; public const string SLASH = "slash";
        public const string LORE = "lore"; public const string EXTRAPLANAR = "extraplanar";

        public static Dictionary<int, System.Data.DataRow> ItemDictionary = new Dictionary<int, System.Data.DataRow>();

        public const short NUM_FOR_UNLIMITED_CHARGES = 101;

        #region Item ID Constants
        public const int ID_BOOTS_OF_SPEED = 1;
        public const int ID_HEALTH_REGENERATION_RING = 2;

        public const int ID_COINS = 30000;
        public const int ID_SPELLBOOK = 31000;
        public const int ID_BLOODWOOD_TOTEM = 40100; // sorcerer's totem of Nergal
        public const int ID_TRUESPIRIT_TOTEM = 40101; // druid's totem
        public const int ID_WORN_BLANK_SCROLL = 31090;
        public const int ID_RAVAGERRING = 13998;
        public const int ID_KNIGHTRING = 13999;
        public const int ID_RECALLRING = 13990;
        public const int ID_GOLDRING = 13100;
        public const int ID_BALMBERRY = 33020;
        public const int ID_POISONBERRY = 33021;
        public const int ID_MANABERRY = 33022;
        public const int ID_STAMINABERRY = 33023;
        public const int ID_GREENSPRIG = 33030;
        public const int ID_MUGWORT = 20090;
        public const int ID_BALM = 20000;
        public const int ID_CORPSE = 30001;
        public const int ID_TIGERSEYE = 30130;
        public const int ID_SUMMONEDMOB = 901;
        public const int ID_TIGERFIG = 30010;
        public const int ID_GRIFFINFIG = 30020;
        public const int ID_DRAKEFIG = 30040;
        public const int ID_DRAGONFIG = 30030;
        public const int ID_EBONSNAKESTAFF = 24502;
        public const int ID_SMOKEYS_SHOVEL = 6050;
        public const int ID_RHAMMER_CHAOTIC = 1217;
        public const int ID_RDAGGER_EVIL = 1218;
        public const int ID_UNCUTPAINITE = 30343;
        public const int ID_RESERVED_FOR_PLAYER_CORPSE = 600000;
        public const int ID_DAZZLING_TSAVORITE = 30341;
        public const int ID_TITANS_MAUL = 29035;
        public const int ID_STORM_DRAGON_SCALES = 8111;
        public const int ID_HUMMINGBIRD_AMULET = 1205;
        public const int ID_HUMMINGBIRD_LONGSWORD_EVIL = 1214;
        public const int ID_HUMMINGBIRD_LONGSWORD_NEUTRAL = 1213;
        public const int ID_HUMMINGBIRD_LONGSWORD_LAWFUL = 1209;
        public const int ID_PRINCESS_CELL_KEY = 1211;
        public const int ID_LOCKPICK = 33001;
        public const int ID_GOLD_NUGGET = 30460;
        public const int ID_COPPER_ORE = 30471;
        public const int ID_SILVER_ORE = 30472;
        public const int ID_GOLD_ORE = 30473;
        public const int ID_THOR_HAMMER = 29022;
        public const int ID_WOODEN_SHIELD = 100;
        public const int ID_IRON_SHIELD = 101;
        public const int ID_STEEL_KITE_SHIELD = 102;

        // torso
        public const int ID_LEATHER_TUNIC = 8010;
        public const int ID_STUDDED_LEATHER_TUNIC = 8011;
        public const int ID_CHAINMAIL_TUNIC = 8015;
        public const int ID_SCALEMAIL_TUNIC = 8016;
        public const int ID_BANDED_MAIL_TUNIC = 8020;
        public const int ID_STEEL_BREASTPLATE = 8021;
        public const int ID_FIRE_SALAMANDER_SCALE_VEST = 8102;

        // legs
        public const int ID_LEATHER_LEGGINGS = 15010;
        public const int ID_STUDDED_LEATHER_LEGGINGS = 15011;
        public const int ID_CHAINMAIL_LEGGINGS = 15015;
        public const int ID_SCALEMAIL_LEGGINGS = 15016;
        public const int ID_IRON_GREAVES = 15020; // part of banded mail armor sets
        public const int ID_STEEL_GREAVES = 15021;

        // helms
        public const int ID_LEATHER_SKULLCAP = 810;
        public const int ID_CHAINMAIL_COIF = 815;
        public const int ID_STEEL_BASINET_WITH_VISOR = 821;

        // gauntlets
        public const int ID_LEATHER_GAUNTLETS = 26010;
        public const int ID_LEATHER_GAUNTLETS_PLUS_ONE = 26011;
        public const int ID_LEATHER_GAUNTLETS_PLUS_TWO = 26012;
        public const int ID_STUDDED_LEATHER_GAUNTLETS = 26013;
        public const int ID_CHAINMAIL_GAUNTLETS = 26015;
        public const int ID_BANDED_GAUNTLETS = 26020;
        public const int ID_STEEL_GAUNTLETS = 26022;

        // bracers (wrists)
        public const int ID_LEATHER_BRACER = 918;
        public const int ID_STUDDED_LEATHER_BRACER = 919;
        public const int ID_CHAINMAIL_BRACER = 920;
        public const int ID_SCALEMAIL_BRACER = 921;
        public const int ID_BANDED_MAIL_BRACER = 922;
        public const int ID_STEEL_BRACER = 923;

        // armbands (biceps)
        public const int ID_STUDDED_LEATHER_ARMBAND = 900;
        public const int ID_STEEL_ARMBAND_PLUS_ONE = 902;
        public const int ID_STEEL_ARMBAND_PLUS_THREE = 903;
        public const int ID_STEEL_ARMBAND_PLUS_SIX = 904;

        // common robes
        public const int ID_BLACK_ROBE_RED_RUNES = 7050;
        public const int ID_SILVER_ROBE_BLACK_LION = 7056;

        // basic weaponry
        public const int ID_STONE_DAGGER = 21500;
        public const int ID_STEEL_DAGGER = 21501;
        public const int ID_DAGGER_PLUS_TWO = 21502;
        public const int ID_WOODEN_SHORTBOW = 21000;
        public const int ID_YEW_LONGBOW = 21010;
        public const int ID_WOODEN_CROSSBOW = 21030;
        public const int ID_WOODEN_FLAIL = 22000;
        public const int ID_IRON_HALBERD = 22500;
        public const int ID_STEEL_WARHAMMER = 23000;
        public const int ID_LARGE_IRON_BATTLEAXE = 23010;
        public const int ID_MACE = 23020;
        public const int ID_STEEL_RAPIER = 23500;
        public const int ID_WOODEN_STAFF = 24500;
        public const int ID_WOODEN_SPEAR = 24550;
        public const int ID_WOODEN_THREESTAFF = 24600;
        public const int ID_SHARP_STEEL_KATANA = 25000;
        public const int ID_FINE_STEEL_LONGSWORD = 25010;
        public const int ID_IRON_SHORTSWORD = 25020;
        public const int ID_IRON_GREATSWORD = 25500;
        public const int ID_GRAY_BIRCH_BOOMERANG = 21520;
        #endregion

        #region Public Variables
        public int catalogID; // this is the control ID number from the database
        public string notes; // notes from the database
        public string visualKey;
        public int UniqueID;
        public int itemID; // this is the unique ID
        public Globals.eItemType itemType; // weapon, wearable, container, miscellaneous, edible, potable, corpse, coin
        public Globals.eItemBaseType baseType; //this is is the base of the item, ie.. axebattlelarge, sword, crossbow, etc..
        public Globals.eSkillType skillType; // this is the skill type being used
        public string name = "undefined"; // name of the item that will be used in the game
        public string unidentifiedName = ""; // unidentified name
        public string identifiedName = ""; // identified name
        public List<int> identifiedList = new List<int>(); // contains a list of playerIDs who have identified this item
        public string shortDesc; // short description of the item...this is the extended name
        public string longDesc; // long description of the item
        public double weight; // weight of this item
        public Globals.eItemSize size; // 0 = belt only, 1 = sack only, 2 = belt or sack, 3 = no container, 4 = belt only (large slot)
        public double coinValue; // coin value
        public string effectType = ""; // magical effect if worn or drink
        public string effectAmount = ""; // magical effect if worn or drink
        public string effectDuration = "";
        public int vRandLow = 0; // if vRandLow > 0, random cValue is between vRandLow and vRandHigh
        public int vRandHigh = 0;
        public long figExp = 0; // this is to hold the exp of figurines
        public bool isRecall = false;	//does the item recall
        public Globals.eAlignment alignment = Globals.eAlignment.None; // alignment of the item
        public string special = "";
        public Globals.eAttuneType attuneType = Globals.eAttuneType.None; // true if the item will attune to the slayer of the current item owner
        public Globals.eWearLocation wearLocation = Globals.eWearLocation.None;
        public string key = "none"; // control for keys/locks
        public int charges = -1; // charges remaining in an item; -1 = never has charges, 0 = had charges, > 100 = unlimited charges
        public int spell = -1;	// spell ID of the spell this item can cast
        public int spellPower = -1; // the spell power (casting level) of the spell in this item
        public double armorClass = 0;
        public int combatAdds = 0;
        public Globals.eAttackType attackType = Globals.eAttackType.None;
        public Globals.eArmorType armorType = Globals.eArmorType.None;
        public int minDamage = 1; //mindamage this weapon can inflict
        public int maxDamage = 3; //maxdamage this weapon can inflict

        public bool returning = false; // true if this item will stay in hand if thrown
        public bool flammable = false; // true if this item will be destroyed by fire
        public bool blueglow = false;
        public bool silver = false;
        public bool lightning = false;
        public bool fragile = false;

        public int facet = 0;
        public int land = 0; //When on the ground, this is the land the item is in
        public int map = 0; //When on the ground, this is the map the item is in
        public int xcord = 20; //When on the ground, this is the Xcord of the item
        public int ycord = 20; //When on the ground, this is the Ycord of the item
        public int zcord = 0;

        public bool IsNocked
        {
            get { return m_nocked; }
            set { m_nocked = value; if (value == false && this.baseType == Globals.eItemBaseType.Bow) venom = 0; }
        }

        private bool m_nocked = false;

        public int venom = 0; // if > 0, the amount of poison damage on a successful hit...then reset to 0

        public int dropRound = 0; // round item was dropped
        public int attunedID = 0; // if this is an attuned item, this will hold the attuned player's playerID
        public Globals.eWearOrientation wearOrientation = Globals.eWearOrientation.None; // for 2 slot wear locations, 0 = left and 1 = right. for fingers positions = 0 through 7
        public DateTime timeCreated = DateTime.Now;
        public string whoCreated = "SYSTEM";

        public string lootTable = "";
        #endregion

        #region Recall Variables
        public bool wasRecall = false; //used for recall reset
        public int recallX = 0;
        public int recallY = 0;
        public int recallZ = 0;
        public int recallMap = 0;
        public int recallLand = 0;
        #endregion

        //public int[] identified; // if the player has identified this item it contains their playerID TODO: add this to tables/sp's        

        #region Constructors (3)
        public Item()
        {
            itemID = -1;
            itemType = Globals.eItemType.Miscellaneous;
            baseType = Globals.eItemBaseType.Unknown;
            name = "undefined";
            shortDesc = null;
            longDesc = null;
            weight = -1;
            coinValue = -1;
            special = null;
            timeCreated = DateTime.Now;
            whoCreated = "SYSTEM";
        }

        public Item(Item item) : base()
        {
            //TODO iterate through variables in Reflection and set them that way
            this.catalogID = item.catalogID;
            this.UniqueID = World.GetNextWorldItemID();
            this.notes = item.notes;
            this.combatAdds = item.combatAdds;
            this.itemID = item.itemID;
            this.itemType = item.itemType;
            this.baseType = item.baseType;
            this.name = item.name;
            this.unidentifiedName = item.unidentifiedName;
            this.identifiedName = item.identifiedName;
            this.identifiedList = item.identifiedList;
            this.shortDesc = item.shortDesc;
            this.longDesc = item.longDesc;
            this.visualKey = item.visualKey;
            this.wearLocation = item.wearLocation;
            this.weight = item.weight;
            this.coinValue = item.coinValue;
            this.size = item.size;
            this.effectType = item.effectType;
            this.effectAmount = item.effectAmount;
            this.effectDuration = item.effectDuration;
            this.special = item.special;
            this.minDamage = item.minDamage;
            this.maxDamage = item.maxDamage;
            this.skillType = item.skillType;
            this.vRandLow = item.vRandLow;
            this.vRandHigh = item.vRandHigh;
            this.key = item.key;
            this.isRecall = item.isRecall;
            this.alignment = item.alignment;
            this.spell = item.spell;
            this.spellPower = item.spellPower;
            this.charges = item.charges;
            this.attackType = item.attackType;
            this.blueglow = item.blueglow;
            this.flammable = item.flammable;
            this.fragile = item.fragile;
            this.lightning = item.lightning;
            this.returning = item.returning;
            this.silver = item.silver;
            this.attuneType = item.attuneType;
            this.figExp = item.figExp;
            this.armorClass = item.armorClass;
            this.armorType = item.armorType;
            this.lootTable = item.lootTable;
        }

        public Item(System.Data.DataRow dr)
        {
            this.UniqueID = World.GetNextWorldItemID();
            this.catalogID = Convert.ToInt32(dr["catalogID"]);
            this.notes = dr["notes"].ToString();
            this.combatAdds = Convert.ToInt32(dr["combatAdds"]);
            this.itemID = Convert.ToInt32(dr["itemID"]);
            this.itemType = (Globals.eItemType)Enum.Parse(typeof(Globals.eItemType), dr["itemType"].ToString());
            this.baseType = (Globals.eItemBaseType)Enum.Parse(typeof(Globals.eItemBaseType), dr["baseType"].ToString());
            this.name = dr["name"].ToString();
            this.visualKey = dr["visualKey"].ToString();
            this.unidentifiedName = dr["unidentifiedName"].ToString();
            this.identifiedName = dr["identifiedName"].ToString();
            this.shortDesc = dr["shortDesc"].ToString();
            this.longDesc = dr["longDesc"].ToString();
            this.wearLocation = (Globals.eWearLocation)Enum.Parse(typeof(Globals.eWearLocation), dr["wearLocation"].ToString());
            this.weight = Convert.ToDouble(dr["weight"]);
            this.coinValue = Convert.ToInt32(dr["coinValue"]);
            this.size = (Globals.eItemSize)Enum.Parse(typeof(Globals.eItemSize), dr["size"].ToString());
            this.effectType = dr["effectType"].ToString();
            this.effectAmount = dr["effectAmount"].ToString();
            this.effectDuration = dr["effectDuration"].ToString();
            this.special = dr["special"].ToString();
            this.minDamage = Convert.ToInt32(dr["minDamage"]);
            this.maxDamage = Convert.ToInt32(dr["maxDamage"]);
            this.skillType = (Globals.eSkillType)Enum.Parse(typeof(Globals.eSkillType), dr["skillType"].ToString());
            this.vRandLow = Convert.ToInt32(dr["vRandLow"]);
            this.vRandHigh = Convert.ToInt32(dr["vRandHigh"]);
            this.key = dr["key"].ToString();
            this.isRecall = Convert.ToBoolean(dr["recall"]);
            this.alignment = (Globals.eAlignment)Enum.Parse(typeof(Globals.eAlignment), dr["alignment"].ToString());
            this.spell = Convert.ToInt16(dr["spell"]);
            this.spellPower = Convert.ToInt16(dr["spellPower"]);
            this.charges = Convert.ToInt16(dr["charges"]);
            try
            {
                this.attackType = (Globals.eAttackType)Enum.Parse(typeof(Globals.eAttackType), dr["attackType"].ToString());
            }
            catch { this.attackType = Globals.eAttackType.None; }
            this.blueglow = Convert.ToBoolean(dr["blueglow"]);
            this.flammable = Convert.ToBoolean(dr["flammable"]);
            this.fragile = Convert.ToBoolean(dr["fragile"]);
            this.lightning = Convert.ToBoolean(dr["lightning"]);
            this.returning = Convert.ToBoolean(dr["returning"]);
            this.silver = Convert.ToBoolean(dr["silver"]);
            this.attuneType = (Globals.eAttuneType)Enum.Parse(typeof(Globals.eAttuneType), dr["attuneType"].ToString());
            this.figExp = Convert.ToInt32(dr["figExp"]);
            this.armorClass = Convert.ToDouble(dr["armorClass"]);
            this.armorType = (Globals.eArmorType)Enum.Parse(typeof(Globals.eArmorType), dr["armorType"].ToString());
            this.lootTable = dr["lootTable"].ToString();
        }
        #endregion

        #region Static Methods (12)
        public static string GetLookShortDesc(Item item, double quantity)
        {
            if (item.itemType == Globals.eItemType.Coin)
            {
                if (item.coinValue == 1)
                    return "a coin";
                else return "coins";
            }

            string what = item.shortDesc;

            if (quantity > 1)
            {
                if (what.Contains("scales")) { return "vests made of scales"; }
                if (what.Contains("boots")) { return "pairs of boots"; }
                if (what.Contains("greaves")) { return "pairs of greaves"; }
                if (what.Contains("leggings")) { return "pairs of leggings"; }
                if (what.Contains("gauntlets")) { return "pairs of gauntlets"; }
                if (what.Contains("pantaloons")) { return "pairs of pantaloons"; }
                if (what.Contains("berries")) { return "bunches of berries"; }
                if (what.Contains("threestaff")) { return "threestaves"; }
                if (what.Contains("staff")) { return "staves"; }

                what = item.name + "s";
            }
            else
            {
                if (what.Contains("scales")) { return "a vest made of scales"; }
                if (what.Contains("boots")) { return "a pair of boots"; }
                if (what.Contains("greaves")) { return "a pair of greaves"; }
                if (what.Contains("leggings")) { return "a pair of leggings"; }
                if (what.Contains("gauntlets")) { return "a pair of gauntlets"; }
                if (what.Contains("pantaloons")) { return "a pair of pantaloons"; }
            }

            return what;
        }

        public static bool IsItemOnGround(string itemName, int facet, int land, int map, int xcord, int ycord, int zcord)
        {
            foreach (Item item in Cell.GetCell(facet, land, map, xcord, ycord, zcord).Items)
            {
                if (item.name.ToLower() == itemName.ToLower() || (int.TryParse(itemName, out int uniqueID) && uniqueID == item.UniqueID))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsItemOnGround(int itemID, Cell cell)
        {
            foreach (Item item in cell.Items)
            {
                if (item.itemID == itemID)
                {
                    return true;
                }
            }
            return false;
        }

        public static Item FindItemOnGround(string itemName, int facet, int land, int map, int xcord, int ycord, int zcord)
        {
            foreach (Item item in Cell.GetCell(facet, land, map, xcord, ycord, zcord).Items)
                if (item.name.ToLower() == itemName.ToLower() || (int.TryParse(itemName, out int uniqueID) && uniqueID == item.UniqueID))
                    return item;

            return null;
        }

        public static Item FindItemOnGround(int itemID, Cell groundCell)
        {
            foreach (Item item in groundCell.Items)
                if (item.itemID == itemID || item.UniqueID == itemID)
                    return item;

            return null;
        }

        public static Item RemoveItemFromGround(string itemName, Cell groundCell)
        {
            foreach (Item item in new List<Item>(groundCell.Items))
            {
                if (item.name.ToLower() == itemName.ToLower() || (int.TryParse(itemName, out int uniqueID) && uniqueID == item.UniqueID))
                {
                    groundCell.Remove(item);
                    return item;
                }
            }
            return null;
        }

        // Return an item on ground, remove it from cell
        public static Item RemoveItemFromGround(string itemname, int facet, int land, int map, int xcord, int ycord, int zcord)
        {
            foreach (Item item in Cell.GetCell(facet, land, map, xcord, ycord, zcord).Items)
            {
                if (item.name.ToLower() == itemname.ToLower() || (int.TryParse(itemname, out int uniqueID) && uniqueID == item.UniqueID))
                {
                    Cell.GetCell(facet, land, map, xcord, ycord, zcord).Remove(item);
                    return item;
                }
            }//end foreach
            return null;
        }

        public static Item[] GetAllItemsFromGround(Cell groundCell)
        {
            Item[] tempItemList = new Item[groundCell.Items.Count];
            groundCell.Items.CopyTo(tempItemList, 0);
            foreach (Item item in tempItemList)
            {
                groundCell.Remove(item);
            }
            return tempItemList;
        }

        public static string GetItemNameFromItemDictionary(int itemID)
        {
            if (Item.ItemDictionary.ContainsKey(itemID))
            {
                return Item.ItemDictionary[itemID]["name"].ToString();
            }
            return "";
        }

        public static string GetItemNotesFromItemDictionary(int itemID)
        {
            if (Item.ItemDictionary.ContainsKey(itemID))
            {
                return Item.ItemDictionary[itemID]["notes"].ToString();
            }
            return "";
        }

        public static Item CopyItemFromDictionary(int itemID)
        {
            try
            {
                if (Item.ItemDictionary.ContainsKey(itemID))
                {
                    System.Data.DataRow dr = Item.ItemDictionary[itemID];

                    if (dr["itemType"].ToString() == Globals.eItemType.Corpse.ToString())
                    {
                        return new Corpse(dr);
                    }

                    Item item = null;

                    switch ((Globals.eItemBaseType)Enum.Parse(typeof(Globals.eItemBaseType), dr["baseType"].ToString()))
                    {
                        case Globals.eItemBaseType.Book:
                            item = new Book(dr);
                            break;
                        case Globals.eItemBaseType.Bottle:
                            item = new Bottle(dr);
                            break;
                        default:
                            item = new Item(dr);
                            break;
                    }

                    Autonomy.ItemBuilding.LootManager.SetRandomCoinValue(item);

                    return item;

                }
                else
                {
                    Utils.Log("Item.CopyItemFromDictionary(" + itemID + ") ITEM ID does not exist.", Utils.LogType.SystemWarning);
                    return null;
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return null;
            }
        }

        public static bool SetRecallVariables(Item item, Character ch)
        {
            if (ch.CurrentCell == null) return false;

            item.recallLand = ch.LandID;
            item.recallMap = ch.MapID;
            item.recallX = ch.X;
            item.recallY = ch.Y;
            item.recallZ = ch.Z;

            if (DragonsSpineMain.ServerStatus == DragonsSpineMain.ServerState.Running)
                ch.WriteToDisplay("You feel a slight electric shock.");

            return true;
        }

        public static bool VerifyRecallMagic(Character ch, Item item)
        {
            if (ch.CurrentCell.IsNoRecall && !ch.IsImmortal)
                return false;

            if (item.recallLand != ch.LandID) // no more recalling between lands, sorry.
                return false;

            return true; // verified
        }

        public static Globals.eLightSource GetBlueglowStrength(Item item)
        {
            if (!item.blueglow) return Globals.eLightSource.None;
            else if (item.combatAdds >= 6) return Globals.eLightSource.StrongItemBlueglow;
            else if (item.size >= Globals.eItemSize.Belt_Large_Slot_Only) return Globals.eLightSource.StrongItemBlueglow;

            return Globals.eLightSource.WeakItemBlueglow;
        }
        #endregion

        public void AttuneItem(int playerID, string comment) // generally called from the code
        {
            this.attuneType = Globals.eAttuneType.None;
            this.attunedID = playerID;
            this.coinValue = 0;

            if (comment == null)
                comment = "None.";

            PC pc = PC.GetPC(playerID);

            if (pc != null)
                Utils.Log(PC.GetPC(playerID).GetLogString() + " attuned to " + this.GetLogString() + ". Comment: " + comment, Utils.LogType.ItemAttuned);
        }

        public void AttuneItem(Character ch) // called when an item meets its attuneType requirement
        {
            if (ch != null && (ch.IsPC || ch is Adventurer) && !ch.IsImmortal) // work around to take items that will attune and then attach them to an email, or give them to someone
            {
                this.attuneType = Globals.eAttuneType.None;
                this.attunedID = ch.UniqueID;
                this.coinValue = 0; // drop the coin value to 0

                if (ch.IsPC && ch.PCState == Globals.ePlayerState.PLAYING)
                {
                    Utils.Log(ch.GetLogString() + " attuned to " + this.GetLogString() + ".", Utils.LogType.ItemAttuned);
                    ch.WriteToDisplay("You feel a tingling sensation.");
                }
            }
            else if (ch != null && ch.IsImmortal)
            {
                ch.WriteToDisplay("The " + this.name + " did not attune to you because your immortal flag is set to true.");

                if (this.attuneType == Globals.eAttuneType.Slain)
                {
                    ch.WriteToDisplay("The " + this.notes + " attune type is being changed to allow a player to take it.");
                    this.attuneType = Globals.eAttuneType.Take;
                }
            }
        }

        public void AttuneItemSilently(Character ch)
        {
            if (ch != null && ch is PC && !ch.IsImmortal) // work around to take items that will attune and then attach them to an email, or give them to someone
            {
                this.attuneType = Globals.eAttuneType.None;
                this.attunedID = ch.UniqueID;
                this.coinValue = 0; // drop the coin value to 0
                Utils.Log(ch.GetLogString() + " silently attuned to " + this.GetLogString() + ".", Utils.LogType.ItemAttuned);
            }
            else if (ch != null && ch.IsImmortal)
            {
                ch.WriteToDisplay("The " + this.name + " did not attune to you because your immortal flag is set to true.");

                if (this.attuneType == Globals.eAttuneType.Slain)
                {
                    ch.WriteToDisplay("The " + this.notes + " attune type is being changed to allow a player to take it.");
                    this.attuneType = Globals.eAttuneType.Take;
                }
            }
        }

        public string GetLookDescription(Character looker)
        {
            if (itemType == Globals.eItemType.Coin)
                return "" + (int)coinValue + " coins.";

            string description = longDesc + ".";

            if (baseType == Globals.eItemBaseType.Bottle)
                description += Bottle.GetFluidDesc((Bottle)this);

            if (blueglow)
                description += " " + (wearLocation == Globals.eWearLocation.Feet || wearLocation == Globals.eWearLocation.Hands ? "They are" : "It is") + " emitting a faint blue glow.";

            if (looker.BaseProfession == Character.ClassType.Thief) // add thief appraisal to description
            {
                #region thief gem and jewelry appraising
                switch (baseType)
                {
                    case Globals.eItemBaseType.Amulet:
                    case Globals.eItemBaseType.Ring:
                    case Globals.eItemBaseType.Gem:
                    case Globals.eItemBaseType.Bracelet:
                        //case BaseType.Figurine:
                        if (coinValue == 0)
                            description += " The " + name + " has no monetary value.";
                        else if (coinValue == 1)
                            description += " The " + name + " is worth " + coinValue + " coin.";
                        else
                            description += " The " + name + " is worth about " + Math.Round(coinValue, coinValue.ToString().Length - 1, MidpointRounding.AwayFromZero) + " coins.";
                        break;
                    default:
                        break;
                }
                #endregion
            }
            else if (looker.BaseProfession == Character.ClassType.Fighter) // add fighter appraisal to description
            {
                #region fighter armor and weapon appraising
                int actualLevel = Rules.GetExpLevel(looker.Experience);

                switch (baseType)
                {
                    case Globals.eItemBaseType.Armor:
                    case Globals.eItemBaseType.Helm:
                    case Globals.eItemBaseType.Boots:
                        if (actualLevel >= 10)
                        {
                            if (this.coinValue == 0)
                                description += " The " + name + " has no monetary value.";
                            else if (this.coinValue == 1)
                                description += " The " + name + " is worth " + coinValue + " coin.";
                            else
                                description += " The " + name + " is worth about " + Math.Round(coinValue, coinValue.ToString().Length - 1, MidpointRounding.AwayFromZero) + " coins.";
                        }
                        break;
                    case Globals.eItemBaseType.Bow:
                    case Globals.eItemBaseType.Shield:
                    case Globals.eItemBaseType.Mace:
                        if (actualLevel >= 11)
                        {
                            if (coinValue == 0)
                                description += " The " + name + " has no monetary value.";
                            else if (coinValue == 1)
                                description += " The " + name + " is worth " + coinValue + " coin.";
                            else
                                description += " The " + name + " is worth about " + Math.Round(coinValue, coinValue.ToString().Length - 1, MidpointRounding.AwayFromZero) + " coins.";
                        }
                        break;
                    case Globals.eItemBaseType.Dagger:
                    case Globals.eItemBaseType.Flail:
                    case Globals.eItemBaseType.Sword:
                        if (actualLevel >= 12)
                        {
                            if (coinValue == 0)
                                description += " The " + name + " has no monetary value.";
                            else if (this.coinValue == 1)
                                description += " The " + name + " is worth " + coinValue + " coin.";
                            else
                                description += " The " + name + " is worth about " + Math.Round(coinValue, coinValue.ToString().Length - 1, MidpointRounding.AwayFromZero) + " coins.";
                        }
                        break;
                    case Globals.eItemBaseType.Halberd:
                    case Globals.eItemBaseType.Rapier:
                        if (actualLevel >= 13)
                        {
                            if (coinValue == 0)
                                description += " The " + name + " has no monetary value.";
                            else if (this.coinValue == 1)
                                description += " The " + name + " is worth " + coinValue + " coin.";
                            else
                                description += " The " + name + " is worth about " + Math.Round(coinValue, coinValue.ToString().Length - 1, MidpointRounding.AwayFromZero) + " coins.";
                        }
                        break;
                    default:
                        break;
                }
                #endregion
            }
            else if (looker.BaseProfession == Character.ClassType.Sorcerer)
            {
                // sorcerers can tell if a corpse looks fresh (will be a zombie vice skeleton if Animate Dead is cast)
                if (itemType == Globals.eItemType.Corpse && World.NPCCorpseDecayTimer - (DragonsSpineMain.GameRound - dropRound) >= (World.NPCCorpseDecayTimer / 2))
                    description += " The " + name + " looks fresh.";
            }
            // add skill level if held weapon
            if ((this == looker.RightHand || this == looker.LeftHand) && itemType == Globals.eItemType.Weapon)
                description += " You are " + Skills.GetSkillTitle(skillType, looker.BaseProfession, looker.GetSkillExperience(skillType), looker.gender) + " with this weapon.";

            if (attunedID == looker.UniqueID)
                description += " You are soulbound to this item.";

            if (this is SoulGem && (looker.BaseProfession == Character.ClassType.Sorcerer || looker.IsImmortal))
                description += " The " + name + " contains the soul of " + (this as SoulGem).Soul.Name + ".";

            if (looker.fighterSpecialization == skillType && skillType != Globals.eSkillType.None)
                description += " You are specialized in the use of this weapon.";

            //if (looker.RightHand == this || looker.LeftHand == this)
            //{
            //    double perceivedWeight = this.weight;

            //    if (!Rules.CheckPerception(looker))
            //        perceivedWeight = Math.Round(perceivedWeight + Rules.Dice.NextDouble());

            //    if (looker.BaseProfession == Character.ClassType.Thief)
            //    {
            //        description += " The " + this.name + " feels like it weighs " + perceivedWeight + " m'na.";
            //    }
            //    else if(perceivedWeight <= 0)
            //    {
            //        description += " The " + this.name + " feels like it weighs less than 1 m'na.";
            //    }
            //    else  description += " The " + this.name + " feels like it weighs about " + perceivedWeight + " m'na.";
            //}

            if (IsNocked)
                description += " The " + name + " is nocked.";

            if (venom > 0)
                description += " The " + name + " drips with a caustic venom.";

            return description;
        }

        public string GetLogString()
        {
            return "[ItemID: " + this.itemID + " | WorldItemID: " + this.UniqueID + "] (" + this.notes + ")";
        }

        public bool IsAttunedToOther(Character ch)
        {
            if (attunedID != 0 && attunedID != ch.UniqueID)
                return true;

            return false;
        }

        public bool AlignmentCheck(Character ch)
        {
            if (!ch.IsPC) return true;

            if (alignment == Globals.eAlignment.None || (alignment == Globals.eAlignment.ChaoticEvil && ch.Alignment == Globals.eAlignment.Evil) ||
                alignment == ch.Alignment)
                return true;

            return false;
        }

        public static void Recall(Character chr, Item item, int hand)
        {
            chr.SendShout("a thunderclap!");
            chr.WriteToDisplay("You hear a thunderclap!");
            chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.ThunderClap));
            chr.CurrentCell = Cell.GetCell(chr.FacetID, item.recallLand, item.recallMap, item.recallX, item.recallY, item.recallZ);
            chr.SendShout("a thunderclap!");
            chr.SendSoundToAllInRange(Sound.GetCommonSound(Sound.CommonSound.ThunderClap));
            item = Item.CopyItemFromDictionary(Item.ID_GOLDRING);
            if (hand == (int)Globals.eWearOrientation.Left)
                chr.EquipLeftHand(item);
            else if (hand == (int)Globals.eWearOrientation.Right)
                chr.EquipRightHand(item);
            // if hand == 0 or Globals.eWearOrientation.None, or any other integer, the item disappears
        }

        /// <summary>
        /// In Dragon's Spine this means a player should not be able to take, receive, steal
        /// or in any means acquire the item if they already possess it.
        /// </summary>
        /// <returns></returns>
        public bool IsArtifact()
        {
                return this.lootTable.ToLower().Contains("artifact") || this.lootTable.ToLower().Contains("lore");
        }

        public bool TwoHandedPreferred()
        {
            switch (this.skillType)
            {
                case Globals.eSkillType.Bow:
                case Globals.eSkillType.Polearm:
                case Globals.eSkillType.Threestaff:
                case Globals.eSkillType.Two_Handed:
                    return true;
                default:
                    break;
            }

            return false;
        }

        public bool RangePreferred()
        {
            // Careful here...
            if (this.returning) return true;

            switch (this.skillType)
            {
                case Globals.eSkillType.Bow:
                case Globals.eSkillType.Shuriken:
                    return true;
                default:
                    break;
            }

            return false;
        }

        public bool IsPiercingWeapon()
        {

            if (baseType == Globals.eItemBaseType.Dagger || baseType == Globals.eItemBaseType.Rapier ||
                baseType == Globals.eItemBaseType.Shuriken || baseType == Globals.eItemBaseType.Bow)
                return true;

            if (special.ToLower().Contains("pierce") || special.ToLower().Contains("piercing") || longDesc.ToLower().Contains("spikes") ||
                longDesc.ToLower().Contains("barbs"))
                return true;

            if (attackType.ToString().ToLower().Contains("pierce")) return true;

            return false;
        }

        public bool HasProcEffects(out Dictionary<Spells.GameSpell, int> procs)
        {
            Dictionary<Spells.GameSpell, int> procEffects = new Dictionary<Spells.GameSpell, int>();

            if (this.effectType == null || this.effectType == "" || this.effectType == "0")
            {
                procs = procEffects;
                return false;
            }

            string[] effectTypes = this.effectType.Split(" ".ToCharArray());
            string[] effectAmounts = this.effectAmount.Split(" ".ToCharArray()); // GameSpell IDs
            string[] effectDurations = this.effectDuration.Split(" ".ToCharArray()); // Spell Levels

            if(effectTypes.Length != effectAmounts.Length || effectTypes.Length != effectDurations.Length)
            {
                procs = null;
                Utils.Log("Unable to determine proc effects for " + this.GetLogString(), Utils.LogType.SystemWarning);

                return false;
            }
            
            int count = 0;

            foreach(string effectNum in effectTypes)
            {
                if(Convert.ToInt32(effectNum) == (int)Effect.EffectTypes.Weapon_Proc)
                {
                    Spells.GameSpell spell = Spells.GameSpell.GetSpell(Convert.ToInt32(effectAmounts[count]));

                    // No double procs. Using a dictionary with keys.
                    if(spell != null && !procEffects.ContainsKey(spell))
                    {
                        procEffects.Add(spell, Convert.ToInt32(effectDurations[count]));
                    }
                    else // Log warning.
                    {
                        Utils.Log("Unable to decipher GameSpell (ID = " + effectNum + "?) proc effects for " + this.GetLogString(), Utils.LogType.SystemWarning);
                    }
                }
            }

            procs = procEffects;

            return procEffects.Count > 0;
        }

        public bool IsCursed()
        {
            return special.ToLower().Contains("cursed");
        }

        public bool RequiresOneFreeHandToShoot()
        {
            if (skillType == Globals.eSkillType.Bow)
                return true;

            if (baseType == Globals.eItemBaseType.Sling)
                return true;

            return false;
        }
    }
}
