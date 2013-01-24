using System.Collections.Generic;
using System.Linq;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges.Commands
{
    /// <summary>
    /// Provides a list of NuGetPackageChange objects representing the difference in a set of build dependencies.
    /// </summary>
    public class PackageChangeComparator : IPackageChangeComparator
    {
        public List<NuGetPackageChange> GetPackageChanges(List<TeamCityApi.PackageDetails> initialPackages, List<TeamCityApi.PackageDetails> finalPackages)
        {
            var returnList = new List<NuGetPackageChange>();
            foreach (var package in initialPackages)
            {
                var exactMatch = finalPackages.Where(a => a.Id == package.Id && a.Version == package.Version);

                //Matching
                returnList.AddRange(exactMatch.Select(detailse => new NuGetPackageChange
                    {
                        PackageId = detailse.Id, 
                        NewVersion = detailse.Version, 
                        OldVersion = detailse.Version, 
                        Type = NuGetPackageChangeType.Unchanged
                    }));

                //Missing
                if (finalPackages.All(a => a.Id != package.Id))
                    returnList.Add(new NuGetPackageChange()
                        {
                            PackageId = package.Id,
                            OldVersion = package.Version,
                            NewVersion = string.Empty,
                            Type = NuGetPackageChangeType.Removed
                        });

                //Modified
                var updatedVersions = finalPackages.Where(a => a.Id == package.Id && a.Version != package.Version);
                returnList.AddRange(updatedVersions.Select(newPackage => new NuGetPackageChange
                    {
                        PackageId = package.Id, 
                        OldVersion = package.Version,
                        NewVersion = newPackage.Version,
                        Type = NuGetPackageChangeType.Modified
                    }));
            }

            foreach (var package in finalPackages)
            {
                //new
                if (initialPackages.All(a => a.Id != package.Id))
                    returnList.Add(new NuGetPackageChange()
                        {
                            PackageId = package.Id,
                            NewVersion = package.Version,
                            OldVersion = string.Empty,
                            Type = NuGetPackageChangeType.Added
                        });
            }

            return returnList;
        }
    }
}