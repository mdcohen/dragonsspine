using System;
using System.Collections;
using System.Collections.Generic;
using DragonsSpine.GameWorld;
using GameSpell = DragonsSpine.Spells.GameSpell;

namespace DragonsSpine
{
    public class Merchant : NPC
    {
        #region Constants
        public const int MAX_STORE_INVENTORY = 12; // maximum store inventory amount
        public const double AG_BALM_PRICE = 160;
        public const double BG_BALM_EFFECT_AMOUNT = 260;
        public const int AG_BALM_EFFECT_AMOUNT = 780;
        public const int DEFAULT_ARMOR_MERCHANT_NPCID = 21001; // Leng armorer in database
        public const int DEFAULT_WEAPON_MERCHANT_NPCID = 21002;
        public const int DEFAULT_GENERAL_MERCHANT_NPCID = 21003;
        private const int MAX_ITEMS_EXCHANGE = 9;
        public const double MAX_BANK_GOLD = 100000000; // 100 million coins

        public const int GENERIC_ARMORER_NPCID = 21001;
        public const int GENERIC_WEAPON_NPCID = 21002;
        public const int GENERIC_PAWN_NPCID = 22011;
        public const int GENERIC_BARKEEP_NPCID = 21008;
        public const int GENERIC_APOTHECARY_NPCID = 25002;
        public const int GENERIC_JEWELER_NPCID = 3102;
        public const int GENERIC_HIGH_SORC_TRAINER_NPCID = 22014;
        public const int GENERIC_EVIL_WEAPONS_TRAINER_NPCID = 22013;
        public const int GENERIC_LAWFUL_HIGH_WEAPONS_TRAINER_NPCID = 25050;
        public const int GENERIC_LAWFUL_HIGH_THAUM_TRAINER = 25052;
        public const int GENERIC_LAWFUL_HIGH_WIZ_TRAINER = 25051;
        public const int GENERIC_LAWFUL_HIGH_MA_TRAINER = 22025;
        public const int GENERIC_NEUTRAL_HIGH_DRUID_TRAINER = 5;
        public const int GENERIC_LAWFUL_HIGH_RANGER_TRAINER = 6;
        public const int GENERIC_NEUTRAL_HIGH_THIEF_TRAINER = 25053;
        public const int GENERIC_TANNER_NPCID = 20025;

        public const int DEFAULT_MERCHANT_SPAWNTIMER = 180;
        #endregion

        #region Public Enumerations
        public enum MerchantType
        {
            None, Pawn, Barkeep, Weapon, Armor, Apothecary, Book, Jewellery, General, Magic
        }

        public enum TrainerType
        {
            None, Spell, Weapon, Martial_Arts, Knight, Sage, HP_Doctor, Animal
        }

        public enum InteractiveType
        {
            None, Banker, Tanner, Balm, Recall_Ring, Mugwort, Confessor, Blacksmith, Mentor, Mender
        }
        #endregion

        #region Public Data
        // TODO: in the future make these collections of values
        public TrainerType trainerType = TrainerType.None;
        public MerchantType merchantType = MerchantType.None; //what itemType or baseType items this merchant will add to its inventory
        public InteractiveType interactiveType = InteractiveType.None;
        public double merchantMarkup = 1.5;
        #endregion

        #region Constructor
        public Merchant(System.Data.DataRow dr)
            : base(dr)
        {
            merchantType = (Merchant.MerchantType)Enum.Parse(typeof(Merchant.MerchantType), dr["merchantType"].ToString(), true);
            merchantMarkup = Convert.ToDouble(dr["merchantMarkup"]);
            trainerType = (Merchant.TrainerType)Enum.Parse(typeof(Merchant.TrainerType), dr["trainerType"].ToString(), true);
            interactiveType = (Merchant.InteractiveType)Enum.Parse(typeof(Merchant.InteractiveType), dr["interactiveType"].ToString(), true);
        }

        public Merchant()
        {
            merchantType = MerchantType.None;
            merchantMarkup = 2.5;
            trainerType = TrainerType.None;
            interactiveType = InteractiveType.None;
        }
        #endregion

        #region Static Methods (3)
        private static string GetOrdinalIndicator(int number)
        {
            switch (number)
            {
                case 1:
                    return "st";
                case 2:
                    return "nd";
                case 3:
                    return "rd";
                default:
                    return "th";
            }
        }

        private static string GetSageAdvice()
        {
            string advice = "";
            #region Proverbs
            string[] proverbs = {"43% of all statistics are worthless.","A bird does not sing because it has an answer -- it sings because it has a song.",
            "A budget is just a method of worrying before you spend money, as well as afterward.",
            "A bus is a vehicle that runs twice as fast when you are after it as when you are in it.",
            "A camel is a horse designed by a committee.",
            "A celebrity is someone who works hard all their life to become known and then wears dark glasses to avoid being recognized.",
            "A classic is something that everybody wants to have read and Nobody has.",
            "A closed mind is a good thing to lose.",
            "A closed mind is like a closed book; just a block of wood.",
            "A crisis is when you can't say: 'let's forget the whole thing'.",
            "A crumb from a winner's table is better than a feast from a loser's table!",
            "Action may not always be happiness, but there is no happiness without action.",
            "A cynic is someone who knows the price of everything and the value of nothing.",
            "A dancer goes quick on her beautiful legs; a duck goes quack on her beautiful eggs.",
            "A diet is a selection of food that makes other people lose weight.",
            "A diplomat is a man who always remembers a woman's birthday but never remembers her age.",
            "A dog inside a kennel barks at his fleas. A dog hunting does not notice them.",
            "A dog who attends a flea circus most likely will steal the whole show.",
            "A dream is just a dream. A goal is a dream with a plan and a deadline.",
            "A drop of ink may make a million think.",
            "A drunk mans' words are a sober mans' thoughts.",
            "Adult: A person who has stopped growing at both ends and is now growing in the middle.",
            "Adversity doesn't build character, it reveals it.",
            "Advice is what we ask for when we already know the answer but wish we didn't.",
            "A fall will always make a wise man wiser.",
            "Sun Tzu said:  The art of war is of vital importance to the State.",
            "If anything just cannot go wrong, it will anyway.",
            "If you perceive that there are four possible ways in which something can go wrong, and circumvent these, then a fifth way, unprepared for, will promptly develop.",
            "Left to themselves, things tend to go from bad to worse.",
            "If everything seems to be going well, you have obviously overlooked something.",
            "Nature always sides with the hidden flaw.",
            "Mother nature is a bitch.",
            "Things get worse under pressure.",
            "Smile . . . tomorrow will be worse.",
            "Everything goes wrong all at once.",
            "Matter will be damaged in direct proportion to its value.",
            "Enough research will tend to support whatever theory.",
            "Research supports a specific theory depending on the amount of funds dedicated to it.",
            "In nature, nothing is ever right. Therefore, if everything is going right ... something is wrong.",
            "It is impossible to make anything foolproof because fools are so ingenious.",
            "Rule of Accuracy: When working toward the solution of a problem, it always helps if you know the answer.",
            "Nothing is as easy as it looks.",
            "Whenever you set out to do something, something else must be done first.",
            "Every solution breeds new problems.",
            "No matter how perfect things are made to appear, Murphy's law will take effect and screw it up.",
            "The chance of the bread falling with the buttered side down is directly proportional to the cost of the carpet.",
            "A falling object will always land where it can do the most damage.",
            "A shatterproof object will always fall on the only surface hard enough to crack or break it.",
            "You will always find something in the last place you look.",
            "If you're looking for more than one thing, you'll find the most important one last.",
            "After you bought a replacement for something you've lost and searched for everywhere, you'll find the original.",
            "The other line always moves faster.",
            "In order to get a loan, you must first prove you don't need it.",
            "If it jams - force it. If it breaks, it needed replacing anyway.",
            "When a broken appliance is demonstrated for the repairman, it will work perfectly.",
            "Build a system that even a fool can use, and only a fool will use it.",
            "Everyone has a scheme for getting rich that will not work.",
            "In any hierarchy, each individual rises to his own level of incompetence, and then remains there.",
            "There's never time to do it right, but there's always time to do it over.",
            "When in doubt, mumble. When in trouble, delegate.",
            "Anything good in life is either illegal, immoral or fattening.",
            "Murphy's golden rule: whoever has the gold makes the rules.",
            "A Smith & Wesson beats four aces.",
            "In case of doubt, make it sound convincing.",
            "Never argue with a fool, people might not know the difference.",
            "Whatever hits the fan will not be evenly distributed.",
            "No good deed goes unpunished.",
            "Where patience fails, force prevails.",
            "If you want something bad enough, chances are you won't get it.",
            "If you think you are doing the right thing, chances are it will back-fire in your face.",
            "The fish are always biting....yesterday!",
            "The cost of the hair do is directly related to the strength of the wind.",
            "Great ideas are never remembered and dumb statements are never forgotten.",
            "When you see light at the end of the tunnel, the tunnel will cave in.",
            "Being dead right, won't make you any less dead.",
            "Whatever you want, you can't have, what you can have, you don't want.",
            "Whatever you want to do, is Not possible, what ever is possible for you to do, you don't want to do it.",
            "A knowledge of Murphy's Law is no help in any situation.",
            "If you apply Murphy's Law, it will no longer be applicable.",
            "If you say something, and stake your reputation on it, you will lose your reputation.",
            "no matter where I go, there I am.",
            "If authority was mass, stupidity would be gravity.",
            "Ants will always infest the nearest food cupboard.",
            "Those who know the least will always know it the loudest.",
            "You will find an easy way to do it, after you've finished doing it.",
            "It always takes longer than you think, even when you take into account Hofstadter's Law.",
            "Laundry Math: 1 Washer + 1 Dryer + 2 Socks = 1 Sock",
            "Anyone who isn't paranoid simply isn't paying attention.",
            "A valuable falling in a hard to reach place will be exactly at the distance of the tip of your fingers.",
            "The probability of rain is inversely proportional to the size of the umbrella you carry around with you all day.",
            "Whenever you cut your finger nails, you find a need for them an hour later.",
            "In order for something to get clean, something else must get dirty.",
            "Nothing is impossible for the man who doesn't have to do it himself.",
            "The likelihood of something happening is in inverse proportion to the desirability of it happening.",
            "Common sense is not so common.",
            "Two wrongs don't make a right. It usually takes three or four.",
            "If the truth is in your favor no one will believe you.",
            "Laws are like a spider web, in that they ensnare the poor and weak while the rich and powerful break them.",
            "The key to happiness is to be O.K. with not being O.K.",
            "Every rule has an exception except the Rule of Exceptions.",
            "If your action has a 50% possibility of being correct, you will be wrong 75% of the time.",
            "The difference between Stupidity and Genius is that Genius has its limits.",
            "The universe is great enough for all possibilities to exist.",
            "Those who don't take decisions never make mistakes.",
            "Anything that seems right, is putting you into a false sense of security.",
            "The only time you're right, is when its about being wrong.",
            "The road to success is always under construction.",
            "Any given program, when running, is obsolete.",
            "Any given program costs more and takes longer each time it is run.",
            "If a program is useful, it will have to be changed.",
            "If a program is useless, it will have to be documented.",
            "Any given program will expand to fill all the available memory.",
            "Program complexity grows until it exceeds the capability of the programmer who must maintain it.",
            "Software bugs are impossible to detect by anybody except the end user.",
            "A sucking chest wound is Nature's way of telling you to slow down.",
            "If it's stupid but it works, it isn't stupid.",
            "Try to look unimportant; the enemy may be low on ammo and not want to waste a bullet on you.",
            "If at first you don't succeed, call in an air strike.",
            "If you are forward of your position, your artillery will fall short.",
            "Never share a foxhole with anyone braver than yourself.",
            "Never go to bed with anyone crazier than yourself.",
            "Never forget that your weapon was made by the lowest bidder.",
            "If your attack is going really well, it's an ambush.",
            "The enemy diversion you're ignoring is their main attack.",
            "There is no such thing as a perfect plan.",
            "Five second fuses always burn three seconds.",
            "There is no such thing as an atheist in a foxhole.",
            "The easy way is always mined.",
            "Teamwork is essential, it gives the enemy other people to shoot at.",
            "Never draw fire, it irritates everyone around you.",
            "If you are short of everything but the enemy, you are in the combat zone.",
            "When you have secured the area, make sure the enemy knows it too.",
            "Incoming fire has the right of way.",
            "If you can't remember, the Claymore is pointed toward you.",
            "The bigger they are, the harder they fall. They punch, kick and choke harder too.",
            "Tear gas works on cops too, and regardless of wind direction, will always blow back in your face.",
            "Any suspect with a rifle is a better shot than any cop with a pistol.",
            "When in doubt, empty your shotgun.",
            "Success occurs when no one is looking, failure occurs when the Client is watching.",
            "We are all born ignorant, but one must work hard to remain stupid.",
            "It isn't what happens that matters, but how you respond to it."};
            #endregion
            advice = proverbs[Rules.Dice.Next(proverbs.Length)];
            return advice;
        }

