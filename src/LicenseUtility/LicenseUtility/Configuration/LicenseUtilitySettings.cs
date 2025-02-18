using Newtonsoft.Json;
using NugetUtility.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NugetUtilityTests")]
namespace NugetUtility.Configuration
{
    public enum OutputFileType
    {
        JSON,
        MARKDOWN,
        EXCEL
    }

    public class LicenseUtilitySettings
    {
        public static LicenseUtilitySettings Load(string configFile)
        {
            var settings = new LicenseUtilitySettings();
            if (string.IsNullOrEmpty(configFile))
            {
                return settings;
            }
            if (!File.Exists(configFile))
            {
                Console.Write($"WARNING: Configuration file {configFile} does not exist. Default config was used.");
                return settings;
            }

            try
            {
                settings = JsonConvert.DeserializeObject<LicenseUtilitySettings>(File.ReadAllText(configFile)) ?? settings;
            }
            catch (Exception)
            {
                Console.Write($"WARNING: Could not read configuration file {configFile}. Default config was used.");
            }
            return settings;
        }

        // HTTP and proxy settings

        public string ProxyURL { get; set; } = string.Empty;

        public bool ProxySystemAuth { get; set; } = false;

        public bool IgnoreSslCertificateErrors { get; set; } = false;

        /// <summary>
        /// In seconds
        /// </summary>
        public double HttpClientTimeout { get; set; } = 10;

        // License filters

        /// <summary>
        /// Array of allowable licenses. If array is empty, all are assumed allowed. Cannot be used alongside 'ForbiddenLicenseTypes'.
        /// </summary>
        public List<string> AllowedLicenseTypes { get; set; } = [];

        /// <summary>
        /// Array of forbidden licenses. If array is empty, none are assumed forbidden. Cannot be used alongside 'AllowedLicenseTypes'.
        /// </summary>
        public List<string> ForbiddenLicenseTypes { get; set; } = [];

        /// <summary>
        /// Array of license mappings. If array is empty, no mappings are assumed.
        /// </summary>
        public List<LicenseMapping> LicenseMappings { get; set; } = [];

        /// <summary>
        /// Exclude nuget packages from the license check.
        /// You can use the full name like 'Microsoft.Extensions.Logging' or use 'Logging*'.
        /// This would exclude all packages which contain 'Logging' in their name.
        /// </summary>
        public List<string> ExcludedPackages { get; set; } = [];

        // Output settings

        public OutputFileType OutputFileType { get; set; } = OutputFileType.JSON;


        internal IEnumerable<string> HardPackageExclusions
        {
            get
            {
                return ExcludedPackages.Where(x => !string.IsNullOrEmpty(x)
                && !x.Contains('*'));
            }
        }

        internal IEnumerable<string> WildcardPackageExclusions
        {
            get
            {
                return ExcludedPackages.Where(x => !string.IsNullOrEmpty(x)
                && x.Contains('*')).Select(x => x.Replace("*", ""));
            }
        }

        internal bool IsExclusionMatch(string packageName)
        {
            if (HardPackageExclusions.Contains(packageName))
                return true;

            foreach (var pattern in WildcardPackageExclusions)
            {
                if (packageName.Contains(pattern))
                    return true;
            }

            return false;
        }
    }
}
