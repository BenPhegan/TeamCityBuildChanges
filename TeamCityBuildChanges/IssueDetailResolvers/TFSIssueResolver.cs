using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeamCityBuildChanges.ExternalApi.TeamCity;

namespace TeamCityBuildChanges.IssueDetailResolvers
{
    public class TFSIssueResolver : IExternalIssueResolver
    {
        public IEnumerable<ExternalIssueDetails> GetDetails(IEnumerable<Issue> issue)
        {
            throw new NotImplementedException();
        }
    }
}
