using NugetUtility.Configuration;

namespace NugetUtilityTests
{
    public static class TestUtility
    {
        public static LicenseUtilitySettings LoadJsonConfig(string json)
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            var configFile = Path.Combine(tempDirectory, "dummy.json");
            File.WriteAllText(configFile, json);

            var settings = LicenseUtilitySettings.Load(configFile);
            Directory.Delete(tempDirectory, true);

            return settings;
        }

        public static string GetTestFilePath(string fileName)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", fileName);
        }
    }
}
