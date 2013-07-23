using System.Collections.Generic;
using System.Text.RegularExpressions;
using TeamCityBuildChanges.ExternalApi.Jira;
using TeamCityBuildChanges.ExternalApi.TeamCity;

namespace TeamCityBuildChanges.IssueDetailResolvers
{
    using System.Linq;
    using Castle.Core;

    public class JiraIssueResolver : IExternalIssueResolver
    {
        private readonly IJiraApi _api;

        public JiraIssueResolver(IJiraApi api)
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
                var changeIssues = Regex.Matches(change.Comment, @"[A-Z]*-[0-9]*", RegexOptions.IgnoreCase)
                       .Cast<Match>()
                       .Select(x => new Issue { Id = x.Value })
                       .Where(x => x.Id != "-") // need a better regex but I'm crap at that
                       .ToList();
                issues.AddRange(changeIssues);
            }
            var distinct = issues.Distinct(new IssueEqualityComparer());
            return distinct;
        }

    }
}