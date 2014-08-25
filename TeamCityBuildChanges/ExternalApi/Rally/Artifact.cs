using System;

namespace TeamCityBuildChanges.ExternalApi.Rally
{
    public abstract class Artifact
    {
        public string Id { get; set; }
        public string FormattedId { get; set; }
        public DateTime Created { get; set; }
        public string State { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
    }
}