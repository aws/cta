using CTA.Rules.Actions;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace CTA.Rules.Test.Actions
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
            var language = LanguageNames.CSharp;
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
                _syntaxGenerator.ClassDeclaration(className, null, Accessibility.NotApplicable, DeclarationModifiers.None, _syntaxGenerator.IdentifierName("Controller"), null, 
                new List<SyntaxNode>() {
                        _syntaxGenerator.FieldDeclaration("storeDB", _syntaxGenerator.IdentifierName(originalIdentifier), Accessibility.Public, DeclarationModifiers.None, 
                            _syntaxGenerator.ObjectCreationExpression(SyntaxFactory.ParseTypeName(originalIdentifier)))
                })).NormalizeWhitespace();

            var replaceIdentifierFunc =
                _identifierNameActions.GetReplaceIdentifierInsideClassAction(newIdentifier, namespaceName + "." + className);
            FieldDeclarationSyntax variableDeclaration = (FieldDeclarationSyntax)customNode.ChildNodes().FirstOrDefault(c => c.IsKind(SyntaxKind.ClassDeclaration)).ChildNodes().FirstOrDefault(f => f.IsKind(SyntaxKind.FieldDeclaration));

            var newNode = replaceIdentifierFunc(_syntaxGenerator, (IdentifierNameSyntax)variableDeclaration.Declaration.Type);

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
