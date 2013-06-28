using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using TeamCityBuildChanges.Commands;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges.Tests.Commands
{
    [TestFixture]
    public class PackageChangeComparatorTests
    {
        [TestCase("1.0", "1.0",Result = 0)]
        [TestCase("1.1", "1.0", Result = 1)]
        [TestCase("1.1", "1.1", Result = 2)]
        public int DetectsModifiedPackages(string package1Final, string package2Final)
        {
            var initial = new[] {Tuple.Create("Package1", "1.0"), Tuple.Create("Package2", "1.0")};
            var final = new[] {Tuple.Create("Package1", package1Final), Tuple.Create("Package2", package2Final)};
            
            var compararator = new PackageChangeComparator();
            var results = compararator.GetPackageChanges(CreateNuGetPackegList(initial), CreateNuGetPackegList(final));

            return results.Count(r => r.Type == NuGetPackageChangeType.Modified);
        }

        [TestCase(1,1, NuGetPackageChangeType.Added, Result = 0)]
        [TestCase(1, 2, NuGetPackageChangeType.Added, Result = 1)]
        [TestCase(2, 4, NuGetPackageChangeType.Added, Result = 2)]
        [TestCase(2, 2, NuGetPackageChangeType.Removed, Result = 0)]
        [TestCase(3, 2, NuGetPackageChangeType.Removed, Result = 1)]
        public int DetectsAddedRemovedPackages(int initialCount, int finalCount, NuGetPackageChangeType changeType)
        {
            var initial = Enumerable.Range(1,initialCount).Select(i => Tuple.Create("Package" +i.ToString(CultureInfo.InvariantCulture), "1.0")).ToList();
            var final = Enumerable.Range(1,finalCount).Select(i => Tuple.Create("Package" + i.ToString(CultureInfo.InvariantCulture), "1.0")).ToList();

            var compararator = new PackageChangeComparator();
            var results = compararator.GetPackageChanges(CreateNuGetPackegList(initial), CreateNuGetPackegList(final));

            return results.Count(r => r.Type == changeType);
        }

        private static List<TeamCityApi.PackageDetails> CreateNuGetPackegList(IEnumerable<Tuple<string, string>> packages)
        {
            return packages.Select(p => new TeamCityApi.PackageDetails { Id = p.Item1, Version = p.Item2 }).ToList();
        }

    }
}
