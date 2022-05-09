using CTA.Rules.Actions.Csharp;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;

namespace CTA.Rules.Test.Actions
{
    public class AttributeActionsTests
    {
        private const string OriginalAttribute = "Serializable";
        private AttributeActions _attributeActions;
        private SyntaxGenerator _syntaxGenerator;
        private AttributeSyntax _node;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.CSharp;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _attributeActions = new AttributeActions();
            _node = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(SyntaxFactory.ParseToken(OriginalAttribute)));
        }

        [Test]
        public void GetChangeAttributeAction_Changes_Attribute_To_Specified_Value()
        {
            const string newAttributeName = "NewAttribute";
            var changeAttributeFunc = _attributeActions.GetChangeAttributeAction(newAttributeName);
            var newNode = changeAttributeFunc(_syntaxGenerator, _node);

            Assert.AreEqual(OriginalAttribute, _node.ToString());
            Assert.AreEqual(newAttributeName, newNode.ToString());
        }

        [Test]
        public void AttributeActionComparison()
        {
            var attributeAction = new AttributeAction()
            {
                Key = "Test",
                Value = "Test2",
                AttributeActionFunc = _attributeActions.GetChangeAttributeAction("NewAttribute")
            };

            var cloned = attributeAction.Clone<AttributeAction>();

            Assert.True(attributeAction.Equals(cloned));
            cloned.Value = "DifferentValue";
            Assert.False(attributeAction.Equals(cloned));
        }
    }
}