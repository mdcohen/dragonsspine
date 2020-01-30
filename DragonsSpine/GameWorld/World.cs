using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Entity = DragonsSpine.Autonomy.EntityBuilding.EntityLists.Entity;

namespace DragonsSpine.GameWorld
{
    public static class World
    {
        #region Enums
        public enum DailyCycle
        {
            Morning,
            Afternoon,
            Evening,
            Night
        }
        public enum LunarCycle
        {
            New,
            Waxing_Crescent,
            Full,
            Waning_Crescent
        } 
        #endregion

        #region Constants
        public const short MAP_HELL = 12;
        public const short LAND_BG = 0;
        public const short LAND_AG = 1;
        public const short LAND_UW = 2;
        public const double FEE_ORNIC_FLAME_USE = .03;
        public const double FEE_JANITORIAL_COIN_REMOVAL = .5;
        public const double FEE_JANITORIAL_ITEM_REMOVAL = .25;
        public const double FEE_ATM_USE_COINS = .02;
        public const double FEE_ATM_USE_ITEMS = .05;
        #endregion

        #region Private Static Data
        private static int m_worldNpcID = Int32.MinValue; //-2147483641;
        private static int m_worldItemID = Int32.MinValue; //-2147483641;
        private static Dictionary<int, Facet> m_facetDict = new Dictionary<int, Facet>();
        private static string m_currentDay = "Nanna";
        private static DailyCycle m_currentDailyCycle = DailyCycle.Morning;
        private static LunarCycle m_currentLunarCycle = LunarCycle.New;
        private static ArrayList m_bannedIPList = new ArrayList();
        private static int m_itemDecayTimer = Utils.TimeSpanToRounds(new TimeSpan(0, 30, 0)); // 30 minutes for items
        private static int m_playerCorpseDecayTimer = Utils.TimeSpanToRounds(new TimeSpan(0, 30, 0)); // 30 minutes for a player corpses
        private static int m_npcCorpseDecayTimer = Utils.TimeSpanToRounds(new TimeSpan(0, 4, 0)); // 4 minutes for npc corpses
        private static int m_attunedOrArtifactItemDecayTimer = Utils.TimeSpanToRounds(new TimeSpan(6, 0, 0)); // 6 hours for attuned items
        private static List<int> m_lotteryParticipants = new List<int>();
        #endregion

        // professions that must be evil
        public static Character.ClassType[] EvilProfessions = new Character.ClassType[] { Character.ClassType.Ravager, Character.ClassType.Sorcerer };
        // professions that have absolutely no inherit spell use
        public static Character.ClassType[] PureMeleeProfessions = new Character.ClassType[] { Character.ClassType.Berserker, Character.ClassType.Fighter, Character.ClassType.Martial_Artist };
        // professions that are a mix between melee and spell users, used in AI combat as most hybrids do not have attack type spells
        public static Character.ClassType[] HybridProfessions = new Character.ClassType[] { Character.ClassType.Knight, Character.ClassType.Ravager };
        // professions that use any sort of spells
        public static Character.ClassType[] SpellUsingProfessions = new Character.ClassType[] { Character.ClassType.Thaumaturge, Character.ClassType.Wizard,
            Character.ClassType.Thief, Character.ClassType.Knight, Character.ClassType.Ravager, Character.ClassType.Sorcerer, Character.ClassType.Druid, Character.ClassType.Ranger };
        // professions that typically must warm a spell before casting it (unless CastMode attribute dictates otherwise)
        public static Character.ClassType[] SpellWarmingProfessions = new Character.ClassType[] { Character.ClassType.Thaumaturge, Character.ClassType.Wizard,
            Character.ClassType.Thief, Character.ClassType.Sorcerer, Character.ClassType.Druid, Character.ClassType.Ranger };
        // professions using intelligence as a factor when casting spells
        public static Character.ClassType[] IntelligenceCasters = new Character.ClassType[] { Character.ClassType.Ranger, Character.ClassType.Wizard, Character.ClassType.Thief, Character.ClassType.Sorcerer };
        // professions using wisdom as a factor when casting spells
        public static Character.ClassType[] WisdomCasters = new Character.ClassType[] { Character.ClassType.Druid, Character.ClassType.Thaumaturge, Character.ClassType.Knight, Character.ClassType.Ravager };
        public static Character.ClassType[] ThieverySkillProfessions = new Character.ClassType[] { Character.ClassType.Thief, Character.ClassType.Martial_Artist };
        public static Character.ClassType[] MagicSkillProfessions = new Character.ClassType[] { Character.ClassType.Sorcerer, Character.ClassType.Thaumaturge, Character.ClassType.Thief,
            Character.ClassType.Wizard, Character.ClassType.Druid, Character.ClassType.Ranger };
        public static Character.ClassType[] BashSkillProfessions = new Character.ClassType[] { Character.ClassType.Berserker, Character.ClassType.Fighter, Character.ClassType.Knight, Character.ClassType.Ranger, Character.ClassType.Ravager,
            Character.ClassType.Thief};
        // professions able to specialize with a weapon skill
        public static Character.ClassType[] WeaponSpecializationProfessions = new Character.ClassType[] { Character.ClassType.Berserker, Character.ClassType.Fighter, Character.ClassType.Ranger };

