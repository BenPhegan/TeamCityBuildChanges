using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ManyConsole;
using NDesk.Options;

namespace TeamCityBuildChanges
{
    class ReleaseNotes : ConsoleCommand
    {
        private string _serverName;
        private string _buildType;
        private bool _currentOnly;
        private bool _noVersion;
        private string _buildId;
        private Boolean _xml;
        private string _output;

        public ReleaseNotes()
        {
            Options = new OptionSet()
                          {
                              {"current|c","Check currently running build only", c => _currentOnly = true},
                              {"noversion|nv", "Don't include the version in the output of the change details, instead use a *", v => _noVersion = true},
                              {"b|buildType=", "TeamCity build type to get the details for.", s => _buildType = s},
                              {"bi|buildid=", "Specific build id to get the release notes for", b => _buildId = b},
                              {"x|xmloutput", "Output to XML", x => _xml = true},
                              {"o|output=", "Output filename (otherwise to console)", x => _output = x},
                          };
            IsCommand("releasenotes", "Provides release notes from TeamCity being the set of comments associated with commits that triggered a build");
            HasRequiredOption("s|server=", "TeamCity server to target (just use base URL and have guestAuth enabled", s => _serverName = s);
        }

        public override int Run(string[] remainingArguments)
        {
            var api = new TeamCityApi(_serverName);
            List<ChangeDetail> changeDetails;

            if (!string.IsNullOrEmpty(_buildId))
            {
                changeDetails = api.GetReleaseNotesByBuildId(_buildId).ToList();
            }
            else
            {
                if (_currentOnly)
                    changeDetails = api.GetReleaseNotesForCurrentBuildByBuildType(_buildType).ToList();
                else
                    changeDetails = api.GetReleaseNotesForLastBuildByBuildType(_buildType).ToList();
            }

            if (_xml)
            {
                var outputStream = !String.IsNullOrEmpty(_output) ? new XmlTextWriter(_output, Encoding.UTF8) : new XmlTextWriter(Console.OpenStandardOutput(),Encoding.UTF8);
                var serializer = new XmlSerializer(typeof(List<ChangeDetail>));
                serializer.Serialize(outputStream, changeDetails);
            }
            else
            {
                var outputAction = GetOutputAction();
                
                foreach (var changeDetail in changeDetails)
                {
                    outputAction(changeDetail);
                }
            }
            return 0;
        }

        private Action<ChangeDetail> GetOutputAction()
        {
            return string.IsNullOrEmpty(_output)
                       ? new Action<ChangeDetail>(a => Console.WriteLine(FormatOutputValue(a)))
                       : new Action<ChangeDetail>(a => File.AppendAllText(_output, FormatOutputValue(a)));
        }

        private string FormatOutputValue(ChangeDetail a)
        {
            return _noVersion ? ChangeDetailNoVersion(a) : ChangeDetailWithVersion(a);
        }

        private string ChangeDetailNoVersion(ChangeDetail changeDetail)
        {
            return string.Format("  *  {0}", changeDetail.Comment.TrimEnd(Environment.NewLine.ToCharArray()));
        }

        private string ChangeDetailWithVersion(ChangeDetail changeDetail)
        {
            return string.Format("{0} - {1}", changeDetail.Version, changeDetail.Comment.TrimEnd(Environment.NewLine.ToCharArray()));
        }
    }
}
