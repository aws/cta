using CTA.FeatureDetection.Common.Extensions;
using CTA.Rules.Actions.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;
using System.Linq;

namespace CTA.Rules.Test.Actions.VisualBasic
{
    internal class NamespaceActionsTests
    {
        private SyntaxGenerator _syntaxGenerator;
        private NamespaceActions _namespaceActions;
        private NamespaceBlockSyntax _node;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.CSharp;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _namespaceActions = new NamespaceActions();

            SyntaxTree tree = VisualBasicSyntaxTree.ParseText(@$"
                Namespace DummyNamespace
                    Class MyClass
                    End Class
                End Namespace");
            _node = tree.GetRoot()
                        .DescendantNodes()
                        .OfType<NamespaceBlockSyntax>()
                        .FirstOrDefault();
        }

        [Test]
        public void GetRenameNamespaceAction_Rename_Namespace()
        {
            const string newNamespaceName = "NewNamespace";
            var renameNamespaceFunc = _namespaceActions.GetRenameNamespaceAction(newNamespaceName);
            var newNode = renameNamespaceFunc(_syntaxGenerator, _node);

            var expectedResult = VisualBasicSyntaxTree.ParseText(@$"
                Namespace NewNamespace
                    Class MyClass
                    End Class
                End Namespace").GetRoot();
            Assert.AreEqual(expectedResult.RemoveAllTrivia().ToFullString(), newNode.RemoveAllTrivia().ToFullString());
        }
    }
}
