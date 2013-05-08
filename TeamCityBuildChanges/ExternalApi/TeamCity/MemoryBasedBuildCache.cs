using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamCityBuildChanges.ExternalApi.TeamCity
{
    public class MemoryBasedBuildCache
    {
        private readonly Dictionary<string, BuildTypeDetails> _buildTypeDetailsCache;
        private readonly Dictionary<BuildTypeDetails, List<Artifact>> _buildTypeArtifactsCache;
        private readonly Dictionary<BuildTypeDetails, List<Build>> _buildTypeBuildDetailsCache; 

        public MemoryBasedBuildCache()
        {
            _buildTypeDetailsCache = new Dictionary<string, BuildTypeDetails>();
            _buildTypeArtifactsCache = new Dictionary<BuildTypeDetails, List<Artifact>>();
            _buildTypeBuildDetailsCache = new Dictionary<BuildTypeDetails, List<Build>>();
        }

        public bool TryCacheForDetailsByBuildTypeId(string buildTypeId, out BuildTypeDetails buildTypeDetails)
        {
            if (_buildTypeDetailsCache.ContainsKey(buildTypeId))
            {
                buildTypeDetails = _buildTypeDetailsCache[buildTypeId];
                return true;
            }
            buildTypeDetails = null;
            return false;
        }

        public bool TryCacheForArtifactsByBuildTypeId(string buildTypeId, out List<Artifact> artifacts)
        {
            if (_buildTypeDetailsCache.ContainsKey(buildTypeId))
            {
                artifacts = _buildTypeArtifactsCache[_buildTypeDetailsCache[buildTypeId]];
                return true;
            }
            artifacts = new List<Artifact>();
            return false;
        }

        public bool TryCacheForBuildsByBuildTypeId(string buildTypeId, out List<Build> builds)
        {
            if (_buildTypeDetailsCache.ContainsKey(buildTypeId))
            {
                builds = _buildTypeBuildDetailsCache[_buildTypeDetailsCache[buildTypeId]];
                return true;
            }
            builds = new List<Build>();
            return false;
        }

        public bool AddCacheBuildTypeDetailsById(string id, BuildTypeDetails buildTypeDetails)
        {
            if (!_buildTypeDetailsCache.ContainsKey(id))
            {
                _buildTypeDetailsCache.Add(id, buildTypeDetails);
                return true;
            }
            return false;
        }

        public bool AddCacheBuildTypeArtifactsById(string id, List<Artifact> artifacts)
        {
            if (_buildTypeDetailsCache.ContainsKey(id))
            {
                _buildTypeArtifactsCache.Add(_buildTypeDetailsCache[id], artifacts);
                return true;
            }
            return false;
        }

        public bool AddCacheBuildsById(string id, List<Build> builds)
        {
            if (_buildTypeDetailsCache.ContainsKey(id))
            {
                _buildTypeBuildDetailsCache.Add(_buildTypeDetailsCache[id], builds);
                return true;
            }
            return false;
        }
    }
}
