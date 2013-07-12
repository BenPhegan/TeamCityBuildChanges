using System.Collections.Generic;

namespace TeamCityBuildChanges.NuGetPackage
{
    public interface IPackageBuildMappingCache
    {
        event PackageBuildMappingCache.BuildCheckEventHandler StartedBuildCheck;
        event PackageBuildMappingCache.BuildCheckEventHandler FinishedBuildCheck;
        event PackageBuildMappingCache.ServerCheckEventHandler StartedServerCheck;
        event PackageBuildMappingCache.ServerCheckEventHandler FinishedServerCheck;

        /// <summary>
        /// Provides access to any PackageBuildMapping objects in the cache
        /// </summary>
        List<PackageBuildMapping> PackageBuildMappings { get; }

        /// <summary>
        /// Attempts to build a a list of PackageBuildMapping objects by interrogating a list of servers.
        /// </summary>
        /// <param name="servers">The servers to query.</param>
        /// <param name="useArtifactsNotPackageSteps">If set, ignore TeamCity NuGet build steps and use the existence of packages in the artifacts as proof of creation.</param>
        void BuildCache(List<string> servers, bool useArtifactsNotPackageSteps = false);

        /// <summary>
        /// Load a cache file.
        /// </summary>
        /// <param name="filename">Defaults to PackageBuildMapping.xml</param>
        void LoadCache(string filename = "PackageBuildMapping.xml");

        /// <summary>
        /// Saves a cache file.
        /// </summary>
        /// <param name="filename">Defaults to PackageBuildMapping.xml</param>
        void SaveCache(string filename = "PackageBuildMapping.xml");
    }
}