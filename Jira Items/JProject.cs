using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PureCM.Client;
using PureCM.Server;

using External_Items;

namespace Jira_Items
{
    /// <summary>
    /// A wrapper class for a Jira project.
    /// </summary>
    internal class JProject : ExProject
    {
        internal JProject(JFactory oFactory, RemoteProject oJProject)
            : base(oFactory, ExFactory.TPluginType.JiraProject, Convert.ToUInt32(oJProject.id))
        {
            m_oJProject = oJProject;
        }

        private JFactory JFactory { get { return Factory as JFactory; } }

        private RemoteProject m_oJProject;

        /// <summary>
        /// The Jira project item.
        /// </summary>
        public RemoteProject Project
        {
            get { return m_oJProject; }
        }

        /// <summary>
        /// Returns an array of the Jira issues which have been updated since the last synchronise.
        /// This will include issues which have been added and deleted.
        /// </summary>
        internal override List<ExTask> GetRecentTasks()
        {
            DateTime oLastSyncTime = LastSyncTime;
            string strJql;

            if (oLastSyncTime.Year > 1)
            {
                strJql = string.Format("project = '{0}' AND updated > '{1}/{2}/{3} {4}:00'", Project.id, oLastSyncTime.Year, oLastSyncTime.Month, oLastSyncTime.Day, oLastSyncTime.Hour, oLastSyncTime.Minute);
            }
            else
            {
                strJql = string.Format("project = '{0}'", Project.id);
            }

            RemoteIssue[] aoJIssues = JFactory.ServiceManager.getIssuesFromJqlSearch(JFactory.RPCToken, strJql, 100000);
            List<ExTask> aoIssues = new List<ExTask>();

            foreach (RemoteIssue oJIssue in aoJIssues)
            {
                // Jira only checks the revised after day, so do the compare to check for hours & minutes
                DateTime tLocalTime = oJIssue.updated.Value.ToLocalTime();

                if (tLocalTime.CompareTo(oLastSyncTime) > 0)
                {
                    aoIssues.Add(new JTask(this, oJIssue));
                }
            }
            return aoIssues;
        }

        /// <summary>
        /// When were the tasks last synchronized?
        /// </summary>
        internal override DateTime GetLastSyncTime()
        {
            JFactory oFactory = Factory as JFactory;
            UInt32 nYear = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeYear), ID);

            if (nYear > 0)
            {
                UInt32 nMonth = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeMonth), ID);
                UInt32 nDay = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeDay), ID);
                UInt32 nHour = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeHour), ID);
                UInt32 nMinute = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeMinute), ID);
                UInt32 nSecond = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeSecond), ID);

                if ((nMonth > 0) && (nDay > 0))
                {
                    return new DateTime(Convert.ToInt32(nYear), Convert.ToInt32(nMonth), Convert.ToInt32(nDay), Convert.ToInt32(nHour), Convert.ToInt32(nMinute), Convert.ToInt32(nSecond));
                }
                else
                {
                    return new DateTime();
                }
            }

            return new DateTime();
        }

        /// <summary>
        /// Set when the tasks last synchronized
        /// </summary>
        internal override void SetLastSyncTime(DateTime oSyncTime)
        {
            JFactory oFactory = Factory as JFactory;

            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeYear), ID, Convert.ToUInt32(oSyncTime.Year));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeMonth), ID, Convert.ToUInt32(oSyncTime.Month));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeDay), ID, Convert.ToUInt32(oSyncTime.Day));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeHour), ID, Convert.ToUInt32(oSyncTime.Hour));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeMinute), ID, Convert.ToUInt32(oSyncTime.Minute));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeSecond), ID, Convert.ToUInt32(oSyncTime.Second));
        }

        /// <summary>
        /// Return the Jira short name for the issue
        /// </summary>
        internal override String GetName()
        {
            return m_oJProject.name;
        }

        /// <summary>
        /// Return the Jira long description for the issue
        /// </summary>
        internal override String GetDescription()
        {
            return m_oJProject.description;
        }
    }
}
