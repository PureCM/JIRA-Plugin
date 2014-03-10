using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PureCM.Server;

namespace External_Items
{
    /// <summary>
    /// Abstract class for a project
    /// </summary>
    internal abstract class ExProject : ExItem
    {
        internal ExProject(ExFactory oFactory, ExFactory.TPluginType tType, UInt32 nExID)
            : base(oFactory, tType, nExID)
        {
        }

        /// <summary>
        /// Get the project which it is synchronized with. If the sync project does not already exist then this will
        /// optionally create a new project.
        /// </summary>
        internal ExProject GetSyncProject(bool bCreateIfNotExist)
        {
            if ( m_oSyncProject == null )
            {
                m_oSyncProject = Factory.SyncFactory.GetProject(this);

                if ((m_oSyncProject == null) && bCreateIfNotExist)
                {
                    m_oSyncProject = Factory.SyncFactory.CreateProject(this);

                    if (m_oSyncProject == null)
                    {
                        Factory.Plugin.LogInfo("Failed to create " + Factory.SyncFactory.PluginName + " project for " + Factory.PluginName + " project '" + Name + "'.");
                    }
                }
            }

            return m_oSyncProject;
        }

        /// <summary>
        /// What tasks have been changed since we last synchronized?
        /// </summary>
        internal List<ExTask> RecentTasks { get { return GetRecentTasks(); } }

        /// <summary>
        /// The last time the tasks were synchronized
        /// </summary>
        internal DateTime LastSyncTime { get { return GetLastSyncTime(); } set { SetLastSyncTime(value); } }

        private ExProject m_oSyncProject;

        /// <summary>
        /// What tasks have been changed since we last synchronized?
        /// </summary>
        internal abstract List<ExTask> GetRecentTasks();

        /// <summary>
        /// When were the tasks last synchronized?
        /// </summary>
        internal abstract DateTime GetLastSyncTime();

        /// <summary>
        /// Set when the tasks last synchronized
        /// </summary>
        internal abstract void SetLastSyncTime(DateTime oSyncTime);
    }
}
