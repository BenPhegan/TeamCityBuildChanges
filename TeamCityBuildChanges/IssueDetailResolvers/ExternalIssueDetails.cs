using System;
using System.Collections.Generic;

namespace TeamCityBuildChanges.IssueDetailResolvers
{
    public class ExternalIssueDetails
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string Created { get; set; }
        public List<string> Comments { get; set; }
        public List<ExternalIssueDetails> SubIssues { get; set; }
    }
}