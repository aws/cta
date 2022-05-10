using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Codelyzer.Analysis;
using CTA.Rules.Config;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions
{
    /// <summary>
    /// Loads actions into the current execution context
    /// </summary>
    public class ActionsLoader
    {
        private readonly List<MethodInfo> compilationUnitActions,
            attributeActions,
            attributeListActions,
            classActions,
            identifierNameActions,
            invocationExpressionActions,
            expressionActions,
            methodDeclarationActions,
            elementAccessActions,
            objectCreationExpressionActions,
            memberAccessActions,
            namespaceActions,
            projectLevelActions,
            projectFileActions,
            projectTypeActions,
            interfaceActions;

        private readonly object attributeObject,
            attributeListObject,
            classObject,
            interfaceObject,
            compilationUnitObject,
            identifierNameObject,
            invocationExpressionObject,
            expressionObject,
            methodDeclarationObject,
            elementAccessObject,
            memberAccessObject,
            objectExpressionObject,
            namespaceObject,
            projectLevelObject,
            projectFileObject,
            projectTypeObject;

        /// <summary>
        /// Initializes a new ActionLoader that loads the default actions
        /// </summary>
        /// <param name="assemblyPaths">A directory containing additional actions to be used</param>
        public ActionsLoader(List<string> assemblyPaths)
        {
            compilationUnitActions = new List<MethodInfo>();
            attributeActions = new List<MethodInfo>();
            attributeListActions = new List<MethodInfo>();
            classActions = new List<MethodInfo>();
            identifierNameActions = new List<MethodInfo>();
            invocationExpressionActions = new List<MethodInfo>();
            expressionActions = new List<MethodInfo>();
            methodDeclarationActions = new List<MethodInfo>();
            elementAccessActions = new List<MethodInfo>();
            memberAccessActions = new List<MethodInfo>();
            objectCreationExpressionActions = new List<MethodInfo>();
            namespaceActions = new List<MethodInfo>();
            projectLevelActions = new List<MethodInfo>();
            interfaceActions = new List<MethodInfo>();
            projectFileActions = new List<MethodInfo>();
            projectTypeActions = new List<MethodInfo>();

            var actionsAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains("CTA.Rules.Actions")).FirstOrDefault();

            var assemblies = new List<Assembly>
            {
                actionsAssembly
            };

            foreach (var path in assemblyPaths)
            {
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                    assemblies.Add(assembly);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, string.Format("Error loading assembly from {0}{1}{2}", path, Environment.NewLine, ex.Message));
                }
            }

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.Name.EndsWith("Actions") &&
                                    (t.Namespace.EndsWith(ProjectLanguage.Csharp.ToString()) ||
                                     t.Name.StartsWith("Project"))).ToList();

                    attributeObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.AttributeActions));
                    attributeListObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.AttributeListActions));
                    classObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.ClassActions));
                    compilationUnitObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.CompilationUnitActions));
                    identifierNameObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.IdentifierNameActions));
                    invocationExpressionObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.InvocationExpressionActions));
                    expressionObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.ExpressionActions));
                    methodDeclarationObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.MethodDeclarationActions));
                    elementAccessObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.ElementAccessActions));
                    memberAccessObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.MemberAccessActions));
                    namespaceObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.NamespaceActions));
                    objectExpressionObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.ObjectCreationExpressionActions));
                    projectLevelObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.ProjectLevelActions));
                    interfaceObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.InterfaceActions));
                    projectFileObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.ProjectFileActions));
                    projectTypeObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.ProjectTypeActions));

                    foreach (var t in types)
                    {
                        switch (t.Name)
                        {
                            case Constants.AttributeActions:
                                {
                                    attributeActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                                    break;
                                }
                            case Constants.AttributeListActions:
                                {
                                    attributeListActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                                    break;
                                }
                            case Constants.ClassActions:
                                {
                                    classActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                                    break;
                                }
                            case Constants.InterfaceActions:
                                {
                                    interfaceActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                                    break;
                                }
                            case Constants.CompilationUnitActions:
                                {
                                    compilationUnitActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                                    break;
                                }
                            case Constants.IdentifierNameActions:
                                {
                                    identifierNameActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                                    break;
                                }
                            case Constants.InvocationExpressionActions:
                                {
                                    invocationExpressionActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                                    break;
                                }
                            case Constants.ExpressionActions:
                                {
                                    expressionActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                                    break;
                                }
                            case Constants.MethodDeclarationActions:
                                {
                                    methodDeclarationActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                                    break;
                                }
                            case Constants.ElementAccessActions:
                                {
                                    elementAccessActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                                    break;
                                }
                            case Constants.MemberAccessActions:
                                {
                                    memberAccessActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                                    break;
                                }
                            case Constants.ObjectCreationExpressionActions:
                                {
                                    objectCreationExpressionActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                                    break;
                                }
                            case Constants.NamespaceActions:
                                {
                                    namespaceActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                                    break;
                                }
                            case Constants.ProjectLevelActions:
                                {
                                    projectLevelActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                                    break;
                                }
                            case Constants.ProjectFileActions:
                                {
                                    projectFileActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                                    break;
                                }
                            case Constants.ProjectTypeActions:
                                {
                                    projectTypeActions.AddRange(ActionLoaderUtils.GetFuncMethods(t));
                                    break;
                                }
                            default:
                                {
                                    LogHelper.LogError(string.Format("Action type {0} is not found", t.Name));
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
        public Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax> GetCompilationUnitAction(string name, dynamic value)
            => ActionLoaderUtils.GetAction<Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax>>
                (compilationUnitActions, compilationUnitObject, name, value);
        public Func<SyntaxGenerator, AttributeSyntax, AttributeSyntax> GetAttributeAction(string name, dynamic value) =>
            ActionLoaderUtils.GetAction<Func<SyntaxGenerator, AttributeSyntax, AttributeSyntax>>
                (attributeActions, attributeObject, name, value);
        public Func<SyntaxGenerator, AttributeListSyntax, AttributeListSyntax> GetAttributeListAction(string name, dynamic value) =>
            ActionLoaderUtils.GetAction<Func<SyntaxGenerator, AttributeListSyntax, AttributeListSyntax>>
                (attributeListActions, attributeListObject, name, value);
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetClassAction(string name, dynamic value) =>
            ActionLoaderUtils.GetAction<Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax>>
                (classActions, classObject, name, value);
        public Func<SyntaxGenerator, InterfaceDeclarationSyntax, InterfaceDeclarationSyntax> GetInterfaceAction(string name, dynamic value) =>
            ActionLoaderUtils.GetAction<Func<SyntaxGenerator, InterfaceDeclarationSyntax, InterfaceDeclarationSyntax>>
                (interfaceActions, interfaceObject, name, value);
        public Func<SyntaxGenerator, IdentifierNameSyntax, IdentifierNameSyntax> GetIdentifierNameAction(string name, dynamic value) =>
            ActionLoaderUtils.GetAction<Func<SyntaxGenerator, IdentifierNameSyntax, IdentifierNameSyntax>>
                (identifierNameActions, identifierNameObject, name, value);
        public Func<SyntaxGenerator, InvocationExpressionSyntax, InvocationExpressionSyntax> GetInvocationExpressionAction(string name, dynamic value) =>
            ActionLoaderUtils.GetAction<Func<SyntaxGenerator, InvocationExpressionSyntax, InvocationExpressionSyntax>>
                (invocationExpressionActions, invocationExpressionObject, name, value);
        public Func<SyntaxGenerator, SyntaxNode, SyntaxNode> GetExpressionAction(string name, dynamic value) =>
            ActionLoaderUtils.GetAction<Func<SyntaxGenerator, SyntaxNode, SyntaxNode>>
                (expressionActions, expressionObject, name, value);
        public Func<SyntaxGenerator, MethodDeclarationSyntax, MethodDeclarationSyntax> GetMethodDeclarationAction(string name, dynamic value) =>
            ActionLoaderUtils.GetAction<Func<SyntaxGenerator, MethodDeclarationSyntax, MethodDeclarationSyntax>>
                (methodDeclarationActions, methodDeclarationObject, name, value);
        public Func<SyntaxGenerator, NamespaceDeclarationSyntax, NamespaceDeclarationSyntax> GetNamespaceActions(string name, dynamic value) =>
            ActionLoaderUtils.GetAction<Func<SyntaxGenerator, NamespaceDeclarationSyntax, NamespaceDeclarationSyntax>>
                (namespaceActions, namespaceObject, name, value);
        public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> GetObjectCreationExpressionActions(string name, dynamic value) =>
            ActionLoaderUtils.GetAction<Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax>>
                (objectCreationExpressionActions, objectExpressionObject, name, value);
        public Func<string, ProjectType, string> GetProjectLevelActions(string name, dynamic value) =>
            ActionLoaderUtils.GetAction<Func<string, ProjectType, string>>
                (projectLevelActions, projectLevelObject, name, value);
        public Func<string, ProjectType, List<string>, Dictionary<string, string>, List<string>, List<string>, string> GetProjectFileActions(string name, dynamic value) =>
            ActionLoaderUtils.GetAction<Func<string, ProjectType, List<string>, Dictionary<string, string>, List<string>, List<string>, string>>
                (projectFileActions, projectFileObject, name, value);
        public Func<ProjectType, ProjectConfiguration, ProjectResult, AnalyzerResult, string> GetProjectTypeActions(string name, dynamic value) =>
            ActionLoaderUtils.GetAction<Func<ProjectType, ProjectConfiguration, ProjectResult, AnalyzerResult, string>>
                (projectTypeActions, projectTypeObject, name, value);
        public Func<SyntaxGenerator, ElementAccessExpressionSyntax, ElementAccessExpressionSyntax> GetElementAccessExpressionActions(string name, dynamic value) =>
            ActionLoaderUtils.GetAction<Func<SyntaxGenerator, ElementAccessExpressionSyntax, ElementAccessExpressionSyntax>>
                (elementAccessActions, elementAccessObject, name, value);
        public Func<SyntaxGenerator, SyntaxNode, SyntaxNode> GetMemberAccessExpressionActions(string name, dynamic value) =>
            ActionLoaderUtils.GetAction<Func<SyntaxGenerator, SyntaxNode, SyntaxNode>>
                (memberAccessActions, memberAccessObject, name, value);
    }

}
