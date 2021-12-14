using CTA.WebForms2Blazor.ClassConverters;
using CTA.WebForms2Blazor.Services;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.Metrics;

namespace CTA.WebForms2Blazor.Tests.ClassConverters
{
    public class HttpModuleClassConverterTests
    {
        private const string InputComplexClassText =
@"using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ProjectNamespace
{
    public class MyModule : IHttpModule
    {
        public void Init(HttpApplication application)
        {
            application.BeginRequest += (new EventHandler(this.Application_BeginRequest));
            application.EndRequest += (new EventHandler(this.Application_EndRequest));
        }

        private void Application_BeginRequest(Object source, EventArgs e)
        {
            HttpContext context = ((HttpApplication)source).Context;
        }

        private void Application_EndRequest(Object source, EventArgs e)
        {
            HttpContext context = ((HttpApplication)source).Context;
        }
    }
}";
        private const string InputClassNoEventHandlerObjectCreationText =
@"using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ProjectNamespace
{
    public class MyModule : IHttpModule
    {
        public void Init(HttpApplication application)
        {
            application.BeginRequest += this.Application_BeginRequest;
        }

        private void Application_BeginRequest(Object source, EventArgs e)
        {
            HttpContext context = ((HttpApplication)source).Context;
        }
    }
}";
        private const string InputClassNoEventHandlerObjectCreationTextOrMemberAccess =
@"using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ProjectNamespace
{
    public class MyModule : IHttpModule
    {
        public void Init(HttpApplication application)
        {
            application.BeginRequest += Application_BeginRequest;
        }

        private void Application_BeginRequest(Object source, EventArgs e)
        {
            HttpContext context = ((HttpApplication)source).Context;
        }
    }
}";
        private const string ExpectedOutputComplexClassText1 =
@"using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ProjectNamespace
{
    // Heavy modifications likely necessary, please
    // review
    // This class was generated using a portion
    // of MyModule
    public class MyModuleBeginRequest
    {
        private readonly RequestDelegate _next;
        public MyModuleBeginRequest(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            HttpContext context = ((HttpApplication)source).Context;
            await _next.Invoke(context);
        }
    }
}";
        private const string ExpectedOutputComplexClassText2 =
@"using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ProjectNamespace
{
    // Heavy modifications likely necessary, please
    // review
    // This class was generated using a portion
    // of MyModule
    public class MyModuleEndRequest
    {
        private readonly RequestDelegate _next;
        public MyModuleEndRequest(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next.Invoke(context);
            HttpContext context = ((HttpApplication)source).Context;
        }
    }
}";
        private const string ExpectedOutputNoEventHandlerObjectCreation =
@"using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ProjectNamespace
{
    public class MyModule
    {
        private readonly RequestDelegate _next;
        public MyModule(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            HttpContext context = ((HttpApplication)source).Context;
            await _next.Invoke(context);
        }
    }
}";

        private static string InputRelativePath => Path.Combine(ClassConverterSetupFixture.TestProjectNestedDirectoryName, "HttpModule.cs");
        private static string ExpectedOutputPath => Path.Combine(
            "Middleware",
            ClassConverterSetupFixture.TestProjectNestedDirectoryName,
            $"{ClassConverterSetupFixture.TestClassName}.cs");

        private HttpModuleClassConverter _converter;

        [SetUp]
        public void SetUp()
        {
            _converter = new HttpModuleClassConverter(InputRelativePath,
                ClassConverterSetupFixture.TestProjectDirectoryPath,
                ClassConverterSetupFixture.TestSemanticModel,
                ClassConverterSetupFixture.TestClassDec,
                ClassConverterSetupFixture.TestTypeSymbol,
                new LifecycleManagerService(),
                new TaskManagerService(),
                new WebFormMetricContext());
        }

        [Test]
        public async Task MigrateClassAsync_Maps_New_Relative_Path_To_Correct_Location()
        {
            var fileInfo = (await _converter.MigrateClassAsync()).Single();

            Assert.AreEqual(ExpectedOutputPath, fileInfo.RelativePath);
        }

