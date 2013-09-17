using System;
using System.Collections.Generic;
using System.IO;
using ManyConsole;
using TeamCityBuildChanges.Output;
using TeamCityBuildChanges.Testing;

namespace TeamCityBuildChanges.Commands
{
    public class TestTemplate : ConsoleCommand
    {
        private string _templateFile;
        private string _outputFilename;

        public TestTemplate()
        {
            IsCommand("testtemplate", "When run, will watch a template file, and regenerate a HTML file against a generated Model.");
            HasOption("o|output=", "Output filename.", s => _outputFilename = s);
            HasAdditionalArguments(1,"Provide the path to the template file to watch.");
        }
        public override int Run(string[] remainingArguments)
        {
            if (RemainingArgumentsCount != 1) throw new ConsoleHelpAsException("Please provide the path to the template file to parse.");

            _templateFile = remainingArguments[0];

            if (!File.Exists(_templateFile)) throw new ConsoleHelpAsException("Please provide a template file that exists.");

            var resolver = CreateMockedAggregateBuildDeltaResolver();

            var result = resolver.CreateChangeManifestFromBuildTypeId("bt1", null, "1.2", "1.4", false, true);

            var renderer = new RazorOutputRenderer(_templateFile);

            Console.WriteLine("Running Render on File Change - Hit ENTER to EXIT.");
            if (!File.Exists(_outputFilename ?? "Output.html")) TryToRender(renderer, result, String.Format("{0} - Rendering on first run...",DateTime.Now));

            var watcher = new FileSystemWatcher
                {
                    Path = Path.GetDirectoryName(Path.GetFullPath(_templateFile)), 
                    Filter = Path.GetFileName(_templateFile), 
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime
                };

            watcher.Changed += (o, e) => TryToRender(renderer, result, "Detected template has changed, running renderer...");

            Console.Read();
            return 0;
        }

        private void TryToRender(IOutputRenderer renderer, ChangeManifest result, string message)
        {
            try
            {
                ConsoleOutputWithColourToggle(message, ConsoleColor.White, Console.Write);
                var results = renderer.Render(result);
                if (!string.IsNullOrEmpty(results))
                {
                    File.WriteAllText(_outputFilename ?? "Output.html", results);
                    ConsoleOutputWithColourToggle("success! Please refresh browser.", ConsoleColor.Green, Console.Write);
                }
                else
                {
                    ConsoleOutputWithColourToggle("failed.  Nothing written...", ConsoleColor.Red, Console.Write);
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.Clear();
                ConsoleOutputWithColourToggle(String.Format("{0} - Failed to render template, fix it, save it and I will try again...",DateTime.Now), ConsoleColor.Red, Console.WriteLine);
                Console.WriteLine();
                ConsoleOutputWithColourToggle(ex.ToString(), ConsoleColor.Yellow, Console.WriteLine);
            }
        }

        private static void ConsoleOutputWithColourToggle(string message, ConsoleColor colour, Action<string> log)
        {
            var oldColour = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            log(message);
            Console.ForegroundColor = oldColour;
        }

        private static AggregateBuildDeltaResolver CreateMockedAggregateBuildDeltaResolver()
        {
            return TestHelpers.CreateMockedAggregateBuildDeltaResolver(new[]
                {
                    new BuildTemplate
                        {
                            BuildId = "bt1", 
                            BuildName = "Build1", 
                            BuildCount = 15, 
                            BuildNumberPattern = "1.{0}", 
                            StartBuildNumber = 2, 
                            FinishBuildNumber = 4, 
                            StartBuildPackages = new Dictionary<string, string> { { "Package1", "1.0" }, { "Package2", "1.0" } }, 
                            FinishBuildPackages = new Dictionary<string, string> { { "Package1", "1.1" }, { "Package2", "1.0" } }, 
                            IssueCount = 5, 
                            NestedIssueDepth = 1, 
                            NestedIssueChance = 80,
                            CreateNuGetPackageChangeManifests = true
                        }
                });
        }
    }
}
