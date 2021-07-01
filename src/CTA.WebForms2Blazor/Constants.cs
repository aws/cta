using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.WebForms2Blazor
{
    internal class Constants
    {
        // Notable Events
        public const WebFormsAppLifecycleEvent FirstPostHandleEvent = WebFormsAppLifecycleEvent.PostRequestHandlerExecute;

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
        public const string RazorCodeBehindFileExtension = ".razor.cs";
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

        // Comment Formatting Constants
        public const string TabAsSpaces = "    ";
        public const string DefaultCommentStartToken = "// ";
        public const int DefaultCommentLineCharacterLimit = 40;

        // Event Handling Constants
        public const string SenderParamTypeName = "object";
        public const string SenderParamTypeNameAlternate = "Object";
        public const string EventArgsParamTypeName = "EventArgs";

        // Common Comments and Templates
        public const string CodeOriginCommentTemplate = "The following lines were extracted from {0}";
        public const string OperationFailedCommentTemplate = "Attempted to {0} but failed, this must be done manually";
        public const string OperationUnattemptedCommentTemplate = "Did not attempt to {0}, this must be done manually";
        public const string ClassSplitCommentTemplate = "This class was generated using a portion of {0}, modifications may be necessary";
        public const string NewEventRepresentationCommentTemplate = "This code replaces the original handling of the {0} event";

        public const string UnusableCodeComment = "Unable to migrate the following code, as a result it was removed";
    }
}
