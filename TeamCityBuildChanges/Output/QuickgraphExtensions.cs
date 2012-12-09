using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using TeamCityBuildChanges.IssueDetailResolvers;

namespace TeamCityBuildChanges.Output
{
    public static class QuickgraphExtensions
    {
        public static AdjacencyGraph<ExternalIssueDetails, SubIssueEdge> AsAdjacencyGraph<TSource>(this IEnumerable<TSource> source)
            where TSource : ExternalIssueDetails
        {
            var graph = new AdjacencyGraph<ExternalIssueDetails, SubIssueEdge>(true, -1, -1, new ExternalIssueDetailsEqualityComparer());
            
            var issues = source as TSource[] ?? source.ToArray();

            foreach (var issue in issues)
                if (!graph.ContainsVertex(issue))
                    graph.AddVertex(issue);

            foreach (var issue in issues)
                ProcessIssue(graph, issue);

            return graph;
        }

        private static void ProcessIssue(AdjacencyGraph<ExternalIssueDetails, SubIssueEdge> graph, ExternalIssueDetails contextIssue, ExternalIssueDetails parentIssue = null)
        {
            if (parentIssue != null)
            {
                var parent = parentIssue;
                if (graph.ContainsVertex(parentIssue))
                    parent = graph.Vertices.First(e => e.Equals(parentIssue));

                graph.AddVertex(contextIssue);
                graph.AddEdge(new SubIssueEdge(parent, contextIssue, "SubIssue"));
            }

            if (contextIssue.SubIssues != null)
                foreach (var issue in contextIssue.SubIssues)
                {
                    ProcessIssue(graph, issue, contextIssue);
                }
        }
    }
}