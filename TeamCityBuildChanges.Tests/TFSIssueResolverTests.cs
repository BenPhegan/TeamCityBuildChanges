using System.Collections.Generic;
using System.Linq;
using Castle.Components.DictionaryAdapter;
using Faker;
using NUnit.Framework;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;
using TeamCityBuildChanges.Testing;

namespace TeamCityBuildChanges.Tests
{
    [TestFixture]
    public class TFSIssueResolverTests
    {
        [Test]
        public void Test()
        {
            var tfsTemplates = new TfsTemplate
                {
                    ConnectionUri = string.Format("http://{0}/tfs", Internet.DomainName()),
                    WorkItems = new Dictionary<int, List<int>>
                        {
                            {1, new List<int> {2, 3, 4}},
                            {2, null},
                            {3, null},
                            {4, null},
                            {5, new List<int> {6, 7, 8}},
                            {6, null},
                            {7, null},
                            {8, null},
                        }
                };

            var buildDetailTemplates = new BuildDetailsTemplate
                {
                    Id = 1.ToString(),
                    RelatedIssueIds = new List<int> {1, 2, 3, 4, 5, 6, 7, 8}
                };

            var resolvers = TestHelpers.CreateMockedTfsApi(new[] { tfsTemplates });
            var externalResolver = new IssueDetailResolver(resolvers);

            var issues = TestHelpers.CreateMockedIssueList(new[] { buildDetailTemplates });

            var result = externalResolver.GetExternalIssueDetails(issues);
            Assert.AreEqual(8, result.Count());
        }
    }
}
