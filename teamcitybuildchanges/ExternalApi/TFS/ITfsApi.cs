using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamCityBuildChanges.ExternalApi.TFS
{
    public interface ITfsApi
    {
        string ConnectionUri { get; }
        IEnumerable<TfsWorkItem> GetWorkItemsByCommit(int commit);
        TfsWorkItem GetWorkItem(int workItemId);
    }
}
