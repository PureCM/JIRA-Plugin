using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace External_Items
{
    /// <summary>
    /// This is the base class for all wrapper objects
    /// </summary>
    internal abstract class ExItem
    {
        internal ExItem(ExFactory oFactory, ExFactory.TPluginType tType, UInt32 nExID)
        {
            m_oFactory = oFactory;
            m_tType = tType;
            m_nExID = nExID;
        }

        /// <summary>
        /// The factory for items
        /// </summary>
        internal ExFactory Factory
        {
            get { return m_oFactory; }
            set { m_oFactory = value; }
        }

        /// <summary>
        /// What is the external ID?
        /// </summary>
        internal UInt32 ID
        {
            get { return m_nExID; }

            set 
            {
                UInt32 nOldSyncID = SyncID;
                m_nExID = value;
                SyncID = nOldSyncID;
            }
        }

        /// <summary>
        /// What is the synchronized ID?
        /// </summary>
        internal UInt32 SyncID
        {
            get
            {
                if (m_nSyncID == 0)
                {
                    m_nSyncID = m_oFactory.GetSyncID(m_tType, m_nExID);
                }

                return m_nSyncID;
            }

            set
            {
                m_oFactory.SetSyncID(m_tType, m_nExID, value);
                m_nSyncID = value;
            }
        }

        /// <summary>
        /// Should this item be synchronized?
        /// </summary>
        internal bool Include{ get{ return ShouldInclude(); } }

        /// <summary>
        /// What is the short name for this item?
        /// </summary>
        internal String Name { get { return GetName(); } }

        /// <summary>
        /// What is the long description for this item?
        /// </summary>
        internal String Description { get { return GetDescription(); } }

        private ExFactory m_oFactory;
        private ExFactory.TPluginType m_tType;
        private UInt32 m_nExID;
        private UInt32 m_nSyncID = 0;

        /// <summary>
        /// Should this item be synchronized with the other system?
        /// </summary>
        internal virtual bool ShouldInclude()
        {
            return true;
        }

        /// <summary>
        /// What is the short name for this item?
        /// </summary>
        internal abstract String GetName();

        /// <summary>
        /// What is the long description for this item?
        /// </summary>
        internal abstract String GetDescription();
    }
}
