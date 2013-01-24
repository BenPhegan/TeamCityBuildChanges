using System;
using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;

namespace TeamCityBuildChanges.Output
{
    /// <summary>
    /// Provide a ChangeManifest that is used as a Model object to represent Build changes.
    /// </summary>
    public class ChangeManifest
    {
        public ChangeManifest()
        {
            ChangeDetails = new List<ChangeDetail>();
            IssueDetails = new List<ExternalIssueDetails>();
            GenerationLog = new List<LogEntry>();
            GenerationStatus = Status.FTMFW;
        }

        public List<ChangeDetail> ChangeDetails { get; set; }
        public List<ExternalIssueDetails> IssueDetails { get; set; }
        public DateTime Generated { get; set; }
        public string FromVersion { get; set; }
        public string ToVersion { get; set; }
        public BuildTypeDetails ReferenceBuildConfiguration { get; set; }
        public BuildTypeDetails BuildConfiguration { get; set; }
        public List<NuGetPackageChange> NuGetPackageChanges { get; set; }
        public List<LogEntry> GenerationLog { get; set; }
        public Status GenerationStatus { get; set; }

        public List<ExternalIssueDetails> ConsolidatedIssueDetails
        {
            get
            {
                var returnList = new List<ExternalIssueDetails>();
                var graph = IssueDetails.AsAdjacencyGraph();

                // this gets a little freaky

                // add root nodes - these comprise the full list
                List<ExternalIssueDetails> roots = graph.Roots().ToList();
                returnList.AddRange(roots);
                // now we just tweak the subissues properties within the source of each edge of every sink vertex
                foreach (ExternalIssueDetails issue in graph.Sinks())
                {
                    ExternalIssueDetails targetIssue = issue;
                    IEnumerable<SubIssueEdge> edges = graph.Edges.Where(e => e.Target.Equals(targetIssue));
                    foreach (SubIssueEdge edge in edges)
                    {
                        ExternalIssueDetails source = edge.GetOtherVertex(issue);
                        if (!source.ContainsSubIssue(issue))
                            source.SubIssues.Add(issue);
                    }
                }
                return returnList;
            }
        }

        public List<ExternalIssueDetails> FlattenedIssueDetails
        {
            get { return IssueDetails.AsAdjacencyGraph().TopologicalSort().ToList(); }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ChangeManifest) obj);
        }

        protected bool Equals(ChangeManifest other)
        {
            return Equals(ChangeDetails, other.ChangeDetails) && Equals(IssueDetails, other.IssueDetails) && Generated.Equals(other.Generated) && string.Equals(FromVersion, other.FromVersion) && string.Equals(ToVersion, other.ToVersion) && Equals(ReferenceBuildConfiguration, other.ReferenceBuildConfiguration) && Equals(BuildConfiguration, other.BuildConfiguration);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (ChangeDetails != null ? ChangeDetails.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (IssueDetails != null ? IssueDetails.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Generated.GetHashCode();
                hashCode = (hashCode*397) ^ (FromVersion != null ? FromVersion.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ToVersion != null ? ToVersion.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ReferenceBuildConfiguration != null ? ReferenceBuildConfiguration.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (BuildConfiguration != null ? BuildConfiguration.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public enum Status
    {
        Ok,
        FTW,
        FTMFW,
        Error,
        Warning
    }
}