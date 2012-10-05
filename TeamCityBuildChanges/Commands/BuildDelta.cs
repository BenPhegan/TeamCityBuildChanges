using System;
using System.Collections.Generic;
using System.Linq;
using NDesk.Options;

namespace TeamCityBuildChanges.Commands
{
    class BuildDelta : TeamCityCommandBase
    {
        private string _buildType;
        private string _from;
        private string _to;

        public BuildDelta()
        {
            IsCommand("builddelta", "Provides a set of changes between two specific versions of a build type.");
            Options.Add("b|buildType=", "TeamCity build type to get the details for.", s => _buildType = s);
            HasRequiredOption("f|from=", "Build number to start checking from", x => _from = x);
            HasRequiredOption("t|to=", "The build to check the delta change to", x => _to = x);
        }

        public override int Run(string[] remainingArguments)
        {
            var api = new TeamCityApi(ServerName);

            if (!string.IsNullOrEmpty(_from) && !string.IsNullOrEmpty(_to) && !string.IsNullOrEmpty(_buildType))
            {
                ChangeDetails = api.GetReleaseNotesByBuildTypeAndBuildNumber(_buildType, _from, _to).ToList();
            }

            OutputChanges();
            return 0;
        }
    }
}
