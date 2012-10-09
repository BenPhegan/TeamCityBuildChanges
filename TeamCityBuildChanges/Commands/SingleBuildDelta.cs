using System.Linq;

namespace TeamCityBuildChanges.Commands
{
    class SingleBuildDelta : TeamCityCommandBase
    {
        private bool _currentOnly;
        private string _buildId;

        public SingleBuildDelta()
        {
            IsCommand("singlebuilddelta", "Provides release notes from TeamCity being the set of comments associated with commits that triggered a build.  Acts on a single build delta.");
            Options.Add("current|c", "Check currently running build only", c => _currentOnly = true);
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
                                    ? api.GetChangeDetailsForCurrentBuildByBuildType(BuildType).ToList()
                                    : api.GetChangeDetailsForLastBuildByBuildType(BuildType).ToList();
            }

            OutputChanges();
            return 0;
        }
    }
}
