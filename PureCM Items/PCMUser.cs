using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PureCM.Client;

using External_Items;

namespace PureCM_Items
{
    /// <summary>
    /// A wrapper class for a PureCM user.
    /// </summary>
    internal class PCMUser : ExUser
    {
        internal PCMUser(PCMFactory oPFactory, UserOrGroup oPUser)
            : base(oPFactory, oPFactory.PluginProjectType + 3, oPUser.Id)
        {
            m_oPUser = oPUser;
        }

        internal UserOrGroup UserOrGroup { get { return m_oPUser; } }

        private UserOrGroup m_oPUser;

        /// <summary>
        /// Return the email address for this user
        /// </summary>
        internal override String GetEmailAddress()
        {
            return m_oPUser.Email;
        }

        /// <summary>
        /// Return the Gemini short name for the user
        /// </summary>
        internal override String GetName()
        {
            return m_oPUser.Name;
        }

        /// <summary>
        /// Return the Gemini long description for the user
        /// </summary>
        internal override String GetDescription()
        {
            return m_oPUser.Desc;
        }
    }
}
