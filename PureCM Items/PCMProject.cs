using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PureCM.Client;

using External_Items;

namespace PureCM_Items
{
    /// <summary>
    /// A wrapper class for a PureCM project.
    /// </summary>
    internal class PCMProject : ExProject
    {
        internal PCMProject(PCMFactory oPFactory, Project oProject)
            : base(oPFactory, oPFactory.PluginProjectType, oProject.Id)
        {
            m_oProject = oProject;
        }

        internal Project Project { get { return m_oProject; } }

        internal PCMFactory PCMFactory { get { return Factory as PCMFactory; } }
        private Project m_oProject;

        /// <summary>
        /// Returns an array of the PureCM tasks which have been updated since the last synchronize.
        /// This will include tasks which have been added and deleted.
        /// </summary>
        internal override List<ExTask> GetRecentTasks()
        {
            ProjectItems oPTasks;
            DateTime oLastSyncTime = LastSyncTime;

            if (oLastSyncTime.Ticks > 0)
            {
                oPTasks = m_oProject.GetProject.UpdatedTasks(oLastSyncTime);
            }
            else
            {
                oPTasks = m_oProject.GetProject.TasksRecursive;
            }

            List<ExTask> aoTasks = new List<ExTask>();

            foreach(ProjectItem oPTask in oPTasks)
            {
                aoTasks.Add(PCMFactory.CreateTask(this, oPTask));
            }

            if (oLastSyncTime.Ticks == 0)
            {
                // Also need to include features
                oPTasks = m_oProject.GetProject.FeaturesRecursive;

                foreach (ProjectItem oPFeature in oPTasks)
                {
                    aoTasks.Add(PCMFactory.CreateTask(this, oPFeature));
                }
            }

            return aoTasks;
        }

        /// <summary>
        /// When were the tasks last synchronized?
        /// </summary>
        internal override DateTime GetLastSyncTime()
        {
            UInt32 nYear = PCMFactory.Repository.GetPluginIDFromPureCMID(Convert.ToUInt32(PCMFactory.PluginProjectSyncType), ID);

            if (nYear > 0)
            {
                UInt32 nMonth = PCMFactory.Repository.GetPluginIDFromPureCMID(Convert.ToUInt32(PCMFactory.PluginProjectSyncType) + 1, ID);
                UInt32 nDay = PCMFactory.Repository.GetPluginIDFromPureCMID(Convert.ToUInt32(PCMFactory.PluginProjectSyncType) + 2, ID);
                UInt32 nHour = PCMFactory.Repository.GetPluginIDFromPureCMID(Convert.ToUInt32(PCMFactory.PluginProjectSyncType) + 3, ID);
                UInt32 nMinute = PCMFactory.Repository.GetPluginIDFromPureCMID(Convert.ToUInt32(PCMFactory.PluginProjectSyncType) + 4, ID);
                UInt32 nSecond = PCMFactory.Repository.GetPluginIDFromPureCMID(Convert.ToUInt32(PCMFactory.PluginProjectSyncType) + 5, ID);

                if ((nMonth > 0) && (nDay > 0))
                {
                    DateTime tUTCDateTime = new DateTime(Convert.ToInt32(nYear), Convert.ToInt32(nMonth), Convert.ToInt32(nDay), Convert.ToInt32(nHour), Convert.ToInt32(nMinute), Convert.ToInt32(nSecond));

                    return tUTCDateTime.ToLocalTime();
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
            DateTime oSyncTimeUTC = oSyncTime.ToUniversalTime();

            PCMFactory.Repository.SetPureCMPluginID(Convert.ToUInt32(PCMFactory.PluginProjectSyncType), ID, Convert.ToUInt32(oSyncTimeUTC.Year));
            PCMFactory.Repository.SetPureCMPluginID(Convert.ToUInt32(PCMFactory.PluginProjectSyncType + 1), ID, Convert.ToUInt32(oSyncTimeUTC.Month));
            PCMFactory.Repository.SetPureCMPluginID(Convert.ToUInt32(PCMFactory.PluginProjectSyncType + 2), ID, Convert.ToUInt32(oSyncTimeUTC.Day));
            PCMFactory.Repository.SetPureCMPluginID(Convert.ToUInt32(PCMFactory.PluginProjectSyncType + 3), ID, Convert.ToUInt32(oSyncTimeUTC.Hour));
            PCMFactory.Repository.SetPureCMPluginID(Convert.ToUInt32(PCMFactory.PluginProjectSyncType + 4), ID, Convert.ToUInt32(oSyncTimeUTC.Minute));
            PCMFactory.Repository.SetPureCMPluginID(Convert.ToUInt32(PCMFactory.PluginProjectSyncType + 5), ID, Convert.ToUInt32(oSyncTimeUTC.Second));
        }

        /// <summary>
        /// Should this project be synchronized with the other system?
        /// </summary>
        internal override bool ShouldInclude()
        {
            return m_oProject.State == SDK.TStreamDataState.Open;
        }

        /// <summary>
        /// The name of the PureCM project
        /// </summary>
        internal override String GetName()
        {
            return m_oProject.Name;
        }

        /// <summary>
        /// The description of the PureCM project
        /// </summary>
        internal override String GetDescription()
        {
            return m_oProject.Description;
        }
    }
}
