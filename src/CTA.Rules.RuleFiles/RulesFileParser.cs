using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CTA.Rules.Actions;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.Models.Tokens;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace CTA.Rules.RuleFiles
{
    /// <summary>
    /// Parser to load rules in form usable by the rules engine
    /// </summary>
    public class RulesFileParser
    {
        private readonly CsharpRootNodes _csharpRootNodes;
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
        /// <param name="overrideNamespaceRecommendations">Override namespace recommendations</param>
        /// <param name="rulesObject">Object containing built in rules</param>
        /// <param name="overrideObject">Object containing override rules</param>
        /// <param name="assembliesDir">Directory containing additional actions assemblies</param>
        /// <param name="namespaceRecommendations">Namespace recommendations</param>
        /// <param name="targetFramework">Framework version being targeted for porting</param>
        /// 
        public RulesFileParser(
            NamespaceRecommendations namespaceRecommendations,
            NamespaceRecommendations overrideNamespaceRecommendations,
            Rootobject rulesObject,
            Rootobject overrideObject,
            string assembliesDir,
            string targetFramework)
        {
            _csharpRootNodes = new CsharpRootNodes();
            _csharpRootNodes.ProjectTokens.Add(new ProjectToken() { Key = "Project" });
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
        public CsharpRootNodes Process()
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

            return _csharpRootNodes;
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
            actionsLoader = new ActionsLoader(assemblies);
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
                        var projectToken = _csharpRootNodes.ProjectTokens.FirstOrDefault();
                        ParseActions((ProjectToken)projectToken, @namespace.Actions);
                    }
                    //Namespace specific actions:
                    else
                    {
                        var usingToken = new UsingDirectiveToken() { Key = @namespace.@namespace };
                        var namespaceToken = new NamespaceToken() { Key = @namespace.@namespace };

                        if (!_csharpRootNodes.Usingdirectivetokens.Contains(usingToken)) { _csharpRootNodes.Usingdirectivetokens.Add(usingToken); }
                        if (!_csharpRootNodes.NamespaceTokens.Contains(namespaceToken)) { _csharpRootNodes.NamespaceTokens.Add(namespaceToken); }

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
                            if (!_csharpRootNodes.Classdeclarationtokens.Contains(token)) { _csharpRootNodes.Classdeclarationtokens.Add(token); }
                            ParseActions(token, @class.Actions);
                        }
                        else if (@class.KeyType == CTA.Rules.Config.Constants.Identifier)
                        {
                            var token = new IdentifierNameToken() { Key = @class.FullKey, FullKey = @class.FullKey, Namespace = @namespace.@namespace };
                            if (!_csharpRootNodes.Identifiernametokens.Contains(token)) { _csharpRootNodes.Identifiernametokens.Add(token); }
                            ParseActions(token, @class.Actions);
                        }
                    }
                    foreach (var attribute in @class.Attributes)
                    {
                        if (attribute.Actions != null && attribute.Actions.Count > 0)
                        {
                            var token = new AttributeToken() { Key = attribute.Key, Namespace = @namespace.@namespace, FullKey = attribute.FullKey, Type = @class.Key };
                            if (!_csharpRootNodes.Attributetokens.Contains(token)) { _csharpRootNodes.Attributetokens.Add(token); }
                            ParseActions(token, attribute.Actions);
                        }
                    }

                    foreach (var method in @class.Methods)
                    {
                        if (method.Actions != null && method.Actions.Count > 0)
                        {
                            var token = new InvocationExpressionToken() { Key = method.Key, Namespace = @namespace.@namespace, FullKey = method.FullKey, Type = @class.Key };
                            if (!_csharpRootNodes.Invocationexpressiontokens.Contains(token)) { _csharpRootNodes.Invocationexpressiontokens.Add(token); }
                            ParseActions(token, method.Actions);
                        }
                    }

                    foreach (var objectCreation in @class.ObjectCreations)
                    {
                        if (objectCreation.Actions != null && objectCreation.Actions.Count > 0)
                        {
                            var token = new ObjectCreationExpressionToken() { Key = objectCreation.Key, Namespace = @namespace.@namespace, FullKey = objectCreation.FullKey, Type = @class.Key };
                            if (!_csharpRootNodes.ObjectCreationExpressionTokens.Contains(token)) { _csharpRootNodes.ObjectCreationExpressionTokens.Add(token); }
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
                            if (!_csharpRootNodes.InterfaceDeclarationTokens.Contains(token)) { _csharpRootNodes.InterfaceDeclarationTokens.Add(token); }
                            ParseActions(token, @interface.Actions);
                        }
                        else if (@interface.KeyType == CTA.Rules.Config.Constants.Identifier)
                        {
                            var token = new IdentifierNameToken() { Key = @interface.FullKey, FullKey = @interface.FullKey, Namespace = @namespace.@namespace };
                            if (!_csharpRootNodes.Identifiernametokens.Contains(token)) { _csharpRootNodes.Identifiernametokens.Add(token); }
                            ParseActions(token, @interface.Actions);
                        }
                    }
                    foreach (var attribute in @interface.Attributes)
                    {
                        if (attribute.Actions != null && attribute.Actions.Count > 0)
                        {
                            var token = new AttributeToken() { Key = attribute.Key, Namespace = @namespace.@namespace, FullKey = attribute.FullKey, Type = @interface.Key };
                            if (!_csharpRootNodes.Attributetokens.Contains(token)) { _csharpRootNodes.Attributetokens.Add(token); }
                            ParseActions(token, attribute.Actions);
                        }
                    }

                    foreach (var method in @interface.Methods)
                    {
                        if (method.Actions != null && method.Actions.Count > 0)
                        {
                            var token = new InvocationExpressionToken() { Key = method.Key, Namespace = @namespace.@namespace, FullKey = method.FullKey, Type = @interface.Key };
                            if (!_csharpRootNodes.Invocationexpressiontokens.Contains(token)) { _csharpRootNodes.Invocationexpressiontokens.Add(token); }
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
        public void ProcessObject(NamespaceRecommendations namespaceRecommendations)
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

                                        if (!_csharpRootNodes.Usingdirectivetokens.Contains(usingToken)) { _csharpRootNodes.Usingdirectivetokens.Add(usingToken); }
                                        if (!_csharpRootNodes.NamespaceTokens.Contains(namespaceToken)) { _csharpRootNodes.NamespaceTokens.Add(namespaceToken); }

                                        ParseActions(usingToken, recommendedActions.Actions);
                                        ParseActions(namespaceToken, recommendedActions.Actions);
                                        break;
                                    }
                                case ActionTypes.Class:
                                    {
                                        if (recommendation.KeyType == CTA.Rules.Config.Constants.BaseClass || recommendation.KeyType == CTA.Rules.Config.Constants.ClassName)
                                        {
                                            var token = new ClassDeclarationToken() { Key = recommendation.Value, Description = recommendedActions.Description, TargetCPU = targetCPUs, FullKey = recommendation.Value, Namespace = @namespace.Name };
                                            if (!_csharpRootNodes.Classdeclarationtokens.Contains(token)) { _csharpRootNodes.Classdeclarationtokens.Add(token); }
                                            ParseActions(token, recommendedActions.Actions);
                                        }
                                        else if (recommendation.KeyType == CTA.Rules.Config.Constants.Identifier)
                                        {
                                            var token = new IdentifierNameToken() { Key = recommendation.Value, Description = recommendedActions.Description, TargetCPU = targetCPUs, FullKey = recommendation.Value, Namespace = @namespace.Name };
                                            if (!_csharpRootNodes.Identifiernametokens.Contains(token)) { _csharpRootNodes.Identifiernametokens.Add(token); }
                                            ParseActions(token, recommendedActions.Actions);
                                        }
                                        break;
                                    }

                                case ActionTypes.Interface:
                                    {
                                        if (recommendation.KeyType == CTA.Rules.Config.Constants.BaseClass || recommendation.KeyType == CTA.Rules.Config.Constants.ClassName)
                                        {
                                            var token = new InterfaceDeclarationToken() { Key = recommendation.Value, Description = recommendedActions.Description, TargetCPU = targetCPUs, FullKey = recommendation.Value, Namespace = @namespace.Name };
                                            if (!_csharpRootNodes.InterfaceDeclarationTokens.Contains(token)) { _csharpRootNodes.InterfaceDeclarationTokens.Add(token); }
                                            ParseActions(token, recommendedActions.Actions);
                                        }
                                        else if (recommendation.KeyType == CTA.Rules.Config.Constants.Identifier)
                                        {
                                            var token = new IdentifierNameToken() { Key = recommendation.Value, Description = recommendedActions.Description, TargetCPU = targetCPUs, FullKey = recommendation.Value, Namespace = @namespace.Name };
                                            if (!_csharpRootNodes.Identifiernametokens.Contains(token)) { _csharpRootNodes.Identifiernametokens.Add(token); }
                                            ParseActions(token, recommendedActions.Actions);
                                        }
                                        break;
                                    }

                                case ActionTypes.Method:
                                    {
                                        var token = new InvocationExpressionToken() { Key = recommendation.Name, Description = recommendedActions.Description, TargetCPU = targetCPUs, Namespace = @namespace.Name, FullKey = recommendation.Value, Type = recommendation.ContainingType };
                                        if (!_csharpRootNodes.Invocationexpressiontokens.Contains(token)) { _csharpRootNodes.Invocationexpressiontokens.Add(token); }
                                        ParseActions(token, recommendedActions.Actions);
                                        break;
                                    }
                                case ActionTypes.Expression:
                                    {
                                        var token = new ExpressionToken() { Key = recommendation.Name, Description = recommendedActions.Description, TargetCPU = targetCPUs, Namespace = @namespace.Name, FullKey = recommendation.Value, Type = recommendation.ContainingType };
                                        if (!_csharpRootNodes.Expressiontokens.Contains(token)) { _csharpRootNodes.Expressiontokens.Add(token); }
                                        ParseActions(token, recommendedActions.Actions);
                                        break;
                                    }
                                case ActionTypes.Attribute:
                                    {
                                        var token = new AttributeToken() { Key = recommendation.Name, Description = recommendedActions.Description, TargetCPU = targetCPUs, Namespace = @namespace.Name, FullKey = recommendation.Value, Type = recommendation.ContainingType };
                                        if (!_csharpRootNodes.Attributetokens.Contains(token)) { _csharpRootNodes.Attributetokens.Add(token); }
                                        ParseActions(token, recommendedActions.Actions);
                                        break;
                                    }

                                case ActionTypes.ObjectCreation:
                                    {
                                        var token = new ObjectCreationExpressionToken() { Key = recommendation.Name, Description = recommendedActions.Description, TargetCPU = targetCPUs, Namespace = @namespace.Name, FullKey = recommendation.Value, Type = recommendation.ContainingType };
                                        if (!_csharpRootNodes.ObjectCreationExpressionTokens.Contains(token)) { _csharpRootNodes.ObjectCreationExpressionTokens.Add(token); }
                                        ParseActions(token, recommendedActions.Actions);
                                        break;
                                    }

                                case ActionTypes.MethodDeclaration:
                                    {
                                        var token = new MethodDeclarationToken() { Key = recommendation.Name, Description = recommendedActions.Description, TargetCPU = targetCPUs, Namespace = @namespace.Name, FullKey = recommendation.Value, Type = recommendation.ContainingType };
                                        if (!_csharpRootNodes.MethodDeclarationTokens.Contains(token)) { _csharpRootNodes.MethodDeclarationTokens.Add(token); }
                                        ParseActions(token, recommendedActions.Actions);
                                        break;
                                    }

                                case ActionTypes.ElementAccess:
                                    {
                                        var token = new ElementAccessToken() { Key = recommendation.Name, Description = recommendedActions.Description, TargetCPU = targetCPUs, Namespace = @namespace.Name, FullKey = recommendation.Value, Type = recommendation.ContainingType };
                                        if (!_csharpRootNodes.ElementAccesstokens.Contains(token)) { _csharpRootNodes.ElementAccesstokens.Add(token); }
                                        ParseActions(token, recommendedActions.Actions);
                                        break;
                                    }

                                case ActionTypes.MemberAccess:
                                    {
                                        var token = new MemberAccessToken() { Key = recommendation.Name, Description = recommendedActions.Description, TargetCPU = targetCPUs, Namespace = @namespace.Name, FullKey = recommendation.Value, Type = recommendation.ContainingType };
                                        if (!_csharpRootNodes.MemberAccesstokens.Contains(token)) { _csharpRootNodes.MemberAccesstokens.Add(token); }
                                        ParseActions(token, recommendedActions.Actions);
                                        break;
                                    }

                                case ActionTypes.Project:
                                    {
                                        var token = new ProjectToken() { Key = recommendation.Name, Description = recommendedActions.Description, TargetCPU = targetCPUs, Namespace = @namespace.Name, FullKey = recommendation.Value };
                                        if (!_csharpRootNodes.ProjectTokens.Contains(token)) { _csharpRootNodes.ProjectTokens.Add(token); }
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
        /// <param name="csharpNodeToken">The token to add the action to</param>
        /// <param name="actions">The list of actions associated with this token</param>
        public void ParseActions(CsharpNodeToken csharpNodeToken, List<Action> actions)
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
                                    csharpNodeToken.InvocationExpressionActions.Add(new InvocationExpressionAction<InvocationExpressionSyntax>()
                                    {
                                        Key = csharpNodeToken.Key,
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
                                    csharpNodeToken.ExpressionActions.Add(new ExpressionAction()
                                    {
                                        Key = csharpNodeToken.Key,
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
                                    csharpNodeToken.ClassDeclarationActions.Add(new ClassDeclarationAction()
                                    {
                                        Key = csharpNodeToken.Key,
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
                                    csharpNodeToken.InterfaceDeclarationActions.Add(new InterfaceDeclarationAction()
                                    {
                                        Key = csharpNodeToken.Key,
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
                                // Using directives can be found in both ComplilationUnit and inside Namespace.
                                // Need to make sure remove action is taken if it's inside Namespace block.
                                // Only add using directives in the CompilationUnit as our convention, so it's not added twice.
                                var namespaceActionFunc = actionsLoader.GetNamespaceActions(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    csharpNodeToken.UsingActions.Add(new UsingAction()
                                    {
                                        Key = csharpNodeToken.Key,
                                        Value = GetActionValue(action.Value),
                                        Description = action.Description,
                                        ActionValidation = action.ActionValidation,
                                        Name = action.Name,
                                        Type = action.Type,
                                        UsingActionFunc = actionFunc,
                                        NamespaceUsingActionFunc = namespaceActionFunc
                                    });
                                }
                                break;
                            }
                        case ActionTypes.Namespace:
                            {
                                var actionFunc = actionsLoader.GetNamespaceActions(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    csharpNodeToken.NamespaceActions.Add(new NamespaceAction<NamespaceDeclarationSyntax>()
                                    {
                                        Key = csharpNodeToken.Key,
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
                                    csharpNodeToken.IdentifierNameActions.Add(new IdentifierNameAction<IdentifierNameSyntax>()
                                    {
                                        Key = csharpNodeToken.Key,
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
                                    csharpNodeToken.AttributeActions.Add(new AttributeAction()
                                    {
                                        Key = csharpNodeToken.Key,
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
                                    csharpNodeToken.AttributeListActions.Add(new AttributeAction()
                                    {
                                        Key = csharpNodeToken.Key,
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
                                    csharpNodeToken.ObjectCreationExpressionActions.Add(new ObjectCreationExpressionAction()
                                    {
                                        Key = csharpNodeToken.Key,
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
                                    csharpNodeToken.MethodDeclarationActions.Add(new MethodDeclarationAction()
                                    {
                                        Key = csharpNodeToken.Key,
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
                                    csharpNodeToken.ElementAccessActions.Add(new ElementAccessAction()
                                    {
                                        Key = csharpNodeToken.Key,
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
                                    csharpNodeToken.MemberAccessActions.Add(new MemberAccessAction()
                                    {
                                        Key = csharpNodeToken.Key,
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
                                    csharpNodeToken.ProjectLevelActions.Add(new ProjectLevelAction()
                                    {
                                        Key = csharpNodeToken.Key,
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
                                    csharpNodeToken.ProjectFileActions.Add(new ProjectLevelAction()
                                    {
                                        Key = csharpNodeToken.Key,
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
                        case ActionTypes.ProjectType:
                            {
                                var actionFunc = actionsLoader.GetProjectTypeActions(action.Name, action.Value);
                                if (actionFunc != null)
                                {
                                    csharpNodeToken.ProjectTypeActions.Add(new ProjectLevelAction()
                                    {
                                        Key = csharpNodeToken.Key,
                                        Value = GetActionValue(action.Value),
                                        Description = action.Description,
                                        ActionValidation = action.ActionValidation,
                                        Name = action.Name,
                                        Type = action.Type,
                                        ProjectTypeActionFunc = actionFunc
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
                                csharpNodeToken.PackageActions.Add(packageAction);
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

        public string GetActionValue(dynamic value)
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
