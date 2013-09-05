using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using ServiceStack.CacheAccess.Providers;
using TeamCityBuildChanges.ExternalApi;
using TeamCityBuildChanges.ExternalApi.TeamCity;

namespace TeamCityBuildChanges.NuGetPackage
{
    /// <summary>
    /// Provides TeamCity Build to NuGet package mappings.  If a build creates a package, it will be listed in the cache to allow cross referencing.
    /// </summary>
    public class PackageBuildMappingCache
    {
        private List<PackageBuildMapping> _packageBuildMappings = new List<PackageBuildMapping>();

        public delegate void BuildCheckEventHandler(object sender, BuildMappingEventArgs eventArgs);
        public delegate void ServerCheckEventHandler(object sender, ServerCheckEventArgs eventArgs);
        public event BuildCheckEventHandler StartedBuildCheck = delegate { };
        public event BuildCheckEventHandler FinishedBuildCheck = delegate { };
        public event ServerCheckEventHandler StartedServerCheck = delegate { };
        public event ServerCheckEventHandler FinishedServerCheck = delegate { };
  
        /// <summary>
        /// Creates a new PackageBuildMappingCache.
        /// </summary>
        public PackageBuildMappingCache(){}

        /// <summary>
        /// Creats a new PackageBuildMappingCache, and attempts to load the passed in file location as a Package Cache file.
        /// </summary>
        /// <param name="packageFile"></param>
        public PackageBuildMappingCache(string packageFile)
        {
            try
            {
                if (File.Exists(packageFile))
                    LoadCache(packageFile);
            }
            catch (Exception e)
            {
                throw new ArgumentException(string.Format("Problem on loading cache file: {0}", packageFile), e);
            }
        }

        /// <summary>
        /// Provides access to any PackageBuildMapping objects in the cache
        /// </summary>
        public List<PackageBuildMapping> PackageBuildMappings
        {
            get { return _packageBuildMappings; }
        }

        /// <summary>
        /// Attempts to build a a list of PackageBuildMapping objects by interrogating a list of servers.
        /// </summary>
        /// <param name="servers">The servers to query.</param>
        /// <param name="useArtifactsNotPackageSteps">If set, ignore TeamCity NuGet build steps and use the existence of packages in the artifacts as proof of creation.</param>
        public void BuildCache(List<string> servers, bool useArtifactsNotPackageSteps = false)
        {
            foreach (var server in servers)
            {
                var apiConnection = new TeamCityApi(new CachingThreadSafeAuthenticatedRestClient(new MemoryCacheClient(), server, null), new MemoryCacheClient());
                var buildConfigurations = apiConnection.GetBuildTypes();
                StartedServerCheck(this, new ServerCheckEventArgs {Count = buildConfigurations.Count, Url = server});
                foreach (var configuration in buildConfigurations)
                {
                    StartedBuildCheck(this,new BuildMappingEventArgs(){Name = configuration.Name});
                    var packages = useArtifactsNotPackageSteps
                                       ? GetPackageListFromArtifacts(configuration, apiConnection).ToList()
                                       : GetPackageListFromSteps(configuration, apiConnection).ToList();

                    foreach (var package in packages)
                    {
                        PackageBuildMappings.Add(new PackageBuildMapping
                            {
                                BuildConfigurationId = configuration.Id,
                                BuildConfigurationName = configuration.Name,
                                Project = configuration.ProjectName,
                                PackageId = package,
                                ServerUrl = server
                            });
                    }
                    FinishedBuildCheck(this, new BuildMappingEventArgs(){Name = configuration.Name});
                }
                FinishedServerCheck(this, new ServerCheckEventArgs(){Count = buildConfigurations.Count,Url = server});
            }
        }

        /// <summary>
        /// Load a cache file.
        /// </summary>
        /// <param name="filename">Defaults to PackageBuildMapping.xml</param>
        public void LoadCache(string filename = "PackageBuildMapping.xml")
        {
            var serializer = new XmlSerializer(typeof(List<PackageBuildMapping>));
            var readFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

            var loadedObj = (List<PackageBuildMapping>)serializer.Deserialize(readFileStream);
            if (loadedObj != null)
                _packageBuildMappings = loadedObj;
            readFileStream.Close();
        }

        /// <summary>
        /// Saves a cache file.
        /// </summary>
        /// <param name="filename">Defaults to PackageBuildMapping.xml</param>
        public void SaveCache(string filename = "PackageBuildMapping.xml")
        {
            var serializerObj = new XmlSerializer(typeof(List<PackageBuildMapping>));
            TextWriter writeFileStream = new StreamWriter(filename);
            serializerObj.Serialize(writeFileStream, PackageBuildMappings);
            writeFileStream.Close();
        }

        private static IEnumerable<string> GetPackageListFromArtifacts(BuildType buildConfig, TeamCityApi api)
        {
            var packages = new List<string>();
            foreach (var artifact in api.GetArtifactListByBuildType(buildConfig.Id).Where(a => a.Ext.Equals("nupkg")))
            {
                var package = Regex.Match(artifact.Name, @".+?(?=(?:(?:[\._]\d+){2,})$)").Value;
                if (!string.IsNullOrEmpty(package))
                    packages.Add(package);
            }
            return packages;
        }

        private static IEnumerable<string> GetPackageListFromSteps(BuildType buildConfig, TeamCityApi api)
        {
            var packages = new List<string>();
            //Check for nuget publish steps
            var details = api.GetBuildTypeDetailsById(buildConfig.Id);
            var steps = details.Steps.Where(s => s.Type.Equals("jb.nuget.publish"));

            foreach (var packageNames in steps.Select(publishStep => GetPackageNames(publishStep.Properties.First(p => p.Name.Equals("nuget.publish.files")).value)))
            {
                packages.AddRange(packageNames);
            }
            return packages;
        }

        private static IEnumerable<string> GetPackageNames(string fullValue)
        {
            var strings = fullValue.Split(Environment.NewLine.ToCharArray());
            return strings.Select(s => Regex.Replace(s, @"\%.*\%", "")).Select(temp => temp.Replace(".nupkg", "").TrimEnd('.')).ToList();
        }

        /// <summary>
        /// Used to let consumers know when a build has been mapped.
        /// </summary>
        public class BuildMappingEventArgs : EventArgs
        {
            public string Name { get; set; } 
        }

        /// <summary>
        /// Used to let consumers know when we have finished querying a server.
        /// </summary>
        public class ServerCheckEventArgs : EventArgs
        {
            public int Count { get; set; }
            public string Url { get; set; }
        }
    }
}
