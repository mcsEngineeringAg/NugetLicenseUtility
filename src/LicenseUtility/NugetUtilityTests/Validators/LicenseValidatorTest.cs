using NugetUtility.Model;
using NugetUtility.Validators;

namespace NugetUtilityTests.Validators
{
    [TestFixture]
    public class LicenseValidatorTest
    {
        #region private constants
        #endregion

        #region constructor
        #endregion

        #region setup methods
        #endregion

        #region test methods

        [Test]
        public static void Check_AnyLicense()
        {
            const string json = @"{
	""ProxyURL"": """",
	""ProxySystemAuth"": false,
	""IgnoreSslCertificateErrors"": false,
	""HttpClientTimeout"": 10,
	""AllowedLicenseTypes"": [],
	""ForbiddenLicenseTypes"": [],
	""OutputFileType"": ""JSON"",
	""LicenseMappings"": [],
	""ExcludedPackages"": []
}";

            var package1 = new Package();
            package1.Metadata.LicenseUrl = "https://opensource.org/licenses/MIT";
            package1.Metadata.License.Text = "MIT";
            var package2 = new Package();
            package2.Metadata.License.Text = "mcs";
            var package3 = new Package();
            package3.Metadata.LicenseUrl = "https://opensource.org/licenses/MIT";
            package3.Metadata.License.Text = "MIT";

            Dictionary<string, Package> packages = new()
            {
                {"Mcs.Package.Test", package1 },
                {"mcs.Boost.Extensions", package2 },
                {"Mock.Pack.Utils", package3 }
            };

            var settings = TestUtility.LoadJsonConfig(json);
            var validator = new LicenseValidator(settings);

            Assert.That(validator.ValidateLicense(packages), Is.True);
        }

        [Test]
        public static void Check_MITonly_Pass()
        {
            const string json = @"{
	""ProxyURL"": """",
	""ProxySystemAuth"": false,
	""IgnoreSslCertificateErrors"": false,
	""HttpClientTimeout"": 10,
	""AllowedLicenseTypes"": [""MIT""],
	""ForbiddenLicenseTypes"": [],
	""OutputFileType"": ""JSON"",
	""LicenseMappings"": [],
	""ExcludedPackages"": []
}";

            var package1 = new Package();
            package1.Metadata.LicenseUrl = "https://opensource.org/licenses/MIT";
            package1.Metadata.License.Text = "MIT";
            var package2 = new Package();
            package2.Metadata.License.Text = "MIT";
            var package3 = new Package();
            package3.Metadata.LicenseUrl = "https://opensource.org/licenses/MIT";
            package3.Metadata.License.Text = "MIT";

            Dictionary<string, Package> packages = new()
            {
                {"Mcs.Package.Test", package1 },
                {"mcs.Boost.Extensions", package2 },
                {"Mock.Pack.Utils", package3 }
            };

            var settings = TestUtility.LoadJsonConfig(json);
            var validator = new LicenseValidator(settings);

            Assert.That(validator.ValidateLicense(packages), Is.True);
        }

        [Test]
        public static void Check_MITonly_IgnoreEmptyLicense()
        {
            const string json = @"{
	""ProxyURL"": """",
	""ProxySystemAuth"": false,
	""IgnoreSslCertificateErrors"": false,
	""HttpClientTimeout"": 10,
	""AllowedLicenseTypes"": [""MIT""],
	""ForbiddenLicenseTypes"": [],
	""OutputFileType"": ""JSON"",
	""LicenseMappings"": [],
	""ExcludedPackages"": []
}";

            var package1 = new Package();
            package1.Metadata.LicenseUrl = "https://opensource.org/licenses/MIT";
            package1.Metadata.License.Text = "MIT";
            var package2 = new Package();
            var package3 = new Package();
            package3.Metadata.LicenseUrl = "https://opensource.org/licenses/MIT";

            Dictionary<string, Package> packages = new()
            {
                {"Mcs.Package.Test", package1 },
                {"mcs.Boost.Extensions", package2 },
                {"Mock.Pack.Utils", package3 }
            };

            var settings = TestUtility.LoadJsonConfig(json);
            var validator = new LicenseValidator(settings);

            Assert.That(validator.ValidateLicense(packages), Is.True);
        }

        [Test]
        public static void Check_MITonly_Fail()
        {
            const string json = @"{
	""ProxyURL"": """",
	""ProxySystemAuth"": false,
	""IgnoreSslCertificateErrors"": false,
	""HttpClientTimeout"": 10,
	""AllowedLicenseTypes"": [""MIT""],
	""ForbiddenLicenseTypes"": [],
	""OutputFileType"": ""JSON"",
	""LicenseMappings"": [],
	""ExcludedPackages"": []
}";