        private static string ConvertSkillRankToString(int rank)
        {
            string skillrank = "rank";
            switch (rank)
            {
                case 1:
                    skillrank = "first rank";
                    break;
                case 2:
                    skillrank = "second rank";
                    break;
                case 3:
                    skillrank = "third rank";
                    break;
                case 4:
                    skillrank = "fourth rank";
                    break;
                case 5:
                    skillrank = "fifth rank";
                    break;
                case 6:
                    skillrank = "sixth rank";
                    break;
                case 7:
                    skillrank = "seventh rank";
                    break;
                case 8:
                    skillrank = "eighth rank";
                    break;
                case 9:
                    skillrank = "ninth rank";
                    break;
                case 10:
                    skillrank = "ninth rank";
                    break;
                default:
                    skillrank = "rank";
                    break;
            }
            return skillrank;
        }

        /// <summary>
        /// Check a cell for coins and exchange them for balm. This method checks if the cell is Advanced Game or Beginner's Game.
        /// </summary>
        /// <param name="cell">The cell to check for coins.</param>
        public static void ExchangeCoinsForBalm(Cell cell)
        {
            int balms = 0;

            Item coin = null;

            foreach (Item item in cell.Items)
            {
                if (item.itemID == Item.ID_BALM) { balms++; }
                else if (item.itemType == Globals.eItemType.Coin) coin = item;
            }

            if (coin != null && balms <= MAX_ITEMS_EXCHANGE)
            {
                Item balm = Item.CopyItemFromDictionary(Item.ID_BALM);

                if (cell.LandID == Land.ID_ADVANCEDGAME) balm.coinValue = AG_BALM_PRICE;

                if (coin.coinValue >= balm.coinValue)
                {
                    int available = (int)Math.Truncate(coin.coinValue / balm.coinValue);

                    bool sentBalmFountainMessage = false;

                    do
                    {
                        balm = Item.CopyItemFromDictionary(Item.ID_BALM); // copy balm from dictionary

                        coin.coinValue -= balm.coinValue; // subtract balm cost from coins
                        cell.Add(balm); // add balm to cell

                        if (cell.IsBalmFountain && !sentBalmFountainMessage)
                        {
                            cell.SendToAllInSight("The balm fountain water shimmers briefly.");
                            sentBalmFountainMessage = true;
                        }

                        available--; // subtract from available
                        balms++; // add to balms amount on ground
                        if (coin.coinValue <= 0)
                        {
                            cell.Remove(coin); // if all the coins are gone, remove the coins from the cell
                            break; // break the do loop
                        }
                    }
                    while (available > 0 && balms <= MAX_ITEMS_EXCHANGE);
                }
            }
        }
        #endregion

        public override void DoAI()
        {
            base.DoAI();

            switch (this.interactiveType)
            {
                case InteractiveType.Balm:
                    this.MerchantBalmSeller();
                    break;
                case InteractiveType.Mugwort:
                    this.MerchantMugwortSeller();
                    break;
                case InteractiveType.Recall_Ring:
                    this.MerchantRecallSeller();
                    break;
                case InteractiveType.Tanner:
                    this.MerchantTanner();
                    break;
                default:
                    break;
            }

            if (this.MostHated == null && this.idleSound != "" && Rules.RollD(1, 100) < 8)
                this.EmitSound(this.idleSound);
        }

