using CycloneDX.Models;
using CycloneDX.Xml;
using NugetUtility.Configuration;
using NugetUtility.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;


[assembly: InternalsVisibleTo("NugetUtilityTests")]
namespace NugetUtility.Logic
{
    internal class LicenseManager
    {
        private const string NUGET_URL = "https://api.nuget.org/v3-flatcontainer/";
        private const string NUGET_FALLBACK = "https://www.nuget.org/api/v2/package/{0}/{1}";
        private const string DEPRECATED_LICENSE_URL = "https://aka.ms/deprecateLicenseUrl";

        public LicenseManager(LicenseUtilitySettings settings)
        {
            mSettings = settings;
            InitHttpClient();
        }

        public async Task<Dictionary<string, Package>> GetNugetInformationFromBomAsync(string bomPath)
        {
            var licenses = new Dictionary<string, Package>();
            foreach (var packageWithVersion in GetPackagesFromBom(bomPath))
            {
                try
                {
                    string identifier = $"{packageWithVersion.Name}_{packageWithVersion.Version}";
                    string nugetUri = $"{packageWithVersion.Name}/{packageWithVersion.Version}/{packageWithVersion.Name}.nuspec".ToLowerInvariant();

                    if (SkipPackage(packageWithVersion))
                    {
                        Console.WriteLine($"Skipped package {identifier}");
                        continue;
                    }

                    if (mPackageCache.TryGetValue(identifier, out var package))
                    {
                        Console.WriteLine($"Added package {identifier} from cache to list");
                        licenses.TryAdd(identifier, package);
                        continue;
                    }

                    // Try Download from v3 nuget
                    if (await DownloadNuspec(identifier, nugetUri, licenses))
                    {
                        Console.WriteLine($"Added package {identifier} from Nuget v3 to list");
                    }
                    else if (LocalNuspec(packageWithVersion, identifier, licenses))
                    {
                        Console.WriteLine($"Added package {identifier} from local Nuget cache to list");
                    }
                    else
                    {
                        Console.WriteLine($"Could not add package {identifier}!");
                        continue;
                    }

                    package = licenses.Last().Value;
                    await UpdatePackageLicenseText(package, packageWithVersion);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            return licenses;
        }


        private void InitHttpClient()
        {
            if (mHttpClient != null) return;

            var httpClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5
            };

            if (!string.IsNullOrWhiteSpace(mSettings.ProxyURL))
            {
                var myProxy = new WebProxy(new Uri(mSettings.ProxyURL));
                if (mSettings.ProxySystemAuth)
                {
                    myProxy.Credentials = CredentialCache.DefaultCredentials;
                }
                httpClientHandler.Proxy = myProxy;
            }

            if (mSettings.IgnoreSslCertificateErrors)
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            }

            mHttpClient = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri(NUGET_URL),
                Timeout = TimeSpan.FromSeconds(mSettings.HttpClientTimeout)
            };
        }

        private bool SkipPackage(PackageNameAndVersion nameAndVersion)
        {
            return mSettings.IsExclusionMatch(nameAndVersion.Name);
        }

        /// <summary>
        /// Get nuget package info from a Cyclone DX Bom
        /// </summary>
        /// <param name="bomPath"></param>
        /// <returns></returns>
        internal IEnumerable<PackageNameAndVersion> GetPackagesFromBom(string bomPath)
        {
            string xmlString = File.ReadAllText(bomPath);
            Bom bom = Serializer.Deserialize(xmlString);
            Component = bom.Metadata.Component.Name;
            var packages = bom.Components.Select(c => new PackageNameAndVersion { Name = c.Name, Version = c.Version });
            return packages;
        }

        /// <summary>
        /// Download nuget and read license.
        /// </summary>
        /// <param name="nameVersion"></param>
        /// <param name="licenseFileName"></param>
        /// <returns></returns>
        private static async Task<string?> DownloadNugetAndGetText(PackageNameAndVersion nameVersion, string licenseFileName)
        {
            string? licenseText = null;
            if (mHttpClient == null)
            {
                Console.WriteLine("HttpClient is not initialized");
                return licenseText;
            }

            var nuspecUri = new Uri(string.Format(NUGET_FALLBACK, nameVersion.Name, nameVersion.Version));
            using var packageRequest = new HttpRequestMessage(HttpMethod.Get, nuspecUri);
            using var packageResponse = await mHttpClient.SendAsync(packageRequest, CancellationToken.None);
            if (!packageResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Could not get info from {nuspecUri}");
                return licenseText;
            }

            licenseText = await ReadLicenseFromNugetPackage(await packageResponse.Content.ReadAsStreamAsync(), licenseFileName);
            return licenseText;
        }

