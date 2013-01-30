using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FakeItEasy;
using NUnit.Framework;
using TeamCityBuildChanges.Commands;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;
using TeamCityBuildChanges.Testing;

namespace TeamCityBuildChanges.Tests.Commands
{
    [TestFixture]
    public class AggregateBuildDeltaResolverTests
    {
        [Test]
        public void TestingFullSetup()
        {
            var template = new BuildTemplate
                {
                    BuildId = "bt1",
                    BuildName = "Build1",
                    BuildCount = 15,
                    BuildNumberPattern = "1.{0}",
                    StartBuildNumber = 2,
                    FinishBuildNumber = 4,
                    StartBuildPackages = new Dictionary<string, string> { { "Package1", "1.0" }, { "Package2", "1.0" } },
                    FinishBuildPackages = new Dictionary<string, string> { { "Package1", "1.1" }, { "Package2", "1.0" } },
                    IssueCount = 5,
                    NestedIssueDepth = 1,
                    NestedIssueChance = 80,
                    CreateNuGetPackageChangeManifests = true
                };

            var resolver = TestHelpers.CreateMockedAggregateBuildDeltaResolver(new[]{template});
            var result = resolver.CreateChangeManifestFromBuildTypeId("bt1", null, "1.2", "1.4",false,true);

            //Assert
            Assert.AreEqual(2,result.ChangeDetails.Count);
            Assert.AreEqual(2,result.NuGetPackageChanges.Count);
        }
    }
}
