using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Faker;
using NUnit.Framework;
using TeamCityBuildChanges.IssueDetailResolvers;
using TeamCityBuildChanges.Testing;

namespace TeamCityBuildChanges.Tests
{
    [TestFixture]
    public class TFSIssueResolverTests
    {
        private static readonly string Uri = string.Format("http://{0}/tfs", Internet.DomainName());

        [Test]
        public void TestSingleTfsUrl()
        {
            var tfsTemplate = new TFSApiMockTemplate
                {
                    ConnectionUri = Uri,
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

            var buildDetailTemplates = new TeamCityApiMockTemplate
                {
                    TfsConnection = Uri,
                    Id = 1.ToString(CultureInfo.InvariantCulture),
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
            var tfsTemplate1 = new TFSApiMockTemplate
            {
                ConnectionUri = Uri,
                WorkItems = new Dictionary<int, List<int>>
                        {
                            {1, new List<int> {2, 3, 4}},
                            {2, null},
                            {3, null},
                            {4, null}
                        }
            };

            var tfsTemplate2 = new TFSApiMockTemplate
            {
                ConnectionUri = Uri,
                WorkItems = new Dictionary<int, List<int>>
                        {
                            {5, new List<int> {6, 7, 8}},
                            {6, null},
                            {7, null},
                            {8, null}
                        }
            };

            var buildDetailTemplates = new TeamCityApiMockTemplate
            {
                TfsConnection = Uri,
                Id = 1.ToString(CultureInfo.InvariantCulture),
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
