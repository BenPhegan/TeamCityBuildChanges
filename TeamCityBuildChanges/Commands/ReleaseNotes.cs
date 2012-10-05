using System.Linq;

namespace TeamCityBuildChanges.Commands
{
    class ReleaseNotes : TeamCityCommandBase
    {
        private string _buildType;
        private bool _currentOnly;
        private string _buildId;

        public ReleaseNotes()
        {
            IsCommand("releasenotes", "Provides release notes from TeamCity being the set of comments associated with commits that triggered a build");
            Options.Add("current|c", "Check currently running build only", c => _currentOnly = true);
            Options.Add("b|buildType=", "TeamCity build type to get the details for.", s => _buildType = s);
            Options.Add("bi|buildid=", "Specific build id to get the release notes for", b => _buildId = b);      
        }

        public override int Run(string[] remainingArguments)
        {
            var api = new TeamCityApi(ServerName);

            if (!string.IsNullOrEmpty(_buildId))
            {
                ChangeDetails = api.GetChangeDetailsByBuildId(_buildId).ToList();
            }
            else
            {
                ChangeDetails = _currentOnly
                                    ? api.GetChangeDetailsForCurrentBuildByBuildType(_buildType).ToList()
                                    : api.GetChangeDetailsForLastBuildByBuildType(_buildType).ToList();
            }

            OutputChanges();
            return 0;
        }
    }
}
