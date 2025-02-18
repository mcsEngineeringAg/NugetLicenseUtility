//--------------------------------------------------------------------------------------------------
// All rights reserved to TRUMPF Werkzeugmaschinen SE + Co. KG, Germany
// -------------------------------------------------------------------------------------------------

using NugetUtility;
using NugetUtility.Configuration;
using NugetUtility.Logic;
using NugetUtility.Model;

namespace NugetUtilityTests.Logic
{
    [TestFixture]
    public class LicenseManagerTest
    {
        #region private constants
        #endregion

        #region constructor
        #endregion

        #region setup methods
        #endregion

        #region test methods

        [Test]
        public void GetPackagesFromBomTest()
        {
            var bomPath = TestUtility.GetTestFilePath("bom.xml");
            var manager = new LicenseManager(new LicenseUtilitySettings());
            var packages = manager.GetPackagesFromBom(bomPath).ToList();

            Assert.That(packages, Is.Not.Empty);
            Assert.That(packages, Has.Count.EqualTo(6));
            Assert.That(manager.Component, Is.EqualTo("FluxCutAdapter"));
        }

        [Test]
        public async Task DownloadNuspecTest()
        {
            var bomPath = TestUtility.GetTestFilePath("bom.xml");
            var licenses = new Dictionary<string, Package>();
            var settings = new LicenseUtilitySettings();
            var manager = new LicenseManager(settings);

            foreach (var packageWithVersion in manager.GetPackagesFromBom(bomPath))
            {
                var identifier = $"{packageWithVersion.Name}_{packageWithVersion.Version}";
                var nugetUri = $"{packageWithVersion.Name}/{packageWithVersion.Version}/{packageWithVersion.Name}.nuspec".ToLowerInvariant();

                await manager.DownloadNuspec(identifier, nugetUri, licenses);
            }

            Assert.That(licenses, Is.Not.Empty);
            Assert.That(licenses, Has.Count.EqualTo(4));
        }

        [Test]
        public async Task PackageLicense_Specific_Local()
        {
            var mapping = new LicenseMapping()
            {
                Identifier = "Trumpf.FluxServiceAPI",
                ReadLocalLicense = true,
                LicenseFile = TestUtility.GetTestFilePath("License.txt"),
                LicenseType = "TRUMPF"
            };

            var nameAndVersion = new PackageNameAndVersion() { Name = "Trumpf.FluxServiceAPI", Version = "4.2.2" };
            var nugetPackage = new Package();
            var settings = new LicenseUtilitySettings() { LicenseMappings = [mapping] };
            var manager = new LicenseManager(settings);

            await manager.UpdatePackageLicenseText(nugetPackage, nameAndVersion);

            Assert.That(nugetPackage.Metadata.License.Text, Is.EqualTo("TRUMPF"));
            Assert.That(nugetPackage.Metadata.License.LicenseText, Is.EqualTo("This is a license text."));

        }

        [Test]
        public async Task PackageLicense_Specific_Url()
        {
            var mapping = new LicenseMapping()
            {
                Identifier = "Trumpf.FluxServiceAPI",
                ReadLocalLicense = false,
                MappedLicenseUrl = "https://opensource.org/licenses/MIT",
                LicenseType = "MIT"
            };

            var nameAndVersion = new PackageNameAndVersion() { Name = "Trumpf.FluxServiceAPI", Version = "4.2.2" };
            var nugetPackage = new Package();
            var settings = new LicenseUtilitySettings() { LicenseMappings = [mapping] };
            var manager = new LicenseManager(settings);

            await manager.UpdatePackageLicenseText(nugetPackage, nameAndVersion);

            Assert.That(nugetPackage.Metadata.License.Text, Is.EqualTo("MIT"));
            Assert.That(nugetPackage.Metadata.License.LicenseText, Is.Not.Empty);

        }

        [Test]
        public async Task PackageLicense_Generic_Url_Mapped()
        {
            var mapping = new LicenseMapping()
            {
                OriginalLicenseUrl = "http://licenses/deprecated/MIT",
                MappedLicenseUrl = "https://opensource.org/licenses/MIT",
                LicenseType = "MIT"
            };

            var nameAndVersion = new PackageNameAndVersion() { Name = "Trumpf.FluxServiceAPI", Version = "4.2.2" };
            var nugetPackage = new Package();
            nugetPackage.Metadata.LicenseUrl = "http://licenses/deprecated/MIT";
            var settings = new LicenseUtilitySettings() { LicenseMappings = [mapping] };
            var manager = new LicenseManager(settings);

            await manager.UpdatePackageLicenseText(nugetPackage, nameAndVersion);

            Assert.That(nugetPackage.Metadata.License.Text, Is.EqualTo("MIT"));
            Assert.That(nugetPackage.Metadata.License.LicenseText, Is.Not.Empty);

        }

        [Test]
        public async Task PackageLicense_Generic_Url()
        {
            var nameAndVersion = new PackageNameAndVersion() { Name = "Trumpf.FluxServiceAPI", Version = "4.2.2" };
            var nugetPackage = new Package();
            nugetPackage.Metadata.LicenseUrl = "https://opensource.org/licenses/MIT";
            nugetPackage.Metadata.License.Text = "MIT";
            var settings = new LicenseUtilitySettings();
            var manager = new LicenseManager(settings);

            await manager.UpdatePackageLicenseText(nugetPackage, nameAndVersion);

            Assert.That(nugetPackage.Metadata.License.Text, Is.EqualTo("MIT"));
            Assert.That(nugetPackage.Metadata.License.LicenseText, Is.Not.Empty);

        }

        [Test]
        public async Task PackageLicense_Generic_Url_NotFound()
        {
            var nameAndVersion = new PackageNameAndVersion() { Name = "Trumpf.FluxServiceAPI", Version = "4.2.2" };
            var nugetPackage = new Package();
            nugetPackage.Metadata.LicenseUrl = "https://opensource.org/licenses/MITTER";
            nugetPackage.Metadata.License.Text = "MIT";
            var settings = new LicenseUtilitySettings();
            var manager = new LicenseManager(settings);

            await manager.UpdatePackageLicenseText(nugetPackage, nameAndVersion);

            Assert.That(nugetPackage.Metadata.License.LicenseText, Is.Empty);

        }

        [Test]
        public async Task PackageLicense_Generic_Url_SetLicenseType()
        {
            var mapping = new LicenseMapping()
            {
                MappedLicenseUrl = "https://opensource.org/licenses/MIT",
                LicenseType = "MIT"
            };
            var nameAndVersion = new PackageNameAndVersion() { Name = "Trumpf.FluxServiceAPI", Version = "4.2.2" };
            var nugetPackage = new Package();
            nugetPackage.Metadata.LicenseUrl = "https://opensource.org/licenses/MIT";
            var settings = new LicenseUtilitySettings() { LicenseMappings = [mapping] };
            var manager = new LicenseManager(settings);

            await manager.UpdatePackageLicenseText(nugetPackage, nameAndVersion);

            Assert.That(nugetPackage.Metadata.License.Text, Is.EqualTo("MIT"));
            Assert.That(nugetPackage.Metadata.License.LicenseText, Is.Not.Empty);

        }

        #endregion

        #region teardown methods
        #endregion

        #region private methods
        #endregion

        #region private members
        #endregion

        #region properties
        #endregion
    }
}
