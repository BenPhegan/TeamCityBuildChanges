using System.Collections.Generic;
using TeamCityBuildChanges.ExternalApi.Jira;
using TeamCityBuildChanges.ExternalApi.TeamCity;

namespace TeamCityBuildChanges.IssueDetailResolvers
{
    public class JiraIssueResolver : IExternalIssueResolver
    {
        private readonly JiraApi _api;

        public JiraIssueResolver(JiraApi api)
        {
            _api = api;
        }

        #region IExternalIssueResolver Members

        public IEnumerable<ExternalIssueDetails> GetDetails(IEnumerable<Issue> issues)
        {
            var results = new List<ExternalIssueDetails>();
            foreach (var issue in issues)
            {
                var jiraDetails = _api.GetJiraIssue(issue.Id);
                if (jiraDetails != null && jiraDetails.Id != null && jiraDetails.Fields != null)
                {
                    results.Add(new ExternalIssueDetails
                        {
                            Id = jiraDetails.Key,
                            Status = jiraDetails.Fields.Status.Name,
                            Created = jiraDetails.Fields.Created,
                            Summary = jiraDetails.Fields.Summary,
                            Description = jiraDetails.Fields.Description,
                            Url = jiraDetails.Self
                        });
                }
            }
            return results;
        }

        #endregion

    }
}