using System.Collections.Generic;
using System.Linq;
using TeamCityBuildChanges.ExternalApi.TeamCity;

namespace TeamCityBuildChanges.IssueDetailResolvers
{
    public class IssueDetailResolver
    {
        private readonly List<IExternalIssueResolver> _issueResolvers = new List<IExternalIssueResolver>();

        public IssueDetailResolver(IEnumerable<IExternalIssueResolver> issueResolvers)
        {
            _issueResolvers = issueResolvers.ToList();
        }

        public IEnumerable<ExternalIssueDetails> GetExternalIssueDetails(IEnumerable<Issue> issues)
        {
            var details = new List<ExternalIssueDetails>();
            if (issues != null)
            {
                var issueList = issues as List<Issue> ?? issues.ToList();
                foreach (var resolver in _issueResolvers)
                {
                    details.AddRange(resolver.GetDetails(issueList));
                }
            }
            return details;
        }

        public IEnumerable<Issue> GetAssociatedIssues(IEnumerable<ChangeDetail> changeDetails)
        {
            var details = new List<Issue>();
            if (changeDetails != null)
            {
                var issueList = changeDetails as List<ChangeDetail> ?? changeDetails.ToList();
                foreach (var resolver in _issueResolvers)
                {
                    details.AddRange(resolver.GetIssues(issueList));
                }
            }
            return details;
        }
    }
}