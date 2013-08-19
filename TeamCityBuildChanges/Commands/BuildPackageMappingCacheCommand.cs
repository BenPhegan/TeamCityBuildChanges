using System;
using System.Linq;
using ManyConsole;
using TeamCityBuildChanges.NuGetPackage;

namespace TeamCityBuildChanges.Commands
{
// ReSharper disable UnusedMember.Global
    public class BuildPackageMappingCacheCommand : ConsoleCommand
// ReSharper restore UnusedMember.Global
    {
        private string _servers;
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
            var cache = BuildPackageMappingCache(_servers, _useArtifacts, Console.WriteLine, Console.Write);
            if (!string.IsNullOrEmpty(_output))
                cache.SaveCache(_output);
            return 0;
        }

        /// <summary>
        /// Provides a PackageBUildMappingCache that is pre-configured with a set of log outputs, and then automatically builds the cache and passes it back. 
        /// </summary>
        /// <param name="servers">A semicolon delimeted list of servers to check.</param>
        /// <param name="useArtifacts">Whether to use artifacts to resolve packages as output of a build.</param>
        /// <param name="logWriteLine">An Action that can be used to output a full line to a log</param>
        /// <param name="logWrite">An Action that can be used to output a partial line to a log</param>
        /// <returns>A PackageBuildMappingCache</returns>
        private static IPackageBuildMappingCache BuildPackageMappingCache(string servers, bool useArtifacts, Action<string> logWriteLine, Action<string> logWrite)
        {
            var serverlist = servers.Split(';').ToList();
            if (!serverlist.Any()) return null;
            
            var cache = new PackageBuildMappingCache();
            cache.StartedServerCheck += (sender, args) => logWriteLine(string.Format("Started Check: {0} with {1} build configurations", args.Url, args.Count));
            cache.FinishedServerCheck += (sender, args) => logWriteLine(string.Format("Finished Check: {0}", args.Url));
            cache.StartedBuildCheck += (sender, args) => logWrite(string.Format("\r\tStarted Check: {0}", args.Name));
            cache.FinishedBuildCheck += (sender, args) => logWrite(string.Format("\r\tFinished Check: {0}", args.Name));
            cache.BuildCache(serverlist, useArtifacts);
            return cache;
        }
    }
}
