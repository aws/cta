using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Codelyzer.Analysis;
using CTA.Rules.Config;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions;

public class VisualBasicActionsLoader
{
    private readonly List<MethodInfo> _compilationUnitActions,
        _invocationExpressionActions;

    private readonly object _compilationUnitObject,
        _invocationExpressionObject;
    
    /// <summary>
    /// Initializes a new ActionLoader that loads the default actions
    /// </summary>
    /// <param name="assemblyPaths">A directory containing additional actions to be used</param>
    public VisualBasicActionsLoader(List<string> assemblyPaths)
    {
        _compilationUnitActions = new List<MethodInfo>();
        _invocationExpressionActions = new List<MethodInfo>();

        var assemblies = ActionLoaderUtils.GetAssemblies(assemblyPaths);

        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t.Name.EndsWith("Actions") &&
                                (t.Namespace.EndsWith(ProjectLanguage.VisualBasic.ToString()) ||
                                 t.Name.StartsWith("Project"))).ToList();
                ActionLoaderUtils.TryCreateInstance(Constants.CompilationUnitActions, types,
                    out _compilationUnitObject);
                ActionLoaderUtils.TryCreateInstance(Constants.InvocationExpressionActions, types,
                    out _invocationExpressionObject);

                foreach (var t in types)
                {
                    switch (t.Name)
                    {
                        case Constants.CompilationUnitActions:
                        {
                            _compilationUnitActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                            break;
                        }
                        case Constants.InvocationExpressionActions:
                        {
                            _invocationExpressionActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                            break;
                        }
                        default:
                        {
                            LogHelper.LogError($"Action type {t.Name} is not found");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex, string.Format("Error loading actions from {0}", assembly.FullName, ex.Message));
            }
        }
    }    

    public Func<SyntaxGenerator, InvocationExpressionSyntax, InvocationExpressionSyntax> GetInvocationExpressionAction(string name, dynamic value)
    {
        return ActionLoaderUtils.GetAction<Func<SyntaxGenerator, InvocationExpressionSyntax, InvocationExpressionSyntax>>
            (_invocationExpressionActions, _invocationExpressionObject, name, value);
    }

    public Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax> GetCompilationUnitAction(string name,
        dynamic value)
    {
        return ActionLoaderUtils.GetAction<Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax>>
            (_compilationUnitActions, _compilationUnitObject, name, value);
    }

    public Func<SyntaxGenerator, AttributeSyntax, AttributeSyntax> GetAttributeAction(string name, dynamic value)
    {
        throw new NotImplementedException();
    }

    public Func<SyntaxGenerator, AttributeListSyntax, AttributeListSyntax> GetAttributeListAction(string name, dynamic value)
    {
        throw new NotImplementedException();
    }

    /*
    public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetClassAction(string name, dynamic value)
    {
        throw new NotImplementedException();
    }
    */
    /*
    public Func<SyntaxGenerator, InterfaceDeclarationSyntax, InterfaceDeclarationSyntax> GetInterfaceAction(string name, dynamic value)
    {
        throw new NotImplementedException();
    }
    */

    public Func<SyntaxGenerator, IdentifierNameSyntax, IdentifierNameSyntax> GetIdentifierNameAction(string name, dynamic value)
    {
        throw new NotImplementedException();
    }

    public Func<SyntaxGenerator, SyntaxNode, SyntaxNode> GetExpressionAction(string name, dynamic value)
    {
        throw new NotImplementedException();
    }

    /*
    public Func<SyntaxGenerator, MethodDeclarationSyntax, MethodDeclarationSyntax> GetMethodDeclarationAction(string name, dynamic value)
    {
        throw new NotImplementedException();
    }
    */
    /*
    public Func<SyntaxGenerator, NamespaceDeclarationSyntax, NamespaceDeclarationSyntax> GetNamespaceActions(string name, dynamic value)
    {
        throw new NotImplementedException();
    }
    */

    public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> GetObjectCreationExpressionActions(string name, dynamic value)
    {
        throw new NotImplementedException();
    }

    public Func<string, ProjectType, string> GetProjectLevelActions(string name, dynamic value)
    {
        throw new NotImplementedException();
    }

    public Func<string, ProjectType, List<string>, Dictionary<string, string>, List<string>, List<string>, string> GetProjectFileActions(string name, dynamic value)
    {
        throw new NotImplementedException();
    }

    public Func<ProjectType, ProjectConfiguration, ProjectResult, AnalyzerResult, string> GetProjectTypeActions(string name, dynamic value)
    {
        throw new NotImplementedException();
    }

    /*
    public Func<SyntaxGenerator, ElementAccessExpressionSyntax, ElementAccessExpressionSyntax> GetElementAccessExpressionActions(string name, dynamic value)
    {
        throw new NotImplementedException();
    }
    */

    public Func<SyntaxGenerator, SyntaxNode, SyntaxNode> GetMemberAccessExpressionActions(string name, dynamic value)
    {
        throw new NotImplementedException();
    }
}
