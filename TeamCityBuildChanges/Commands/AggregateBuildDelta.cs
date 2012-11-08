﻿using System;
using System.Collections.Generic;
using System.Linq;
using NDesk.Options;

namespace TeamCityBuildChanges.Commands
{
    class AggregateBuildDelta : TeamCityCommandBase
    {
        private string _from;
        private string _to;
        private string _referenceBuild;

        public AggregateBuildDelta()
        {
            IsCommand("aggregatebuilddelta", "Provides a set of changes between two specific versions of a build type.");
            Options.Add("rb=|referencebuild=", "Reference build to query resolved version deltas from", s => _referenceBuild = s);
            Options.Add("f|from=", "Build number to start checking from", x => _from = x);
            Options.Add("t|to=", "The build to check the delta change to", x => _to = x);
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
            if (!string.IsNullOrEmpty(_from) && !string.IsNullOrEmpty(_to) && !string.IsNullOrEmpty(BuildType))
            {
                ChangeDetails = api.GetReleaseNotesByBuildTypeAndBuildNumber(buildWithCommitData, _from, _to).ToList();
            }
            var test = api.GetBuildsByBuildType(BuildType);
            var buildDetails = test.Select(build => api.GetBuildDetailsByBuildId(build.Id)).ToList();
            var issues = buildDetails.SelectMany(b => b.RelatedIssues).Select(i => i.Issue).Distinct().ToList();
            issues.ForEach(i => Console.WriteLine(i.Id));
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
