using NUnit.Framework;
using TeamCityBuildChanges.ExternalApi.Rally;

namespace TeamCityBuildChanges.Tests.ExternalApi
{
    [TestFixture]
    public class RallyApiTests
    {
        private RallyApi _rallyApi;

        [TestFixtureSetUp]
        public void Setup()
        {
            _rallyApi = new RallyApi("test", "test", "http://test.blahsasdas");
        }
        [Test]
        public void CanHandleThrownExceptionGettingStory()
        {
            var result = _rallyApi.GetRallyUserStory("test");
            Assert.IsNull(result);
        }

        [Test]
        public void CanHandleThrownExceptionGettingDefect()
        {
            var result = _rallyApi.GetRallyDefect("test");
            Assert.IsNull(result);
        }
    }
}
