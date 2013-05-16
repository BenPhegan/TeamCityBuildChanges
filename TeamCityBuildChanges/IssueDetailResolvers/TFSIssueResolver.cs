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
        readonly ITfsApi _tfsApi;

        public TFSIssueResolver(ITfsApi tfsApi)
        {
            _tfsApi = tfsApi;
        }

        public IEnumerable<ExternalIssueDetails> GetDetails(IEnumerable<Issue> issues)
        {
            var externalIssueDetails = new List<ExternalIssueDetails>();

            externalIssueDetails.AddRange(issues.Where(i => IsTfsUrl(i.Url)).Select(GetDetails).Where(i => i != null));

            return externalIssueDetails;
        }

        public IEnumerable<Issue> GetIssues(IEnumerable<ChangeDetail> changeDetails)
        {
            var issues = new List<Issue>();
            try
            {
                foreach (var workItems in changeDetails.Select(changeDetail => _tfsApi.GetWorkItemsByCommit(Convert.ToInt32(changeDetail.Version))))
                {
                    issues.AddRange(workItems.Select(GetIssueFromTfsWorkItem));
                }
            }
            catch (Exception e)
            {
            }
            return issues;
        }

        private ExternalIssueDetails GetDetails(Issue issue)
        {
            if (issue.TfsRootUrl != _tfsApi.ConnectionUri)
                return null;
            var tfsWi = _tfsApi.GetWorkItem(ParseTfsWorkItemId(issue.Id));
            if (tfsWi == null)
                return null;
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
                Id = wi.Id.ToString(),
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
                Id = wi.Id.ToString(),
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
            return url.Contains("/tfs");
        }
    }
}
