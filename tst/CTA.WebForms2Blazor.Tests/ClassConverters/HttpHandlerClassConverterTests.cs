using CTA.WebForms2Blazor.ClassConverters;
using CTA.WebForms2Blazor.Services;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.Rules.Metrics;
using CTA.WebForms2Blazor.Metrics;

namespace CTA.WebForms2Blazor.Tests.ClassConverters
{
    public class HttpHandlerClassConverterTests
    {
        private const string InputComplexClassText =
@"namespace ProjectNamespace
{
    public class MyHandler : IHttpHandler
    {
        public bool IsReusable { get { return true; } }

        public void ProcessRequest(HttpContext context)
        {
            string response = GenerateResponse(context);

            context.Response.ContentType = GetContentType();
            context.Response.Output.Write(response);
        }

        private string GenerateResponse(HttpContext context)
        {
            string title = context.Request.QueryString[""title""];
            return string.Format(""Title of the report: {0}"", title);
        }

        private string GetContentType()
        {
            return ""text/plain"";
        }
    }
}";
        private const string ExpectedOutputComplexClassText =
@"using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ProjectNamespace
{
    public class MyHandler
    {
        private readonly RequestDelegate _next;
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public MyHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // The following lines were extracted from ProcessRequest
            string response = GenerateResponse(context);
            context.Response.ContentType = GetContentType();
            context.Response.Output.Write(response);
        }

        private string GenerateResponse(HttpContext context)
        {
            string title = context.Request.QueryString[""title""];
            return string.Format(""Title of the report: {0}"", title);
        }

        private string GetContentType()
        {
            return ""text/plain"";
        }
    }
}";

        private static string InputRelativePath => Path.Combine(ClassConverterSetupFixture.TestProjectNestedDirectoryName, "HttpHandler.cs");
        private static string ExpectedOutputPath => Path.Combine(
            "Middleware",
            ClassConverterSetupFixture.TestProjectNestedDirectoryName,
            $"{ClassConverterSetupFixture.TestClassName}.cs");

        private HttpHandlerClassConverter _converter;

        [SetUp]
        public void SetUp()
        {
            MetricsContext context = new MetricsContext(ClassConverterSetupFixture.TestProjectDirectoryPath);
            _converter = new HttpHandlerClassConverter(InputRelativePath,
                ClassConverterSetupFixture.TestProjectDirectoryPath,
                ClassConverterSetupFixture.TestSemanticModel,
                ClassConverterSetupFixture.TestClassDec,
                ClassConverterSetupFixture.TestTypeSymbol,
                new LifecycleManagerService(),
                new TaskManagerService(),
                new WebFormMetricContext(context, ClassConverterSetupFixture.TestProjectDirectoryPath));
        }

        [Test]
        public async Task MigrateClassAsync_Maps_New_Relative_Path_To_Correct_Location()
        {
            var fileInfo = (await _converter.MigrateClassAsync()).Single();

            Assert.AreEqual(ExpectedOutputPath, fileInfo.RelativePath);
        }


        [Test]
        public async Task MigrateClassAsync_Correctly_Builds_Complex_Handler_Middleware_Class()
        {
            var complexSyntaxTree = SyntaxFactory.ParseSyntaxTree(InputComplexClassText);
            var complexSemanticModel = CSharpCompilation.Create("TestCompilation", new[] { complexSyntaxTree }).GetSemanticModel(complexSyntaxTree);
            var complexClassDec = complexSyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
            var complexTypeSymbol = complexSemanticModel.GetDeclaredSymbol(complexClassDec);

            MetricsContext context = new MetricsContext(InputRelativePath);
            var complexConverter = new HttpHandlerClassConverter(InputRelativePath,
                ClassConverterSetupFixture.TestProjectDirectoryPath,
                complexSemanticModel,
                complexClassDec,
                complexTypeSymbol,
                new LifecycleManagerService(),
                new TaskManagerService(),
                new WebFormMetricContext(context, ClassConverterSetupFixture.TestProjectDirectoryPath));

            var fileInfo = (await complexConverter.MigrateClassAsync()).Single();
            var fileText = Encoding.UTF8.GetString(fileInfo.FileBytes);

            Assert.AreEqual(ExpectedOutputComplexClassText, fileText);
        }
    }
}
