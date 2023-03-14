using CTA.Rules.Actions.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;
using VisualBasicSyntaxTree = Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree;


namespace CTA.Rules.Test.Actions.VisualBasic
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
            var language = LanguageNames.VisualBasic;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _compilationUnitActions = new CompilationUnitActions();

            SyntaxTree tree = VisualBasicSyntaxTree.ParseText(@$"Imports System.Web
Class MyClass
End Class");
            _node = tree.GetCompilationUnitRoot();
        }

        [Test]
        public void GetAddDirectiveAction_Adds_Directive()
        {
            const string directive = "System.Collections.Generic";
            var addStatementAction = _compilationUnitActions.GetAddStatementAction(directive);
            var newNode = addStatementAction(_syntaxGenerator, _node);

            var expectedResult = @$"Imports System.Web
Imports {directive}

Class MyClass
End Class";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetRemoveDirectiveAction_Removes_Directive()
        {
            const string directive = "System.Web";
            var removeStatementAction = _compilationUnitActions.GetRemoveStatementAction(directive);
            var newNode = removeStatementAction(_syntaxGenerator, _node);

            var expectedResult = @$"Class MyClass
End Class";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetAddCommentAction_Adds_Comment_Above_Directive()
        {
            const string commentToAdd = "This is a comment";
            var addCommentFunc = _compilationUnitActions.GetAddCommentAction(commentToAdd);
            var newNode = addCommentFunc(_syntaxGenerator, _node);

            var expectedResult = @$"' Added by CTA: {commentToAdd}Imports System.Web
Class MyClass
End Class";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }
    }
}
