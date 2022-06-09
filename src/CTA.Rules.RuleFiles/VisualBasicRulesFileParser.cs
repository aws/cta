using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CTA.Rules.Actions;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.Models.Actions.VisualBasic;
using CTA.Rules.Models.VisualBasic;
using CTA.Rules.Models.Tokens.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using AttributeToken = CTA.Rules.Models.Tokens.VisualBasic.AttributeToken;
using ElementAccessAction = CTA.Rules.Models.Actions.VisualBasic.ElementAccessAction;
using ElementAccessToken = CTA.Rules.Models.Tokens.VisualBasic.ElementAccessToken;
using IdentifierNameToken = CTA.Rules.Models.VisualBasic.IdentifierNameToken;
using MemberAccessToken = CTA.Rules.Models.Tokens.VisualBasic.MemberAccessToken;
using ObjectCreationExpressionAction = CTA.Rules.Models.Actions.VisualBasic.ObjectCreationExpressionAction;


namespace CTA.Rules.RuleFiles
{
    /// <summary>
    /// Parser to load rules in form usable by the rules engine
    /// </summary>
    public class VisualBasicRulesFileParser
    {
        private readonly VisualBasicRootNodes _visualBasicRootNodes;
        private readonly string _assembliesDir;
        private readonly string _targetFramework;

        private VisualBasicActionsLoader _actionsLoader;
        private readonly Rootobject _rulesObject;
        private readonly Rootobject _overrideObject;

        private readonly NamespaceRecommendations _namespaceRecommendations;
        private readonly NamespaceRecommendations _overrideNamespaceRecommendations;

        /// <summary>
        /// Runs the rules parser
        /// </summary>
        /// <param name="overrideNamespaceRecommendations">Override namespace recommendations</param>
        /// <param name="rulesObject">Object containing built in rules</param>
        /// <param name="overrideObject">Object containing override rules</param>
        /// <param name="assembliesDir">Directory containing additional actions assemblies</param>
        /// <param name="namespaceRecommendations">Namespace recommendations</param>
        /// <param name="targetFramework">Framework version being targeted for porting</param>
        /// 
        public VisualBasicRulesFileParser(
            NamespaceRecommendations namespaceRecommendations,
            NamespaceRecommendations overrideNamespaceRecommendations,
            Rootobject rulesObject,
            Rootobject overrideObject,
            string assembliesDir,
            string targetFramework)
        {
            _visualBasicRootNodes = new VisualBasicRootNodes();
            _visualBasicRootNodes.ProjectTokens.Add(new ProjectToken() { Key = "Project" });
            _rulesObject = rulesObject;
            _overrideObject = overrideObject;
            _assembliesDir = assembliesDir;
            _namespaceRecommendations = namespaceRecommendations;
            _overrideNamespaceRecommendations = overrideNamespaceRecommendations;
            _targetFramework = targetFramework;

            LoadActions();
        }

        /// <summary>
        /// Runs the parser to merge the rules
        /// </summary>
        /// <returns>RootNodes object that contains the tokens and their associated actions</returns>
        public VisualBasicRootNodes Process()
        {
            //Process overrides first:
            if (_overrideObject.NameSpaces != null)
            {
                ProcessObject(_overrideObject);
            }

            //Add remaining objects, if not available:
            if (_overrideNamespaceRecommendations.NameSpaces != null)
            {
                ProcessObject(_overrideNamespaceRecommendations);
            }

            //Add remaining objects, if not available:
            if (_rulesObject.NameSpaces != null)
            {
                ProcessObject(_rulesObject);
            }


            //Add remaining objects, if not available:
            if (_namespaceRecommendations.NameSpaces != null)
            {
                ProcessObject(_namespaceRecommendations);
            }

            return _visualBasicRootNodes;
        }

