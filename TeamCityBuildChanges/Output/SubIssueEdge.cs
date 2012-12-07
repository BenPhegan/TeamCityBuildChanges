using QuickGraph;
using TeamCityBuildChanges.IssueDetailResolvers;

namespace TeamCityBuildChanges.Output
{
    public class SubIssueEdge : TaggedEdge<ExternalIssueDetails, string>
    {
        public SubIssueEdge(ExternalIssueDetails source, ExternalIssueDetails target, string tag)
            : base(source, target, tag)
        {
        }
    }
}