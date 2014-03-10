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
    /// Class for creating Jira objects. Each Jira project has a factory.
    /// </summary>
    class JFactory : ExFactory
    {
        internal JFactory(PureCM.Server.Plugin oPlugin, JiraSoapServiceService oServiceManager, Repository oPRepos, JOptions oOptions)
            : base( "Jira", oPlugin )
        {
            m_oServiceManager = oServiceManager;
            m_oPRepos = oPRepos;
            m_oOptions = oOptions;
            RPCToken = oServiceManager.login(oOptions.JUser, oOptions.JPassword);

            if (m_oOptions.JProjectCreate)
            {
                RemoteProject[] atProjects = m_oServiceManager.getProjectsNoSchemes(RPCToken);

                if (atProjects.Length > 0)
                {
                    if (m_oOptions.JProjectCreateTemplate.Length > 0)
                    {
                        m_tProjectCreationTemplate = null;

                        foreach (RemoteProject tProject in atProjects)
                        {
                            if (tProject.name == m_oOptions.JProjectCreateTemplate)
                            {
                                m_tProjectCreationTemplate = tProject;
                                break;
                            }
                        }

                        if (m_tProjectCreationTemplate == null)
                        {
                            m_oOptions.JProjectCreate = false;
                            Plugin.LogWarning("Disabling Jira project creation. A Jira projects with the name '" + m_oOptions.JProjectCreateTemplate + "' does not exist to use as a template.");
                        }
                    }
                    else
                    {
                        m_tProjectCreationTemplate = atProjects[0];
                    }
                }
                else
                {
                    m_oOptions.JProjectCreate = false;
                    Plugin.LogWarning("Disabling Jira project creation. There are no existing Jira projects to use as templates.");
                }
            }

            if (m_oOptions.JTaskCreate)
            {
                RemoteIssueType[] atTypes = m_oServiceManager.getIssueTypes(RPCToken);

                foreach (RemoteIssueType tType in atTypes)
                {
                    if (tType.name == m_oOptions.JTaskCreateTypeDescription)
                    {
                        m_tTaskCreationType = tType;
                        break;
                    }
                }

                if (m_tTaskCreationType == null)
                {
                    Plugin.LogWarning("Disabling Jira issue creation. The specified issue creation type '" + m_oOptions.JTaskCreateTypeDescription + "' is not a valid issue type description.");
                    m_oOptions.JTaskCreate = false;
                }
            }

            if (m_oOptions.JFeatureCreate)
            {
                RemoteIssueType[] atTypes = m_oServiceManager.getIssueTypes(RPCToken);

                foreach (RemoteIssueType tType in atTypes)
                {
                    if (tType.name == m_oOptions.JFeatureCreateTypeDescription)
                    {
                        FeatureCreationType = tType;
                        break;
                    }
                }

                if (FeatureCreationType == null)
                {
                    Plugin.LogWarning("Disabling Jira feature creation. The specified issue creation type '" + m_oOptions.JFeatureCreateTypeDescription + "' is not a valid issue type description.");
                    m_oOptions.JFeatureCreate = false;
                }
            }
        }

        internal RemotePriority GetPriorityFromPCMPriority(UInt16 nPCMPriority)
        {
            RemotePriority[] atPriorities = m_oServiceManager.getPriorities(RPCToken);

            if (atPriorities.Length < nPCMPriority)
            {
                return atPriorities[atPriorities.Length - 1];
            }
            else
            {
                return atPriorities[Math.Max(nPCMPriority - 1, 0)];
            }
        }

        /// <summary>
        /// Returns the Jira Service Manager (this is used to view and update the Jira database)
        /// </summary>
        internal JiraSoapServiceService ServiceManager
        {
            get { return m_oServiceManager; }
        }

        /// <summary>
        /// Get the RPC token
        /// </summary>
        internal string RPCToken{ get; set; }

        /// <summary>
        /// Get the corresponding PureCM repository.
        /// </summary>
        internal Repository PCMRepository
        {
            get { return m_oPRepos; }
        }

        /// <summary>
        /// Get the options.
        /// </summary>
        internal JOptions Options
        {
            get { return m_oOptions; }
        }

        private JiraSoapServiceService m_oServiceManager;
        private Repository m_oPRepos;
        private JOptions m_oOptions;
        private List<ExProject> m_aoProjects;

        private RemoteProject m_tProjectCreationTemplate;
        private RemoteIssueType m_tTaskCreationType;
        internal RemoteIssueType FeatureCreationType {get; set;}

        /// <summary>
        /// Get an array of all the Jira projects
        /// </summary>
        internal override List<ExProject> GetProjects()
        {
            if ( m_aoProjects == null )
            {
                RemoteProject[] aoJProjects = ServiceManager.getProjectsNoSchemes(RPCToken);
                m_aoProjects = new List<ExProject>();

                foreach (RemoteProject oJProject in aoJProjects)
                {
                    m_aoProjects.Add(new JProject(this, oJProject));
                }
            }

            return m_aoProjects;
        }

        /// <summary>
        /// Get the ID of the project we are synchronized with
        /// </summary>
        internal override UInt32 GetSyncID(TPluginType tType, UInt32 nID)
        {
            return m_oPRepos.GetPureCMIDFromPluginID(System.Convert.ToUInt32(tType), nID);
        }

        /// <summary>
        /// Set the ID of the project we are synchronized with
        /// </summary>
        internal override void SetSyncID(TPluginType tType, UInt32 nID, UInt32 nSyncID)
        {
            m_oPRepos.SetPureCMPluginID(System.Convert.ToUInt32(tType), nSyncID, nID);
        }

        /// <summary>
        /// Get the project from the Gemin project ID. Return null if the project is not synchronized.
        /// </summary>
        internal JProject GetProject(int nID)
        {
            if (nID > 0)
            {
                List<ExProject> aoProjects = GetProjects();

                foreach (ExProject oProject in aoProjects)
                {
                    if (oProject.ID == nID)
                    {
                        return oProject as JProject;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get the project from the synchronized project. Return null if the project is not synchronized.
        /// </summary>
        internal override ExProject GetProject(ExProject oSyncProject)
        {
            UInt32 nID = oSyncProject.SyncID;

            if ( nID > 0 )
            {
                return GetProject(Convert.ToInt32(nID));
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
                Plugin.LogWarning("Failed to create Jira project '" + oSyncProject.Name + "'. This project is flagged as not to be included.");
                return null;
            }

            RemoteProject tProject = null;

            // If a Jira project already exists with this name then use that
            {
                RemoteProject[] atProjects = ServiceManager.getProjectsNoSchemes(RPCToken);

                foreach (RemoteProject tPossibleProject in atProjects)
                {
                    if (tPossibleProject.name == oSyncProject.Name)
                    {
                        tProject = tPossibleProject;
                        break;
                    }
                }
            }

            if (tProject == null)
            {
                if (!Options.JProjectCreate)
                {
                    Plugin.LogInfo("Failed to create Jira project '" + oSyncProject.Name + "'. Jira project creation is disabled.");
                    return null;
                }

                try
                {
                    String strKey = GetProjectKey(oSyncProject.Name);

                    if (m_tProjectCreationTemplate.permissionScheme == null)
                    {
                        RemotePermissionScheme[] aoSchemes = ServiceManager.getPermissionSchemes(RPCToken);

                        if (aoSchemes.Length > 0)
                        {
                            m_tProjectCreationTemplate.permissionScheme = aoSchemes[0];
                        }
                    }

                    tProject = ServiceManager.createProject(RPCToken, strKey, oSyncProject.Name, oSyncProject.Description,
                        "", m_tProjectCreationTemplate.lead, m_tProjectCreationTemplate.permissionScheme,
                        m_tProjectCreationTemplate.notificationScheme, m_tProjectCreationTemplate.issueSecurityScheme);
                }
                catch (Exception e)
                {
                    Plugin.LogError("Failed to create Jira project '" + oSyncProject.Name + "'. Exception (" + e.Message + "). Try creating a project with this name in Jira.");
                    tProject = null;
                }
            }

            if (tProject != null)
            {
                JProject oJProject = new JProject(this, tProject);

                oJProject.SyncID = oSyncProject.ID;

                return oJProject;
            }

            return null;
        }

        private String GetProjectKey(String strName)
        {
            String strKey;
            {
                StringBuilder strKeyB = new StringBuilder();

                for (int nIdx = 0; nIdx < strName.Length; nIdx++)
                {
                    if (char.IsLetter(strName[nIdx]))
                    {
                        strKeyB.Append(char.ToUpper(strName[nIdx]));
                    }
                }

                strKey = strKeyB.ToString();

                if (strKey.Length == 0)
                {
                    strKey = "PURECM";
                }
            }

            // Check the key is unique
            RemoteProject[] aoProjects = ServiceManager.getProjectsNoSchemes(RPCToken);
            Char cKeyIdx = 'A';
            String strKeyBase = strKey;

            while (true)
            {
                bool bUnique = true;

                foreach (RemoteProject oProject in aoProjects)
                {
                    if (oProject.key == strKey)
                    {
                        bUnique = false;
                        break;
                    }
                }

                if (bUnique)
                {
                    break;
                }
                else
                {
                    strKey = String.Format("{0}{1}", strKeyBase, cKeyIdx);
                    cKeyIdx = (Char)((int)cKeyIdx + 1);

                    if (!Char.IsLetter(cKeyIdx))
                    {
                        strKeyBase += '_';
                        cKeyIdx = 'A';
                    }
                }
            }

            return strKey;
        }

        /// <summary>
        /// Get all the Jira versions for this Jira project
        /// </summary>
        internal override List<ExVersion> GetVersions(ExProject oProject)
        {
            List<ExVersion> aoVersions = new List<ExVersion>();
            JProject oJProject = (JProject)oProject;
            RemoteVersion[] aoJVersions = ServiceManager.getVersions(RPCToken, oJProject.Project.key);

            foreach(RemoteVersion oJVersion in aoJVersions)
            {
                aoVersions.Add(GetVersion(oJProject,oJVersion));
            }

            return aoVersions;
        }

        /// <summary>
        /// Get the version from the Jira version ID. Return null if the version is not synchronized.
        /// </summary>
        internal JVersion GetVersion(int nProjectID, int nID)
        {
            if ( (nProjectID > 0 ) && (nID > 0) )
            {
                JProject oProject = GetProject(nProjectID);

                if (oProject != null)
                {
                    List<ExVersion> aoVersions = GetVersions(oProject);

                    foreach (ExVersion oVersion in aoVersions)
                    {
                        if (oVersion.ID == nID)
                        {
                            return oVersion as JVersion;
                        }
                    }

                    Plugin.LogError("Failed to get Jira version '" + nID + "'. A version with this id  does not exist in project '" + oProject.Name + "'.");
                }
                else
                {
                    Plugin.LogError("Failed to get Jira version '" + nID + "'. A Jira project with id '" + nProjectID + "' does not exist.");
                }
            }

            return null;
        }

        internal static JVersion GetVersion(JProject oProject, RemoteVersion oJVersion)
        {
            return new JVersion(oProject, oJVersion);
        }

        /// <summary>
        /// Get the version from the synchronized version. Return null if the version is not synchronized.
        /// </summary>
        internal override ExVersion GetVersion(ExVersion oSyncVersion)
        {
            ExProject oProject = oSyncVersion.Project.GetSyncProject(false);

            if (oProject != null)
            {
                UInt32 nID = oSyncVersion.SyncID;

                if (nID > 0)
                {
                    return GetVersion(Convert.ToInt32(oProject.ID), Convert.ToInt32(nID));
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
                Plugin.LogWarning("Failed to create Jira version '" + oSyncVersion.Name + "'. This version is flagged as not to be included.");
                return null;
            }

            JProject tProject = oSyncVersion.Project.GetSyncProject(true) as JProject;

            if (tProject != null)
            {
                RemoteVersion tVersion = null;

                // If a Jira version already exists with this name then use that
                {
                    List<ExVersion> atVersions = GetVersions(tProject);

                    foreach (ExVersion tPossibleVersion in atVersions)
                    {
                        if (tPossibleVersion.Name == oSyncVersion.Name)
                        {
                            return tPossibleVersion;
                        }
                    }
                }

                if (tVersion == null)
                {
                    int nSequence = 0;

                    if (oSyncVersion.Version != null)
                    {
                        ExVersion tParentVersion = oSyncVersion.Version.GetSyncVersion(true);

                        if (tParentVersion != null)
                        {
                            JVersion tJParentVersion = tParentVersion as JVersion;

                            if ( tJParentVersion != null )
                            {
                                nSequence = (int)tJParentVersion.RemoteVersion.sequence.Value + 1;
                            }
                        }
                        else
                        {
                            Plugin.LogWarning("Failed to set parent version for Jira version '" + oSyncVersion.Name + "'. Failed to get Jira parent version.");
                        }
                    }

                    try
                    {
                        tVersion = new RemoteVersion();

                        tVersion.name = oSyncVersion.Name;

                        if (nSequence > 0)
                        {
                            tVersion.sequence = nSequence;
                        }
                        
                        m_oServiceManager.addVersion( RPCToken, tProject.Project.key, tVersion);
                    }
                    catch (Exception e)
                    {
                        Plugin.LogError("Failed to create Jira version '" + oSyncVersion.Name + "'. Exception (" + e.Message + "). Try creating a version with this name in Jira.");
                        tProject = null;
                    }
                }

                if (tVersion != null)
                {
                    JVersion oJVersion = new JVersion(tProject as JProject, tVersion);

                    oJVersion.SyncID = oSyncVersion.ID;

                    return oJVersion;
                }
            }
            else
            {
                Plugin.LogInfo("Failed to create Jira version '" + oSyncVersion.Name + "'. Failed to get Jira project.");
            }

            return null;
        }

        /// <summary>
        /// Get the task from the synchronized task. Return null if the task is not synchronized.
        /// </summary>
        internal override ExTask GetTask(ExTask oSyncTask)
        {
            UInt32 nID = oSyncTask.SyncID;

            if (nID > 0)
            {
                RemoteIssue oJIssue = m_oServiceManager.getIssueById(RPCToken, nID.ToString());

                if (oJIssue != null)
                {
                    List<ExProject> aoProjects = GetProjects();

                    foreach (ExProject oProject in aoProjects)
                    {
                        JProject oJProject = oProject as JProject;

                        if (oJProject.Project.key == oJIssue.project)
                        {
                            return new JTask(oJProject, oJIssue);
                        }
                    }

                    Plugin.LogError("Failed to get Jira issue '" + oSyncTask.Name + "'. Failed to get project '" + oJIssue.project + "'.");
                }
            }

            return null;
        }

        internal ExTask CreateTask(ExProject tProject, RemoteIssue tIssue)
        {
            return new JTask(tProject as JProject, tIssue);
        }

        /// <summary>
        /// Create a new task and synchronize it with this task
        /// </summary>
        internal override ExTask CreateTask(ExTask oSyncTask)
        {
            Plugin.Trace("Creating Jira issue for PureCM task '" + oSyncTask.Name + "'.");

            if ( !m_oOptions.JTaskCreate)
            {
                Plugin.LogWarning("Failed to create Jira issue '" + oSyncTask.Name + "'. Task creation is disabled.");
                return null;
            }

            JProject tProject = null;
            {
                ExProject tExProject = oSyncTask.Project.GetSyncProject(true);

                if (tExProject != null)
                {
                    tProject = tExProject as JProject;
                }
            }

            if (tProject != null)
            {
                RemoteIssue tIssue = new RemoteIssue();

                tIssue.project = tProject.Project.key;
                tIssue.summary = oSyncTask.Name;
                tIssue.description = oSyncTask.Description;
                tIssue.priority = GetPriorityFromPCMPriority(oSyncTask.Priority).id;

                if (oSyncTask.IsFeature() && Options.JFeatureCreate)
                {
                    tIssue.type = FeatureCreationType.id;
                }
                else
                {
                    tIssue.type = m_tTaskCreationType.id;
                }

                if (oSyncTask.Version != null)
                {
                    JVersion tParentVersion = oSyncTask.Version.GetSyncVersion(true) as JVersion;

                    if (tParentVersion != null)
                    {
                        tIssue.fixVersions = new RemoteVersion[1];
                        tIssue.fixVersions[0] = tParentVersion.RemoteVersion;
                    }
                    else
                    {
                        Plugin.LogWarning("Failed to set fixed in version for Jira issue '" + oSyncTask.Name + "'. Failed to get Jira issue parent version.");
                    }
                }

                tIssue = ServiceManager.createIssue(RPCToken, tIssue);

                ExTask oTask = CreateTask(tProject, tIssue);

                oTask.SyncID = oSyncTask.ID;
                oSyncTask.Url = oTask.Url;

                return oTask;
            }
            else
            {
                Plugin.LogInfo("Failed to create Jira issue '" + oSyncTask.Name + "'. Failed to get Jira project.");
            }

            return null;
        }

        /// <summary>
        /// Get the user from the Jira username. Return null if the user is not synchronized.
        /// </summary>
        internal JUser GetUser(string strName)
        {
            if (strName.Length > 0)
            {
                RemoteUser oJUser = ServiceManager.getUser(RPCToken, strName);

                if ((oJUser != null) && (oJUser.name != null) && (oJUser.name.Length > 0))
                {
                    return new JUser(this, oJUser);
                }
            }

            return null;
        }

        /// <summary>
        /// Get the user from the synchronized user. Return null if the user is not synchronized.
        /// </summary>
        internal override ExUser GetUser(ExUser oSyncUser)
        {
            return GetUser(GetJiraUserName(oSyncUser.Name.ToLower()));
        }

        private string GetJiraUserName(string strPureCMName)
        {
            String strJiraName;

            if (m_oOptions.PUsers.TryGetValue(strPureCMName, out strJiraName))
            {
                return strJiraName.ToLower();
            }

            return strPureCMName;
        }

        /// <summary>
        /// Create a new user and synchronize it with this user
        /// </summary>
        internal override ExUser CreateUser(ExUser oSyncUser)
        {
            RemoteUser tUser = ServiceManager.getUser(RPCToken, oSyncUser.Name);

            if (tUser == null)
            {
                String strName = oSyncUser.Name.ToLower();
                String strEmail = oSyncUser.EmailAddress;

                if (strEmail.Length == 0)
                {
                    strEmail = "developer@mycompany.com";
                }
                tUser = ServiceManager.createUser(RPCToken, strName, "secret", oSyncUser.Name, strEmail);
            }

            if (tUser != null)
            {
                JUser oJUser = new JUser(this, tUser);

                oJUser.SyncID = oSyncUser.ID;

                return oJUser;
            }
            else
            {
                Plugin.LogWarning("Failed to create Jira user '" + oSyncUser.Name + "'. Do you have enough Jira licenses?");
                return null;
            }
        }

        private Dictionary<int, SDK.TStreamDataState> m_aoStreamDataStates;

        /// <summary>
        /// Get the stream data state from a status id
        /// </summary>
        internal SDK.TStreamDataState GetStreamDataState(RemoteIssue oIssue)
        {
            if (m_aoStreamDataStates == null)
            {
                m_aoStreamDataStates = new Dictionary<int, SDK.TStreamDataState>();

                RemoteStatus[] aoStatuses = m_oServiceManager.getStatuses(RPCToken);

                foreach (RemoteStatus oStatus in aoStatuses)
                {
                    m_aoStreamDataStates.Add(int.Parse(oStatus.id), GetStreamDataState(oStatus.name));
                }
            }

            SDK.TStreamDataState tRet;

            if (m_aoStreamDataStates.TryGetValue(int.Parse(oIssue.status), out tRet))
            {
                return tRet;
            }
            else
            {
                Plugin.LogWarning("Failed to get Jira status with id '" + oIssue.status + "'.");
                return SDK.TStreamDataState.Open;
            }
        }

        /// <summary>
        /// Get the stream data state from a status name
        /// </summary>
        private SDK.TStreamDataState GetStreamDataState(string strName)
        {
            SDK.TStreamDataState tState;

            if (m_oOptions.JStates.TryGetValue(strName.ToUpper(),out tState))
            {
                return tState;
            }
            else
            {
                Plugin.LogWarning("Failed to get PureCM status for Jira status '" + strName + "'.");
                return SDK.TStreamDataState.Open;
            }
        }

        internal bool DoesJiraActionMatchState(String strActionName, SDK.TStreamDataState tState)
        {
            SDK.TStreamDataState tActionState;

            if (m_oOptions.JTransitions.TryGetValue(strActionName.ToUpper(), out tActionState))
            {
                return (tActionState == tState);
            }

            return false;
        }

        private List<UInt32> m_anChildIssueTypeIds;

        internal bool IsIssueTypeChildType(UInt32 nTypeId)
        {
            if (m_anChildIssueTypeIds == null)
            {
                m_anChildIssueTypeIds = new List<UInt32>();

                RemoteIssueType[] aoChildTypes = m_oServiceManager.getSubTaskIssueTypes(RPCToken);

                foreach(RemoteIssueType oChild in aoChildTypes)
                {
                    m_anChildIssueTypeIds.Add(UInt32.Parse(oChild.id));
                }
            }

            return (m_anChildIssueTypeIds.IndexOf(nTypeId) >= 0);
        }
    }
}
