using System.Collections.Generic;

namespace TeamCityBuildChanges.ExternalApi.TFS
{
    public interface ITfsApi
    {
        string ConnectionUri { get; }
        IEnumerable<TfsWorkItem> GetWorkItemsByCommit(int commit);
        TfsWorkItem GetWorkItem(int workItemId);
    }
}