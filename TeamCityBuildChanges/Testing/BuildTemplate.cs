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
        public bool CreateNuGetPackageChangeManifests;
        public Dictionary<string, string> StartBuildPackages = new Dictionary<string, string>();
        public Dictionary<string, string> FinishBuildPackages = new Dictionary<string, string>();
    }
}