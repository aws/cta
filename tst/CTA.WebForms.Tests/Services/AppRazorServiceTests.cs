using CTA.WebForms.Services;
using NUnit.Framework;
using System.Text;

namespace CTA.WebForms.Tests.Services
{
    public class AppRazorServiceTests
    {
        private const string TestLayout = "SiteMaster";
        private const string ExpectedPath = "App.razor";
        private const string ExpectedContent =
@"<Router AppAssembly=""typeof(Program).Assembly"">
    <Found Context=""routeData"">
        <RouteView RouteData=""routeData"" DefaultLayout=""typeof(SiteMaster)""/>
    </Found>
    <NotFound>
        <h1>Page not found</h1>
        <p>Sorry, but there's nothing here!</p>
    </NotFound>
</Router>";

        private AppRazorService _appRazorService;

        [SetUp]
        public void SetUp()
        {
            _appRazorService = new AppRazorService();
            _appRazorService.DefaultLayout = TestLayout;
        }

        [Test]
        public void ConstructAppRazorFile_Properly_Creates_File_Contents()
        {
            var fileBytes = _appRazorService.ConstructAppRazorFile().FileBytes;
            var actualContent = Encoding.UTF8.GetString(fileBytes);

            Assert.AreEqual(ExpectedContent, actualContent);
        }

        [Test]
        public void ConstructAppRazorFile_Writes_To_Correct_Path()
        {
            var actualPath = _appRazorService.ConstructAppRazorFile().RelativePath;

            Assert.AreEqual(ExpectedPath, actualPath);
        }
    }
}
