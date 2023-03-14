using CTA.Rules.Actions.Csharp;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;

namespace CTA.Rules.Test.Actions
{
    public class InterfaceActionsTests
    {
        private SyntaxGenerator _syntaxGenerator;
        private InterfaceActions _interfaceActions;
        private InterfaceDeclarationSyntax _node;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.CSharp;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _interfaceActions = new InterfaceActions();
            _node = _syntaxGenerator.InterfaceDeclaration("ISomeInterface")
                    .NormalizeWhitespace() as InterfaceDeclarationSyntax;
        }

        [Test]
        public void GetRemoveAttributeAction_Removes_Specified_Attribute()
        {
            var nodeWithAttribute = (InterfaceDeclarationSyntax)_syntaxGenerator.AddAttributes(_node,
                _syntaxGenerator.Attribute("SomeAttribute"));

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

            var expectedResult = @$"/* Added by CTA: {commentToAdd} */interface ISomeInterface
{{
}}";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }


        [Test]
        public void InterfaceDeclarationEquals()
        {
            var interfaceAction = new InterfaceDeclarationAction() { Key = "Test", Value = "Test2", InterfaceDeclarationActionFunc = _interfaceActions.GetAddAttributeAction("Test") };
            var cloned = interfaceAction.Clone<InterfaceDeclarationAction>();
            Assert.True(interfaceAction.Equals(cloned));

            cloned.Value = "DifferentValue";
            Assert.False(interfaceAction.Equals(cloned));
        }

        [Test]
        public void TestGetAddMethodAction()
        {
            var interfaceAction = new InterfaceDeclarationAction();

            const string methodToAdd = "string HelloWorld(string input);";
            var addMethodAction = _interfaceActions.GetAddMethodAction(methodToAdd);
            var newNode = addMethodAction(_syntaxGenerator, _node);

            var expectedResult = @$"interface ISomeInterface
{{
string HelloWorld(string input);}}";
            Assert.AreEqual(expectedResult, newNode.ToFullString());

        }
    }
}