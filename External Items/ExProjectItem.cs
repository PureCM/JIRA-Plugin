using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace External_Items
{
    /// <summary>
    /// This is the base class for all project items
    /// </summary>
    internal abstract class ExProjectItem : ExItem
    {
        internal ExProjectItem(ExProject oExProject, ExFactory.TPluginType tType, UInt32 nExID)
            : base(oExProject.Factory, tType, nExID)
        {
            m_oProject = oExProject;
        }

        /// <summary>
        /// What project does this project item belong too?
        /// </summary>
        internal ExProject Project { get { return m_oProject; } set { m_oProject = value; }  }

        /// <summary>
        /// Who owns this project item?
        /// </summary>
        internal ExUser Owner
        {
            get
            {
                if (m_oOwner == null)
                {
                    m_oOwner = GetOwner();
                }

                return m_oOwner;
            }
        }

        /// <summary>
        /// What version does this project item belong too?
        /// </summary>
        internal ExVersion Version
        {
            get
            {
                if (m_oParentVersion == null)
                {
                    m_oParentVersion = GetVersion();
                }

                return m_oParentVersion;
            }
        }

        private ExProject m_oProject;
        private ExUser m_oOwner;
        private ExVersion m_oParentVersion;

        /// <summary>
        /// Who owns this project item?
        /// </summary>
        internal abstract ExUser GetOwner();

        /// <summary>
        /// What version does this project item belong too?
        /// </summary>
        internal abstract ExVersion GetVersion();
    }
}
