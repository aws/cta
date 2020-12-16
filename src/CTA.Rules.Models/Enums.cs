using System.ComponentModel;

namespace CTA.Rules.Models
{
    public enum KeyType
    {
        ClassName,
        Expression
    }

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
    public enum NodeType
    {
        Attribute,
        ClassDeclaration,
        ElementAccess,
        IdentifierName,
        InvocationExpression,
        MemberAccess,
        UsingDirective
    }
    public enum TokenCategory
    {
        Attribute,
        ClassDeclaration,
        ElementAccess,
        IdentifierName,
        InvocationExpression,
        MemberAccess,
        UsingDirective
    }
    public enum ActionCategory
    {
        Attribute,
        ClassDeclaration,
        ElementAccess,
        IdentifierName,
        InvocationExpression,
        MemberAccess,
        UsingDirective,
        Package
    }

    public enum UsingActionType
    {
        Add,
        Remove
    }
    public enum WriteLocation
    {
        SameFile,
        Startup,
        Program,
        Csproj
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
    public enum FileType
    {
        CodeFile = 0,
        ConfigFile = 1,
        View = 2,
        CsProj = 3
    }

    public enum Score
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    public enum AssemblyType
    {
        [Description("Framework")]
        Framework = 0,
        [Description("Core")]
        Core = 1
    }

    public enum RunStatus
    {
        [Description("Not Started")]
        NotStarted,
        [Description("Processing Files")]
        ProcessingFiles,
        [Description("Processed Files")]
        ProcessedFiles,
        [Description("Running Replacements")]
        RunningReplacements,
        [Description("Complete")]
        Complete
    }
}
