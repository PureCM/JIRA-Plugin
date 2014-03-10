using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using External_Items;
using Jira_Items;

namespace Jira_Items
{
    /// <summary>
    /// A class for monitoring the Jira system to check for changes and synchronize with PureCM.
    /// </summary>
    internal class JMonitor : ExMonitor
    {
        internal JMonitor(JFactory oFactory, bool bForceSync)
            : base(oFactory, bForceSync)
        {
        }
    }
}
