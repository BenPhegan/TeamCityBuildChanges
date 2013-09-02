using System.Collections.Generic;
using TeamCityBuildChanges.ExternalApi.TeamCity;

namespace TeamCityBuildChanges.IssueDetailResolvers
{
    public interface IIssueDetailResolver
    {
        IEnumerable<ExternalIssueDetails> GetExternalIssueDetails(IEnumerable<Issue> issues);
        IEnumerable<Issue> GetAssociatedIssues(IEnumerable<ChangeDetail> changeDetails);
    }
}