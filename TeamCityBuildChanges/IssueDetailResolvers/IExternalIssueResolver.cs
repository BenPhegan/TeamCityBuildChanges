using System.Collections.Generic;
using TeamCityBuildChanges.ExternalApi.TeamCity;

namespace TeamCityBuildChanges.IssueDetailResolvers
{
    public interface IExternalIssueResolver
    {
        IEnumerable<ExternalIssueDetails> GetDetails(IEnumerable<Issue> issues);
        IEnumerable<Issue> GetIssues(IEnumerable<ChangeDetail> changeDetails);
    }
}