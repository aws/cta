using System;
using System.Collections.Generic;
using System.Linq;
using CTA.Rules.Actions;
using CTA.Rules.Models;
using CTA.Rules.Models.Actions.VisualBasic;
using CTA.Rules.Update;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using NUnit.Framework;
using AttributeAction = CTA.Rules.Models.Actions.VisualBasic.AttributeAction;
using GenericAction = CTA.Rules.Models.GenericAction;
using ObjectCreationExpressionAction = CTA.Rules.Models.Actions.VisualBasic.ObjectCreationExpressionAction;

namespace CTA.Rules.Test;
public class VisualBasicActionRewriterTest
{
    private string codeBlock = @"
Imports System

Public Namespace TestNamespace
    Public Module TestModule
        Public Sub Main()
            Math.Abs(1)
            dim test = New TestClass()
        End Sub
    End Module

    <Serializable()>
    Public Class TestClass
    End Class

    Public Interface TestInterface
    End Interface
End Namespace
";

    private VisualBasicSyntaxTree _syntaxTree;
    private SemanticModel _semanticModel;
    private VisualBasicCompilation _compilation;
    private AdhocWorkspace _workspace;
    private VisualBasicActionsLoader _actionsLoader;

    [SetUp]
    public void Setup()
    {
        _syntaxTree = (VisualBasicSyntaxTree) VisualBasicSyntaxTree.ParseText(codeBlock);
        var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        _compilation = VisualBasicCompilation.Create("MyCompilation",
            syntaxTrees: new[] {_syntaxTree}, references: new[] {mscorlib});
        _semanticModel = _compilation.GetSemanticModel(_syntaxTree);
        _workspace = new AdhocWorkspace();
        _actionsLoader = new VisualBasicActionsLoader(new List<string>());

        var diagnostics = _compilation.GetDiagnostics();
        Console.WriteLine(1);
    }

    [Test]
    public void TestSingleActionRewriterOnAttributeList()
    {
        var node = _syntaxTree.GetRoot()
            .DescendantNodes()
            .First(n => n.IsKind(SyntaxKind.AttributeList));
        var action = new AttributeAction
        {
            AttributeListActionFunc = _actionsLoader.GetAttributeListAction("AddComment", "Comment"),
            Key = "Serializable"
        };
        var rewriter = new VisualBasicActionsRewriter(_semanticModel, null,
            SyntaxGenerator.GetGenerator(_workspace, "Visual Basic"), "",
            action);
        var newNode = rewriter.VisitAttributeList((AttributeListSyntax)node);
        Assert.IsTrue(newNode.GetLeadingTrivia().ToString().Contains("Comment"));
    }

    [Test]
    public void TestSingleNullActionOnAttributeList()
    {
        var node = _syntaxTree.GetRoot()
            .DescendantNodes()
            .First(n => n.IsKind(SyntaxKind.AttributeList));
        var action = new AttributeAction
        {
            AttributeListActionFunc = null,
            Key = "Serializable"
        };
        var rewriter = new VisualBasicActionsRewriter(_semanticModel, null,
            SyntaxGenerator.GetGenerator(_workspace, "Visual Basic"), "",
            action);
        var newNode = rewriter.VisitAttributeList((AttributeListSyntax) node);
        Assert.IsTrue(newNode == node);
    }

    [Test]
    public void TestVisitAttribute()
    {
        var node = _syntaxTree.GetRoot()
            .DescendantNodes()
            .First(n => n.IsKind(SyntaxKind.Attribute));
        var action = new AttributeAction
        {
            AttributeActionFunc = _actionsLoader.GetAttributeAction("ChangeAttribute", "NewAttribute"),
            Key = "Serializable"
        };
        var nullAction = new AttributeAction()
        {
            AttributeActionFunc = null,
            Key = "Serializable"
        };

        var rewriter = new VisualBasicActionsRewriter(_semanticModel, null,
            SyntaxGenerator.GetGenerator(_workspace, "Visual Basic"), "",
            new List<GenericAction>{ action, nullAction });
        var newNode = rewriter.VisitAttribute((AttributeSyntax)node);
        Assert.IsTrue(newNode.ToString().Contains("NewAttribute"));
    }

    [Test]
    public void TestVisitInterfaceBlock()
    {
        var node = _syntaxTree.GetRoot()
            .DescendantNodes()
            .First(n => n.IsKind(SyntaxKind.InterfaceBlock));
        var action = new InterfaceBlockAction
        {
            InterfaceBlockActionFunc = _actionsLoader.GetInterfaceAction("ChangeName", "NewInterfaceName"),
            Key = "TestInterface"
        };
        var nullAction = new InterfaceBlockAction
        {
            InterfaceBlockActionFunc = null,
            Key = "TestInterface"
        };
        var rewriter = new VisualBasicActionsRewriter(_semanticModel, null,
            SyntaxGenerator.GetGenerator(_workspace, "Visual Basic"), "",
            new List<GenericAction>{ action, nullAction });
        var newNode = rewriter.VisitInterfaceBlock((InterfaceBlockSyntax)node);
        Assert.IsTrue(newNode.ToString().Contains("NewInterfaceName"));
    }

