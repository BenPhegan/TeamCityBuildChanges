using System;
using System.Collections.Generic;
using System.IO;
using TeamCityBuildChanges.ExternalApi.TeamCity;

namespace TeamCityBuildChanges.Commands
{
    internal class AggregateBuildDelta : TeamCityCommandBase
    {
        private string _from;
        private string _referenceBuild;
        private string _to;
        private string _zeroChangesComment;

        public AggregateBuildDelta()
        {
            IsCommand("aggregatebuilddelta", "Provides a set of changes between two specific versions of a build type.");
            Options.Add("rb=|referencebuild=", "Reference build to query resolved version deltas from", s => _referenceBuild = s);
            Options.Add("f|from=", "Build number to start checking from (optional - detects the last successful build number if omitted)", x => _from = x);
            Options.Add("t|to=", "The build to check the delta change to", x => _to = x);
            Options.Add("zerochangescomment=", "If there are no changes detected, add the provided comment rather than leave it null", x => _zeroChangesComment = x);
            SkipsCommandSummaryBeforeRunning();
        }

        public override int Run(string[] remainingArguments)
        {
            var api = new TeamCityApi(ServerName);

            var resolver = new AggregateBuildDeltaResolver(api, CreateExternalIssueResolvers());
            ChangeManifest = resolver.CreateChangeManifest(BuildName, BuildType, _referenceBuild, _from, _to, ProjectName);

            OutputChanges(CreateOutputRenderers(), new List<Action<string>> {Console.Write, a =>
                {
                    if (!string.IsNullOrEmpty(OutputFileName))
                        File.WriteAllText(OutputFileName, a);
                }});
            return 0;
        }
    }
}