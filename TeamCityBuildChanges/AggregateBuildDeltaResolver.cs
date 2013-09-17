using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack.CacheAccess.Providers;
using TeamCityBuildChanges.ExternalApi;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;
using TeamCityBuildChanges.NuGetPackage;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges
{
    /// <summary>
    /// Calculates ChangeManifest objects based on TeamCity builds.
    /// </summary>
    public class AggregateBuildDeltaResolver
    {
        private readonly ITeamCityApi _api;
        private readonly IPackageChangeComparator _packageChangeComparator;
        private readonly PackageBuildMappingCache _packageBuildMappingCache;
        private readonly ConcurrentBag<NuGetPackageChange> _traversedPackageChanges;
        private readonly IIssueDetailResolver _issueDetailResolver;

        /// <summary>
        /// Provides the ability to generate delta change manifests between arbitrary build versions.
        /// </summary>
        /// <param name="api">A TeamCityApi.</param>
        /// <param name="issueDetailResolver"></param>
        /// <param name="packageChangeComparator">Provides package dependency comparison capability.</param>
        /// <param name="packageBuildMappingCache">Provides the ability to map from a Nuget package to the build that created the package.</param>
        /// <param name="traversedPackageChanges">Packages changes that we have already calculated and can reuse.</param>
        public AggregateBuildDeltaResolver(ITeamCityApi api, IIssueDetailResolver issueDetailResolver, IPackageChangeComparator packageChangeComparator, PackageBuildMappingCache packageBuildMappingCache, ConcurrentBag<NuGetPackageChange> traversedPackageChanges)
        {
            _api = api;
            _issueDetailResolver = issueDetailResolver;
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
        /// <param name="branchName">Name of the branch.</param>
        /// <returns>
        /// The calculated ChangeManifest object.
        /// </returns>
        public ChangeManifest CreateChangeManifestFromBuildTypeName(string projectName, string buildName, string referenceBuild = null, string @from = null, string to = null, bool useBuildSystemIssueResolution = true, bool recurse = false, string branchName = null)
        {
            return CreateChangeManifest(buildName, null, referenceBuild, from, to, projectName, useBuildSystemIssueResolution, recurse, branchName);
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
        /// <param name="branchName">Name of the branch.</param>
        /// <returns>
        /// The calculated ChangeManifest object.
        /// </returns>
        public ChangeManifest CreateChangeManifestFromBuildTypeId(string buildType, string referenceBuild = null, string from = null, string to = null, bool useBuildSystemIssueResolution = true, bool recurse = false, string branchName = null)
        {
            return CreateChangeManifest(null, buildType, referenceBuild, from, to, null, useBuildSystemIssueResolution, recurse, branchName);
        }

        private ChangeManifest CreateChangeManifest(string buildName, string buildType, string referenceBuild = null, string from = null, string to = null, string projectName = null, bool useBuildSystemIssueResolution = true, bool recurse = false, string branchName = null)
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
                from = ResolveFromVersion(buildType, branchName);
            }

            if (String.IsNullOrEmpty(to))
            {
                changeManifest.GenerationLog.Add(new LogEntry(DateTime.Now, Status.Warning, "Resolving TO version based on the provided BuildType (TO was not provided)."));
                to = ResolveToVersion(buildType, branchName);
            }

            var buildWithCommitData = referenceBuild ?? buildType;
            var buildTypeDetails = _api.GetBuildTypeDetailsById(buildType);
            var referenceBuildTypeDetails = !String.IsNullOrEmpty(referenceBuild) ? _api.GetBuildTypeDetailsById(referenceBuild) : null;

            if (!String.IsNullOrEmpty(from) && !String.IsNullOrEmpty(to) && !String.IsNullOrEmpty(buildWithCommitData))
            {
                changeManifest.GenerationLog.Add(new LogEntry(DateTime.Now, Status.Ok, "Getting builds based on BuildType"));
                var builds = _api.GetBuildsByBuildType(buildWithCommitData, branchName);
                if (builds != null)
                {
                    var buildList = builds as List<Build> ?? builds.ToList();
                    changeManifest.GenerationLog.Add(new LogEntry(DateTime.Now,Status.Ok, string.Format("Got {0} builds for BuildType {1}.",buildList.Count(), buildType)));
                    var changeDetails =_api.GetChangeDetailsByBuildTypeAndBuildNumber(buildWithCommitData, @from, to, buildList, branchName).ToList();

                    //Rather than use TeamCity to resolve the issue to commit details (via TeamCity plugins) use the issue resolvers directly...
                    var issues = useBuildSystemIssueResolution
                                     ? _api.GetIssuesByBuildTypeAndBuildRange(buildWithCommitData, @from, to, buildList, branchName).ToList()
                                     : _issueDetailResolver.GetAssociatedIssues(changeDetails).ToList();

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

                    var issueDetails = _issueDetailResolver.GetExternalIssueDetails(issues);

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
            var modifiedPackages = changeManifest.NuGetPackageChanges.Where(c => c.Type == NuGetPackageChangeType.Modified).ToList();
            if (!modifiedPackages.Any() || !recurse || _packageBuildMappingCache == null) return changeManifest;

            Parallel.ForEach(modifiedPackages, dependency =>
            {
                var traversedDependency =
                    _traversedPackageChanges.FirstOrDefault(
                        p => p.NewVersion == dependency.NewVersion && p.OldVersion == dependency.OldVersion && p.PackageId == dependency.PackageId);
                if (traversedDependency != null)
                {
                    dependency.ChangeManifest = traversedDependency.ChangeManifest;
                    return;
                }
                var mappings =
                    _packageBuildMappingCache.PackageBuildMappings.Where(
                        m => m.PackageId.Equals(dependency.PackageId, StringComparison.CurrentCultureIgnoreCase)).ToList();
                PackageBuildMapping build = null;
                if (mappings.Count == 1)
                {
                    //We only got one back, this is good...
                    build = mappings.First();
                    changeManifest.GenerationLog.Add(new LogEntry(DateTime.Now, Status.Ok,
                        string.Format("Found singular packages to build mapping {0}.", build.BuildConfigurationName)));
                }
                else if (mappings.Any())
                {
                    //Ok, so multiple builds are outputting this package, so we need to try and constrain on project...
                    build = mappings.FirstOrDefault(m => m.Project.Equals(buildTypeDetails.Project.Name, StringComparison.OrdinalIgnoreCase));
                    if (build != null)
                        changeManifest.GenerationLog.Add(new LogEntry(DateTime.Now, Status.Warning,
                            string.Format("Found duplicate mappings, using package to build mapping {0}.", build.BuildConfigurationName)));
                }

                if (build != null)
                {
                    if (build.BuildConfigurationId == buildType)
                        return;
                    //TODO if we are newing up a new RestClientFactory, we dont have a token for it...none passed in....
                    var instanceTeamCityApi = _api.Url.Equals(build.ServerUrl, StringComparison.OrdinalIgnoreCase)
                        ? _api
                        : new TeamCityApi(new CachingThreadSafeAuthenticatedRestClient(new MemoryCacheClient(), build.ServerUrl), new MemoryCacheClient());

                    var resolver = new AggregateBuildDeltaResolver(instanceTeamCityApi, _issueDetailResolver, _packageChangeComparator,
                        _packageBuildMappingCache, _traversedPackageChanges);
                    var dependencyManifest = resolver.CreateChangeManifest(null, build.BuildConfigurationId, null, dependency.OldVersion, dependency.NewVersion,
                        null, true, true, branchName);
                    dependency.ChangeManifest = dependencyManifest;
                }
                else
                {
                    changeManifest.GenerationLog.Add(new LogEntry(DateTime.Now, Status.Warning,
                        string.Format("Did not find a mapping for package: {0}.", dependency.PackageId)));
                }
                _traversedPackageChanges.Add(dependency);
            });

            return changeManifest;
        }

        private string ResolveToVersion(string buildType, string branchName = null)
        {
            var runningBuild = _api.GetRunningBuildByBuildType(buildType, branchName).FirstOrDefault();
            if (runningBuild != null)
            {
                return runningBuild.Number;
            }
            
            throw new ApplicationException(String.Format("Could not resolve a build number for the running build."));
        }

        private string ResolveFromVersion(string buildType, string branchName = null)
        {
            var latestSuccessful = _api.GetLatestSuccessfulBuildByBuildType(buildType, branchName);
            if (latestSuccessful != null)
            {
                return latestSuccessful.Number;
            }

            // fall back to the current running build
            var runningBuild = _api.GetRunningBuildByBuildType(buildType, branchName).FirstOrDefault();
            if (runningBuild != null)
            {
                return runningBuild.Number;
            }

            throw new ApplicationException(String.Format("Could not find <from> build for build type {0}", buildType));
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