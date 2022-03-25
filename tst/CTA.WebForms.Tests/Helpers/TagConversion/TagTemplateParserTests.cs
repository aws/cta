using CTA.WebForms.Helpers.TagConversion;
using CTA.WebForms.Services;
using HtmlAgilityPack;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CTA.WebForms.Tests.Helpers.TagConversion
{
    [TestFixture]
    public class TagTemplateParserTests
    {
        private TagTemplateParser _templateParser;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _templateParser = new TagTemplateParser(
                new TaskManagerService(),
                new CodeBehindReferenceLinkerService());
        }

        [Test]
        public void AttributeReplacementRegex_Matches_Minimal_Placeholder()
        {
            var input = "Attribute0=#SourceAttr0#";

            var match = TagTemplateParser.AttributeReplacementRegex.Match(input);

            Assert.True(match.Success);
            Assert.AreEqual("Attribute0", match.Groups[TagTemplateParser.TargetAttributeGroup].Value);
            Assert.AreEqual("SourceAttr0", match.Groups[TagTemplateParser.SourceAttributeGroup].Value);
        }

        [Test]
        public void AttributeReplacementRegex_Matches_Placeholder_With_Target_Type()
        {
            var input = "Attribute0=#SourceAttr0:TargetType0#";

            var match = TagTemplateParser.AttributeReplacementRegex.Match(input);

            Assert.True(match.Success);
            Assert.AreEqual("Attribute0", match.Groups[TagTemplateParser.TargetAttributeGroup].Value);
            Assert.AreEqual("SourceAttr0", match.Groups[TagTemplateParser.SourceAttributeGroup].Value);
            Assert.AreEqual("TargetType0", match.Groups[TagTemplateParser.Context0Group].Value);
        }

        [Test]
        public void AttributeReplacementRegex_Matches_Placeholder_With_Target_Type_And_Code_Behind_Name()
        {
            var input = "Attribute0=#SourceAttr0:CodeBehindName0:TargetType0#";

            var match = TagTemplateParser.AttributeReplacementRegex.Match(input);

            Assert.True(match.Success);
            Assert.AreEqual("Attribute0", match.Groups[TagTemplateParser.TargetAttributeGroup].Value);
            Assert.AreEqual("SourceAttr0", match.Groups[TagTemplateParser.SourceAttributeGroup].Value);
            Assert.AreEqual("CodeBehindName0", match.Groups[TagTemplateParser.Context0Group].Value);
            Assert.AreEqual("TargetType0", match.Groups[TagTemplateParser.Context1Group].Value);
        }

        [TestCase(" Attribute0 = # SourceAttr0 : CodeBehindName0 : TargetType0 # ")]
        [TestCase("    Attribute0  =   # SourceAttr0    : CodeBehindName0  :  TargetType0    #  ")]
        [TestCase("  \t Attribute0  =\t  # SourceAttr0\t\t\t   : CodeBehindName0  :  TargetType0   \t# \t")]
        public void AttributeReplacementRegex_Matches_Despite_Extraneous_Space_Characters(string input)
        {
            var match = TagTemplateParser.AttributeReplacementRegex.Match(input);

            Assert.True(match.Success);
            Assert.AreEqual("Attribute0", match.Groups[TagTemplateParser.TargetAttributeGroup].Value);
            Assert.AreEqual("SourceAttr0", match.Groups[TagTemplateParser.SourceAttributeGroup].Value);
            Assert.AreEqual("CodeBehindName0", match.Groups[TagTemplateParser.Context0Group].Value);
            Assert.AreEqual("TargetType0", match.Groups[TagTemplateParser.Context1Group].Value);
        }

        [TestCase("#SourceAttr0#")]
        [TestCase("#SourceAttr0:TargetType0#")]
        [TestCase("#SourceAttr0:CodeBehindName0:TargetType0#")]
        public void AttributeReplacementRgex_Does_Not_Match_Basic_Placeholder(string input)
        {
            var match = TagTemplateParser.AttributeReplacementRegex.Match(input);

            Assert.False(match.Success);
        }

        [TestCase("Attribute0=#Sou rceAttr0:CodeBehindName0:Targe tType0#")]
        [TestCase("Attribute0=#Sou rceAttr0:CodeBehindName0:TargetType0#")]
        [TestCase("Attribute0=#Sou rceAttr0:CodeBeh\tindName0:TargetType0#")]
        [TestCase("Attribute0=SourceAttr0:CodeBehindName0:TargetType0")]
        [TestCase("Attribute0=#SourceAttr0:CodeBehindName0:TargetType0")]
        [TestCase("Attribute0=SourceAttr0:CodeBehindName0:TargetType0#")]
        public void AttributeReplacementRegex_Does_Not_Match_Malformed_Placeholder(string input)
        {
            var match = TagTemplateParser.AttributeReplacementRegex.Match(input);

            Assert.False(match.Success);
        }

        [Test]
        public void BasicReplacementRegex_Matches_Minimal_Placeholder()
        {
            var input = "#SourceAttr0#";

            var match = TagTemplateParser.BasicReplacementRegex.Match(input);

            Assert.True(match.Success);
            Assert.AreEqual("SourceAttr0", match.Groups[TagTemplateParser.SourceAttributeGroup].Value);
        }

        [Test]
        public void BasicReplacementRegex_Matches_Placeholder_With_Target_Type()
        {
            var input = "#SourceAttr0:TargetType0#";

            var match = TagTemplateParser.BasicReplacementRegex.Match(input);

            Assert.True(match.Success);
            Assert.AreEqual("SourceAttr0", match.Groups[TagTemplateParser.SourceAttributeGroup].Value);
            Assert.AreEqual("TargetType0", match.Groups[TagTemplateParser.Context0Group].Value);
        }

        [Test]
        public void BasicReplacementRegex_Matches_Placeholder_With_Target_Type_And_Code_Behind_Name()
        {
            var input = "#SourceAttr0:CodeBehindName0:TargetType0#";

            var match = TagTemplateParser.BasicReplacementRegex.Match(input);

            Assert.True(match.Success);
            Assert.AreEqual("SourceAttr0", match.Groups[TagTemplateParser.SourceAttributeGroup].Value);
            Assert.AreEqual("CodeBehindName0", match.Groups[TagTemplateParser.Context0Group].Value);
            Assert.AreEqual("TargetType0", match.Groups[TagTemplateParser.Context1Group].Value);
        }

        [TestCase(" # SourceAttr0 : CodeBehindName0 : TargetType0 # ")]
        [TestCase("    # SourceAttr0    : CodeBehindName0  :  TargetType0    #  ")]
        [TestCase(" \t  # SourceAttr0\t\t\t   : CodeBehindName0  :  TargetType0   \t# \t")]
        public void BasicReplacementRegex_Matches_Despite_Extraneous_Space_Characters(string input)
        {
            var match = TagTemplateParser.BasicReplacementRegex.Match(input);

            Assert.True(match.Success);
            Assert.AreEqual("SourceAttr0", match.Groups[TagTemplateParser.SourceAttributeGroup].Value);
            Assert.AreEqual("CodeBehindName0", match.Groups[TagTemplateParser.Context0Group].Value);
            Assert.AreEqual("TargetType0", match.Groups[TagTemplateParser.Context1Group].Value);
        }

        [TestCase("#Sou rceAttr0:CodeBehindName0:Targe tType0#")]
        [TestCase("#Sou rceAttr0:CodeBehindName0:TargetType0#")]
        [TestCase("#Sou rceAttr0:CodeBeh\tindName0:TargetType0#")]
        [TestCase("SourceAttr0:CodeBehindName0:TargetType0")]
        [TestCase("#SourceAttr0:CodeBehindName0:TargetType0")]
        [TestCase("SourceAttr0:CodeBehindName0:TargetType0#")]
        public void BasicReplacementRegex_Does_Not_Match_Malformed_Placeholder(string input)
        {
            var match = TagTemplateParser.BasicReplacementRegex.Match(input);

            Assert.False(match.Success);
        }

        [Test]
        public async Task ParseTemplate_Correctly_Handles_Template_With_Only_Attribute_Replacements()
        {
            var node = HtmlNode.CreateNode("<p stringAttr=\"value0\" booleanAttr>Content a, b, c</p>");
            var template =
@"<div newStringAttr=#stringAttr:String# newBooleanAttr=#booleanAttr:HtmlBoolean#>
</div>";

            var expectedResult =
@"<div newStringAttr=""value0"" newBooleanAttr>
</div>";

            Assert.AreEqual(expectedResult, await _templateParser.ParseTemplateAsync(template, node, "TestPath", null, 0));
        }

        [Test]
        public async Task ParseTemplate_Correctly_Handles_Template_With_Only_Basic_Replacements()
        {
            var node = HtmlNode.CreateNode("<p stringAttr=\"value0\" booleanAttr>true</p>");
            var template =
@"#stringAttr#
#booleanAttr#
#InnerHtml:ComponentBoolean#";

            var expectedResult =
@"value0

True"; ;

            Assert.AreEqual(expectedResult, await _templateParser.ParseTemplateAsync(template, node, "TestPath", null, 0));
        }

        [Test]
        public async Task ParseTemplate_Correctly_Handles_Template_With_Mixed_Replacements()
        {
            var node = HtmlNode.CreateNode("<p stringAttr=\"value0\" booleanAttr>Content a, b, c</p>");
            var template =
@"<div newStringAttr=#stringAttr:String# newBooleanAttr=#booleanAttr:HtmlBoolean#>
    <p><b>InnerHtml:</b> #InnerHtml#</p>
</div>";

            var expectedResult =
@"<div newStringAttr=""value0"" newBooleanAttr>
    <p><b>InnerHtml:</b> Content a, b, c</p>
</div>";

            Assert.AreEqual(expectedResult, await _templateParser.ParseTemplateAsync(template, node, "TestPath", null, 0));
        }
    }
}
