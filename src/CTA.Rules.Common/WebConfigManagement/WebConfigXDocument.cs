using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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

        public IEnumerable<XElement> GetConfigProperties()
        {
            return _webConfig.Descendants();
        }

        public IEnumerable<XElement> GetConfigPropertiesByName(string propertyName)
        {
            return GetConfigProperties().Where(n => n.Name.LocalName == propertyName);
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
