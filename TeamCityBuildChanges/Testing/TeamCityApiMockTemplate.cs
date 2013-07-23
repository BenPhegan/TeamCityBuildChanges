using System.Collections.Generic;

namespace TeamCityBuildChanges.Testing
{
    public class TeamCityApiMockTemplate
    {
        public string TfsConnection;
        public string Id;
        public List<int> RelatedIssueIds = new List<int>(); 
    }
}