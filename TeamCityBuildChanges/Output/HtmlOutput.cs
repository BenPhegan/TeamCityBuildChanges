using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RazorEngine;

namespace TeamCityBuildChanges.Output
{
    public class HtmlOutput
    {
        public static string Render(ChangeManifest manifest)
        {
            const  string template = 
                      @"<html>
                          <head>
                            <title>Hello @Model</title>
                          </head>
                          <body>
                            Email: @Html.TextBoxFor(m => m.Email)
                          </body>
                        </html>";

            return Razor.Parse<ChangeManifest>(template, manifest, "Changes");
        }
    }
}
