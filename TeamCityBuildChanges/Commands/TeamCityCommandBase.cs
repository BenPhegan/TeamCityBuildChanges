using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManyConsole;
using NDesk.Options;
using ServiceStack.Text;
using TeamCityBuildChanges.ExternalApi.Jira;
using TeamCityBuildChanges.ExternalApi.TFS;
using TeamCityBuildChanges.IssueDetailResolvers;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges.Commands
{
    internal class TeamCityCommandBase : ConsoleCommand
    {
        protected string OutputFileName;
        protected ChangeManifest ChangeManifest = new ChangeManifest();
        protected string ServerName;
        protected string BuildType;
        protected string ProjectName;
        protected string BuildName;
        private string _jiraUrl;
        private string _jiraToken;
        private string _tfsUrl;
        private string _template;

        protected TeamCityCommandBase()
        {
            Options = new OptionSet
                {
                    {"o|output=", "OutputFileName filename (otherwise to console)", x => OutputFileName = x},
                    {"b=|buildType=", "TeamCity build type to get the details for.", s => BuildType = s},
                    {"p=|project=", "TeamCity project to search within for specific build name.", s => ProjectName = s},
                    {"bn=|buildName=", "TeamCity build type to get the details for.", s => BuildName = s},
                    {"jiraurl=", "The Jira URL to query for issue details", x => _jiraUrl = x},
                    {"jiraauthtoken=", "The Jira authorisation token to use (refer to 'encode' subcommand", x => _jiraToken = x},
                    {"tfsurl=", "TFS URL to check issues on (can be semicolon separated)", x => _tfsUrl = x},
                    {"template=", "Template to use for output.  Must be a Razor template that accepts a ChangeManifest model.", x => _template = x}
                };

            HasRequiredOption("s|server=", "TeamCity server to target (just use base URL and have guestAuth enabled", s => ServerName = s);
        }

        public override int Run(string[] remainingArguments)
        {
            return 0;
        }

        protected void OutputChanges(IEnumerable<IOutputRenderer> renderers, IEnumerable<Action<string>> writers)
        {
            var enumerable = writers as List<Action<string>> ?? writers.ToList();
            foreach (var renderer in renderers)
            {
                foreach (var writer in enumerable)
                {
                    writer(renderer.Render(ChangeManifest));
                }
            }
        }

        protected IEnumerable<IExternalIssueResolver> CreateExternalIssueResolvers()
        {
            var resolvers = new List<IExternalIssueResolver>();

            if (!string.IsNullOrEmpty(_jiraUrl))
            {
                resolvers.Add(new JiraIssueResolver(new JiraApi(_jiraUrl, _jiraToken)));
            }
            if (!string.IsNullOrEmpty(_tfsUrl))
            {
                resolvers.AddRange(_tfsUrl.Split(';').Select(url => new TFSIssueResolver(new TfsApi(url))));
            }
            return resolvers;
        }

        protected IEnumerable<IOutputRenderer> CreateOutputRenderers()
        {
            var renderers = new List<IOutputRenderer>
                {
                    string.IsNullOrEmpty(_template) ? new RazorOutputRenderer() : new RazorOutputRenderer(_template)
                };

            return renderers;
        }

        protected void SerializeManifest(ChangeManifest changeManifest, string outputType, string outputFileName)
        {
            var outputFile = new StreamWriter(string.Format("{0}.{1}", outputFileName, outputType.ToLowerInvariant()));
            switch (outputType.ToLowerInvariant())
            {
                case "json":
                    JsonSerializer.SerializeToWriter(changeManifest, outputFile);
                    break;
                case "jsv":
                    TypeSerializer.SerializeToWriter(changeManifest, outputFile);
                    break;
                case "csv":
                    CsvSerializer.SerializeToWriter(changeManifest, outputFile);
                    break;
                default:
                    XmlSerializer.SerializeToWriter(changeManifest, outputFile);
                    break;
            }
        }
    }
}