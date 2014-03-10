using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PureCM.Client;
using PureCM.Server;

using External_Items;

namespace PureCM_Items
{
    /// <summary>
    /// A wrapper class for a PureCM task.
    /// </summary>
    internal class PCMTask : ExTask
    {
        internal PCMTask(PCMProject oProject, ProjectItem oTask)
            : base(oProject, oProject.PCMFactory.PluginProjectType + 2, oTask.Id)
        {
            m_oTask = oTask;
        }

        private PCMFactory PCMFactory { get { return Factory as PCMFactory; } }
        private PCMProject PCMProject { get { return Project as PCMProject; } }

        private ProjectItem m_oTask;

        /// <summary>
        /// Return the owner of the task, or null if it has no owner.
        /// </summary>
        internal override ExUser GetOwner()
        {
            if (m_oTask.Owner != null)
            {
                return PCMFactory.GetUser(m_oTask.Owner);
            }

            return null;
        }

        /// <summary>
        /// Set the owner of the task, or null if it has no owner.
        /// </summary>
        internal override void SetOwner(ExUser oOwner)
        {
            if (oOwner == null)
            {
                m_oTask.Owner = null;
            }
            else if (oOwner is PCMUser)
            {
                PCMUser oPCMOwner = oOwner as PCMUser;

                if (oPCMOwner != null)
                {
                    m_oTask.Owner = oPCMOwner.UserOrGroup;
                }
                else
                {
                    Factory.Plugin.LogWarning("Failed to set owner for PureCM task '" + Name + "'. The owner is not a PureCM user.");
                }
            }
            else
            {
                Factory.Plugin.LogWarning("Failed to set owner for PureCM task '" + Name + "'. The owner is not a PureCM user.");
            }
        }

        /// <summary>
        /// Set the project in which the task will be fixed (the version will be null so it will go into the backlog)
        /// </summary>
        internal override void SetProject(ExProject oProject)
        {
            if (oProject is PCMProject)
            {
                PCMProject oPCMProject= oProject as PCMProject;

                if (oPCMProject != null)
                {
                    m_oTask.Project = oPCMProject.Project;
                }
                else
                {
                    Factory.Plugin.LogWarning("Failed to set project for PureCM task '" + m_oTask.Name + "'. The PureCM project is null.");
                }
            }
            else
            {
                Factory.Plugin.LogWarning("Failed to set project for PureCM task '" + m_oTask.Name + "'. Passing incorrect project type.");
            }
        }

        /// <summary>
        /// Return the version in which the task will be fixed, or null if it has no version.
        /// </summary>
        internal override ExVersion GetVersion()
        {
            if (m_oTask.Version != null)
            {
                return PCMFactory.GetVersion(PCMProject, m_oTask.Version);
            }

            return null;
        }

        /// <summary>
        /// Set the version in which the task will be fixed, or null if it has no version.
        /// </summary>
        internal override void SetVersion(ExVersion oVersion)
        {
            if (oVersion == null)
            {
                m_oTask.Version = null;
            }
            else if (oVersion is PCMVersion)
            {
                PCMVersion oPCMVersion = oVersion as PCMVersion;

                if (oPCMVersion != null)
                {
                    m_oTask.Version = oPCMVersion.PureCMObject;
                }
                else
                {
                    Factory.Plugin.LogWarning("Failed to set version for PureCM task '" + m_oTask.Name + "'. The PureCM version is null.");
                }
            }
            else
            {
                Factory.Plugin.LogWarning("Failed to set version for PureCM task '" + m_oTask.Name + "'. Passing incorrect version type.");
            }
        }

        /// <summary>
        /// Get the parent task for the task
        /// </summary>
        internal override ExTask GetParentTask()
        {
            ProjectItem oParentTask = m_oTask.Parent;

            while (oParentTask != null && oParentTask.StreamDataType == SDK.TStreamDataType.pcmStreamTypeFeature)
            {
                Feature oFeature = oParentTask as Feature;

                if (oFeature != null && !oFeature.IsFolder())
                {
                    PCMTask oParent = PCMFactory.CreateTask(PCMProject, oFeature);

                    if (oParent.Include)
                    {
                        return oParent;
                    }
                }

                oParentTask = oParentTask.Parent;
            }

            return null;
        }

