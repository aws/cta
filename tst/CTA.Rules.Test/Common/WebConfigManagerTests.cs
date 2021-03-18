using System.IO;
using System.Linq;
using System.Reflection;
using CTA.Rules.Common.WebConfigManagement;
using NUnit.Framework;

namespace CTA.Rules.Test.Common
{
    public class WebConfigManagerTests
    {
        private string _configsDir;

        [SetUp]
        public void Setup()
        {
            _configsDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestFiles", "Configs");
        }

        [Test]
        public void LoadWebConfigAsXDocument_Loads_WebConfig_As_XDocument()
        {
            var xDocumentWebConfig = WebConfigManager.LoadWebConfigAsXDocument(_configsDir);
            Assert.True(xDocumentWebConfig.ContainsElement("configuration"));
        }

        [Test]
        public void LoadWebConfigAsXDocument_Loads_Empty_Config_If_No_Config_Exists()
        {
            var xDocumentWebConfig = WebConfigManager.LoadWebConfigAsXDocument("");
            Assert.AreEqual(0, xDocumentWebConfig.GetConfigProperties().Count());
        }
    }
}