        public static int[] DoctoredHPLimits =
        {
            0, // None = 0
            425, // Fighter
            350, // Thaumaturge,
            300, // Wizard,
            375, // Martial_Artist
            325, // Thief = 5
            400, // Knight
            405, // Ravager
            315, // Sorcerer
            340, // Druid
            360, // Ranger = 10
            450, // Berserker
            0 // All
        }; // corresponds to Character.ClassType enumeration

        public static int[] DoctoredStaminaLimits =
        {
            0, // None = 0
            225, // Fighter
            150, // Thaumaturge,
            100, // Wizard,
            175, // Martial_Artist
            125, // Thief = 5
            200, // Knight
            205, // Ravager
            115, // Sorcerer
            140, // Druid
            160, // Ranger = 10
            275, // Berserker
            0 // All
        }; // corresponds to Character.ClassType enumeration

        public static int[] HitDice =
        {
            0, // None = 0
            16, // Fighter
            10, // Thaumaturge,
            8, // Wizard,
            14, // Martial_Artist
            12, // Thief = 5
            14, // Knight
            14, // Ravager
            8, // Sorcerer
            10, // Druid
            12, // Ranger = 10
            14, // Berserker
            0 // All
        }; // corresponds to Character.ClassType enumeration

        public static int[] ManaDice =
        {
            0, // None = 0
            0, // Fighter
            8, // Thaumaturge,
            10, // Wizard,
            0, // Martial_Artist
            6, // Thief = 5
            0, // Knight
            0, // Ravager
            10, // Sorcerer
            8, // Druid
            6, // Ranger = 10
            0, // Berserker
            0 // All
        }; // corresponds to Character.ClassType enumeration

        public static int[] StaminaDice =
        {
            0, // None = 0
            8, // Fighter
            6, // Thaumaturge,
            6, // Wizard,
            10, // Martial_Artist
            6, // Thief = 5
            6, // Knight
            6, // Ravager
            6, // Sorcerer
            6, // Druid
            6, // Ranger = 10
            12, // Berserker
            0 // All
        }; // corresponds to Character.ClassType enumeration

        public static string[] DaysOfTheWeek = { "Nanna", "Enki", "Inanna", "Utu", "Gugalanna", "Enlil", "Ninurta" }; // days of the week

        public static string[] Seasons = { "Emesh", "Enten" };

        // DragonsSpineMain

        public static List<int> AgeCycles = new List<int>()
        {
            (int)DragonsSpineMain.MasterRoundInterval / 1000 * (5 * 14400 / ((int)DragonsSpineMain.MasterRoundInterval / 1000)),
            (int)DragonsSpineMain.MasterRoundInterval / 1000 * (5 * 28800 / ((int)DragonsSpineMain.MasterRoundInterval / 1000)),
            (int)DragonsSpineMain.MasterRoundInterval / 1000 * (5 * 43200 / ((int)DragonsSpineMain.MasterRoundInterval / 1000)),
            (int)DragonsSpineMain.MasterRoundInterval / 1000 * (5 * 57600 / ((int)DragonsSpineMain.MasterRoundInterval / 1000)),
            (int)DragonsSpineMain.MasterRoundInterval / 1000 * (5 * 72000 / ((int)DragonsSpineMain.MasterRoundInterval / 1000)),
            (int)DragonsSpineMain.MasterRoundInterval / 1000 * (5 * 80000 / ((int)DragonsSpineMain.MasterRoundInterval / 1000)),

            // hard coded when the round interval was 5 seconds
            //14400,
            //28800,
            //43200,
            //57600,
            //72000,
            //80000
        };        

