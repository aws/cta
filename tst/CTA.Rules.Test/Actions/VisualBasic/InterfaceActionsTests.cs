using System;
using CTA.Rules.Actions.VisualBasic;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;

namespace CTA.Rules.Test.Actions.VisualBasic
{
    public class InterfaceActionsTests
    {
        private SyntaxGenerator _syntaxGenerator;
        private InterfaceActions _interfaceActions;
        private InterfaceBlockSyntax _node;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.VisualBasic;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _interfaceActions = new InterfaceActions();
            _node = _syntaxGenerator.InterfaceDeclaration("ISomeInterface")
                    .NormalizeWhitespace() as InterfaceBlockSyntax;
        }

        [Test]
        public void ChangeName()
        {
            string newName = "INewInterface";
            var changeNameFunc = _interfaceActions.GetChangeNameAction(newName);
            var newNode = changeNameFunc(_syntaxGenerator, _node);
            StringAssert.Contains(newName, newNode.ToFullString());
        }

        [Test]
        public void GetAddRemoveAttributeAction()
        {
            var addAttributeFunc = _interfaceActions.GetAddAttributeAction("SomeAttribute");
            var nodeWithAttribute = addAttributeFunc(_syntaxGenerator, _node);

            StringAssert.Contains("SomeAttribute", nodeWithAttribute.ToFullString());

            var removeAttributeFunc =
                _interfaceActions.GetRemoveAttributeAction("SomeAttribute");
            var newNode = removeAttributeFunc(_syntaxGenerator, nodeWithAttribute);

            var expectedResult = _node.ToFullString();
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetAddCommentAction_Appends_Comment_To_Interface_Declaration()
        {
            const string commentToAdd = "This is a comment";
            var addCommentFunc = _interfaceActions.GetAddCommentAction(commentToAdd);
            var newNode = addCommentFunc(_syntaxGenerator, _node);

            var expectedResult = @$"' Added by CTA: {commentToAdd}
Interface ISomeInterface
End Interface";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void AddAndRemoveMethod()
        {
            string newMethod = @"Public Function NewMethod As String";
            var addMethodFunc = _interfaceActions.GetAddMethodAction(newMethod);
            var newNode = addMethodFunc(_syntaxGenerator, _node);

            var addSubFunc = _interfaceActions.GetAddMethodAction("Public Sub NewSub As Integer");

            Assert.IsTrue(newNode.Members.Count == 1);
            Assert.IsTrue(newNode.ToFullString().Contains("NewMethod"));

            newNode = addSubFunc(_syntaxGenerator, newNode);
            Assert.IsTrue(newNode.Members.Count == 2);
            
            var removeMethodFunc = _interfaceActions.GetRemoveMethodAction("NewMethod");
            newNode = removeMethodFunc(_syntaxGenerator, newNode);
            
            Assert.IsTrue(newNode.Members.Count == 1);
            Assert.IsTrue(!newNode.ToFullString().Contains("NewMethod"));
            Assert.IsTrue(newNode.ToFullString().Contains("NewSub"));
        }
        
        [Test]
        public void InterfaceDeclarationEquals()
        {
            throw new NotImplementedException();
            // var interfaceAction = new InterfaceDeclarationAction
            // {
            //     Key = "Test", Value = "Test2",
            //     InterfaceDeclarationActionFunc = _interfaceActions.GetAddAttributeAction("Test")
            // };
            // var cloned = interfaceAction.Clone<InterfaceDeclarationAction>();
            // Assert.True(interfaceAction.Equals(cloned));
            //
            // cloned.Value = "DifferentValue";
            // Assert.False(interfaceAction.Equals(cloned));
        }
    }
}