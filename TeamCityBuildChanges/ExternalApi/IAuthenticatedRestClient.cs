using RestSharp;

namespace TeamCityBuildChanges.ExternalApi
{
    public interface IAuthenticatedRestClient
    {
        string AuthenticationToken { get; }
        string BaseUrl { get; set; }
        IRestResponse<T> Execute<T>(IRestRequest request)  where T : new();
        byte[] DownloadData(IRestRequest request);
    }
}