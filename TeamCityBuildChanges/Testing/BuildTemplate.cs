using System.Collections.Generic;

namespace TeamCityBuildChanges.Testing
{
    public class BuildTemplate
    {
        public string BuildId;
        public string BuildName;
        public int BuildCount;
        public string BuildNumberPattern;
        public int StartBuildNumber;
        public int FinishBuildNumber;
        public int IssueCount;
        public int NestedIssueChance;
        public int NestedIssueDepth;
        public Dictionary<string, string> StartBuildPackages;
        public Dictionary<string, string> FinishBuildPackages;
    }
}