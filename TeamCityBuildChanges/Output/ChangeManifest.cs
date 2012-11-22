using System;
using System.Collections.Generic;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;

namespace TeamCityBuildChanges.Output
{
    public class ChangeManifest
    {
        public List<ChangeDetail> ChangeDetails { get; set; }
        public List<ExternalIssueDetails> IssueDetails { get; set; }
        public DateTime Generated { get; set; }
        public ChangeManifest()
        {
            ChangeDetails = new List<ChangeDetail>();
            IssueDetails = new List<ExternalIssueDetails>();
        }
}
}