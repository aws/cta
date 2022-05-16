using System;
using System.Collections.Generic;
using System.Linq;
using Codelyzer.Analysis.CSharp;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using AttributeListSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax.AttributeListSyntax;
using AttributeSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax.AttributeSyntax;
using CastExpressionSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax.CastExpressionSyntax;
using CompilationUnitSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax.CompilationUnitSyntax;
using ExpressionStatementSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax.ExpressionStatementSyntax;
using ExpressionSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax.ExpressionSyntax;
using IdentifierNameSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax.IdentifierNameSyntax;
using InvocationExpressionSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax.InvocationExpressionSyntax;
using MemberAccessExpressionSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax.MemberAccessExpressionSyntax;
using ObjectCreationExpressionSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax.ObjectCreationExpressionSyntax;
using ParameterSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax.ParameterSyntax;
using QualifiedNameSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax.QualifiedNameSyntax;
using TypeArgumentListSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax.TypeArgumentListSyntax;
using TypeParameterListSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax.TypeParameterListSyntax;

namespace CTA.Rules.Update;

public class VisualBasicActionsRewriter : VisualBasicSyntaxRewriter, ISyntaxRewriter
{
    private readonly SemanticModel _semanticModel;
    private readonly SemanticModel _preportSemanticModel;
    private readonly SyntaxGenerator _syntaxGenerator;

    private readonly string _filePath;
    private readonly List<GenericAction> _allActions;

    public List<GenericActionExecution> AllExecutedActions { get; set; }

    private static readonly Type[] identifierNameTypes = new Type[]
    {
        typeof(MethodDeclarationSyntax),
        typeof(ConstructorDeclarationSyntax),
        typeof(ClassDeclarationSyntax),
        typeof(VariableDeclarationSyntax),
        typeof(TypeArgumentListSyntax),
        typeof(TypeParameterListSyntax),
        typeof(ParameterSyntax),
        typeof(ObjectCreationExpressionSyntax),
        typeof(QualifiedNameSyntax),
        typeof(CastExpressionSyntax)
    };

    public VisualBasicActionsRewriter(SemanticModel semanticModel, SemanticModel preportSemanticModel,
        SyntaxGenerator syntaxGenerator, string filePath, List<GenericAction> allActions)
    {
        _semanticModel = semanticModel;
        _preportSemanticModel = preportSemanticModel;
        _syntaxGenerator = syntaxGenerator;
        _filePath = filePath;
        _allActions = allActions;
        AllExecutedActions = new List<GenericActionExecution>();
    }

