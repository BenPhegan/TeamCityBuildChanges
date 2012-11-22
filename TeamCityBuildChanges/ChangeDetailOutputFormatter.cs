using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using TeamCityBuildChanges.ExternalApi;
using TeamCityBuildChanges.ExternalApi.TeamCity;

namespace TeamCityBuildChanges
{
    class ChangeDetailOutputFormatter
    {
        private readonly IEnumerable<ChangeDetail> _changeDetails;
        private readonly IEnumerable<Issue> _issues;

        public ChangeDetailOutputFormatter(IEnumerable<ChangeDetail> changeDetails, IEnumerable<Issue> issues)
        {
            _changeDetails = changeDetails;
            _issues = issues;
        }

        public void OutputAsXml(bool noVersion = false, string outputFileName = "" )
        {
            var outputStream = !String.IsNullOrEmpty(outputFileName) 
                ? new XmlTextWriter(outputFileName, Encoding.UTF8) 
                : new XmlTextWriter(Console.OpenStandardOutput(), Encoding.UTF8);
            var serializer = new XmlSerializer(typeof (List<ChangeDetail>));
            serializer.Serialize(outputStream, _changeDetails);
        }

        public void OutputAsText(bool noVersion = false, string outputFilename = "")
        {
            var changeDetailsOutputAction = string.IsNullOrEmpty(outputFilename)
                       ? new Action<ChangeDetail>(a => Console.WriteLine(FormatChangeDetailOutputValue(a, noVersion)))
                       : new Action<ChangeDetail>(a => File.AppendAllText(outputFilename, FormatChangeDetailOutputValue(a, noVersion)));

            var issueOutputAction = string.IsNullOrEmpty(outputFilename)
                       ? new Action<Issue>(issue => Console.WriteLine("{0} - {1}", issue.Id, issue.Url))
                       : new Action<Issue>(issue => File.AppendAllText(outputFilename, string.Format("{0} - {1}", issue.Id, issue.Url)));

            foreach (var changeDetail in _changeDetails)
            {
                changeDetailsOutputAction(changeDetail);
            }

            foreach (var issue in _issues)
            {
                issueOutputAction(issue);
            }
        }

        private static string FormatChangeDetailOutputValue(ChangeDetail a, bool noVersion)
        {
            return noVersion ? ChangeDetailNoVersion(a) : ChangeDetailWithVersion(a);
        }

        private static string ChangeDetailNoVersion(ChangeDetail changeDetail)
        {
            return string.Format("  *  {0}", changeDetail.Comment.TrimEnd(Environment.NewLine.ToCharArray()));
        }

        private static string ChangeDetailWithVersion(ChangeDetail changeDetail)
        {
            return string.Format("{0} - {1}", changeDetail.Version, changeDetail.Comment.TrimEnd(Environment.NewLine.ToCharArray()));
        }
    }
}