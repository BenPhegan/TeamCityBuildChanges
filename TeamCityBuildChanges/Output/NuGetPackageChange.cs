namespace TeamCityBuildChanges.Output
{
    public class NuGetPackageChange
    {
        public string PackageId { get; set; }
        public string OldVersion { get; set; }
        public string NewVersion { get; set; }
        public NuGetPackageChangeType Type { get; set; }
        public ChangeManifest ChangeManifest { get; set; }
    }

    public enum NuGetPackageChangeType
    {
        Added,
        Removed,
        Modified,
        Unchanged
    }
}
