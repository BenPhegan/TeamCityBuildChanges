using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.NuGetPackage;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges.Commands
{
    internal class AggregateBuildDeltaCommand : TeamCityCommandBase
    {
        private string _from;
        private string _referenceBuild;
        private string _to;
        private string _zeroChangesComment;
        private bool _useBuildSystemIssueResolution = true;
        private string _teamCityAuthToken;
        private string _buildPackageCacheFile;
        private bool _recurse;
        private string _xmlOutput;

        public AggregateBuildDeltaCommand()
        {
            IsCommand("aggregatebuilddelta", "Provides a set of changes between two specific versions of a build type.");
            Options.Add("rb=|referencebuild=", "Reference build to query resolved version deltas from", s => _referenceBuild = s);
            Options.Add("f|from=", "Build number to start checking from (optional - detects the last successful build number if omitted)", x => _from = x);
            Options.Add("t|to=", "The build to check the delta change to", x => _to = x);
            Options.Add("zerochangescomment=", "If there are no changes detected, add the provided comment rather than leave it null", x => _zeroChangesComment = x);
            Options.Add("directissueresolution|d", "Force issues to be resolved directly instead of via build system (if TFS -> queries commits directly against the TFS API to get Work Items / Issues, if JIRA -> query commit comments for issue IDs)", c => _useBuildSystemIssueResolution = false);
            Options.Add("tat=", "TeamCity Auth Token", c => _teamCityAuthToken = c);
            Options.Add("bpc|buildpackagecache=", "An xml build package cache file for package to build mapping.", c => _buildPackageCacheFile = c);
            Options.Add("r|recurse", "Recurse into package dependencies and generate full tree delta.", c => _recurse = c != null);
            Options.Add("xml=", "Serialize ChangeManifest object to xml", c => _xmlOutput = c);
            SkipsCommandSummaryBeforeRunning();
        }

        public override int Run(string[] remainingArguments)
        {
            var api = string.IsNullOrEmpty(_teamCityAuthToken) ? new TeamCityApi(ServerName) : new TeamCityApi(ServerName,_teamCityAuthToken);

            var buildPackageCache = string.IsNullOrEmpty(_buildPackageCacheFile) ? null : new PackageBuildMappingCache(_buildPackageCacheFile);

            var resolver = new AggregateBuildDeltaResolver(api, CreateExternalIssueResolvers(), new PackageChangeComparator(), buildPackageCache, new List<NuGetPackageChange>());
            ChangeManifest = string.IsNullOrEmpty(BuildType)
                ? resolver.CreateChangeManifestFromBuildTypeName(ProjectName, BuildName, _referenceBuild, _from, _to, _useBuildSystemIssueResolution, _recurse)
                : resolver.CreateChangeManifestFromBuildTypeId(BuildType, _referenceBuild, _from, _to, _useBuildSystemIssueResolution, _recurse);
            
            if (!string.IsNullOrEmpty(_xmlOutput))
                SerializeToXML(ChangeManifest);

            OutputChanges(CreateOutputRenderers(), new List<Action<string>> {Console.Write, a =>
                {
                    if (!string.IsNullOrEmpty(OutputFileName))
                        File.WriteAllText(OutputFileName, a);
                }});
            return 0;
        }

        private void SerializeToXML(ChangeManifest changeManifest)
        {
            var serializer = new XmlSerializer(typeof (ChangeManifest));
            var textWriter = new StreamWriter(_xmlOutput);
            serializer.Serialize(textWriter, changeManifest);
            textWriter.Close();
        }
    }
}