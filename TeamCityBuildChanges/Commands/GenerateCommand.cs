using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManyConsole;
using ServiceStack.CacheAccess;
using TeamCityBuildChanges.ExternalApi.Jira;
using TeamCityBuildChanges.ExternalApi.TFS;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;
using TeamCityBuildChanges.NuGetPackage;
using TeamCityBuildChanges.Output;
using ServiceStack.CacheAccess.Providers;

namespace TeamCityBuildChanges.Commands
{
    internal class GenerateCommand : ConsoleCommand
    {
        private string _from;
        private string _referenceBuild;
        private string _to;
        private bool _useBuildSystemIssueResolution = true;
        private string _teamCityAuthToken;
        private string _buildPackageCacheFile;
        private bool _recurse;
        private string _branchName;
        private string _outputFileName;
        private ChangeManifest _changeManifest = new ChangeManifest();
        private string _serverName;
        private string _buildType;
        private string _projectName;
        private string _buildName;
        private string _jiraUrl;
        private string _jiraToken;
        private string _tfsUrl;
        private string _template;


        public GenerateCommand()
        {
            IsCommand("generate", "Provides a set of changes between two specific versions of a build type.");
            Options.Add("rb=|referencebuild=", "Reference build to query resolved version deltas from", s => _referenceBuild = s);
            Options.Add("f|from=", "Build number to start checking from (optional - detects the last successful build number if omitted)", x => _from = x);
            Options.Add("t|to=", "The build to check the delta change to", x => _to = x);
            Options.Add("directissueresolution|d", "Force issues to be resolved directly instead of via build system (if TFS -> queries commits directly against the TFS API to get Work Items / Issues, if JIRA -> query commit comments for issue IDs)", c => _useBuildSystemIssueResolution = false);
            Options.Add("tat=", "TeamCity Auth Token", c => _teamCityAuthToken = c);
            Options.Add("bpc|buildpackagecache=", "An xml build package cache file for package to build mapping.", c => _buildPackageCacheFile = c);
            Options.Add("r|recurse", "Recurse into package dependencies and generate full tree delta.", c => _recurse = c != null);
            Options.Add("br=|branch=", "The specific branch name in TeamCity", x => _branchName = x);
            Options.Add("o|output=", "OutputFileName filename (otherwise to console)", x => _outputFileName = x);
            Options.Add("b=|buildType=", "TeamCity build type to get the details for.", s => _buildType = s);
            Options.Add("p=|project=", "TeamCity project to search within for specific build name.", s => _projectName = s);
            Options.Add("bn=|buildName=", "TeamCity build type to get the details for.", s => _buildName = s);
            Options.Add("jiraurl=", "The Jira URL to query for issue details", x => _jiraUrl = x);
            Options.Add("jiraauthtoken=", "The Jira authorisation token to use (refer to 'encode' subcommand", x => _jiraToken = x);
            Options.Add("tfsurl=", "TFS URL to check issues on.", x => _tfsUrl = x);
            Options.Add("template=", "Template to use for output.  Must be a Razor template that accepts a ChangeManifest model.", x => _template = x);

            HasRequiredOption("s|server=", "TeamCity server to target (just use base URL and have guestAuth enabled", s => _serverName = s);
            SkipsCommandSummaryBeforeRunning();
        }

        public override int Run(string[] remainingArguments)
        {
            ICacheClient client = new MemoryCacheClient();
            var api = new TeamCityApi(new AuthenticatedRestClientFactory(_serverName, _teamCityAuthToken), client);

            var buildPackageCache = string.IsNullOrEmpty(_buildPackageCacheFile) ? null : new PackageBuildMappingCache(_buildPackageCacheFile);

            var issueDetailResolver = new IssueDetailResolver(CreateExternalIssueResolvers());

            var resolver = new AggregateBuildDeltaResolver(api, issueDetailResolver, new PackageChangeComparator(), buildPackageCache, new ConcurrentBag<NuGetPackageChange>());
            _changeManifest = string.IsNullOrEmpty(_buildType) 
                ? resolver.CreateChangeManifestFromBuildTypeName(_projectName, _buildName,_referenceBuild, _from, _to, _useBuildSystemIssueResolution, _recurse, _branchName)
                : resolver.CreateChangeManifestFromBuildTypeId(_buildType, _referenceBuild, _from, _to, _useBuildSystemIssueResolution, _recurse, _branchName);

            OutputChanges(CreateOutputRenderers(), new List<Action<string>> {Console.Write, a =>
                {
                    if (!string.IsNullOrEmpty(_outputFileName))
                        File.WriteAllText(_outputFileName, a);
                }});
            return 0;
        }

        private void OutputChanges(IEnumerable<IOutputRenderer> renderers, IEnumerable<Action<string>> writers)
        {
            var enumerable = writers as List<Action<string>> ?? writers.ToList();
            foreach (var renderer in renderers)
            {
                foreach (var writer in enumerable)
                {
                    writer(renderer.Render(_changeManifest));
                }
            }
        }

        private IEnumerable<IExternalIssueResolver> CreateExternalIssueResolvers()
        {
            var resolvers = new List<IExternalIssueResolver>();

            if (!string.IsNullOrEmpty(_jiraUrl))
            {
                resolvers.Add(new JiraIssueResolver(new JiraApi(_jiraUrl, _jiraToken)));
            }
            if (!string.IsNullOrEmpty(_tfsUrl))
            {
                resolvers.Add(new TFSIssueResolver(new TfsApi(_tfsUrl)));
            }
            return resolvers;
        }

        private IEnumerable<IOutputRenderer> CreateOutputRenderers()
        {
            var renderers = new List<IOutputRenderer>
                {
                    string.IsNullOrEmpty(_template) ? new RazorOutputRenderer() : new RazorOutputRenderer(_template)
                };

            return renderers;
        }
    }
}