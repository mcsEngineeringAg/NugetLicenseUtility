using NugetUtility;
using NugetUtility.Configuration;

namespace NugetUtilityTests
{
    [TestFixture]
    public class MainFunctionTest
    {
        #region private constants
        #endregion

        #region constructor
        #endregion

        #region setup methods
        #endregion

        #region test methods

        [Test]
        public async Task Bom_Missing()
        {
            var options = new CmdLineOptions() { BomFile = "bliblablubb.xml" };

            var result = await Program.Execute(options);

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task FoorbiddenAllowed_Check()
        {
            var options = new CmdLineOptions()
            {
                BomFile = @"TestFiles\bom.xml",
                ConfigFile = @"TestFiles\config_corrupted.json"
            };

            var result = await Program.Execute(options);

            Assert.That(result, Is.EqualTo(1));
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
