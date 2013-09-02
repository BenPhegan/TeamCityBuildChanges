using TeamCityBuildChanges.ExternalApi;

namespace TeamCityBuildChanges.Tests.ExternalApi
{
    using NUnit.Framework;
    using TeamCityBuildChanges.ExternalApi;

    [TestFixture]
    public class AuthenticatedRestClientTests
    {
        [TestCase("http://localhost")]
        [TestCase("http://localhost/")]
        public void CreateWithNoAuth(string url)
        {
            IAuthenticatedRestClient restClient = new AuthenticatedRestClient(url);
            Assert.That(restClient.AuthenticationToken, Is.Null);
            Assert.That(restClient.BaseUrl, Is.EqualTo("http://localhost")); // ensure trailing slashes are omitted
        }

        [TestCase("http://user:password@localhost")]
        [TestCase("http://user:password@localhost/")]
        public void CreateWithDerivedAuth(string url)
        {
            IAuthenticatedRestClient restClient = new AuthenticatedRestClient(url);
            Assert.That(restClient.AuthenticationToken, Is.EqualTo("dXNlcjpwYXNzd29yZA=="));
            Assert.That(restClient.BaseUrl, Is.EqualTo("http://localhost")); // ensure trailing slashes are omitted
        }

        [TestCase("http://localhost", "dXNlcjpwYXNzd29yZA==")]
        [TestCase("http://localhost/", "dXNlcjpwYXNzd29yZA==")]
        public void CreateWithComposedAuth(string url, string authenticationToken)
        {
            IAuthenticatedRestClient restClient = new AuthenticatedRestClient(url, authenticationToken);
            Assert.That(restClient.AuthenticationToken, Is.EqualTo("dXNlcjpwYXNzd29yZA=="));
            Assert.That(restClient.BaseUrl, Is.EqualTo("http://localhost")); // ensure trailing slashes are omitted
        }

        [TestCase("http://frank:sinatra@localhost", "dXNlcjpwYXNzd29yZA==")]
        [TestCase("http://frank:sinatra@localhost/", "dXNlcjpwYXNzd29yZA==")]
        public void CreateWithBothDefaultsToComposedAuthToken(string url, string authenticationToken)
        {
            IAuthenticatedRestClient restClient = new AuthenticatedRestClient(url, authenticationToken);
            Assert.That(restClient.AuthenticationToken, Is.EqualTo("dXNlcjpwYXNzd29yZA=="));
            Assert.That(restClient.BaseUrl, Is.EqualTo("http://localhost")); // ensure trailing slashes are omitted
        }
    }
}
