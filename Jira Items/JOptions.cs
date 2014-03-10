using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;

using PureCM.Client;

namespace Jira_Items
{
    class JOptions
    {
        public JOptions(XElement oConfig)
        {
            JURL = oConfig.Element("JiraURL").Value;
            JUser = oConfig.Element("JiraUser").Value;
            JPassword = oConfig.Element("JiraPassword").Value;
            PRepository = oConfig.Element("PureCMRepository").Value;

            if (oConfig.Element("Interval") != null)
            {
                Interval = UInt32.Parse(oConfig.Element("Interval").Value);
            }
            else
            {
                Interval = 60;
            }

            XElement tProjectCreationElement = oConfig.Element("ProjectCreation");

            if (tProjectCreationElement != null)
            {
                if (tProjectCreationElement.Element("Enabled") != null)
                {
                    JProjectCreate = tProjectCreationElement.Element("Enabled").Value.ToUpper() == "TRUE";
                }

                if (JProjectCreate)
                {
                    if (tProjectCreationElement.Element("TemplateProject") != null)
                    {
                        JProjectCreateTemplate = tProjectCreationElement.Element("TemplateProject").Value;
                    }
                    else
                    {
                        JProjectCreate = false;
                    }
                }
            }

            XElement tTaskCreationElement = oConfig.Element("TaskCreation");

            if (tTaskCreationElement != null)
            {
                if (tTaskCreationElement.Element("Enabled") != null)
                {
                    JTaskCreate = tTaskCreationElement.Element("Enabled").Value.ToUpper() == "TRUE";
                }

                if (JTaskCreate)
                {
                    if (tTaskCreationElement.Element("CreationType") != null)
                    {
                        JTaskCreateTypeDescription = tTaskCreationElement.Element("CreationType").Value;
                    }
                    else
                    {
                        JTaskCreate = false;
                    }
                }
            }

            XElement tFeatureCreationElement = oConfig.Element("FeatureCreation");

            if (tFeatureCreationElement != null)
            {
                if (tFeatureCreationElement.Element("Enabled") != null)
                {
                    JFeatureCreate = tFeatureCreationElement.Element("Enabled").Value.ToUpper() == "TRUE";
                }

                if (JFeatureCreate)
                {
                    if (tFeatureCreationElement.Element("CreationType") != null)
                    {
                        JFeatureCreateTypeDescription = tFeatureCreationElement.Element("CreationType").Value;
                    }
                    else
                    {
                        JFeatureCreate = false;
                    }
                }
            }

            if (oConfig.Element("UpdateURL") != null)
            {
                UpdateURL = oConfig.Element("UpdateURL").Value.ToUpper() == "TRUE";
            }
            else
            {
                UpdateURL = true;
            }

            if (oConfig.Element("ForceJiraSync") != null)
            {
                ForceJiraSync = oConfig.Element("ForceJiraSync").Value.ToUpper() == "TRUE";
            }

            if (oConfig.Element("ForcePureCMSync") != null)
            {
                ForcePureCMSync = oConfig.Element("ForcePureCMSync").Value.ToUpper() == "TRUE";
            }

            if (oConfig.Element("DelayOnStart") != null)
            {
                DelayOnStart = oConfig.Element("DelayOnStart").Value.ToUpper() == "TRUE";
            }

            {
                XElement tMappings = oConfig.Element("StatusMappings");

                if (tMappings != null)
                {
                    foreach (XElement tMapping in tMappings.Elements("StatusMapping"))
                    {
                        string strPureCMState = tMapping.Attribute("PureCMState").Value.ToUpper();
                        SDK.TStreamDataState tPureCMState = GetPureCMState(strPureCMState);

                        foreach (XElement tStatus in tMapping.Element("JiraStatuses").Elements("JiraStatus"))
                        {
                            JStates.Add(tStatus.Value.ToUpper(), tPureCMState);
                        }

                        foreach (XElement tTransition in tMapping.Element("JiraTransitions").Elements("JiraTransition"))
                        {
                            JTransitions.Add(tTransition.Value.ToUpper(), tPureCMState);
                        }
                    }
                }
            }

            {
                XElement tMappings = oConfig.Element("UserMappings");

                if (tMappings != null)
                {
                    foreach (XElement tMapping in tMappings.Elements("UserMapping"))
                    {
                        PUsers.Add(tMapping.Element("PureCMUser").Value.ToLower(), tMapping.Element("JiraUser").Value.ToLower());
                    }
                }
            }
        }

