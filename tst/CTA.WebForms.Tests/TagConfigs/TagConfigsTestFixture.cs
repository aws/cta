using CTA.WebForms.Helpers;
using CTA.WebForms.Helpers.TagConversion;
using CTA.WebForms.Services;
using HtmlAgilityPack;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CTA.WebForms.Tests.TagConfigs
{
    [TestFixture]
    public class TagConfigsTestFixture : WebFormsTestBase
    {
        private protected TagConfigParser _tagConfigParser;
        private protected ViewImportService _viewImportService;
        private protected CodeBehindReferenceLinkerService _codeBehindLinkerService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _codeBehindLinkerService = new CodeBehindReferenceLinkerService();
            _viewImportService = new ViewImportService();

            var configParser = new TagConfigParser(Rules.Config.Constants.TagConfigsExtractedPath);
            _tagConfigParser = configParser;
        }

        private protected async Task<string> GetConverterOutput(string inputText)
        {
            var doc = new HtmlDocument();
            doc.OptionOutputOriginalCase = true;
            doc.LoadHtml(inputText);

            var node = doc.DocumentNode.ChildNodes[0];

            var converter = _tagConfigParser.GetConfigForNode(node.Name);
            converter.Initialize(new TaskManagerService(), _codeBehindLinkerService, _viewImportService);
            await converter.MigrateTagAsync(node, "TestPath", null, 0);

            Utilities.NormalizeHtmlContent(doc.DocumentNode);

            return doc.DocumentNode.WriteTo();
        }
    }
}
