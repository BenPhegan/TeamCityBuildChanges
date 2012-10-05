using System;
using System.Collections.Generic;
using ManyConsole;
using NDesk.Options;

namespace TeamCityBuildChanges.Commands
{
    internal class TeamCityCommandBase : ConsoleCommand
    {
        protected bool NoVersion;
        protected Boolean Xml;
        protected string OutputFileName;
        protected List<ChangeDetail> ChangeDetails;
        protected string ServerName;

        public TeamCityCommandBase()
        {
            Options = new OptionSet
                {
                    {"noversion|nv", "Don't include the version in the output of the change details, instead use a *", v => NoVersion = true},
                    {"x|xmloutput", "OutputFileName to XML", x => Xml = true},
                    {"o|output=", "OutputFileName filename (otherwise to console)", x => OutputFileName = x},

                };
            HasRequiredOption("s|server=", "TeamCity server to target (just use base URL and have guestAuth enabled", s => ServerName = s);
        }

        public override int Run(string[] remainingArguments)
        {
            return 0;
        }

        protected void OutputChanges()
        {
            var outputFormatter = new ChangeDetailOutputFormatter();
            if (Xml)
            {
                outputFormatter.OutputAsXml(ChangeDetails, NoVersion, OutputFileName);
            }
            else
            {
                outputFormatter.OutputAsText(ChangeDetails, NoVersion, OutputFileName);
            }
        }
    }
}