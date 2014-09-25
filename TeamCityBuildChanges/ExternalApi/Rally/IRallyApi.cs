namespace TeamCityBuildChanges.ExternalApi.Rally
{
    public interface IRallyApi
    {
        Defect GetRallyDefect(string key);

        UserStory GetRallyUserStory(string key);
    }
}