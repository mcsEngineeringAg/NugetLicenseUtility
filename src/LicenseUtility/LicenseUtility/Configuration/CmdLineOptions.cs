using CommandLine;

namespace NugetUtility.Configuration
{
    internal class CmdLineOptions
    {
        [Option("bom", Default = "bom.xml", HelpText = "The CycloneDX bom.xml input file")]
        public string BomFile { get; set; } = "bom.xml";

        [Option("config", HelpText = "The json configuration file")]
        public string ConfigFile { get; set; } = string.Empty;

        [Option("output-folder", HelpText = "Output folder for the license file")]
        public string OutputFolder { get; set; } = string.Empty;
    }
}
