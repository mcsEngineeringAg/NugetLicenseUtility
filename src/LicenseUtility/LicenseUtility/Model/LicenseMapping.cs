using System.IO;

namespace NugetUtility.Model
{
    public enum MappingType
    {
        UNKNOWN,
        LOCAL,
        URL
    }

    public class LicenseMapping
    {
        public string Identifier { get; set; } = string.Empty;

        public bool ReadLocalLicense { get; set; } = false;

        public string LicenseFile { get; set; } = string.Empty;

        public string OriginalLicenseUrl { get; set; } = string.Empty;

        public string MappedLicenseUrl { get; set; } = string.Empty;

        public string LicenseType { get; set; } = string.Empty;


        /// <summary>
        /// Get the type of license mapping
        /// </summary>
        /// <returns></returns>
        public MappingType GetMappingType()
        {
            if (LocalLicenseOk())
            {
                return MappingType.LOCAL;
            }
            return UrlMappingOk() ? MappingType.URL : MappingType.UNKNOWN;
        }

        private bool LocalLicenseOk()
        {
            return ReadLocalLicense && !string.IsNullOrEmpty(Identifier) && !string.IsNullOrEmpty(LicenseFile) && File.Exists(LicenseFile);
        }

        private bool UrlMappingOk()
        {
            return !string.IsNullOrEmpty(MappedLicenseUrl);
        }
    }
}
