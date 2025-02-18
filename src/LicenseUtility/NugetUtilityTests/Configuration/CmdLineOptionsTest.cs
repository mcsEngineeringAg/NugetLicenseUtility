//--------------------------------------------------------------------------------------------------
// All rights reserved to TRUMPF Werkzeugmaschinen SE + Co. KG, Germany
// -------------------------------------------------------------------------------------------------

using CommandLine;
using NugetUtility.Configuration;

namespace NugetUtilityTests.Configuration
{
    [TestFixture]
    public class CmdLineOptionsTest
    {
        #region private constants
        #endregion

        #region constructor
        #endregion

        #region setup methods
        #endregion

        #region test methods

        [Test]
        public void ParseCmdLine_All()
        {
            string[] args = ["--bom=\"C:\\temp\\small_bom.xml\"", "--output-folder=\"C:\\temp\"", "--config=\"C:\\temp\\config.json\""];
            Parser.Default.ParseArguments<CmdLineOptions>(args)
                .WithParsed(option =>
                {
                    Assert.That(option.BomFile, Is.EqualTo("\"C:\\temp\\small_bom.xml\""));
                    Assert.That(option.OutputFolder, Is.EqualTo("\"C:\\temp\""));
                    Assert.That(option.ConfigFile, Is.EqualTo("\"C:\\temp\\config.json\""));
                });
        }

        [Test]
        public void ParseCmdLine_NoArgs()
        {
            string[] args = [];
            Parser.Default.ParseArguments<CmdLineOptions>(args)
                .WithParsed(option =>
                {
                    Assert.That(option.BomFile, Is.EqualTo("bom.xml"));
                    Assert.That(option.OutputFolder, Is.EqualTo(string.Empty));
                    Assert.That(option.ConfigFile, Is.EqualTo(string.Empty));
                });
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
