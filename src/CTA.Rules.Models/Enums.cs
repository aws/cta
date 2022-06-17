namespace CTA.Rules.Models
{
    public enum ActionTypes
    {
        Method,
        Expression,
        Using,
        Class,
        Interface,
        Identifier,
        Attribute,
        AttributeList,
        Package,
        Namespace,
        ObjectCreation,
        MethodDeclaration,
        ElementAccess,
        MemberAccess,
        Project,
        ProjectFile,
        ProjectType
    }
    public enum ProjectType
    {
        WebApi,
        Mvc,
        WebClassLibrary,
        ClassLibrary,
        CoreWebApi,
        CoreMvc,
        WCFConfigBasedService,
        WCFCodeBasedService,
        WCFClient,
        WCFServiceLibrary,
        MonolithService,
        WebForms,
        VBNetMvc,
        VBWebForms,
        VBWebApi,
        VBClassLibrary
    }
    public enum FileTypeCreation
    {
        Startup,
        Program,
        MonolithService,
        MonolithServiceMvc,
        MonolithServiceCore
    }
    public enum WebServerConfigAttributes
    {
        Authorization,
        Authentication,
        Modules,
        Handlers,
        Security,
        HttpCompression,
        HttpRedirect,
        AnonymousAuthentication,
        WindowsAuthentication,
        FormsAuthentication,
        BasicAuthentication,
        DigestAuthentication,
        RequestFiltering,
        RequestLimits
    }

    public enum WebFormsActionType
    {
        DirectiveConversion,
        ControlConversion,
        ClassConversion,
        FileConversion
    }
}
