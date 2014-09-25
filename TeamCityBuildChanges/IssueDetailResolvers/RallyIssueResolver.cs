using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TeamCityBuildChanges.ExternalApi.Rally;
using TeamCityBuildChanges.ExternalApi.TeamCity;

namespace TeamCityBuildChanges.IssueDetailResolvers
{
    public class RallyIssueResolver : IExternalIssueResolver
    {
        private readonly IRallyApi _rallyApi;
        private readonly Regex _defectIssueRegex;
        private readonly Regex _userStoryIssueRegex;

        public RallyIssueResolver(IRallyApi rallyApi)
        {
            _rallyApi = rallyApi;
            _defectIssueRegex = new Regex(@"(^|\W)(?<Id>DE[0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _userStoryIssueRegex = new Regex(@"(^|\W)(?<Id>US[0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public IEnumerable<ExternalIssueDetails> GetDetails(IEnumerable<Issue> issues)
        {
            var artifacts = new List<ExternalApi.Rally.Artifact>();

            foreach (var issue in issues)
            {
                if (_defectIssueRegex.IsMatch(issue.Id))
                    artifacts.Add(_rallyApi.GetRallyDefect(issue.Id));

                if (_userStoryIssueRegex.IsMatch(issue.Id))
                    artifacts.Add(_rallyApi.GetRallyUserStory(issue.Id));
            }

            var result = artifacts.Where(a => a != null).Select(a => new ExternalIssueDetails
                {
                    Id = a.FormattedId,
                    Created = a.Created.ToString("yyyy-MM-dd"),
                    Type = a.GetType().Name,
                    Status = a.State,
                    Summary = a.Name,
                    Description = a.Description,
                    Url = a.Url
                }).ToArray();

            return result;
        }

        public IEnumerable<Issue> GetIssues(IEnumerable<ChangeDetail> changeDetails)
        {
            var issues = new List<Issue>();
            foreach (var change in changeDetails)
            {
                var defectIssues = _defectIssueRegex.Matches(change.Comment)
                       .Cast<Match>()
                       .Select(x => new Issue { Id = x.Groups["Id"].Value })
                       .ToList();

                issues.AddRange(defectIssues);
            }
            var distinct = issues.Distinct(new IssueEqualityComparer());
            return distinct;
        }
    }
}