        public static string[] age_humanoid = new string[] { "a very young", "a young", "a middle-aged", "an old", "a very old", "an ancient" };
        public static string[] age_wyrmKin = new string[] { "a hatchling", "a young", "an adolescent", "a mature", "an aged", "an ancient" };
        public static string[] homelands = new string[] { "Illyria", "Mu", "Lemuria", "Leng", "Draznia", "Hovath", "Mnar", "the plains" };

        #region Properties
        public static int ItemDecayTimer
        {
            get { return m_itemDecayTimer; }
        }
        public static int PlayerCorpseDecayTimer
        {
            get { return m_playerCorpseDecayTimer; }
        }
        public static int NPCCorpseDecayTimer
        {
            get { return m_npcCorpseDecayTimer; }
        }
        public static int AttunedOrArtifactItemDecayTimer
        {
            get { return m_attunedOrArtifactItemDecayTimer; }
        }
        public static ArrayList BannedIPList
        {
            get { return m_bannedIPList; }
            set { m_bannedIPList = value; }
        }

        public static Dictionary<int, Facet>.ValueCollection Facets
        {
            get { return World.m_facetDict.Values; }
        }

        public static DailyCycle CurrentDailyCycle
        {
            get { return m_currentDailyCycle; }
            set { m_currentDailyCycle = value; }
        }

        public static LunarCycle CurrentLunarCycle
        {
            get { return m_currentLunarCycle; }
            set { m_currentLunarCycle = value; }
        }

        public static List<int> LotteryParticipants
        {
            get { return m_lotteryParticipants; }
        }
        #endregion