            var package1 = new Package();
            package1.Metadata.LicenseUrl = "https://opensource.org/licenses/MIT";
            package1.Metadata.License.Text = "MIT";
            var package2 = new Package();
            package2.Metadata.License.Text = "mcs";
            var package3 = new Package();
            package3.Metadata.LicenseUrl = "https://opensource.org/licenses/MIT";
            package3.Metadata.License.Text = "MIT";

            Dictionary<string, Package> packages = new()
            {
                {"Mcs.Package.Test", package1 },
                {"mcs.Boost.Extensions", package2 },
                {"Mock.Pack.Utils", package3 }
            };

            var settings = TestUtility.LoadJsonConfig(json);
            var validator = new LicenseValidator(settings);

            Assert.That(validator.ValidateLicense(packages), Is.False);
        }

        [Test]
        public static void Check_Apache_Forbidden_Fail()
        {
            const string json = @"{
	""ProxyURL"": """",
	""ProxySystemAuth"": false,
	""IgnoreSslCertificateErrors"": false,
	""HttpClientTimeout"": 10,
	""AllowedLicenseTypes"": [],
	""ForbiddenLicenseTypes"": [""Apache-2.0""],
	""OutputFileType"": ""JSON"",
	""LicenseMappings"": [],
	""ExcludedPackages"": []
}";

            var package1 = new Package();
            package1.Metadata.LicenseUrl = "https://opensource.org/licenses/MIT";
            package1.Metadata.License.Text = "MIT";
            var package2 = new Package();
            package2.Metadata.License.Text = "mcs";
            var package3 = new Package();
            package3.Metadata.LicenseUrl = "https://opensource.org/licenses/MIT";
            package3.Metadata.License.Text = "Apache-2.0";

            Dictionary<string, Package> packages = new()
            {
                {"Mcs.Package.Test", package1 },
                {"mcs.Boost.Extensions", package2 },
                {"Mock.Pack.Utils", package3 }
            };

            var settings = TestUtility.LoadJsonConfig(json);
            var validator = new LicenseValidator(settings);

            Assert.That(validator.ValidateLicense(packages), Is.False);
        }

        [Test]
        public static void Check_Apache_Forbidden_Pass()
        {
            const string json = @"{
	""ProxyURL"": """",
	""ProxySystemAuth"": false,
	""IgnoreSslCertificateErrors"": false,
	""HttpClientTimeout"": 10,
	""AllowedLicenseTypes"": [],
	""ForbiddenLicenseTypes"": [""Apache-2.0""],
	""OutputFileType"": ""JSON"",
	""LicenseMappings"": [],
	""ExcludedPackages"": []
}";

            var package1 = new Package();
            package1.Metadata.LicenseUrl = "https://opensource.org/licenses/MIT";
            package1.Metadata.License.Text = "MIT";
            var package2 = new Package();
            package2.Metadata.License.Text = "mcs";
            var package3 = new Package();
            package3.Metadata.LicenseUrl = "https://opensource.org/licenses/MIT";
            package3.Metadata.License.Text = "Apache-1.1";

            Dictionary<string, Package> packages = new()
            {
                {"Mcs.Package.Test", package1 },
                {"mcs.Boost.Extensions", package2 },
                {"Mock.Pack.Utils", package3 }
            };

            var settings = TestUtility.LoadJsonConfig(json);
            var validator = new LicenseValidator(settings);

            Assert.That(validator.ValidateLicense(packages), Is.True);
        }

        [Test]
        public static void Check_Apache_Forbidden_IgnoreEmptyLicense()
        {
            const string json = @"{
	""ProxyURL"": """",
	""ProxySystemAuth"": false,
	""IgnoreSslCertificateErrors"": false,
	""HttpClientTimeout"": 10,
	""AllowedLicenseTypes"": [],
	""ForbiddenLicenseTypes"": [""Apache-2.0""],
	""OutputFileType"": ""JSON"",
	""LicenseMappings"": [],
	""ExcludedPackages"": []
}";

            var package1 = new Package();
            package1.Metadata.LicenseUrl = "https://opensource.org/licenses/MIT";
            package1.Metadata.License.Text = "MIT";
            var package2 = new Package();
            var package3 = new Package();
            package3.Metadata.LicenseUrl = "https://opensource.org/licenses/MIT";

            Dictionary<string, Package> packages = new()
            {
                {"Mcs.Package.Test", package1 },
                {"mcs.Boost.Extensions", package2 },
                {"Mock.Pack.Utils", package3 }
            };

            var settings = TestUtility.LoadJsonConfig(json);
            var validator = new LicenseValidator(settings);

            Assert.That(validator.ValidateLicense(packages), Is.True);
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
