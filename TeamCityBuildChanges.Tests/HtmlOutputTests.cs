using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;
using TeamCityBuildChanges.Output;
using TeamCityBuildChanges.Testing;

namespace TeamCityBuildChanges.Tests
{
    [TestFixture]
    public class HtmlOutputTests
    {
        [Test]
        public void CanRenderSimpleTemplate()
        {
            var result = new RazorOutputRenderer(@".\templates\text.cshtml").Render(TestHelpers.CreateSimpleChangeManifest());
            Assert.True(result.ToString().StartsWith("Version"));//Giddyup.
        }

        [TestCase("changeManifest.xml", "Default.cshtml", "FlattenNuGetPackageChangesTest.html")]
        public void FlattenNuGetPackageChangesTest(string inputManifest, string template, string outputHtml)
        {
            var manifest = TestHelpers.DeserializeFromXML(string.Format(@"{0}\{1}", Directory.GetCurrentDirectory(), inputManifest));
            var result = new RazorOutputRenderer(string.Format(@".\templates\{0}", template)).Render(manifest);
            File.WriteAllText(string.Format(@"{0}\{1}", Directory.GetCurrentDirectory(), outputHtml), result);
            Assert.True(true);
        }
    }
}