        public static bool LoadWorld()
        {
            if (!World.LoadFacets())
            {
                return false;
            }
            else
            {
                foreach (Facet facet in World.Facets)
                {
                    if (!facet.LoadLands())
                    {
                        return false;
                    }
                    else
                    {
                        foreach (Land land in facet.Lands)
                        {
                            if (!land.FillLand())
                            {
                                Utils.Log("Fatal error while filling Facet: " + facet.Name + " Land: " + land.Name + " with maps.", Utils.LogType.SystemFatalError);
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private static bool LoadFacets()
        {
            return DAL.DBWorld.LoadFacets();
        }

        public static bool LoadBannedIPList() // helper function for filling banned IP list
        {
            try
            {
                BannedIPList = DAL.DBWorld.loadBannedIPList();
                return true;
            }
            catch (Exception e)
            {
                Utils.Log("World.loadBannedIPList() " + e.Message + "  Stack: " + e.StackTrace, Utils.LogType.Exception);
                return false;
            }
        }

        public static void Add(Facet facet)
        {
            World.m_facetDict.Add(facet.FacetID, facet);
        }

        public static int GetNumberPlayersInLand(short landID)
        {
            int number = 0;
            foreach (PC ch in new List<PC>(Character.PCInGameWorld))
            {
                if (!ch.IsPC) continue;
                if (ch.LandID == landID)
                {
                    number++;
                }
            }
            return number;
        }

        public static int GetNumberPlayersInMap(int mapID)
        {
            int number = 0;
            foreach (PC ch in new List<PC>(Character.PCInGameWorld))
            {
                if (!ch.IsPC) continue;
                if (ch.MapID == mapID)
                {
                    number++;
                }
            }
            return number;
        }

        public static Facet GetFacetByIndex(int index)
        {
            int a = 0;
            foreach (Facet facet in World.Facets)
            {
                if (a == index)
                {
                    return facet;
                }
                a++;
            }
            return null;
        }

        public static Facet GetFacetByID(int facetID)
        {
            if (World.m_facetDict.ContainsKey(facetID))
            {
                return World.m_facetDict[facetID];
            }
            return null;
        }

        public static int GetNextNPCUniqueID()
        {
            return World.m_worldNpcID++;
        }

        public static int GetNextWorldItemID()
        {
            return m_worldItemID++;
        }

        public static string CurrentDay //mlt added for look around command tweak
        {
            get { return m_currentDay; }
            set { m_currentDay = value; }
        }

        public static void ShiftDay()
        {
            for (int a = 0; a < DaysOfTheWeek.Length; a++)
            {
                if (DaysOfTheWeek[a] == m_currentDay)
                {
                    if (a + 1 >= DaysOfTheWeek.Length - 1)
                    {
                        m_currentDay = DaysOfTheWeek[0];
                    }
                    else
                    {
                        m_currentDay = DaysOfTheWeek[a + 1];
                    }
                    break;
                }
            }
  
            // resupply the berry bushes
            foreach (Facet facet in World.Facets)
            {
                foreach (Land land in facet.Lands)
                {
                    foreach (Map map in land.Maps)
                    {
                        if (map != null)
                        {
                            foreach (Cell cell in map.cells.Values)
                            {
                                if (cell != null)
                                {
                                    if (cell.balmBerry || cell.manaBerry || cell.poisonBerry || cell.stamBerry)
                                    {
                                        cell.droppedFruit = 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void ShiftDailyCycle(Object myObject, System.Timers.ElapsedEventArgs myEventArgs)
        {
            if (m_currentDailyCycle == (DailyCycle)Enum.GetValues(typeof(DailyCycle)).GetValue(Enum.GetValues(typeof(DailyCycle)).Length - 1))
            {
                m_currentDailyCycle = (DailyCycle)Enum.GetValues(typeof(DailyCycle)).GetValue(0);
                ShiftDay();
            }
            else
            {
                m_currentDailyCycle = (DailyCycle)World.m_currentDailyCycle + 1;
            }
        }

        public static void ShiftLunarCycle(Object myObject, System.Timers.ElapsedEventArgs myEventArgs)
        {
            if (m_currentLunarCycle == (LunarCycle)Enum.GetValues(typeof(LunarCycle)).GetValue(Enum.GetValues(typeof(LunarCycle)).Length - 1))
            {
                m_currentLunarCycle = (LunarCycle)Enum.GetValues(typeof(LunarCycle)).GetValue(0);
                // nothing currently happens when the cycle starts over, should there be an event?
            }
            else
            {
                m_currentLunarCycle = (LunarCycle)World.m_currentLunarCycle + 1;
            }
        }

        //public static bool IsMagicWithinRange(NPC npc)
        //{
        //    if (npc.gotoWarmedMagic != "")
        //    {
        //        string[] flmxyz = npc.gotoWarmedMagic.Split("|".ToCharArray());
        //        int x = Convert.ToInt32(flmxyz[0]);
        //        int y = Convert.ToInt32(flmxyz[1]);
        //        int z = Convert.ToInt32(flmxyz[2]);

        //        if(npc.X != x && npc.Y != y && npc.Z != z)
        //            return true;
        //    }

        //    foreach (string cord in new List<string>(World.magicCordLastRound))
        //    {
        //        string [] flmxyz = cord.Split("|".ToCharArray());
        //        int rf = Convert.ToInt32(flmxyz[0]);
        //        int rl = Convert.ToInt32(flmxyz[1]);
        //        int rm = Convert.ToInt32(flmxyz[2]);
        //        int rx = Convert.ToInt32(flmxyz[3]);
        //        int ry = Convert.ToInt32(flmxyz[4]);
        //        int rz = Convert.ToInt32(flmxyz[5]);

        //        if ((npc.lairCritter || Rules.CheckPerception(npc)) && rf == npc.FacetID && rl == npc.LandID && rm == npc.MapID && rz == npc.Z
        //            && Cell.GetCellDistance(rx, ry, npc.X, npc.Y) <= 8)
        //        {
        //            npc.gotoWarmedMagic = rx + "|" + ry + "|" + rz;
        //            return true;
        //        }
        //    }

        //    npc.gotoWarmedMagic = "";

        //    return false;
        //}

        public static void CollectFeeForLottery(double feePercentage, int landID, ref double coinValue)
        {
            if (!DragonsSpineMain.Instance.Settings.LotteryEnabled)
                return;

            long collection = 0;

            if (feePercentage >= 1) collection = (long)coinValue;
            else collection = (long)(feePercentage * coinValue);
            
            if (collection < 1) collection = 1;

            coinValue -= collection;

            World.GetFacetByID(0).GetLandByID(landID).Lottery += collection;

            DAL.DBWorld.SaveLottery(World.GetFacetByID(0).GetLandByID(landID));
        }
    }
}
