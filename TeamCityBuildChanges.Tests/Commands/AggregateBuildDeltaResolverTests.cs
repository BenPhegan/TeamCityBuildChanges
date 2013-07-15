using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FakeItEasy;
using NUnit.Framework;
using TeamCityBuildChanges.Commands;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;
using TeamCityBuildChanges.NuGetPackage;
using TeamCityBuildChanges.Output;
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

        [Test]
        public void ResolveCorrectBuild()
        {
            var mappingTemplate = new List<PackageBuildMapping>
                {
                    new PackageBuildMapping()
                        {
                            BuildConfigurationId = "bt1",
                            BuildConfigurationName = "Build1",
                            PackageId = "Package1",
                            Project = "Project1",
                            ServerUrl = "http://test.server"
                        },
                    new PackageBuildMapping()
                        {
                            BuildConfigurationId = "bt2",
                            BuildConfigurationName = "Build2",
                            PackageId = "Package1",
                            Project = "Project1",
                            ServerUrl = "http://test.server"
                        }
                };

            var buildTemplates = new[]
                {
                    new BuildTemplate
                        {
                            BuildId = "bt3",
                            BuildName = "Build3",
                            BuildCount = 10,
                            BuildNumberPattern = "1.{0}",
                            StartBuildNumber = 2,
                            FinishBuildNumber = 4,
                            StartBuildPackages = new Dictionary<string, string> {{"Package1", "1.2"}},
                            FinishBuildPackages = new Dictionary<string, string> {{"Package1", "1.4"}},
                        },
                    new BuildTemplate
                        {
                            BuildId = "bt1",
                            BuildName = "Build1",
                            BuildCount = 5,
                            BuildNumberPattern = "1.{0}",
                            StartBuildNumber = 1,
                            FinishBuildNumber = 5,
                        },
                    new BuildTemplate
                        {
                            BuildId = "bt2",
                            BuildName = "Build2",
                            BuildCount = 5,
                            BuildNumberPattern = "2.{0}",
                            StartBuildNumber = 1,
                            FinishBuildNumber = 5,
                        }
                };

            var packageChange = new NuGetPackageChange
                {
                    ChangeManifest = new ChangeManifest(),
                    PackageId = "Package1",
                    OldVersion = "1.2",
                    NewVersion = "1.5",
                    Type = NuGetPackageChangeType.Modified
                };

            var api = TestHelpers.CreateMockedTeamCityApi();
            var cache = TestHelpers.CreateMockedMultiplePackageBuildMappingCache(mappingTemplate);
            var resolver = TestHelpers.CreateMockedAggregateBuildDeltaResolver(buildTemplates, api, cache);
            var build = resolver.RetrieveBuild(api, cache, packageChange, new ChangeManifest());
            Assert.AreSame("bt1", build.BuildConfigurationId);
        }
    }
}
