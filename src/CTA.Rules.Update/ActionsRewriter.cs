using System;
using System.Collections.Generic;
using System.Linq;
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
        FileActions _fileActions;
        private SemanticModel _semanticModel;
        private SyntaxGenerator _syntaxGenerator;
        public List<GenericActionExecution> allActions { get; private set; }

        private static Type[] identifierNameTypes = new Type[] {
            typeof(MethodDeclarationSyntax),
            typeof(ClassDeclarationSyntax),
            typeof(VariableDeclarationSyntax),
            typeof(ParameterSyntax),
            typeof(ObjectCreationExpressionSyntax)};

        public ActionsRewriter(SemanticModel semanticModel, SyntaxGenerator syntaxGenerator, FileActions fileActions)
        {
            _semanticModel = semanticModel;
            _syntaxGenerator = syntaxGenerator;
            _fileActions = fileActions;
            allActions = new List<GenericActionExecution>();
        }

        public override SyntaxNode VisitAttributeList(AttributeListSyntax node)
        {
            AttributeListSyntax attributeListSyntax = (AttributeListSyntax)base.VisitAttributeList(node);

            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                foreach (var action in _fileActions.AttributeActions)
                {
                    if (action.Key == attributeSyntax.Name.ToString())
                    {
                        if (action.AttributeListActionFunc != null)
                        {
                            var actionExecution = new GenericActionExecution(action, _fileActions.FilePath);
                            actionExecution.TimesRun = 1;
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
                            allActions.Add(actionExecution);
                        }
                    }
                }
            }

            return attributeListSyntax;
        }
        public override SyntaxNode VisitAttribute(AttributeSyntax node)
        {
            var attributeSymbol = _semanticModel.GetSymbolInfo(node);
            AttributeSyntax attributeSyntax = (AttributeSyntax)base.VisitAttribute(node);

            foreach (var action in _fileActions.AttributeActions)
            {
                if (action.Key == node.Name.ToString())
                {
                    if (action.AttributeActionFunc != null)
                    {
                        var actionExecution = new GenericActionExecution(action, _fileActions.FilePath);
                        actionExecution.TimesRun = 1;
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
                        allActions.Add(actionExecution);
                    }
                }
            }

            return attributeSyntax;
        }
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var classSymbol = _semanticModel.GetDeclaredSymbol(node);
            ClassDeclarationSyntax newNode = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

            foreach (var action in _fileActions.ClassDeclarationActions)
            {
                if (action.Key == node.Identifier.Text.Trim())
                {
                    var actionExecution = new GenericActionExecution(action, _fileActions.FilePath);
                    actionExecution.TimesRun = 1;
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
                    allActions.Add(actionExecution);
                }
            }
            return newNode;
        }
        public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            var classSymbol = _semanticModel.GetDeclaredSymbol(node);
            InterfaceDeclarationSyntax newNode = (InterfaceDeclarationSyntax)base.VisitInterfaceDeclaration(node);

            foreach (var action in _fileActions.InterfaceDeclarationActions)
            {
                if (action.Key == node.Identifier.Text.Trim())
                {
                    var actionExecution = new GenericActionExecution(action, _fileActions.FilePath);
                    actionExecution.TimesRun = 1;
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
                    allActions.Add(actionExecution);
                }
            }
            return newNode;
        }
        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(node);

            var identifierNameSyntax = (IdentifierNameSyntax)base.VisitIdentifierName(node);
            if (symbolInfo.Symbol != null)
            {
                foreach (var action in _fileActions.IdentifierNameActions)
                {
                    if (symbolInfo.Symbol.ToString() == action.Key && identifierNameTypes.Contains(identifierNameSyntax.Parent?.GetType()))
                    {
                        var actionExecution = new GenericActionExecution(action, _fileActions.FilePath);
                        actionExecution.TimesRun = 1;
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
                        allActions.Add(actionExecution);
                    }
                }
            }
            return identifierNameSyntax;
        }
        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symbols = _semanticModel.GetSymbolInfo(node);
            var newNode = (InvocationExpressionSyntax)base.VisitInvocationExpression(node);

            foreach (var action in _fileActions.InvocationExpressionActions)
            {
                if (symbols.Symbol.Name == action.Key)
                {
                    var actionExecution = new GenericActionExecution(action, _fileActions.FilePath);
                    actionExecution.TimesRun = 1;
                    try
                    {
                        newNode = action.InvocationExpressionActionFunc(_syntaxGenerator, newNode);
                        LogHelper.LogInformation(string.Format("{0}: {1}", node.SpanStart, action.Description));
                    }
                    catch (Exception ex)
                    {
                        var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                        actionExecution.InvalidExecutions = 1;
                        LogHelper.LogError(actionExecutionException);
                    }
                    allActions.Add(actionExecution);
                }
            }
            return newNode;
        }

        public override SyntaxNode VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            var symbols = _semanticModel.GetSymbolInfo(node);
            var newNode = (ElementAccessExpressionSyntax)base.VisitElementAccessExpression(node);

            var symbol = symbols.Symbol;

            if (symbol != null)
            {
                var nodeKey = $"{symbol.ContainingType}.{node.Expression}";

                foreach (var action in _fileActions.ElementAccessActions)
                {
                    if (nodeKey == action.Key)
                    {
                        var actionExecution = new GenericActionExecution(action, _fileActions.FilePath);
                        actionExecution.TimesRun = 1;
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
                        allActions.Add(actionExecution);
                    }
                }
            }
            return newNode;
        }

        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var symbols = _semanticModel.GetSymbolInfo(node);
            var newNode = base.VisitMemberAccessExpression(node);
            var symbol = symbols.Symbol;

            if (symbol != null)
            {
                var nodeKey = $"{symbol.ContainingType}.{node.Name}";

                foreach (var action in _fileActions.MemberAccessActions)
                {
                    if (nodeKey == action.Key)
                    {
                        var actionExecution = new GenericActionExecution(action, _fileActions.FilePath);
                        actionExecution.TimesRun = 1;
                        try
                        {
                            newNode = action.MemberAccessActionFunc(_syntaxGenerator, (MemberAccessExpressionSyntax)newNode);
                            LogHelper.LogInformation(string.Format("{0}: {1}", node.SpanStart, action.Description));
                        }
                        catch (Exception ex)
                        {
                            var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                            actionExecution.InvalidExecutions = 1;
                            LogHelper.LogError(actionExecutionException);
                        }
                        allActions.Add(actionExecution);
                    }
                }
            }
            return newNode;
        }

        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {
            CompilationUnitSyntax newNode = (CompilationUnitSyntax)base.VisitCompilationUnit(node);
            //Applying using actions
            foreach (var action in _fileActions.Usingactions)
            {
                var actionExecution = new GenericActionExecution(action, _fileActions.FilePath);
                actionExecution.TimesRun = 1;
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
                allActions.Add(actionExecution);
            }

            return newNode;
        }
        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var symbols = _semanticModel.GetSymbolInfo(node);
            ObjectCreationExpressionSyntax newNode = node;// (ObjectCreationExpressionSyntax)base.VisitObjectCreationExpression(node);
            bool skipChildren = false;
            foreach (var action in _fileActions.ObjectCreationExpressionActions)
            {
                if (newNode.ToString() == action.Key || symbols.Symbol.OriginalDefinition.ToString() == action.Key)
                {
                    var actionExecution = new GenericActionExecution(action, _fileActions.FilePath);
                    actionExecution.TimesRun = 1;
                    try
                    {
                        skipChildren = true;
                        var createdNode = action.ObjectCreationExpressionGenericActionFunc(_syntaxGenerator, newNode);
                        allActions.Add(actionExecution);
                        LogHelper.LogInformation(string.Format("{0}", action.Description));
                        return createdNode;
                    }
                    catch (Exception ex)
                    {
                        var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                        actionExecution.InvalidExecutions = 1;
                        LogHelper.LogError(actionExecutionException);
                    }
                    allActions.Add(actionExecution);
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
            foreach (var action in _fileActions.NamespaceActions)
            {
                if (action.Key == newNode.Name.ToString())
                {
                    var actionExecution = new GenericActionExecution(action, _fileActions.FilePath);
                    actionExecution.TimesRun = 1;
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
                    allActions.Add(actionExecution);
                }
            }
            return newNode;
        }
    }
}