        /// <summary>
        /// Loads actions from the actions project and additional assemblies
        /// </summary>
        public void LoadActions()
        {
            List<string> assemblies = new List<string>();
            if (!string.IsNullOrEmpty(_assembliesDir))
            {
                assemblies = Directory.EnumerateFiles(_assembliesDir, "*.dll").ToList();
            }
            _actionsLoader = new VisualBasicActionsLoader(assemblies);
        }

        /// <summary>
        /// Processes each rule object by creating tokens and associated actions
        /// </summary>
        /// <param name="rootobject">An object containing tokens and actions to run on these tokens</param>
        public void ProcessObject(Rootobject rootobject)
        {
            var namespaces = rootobject.NameSpaces;

            foreach (var @namespace in namespaces)
            {
                if (@namespace.Actions != null && @namespace.Actions.Count > 0)
                {
                    //Global Actions:
                    if (@namespace.@namespace == Constants.Project && @namespace.Assembly == Constants.Project)
                    {
                        var projectToken = _visualBasicRootNodes.ProjectTokens.FirstOrDefault();
                        ParseActions((VisualBasicNodeToken)projectToken, @namespace.Actions);
                    }
                    //Namespace specific actions:
                    else
                    {
                        var usingToken = new ImportStatementToken() { Key = @namespace.@namespace };
                        var namespaceToken = new NamespaceToken() { Key = @namespace.@namespace };

                        if (!_visualBasicRootNodes.ImportStatementTokens.Contains(usingToken))
                        {
                            _visualBasicRootNodes.ImportStatementTokens.Add(usingToken);
                        }

                        if (!_visualBasicRootNodes.NamespaceTokens.Contains(namespaceToken))
                        {
                            _visualBasicRootNodes.NamespaceTokens.Add(namespaceToken);
                        }

                        ParseActions(usingToken, @namespace.Actions);
                        ParseActions(namespaceToken, @namespace.Actions);
                    }
                }
                foreach (var @class in @namespace.Classes)
                {
                    if (@class.Actions != null && @class.Actions.Count > 0)
                    {
                        if (@class.KeyType is Constants.BaseClass or Constants.ClassName)
                        {
                            var token = new TypeBlockToken() { Key = @class.FullKey, FullKey = @class.FullKey, Namespace = @namespace.@namespace };
                            if (!_visualBasicRootNodes.TypeBlockTokens.Contains(token)) { _visualBasicRootNodes.TypeBlockTokens.Add(token); }
                            ParseActions(token, @class.Actions);
                        }
                        if (@class.KeyType == CTA.Rules.Config.Constants.Identifier)
                        {
                            var token = new IdentifierNameToken
                            {
                                Key = @class.FullKey, FullKey = @class.FullKey, Namespace = @namespace.@namespace
                            };
                            if (!_visualBasicRootNodes.IdentifierNameTokens.Contains(token))
                            {
                                _visualBasicRootNodes.IdentifierNameTokens.Add(token);
                            }

                            ParseActions(token, @class.Actions);
                        }
                    }
                    
                    foreach (var attribute in @class.Attributes)
                    {
                        if (attribute.Actions != null && attribute.Actions.Count > 0)
                        {
                            var token = new AttributeToken
                            {
                                Key = attribute.Key,
                                Namespace = @namespace.@namespace,
                                FullKey = attribute.FullKey,
                                Type = @class.Key
                            };
                            if (!_visualBasicRootNodes.AttributeTokens.Contains(token))
                            {
                                _visualBasicRootNodes.AttributeTokens.Add(token);
                            }

                            ParseActions(token, attribute.Actions);
                        }
                    }
                    
                    foreach (var objectCreation in @class.ObjectCreations)
                    {
                        if (objectCreation.Actions != null && objectCreation.Actions.Count > 0)
                        {
                            var token = new ObjectCreationExpressionToken
                            {
                                Key = objectCreation.Key,
                                Namespace = @namespace.@namespace,
                                FullKey = objectCreation.FullKey,
                                Type = @class.Key
                            };
                            if (!_visualBasicRootNodes.ObjectCreationExpressionTokens.Contains(token))
                            {
                                _visualBasicRootNodes.ObjectCreationExpressionTokens.Add(token);
                            }

                            ParseActions(token, objectCreation.Actions);
                        }
                    }

                    foreach (var method in @class.Methods)
                    {
                        if (method.Actions != null && method.Actions.Count > 0)
                        {
                            var token = new InvocationExpressionToken() { Key = method.Key, Namespace = @namespace.@namespace, FullKey = method.FullKey, Type = @class.Key };
                            if (!_visualBasicRootNodes.InvocationExpressionTokens.Contains(token)) { _visualBasicRootNodes.InvocationExpressionTokens.Add(token); }
                            ParseActions(token, method.Actions);
                        }
                    }

                }

                foreach (var @interface in @namespace.Interfaces)
                {
                    if (@interface.Actions != null && @interface.Actions.Count > 0)
                    {
                        if (@interface.KeyType == Constants.BaseClass || @interface.KeyType == CTA.Rules.Config.Constants.InterfaceName)
                        {
                        }
                        else if (@interface.KeyType == Constants.Identifier)
                        {
                            var token = new IdentifierNameToken
                            {
                                Key = @interface.FullKey,
                                FullKey = @interface.FullKey,
                                Namespace = @namespace.@namespace
                            };
                            if (!_visualBasicRootNodes.IdentifierNameTokens.Contains(token))
                            {
                                _visualBasicRootNodes.IdentifierNameTokens.Add(token);
                            }

                            ParseActions(token, @interface.Actions);
                        }
                    }
                    foreach (var attribute in @interface.Attributes)
                    {
                        if (attribute.Actions != null && attribute.Actions.Count > 0)
                        {
                            var token = new AttributeToken
                            {
                                Key = attribute.Key,
                                Namespace = @namespace.@namespace,
                                FullKey = attribute.FullKey,
                                Type = @interface.Key
                            };
                            if (!_visualBasicRootNodes.AttributeTokens.Contains(token))
                            {
                                _visualBasicRootNodes.AttributeTokens.Add(token);
                            }

                            ParseActions(token, attribute.Actions);
                        }
                    }

                    foreach (var method in @interface.Methods)
                    {
                        if (method.Actions != null && method.Actions.Count > 0)
                        {
                            var token = new InvocationExpressionToken() { Key = method.Key, Namespace = @namespace.@namespace, FullKey = method.FullKey, Type = @interface.Key };
                            if (!_visualBasicRootNodes.InvocationExpressionTokens.Contains(token)) { _visualBasicRootNodes.InvocationExpressionTokens.Add(token); }
                            ParseActions(token, method.Actions);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Processes each rule object by creating tokens and associated actions
        /// </summary>
        /// <param name="namespaceRecommendations">An object containing tokens and actions to run on these tokens</param>
        private void ProcessObject(NamespaceRecommendations namespaceRecommendations)
        {
            var namespaces = namespaceRecommendations.NameSpaces;

            foreach (var @namespace in namespaces)
            {
                foreach (var recommendation in @namespace.Recommendations)
                {
                    var recommendedActions = recommendation.RecommendedActions.FirstOrDefault(ra =>
                        ra.Preferred == "Yes" && ra.TargetFrameworks.Any(t => t.Name.Equals(_targetFramework)));

                    //There are recommendations, but none of them are preferred
                    if (recommendedActions == null && recommendation.RecommendedActions.Count > 0)
                    {
                        LogHelper.LogError(
                            "No preferred recommendation set for recommendation {0} with target framework {1}",
                            recommendation.Value, _targetFramework);
                        continue;
                    }

                    if (recommendedActions != null)
                    {
                        if (recommendedActions.Actions != null && recommendedActions.Actions.Count > 0)
                        {
                            var targetCPUs = new List<string> { "x86", "x64", "ARM64" };
                            try
                            {
                                targetCPUs = recommendedActions.TargetFrameworks
                                    .FirstOrDefault(t => t.Name == _targetFramework)?.TargetCPU;
                            }
                            catch
                            {
                                LogHelper.LogError("Error parsing CPUs for target framework");
                            }

                            var recommendationType = Enum.Parse(typeof(ActionTypes), recommendation.Type);
                            switch (recommendationType)
                            {
                                case ActionTypes.Namespace:
                                {
                                    var importToken = new ImportStatementToken() { Key = recommendation.Value, Description = recommendedActions.Description, TargetCPU = targetCPUs };
                                    var namespaceToken = new NamespaceToken() { Key = recommendation.Value, Description = recommendedActions.Description, TargetCPU = targetCPUs };

                                    if (!_visualBasicRootNodes.ImportStatementTokens.Contains(importToken)) { _visualBasicRootNodes.ImportStatementTokens.Add(importToken); }
                                    if (!_visualBasicRootNodes.NamespaceTokens.Contains(namespaceToken)) { _visualBasicRootNodes.NamespaceTokens.Add(namespaceToken); }

                                    ParseActions(importToken, recommendedActions.Actions);
                                    ParseActions(namespaceToken, recommendedActions.Actions);
                                    break;
                                }
                                case ActionTypes.Class:
                                {
                                    if (recommendation.KeyType == CTA.Rules.Config.Constants.BaseClass || recommendation.KeyType == CTA.Rules.Config.Constants.ClassName)
                                    {
                                    }
                                    else if (recommendation.KeyType == CTA.Rules.Config.Constants.Identifier)
                                    {
                                        var token = new IdentifierNameToken
                                        {
                                            Key = recommendation.Value,
                                            Description = recommendedActions.Description,
                                            TargetCPU = targetCPUs,
                                            FullKey = recommendation.Value,
                                            Namespace = @namespace.Name
                                        };
                                        if (!_visualBasicRootNodes.IdentifierNameTokens.Contains(token))
                                        {
                                            _visualBasicRootNodes.IdentifierNameTokens.Add(token);
                                        }

                                        ParseActions(token, recommendedActions.Actions);
                                    }
                                    break;
                                }

                                case ActionTypes.Interface:
                                {
                                    if (recommendation.KeyType == CTA.Rules.Config.Constants.BaseClass || recommendation.KeyType == CTA.Rules.Config.Constants.ClassName)
                                    {
                                    }
                                    else if (recommendation.KeyType == CTA.Rules.Config.Constants.Identifier)
                                    {
                                        var token = new IdentifierNameToken
                                        {
                                            Key = recommendation.Value,
                                            Description = recommendedActions.Description,
                                            TargetCPU = targetCPUs,
                                            FullKey = recommendation.Value,
                                            Namespace = @namespace.Name
                                        };
                                        if (!_visualBasicRootNodes.IdentifierNameTokens.Contains(token))
                                        {
                                            _visualBasicRootNodes.IdentifierNameTokens.Add(token);
                                        }

                                        ParseActions(token, recommendedActions.Actions);
                                    }
                                    break;
                                }

                                case ActionTypes.Method:
                                {
                                    var token = new InvocationExpressionToken
                                    {
                                        Key = recommendation.Name,
                                        Description = recommendedActions.Description,
                                        TargetCPU = targetCPUs,
                                        Namespace = @namespace.Name,
                                        FullKey = recommendation.Value,
                                        Type = recommendation.ContainingType
                                    };
                                    if (!_visualBasicRootNodes.InvocationExpressionTokens.Contains(token))
                                    {
                                        _visualBasicRootNodes.InvocationExpressionTokens.Add(token);
                                    }

                                    ParseActions(token, recommendedActions.Actions);
                                    break;
                                }
                                case ActionTypes.Expression:
                                {
                                    var token = new ExpressionToken
                                    {
                                        Key = recommendation.Name,
                                        Description = recommendedActions.Description,
                                        TargetCPU = targetCPUs,
                                        Namespace = @namespace.Name,
                                        FullKey = recommendation.Value,
                                        Type = recommendation.ContainingType
                                    };
                                    if (!_visualBasicRootNodes.ExpressionTokens.Contains(token))
                                    {
                                        _visualBasicRootNodes.ExpressionTokens.Add(token);
                                    }

                                    ParseActions(token, recommendedActions.Actions);
                                    break;
                                }
                                case ActionTypes.Attribute:
                                {
                                    var token = new AttributeToken()
                                    {
                                        Key = recommendation.Name,
                                        Description = recommendedActions.Description,
                                        TargetCPU = targetCPUs,
                                        Namespace = @namespace.Name,
                                        FullKey = recommendation.Value,
                                        Type = recommendation.ContainingType
                                    };
                                    if (!_visualBasicRootNodes.AttributeTokens.Contains(token))
                                    {
                                        _visualBasicRootNodes.AttributeTokens.Add(token);
                                    }

                                    ParseActions(token, recommendedActions.Actions);
                                    break;
                                }

                                case ActionTypes.ObjectCreation:
                                {
                                    var token = new ObjectCreationExpressionToken
                                    {
                                        Key = recommendation.Name,
                                        Description = recommendedActions.Description,
                                        TargetCPU = targetCPUs,
                                        Namespace = @namespace.Name,
                                        FullKey = recommendation.Value,
                                        Type = recommendation.ContainingType
                                    };
                                    if (!_visualBasicRootNodes.ObjectCreationExpressionTokens.Contains(token))
                                    {
                                        _visualBasicRootNodes.ObjectCreationExpressionTokens.Add(token);
                                    }
                                    ParseActions(token, recommendedActions.Actions);
                                    break;
                                }

                                case ActionTypes.MethodDeclaration:
                                {
                                    var token = new MethodBlockToken
                                    {
                                        Key = recommendation.Name,
                                        Description = recommendedActions.Description,
                                        TargetCPU = targetCPUs,
                                        Namespace = @namespace.Name,
                                        FullKey = recommendation.Value,
                                        Type = recommendation.ContainingType
                                    };
                                    if (!_visualBasicRootNodes.MethodBlockTokens.Contains(token))
                                    {
                                        _visualBasicRootNodes.MethodBlockTokens.Add(token);
                                    }

                                    ParseActions(token, recommendedActions.Actions);
                                    break;
                                }

                                case ActionTypes.ElementAccess:
                                {
                                    var token = new ElementAccessToken
                                    {
                                        Key = recommendation.Name,
                                        Description = recommendedActions.Description,
                                        TargetCPU = targetCPUs,
                                        Namespace = @namespace.Name,
                                        FullKey = recommendation.Value,
                                        Type = recommendation.ContainingType
                                    };
                                    if (!_visualBasicRootNodes.ElementAccessTokens.Contains(token))
                                    {
                                        _visualBasicRootNodes.ElementAccessTokens.Add(token);
                                    }

                                    ParseActions(token, recommendedActions.Actions);
                                    break;
                                }

                                case ActionTypes.MemberAccess:
                                {
                                    var token = new MemberAccessToken
                                    {
                                        Key = recommendation.Name,
                                        Description = recommendedActions.Description,
                                        TargetCPU = targetCPUs,
                                        Namespace = @namespace.Name,
                                        FullKey = recommendation.Value,
                                        Type = recommendation.ContainingType
                                    };
                                    if (!_visualBasicRootNodes.MemberAccessTokens.Contains(token))
                                    {
                                        _visualBasicRootNodes.MemberAccessTokens.Add(token);
                                    }

                                    ParseActions(token, recommendedActions.Actions);
                                    break;
                                }

                                case ActionTypes.Project:
                                {
                                    var token = new ProjectToken() { Key = recommendation.Name, Description = recommendedActions.Description, TargetCPU = targetCPUs, Namespace = @namespace.Name, FullKey = recommendation.Value };
                                    if (!_visualBasicRootNodes.ProjectTokens.Contains(token)) { _visualBasicRootNodes.ProjectTokens.Add(token); }
                                    ParseActions(token, recommendedActions.Actions);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add actions to each node type
        /// </summary>
        /// <param name="visualBasicNodeToken">The token to add the action to</param>
        /// <param name="actions">The list of actions associated with this token</param>
        public void ParseActions(VisualBasicNodeToken visualBasicNodeToken, List<Action> actions)
        {
            foreach (var action in actions)
            {
                try
                {
                    var actionType = Enum.Parse(typeof(ActionTypes), action.Type);

                    dynamic value = action.VbValue ?? action.Value;
                    ActionValidation validation = action.VbActionValidation ?? action.ActionValidation;
                    string vbActionName = "Vb" + action.Name;
                    
                    switch (actionType)
                    {
                        case ActionTypes.Method:
                        {
                            var actionFunc = _actionsLoader.GetInvocationExpressionAction(action.Name, value);
                            if (actionFunc != null)
                            {
                                visualBasicNodeToken.InvocationExpressionActions.Add(
                                    new InvocationExpressionAction<InvocationExpressionSyntax>
                                    {
                                        Key = visualBasicNodeToken.Key,
                                        Value = GetActionValue(value),
                                        Description = action.Description,
                                        ActionValidation = validation,
                                        Name = vbActionName,
                                        Type = action.Type,
                                        InvocationExpressionActionFunc = actionFunc
                                    });
                            }

                            break;
                        }
                        case ActionTypes.Expression:
                        {
                            var actionFunc = _actionsLoader.GetExpressionAction(action.Name, value);
                            if (actionFunc != null)
                            {
                                visualBasicNodeToken.ExpressionActions.Add(new ExpressionAction()
                                {
                                    Key = visualBasicNodeToken.Key,
                                    Value = GetActionValue(value),
                                    Description = action.Description,
                                    ActionValidation = action.ActionValidation,
                                    Name = vbActionName,
                                    Type = action.Type,
                                    ExpressionActionFunc = actionFunc
                                });
                            }
                            break;
                        }
                        case ActionTypes.Class:
                        {
                            var actionFunc = _actionsLoader.GetClassAction(action.Name, value);
                            if (actionFunc != null)
                            {
                                visualBasicNodeToken.TypeBlockActions.Add(new TypeBlockAction()
                                {
                                    Key = visualBasicNodeToken.Key,
                                    Value = GetActionValue(value),
                                    Description = action.Description,
                                    ActionValidation = validation,
                                    Name = vbActionName,
                                    Type = action.Type,
                                    TypeBlockActionFunc = actionFunc
                                });
                            }
                            break;
                        }
                        case ActionTypes.Interface:
                        {
                            var actionFunc = _actionsLoader.GetInterfaceAction(action.Name, value);
                            if (actionFunc != null)
                            {
                                visualBasicNodeToken.InterfaceBlockActions.Add(new InterfaceBlockAction()
                                {
                                    Key = visualBasicNodeToken.Key,
                                    Value = GetActionValue(value),
                                    Description = action.Description,
                                    ActionValidation = action.ActionValidation,
                                    Name = vbActionName,
                                    Type = action.Type,
                                    InterfaceBlockActionFunc = actionFunc
                                });
                            }
                            break;
                        }
                        case ActionTypes.Using:
                        {
                            var actionFunc = _actionsLoader.GetCompilationUnitAction(action.Name, value);
                            // Using directives can be found in both ComplilationUnit and inside Namespace.
                            // Need to make sure remove action is taken if it's inside Namespace block.
                            // Only add using directives in the CompilationUnit as our convention, so it's not added twice.
                            var namespaceActionFunc = _actionsLoader.GetNamespaceActions(action.Name, value);
                            if (actionFunc != null)
                            {
                                visualBasicNodeToken.ImportActions.Add(new ImportAction()
                                {
                                    Key = visualBasicNodeToken.Key,
                                    Value = GetActionValue(value),
                                    Description = action.Description,
                                    ActionValidation = validation,
                                    Name = vbActionName,
                                    Type = action.Type,
                                    ImportActionFunc = actionFunc,
                                    ImportsClauseActionFunc = namespaceActionFunc
                                });
                            }
                            break;
                        }
                        case ActionTypes.Namespace:
                        {
                            var actionFunc = _actionsLoader.GetNamespaceActions(action.Name, value);
                            if (actionFunc != null)
                            {
                                visualBasicNodeToken.NamespaceActions.Add(new NamespaceAction<NamespaceBlockSyntax>()
                                {
                                    Key = visualBasicNodeToken.Key,
                                    Value = GetActionValue(value),
                                    Description = action.Description,
                                    ActionValidation = validation,
                                    Name = vbActionName,
                                    Type = action.Type,
                                    NamespaceActionFunc = actionFunc
                                });
                            }
                            break;
                        }
                        case ActionTypes.Identifier:
                        {
                            var actionFunc = _actionsLoader.GetIdentifierNameAction(action.Name, value);
                            if (actionFunc != null)
                            {
                                visualBasicNodeToken.IdentifierNameActions.Add(new IdentifierNameAction<IdentifierNameSyntax>()
                                {
                                    Key = visualBasicNodeToken.Key,
                                    Value = GetActionValue(value),
                                    Description = action.Description,
                                    ActionValidation = validation,
                                    Name = vbActionName,
                                    Type = action.Type,
                                    IdentifierNameActionFunc = actionFunc
                                });
                            }
                            break;
                        }
                        case ActionTypes.Attribute:
                        {
                            var actionFunc = _actionsLoader.GetAttributeAction(action.Name, value);
                            if (actionFunc != null)
                            {
                                visualBasicNodeToken.AttributeActions.Add(new Models.Actions.VisualBasic.AttributeAction()
                                {
                                    Key = visualBasicNodeToken.Key,
                                    Value = GetActionValue(value),
                                    Description = action.Description,
                                    ActionValidation = action.ActionValidation,
                                    Name = vbActionName,
                                    Type = action.Type,
                                    AttributeActionFunc = actionFunc
                                });
                            }
                            break;
                        }
                        case ActionTypes.AttributeList:
                        {
                            var actionFunc = _actionsLoader.GetAttributeListAction(action.Name, value);
                            if (actionFunc != null)
                            {
                                visualBasicNodeToken.VbAttributeListActions.Add(new Models.Actions.VisualBasic.AttributeListAction()
                                {
                                    Key = visualBasicNodeToken.Key,
                                    Value = GetActionValue(value),
                                    Description = action.Description,
                                    ActionValidation = action.ActionValidation,
                                    Name = vbActionName,
                                    Type = action.Type,
                                    AttributeListActionFunc = actionFunc
                                });
                            }
                            break;
                        }
                        case ActionTypes.ObjectCreation:
                        {
                            var actionFunc = _actionsLoader.GetObjectCreationExpressionActions(action.Name, value);
                            if (actionFunc != null)
                            {
                                visualBasicNodeToken.ObjectCreationExpressionActions.Add(new ObjectCreationExpressionAction()
                                {
                                    Key = visualBasicNodeToken.Key,
                                    Value = GetActionValue(value),
                                    Description = action.Description,
                                    ActionValidation = validation,
                                    Name = vbActionName,
                                    Type = action.Type,
                                    ObjectCreationExpressionGenericActionFunc = actionFunc
                                });
                            }
                            break;
                        }
                        case ActionTypes.MethodDeclaration:
                        {
                            var actionFunc = _actionsLoader.GetMethodDeclarationAction(action.Name, value);
                            if (actionFunc != null)
                            {
                                visualBasicNodeToken.MethodBlockActions.Add(new MethodBlockAction()
                                {
                                    Key = visualBasicNodeToken.Key,
                                    Value = GetActionValue(value),
                                    Description = action.Description,
                                    ActionValidation = action.ActionValidation,
                                    Name = vbActionName,
                                    Type = action.Type,
                                    MethodBlockActionFunc = actionFunc
                                });
                            }
                            break;
                        }
                        case ActionTypes.ElementAccess:
                        {
                            var actionFunc = _actionsLoader.GetElementAccessExpressionActions(action.Name, value);
                            if (actionFunc != null)
                            {
                                visualBasicNodeToken.ElementAccessActions.Add(new ElementAccessAction()
                                {
                                    Key = visualBasicNodeToken.Key,
                                    Value = GetActionValue(value),
                                    Description = action.Description,
                                    ActionValidation = action.ActionValidation,
                                    Name = vbActionName,
                                    Type = action.Type,
                                    ElementAccessExpressionActionFunc = actionFunc
                                });
                            }
                            break;
                        }
                        case ActionTypes.MemberAccess:
                        {
                            var actionFunc = _actionsLoader.GetMemberAccessExpressionActions(action.Name, value);
                            if (actionFunc != null)
                            {
                                visualBasicNodeToken.MemberAccessActions.Add(new MemberAccessAction()
                                {
                                    Key = visualBasicNodeToken.Key,
                                    Value = GetActionValue(value),
                                    Description = action.Description,
                                    ActionValidation = action.ActionValidation,
                                    Name = vbActionName,
                                    Type = action.Type,
                                    MemberAccessActionFunc = actionFunc
                                });
                            }
                            break;
                        }
                        case ActionTypes.Project:
                        {
                            var actionFunc = _actionsLoader.GetProjectLevelActions(action.Name, value);
                            if (actionFunc != null)
                            {
                                visualBasicNodeToken.ProjectLevelActions.Add(new ProjectLevelAction()
                                {
                                    Key = visualBasicNodeToken.Key,
                                    Value = GetActionValue(value),
                                    Description = action.Description,
                                    ActionValidation = validation,
                                    Name = vbActionName,
                                    Type = action.Type,
                                    ProjectLevelActionFunc = actionFunc
                                });
                            }
                            break;
                        }
                        case ActionTypes.ProjectFile:
                        {
                            var actionFunc = _actionsLoader.GetProjectFileActions(action.Name, value);
                            if (actionFunc != null)
                            {
                                visualBasicNodeToken.ProjectFileActions.Add(new ProjectLevelAction()
                                {
                                    Key = visualBasicNodeToken.Key,
                                    Value = GetActionValue(value),
                                    Description = action.Description,
                                    ActionValidation = action.ActionValidation,
                                    Name = vbActionName,
                                    Type = action.Type,
                                    ProjectFileActionFunc = actionFunc
                                });
                            }
                            break;
                        }
                        case ActionTypes.ProjectType:
                        {
                            var actionFunc = _actionsLoader.GetProjectTypeActions(action.Name, value);
                            if (actionFunc != null)
                            {
                                visualBasicNodeToken.ProjectTypeActions.Add(new ProjectLevelAction()
                                {
                                    Key = visualBasicNodeToken.Key,
                                    Value = GetActionValue(value),
                                    Description = action.Description,
                                    ActionValidation = action.ActionValidation,
                                    Name = vbActionName,
                                    Type = action.Type,
                                    ProjectTypeActionFunc = actionFunc
                                });
                            }
                            break;
                        }
                        case ActionTypes.Package:
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, $"Error parsing action type {action.Type}");
                }
            }
        }

        private string GetActionValue(dynamic value)
        {
            if (value is string)
            {
                return value;
            }
            else
            {
                return value + string.Empty;
            }
        }
    }
}
