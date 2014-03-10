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
    /// The PureCM Factory. This is used to create PureCM objects. Each PureCM project has it's own
    /// factory.
    /// </summary>
    internal class PCMFactory : ExFactory
    {
        internal PCMFactory(PureCM.Server.Plugin oPlugin, ExFactory.TPluginType tPluginProjectType, ExFactory.TPluginSyncType tPluginProjectSyncType, Repository oRepository, bool bSynchronizeTasks, bool bSynchronizeChildTasks)
            : base("PureCM", oPlugin)
        {
            m_oRepository = oRepository;
            m_bSynchronizeTasks = bSynchronizeTasks;
            m_bSynchronizeChildTasks = bSynchronizeChildTasks;
            PluginProjectType = tPluginProjectType;
            PluginProjectSyncType = tPluginProjectSyncType;
        }

        internal Repository Repository { get { return m_oRepository; } }
        internal bool SynchronizeTasks { get { return m_bSynchronizeTasks; } }
        internal bool SynchronizeChildTasks { get { return m_bSynchronizeChildTasks; } }
        internal ExFactory.TPluginType PluginProjectType { get; set; }
        internal ExFactory.TPluginSyncType PluginProjectSyncType { get; set; }

        private PureCM.Client.Version GetParentVersionFromSyncItem(ExProjectItem oSyncItem)
        {
            ExVersion oSyncParentVersion = oSyncItem.Version;

            if ( oSyncParentVersion != null )
            {
                PCMVersion oPCMParentVersion = oSyncParentVersion.GetSyncVersion(true) as PCMVersion;

                if (oPCMParentVersion != null)
                {
                    return oPCMParentVersion.PureCMObject;
                }
                else
                {
                    Plugin.LogWarning("Failed to create PureCM version from external version '" + oSyncParentVersion.Name + ".");
                }
            }

            return null;
        }

        private readonly Repository m_oRepository;
        private readonly bool m_bSynchronizeTasks;
        private readonly bool m_bSynchronizeChildTasks;
        private List<ExProject> m_aoProjects;

        /// <summary>
        /// Get an array of all the PureCM projects
        /// </summary>
        internal override List<ExProject> GetProjects()
        {
            if ( m_aoProjects == null )
            {
                m_aoProjects = new List<ExProject>();

                foreach (Project oProject in Repository.ProjectsRecursive)
                {
                    m_aoProjects.Add(GetProject(oProject));
                }
            }

            return m_aoProjects;
        }

        /// <summary>
        /// Get the ID of the project we are synchronized with
        /// </summary>
        internal override UInt32 GetSyncID(TPluginType tType, UInt32 nID)
        {
            return m_oRepository.GetPluginIDFromPureCMID(System.Convert.ToUInt32(tType), nID);
        }

        /// <summary>
        /// Set the ID of the project we are synchronized with
        /// </summary>
        internal override void SetSyncID(TPluginType tType, UInt32 nID, UInt32 nSyncID)
        {
            m_oRepository.SetPureCMPluginID(System.Convert.ToUInt32(tType), nID, nSyncID);
        }

        /// <summary>
        /// Get the project from the PureCM project.
        /// </summary>
        internal PCMProject GetProject(Project oProject)
        {
            return new PCMProject(this, oProject);
        }

        /// <summary>
        /// Get the project from the synchronized project. Return null if the project is not synchronized.
        /// </summary>
        internal override ExProject GetProject(ExProject oSyncProject)
        {
            UInt32 nID = oSyncProject.SyncID;

            if ( nID > 0 )
            {
                Project oProject = Repository.Projects.ById(nID) as Project;

                if (oProject != null)
                {
                    return GetProject(oProject);
                }
                else
                {
                    Plugin.LogWarning("Failed to get PureCM project from external project '" + oSyncProject.Name + ". A PureCM project with ID '" + nID + "' does not exist.");
                }
            }

            return null;
        }

        /// <summary>
        /// Create a new project and synchronize it with this project
        /// </summary>
        internal override ExProject CreateProject(ExProject oSyncProject)
        {
            if (!oSyncProject.Include)
            {
                Plugin.LogWarning("Failed to create PureCM project '" + oSyncProject.Name + "'. This project is flagged as not to be included.");
                return null;
            }

            // If a project already exists with this name then use that
            Project oProject = (Project)Repository.Projects.ByName(oSyncProject.Name);

            if (oProject == null)
            {
                oProject = Repository.Projects.AddProject(oSyncProject.Name);
            }

            if (oProject != null)
            {
                ExProject oPCMProject = GetProject(oProject);

                oPCMProject.SyncID = oSyncProject.ID;

                return oPCMProject;
            }
            else
            {
                Plugin.LogWarning("Failed to create PureCM project from external project '" + oSyncProject.Name + ".");
            }

            return null;
        }

        /// <summary>
        /// Get all the PureCM versions for this PureCM project
        /// </summary>
        internal override List<ExVersion> GetVersions(ExProject oProject)
        {
            List<ExVersion> aoVersions = new List<ExVersion>();
            PCMProject oPCMProject = (PCMProject)oProject;

            foreach (PureCM.Client.Version oPCMVersion in oPCMProject.Project.VersionsRecursive)
            {
                aoVersions.Add(GetVersion(oPCMProject, oPCMVersion));
            }

            return aoVersions;
        }

        /// <summary>
        /// Get the version from the PureCM version.
        /// </summary>
        internal PCMVersion GetVersion(PCMProject oPCMProject, PureCM.Client.Version oVersion)
        {
            return new PCMVersion(oPCMProject, oVersion);
        }

        /// <summary>
        /// Get the version from the synchronized version. Return null if the version is not synchronized.
        /// </summary>
        internal override ExVersion GetVersion(ExVersion oSyncVersion)
        {
            UInt32 nVersionID = oSyncVersion.SyncID;

            if ( nVersionID > 0 )
            {
                ExProject oSyncProject = oSyncVersion.Project;

                if (oSyncProject != null)
                {
                    PCMProject oProject = oSyncProject.GetSyncProject(false) as PCMProject;

                    if (oProject != null)
                    {
                        PureCM.Client.Version oVersion = oProject.Project.VersionsRecursive.ById(nVersionID) as PureCM.Client.Version;

                        if (oVersion != null)
                        {
                            return GetVersion(oProject, oVersion);
                        }
                        else
                        {
                            Plugin.LogWarning("Failed to create PureCM version from external version '" + oSyncVersion.Name + ". The PureCM version with ID '" + nVersionID + "' does not exist.");
                        }
                    }
                }
                else
                {
                    Plugin.LogWarning("Failed to create PureCM version from external version '" + oSyncVersion.Name + ". Failed to get external project from version.");
                }
            }

            return null;
        }

        /// <summary>
        /// Create a new version and synchronize it with this version
        /// </summary>
        internal override ExVersion CreateVersion(ExVersion oSyncVersion)
        {
            if (!oSyncVersion.Include)
            {
                Plugin.LogWarning("Failed to create PureCM version '" + oSyncVersion.Name + "'. This version is flagged as not to be included.");
                return null;
            }

            ExProject oSyncProject = oSyncVersion.Project;

            if (oSyncProject != null)
            {
                PCMProject oProject = oSyncProject.GetSyncProject(true) as PCMProject;

                if (oProject != null)
                {
                    PureCM.Client.Version oParentVersion = GetParentVersionFromSyncItem(oSyncVersion);
                    PureCM.Client.Version oVersion = (PureCM.Client.Version)oProject.Project.VersionsRecursive.ByName(oSyncVersion.Name);

                    if (oVersion == null)
                    {
                        oVersion = oProject.Project.Versions.AddVersion(oSyncVersion.Name, oParentVersion);
                    }

                    if (oVersion != null)
                    {
                        PCMVersion oPCMVersion = GetVersion(oProject, oVersion);

                        oPCMVersion.SyncID = oSyncVersion.ID;

                        return oPCMVersion;
                    }
                    else
                    {
                        Plugin.LogWarning("Failed to create PureCM version from external version '" + oSyncVersion.Name + ". Failed to create PureCM version.");
                    }
                }
                else
                {
                    Plugin.LogWarning("Failed to create PureCM version from external version '" + oSyncVersion.Name + ". Failed to create PureCM project.");
                }
            }
            else
            {
                Plugin.LogWarning("Failed to create PureCM version from external version '" + oSyncVersion.Name + ". Failed to get project from external version.");
            }

            return null;
        }

        /// <summary>
        /// Get the task from the synchronized task. Return null if the task is not synchronized.
        /// </summary>
        internal override ExTask GetTask(ExTask oSyncTask)
        {
            UInt32 nTaskID = oSyncTask.SyncID;

            if ( nTaskID > 0 )
            {
                ExProject oSyncProject = oSyncTask.Project;

                if (oSyncProject != null)
                {
                    PCMProject oProject = oSyncProject.GetSyncProject(false) as PCMProject;

                    if (oProject != null)
                    {
                        ProjectItem oTask = oProject.Project.Tasks.ById(nTaskID);

                        if (oTask != null)
                        {
                            // If the task has moved projects in the external tool, then we need to use the actual
                            // project we are linked too in PureCM
                            if (oTask.Project.Id != oProject.Project.Id)
                            {
                                oProject = GetProject( oTask.Project );
                            }

                            PCMTask oPCMTask = CreateTask(oProject, oTask);

                            return oPCMTask;
                        }
                        else
                        {
                            Plugin.LogWarning("Failed to get PureCM task from external task '" + oSyncTask.Name + ". A PureCM task with ID '" + nTaskID + "' does not exist.");
                        }
                    }
                }
                else
                {
                    Plugin.LogWarning("Failed to get PureCM task from external task '" + oSyncTask.Name + ". Failed to get project from external task.");
                }
            }

            return null;
        }

        /// <summary>
        /// Create a new task from the PureCM task
        /// </summary>
        internal PCMTask CreateTask(PCMProject oProject, ProjectItem oTask)
        {
            return new PCMTask(oProject, oTask);
        }

        /// <summary>
        /// Create a new task and synchronize it with this task
        /// </summary>
        internal override ExTask CreateTask(ExTask oSyncTask)
        {
            ExProject oSyncProject = oSyncTask.Project;

            if (oSyncProject != null)
            {
                PCMProject oProject = oSyncProject.GetSyncProject(true) as PCMProject;

                if (oProject != null)
                {
                    PureCM.Client.Version oParentVersion = GetParentVersionFromSyncItem(oSyncTask);
                    ProjectItem oTask;

                    if (oSyncTask.IsFeature())
                    {
                        if (oParentVersion != null)
                        {
                            oTask = oParentVersion.Features.AddFeature(oSyncTask.Name);
                        }
                        else
                        {
                            oTask = oProject.Project.Features.AddFeature(oSyncTask.Name);
                        }

                        if ( oTask != null && oSyncTask.Description.Length > 0 )
                        {
                            oTask.Description = oSyncTask.Description;
                        }
                    }
                    else
                    {
                        if (oParentVersion != null)
                        {
                            oTask = oParentVersion.Tasks.AddTask(oSyncTask.Name, oSyncTask.Description);
                        }
                        else
                        {
                            oTask = oProject.Project.Tasks.AddTask(oSyncTask.Name, oSyncTask.Description);
                        }
                    }

                    if (oTask != null)
                    {
                        PCMTask oPCMTask = CreateTask(oProject, oTask);

                        oPCMTask.SyncID = oSyncTask.ID;

                        return oPCMTask;
                    }
                    else
                    {
                        Plugin.LogWarning("Failed to create PureCM task from external task '" + oSyncTask.Name + ". Failed to create PureCM task.");
                    }
                }
                else
                {
                    Plugin.LogWarning("Failed to create PureCM task from external task '" + oSyncTask.Name + ". Failed to create PureCM project.");
                }
            }
            else
            {
                Plugin.LogWarning("Failed to create PureCM task from external task '" + oSyncTask.Name + ". Failed to get project from external task.");
            }

            return null;
        }

        /// <summary>
        /// Get the user from the PureCM user.
        /// </summary>
        internal PCMUser GetUser(UserOrGroup oUser)
        {
            return new PCMUser(this, oUser);
        }

        /// <summary>
        /// Get the user from the synchronized user. Return null if the user is not synchronized.
        /// </summary>
        internal override ExUser GetUser(ExUser oSyncUser)
        {
            UserOrGroup oUser;
            UInt32 nID = oSyncUser.SyncID;
            bool bUpdateSyncID = false;

            if ( nID > 0 )
            {
                oUser = Repository.Connection.Users.ById(nID);

                if (oUser == null)
                {
                    Plugin.LogWarning("Failed to get PureCM user from external user '" + oSyncUser.Name + ". A PureCM user with ID '" + nID + "' does not exist.");
                }
            }
            else
            {
                bUpdateSyncID = true;
                oUser = Repository.Connection.Users.ByName(oSyncUser.Name);

                if ( oUser == null )
                {
                    foreach( UserOrGroup oTmpUser in Repository.Connection.Users )
                    {
                        if ( oTmpUser.Email == oSyncUser.EmailAddress)
                        {
                            oUser = oTmpUser;
                            break;
                        }
                    }
                }
            }

            if (oUser != null)
            {
                PCMUser oPCMUser = GetUser(oUser);

                if ( bUpdateSyncID )
                {
                    oPCMUser.SyncID = oSyncUser.ID;
                }

                return oPCMUser;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Create a new user and synchronize it with this user
        /// </summary>
        internal override ExUser CreateUser(ExUser oSyncUser)
        {
            UserOrGroup oUser = Repository.Connection.Users.Add(oSyncUser.Name, "", oSyncUser.Description, oSyncUser.EmailAddress, "", "", false, false );

            if (oUser != null)
            {
                PCMUser oPCMUser = GetUser(oUser);

                oPCMUser.SyncID = oSyncUser.ID;

                return oPCMUser;
            }
            else
            {
                Plugin.LogWarning("Failed to create PureCM user from external user '" + oSyncUser.Name + "'.");
            }

            return null;
        }
    }
}
