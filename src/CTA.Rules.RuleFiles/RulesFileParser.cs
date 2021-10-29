using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CTA.Rules.Actions;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.Models.Tokens;
using Newtonsoft.Json;

namespace CTA.Rules.RuleFiles
{
    /// <summary>
    /// Parser to load rules in form usable by the rules engine
    /// </summary>
    public class RulesFileParser
    {
        private readonly RootNodes _rootNodes;
        private readonly string _assembliesDir;
        private readonly string _targetFramework;

        private ActionsLoader actionsLoader;
        private readonly Rootobject _rulesObject;
        private readonly Rootobject _overrideObject;

        private readonly NamespaceRecommendations _namespaceRecommendations;
        private readonly NamespaceRecommendations _overrideNamespaceRecommendations;

        /// <summary>
        /// Runs the rules parser
        /// </summary>
        /// <param name="rulesObject">Object containing built in rules</param>
        /// <param name="overrideObject">Object containing override rules</param>
        /// <param name="assembliesDir">Directory containing additional actions assemblies</param>
        public RulesFileParser(NamespaceRecommendations namespaceRecommendations, NamespaceRecommendations overrideNamespaceRecommendations,
            Rootobject rulesObject, Rootobject overrideObject, string assembliesDir, string targetFramework)
        {
            _rootNodes = new RootNodes();
            _rootNodes.ProjectTokens.Add(new ProjectToken() { Key = "Project" });
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
        public RootNodes Process()
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

            return _rootNodes;
        }

        /// <summary>
        /// Loads actions from the actions project and additional assemblies
        /// </summary>
        private void LoadActions()
        {
            List<string> assemblies = new List<string>();
            if (!string.IsNullOrEmpty(_assembliesDir))
            {
                assemblies = Directory.EnumerateFiles(_assembliesDir, "*.dll").ToList();
            }
            actionsLoader = new ActionsLoader(assemblies);
        }

