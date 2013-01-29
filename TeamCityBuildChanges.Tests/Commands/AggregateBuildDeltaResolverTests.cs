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
        private readonly TestHelpers _testHelpers = new TestHelpers();

        [Test]
        public void TestingFullSetup()
        {
            var resolver = TestHelpers.CreateMockedAggregateBuildDeltaResolver(new List<Tuple<string, string, int>>{Tuple.Create("bt1","Build1",15)});
            var result = resolver.CreateChangeManifestFromBuildTypeId("bt1", null, "1.2", "1.4");

            //Assert
            Assert.AreEqual(2,result.ChangeDetails.Count);
            Assert.AreEqual(2,result.NuGetPackageChanges.Count);
        }
    }
}
