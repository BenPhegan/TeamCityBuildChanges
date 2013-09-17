using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using NUnit.Framework;
using RestSharp;
using ServiceStack.CacheAccess.Providers;
using TeamCityBuildChanges.ExternalApi;
using TeamCityBuildChanges.ExternalApi.TeamCity;

namespace TeamCityBuildChanges.Tests.ExternalApi
{
    [TestFixture]
    public class TeamCityApiTests
    {
        [Test]
        public void OnlyFirstRequestForSameItemShouldGoToRestClient()
        {
            var mockClient = A.Fake<IAuthenticatedRestClient>();
            A.CallTo(mockClient).WithReturnType<BuildDetails>().ReturnsLazily(() => new BuildDetails {Id = "5555"});

            var api = new TeamCityApi(mockClient, new MemoryCacheClient());
            api.GetBuildDetailsByBuildId("test");
            api.GetBuildDetailsByBuildId("test");

            A.CallTo(() => mockClient.Execute<BuildDetails>(A<IRestRequest>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void BuildsShouldComeBackOrderedByNumericalId()
        {
            var mockClient = A.Fake<IAuthenticatedRestClient>();

            var api = new TeamCityApi(mockClient, new MemoryCacheClient());
            SetMockReturnValues(mockClient, "bt100","98", new List<IssueUsage>());
            SetMockReturnValues(mockClient, "bt100", "99", new List<IssueUsage>());
            SetMockReturnValues(mockClient, "bt100", "100", new List<IssueUsage>
                {
                    new IssueUsage{Issue = new Issue{Id = "TEST-1", Url = "http://test.com/api/test-1"}}
                });
            SetMockReturnValues(mockClient, "bt100", "101", new List<IssueUsage>
                {
                    new IssueUsage{Issue = new Issue{Id = "TEST-2", Url = "http://test.com/api/test-2"}},
                    new IssueUsage{Issue = new Issue{Id = "TEST-3", Url = "http://test.com/api/test-3"}}
                });
            SetMockReturnValues(mockClient, "bt100", "102", new List<IssueUsage>());

            //Construct a build number order that is int but not string correct (crossing length/number boundary
            var issues = api.GetIssuesByBuildTypeAndBuildRange("bt100", "1.0.0.0", "1.0.0.2", new List<Build>
                {
                    new Build {BuildTypeId = "bt100", Id = "98", Number = "0.9.0.0"},
                    new Build {BuildTypeId = "bt100", Id = "99", Number = "1.0.0.0"},
                    new Build {BuildTypeId = "bt100", Id = "100", Number = "1.0.0.1"},
                    new Build {BuildTypeId = "bt100", Id = "101", Number = "1.0.0.2"},
                    new Build {BuildTypeId = "bt100", Id = "102", Number = "1.0.0.3"},
                });

            Assert.AreEqual(3, issues.Count());

        }

        private static void SetMockReturnValues(IAuthenticatedRestClient mockClient, string buildTypeId, string buildId, IEnumerable<IssueUsage> issues)
        {
            A.CallTo(() => mockClient.Execute<BuildDetails>(A<IRestRequest>
                .That.Matches(r => r.Parameters.Any(p => p.Value.ToString() == buildId))))
                .Returns(new RestResponse<BuildDetails>
                 {
                     Data = new BuildDetails { BuildTypeId = buildTypeId, RelatedIssues = issues.ToList() }
                 });
        }
    }
}
