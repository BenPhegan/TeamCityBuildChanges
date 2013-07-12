using System;
using System.Collections.Generic;
using System.Linq;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;
using TeamCityBuildChanges.NuGetPackage;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges.Commands
{
    /// <summary>
    /// Calculates ChangeManifest objects based on TeamCity builds.
    /// </summary>
    public class AggregateBuildDeltaResolver
    {
        private readonly ITeamCityApi _api;
        private readonly IEnumerable<IExternalIssueResolver> _externalIssueResolvers;
        private readonly IPackageChangeComparator _packageChangeComparator;
        private readonly IPackageBuildMappingCache _packageBuildMappingCache;
        private List<NuGetPackageChange> _traversedPackageChanges;

        /// <summary>
        /// Provides the ability to generate delta change manifests between arbitrary build versions.
        /// </summary>
        /// <param name="api">A TeamCityApi.</param>
        /// <param name="externalIssueResolvers">A list of IExternalIssueResolver objects.</param>
        /// <param name="packageChangeComparator">Provides package dependency comparison capability.</param>
        /// <param name="packageBuildMappingCache">Provides the ability to map from a Nuget package to the build that created the package.</param>
        public AggregateBuildDeltaResolver(ITeamCityApi api, IEnumerable<IExternalIssueResolver> externalIssueResolvers, IPackageChangeComparator packageChangeComparator, IPackageBuildMappingCache packageBuildMappingCache, List<NuGetPackageChange> traversedPackageChanges)
        {
            _api = api;
            _externalIssueResolvers = externalIssueResolvers;
            _packageChangeComparator = packageChangeComparator;
            _packageBuildMappingCache = packageBuildMappingCache;
            _traversedPackageChanges = traversedPackageChanges;
        }

        /// <summary>
        /// Creates a change manifest based on a build name and a project.
        /// </summary>
        /// <param name="projectName">The project name</param>
        /// <param name="buildName">The build type name to use.</param>
        /// <param name="referenceBuild">Any reference build that provides the actual build information.</param>
        /// <param name="from">The From build number</param>
        /// <param name="to">The To build number</param>
        /// <param name="useBuildSystemIssueResolution">Uses the issues resolved by the build system at time of build, rather than getting them directly from the version control system.</param>
        /// <param name="recurse">Recurses down through any detected package dependency changes.</param>
        /// <returns>The calculated ChangeManifest object.</returns>
        public ChangeManifest CreateChangeManifestFromBuildTypeName(string projectName, string buildName, string referenceBuild = null, string @from = null, string to = null, bool useBuildSystemIssueResolution = true, bool recurse = false)
        {
            return CreateChangeManifest(buildName, null, referenceBuild, from, to, projectName, useBuildSystemIssueResolution, recurse);
        }

        /// <summary>
        /// Creates a change manifest based on a build name and a project.
        /// </summary>
        /// <param name="buildType">The Build Type ID to work on.</param>
        /// <param name="referenceBuild">Any reference build that provides the actual build information.</param>
        /// <param name="from">The From build number</param>
        /// <param name="to">The To build number</param>
        /// <param name="useBuildSystemIssueResolution">Uses the issues resolved by the build system at time of build, rather than getting them directly from the version control system.</param>
        /// <param name="recurse">Recurses down through any detected package dependency changes.</param>
        /// <returns>The calculated ChangeManifest object.</returns>
        public ChangeManifest CreateChangeManifestFromBuildTypeId(string buildType, string referenceBuild = null, string from = null, string to = null, bool useBuildSystemIssueResolution = true, bool recurse = false)
        {
            return CreateChangeManifest(null, buildType, referenceBuild, from, to, null, useBuildSystemIssueResolution, recurse);
        }

        private ChangeManifest CreateChangeManifest(string buildName, string buildType, string referenceBuild = null, string from = null, string to = null, string projectName = null, bool useBuildSystemIssueResolution = true, bool recurse = false)
        {
            var changeManifest = new ChangeManifest();
            if (recurse && _packageBuildMappingCache == null)
            {
                changeManifest.GenerationLog.Add(new LogEntry(DateTime.Now,Status.Warning,"Recurse option provided with no PackageBuildMappingCache, we will not be honoring the Recurse option."));
                changeManifest.GenerationStatus = Status.Warning;
            }

            buildType = buildType ?? ResolveBuildTypeId(projectName, buildName);

            if (String.IsNullOrEmpty(from))
            {
                changeManifest.GenerationLog.Add(new LogEntry(DateTime.Now, Status.Warning, "Resolving FROM version based on the provided BuildType (FROM was not provided)."));
                from = ResolveFromVersion(buildType);
            }

            if (String.IsNullOrEmpty(to))
            {
                changeManifest.GenerationLog.Add(new LogEntry(DateTime.Now, Status.Warning, "Resolving TO version based on the provided BuildType (TO was not provided)."));
                to = ResolveToVersion(buildType);
            }

            var buildWithCommitData = referenceBuild ?? buildType;
            var buildTypeDetails = _api.GetBuildTypeDetailsById(buildType);
            var referenceBuildTypeDetails = !String.IsNullOrEmpty(referenceBuild) ? _api.GetBuildTypeDetailsById(referenceBuild) : null;

            if (!String.IsNullOrEmpty(from) && !String.IsNullOrEmpty(to) && !String.IsNullOrEmpty(buildWithCommitData))
            {
                changeManifest.GenerationLog.Add(new LogEntry(DateTime.Now, Status.Ok, "Getting builds based on BuildType"));
                var builds = _api.GetBuildsByBuildType(buildWithCommitData);
                if (builds != null)
                {
                    var buildList = builds as List<Build> ?? builds.ToList();
                    changeManifest.GenerationLog.Add(new LogEntry(DateTime.Now,Status.Ok, string.Format("Got {0} builds for BuildType {1}.",buildList.Count(), buildType)));
                    var changeDetails =_api.GetChangeDetailsByBuildTypeAndBuildNumber(buildWithCommitData, @from, to, buildList).ToList();
                    var issueDetailResolver = new IssueDetailResolver(_externalIssueResolvers);

                    //Rather than use TeamCity to resolve the issue to commit details (via TeamCity plugins) use the issue resolvers directly...
                    var issues = useBuildSystemIssueResolution
                                     ? _api.GetIssuesByBuildTypeAndBuildRange(buildWithCommitData, @from, to, buildList).ToList()
                                     : issueDetailResolver.GetAssociatedIssues(changeDetails).ToList();

                    changeManifest.GenerationLog.Add(new LogEntry(DateTime.Now,Status.Ok, string.Format("Got {0} issues for BuildType {1}.", issues.Count(),buildType)));

                    changeManifest.GenerationLog.Add(new LogEntry(DateTime.Now, Status.Ok, "Checking package dependencies."));
                    var buildFrom = buildList.FirstOrDefault(b => b.Number == @from);
                    var buildTo = buildList.FirstOrDefault(b => b.Number == to);
                    var initialPackages = new List<TeamCityApi.PackageDetails>();
                    var finalPackages = new List<TeamCityApi.PackageDetails>();
                    if (buildFrom != null)
                        initialPackages = _api.GetNuGetDependenciesByBuildTypeAndBuildId(buildType,buildFrom.Id).ToList();
                    if (buildTo != null)
                        finalPackages = _api.GetNuGetDependenciesByBuildTypeAndBuildId(buildType, buildTo.Id).ToList();

                    var packageChanges = _packageChangeComparator.GetPackageChanges(initialPackages, finalPackages).ToList();

                    var issueDetails = issueDetailResolver.GetExternalIssueDetails(issues);

                    changeManifest.NuGetPackageChanges = packageChanges;
                    changeManifest.ChangeDetails.AddRange(changeDetails);
                    changeManifest.IssueDetails.AddRange(issueDetails);
                    changeManifest.Generated = DateTime.Now;
                    changeManifest.FromVersion = @from;
                    changeManifest.ToVersion = to;
                    changeManifest.BuildConfiguration = buildTypeDetails;
                    changeManifest.ReferenceBuildConfiguration = referenceBuildTypeDetails ?? new BuildTypeDetails();
                }
                else
                {
                    changeManifest.GenerationLog.Add(new LogEntry(DateTime.Now, Status.Warning, string.Format("No builds returned for BuildType {0}.", buildType)));
                }
            }
            //Now we need to see if we need to recurse, and whether we have been given a cache file....
            if (changeManifest.NuGetPackageChanges.Any() && recurse && _packageBuildMappingCache != null)
            {
                foreach (var dependency in changeManifest.NuGetPackageChanges.Where(c => c.Type == NuGetPackageChangeType.Modified))
                {
                    var traversedDependency = _traversedPackageChanges.FirstOrDefault(p => p.NewVersion == dependency.NewVersion && p.OldVersion == dependency.OldVersion && p.PackageId == dependency.PackageId);
                    if (traversedDependency != null)
                    {
                        dependency.ChangeManifest = traversedDependency.ChangeManifest;
                        continue;
                    }

                    var build = RetrieveBuild(_api, _packageBuildMappingCache, dependency, changeManifest);

                    if (build != null)
                    {
                        if (build.BuildConfigurationId == buildType)
                            continue;
                        dependency.ChangeManifest = CreateChangeManifest(null, build.BuildConfigurationId, null, dependency.OldVersion, dependency.NewVersion, null, true, true); ;
                    }
                    else
                    {
                        changeManifest.GenerationLog.Add(new LogEntry(DateTime.Now, Status.Warning, string.Format("Did not find a mapping for package: {0}.", dependency.PackageId)));
                    }
                    _traversedPackageChanges.Add(dependency);
                }
            }

            return changeManifest;
        }

        public PackageBuildMapping RetrieveBuild(ITeamCityApi api, IPackageBuildMappingCache packageBuildMappingCache, NuGetPackageChange dependency, ChangeManifest changeManifest)
        {
            var mappings = packageBuildMappingCache.PackageBuildMappings.Where(m => m.PackageId.Equals(dependency.PackageId, StringComparison.CurrentCultureIgnoreCase)).ToList();
            PackageBuildMapping build = null;
            if (mappings.Count == 1)
            {
                //We only got one back, this is good...
                build = mappings.First();
                changeManifest.GenerationLog.Add(new LogEntry(DateTime.Now, Status.Ok, string.Format("Found singular packages to build mapping {0}.", build.BuildConfigurationName)));
            }
            else if (mappings.Any())
            {
                //Because there are STILL multiple builds, now we have to troll along and query TeamCity for the correct build...
                var buildDetails = api.GetBuildDetailsFromBuildNumber(mappings.Select(map => map.BuildConfigurationId), dependency.NewVersion);
                var buildTypeId = buildDetails.BuildTypeId;
                var blah = mappings.FirstOrDefault(b => b.BuildConfigurationId == buildTypeId);
                build = mappings.FirstOrDefault(b => b.BuildConfigurationId == api.GetBuildDetailsFromBuildNumber(mappings.Select(map => map.BuildConfigurationId), dependency.NewVersion).BuildTypeId);
                if (build != null)
                    changeManifest.GenerationLog.Add(new LogEntry(DateTime.Now, Status.Warning, string.Format("Found duplicate mappings, using package to build mapping {0}.", build.BuildConfigurationName)));
            }
            return build;
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