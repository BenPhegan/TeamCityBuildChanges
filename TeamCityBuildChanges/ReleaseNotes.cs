using System;
using System.Collections.Generic;
using System.Linq;
using ManyConsole;
using NDesk.Options;

namespace TeamCityBuildChanges
{
    class ReleaseNotes : ConsoleCommand
    {
        private string _serverName;
        private string _buildType;
        private bool _currentOnly;
        private bool _noVersion;
        private string _buildId;

        public ReleaseNotes()
        {
            Options = new OptionSet()
                          {
                              {"current|c","Check currently running build only", c => _currentOnly = true},
                              {"noversion|nv", "Don't include the version in the output of the change details, instead use a *", v => _noVersion = true},
                              {"b|buildType=", "TeamCity build type to get the details for.", s => _buildType = s},
                              {"bi|buildid=", "Specific build id to get the release notes for", b => _buildId = b}
                          };
            IsCommand("releasenotes", "Provides release notes from TeamCity being the set of comments associated with commits that triggered a build");
            HasRequiredOption("s|server=", "TeamCity server to target (just use base URL and have guestAuth enabled", s => _serverName = s);
        }

        public override int Run(string[] remainingArguments)
        {
            var api = new TeamCityApi(_serverName);
            List<ChangeDetail> changeDetails;

            if (!string.IsNullOrEmpty(_buildId))
            {
                changeDetails = api.GetReleaseNotesByBuildId(_buildId).ToList();
            }
            else
            {
                if (_currentOnly)
                    changeDetails = api.GetReleaseNotesForCurrentBuildByBuildType(_buildType).ToList();
                else
                    changeDetails = api.GetReleaseNotesForLastBuildByBuildType(_buildType).ToList();
            }

            foreach (var changeDetail in changeDetails)
            {
                if (_noVersion)
                    Console.WriteLine("  *  {0}", changeDetail.Comment.TrimEnd(Environment.NewLine.ToCharArray()));
                else
                    Console.WriteLine("{0} - {1}", changeDetail.Version, changeDetail.Comment.TrimEnd(Environment.NewLine.ToCharArray()));
            }
            return 0;
        }
    }
}
