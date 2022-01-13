using CTA.WebForms.ControlConverters;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms.Tests.ControlConverters
{
    public class TextBoxControlConverterTests
    {
        private TextBoxControlConverter _testConverter;

        [SetUp]
        public void SetUp()
        {
            _testConverter = new TextBoxControlConverter();
        }

        [Test]
        public void Convert2Blazor_Returns_Standard_Input_For_SingleLine_Text_Mode()
        {
            var htmlString = @"<asp:TextBox ID=""TestId"" Text=""Example Text"">";
            var expectedString = @"<input id=""TestId"" value=""Example Text"">";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void Convert2Blazor_Returns_TextArea_Input_For_MultiLine_Text_Mode()
        {
            var htmlString = @"<asp:TextBox ID=""TestId"" TextMode=""MultiLine"" Text=""Example Text"">";
            var expectedString = @"<textarea id=""TestId"">Example Text</textarea>";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void Convert2Blazor_Sets_Password_Type_For_Password_Text_Mode()
        {
            var htmlString = @"<asp:TextBox ID=""TestId"" TextMode=""Password"">";
            var expectedString = @"<input id=""TestId"" type=""password"">";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void Convert2Blazor_Sets_ReadOnly_Attribute_Properly()
        {
            var htmlString = @"<asp:TextBox ID=""TestId"" Text=""Example Text"" ReadOnly=""true"">";
            var expectedString = @"<input id=""TestId"" value=""Example Text"" readonly="""">";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void Convert2Blazor_Sets_Disabled_Attribute_Properly()
        {
            var htmlString = @"<asp:TextBox ID=""TestId"" Text=""Example Text"" Enabled=""false"">";
            var expectedString = @"<input id=""TestId"" value=""Example Text"" disabled="""">";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }
    }
}
