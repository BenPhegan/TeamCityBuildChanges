using System;
using System.Collections.Concurrent;
using System.Threading;
using RestSharp;
using ServiceStack.CacheAccess;

namespace TeamCityBuildChanges.ExternalApi.TeamCity
{
    public class CachingThreadSafeAuthenticatedRestClient : AuthenticatedRestClient
    {
        private readonly ICacheClient _cacheClient;
        private readonly string _url;
        private readonly string _authenticationToken;
        private readonly ConcurrentDictionary<string, DateTime> _runningRequests = new ConcurrentDictionary<string, DateTime>();

        public CachingThreadSafeAuthenticatedRestClient(ICacheClient cacheClient, string url, string authenticationToken = null) 
            : base(url, authenticationToken)
        {
            _url = url;
            _authenticationToken = authenticationToken;
            _cacheClient = cacheClient;
        }

        public override IRestResponse<T> Execute<T>(IRestRequest request)
        {
            var key = String.Format("{0}/{1}", BaseUrl, request.Resource);
            //If we are waiting for a response already...
            if (_runningRequests.ContainsKey(key))
            {
                //TODO is there a nicer way to block....
                while (_runningRequests.ContainsKey(key))
                    Thread.Sleep(50);
            }

            //Check the cache...
            var response = _cacheClient.Get<RestResponse<T>>(key);
            if (response != null)
                return response;

            //Ok, get it remotely...
            _runningRequests.TryAdd(key,DateTime.Now);
            var client = new AuthenticatedRestClient(_url, _authenticationToken);

            var builder = new UriBuilder(client.BaseUrl)
                {
                    Path = string.IsNullOrEmpty(client.AuthenticationToken) ? "guestAuth" : "httpAuth"
                };

            client.BaseUrl = builder.ToString();

            var result = client.Execute<T>(request);

            //Add to cache, and remove from the list of running requests...
            _cacheClient.Add(key, result);
            DateTime requestStarted;
            _runningRequests.TryRemove(key, out requestStarted);
            return result;
        }
    }
}
