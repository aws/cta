using NUnit.Framework;
using System.Threading.Tasks;

namespace CTA.WebForms.Tests.TagConfigs
{
    public class HtmlElementTests : TagConfigsTestFixture
    {
        [Test]
        public async Task Body_Is_Properly_Removed()
        {
            var inputText =
@"<body>
    <h1>Section</h1>
    <div>
        <h2>Sub-Section</h2>
        <p>Content, content, content, content...</p>
    </div>
</body>";
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
        public async Task Head_Is_Properly_Removed()
        {
            var inputText =
@"<head>
    <title>My Web Page Title</title>
    <link rel=""stylesheet"" href=""mystyle.css"">
</head>";
            var expectedOutput =
@"@*
<head>
    <title>
        My Web Page Title
    </title>
    <link rel=""stylesheet"" href=""mystyle.css"">
</head>
*@";

            expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");
            var output = (await GetConverterOutput(inputText)).Trim().Replace("\r\n", "\n");

            Assert.AreEqual(expectedOutput, output);
        }

        [Test]
        public async Task Html_Is_Properly_Removed()
        {
            var inputText =
@"<html>
    <h1>Section</h1>
    <div>
        <h2>Sub-Section</h2>
        <p>Content, content, content, content...</p>
    </div>
</html>";
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
    }
}
