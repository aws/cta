using CTA.WebForms2Blazor.ControlConverters;
using CTA.WebForms2Blazor.Helpers;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests.ControlConverters
{
    public class RadioButtonConverterTests
    {
        private RadioButtonControlConverter _testConverter;

        [SetUp]
        public void SetUp()
        {
            _testConverter = new RadioButtonControlConverter();
        }

        [Test]
        public void Convert2Blazor_Returns_Only_Radio_If_No_Text_Present()
        {
            var htmlString = @"<asp:RadioButton ID=""TestId"" GroupName=""TestGroup"">";
            var expectedString = @"<input id=""TestId"" name=""TestGroup"" type=""radio"">";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void Convert2Blazor_Returns_Radio_With_Label_If_Text_Present()
        {
            var htmlString = @"<asp:RadioButton ID=""TestId"" GroupName=""TestGroup"" Text=""Test Text 000"">";
            var expectedString =@"<div><input id=""TestId"" name=""TestGroup"" type=""radio""><label for=""TestId"">Test Text 000</label></div>";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void Convert2Blazor_Adds_Generated_Id_If_Text_Present_But_No_Id()
        {
            var genId = $"GeneratedId{IncrementalViewIdGenerator.NextGeneratedIdNumber}";

            var htmlString = @"<asp:RadioButton GroupName=""TestGroup"" Text=""Test Text 000"">";
            var expectedString = @$"<div><input name=""TestGroup"" type=""radio"" id=""{genId}""><label for=""{genId}"">Test Text 000</label></div>";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void Convert2Blazor_Sets_Checked_Attribute_Properly()
        {
            var htmlString = @"<asp:RadioButton ID=""TestId"" GroupName=""TestGroup"" Checked=""True"">";
            var expectedString = @"<input id=""TestId"" name=""TestGroup"" type=""radio"" checked="""">";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void Convert2Blazor_Sets_Disabled_Attribute_Properly()
        {
            var htmlString = @"<asp:RadioButton ID=""TestId"" GroupName=""TestGroup"" Enabled=""False"">";
            var expectedString = @"<input id=""TestId"" name=""TestGroup"" type=""radio"" disabled="""">";

            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var convertedNode = _testConverter.Convert2Blazor(htmlNode);
            var actualString = convertedNode.WriteTo();

            Assert.AreEqual(expectedString, actualString);
        }
    }
}
