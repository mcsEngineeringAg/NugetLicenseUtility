using CommandLine;
using NugetUtility.Configuration;
using NugetUtility.Logic;
using NugetUtility.Validators;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("NugetUtilityTests")]
namespace NugetUtility
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<CmdLineOptions>(args);
            return await result.MapResult(
                Execute,
                _ => Task.FromResult(1));
        }

        internal static async Task<int> Execute(CmdLineOptions options)
        {
            var settings = LicenseUtilitySettings.Load(options.ConfigFile);
            var bom = options.BomFile;
            var outputLocation = options.OutputFolder;

            if (!File.Exists(bom))
            {
                Console.WriteLine("ERROR: BOM file does not exist.");
                return 1;
            }

            if (settings.ForbiddenLicenseTypes.Count != 0 && settings.AllowedLicenseTypes.Count != 0)
            {
                Console.WriteLine("ERROR: Forbidden and allowed license types can not be used together.");
                return 1;
            }

            try
            {
                var manager = new LicenseManager(settings);
                var packages = await manager.GetNugetInformationFromBomAsync(bom);

                var validator = new LicenseValidator(settings);
                if (!validator.ValidateLicense(packages))
                {
                    Console.WriteLine("ERROR: Invalid licenses found in the packages.");
                    return 1;
                }

                switch (settings.OutputFileType)
                {
                    case OutputFileType.JSON:
                        JsonPrinter.PrintToJson(packages, outputLocation);
                        break;
                    case OutputFileType.MARKDOWN:
                        MarkdownPrinter.PrintToMarkdown(packages, outputLocation);
                        break;
                    case OutputFileType.EXCEL:
                        ExcelPrinter.PrintToExcel(packages, manager.Component, outputLocation);
                        break;
                    default:
                        Console.WriteLine("ERROR: Invalid output file type.");
                        return 1;
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
        }
    }
}