        /// <summary>
        /// Get license from local nuget
        /// </summary>
        /// <param name="nameVersion"></param>
        /// <param name="licenseFileName"></param>
        /// <returns></returns>
        private async Task<string?> OpenLocalNugetAndGetText(PackageNameAndVersion nameVersion, string licenseFileName)
        {
            string? licenseText = null;

            string nugetPath = GetNugetDirectory(nameVersion.Version, nameVersion.Name);
            var nuget = Directory.GetFiles(nugetPath, "*.nupkg").FirstOrDefault();
            if (nuget == null)
            {
                Console.WriteLine($"There is no nuget for {nameVersion}");
                return licenseText;
            }

            licenseText = await ReadLicenseFromNugetPackage(File.OpenRead(nuget), licenseFileName);
            return licenseText;
        }

        /// <summary>
        /// Unzip a nuget and read the license file.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="licenseFileName"></param>
        /// <returns></returns>
        private static async Task<string?> ReadLicenseFromNugetPackage(Stream s, string licenseFileName)
        {
            string? licenseText = null;
            using (var zip = new ZipArchive(s)) // zip archive
            {
                var license = zip.GetEntry(licenseFileName);
                if (license != null)
                {
                    var stream = license.Open();
                    using (var sr = new StreamReader(stream, Encoding.UTF8))
                    {
                        licenseText = await sr.ReadToEndAsync();
                    }
                }
            }
            return licenseText;
        }

        /// <summary>
        /// Download license text from a given url.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task<string?> GetLicenseFromUrl(string url)
        {
            string? licenseText = null;
            if (mHttpClient == null)
            {
                Console.WriteLine("HttpClient is not initialized");
                return licenseText;
            }

            url = GetRawLicenseUrlGithub(url);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await mHttpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Request to {url} failed with code {response.StatusCode}");
                return licenseText;
            }

            try
            {
                var array = await response.Content.ReadAsByteArrayAsync();
                licenseText = Encoding.UTF8.GetString(array);
                // Convert Html to text if necessary
                if (HtmlUtilities.IsHtmlFile(licenseText))
                {
                    licenseText = HtmlUtilities.ConvertToPlainText(licenseText);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return licenseText;
        }


        private static string GetRawLicenseUrlGithub(string url)
        {
            if (url.Contains("//github.com/"))
            {
                url = url.Replace("//github.com/", "//raw.githubusercontent.com/");
                url = url.Replace("blob/", string.Empty);
            }
            return url;
        }

        /// <summary>
        /// Download a nuspec file from a given url and read it.
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="nuspecUrl"></param>
        /// <param name="licenses"></param>
        /// <returns></returns>
        internal async Task<bool> DownloadNuspec(string identifier, string nuspecUrl, Dictionary<string, Package> licenses)
        {
            bool success = false;
            if (mHttpClient == null)
            {
                Console.WriteLine("HttpClient is not initialized");
                return success;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, nuspecUrl);
            var response = await mHttpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Request to {nuspecUrl} failed with code {response.StatusCode}");
                return success;
            }

            var content = await response.Content.ReadAsStreamAsync();
            success = ReadNuspecFile(content, identifier, licenses);

            return success;
        }

        /// <summary>
        /// Read a local nuspec file.
        /// </summary>
        /// <param name="nameVersion"></param>
        /// <param name="identifier"></param>
        /// <param name="packages"></param>
        /// <returns></returns>
        private bool LocalNuspec(PackageNameAndVersion nameVersion, string identifier, Dictionary<string, Package> packages)
        {
            bool success = false;
            string localFile = GetNuSpecPath(nameVersion.Version, nameVersion.Name);
            if (!File.Exists(localFile))
            {
                Console.WriteLine($"Nuspec {localFile} does not exists");
                return success;
            }

            Stream fs = File.OpenRead(localFile);
            success = ReadNuspecFile(fs, identifier, packages);
            return success;
        }

