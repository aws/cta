using System;
using System.Collections.Generic;
using System.Linq;
using Codelyzer.Analysis.CSharp;
using CTA.Rules.Config;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Update.Rewriters
{
    public class ActionsRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _semanticModel;
        private readonly SemanticModel _preportSemanticModel;
        private readonly SyntaxGenerator _syntaxGenerator;

        private readonly string _filePath;
        private readonly List<GenericAction> _allActions;

        public List<GenericActionExecution> allExecutedActions { get; private set; }

        private static readonly Type[] identifierNameTypes = new Type[] {
            typeof(MethodDeclarationSyntax),
            typeof(ConstructorDeclarationSyntax),
            typeof(ClassDeclarationSyntax),
            typeof(VariableDeclarationSyntax),
            typeof(TypeArgumentListSyntax),
            typeof(TypeParameterListSyntax),
            typeof(ParameterSyntax),
            typeof(ObjectCreationExpressionSyntax),
            typeof(QualifiedNameSyntax),
            typeof(CastExpressionSyntax),
        };

        public ActionsRewriter(SemanticModel semanticModel, SemanticModel preportSemanticModel, SyntaxGenerator syntaxGenerator, string filePath, List<GenericAction> allActions)
        {
            _semanticModel = semanticModel;
            _preportSemanticModel = preportSemanticModel;
            _syntaxGenerator = syntaxGenerator;
            _filePath = filePath;
            _allActions = allActions;
            allExecutedActions = new List<GenericActionExecution>();
        }

        public ActionsRewriter(SemanticModel semanticModel, SemanticModel preportSemanticModel, SyntaxGenerator syntaxGenerator, string filePath, GenericAction runningAction)
        {
            _semanticModel = semanticModel;
            _preportSemanticModel = preportSemanticModel;
            _syntaxGenerator = syntaxGenerator;
            _filePath = filePath;
            _allActions = new List<GenericAction>() { runningAction };
            allExecutedActions = new List<GenericActionExecution>();
        }

        public override SyntaxNode VisitAttributeList(AttributeListSyntax node)
        {
            AttributeListSyntax attributeListSyntax = (AttributeListSyntax)base.VisitAttributeList(node);

            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                foreach (var action in _allActions.OfType<AttributeAction>())
                {
                    if (action.Key == attributeSyntax.Name.ToString())
                    {
                        if (action.AttributeListActionFunc != null)
                        {
                            var actionExecution = new GenericActionExecution(action, _filePath)
                            {
                                TimesRun = 1
                            };
                            try
                            {
                                attributeListSyntax = action.AttributeListActionFunc(_syntaxGenerator, attributeListSyntax);
                                LogHelper.LogInformation(string.Format("{0}: {1}", node.SpanStart, action.Description));
                            }
                            catch (Exception ex)
                            {
                                var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                                actionExecution.InvalidExecutions = 1;
                                LogHelper.LogError(actionExecutionException);
                            }
                            allExecutedActions.Add(actionExecution);
                        }
                    }
                }
            }

            return attributeListSyntax;
        }
        public override SyntaxNode VisitAttribute(AttributeSyntax node)
        {
            AttributeSyntax attributeSyntax = (AttributeSyntax)base.VisitAttribute(node);

            foreach (var action in _allActions.OfType<AttributeAction>())
            {
                if (action.Key == node.Name.ToString())
                {
                    if (action.AttributeActionFunc != null)
                    {
                        var actionExecution = new GenericActionExecution(action, _filePath)
                        {
                            TimesRun = 1
                        };
                        try
                        {
                            attributeSyntax = action.AttributeActionFunc(_syntaxGenerator, attributeSyntax);
                            LogHelper.LogInformation(string.Format("{0}: {1}", node.SpanStart, action.Description));
                        }
                        catch (Exception ex)
                        {
                            var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                            actionExecution.InvalidExecutions = 1;
                            LogHelper.LogError(actionExecutionException);
                        }
                        allExecutedActions.Add(actionExecution);
                    }
                }
            }

            return attributeSyntax;
        }
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var classSymbol = SemanticHelper.GetDeclaredSymbol(node, _semanticModel, _preportSemanticModel);
            ClassDeclarationSyntax newNode = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

            foreach (var action in _allActions.OfType<ClassDeclarationAction>())
            {
                if (action.Key == node.Identifier.Text.Trim())
                {
                    var actionExecution = new GenericActionExecution(action, _filePath)
                    {
                        TimesRun = 1
                    };
                    try
                    {
                        newNode = action.ClassDeclarationActionFunc(_syntaxGenerator, newNode);
                        LogHelper.LogInformation(string.Format("{0}: {1}", node.SpanStart, action.Description));
                    }
                    catch (Exception ex)
                    {
                        var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                        actionExecution.InvalidExecutions = 1;
                        LogHelper.LogError(actionExecutionException);
                    }
                    allExecutedActions.Add(actionExecution);
                }
            }
            return newNode;
        }
        public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            var classSymbol = SemanticHelper.GetDeclaredSymbol(node, _semanticModel, _preportSemanticModel);
            InterfaceDeclarationSyntax newNode = (InterfaceDeclarationSyntax)base.VisitInterfaceDeclaration(node);

            foreach (var action in _allActions.OfType<InterfaceDeclarationAction>())
            {
                if (action.Key == node.Identifier.Text.Trim())
                {
                    var actionExecution = new GenericActionExecution(action, _filePath)
                    {
                        TimesRun = 1
                    };
                    try
                    {
                        newNode = action.InterfaceDeclarationActionFunc(_syntaxGenerator, newNode);
                        LogHelper.LogInformation(string.Format("{0}: {1}", node.SpanStart, action.Description));
                    }
                    catch (Exception ex)
                    {
                        var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                        actionExecution.InvalidExecutions = 1;
                        LogHelper.LogError(actionExecutionException);
                    }
                    allExecutedActions.Add(actionExecution);
                }
            }
            return newNode;
        }
        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            var symbol = SemanticHelper.GetSemanticSymbol(node, _semanticModel, _preportSemanticModel);

            var identifierNameSyntax = (IdentifierNameSyntax)base.VisitIdentifierName(node);
            if (symbol != null)
            {
                var nodeKey = symbol.OriginalDefinition != null ? symbol.OriginalDefinition.ToString() : symbol.ToString();
                foreach (var action in _allActions.OfType<IdentifierNameAction>())
                {
                    if (nodeKey == action.Key && identifierNameTypes.Contains(identifierNameSyntax.Parent?.GetType()))
                    {
                        var actionExecution = new GenericActionExecution(action, _filePath)
                        {
                            TimesRun = 1
                        };
                        try
                        {
                            identifierNameSyntax = action.IdentifierNameActionFunc(_syntaxGenerator, identifierNameSyntax);
                            LogHelper.LogInformation(string.Format("{0}: {1}", node.SpanStart, action.Description));
                        }
                        catch (Exception ex)
                        {
                            var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                            actionExecution.InvalidExecutions = 1;
                            LogHelper.LogError(actionExecutionException);
                        }
                        allExecutedActions.Add(actionExecution);
                    }
                }
            }
            return identifierNameSyntax;
        }

        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            var newNode = base.VisitExpressionStatement(node);
            SyntaxNode modifiedNode = newNode;

            var invocationExpressionNodes = node.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();
            if(invocationExpressionNodes.Count <= 0)
            {
                return newNode;
            }
            var invocationExpressionNode = invocationExpressionNodes.First();

            var symbol = SemanticHelper.GetSemanticSymbol(invocationExpressionNode, _semanticModel, _preportSemanticModel);
            if (symbol == null)
            {
                return newNode;
            }
            var nodeKey = symbol.OriginalDefinition.ToString();

            foreach (var action in _allActions.OfType<ExpressionAction>())
            {
                if (nodeKey == action.Key)
                {
                    var actionExecution = new GenericActionExecution(action, _filePath)
                    {
                        TimesRun = 1
                    };
                    try
                    {
                        modifiedNode = action.ExpressionActionFunc(_syntaxGenerator, newNode);
                        LogHelper.LogInformation(string.Format("{0}: {1}", node.SpanStart, action.Description));
                    }
                    catch (Exception ex)
                    {
                        var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                        actionExecution.InvalidExecutions = 1;
                        LogHelper.LogError(actionExecutionException);
                    }
                    allExecutedActions.Add(actionExecution);
                }
            }
            return modifiedNode;
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symbol = SemanticHelper.GetSemanticSymbol(node, _semanticModel, _preportSemanticModel);
            var newNode = base.VisitInvocationExpression(node);


            if(symbol == null)
            {
                return node;
            }

            var nodeKey = symbol.OriginalDefinition.ToString();

            foreach (var action in _allActions.OfType<InvocationExpressionAction>())
            {
                if (nodeKey == action.Key)
                {
                    var actionExecution = new GenericActionExecution(action, _filePath)
                    {
                        TimesRun = 1
                    };
                    try
                    {
                        newNode = action.InvocationExpressionActionFunc(_syntaxGenerator, (InvocationExpressionSyntax)newNode);
                        LogHelper.LogInformation(string.Format("{0}: {1}", node.SpanStart, action.Description));
                    }
                    catch (Exception ex)
                    {
                        var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                        actionExecution.InvalidExecutions = 1;
                        LogHelper.LogError(actionExecutionException);
                    }
                    allExecutedActions.Add(actionExecution);
                }
            }
            return newNode;
        }

        public override SyntaxNode VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            var symbol = SemanticHelper.GetSemanticSymbol(node, _semanticModel, _preportSemanticModel);
            var newNode = (ElementAccessExpressionSyntax)base.VisitElementAccessExpression(node);


            if (symbol != null)
            {
                var nodeKey = $"{symbol.ContainingType}.{node.Expression}";

                foreach (var action in _allActions.OfType<ElementAccessAction>())
                {
                    if (nodeKey == action.Key)
                    {
                        var actionExecution = new GenericActionExecution(action, _filePath)
                        {
                            TimesRun = 1
                        };
                        try
                        {
                            newNode = action.ElementAccessExpressionActionFunc(_syntaxGenerator, newNode);
                            LogHelper.LogInformation(string.Format("{0}: {1}", node.SpanStart, action.Description));
                        }
                        catch (Exception ex)
                        {
                            var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                            actionExecution.InvalidExecutions = 1;
                            LogHelper.LogError(actionExecutionException);
                        }
                        allExecutedActions.Add(actionExecution);
                    }
                }
            }
            return newNode;
        }

        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var symbol = SemanticHelper.GetSemanticSymbol(node, _semanticModel, _preportSemanticModel);
            var newNode = base.VisitMemberAccessExpression(node);

            if (symbol != null)
            {
                var nodeKey = $"{symbol.ContainingType}.{node.Name}";

                foreach (var action in _allActions.OfType<MemberAccessAction>())
                {
                    if (nodeKey == action.Key)
                    {
                        var actionExecution = new GenericActionExecution(action, _filePath)
                        {
                            TimesRun = 1
                        };
                        try
                        {
                            newNode = action.MemberAccessActionFunc(_syntaxGenerator, newNode);
                            LogHelper.LogInformation(string.Format("{0}: {1}", node.SpanStart, action.Description));
                        }
                        catch (Exception ex)
                        {
                            var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                            actionExecution.InvalidExecutions = 1;
                            LogHelper.LogError(actionExecutionException);
                        }
                        allExecutedActions.Add(actionExecution);
                    }
                }
            }
            return newNode;
        }

        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {
            CompilationUnitSyntax newNode = (CompilationUnitSyntax)base.VisitCompilationUnit(node);
            //Applying using actions
            foreach (var action in _allActions.OfType<UsingAction>())
            {
                var actionExecution = new GenericActionExecution(action, _filePath)
                {
                    TimesRun = 1
                };
                try
                {
                    newNode = action.UsingActionFunc(_syntaxGenerator, newNode);
                    LogHelper.LogInformation(string.Format("{0}", action.Description));
                }
                catch (Exception ex)
                {
                    var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                    actionExecution.InvalidExecutions = 1;
                    LogHelper.LogError(actionExecutionException);
                }
                allExecutedActions.Add(actionExecution);
            }

            return newNode;
        }
        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var symbol = SemanticHelper.GetSemanticSymbol(node, _semanticModel, _preportSemanticModel);
            ExpressionSyntax newNode = node;
            bool skipChildren = false; // This is here to skip actions on children node when the main identifier was changed. Just use new expression for the subsequent children actions.
            foreach (var action in _allActions.OfType<ObjectCreationExpressionAction>())
            {
                if (newNode.ToString() == action.Key || symbol?.OriginalDefinition.ToDisplayString() == action.Key)
                {
                    var actionExecution = new GenericActionExecution(action, _filePath)
                    {
                        TimesRun = 1
                    };
                    try
                    {
                        skipChildren = true;
                        newNode = action.ObjectCreationExpressionGenericActionFunc(_syntaxGenerator, (ObjectCreationExpressionSyntax)newNode);
                        allExecutedActions.Add(actionExecution);
                        LogHelper.LogInformation(string.Format("{0}", action.Description));
                    }
                    catch (Exception ex)
                    {
                        var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                        actionExecution.InvalidExecutions = 1;
                        LogHelper.LogError(actionExecutionException);
                    }
                }
            }
            if (!skipChildren)
            {
                newNode = (ObjectCreationExpressionSyntax)base.VisitObjectCreationExpression(node);
            }
            return newNode;
        }
        public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            NamespaceDeclarationSyntax newNode = (NamespaceDeclarationSyntax)base.VisitNamespaceDeclaration(node);
            foreach (var action in _allActions.OfType<NamespaceAction>())
            {
                if (action.Key == newNode.Name.ToString())
                {
                    var actionExecution = new GenericActionExecution(action, _filePath)
                    {
                        TimesRun = 1
                    };
                    try
                    {
                        newNode = action.NamespaceActionFunc(_syntaxGenerator, newNode);
                        LogHelper.LogInformation(string.Format("{0}", action.Description));
                    }
                    catch (Exception ex)
                    {
                        var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                        actionExecution.InvalidExecutions = 1;
                        LogHelper.LogError(actionExecutionException);
                    }
                    allExecutedActions.Add(actionExecution);
                }
            }
            return newNode;
        }
    }
}
