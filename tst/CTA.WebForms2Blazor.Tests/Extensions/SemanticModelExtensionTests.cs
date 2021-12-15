using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.Services;
using CTA.WebForms2Blazor.Extensions;

namespace CTA.WebForms2Blazor.Tests.Extensions
{
    public class SemanticModelExtensionTests
    {
        private const string TestDocument1NamespaceText =
            @"namespace TestNamespace1 {
                public class TestClass1 {
                    public TestClass1() {
                        TestClass2.Method1();
                    }
                }

                public static class TestClass2 {
                    public static void Method1() { }
                }
            }";
        private const string TestDocument1Of2Text =
            @"namespace TestNamespace1 {
                using TestNamespace2;    

                public class TestClass1 {
                    public TestClass1() {
                        TestClass2.Method1();
                    }
                }
            }";
        private const string TestDocument2Of2Text =
            @"namespace TestNamespace2 {
                public static class TestClass2 {
                    public static void Method1() { }
                }
            }";
        private const string TestDocumentClassExtensionText =
            @"namespace TestNamespace1 {
                using TestNamespace2;    

                public class TestClass1 : TestClass2 { }
            }
            namespace TestNamespace2 {
                public class TestClass2 { }
            }";
        private const string TestDocumentInterfaceMultipleImplementationText =
            @"namespace TestNamespace1 {
                using TestNamespace2;    
                using TestNamespace3;

                public class TestClass1 : ITestClass2, ITestClass3 { }
            }
            namespace TestNamespace2 {
                public interface ITestClass2 { }
            }
            namespace TestNamespace3 {
                public interface ITestClass3 { }
            }";
        private const string TestDocumentMethodCallText =
            @"namespace TestNamespace1 {
                using TestNamespace2;    

                public class TestClass1 {
                    public TestClass1() {
                        TestClass2.Method1();
                    }
                }
            }
            namespace TestNamespace2 {
                public static class TestClass2 {
                    public static void Method1() { }
                }
            }";
        private const string TestDocumentStackedBaseClassesText =
            @"namespace TestNamespace1 {
                using TestNamespace2;    

                public class TestClass1 : TestClass2 { }
            }
            namespace TestNamespace2 {
                using TestNamespace3;

                public class TestClass2 : TestClass3 { }
            }
            namespace TestNamespace3 {
                public class TestClass3 { }
            }";

        private WorkspaceManagerService _workspaceManager;

        [SetUp]
        public void SetUp()
        {
            _workspaceManager = new WorkspaceManagerService();
            _workspaceManager.CreateSolutionFile();
        }

        [Test]
        public async Task GetNamespacesReferencedByType_Does_Not_Contain_Own_Namespace()
        {
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedDocument();

            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            var did1 = _workspaceManager.AddDocument(pid1, "TestDocumentName1", TestDocument1NamespaceText);

            var model = await _workspaceManager.GetCurrentDocumentSemanticModel(did1);
            var treeNodes = model.SyntaxTree.GetRoot().DescendantNodes();

            var testClass1Declaration = treeNodes.OfType<ClassDeclarationSyntax>().Single(node => node.Identifier.ToString().Equals("TestClass1"));
            var classSymbol = model.GetDeclaredSymbol(testClass1Declaration);
            var requiredNamespaces = model.GetNamespacesReferencedByType(testClass1Declaration);

            Assert.False(requiredNamespaces.Contains(classSymbol?.ContainingNamespace?.ToDisplayString()));
        }

        [Test]
        public async Task GetNamespacesReferencedByType_Contains_Namespaces_In_Other_Files()
        {
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedDocument();
            _workspaceManager.NotifyNewExpectedDocument();

            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            var did1 = _workspaceManager.AddDocument(pid1, "TestDocumentName1", TestDocument1Of2Text);
            var did2 = _workspaceManager.AddDocument(pid1, "TestDocumentName2", TestDocument2Of2Text);

            var model1 = await _workspaceManager.GetCurrentDocumentSemanticModel(did1);
            var model2 = await _workspaceManager.GetCurrentDocumentSemanticModel(did2);
            var treeNodes1 = model1.SyntaxTree.GetRoot().DescendantNodes();
            var treeNodes2 = model2.SyntaxTree.GetRoot().DescendantNodes();

            var class1Declaration = treeNodes1.OfType<ClassDeclarationSyntax>().Single();
            var class2Declaration = treeNodes2.OfType<ClassDeclarationSyntax>().Single();

            var class2Symbol = model2.GetDeclaredSymbol(class2Declaration);
            var requiredNamespaces = model1.GetNamespacesReferencedByType(class1Declaration);

            Assert.True(requiredNamespaces.Contains(class2Symbol?.ContainingNamespace?.ToDisplayString()));
        }

        [Test]
        public async Task GetNamespacesReferencedByType_Contains_Namespace_Used_By_Base_Class()
        {
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedDocument();

            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            var did1 = _workspaceManager.AddDocument(pid1, "TestDocumentName1", TestDocumentClassExtensionText);

            var model = await _workspaceManager.GetCurrentDocumentSemanticModel(did1);
            var treeNodes = model.SyntaxTree.GetRoot().DescendantNodes();

            var class1Declaration = treeNodes.OfType<ClassDeclarationSyntax>().Single(node => node.Identifier.ToString().Equals("TestClass1"));
            var class2Declaration = treeNodes.OfType<ClassDeclarationSyntax>().Single(node => node.Identifier.ToString().Equals("TestClass2"));

            var class2Symbol = model.GetDeclaredSymbol(class2Declaration);
            var requiredNamespaces = model.GetNamespacesReferencedByType(class1Declaration);

            Assert.True(requiredNamespaces.Contains(class2Symbol?.ContainingNamespace?.ToDisplayString()));
        }

        [Test]
        public async Task GetNamespacesReferencedByType_Contains_Namespaces_Used_By_Implemented_Interfaces()
        {
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedDocument();

            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            var did1 = _workspaceManager.AddDocument(pid1, "TestDocumentName1", TestDocumentInterfaceMultipleImplementationText);

            var model = await _workspaceManager.GetCurrentDocumentSemanticModel(did1);
            var treeNodes = model.SyntaxTree.GetRoot().DescendantNodes();

            var class1Declaration = treeNodes.OfType<ClassDeclarationSyntax>().Single(node => node.Identifier.ToString().Equals("TestClass1"));
            var interface1Declaration = treeNodes.OfType<InterfaceDeclarationSyntax>().Single(node => node.Identifier.ToString().Equals("ITestClass2"));
            var interface2Declaration = treeNodes.OfType<InterfaceDeclarationSyntax>().Single(node => node.Identifier.ToString().Equals("ITestClass3"));

            var interface1Symbol = model.GetDeclaredSymbol(interface1Declaration);
            var interface2Symbol = model.GetDeclaredSymbol(interface2Declaration);
            var requiredNamespaces = model.GetNamespacesReferencedByType(class1Declaration);

            Assert.True(requiredNamespaces.Contains(interface1Symbol?.ContainingNamespace?.ToDisplayString()));
            Assert.True(requiredNamespaces.Contains(interface2Symbol?.ContainingNamespace?.ToDisplayString()));
        }

        [Test]
        public async Task GetNamespacesReferencedByType_Contains_Namespaces_Used_In_Class_Body()
        {
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedDocument();

            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            var did1 = _workspaceManager.AddDocument(pid1, "TestDocumentName1", TestDocumentMethodCallText);

            var model = await _workspaceManager.GetCurrentDocumentSemanticModel(did1);
            var treeNodes = model.SyntaxTree.GetRoot().DescendantNodes();

            var class1Declaration = treeNodes.OfType<ClassDeclarationSyntax>().Single(node => node.Identifier.ToString().Equals("TestClass1"));
            var class2Declaration = treeNodes.OfType<ClassDeclarationSyntax>().Single(node => node.Identifier.ToString().Equals("TestClass2"));

            var class2Symbol = model.GetDeclaredSymbol(class2Declaration);
            var requiredNamespaces = model.GetNamespacesReferencedByType(class1Declaration);

            Assert.True(requiredNamespaces.Contains(class2Symbol?.ContainingNamespace?.ToDisplayString()));
        }

        [Test]
        public async Task GetAllInheritedBaseTypes_Contains_Immediate_Base_Class()
        {
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedDocument();

            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            var did1 = _workspaceManager.AddDocument(pid1, "TestDocumentName1", TestDocumentClassExtensionText);

            var model = await _workspaceManager.GetCurrentDocumentSemanticModel(did1);
            var treeNodes = model.SyntaxTree.GetRoot().DescendantNodes();

            var class1Declaration = treeNodes.OfType<ClassDeclarationSyntax>().Single(node => node.Identifier.ToString().Equals("TestClass1"));
            var class2Declaration = treeNodes.OfType<ClassDeclarationSyntax>().Single(node => node.Identifier.ToString().Equals("TestClass2"));

            var class1Symbol = model.GetDeclaredSymbol(class1Declaration);
            var class2Symbol = model.GetDeclaredSymbol(class2Declaration);
            var inheritedBaseTypes = class1Symbol.GetAllInheritedBaseTypes();

            Assert.True(inheritedBaseTypes.Contains(class2Symbol));
        }

        [Test]
        public async Task GetAllInheritedBaseTypes_Contains_Deeper_Base_Classes()
        {
            _workspaceManager.NotifyNewExpectedProject();
            _workspaceManager.NotifyNewExpectedDocument();

            var pid1 = _workspaceManager.CreateProjectFile("TestProjectName1");
            var did1 = _workspaceManager.AddDocument(pid1, "TestDocumentName1", TestDocumentStackedBaseClassesText);

            var model = await _workspaceManager.GetCurrentDocumentSemanticModel(did1);
            var treeNodes = model.SyntaxTree.GetRoot().DescendantNodes();

            var class1Declaration = treeNodes.OfType<ClassDeclarationSyntax>().Single(node => node.Identifier.ToString().Equals("TestClass1"));
            var class2Declaration = treeNodes.OfType<ClassDeclarationSyntax>().Single(node => node.Identifier.ToString().Equals("TestClass2"));
            var class3Declaration = treeNodes.OfType<ClassDeclarationSyntax>().Single(node => node.Identifier.ToString().Equals("TestClass3"));

            var class1Symbol = model.GetDeclaredSymbol(class1Declaration);
            var class2Symbol = model.GetDeclaredSymbol(class2Declaration);
            var class3Symbol = model.GetDeclaredSymbol(class3Declaration);
            var inheritedBaseTypes = class1Symbol.GetAllInheritedBaseTypes();

            Assert.True(inheritedBaseTypes.Contains(class2Symbol));
            Assert.True(inheritedBaseTypes.Contains(class3Symbol));
        }
    }
}
