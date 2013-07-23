using System.Collections.Generic;

namespace TeamCityBuildChanges.Testing
{
    public class TFSApiMockTemplate
    {
        public string ConnectionUri;
        public Dictionary<int, List<int>> WorkItems = new Dictionary<int, List<int>>();
    }
}