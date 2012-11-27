using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges.Commands
{
    class AggregateBuildDelta : TeamCityCommandBase
    {
        private string _from;
        private string _to;
        private string _referenceBuild;
        private string _zeroChangesComment;

        public AggregateBuildDelta()
        {
            IsCommand("aggregatebuilddelta", "Provides a set of changes between two specific versions of a build type.");
            Options.Add("rb=|referencebuild=", "Reference build to query resolved version deltas from", s => _referenceBuild = s);
            Options.Add("f|from=", "Build number to start checking from", x => _from = x);
            Options.Add("t|to=", "The build to check the delta change to", x => _to = x);

            Options.Add("zerochangescomment=", "If there are no changes detected, add the provided comment rather than leave it null", x => _zeroChangesComment = x);
            SkipsCommandSummaryBeforeRunning();
        }

        public override int Run(string[] remainingArguments)
        {
            var api = new TeamCityApi(ServerName);

            ResolveBuildTypeId(api);

            if (string.IsNullOrEmpty(_from))
            {
                var latestSuccesfull = api.GetLatestSuccesfulBuildByBuildType(BuildType);
                if (latestSuccesfull != null)
                    _from = latestSuccesfull.Number;
                else
                    throw new ApplicationException(string.Format("Could not find latest build for build type {0}", BuildType));
            }
            if (string.IsNullOrEmpty(_to))
            {
                var runningBuild = api.GetRunningBuildByBuildType(BuildType).FirstOrDefault();
                if (runningBuild != null) 
                    _to = runningBuild.Number;
                else
                    throw new ApplicationException(String.Format("Could not resolve a build number for the running build."));
            }

            var buildWithCommitData = _referenceBuild ?? BuildType;
            //TODO TFS collection data should come from the BuildType/VCS root data from TeamCity...but not for now...
            if (!string.IsNullOrEmpty(_from) && !string.IsNullOrEmpty(_to) && !string.IsNullOrEmpty(buildWithCommitData))
            {
                var builds = api.GetBuildsByBuildType(buildWithCommitData);
                if (builds != null)
                {
                    var buildList = builds as List<Build> ?? builds.ToList();
                    ChangeDetails = api.GetChangeDetailsByBuildTypeAndBuildNumber(buildWithCommitData, _from, _to, buildList).ToList();
                    IssueDetails = api.GetIssuesByBuildTypeAndBuildRange(buildWithCommitData, _from, _to, buildList).ToList();
                }
            }

            var resolvers = CreateExternalIssueResolvers();


            var issueDetailResolver = new IssueDetailResolver(resolvers);
            var issueDetails = issueDetailResolver.GetExternalIssueDetails(IssueDetails);
            ChangeManifest.ChangeDetails.AddRange(ChangeDetails);
            ChangeManifest.IssueDetails.AddRange(issueDetails);
            ChangeManifest.Generated = DateTime.Now;
            ChangeManifest.FromVersion = _from;
            ChangeManifest.ToVersion = _to;
            ChangeManifest.BuildConfiguration = BuildType;
            ChangeManifest.ReferenceBuildConfiguration = _referenceBuild ?? string.Empty;


            OutputChanges(CreateOutputRenderers(),new List<Action<string>>()
                {
                    Console.Write,
                    a => File.WriteAllText(OutputFileName,a)
                });
            return 0;
        }

        private void ResolveBuildTypeId(TeamCityApi api)
        {
            if (!String.IsNullOrEmpty(BuildType)) return;
            if (string.IsNullOrEmpty(ProjectName) || string.IsNullOrEmpty(BuildName))
            {
                throw new ApplicationException(String.Format("Could not resolve Project: {0} and BuildName:{1} to a build type", ProjectName, BuildName));
            }
            var resolvedBuildType = api.GetBuildTypeByProjectAndName(ProjectName, BuildName).FirstOrDefault();
            if (resolvedBuildType != null) 
                BuildType = resolvedBuildType.Id;
        }
    }
}
