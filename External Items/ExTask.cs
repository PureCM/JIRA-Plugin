using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PureCM.Client;
using PureCM.Server;

namespace External_Items
{
    /// <summary>
    /// Abstract class for a task.
    /// </summary>
    internal abstract class ExTask : ExProjectItem
    {
        internal ExTask(ExProject oExProject, ExFactory.TPluginType tType, UInt32 nExID)
            : base(oExProject, tType, nExID)
        {
        }

        /// <summary>
        /// Get the synchronized task.
        /// </summary>
        internal ExTask GetSyncTask( bool bCreateIfNotExist )
        {
            if (m_oSyncTask == null)
            {
                m_oSyncTask = Factory.SyncFactory.GetTask(this);

                if ((m_oSyncTask == null) && bCreateIfNotExist)
                {
                    m_oSyncTask = Factory.SyncFactory.CreateTask(this);

                    if (m_oSyncTask == null)
                    {
                        Factory.Plugin.LogInfo("Failed to create " + Factory.SyncFactory.PluginName + " task for " + Factory.PluginName + " task '" + Name + "'.");
                    }
                }
            }

            return m_oSyncTask;
        }

        /// <summary>
        /// Update the sync task details to match this task details
        /// </summary>
        internal void Synchronize()
        {
            if (Include)
            {
                ExTask oSyncTask = GetSyncTask(true);

                if (oSyncTask != null)
                {
                    Factory.Plugin.Trace("Synchronizing task '" + oSyncTask.Name + "'.");

                    m_bUpdated = false;

                    bool bUpdatedState = false;

                    if ( (State != SDK.TStreamDataState.Rejected) &&
                         (State != SDK.TStreamDataState.Closed))
                    {
                        SyncState(oSyncTask);
                        bUpdatedState = true;
                    }

                    SyncName(oSyncTask);
                    SyncDescription(oSyncTask);
                    SyncProjectAndVersion(oSyncTask);
                    SyncParentTask(oSyncTask);

                    if (!bUpdatedState)
                    {
                        SyncState(oSyncTask);
                    }

                    SyncOwner(oSyncTask);
                    SyncPriority(oSyncTask);
                    SyncUrl(oSyncTask);

                    if (m_bUpdated)
                    {
                        oSyncTask.OnSyncComplete();
                    }
                }
            }
        }

        private void SyncName(ExTask oSyncTask)
        {
            if (Name != oSyncTask.Name)
            {
                Factory.Plugin.Trace("Updating name for task '" + oSyncTask.Name + "' to be '" + Name + "'.");
                oSyncTask.Name = Name;
                m_bUpdated = true;
            }
        }

        private void SyncDescription(ExTask oSyncTask)
        {
            if (Description != oSyncTask.Description)
            {
                Factory.Plugin.Trace("Updating description for task '" + oSyncTask.Name + "' to be '" + Description + "'.");
                oSyncTask.Description = Description;
                m_bUpdated = true;
            }
        }

        private void SyncProjectAndVersion(ExTask oSyncTask)
        {
            ExProject oNewSyncProject = Project.GetSyncProject(true);

            if (oNewSyncProject != null)
            {
                if (oNewSyncProject.ID != oSyncTask.Project.ID)
                {
                    Factory.Plugin.Trace("Moving task '" + oSyncTask.Name + "' to project '" + oNewSyncProject.Name + "'.");
                    oSyncTask.Project = oNewSyncProject;
                    m_bUpdated = true;
                }
                else if (Version == null)
                {
                    if (oSyncTask.Version != null)
                    {
                        Factory.Plugin.Trace("Moving task '" + oSyncTask.Name + "' to the backlog.");
                        oSyncTask.Version = null;
                        m_bUpdated = true;
                    }
                }
                
                if (Version != null)
                {
                    ExVersion oNewSyncVersion = Version.GetSyncVersion(true);

                    if ((oSyncTask.Version == null) || (oSyncTask.Version.ID != oNewSyncVersion.ID))
                    {
                        if (oNewSyncVersion == null)
                        {
                            Factory.Plugin.Trace("Moving task '" + oSyncTask.Name + "' to backlog.");
                        }
                        else
                        {
                            Factory.Plugin.Trace("Moving task '" + oSyncTask.Name + "' to version '" + oNewSyncVersion.Name + "'.");
                        }
                        oSyncTask.Version = oNewSyncVersion;
                        m_bUpdated = true;
                    }
                }
            }
            else
            {
                Factory.Plugin.LogWarning("Failed to move PureCM task '" + oSyncTask.Name + "'. Failed to create the new project '" + oNewSyncProject.Name + "'.");
            }
        }

