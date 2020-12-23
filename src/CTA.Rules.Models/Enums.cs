using System.ComponentModel;

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
        Project,
        ProjectFile
    }
    public enum ProjectType
    {
        WebApi,
        Mvc,
        WebClassLibrary,
        ClassLibrary
    }
    public enum FileTypeCreation
    {
        Startup,
        Program
    }
}
