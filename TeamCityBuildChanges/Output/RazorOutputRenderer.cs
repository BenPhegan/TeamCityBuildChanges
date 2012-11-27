using System;
using System.IO;
using RazorEngine;

namespace TeamCityBuildChanges.Output
{
    public class RazorOutputRenderer : IOutputRenderer
    {
        private readonly string _templateFile;

        public RazorOutputRenderer(string templateFile = @".\Templates\Default.cshtml")
        {
            _templateFile = templateFile;
        }

        public string Render(ChangeManifest manifest)
        {
            if (!File.Exists(_templateFile))
            {
                throw new ArgumentException("Template file could not be found: {0}",_templateFile);
            }
            
            var defaultTemplate = File.ReadAllText(_templateFile);
            var result = Razor.Parse(defaultTemplate, manifest);
            return result;
        }
    }

}
