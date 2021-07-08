namespace CTA.Rules.Models
{
    public enum ActionTypes
    {
        Method,
        Statement,
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
        CoreMvc
    }
    public enum FileTypeCreation
    {
        Startup,
        Program
    }
}
