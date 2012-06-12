using System;
using ManyConsole;

namespace TeamCityBuildChanges
{
    class ReleaseNotes : ConsoleCommand
    {
        private string _serverName;
        private string _buildType;

        public ReleaseNotes()
        {
            IsCommand("releasenotes", "Provides release notes from TeamCity being the set of comments associated with commits that triggered a build");
            HasRequiredOption("s|server=", "TeamCity server to target (just use base URL and have guestAuth enabled", s => _serverName = s);
            HasRequiredOption("b|buildType=", "TeamCity build type to get the details for.", s => _buildType = s);
        }

        public override int Run(string[] remainingArguments)
        {
            var api = new TeamCityApi(_serverName);
            var releaseNotes = api.GetReleaseNotesForLastBuildByBuildId(_buildType);
            foreach (var line in releaseNotes)
            {
                Console.WriteLine(line);
            }
            return 0;
        }
    }
}
