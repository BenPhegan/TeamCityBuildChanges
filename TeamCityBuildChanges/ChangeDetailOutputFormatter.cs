using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace TeamCityBuildChanges
{
    class ChangeDetailOutputFormatter
    {
        public void OutputAsXml(IEnumerable<ChangeDetail> changeDetails, bool noVersion = false, string outputFileName = "" )
        {
            var outputStream = !String.IsNullOrEmpty(outputFileName) 
                ? new XmlTextWriter(outputFileName, Encoding.UTF8) 
                : new XmlTextWriter(Console.OpenStandardOutput(), Encoding.UTF8);
            var serializer = new XmlSerializer(typeof (List<ChangeDetail>));
            serializer.Serialize(outputStream, changeDetails);
        }

        public void OutputAsText(IEnumerable<ChangeDetail> changeDetails, bool noVersion = false, string outputFilename = "")
        {
            var outputAction = string.IsNullOrEmpty(outputFilename)
                       ? new Action<ChangeDetail>(a => Console.WriteLine(FormatOutputValue(a, noVersion)))
                       : new Action<ChangeDetail>(a => File.AppendAllText(outputFilename, FormatOutputValue(a, noVersion)));

            foreach (var changeDetail in changeDetails)
            {
                outputAction(changeDetail);
            }
        }

        private static string FormatOutputValue(ChangeDetail a, bool noVersion)
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