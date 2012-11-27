using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.ExternalApi.TFS;

namespace TeamCityBuildChanges.IssueDetailResolvers
{
    public class TFSIssueResolver : IExternalIssueResolver
    {
        TfsApi _tfsApi;

        public TFSIssueResolver(TfsApi tfsApi)
        {
            _tfsApi = tfsApi;
        }

        public IEnumerable<ExternalIssueDetails> GetDetails(IEnumerable<Issue> issues)
        {
            List<ExternalIssueDetails> externalIssueDetails = new List<ExternalIssueDetails>();

            foreach (var issue in issues)
            {
                if (!IsTfsUrl(issue.Url)) continue;

                externalIssueDetails.Add(GetDetails(issue));
            }

            return externalIssueDetails;
        }

        private ExternalIssueDetails GetDetails(Issue issue)
        {            
            var tfsWi = _tfsApi.GetWorkItem(ParseTfsWorkItemId(issue.Id));
            ExternalIssueDetails extIssue = GetDetails(tfsWi);

            while (tfsWi.ParentId.HasValue)
            {
                var parentWi = _tfsApi.GetWorkItem(tfsWi.ParentId.Value);
                var parentExtIssue = GetDetails(parentWi);
                parentExtIssue.SubIssues = new List<ExternalIssueDetails> { extIssue };

                extIssue = parentExtIssue;
                tfsWi = parentWi;
            }

            return extIssue;
        }

        private ExternalIssueDetails GetDetails(TfsWorkItem wi)
        {
            ExternalIssueDetails eid = new ExternalIssueDetails
            {
                Id = wi.Id.ToString(),
                Created = wi.Created.ToString("dd-MM-yyyy HH:mm:ss"),
                Comments = wi.HistoryComments,
                Status = wi.State
            };

            return eid;
        }

        private int ParseTfsWorkItemId(string issueId)
        {
            int workItemId;
            if (!Int32.TryParse(issueId, out workItemId)) throw new FormatException(String.Format("Issue Id '{0}' is not an integer!", issueId));
            return workItemId;
        }

        private bool IsTfsUrl(string url)
        {
            return url.Contains("/tfs");
        }
    }
}
