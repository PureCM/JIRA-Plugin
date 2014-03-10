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
    /// A Jira issue.
    /// </summary>
    internal class JTask : ExTask
    {
        internal JTask(JProject oProject, RemoteIssue oJIssue)
            : base(oProject, ExFactory.TPluginType.JiraTask, Convert.ToUInt32(oJIssue.id))
        {
            m_oJIssue = oJIssue;
        }

        private JFactory JFactory { get { return Factory as JFactory; } }

        private RemoteIssue m_oJIssue;

        /// <summary>
        /// The last time this task was synced with PureCM
        /// </summary>
        internal DateTime LastSyncTime { get { return GetLastSyncTime(); } set { SetLastSyncTime(value); } }

        /// <summary>
        /// Return the owner of the issue, or null if it has no owner. This is the equivalent of
        /// 'Assigned To' in Jira.
        /// </summary>
        internal override ExUser GetOwner()
        {
            if (m_oJIssue.assignee != null && m_oJIssue.assignee.Length > 0)
            {
                return JFactory.GetUser(m_oJIssue.assignee);
            }

            return null;
        }

        /// <summary>
        /// Set the owner of the task, or null if it has no owner.
        /// </summary>
        internal override void SetOwner(ExUser oOwner)
        {
            JUser oJOwner = oOwner as JUser;

            RemoteFieldValue oAction = new RemoteFieldValue();

            oAction.id = "assignee";
            oAction.values = new string[1];

            if (oJOwner != null)
            {
                oAction.values[0] = oOwner.Name;
            }
            else
            {
                if (oOwner != null)
                {
                    Factory.Plugin.LogWarning("Failed to update Jira task '" + Name + "' owner to be '" + oOwner.Name + "'. The owner object is the wrong type.");
                }

                oAction.values[0] = "";
            }

            RemoteFieldValue[] aoActions = { oAction };

            m_oJIssue = JFactory.ServiceManager.updateIssue(JFactory.RPCToken, m_oJIssue.key, aoActions);
        }

        /// <summary>
        /// Set the project of the task (version will be null so it will go into the backlog)
        /// </summary>
        internal override void SetProject(ExProject oProjectParam)
        {
            JProject    oProject = oProjectParam as JProject;
            string strOldKey = m_oJIssue.key;

            m_oJIssue.id = "";
            m_oJIssue.key = "";
            m_oJIssue.project = oProject.Project.key;
            m_oJIssue.affectsVersions = null;
            m_oJIssue.fixVersions = null;

            m_oJIssue = JFactory.ServiceManager.createIssue(JFactory.RPCToken, m_oJIssue);

            JFactory.ServiceManager.deleteIssue(JFactory.RPCToken, strOldKey);

            ID = UInt32.Parse(m_oJIssue.id);
        }

        /// <summary>
        /// Return the version in which the issue will be fixed, or null if it has no version. This is the equivalent of
        /// 'Fixed In' in Jira.
        /// </summary>
        internal override ExVersion GetVersion()
        {
            if (m_oJIssue.fixVersions.Length > 0)
            {
                return JFactory.GetVersion(Project as JProject, m_oJIssue.fixVersions[0]);
            }

            return null;
        }

        /// <summary>
        /// Set the version in which the task will be fixed, or null if it has no version.
        /// </summary>
        internal override void SetVersion(ExVersion oVersion)
        {
            RemoteFieldValue oAction = new RemoteFieldValue();

            oAction.id = "fixVersions";

            if (oVersion != null)
            {
                JVersion oJVersion = oVersion as JVersion;

                oAction.values = new string[1];
                oAction.values[0] = oJVersion.RemoteVersion.id;
            }
            else
            {
                oAction.values = new string[0];
            }

            RemoteFieldValue[] aoActions = { oAction };

            m_oJIssue = JFactory.ServiceManager.updateIssue(JFactory.RPCToken, m_oJIssue.key, aoActions);
        }

        /// <summary>
        /// Get the parent issue for the issue
        /// </summary>
        internal override ExTask GetParentTask()
        {
            // I cannot see a way to do this with the JIRA API
            return null;
        }

        /// <summary>
        /// Set the parent issue for the issue
        /// </summary>
        internal override void SetParentTask(ExTask oTaskParam)
        {
            // TODO : I cannot see a way to move sub issues via the Jira API
            Factory.Plugin.LogWarning("Failed to update Jira task '" + Name + "' parent task. Sub-issues are currently unsupported.");
        }

        /// <summary>
        /// Is this issue a feature?
        /// </summary>
        internal override bool IsFeature()
        {
            if (JFactory.FeatureCreationType != null)
            {
                return m_oJIssue.type == JFactory.FeatureCreationType.id;
            }

            return false;
        }

        /// <summary>
        /// Return the name of the issue. This is the equivalent of the Jira issue summary.
        /// </summary>
        internal override String GetName()
        {
            return m_oJIssue.summary;
        }

        /// <summary>
        /// Set the name of the task.
        /// </summary>
        internal override void SetName(String strName)
        {
            RemoteFieldValue oAction = new RemoteFieldValue();

            oAction.id = "summary";
            oAction.values = new string[1];
            oAction.values[0] = strName;

            RemoteFieldValue[] aoActions = { oAction };

            m_oJIssue = JFactory.ServiceManager.updateIssue(JFactory.RPCToken, m_oJIssue.key, aoActions);
        }

        /// <summary>
        /// Return the Jira long description for the issue
        /// </summary>
        internal override String GetDescription()
        {
            if (m_oJIssue.description == null)
            {
                return "";
            }
            else
            {
                return m_oJIssue.description;
            }
        }

        /// <summary>
        /// Set the long description of the task.
        /// </summary>
        internal override void SetDescription(String strDescription)
        {
            RemoteFieldValue oAction = new RemoteFieldValue();

            oAction.id = "description";
            oAction.values = new string[1];
            oAction.values[0] = strDescription;

            RemoteFieldValue[] aoActions = { oAction };

            m_oJIssue = JFactory.ServiceManager.updateIssue(JFactory.RPCToken, m_oJIssue.key, aoActions);
        }

        /// <summary>
        /// What state is this issue in?
        /// </summary>
        internal override SDK.TStreamDataState GetState()
        {
            return JFactory.GetStreamDataState(m_oJIssue);
        }

        /// <summary>
        /// Do we need to update the state for this task?
        /// </summary>
        internal override bool StateNeedsUpdating(SDK.TStreamDataState nSyncState)
        {
            return (State != nSyncState);
        }

        /// <summary>
        /// Set the state for the task?
        /// </summary>
        internal override void SetState(SDK.TStreamDataState tState)
        {
            RemoteNamedObject[] aoActions = JFactory.ServiceManager.getAvailableActions(JFactory.RPCToken, m_oJIssue.key);

            foreach (RemoteNamedObject oAction in aoActions)
            {
                if ( JFactory.DoesJiraActionMatchState(oAction.name.ToLower(), tState) )
                {
                    RemoteFieldValue[] aoFieldValues = new RemoteFieldValue[0];
                    m_oJIssue = JFactory.ServiceManager.progressWorkflowAction(JFactory.RPCToken, m_oJIssue.key, oAction.id, aoFieldValues);
                    return;
                }
            }

            if (tState == SDK.TStreamDataState.Rejected)
            {
                Factory.Plugin.Trace("Failed to find rejected state for task '" + Name + "'. Will use the closed state instead.");
                SetState(SDK.TStreamDataState.Closed);
                return;
            }

            if (tState == SDK.TStreamDataState.Completed)
            {
                Factory.Plugin.Trace("Failed to find completed state for task '" + Name + "'. Will use the open state instead.");
                SetState(SDK.TStreamDataState.Open);
                return;
            }

            Factory.Plugin.LogWarning("Failed to update Jira task '" + Name + "' state. An action could not be found to put the issue into state '" + tState + "'.");
        }

        /// <summary>
        /// Do we need to update the priority for this task?
        /// </summary>
        internal override bool PriorityNeedsUpdating(UInt16 nSyncPriority)
        {
            int nPriority = Priority;

            if (nPriority == nSyncPriority)
            {
                return false;
            }
            else if (nSyncPriority > nPriority)
            {
                // Need to handle the case where there are more PCM priorities than Jira - so we just use
                // the last Jira priority for all lower priorities
                RemotePriority[] atPriorities = JFactory.ServiceManager.getPriorities(JFactory.RPCToken);

                return (m_oJIssue.priority != atPriorities[atPriorities.Length - 1].id);
            }

            return true;
        }

        /// <summary>
        /// What is the priority of the issue? (1 is high, 5 is low)
        /// </summary>
        internal override UInt16 GetPriority()
        {
            UInt16 nPriority = 1;
            RemotePriority[] atPriorities = JFactory.ServiceManager.getPriorities(JFactory.RPCToken);

            foreach(RemotePriority tPriority in atPriorities)
            {
                if (tPriority.id == m_oJIssue.priority)
                {
                    return nPriority;
                }

                nPriority++;
            }

            Factory.Plugin.LogWarning("Failed to get Jira task '" + Name + "' priority. Failed to get a priority object with id '" + m_oJIssue.priority + "'.");

            return 3; // 3 is average priority
        }

        /// <summary>
        /// Set the priority of the task? (1 is high, 5 is low)
        /// </summary>
        internal override void SetPriority(UInt16 nPriority)
        {
            RemotePriority[] atPriorities = JFactory.ServiceManager.getPriorities(JFactory.RPCToken);
            RemotePriority tPriority = JFactory.GetPriorityFromPCMPriority(nPriority);

            if (tPriority != null)
            {
                RemoteFieldValue oAction = new RemoteFieldValue();

                oAction.id = "priority";
                oAction.values = new string[1];
                oAction.values[0] = tPriority.id;

                RemoteFieldValue[] aoActions = { oAction };

                m_oJIssue = JFactory.ServiceManager.updateIssue(JFactory.RPCToken, m_oJIssue.key, aoActions);
            }
            else
            {
                Factory.Plugin.LogWarning("Failed to update Jira task '" + Name + "' priority. Failed to get a priority object.");
            }
        }

        /// <summary>
        /// What is the URL of the issue?
        /// </summary>
        internal override String GetUrl()
        {
            if (JFactory.Options.UpdateURL)
            {
                return JFactory.Options.JURL + "/browse/" + m_oJIssue.key;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Set the URL of the task?
        /// </summary>
        internal override void SetUrl(String strUrl)
        {
            // Doesn't make sense to do this in Jira
        }

        /// <summary>
        /// When was this task last synced?
        /// </summary>
        private DateTime GetLastSyncTime()
        {
            JFactory oFactory = JFactory;
            UInt32 nYear = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeTaskYear), ID);

            if (nYear > 0)
            {
                UInt32 nMonth = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeTaskMonth), ID);
                UInt32 nDay = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeTaskDay), ID);
                UInt32 nHour = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeTaskHour), ID);
                UInt32 nMinute = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeTaskMinute), ID);
                UInt32 nSecond = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeTaskSecond), ID);

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
        /// Set the last time this task was synced
        /// </summary>
        private void SetLastSyncTime(DateTime oSyncTime)
        {
            JFactory oFactory = Factory as JFactory;

            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeTaskYear), ID, Convert.ToUInt32(oSyncTime.Year));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeTaskMonth), ID, Convert.ToUInt32(oSyncTime.Month));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeTaskDay), ID, Convert.ToUInt32(oSyncTime.Day));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeTaskHour), ID, Convert.ToUInt32(oSyncTime.Hour));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeTaskMinute), ID, Convert.ToUInt32(oSyncTime.Minute));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.JiraSyncTimeTaskSecond), ID, Convert.ToUInt32(oSyncTime.Second));
        }

        /// <summary>
        /// This is called if a Jira task has been updated as a result of PureCM changes. This is called after
        /// all updates have been applied
        /// </summary>
        internal override void OnSyncComplete()
        {
            // Update the remote issue record - bacause Jira does not seem to update the returned 'updated' field after
            // updating the issue
            m_oJIssue = JFactory.ServiceManager.getIssue(JFactory.RPCToken, m_oJIssue.key);

            // Store the date revised time so that we know to ignore this in ShouldInclude
            LastSyncTime = m_oJIssue.updated.Value;
        }

        /// <summary>
        /// Should the Jira task be synchronized with PureCM
        /// </summary>
        internal override bool ShouldInclude()
        {
            if (base.ShouldInclude())
            {
                // Don't include child issues
                if ((SyncID == 0) && (JFactory.IsIssueTypeChildType(UInt32.Parse(m_oJIssue.type))))
                {
                    JFactory.Plugin.Trace("Not syncing '" + Name + "' because it is a child issue. Jira child issues are not supported.");
                    return false;
                }

                // We don't want to synchronize changes made by PureCM back into PureCM - so we need to check the LastSyncTime
                // is before the Jira issue modification time
                DateTime oLastSyncTime = LastSyncTime;

                if (oLastSyncTime.Year > 0)
                {
                    if (oLastSyncTime.CompareTo(m_oJIssue.updated.Value) >= 0)
                    {
                        JFactory.Plugin.Trace("Not syncing '" + Name + "' because the change was made in Jira.");
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
