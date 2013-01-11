using System;
using System.Collections.Generic;
using System.Linq;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges.Commands
{
    public class AggregateBuildDeltaResolver
    {
        private readonly TeamCityApi _api;
        private readonly IEnumerable<IExternalIssueResolver> _externalIssueResolvers;

        public AggregateBuildDeltaResolver(TeamCityApi api, IEnumerable<IExternalIssueResolver> externalIssueResolvers)
        {
            _api = api;
            _externalIssueResolvers = externalIssueResolvers;
        }

        public ChangeManifest CreateChangeManifest(string buildName, string buildType, string referenceBuild = null, string from = null, string to = null, string projectName = null, bool useBuildSystemIssueResolution = true)
        {
            var changeManifest = new ChangeManifest();
            var changeDetails = new List<ChangeDetail>();
            var issues = new List<Issue>();

            buildType = buildType ?? ResolveBuildTypeId(projectName, buildName);

            if (String.IsNullOrEmpty(from))
            {
                from = ResolveFromVersion(buildType);
            }

            if (String.IsNullOrEmpty(to))
            {
                to = ResolveToVersion(buildType);
            }

            var buildWithCommitData = referenceBuild ?? buildType;
            var buildTypeDetails = _api.GetBuildTypeDetailsById(buildType);
            var referenceBuildTypeDetails = !String.IsNullOrEmpty(referenceBuild)
                                                ? _api.GetBuildTypeDetailsById(referenceBuild)
                                                : null;
            //TODO TFS collection data should come from the BuildType/VCS root data from TeamCity...but not for now...
            if (!String.IsNullOrEmpty(from) && !String.IsNullOrEmpty(to) && !String.IsNullOrEmpty(buildWithCommitData))
            {
                var builds = _api.GetBuildsByBuildType(buildWithCommitData);
                if (builds != null)
                {
                    var buildList = builds as List<Build> ?? builds.ToList();
                    changeDetails = _api.GetChangeDetailsByBuildTypeAndBuildNumber(buildWithCommitData, from, to, buildList).ToList();
                    var issueDetailResolver = new IssueDetailResolver(_externalIssueResolvers);
                    
                    //Rather than use TeamCity to resolve the issue to commit details (via TeamCity plugins) use the issue resolvers directly...
                    if (useBuildSystemIssueResolution)
                        issues = _api.GetIssuesByBuildTypeAndBuildRange(buildWithCommitData, from, to, buildList).ToList();
                    else
                        issues = issueDetailResolver.GetAssociatedIssues(changeDetails).ToList();

                    var initialPackages = _api.GetNuGetDependenciesByBuildTypeAndBuildId(buildType, from);
                    var finalPackages = _api.GetNuGetDependenciesByBuildTypeAndBuildId(buildType, to);
                    var packagesChanges = GetPackageChanges(initialPackages, finalPackages);

                    var issueDetails = issueDetailResolver.GetExternalIssueDetails(issues);

                    changeManifest.NuGetPackageChanges = packagesChanges;
                    changeManifest.ChangeDetails.AddRange(changeDetails);
                    changeManifest.IssueDetails.AddRange(issueDetails);
                    changeManifest.Generated = DateTime.Now;
                    changeManifest.FromVersion = from;
                    changeManifest.ToVersion = to;
                    changeManifest.BuildConfiguration = buildTypeDetails;
                    changeManifest.ReferenceBuildConfiguration = referenceBuildTypeDetails ?? new BuildTypeDetails();
                }
            }


            return changeManifest;
        }

        private static List<NuGetPackageChange> GetPackageChanges(List<TeamCityApi.PackageDetails> initialPackages, List<TeamCityApi.PackageDetails> finalPackages)
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

        private string ResolveToVersion(string buildType)
        {
            string to;
            var runningBuild = _api.GetRunningBuildByBuildType(buildType).FirstOrDefault();
            if (runningBuild != null)
                to = runningBuild.Number;
            else
                throw new ApplicationException(String.Format("Could not resolve a build number for the running build."));
            return to;
        }

        private string ResolveFromVersion(string buildType)
        {
            string from;
            var latestSuccesfull = _api.GetLatestSuccesfulBuildByBuildType(buildType);
            if (latestSuccesfull != null)
                from = latestSuccesfull.Number;
            else
                throw new ApplicationException(String.Format("Could not find latest build for build type {0}", buildType));
            return from;
        }

        private string ResolveBuildTypeId(string projectName, string buildName)
        {
            if (String.IsNullOrEmpty(projectName) || String.IsNullOrEmpty(buildName))
            {
                throw new ApplicationException(String.Format("Could not resolve Project: {0} and BuildName:{1} to a build type", projectName, buildName));
            }
            var resolvedBuildType = _api.GetBuildTypeByProjectAndName(projectName, buildName).FirstOrDefault();
            if (resolvedBuildType != null) 
                return resolvedBuildType.Id;

            return String.Empty;
        }
    }
}