        private void SyncParentTask(ExTask oSyncTask)
        {
            ExTask oNewParentTask = ParentTask;

            if (oNewParentTask == null)
            {
                if (oSyncTask.ParentTask != null)
                {
                    Factory.Plugin.Trace("Setting task '" + oSyncTask.Name + "' to not have a parent task.");
                    oSyncTask.ParentTask = null;
                    m_bUpdated = true;
                }
            }
            else
            {
                ExTask oNewParentSyncTask = oNewParentTask.GetSyncTask(true);

                if (oNewParentSyncTask != null)
                {
                    ExTask oParentSyncTask = oSyncTask.ParentTask;

                    if (oParentSyncTask == null || oNewParentSyncTask.ID != oParentSyncTask.SyncID)
                    {
                        Factory.Plugin.Trace("Setting task '" + oSyncTask.Name + "' to be a child of task '" + oNewParentSyncTask.Name + "'.");
                        oSyncTask.ParentTask = oNewParentSyncTask;
                        m_bUpdated = true;
                    }
                }
                else
                {
                    Factory.Plugin.LogWarning("Failed to set task '" + oSyncTask.Name + "' parent. Failed to create parent task.");
                    oSyncTask.ParentTask = null;
                    m_bUpdated = true;
                }
            }
        }

        private void SyncOwner(ExTask oSyncTask)
        {
            ExUser oOwner = Owner;
            ExUser oSyncOwner = null;
            
            if ( oOwner != null )
            {
                oSyncOwner = oOwner.GetSyncUser(true);
            }

            if (oSyncOwner == null)
            {
                if (oSyncTask.Owner != null)
                {
                    Factory.Plugin.Trace("Setting task '" + oSyncTask.Name + "' owner to be null.");
                    oSyncTask.Owner = null;
                    m_bUpdated = true;
                }
            }
            else
            {
                if ((oSyncTask.Owner == null) || (oSyncTask.Owner.ID != oSyncOwner.ID))
                {
                    Factory.Plugin.Trace("Setting task '" + oSyncTask.Name + "' owner to be '" + oSyncOwner.Name + "'.");
                    oSyncTask.Owner = oSyncOwner;
                    m_bUpdated = true;
                }
            }
        }

        private void SyncState(ExTask oSyncTask)
        {
            SDK.TStreamDataState tState = State;

            if (oSyncTask.StateNeedsUpdating(tState))
            {
                Factory.Plugin.Trace("Updating state for task '" + oSyncTask.Name + "' to be '" + GetStateText(tState) + "'.");
                oSyncTask.State = tState;
                m_bUpdated = true;
            }
        }

        static private String GetStateText(SDK.TStreamDataState tState)
        {
            switch (tState)
            {
                case SDK.TStreamDataState.Open:
                    return "Open";
                case SDK.TStreamDataState.Completed:
                    return "Completed";
                case SDK.TStreamDataState.Closed:
                    return "Closed";
                case SDK.TStreamDataState.Rejected:
                    return "Rejected";
                case SDK.TStreamDataState.Unknown:
                default:
                    return "[UNKNOWN]";
            }
        }

        private void SyncPriority(ExTask oSyncTask)
        {
            UInt16 nPriority = Priority;

            if (oSyncTask.PriorityNeedsUpdating(nPriority))
            {
                Factory.Plugin.Trace("Setting task '" + oSyncTask.Name + "' priority to be '" + nPriority + "'.");
                oSyncTask.Priority = nPriority;
                m_bUpdated = true;
            }
        }

        private void SyncUrl(ExTask oSyncTask)
        {
            String strUrl = Url;
            String strSyncUrl = oSyncTask.Url;

            if (oSyncTask.DoSetUrl() && (strUrl.Length > 0) && (strUrl != strSyncUrl))
            {
                Factory.Plugin.Trace("Setting task '" + oSyncTask.Name + "' URL to be '" + strUrl + "'.");
                oSyncTask.Url = strUrl;
                m_bUpdated = true;
            }
            else if (!oSyncTask.DoSetUrl() && DoSetUrl() && (strSyncUrl.Length > 0) && (strUrl != strSyncUrl))
            {
                Factory.Plugin.Trace("Setting task '" + Name + "' URL to be '" + strSyncUrl + "' (reversed).");
                Url = strSyncUrl;
            }
        }

        /// <summary>
        /// Do we set the URL for this task?
        /// </summary>
        internal virtual bool DoSetUrl()
        {
            return false;
        }

