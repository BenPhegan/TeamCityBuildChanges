using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManyConsole;
using NDesk.Options;
using ServiceStack.Text;
using TeamCityBuildChanges.ExternalApi.Jira;
using TeamCityBuildChanges.ExternalApi.TFS;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges.Commands
{
    internal class TeamCityCommandBase : ConsoleCommand
    {
        protected bool NoVersion;
        protected Boolean Xml;
        protected string OutputFileName;
        protected List<ChangeDetail> ChangeDetails;
        protected List<Issue> IssueDetails;
        protected ChangeManifest ChangeManifest = new ChangeManifest();
        protected string ServerName;
        protected string BuildType;
        protected string ProjectName;
        protected string BuildName;
        protected string JiraUrl;
        protected string JiraToken;
        protected string TfsUrl;
        protected string Template;

        public TeamCityCommandBase()
        {
            Options = new OptionSet
                {
                    {"noversion|nv", "Don't include the version in the output of the change details, instead use a *", v => NoVersion = true},
                    {"x|xmloutput", "OutputFileName to XML", x => Xml = true},
                    {"o|output=", "OutputFileName filename (otherwise to console)", x => OutputFileName = x},
                    {"b=|buildType=", "TeamCity build type to get the details for.", s => BuildType = s},
                    {"p=|project=", "TeamCity project to search within for specific build name.", s => ProjectName = s},
                    {"bn=|buildName=", "TeamCity build type to get the details for.", s => BuildName = s},
                    {"jiraurl=", "The Jira URL to query for issue details", x => JiraUrl = x},
                    {"jiraauthtoken=", "The Jira authorisation token to use (refer to 'encode' subcommand", x => JiraToken = x},
                    {"tfsurl=", "TFS URL to check issues on (can be semicolon separated)", x => TfsUrl = x},
                    {"template=", "Template to use for output.  Must be a Razor template that accepts a ChangeManifest model.", x => Template = x}
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

            if (!string.IsNullOrEmpty(JiraUrl))
            {
                resolvers.Add(new JiraIssueResolver(new JiraApi(JiraUrl, JiraToken)));
            }
            if (!string.IsNullOrEmpty(TfsUrl))
            {
                foreach (var url in TfsUrl.Split(';'))
                    resolvers.Add(new TFSIssueResolver(new TfsApi(url)));
            }
            return resolvers;
        }

        protected IEnumerable<IOutputRenderer> CreateOutputRenderers()
        {
            var renderers = new List<IOutputRenderer>
                {
                    string.IsNullOrEmpty(Template) ? new RazorOutputRenderer() : new RazorOutputRenderer(Template)
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