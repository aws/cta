using CTA.Rules.Actions.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace CTA.Rules.Test.Actions.VisualBasic
{
    public class IdentifierNameActionsTests
    {
        private SyntaxGenerator _syntaxGenerator;
        private IdentifierNameActions _identifierNameActions;
        private IdentifierNameSyntax _node;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.VisualBasic;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _identifierNameActions = new IdentifierNameActions();
            _node = (IdentifierNameSyntax)_syntaxGenerator.IdentifierName("MusicStoreEntities");
        }

        [Test]
        public void ReplaceIdentifierInsideClassActionTest()
        {
            string newIdentifier = "SuperAwesomeClass.SomethingElse";
            string originalIdentifier = "SuperAwesomeClass.MusicStoreEntities";
            string namespaceName = "MvcMusicStore.Controllers";
            string className = "ShoppingCartController";
            SyntaxNode customNode = _syntaxGenerator.NamespaceDeclaration(namespaceName,
                _syntaxGenerator.ClassDeclaration(className, null, Accessibility.NotApplicable,
                    DeclarationModifiers.None, _syntaxGenerator.IdentifierName("Controller"), null,
                    new List<SyntaxNode>()
                    {
                        _syntaxGenerator.FieldDeclaration("storeDB",
                            _syntaxGenerator.IdentifierName(originalIdentifier), Accessibility.Public,
                            DeclarationModifiers.None,
                            _syntaxGenerator.ObjectCreationExpression(SyntaxFactory.ParseTypeName(originalIdentifier)))
                    })).NormalizeWhitespace();

            var replaceIdentifierFunc =
                _identifierNameActions.GetReplaceIdentifierInsideClassAction(newIdentifier,
                    namespaceName + "." + className);
            var variableDeclaration = (FieldDeclarationSyntax)customNode.ChildNodes()
                .FirstOrDefault(c => c.IsKind(SyntaxKind.ClassBlock)).ChildNodes()
                .FirstOrDefault(f => f.IsKind(SyntaxKind.FieldDeclaration));

            var newNode = replaceIdentifierFunc(_syntaxGenerator,
                (IdentifierNameSyntax)variableDeclaration.Declarators.First().AsClause.ChildNodes()
                    .FirstOrDefault(c => c.IsKind(SyntaxKind.IdentifierName)));

            Assert.AreEqual(newIdentifier, newNode.ToFullString().Trim());
        }

        [Test]
        public void ReplaceIdentifierActionTest()
        {
            string newIdentifier = "SomethingElse";
            var removeAttributeFunc =
                _identifierNameActions.GetReplaceIdentifierAction(newIdentifier);
            var newNode = removeAttributeFunc(_syntaxGenerator, _node);

            Assert.AreEqual(newIdentifier, newNode.ToFullString());
        }
    }
}
