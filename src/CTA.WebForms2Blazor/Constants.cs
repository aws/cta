namespace CTA.WebForms2Blazor
{
    internal class Constants
    {
        // Notable Events
        public const WebFormsAppLifecycleEvent FirstPostHandleEvent = WebFormsAppLifecycleEvent.PostRequestHandlerExecute;
        public const WebFormsPageLifecycleEvent FirstOnInitializedEvent = WebFormsPageLifecycleEvent.InitComplete;
        public const WebFormsPageLifecycleEvent FirstOnParametersSetEvent = WebFormsPageLifecycleEvent.PreRender;
        public const WebFormsPageLifecycleEvent FirstOnAfterRenderEvent = WebFormsPageLifecycleEvent.SaveStateComplete;
        public const WebFormsPageLifecycleEvent FirstDisposeEvent = WebFormsPageLifecycleEvent.Unload;

        // Notable Methods
        public const string ProcessRequestMethodName = "ProcessRequest";
        public const string InitMethodName = "Init";

        // Notable Base Classes
        public const string ExpectedGlobalBaseClass = "HttpApplication";
        public const string ExpectedPageBaseClass = "Page";
        public const string ExpectedControlBaseClass = "UserControl";
        public const string ExpectedMasterPageBaseClass = "MasterPage";
        public const string ComponentBaseClass = "ComponentBase";
        public const string LayoutComponentBaseClass = "LayoutComponentBase";

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
        public const string StyleSheetFileExtension = ".css";

        // Notable File Names
        public const string ExpectedGlobalFileName = "Global.asax.cs";
        public const string StartupFileName = "Startup.cs";
        public const string AppSettingsFileName = "appsettings.json";

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

        // View Text Templates
        public const string MarkupCommentTemplate = "<!-- {0} -->";
        public const string RazorServerSideCommentTemplate = "@* {0} *@";
        public const string RazorCodeBlockEmbeddingTemplate = "@{{ {0} }}";
        public const string RazorExplicitEmbeddingTemplate = "@({0})";
        public const string RazorExplicitRawEmbeddingTemplate = "@(new MarkupString({0}))";
        public const string RazorConfigurationAccessTemplate = "@(Configuration[\"{0}\"])";
        public const string RazorNamespaceDirective = "@namespace {0}";
        public const string RazorImplementsDirective = "@implements {0}";
        public const string RazorInheritsDirective = "@inherits {0}";
        public const string RazorPageDirective = "@page \"{0}\"";

        // appsettings.json Section Names
        public const string AppSettingsSection = "appsettings";
        public const string ConnectionStringsSection = "ConnectionStrings";

        // Common Comments and Templates
        public const string CodeOriginCommentTemplate = "The following lines were extracted from {0}";
        public const string OperationFailedCommentTemplate = "Attempted to {0} but failed, this must be done manually";
        public const string OperationUnattemptedCommentTemplate = "Did not attempt to {0}, this must be done manually";
        public const string ClassSplitCommentTemplate = "This class was generated using a portion of {0}";
        public const string NewEventRepresentationCommentTemplate = "This code replaces the original handling of the {0} event";
        public const string IdentificationFailureCommentTemplate = "Could not identify {0}, dependant {1} operation must be done manually";

        public const string UnusableCodeComment = "Unable to migrate the following code, as a result it was removed";
        public const string HeavyModificationNecessaryComment = "Heavy modifications likely necessary, please review";

        // Common Errors and Templates
        public const string TooManyOperationsError = "Attempted {0} operation, but the expected number of operations has already been reached";
        public const string InvalidStateError = "Attempted {0} operation, but the {1} state was invalid";

        // Logging Templates
        public const string GenericInformationLogTemplate = "{0}: {1}";
        public const string RegisteredAsTaskLogTemplate = "{0}: Registered {1} at {2} as Task {3}";
        public const string CaneledServiceCallLogTemplate = "{0}: {1} {2} Call Canceled";
        public const string OperationFailedLogTemplate = "{0}: {1} Operation Failed";
        public const string StartedLogTemplate = "{0}: Started {1}";
        public const string StartedAtLogTemplate = "{0}: Started {1} at {2}";
        public const string StartedForLogTemplate = "{0}: Started {1} for {2}";
        public const string StartedForAtLogTemplate = "{0}: Started {1} for {2} at {3}";
        public const string StartedFromToLogTemplate = "{0}: Started {1} from {2} to {3}";
        public const string EndedLogTemplate = "{0}: Ended {1}";
        public const string EndedAtLogTemplate = "{0}: Ended {1} at {2}";
        public const string EndedForLogTemplate = "{0}: Ended {1} for {2}";
        public const string EndedForAtLogTemplate = "{0}: Ended {1} for {2} at {3}";
        public const string EndedFromToLogTemplate = "{0}: Ended {1} from {2} to {3}";

        // Logging Actions
        public const string ProjectMigrationLogAction = "Project Migration";
        public const string FileMigrationLogAction = "File Migration";
        public const string ClassMigrationLogAction = "Class Migration";
        
        //View layer constants
        public const string AspControlTag = "asp:";

        // Routing constants
        public const string DefaultHomePagePath = "Default.aspx";
        public const char RouteSeparator = '/';
    }
}