    public VisualBasicActionsRewriter(SemanticModel semanticModel, SemanticModel preportSemanticModel,
        SyntaxGenerator syntaxGenerator, string filePath, GenericAction runningAction)
    {
        _semanticModel = semanticModel;
        _preportSemanticModel = preportSemanticModel;
        _syntaxGenerator = syntaxGenerator;
        _filePath = filePath;
        _allActions = new List<GenericAction>() { runningAction };
        AllExecutedActions = new List<GenericActionExecution>();
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
                        var actionExecution = new GenericActionExecution(action, _filePath) { TimesRun = 1 };
                        try
                        {
                            //todo: attributeslistfunc
                            //attributeListSyntax = action.AttributeListActionFunc(_syntaxGenerator, attributeListSyntax);
                            LogHelper.LogInformation(string.Format("{0}: {1}", node.SpanStart, action.Description));
                        }
                        catch (Exception ex)
                        {
                            var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                            actionExecution.InvalidExecutions = 1;
                            LogHelper.LogError(actionExecutionException);
                        }

                        AllExecutedActions.Add(actionExecution);
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
                    var actionExecution = new GenericActionExecution(action, _filePath) { TimesRun = 1 };
                    try
                    {
                        //todo: attributesactionfunc
                        //attributeSyntax = action.AttributeActionFunc(_syntaxGenerator, attributeSyntax);
                        LogHelper.LogInformation(string.Format("{0}: {1}", node.SpanStart, action.Description));
                    }
                    catch (Exception ex)
                    {
                        var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                        actionExecution.InvalidExecutions = 1;
                        LogHelper.LogError(actionExecutionException);
                    }

                    AllExecutedActions.Add(actionExecution);
                }
            }
        }

        return attributeSyntax;
    }

    public override SyntaxNode VisitClassBlock(ClassBlockSyntax node)
    {
        var classSymbol = SemanticHelper.GetDeclaredSymbol(node, _semanticModel, _preportSemanticModel);
        ClassDeclarationSyntax newNode = (ClassDeclarationSyntax)base.VisitClassBlock(node);

        foreach (var action in _allActions.OfType<ClassDeclarationAction>())
        {
            if (action.Key == node.ClassStatement.Identifier.Text.Trim())
            {
                var actionExecution = new GenericActionExecution(action, _filePath) { TimesRun = 1 };
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

                AllExecutedActions.Add(actionExecution);
            }
        }

        return newNode;
    }

    public override SyntaxNode VisitInterfaceBlock(InterfaceBlockSyntax node)
    {
        var classSymbol = SemanticHelper.GetDeclaredSymbol(node, _semanticModel, _preportSemanticModel);
        InterfaceDeclarationSyntax newNode = (InterfaceDeclarationSyntax)base.VisitInterfaceBlock(node);

        foreach (var action in _allActions.OfType<InterfaceDeclarationAction>())
        {
            if (action.Key == node.InterfaceStatement.Identifier.Text.Trim())
            {
                var actionExecution = new GenericActionExecution(action, _filePath) { TimesRun = 1 };
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

                AllExecutedActions.Add(actionExecution);
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
                    var actionExecution = new GenericActionExecution(action, _filePath) { TimesRun = 1 };
                    try
                    {
                        //todo: identifiernamefunc
                        //identifierNameSyntax = action.IdentifierNameActionFunc(_syntaxGenerator, identifierNameSyntax);
                        LogHelper.LogInformation(string.Format("{0}: {1}", node.SpanStart, action.Description));
                    }
                    catch (Exception ex)
                    {
                        var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                        actionExecution.InvalidExecutions = 1;
                        LogHelper.LogError(actionExecutionException);
                    }

                    AllExecutedActions.Add(actionExecution);
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
        if (invocationExpressionNodes.Count <= 0)
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
                var actionExecution = new GenericActionExecution(action, _filePath) { TimesRun = 1 };
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

                AllExecutedActions.Add(actionExecution);
            }
        }

        return modifiedNode;
    }

    public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        var symbol = SemanticHelper.GetSemanticSymbol(node, _semanticModel, _preportSemanticModel);
        var newNode = base.VisitInvocationExpression(node);
        
        if (symbol == null)
        {
            return node;
        }

        var nodeKey = symbol.OriginalDefinition.ToString();

        foreach (var action in _allActions.OfType<InvocationExpressionAction<InvocationExpressionSyntax>>())
        {
            if (nodeKey == action.Key)
            {
                var actionExecution = new GenericActionExecution(action, _filePath) { TimesRun = 1 };
                try
                {
                    newNode = action.InvocationExpressionActionFunc(_syntaxGenerator,
                        (InvocationExpressionSyntax)newNode);
                    LogHelper.LogInformation($"{node.SpanStart}: {action.Description}");
                }
                catch (Exception ex)
                {
                    var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                    actionExecution.InvalidExecutions = 1;
                    LogHelper.LogError(actionExecutionException);
                }

                AllExecutedActions.Add(actionExecution);
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
                    var actionExecution = new GenericActionExecution(action, _filePath) { TimesRun = 1 };
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

                    AllExecutedActions.Add(actionExecution);
                }
            }
        }

        return newNode;
    }

    public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
    {
        CompilationUnitSyntax newNode = (CompilationUnitSyntax)base.VisitCompilationUnit(node);
        //Applying using actions
        foreach (var action in _allActions.OfType<ImportAction>())
        {
            var actionExecution = new GenericActionExecution(action, _filePath) { TimesRun = 1 };
            try
            {
                newNode = action.ImportActionFunc(_syntaxGenerator, newNode);
                LogHelper.LogInformation(string.Format("{0} in CompilationUnit.", action.Description));
            }
            catch (Exception ex)
            {
                var actionExecutionException = new ActionExecutionException(action.Name, action.Key, ex);
                actionExecution.InvalidExecutions = 1;
                LogHelper.LogError(actionExecutionException);
            }

            AllExecutedActions.Add(actionExecution);
        }

        return newNode;
    }

    public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        var symbol = SemanticHelper.GetSemanticSymbol(node, _semanticModel, _preportSemanticModel);
        ExpressionSyntax newNode = node;
        var
            skipChildren =
                false; // This is here to skip actions on children node when the main identifier was changed. Just use new expression for the subsequent children actions.
        foreach (var action in _allActions.OfType<ObjectCreationExpressionAction>())
        {
            if (newNode.ToString() == action.Key || symbol?.OriginalDefinition.ToDisplayString() == action.Key)
            {
                var actionExecution = new GenericActionExecution(action, _filePath) { TimesRun = 1 };
                try
                {
                    skipChildren = true;
                    //todo: objectcreationexpressionsyntaxfunc
                    //newNode = action.ObjectCreationExpressionGenericActionFunc(_syntaxGenerator,
                        //(ObjectCreationExpressionSyntax)newNode);
                    AllExecutedActions.Add(actionExecution);
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

    public override SyntaxNode VisitNamespaceBlock(NamespaceBlockSyntax node)
    {
        NamespaceBlockSyntax newNode = (NamespaceBlockSyntax)base.VisitNamespaceBlock(node);
        // Handle namespace renaming actions etc.
        foreach (var action in _allActions.OfType<NamespaceAction<NamespaceBlockSyntax>>())
        {
            if (action.Key == newNode.NamespaceStatement.Name.ToString())
            {
                var actionExecution = new GenericActionExecution(action, _filePath) { TimesRun = 1 };
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

                AllExecutedActions.Add(actionExecution);
            }
        }
        //VB doesn't allow for imports within a namespace.
        return newNode;
    }
}
