using CTA.Rules.Config;
using CTA.Rules.Metrics;
using CTA.Rules.Models;
using CTA.Rules.Models.Tokens;
using Codelyzer.Analysis.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CTA.Rules.Analyzer
{
    /// <summary>
    /// Object to use for creating an analysis based on a code analysis and a list of rules
    /// </summary>
    public class RulesAnalysis
    {
        private RootNodes _rootNodes;
        private List<RootUstNode> _sourceFileResults;
        private ProjectActions _projectActions;

        /// <summary>
        /// Initializes an RulesAnalysis instance
        /// </summary>
        /// <param name="sourceFileResults">List of analyzed code files</param>
        /// <param name="rootNodes">List of rules to be applied to the code files</param>
        public RulesAnalysis(List<RootUstNode> sourceFileResults, RootNodes rootNodes)
        {
            _projectActions = new ProjectActions();
            _sourceFileResults = sourceFileResults;
            _rootNodes = rootNodes;
        }
        /// <summary>
        /// Runs the Rules Analysis
        /// </summary>
        /// <returns></returns>
        public ProjectActions Analyze()
        {
            var options = new ParallelOptions() { MaxDegreeOfParallelism = Constants.ThreadCount };
            Parallel.ForEach(_sourceFileResults, options, result => {
                var fileAction = new FileActions() { FilePath = result.FileFullPath };
                if (AnalyzeChildren(fileAction, result.Children, 0))
                {
                    _projectActions.FileActions.Add(fileAction);
                }
            });     
            return _projectActions;
        }

        /// <summary>
        /// Analyzes children of nodes in a particular file
        /// </summary>
        /// <param name="fileAction">The object containing the actions to run on the file</param>
        /// <param name="children">List of child nodes to check</param>
        /// <param name="level">Recursion level to avoid stack overflows</param>
        private bool AnalyzeChildren(FileActions fileAction, UstList<UstNode> children, int level, string parentNamespace = "", string parentClass = "")
        {
            bool containsActions = false;

            if (children == null || level > Constants.MaxRecursionDepth)
            {
                return false;
            }

            foreach (var child in children)
            {
                try
                {
                    switch (child.NodeType)
                    {
                        case IdConstants.AnnotationIdName:
                            {
                                var annotation = (Annotation)child;
                                var compareToken = new AttributeToken() { Key = annotation.Identifier, Namespace = annotation.Reference.Namespace, Type = annotation.SemanticClassType };
                                AttributeToken token = null;
                                _rootNodes.Attributetokens.TryGetValue(compareToken, out token);
                                if (token != null)
                                {
                                    AddActions(fileAction, token, child.TextSpan);
                                    containsActions = true;
                                }
                                break;
                            }
                        case IdConstants.UsingDirectiveIdName:
                            {
                                var compareToken = new UsingDirectiveToken() { Key = child.Identifier };
                                UsingDirectiveToken token = null;
                                _rootNodes.Usingdirectivetokens.TryGetValue(compareToken, out token);
                                if (token != null)
                                {
                                    AddActions(fileAction, token, child.TextSpan);
                                    containsActions = true;
                                }
                                break;
                            }
                        case IdConstants.NamespaceIdName:
                            {
                                var compareToken = new NamespaceToken() { Key = child.Identifier };
                                NamespaceToken token = null;
                                _rootNodes.NamespaceTokens.TryGetValue(compareToken, out token);
                                if (token != null)
                                {
                                    AddActions(fileAction, token, child.TextSpan);
                                    containsActions = true;
                                }
                                if (AnalyzeChildren(fileAction, child.Children, ++level, child.Identifier)) { containsActions = true; }
                                break;
                            }
                        case IdConstants.ClassIdName:
                            {
                                var classType = (ClassDeclaration)child;
                                var baseToken = new ClassDeclarationToken() { FullKey = classType.BaseType };
                                ClassDeclarationToken token = null;
                                _rootNodes.Classdeclarationtokens.TryGetValue(baseToken, out token);

                                if (token != null)
                                {
                                    //In case of class declarations, add actions on the class by name, instead of property                                
                                    AddNamedActions(fileAction, token, classType.Identifier, child.TextSpan);
                                    AddActions(fileAction, token, child.TextSpan);
                                    containsActions = true;
                                }

                                token = null;
                                string name = string.Concat(classType.Reference != null ? string.Concat(classType.Reference.Namespace, ".") : string.Empty, classType.Identifier);
                                var nameToken = new ClassDeclarationToken() { FullKey = name };
                                _rootNodes.Classdeclarationtokens.TryGetValue(nameToken, out token);

                                if (token != null)
                                {
                                    //In case of class declarations, add actions on the class by name, instead of property                                
                                    AddNamedActions(fileAction, token, classType.Identifier, child.TextSpan);
                                    AddActions(fileAction, token, child.TextSpan);
                                    containsActions = true;
                                }

                                if (AnalyzeChildren(fileAction, child.Children, ++level, parentNamespace, classType.Identifier)) { containsActions = true; }
                                break;
                            }
                        case IdConstants.InterfaceIdName:
                            {
                                var interfaceType = (InterfaceDeclaration)child;
                                var baseToken = new InterfaceDeclarationToken() { FullKey = interfaceType.BaseType };
                                InterfaceDeclarationToken token = null;

                                if (!string.IsNullOrEmpty(interfaceType.BaseType))
                                {
                                    _rootNodes.InterfaceDeclarationTokens.TryGetValue(baseToken, out token);
                                }

                                if (token != null)
                                {
                                    //In case of interface declarations, add actions on the interface by name, instead of property                                
                                    AddNamedActions(fileAction, token, interfaceType.Identifier, child.TextSpan);
                                    AddActions(fileAction, token, child.TextSpan);
                                    containsActions = true;
                                }

                                token = null;
                                string name = string.Concat(interfaceType.Reference != null ? string.Concat(interfaceType.Reference.Namespace, ".") : string.Empty, interfaceType.Identifier);
                                var nameToken = new InterfaceDeclarationToken() { FullKey = name };
                                _rootNodes.InterfaceDeclarationTokens.TryGetValue(nameToken, out token);

                                if (token != null)
                                {
                                    //In case of interface declarations, add actions on the interface by name, instead of property                                   
                                    AddNamedActions(fileAction, token, interfaceType.Identifier, child.TextSpan);
                                    AddActions(fileAction, token, child.TextSpan);
                                    containsActions = true;
                                }

                                if (AnalyzeChildren(fileAction, child.Children, ++level, parentNamespace)) { containsActions = true; }
                                break;
                            }
                        case IdConstants.MethodIdName:
                            {
                                var compareToken = new MethodDeclarationToken() { FullKey = string.Concat(child.Identifier) };
                                MethodDeclarationToken token = null;
                                _rootNodes.MethodDeclarationTokens.TryGetValue(compareToken, out token);
                                if (token != null)
                                {
                                    AddNamedActions(fileAction, token, child.Identifier, child.TextSpan);
                                    AddActions(fileAction, token, child.TextSpan);
                                    containsActions = true;
                                }
                                if (AnalyzeChildren(fileAction, child.Children, ++level, parentNamespace, parentClass)) { containsActions = true; }
                                break;
                            }
                        case IdConstants.InvocationIdName:
                            {
                                InvocationExpression invocationExpression = (InvocationExpression)child;
                                var compareToken = new InvocationExpressionToken() { Key = invocationExpression.MethodName, Namespace = invocationExpression.Reference.Namespace, Type = invocationExpression.SemanticClassType };
                                InvocationExpressionToken token = null;
                                _rootNodes.Invocationexpressiontokens.TryGetValue(compareToken, out token);
                                if (token != null)
                                {
                                    AddActions(fileAction, token, child.TextSpan);
                                    containsActions = true;
                                }
                                if (AnalyzeChildren(fileAction, child.Children, ++level, parentNamespace, parentClass)) { containsActions = true; }
                                break;
                            }
                        case IdConstants.DeclarationNodeIdName:
                            {
                                var declarationNode = (DeclarationNode)child;
                                var compareToken = new IdentifierNameToken() { Key = string.Concat(declarationNode.Reference.Namespace, ".", declarationNode.Identifier), Namespace = declarationNode.Reference.Namespace };
                                IdentifierNameToken token = null;
                                _rootNodes.Identifiernametokens.TryGetValue(compareToken, out token);
                                if (token != null)
                                {
                                    AddActions(fileAction, token, child.TextSpan);
                                    containsActions = true;
                                }
                                if (AnalyzeChildren(fileAction, child.Children, ++level, parentNamespace, parentClass)) { containsActions = true; }
                                break;
                            }
                        case IdConstants.ObjectCreationIdName:
                            {
                                var objectCreationNode = (ObjectCreationExpression)child;
                                //Rules based on Object Creation Parent Hierarchy
                                var compareToken = new ObjectCreationExpressionToken() { Key = objectCreationNode.Identifier, Namespace = objectCreationNode.Reference?.Namespace, Type = objectCreationNode.SemanticClassType };
                                ObjectCreationExpressionToken token = null;
                                _rootNodes.ObjectCreationExpressionTokens.TryGetValue(compareToken, out token);
                                if (token != null)
                                {
                                    AddActions(fileAction, token, child.TextSpan);
                                    containsActions = true;
                                }

                                //Rules based on Object Creation location within code
                                var compareTokenLocation = new ObjectCreationExpressionToken() { Key = objectCreationNode.Identifier, Namespace = parentNamespace, Type = parentClass };
                                ObjectCreationExpressionToken tokenLocation = null;
                                _rootNodes.ObjectCreationExpressionTokens.TryGetValue(compareTokenLocation, out tokenLocation);
                                if (tokenLocation != null)
                                {
                                    AddActions(fileAction, tokenLocation, child.TextSpan);
                                    containsActions = true;
                                }

                                if (AnalyzeChildren(fileAction, child.Children, ++level, parentNamespace, parentClass)) { containsActions = true; }
                                break;
                            }
                        default:
                            {
                                if (AnalyzeChildren(fileAction, child.Children, ++level, parentNamespace, parentClass)) { containsActions = true; }
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, "Error loading actions for item {0} of type {1}", child.Identifier, child.NodeType);
                }
            }
            return containsActions;
        }

        /// <summary>
        /// Add actions matching the token
        /// </summary>
        /// <param name="fileAction">The file to run actions on</param>
        /// <param name="token">The token that matched the file</param>
        private void AddActions(FileActions fileAction, NodeToken token, TextSpan textSpan)
        {
            fileAction.AttributeActions.UnionWith(token.AttributeActions.Select(a => new AttributeAction()
            {
                Key = a.Key,
                Description = a.Description,
                Value = a.Value,
                Name = a.Name,
                Type = a.Type,
                TextSpan = textSpan,
                ActionValidation = a.ActionValidation,
                AttributeActionFunc = a.AttributeActionFunc,
                AttributeListActionFunc = a.AttributeListActionFunc
            }).ToList());

            fileAction.AttributeActions.UnionWith(token.AttributeListActions.Select(a => new AttributeAction()
            {
                Key = a.Key,
                Description = a.Description,
                Value = a.Value,
                Name = a.Name,
                Type = a.Type,
                TextSpan = textSpan,
                ActionValidation = a.ActionValidation,
                AttributeActionFunc = a.AttributeActionFunc,
                AttributeListActionFunc = a.AttributeListActionFunc
            }).ToList());

            fileAction.IdentifierNameActions.UnionWith(token.IdentifierNameActions.Select(a => new IdentifierNameAction()
            {
                Key = a.Key,
                Description = a.Description,
                Value = a.Value,
                Name = a.Name,
                Type = a.Type,
                TextSpan = textSpan,
                ActionValidation = a.ActionValidation,
                IdentifierNameActionFunc = a.IdentifierNameActionFunc,
            }).ToList());

            fileAction.InvocationExpressionActions.UnionWith(token.InvocationExpressionActions.Select(a => new InvocationExpressionAction()
            {
                Key = a.Key,
                Description = a.Description,
                Value = a.Value,
                Name = a.Name,
                Type = a.Type,
                TextSpan = textSpan,
                ActionValidation = a.ActionValidation,
                InvocationExpressionActionFunc = a.InvocationExpressionActionFunc
            }).ToList());

            fileAction.Usingactions.UnionWith(token.UsingActions.Select(a => new UsingAction()
            {
                Key = a.Key,
                Description = a.Description,
                Value = a.Value,
                Name = a.Name,
                Type = a.Type,
                TextSpan = textSpan,
                ActionValidation = a.ActionValidation,
                UsingActionFunc = a.UsingActionFunc
            }).ToList());

            fileAction.NamespaceActions.UnionWith(token.NamespaceActions.Select(a => new NamespaceAction()
            {
                Key = a.Key,
                Description = a.Description,
                Value = a.Value,
                Name = a.Name,
                Type = a.Type,
                TextSpan = textSpan,
                ActionValidation = a.ActionValidation,
                NamespaceActionFunc = a.NamespaceActionFunc
            }).ToList());


            fileAction.ObjectCreationExpressionActions.UnionWith(token.ObjectCreationExpressionActions.Select(a => new ObjectCreationExpressionAction()
            {
                Key = a.Key,
                Description = a.Description,
                Value = a.Value,
                Name = a.Name,
                Type = a.Type,
                TextSpan = textSpan,
                ActionValidation = a.ActionValidation,
                ObjectCreationExpressionGenericActionFunc = a.ObjectCreationExpressionGenericActionFunc
            }).ToList());


            if (fileAction.AttributeActions.Any()
                || fileAction.IdentifierNameActions.Any()
                || fileAction.InvocationExpressionActions.Any() 
                || fileAction.Usingactions.Any() 
                || fileAction.NamespaceActions.Any() 
                || fileAction.ObjectCreationExpressionActions.Any())
            {
                var nodeToken = NodeToken.CopyToken(token);
                nodeToken.TextSpan = textSpan;
                fileAction.NodeTokens.Add(nodeToken);
            }

            AddPackages(token.PackageActions, textSpan);
        }

        /// <summary>
        /// Adds a list of packages to the project actions
        /// </summary>
        /// <param name="packageActions">List of package actions based on the rules</param>
        private void AddPackages(List<PackageAction> packageActions, TextSpan textSpan)
        {
            if (packageActions != null && packageActions.Count > 0)
            {
                packageActions.ForEach((p) =>
                {
                    if (!_projectActions.PackageActions.Contains(p))
                    {
                        _projectActions.PackageActions.Add(new PackageAction() { Name = p.Name, Version = p.Version, TextSpan = textSpan });
                    }
                });
            }
        }

        /// <summary>
        /// Add actions using the identifier of the object matching the token
        /// </summary>
        /// <param name="fileAction"></param>
        /// <param name="token"></param>
        /// <param name="identifier"></param>
        private void AddNamedActions(FileActions fileAction, NodeToken token, string identifier, TextSpan textSpan)
        {

            fileAction.ClassDeclarationActions.UnionWith(token.ClassDeclarationActions
                .Select(c => new ClassDeclarationAction()
                {
                    Key = identifier,
                    Value = c.Value,
                    Description = c.Description,
                    Name = c.Name,
                    Type =c.Type,
                    TextSpan = textSpan,
                    ActionValidation = c.ActionValidation,
                    ClassDeclarationActionFunc = c.ClassDeclarationActionFunc
                }));

            fileAction.InterfaceDeclarationActions.UnionWith(token.InterfaceDeclarationActions
                .Select(c => new InterfaceDeclarationAction()
                {
                    Key = identifier,
                    Value = c.Value,
                    Name = c.Name,
                    Type = c.Type,
                    Description = c.Description,
                    TextSpan = textSpan,
                    ActionValidation = c.ActionValidation,
                    InterfaceDeclarationActionFunc = c.InterfaceDeclarationActionFunc
                }));

            fileAction.MethodDeclarationActions.UnionWith(token.MethodDeclarationActions
                .Select(c => new MethodDeclarationAction()
                {
                    Key = identifier,
                    Value = c.Value,
                    Description = c.Description,
                    Name = c.Name,
                    Type = c.Type,
                    TextSpan = textSpan,
                    ActionValidation = c.ActionValidation,
                    MethodDeclarationActionFunc = c.MethodDeclarationActionFunc
                }));

            if (fileAction.ClassDeclarationActions.Any() || fileAction.InterfaceDeclarationActions.Any() || fileAction.MethodDeclarationActions.Any())
            {
                var nodeToken = NodeToken.CopyToken(token);
                nodeToken.TextSpan = textSpan;
                fileAction.NodeTokens.Add(nodeToken);
            }
        }
    }
}
