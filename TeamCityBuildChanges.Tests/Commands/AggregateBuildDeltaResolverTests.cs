using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FakeItEasy;
using NUnit.Framework;
using TeamCityBuildChanges.Commands;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;

namespace TeamCityBuildChanges.Tests.Commands
{
    [TestFixture]
    public class AggregateBuildDeltaResolverTests
    {
        [Test]
        public void TestingFullSetup()
        {
            var api = A.Fake<ITeamCityApi>();
            
            //BuildType/Builds/ChangeDetails
            var changeDetails = SetupBuildTypeAndBuilds(api, "bt1", "Build1");
            A.CallTo(() => api.GetChangeDetailsByBuildTypeAndBuildNumber("bt1", "1.2", "1.4", A<IEnumerable<Build>>.Ignored))
                .Returns(changeDetails.Where(c => Convert.ToInt16(c.Id) > 2 && Convert.ToInt16(c.Id) < 5).ToList());

            //Issues
            var issues = Enumerable.Range(1, Faker.RandomNumber.Next(0, 3)).Select(i => new Issue {Id = Faker.RandomNumber.Next(2000).ToString()}).ToList();
            A.CallTo(() => api.GetIssuesByBuildTypeAndBuildRange("bt1", "1.2", "1.4", A<IEnumerable<Build>>.Ignored))
                .Returns(issues);
            var issueResolver = A.Fake<IExternalIssueResolver>();
            A.CallTo(issueResolver)
                .WithReturnType<IEnumerable<ExternalIssueDetails>>()
                .Returns(CreateExternalIssueDetails(issues));

            //NuGetPackages
            SetNugetPackageDependencyExpectations(api,"bt1","2",new[]{Tuple.Create("Package1","1.0"),Tuple.Create("Package2","1.0")});
            SetNugetPackageDependencyExpectations(api,"bt1","4",new[]{Tuple.Create("Package1","1.1"),Tuple.Create("Package2","1.0")});

            //ACT
            var resolver = new AggregateBuildDeltaResolver(api, new[] {issueResolver}, new PackageChangeComparator(), null);
            var result = resolver.CreateChangeManifestFromBuildTypeId("bt1", null, "1.2", "1.4");

            //Assert
            Assert.AreEqual(2,result.ChangeDetails.Count);
            Assert.AreEqual(2,result.NuGetPackageChanges.Count);
        }

        private static IEnumerable<ExternalIssueDetails> CreateExternalIssueDetails(IEnumerable<Issue> issues)
        {
            return issues.Select(i => new ExternalIssueDetails
                {
                    Id = i.Id,
                    Description = Faker.Company.BS(),
                    Created = DateTime.Today.AddDays(-Faker.RandomNumber.Next(150)).ToString(),
                    Status = new[]{"Open","Closed","Resolved"}[Faker.RandomNumber.Next(0,2)],
                    Comments = new List<string>{Faker.Company.BS(),Faker.Company.CatchPhrase(),Faker.Lorem.Sentence()},
                    Type = new[]{"Bug","Task","Issue"}[Faker.RandomNumber.Next(0,2)],
                    Summary = Faker.Company.CatchPhrase(),
                    Url = string.Format("http://{0}/issues/{1}",Faker.Internet.DomainName(),i)
                });
        }

        private static void SetNugetPackageDependencyExpectations(ITeamCityApi api, string buildName, string version, IEnumerable<Tuple<string, string>> packages)
        {
            A.CallTo(() => api.GetNuGetDependenciesByBuildTypeAndBuildId(buildName, version))
                .Returns(packages.Select(p => new TeamCityApi.PackageDetails {Id = p.Item1,Version = p.Item2}).ToList());
        }

        private static IEnumerable<ChangeDetail> SetupBuildTypeAndBuilds(ITeamCityApi api, string buildType, string buildName)
        {
            A.CallTo(() => api.GetBuildDetailsByBuildId("bt1")).Returns(new BuildDetails {BuildTypeId = buildType, Name = buildName, Id = buildType});
            var builds = Enumerable.Range(1, 15).Select(i => new Build() {BuildTypeId = buildType, Id = i.ToString(), Number = string.Format("1.{0}", i)});
            var changeDetails = Enumerable.Range(1, 15).Select(i => new ChangeDetail 
            {
                Comment = Faker.Company.CatchPhrase(), 
                Id = i.ToString(),
                Username = Faker.Name.FullName(),
                Version = (100+i*10+Faker.RandomNumber.Next(9)).ToString(),
                Files = Enumerable.Range(1,Faker.RandomNumber.Next(1,50)).Select(j => new FileDetails 
                {
                        beforerevision = Faker.RandomNumber.Next(50,500).ToString(),
                        afterrevision = Faker.RandomNumber.Next(450, 700).ToString(),
                        File = Path.GetTempFileName(),
                        relativefile = Path.GetTempFileName()
                }).ToList(),
            });
            A.CallTo(() => api.GetBuildsByBuildType(buildType)).Returns(builds);
            return changeDetails;
        }


    }
}