    [Test]
    public void TestVisitExpressionStatement()
    {
        var node = _syntaxTree.GetRoot()
            .DescendantNodes()
            .First(n => n.IsKind(SyntaxKind.ExpressionStatement));
        var action = new ExpressionAction
        {
            ExpressionActionFunc = _actionsLoader.GetExpressionAction("AddComment", "NewComment"),
            Key = "System.Math.Abs(integer)"
        };
        var nullAction = new ExpressionAction
        {
            ExpressionActionFunc = null,
            Key = "System.Math.Abs(integer)"
        };
        var rewriter = new VisualBasicActionsRewriter(_semanticModel, null,
            SyntaxGenerator.GetGenerator(_workspace, "Visual Basic"), "",
            new List<GenericAction>{ action, nullAction });
        var newNode = rewriter.VisitExpressionStatement((ExpressionStatementSyntax)node);
        Assert.IsTrue(newNode.GetLeadingTrivia().ToString().Contains("NewComment"));
    }

    [Test]
    public void TestVisitNamespaceBlock()
    {
        var node = _syntaxTree.GetRoot()
            .DescendantNodes()
            .First(n => n.IsKind(SyntaxKind.NamespaceBlock));
        var action = new NamespaceAction<NamespaceBlockSyntax>()
        {
            NamespaceActionFunc = _actionsLoader.GetNamespaceActions("RenameNamespace", "NewNamespace"),
            Key = "TestNamespace"
        };
        var nullAction = new NamespaceAction<NamespaceBlockSyntax>()
        {
            NamespaceActionFunc = null,
            Key = "TestNamespace"
        };
        var rewriter = new VisualBasicActionsRewriter(_semanticModel, null,
            SyntaxGenerator.GetGenerator(_workspace, "Visual Basic"), "",
            new List<GenericAction>{ action, nullAction });
        var newNode = rewriter.VisitNamespaceBlock((NamespaceBlockSyntax)node);
        Assert.IsTrue(newNode.ToString().Contains("NewNamespace"));
    }

    [Test]
    public void TestVisitClassBlock()
    {
        var node = _syntaxTree.GetRoot()
            .DescendantNodes()
            .First(n => n.IsKind(SyntaxKind.ClassBlock));
        var action = new TypeBlockAction()
        {
            TypeBlockActionFunc = _actionsLoader.GetClassAction("ChangeName", "NewClass"),
            Key = "TestClass"
        };
        var nullAction = new TypeBlockAction()
        {
            TypeBlockActionFunc = null,
            Key = "TestClass"
        };
        var rewriter = new VisualBasicActionsRewriter(_semanticModel, null,
            SyntaxGenerator.GetGenerator(_workspace, "Visual Basic"), "",
            new List<GenericAction>{ action, nullAction });
        var newNode = rewriter.VisitClassBlock((ClassBlockSyntax)node);
        Assert.IsTrue(newNode.ToString().Contains("NewClass"));
    }

    [Test]
    public void TestVisitModuleBlock()
    {
        var node = _syntaxTree.GetRoot()
            .DescendantNodes()
            .First(n => n.IsKind(SyntaxKind.ModuleBlock));
        var action = new TypeBlockAction
        {
            TypeBlockActionFunc = _actionsLoader.GetClassAction("ChangeName", "NewModule"),
            Key = "TestModule"
        };
        var nullAction = new TypeBlockAction
        {
            TypeBlockActionFunc = null,
            Key = "TestModule"
        };
        var rewriter = new VisualBasicActionsRewriter(_semanticModel, null,
            SyntaxGenerator.GetGenerator(_workspace, "Visual Basic"), "",
            new List<GenericAction>{ action, nullAction });
        var newNode = rewriter.VisitModuleBlock((ModuleBlockSyntax)node);
        Assert.IsTrue(newNode.ToString().Contains("NewModule"));
    }

    [Test]
    public void TestVisitMemberAccessExpression()
    {
        var node = _syntaxTree.GetRoot()
            .DescendantNodes()
            .First(n => n.IsKind(SyntaxKind.SimpleMemberAccessExpression));
        var action = new MemberAccessAction()
        {
            MemberAccessActionFunc = _actionsLoader.GetMemberAccessExpressionActions("AddComment", "NewComment"),
            Key = "System.Math.Abs"
        };
        var nullAction = new MemberAccessAction()
        {
            MemberAccessActionFunc = null,
            Key = "System.Math.Abs"
        };
        var rewriter = new VisualBasicActionsRewriter(_semanticModel, null,
            SyntaxGenerator.GetGenerator(_workspace, "Visual Basic"), "",
            new List<GenericAction>{ action, nullAction });
        var newNode = rewriter.VisitMemberAccessExpression((MemberAccessExpressionSyntax)node);
        Assert.IsTrue(newNode.GetLeadingTrivia().ToString().Contains("NewComment"));
    }

    [Test]
    public void TestVisitObjectCreationExpression()
    {
        var node = _syntaxTree.GetRoot()
            .DescendantNodes()
            .First(n => n.IsKind(SyntaxKind.ObjectCreationExpression));
        var action = new ObjectCreationExpressionAction()
        {
            ObjectCreationExpressionGenericActionFunc = _actionsLoader.GetObjectCreationExpressionActions("AddComment", "NewComment"),
            Key = "New TestClass()"
        };
        var nullAction = new ObjectCreationExpressionAction()
        {
            ObjectCreationExpressionGenericActionFunc = null,
            Key = "New TestClass()"
        };
        var rewriter = new VisualBasicActionsRewriter(_semanticModel, null,
            SyntaxGenerator.GetGenerator(_workspace, "Visual Basic"), "",
            new List<GenericAction>{ action, nullAction });
        var newNode = rewriter.VisitObjectCreationExpression((ObjectCreationExpressionSyntax)node);
        Assert.IsTrue(newNode.GetLeadingTrivia().ToString().Contains("NewComment"));
    }
}