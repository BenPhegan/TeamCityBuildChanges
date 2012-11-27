using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamCityBuildChanges.ExternalApi.TFS
{
    public class TfsWorkItem
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
        public int? ParentId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> HistoryComments { get; set; }

        public IEnumerable<int> ChildrenIds { get; set; }
    }
}
