using System.Collections.Generic;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class WindowsImpersonationFeature : WebConfigFeature
    {
        private Dictionary<string, object> _attributeValues;
        private IEnumerable<string> _elementPaths;

        /// <summary>
        /// Returns a dictionary of attribute name-value pairs required to confirm this feature
        /// </summary>
        protected override Dictionary<string, object> AttributeValues
        {
            get
            {
                _attributeValues ??= new Dictionary<string, object>
                {
                    {Constants.ImpersonateAttribute, true }
                };

                return _attributeValues;
            }
        }

        /// <summary>
        /// Returns a list of XML element paths to inspect for the attribute name-value pairs defined in <see cref="AttributeValues"/> 
        /// </summary>
        protected override IEnumerable<string> ElementPaths
        {
            get
            {
                _elementPaths ??= new[]
                {
                    $"{Constants.IdentityElement}",
                    $"{Constants.SystemWebElement}/{Constants.IdentityElement}",
                    $"{Constants.ConfigurationElement}/{Constants.SystemWebElement}/{Constants.IdentityElement}"
                };

                return _elementPaths;
            }
        }
    }
}
