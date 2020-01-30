using System;

namespace DragonsSpine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ArmorSetAttribute : Attribute
    {
        #region Private Data
        private string m_name;
        private int m_torsoID;
        private int m_legsID;
        private int m_headID;
        private int m_gauntletsID;
        private int m_wristID;
        private int m_bicepID;
        #endregion

        #region Public Properties
        public string Name
        {
            get { return m_name; }
        }

        public int TorsoID
        {
            get { return m_torsoID; }
        }

        public int LegsID
        {
            get { return m_legsID; }
        }

        public int HeadID
        {
            get { return m_headID; }
        }

        public int GauntletsID
        {
            get { return m_gauntletsID; }
        }

        public int WristID
        {
            get { return m_wristID; }
        }

        public int BicepID
        {
            get{return m_bicepID;}
        }
        #endregion

        public ArmorSetAttribute(string name, int torsoID, int legsID, int headID, int gauntletsID, int wristID, int bicepID)
        {
            m_name = name;
            m_torsoID = torsoID;
            m_legsID = legsID;
            m_headID = headID;
            m_gauntletsID = gauntletsID;
            m_wristID = wristID;
            m_bicepID = bicepID;
        }

        /// <summary>
        /// Basic ArmorSetAttribute constructor for only torso and legs ArmorSet.
        /// </summary>
        /// <param name="torsoID"></param>
        /// <param name="legsID"></param>
        public ArmorSetAttribute(string name, int torsoID, int legsID) : this(name, torsoID, legsID, 0, 0, 0, 0)
        {

        }
    }
}
