using System;
using System.Collections.Generic;
using System.IO;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.NuGetPackage;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges.Commands
{
// ReSharper disable UnusedMember.Global
    internal class AggregateBuildDeltaCommand : TeamCityCommandBase
// ReSharper restore UnusedMember.Global
    {
        private string _from;
        private string _referenceBuild;
        private string _to;
        private bool _useBuildSystemIssueResolution = true;
        private string _teamCityAuthToken;
        private string _buildPackageCacheFile;
        private bool _recurse;
        private string _serializeType = "xml";
        private string _serializeOutput;

        public AggregateBuildDeltaCommand()
        {
            IsCommand("aggregatebuilddelta", "Provides a set of changes between two specific versions of a build type.");
            Options.Add("rb=|referencebuild=", "Reference build to query resolved version deltas from", s => _referenceBuild = s);
            Options.Add("f|from=", "Build number to start checking from (optional - detects the last successful build number if omitted)", x => _from = x);
            Options.Add("t|to=", "The build to check the delta change to", x => _to = x);
            Options.Add("directissueresolution|d", "Force issues to be resolved directly instead of via build system (if TFS -> queries commits directly against the TFS API to get Work Items / Issues, if JIRA -> query commit comments for issue IDs)", c => _useBuildSystemIssueResolution = false);
            Options.Add("tat=", "TeamCity Auth Token", c => _teamCityAuthToken = c);
            Options.Add("bpc|buildpackagecache=", "An xml build package cache file for package to build mapping.", c => _buildPackageCacheFile = c);
            Options.Add("r|recurse", "Recurse into package dependencies and generate full tree delta.", c => _recurse = c != null);
            Options.Add("serializeType=", "Serialize ChangeManifest object to type (default: xml)", c => _serializeType = c);
            Options.Add("serializeOutput=", "Serialize ChangeManifest object to filename", c => _serializeOutput = c);
            SkipsCommandSummaryBeforeRunning();
        }

        public override int Run(string[] remainingArguments)
        {
            var api = string.IsNullOrEmpty(_teamCityAuthToken) ? new TeamCityApi(ServerName, new MemoryBasedBuildCache()) : new TeamCityApi(ServerName, new MemoryBasedBuildCache(), _teamCityAuthToken);

            var buildPackageCache = string.IsNullOrEmpty(_buildPackageCacheFile) ? null : new PackageBuildMappingCache(_buildPackageCacheFile);

            var resolver = new AggregateBuildDeltaResolver(api, CreateExternalIssueResolvers(), new PackageChangeComparator(), buildPackageCache, new List<NuGetPackageChange>());
            ChangeManifest = string.IsNullOrEmpty(BuildType)
                ? resolver.CreateChangeManifestFromBuildTypeName(ProjectName, BuildName, _referenceBuild, _from, _to, _useBuildSystemIssueResolution, _recurse)
                : resolver.CreateChangeManifestFromBuildTypeId(BuildType, _referenceBuild, _from, _to, _useBuildSystemIssueResolution, _recurse);
            
            if (!string.IsNullOrEmpty(_serializeOutput))
                SerializeManifest(ChangeManifest, _serializeType, _serializeOutput);

            OutputChanges(CreateOutputRenderers(), new List<Action<string>> {Console.Write, a =>
                {
                    if (!string.IsNullOrEmpty(OutputFileName))
                        File.WriteAllText(OutputFileName, a);
                }});
            return 0;
        }
    }
}