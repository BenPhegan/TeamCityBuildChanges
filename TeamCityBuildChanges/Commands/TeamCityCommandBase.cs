using System;
using System.Collections.Generic;
using ManyConsole;
using NDesk.Options;
using TeamCityBuildChanges.ExternalApi;
using TeamCityBuildChanges.ExternalApi.TeamCity;

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
                };
            HasRequiredOption("s|server=", "TeamCity server to target (just use base URL and have guestAuth enabled", s => ServerName = s);
        }

        public override int Run(string[] remainingArguments)
        {
            return 0;
        }

        protected void OutputChanges()
        {
            var outputFormatter = new ChangeDetailOutputFormatter(ChangeDetails, IssueDetails);
            if (Xml)
            {
                outputFormatter.OutputAsXml(NoVersion, OutputFileName);
            }
            else
            {
                outputFormatter.OutputAsText(NoVersion, OutputFileName);
            }
        }
    }
}