using CTA.Rules.Actions.Csharp;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;
using System.Collections.Generic;


namespace CTA.Rules.Test.Actions
{
    public class CompilationUnitActionsTests
    {
        private SyntaxGenerator _syntaxGenerator;
        private CompilationUnitActions _compilationUnitActions;
        private CompilationUnitSyntax _node;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.CSharp;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _compilationUnitActions = new CompilationUnitActions();

            SyntaxTree tree = CSharpSyntaxTree.ParseText(@$"using System.Web; 
class MyClass
{{
}}");
            _node = tree.GetCompilationUnitRoot();
        }

        [Test]
        public void GetAddDirectiveAction_Adds_Directive()
        {
            const string directive = "System.Collections.Generic";
            var addCommentFunc = _compilationUnitActions.GetAddDirectiveAction(directive);
            var newNode = addCommentFunc(_syntaxGenerator, _node);

            var expectedResult = @$"using System.Web;
using {directive};

class MyClass
{{
}}";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetRemoveDirectiveAction_Removes_Directive()
        {
            const string directive = "System.Web";
            var changeNameFunc = _compilationUnitActions.GetRemoveDirectiveAction(directive);
            var newNode = changeNameFunc(_syntaxGenerator, _node);

            var expectedResult = @$"class MyClass
{{
}}";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetAddCommentAction_Adds_Comment_Above_Directive()
        {
            const string commentToAdd = "This is a comment";
            var addCommentFunc = _compilationUnitActions.GetAddCommentAction(commentToAdd);
            var newNode = addCommentFunc(_syntaxGenerator, _node);

            var expectedResult = @$"/* Added by CTA: {commentToAdd} */
using System.Web;

class MyClass
{{
}}";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }
    }
}
