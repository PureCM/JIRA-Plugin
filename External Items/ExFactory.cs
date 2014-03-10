using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace External_Items
{
    internal abstract class ExFactory
    {
        internal enum TPluginType
        {
            GeminiProject = 1,
            GeminiVersion,
            GeminiTask,
            GeminiUser,
            JiraProject,
            JiraVersion,
            JiraTask,
            JiraUser
        }

        internal enum TPluginSyncType
        {
            PureCMGeminiSyncTimeYear = 1000,
            PureCMGeminiSyncTimeMonth,
            PureCMGeminiSyncTimeDay,
            PureCMGeminiSyncTimeHour,
            PureCMGeminiSyncTimeMinute,
            PureCMGeminiSyncTimeSecond,
            GeminiSyncTimeYear,
            GeminiSyncTimeMonth,
            GeminiSyncTimeDay,
            GeminiSyncTimeHour,
            GeminiSyncTimeMinute,
            GeminiSyncTimeSecond,
            GeminiSyncTimeTaskYear,
            GeminiSyncTimeTaskMonth,
            GeminiSyncTimeTaskDay,
            GeminiSyncTimeTaskHour,
            GeminiSyncTimeTaskMinute,
            GeminiSyncTimeTaskSecond,
            JiraSyncTimeYear,
            JiraSyncTimeMonth,
            JiraSyncTimeDay,
            JiraSyncTimeHour,
            JiraSyncTimeMinute,
            JiraSyncTimeSecond,
            JiraSyncTimeTaskYear,
            JiraSyncTimeTaskMonth,
            JiraSyncTimeTaskDay,
            JiraSyncTimeTaskHour,
            JiraSyncTimeTaskMinute,
            JiraSyncTimeTaskSecond,
            PureCMJiraSyncTimeYear,
            PureCMJiraSyncTimeMonth,
            PureCMJiraSyncTimeDay,
            PureCMJiraSyncTimeHour,
            PureCMJiraSyncTimeMinute,
            PureCMJiraSyncTimeSecond
        };

        internal ExFactory(String strPluginName, PureCM.Server.Plugin oPlugin)
        {
            PluginName = strPluginName;
            Plugin = oPlugin;
        }

        internal String PluginName { get; set; }
        internal ExFactory SyncFactory { get; set; }
        internal PureCM.Server.Plugin Plugin { get; set; }

        internal abstract List<ExProject> GetProjects();

        internal abstract UInt32 GetSyncID( TPluginType tType, UInt32 nID );
        internal abstract void SetSyncID( TPluginType tType, UInt32 nID, UInt32 nSyncID );

        internal abstract ExProject GetProject(ExProject oSyncProject);
        internal abstract ExProject CreateProject(ExProject oSyncProject);

        internal abstract List<ExVersion> GetVersions(ExProject oProject);
        internal abstract ExVersion GetVersion(ExVersion oSyncVersion);
        internal abstract ExVersion CreateVersion(ExVersion oSyncVersion);

        internal abstract ExTask GetTask(ExTask oSyncTask);
        internal abstract ExTask CreateTask(ExTask oSyncTask);

        internal abstract ExUser GetUser(ExUser oSyncUser);
        internal abstract ExUser CreateUser(ExUser oSyncUser);
    }
}
