using CTA.WebForms.Helpers;
using CTA.WebForms.Helpers.TagConversion;
using CTA.WebForms.Services;
using CTA.WebForms.TagConverters;
using HtmlAgilityPack;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CTA.WebForms.Tests.TagConfigs
{
    [TestFixture]
    public class AspControlTests : TagConfigsTestFixture
    {
        [Test]
        public async Task AspBoundField_Is_Properly_Converted()
        {
            var inputText =
@"<asp:BoundField DataField=""Field"" DataFormatString=""Data: {0}"" HeaderText=""This is a header"" Visible=""True"">
    Some text...
</asp:BoundField>";
            var expectedOutput =
@"<BoundField DataField=""Field"" DataFormatString=""Data: {0}"" HeaderText=""This is a header"" Visible=""True"">
    Some text...
</BoundField>";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
            Assert.True(_viewImportService.ViewUsingDirectives.Contains("@using BlazorWebFormsComponents"));
        }

        [Test]
        public async Task AspButton_Is_Properly_Converted()
        {
            var inputText =
@"<asp:Button ID=""Identifier0"" CssClass=""Class0"" OnClick=""ClickHandler0"" Text=""Some text..."" Enabled=""False""/>";
            var expectedOutput =
@"<button id=""Identifier0"" class=""Class0"" @onclick=""(args) => ClickHandler0(null, args)"" disabled="""">
    Some text...
</button>";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public async Task AspButtonField_Is_Properly_Converted()
        {
            var inputText =
@"<asp:ButtonField
    ButtonType=""Button""
    CommandName=""Command0""
    DataTextField=""ButtonInputData""
    DataTextFormatString=""Data: {0}""
    ImageUrl=""https://wwww.google.com/images/something_or_other.jpg""
    HeaderText=""This is a header""
    Visible=""True"">
    Some text...
</asp:ButtonField>";
            var expectedOutput =
@"<ButtonField ButtonType=""Button"" CommandName=""Command0"" DataTextField=""ButtonInputData"" DataTextFormatString=""Data: {0}"" ImageUrl=""https://wwww.google.com/images/something_or_other.jpg"" HeaderText=""This is a header"" Visible=""True"">
    Some text...
</ButtonField>";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
            Assert.True(_viewImportService.ViewUsingDirectives.Contains("@using BlazorWebFormsComponents"));
        }

        [Test]
        public async Task AspContent_Is_Properly_Removed()
        {
            var inputText =
@"<asp:Content>
    <h1>Section</h1>
    <div>
        <h2>Sub-Section</h2>
        <p>Content, content, content, content...</p>
    </div>
</asp:Content>";
            var expectedOutput =
@"<h1>
    Section
</h1>
<div>
    <h2>
        Sub-Section
    </h2>
    <p>
        Content, content, content, content...
    </p>
</div>";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public async Task AspContentPlaceHolder_Is_Properly_Converted()
        {
            var inputText = "<asp:ContentPlaceHolder></asp:ContentPlaceHolder>";
            var expectedOutput = "@Body";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public async Task AspGridView_Is_Properly_Converted()
        {
            var inputText =
@"<asp:GridView
    ID=""Identifier0""
    AutoGenerateColumns=""True""
    CssClass=""Class0""
    DataKeyNames=""DataKey0,DataKey1""
    DataSource=""DataSourceFragment""
    EmptyDataText=""No data to show.""
    Enabled=""False""
    Items=""ItemsEnumberable0""
    ItemType=""TypeName""
    OnDataBinding=""DataBindingEventHandler""
    OnDataBound=""DataBoundEventHandler""
    OnRowDataBound=""OnRowDataBoundEventHandler""
    OnInit=""OnInitEventHandler""
    OnLoad=""OnLoadEventHandler""
    OnPreRender=""OnPreRenderEventHandler""
    OnUnload=""OnUnloadEventHandler""
    OnDisposed=""OnDisposedEventHandler""
    TabIndex=""0""
    Visible=""True"">
    <Columns>
        ...Column stuff...
    </Columns>
</asp:GridView>";
            var expectedOutput =
@"<GridView ID=""Identifier0"" AutoGenerateColumns=""True"" CssClass=""Class0"" DataKeyNames=""DataKey0,DataKey1"" DataSource=""DataSourceFragment"" EmptyDataText=""No data to show."" Enabled=""False"" Items=""ItemsEnumberable0"" ItemType=""TypeName"" OnDataBinding=""(args) => DataBindingEventHandler(null, args)"" OnDataBound=""(args) => DataBoundEventHandler(null, args)"" OnItemDataBound=""(args) => OnRowDataBoundEventHandler(null, args)"" OnInit=""(args) => OnInitEventHandler(null, args)"" OnLoad=""(args) => OnLoadEventHandler(null, args)"" OnPreRender=""(args) => OnPreRenderEventHandler(null, args)"" OnUnload=""(args) => OnUnloadEventHandler(null, args)"" OnDisposed=""(args) => OnDisposedEventHandler(null, args)"" TabIndex=""0"" Visible=""True"">
    <Columns>
        ...Column stuff...
    </Columns>
</GridView>";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public async Task AspHyperLink_With_InnerHtml_Is_Properly_Converted()
        {
            var inputText =
@"<asp:HyperLink ID=""Identifier0"" CssClass=""Class0"" NavigateUrl=""https://aws.amazon.com"">
    Some text...
</asp:HyperLink>";
            var expectedOutput =
@"<a id=""Identifier0"" class=""Class0"" href=""https://aws.amazon.com"">
    Some text...
</a>";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
        }


        [Test]
        public async Task AspHyperLink_With_Text_Is_Properly_Converted()
        {
            var inputText =
@"<asp:HyperLink ID=""Identifier0"" CssClass=""Class0"" Text=""Some text..."" NavigateUrl=""https://aws.amazon.com""/>";
            var expectedOutput =
@"<a id=""Identifier0"" class=""Class0"" href=""https://aws.amazon.com"">
    Some text...
</a>";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public async Task AspHyperLinkField_Is_Properly_Converted()
        {
            var inputText =
@"<asp:HyperLinkField
    AccessibleHeaderText=""Accessible header text here...""
    DataTextField=""DataText""
    DataTextFormatString=""Data Value: {0}""
    DataNavigateUrlFields=""Field1""
    DataNavigateUrlFormatString=""www.{0}.com""
    HeaderText=""HyperLinkField Header""
    NavigateUrl=""https://aws.amazon.com""
    Text=""Go to the link""
    Target=""_blank""
    Visible=""True"">
    Some text...
</asp:HyperLinkField>";
            var expectedOutput =
@"<HyperLinkField AccessibleHeaderText=""Accessible header text here..."" DataTextField=""DataText"" DataTextFormatString=""Data Value: {0}"" DataNavigateUrlFields=""Field1"" DataNavigateUrlFormatString=""www.{0}.com"" HeaderText=""HyperLinkField Header"" NavigateUrl=""https://aws.amazon.com"" Text=""Go to the link"" Target=""_blank"" Visible=""True"">
    Some text...
</HyperLinkField>";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
            Assert.True(_viewImportService.ViewUsingDirectives.Contains("@using BlazorWebFormsComponents"));
        }

        [Test]
        public async Task AspLabel_With_InnerHtml_Is_Properly_Converted()
        {
            var inputText =
@"<asp:Label ID=""Identifier0"" CssClass=""Class0"">
    Some text...
</asp:Label>";
            var expectedOutput =
@"<label id=""Identifier0"" class=""Class0"">
    Some text...
</label>";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public async Task AspLabel_With_Text_Is_Properly_Converted()
        {
            var inputText =
@"<asp:Label ID=""Identifier0"" CssClass=""Class0"" Text=""Some text...""/>";
            var expectedOutput =
@"<label id=""Identifier0"" class=""Class0"">
    Some text...
</label>";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public async Task AspListView_Is_Properly_Converted()
        {
            var inputText =
@"<asp:ListView
    ID=""Identifier0""
    ItemType=""TestModel""
    AdditionalAttributes=""AttrCollection""
    AlternatingItemTemplate=""Template""
    ChildComponents=""SubComponentX""
    ChildContent=""Content String Here""
    DataKeys=""DataKey1,DataKey2""
    DataMember=""Member0""
    DataSource=""SourceRef""
    DataSourceID=""Identifier""
    EmptyDataTemplate=""Template""
    Enabled=""True""
    EnableTheming=""True""
    EnableViewState=""True""
    GroupItemCount=""3""
    GroupSeparatorTemplate=""Template""
    GroupTemplate=""Template""
    InsertItemPosition=""0""
    ItemPlaceholderID=""ItemPlaceholder""
    Items=""ItemCollection""
    ItemSeparatorTemplate=""Template""
    ItemTemplate=""Template""
    LayoutTemplate=""Template""
    OnDataBinding=""OnDataBindingEventHandler""
    OnDataBound=""OnDataBoundEventHandler""
    OnDisposed=""OnDisposedEventHandler""
    OnInit=""OnInitEventHandler""
    OnItemDataBound=""OnItemDataBoundEventHandler""
    OnLayoutCreated=""OnLayoutCreatedEventHandler""
    OnLoad=""OnLoadEventHandler""
    OnPreRender=""OnPreRenderEventHandler""
    OnUnload=""OnUnloadEventHandler""
    SelectedIndex=""0""
    Style=""StyleAttr0=Value0""
    TabIndex=""23""
    Visible=""True"">
    <LayoutTemplate>
        ...LayoutTemplateContent...
    </LayoutTemplate>
    <ItemTemplate>
        ...ItemTemplateContent...
    </ItemTemplate>
</asp:ListView>";
            var expectedOutput =
@"<ListView ID=""Identifier0"" ItemType=""TestModel"" AdditionalAttributes=""AttrCollection"" AlternatingItemTemplate=""Template"" ChildComponents=""SubComponentX"" ChildContent=""Content String Here"" DataKeys=""DataKey1,DataKey2"" DataMember=""Member0"" DataSource=""SourceRef"" DataSourceID=""Identifier"" EmptyDataTemplate=""Template"" Enabled=""True"" EnableTheming=""True"" EnableViewState=""True"" GroupItemCount=""3"" GroupSeparatorTemplate=""Template"" GroupTemplate=""Template"" InsertItemPosition=""0"" ItemPlaceholderID=""ItemPlaceholder"" Items=""ItemCollection"" ItemSeparatorTemplate=""Template"" ItemTemplate=""Template"" LayoutTemplate=""Template"" OnDataBinding=""(args) => OnDataBindingEventHandler(null, args)"" OnDataBound=""(args) => OnDataBoundEventHandler(null, args)"" OnDisposed=""(args) => OnDisposedEventHandler(null, args)"" OnInit=""(args) => OnInitEventHandler(null, args)"" OnItemDataBound=""(args) => OnItemDataBoundEventHandler(null, args)"" OnLayoutCreated=""(args) => OnLayoutCreatedEventHandler(null, args)"" OnLoad=""(args) => OnLoadEventHandler(null, args)"" OnPreRender=""(args) => OnPreRenderEventHandler(null, args)"" OnUnload=""(args) => OnUnloadEventHandler(null, args)"" SelectedIndex=""0"" Style=""StyleAttr0=Value0"" TabIndex=""23"" Visible=""True"">
    <LayoutTemplate>
        ...LayoutTemplateContent...
    </LayoutTemplate>
    <ItemTemplate>
        ...ItemTemplateContent...
    </ItemTemplate>
</ListView>";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
            Assert.True(_viewImportService.ViewUsingDirectives.Contains("@using BlazorWebFormsComponents"));
        }

        [Test]
        public async Task AspPlaceHolder_Is_Properly_Converted()
        {
            var inputText = @"<asp:PlaceHolder ID=""PlaceHolderId""></asp:PlaceHolder>";
            var expectedOutput = @"@PlaceHolderId";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public async Task AspRadioButton_Is_Properly_Converted_With_Label()
        {
            var inputText =
@"<asp:RadioButton ID=""Identifier0"" CssClass=""Class0"" GroupName=""RadioGroup"" Text=""Some text..."" Checked=""True"" Enabled=""True""/>";
            var expectedOutput =
@"<input id=""Identifier0"" class=""Class0"" type=""radio"" name=""RadioGroup"" checked="""">
<label for=""Identifier0"">
    Some text...
</label>";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public async Task AspRadioButton_Is_Properly_Converted_Without_Label()
        {
            var inputText =
@"<asp:RadioButton ID=""Identifier0"" CssClass=""Class0"" GroupName=""RadioGroup"" Checked=""True"" Enabled=""True""/>";
            var expectedOutput =
@"<input id=""Identifier0"" class=""Class0"" type=""radio"" name=""RadioGroup"" checked="""">";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public async Task AspTemplateField_Is_Properly_Converted()
        {
            var inputText =
@"<asp:TemplateField ItemTemplate=""TemplateName"" HeaderText=""This is a header"" Visible=""True"">
    Some text...
</asp:TemplateField>";
            var expectedOutput =
@"<TemplateField ItemTemplate=""TemplateName"" HeaderText=""This is a header"" Visible=""True"">
    Some text...
</TemplateField>";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
            Assert.True(_viewImportService.ViewUsingDirectives.Contains("@using BlazorWebFormsComponents"));
        }

        [Test]
        public async Task AspTextBox_Is_Properly_Converted_To_TextArea()
        {
            var inputText =
@"<asp:TextBox
    ID=""Identifier0""
    CssClass=""Class0""
    TextMode=""MultiLine""
    Text=""Some text...""
    MaxLength=""100""
    Rows=""10""
    Columns=""15""
    OnTextChanged=""OnTextChangedEventHandler""
    ReadOnly=""True""
    Enabled=""False"">
</asp:TextBox>";
            var expectedOutput =
@"<textarea id=""Identifier0"" class=""Class0"" maxlength=""100"" rows=""10"" cols=""15"" @onchange=""(args) => OnTextChangedEventHandler(null, args)"" readonly="""" disabled="""">
    Some text...
</textarea>";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public async Task AspTextBox_Is_Properly_Converted_To_Password_Input()
        {
            var inputText =
@"<asp:TextBox
    ID=""Identifier0""
    CssClass=""Class0""
    TextMode=""Password""
    Text=""Some text...""
    MaxLength=""100""
    OnTextChanged=""OnTextChangedEventHandler""
    ReadOnly=""True""
    Enabled=""False"">
</asp:TextBox>";
            var expectedOutput =
@"<input id=""Identifier0"" class=""Class0"" value=""Some text..."" maxlength=""100"" type=""password"" @onchange=""(args) => OnTextChangedEventHandler(null, args)"" readonly="""" disabled="""">";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public async Task AspTextBox_Is_Properly_Converted_To_Default_Input()
        {
            var inputText =
@"<asp:TextBox
    ID=""Identifier0""
    CssClass=""Class0""
    Text=""Some text...""
    MaxLength=""100""
    OnTextChanged=""OnTextChangedEventHandler""
    ReadOnly=""True""
    Enabled=""False"">
</asp:TextBox>";
            var expectedOutput =
@"<input id=""Identifier0"" class=""Class0"" value=""Some text..."" maxlength=""100"" type=""text"" @onchange=""(args) => OnTextChangedEventHandler(null, args)"" readonly="""" disabled="""">";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
        }
    }
}
