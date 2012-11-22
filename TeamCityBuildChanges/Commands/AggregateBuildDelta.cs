using System;
using System.Collections.Generic;
using System.Linq;
using NDesk.Options;
using TeamCityBuildChanges.ExternalApi;
using TeamCityBuildChanges.ExternalApi.Jira;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;

namespace TeamCityBuildChanges.Commands
{
    class AggregateBuildDelta : TeamCityCommandBase
    {
        private string _from;
        private string _to;
        private string _referenceBuild;
        private string _jiraUrl;
        private string _jiraToken;

        public AggregateBuildDelta()
        {
            IsCommand("aggregatebuilddelta", "Provides a set of changes between two specific versions of a build type.");
            Options.Add("rb=|referencebuild=", "Reference build to query resolved version deltas from", s => _referenceBuild = s);
            Options.Add("f|from=", "Build number to start checking from", x => _from = x);
            Options.Add("t|to=", "The build to check the delta change to", x => _to = x);
            Options.Add("jiraurl=", "The Jira URL to query for issue details", x => _jiraUrl = x);
            Options.Add("jiraauthtoken=", "The Jira authorisation token to use (refer to 'encode' subcommand", x => _jiraToken = x);

            base.SkipsCommandSummaryBeforeRunning();
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
            if (!string.IsNullOrEmpty(_from) && !string.IsNullOrEmpty(_to) && !string.IsNullOrEmpty(buildWithCommitData))
            {
                var builds = api.GetBuildsByBuildType(buildWithCommitData).ToList();
                ChangeDetails = api.GetChangeDetailsByBuildTypeAndBuildNumber(buildWithCommitData, _from, _to, builds).ToList();
                IssueDetails = api.GetIssuesByBuildTypeAndBuildRange(buildWithCommitData, _from, _to, builds).ToList();
            }

            var resolvers = new List<IExternalIssueResolver>{new JiraExternalIssueResolver(new JiraApi(_jiraUrl, _jiraToken))};

            var issueDetailResolver = new IssueDetailResolver(resolvers);
            var issueDetails = issueDetailResolver.GetExternalIssueDetails(IssueDetails);
            ChangeManifest.ChangeDetails.AddRange(ChangeDetails);
            ChangeManifest.IssueDetails.AddRange(issueDetails);

            OutputChanges();
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
