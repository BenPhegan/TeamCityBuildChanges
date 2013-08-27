using System.Collections.Generic;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges
{
    public interface IPackageChangeComparator
    {
        IEnumerable<NuGetPackageChange> GetPackageChanges(List<TeamCityApi.PackageDetails> initialPackages, List<TeamCityApi.PackageDetails> finalPackages);
    }
}