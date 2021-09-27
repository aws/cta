using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CTA.Rules.Common.Extensions;

namespace CTA.Rules.Common.WebConfigManagement
{
    public class WebConfigXDocument
    {
        private readonly XDocument _webConfig;

        public WebConfigXDocument(XDocument webConfig)
        {
            _webConfig = webConfig ?? new XDocument();
        }

        public string GetDocAsString()
        {
            return _webConfig.Document.ToString();
        }

        public IEnumerable<XElement> GetConfigProperties()
        {
            return _webConfig.Descendants();
        }

        public IEnumerable<XElement> GetConfigPropertiesByName(string propertyName)
        {
            return GetConfigProperties().Where(n => n.Name.LocalName == propertyName);
        }

        public XElement GetElementByPath(string path)
        {
            return _webConfig.XPathSelectElement(path);
        }

        public IEnumerable<XElement> GetDescendantsAndSelf(string element)
        {
            return _webConfig.Root.DescendantsAndSelf(element);
        }

        public IEnumerable<XElement> GetElementsByPath(string path)
        {
            return _webConfig.XPathSelectElements(path);
        }

        public bool ContainsElement(string elementPath)
        {
            return _webConfig.ContainsElementPath(elementPath);
        }

        public bool ContainsAttribute(string elementPath, string attributeName)
        {
            return _webConfig.ContainsAttribute(elementPath, attributeName);
        }

        public bool ContainsAttributeWithValue(string elementPath, string attributeName, string value)
        {
            return _webConfig.ContainsAttributeValue(elementPath, attributeName, value);
        }
    }
}
