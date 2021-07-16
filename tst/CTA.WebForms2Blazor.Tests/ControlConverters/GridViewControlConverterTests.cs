using CTA.WebForms2Blazor.ControlConverters;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests.ControlConverters
{
    public class GridViewControlConverterTests
    {
        [Test]
        public void GridViewControlConverter_Returns_GridView_Node()
        {
            var htmlString = @"<asp:GridView ID=""GridView1"" runat=""server"" AutoGenerateColumns=""false"" ItemType=""People"" SelectMethod=""People.GetPeople"">
    <Columns>
        <asp:BoundField  DataField=""Name"" HeaderText=""First Name"" ItemType=""Random""/>
        <asp:BoundField  DataField=""LastName"" HeaderText=""Last Name"" />
        <asp:BoundField  DataField=""Position"" HeaderText=""Person Type"" />
    </Columns>
</asp:GridView>";
            var htmlNode = HtmlNode.CreateNode(htmlString, ControlConverter.PreserveCapitalization);
            var testGridViewControlConverter = new GridViewControlConverter();
            var convertedNode = testGridViewControlConverter.Convert2Blazor(htmlNode);

            var actualString = convertedNode.WriteTo();
            var expectedString = @"<GridView @ref=""GridView1"" AutoGenerateColumns=""false"" ItemType=""People"" SelectMethod=""People.GetPeople"">
    <Columns>
        <BoundField DataField=""Name"" HeaderText=""First Name"" ItemType=""Random""></BoundField>
        <BoundField DataField=""LastName"" HeaderText=""Last Name"" ItemType=""People""></BoundField>
        <BoundField DataField=""Position"" HeaderText=""Person Type"" ItemType=""People""></BoundField>
    </Columns>
</GridView>";
            
            Assert.AreEqual(expectedString, actualString);
        }
    }
}