        static private SDK.TStreamDataState GetPureCMState(String strState)
        {
            if (strState == "OPEN")
            {
                return SDK.TStreamDataState.Open;
            }
            else if (strState == "COMPLETED")
            {
                return SDK.TStreamDataState.Completed;
            }
            else if (strState == "CLOSED")
            {
                return SDK.TStreamDataState.Closed;
            }
            else if (strState == "REJECTED")
            {
                return SDK.TStreamDataState.Rejected;
            }
            else
            {
                throw new Exception(strState + " is not a valid PureCM state. Must be Open, Completed, Closed or Rejected.");
            }
        }

        /// <summary>
        /// The Jira URL
        /// </summary>
        public String JURL { get; set; }

        /// <summary>
        /// The Jira User
        /// </summary>
        public String JUser { get; set; }

        /// <summary>
        /// The Jira User Password
        /// </summary>
        public String JPassword { get; set; }

        /// <summary>
        /// The PureCM repository name
        /// </summary>
        public String PRepository { get; set; }

        /// <summary>
        /// The interval in seconds between each Jira-PureCM synchronization
        /// </summary>
        public uint Interval { get; set; }

        /// <summary>
        /// Whether to create Jira projects from PureCM projects
        /// </summary>
        public bool JProjectCreate { get; set; }

        /// <summary>
        /// Whether to create Jira projects from PureCM projects
        /// </summary>
        public string JProjectCreateTemplate { get; set; }

        /// <summary>
        /// Whether to create Jira issues from PureCM tasks
        /// </summary>
        public bool JTaskCreate { get; set; }

        /// <summary>
        /// The creation type for Jira issues
        /// </summary>
        public string JTaskCreateTypeDescription { get; set; }

        /// <summary>
        /// The creation status for Jira issues
        /// </summary>
        public string JTaskCreateStatusDescription { get; set; }

        /// <summary>
        /// The creation severity for Jira issues
        /// </summary>
        public string JTaskCreatePriorityDescription { get; set; }

        /// <summary>
        /// Whether to create Jira issues from PureCM tasks
        /// </summary>
        public bool JFeatureCreate { get; set; }

        /// <summary>
        /// The creation type for Jira issues
        /// </summary>
        public string JFeatureCreateTypeDescription { get; set; }

        /// <summary>
        /// The creation status for Jira issues
        /// </summary>
        public string JFeatureCreateStatusDescription { get; set; }

        /// <summary>
        /// The creation severity for Jira issues
        /// </summary>
        public string JFeatureCreatePriorityDescription { get; set; }

        /// <summary>
        /// Whether to update the PureCM task URLs to use the Jira issue URLs
        /// </summary>
        public bool UpdateURL { get; set; }

        /// <summary>
        /// Each Jira status and the corresponding PureCM state
        /// </summary>
        public Dictionary<String, SDK.TStreamDataState> JStates = new Dictionary<String, SDK.TStreamDataState>();

        /// <summary>
        /// Each Jira transition and the corresponding PureCM state
        /// </summary>
        public Dictionary<String, SDK.TStreamDataState> JTransitions = new Dictionary<String, SDK.TStreamDataState>();

        /// <summary>
        /// Each PureCM user with a corresponding Jira user
        /// </summary>
        public Dictionary<String, String> PUsers = new Dictionary<String, String>();

        /// <summary>
        /// Whether we force all Jira issues to be synchronized on startup
        /// </summary>
        public bool ForceJiraSync { get; set; }

        /// <summary>
        /// Whether we force all PureCM tasks to be synchronized on startup
        /// </summary>
        public bool ForcePureCMSync { get; set; }

        /// <summary>
        /// Whether to delay the call to OnStart (useful to attach the debugger)
        /// </summary>
        public bool DelayOnStart { get; set; }
    }
}
