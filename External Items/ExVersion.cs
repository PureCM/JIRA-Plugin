using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PureCM.Server;

namespace External_Items
{
    /// <summary>
    /// Abstract class for a version
    /// </summary>
    internal abstract class ExVersion : ExProjectItem
    {
        internal ExVersion(ExProject oExProject, ExFactory.TPluginType tType, UInt32 nExID)
            : base(oExProject, tType, nExID)
        {
        }

        /// <summary>
        /// Get the synchronized version. If the external version does not already exist then this will
        /// optionally create a new one.
        /// </summary>
        internal ExVersion GetSyncVersion(bool bCreateIfNotExist)
        {
            if (m_oSyncVersion == null)
            {
                m_oSyncVersion = Factory.SyncFactory.GetVersion(this);

                if ((m_oSyncVersion == null) && bCreateIfNotExist)
                {
                    m_oSyncVersion = Factory.SyncFactory.CreateVersion(this);

                    if (m_oSyncVersion == null)
                    {
                        Factory.Plugin.LogInfo("Failed to create " + Factory.SyncFactory.PluginName + " version for " + Factory.PluginName + " version '" + Name + "'.");
                    }
                }
            }

            return m_oSyncVersion;
        }

        private ExVersion m_oSyncVersion;
    }
}
