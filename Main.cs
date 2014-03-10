using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Xml.Linq;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using System.Diagnostics;

using PureCM_Items;
using Jira_Items;

using PureCM.Server;
using PureCM.Client;

namespace Plugin_Jira
{
    [EventHandlerDescription("Plugin that integrates with the Jira Project Management System")]
    public class JiraPlugin : PureCM.Server.Plugin
    {
        public override bool OnStart(XElement oXMLConfig, Connection oConnection)
        {
            LogInfo("Starting Plugin_Jira");

            m_oOptions = new JOptions(oXMLConfig);

            // This is for debugging so we can attach to the process
            if (m_oOptions.DelayOnStart)
            {
                System.Threading.Thread.Sleep(60000);
                Debugger.Break();
            }

            m_bForceJiraSync = m_oOptions.ForceJiraSync;
            m_bForcePureCMSync = m_oOptions.ForcePureCMSync;

            // Check that we can connect to Jira and that it has at least one project
            JiraSoapServiceService oServiceManager = new JiraSoapServiceService();

            oServiceManager.Url = String.Format("{0}/rpc/soap/jirasoapservice-v2", m_oOptions.JURL);

            string strRPCToken;

            try
            {
                strRPCToken = oServiceManager.login(m_oOptions.JUser, m_oOptions.JPassword);
            }
            catch (Exception e)
            {
                LogError("Failed to connect to Jira server (" + e.Message + ").");
                return false;
            }

            RemoteProject[] aoJProjects = oServiceManager.getProjectsNoSchemes(strRPCToken);

            if (aoJProjects.Length <= 0)
            {
                LogWarning("Unable to integrate with Jira. Jira does not contain any projects.");
                return false;
            }

            m_oRepos = oConnection.Repositories.ByName(m_oOptions.PRepository);

            if (m_oRepos == null)
            {
                LogWarning("Unable to integrate with Jira. PureCM repository '" + m_oOptions.PRepository + "' does not exist.");
                return false;
            }

            oConnection.OnIdle = OnIdle;
            oConnection.OnStreamCreated = OnStreamCreated;

            LogInfo("Finished Plugin_Jira OnStart");

            return true;
        }

        public override void OnStop()
        {
        }

        private void OnStreamCreated(StreamCreatedEvent evt)
        {
            if (evt.Repository != null)
            {
                evt.Repository.RefreshStreams();
            }
        }

        private void OnIdle()
        {
            if (!m_oStopwatch.IsRunning || (m_oStopwatch.ElapsedMilliseconds > m_oOptions.Interval * 1000))
            {
                Synchronize();
            }
        }

        private void Synchronize()
        {
            try
            {
                JiraSoapServiceService oServiceManager = new JiraSoapServiceService();

                oServiceManager.Url = String.Format("{0}/rpc/soap/jirasoapservice-v2", m_oOptions.JURL);

                JFactory oJFactory = new JFactory(this, oServiceManager, m_oRepos, m_oOptions);
                PCMFactory oPCMFactory = new PCMFactory(this, External_Items.ExFactory.TPluginType.JiraProject, External_Items.ExFactory.TPluginSyncType.PureCMJiraSyncTimeYear, m_oRepos, m_oOptions.JTaskCreate, false);

                oJFactory.SyncFactory = oPCMFactory;
                oPCMFactory.SyncFactory = oJFactory;

                JMonitor oJMonitor = new JMonitor(oJFactory, m_bForceJiraSync);

                oJMonitor.CheckForUpdates();

                PCMMonitor oPMonitor = new PCMMonitor(oPCMFactory, m_bForcePureCMSync);

                oPMonitor.CheckForUpdates();

                m_bForceJiraSync = false;
                m_bForcePureCMSync = false;

                m_oStopwatch.Reset();
                m_oStopwatch.Start();

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception e)
            {
                LogError("Unhandled exception (" + e.Message + ").");
            }
        }

        private Repository m_oRepos;
        private JOptions m_oOptions;
        private bool m_bForceJiraSync = false;
        private bool m_bForcePureCMSync = false;

        private Stopwatch m_oStopwatch = new Stopwatch();
    }
}
