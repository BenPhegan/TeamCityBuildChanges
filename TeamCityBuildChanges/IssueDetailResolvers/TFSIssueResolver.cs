using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.ExternalApi.TFS;

namespace TeamCityBuildChanges.IssueDetailResolvers
{
    public class TFSIssueResolver : IExternalIssueResolver
    {
        readonly ITfsApi _tfsApi;

        public TFSIssueResolver(ITfsApi tfsApi)
        {
            _tfsApi = tfsApi;
        }

        public IEnumerable<ExternalIssueDetails> GetDetails(IEnumerable<Issue> issues)
        {
            int ignoredOut;
            var tfsIssues = issues.Where(i => IsTfsUrl(i.Url) && int.TryParse(i.Id, out ignoredOut)).ToList();
            var queriedIssues = new ConcurrentBag<ExternalIssueDetails>();
            Parallel.ForEach(tfsIssues, issue => queriedIssues.Add(GetDetails(issue)));

            return queriedIssues;
        }

        public IEnumerable<Issue> GetIssues(IEnumerable<ChangeDetail> changeDetails)
        {
            var issues = new List<Issue>();
            foreach (
                var workItems in
                    changeDetails.Select(
                        changeDetail =>
                            {
                                int commit;
                                return Int32.TryParse(changeDetail.Version, out commit).Equals(true)
                                           ? _tfsApi.GetWorkItemsByCommit(commit)
                                           : Enumerable.Empty<TfsWorkItem>();
                            }))
            {
                issues.AddRange(workItems.Select(GetIssueFromTfsWorkItem));
            }
            return issues;
        }

        private ExternalIssueDetails GetDetails(Issue issue)
        {            
            var tfsWi = _tfsApi.GetWorkItem(ParseTfsWorkItemId(issue.Id));
            var extIssue = GetDetails(tfsWi);
            extIssue.Url = _tfsApi.ConnectionUri + "/web/wi.aspx?id=" + extIssue.Id;

            while (tfsWi.ParentId.HasValue)
            {
                var parentWi = _tfsApi.GetWorkItem(tfsWi.ParentId.Value);
                var parentExtIssue = GetDetails(parentWi);
                parentExtIssue.Url = _tfsApi.ConnectionUri + "/web/wi.aspx?id=" + parentExtIssue.Id; 
                parentExtIssue.SubIssues = new List<ExternalIssueDetails> { extIssue };

                extIssue = parentExtIssue;
                tfsWi = parentWi;
            }

            return extIssue;
        }

        private ExternalIssueDetails GetDetails(TfsWorkItem wi)
        {
            var eid = new ExternalIssueDetails
            {
                Id = wi.Id.ToString(CultureInfo.InvariantCulture),
                Created = wi.Created.ToString("dd-MM-yyyy HH:mm:ss"),
                Type = wi.Type,
                Comments = wi.HistoryComments,
                Status = wi.State,
                Summary = wi.Title,
                Description = wi.Description,
            };

            return eid;
        }

        private Issue GetIssueFromTfsWorkItem(TfsWorkItem wi)
        {
            return new Issue
            {
                Id = wi.Id.ToString(CultureInfo.InvariantCulture),
                Url = _tfsApi.ConnectionUri + "/web/wi.aspx?id=" + wi.Id
            };
        }

        private static int ParseTfsWorkItemId(string issueId)
        {
            int workItemId;
            if (!Int32.TryParse(issueId, out workItemId)) throw new FormatException(String.Format("Issue Id '{0}' is not an integer!", issueId));
            return workItemId;
        }

        private static bool IsTfsUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && url.Contains("/tfs");
        }
    }
}
