using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using CTA.Rules.Common.WebConfigManagement;
using NUnit.Framework;

namespace CTA.Rules.Test.Common
{
    public class WebConfigXDocumentTests
    {
        private string _configsDir;
        private WebConfigXDocument _validConfig;
        private WebConfigXDocument _emptyConfig;

        [SetUp]
        public void Setup()
        {
            _configsDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestFiles", "Configs");
            var validXDoc = XDocument.Load(Path.Combine(_configsDir, "authWeb.config"));

            _validConfig = new WebConfigXDocument(validXDoc);
            _emptyConfig = new WebConfigXDocument(null);
        }

        [Test]
        public void GetConfigProperties_Returns_All_Elements_In_Config()
        {
            var properties = _validConfig.GetConfigProperties();
            Assert.AreEqual(41, properties.Count());
        }

        [Test]
        public void GetConfigProperties_Returns_Empty_Collection_When_Config_Is_Empty()
        {
            var properties = _emptyConfig.GetConfigProperties();
            Assert.AreEqual(0, properties.Count());
        }

        [Test]
        public void GetConfigPropertiesByName_Returns_All_Elements_In_Config_With_Name()
        {
            var properties = _validConfig.GetConfigPropertiesByName("add");
            Assert.AreEqual(4, properties.Count());
        }

        [Test]
        public void GetConfigPropertiesByName_Returns_Empty_Collection_If_Name_Does_Not_Exist()
        {
            var properties = _validConfig.GetConfigPropertiesByName("ThisPropertyDoesNotExist");
            Assert.AreEqual(0, properties.Count());
        }

        [Test]
        public void GetConfigPropertiesByName_Returns_Empty_Collection_If_Config_Is_Empty()
        {
            var properties = _emptyConfig.GetConfigPropertiesByName("ThisPropertyDoesNotExist");
            Assert.AreEqual(0, properties.Count());
        }

        [Test]
        public void ContainsAttributeWithValue_Returns_True_If_AttributeValue_Exists()
        {
            Assert.True(_validConfig.ContainsAttributeWithValue("configuration/appSettings/add", "key", "webpages:Version"));
        }

        [Test]
        public void ContainsAttributeWithValue_Returns_False_If_AttributeValue_Does_Not_Exist()
        {
            Assert.False(_validConfig.ContainsAttributeWithValue("configuration/appSettings/add", "key", "ValueThatDoesNotExist"));
        }

        [Test]
        public void ContainsAttributeWithValue_Returns_False_If_Config_Is_Empty()
        {
            Assert.False(_emptyConfig.ContainsAttributeWithValue("valuesCanBeAnything", "valuesCanBeAnything", "valuesCanBeAnything"));
        }

        [Test]
        public void ContainsAttribute_Returns_True_If_Attribute_Exists()
        {
            Assert.True(_validConfig.ContainsAttribute("configuration/appSettings/add", "key"));
        }

        [Test]
        public void ContainsAttribute_Returns_False_If_Attribute_Does_Not_Exists()
        {
            Assert.False(_validConfig.ContainsAttribute("configuration/appSettings/add", "NonexistentAttribute"));
        }

        [Test]
        public void ContainsAttribute_Returns_False_If_Config_Is_Empty()
        {
            Assert.False(_emptyConfig.ContainsAttribute("valuesCanBeAnything", "valuesCanBeAnything"));
        }

        [Test]
        public void ContainsElement_Returns_True_If_Element_Exists()
        {
            Assert.True(_validConfig.ContainsElement("configuration/system.web/authorization"));
        }

        [Test]
        public void ContainsElement_Returns_False_If_Element_Does_Not_Exists()
        {
            Assert.False(_validConfig.ContainsElement("NonexistentElement"));
        }

        [Test]
        public void ContainsElement_Returns_False_If_Config_Is_Empty()
        {
            Assert.False(_emptyConfig.ContainsElement("valuesCanBeAnything"));
        }
    }
}