using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RazorEngine;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges.Output
{
    public class HtmlOutput
    {
        public static string Render(ChangeManifest manifest)
        {

            var defaultTemplate = File.ReadAllText(@".\Templates\Default.cshtml");
            var result = Razor.Parse(defaultTemplate,manifest);
            return result;
        }
    }
}
