using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.WebForms2Blazor
{
    internal class Constants
    {
        // Notable Base Classes
        public const string ExpectedGlobalBaseClass = "HttpApplication";
        public const string ExpectedPageBaseClass = "Page";
        public const string ExpectedControlBaseClass = "UserControl";
        public const string ExpectedMasterPageBaseClass = "MasterPage";

        // Notable Interfaces
        public const string HttpHandlerInterface = "IHttpHandler";
        public const string HttpModuleInterface = "IHttpModule";

        // Notable Extensions
        public const string PageCodeBehindExtension = ".aspx.cs";
        public const string ControlCodeBehindExtension = ".ascx.cs";
        public const string MasterPageCodeBehindExtension = ".Master.cs";
        public const string RazorFileExtension = ".razor";
        public const string CSharpCodeFileExtension = ".cs";
        public const string CSharpProjectFileExtension = ".csproj";
        public const string SolutionFileExtension = ".sln";
        public const string WebFormsPageMarkupFileExtension = ".aspx";
        public const string WebFormsControlMarkupFileExtenion = ".ascx";
        public const string WebFormsMasterPageMarkupFileExtension = ".Master";
        public const string WebFormsGlobalMarkupFileExtension = ".asax";
        public const string WebFormsConfigFileExtension = ".config";

        // Notable File Names
        public const string ExpectedGlobalFileName = "Global.asax.cs";
        public const string StartupFileName = "Startup.cs";

        // Notable Directory Names
        public const string WebRootDirectoryName = "wwwroot";
        public const string RazorPageDirectoryName = "Pages";
        public const string RazorComponentDirectoryName = "Components";
        public const string RazorLayoutDirectoryName = "Layouts";
        public const string MiddlewareDirectoryName = "Middleware";
    }
}
