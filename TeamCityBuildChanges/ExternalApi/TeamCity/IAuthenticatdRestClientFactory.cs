namespace TeamCityBuildChanges.ExternalApi.TeamCity
{
    public interface IAuthenticatdRestClientFactory
    {
        string Server { get; }
        string AuthToken { get; }
        IAuthenticatedRestClient Client();
    }
}