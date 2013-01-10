using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using TeamCityBuildChanges.ExternalApi.TeamCity;

namespace TeamCityBuildChanges.NuGetPackage
{
    public class PackageBuildMappingCache
    {
        private List<PackageBuildMapping> _packageBuildMappings = new List<PackageBuildMapping>();
        private readonly List<string> _servers;
        private readonly bool _useArtifactsNotPackageSteps;

        public delegate void BuildCheckEventHandler(object sender, BuildMappingEventArgs eventArgs);
        public delegate void ServerCheckEventHandler(object sender, ServerCheckEventArgs eventArgs);
        public event BuildCheckEventHandler StartedBuildCheck = delegate { };
        public event BuildCheckEventHandler FinishedBuildCheck = delegate { };
        public event ServerCheckEventHandler StartedServerCheck = delegate { };
        public event ServerCheckEventHandler FinishedServerCheck = delegate { };
  
        public PackageBuildMappingCache(List<string> servers, bool useArtifactsNotPackageSteps = false)
        {
            _servers = servers;
            _useArtifactsNotPackageSteps = useArtifactsNotPackageSteps;
        }

        public void BuildCache()
        {
            foreach (var server in _servers)
            {
                var apiConnection = new TeamCityApi(server);
                var buildConfigurations = apiConnection.GetBuildTypes();
                StartedServerCheck(this, new ServerCheckEventArgs(){Count = buildConfigurations.Count, Url = server});
                foreach (var configuration in buildConfigurations)
                {
                    StartedBuildCheck(this,new BuildMappingEventArgs(){Name = configuration.Name});
                    var packages = _useArtifactsNotPackageSteps
                                       ? GetPackageListFromArtifacts(configuration, apiConnection).ToList()
                                       : GetPackageListFromSteps(configuration, apiConnection).ToList();

                    foreach (var package in packages)
                    {
                        _packageBuildMappings.Add(new PackageBuildMapping
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

        public void LoadCache(string filename = "PackageBuildMapping.xml")
        {
            var serializer = new XmlSerializer(typeof(List<PackageBuildMapping>));
            var readFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

            var loadedObj = (List<PackageBuildMapping>)serializer.Deserialize(readFileStream);
            if (loadedObj != null)
                _packageBuildMappings = loadedObj;
            readFileStream.Close();
        }

        public void SaveCache(string filename = "PackageBuildMapping.xml")
        {
            var serializerObj = new XmlSerializer(typeof(List<PackageBuildMapping>));
            TextWriter writeFileStream = new StreamWriter(filename);
            serializerObj.Serialize(writeFileStream, _packageBuildMappings);
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

        public class BuildMappingEventArgs : EventArgs
        {
            public string Name { get; set; } 
        }

        public class ServerCheckEventArgs : EventArgs
        {
            public int Count { get; set; }
            public string Url { get; set; }
        }
    }
}
