﻿using CTA.WebForms2Blazor.ClassConverters;
using CTA.WebForms2Blazor.Services;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTA.WebForms2Blazor.Tests.ClassConverters
{
    public class HttpModuleClassConverterTests
    {
        private const string InputComplexClassText =
@"namespace ProjectNamespace
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
        private const string ExpectedOutputComplexClassText1 =
@"namespace ProjectNamespace
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
@"namespace ProjectNamespace
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
                new TaskManagerService());
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
                new TaskManagerService());

            var fileInfo = await complexConverter.MigrateClassAsync();

            Assert.AreEqual(2, fileInfo.Count());

            var fileText1 = Encoding.UTF8.GetString(fileInfo.First().FileBytes);
            var fileText2 = Encoding.UTF8.GetString(fileInfo.Last().FileBytes);

            Assert.AreEqual(ExpectedOutputComplexClassText1, fileText1);
            Assert.AreEqual(ExpectedOutputComplexClassText2, fileText2);
        }
    }
}