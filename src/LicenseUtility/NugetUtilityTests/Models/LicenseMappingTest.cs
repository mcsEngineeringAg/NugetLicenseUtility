//--------------------------------------------------------------------------------------------------
// All rights reserved to TRUMPF Werkzeugmaschinen SE + Co. KG, Germany
// -------------------------------------------------------------------------------------------------

using NugetUtility.Model;

namespace NugetUtilityTests.Models
{
    [TestFixture]
    public class LicenseMappingTest
    {
        #region private constants
        #endregion

        #region constructor
        #endregion

        #region setup methods
        #endregion

        #region test methods

        [Test]
        public void CheckEmptyMapping()
        {
            var emptyMapping = new LicenseMapping();
            Assert.That(emptyMapping.GetMappingType(), Is.EqualTo(MappingType.UNKNOWN));
        }

        [Test]
        public void CheckLocalMapping()
        {
            var mapping = new LicenseMapping()
            {
                Identifier = "Abc.Lib",
                ReadLocalLicense = true,
                LicenseFile = TestUtility.GetTestFilePath("License.txt"),
                LicenseType = "MIT",
                MappedLicenseUrl = "https://opensource.org/licenses/MIT"
            };
            Assert.That(mapping.GetMappingType(), Is.EqualTo(MappingType.LOCAL));
        }

        [Test]
        public void CheckLocalMapping_FileNotExist()
        {
            var mapping = new LicenseMapping()
            {
                Identifier = "Abc.Lib",
                ReadLocalLicense = true,
                LicenseFile = TestUtility.GetTestFilePath("HalliHallo.txt"),
                LicenseType = "MIT",
                MappedLicenseUrl = ""
            };
            Assert.That(mapping.GetMappingType(), Is.EqualTo(MappingType.UNKNOWN));

            mapping.MappedLicenseUrl = "https://opensource.org/licenses/MIT";
            Assert.That(mapping.GetMappingType(), Is.EqualTo(MappingType.URL));
        }

        [Test]
        public void CheckLocalMapping_MissingIdentifier()
        {
            var mapping = new LicenseMapping()
            {
                Identifier = "",
                ReadLocalLicense = true,
                LicenseFile = TestUtility.GetTestFilePath("License.txt"),
                LicenseType = "MIT",
                MappedLicenseUrl = ""
            };
            Assert.That(mapping.GetMappingType(), Is.EqualTo(MappingType.UNKNOWN));

            mapping.MappedLicenseUrl = "https://opensource.org/licenses/MIT";
            Assert.That(mapping.GetMappingType(), Is.EqualTo(MappingType.URL));
        }

        [Test]
        public void CheckUrlMapping()
        {
            var mapping = new LicenseMapping()
            {
                ReadLocalLicense = false,
                LicenseType = "MIT",
                OriginalLicenseUrl = "http://licenses/deprecated/MIT",
                MappedLicenseUrl = "https://opensource.org/licenses/MIT"
            };
            Assert.That(mapping.GetMappingType(), Is.EqualTo(MappingType.URL));

            mapping.OriginalLicenseUrl = "";
            Assert.That(mapping.GetMappingType(), Is.EqualTo(MappingType.URL));
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
