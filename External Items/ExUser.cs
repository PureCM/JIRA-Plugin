using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PureCM.Server;
namespace External_Items
{
    /// <summary>
    /// Abstract class for a user
    /// </summary>
    internal abstract class ExUser : ExItem
    {
        internal ExUser(ExFactory oFactory, ExFactory.TPluginType tType, UInt32 nExID)
            : base(oFactory, tType, nExID)
        {
        }

        /// <summary>   
        /// Get the synchronized user. If the user does not already exist then this will
        /// optionally create a new one.
        /// </summary>
        internal ExUser GetSyncUser(bool bCreateIfNotExist)
        {
            if (m_oSyncUser == null)
            {
                m_oSyncUser = Factory.SyncFactory.GetUser(this);

                if ((m_oSyncUser == null) && bCreateIfNotExist)
                {
                    m_oSyncUser = Factory.SyncFactory.CreateUser(this);

                    if (m_oSyncUser != null && m_oSyncUser.ID == UInt32.MaxValue)
                    {
                        m_oSyncUser = null;
                    }

                    if (m_oSyncUser == null)
                    {
                        Factory.Plugin.LogWarning("Failed to create " + Factory.SyncFactory.PluginName + " user for " + Factory.PluginName + " user '" + Name + "'.");
                    }
                }
            }

            return m_oSyncUser;
        }

        /// <summary>
        /// What is the email address for this user?
        /// </summary>
        internal String EmailAddress { get { return GetEmailAddress(); } }

        private ExUser m_oSyncUser;

        /// <summary>
        /// What is the email address for this user?
        /// </summary>
        internal abstract String GetEmailAddress();
    }
}
