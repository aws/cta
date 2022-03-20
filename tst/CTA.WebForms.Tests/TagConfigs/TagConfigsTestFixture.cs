using CTA.WebForms.Helpers;
using CTA.WebForms.Helpers.TagConversion;
using CTA.WebForms.Services;
using CTA.WebForms.TagConverters;
using HtmlAgilityPack;
using NUnit.Framework;
using System.Collections.Generic;

namespace CTA.WebForms.Tests.TagConfigs
{
    [TestFixture]
    public class TagConfigsTestFixture : WebFormsTestBase
    {
        private protected IDictionary<string, TagConverter> _configMap;
        private protected ViewImportService _viewImportService;
        private protected CodeBehindReferenceLinkerService _codeBehindLinkerService;

        [SetUp]
        public void SetUp()
        {
            _codeBehindLinkerService = new CodeBehindReferenceLinkerService();
            _viewImportService = new ViewImportService();

            var configParser = new TagConfigParser(Rules.Config.Constants.TagConfigsExtractedPath);
            _configMap = configParser.GetConfigMap();

            foreach (var kvp in _configMap)
            {
                kvp.Value.Initialize(_codeBehindLinkerService, _viewImportService);
            }
        }

        private protected string GetConverterOutput(string inputText)
        {
            var doc = new HtmlDocument();
            doc.OptionOutputOriginalCase = true;
            doc.LoadHtml(inputText);

            var node = doc.DocumentNode.ChildNodes[0];

            var converter = _configMap[node.Name];
            converter.MigrateTag(node);

            Utilities.NormalizeHtmlContent(doc.DocumentNode);

            return doc.DocumentNode.WriteTo();
        }
    }
}
