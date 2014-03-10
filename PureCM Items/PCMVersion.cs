using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PureCM.Client;

using External_Items;

namespace PureCM_Items
{
    /// <summary>
    /// A wrapper class for a PureCM version.
    /// </summary>
    internal class PCMVersion : ExVersion
    {
        internal PCMVersion(PCMProject oProject, PureCM.Client.Version oVersion)
            : base(oProject, oProject.PCMFactory.PluginProjectType + 1, oVersion.Id)
        {
            m_oVersion = oVersion;
        }

        internal PureCM.Client.Version PureCMObject { get { return m_oVersion; } }

        private PCMProject PCMProject { get { return Project as PCMProject; } }
        private PCMFactory PCMFactory { get { return Factory as PCMFactory; } }

        private PureCM.Client.Version m_oVersion;

        /// <summary>
        /// Should this version be synchronized with the other system?
        /// </summary>
        internal override bool ShouldInclude()
        {
            return m_oVersion.State == SDK.TStreamDataState.Open;
        }

        /// <summary>
        // Returns the name of the version.
        /// </summary>
        internal override String GetName()
        {
            return m_oVersion.Name;
        }

        /// <summary>
        // Returns the description of the version.
        /// </summary>
        internal override String GetDescription()
        {
            return m_oVersion.Description;
        }

        /// <summary>
        /// Who owns this PureCM version?
        /// </summary>
        internal override ExUser GetOwner()
        {
            if (m_oVersion.Owner != null)
            {
                return PCMFactory.GetUser(m_oVersion.Owner);
            }

            return null;
        }

        /// <summary>
        /// What is the parent version for this version?
        /// </summary>
        internal override ExVersion GetVersion()
        {
            PureCM.Client.Version oParentVersion = m_oVersion.Parent as PureCM.Client.Version;

            if (oParentVersion != null)
            {
                return PCMFactory.GetVersion(PCMProject, oParentVersion);
            }

            return null;
        }
    }
}
