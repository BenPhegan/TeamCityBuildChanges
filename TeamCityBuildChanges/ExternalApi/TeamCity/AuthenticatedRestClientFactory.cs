using System;

namespace TeamCityBuildChanges.ExternalApi.TeamCity
{
    public class AuthenticatedRestClientFactory : IAuthenticatdRestClientFactory
    {

        public AuthenticatedRestClientFactory(string server, string authToken = null)
        {
            AuthToken = authToken;
            Server = server;
        }

        public string Server { get; private set; }
        public string AuthToken { get; private set; }

        public IAuthenticatedRestClient Client()
        {
            IAuthenticatedRestClient client = new AuthenticatedRestClient(Server, AuthToken);

            var builder = new UriBuilder(client.BaseUrl)
            {
                Path = string.IsNullOrEmpty(client.AuthenticationToken) ? "guestAuth" : "httpAuth"
            };

            client.BaseUrl = builder.ToString();

            return client;
        }
    }
}