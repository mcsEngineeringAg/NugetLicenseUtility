using NugetUtility.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NugetUtility.Logic
{
    internal class MarkdownPrinter
    {
        public static void PrintToMarkdown(Dictionary<string, Package> packages, string outputPath)
        {
            var nuget = packages.Values.ToList();
            var markdown = new StringBuilder();
            markdown.AppendLine("| Component | Version | Authors | Url | Description | License | License Url |");
            markdown.AppendLine("|---|---|---|---|---|---|---|");
            foreach (var package in nuget)
            {
                var p = package.Metadata;
                var description = p.Description.Replace(Environment.NewLine, " ");
                markdown.AppendLine($"| {p.Id} | {p.Version} | {p.Authors} | {p.ProjectUrl} | {description} | {p.License.Text} | {p.LicenseUrl} |");
            }
            File.WriteAllText(Path.Combine(outputPath, "packages.md"), markdown.ToString());
        }
    }
}
