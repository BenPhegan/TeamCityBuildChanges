using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges.Tests
{
    [TestFixture]
    public class HtmlOutputTests
    {
        [Test]
        public void Test()
        {
            var result = new RazorOutputRenderer(@".\templates\text.cshtml").Render(TestHelpers.CreateChangeManifest());
            Assert.True(true);//Giddyup.
        }

    }
}
