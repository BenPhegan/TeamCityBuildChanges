using System;
using System.Collections.Generic;
using System.IO;
using ManyConsole;
using TeamCityBuildChanges.ExternalApi.Jira;
using TeamCityBuildChanges.Output;
using TeamCityBuildChanges.Testing;

namespace TeamCityBuildChanges.Commands
{
    public class TestTemplate : ConsoleCommand
    {
        private string _username;
        private string _password;
        private string _templateFile;
        private string _outputFilename;

        public TestTemplate()
        {
            IsCommand("testtemplate", "When run, will watch a template file, and regenerate a HTML file against a generated Model.");
            HasRequiredOption("t|template=", "Template to watch.", s => _templateFile = s);
            HasOption("o|output=", "Output filename.", s => _outputFilename = s);

        }
        public override int Run(string[] remainingArguments)
        {
            if (!File.Exists(_templateFile)) throw new ArgumentException("Please provide a template file that exists.");

            var resolver = CreateMockedAggregateBuildDeltaResolver();

            var result = resolver.CreateChangeManifestFromBuildTypeId("bt1", null, "1.2", "1.6");
            var renderer = new RazorOutputRenderer(_templateFile);

            if (!File.Exists(_outputFilename)) TryToRender(renderer, result);

            var watcher = new FileSystemWatcher {Path = Path.GetDirectoryName(_templateFile), Filter = Path.GetFileName(_templateFile), EnableRaisingEvents = true};

            watcher.Changed += (o, e) => TryToRender(renderer, result);
            while (!Console.KeyAvailable)
            {
            }
            return 0;
        }

        private void TryToRender(RazorOutputRenderer renderer, ChangeManifest result)
        {
            try
            {
                Console.WriteLine("Detected the file has changed, running renderer...");
                var results = renderer.Render(result);
                if (!string.IsNullOrEmpty(results))
                    File.WriteAllText(_outputFilename ?? "Output.html", results);
                else
                    Console.WriteLine("Did not render any output, nothing written...");
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine("Failed to render template, fix it, save it and I will try again...");
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
            }
        }

        private static AggregateBuildDeltaResolver CreateMockedAggregateBuildDeltaResolver()
        {
            return TestHelpers.CreateMockedAggregateBuildDeltaResolver(new[] { new BuildTemplate { BuildId = "bt1", BuildName = "Build1", BuildCount = 15, BuildNumberPattern = "1.{0}", StartBuildNumber = 2, FinishBuildNumber = 4, StartBuildPackages = new Dictionary<string, string> { { "Package1", "1.0" }, { "Package2", "1.0" } }, FinishBuildPackages = new Dictionary<string, string> { { "Package1", "1.1" }, { "Package2", "1.0" } }, IssueCount = 5, NestedIssueDepth = 1, NestedIssueChance = 80 } });
        }
    }
}