        /// <summary>
        /// Read nuspec file (xml) to Package object and add it
        /// to the package list.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="identifier"></param>
        /// <param name="packages"></param>
        /// <returns></returns>
        private bool ReadNuspecFile(Stream content, string identifier, Dictionary<string, Package> packages)
        {
            bool success = false;
            using (var sr = new StreamReader(content))
            {
                try
                {
                    var package = (Package?)mPackageSerializer.Deserialize(new NamespaceIgnorantXmlTextReader(sr));
                    if (package != null)
                    {
                        success = packages.TryAdd(identifier, package);
                        mPackageCache.TryAdd(identifier, package);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            return success;
        }

        /// <summary>
        /// Get License text for a given package and store it in package.Metadata.License.LicenseText.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="nameAndVersion"></param>
        /// <returns></returns>
        internal async Task UpdatePackageLicenseText(Package package, PackageNameAndVersion nameAndVersion)
        {
            // First, check for individual mappings
            if (mSettings.LicenseMappings.Any(x => x.Identifier.Equals(nameAndVersion.Name)))
            {
                var mapping = mSettings.LicenseMappings.First(x => x.Identifier.Equals(nameAndVersion.Name));
                var success = await UpdateLicense(package, mapping);
                if (success) return;
            }

            string licenseUrl = package.Metadata.LicenseUrl;
            if (string.IsNullOrEmpty(licenseUrl))
            {
                Console.WriteLine($"There is no license information for {nameAndVersion}");
                return;
            }

            string? licenseText;
            // Check for deprecated license
            if (licenseUrl.Equals(DEPRECATED_LICENSE_URL))
            {
                licenseText = await OpenLocalNugetAndGetText(nameAndVersion, package.Metadata.License.Text);
                if (string.IsNullOrEmpty(licenseText)) // try downloading from fallback uri
                {
                    licenseText = await DownloadNugetAndGetText(nameAndVersion, package.Metadata.License.Text);
                }
            }
            else
            {
                licenseText = await GetLicenseText(licenseUrl, package);
            }

            package.Metadata.License.LicenseText = string.IsNullOrEmpty(licenseText) ? string.Empty : licenseText;
            if (string.IsNullOrEmpty(licenseText))
            {
                Console.WriteLine($"No license text found for package {nameAndVersion}. License file is {package.Metadata.License.Text}");
            }
        }

        private static async Task<bool> UpdateLicense(Package package, LicenseMapping mapping)
        {
            bool ok = true;
            var mappingType = mapping.GetMappingType();
            switch (mappingType)
            {
                case MappingType.LOCAL:
                    package.Metadata.License.LicenseText = await File.ReadAllTextAsync(mapping.LicenseFile);
                    package.Metadata.License.Text = mapping.LicenseType;
                    break;
                case MappingType.URL:
                    package.Metadata.License.LicenseText = await GetLicenseFromUrl(mapping.MappedLicenseUrl) ?? string.Empty;
                    package.Metadata.License.Text = mapping.LicenseType;
                    break;
                default:
                    Console.WriteLine($"WARNING: Undefined mapping type for package {mapping.Identifier}");
                    ok = false;
                    break;
            }
            return ok;
        }

        /// <summary>
        /// Get license text from web.
        /// </summary>
        /// <param name="licenseUrl"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        private async Task<string?> GetLicenseText(string licenseUrl, Package package)
        {
            // Check if the url needs mapping
            if (mSettings.LicenseMappings.FirstOrDefault(x => x.OriginalLicenseUrl.Equals(licenseUrl)) is { } mapping
                && mapping.GetMappingType() is MappingType.URL)
            {
                licenseUrl = mapping.MappedLicenseUrl;
                package.Metadata.License.Text = mapping.LicenseType;
            }
            // Check if the license type is empty; if so, look for a mapping between url and type
            if (string.IsNullOrEmpty(package.Metadata.License.Text)
               && mSettings.LicenseMappings.FirstOrDefault(x => x.MappedLicenseUrl.Equals(licenseUrl)) is { } map)
            {
                package.Metadata.License.Text = map.LicenseType;
            }

            string text = await GetLicenseFromUrl(licenseUrl) ?? string.Empty;
            return text;
        }

        private string GetNugetDirectory(string version, string packageName) => Path.Combine(mNugetRootPath, packageName, version);

        private string GetNuSpecPath(string version, string packageName) => Path.Combine(GetNugetDirectory(version, packageName), $"{packageName}.nuspec");


        private static HttpClient? mHttpClient;

        private readonly LicenseUtilitySettings mSettings;
        private readonly Dictionary<string, Package> mPackageCache = new();
        private readonly XmlSerializer mPackageSerializer = new XmlSerializer(typeof(Package));

        // Search nuspec in local cache
        private readonly string mNugetRootPath = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

        public string Component { get; private set; } = string.Empty;
    }
}
