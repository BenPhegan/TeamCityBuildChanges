namespace TeamCityBuildChanges.ExternalApi.TeamCity
{
    public interface ICache
    {
        T GetItem<T>(string key) where T : class;
        bool TryGetItem<T>(string key, out T value) where T : class;
        bool SetItem<T>(string key, T value) where T : class;
    }
}