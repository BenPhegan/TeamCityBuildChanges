using System.Collections.Generic;
using System.Text.RegularExpressions;
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
                            Type = jiraDetails.Fields.Issuetype.Name,
                            Created = jiraDetails.Fields.Created,
                            Summary = jiraDetails.Fields.Summary,
                            Description = jiraDetails.Fields.Description,
                            Url = jiraDetails.Self
                        });
                }
            }
            return results;
        }

        public IEnumerable<Issue> GetIssues(IEnumerable<ChangeDetail> changeDetails)
        {
            var issues = new List<Issue>();
            foreach (var change in changeDetails)
            {
                var regex = new Regex("/[A-Z]*-[0-9]*");
                var issue = new Issue()
                    {
                        Id = regex.Match(change.Comment).Groups[0].Captures[0].ToString()
                    };
                //parse the change with a regex and add the result to a list
            }
            return issues;
        }
    }
}