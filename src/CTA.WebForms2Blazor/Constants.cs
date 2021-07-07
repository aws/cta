namespace CTA.WebForms2Blazor
{
    internal class Constants
    {
        // Notable Events
        public const WebFormsAppLifecycleEvent FirstPostHandleEvent = WebFormsAppLifecycleEvent.PostRequestHandlerExecute;
        public const WebFormsPageLifecycleEvent FirstOnInitializedEvent = WebFormsPageLifecycleEvent.InitComplete;
        public const WebFormsPageLifecycleEvent FirstOnParametersSetEvent = WebFormsPageLifecycleEvent.SaveStateComplete;
        public const WebFormsPageLifecycleEvent FirstOnAfterRenderEvent = WebFormsPageLifecycleEvent.Render;
        public const WebFormsPageLifecycleEvent FirstDisposeEvent = WebFormsPageLifecycleEvent.Unload;

        // Notable Methods
        public const string ProcessRequestMethodName = "ProcessRequest";

        // Notable Base Classes
        public const string ExpectedGlobalBaseClass = "HttpApplication";
        public const string ExpectedPageBaseClass = "Page";
        public const string ExpectedControlBaseClass = "UserControl";
        public const string ExpectedMasterPageBaseClass = "MasterPage";
        public const string ComponentBaseClass = "ComponentBase";

        // Notable Interfaces
        public const string HttpHandlerInterface = "IHttpHandler";
        public const string HttpModuleInterface = "IHttpModule";
        public const string DisposableInterface = "IDisposable";

        // Notable Namespaces
        public const string GlobalNamespace = "<global namespace>";
        public const string BlazorComponentsNamespace = "Microsoft.AspNetCore.Components";

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
        public const string DefaultCommentStartToken = "// ";
        public const int DefaultCommentLineCharacterLimit = 40;
        public const int SpacesPerCommentTab = 4;

        // Event Handling Constants
        public const string SenderParamTypeName = "object";
        public const string SenderParamTypeNameAlternate = "Object";
        public const string EventArgsParamTypeName = "EventArgs";
        public const string HttpContextParamTypeName = "HttpContext";

        // Common Comments and Templates
        public const string CodeOriginCommentTemplate = "The following lines were extracted from {0}";
        public const string OperationFailedCommentTemplate = "Attempted to {0} but failed, this must be done manually";
        public const string OperationUnattemptedCommentTemplate = "Did not attempt to {0}, this must be done manually";
        public const string ClassSplitCommentTemplate = "This class was generated using a portion of {0}, modifications may be necessary";
        public const string NewEventRepresentationCommentTemplate = "This code replaces the original handling of the {0} event";
        public const string IdentificationFailureCommentTemplate = "Could not identify {0}, dependant {1} operation must be done manually";

        public const string UnusableCodeComment = "Unable to migrate the following code, as a result it was removed";
    }
}
