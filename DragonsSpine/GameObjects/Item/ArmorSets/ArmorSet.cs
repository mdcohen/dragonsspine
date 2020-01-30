using System;
using System.Collections.Generic;

namespace DragonsSpine
{
    public class ArmorSet
    {
        public static Dictionary<string, ArmorSet> ArmorSetDictionary = new Dictionary<string, ArmorSet>();

        public const string BASIC_BANDED_MAIL = "Basic Banded Mail";
        public const string BASIC_CHAINMAIL = "Basic Chainmail";
        public const string BASIC_LEATHER = "Basic Leather";
        public const string BASIC_SCALEMAIL = "Basic Scalemail";
        public const string BASIC_STUDDED_LEATHER = "Basic Studded Leather";
        public const string BASIC_STEEL = "Basic Steel";

        public const string FULL_BANDED_MAIL = "Full Banded Mail";
        public const string FULL_CHAINMAIL = "Full Chainmail";
        public const string FULL_LEATHER = "Full Leather";
        public const string FULL_SCALEMAIL = "Full Scalemail";
        public const string FULL_STUDDED_LEATHER = "Full Studded Leather";
        public const string FULL_STEEL = "Full Steel";

        #region Private Data
        private string m_name;
        private int m_torsoID;
        private int m_legsID;
        private int m_headID;
        private int m_handsID;
        private int m_wristID;
        private int m_bicepID;

        private IArmorSetBenefits m_armorSetBenefits;
        #endregion

        #region Public Properties
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public int TorsoID
        {
            get { return m_torsoID; }
            set { m_torsoID = value; }
        }

        public int LegsID
        {
            get { return m_legsID; }
            set { m_legsID = value; }
        }

        public int HeadID
        {
            get { return m_headID; }
            set { m_headID = value; }
        }

        public int HandsID
        {
            get { return m_handsID; }
            set { m_handsID = value; }
        }

        public int WristID
        {
            get { return m_wristID; }
            set { m_wristID = value; }
        }

        public int BicepID
        {
            get { return m_bicepID; }
            set { m_bicepID = value; }
        }

        public IArmorSetBenefits ArmorSetBenefits
        {
            get { return m_armorSetBenefits; }
            set { m_armorSetBenefits = value; }
        }
        #endregion

        public ArmorSet(string name, int torsoID, int legsID, int headID, int handsID, int wristID, int bicepID, IArmorSetBenefits benefits)
        {
            m_name = name;
            m_torsoID = torsoID;
            m_legsID = legsID;
            m_headID = headID;
            m_handsID = handsID;
            m_wristID = wristID;
            m_bicepID = bicepID;

            m_armorSetBenefits = benefits;
        }

        public bool MatchSimilarArmorComponent(Globals.eWearLocation wearLocation, int itemID, Type t)
        {
            switch (wearLocation)
            {
                case Globals.eWearLocation.Hands:
                    if (t == typeof(BasicLeatherArmorSet) || t == typeof(FullLeatherArmorSet))
                    {
                        if (itemID == Item.ID_LEATHER_GAUNTLETS || itemID == Item.ID_LEATHER_GAUNTLETS_PLUS_ONE || itemID == Item.ID_LEATHER_GAUNTLETS_PLUS_TWO)
                            return true;
                    }
                    break;
                case Globals.eWearLocation.Bicep:
                    if (t == typeof(FullSteelArmorSet))
                    {
                        if (itemID == Item.ID_STEEL_ARMBAND_PLUS_ONE || itemID == Item.ID_STEEL_ARMBAND_PLUS_THREE || itemID == Item.ID_STEEL_ARMBAND_PLUS_SIX)
                            return true;
                    }
                    break;
            }

            return false;
        }

        public List<int> GetArmorList()
        {
            var armor = new List<int>();

            if (TorsoID > 0) armor.Add(TorsoID);
            if (LegsID > 0) armor.Add(LegsID);
            if (HeadID > 0) armor.Add(HeadID);
            if (HandsID > 0) armor.Add(HandsID);
            if (WristID > 0) armor.Add(WristID);
            if (BicepID > 0) armor.Add(BicepID);

            return armor;
        }

        public static bool LoadArmorSets()
        {
            try
            {
                foreach (var t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (Array.IndexOf(t.GetInterfaces(), typeof(IArmorSetBenefits)) > -1)
                    {
                        var a = (ArmorSetAttribute)t.GetCustomAttributes(typeof(ArmorSetAttribute), true)[0];                        
                        var armorSet = new ArmorSet(a.Name, a.TorsoID, a.LegsID, a.HeadID, a.GauntletsID, a.WristID, a.BicepID, (IArmorSetBenefits)Activator.CreateInstance(t));

                        // Add the GameCommand to a dictionary.
                        if (!ArmorSetDictionary.ContainsKey(armorSet.Name))
                        {
                            ArmorSetDictionary.Add(armorSet.Name, armorSet);
                        }
                        else
                        {
                            Utils.Log("ArmorSet already exists: " + armorSet.Name, Utils.LogType.SystemWarning);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return false;
            }
            return true;
        }
    }
}
