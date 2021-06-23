namespace CTA.Rules.Models
{
    public enum ActionTypes
    {
        Method,
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
        ProjectFile
    }
    public enum ProjectType
    {
        WebApi,
        Mvc,
        WebClassLibrary,
        ClassLibrary,
        CoreWebApi,
        CoreMvc,
        WebForms
    }
    public enum FileTypeCreation
    {
        Startup,
        Program
    }
}
