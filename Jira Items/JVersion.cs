using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PureCM.Client;

using External_Items;

namespace Jira_Items
{
    /// <summary>
    /// Wrapper class for a Jira version.
    /// </summary>
    internal class JVersion : ExVersion
    {
        internal JVersion(JProject oProject, RemoteVersion oJVersion)
            : base(oProject, ExFactory.TPluginType.JiraVersion, System.Convert.ToUInt32(oJVersion.id))
        {
            m_oJVersion = oJVersion;
        }

        private JFactory JFactory { get { return Factory as JFactory; } }

        private RemoteVersion m_oJVersion;

        internal RemoteVersion RemoteVersion { get { return m_oJVersion; } }

        /// <summary>
        // Returns the Jira name of the version.
        /// </summary>
        internal override String GetName()
        {
            return m_oJVersion.name;
        }

        /// <summary>
        // Returns the Jira description of the version.
        /// </summary>
        internal override String GetDescription()
        {
            return "";
        }

        /// <summary>
        /// Who owns this Jira version?
        /// </summary>
        internal override ExUser GetOwner()
        {
            return null;
        }

        /// <summary>
        /// What is the parent version for this version?
        /// </summary>
        internal override ExVersion GetVersion()
        {
            if (m_oJVersion.sequence > 1)
            {
                JProject oProject = Project as JProject;

                RemoteVersion[] aoVersions = JFactory.ServiceManager.getVersions(JFactory.RPCToken, oProject.Project.key);

                foreach (RemoteVersion oVersion in aoVersions)
                {
                    if (oVersion.sequence == (m_oJVersion.sequence - 1))
                    {
                        return JFactory.GetVersion(oProject, oVersion);
                    }
                }
            }

            return null;
        }
    }
}
