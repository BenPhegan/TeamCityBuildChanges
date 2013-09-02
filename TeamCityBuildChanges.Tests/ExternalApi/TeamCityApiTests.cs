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
            var mockRestClientFactory = A.Fake<IAuthenticatdRestClientFactory>();
            var mockClient = A.Fake<IAuthenticatedRestClient>();
            A.CallTo(mockClient).WithReturnType<BuildDetails>().ReturnsLazily(() => new BuildDetails {Id = "5555"});
            A.CallTo(() => mockRestClientFactory.Client()).ReturnsLazily(() => mockClient);

            var api = new TeamCityApi(mockRestClientFactory, new MemoryCacheClient());
            api.GetBuildDetailsByBuildId("test");
            api.GetBuildDetailsByBuildId("test");

            A.CallTo(() => mockClient.Execute<BuildDetails>(A<IRestRequest>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}
