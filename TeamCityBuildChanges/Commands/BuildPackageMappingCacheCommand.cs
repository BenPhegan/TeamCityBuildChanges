using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManyConsole;
using TeamCityBuildChanges.NuGetPackage;

namespace TeamCityBuildChanges.Commands
{
    public class BuildPackageMappingCacheCommand : ConsoleCommand
    {
        private string _servers;
        private List<String> _serverList = new List<string>();
        private string _output;
        private bool _useArtifacts;

        public BuildPackageMappingCacheCommand()
        {
            IsCommand("buildcache", "Creates an XML cache of build configuration to NuGet package.");
            HasRequiredOption("s|servers=", "Servers to build cache from.", s => _servers = s);
            Options.Add("o|output=", "Output filename for cache.", s => _output = s);
            Options.Add("a", "Use artifacts to resolve packages.", s => _useArtifacts = s != null);
            SkipsCommandSummaryBeforeRunning();
        }
        public override int Run(string[] remainingArguments)
        {
            _serverList = _servers.Split(';').ToList();
            if (_serverList.Any())
            {
                var cache = new PackageBuildMappingCache(_serverList,_useArtifacts);
                cache.StartedServerCheck += (sender, args) => Console.WriteLine("Started Check: {0} with {1} build configurations", args.Url, args.Count);
                cache.FinishedServerCheck += (sender, args) => Console.WriteLine("Finished Check: {0}", args.Url);
                cache.StartedBuildCheck += (sender, args) => Console.Write("\r\tStarted Check: {0}", args.Name);
                cache.FinishedBuildCheck += (sender, args) => Console.Write("\r\tFinished Check: {0}", args.Name);
                cache.BuildCache();
                cache.SaveCache(_output);
            }
            return 0;
        }
    }
}
