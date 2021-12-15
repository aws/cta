using System.Text;
using CTA.WebForms.FileInformationModel;

namespace CTA.WebForms.Services
{
    public class AppRazorService
    {
        private const string FileName = "App.razor";
        private const string AppRazorTemplate =
@"<Router AppAssembly=""typeof(Program).Assembly"">
    <Found Context=""routeData"">
        <RouteView RouteData=""routeData"" {0}/>
    </Found>
    <NotFound>
        <h1>Page not found</h1>
        <p>Sorry, but there's nothing here!</p>
    </NotFound>
</Router>";
        private const string DefaultLayoutAttrTemplate = "DefaultLayout=\"typeof({0})\"";

        public string DefaultLayout { get; set; }

        public FileInformation ConstructAppRazorFile()
        {
            // TODO: Allow modification of <NotFound> content
            var attrs = string.IsNullOrEmpty(DefaultLayout) ? string.Empty : string.Format(DefaultLayoutAttrTemplate, DefaultLayout);
            var fileContents = string.Format(AppRazorTemplate, attrs);
            var fileBytes = Encoding.UTF8.GetBytes(fileContents);

            return new FileInformation(FileName, fileBytes);
        }
    }
}
