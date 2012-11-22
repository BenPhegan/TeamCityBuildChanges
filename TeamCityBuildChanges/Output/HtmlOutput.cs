using System.IO;
using RazorEngine;

namespace TeamCityBuildChanges.Output
{
    public class HtmlOutput
    {
        public static string Render(ChangeManifest manifest, string templateFile = @".\Templates\Default.cshtml")
        {
            var defaultTemplate = File.ReadAllText(templateFile);
            var result = Razor.Parse(defaultTemplate,manifest);
            return result;
        }
    }
}
