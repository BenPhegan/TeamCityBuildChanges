using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;
using ServiceStack.Text;
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
    }
}
