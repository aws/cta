using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codelyzer.Analysis.Model;
using CTA.Rules.Common.Extensions;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.Models.Actions.VisualBasic;
using CTA.Rules.Models.Tokens.VisualBasic;
using CTA.Rules.Models.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using AttributeAction = CTA.Rules.Models.Actions.VisualBasic.AttributeAction;
using ElementAccessAction = CTA.Rules.Models.Actions.VisualBasic.ElementAccessAction;
using ElementAccessToken = CTA.Rules.Models.Tokens.VisualBasic.ElementAccessToken;
using IdentifierNameToken = CTA.Rules.Models.VisualBasic.IdentifierNameToken;
using MemberAccessToken = CTA.Rules.Models.Tokens.VisualBasic.MemberAccessToken;
using ObjectCreationExpressionAction = CTA.Rules.Models.Actions.VisualBasic.ObjectCreationExpressionAction;

namespace CTA.Rules.Analyzer;

public class VisualBasicRulesAnalysis : IRulesAnalysis
{
    private readonly VisualBasicRootNodes _visualBasicRootNodes;
    private readonly List<RootUstNode> _sourceFileResults;
    private readonly ProjectActions _projectActions;
    private readonly ProjectType _projectType;

    /// <summary>
    ///     Initializes a RulesAnalysis instance for Visual Basic projects
    /// </summary>
    /// <param name="sourceFileResults">List of analyzed code files</param>
    /// <param name="visualBasicRootNodes">List of rules to be applied to the code files</param>
    /// <param name="projectType">Type of project</param>
    public VisualBasicRulesAnalysis(List<RootUstNode> sourceFileResults, VisualBasicRootNodes visualBasicRootNodes,
        ProjectType projectType = ProjectType.ClassLibrary)
    {
        _projectActions = new ProjectActions();
        _sourceFileResults = sourceFileResults;
        _visualBasicRootNodes = visualBasicRootNodes;
        _projectType = projectType;
    }

    public ProjectActions Analyze()
    {
        var options = new ParallelOptions { MaxDegreeOfParallelism = Constants.ThreadCount };
        Parallel.ForEach(_sourceFileResults, options, result =>
        {
            var fileAction = new FileActions { FilePath = result.FileFullPath };
            if (AnalyzeChildren(fileAction, result.Children, 0))
            {
                _projectActions.FileActions.Add(fileAction);
            }
        });

        AddPackages(
            _visualBasicRootNodes.ProjectTokens.Where(p => p.FullKey == _projectType.ToString())
                ?.SelectMany(p => p.PackageActions)?.Distinct()?.ToList(), null);

        return _projectActions;
    }

    public ProjectActions AnalyzeFiles(ProjectActions projectActions, List<string> updatedFiles)
    {
        var options = new ParallelOptions { MaxDegreeOfParallelism = Constants.ThreadCount };
        var selectedSourceFileResults = _sourceFileResults.Where(s => updatedFiles.Contains(s.FileFullPath));

        Parallel.ForEach(selectedSourceFileResults, options, result =>
        {
            var fileAction = new FileActions { FilePath = result.FileFullPath };

            if (AnalyzeChildren(fileAction, result.Children, 0))
            {
                var existingFileAction =
                    _projectActions.FileActions.FirstOrDefault(f => f.FilePath == fileAction.FilePath);
                if (existingFileAction != null)
                {
                    existingFileAction = fileAction;
                }
                else
                {
                    _projectActions.FileActions.Add(fileAction);
                }
            }
        });
        return _projectActions;
    }

