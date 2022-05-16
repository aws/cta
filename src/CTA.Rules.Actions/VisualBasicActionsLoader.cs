using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CTA.Rules.Config;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions;

public class VisualBasicActionsLoader : ActionLoaderBase
{
    private readonly List<MethodInfo> _compilationUnitActions,
        _invocationExpressionActions,
        _namespaceActions;
        

    private readonly object _compilationUnitObject,
        _invocationExpressionObject,
        _namespaceObject;
    
    /// <summary>
    /// Initializes a new ActionLoader that loads the default actions
    /// </summary>
    /// <param name="assemblyPaths">A directory containing additional actions to be used</param>
    public VisualBasicActionsLoader(List<string> assemblyPaths)
    {
        _compilationUnitActions = new List<MethodInfo>();
        _invocationExpressionActions = new List<MethodInfo>();
        _namespaceActions = new List<MethodInfo>();
        projectLevelActions = new List<MethodInfo>();
        projectFileActions = new List<MethodInfo>();
        projectTypeActions = new List<MethodInfo>();

        var assemblies = GetAssemblies(assemblyPaths);

        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t.Name.EndsWith("Actions") &&
                                (t.Namespace.EndsWith(ProjectLanguage.VisualBasic.ToString()) ||
                                 t.Name.StartsWith("Project"))).ToList();
                TryCreateInstance(Constants.CompilationUnitActions, types,
                    out _compilationUnitObject);
                TryCreateInstance(Constants.InvocationExpressionActions, types,
                    out _invocationExpressionObject);
                TryCreateInstance(Constants.NamespaceActions, types,
                    out _namespaceObject);
                TryCreateInstance(Constants.ProjectLevelActions, types, out projectLevelObject);
                TryCreateInstance(Constants.ProjectFileActions, types, out projectFileObject);
                TryCreateInstance(Constants.ProjectTypeActions, types, out projectTypeObject);
                

                foreach (var t in types)
                {
                    switch (t.Name)
                    {
                        case Constants.CompilationUnitActions:
                        {
                            _compilationUnitActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                        case Constants.InvocationExpressionActions:
                        {
                            _invocationExpressionActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                        case Constants.NamespaceActions:
                        {
                            _namespaceActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                        case Constants.ProjectLevelActions:
                        {
                            projectLevelActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                        case Constants.ProjectFileActions:
                        {
                            projectFileActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                        case Constants.ProjectTypeActions:
                        {
                            projectTypeActions.AddRange(GetFuncMethods(t));
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
        return GetAction<Func<SyntaxGenerator, InvocationExpressionSyntax, InvocationExpressionSyntax>>
            (_invocationExpressionActions, _invocationExpressionObject, name, value);
    }

    public Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax> GetCompilationUnitAction(string name,
        dynamic value)
    {
        return GetAction<Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax>>
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
    
    public Func<SyntaxGenerator, NamespaceBlockSyntax, NamespaceBlockSyntax> GetNamespaceActions(string name, dynamic value)
    {
        return GetAction<Func<SyntaxGenerator, NamespaceBlockSyntax, NamespaceBlockSyntax>>
            (_namespaceActions, _namespaceObject, name, value);
    }
    

    public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> GetObjectCreationExpressionActions(string name, dynamic value)
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
