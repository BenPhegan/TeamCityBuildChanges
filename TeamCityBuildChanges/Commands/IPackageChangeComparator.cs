using System.Collections.Generic;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges.Commands
{
    public interface IPackageChangeComparator
    {
        List<NuGetPackageChange> GetPackageChanges(List<TeamCityApi.PackageDetails> initialPackages, List<TeamCityApi.PackageDetails> finalPackages);
    }
}