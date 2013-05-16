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
        public void TestSingleTfsUrl()
        {
            var tfsConnections = new List<string>
                {
                    string.Format("http://{0}/tfs", Internet.DomainName())
                };

            var tfsTemplate = new TfsTemplate
                {
                    ConnectionUri = tfsConnections.First(),
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
                    TfsConnection = tfsConnections.First(),
                    Id = 1.ToString(),
                    RelatedIssueIds = new List<int> {1, 2, 3, 4, 5, 6, 7, 8}
                };

            var resolvers = TestHelpers.CreateMockedTfsApi(new[] { tfsTemplate });
            var externalResolver = new IssueDetailResolver(resolvers);

            var teamcityApi = TestHelpers.CreateMockedTeamCityApi(new[] {buildDetailTemplates});

            var result = externalResolver.GetExternalIssueDetails(TestHelpers.GetIssuesFromMockedTeamCityApi(teamcityApi, new[] {buildDetailTemplates}));
            Assert.AreEqual(8, result.Count());
        }

        [Test]
        public void TestMultipleTfsUrl()
        {
            var tfsConnections = new List<string>
                {
                    string.Format("http://{0}/tfs", Internet.DomainName()),
                    string.Format("http://{0}/tfs", Internet.DomainName())
                };

            var tfsTemplate1 = new TfsTemplate
            {
                ConnectionUri = tfsConnections.First(),
                WorkItems = new Dictionary<int, List<int>>
                        {
                            {1, new List<int> {2, 3, 4}},
                            {2, null},
                            {3, null},
                            {4, null}
                        }
            };

            var tfsTemplate2 = new TfsTemplate
            {
                ConnectionUri = tfsConnections.Last(),
                WorkItems = new Dictionary<int, List<int>>
                        {
                            {5, new List<int> {6, 7, 8}},
                            {6, null},
                            {7, null},
                            {8, null}
                        }
            };

            var buildDetailTemplates = new BuildDetailsTemplate
            {
                TfsConnection = tfsConnections.First(),
                Id = 1.ToString(),
                RelatedIssueIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 }
            };

            var resolvers = TestHelpers.CreateMockedTfsApi(new[] { tfsTemplate1, tfsTemplate2 });
            var externalResolver = new IssueDetailResolver(resolvers);

            var teamcityApi = TestHelpers.CreateMockedTeamCityApi(new[] { buildDetailTemplates });

            var result = externalResolver.GetExternalIssueDetails(TestHelpers.GetIssuesFromMockedTeamCityApi(teamcityApi, new[] { buildDetailTemplates }));
            Assert.AreEqual(4, result.Count());
        }
    }
}
