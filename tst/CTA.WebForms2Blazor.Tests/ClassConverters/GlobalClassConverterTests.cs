using CTA.WebForms2Blazor.ClassConverters;
using CTA.WebForms2Blazor.Services;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTA.WebForms2Blazor.Tests.ClassConverters
{
    public class GlobalClassConverterTests
    {
        private const string InputRelativePath = "Global.asax.cs";
        private const string ExpectedOutputPath = "Startup.cs";

        private const string InputComplexClassText =
@"namespace ProjectNamespace
{
    public class Global
    {
        private int y = 11;
        public int Z { get; set; }

        protected void Application_Start(object sender, EventArgs e)
        {
            var x = 10;
            x *= 2;
            Z = y;
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            TestMethod1();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            TestMethod1();
        }

        public void TestMethod1()
        {
            y -= 1;
        }
    }
}";
        private const string ExpectedOutputComplexClassText =
@"using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectNamespace
{
    public class Startup
    {
        private int y = 11;
        public IConfiguration Configuration
        {
            get;
        }

        public IWebHostEnvironment Env
        {
            get;
        }

        public int Z
        {
            get;
            set;
        }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage(""/_Host"");
            });
            if (Env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            
            // The following lines were extracted from Application_Start
            var x = 10;
            x *= 2;
            Z = y;
            // This code replaces the original handling
            // of the BeginRequest event
            app.Use(async (context, next) =>
            {
                TestMethod1();
                await next();
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
             // Did not attempt to migrate service layer
            // and configure depenency injection in ConfigureServices(),
            // this must be done manually
        }

        public void TestMethod1()
        {
            y -= 1;
        }
        // Unable to migrate the following code, as a result it was removed
        // protected void Session_Start(object sender, EventArgs e)
        // {
        //     TestMethod1();
        // }
    }
}";

        private GlobalClassConverter _converter;

        [SetUp]
        public void SetUp()
        {
            _converter = new GlobalClassConverter(InputRelativePath,
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
        public async Task MigrateClassAsync_Correctly_Builds_Complex_Startup_Class()
        {
            var complexSyntaxTree = SyntaxFactory.ParseSyntaxTree(InputComplexClassText);
            var complexSemanticModel = CSharpCompilation.Create("TestCompilation", new[] { complexSyntaxTree }).GetSemanticModel(complexSyntaxTree);
            var complexClassDec = complexSyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
            var complexTypeSymbol = complexSemanticModel.GetDeclaredSymbol(complexClassDec);

            var complexConverter = new GlobalClassConverter(InputRelativePath,
                ClassConverterSetupFixture.TestProjectDirectoryPath,
                complexSemanticModel,
                complexClassDec,
                complexTypeSymbol,
                new LifecycleManagerService(),
                new TaskManagerService());

            var fileInfo = (await complexConverter.MigrateClassAsync()).Single();
            var fileText = Encoding.UTF8.GetString(fileInfo.FileBytes);

            Assert.AreEqual(ExpectedOutputComplexClassText, fileText);
        }
    }
}