        /// <summary>
        /// Processes each rule object by creating tokens and associated actions
        /// </summary>
        /// <param name="rootobject">An object containing tokens and actions to run on these tokens</param>
        private void ProcessObject(Rootobject rootobject)
        {
            var namespaces = rootobject.NameSpaces;

            foreach (var @namespace in namespaces)
            {
                if (@namespace.Actions != null && @namespace.Actions.Count > 0)
                {
                    //Global Actions:
                    if (@namespace.@namespace == Constants.Project && @namespace.Assembly == Constants.Project)
                    {
                        var projectToken = _rootNodes.ProjectTokens.FirstOrDefault();
                        ParseActions(projectToken, @namespace.Actions);
                    }
                    //Namespace specific actions:
                    else
                    {
                        var usingToken = new UsingDirectiveToken() { Key = @namespace.@namespace };
                        var namespaceToken = new NamespaceToken() { Key = @namespace.@namespace };

                        if (!_rootNodes.Usingdirectivetokens.Contains(usingToken)) { _rootNodes.Usingdirectivetokens.Add(usingToken); }
                        if (!_rootNodes.NamespaceTokens.Contains(namespaceToken)) { _rootNodes.NamespaceTokens.Add(namespaceToken); }

                        ParseActions(usingToken, @namespace.Actions);
                        ParseActions(namespaceToken, @namespace.Actions);
                    }
                }
                foreach (var @class in @namespace.Classes)
                {
                    if (@class.Actions != null && @class.Actions.Count > 0)
                    {
                        if (@class.KeyType == CTA.Rules.Config.Constants.BaseClass || @class.KeyType == CTA.Rules.Config.Constants.ClassName)
                        {
                            var token = new ClassDeclarationToken() { Key = @class.FullKey, FullKey = @class.FullKey, Namespace = @namespace.@namespace };
                            if (!_rootNodes.Classdeclarationtokens.Contains(token)) { _rootNodes.Classdeclarationtokens.Add(token); }
                            ParseActions(token, @class.Actions);
                        }
                        else if (@class.KeyType == CTA.Rules.Config.Constants.Identifier)
                        {
                            var token = new IdentifierNameToken() { Key = @class.FullKey, FullKey = @class.FullKey, Namespace = @namespace.@namespace };
                            if (!_rootNodes.Identifiernametokens.Contains(token)) { _rootNodes.Identifiernametokens.Add(token); }
                            ParseActions(token, @class.Actions);
                        }
                    }
                    foreach (var attribute in @class.Attributes)
                    {
                        if (attribute.Actions != null && attribute.Actions.Count > 0)
                        {
                            var token = new AttributeToken() { Key = attribute.Key, Namespace = @namespace.@namespace, FullKey = attribute.FullKey, Type = @class.Key };
                            if (!_rootNodes.Attributetokens.Contains(token)) { _rootNodes.Attributetokens.Add(token); }
                            ParseActions(token, attribute.Actions);
                        }
                    }

                    foreach (var method in @class.Methods)
                    {
                        if (method.Actions != null && method.Actions.Count > 0)
                        {
                            var token = new InvocationExpressionToken() { Key = method.Key, Namespace = @namespace.@namespace, FullKey = method.FullKey, Type = @class.Key };
                            if (!_rootNodes.Invocationexpressiontokens.Contains(token)) { _rootNodes.Invocationexpressiontokens.Add(token); }
                            ParseActions(token, method.Actions);
                        }
                    }

                    foreach (var objectCreation in @class.ObjectCreations)
                    {
                        if (objectCreation.Actions != null && objectCreation.Actions.Count > 0)
                        {
                            var token = new ObjectCreationExpressionToken() { Key = objectCreation.Key, Namespace = @namespace.@namespace, FullKey = objectCreation.FullKey, Type = @class.Key };
                            if (!_rootNodes.ObjectCreationExpressionTokens.Contains(token)) { _rootNodes.ObjectCreationExpressionTokens.Add(token); }
                            ParseActions(token, objectCreation.Actions);
                        }
                    }
                }

                foreach (var @interface in @namespace.Interfaces)
                {
                    if (@interface.Actions != null && @interface.Actions.Count > 0)
                    {
                        if (@interface.KeyType == CTA.Rules.Config.Constants.BaseClass || @interface.KeyType == CTA.Rules.Config.Constants.InterfaceName)
                        {
                            var token = new InterfaceDeclarationToken() { Key = @interface.FullKey, FullKey = @interface.FullKey, Namespace = @namespace.@namespace };
                            if (!_rootNodes.InterfaceDeclarationTokens.Contains(token)) { _rootNodes.InterfaceDeclarationTokens.Add(token); }
                            ParseActions(token, @interface.Actions);
                        }
                        else if (@interface.KeyType == CTA.Rules.Config.Constants.Identifier)
                        {
                            var token = new IdentifierNameToken() { Key = @interface.FullKey, FullKey = @interface.FullKey, Namespace = @namespace.@namespace };
                            if (!_rootNodes.Identifiernametokens.Contains(token)) { _rootNodes.Identifiernametokens.Add(token); }
                            ParseActions(token, @interface.Actions);
                        }
                    }
                    foreach (var attribute in @interface.Attributes)
                    {
                        if (attribute.Actions != null && attribute.Actions.Count > 0)
                        {
                            var token = new AttributeToken() { Key = attribute.Key, Namespace = @namespace.@namespace, FullKey = attribute.FullKey, Type = @interface.Key };
                            if (!_rootNodes.Attributetokens.Contains(token)) { _rootNodes.Attributetokens.Add(token); }
                            ParseActions(token, attribute.Actions);
                        }
                    }

                    foreach (var method in @interface.Methods)
                    {
                        if (method.Actions != null && method.Actions.Count > 0)
                        {
                            var token = new InvocationExpressionToken() { Key = method.Key, Namespace = @namespace.@namespace, FullKey = method.FullKey, Type = @interface.Key };
                            if (!_rootNodes.Invocationexpressiontokens.Contains(token)) { _rootNodes.Invocationexpressiontokens.Add(token); }
                            ParseActions(token, method.Actions);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Processes each rule object by creating tokens and associated actions
        /// </summary>
        /// <param name="rootobject">An object containing tokens and actions to run on these tokens</param>
        private void ProcessObject(NamespaceRecommendations namespaceRecommendations)
        {
            var namespaces = namespaceRecommendations.NameSpaces;

            foreach (var @namespace in namespaces)
            {
                foreach (var recommendation in @namespace.Recommendations)
                {
                    RecommendedActions recommendedActions = recommendation.RecommendedActions
                        .Where(ra => ra.Preferred == "Yes" && ra.TargetFrameworks.Any(t => t.Name.Equals(_targetFramework))).FirstOrDefault();

                    //There are recommendations, but none of them are preferred
                    if (recommendedActions == null && recommendation.RecommendedActions.Count > 0)
                    {
                        LogHelper.LogError("No preferred recommendation set for recommendation {0} with target framework {1}", recommendation.Value, _targetFramework);
                        continue;
                    }
                    else if (recommendedActions != null)
                    {
                        if (recommendedActions.Actions != null && recommendedActions.Actions.Count > 0)
                        {
                            var targetCPUs = new List<string> { "x86", "x64", "ARM64" };
                            try
                            {
                                targetCPUs = recommendedActions.TargetFrameworks.FirstOrDefault(t => t.Name == _targetFramework)?.TargetCPU;
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
                                        var usingToken = new UsingDirectiveToken() { Key = recommendation.Value, Description = recommendedActions.Description, TargetCPU = targetCPUs };
                                        var namespaceToken = new NamespaceToken() { Key = recommendation.Value, Description = recommendedActions.Description, TargetCPU = targetCPUs };

                                        if (!_rootNodes.Usingdirectivetokens.Contains(usingToken)) { _rootNodes.Usingdirectivetokens.Add(usingToken); }
                                        if (!_rootNodes.NamespaceTokens.Contains(namespaceToken)) { _rootNodes.NamespaceTokens.Add(namespaceToken); }

                                        ParseActions(usingToken, recommendedActions.Actions);
                                        ParseActions(namespaceToken, recommendedActions.Actions);
                                        break;
                                    }
                                case ActionTypes.Class:
                                    {
                                        if (recommendation.KeyType == CTA.Rules.Config.Constants.BaseClass || recommendation.KeyType == CTA.Rules.Config.Constants.ClassName)
                                        {
                                            var token = new ClassDeclarationToken() { Key = recommendation.Value, Description = recommendedActions.Description, TargetCPU = targetCPUs, FullKey = recommendation.Value, Namespace = @namespace.Name };
                                            if (!_rootNodes.Classdeclarationtokens.Contains(token)) { _rootNodes.Classdeclarationtokens.Add(token); }
                                            ParseActions(token, recommendedActions.Actions);
                                        }
                                        else if (recommendation.KeyType == CTA.Rules.Config.Constants.Identifier)
                                        {
                                            var token = new IdentifierNameToken() { Key = recommendation.Value, Description = recommendedActions.Description, TargetCPU = targetCPUs, FullKey = recommendation.Value, Namespace = @namespace.Name };
                                            if (!_rootNodes.Identifiernametokens.Contains(token)) { _rootNodes.Identifiernametokens.Add(token); }
                                            ParseActions(token, recommendedActions.Actions);
                                        }
                                        break;
                                    }

                                case ActionTypes.Interface:
                                    {
                                        if (recommendation.KeyType == CTA.Rules.Config.Constants.BaseClass || recommendation.KeyType == CTA.Rules.Config.Constants.ClassName)
                                        {
                                            var token = new InterfaceDeclarationToken() { Key = recommendation.Value, Description = recommendedActions.Description, TargetCPU = targetCPUs, FullKey = recommendation.Value, Namespace = @namespace.Name };
                                            if (!_rootNodes.InterfaceDeclarationTokens.Contains(token)) { _rootNodes.InterfaceDeclarationTokens.Add(token); }
                                            ParseActions(token, recommendedActions.Actions);
                                        }
                                        else if (recommendation.KeyType == CTA.Rules.Config.Constants.Identifier)
                                        {
                                            var token = new IdentifierNameToken() { Key = recommendation.Value, Description = recommendedActions.Description, TargetCPU = targetCPUs, FullKey = recommendation.Value, Namespace = @namespace.Name };
                                            if (!_rootNodes.Identifiernametokens.Contains(token)) { _rootNodes.Identifiernametokens.Add(token); }
                                            ParseActions(token, recommendedActions.Actions);
                                        }
                                        break;
                                    }

                                case ActionTypes.Method:
                                    {
                                        var token = new InvocationExpressionToken() { Key = recommendation.Name, Description = recommendedActions.Description, TargetCPU = targetCPUs, Namespace = @namespace.Name, FullKey = recommendation.Value, Type = recommendation.ContainingType };
                                        if (!_rootNodes.Invocationexpressiontokens.Contains(token)) { _rootNodes.Invocationexpressiontokens.Add(token); }
                                        ParseActions(token, recommendedActions.Actions);
                                        break;
                                    }
                                case ActionTypes.Expression:
                                    {
                                        var token = new ExpressionToken() { Key = recommendation.Name, Description = recommendedActions.Description, TargetCPU = targetCPUs, Namespace = @namespace.Name, FullKey = recommendation.Value, Type = recommendation.ContainingType };
                                        if (!_rootNodes.Expressiontokens.Contains(token)) { _rootNodes.Expressiontokens.Add(token); }
                                        ParseActions(token, recommendedActions.Actions);
                                        break;
                                    }
                                case ActionTypes.Attribute:
                                    {
                                        var token = new AttributeToken() { Key = recommendation.Name, Description = recommendedActions.Description, TargetCPU = targetCPUs, Namespace = @namespace.Name, FullKey = recommendation.Value, Type = recommendation.ContainingType };
                                        if (!_rootNodes.Attributetokens.Contains(token)) { _rootNodes.Attributetokens.Add(token); }
                                        ParseActions(token, recommendedActions.Actions);
                                        break;
                                    }

                                case ActionTypes.ObjectCreation:
                                    {
                                        var token = new ObjectCreationExpressionToken() { Key = recommendation.Name, Description = recommendedActions.Description, TargetCPU = targetCPUs, Namespace = @namespace.Name, FullKey = recommendation.Value, Type = recommendation.ContainingType };
                                        if (!_rootNodes.ObjectCreationExpressionTokens.Contains(token)) { _rootNodes.ObjectCreationExpressionTokens.Add(token); }
                                        ParseActions(token, recommendedActions.Actions);
                                        break;
                                    }

                                case ActionTypes.MethodDeclaration:
                                    {
                                        var token = new MethodDeclarationToken() { Key = recommendation.Name, Description = recommendedActions.Description, TargetCPU = targetCPUs, Namespace = @namespace.Name, FullKey = recommendation.Value, Type = recommendation.ContainingType };
                                        if (!_rootNodes.MethodDeclarationTokens.Contains(token)) { _rootNodes.MethodDeclarationTokens.Add(token); }
                                        ParseActions(token, recommendedActions.Actions);
                                        break;
                                    }

                                case ActionTypes.ElementAccess:
                                    {
                                        var token = new ElementAccessToken() { Key = recommendation.Name, Description = recommendedActions.Description, TargetCPU = targetCPUs, Namespace = @namespace.Name, FullKey = recommendation.Value, Type = recommendation.ContainingType };
                                        if (!_rootNodes.ElementAccesstokens.Contains(token)) { _rootNodes.ElementAccesstokens.Add(token); }
                                        ParseActions(token, recommendedActions.Actions);
                                        break;
                                    }

                                case ActionTypes.MemberAccess:
                                    {
                                        var token = new MemberAccessToken() { Key = recommendation.Name, Description = recommendedActions.Description, TargetCPU = targetCPUs, Namespace = @namespace.Name, FullKey = recommendation.Value, Type = recommendation.ContainingType };
                                        if (!_rootNodes.MemberAccesstokens.Contains(token)) { _rootNodes.MemberAccesstokens.Add(token); }
                                        ParseActions(token, recommendedActions.Actions);
                                        break;
                                    }

                                case ActionTypes.Project:
                                    {
                                        var token = new ProjectToken() { Key = recommendation.Name, Description = recommendedActions.Description, TargetCPU = targetCPUs, Namespace = @namespace.Name, FullKey = recommendation.Value };
                                        if (!_rootNodes.ProjectTokens.Contains(token)) { _rootNodes.ProjectTokens.Add(token); }
                                        ParseActions(token, recommendedActions.Actions);
                                        break;
                                    }

                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add actions to each node type
        /// </summary>
        /// <param name="nodeToken">The token to add the action to</param>
        /// <param name="actions">The list of actions associated with this token</param>
        private void ParseActions(NodeToken nodeToken, List<Action> actions)
        {
            foreach (var action in actions)
            {
                try
                {
                    var actionType = Enum.Parse(typeof(ActionTypes), action.Type);
                    switch (actionType)
                    {
                        case ActionTypes.Method:
                            {
                                var actionFunc = actionsLoader.GetInvocationExpressionAction(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    nodeToken.InvocationExpressionActions.Add(new InvocationExpressionAction()
                                    {
                                        Key = nodeToken.Key,
                                        Value = GetActionValue(action.Value),
                                        Description = action.Description,
                                        ActionValidation = action.ActionValidation,
                                        Name = action.Name,
                                        Type = action.Type,
                                        InvocationExpressionActionFunc = actionFunc
                                    });
                                }
                                break;
                            }
                        case ActionTypes.Expression:
                            {
                                var actionFunc = actionsLoader.GetExpressionAction(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    nodeToken.ExpressionActions.Add(new ExpressionAction()
                                    {
                                        Key = nodeToken.Key,
                                        Value = GetActionValue(action.Value),
                                        Description = action.Description,
                                        ActionValidation = action.ActionValidation,
                                        Name = action.Name,
                                        Type = action.Type,
                                        ExpressionActionFunc = actionFunc
                                    });
                                }
                                break;
                            }
                        case ActionTypes.Class:
                            {
                                var actionFunc = actionsLoader.GetClassAction(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    nodeToken.ClassDeclarationActions.Add(new ClassDeclarationAction()
                                    {
                                        Key = nodeToken.Key,
                                        Value = GetActionValue(action.Value),
                                        Description = action.Description,
                                        ActionValidation = action.ActionValidation,
                                        Name = action.Name,
                                        Type = action.Type,
                                        ClassDeclarationActionFunc = actionFunc
                                    });
                                }
                                break;
                            }

                        case ActionTypes.Interface:
                            {
                                var actionFunc = actionsLoader.GetInterfaceAction(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    nodeToken.InterfaceDeclarationActions.Add(new InterfaceDeclarationAction()
                                    {
                                        Key = nodeToken.Key,
                                        Value = GetActionValue(action.Value),
                                        Description = action.Description,
                                        ActionValidation = action.ActionValidation,
                                        Name = action.Name,
                                        Type = action.Type,
                                        InterfaceDeclarationActionFunc = actionFunc
                                    });
                                }
                                break;
                            }
                        case ActionTypes.Using:
                            {
                                var actionFunc = actionsLoader.GetCompilationUnitAction(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    nodeToken.UsingActions.Add(new UsingAction()
                                    {
                                        Key = nodeToken.Key,
                                        Value = GetActionValue(action.Value),
                                        Description = action.Description,
                                        ActionValidation = action.ActionValidation,
                                        Name = action.Name,
                                        Type = action.Type,
                                        UsingActionFunc = actionFunc
                                    });
                                }
                                break;
                            }
                        case ActionTypes.Namespace:
                            {
                                var actionFunc = actionsLoader.GetNamespaceActions(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    nodeToken.NamespaceActions.Add(new NamespaceAction()
                                    {
                                        Key = nodeToken.Key,
                                        Value = GetActionValue(action.Value),
                                        Description = action.Description,
                                        ActionValidation = action.ActionValidation,
                                        Name = action.Name,
                                        Type = action.Type,
                                        NamespaceActionFunc = actionFunc
                                    });
                                }
                                break;
                            }
                        case ActionTypes.Identifier:
                            {
                                var actionFunc = actionsLoader.GetIdentifierNameAction(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    nodeToken.IdentifierNameActions.Add(new IdentifierNameAction()
                                    {
                                        Key = nodeToken.Key,
                                        Value = GetActionValue(action.Value),
                                        Description = action.Description,
                                        ActionValidation = action.ActionValidation,
                                        Name = action.Name,
                                        Type = action.Type,
                                        IdentifierNameActionFunc = actionFunc
                                    });
                                }
                                break;
                            }
                        case ActionTypes.Attribute:
                            {
                                var actionFunc = actionsLoader.GetAttributeAction(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    nodeToken.AttributeActions.Add(new AttributeAction()
                                    {
                                        Key = nodeToken.Key,
                                        Value = GetActionValue(action.Value),
                                        Description = action.Description,
                                        ActionValidation = action.ActionValidation,
                                        Name = action.Name,
                                        Type = action.Type,
                                        AttributeActionFunc = actionFunc
                                    });
                                }
                                break;
                            }
                        case ActionTypes.AttributeList:
                            {
                                var actionFunc = actionsLoader.GetAttributeListAction(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    nodeToken.AttributeListActions.Add(new AttributeAction()
                                    {
                                        Key = nodeToken.Key,
                                        Value = GetActionValue(action.Value),
                                        Description = action.Description,
                                        ActionValidation = action.ActionValidation,
                                        Name = action.Name,
                                        Type = action.Type,
                                        AttributeListActionFunc = actionFunc
                                    });
                                }
                                break;
                            }
                        case ActionTypes.ObjectCreation:
                            {
                                var actionFunc = actionsLoader.GetObjectCreationExpressionActions(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    nodeToken.ObjectCreationExpressionActions.Add(new ObjectCreationExpressionAction()
                                    {
                                        Key = nodeToken.Key,
                                        Value = GetActionValue(action.Value),
                                        Description = action.Description,
                                        ActionValidation = action.ActionValidation,
                                        Name = action.Name,
                                        Type = action.Type,
                                        ObjectCreationExpressionGenericActionFunc = actionFunc
                                    });
                                }
                                break;
                            }
                        case ActionTypes.MethodDeclaration:
                            {
                                var actionFunc = actionsLoader.GetMethodDeclarationAction(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    nodeToken.MethodDeclarationActions.Add(new MethodDeclarationAction()
                                    {
                                        Key = nodeToken.Key,
                                        Value = GetActionValue(action.Value),
                                        Description = action.Description,
                                        ActionValidation = action.ActionValidation,
                                        Name = action.Name,
                                        Type = action.Type,
                                        MethodDeclarationActionFunc = actionFunc
                                    });
                                }
                                break;
                            }
                        case ActionTypes.ElementAccess:
                            {
                                var actionFunc = actionsLoader.GetElementAccessExpressionActions(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    nodeToken.ElementAccessActions.Add(new ElementAccessAction()
                                    {
                                        Key = nodeToken.Key,
                                        Value = GetActionValue(action.Value),
                                        Description = action.Description,
                                        ActionValidation = action.ActionValidation,
                                        Name = action.Name,
                                        Type = action.Type,
                                        ElementAccessExpressionActionFunc = actionFunc
                                    });
                                }
                                break;
                            }
                        case ActionTypes.MemberAccess:
                            {
                                var actionFunc = actionsLoader.GetMemberAccessExpressionActions(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    nodeToken.MemberAccessActions.Add(new MemberAccessAction()
                                    {
                                        Key = nodeToken.Key,
                                        Value = GetActionValue(action.Value),
                                        Description = action.Description,
                                        ActionValidation = action.ActionValidation,
                                        Name = action.Name,
                                        Type = action.Type,
                                        MemberAccessActionFunc = actionFunc
                                    });
                                }
                                break;
                            }
                        case ActionTypes.Project:
                            {
                                var actionFunc = actionsLoader.GetProjectLevelActions(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    nodeToken.ProjectLevelActions.Add(new ProjectLevelAction()
                                    {
                                        Key = nodeToken.Key,
                                        Value = GetActionValue(action.Value),
                                        Description = action.Description,
                                        ActionValidation = action.ActionValidation,
                                        Name = action.Name,
                                        Type = action.Type,
                                        ProjectLevelActionFunc = actionFunc
                                    });
                                }
                                break;
                            }
                        case ActionTypes.ProjectFile:
                            {
                                var actionFunc = actionsLoader.GetProjectFileActions(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    nodeToken.ProjectFileActions.Add(new ProjectLevelAction()
                                    {
                                        Key = nodeToken.Key,
                                        Value = GetActionValue(action.Value),
                                        Description = action.Description,
                                        ActionValidation = action.ActionValidation,
                                        Name = action.Name,
                                        Type = action.Type,
                                        ProjectFileActionFunc = actionFunc
                                    });
                                }
                                break;
                            }
                        case ActionTypes.Package:
                            {
                                PackageAction packageAction = new PackageAction();

                                if (action.Value is string)
                                {
                                    packageAction.Name = action.Value;
                                }
                                else
                                {
                                    Dictionary<string, string> jsonParameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(action.Value.ToString());
                                    if (jsonParameters.ContainsKey(CTA.Rules.Config.Constants.PackageName))
                                    {
                                        packageAction.Name = jsonParameters[CTA.Rules.Config.Constants.PackageName];
                                    }
                                    else
                                    {
                                        LogHelper.LogDebug(string.Format("Parameter {0} is not available for action {1}"
                                            , Config.Constants.PackageName, action.Name));
                                        continue;
                                    }

                                    //TODO: If version is not available/valid, we use latest version. Should we reconsider?
                                    if (jsonParameters.ContainsKey(CTA.Rules.Config.Constants.PackageVersion))
                                    {
                                        packageAction.Version = jsonParameters[CTA.Rules.Config.Constants.PackageVersion];
                                    }
                                }
                                nodeToken.PackageActions.Add(packageAction);
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, string.Format("Error parsing action type {0}", action.Type));
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