        /// <summary>
        /// Set the parent task for the task
        /// </summary>
        internal override void SetParentTask(ExTask oTaskParam)
        {
            PCMTask oTask = (PCMTask)oTaskParam;

            if ( (oTask != null) && (oTask.m_oTask.StreamDataType != SDK.TStreamDataType.pcmStreamTypeTask) )
            {
                m_oTask.Parent = oTask.m_oTask;
            }
            else
            {
                ExVersion oVersionEx = Version;

                if (oVersionEx != null)
                {
                    PCMVersion oVersion = (PCMVersion)oVersionEx;
                    m_oTask.Parent = oVersion.PureCMObject;
                }
                else
                {
                    PCMProject oProject = (PCMProject)Project;

                    m_oTask.Parent = oProject.Project;
                }
            }
        }

        /// <summary>
        /// Should this task be synchronized?
        /// </summary>
        internal override bool ShouldInclude()
        {
            if (SyncID > 0)
            {
                return true;
            }
            else if (PCMFactory.SynchronizeTasks)
            {
                if (PCMFactory.SynchronizeChildTasks || (GetParentTask() == null))
                {
                    return base.ShouldInclude();
                }
            }

            return false;
        }

        /// <summary>
        /// Is this task a feature?
        /// </summary>
        internal override bool IsFeature()
        {
            return m_oTask.StreamDataType == SDK.TStreamDataType.pcmStreamTypeFeature;
        }

        /// <summary>
        /// Return the name of the task.
        /// </summary>
        internal override String GetName()
        {
            return m_oTask.Name;
        }

        /// <summary>
        /// Set the name of the task.
        /// </summary>
        internal override void SetName(String strName)
        {
            m_oTask.Name = strName;
        }

        /// <summary>
        /// Return the long description for the task
        /// </summary>
        internal override String GetDescription()
        {
            return m_oTask.Description;
        }

        /// <summary>
        /// Set the long description of the task.
        /// </summary>
        internal override void SetDescription(String strDescription)
        {
            m_oTask.Description = strDescription;
        }

        /// <summary>
        /// What state is this task in?
        /// </summary>
        internal override SDK.TStreamDataState GetState()
        {
            return m_oTask.State;
        }

        /// <summary>
        /// Set the state for the task?
        /// </summary>
        internal override void SetState(SDK.TStreamDataState tState)
        {
            switch (tState)
            {
                case SDK.TStreamDataState.Open:
                    if (!m_oTask.Assign(0))
                    {
                        Factory.Plugin.LogWarning("Failed to set state for PureCM task '" + m_oTask.Name + "'. Failed to set the state as 'open'.");
                    }
                    break;
                case SDK.TStreamDataState.Completed:
                    if (!m_oTask.Complete())
                    {
                        Factory.Plugin.LogWarning("Failed to set state for PureCM task '" + m_oTask.Name + "'. Failed to set the state as 'completed'.");
                    }
                    break;
                case SDK.TStreamDataState.Closed:
                    if (!m_oTask.Close())
                    {
                        Factory.Plugin.LogWarning("Failed to set state for PureCM task '" + m_oTask.Name + "'. Failed to set the state as 'closed'.");
                    }
                    break;
                case SDK.TStreamDataState.Rejected:
                    if (!m_oTask.Reject())
                    {
                        Factory.Plugin.LogWarning("Failed to set state for PureCM task '" + m_oTask.Name + "'. Failed to set the state as 'rejected'.");
                    }
                    break;
                default:
                    Factory.Plugin.LogWarning("Failed to set state for PureCM task '" + m_oTask.Name + "'. Trying to set unknown state.");
                    break;
            }
        }

        /// <summary>
        /// What is the priority of the task? (1 is high, 5 is low)
        /// </summary>
        internal override UInt16 GetPriority()
        {
            return m_oTask.Priority;
        }

        /// <summary>
        /// Set the priority of the task? (1 is high, 5 is low)
        /// </summary>
        internal override void SetPriority(UInt16 nPriority)
        {
            m_oTask.Priority = nPriority;
        }

        /// <summary>
        /// What is the URL of the task?
        /// </summary>
        internal override String GetUrl()
        {
            return m_oTask.Url;
        }

        /// <summary>
        /// Set the URL of the task?
        /// </summary>
        internal override void SetUrl(String strUrl)
        {
            m_oTask.Url = strUrl;
        }

        /// <summary>
        /// Do we set the URL for this task?
        /// </summary>
        internal override bool DoSetUrl()
        {
            return true;
        }
    }
}