    /// <summary>
    ///     Analyzes children of nodes in a particular file
    /// </summary>
    /// <param name="fileAction">The object containing the actions to run on the file</param>
    /// <param name="children">List of child nodes to check</param>
    /// <param name="level">Recursion level to avoid stack overflows</param>
    private bool AnalyzeChildren(FileActions fileAction, UstList<UstNode> children, int level,
        string parentNamespace = "", string parentClass = "")
    {
        var containsActions = false;

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
                    case IdConstants.AttributeListName:
                    {
                        var attributeList = (AttributeList)child;
                        var compareToken = new AttributeListToken
                        {
                            Key = attributeList.Identifier,
                            Namespace = attributeList.Reference.Namespace,
                            Type = attributeList.SemanticClassType
                        };
                        _visualBasicRootNodes.AttributeListTokens.TryGetValue(compareToken, out var token);
                        if (token != null)
                        {
                            AddActions(fileAction, token, child.TextSpan);
                            containsActions = true;
                        }

                        break;
                    }
                    case IdConstants.ImportsStatementName:
                    {
                        var overrideKey = string.Empty;

                        var compareToken = new ImportStatementToken { Key = child.Identifier };
                        _visualBasicRootNodes.ImportStatementTokens.TryGetValue(compareToken, out var token);
                        if (token != null)
                        {
                            AddActions(fileAction, token, child.TextSpan);
                            containsActions = true;
                        }

                        //Attempt a wildcard search, if applicable. This is import specific because it might want to include all sub-namespaces
                        if (token == null)
                        {
                            var wildcardMatches = _visualBasicRootNodes.ImportStatementTokens
                                .Where(i => i.Key.Contains("*")).ToList();
                            if (wildcardMatches.Any())
                            {
                                token = wildcardMatches.FirstOrDefault(i => compareToken.Key.WildcardEquals(i.Key));

                                if (token != null)
                                {
                                    //We set the key so that we don't do another wildcard search during replacement, we just use the name as it was declared in the code
                                    overrideKey = compareToken.Key;
                                }
                            }
                        }

                        if (token != null)
                        {
                            AddActions(fileAction, token, child.TextSpan, overrideKey);
                            containsActions = true;
                        }

                        break;
                    }
                    case IdConstants.NamespaceBlockIdName:
                    {
                        var compareToken = new NamespaceToken { Key = child.Identifier };
                        _visualBasicRootNodes.NamespaceTokens.TryGetValue(compareToken, out var token);
                        if (token != null)
                        {
                            AddActions(fileAction, token, child.TextSpan);
                            containsActions = true;
                        }

                        if (AnalyzeChildren(fileAction, child.Children, ++level, child.Identifier))
                        {
                            containsActions = true;
                        }

                        break;
                    }
                    case IdConstants.ModuleBlockName:
                    {
                        var moduleType = (ModuleBlock)child;
                        var name = string.Concat(
                            moduleType.Reference != null
                                ? string.Concat(moduleType.Reference.Namespace, ".")
                                : string.Empty, moduleType.Identifier);
                        var nameToken = new TypeBlockToken { FullKey = name };
                        if (_visualBasicRootNodes.TypeBlockTokens.TryGetValue(nameToken, out var token))
                        {
                            AddNamedActions(fileAction, token, moduleType.Identifier, child.TextSpan);
                            AddActions(fileAction, token, child.TextSpan);
                            containsActions = true;
                        }

                        break;
                    }
                    case IdConstants.ClassBlockName:
                    {
                        var classType = (ClassBlock)child;
                        var baseToken = new TypeBlockToken { FullKey = classType.BaseType };
                        if (_visualBasicRootNodes.TypeBlockTokens.TryGetValue(baseToken, out var token))
                        {
                            AddNamedActions(fileAction, token, classType.Identifier, child.TextSpan);
                            AddActions(fileAction, token, child.TextSpan);
                            containsActions = true;
                        }

                        token = null;
                        var name = string.Concat(
                            classType.Reference != null
                                ? string.Concat(classType.Reference.Namespace, ".")
                                : string.Empty, classType.Identifier);
                        var nameToken = new TypeBlockToken { FullKey = name };
                        if (_visualBasicRootNodes.TypeBlockTokens.TryGetValue(nameToken, out token))
                        {
                            AddNamedActions(fileAction, token, classType.Identifier, child.TextSpan);
                            AddActions(fileAction, token, child.TextSpan);
                            containsActions = true;
                        }

                        token = null;
                        foreach (var interfaceName in classType.Inherits)
                        {
                            var baseListToken = new TypeBlockToken { FullKey = interfaceName };
                            if (_visualBasicRootNodes.TypeBlockTokens.TryGetValue(baseListToken, out token))
                            {
                                AddNamedActions(fileAction, token, classType.Identifier, child.TextSpan);
                                AddActions(fileAction, token, child.TextSpan);
                                containsActions = true;
                            }

                            token = null;
                        }

                        if (AnalyzeChildren(fileAction, child.Children, ++level, parentNamespace, classType.Identifier))
                        {
                            containsActions = true;
                        }

                        break;
                    }
                    case IdConstants.InterfaceBlockIdName:
                    {
                        var interfaceType = (InterfaceBlock)child;
                        var baseToken = new InterfaceBlockToken { FullKey = interfaceType.BaseType };
                        InterfaceBlockToken token = null;

                        if (!string.IsNullOrEmpty(interfaceType.BaseType))
                        {
                            _visualBasicRootNodes.InterfaceBlockTokens.TryGetValue(baseToken, out token);
                        }

                        if (token != null)
                        {
                            //In case of interface blocks, add actions on the interface by name, instead of property                                
                            AddNamedActions(fileAction, token, interfaceType.Identifier, child.TextSpan);
                            AddActions(fileAction, token, child.TextSpan);
                            containsActions = true;
                        }

                        token = null;
                        var name = string.Concat(
                            interfaceType.Reference != null
                                ? string.Concat(interfaceType.Reference.Namespace, ".")
                                : string.Empty, interfaceType.Identifier);
                        var nameToken = new InterfaceBlockToken { FullKey = name };
                        _visualBasicRootNodes.InterfaceBlockTokens.TryGetValue(baseToken, out token);

                        if (token != null)
                        {
                            //In case of interface blocks, add actions on the interface by name, instead of property                                   
                            AddNamedActions(fileAction, token, interfaceType.Identifier, child.TextSpan);
                            AddActions(fileAction, token, child.TextSpan);
                            containsActions = true;
                        }

                        if (AnalyzeChildren(fileAction, child.Children, ++level, parentNamespace))
                        {
                            containsActions = true;
                        }

                        break;
                    }
                    case IdConstants.AccessorBlockName:
                    {
                        var accessorType = (AccessorBlock)child;
                        var name = string.Concat(
                            accessorType.Reference != null
                                ? string.Concat(accessorType.Reference.Namespace, ".")
                                : string.Empty, accessorType.Identifier);
                        var nameToken = new AccessorBlockToken { FullKey = name };
                        if (_visualBasicRootNodes.AccessorBlockTokens.TryGetValue(nameToken, out var token))
                        {
                            AddNamedActions(fileAction, token, accessorType.Identifier, child.TextSpan);
                            AddActions(fileAction, token, child.TextSpan);
                            containsActions = true;
                        }

                        break;
                    }
                    case IdConstants.SubBlockName:
                    {
                        var compareToken = new MethodBlockToken { FullKey = string.Concat(child.Identifier) };
                        _visualBasicRootNodes.MethodBlockTokens.TryGetValue(compareToken, out var token);
                        if (token != null)
                        {
                            AddNamedActions(fileAction, token, child.Identifier, child.TextSpan);
                            AddActions(fileAction, token, child.TextSpan);
                            containsActions = true;
                        }

                        if (AnalyzeChildren(fileAction, child.Children, ++level, parentNamespace, parentClass))
                        {
                            containsActions = true;
                        }

                        break;
                    }
                    case IdConstants.InvocationIdName:
                    {
                        var overrideKey = string.Empty;

                        var invocationExpression = (InvocationExpression)child;

                        ////If we don't have a semantic analysis, we dont want to replace invocation expressions, otherwise we'll be replacing expressions regardless of their class/namespace
                        if (string.IsNullOrEmpty(invocationExpression.SemanticOriginalDefinition))
                        {
                            break;
                        }

                        var compareToken = new InvocationExpressionToken
                        {
                            Key = invocationExpression.SemanticOriginalDefinition,
                            Namespace = invocationExpression.Reference.Namespace,
                            Type = invocationExpression.SemanticClassType
                        };
                        _visualBasicRootNodes.InvocationExpressionTokens.TryGetValue(compareToken, out var token);

                        //Attempt a wildcard search, if applicable. This is invocation expression specific because it has to look inside the invocation expressions only
                        if (token == null)
                        {
                            var wildcardMatches =
                                _visualBasicRootNodes.InvocationExpressionTokens.Where(i => i.Key.Contains("*"));
                            if (wildcardMatches.Any())
                            {
                                token = wildcardMatches.FirstOrDefault(i =>
                                    compareToken.Key.WildcardEquals(i.Key) && compareToken.Namespace == i.Namespace &&
                                    compareToken.Type == i.Type);

                                if (token != null)
                                {
                                    //We set the key so that we don't do another wildcard search during replacement, we just use the name as it was declared in the code
                                    overrideKey = compareToken.Key;
                                }
                            }

                            //If the semanticClassType is too specific to apply to all TData types
                            if (token == null)
                            {
                                if (invocationExpression.SemanticClassType.Contains('<'))
                                {
                                    var semanticClassType = invocationExpression.SemanticClassType.Substring(0,
                                        invocationExpression.SemanticClassType.IndexOf('<'));
                                    compareToken = new InvocationExpressionToken
                                    {
                                        Key = invocationExpression.SemanticOriginalDefinition,
                                        Namespace = invocationExpression.Reference.Namespace,
                                        Type = semanticClassType
                                    };
                                    _visualBasicRootNodes.InvocationExpressionTokens.TryGetValue(compareToken,
                                        out token);
                                }
                            }
                        }

                        if (token != null)
                        {
                            AddActions(fileAction, token, child.TextSpan, overrideKey);
                            containsActions = true;
                        }

                        if (AnalyzeChildren(fileAction, child.Children, ++level, parentNamespace, parentClass))
                        {
                            containsActions = true;
                        }

                        break;
                    }
                    case IdConstants.ElementAccessIdName:
                    {
                        ElementAccess elementAccess = (ElementAccess)child;
                        var compareToken = new ElementAccessToken()
                        {
                            Key = elementAccess.Expression,
                            FullKey = GetFullKey(elementAccess.Reference?.Namespace, elementAccess.SemanticClassType, elementAccess.Expression),
                            Type = elementAccess.SemanticClassType,
                            Namespace = elementAccess.Reference?.Namespace
                        };
                        _visualBasicRootNodes.ElementAccessTokens.TryGetValue(compareToken, out var token);
                        if (token != null)
                        {
                            AddActions(fileAction, token, child.TextSpan);
                            containsActions = true;
                        }
                        if (AnalyzeChildren(fileAction, child.Children, ++level, parentNamespace, parentClass)) { containsActions = true; }
                        break;
                    }

                    case IdConstants.MemberAccessIdName:
                    {
                        var memberAccess = (MemberAccess)child;
                        var compareToken = new MemberAccessToken
                        {
                            Key = memberAccess.Name,
                            FullKey = GetFullKey(memberAccess.Reference?.Namespace, memberAccess.SemanticClassType,
                                memberAccess.Name),
                            Type = memberAccess.SemanticClassType,
                            Namespace = memberAccess.Reference?.Namespace
                        };
                        _visualBasicRootNodes.MemberAccessTokens.TryGetValue(compareToken, out var token);
                        if (token != null)
                        {
                            AddActions(fileAction, token, child.TextSpan);
                            containsActions = true;
                        }

                        if (AnalyzeChildren(fileAction, child.Children, ++level, parentNamespace, parentClass))
                        {
                            containsActions = true;
                        }

                        break;
                    }
                    case IdConstants.DeclarationNodeIdName:
                    {
                        var declarationNode = (DeclarationNode)child;
                        var compareToken = new IdentifierNameToken
                        {
                            Key = string.Concat(declarationNode.Reference.Namespace, ".",
                                declarationNode.Identifier),
                            Namespace = declarationNode.Reference.Namespace
                        };
                        _visualBasicRootNodes.IdentifierNameTokens.TryGetValue(compareToken, out var token);
                        if (token != null)
                        {
                            AddActions(fileAction, token, child.TextSpan);
                            containsActions = true;
                        }

                        if (AnalyzeChildren(fileAction, child.Children, ++level, parentNamespace, parentClass))
                        {
                            containsActions = true;
                        }

                        break;
                    }
                    case IdConstants.ObjectCreationIdName:
                    {
                        var objectCreationNode = (ObjectCreationExpression)child;
                        //Rules based on Object Creation Parent Hierarchy
                        var compareToken = new ObjectCreationExpressionToken
                        {
                            Key = objectCreationNode.Identifier,
                            Namespace = objectCreationNode.Reference?.Namespace,
                            Type = objectCreationNode.SemanticClassType
                        };
                        _visualBasicRootNodes.ObjectCreationExpressionTokens.TryGetValue(compareToken, out var token);
                        if (token != null)
                        {
                            AddActions(fileAction, token, child.TextSpan);
                            containsActions = true;
                        }

                        //Rules based on Object Creation location within code
                        var compareTokenLocation = new ObjectCreationExpressionToken
                        {
                            Key = objectCreationNode.Identifier, Namespace = parentNamespace, Type = parentClass
                        };
                        _visualBasicRootNodes.ObjectCreationExpressionTokens.TryGetValue(compareTokenLocation,
                            out var tokenLocation);
                        if (tokenLocation != null)
                        {
                            AddActions(fileAction, tokenLocation, child.TextSpan);
                            containsActions = true;
                        }

                        token = null;
                        if (!string.IsNullOrEmpty(objectCreationNode.SemanticOriginalDefinition))
                        {
                            var nameToken = new ObjectCreationExpressionToken
                            {
                                Key = objectCreationNode.SemanticOriginalDefinition,
                                Namespace = objectCreationNode.SemanticNamespace,
                                Type = objectCreationNode.SemanticClassType
                            };
                            _visualBasicRootNodes.ObjectCreationExpressionTokens.TryGetValue(nameToken, out token);
                            if (token != null)
                            {
                                AddActions(fileAction, token, child.TextSpan);
                                containsActions = true;
                            }
                        }

                        if (AnalyzeChildren(fileAction, child.Children, ++level, parentNamespace, parentClass))
                        {
                            containsActions = true;
                        }

                        break;
                    }
                    default:
                    {
                        if (AnalyzeChildren(fileAction, child.Children, ++level, parentNamespace, parentClass))
                        {
                            containsActions = true;
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex, "Error loading actions for item {0} of type {1}", child.Identifier,
                    child.NodeType);
            }
        }