        public void SetTrainerSkillLevels()
        {
            if (this.Map == null) return;
            if (!this.Map.ZPlanes.ContainsKey(this.Z)) return;

            ZAutonomy zAuto = this.Map.ZPlanes[this.Z].zAutonomy;

            if (zAuto == null) return;

            int maxSkill = this.Map.ZPlanes[this.Z].zAutonomy.maximumSuggestedLevel;

            switch (trainerType)
            {
                case TrainerType.Weapon:
                    // all weapons except threestaff and martial arts
                    foreach (Globals.eSkillType skillType in Enum.GetValues(typeof(Globals.eSkillType)))
                    {
                        if (skillType != Globals.eSkillType.Thievery && skillType != Globals.eSkillType.Magic &&
                            skillType != Globals.eSkillType.Unarmed)
                        {
                            SetSkillExperience(skillType, Skills.GetSkillForLevel(maxSkill + 5));
                        }
                    }
                    break;
                case TrainerType.Spell:
                    this.SetSkillExperience(Globals.eSkillType.Magic, Skills.GetSkillForLevel(maxSkill + 5));
                    if (BaseProfession == ClassType.Thief)
                        SetSkillExperience(Globals.eSkillType.Thievery, Skills.GetSkillForLevel(maxSkill + 5));
                    break;
                case TrainerType.Martial_Arts:
                    // threestaff, shuriken, martial arts
                    SetSkillExperience(Globals.eSkillType.Threestaff, Skills.GetSkillForLevel(maxSkill + 5));
                    SetSkillExperience(Globals.eSkillType.Shuriken, Skills.GetSkillForLevel(maxSkill + 5));
                    SetSkillExperience(Globals.eSkillType.Unarmed, Skills.GetSkillForLevel(maxSkill + 5));
                    SetSkillExperience(Globals.eSkillType.Thievery, Skills.GetSkillForLevel(maxSkill));
                    break;
                default:
                    break;
            }
        }
        protected void MerchantTanner()
        {
            if (CurrentCell == null) return;

            if (Rules.RollD(1, 100) < 10)
            {
                string message = "";

                if (Rules.RollD(1, 2) == 2)
                {
                    if (Alignment == Globals.eAlignment.Evil)
                        message = "Bring your fresh kills to me and I'll make you a real nice flesh mask. Still warm and everything.";
                    message = "Bring your kills to me for the finest leatherwork this side of Mu.";
                }
                else
                {
                    if (Alignment == Globals.eAlignment.Evil) message = "Kill you some goody goody lawfuls and we'll see just how tough their skin really is.";
                    else message = "You kill 'em and I make 'em pretty. Impress your significant others with a new elk-hide vest!";
                }

                CommandTasker.ParseCommand(this, "say", message);
            }

            try
            {
                if (CurrentCell.Items.Count > 0)
                {
                    List<Item> corpses = new List<Item>();

                    for (int a = 0; a < CurrentCell.Items.Count; a++)
                    {
                        Item cellItem = CurrentCell.Items[a];

                        if (cellItem is Corpse && !(cellItem as Corpse).IsPlayerCorpse) // type corpse and itemID less than player's corpse itemID
                        {
                            if ((cellItem as Corpse).Contents.Count > 0)
                            {
                                for (int b = 0; b < (cellItem as Corpse).Contents.Count; b++) // tanner will rummage through corpses and drop items in the corpse contentList
                                {
                                    CurrentCell.Add((cellItem as Corpse).Contents[b]);
                                }
                            }

                            if (((cellItem as Corpse).Ghost as NPC).tanningResult != null && ((cellItem as Corpse).Ghost as NPC).tanningResult.Count > 0)
                            {
                                Item tannedItem = null;

                                foreach (int id in ((cellItem as Corpse).Ghost as NPC).tanningResult.Keys)
                                {
                                    Autonomy.ItemBuilding.LootManager.LootRarityLevel rarity = ((cellItem as Corpse).Ghost as NPC).tanningResult[id];

                                    if (Autonomy.ItemBuilding.LootManager.GetLootSuccess(rarity))
                                    {
                                        tannedItem = Item.CopyItemFromDictionary(id);
                                        Utils.Log(GetLogString() + " tanned " + tannedItem.notes + " from " + cellItem.longDesc + ". Rarity: " + rarity.ToString(), Utils.LogType.MerchantTanning);
                                        CurrentCell.Add(tannedItem);
                                    }
                                    else
                                        Utils.Log(GetLogString() + " did NOT tan " + tannedItem.notes + " from " + cellItem.longDesc + ". Rarity: " + rarity.ToString(), Utils.LogType.MerchantTanning);
                                }

                                if (tannedItem != null)
                                    this.SendToAllInSight(Name + ": There you go!");
                                else this.SendToAllInSight(Name + ": There's nothing left on this corpse to be tanned.");
                            }

                            corpses.Add(cellItem);
                        }
                    }

                    if (corpses.Count > 0) // remove the corpses from this merchant's cellItemList
                    {
                        for (int a = 0; a < corpses.Count; a++)
                            CurrentCell.Remove(corpses[a]);
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        protected void MerchantRecallSeller()
        {
            if (CurrentCell == null) return;

            Item coin = null; // coin found on the ground

            int rings = 0; // # of recall rings on the ground

            if (Rules.RollD(1, 100) < 10)
            {
                if (Rules.RollD(1, 100) > 50)
                {
                    CommandTasker.ParseCommand(this, "say", "Buy a recall ring, gets you out of trouble every time.");
                }
                else
                {
                    CommandTasker.ParseCommand(this, "say", "All I ask is 150 coins to defray my modest expenses.");
                }
            }

            foreach (Item item in CurrentCell.Items)
            {
                if (item.itemID == Item.ID_RECALLRING) { rings++; }

                if (item.itemType == Globals.eItemType.Coin)
                {
                    coin = item;
                }
            }

            if (coin != null && rings <= 9)
            {
                Item recallRing = Item.CopyItemFromDictionary(Item.ID_RECALLRING);

                if (coin.coinValue >= recallRing.coinValue)
                {
                    int available = (int)Math.Truncate(coin.coinValue / recallRing.coinValue);
                    do
                    {
                        recallRing = Item.CopyItemFromDictionary(Item.ID_RECALLRING); // copy recall ring from dictionary
                        coin.coinValue -= recallRing.coinValue; // subtract ring cost from coins
                        this.CurrentCell.Add(recallRing); // add balm to cell
                        available--; // subtract from available
                        rings++; // add to rings amount on ground
                        if (coin.coinValue <= 0)
                        {
                            this.CurrentCell.Remove(coin); // if all the coins are gone, remove the coins from the cell
                            break; // break the do loop
                        }
                    }
                    while (available > 0 && rings <= 9);
                }
            }
        }

        protected void MerchantMugwortSeller()
        {
            if (this.CurrentCell == null) return;

            if (Rules.RollD(1, 100) < 10)
            {
                if (Rules.RollD(1, 10) > 5)
                {
                    CommandTasker.ParseCommand(this, "say", "Get your Snake Oil and Hair Tonic here! Just 25 coins and you will see the light.");
                }
                else
                {
                    CommandTasker.ParseCommand(this, "say", "Only 25 coins and you too can have a head of hair like this!");
                }
            }

            int mugworts = 0;

            Item coin = null;

            foreach (Item itm in this.CurrentCell.Items)
            {
                if (itm.itemID == Item.ID_MUGWORT) { mugworts++; }
                else if (itm.itemType == Globals.eItemType.Coin) coin = itm;
            }

            if (coin != null && coin.coinValue >= 25 && mugworts <= MAX_ITEMS_EXCHANGE)
            {
                CommandTasker.ParseCommand(this, "take", "25 coins");
                this.UnequipRightHand(this.RightHand); // make the coins disappear
                this.CurrentCell.Add(Item.CopyItemFromDictionary(Item.ID_MUGWORT));
            }
        }

        protected void MerchantBalmSeller()
        {
            if (this.CurrentCell == null) return;

            if (Rules.RollD(1, 100) < 10)
            {
                CommandTasker.ParseCommand(this, "say", "Fresh Balm of Gilead here!");
            }

            ExchangeCoinsForBalm(this.CurrentCell);
        }

        public void SendMerchantList(PC chr)
        {
            if (this.FlaggedUniqueIDs.Contains(chr.UniqueID))
            {
                chr.WriteToDisplay(this.Name + ": I am quite busy. You will have to come back later.");
                return;
            }

            List<StoreItem> storeInventory = DAL.DBWorld.LoadStoreItems(this.SpawnZoneID); // storeInventory of StoreItem objects

            if (storeInventory.Count <= 0)
            {
                chr.WriteToDisplay(this.Name + ": I have been cleaned out! It's a glorious day indeed. Come back later and I might have something for you.");
                return;
            }

            string header = "Name          Price Stocked   Name          Price Stocked";

            bool devRequest = false;

            if (chr.ImpLevel >= Globals.eImpLevel.GM & chr.protocol != "old-kesmai") // old-kesmai not wide enough
            {
                header = "ID     Name          Price Qty Seller           ID     Name          Price Qty Seller";
                devRequest = true;
            }

            chr.WriteToDisplay(header);

            int i = 0, j = 6;
            System.Text.StringBuilder sb = new System.Text.StringBuilder(60);
            while (i < 6)
            {
                sb = new System.Text.StringBuilder(60);
                if (i < storeInventory.Count)
                    sb.Append(this.WritePriceline(chr.protocol, storeInventory[i], "left", devRequest));

                if (j < storeInventory.Count)
                    sb.Append(this.WritePriceline(chr.protocol, storeInventory[j], "right", devRequest));

                if (i < storeInventory.Count || j < storeInventory.Count)
                {
                    chr.WriteToDisplay(sb.ToString().Replace("-1", "--"));
                }
                i++; j++;
            }
        }

        // Write an item name and its price to the stream with KP formatting
        // align should be "left" or "right"
        private string WritePriceline(string proto, StoreItem storeItem, string align, bool devRequest)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (align == "right") { sb.Append("   "); }

            if (devRequest) sb.Append(storeItem.stockID.ToString().PadRight(7, ' '));

            sb.Append(Item.GetItemNameFromItemDictionary(storeItem.itemID).PadRight(13, ' '));
            sb.Append(storeItem.sellPrice.ToString().PadLeft(6, ' '));

            if (devRequest)
            {
                sb.Append(storeItem.stocked.ToString().PadLeft(4, ' '));

                if (storeItem.original)
                {
                    sb.Append(" Original Stock");
                }
                else
                {
                    if (storeItem.seller != null)
                    {
                        sb.Append(" " + storeItem.seller.ToString().PadRight(14, ' '));
                    }
                    else
                    {
                        sb.Append(" Unknown       ");
                    }
                }
            }
            else
                sb.Append(storeItem.stocked.ToString().PadLeft(8, ' '));

            return sb.ToString();
        }

        public bool MerchantShowItem(Character viewer, string args)
        {
            #region Reasons not to show an item.
            if (this.FlaggedUniqueIDs.Contains(viewer.UniqueID))
            {
                viewer.WriteToDisplay(this.Name + ": I am quite busy. You will have to come back later.");
                return false;
            }

            if (args == null || args == "")
            {
                viewer.WriteToDisplay(this.Name + ": What do you want me to show you?");
                return false;
            }
            #endregion

            args = args.Replace("show ", ""); // just in case...

            List<StoreItem> storeList = DAL.DBWorld.LoadStoreItems(this.SpawnZoneID);

            Item item = null;

            string[] sArgs = args.Split(" ".ToCharArray());

            if (sArgs.Length == 1) // name, show <item> (<item> is only arg)
            {
                foreach (StoreItem storeItem in storeList)
                {
                    if (Item.GetItemNameFromItemDictionary(storeItem.itemID).ToLower() == sArgs[0].ToLower())
                    {
                        item = Item.CopyItemFromDictionary(storeItem.itemID);
                        MerchantAppraisal(viewer, item, storeItem);
                        return true;
                    }
                }
            }

            if (sArgs.Length >= 2) // name, show <#> <item> OR name, show all <item>
            {
                if (sArgs[0].ToLower() == "all")
                {
                    foreach (StoreItem storeItem in storeList)
                    {
                        if (Item.GetItemNameFromItemDictionary(storeItem.itemID).ToLower() == sArgs[1].ToLower())
                        {
                            item = Item.CopyItemFromDictionary(storeItem.itemID);
                            MerchantAppraisal(viewer, item, storeItem);
                        }
                    }
                    return true;
                }
                else
                {
                    // arg 0 = number or name
                    // arg 1 = name or nonsense

                    if (!Int32.TryParse(sArgs[0], out int countTo))
                    {
                        viewer.WriteToDisplay("Format: " + this.Name + ", show <item> OR " + this.Name + ", show # <item> OR " + this.Name + ", show all <item>");
                        return false;
                    }

                    int count = 1;

                    foreach (StoreItem storeItem in storeList)
                    {
                        if (Item.GetItemNameFromItemDictionary(storeItem.itemID).ToLower() == sArgs[1].ToLower())
                        {
                            if (count == countTo)
                            {
                                item = Item.CopyItemFromDictionary(storeItem.itemID);
                                MerchantAppraisal(viewer, item, storeItem);
                                return true;
                            }
                            count++;
                        }
                    }
                }
            }
            else
            {
                viewer.WriteToDisplay("Format: " + this.Name + ", show <item> OR " + this.Name + ", show # <item> OR " + this.Name + ", show all <item>");
                return false;
            }

            if (item == null)
            {
                if (sArgs.Length == 1 || args.ToLower().StartsWith("all"))
                {
                    viewer.WriteToDisplay(this.Name + ": I am not selling any of those.");
                }
                else if (sArgs.Length == 2)
                {
                    viewer.WriteToDisplay(this.Name + ": I don't have that many " + sArgs[1] + "s for sale.");
                }
                else
                {
                    viewer.WriteToDisplay("Format: " + this.Name + ", show <item> OR " + this.Name + ", show # <item> OR " + this.Name + ", show all <item>");
                }
            }

            return true;
        }

        public void MerchantAppraisal(Character viewer, Item item, StoreItem sItem)
        {
            string itmeffect = "";
            string itmSpell = "";
            string itmspecial = "";
            string itmattuned = "";
            string itmvalue = "";

            if (item.spell > 0)
            {
                itmSpell = " It contains the spell of " + GameSpell.GetSpell(item.spell).Name;

                if (item.charges == 0)
                {
                    // a scroll with 0 charges can be scribed (scrolls with 1 or greater charges disintegrate when they reach 0 after use)
                    if (item.baseType == Globals.eItemBaseType.Scroll)
                    {
                        itmSpell += " that can be scribed into a spellbook.";
                    }
                    else itmSpell += ", but there are no charges remaining.";
                }
                else if (item.charges > 1 && item.charges < 100) { itmSpell += " with " + item.charges + " charges remaining."; }
                else if (item.charges == 1) { itmSpell += " and has 1 charge remaining."; }
                else if (item.charges == -1)
                {
                    if (item.baseType == Globals.eItemBaseType.Scroll)
                        itmSpell += " and may be scribed.";
                    else itmSpell += " and has unlimited charges.";
                }
                else if (item.charges >= 100)
                {
                    itmSpell += " and has unlimited charges.";
                }
            }
            var sb = new System.Text.StringBuilder(40);
            if (item.baseType == Globals.eItemBaseType.Figurine || item.figExp > 0)
            {
                sb.AppendFormat(" The {0}'s avatar has " + item.figExp + " experience.", item.name);
            }
            if (item.combatAdds > 0)
            {
                sb.AppendFormat(" The combat adds are {0}.", item.combatAdds);
            }
            if (item.silver)
            {
                string silver = "silver";

                if (item.longDesc.ToLower().Contains("mithril") || item.armorType == Globals.eArmorType.Mithril)
                    silver = "mithril silver";

                sb.AppendFormat(" The {0} is " + silver + ".", item.name);
            }
            if (item.blueglow)
            {
                sb.AppendFormat(" The {0} is emitting a faint blue glow.", item.name);
            }
            if (item.alignment != Globals.eAlignment.None)
            {
                sb.AppendFormat(" The {0} is {1}.", item.name, Utils.FormatEnumString(item.alignment.ToString()).ToLower());
            }

            itmspecial = sb.ToString();

            if (item.effectType.Length > 0)
            {
                string[] itmEffectType = item.effectType.Split(" ".ToCharArray());
                string[] itmEffectAmount = item.effectAmount.Split(" ".ToCharArray());

                if (itmEffectType.Length == 1 && Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])) != "")
                {
                    if (item.baseType == Globals.eItemBaseType.Bottle)
                    {
                        Bottle bottle = (Bottle)item;
                        itmeffect = Bottle.GetFluidDesc(bottle);
                        itmeffect += " The " + item.name + " is a potion of " + Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])) + ".";
                    }
                    else
                    {
                        string effectName = Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0]));

                        if (effectName.ToLower() != Effect.EffectTypes.None.ToString().ToLower() && effectName.ToLower() != Effect.EffectTypes.Weapon_Proc.ToString().ToLower())
                            itmeffect = " The " + item.name + " contains the enchantment of " + Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])) + ".";
                    }
                }
                else
                {
                    List<string> itemEffectList = new List<string>();

                    for (int a = 0; a < itmEffectType.Length; a++)
                    {
                        Effect.EffectTypes effectType = (Effect.EffectTypes)Convert.ToInt32(itmEffectType[a]);
                        if (Effect.GetEffectName(effectType).ToLower() != "unknown" && Effect.GetEffectName(effectType).ToLower() != Effect.EffectTypes.Weapon_Proc.ToString().ToLower())
                        {
                            itemEffectList.Add(Effect.GetEffectName(effectType));
                        }
                    }

                    itemEffectList.Remove(Effect.EffectTypes.None.ToString());

                    if (itemEffectList.Count > 0)
                    {
                        if (itemEffectList.Count > 1)
                        {
                            itmeffect = " The " + item.name + " contains the enchantments of";
                            for (int a = 0; a < itemEffectList.Count; a++)
                            {
                                if (a != itemEffectList.Count - 1)
                                {
                                    itmeffect += " " + (string)itemEffectList[a] + ",";
                                }
                                else
                                {
                                    itmeffect += " and " + (string)itemEffectList[a] + ".";
                                }
                            }
                        }
                        else if (itemEffectList.Count == 1 &&
                            Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])).ToLower() != "unknown" &&
                            Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])).ToLower() != Effect.EffectTypes.Weapon_Proc.ToString().ToLower())
                        {
                            if (item.baseType == Globals.eItemBaseType.Bottle)
                            {
                                itmeffect = " Inside the bottle is a potion of " + Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])) + ".";
                            }
                            else
                            {
                                itmeffect = " The " + item.name + " contains the enchantment of " + Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])) + ".";
                            }
                        }
                    }
                    else
                    {
                        if (item.baseType == Globals.eItemBaseType.Bottle)
                        {
                            Bottle bottle = (Bottle)item;
                            bool bottleOpen = bottle.IsOpen;
                            bottle.IsOpen = true;
                            itmeffect = Bottle.GetFluidDesc(bottle);
                            bottle.IsOpen = bottleOpen;
                        }
                    }
                }
            }

            #region Attuned / Soulbound
            if (item.attunedID != 0)
            {
                if (item.attunedID == viewer.UniqueID) { itmattuned = " The " + item.name + " is soulbound to you."; }
                else { itmattuned = " The " + item.name + " is soulbound to another individual."; }
            }
            #endregion

            #region Coin Value
            if (sItem.sellPrice == 1)
                itmvalue = " The " + item.name + " is worth 1 coin.";
            else if (sItem.sellPrice > 1)
                itmvalue = " The " + item.name + " is worth " + sItem.sellPrice + " coins.";
            else
                itmvalue = " The " + item.name + " has no monetary value.";
            #endregion

            viewer.WriteToDisplay(Name + ": We are looking at " + item.longDesc + "." + itmeffect + itmSpell + itmspecial + itmattuned + itmvalue);

        }

        public string MerchantSellItem(Character buyer, string args) // merchant sells an item to a character
        {
            if (this.FlaggedUniqueIDs.Contains(buyer.UniqueID))
                return this.Name + ": I am quite busy. You will have to come back later.";

            string requestedItemName = "";

            int countTo = 1; // which item in the display listing (2nd necklace, 4th necklace, etc)
            int amount = 1; // how many items to sell
            bool maxSale = false; // sell as many items as there is gold

            string[] sArgs = args.Split(" ".ToCharArray());

            // <name>, sell <num> <item>
            // <name>, sell <num> <item> <amount>
            // <name>, sell <item> <amount>

            if (sArgs.Length == 1) // <name>, sell (<item>)
                requestedItemName = sArgs[0];
            else if (sArgs.Length == 2) // <name>, sell (# <item>)
            {
                try
                {
                    if (!Int32.TryParse(sArgs[0], out countTo))
                        return this.Name + ": That is not possible.";

                    if (countTo < 1) // in case a player types in 0 or less as the countTo
                        return this.Name + ": That is not possible.";

                    requestedItemName = sArgs[1];

                }
                catch
                {
                    try
                    {
                        if (sArgs[1].ToLower() != "max")
                        {
                            if (!Int32.TryParse(sArgs[1], out amount))
                                return this.Name + ": That is not possible.";

                            if (amount < 1)
                                return this.Name + ": That is not possible.";
                        }
                        else
                            maxSale = true;

                        requestedItemName = sArgs[0];
                    }
                    catch
                    {
                        return this.Name + ": What do you want me to sell to you?";
                    }
                }
            }
            else if (sArgs.Length == 3) // <name>, sell (<num> <item> <amount | max>)
            {
                requestedItemName = sArgs[1];

                if (!Int32.TryParse(sArgs[0], out countTo))
                    return this.Name + ": That is not possible.";

                if (sArgs[2].ToLower() == "max")
                    maxSale = true;
                else
                {
                    try
                    {
                        if (!Int32.TryParse(sArgs[2], out amount))
                            return this.Name + ": That is not possible.";

                        if (amount < 1)
                            return this.Name + ": That is not possible.";
                    }
                    catch
                    {
                        return this.Name + ": What do you want me to sell to you?";
                    }
                }
            }

            #region Requested item is a spellbook or book.
            if (requestedItemName.ToLower() == "spellbook")
            {
                if (this.trainerType == TrainerType.Spell)
                {
                    Item newSpellbook = Item.CopyItemFromDictionary(Item.ID_SPELLBOOK);
                    newSpellbook.AttuneItem(buyer.UniqueID, "Requested new spellbook from spell trainer.");
                    buyer.EquipEitherHandOrDrop(newSpellbook);
                    Utils.Log("[" + this.SpawnZoneID + "][" + this.npcID + "] " + this.Name + " sold [" + newSpellbook.itemID + "]" + newSpellbook.name + " to " + this.TargetID + ".", Utils.LogType.MerchantSell);
                    return this.Name + ": Be careful not to lose this one.";
                }
                else
                {
                    return this.Name + ": I don't sell those. Why don't you try the Grand Library of Mu?";
                }
            }
            #endregion
            #region Requested item is a totem.
            else if (requestedItemName.ToLower() == "totem")
            {
                if (this.trainerType == TrainerType.Spell && this.BaseProfession == ClassType.Sorcerer && this.BaseProfession == buyer.BaseProfession)
                {
                    Item newTotem = Item.CopyItemFromDictionary(Item.ID_BLOODWOOD_TOTEM);
                    newTotem.AttuneItem(buyer.UniqueID, "Requested new totem from spell trainer.");
                    buyer.EquipEitherHandOrDrop(newTotem);
                    //Map.PutItemOnCounter(this, newTotem);
                    Utils.Log("[" + this.SpawnZoneID + "][" + this.npcID + "] " + this.Name + " sold [" + newTotem.itemID + "]" + newTotem.name + " to " + this.TargetID + ".", Utils.LogType.MerchantSell);
                    return this.Name + ": Do not lose this one!";
                }
                else
                    return this.Name + ": I don't sell those.";
            }
            #endregion

            List<StoreItem> storeInventory = DAL.DBWorld.LoadStoreItems(this.SpawnZoneID); // inventory of StoreItem objects

            StoreItem requestedStoreItem = null;
            int count = 1;

            foreach (StoreItem storeItem in storeInventory)
            {
                if (Item.GetItemNameFromItemDictionary(storeItem.itemID).ToLower() == requestedItemName.ToLower())
                {
                    if (count == countTo)
                    {
                        requestedStoreItem = storeItem;

                        // If 0, store item would have been deleted. If -1 then it is unlimited stock.
                        if (requestedStoreItem.stocked > 0 && requestedStoreItem.stocked < amount)
                        {
                            return this.Name + ": I only have " + requestedStoreItem.stocked + " of those in stock.";
                        }

                        break; // break the foreach loop as the item was found
                    }
                    count++;
                }
            }

            if (requestedStoreItem != null)
            {
                Item coin = Map.RemoveItemFromCounter(this, "coins");

                if (coin == null)
                {
                    coin = Item.RemoveItemFromGround("coins", this.CurrentCell);
                }

                Item item = Item.CopyItemFromDictionary(requestedStoreItem.itemID);//Item.getItemIDFromCatalogItems(itemName));
                item.charges = requestedStoreItem.charges;
                item.figExp = requestedStoreItem.figExp;

                if (coin != null && coin.coinValue >= (requestedStoreItem.sellPrice * amount)) // amount of coins on counter greater than or equal to sellPrice
                {
                    // last argument was "max" -- sell as many as possible until coins depleted
                    if (maxSale)
                    {
                        if (coin.coinValue >= (requestedStoreItem.sellPrice * requestedStoreItem.stocked))
                        {
                            amount = requestedStoreItem.stocked;
                        }
                        else if (coin.coinValue >= requestedStoreItem.sellPrice * 2)
                        {
                            amount = Convert.ToInt32(coin.coinValue / requestedStoreItem.sellPrice);
                        }
                    }

                    // give change
                    if (coin.coinValue >= (requestedStoreItem.sellPrice * amount))
                    {
                        coin.coinValue -= (requestedStoreItem.sellPrice * amount);

                        if (coin.coinValue > 0)
                        {
                            if (Map.IsNextToCounter(this))
                                Map.PutItemOnCounter(this, coin);
                            else this.CurrentCell.Add(coin);
                        }
                    }

                    int counter = amount;

                    while (counter > 0)
                    {
                        if (Map.IsNextToCounter(this))
                            Map.PutItemOnCounter(this, item); // place the item on the counter
                        else this.CurrentCell.Add(item);

                        // generate random coin value if necessary??
                        if (item.vRandLow > 0)
                        {
                            item.coinValue = Rules.Dice.Next(item.vRandLow, item.vRandHigh);
                        }

                        item = Item.CopyItemFromDictionary(requestedStoreItem.itemID);
                        item.charges = requestedStoreItem.charges;
                        item.figExp = requestedStoreItem.figExp;

                        counter--;

                        // reduce stock if not unlimited
                        if (requestedStoreItem.stocked > 0)
                            requestedStoreItem.stocked--; // reduce stock
                    }

                    if (!requestedStoreItem.original && requestedStoreItem.stocked == 0) // if true delete this record from the Stores table in the database
                    {
                        DAL.DBWorld.DeleteStoreItem(requestedStoreItem.stockID);
                    }
                    else // otherwise just update the stocked amount
                    {
                        DAL.DBWorld.UpdateStoreItem(requestedStoreItem.stockID, requestedStoreItem.stocked);
                    }

                    Utils.Log("[" + SpawnZoneID + "][" + npcID + "] " + Name + " sold [" + item.itemID + "]" + item.name + " to " + TargetID + ".", Utils.LogType.MerchantSell);

                    if (amount > 1) requestedItemName = GameSystems.Text.TextManager.Multinames(requestedItemName);

                    buyer.WriteToDisplay("You purchased " + amount + " " + requestedItemName + " from " + Name + ".");

                    return Name + ": Thank you for your business.";
                }
                else
                {
                    if (coin == null)
                    {
                        return Name + ": I don't see any coins here.";
                    }
                    else
                    {
                        if (Map.IsNextToCounter(this))
                            Map.PutItemOnCounter(this, coin);
                        else CurrentCell.Add(coin);

                        if (amount == 1)
                        {
                            return Name + ": There aren't enough coins here for the " + item.name + ".";
                        }
                        else return Name + ": There aren't enough coins here for " + amount + " " + item.name + "s.";
                    }
                }
            }
            else
            {
                return Name + ": I don't sell that.";
            }
        }

        public string MerchantBuyItem(Character chr, string args) // merchant buys an item from the character
        {
            // name, buy <item>
            // name, buy all
            // name, buy # <item>

            if (FlaggedUniqueIDs.Contains(chr.UniqueID))
                return Name + ": I am quite busy. You will have to come back later.";

            string[] sArgs = args.Split(" ".ToCharArray());
            string itemName = sArgs[0];

            bool buyAllSpecific = false;

            Cell counterCell = Map.GetNearestCounterOrAltarCell(CurrentCell);

            if (sArgs[0].ToLower().EndsWith("s"))
            {
                foreach (Item item in counterCell.Items)
                {
                    if (item.name.ToLower() == sArgs[1].ToLower().Substring(0, sArgs[1].Length - 1))
                    {
                        buyAllSpecific = true;
                        itemName = item.name;
                        break;
                    }
                }
            }

            try
            {
                List<StoreItem> storeInventory = DAL.DBWorld.LoadStoreItems(this.SpawnZoneID); // get the merchant's current store inventory
                string purchaseinfo = ""; // information sent to the character about purchases
                bool addToInventory = true; // will this item be inserted as a record into the Store table
                                            // asked to buy everything on the counter
                if (sArgs[0] == "all" || sArgs[0] == "items" || sArgs[0] == "everything" || buyAllSpecific)
                {
                    if (counterCell.Items.Count < 1)
                    {
                        return Name + ": There is nothing here to buy.";
                    }
                    else
                    {
                        double totalPurchase = 0;
                        //create a temporary array to keep track of purchased items
                        ArrayList purchasedItems = new ArrayList();

                        foreach (Item counterItem in counterCell.Items)
                        {
                            addToInventory = true;
                            bool buyItem = true;
                            //make a copy of the item from catalog to compare the two
                            Item cloneItem = Item.CopyItemFromDictionary(counterItem.itemID);
                            //does the item have less charges than max
                            if (counterItem.charges > -1 && counterItem.charges < cloneItem.charges)
                            {
                                //if the merchant has lower intelligence than the player let's random to see if we decrease the cValue
                                if (Intelligence < chr.Intelligence)
                                {
                                    //second check to see if we lower the value
                                    if (Rules.RollD(1, 20) < Intelligence)
                                    {
                                        counterItem.coinValue = counterItem.coinValue - (int)(counterItem.coinValue / (cloneItem.charges - counterItem.charges));
                                        purchaseinfo += " The " + counterItem.name + " has less than its normal magical charges so I have paid you less for it.";
                                    }
                                }
                            }
                            //do not buy an item that has been depleted of charges
                            if (counterItem.charges == 0 && cloneItem.charges > 0) // does not include scribe scrolls as they start with 0 charges
                            {
                                buyItem = false;
                                purchaseinfo += " The " + counterItem.name + " is out of charges so I am not interested.";
                            }
                            //do not buy an item that is attuned
                            if (counterItem.attunedID != 0)
                            {
                                buyItem = false;
                                purchaseinfo += " The " + counterItem.name + " is soulbound to an individual so I am not interested.";
                            }
                            //do not buy a soul gem
                            if (counterItem is SoulGem)
                            {
                                buyItem = false;
                                purchaseinfo += " The " + counterItem.name + " is not something I can sell.";
                            }
                            // scribed scrolls 
                            if (counterItem.baseType == Globals.eItemBaseType.Scroll && counterItem.spell > -1 && counterItem.charges <= 0)
                            {
                                addToInventory = false;
                            }
                            //do not buy an item that has no cValue
                            if (counterItem.coinValue < 1)
                            {
                                buyItem = false;
                                purchaseinfo += " The " + counterItem.name + " has no monetary value.";
                            }
                            if (buyItem && counterItem.itemType != Globals.eItemType.Coin)
                            {
                                //put this item in our temporary array
                                if (buyAllSpecific)
                                {
                                    if (counterItem.name == itemName)
                                    {
                                        purchasedItems.Add(counterItem);
                                        totalPurchase += counterItem.coinValue;
                                    }
                                }
                                else
                                {
                                    purchasedItems.Add(counterItem);
                                    totalPurchase += counterItem.coinValue;
                                }

                                foreach (StoreItem sItem in storeInventory)
                                {
                                    //if this item is already in our inventory increase the stock amount
                                    if (counterItem.itemID == sItem.itemID)
                                    {
                                        if (DAL.DBWorld.UpdateStoreItem(sItem.stockID, sItem.stocked + 1) == 1)
                                        {
                                            //update our Inventory array with the increase in stock amount
                                            sItem.stocked = sItem.stocked + 1;
                                            //log this transaction if the menu item is checked
                                            if (chr is PC)
                                                Utils.Log(this.GetLogString() + " bought and increased stock of item [" + counterItem.itemID + "]" + counterItem.name + " from " + chr.GetLogString(), Utils.LogType.MerchantBuy);
                                            //don't insert this item as a record into Store
                                            addToInventory = false;
                                        }
                                    }
                                }
                                //if this item wasn't updated and our Inventory isn't full
                                if (addToInventory && storeInventory.Count < MAX_STORE_INVENTORY)
                                {
                                    //run a check to see if we're the right merchantType to insert this item into our Inventory
                                    if (StoreItem.VerifyStoreInsert(counterItem, this.merchantType))
                                    {
                                        StoreItem storeItem = new StoreItem();
                                        storeItem.itemID = counterItem.itemID;
                                        storeItem.original = false;
                                        storeItem.sellPrice = (int)counterItem.coinValue * this.merchantMarkup;
                                        storeItem.stocked = 1;
                                        storeItem.seller = chr.Name;
                                        storeItem.restock = 0;
                                        storeItem.charges = counterItem.charges;
                                        storeItem.figExp = counterItem.figExp;
                                        if (DAL.DBWorld.InsertStoreItem(this.SpawnZoneID, storeItem) != -1)
                                        {
                                            storeInventory = DAL.DBWorld.LoadStoreItems(this.SpawnZoneID); // update Inventory to reflect the addition of this item
                                            if (chr is PC)
                                                Utils.Log("[" + this.SpawnZoneID + "][" + this.npcID + "]" + this.Name + " bought and added store item [" + counterItem.itemID + "]" + counterItem.name + " from [" + chr.UniqueID + "]" + chr.Name + "(" + (chr as PC).Account.accountName + ").", Utils.LogType.MerchantBuy);
                                        }
                                    }
                                }
                            }
                        }
                        foreach (Item pItem in purchasedItems)
                            counterCell.Remove(pItem);

                        if (totalPurchase < 1)
                            return this.Name + ": There is nothing of value here.";

                        Item totalSale = Item.CopyItemFromDictionary(Item.ID_COINS);
                        totalSale.coinValue = totalPurchase;
                        Map.PutItemOnCounter(this, totalSale);
                    }
                }
                else
                {
                    Item purchaseItem = null;
                    Item compareItem = null;

                    if (sArgs.Length == 1)
                    {
                        purchaseItem = Map.RemoveItemFromCounter(this, itemName);

                        if (purchaseItem != null)
                            compareItem = Item.CopyItemFromDictionary(purchaseItem.itemID);
                    }
                    else if (sArgs.Length == 2)
                    {
                        itemName = sArgs[1];

                        int countRequest = 1;

                        if (!Int32.TryParse(sArgs[0], out countRequest))
                            return this.Name + ": What do you want me to buy?";

                        if (countRequest <= 0)
                            countRequest = 1;

                        int itemFound = 0;

                        foreach (Item found in counterCell.Items)
                        {
                            if (found.name.ToLower() == itemName.ToLower() && ++itemFound == countRequest)
                            {
                                purchaseItem = Map.RemoveItemFromCounter(this, found.UniqueID);

                                if (purchaseItem != null)
                                    compareItem = Item.CopyItemFromDictionary(purchaseItem.itemID);

                                break;
                            }
                        }
                    }

                    if (purchaseItem == null)
                        return this.Name + ": I don't see a " + itemName + " here.";

                    // Do not buy coins.
                    if (purchaseItem.itemType == Globals.eItemType.Coin)
                        return this.Name + ": Do you really want me to buy your " + purchaseItem.name + "? How does an exchange of five to one sound?";

                    // Do not buy items that had charges and no longer have charges.
                    if (purchaseItem.charges < 1 && compareItem.charges > 0)
                    {
                        Map.PutItemOnCounter(this, purchaseItem);
                        return this.Name + ": The " + purchaseItem.name + " is out of charges so I am not interested. Perhaps an enchanter can recharge it for you.";
                    }

                    if (purchaseItem.attunedID != 0 && compareItem.attunedID == 0)
                    {
                        Map.PutItemOnCounter(this, purchaseItem);
                        return this.Name + ": The " + purchaseItem.name + " is soulbound to an individual so I am not interested.";
                    }

                    if (purchaseItem.coinValue < 1)
                    {
                        Map.PutItemOnCounter(this, purchaseItem);
                        return this.Name + ": The " + purchaseItem.name + " has no monetary value.";
                    }

                    if (purchaseItem.charges != 0 && purchaseItem.charges < compareItem.charges)
                    {
                        if (chr.Intelligence > this.Intelligence)
                        {
                            if (Rules.RollD(1, this.Intelligence) < this.Intelligence)
                            {
                                purchaseItem.coinValue = purchaseItem.coinValue - (int)(purchaseItem.coinValue / (compareItem.charges - purchaseItem.charges));
                                purchaseinfo += " The " + purchaseItem.name + " has less than its normal magical charges so I have paid you less for it.";
                            }
                        }
                    }

                    if (storeInventory.Count < MAX_STORE_INVENTORY)
                    {
                        foreach (StoreItem sItem in storeInventory)
                        {
                            if (purchaseItem.itemID == sItem.itemID) // if the merchant already has this item just the stockedNum is increased
                            {
                                if (DAL.DBWorld.UpdateStoreItem(sItem.stockID, sItem.stocked + 1) == 1)
                                {
                                    if (chr is PC)
                                        Utils.Log(this.GetLogString() + " bought and increased stock of item [" + purchaseItem.itemID + "]" + purchaseItem.name + " from " + chr.GetLogString() + ".", Utils.LogType.MerchantBuy);
                                    addToInventory = false;
                                }
                            }
                        }
                    }

                    if (this.RightHand != null) { this.RightHand = null; }

                    if (addToInventory && storeInventory.Count < MAX_STORE_INVENTORY)
                    {
                        if (StoreItem.VerifyStoreInsert(purchaseItem, this.merchantType))
                        {
                            StoreItem storeItem = new StoreItem();
                            storeItem.itemID = purchaseItem.itemID;
                            storeItem.original = false;
                            storeItem.sellPrice = (int)purchaseItem.coinValue * this.merchantMarkup;
                            storeItem.stocked = 1;
                            storeItem.seller = chr.Name;
                            storeItem.restock = 0;

                            storeItem.charges = purchaseItem.charges;
                            storeItem.figExp = purchaseItem.figExp;
                            if (DAL.DBWorld.InsertStoreItem(this.SpawnZoneID, storeItem) != -1)
                            {
                                if (chr is PC)
                                    Utils.Log(this.GetLogString() + " bought and added store item " + purchaseItem.GetLogString() + " from " + chr.GetLogString(), Utils.LogType.MerchantBuy);
                            }
                        }
                    }

                    Item totalCoins = Item.CopyItemFromDictionary(Item.ID_COINS);
                    totalCoins.coinValue = purchaseItem.coinValue;
                    Map.PutItemOnCounter(this, totalCoins);
                }
                return this.Name + ":" + purchaseinfo + " Thank you for your business.";
            }
            catch (Exception e)
            {
                Utils.Log("Failure at merchantBuyItem(" + chr.GetLogString() + ", " + args + ")", Utils.LogType.SystemFailure);
                Utils.LogException(e);
                return this.Name + ": I've encountered an error. Please pray to the Ghods with me.";
            }
        }

        public void MerchantWithdraw(PC chr, string amount) // withdraw money for character
        {
            if (this.interactiveType != Merchant.InteractiveType.Banker)
            {
                chr.WriteToDisplay(this.Name + ": I am not a banker.");
                return;
            }

            if (this.FlaggedUniqueIDs.Contains(chr.UniqueID))
            {
                chr.WriteToDisplay(this.Name + ": I am quite busy. You will have to come back later.");
                return;
            }

            if (String.IsNullOrEmpty(amount))
            {
                chr.WriteToDisplay(this.Name + ": Please tell me how much you wish to withdraw.");
                return;
            }

            if (amount.StartsWith("-"))
            {
                chr.WriteToDisplay(this.Name + ": That's interesting. Next!");
                return;
            }

            if (amount == "all")
            {
                amount = chr.bankGold.ToString();
            }

            long gps = Convert.ToInt64(amount, 10);

            if (gps <= 0)
            {
                chr.WriteToDisplay(this.Name + ": Robbing the bank is not tolerated.", ProtocolYuusha.TextType.CreatureChat);
                return;
            }

            if (chr.bankGold < gps)
            {
                chr.WriteToDisplay(this.Name + ": You don't have that many coins in your account.", ProtocolYuusha.TextType.CreatureChat);
            }
            else
            {
                Item gold = Item.CopyItemFromDictionary(Item.ID_COINS);
                gold.coinValue = Convert.ToInt64(amount);
                chr.bankGold -= Convert.ToInt64(amount);
                Map.PutItemOnCounter(this, gold);
                chr.WriteToDisplay(this.Name + ": Thanks for the business.", ProtocolYuusha.TextType.CreatureChat);
            }
        }

        public void MerchantDeposit(PC chr, string amount) // deposit money for character
        {
            if (this.FlaggedUniqueIDs.Contains(chr.UniqueID))
            {
                chr.WriteToDisplay(this.Name + ": I am quite busy. You will have to come back later.");
                return;
            }

            if (amount.StartsWith("-"))
            {
                chr.WriteToDisplay(this.Name + ": That's interesting. Next!");
                return;
            }

            try
            {
                if (amount == null || amount == "" || amount == "coins" || amount == "gold" || amount == "all" || amount == "deposit")
                {
                    Item gold = Map.RemoveItemFromCounter(this, "coins");

                    (chr as PC).bankGold += gold.coinValue;
                    chr.WriteToDisplay(this.Name + ": You now have " + (chr as PC).bankGold + " coins in your account.", ProtocolYuusha.TextType.CreatureChat);
                    Map.RemoveItemFromCounter(this, "coins");
                }
                else
                {
                    //long gps = Convert.ToInt64(amount, 10);                 // This will fail if the argument is non-numeric and not coins,gold,all, or blank.
                    long gps = 0;

                    if (!Int64.TryParse(amount, out gps))
                    {
                        chr.WriteToDisplay(this.Name + ": I don't see " + amount + " coins here.", ProtocolYuusha.TextType.CreatureChat);
                        return;
                    }

                    Item gold = Map.RemoveItemFromCounter(this, "coins");

                    // make sure there are enough coins on the counter
                    if (gold.coinValue > gps)
                    {
                        gold.coinValue -= gps;
                        (chr as PC).bankGold += gps;
                        chr.WriteToDisplay(this.Name + ": You now have " + chr.bankGold + " coins in your account.", ProtocolYuusha.TextType.CreatureChat);
                        Map.PutItemOnCounter(this, gold);
                    }
                    else if (gold.coinValue == gps)
                    {
                        (chr as PC).bankGold += gps;
                    }
                    else
                    {
                        chr.WriteToDisplay(this.Name + ": I don't see " + amount + " coins here.", ProtocolYuusha.TextType.CreatureChat);
                        Map.PutItemOnCounter(this, gold);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        /// <summary>
        /// Train the character if gold is on the counter or in the Cell. "max" may also be used as an argument (args[1]) to train from bank account.
        /// </summary>
        /// <param name="chr">Player character requesting training.</param>
        /// <param name="trainToMax">True if training will use bank gold.</param>
        public void MerchantTrain(PC chr, string[] sArgs)
        {
            // Do not train a player that is flagged as attacking this Merchant.
            if (FlaggedUniqueIDs.Contains(chr.UniqueID))
            {
                chr.WriteToDisplay(Name + ": I am quite busy. You will have to come back later.");
                return;
            }

            Item gold = null;

            bool trainToMax = sArgs.Length >= 1 && sArgs[0].ToLower() == "max"; // train to max (use bank account gold and spend it all if possible)

            double specifiedAmount = 0;

            // name, train amount
            if (sArgs.Length >= 1 && Double.TryParse(sArgs[0], out specifiedAmount))
            {
                if (specifiedAmount <= 0)
                {
                    chr.WriteToDisplay(Name + ": I am quite busy. Come back later when you are ready to focus on training.");
                    return;
                }
            }

            if (trainToMax && chr.bankGold > 0) // gold from bank account will be used to max
            {
                gold = Item.CopyItemFromDictionary(Item.ID_COINS);
                gold.coinValue = chr.bankGold;
                gold.land = -2; // -2 will be used later to determine gold came from bank
            }
            else // gold coming from the ground or counter
            {
                gold = Map.RemoveItemFromCounter(this, "coins");

                if (gold == null)
                {
                    gold = Item.RemoveItemFromGround("coins", this.CurrentCell);

                    if (gold != null) gold.land = this.LandID; // this means it was taken from the ground
                }
                else gold.land = -1; // -1 means it was taken from the counter

                if (specifiedAmount > 0)
                {
                    if (gold != null && gold.coinValue < specifiedAmount)
                    {
                        chr.WriteToDisplay(this.Name + ": There is not enough gold here.");
                        switch (gold.land)
                        {
                            case -1:
                                Map.PutItemOnCounter(chr, gold);
                                break;
                            default:
                                this.CurrentCell.Add(gold);
                                break;
                        }
                        return;
                    }

                    // found gold on the counter or ground
                    if (gold != null && gold.coinValue >= specifiedAmount)
                    {
                        int landID = gold.land;
                        gold.coinValue -= specifiedAmount;
                        switch (gold.land)
                        {
                            case -1:
                                Map.PutItemOnCounter(chr, gold);
                                break;
                            default:
                                this.CurrentCell.Add(gold);
                                break;
                        }
                        gold = Item.CopyItemFromDictionary(Item.ID_COINS);
                        gold.coinValue = specifiedAmount;
                        gold.land = landID;
                    }
                    else if (gold == null && chr.bankGold >= specifiedAmount)
                    {
                        if (chr.bankGold >= specifiedAmount)
                        {
                            gold = Item.CopyItemFromDictionary(Item.ID_COINS);
                            gold.coinValue = specifiedAmount;
                            gold.land = -3; // -3 will be used in SkillTrain to determine how much gold to remove from the bank
                        }
                        else
                        {
                            chr.WriteToDisplay(this.Name + ": You do not have enough gold available in the bank.");
                            return;
                        }
                    }
                }
            }

            if (gold == null)
            {
                if (!trainToMax)
                    chr.WriteToDisplay(Name + ": There is no gold here.");
                else if (trainerType == TrainerType.Sage)
                    chr.WriteToDisplay(Name + ": Beware of offering advice on credit.");
                else chr.WriteToDisplay(Name + ": You do not have enough gold in the bank.");
                return;
            }

            if (trainerType == TrainerType.Sage)
            {
                #region Sage
                // Sage does not offer advice on credit.
                if (gold.land == -2 || gold.land == -3)
                {
                    chr.WriteToDisplay(this.Name + ": Beware of offering advice on credit.");
                }
                else
                {
                    chr.Experience += (int)(gold.coinValue / 2);
                    chr.WriteToDisplay(Name + ": " + GetSageAdvice());
                }
                return;
                #endregion
            }
            else if(trainerType == TrainerType.Animal)
            {
                if(sArgs == null || sArgs.Length <= 0)
                {
                    chr.WriteToDisplay(Name + ": Show me which animal shall be trained.");
                    CurrentCell.Add(gold);
                    return;
                }

                Character target = GameSystems.Targeting.TargetAquisition.FindTargetInCell(this, sArgs[0]);

                if(target == null)
                {
                    chr.WriteToDisplay(Name + ": I do not see a " + sArgs[0] + " here.");
                    CurrentCell.Add(gold);
                    return;
                }
                else if(!(target is NPC) || !(target as NPC).animal && !Autonomy.EntityBuilding.EntityLists.IsAnimal(target.entity))
                {
                    chr.WriteToDisplay(Name + ": " + target.GetNameForActionResult(false) + " is no animal.");
                    CurrentCell.Add(gold);
                    return;
                }
                else
                {
                    target.Experience += (int)(gold.coinValue / 3);
                    SendToAllInSight(Name + " blows in a strange device hanging on a chain around " + PRONOUN_2[(int)gender].ToLower() + " neck. " + target.GetNameForActionResult(false) + " bows its head and obeys " + POSSESSIVE[(int)gender].ToLower() + " commands.");
                    EmitSound(GameSpell.GetSpell((int)GameSpell.GameSpellID.Summon_Nature__s_Ally).SoundFile);
                }
                return;
            }
            else if (trainerType == TrainerType.HP_Doctor)
            {
                #region Hit Point Doctor
                short hpDoctorRequest = 0; // request hp doctoring

                if (sArgs.Length > 1 && char.IsNumber(sArgs[1].ToCharArray()[0]))
                {
                    hpDoctorRequest = Convert.ToInt16(sArgs[1]);
                }

                if (FlaggedUniqueIDs.Contains(chr.UniqueID))
                {
                    chr.WriteToDisplay(this.Name + ": I am quite busy. You will have to come back later.");
                    switch (gold.land)
                    {
                        case -1:
                            Map.PutItemOnCounter(this, gold);
                            break;
                        case -2:  // do nothing if the gold value came from the bank
                            break;
                        default:
                            this.CurrentCell.Add(gold);
                            break;
                    }
                    return;
                }

                int doctorLimit = World.DoctoredHPLimits[(int)chr.BaseProfession];

                if (chr.HitsDoctored >= doctorLimit)
                {
                    chr.WriteToDisplay(this.Name + ": You have reached your limit of " + doctorLimit + " doctored hit points.");
                    switch (gold.land)
                    {
                        case -1:
                            Map.PutItemOnCounter(this, gold);
                            break;
                        case -2:   // do nothing if the gold value came from the bank
                            break;
                        default:
                            this.CurrentCell.Add(gold);
                            break;
                    }
                    return;
                }

                long nextHPCost = 0;
                int totalHPDoctored = 0;
                long totalCost = 0;

                nextHPCost = Rules.Formula_DoctoredHPCost(chr as PC);

                // Specific amount of hit points requested.
                if (hpDoctorRequest > 0)
                {
                    while (totalHPDoctored < hpDoctorRequest && gold.coinValue >= nextHPCost && chr.HitsDoctored < doctorLimit)
                    {
                        chr.HitsDoctored++;
                        totalHPDoctored++;
                        gold.coinValue = gold.coinValue - (double)nextHPCost;
                        totalCost += nextHPCost;
                        nextHPCost = Rules.Formula_DoctoredHPCost(chr as PC);
                    }
                } // Training to max was selected or gold came from counter/altar/ground.
                else if (trainToMax || gold.land > -2)
                {
                    while (gold.coinValue >= nextHPCost && chr.HitsDoctored < doctorLimit)
                    {
                        chr.HitsDoctored++;
                        totalHPDoctored++;
                        gold.coinValue = gold.coinValue - (double)nextHPCost;
                        totalCost += nextHPCost;
                        nextHPCost = Rules.Formula_DoctoredHPCost(chr as PC);
                    }
                }
                else
                {
                    chr.WriteToDisplay(this.Name + ": There is no gold here. I can treat you on credit via your bank account. Ask me to 'train max' or 'train #' where # is the amount of treatment (hit points) you desire.");
                    return;
                }

                if (totalHPDoctored == 0)
                {
                    switch (gold.land)
                    {
                        case -1:
                            Map.PutItemOnCounter(this, gold);
                            chr.WriteToDisplay(this.Name + ": There is not enough gold here to properly treat you.");
                            break;
                        case -2:   // do nothing if the gold value came from the bank
                            break;
                        default:
                            this.CurrentCell.Add(gold);
                            chr.WriteToDisplay(this.Name + ": There is not enough gold here to properly treat you.");
                            break;
                    }
                }
                else
                {
                    // Successful hits doctoring.

                    switch (gold.land)
                    {
                        case -1:
                            Map.PutItemOnCounter(this, gold);
                            break;
                        case -2:
                            chr.bankGold = gold.coinValue;
                            if (chr.bankGold < 0) chr.bankGold = 0;
                            break;
                        default:
                            this.CurrentCell.Add(gold);
                            break;
                    }

                    chr.WriteToDisplay("You purchased " + totalHPDoctored + " hit " + (totalHPDoctored > 1 ? "points" : "point") + " at a cost of " + String.Format("{0:n0}", totalCost) + " coins.");
                    if (gold.land == -2)
                    {
                        chr.WriteToDisplay(totalCost + " coins has been deducted from your bank account.");
                        chr.WriteToDisplay("Your new bank account balance is " + chr.bankGold + " coins.");
                    }

                    chr.WriteToDisplay(this.Name + ": I do thank you for your donation. Good hunting, " + chr.Name + ".");
                }
                #endregion
            }
            else
            {
                #region Default skill train
                // -3 = specified amount of gold from bank, -2 = gold from bank, -1 = gold from counter/altar, otherwise = ground
                switch (gold.land)
                {
                    case -3:
                    case -2:
                        if (Skills.SkillTrain(chr, this, gold))
                        {
                            if (chr.bankGold == 0)
                                chr.WriteToDisplay(this.Name + ": Your bank account is empty.", ProtocolYuusha.TextType.CreatureChat);
                            else chr.WriteToDisplay(this.Name + ": You now have " + chr.bankGold + " coins in your account.", ProtocolYuusha.TextType.CreatureChat);
                        }
                        return;
                    case -1:
                        if (!Skills.SkillTrain(chr, this, gold))
                        {
                            Map.PutItemOnCounter(chr, gold); // put the entire quantity back on the counter
                        }
                        break;
                    default:
                        if (!Skills.SkillTrain(chr, this, gold))
                        {
                            CurrentCell.Add(gold); // drop the gold
                        }
                        return;
                }
                #endregion
            }
        }

        public void MerchantShowBalance(PC chr)
        {
            if (this.interactiveType != Merchant.InteractiveType.Banker)
            {
                chr.WriteToDisplay(this.Name + ": I am not a banker.");
                return;
            }

            if (this.FlaggedUniqueIDs.Contains(chr.UniqueID))
            {
                chr.WriteToDisplay(this.Name + ": I am quite busy. You will have to come back later.");
                return;
            }

            chr.WriteToDisplay(this.Name + ": You have " + chr.bankGold + " coins in your account.");
        }

        public string GetMerchantTalentList(Character chr)
        {
            if (FlaggedUniqueIDs.Contains(chr.UniqueID))
                return Name + ": I am quite busy. You will have to come back later.";

            string talents = "";
            List<string> teachList = new List<string>();

            bool talentsAvailableElsewhere = false; // for notifying the player they have talents available to learn at a more experienced trainer

            foreach (Talents.GameTalent talent in Talents.GameTalent.GameTalentDictionary.Values)
            {
                if (!chr.IsImmortal)
                {
                    if (!talent.IsAvailableAtMentor)
                        continue;

                    if (!talent.IsProfessionElgible(chr.BaseProfession) || !talent.IsProfessionElgible(BaseProfession))
                        continue;

                    if (chr.Level < talent.MinimumLevel)
                        continue;

                    // Talent is available for player to learn, however this trainer NPC is not high enough level.
                    if (Level < talent.MinimumLevel)
                    {
                        talentsAvailableElsewhere = true;
                        continue;
                    }
                }

                if (chr.talentsDictionary.ContainsKey(talent.Command))
                    continue;

                teachList.Add(talent.Name + " (" + talent.Command + ", " + talent.PurchasePrice + " coins)");
            }

            for (int a = 0; a < teachList.Count; a++)
            {
                if (a == teachList.Count - 1 && teachList.Count > 1)
                    talents += "and " + teachList[a] + ".";
                else if (teachList.Count > 1)
                    talents += teachList[a] + ", ";
                else
                    talents += teachList[a] + ".";
            }

            if (chr.IsImmortal)
                chr.WriteToDisplay("Your immortal flag is enabled and thus you may learn any talent.");

            if (teachList.Count < 1)
                return this.Name + ": I cannot teach you anything more at this time." + (talentsAvailableElsewhere ? " However, you do look ready to learn more." : "");
            else return this.Name + ": I am willing to teach you " + talents;
        }

        public string GetMerchantSpellList(Character chr)
        {
            if (this.FlaggedUniqueIDs.Contains(chr.UniqueID))
                return this.Name + ": I am quite busy. You will have to come back later.";

            string spells = "";
            bool addSpell = false;
            List<string> teachList = new List<string>();
            string spellsNotAvailableAtTrainer = "";

            foreach (GameSpell spell in GameSpell.GameSpellDictionary.Values)
            {
                addSpell = false;

                // Is available to profession.
                if (spell.IsClassSpell(chr.BaseProfession))
                {
                    addSpell = true;
                }

                // Trainer is of required skill level.
                if (addSpell)
                {
                    if (Skills.GetSkillLevel(chr.magic) < spell.RequiredLevel)
                    {
                        addSpell = false;
                    }
                }

                // If the character already has the spell it's not available to learn.
                if (chr.spellDictionary.ContainsKey(spell.ID))
                {
                    addSpell = false;
                }

                // Spell is available on a scroll.
                if (addSpell && !spell.IsAvailableAtTrainer && spell.IsFoundForScribing)
                {
                    addSpell = false;
                    spellsNotAvailableAtTrainer = " There may " + (addSpell ? "also " : "") +
                        "be " + (teachList.Count <= 0 ? "a " : "") + "spell" + (teachList.Count > 0 ? "s" : "") +
                        " you're able to scribe if you can locate the necessary scroll" + (teachList.Count > 0 ? "s" : "") + " and a scribing crystal.";
                }

                if (addSpell)
                {
                    teachList.Add(spell.Name + " (" + spell.Command + ", " + spell.TrainingPrice + "g)");
                }
            }

            for (int a = 0; a < teachList.Count; a++)
            {
                if (a == teachList.Count - 1 && teachList.Count > 1)
                {
                    spells += "and " + teachList[a] + ".";
                }
                else if (teachList.Count > 1)
                {
                    spells += teachList[a] + ", ";
                }
                else
                {
                    spells += teachList[a] + ".";
                }
            }

            if (teachList.Count < 1)
                return this.Name + ": I cannot teach you anything more at this time." + spellsNotAvailableAtTrainer;

            return this.Name + ": I am willing to teach you " + spells + spellsNotAvailableAtTrainer;
        }

        public void MerchantAppraise(Character chr, string args)
        {
            if (this.FlaggedUniqueIDs.Contains(chr.UniqueID))
            {
                chr.WriteToDisplay(this.Name + ": I am quite busy. You will have to come back later.");
                return;
            }

            var sArgs = args.Split(" ".ToCharArray());

            if (sArgs.Length >= 1 && sArgs[0].Equals("all"))
            {
                chr.WriteToDisplay(this.Name + ": Appraising is a fine art. I will appraise one item at a time for you.");
                return;
            }

            Item item;
            int countRequest = 1;
            string itemName = "";

            if (sArgs.Length == 1)
                goto appraiseFirstItem;

            try
            {
                countRequest = Convert.ToInt32(sArgs[0]);
                if (countRequest <= 0)
                    countRequest = 1;

                if (sArgs.Length >= 2)
                    itemName = sArgs[1];
                else
                {
                    chr.WriteToDisplay(this.Name + ": I cannot appraise something I cannot see.");
                    return;
                }

                item = Map.GetItemCopyFromCounter(this, countRequest, itemName);

                if (item == null)
                {
                    chr.WriteToDisplay(this.Name + ": I don't see a " + countRequest + GetOrdinalIndicator(countRequest) + " " + itemName + " here.");
                    return;
                }
                else
                {
                    goto itemAppraisal;
                }
            }
            catch
            {
                goto appraiseFirstItem;
            }
            appraiseFirstItem:

            item = Map.GetItemCopyFromCounter(chr, 1, sArgs[0]);
            if (item == null)
            {
                chr.WriteToDisplay(this.Name + ": I don't see a " + sArgs[0] + " here.");
                return;
            }
            itemAppraisal:

            var itmeffect = "";
            var itmSpell = "";
            var itmspecial = "";
            var itmattuned = "";
            var itmvalue = "";

            if (item.spell > 0)
            {
                var spell = GameSpell.GetSpell(item.spell);

                itmSpell = " It contains the spell of " + spell.Name;

                if (item.charges == 0)
                {
                    // a scroll with 0 charges can be scribed (scrolls with 1 or greater charges disintegrate when they reach 0 after use)
                    if (item.baseType == Globals.eItemBaseType.Scroll)
                    {
                        if (chr.IsSpellWarmingProfession && spell.IsClassSpell(chr.BaseProfession))
                            itmSpell += " that you may scribe into your spellbook.";
                        else itmSpell += " that can be scribed into a spellbook.";
                    }
                    else itmSpell += ", but there are no charges remaining.";
                }
                else if (item.charges > 1) { itmSpell += " with " + item.charges + " charges remaining."; }
                else if (item.charges == 1) { itmSpell += " and has 1 charge remaining."; }
                else if (item.charges <= -1) { itmSpell += " and has unlimited charges."; }
            }

            var sb = new System.Text.StringBuilder(40);

            if (item.baseType == Globals.eItemBaseType.Figurine || item.figExp > 0)
                sb.AppendFormat(" The {0}'s avatar has " + item.figExp + " experience.", item.name);

            if (item.combatAdds > 0)
                sb.AppendFormat(" The combat adds are {0}.", item.combatAdds);

            string isOrAre = "is";
            string hasOrHave = "has";

            if (item.wearLocation == Globals.eWearLocation.Feet || item.wearLocation == Globals.eWearLocation.Hands)
            {
                isOrAre = "are";
                hasOrHave = "have";
            }

            if (item.silver)
                sb.AppendFormat(" The {0} " + isOrAre + " silver.", item.name);

            if (item.blueglow)
                sb.AppendFormat(" The {0} " + isOrAre + " emitting a faint blue glow.", item.name);

            if (item.alignment != Globals.eAlignment.None)
                sb.AppendFormat(" The {0} " + isOrAre + " {1}.", item.name, Utils.FormatEnumString(item.alignment.ToString()).ToLower());

            itmspecial = sb.ToString();

            if (item.effectType.Length > 0)
            {
                string[] itmEffectType = item.effectType.Split(" ".ToCharArray());
                string[] itmEffectAmount = item.effectAmount.Split(" ".ToCharArray());

                if (itmEffectType.Length == 1 && Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])) != "")
                {
                    if (item.baseType == Globals.eItemBaseType.Bottle)
                    {
                        Bottle bottle = (Bottle)item;
                        itmeffect = Bottle.GetFluidDesc(bottle);
                        itmeffect += " The " + item.name + " is a potion of " + Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])) + ".";
                    }
                    else if (Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])).ToLower() != "none")
                    {
                        itmeffect = " The " + item.name + " contains the enchantment of " + Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])) + ".";
                    }
                }
                else
                {
                    ArrayList itemEffectList = new ArrayList();

                    for (int a = 0; a < itmEffectType.Length; a++)
                    {
                        Effect.EffectTypes effectType = (Effect.EffectTypes)Convert.ToInt32(itmEffectType[a]);
                        if (Effect.GetEffectName(effectType).ToLower() != "none")
                        {
                            itemEffectList.Add(Effect.GetEffectName(effectType));
                        }
                    }

                    if (itemEffectList.Count > 0)
                    {
                        if (itemEffectList.Count > 1)
                        {
                            itmeffect = " The " + item.name + " contain" + ((item.wearLocation == Globals.eWearLocation.Feet || item.wearLocation == Globals.eWearLocation.Hands) ? "" : "s") + " the enchantments of";
                            for (int a = 0; a < itemEffectList.Count; a++)
                            {
                                if (a != itemEffectList.Count - 1)
                                {
                                    itmeffect += " " + (string)itemEffectList[a] + ",";
                                }
                                else
                                {
                                    itmeffect += " and " + (string)itemEffectList[a] + ".";
                                }
                            }
                        }
                        else if (itemEffectList.Count == 1)
                        {
                            if (item.baseType == Globals.eItemBaseType.Bottle)
                            {
                                itmeffect = " Inside the " + item.name + " is a potion of " + Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])) + ".";
                            }
                            else
                            {
                                itmeffect = " The " + item.name + " contain" + ((item.wearLocation== Globals.eWearLocation.Feet || item.wearLocation == Globals.eWearLocation.Hands) ? "" : "s") + " the enchantment of " + Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])) + ".";
                            }
                        }
                    }
                    else
                    {
                        if (item.baseType == Globals.eItemBaseType.Bottle)
                        {
                            Bottle bottle = (Bottle)item;
                            itmeffect = Bottle.GetFluidDesc(bottle);
                        }
                    }
                }

            }

            if (item.attunedID != 0)
            {
                if (item.attunedID == chr.UniqueID)
                    itmattuned = " The " + item.name + " " + isOrAre + " soulbound to you.";
                else
                    itmattuned = " The " + item.name + " " + isOrAre + " soulbound to another individual.";
            }

            if (item.coinValue == 1)
                itmvalue = " The " + item.name + " " + isOrAre + " worth 1 coin.";
            else if (item.coinValue > 1)
                itmvalue = " The " + item.name + " " + isOrAre + " worth " + item.coinValue + " coins.";
            else
                itmvalue = " The " + item.name + " " + hasOrHave + " no monetary value.";

            chr.WriteToDisplay(this.Name + ": We are looking at " + item.longDesc.Trim() + "." + itmeffect + itmSpell + itmspecial + itmattuned + itmvalue);
        }

        public void MerchantCritique(PC chr, string skillName)
        {
            if (this.FlaggedUniqueIDs.Contains(chr.UniqueID))
            {
                chr.WriteToDisplay(this.Name + ": I am quite busy. You will have to come back later.");
                return;
            }

            Globals.eSkillType skillType = Globals.eSkillType.None;

            if (!Enum.TryParse<Globals.eSkillType>(skillName.Replace(" ", "_"), true, out skillType))
            {
                chr.WriteToDisplay(this.Name + ": I do not know how to critique " + Utils.FormatEnumString(skillName).ToLower() + ".");
                return;
            }

            #region Found no skillType match

            #endregion

            #region Weapon skill critique requested, but this trainer is not a weapon trainer
            if ((skillType != Globals.eSkillType.Magic && skillType != Globals.eSkillType.Unarmed && skillType != Globals.eSkillType.Thievery) && this.trainerType != TrainerType.Weapon)
            {
                chr.WriteToDisplay(this.Name + ": I do not know how to critique " + Utils.FormatEnumString(skillType.ToString()).ToLower() + " skill.");
                return;
            }
            #endregion

            #region Thievery skill critique requested, but this trainer is not a thief
            if (skillType == Globals.eSkillType.Thievery && this.BaseProfession != ClassType.Thief)
            {
                chr.WriteToDisplay(this.Name + ": I do not know how to critique " + Utils.FormatEnumString(skillType.ToString()).ToLower() + " skill.");
                return;
            }
            #endregion

            #region Unarmed skill critique requested, but this trainer is not a martial arts trainer
            if (skillType == Globals.eSkillType.Unarmed && this.trainerType != TrainerType.Martial_Arts)
            {
                chr.WriteToDisplay(this.Name + ": I do not know how to critique the martial arts.");
                return;
            }
            #endregion

            #region Magic skill critique requested, but this trainer type is not a spell trainer OR is not the same class
            if (skillType == Globals.eSkillType.Magic && (this.BaseProfession != chr.BaseProfession || this.trainerType != TrainerType.Spell))
            {
                string magicType = GameSpell.GetSpellCastingNoun(chr.BaseProfession);
                if (magicType != "")
                    chr.WriteToDisplay(this.Name + ": I am not skilled in the art of " + magicType + ".");
                else chr.WriteToDisplay(this.Name + ": You know...magic?");
                return;
            }
            #endregion

            long skillExp = chr.GetSkillExperience(skillType);
            long highSkillExp = chr.GetHighSkillExperience(skillType);
            if (this.GetSkillExperience(skillType) < skillExp)
            {
                chr.WriteToDisplay(this.Name + ": You are more skilled with " + skillType.ToString().ToLower() + " skill than I.");
                return;
            }
            if (skillExp == -1 || skillType == Globals.eSkillType.None)
            {
                chr.WriteToDisplay(this.Name + ": I cannot critique that skill.");
                return;
            }

            long skillMaxExp = Skills.GetSkillToMax(Skills.GetSkillLevel(skillExp)); // get skill to max
            long skillIntoLevel = skillMaxExp - skillExp; // get skill into current skill level
            long skillRemain = Skills.GetSkillToNext(Skills.GetSkillLevel(skillExp)) - skillIntoLevel; // get skill remaining in this skill level
            int skillPercent = (int)(((float)skillRemain / (float)Skills.GetSkillToNext(Skills.GetSkillLevel(skillExp))) * 10);
            string skillTitle = Skills.GetSkillTitle(skillType, chr.BaseProfession, skillExp, chr.gender);
            string skillRank = ConvertSkillRankToString(skillPercent);
            string skillMessage = this.Name + ": You have achieved the " + skillRank + " of " + skillTitle + " in your " + Utils.FormatEnumString(skillType.ToString()).ToLower() + " skill";
            if (skillTitle.ToLower() == "untrained")
            {
                skillMessage = this.Name + ": You have not been trained with your " + Utils.FormatEnumString(skillType.ToString()).ToLower() + " skill.";
                chr.WriteToDisplay(skillMessage);
                return;
            }
            if (skillExp < highSkillExp)
            {
                skillMaxExp = Skills.GetSkillToMax(Skills.GetSkillLevel(highSkillExp));
                skillIntoLevel = skillMaxExp - highSkillExp;
                skillRemain = Skills.GetSkillToNext(Skills.GetSkillLevel(highSkillExp)) - skillIntoLevel;
                skillPercent = (int)(((float)skillRemain / (float)Skills.GetSkillToNext(Skills.GetSkillLevel(highSkillExp))) * 10);
                skillRank = ConvertSkillRankToString(skillPercent);
                skillTitle = Skills.GetSkillTitle(skillType, chr.BaseProfession, highSkillExp, chr.gender);
                skillMessage += ", however you are below your peak " + skillRank + " of " + skillTitle + ".";
            }
            else
            {
                skillMessage += ".";
            }
            chr.WriteToDisplay(skillMessage);
        }

        public void MerchantMentor(PC chr, string talentCommand)
        {
            // redundancy check
            if (this.interactiveType != InteractiveType.Mentor)
            {
                chr.WriteToDisplay(this.Name + ": I do not know how to do that.");
                return;
            }

            if (!Talents.GameTalent.GameTalentDictionary.ContainsKey(talentCommand))
            {
                chr.WriteToDisplay(this.Name + ": I am not familiar with " + talentCommand + ".");
                return;
            }

            // currently mentor must be same profession as player desiring to learn a talent
            // removed 2/15/2014 as some talents are across professions
            //if (this.BaseProfession != chr.BaseProfession)
            //{
            //    chr.WriteToDisplay(this.Name + ": Speak with a " + Utils.FormatEnumString(chr.BaseProfession.ToString()) + " about your interest.");
            //    return;
            //}

            Talents.GameTalent talent = Talents.GameTalent.GameTalentDictionary[talentCommand];

            // verify mentor can have the talent
            if (!chr.IsImmortal) // testing purposes
            {
                if (!talent.IsAvailableAtMentor || !talent.IsProfessionElgible(this.BaseProfession) || talent.MinimumLevel > this.Level)
                {
                    chr.WriteToDisplay(this.Name + ": I am not familiar with how to " + talent.Name.ToLower() + ".");
                    return;
                }

                // verify character's profession may learn the talent
                if (!talent.IsProfessionElgible(chr.BaseProfession))
                {
                    chr.WriteToDisplay(this.Name + ": A " + Utils.FormatEnumString(chr.BaseProfession.ToString()).ToLower() + " cannot learn how to " + talent.Name.ToLower() + ".");
                    return;
                }
            }
            else chr.WriteToDisplay("Your immortal flag is enabled and thus you may learn any talent.");

            // verify character does not already know the talent
            if (chr.talentsDictionary.ContainsKey(talent.Command))
            {
                chr.WriteToDisplay(this.Name + ": You already know how to " + talent.Name.ToLower() + ".");
                return;
            }

            // verify minimum level requirement is met
            if (!chr.IsImmortal && talent.MinimumLevel > chr.Level)
            {
                chr.WriteToDisplay(this.Name + ": You are not experienced enough to learn how to " + talent.Name.ToLower() + ".");
                return;
            }

            // check for gold matching purchase price
            if (talent.PurchasePrice > 0)
            {
                Item gold = Map.RemoveItemFromCounter(this, "coins");
                bool fromCounter = false;

                if (gold == null)
                    gold = Item.RemoveItemFromGround("coins", this.CurrentCell);
                else fromCounter = true;

                if (gold == null || gold.coinValue < talent.PurchasePrice)
                {
                    chr.WriteToDisplay(this.Name + ": There is not enough gold here.");

                    if (fromCounter)
                        Map.PutItemOnCounter(this, gold);
                    else this.CurrentCell.Add(gold);

                    return;
                }
                else
                {
                    gold.coinValue -= talent.PurchasePrice;

                    if (fromCounter)
                        Map.PutItemOnCounter(this, gold);
                    else CurrentCell.Add(gold);
                }
            }

            chr.talentsDictionary.Add(talent.Command, DateTime.UtcNow - talent.DownTime);
            chr.WriteToDisplay(GetNameForActionResult() + " teaches you how to " + talent.Description[0].ToString().ToLower() + talent.Description.Substring(1, talent.Description.Length - 2) + ".");
            DAL.DBPlayer.InsertPlayerTalent(chr.UniqueID, talent.Command);
            ProtocolYuusha.SendCharacterTalents(chr as PC, chr);
        }

        public void MerchantMender(Character chr, string request, string args)
        {
            if (FlaggedUniqueIDs.Contains(chr.UniqueID))
            {
                chr.WriteToDisplay(this.Name + ": I am quite busy. You will have to come back later.");
                return;
            }

            switch (request)
            {
                case "diag":
                case "diagnose":

                    // Check for coins. Ground, altar or counter. Diagnosis comes with a price.

                    // Nods. Come closer so I may touch you.
                    // Closes eyes.
                    // You are free from any afflictions. Consider it a gift in this age.

                    if (chr.EffectsList.ContainsKey(Effect.EffectTypes.Contagion))
                    {
                        chr.WriteToDisplay(this.GetNameForActionResult() + " lowers their head and nods.");
                        chr.WriteToDisplay(this.GetNameForActionResult() +
                            ": You are infected. I've seen this affliction before, and we both know an evil sorcerer is to blame. I will heal you for a donation of half a million.");
                    }

                    // Check GameAfflictions enum here.

                    break;
                case "heal":
                    break;
                default:
                    chr.WriteToDisplay(this.GetNameForActionResult() + ": This is something I am unable to assist you with.");
                    break;
            }
        }

        public override string GetLogString()
        {
            try
            {
                return "(MERCHANT) [SpawnZone: " + (this as NPC).SpawnZoneID + " | ID: " + (this as NPC).npcID + " | WorldNPCID: " + this.UniqueID + "] " +
                                this.Name + " [" + Utils.FormatEnumString(this.Alignment.ToString()) + " " + Utils.FormatEnumString(this.BaseProfession.ToString()) +
                                "(" + this.Level + ")] (" + (this.CurrentCell != null ? this.CurrentCell.GetLogString(false) : "Current Cell = null") + ")";
            }
            catch (Exception e)
            {
                Utils.LogException(e);

                return "(MERCHANT) [Exception in GetLogString()]";
            }
        }
    }
}