using NUnit.Framework;
using System;
using System.Collections.Generic;
using CTA.WebForms.Factories;
using CTA.WebForms.ClassConverters;
using CTA.WebForms.Services;
using Microsoft.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CTA.Rules.Metrics;
using CTA.WebForms.Metrics;

namespace CTA.WebForms.Tests.Factories
{
    [TestFixture]
    public class ClassConverterFactoryTests
    {
        private const string SystemWebDllName = "System.Web.dll";
        private const string DocumentMultiClassText =
            @"namespace TestNamespace1 {
                public class TestClass1 { }
                public class TestClass2 { }
                public class TestClass3 { }
            }
            namespace TestNamespace2 {
                public class TestClass4 { }
            }";

        private string TestProjectPath => Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        private string TestingAssembliesPath => Path.Combine(TestProjectPath, "TestingArea", "TestFiles", "TestAssemblies");

        private IEnumerable<MetadataReference> _metadataReferences;
        private ClassConverterFactory _classConverterFactory;
        private WorkspaceManagerService _workspaceManager;
        private ProjectId _primaryProjectId;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _metadataReferences = new List<MetadataReference>() {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(TestingAssembliesPath, SystemWebDllName))
            };
        }

        [SetUp]
        public void SetUp()
        {
            WebFormMetricContext metricContext = new WebFormMetricContext();
            _classConverterFactory = new ClassConverterFactory(
                string.Empty,
                new LifecycleManagerService(),
                new TaskManagerService(),
                new CodeBehindReferenceLinkerService(),
                metricContext);
            _workspaceManager = new WorkspaceManagerService();
            _workspaceManager.CreateSolutionFile();
            _workspaceManager.NotifyNewExpectedProject();
            _primaryProjectId = _workspaceManager.CreateProjectFile("TestProjectName", metadataReferences: _metadataReferences);
        }

        [TestCase(typeof(GlobalClassConverter), "Global.asax.cs",
            @"namespace TestNamespace {
                using System.Web;

                public class Global : HttpApplication { }
            }")]
        [TestCase(typeof(HttpHandlerClassConverter), "TestHandler.cs",
            @"namespace TestNamespace {
                using System.Web;

                public class TestHandler : IHttpHandler { }
            }")]
        [TestCase(typeof(HttpModuleClassConverter), "TestModule.cs",
            @"namespace TestNamespace {
                using System.Web;

                public class TestModule : IHttpModule { }
            }")]
        [TestCase(typeof(PageCodeBehindClassConverter), "TestPage.aspx.cs",
            @"namespace TestNamespace {
                using System.Web.UI;

                public class TestPage : Page { }
            }")]
        [TestCase(typeof(ControlCodeBehindClassConverter), "TestControl.ascx.cs",
            @"namespace TestNamespace {
                using System.Web.UI;

                public class TestComponent : UserControl { }
            }")]
        [TestCase(typeof(MasterPageCodeBehindClassConverter), "TestMasterPage.Master.cs",
            @"namespace TestNamespace {
                using System.Web.UI;

                public class TestMasterPage : MasterPage { }
            }")]
        [TestCase(typeof(UnknownClassConverter), "TestUnknownClass.cs",
            @"namespace TestNamespace {
                public class TestUnknownClass { }
            }")]
        public async Task Build_Recognizes_Class_Type(Type targetType, string testFileName, string testDocumentText)
        {
            _workspaceManager.NotifyNewExpectedDocument();

            var testDocumentPath = Path.Combine("C:", "Directory1", "Directory2", testFileName);
            var did = _workspaceManager.AddDocument(_primaryProjectId, "TestDocument", testDocumentText);
            var model = await _workspaceManager.GetCurrentDocumentSemanticModel(did);
            var classConverter = _classConverterFactory.BuildMany(new Dictionary<string, string>() { { "TestNamespace.TestPage" , "PageCodeBehindClassConverter" } }, testDocumentPath, model).Single();

            Assert.IsInstanceOf(targetType, classConverter);
        }

        [Test]
        public async Task BuildMany_Splits_Classes()
        {
            _workspaceManager.NotifyNewExpectedDocument();

            var testDocumentPath = Path.Combine("C:", "Directory1", "Directory2", "TestDocumentName.cs");
            var did = _workspaceManager.AddDocument(_primaryProjectId, "TestDocument", DocumentMultiClassText);
            var model = await _workspaceManager.GetCurrentDocumentSemanticModel(did);
            var classConverters = _classConverterFactory.BuildMany(new Dictionary<string, string>(), testDocumentPath, model);

            Assert.AreEqual(4, classConverters.Count());
        }
    }
}
