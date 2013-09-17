using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using ServiceStack.CacheAccess;
using TeamCityBuildChanges.ExtensionMethods;

namespace TeamCityBuildChanges.ExternalApi.TFS
{
    public class TfsApi : ITfsApi
    {
        private readonly ICacheClient _cacheClient;
        public string ConnectionUri { get; private set; }

        private TfsTeamProjectCollection _connection;

        private void Connect()
        {
            if (Uri.IsWellFormedUriString(ConnectionUri, UriKind.Absolute))
            {
                _connection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(ConnectionUri));
            }
        }

        public TfsApi(string connectionUri, ICacheClient cacheClient)
        {
            _cacheClient = cacheClient;
            ConnectionUri = connectionUri;
        }

        public IEnumerable<TfsWorkItem> GetWorkItemsByCommit(int commit)
        {
            return _cacheClient.GetFromCacheOrFunc<IEnumerable<TfsWorkItem>>(commit.ToString(CultureInfo.InvariantCulture), key =>
                {
                    if (_connection == null) Connect();

                    if (_connection != null)
                    {
                        var versionControlServer = _connection.GetService<VersionControlServer>();
                        var changeSet = versionControlServer.GetChangeset(commit);
                        var workItems = changeSet.WorkItems.ToList();
                        var results = workItems.Select(workItem => new TfsWorkItem
                            {
                                Id = workItem.Id,
                                Title = workItem.Title,
                                Type = workItem.Type.Name,
                                State = workItem.State,
                                Created = workItem.CreatedDate,
                                Description = workItem.Description,
                                ParentId = GetParentId(workItem),
                                ChildrenIds = GetChildrenIds(workItem),
                                HistoryComments = workItem.Revisions.Cast<Revision>().Select(r => r.Fields[CoreField.History].Value.ToString()).ToList()
                            }).ToList();

                        return results;
                    }

                    return null;
                });
        }

        public TfsWorkItem GetWorkItem(int workItemId)
        {
            return _cacheClient.GetFromCacheOrFunc(workItemId.ToString(CultureInfo.InvariantCulture), key =>
                {
                    if (_connection == null) Connect();

                    if (_connection != null)
                    {
                        var workItemStore = _connection.GetService<WorkItemStore>();

                        var workItem = workItemStore.GetWorkItem(workItemId);

                        var result = new TfsWorkItem
                            {
                                Id = workItem.Id,
                                Title = workItem.Title,
                                Type = workItem.Type.Name,
                                State = workItem.State,
                                Created = workItem.CreatedDate,
                                Description = workItem.Description,
                                ParentId = GetParentId(workItem),
                                ChildrenIds = GetChildrenIds(workItem),
                                HistoryComments = workItem.Revisions.Cast<Revision>().Select(r => r.Fields[CoreField.History].Value.ToString()).ToList()
                            };

                        return result;
                    }
                    //TODO Throw a better error?
                    return null;
                });
        }

        private IEnumerable<int> GetChildrenIds(WorkItem workItem)
        {
            return GetRelatedWorkItemLinks(workItem)
                .Where(IsChildLink)
                .Select(link => link.RelatedWorkItemId).ToList();
        }

        private int? GetParentId(WorkItem workItem)
        {
            var parents = GetRelatedWorkItemLinks(workItem).Where(IsParentLink).ToList();
            if (!parents.Any()) return null;
            return parents.First().RelatedWorkItemId;
        }

        private IEnumerable<RelatedLink> GetRelatedWorkItemLinks(WorkItem workItem)
        {
            return workItem.Links.Cast<Link>()
                .Where(link => link.BaseType == BaseLinkType.RelatedLink)
                .Select(link => link as RelatedLink)
                .ToList();
        }

        private bool IsChildLink(RelatedLink link)
        {
            return link.LinkTypeEnd.Name == "Child";
        }

        private bool IsParentLink(RelatedLink link)
        {
            return link.LinkTypeEnd.Name == "Parent";
        }
    }
}
