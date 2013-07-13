namespace TeamCityBuildChanges.ExternalApi
{
    using System;
    using RestSharp;
    using TeamCityBuildChanges.ExternalApi.TeamCity;

    public class AuthenticatedRestClient : RestClient
    {
        private readonly string authenticationToken;

        /// <summary>
        /// Gets the authentication token.
        /// </summary>
        /// <value>
        /// The authentication token.
        /// </value>
        public string AuthenticationToken
        {
            get { return this.authenticationToken; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticatedRestClient"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="authenticationToken">The authentication token.</param>
        public AuthenticatedRestClient(string url, string authenticationToken = null) : base(url)
        {
            var uri = new Uri(url);
            var derivedAuthToken = uri.TryResolveAuthToken(out uri);
            
            this.authenticationToken = authenticationToken ?? derivedAuthToken;

            base.BaseUrl = uri.ToString();
        }

        /// <summary>
        /// Executes the specified request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public override IRestResponse<T> Execute<T>(IRestRequest request)
        {
            if (!string.IsNullOrEmpty(AuthenticationToken))
            {
                request.AddHeader("Authorization", "Basic " + AuthenticationToken);
            }
            return base.Execute<T>(request);
        }
    }
}