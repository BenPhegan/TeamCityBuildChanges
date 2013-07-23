namespace TeamCityBuildChanges.ExternalApi.Jira
{
    public interface IJiraApi
    {
        RootObject GetJiraIssue(string key);
    }
}