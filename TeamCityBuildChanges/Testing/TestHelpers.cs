using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FakeItEasy;
using Faker;
using TeamCityBuildChanges.Commands;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;
using TeamCityBuildChanges.NuGetPackage;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges.Testing
{
    public class TestHelpers
    {
        public static AggregateBuildDeltaResolver CreateMockedAggregateBuildDeltaResolver(IEnumerable<BuildTemplate> buildTemplates)
        {
            const string apiServer = "http://test.server";

            var api = A.Fake<ITeamCityApi>();
            A.CallTo(() => api.TeamCityServer).Returns(apiServer);

            var packageCache = new PackageBuildMappingCache();

            var issueResolver = A.Fake<IExternalIssueResolver>();

            foreach (var template in buildTemplates)
            {
                SetExpectations(template, api, issueResolver, packageCache);
            }
            
            var resolver = new AggregateBuildDeltaResolver(api, new[] {issueResolver}, new PackageChangeComparator(), packageCache, new ConcurrentBag<NuGetPackageChange>());
            return resolver;
        }

        private static void SetExpectations(BuildTemplate template, ITeamCityApi api, IExternalIssueResolver issueResolver, PackageBuildMappingCache packageCache)
        {
            var startBuild = string.Format(template.BuildNumberPattern, template.StartBuildNumber);
            var finishBuild = string.Format(template.BuildNumberPattern, template.FinishBuildNumber);

            //BuildType/Builds/ChangeDetails
            A.CallTo(() => api.GetBuildTypeDetailsById(template.BuildId))
             .Returns(new BuildTypeDetails
                 {
                     Id = template.BuildId,
                     Name = template.BuildName,
                     Description = template.BuildName,
                 });

            var changeDetails = SetupBuildTypeAndBuilds(api, template);
            A.CallTo(() =>api.GetChangeDetailsByBuildTypeAndBuildNumber(template.BuildId, startBuild, finishBuild, A<IEnumerable<Build>>.Ignored, null))
             .Returns(changeDetails.Where(c => Convert.ToInt16(c.Id) > template.StartBuildNumber && Convert.ToInt16(c.Id) <= template.FinishBuildNumber).ToList());

            //Issues
            if (template.IssueCount > 0)
            {
                var issues = Enumerable.Range(1, template.IssueCount).Select(i => new Issue {Id = RandomNumber.Next(2000).ToString()}).ToList();
                
                A.CallTo(() => api.GetIssuesByBuildTypeAndBuildRange(template.BuildId, startBuild, finishBuild, A<IEnumerable<Build>>.Ignored, null))
                 .Returns(issues);

                A.CallTo(issueResolver).WithReturnType<IEnumerable<ExternalIssueDetails>>()
                 .Returns(CreateExternalIssueDetails(issues, template));
            }

            //NuGetPackages
            SetNugetPackageDependencyExpectations(api, packageCache, template, issueResolver);
            SetNugetPackageDependencyExpectations(api, packageCache, template, issueResolver);
        }

        private static IEnumerable<ExternalIssueDetails> CreateExternalIssueDetails(IEnumerable<Issue> issues, BuildTemplate template)
        {
            var returnIssues = issues.Select(CreateRandomExternalIssue);
            var tempIssues = new List<ExternalIssueDetails>();
            
            if (template.NestedIssueChance > 0 && template.NestedIssueDepth > 0)
            {
                for (int j = 0; j < template.NestedIssueDepth; j++)
                {
                    foreach (var externalIssueDetails in returnIssues.ToList())
                    {
                        if (RandomNumber.Next(0, 100) < template.NestedIssueChance)
                        {
                            var uniqueIssueId = Enumerable.Repeat(RandomNumber.Next(2000), 50)
                                .First(r => returnIssues.All(i => Convert.ToInt16(i.Id) != r));
                            var parentIssue = CreateRandomExternalIssue(new Issue {Id = uniqueIssueId.ToString()});
                            parentIssue.SubIssues.Add(externalIssueDetails);
                            tempIssues.Add(parentIssue);
                        }
                    }
                    returnIssues = tempIssues;
                    tempIssues = new List<ExternalIssueDetails>();
                }
            }
            return returnIssues;
        }

        private static ExternalIssueDetails CreateRandomExternalIssue(Issue i)
        {
            return new ExternalIssueDetails
                {
                    Id = i.Id,
                    Description = Company.BS(),
                    Created = DateTime.Today.AddDays(-RandomNumber.Next(150)).ToString(),
                    Status = new[]{"Open","Closed","Resolved"}[RandomNumber.Next(0,2)],
                    Comments = new List<string> {Company.BS(),Company.CatchPhrase(),Lorem.Sentence()},
                    Type = new[]{"Bug","Task","Issue"}[RandomNumber.Next(0,2)],
                    Summary = Company.CatchPhrase(),
                    Url = String.Format("http://{0}/issues/{1}",Internet.DomainName(),i),
                    SubIssues = new List<ExternalIssueDetails>()
                };
        }

        private static void SetNugetPackageDependencyExpectations(ITeamCityApi api, PackageBuildMappingCache cache, BuildTemplate template, IExternalIssueResolver issueResolver)
        {
            var initial = template.StartBuildPackages.Select(p => new TeamCityApi.PackageDetails {Id = p.Key, Version = p.Value}).ToList();
            var final = template.FinishBuildPackages.Select(p => new TeamCityApi.PackageDetails { Id = p.Key, Version = p.Value }).ToList();

            if (initial.Any())
            {
                A.CallTo(() => api.GetNuGetDependenciesByBuildTypeAndBuildId(template.BuildId, template.StartBuildNumber.ToString()))
                .Returns(initial);
            }
            
            if (initial.Any())
            {
                A.CallTo(() => api.GetNuGetDependenciesByBuildTypeAndBuildId(template.BuildId, template.FinishBuildNumber.ToString()))
                .Returns(final);
            }

            if (template.CreateNuGetPackageChangeManifests && initial.Any() && final.Any())
            {
                var packageDiffs = new PackageChangeComparator().GetPackageChanges(initial, final);
                foreach (var diff in packageDiffs.Where(d => d.Type == NuGetPackageChangeType.Modified))
                {
                    if (!cache.PackageBuildMappings.Any(c => c.PackageId.Equals(diff.PackageId) && c.BuildConfigurationId.Equals(diff.PackageId)))
                    {
                        cache.PackageBuildMappings.Add(new PackageBuildMapping
                        {
                            BuildConfigurationId = diff.PackageId,
                            BuildConfigurationName = diff.PackageId,
                            PackageId = diff.PackageId,
                            Project = diff.PackageId,
                            ServerUrl = api.TeamCityServer
                        });
                    }

                    SetExpectations(new BuildTemplate
                        {
                            BuildId = diff.PackageId,
                            BuildCount = 15,
                            BuildName = diff.PackageId,
                            BuildNumberPattern = "1.{0}",
                            CreateNuGetPackageChangeManifests = false,
                            StartBuildNumber = Convert.ToInt16(diff.OldVersion.Split('.')[1]),
                            FinishBuildNumber = Convert.ToInt16(diff.NewVersion.Split('.')[1]),
                            IssueCount = 1,
                            NestedIssueChance = 100,
                            NestedIssueDepth = 1,
                        },
                        api,
                        issueResolver,
                        cache);
                }
            }
        }

        private static IEnumerable<ChangeDetail> SetupBuildTypeAndBuilds(ITeamCityApi api, BuildTemplate template)
        {
            A.CallTo(() => api.GetBuildDetailsByBuildId(template.BuildId)).Returns(new BuildDetails {BuildTypeId = template.BuildId, Name = template.BuildName, Id = template.BuildId});
            var builds = Enumerable.Range(1, template.BuildCount).Select(i => new Build {BuildTypeId = template.BuildId, Id = i.ToString(), Number = String.Format(template.BuildNumberPattern, i)});
            var changeDetails = Enumerable.Range(1, template.BuildCount).Select(i => new ChangeDetail 
                {
                    Comment = Company.CatchPhrase(), 
                    Id = i.ToString(),
                    Username = Name.FullName(),
                    Version = (100+i*10+RandomNumber.Next(9)).ToString(),
                    Files = Enumerable.Range(1,RandomNumber.Next(1,50)).Select(j => new FileDetails 
                        {
                            beforerevision = RandomNumber.Next(50,500).ToString(),
                            afterrevision = RandomNumber.Next(450, 700).ToString(),
                            File = Path.GetTempFileName(),
                            relativefile = Path.GetTempFileName()
                        }).ToList(),
                });
            A.CallTo(() => api.GetBuildsByBuildType(template.BuildId, null)).Returns(builds);
            return changeDetails;
        }

        public static ChangeManifest CreateSimpleChangeManifest()
        {
            return new ChangeManifest
                {
                    Generated = DateTime.Now,
                    FromVersion = "1.0",
                    ToVersion = "1.1",
                    BuildConfiguration = new BuildTypeDetails
                        {
                            Description = "Test",
                            Name = "Test"
                        },
                    ReferenceBuildConfiguration = new BuildTypeDetails
                        {
                            Description = "Test",
                            Name = "Test"
                        },
                    ChangeDetails = new List<ChangeDetail>
                        {
                            new ChangeDetail
                                {
                                    Comment = "Test1",
                                    Id = "1"
                                },
                            new ChangeDetail
                                {
                                    Comment = "Test2",
                                    Id = "2"
                                }
                        },
                    IssueDetails = new List<ExternalIssueDetails>
                        {
                            new ExternalIssueDetails
                                {
                                    Id = "TEST-1",
                                    Status = "Open",
                                    SubIssues = new List<ExternalIssueDetails>
                                        {
                                            new ExternalIssueDetails
                                                {
                                                    Id = "TEST-3",
                                                    Status = "Resolved",
                                                }
                                        }
                                },
                            new ExternalIssueDetails
                                {
                                    Id = "Test-2",
                                    Status = "Resolved",
                                    SubIssues = new List<ExternalIssueDetails>
                                        {
                                            new ExternalIssueDetails
                                                {
                                                    Id = "TEST-5",
                                                    Status = "Resolved",
                                                }
                                        }
                                },
                            new ExternalIssueDetails
                                {
                                    Id = "TEST-1",
                                    Status = "Open",
                                    SubIssues = new List<ExternalIssueDetails>
                                        {
                                            new ExternalIssueDetails
                                                {
                                                    Id = "TEST-4",
                                                    Status = "Resolved",
                                                }
                                        }
                                }
                        }
                };
        }
    }
}