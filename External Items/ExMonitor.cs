using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PureCM.Client;
using PureCM.Server;

using PureCM_Items;

namespace External_Items
{
    /// <summary>
    /// A class for monitoring the system to check for changes and synchronize with PureCM.
    /// </summary>
    internal abstract class ExMonitor
    {
        internal ExMonitor(ExFactory oFactory, bool bForceSync)
        {
            m_oFactory = oFactory;
            m_bForceSync = bForceSync;
        }

        /// <summary>
        /// Check for any updates to tasks since the last check and synchronize the changes
        /// with PureCM
        /// </summary>
        internal void CheckForUpdates()
        {
            List<ExProject> aoProjects = m_oFactory.GetProjects();

            foreach (ExProject oExProject in aoProjects)
            {
                if ( oExProject.Include )
                {
                    UpdateVersions(oExProject);

                    System.DateTime oNow = System.DateTime.Now;

                    UpdateTasks(oExProject);

                    if (m_bForceSync)
                    {
                        oExProject.LastSyncTime = new DateTime(0);
                    }
                    else
                    {
                        oExProject.LastSyncTime = oNow;
                    }
                }
                else
                {
                    m_oFactory.Plugin.Trace(m_oFactory.PluginName + " project '" + oExProject.Name + "' will not be synchronized with PureCM. It is marked as exlcuded.");
                }
            }

            if (m_bForceSync)
            {
                m_oFactory.Plugin.Trace("Performing a forced synchronization. This should only be performed if there is a problem.");
                m_bForceSync = false;
                CheckForUpdates();
            }
        }

        private void UpdateTasks(ExProject oExProject)
        {
            m_oFactory.Plugin.Trace("Checking for " + m_oFactory.PluginName + " project '" + oExProject.Name + "' updates");

            foreach (ExTask oExTask in oExProject.RecentTasks)
            {
                if (oExTask != null)
                {
                    try
                    {
                        oExTask.Synchronize();
                    }
                    catch (Exception e)
                    {
                        m_oFactory.Plugin.LogError("Handled exception when processing " + m_oFactory.PluginName + " task '" + oExTask.Name + "' (" + e.Message + ").");
                    }
                }
            }

            m_oFactory.Plugin.Trace("Finished checking for " + m_oFactory.PluginName + " project '" + oExProject.Name + "' updates");
        }

        private void UpdateVersions(ExProject oExProject)
        {
            m_oFactory.Plugin.Trace("Checking for " + m_oFactory.PluginName + " project '" + oExProject.Name + "' created versions");

            List<ExVersion> aoVersions = m_oFactory.GetVersions(oExProject);

            foreach (ExVersion oVersion in aoVersions)
            {
                if (oVersion.Include)
                {
                    oVersion.GetSyncVersion(true);
                }
            }
        }

        private ExFactory m_oFactory;
        private bool m_bForceSync;
    }
}
