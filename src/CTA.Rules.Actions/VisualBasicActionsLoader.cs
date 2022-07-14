using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Codelyzer.Analysis.Model;
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
        _namespaceActions,
        _identifierNameActions,
        _attributeActions,
        _attributeListActions,
        _typeBlockActions,
        _elementAccessActions,
        _expressionActions,
        _interfaceActions,
        _methodBlockActions,
        _objectCreationExpressionActions;
        

    private readonly object _compilationUnitObject,
        _invocationExpressionObject,
        _namespaceObject,
        _identifierNameObject,
        _attributeObject,
        _attributeListObject,
        _typeBlockObject,
        _elementAccessObject,
        _expressionObject,
        _interfaceObject,
        _methodBlockObject,
        _objectExpressionObject;
    
    /// <summary>
    /// Initializes a new ActionLoader that loads the default actions
    /// </summary>
    /// <param name="assemblyPaths">A directory containing additional actions to be used</param>
    public VisualBasicActionsLoader(List<string> assemblyPaths)
    {
        _compilationUnitActions = new List<MethodInfo>();
        _invocationExpressionActions = new List<MethodInfo>();
        _namespaceActions = new List<MethodInfo>();
        _identifierNameActions = new List <MethodInfo>();
        _attributeActions = new List <MethodInfo>();
        _attributeListActions = new List <MethodInfo>();
        _typeBlockActions = new List <MethodInfo>();
        _elementAccessActions = new List <MethodInfo>();
        _expressionActions = new List <MethodInfo>();
        _interfaceActions = new List <MethodInfo>();
        _methodBlockActions = new List <MethodInfo>();
        _objectCreationExpressionActions = new List<MethodInfo>();
        memberAccessActions = new List<MethodInfo>();
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
                                 t.Name.StartsWith("Project") ||
                                 t.Name.StartsWith("MemberAccess"))).ToList();
                TryCreateInstance(Constants.CompilationUnitActions, types, out _compilationUnitObject);
                TryCreateInstance(Constants.InvocationExpressionActions, types, out _invocationExpressionObject);
                TryCreateInstance(Constants.NamespaceActions, types, out _namespaceObject);
                TryCreateInstance(Constants.AttributeActions, types, out _attributeObject);
                TryCreateInstance(Constants.AttributeListActions, types, out _attributeListObject);
                TryCreateInstance(Constants.TypeBlockActions, types, out _typeBlockObject);
                TryCreateInstance(Constants.ElementAccessActions, types, out _elementAccessObject);
                TryCreateInstance(Constants.ExpressionActions, types, out _expressionObject);
                TryCreateInstance(Constants.InterfaceActions, types, out _interfaceObject);
                TryCreateInstance(Constants.MethodBlockActions, types, out _methodBlockObject);
                TryCreateInstance(Constants.ProjectLevelActions, types, out projectLevelObject);
                TryCreateInstance(Constants.ProjectFileActions, types, out projectFileObject);
                TryCreateInstance(Constants.ProjectTypeActions, types, out projectTypeObject);
                TryCreateInstance(Constants.IdentifierNameActions, types, out _identifierNameObject);
                TryCreateInstance(Constants.ObjectCreationExpressionActions, types, out _objectExpressionObject);
                TryCreateInstance(Constants.MemberAccessActions, types, out memberAccessObject);

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
                        case Constants.IdentifierNameActions:
                        {
                            _identifierNameActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                        case Constants.AttributeActions:
                        {
                            _attributeActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                        case Constants.AttributeListActions:
                        {
                            _attributeListActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                        case Constants.TypeBlockActions:
                        {
                            _typeBlockActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                        case Constants.InterfaceActions:
                        {
                            _interfaceActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                        case Constants.ExpressionActions:
                        {
                            _expressionActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                        case Constants.MethodBlockActions:
                        {
                            _methodBlockActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                        case Constants.ElementAccessActions:
                        {
                            _elementAccessActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                        case Constants.ObjectCreationExpressionActions:
                        {
                            _objectCreationExpressionActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                        case Constants.MemberAccessActions:
                        {
                            memberAccessActions.AddRange(GetFuncMethods(t));
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
        return GetAction<Func<SyntaxGenerator, AttributeSyntax, AttributeSyntax>>
            (_attributeActions, _attributeObject, name, value);
    }

    public Func<SyntaxGenerator, AttributeListSyntax, AttributeListSyntax> GetAttributeListAction(string name, dynamic value)
    {
        return GetAction<Func<SyntaxGenerator, AttributeListSyntax, AttributeListSyntax>>
            (_attributeListActions, _attributeListObject, name, value);
    }
    
    public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetClassAction(string name, dynamic value)
    {
        return GetAction<Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax>>
            (_typeBlockActions, _typeBlockObject, name, value);
    }
    
    public Func<SyntaxGenerator, InterfaceBlockSyntax, InterfaceBlockSyntax> GetInterfaceAction(string name, dynamic value)
    {
        return GetAction<Func<SyntaxGenerator, InterfaceBlockSyntax, InterfaceBlockSyntax>>
            (_interfaceActions, _interfaceObject, name, value);
    }
    
    public Func<SyntaxGenerator, IdentifierNameSyntax, IdentifierNameSyntax> GetIdentifierNameAction(string name, dynamic value)
    {
        return GetAction<Func<SyntaxGenerator, IdentifierNameSyntax, IdentifierNameSyntax>>
            (_identifierNameActions, _identifierNameObject, name, value);
    }

    public Func<SyntaxGenerator, SyntaxNode, SyntaxNode> GetExpressionAction(string name, dynamic value)
    {
        return GetAction<Func<SyntaxGenerator, SyntaxNode, SyntaxNode>>
            (_expressionActions, _expressionObject, name, value);
    }
    
    public Func<SyntaxGenerator, MethodBlockSyntax, MethodBlockSyntax> GetMethodDeclarationAction(string name, dynamic value)
    {
        return GetAction<Func<SyntaxGenerator, MethodBlockSyntax, MethodBlockSyntax>>
            (_methodBlockActions, _methodBlockObject, name, value);
    }

    public Func<SyntaxGenerator, NamespaceBlockSyntax, NamespaceBlockSyntax> GetNamespaceActions(string name,
        dynamic value)
    {
        return GetAction<Func<SyntaxGenerator, NamespaceBlockSyntax, NamespaceBlockSyntax>>
            (_namespaceActions, _namespaceObject, name, value);
    }

    public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> GetObjectCreationExpressionActions(
        string name, dynamic value)
    {
        return GetAction<Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax>>
            (_objectCreationExpressionActions, _objectExpressionObject, name, value);
    }

    public Func<SyntaxGenerator, MemberAccessExpressionSyntax, MemberAccessExpressionSyntax>
        GetElementAccessExpressionActions(string name, dynamic value)
    {
        return GetAction<Func<SyntaxGenerator, MemberAccessExpressionSyntax, MemberAccessExpressionSyntax>>
            (_elementAccessActions, _elementAccessObject, name, value);
    }
}
