using CTA.FeatureDetection.Common.Extensions;
using CTA.Rules.Actions.Csharp;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;
using System.Linq;

namespace CTA.Rules.Test.Actions
{
    internal class NamespaceActionsTests
    {
        private SyntaxGenerator _syntaxGenerator;
        private NamespaceActions _namespaceActions;
        private NamespaceDeclarationSyntax _node;

        [SetUp]
        public void SetUp()
        {
            var workspace = new AdhocWorkspace();
            var language = LanguageNames.CSharp;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _namespaceActions = new NamespaceActions();

            SyntaxTree tree = CSharpSyntaxTree.ParseText(
                @$"
                namespace DummyNamespace
                {{
                    using System.Web;
                    class MyClass
                    {{
                    }};
                }}");
            _node = tree.GetRoot()
                        .DescendantNodes()
                        .OfType<NamespaceDeclarationSyntax>()
                        .FirstOrDefault();
        }

        [Test]
        public void GetRenameNamespaceAction_Rename_Namespace()
        {
            const string newNamespaceName = "NewNamespace";
            var renameNamespaceFunc = _namespaceActions.GetRenameNamespaceAction(newNamespaceName);
            var newNode = renameNamespaceFunc(_syntaxGenerator, _node);

            var expectedResult = CSharpSyntaxTree.ParseText(@$"
                    namespace NewNamespace
                    {{
                        using System.Web;
                        class MyClass
                        {{
                        }};
                    }}").GetRoot();
            Assert.AreEqual(expectedResult.RemoveAllTrivia().ToFullString(), newNode.RemoveAllTrivia().ToFullString());
        }

        [Test]
        public void GetRemoveDirectiveAction_Removes_Directive()
        {
            const string directive = "System.Web";
            var removeDirectiveFunc = _namespaceActions.GetRemoveDirectiveAction(directive);
            var newNode = removeDirectiveFunc(_syntaxGenerator, _node);

            var expectedResult = CSharpSyntaxTree.ParseText(@$"
                    namespace DummyNamespace
                    {{
                        class MyClass
                        {{
                        }};
                    }}").GetRoot();
            Assert.AreEqual(expectedResult.RemoveAllTrivia().ToFullString(), newNode.RemoveAllTrivia().ToFullString());
        }


    }
}
