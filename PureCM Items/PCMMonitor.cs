using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using External_Items;

namespace PureCM_Items
{
    /// <summary>
    /// A class for monitoring PureCM to check for changes and synchronize with the external tool.
    /// </summary>
    internal class PCMMonitor : ExMonitor
    {
        internal PCMMonitor(PCMFactory oFactory, bool bForceSync)
            : base(oFactory, bForceSync)
        {
        }
    }
}
