using NugetUtility.Configuration;

namespace NugetUtilityTests.Configuration
{
    [TestFixture]
    public class LicenseUtilitySettingsTest
    {
        #region private constants
        #endregion

        #region constructor
        #endregion

        #region setup methods
        #endregion

        #region test methods

        [Test]
        public void Load_Config_Ok()
        {
            // Arrange
            string configFile = TestUtility.GetTestFilePath("config.json");

            // Act
            var settings = LicenseUtilitySettings.Load(configFile);

            // Assert
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.AllowedLicenseTypes, Has.Exactly(1).EqualTo("MIT"));
            Assert.That(settings.OutputFileType, Is.EqualTo(OutputFileType.MARKDOWN));
            Assert.That(settings.LicenseMappings.Count, Is.EqualTo(2));
            Assert.That(settings.ExcludedPackages.Count, Is.EqualTo(2));
        }

        [Test]
        public void Load_Config_NotFound()
        {
            // Arrange
            string configFile = @"C:\not\a\path\config.json";

            // Act
            var settings = LicenseUtilitySettings.Load(configFile);

            // Assert
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.OutputFileType, Is.EqualTo(OutputFileType.JSON));
            Assert.That(settings.AllowedLicenseTypes, Is.Empty);
            Assert.That(settings.ForbiddenLicenseTypes, Is.Empty);
            Assert.That(settings.LicenseMappings, Is.Empty);
            Assert.That(settings.ExcludedPackages, Is.Empty);
        }

        [Test]
        public void Load_Config_Empty()
        {
            var settings = LicenseUtilitySettings.Load(string.Empty);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.OutputFileType, Is.EqualTo(OutputFileType.JSON));
            Assert.That(settings.AllowedLicenseTypes, Is.Empty);
            Assert.That(settings.ForbiddenLicenseTypes, Is.Empty);
            Assert.That(settings.LicenseMappings, Is.Empty);
            Assert.That(settings.ExcludedPackages, Is.Empty);
        }

        [Test]
        public void Load_Config_Invalid()
        {
            const string json = @"{
	""ProxyURL"": """",
	""ProxySystemAuth"": false,
	""IgnoreSslCertificateErrors"": false,
	""HttpClientTimeout"": 10,
	""AllowedLicenseTypes"": [],
	""ForbiddenLicenseTypes"": [],
	""OutputFileType"": ""NOTEPAD"",
	""LicenseMappings"": [],
	""ExcludedPackages"": []
}";

            var settings = TestUtility.LoadJsonConfig(json);

            // Assert
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.OutputFileType, Is.EqualTo(OutputFileType.JSON));
            Assert.That(settings.AllowedLicenseTypes, Is.Empty);
            Assert.That(settings.ForbiddenLicenseTypes, Is.Empty);
            Assert.That(settings.LicenseMappings, Is.Empty);
            Assert.That(settings.ExcludedPackages, Is.Empty);
        }

        [Test]
        public void Exclude_Packages()
        {
            var settings = new LicenseUtilitySettings
            {
                ExcludedPackages = ["System.*", "Microsoft.Extensions.Logging", "*mcs*.**", "mcs.Dummy.Logging"]
            };

            var hardExclusions = settings.HardPackageExclusions.ToList();
            var wildcardExclusions = settings.WildcardPackageExclusions.ToList();

            Assert.That(hardExclusions, Has.Member("Microsoft.Extensions.Logging"));
            Assert.That(hardExclusions, Has.Member("mcs.Dummy.Logging"));
            Assert.That(wildcardExclusions, Has.Member("System."));
            Assert.That(wildcardExclusions, Has.Member("mcs."));
        }

        [Test]
        public void ExclusionMatch_Test()
        {
            var settings = new LicenseUtilitySettings
            {
                ExcludedPackages = ["System.*", "Microsoft.Extensions.Logging", "*mcs*.**", "mcs.Dummy.Logging"]
            };

            Assert.That(settings.IsExclusionMatch("mcs.Dummy.Logging"), Is.True);
            Assert.That(settings.IsExclusionMatch("Microsoft.Extensions.Logging"), Is.True);
            Assert.That(settings.IsExclusionMatch("System.Text.Json"), Is.True);
            Assert.That(settings.IsExclusionMatch("Tut.mcs.CellControl"), Is.True);
            Assert.That(settings.IsExclusionMatch("Aldisoft.Messaging"), Is.False);
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