        [Test]
        public async Task MigrateClassAsync_Correctly_Builds_Complex_Module_Middleware_Class()
        {
            var complexSyntaxTree = SyntaxFactory.ParseSyntaxTree(InputComplexClassText);
            var complexSemanticModel = CSharpCompilation.Create("TestCompilation", new[] { complexSyntaxTree }).GetSemanticModel(complexSyntaxTree);
            var complexClassDec = complexSyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
            var complexTypeSymbol = complexSemanticModel.GetDeclaredSymbol(complexClassDec);

            var complexConverter = new HttpModuleClassConverter(InputRelativePath,
                ClassConverterSetupFixture.TestProjectDirectoryPath,
                complexSemanticModel,
                complexClassDec,
                complexTypeSymbol,
                new LifecycleManagerService(),
                new TaskManagerService(),
                new WebFormMetricContext());

            var fileInfo = await complexConverter.MigrateClassAsync();

            Assert.AreEqual(2, fileInfo.Count());

            var fileText1 = Encoding.UTF8.GetString(fileInfo.First().FileBytes);
            var fileText2 = Encoding.UTF8.GetString(fileInfo.Last().FileBytes);

            Assert.AreEqual(ExpectedOutputComplexClassText1, fileText1);
            Assert.AreEqual(ExpectedOutputComplexClassText2, fileText2);
        }

        [Test]
        public async Task MigrateClassAsync_Correctly_Identifies_Events_Without_EventHandler_Object_Creation()
        {
            var testSyntaxTree = SyntaxFactory.ParseSyntaxTree(InputClassNoEventHandlerObjectCreationText);
            var testSemanticModel = CSharpCompilation.Create("TestCompilation", new[] { testSyntaxTree }).GetSemanticModel(testSyntaxTree);
            var testClassDec = testSyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
            var testTypeSymbol = testSemanticModel.GetDeclaredSymbol(testClassDec);

            var testConverter = new HttpModuleClassConverter(InputRelativePath,
                ClassConverterSetupFixture.TestProjectDirectoryPath,
                testSemanticModel,
                testClassDec,
                testTypeSymbol,
                new LifecycleManagerService(),
                new TaskManagerService(),
                new WebFormMetricContext());

            var fileInfo = await testConverter.MigrateClassAsync();

            Assert.AreEqual(1, fileInfo.Count());

            var fileText1 = Encoding.UTF8.GetString(fileInfo.Single().FileBytes);

            Assert.AreEqual(ExpectedOutputNoEventHandlerObjectCreation, fileText1);
        }

        [Test]
        public async Task MigrateClassAsync_Correctly_Identifies_Events_Without_EventHandler_Object_Creation_Or_Member_Access()
        {
            var testSyntaxTree = SyntaxFactory.ParseSyntaxTree(InputClassNoEventHandlerObjectCreationTextOrMemberAccess);
            var testSemanticModel = CSharpCompilation.Create("TestCompilation", new[] { testSyntaxTree }).GetSemanticModel(testSyntaxTree);
            var testClassDec = testSyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
            var testTypeSymbol = testSemanticModel.GetDeclaredSymbol(testClassDec);

            var testConverter = new HttpModuleClassConverter(InputRelativePath,
                ClassConverterSetupFixture.TestProjectDirectoryPath,
                testSemanticModel,
                testClassDec,
                testTypeSymbol,
                new LifecycleManagerService(),
                new TaskManagerService(),
                new WebFormMetricContext());

            var fileInfo = await testConverter.MigrateClassAsync();

            Assert.AreEqual(1, fileInfo.Count());

            var fileText1 = Encoding.UTF8.GetString(fileInfo.Single().FileBytes);

            Assert.AreEqual(ExpectedOutputNoEventHandlerObjectCreation, fileText1);
        }
    }
}
