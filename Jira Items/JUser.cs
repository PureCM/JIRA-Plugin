using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PureCM.Client;

using External_Items;

namespace Jira_Items
{
    /// <summary>
    /// A Jira user.
    /// </summary>
    internal class JUser : ExUser
    {
        internal JUser(JFactory oGFactory, RemoteUser oJUser)
            : base(oGFactory, ExFactory.TPluginType.JiraUser, (UInt32)oJUser.name.GetHashCode())
        {
            m_oJUser = oJUser;
        }

        private RemoteUser m_oJUser;

        /// <summary>
        /// The Jira user object
        /// </summary>
        internal RemoteUser User
        {
            get { return m_oJUser; }
        }

        /// <summary>
        /// Return the email address for this user
        /// </summary>
        internal override String GetEmailAddress()
        {
            return m_oJUser.email;
        }

        /// <summary>
        /// Return the Jira short name for the user
        /// </summary>
        internal override String GetName()
        {
            return m_oJUser.name;
        }

        /// <summary>
        /// Return the Jira long description for the user
        /// </summary>
        internal override String GetDescription()
        {
            return m_oJUser.fullname;
        }
    }
}
