using CTA.Rules.Config;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace CTA.Rules.Actions
{
    /// <summary>
    /// Loads actions into the current execution context
    /// </summary>
    public class ActionsLoader
    {
        private List<MethodInfo> compilationUnitActions, attributeActions, attributeListActions, classActions,
        identifierNameActions, invocationExpressionActions, methodDeclarationActions, elementAccessActions,
        objectCreationExpressionActions, memberAccessActions, namespaceActions, projectLevelActions, projectFileActions, interfaceActions;

        private object attributeObject, attributeListObject, classObject, interfaceObject, compilationUnitObject, identifierNameObject
            , invocationExpressionObject, methodDeclarationObject, elementAccessObject, memberAccessObject, objectExpressionObject, namespaceObject, projectLevelObject,
            projectFileObject;

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
            methodDeclarationActions = new List<MethodInfo>();
            elementAccessActions = new List<MethodInfo>();
            memberAccessActions = new List<MethodInfo>();
            objectCreationExpressionActions = new List<MethodInfo>();
            namespaceActions = new List<MethodInfo>();
            projectLevelActions = new List<MethodInfo>();
            interfaceActions = new List<MethodInfo>();
            projectFileActions = new List<MethodInfo>();

            var actionsAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains("CTA.Rules.Actions")).FirstOrDefault();

            try
            {
                LoadAssemblyTypes(actionsAssembly);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex, "Error while loading actions assembly");
                throw ex;
            }
            foreach (var path in assemblyPaths)
            {
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                    LoadAssemblyTypes(assembly);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, string.Format("Error loading actions from {0}{1}{2}", path, Environment.NewLine, ex.Message));
                }
            }
        }

        /// <summary>
        /// Loads actions from the provided assembly
        /// </summary>
        /// <param name="assembly">Assembly containing Roslyn actions</param>
        private void LoadAssemblyTypes(Assembly assembly)
        {
            var types = assembly.GetTypes().Where(t => t.Name.EndsWith("Actions"));

            attributeObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.AttributeActions));
            attributeListObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.AttributeListActions));
            classObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.ClassActions));
            compilationUnitObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.CompilationUnitActions));
            identifierNameObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.IdentifierNameActions));
            invocationExpressionObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.InvocationExpressionActions));
            methodDeclarationObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.MethodDeclarationActions));
            elementAccessObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.ElementAccessActions));
            memberAccessObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.MemberAccessActions));
            namespaceObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.NamespaceActions));
            objectExpressionObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.ObjectCreationExpressionActions));
            projectLevelObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.ProjectLevelActions));
            interfaceObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.InterfaceActions));
            projectFileObject = Activator.CreateInstance(types.FirstOrDefault(t => t.Name == Constants.ProjectFileActions));


            foreach (var t in types)
            {
                switch (t.Name)
                {
                    case Constants.AttributeActions:
                        {
                            attributeActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                    case Constants.AttributeListActions:
                        {
                            attributeListActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                    case Constants.ClassActions:
                        {
                            classActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                    case Constants.InterfaceActions:
                        {
                            interfaceActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                    case Constants.CompilationUnitActions:
                        {
                            compilationUnitActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                    case Constants.IdentifierNameActions:
                        {
                            identifierNameActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                    case Constants.InvocationExpressionActions:
                        {
                            invocationExpressionActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                    case Constants.MethodDeclarationActions:
                        {
                            methodDeclarationActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                    case Constants.ElementAccessActions:
                        {
                            elementAccessActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                    case Constants.MemberAccessActions:
                        {
                            memberAccessActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                    case Constants.ObjectCreationExpressionActions:
                        {
                            objectCreationExpressionActions.AddRange(GetFuncMethods(t));
                            break;
                        }
                    case Constants.NamespaceActions:
                        {
                            namespaceActions.AddRange(GetFuncMethods(t));
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
                    default:
                        {
                            LogHelper.LogError(string.Format("Action type {0} is not found", t.Name));
                            break;
                        }
                }
            }
        }

        private List<MethodInfo> GetFuncMethods(Type t) => t.GetMethods().Where(m => m.ReturnType.ToString().Contains("System.Func")).ToList();
        
        private string GetActionName(string name)
        {
            return string.Concat("Get", name, "Action");
        }
        public Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax> GetCompilationUnitAction(string name, dynamic value)
            => GetAction<Func<SyntaxGenerator, CompilationUnitSyntax, CompilationUnitSyntax>>
                (compilationUnitActions, compilationUnitObject, name, value);
        public Func<SyntaxGenerator, AttributeSyntax, AttributeSyntax> GetAttributeAction(string name, dynamic value) =>
            GetAction<Func<SyntaxGenerator, AttributeSyntax, AttributeSyntax>>
                (attributeActions, attributeObject, name, value);
        public Func<SyntaxGenerator, AttributeListSyntax, AttributeListSyntax> GetAttributeListAction(string name, dynamic value) =>
            GetAction<Func<SyntaxGenerator, AttributeListSyntax, AttributeListSyntax>>
                (attributeListActions, attributeListObject, name, value);
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetClassAction(string name, dynamic value) =>
            GetAction<Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax>>
                (classActions, classObject, name, value);
        public Func<SyntaxGenerator, InterfaceDeclarationSyntax, InterfaceDeclarationSyntax> GetInterfaceAction(string name, dynamic value) =>
            GetAction<Func<SyntaxGenerator, InterfaceDeclarationSyntax, InterfaceDeclarationSyntax>>
                (interfaceActions, interfaceObject, name, value);
        public Func<SyntaxGenerator, IdentifierNameSyntax, IdentifierNameSyntax> GetIdentifierNameAction(string name, dynamic value) =>
            GetAction<Func<SyntaxGenerator, IdentifierNameSyntax, IdentifierNameSyntax>>
                (identifierNameActions, identifierNameObject, name, value);
        public Func<SyntaxGenerator, InvocationExpressionSyntax, InvocationExpressionSyntax> GetInvocationExpressionAction(string name, dynamic value) =>
            GetAction<Func<SyntaxGenerator, InvocationExpressionSyntax, InvocationExpressionSyntax>>
                (invocationExpressionActions, invocationExpressionObject, name, value);
        public Func<SyntaxGenerator, MethodDeclarationSyntax, MethodDeclarationSyntax> GetMethodDeclarationAction(string name, dynamic value) =>
            GetAction<Func<SyntaxGenerator, MethodDeclarationSyntax, MethodDeclarationSyntax>>
                (methodDeclarationActions, methodDeclarationObject, name, value);
        public Func<SyntaxGenerator, NamespaceDeclarationSyntax, NamespaceDeclarationSyntax> GetNamespaceActions(string name, dynamic value) =>
            GetAction<Func<SyntaxGenerator, NamespaceDeclarationSyntax, NamespaceDeclarationSyntax>>
                (namespaceActions, namespaceObject, name, value);
        public Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax> GetObjectCreationExpressionActions(string name, dynamic value) =>
            GetAction<Func<SyntaxGenerator, ObjectCreationExpressionSyntax, ExpressionSyntax>>
                (objectCreationExpressionActions, objectExpressionObject, name, value);
        public Func<string, ProjectType, string> GetProjectLevelActions(string name, dynamic value) =>
            GetAction<Func<string, ProjectType, string>>
                (projectLevelActions, projectLevelObject, name, value);
        public Func<string, ProjectType, List<string>, Dictionary<string, string>, List<string>, string> GetProjectFileActions(string name, dynamic value) =>
            GetAction<Func<string, ProjectType, List<string>, Dictionary<string, string>, List<string>, string>>
                (projectFileActions, projectFileObject, name, value);
        public Func<SyntaxGenerator, ElementAccessExpressionSyntax, ElementAccessExpressionSyntax> GetElementAccessExpressionActions(string name, dynamic value) =>
            GetAction<Func<SyntaxGenerator, ElementAccessExpressionSyntax, ElementAccessExpressionSyntax>>
                (elementAccessActions, elementAccessObject, name, value);
        public Func<SyntaxGenerator, MemberAccessExpressionSyntax, MemberAccessExpressionSyntax> GetMemberAccessExpressionActions(string name, dynamic value) =>
            GetAction<Func<SyntaxGenerator, MemberAccessExpressionSyntax, MemberAccessExpressionSyntax>>
                (memberAccessActions, memberAccessObject, name, value);

        /// <summary>
        /// Gets the action by invoking the methods that will create it
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="actions">List of actions on the type T</param>
        /// <param name="invokeObject">The object that will be used to retrieve the action</param>
        /// <param name="name">Name of the action</param>
        /// <param name="value">Parameter(s) of the action</param>
        /// <returns></returns>
        private T GetAction<T>(List<MethodInfo> actions, object invokeObject, string name, dynamic value)
        {
            T val = default(T);
            try
            {
                string actionName = GetActionName(name);
                var method = actions.Where(m => m.Name == actionName).FirstOrDefault();
                if (method == null)
                {
                    LogHelper.LogDebug(string.Format("No such action {0}", actionName));
                }
                else
                {
                    var parameters = GetParameters(value, method);

                    if (parameters != null)
                    {
                        val = (T)method.Invoke(invokeObject, parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex, "Error while loading action {0}", name);
            }
            return val;
        }

        /// <summary>
        /// Gets the parameters for the action. The parameters should match the action signature in the provided rules file
        /// </summary>
        /// <param name="value">The paramter(s) as a string or JSON object</param>
        /// <param name="method">The method for these parameters</param>
        /// <returns></returns>
        private string[] GetParameters(dynamic value, MethodInfo method)
        {
            List<string> result = new List<string>();

            try
            {
                if (value is string)
                {
                    var strValue = value.ToString();
                    if (strValue.StartsWith("{"))
                    {
                        try
                        {
                            result = GetJsonParameters(value.ToString(), method);
                        }
                        catch (Exception)
                        {
                            result = new List<string>() { value };
                        }
                    }
                    else
                    {
                        result = new List<string>() { value };
                    }
                }
                else
                {
                    result = GetJsonParameters(value.ToString(), method);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex, "Error while loading parameters for action {0}", method.Name);
            }
            return result.ToArray();
        }

        private List<string> GetJsonParameters(string value, MethodInfo method)
        {
            List<string> result = new List<string>();

            Dictionary<string, string> jsonParameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);

            var methodParams = method.GetParameters();
            foreach (var p in methodParams)
            {
                if (jsonParameters.ContainsKey(p.Name))
                {
                    result.Add(jsonParameters[p.Name]);
                }
                else
                {
                    LogHelper.LogDebug(string.Format("Parameter {0} is not available for action {1}", p.Name, method.Name));
                    return null;
                }
            }
            return result;
        }
    }

}
