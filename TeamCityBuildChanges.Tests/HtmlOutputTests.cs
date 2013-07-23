using NUnit.Framework;
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
            Assert.True(result.StartsWith("Version"));//Giddyup.
        }
    }
}
