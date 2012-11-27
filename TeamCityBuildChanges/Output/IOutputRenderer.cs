namespace TeamCityBuildChanges.Output
{
    public interface IOutputRenderer
    {
        string Render(ChangeManifest manifest);
    }
}