        return containsActions;
    }

    private string GetFullKey(string containingNamespace, string containingClass, string key)
    {
        if (string.IsNullOrEmpty(containingNamespace))
        {
            return key;
        }

        if (!string.IsNullOrEmpty(containingClass))
        {
            return $"{containingNamespace}.{containingClass}.{key}";
        }

        return $"{containingNamespace}.{key}";
    }

    /// <summary>
    ///     Add actions matching the token
    /// </summary>
    /// <param name="fileAction">The file to run actions on</param>
    /// <param name="token">The token that matched the file</param>
    private void AddActions(FileActions fileAction, VisualBasicNodeToken token, TextSpan textSpan,
        string overrideKey = "")
    {
        fileAction.VbInvocationExpressionActions.UnionWith(token.InvocationExpressionActions.Select(a =>
            new InvocationExpressionAction<InvocationExpressionSyntax>
            {
                Key = !string.IsNullOrEmpty(overrideKey) ? overrideKey : a.Key,
                Description = a.Description,
                Value = a.Value,
                Name = a.Name,
                Type = a.Type,
                TextSpan = textSpan,
                ActionValidation = a.ActionValidation,
                InvocationExpressionActionFunc = a.InvocationExpressionActionFunc
            }).ToList());

        fileAction.VbImportActions.UnionWith(token.ImportActions.Select(a => new ImportAction
        {
            Key = a.Key,
            Description = a.Description,
            Value = a.Value,
            Name = a.Name,
            Type = a.Type,
            TextSpan = textSpan,
            ActionValidation = a.ActionValidation,
            ImportActionFunc = a.ImportActionFunc,
            ImportsClauseActionFunc = a.ImportsClauseActionFunc
        }).ToList());

        fileAction.VbNamespaceActions.UnionWith(token.NamespaceActions.Select(a =>
            new NamespaceAction<NamespaceBlockSyntax>
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

        fileAction.VbIdentifierNameActions.UnionWith(token.IdentifierNameActions.Select(a => new IdentifierNameAction<IdentifierNameSyntax>()
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
        
        fileAction.VbAttributeActions.UnionWith(token.AttributeActions.Select(a => new AttributeAction()
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
        
        fileAction.VbAttributeListActions.UnionWith(token.VbAttributeListActions.Select(a => new AttributeListAction
        {
            Key = a.Key,
            Description = a.Description,
            Value = a.Value,
            Name = a.Name,
            Type = a.Type,
            TextSpan = textSpan,
            ActionValidation = a.ActionValidation,
            AttributeListActionFunc = a.AttributeListActionFunc
        }).ToList());

        fileAction.VbElementAccessActions.UnionWith(token.ElementAccessActions.Select(a => new ElementAccessAction()
        {
            Key = (token is ElementAccessToken) ? token.FullKey : a.Key,
            Description = a.Description,
            Value = a.Value,
            Name = a.Name,
            Type = a.Type,
            TextSpan = textSpan,
            ActionValidation = a.ActionValidation,
            ElementAccessExpressionActionFunc = a.ElementAccessExpressionActionFunc
        }).ToList());
        
        fileAction.MemberAccessActions.UnionWith(token.MemberAccessActions.Select(a => new MemberAccessAction
        {
            Key = token is MemberAccessToken ? token.FullKey : a.Key,
            Description = a.Description,
            Value = a.Value,
            Name = a.Name,
            Type = a.Type,
            TextSpan = textSpan,
            ActionValidation = a.ActionValidation,
            MemberAccessActionFunc = a.MemberAccessActionFunc
        }).ToList());

        fileAction.VbObjectCreationExpressionActions.UnionWith(token.ObjectCreationExpressionActions.Select(a =>
            new ObjectCreationExpressionAction
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
        
        fileAction.ExpressionActions.UnionWith(token.ExpressionActions.Select(a => new ExpressionAction()
        {
            Key = !string.IsNullOrEmpty(overrideKey) ? overrideKey : a.Key,
            Description = a.Description,
            Value = a.Value,
            Name = a.Name,
            Type = a.Type,
            TextSpan = textSpan,
            ActionValidation = a.ActionValidation,
            ExpressionActionFunc = a.ExpressionActionFunc
        }).ToList());

        if (fileAction.InvocationExpressionActions.Any()
            || fileAction.VbImportActions.Any()
            || fileAction.VbNamespaceActions.Any()
            || fileAction.VbIdentifierNameActions.Any()
            || fileAction.VbAttributeListActions.Any()
            || fileAction.MemberAccessActions.Any()
            || fileAction.VbNamespaceActions.Any()
            || fileAction.VbIdentifierNameActions.Any()
            || fileAction.VbObjectCreationExpressionActions.Any()
            || fileAction.VbElementAccessActions.Any()
            || fileAction.ExpressionActions.Any())
        {
            var nodeToken = token.Clone();
            nodeToken.TextSpan = textSpan;
            fileAction.VbNodeTokens.Add(nodeToken);
        }

        AddPackages(token.PackageActions, textSpan);
    }

    /// <summary>
    ///     Adds a list of packages to the project actions
    /// </summary>
    /// <param name="packageActions">List of package actions based on the rules</param>
    private void AddPackages(List<PackageAction> packageActions, TextSpan textSpan)
    {
        if (packageActions != null && packageActions.Count > 0)
        {
            packageActions.ForEach(p =>
            {
                if (!_projectActions.PackageActions.Contains(p))
                {
                    _projectActions.PackageActions.Add(new PackageAction
                    {
                        Name = p.Name, Version = p.Version, TextSpan = textSpan
                    });
                }
            });
        }
    }

    /// <summary>
    ///     Add actions using the identifier of the object matching the token
    /// </summary>
    /// <param name="fileAction"></param>
    /// <param name="token"></param>
    /// <param name="identifier"></param>
    private void AddNamedActions(FileActions fileAction, VisualBasicNodeToken token, string identifier,
        TextSpan textSpan)
    {
        fileAction.VbTypeBlockActions.UnionWith(token.TypeBlockActions
            .Select(c => new TypeBlockAction
            {
                Key = identifier,
                Value = c.Value,
                Description = c.Description,
                Name = c.Name,
                Type = c.Type,
                TextSpan = textSpan,
                ActionValidation = c.ActionValidation,
                TypeBlockActionFunc = c.TypeBlockActionFunc
            }));

        fileAction.VbMethodBlockActions.UnionWith(token.MethodBlockActions
            .Select(c => new MethodBlockAction
            {
                Key = identifier,
                Value = c.Value,
                Description = c.Description,
                Name = c.Name,
                Type = c.Type,
                TextSpan = textSpan,
                ActionValidation = c.ActionValidation,
                MethodBlockActionFunc = c.MethodBlockActionFunc
            }));

        fileAction.VbInterfaceBlockActions.UnionWith(token.InterfaceBlockActions
            .Select(c => new InterfaceBlockAction
            {
                Key = identifier,
                Value = c.Value,
                Name = c.Name,
                Type = c.Type,
                Description = c.Description,
                TextSpan = textSpan,
                ActionValidation = c.ActionValidation,
                InterfaceBlockActionFunc = c.InterfaceBlockActionFunc
            }));

        fileAction.VbAccessorBlockActions.UnionWith(token.AccessorBlockActions
            .Select(c => new AccessorBlockAction
            {
                Key = identifier,
                Value = c.Value,
                Name = c.Name,
                Type = c.Type,
                Description = c.Description,
                TextSpan = textSpan,
                ActionValidation = c.ActionValidation,
                AccessorBlockActionFunc = c.AccessorBlockActionFunc
            }));

        if (fileAction.VbTypeBlockActions.Any() || fileAction.VbMethodBlockActions.Any()
                                                || fileAction.VbInterfaceBlockActions.Any() ||
                                                fileAction.VbAccessorBlockActions.Any())
        {
            var nodeToken = token.Clone();
            nodeToken.TextSpan = textSpan;
            nodeToken.AllActions.ForEach(action =>
            {
                action.Key = identifier;
            });
            fileAction.VbNodeTokens.Add(nodeToken);
        }
    }
}