        /// <summary>
        /// The short name for this task?
        /// </summary>
        new internal String Name { get { return GetName(); } set { SetName(value); } }

        /// <summary>
        /// The long description for this task?
        /// </summary>
        new internal String Description { get { return GetDescription(); } set { SetDescription(value); } }

        /// <summary>
        /// What project does this task belong too?
        /// </summary>
        new internal ExProject Project { get { return base.Project; } set { SetProject(value); } }

        /// <summary>
        /// What version does this task belong too?
        /// </summary>
        new internal ExVersion Version { get { return GetVersion(); } set { SetVersion(value); } }

        /// <summary>
        /// What is the parent task (or null)?
        /// </summary>
        internal ExTask ParentTask { get { return GetParentTask(); } set { SetParentTask(value); } }

        /// <summary>
        /// Who owns this task?
        /// </summary>
        new internal ExUser Owner { get { return GetOwner(); } set { SetOwner(value); } }

        /// <summary>
        /// The state of the task (open, completed, closed or rejected)
        /// </summary>
        internal SDK.TStreamDataState State { get { return GetState(); } set { SetState(value); } }

        /// <summary>
        /// The priority of the task (1 is high, 5 is low)
        /// </summary>
        internal UInt16 Priority { get { return GetPriority(); } set { SetPriority(value); } }

        /// <summary>
        /// The URL of the task
        /// </summary>
        internal String Url { get { return GetUrl(); } set { SetUrl(value); } }

        private ExTask m_oSyncTask;
        private bool m_bUpdated = false;

        /// <summary>
        /// Should this task be synchronized?
        /// </summary>
        internal override bool ShouldInclude()
        {
            if ((SyncID > 0) || (State == SDK.TStreamDataState.Open))
            {
                return true;
            }
            else
            {
                Factory.Plugin.Trace("Not synchronizing task '" + Name + "' because it is not open.");
                return false;
            }
        }

        /// <summary>
        /// Is this task a feature?
        /// </summary>
        internal abstract bool IsFeature();

        /// <summary>
        /// Set the short name for this task?
        /// </summary>
        internal abstract void SetName(String strName);

        /// <summary>
        /// Set the long description for this task?
        /// </summary>
        internal abstract void SetDescription(String strDescription);

        /// <summary>
        /// Set the project of the task
        /// </summary>
        internal abstract void SetProject(ExProject oProject);

        /// <summary>
        /// Set the version of the task
        /// </summary>
        internal abstract void SetVersion(ExVersion oVersion);

        /// <summary>
        /// Get the parent task for the task
        /// </summary>
        internal abstract ExTask GetParentTask();

        /// <summary>
        /// Set the parent task for the task
        /// </summary>
        internal abstract void SetParentTask(ExTask oTask);

        /// <summary>
        /// Set the owner of the task
        /// </summary>
        internal abstract void SetOwner(ExUser oOwner);

        /// <summary>
        /// What state is this task in?
        /// </summary>
        internal abstract SDK.TStreamDataState GetState();

        /// <summary>
        /// Do we need to update the state for this task?
        /// </summary>
        internal virtual bool StateNeedsUpdating(SDK.TStreamDataState nSyncState)
        {
            return (State != nSyncState);
        }

        /// <summary>
        /// Set the state of the task
        /// </summary>
        internal abstract void SetState(SDK.TStreamDataState tState);

        /// <summary>
        /// What is the priority of the task? (1 is high, 5 is low)
        /// </summary>
        internal abstract UInt16 GetPriority();

        /// <summary>
        /// Do we need to update the priority for this task?
        /// </summary>
        internal virtual bool PriorityNeedsUpdating(UInt16 nSyncPriority)
        {
            return (Priority != nSyncPriority);
        }

        /// <summary>
        /// Set the priority of the task (1 is high, 5 is low)
        /// </summary>
        internal abstract void SetPriority(UInt16 nPriority);

        /// <summary>
        /// What is the Url of the task?
        /// </summary>
        internal abstract String GetUrl();

        /// <summary>
        /// Set the Url of the task
        /// </summary>
        internal abstract void SetUrl(String strUrl);

        /// <summary>
        /// Override this if you want to batch update the task changes. This will be called
        /// after all the task values have been updated - if one or more values have been
        /// updated. The alternative is to update the task for each call to SetName(), SetDescription(),
        /// etc. But this is non-transactional and may be inefficient.
        /// </summary>
        internal virtual void OnSyncComplete() { }
    }
}
