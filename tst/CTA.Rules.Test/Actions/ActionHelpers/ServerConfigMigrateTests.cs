using CTA.Rules.Actions;
using CTA.Rules.Models;
using NUnit.Framework;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace CTA.Rules.Test.Actions.ActionHelpers
{
    [TestFixture]
    public class ServerConfigMigrateTests
    {
        private string _projectDir;
        private ConfigMigrate _configMigrateInstance;
        private MethodInfo _loadWebConfigMethod;
        private MethodInfo _portServerConfigMethod;
        private ProjectType _projectType;

        [SetUp]
        public void SetUp()
        {
            _projectDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var file = Path.Combine(_projectDir, "Startup.cs");
            File.WriteAllText(file, GetstartupTestTemplate());
            file = Path.Combine(_projectDir, "Program.cs");
            File.WriteAllText(file, GetProgramcsTestTemplate());

            // Get private method to invoke
            _projectType = ProjectType.Mvc;
            _configMigrateInstance = new ConfigMigrate(Directory.GetCurrentDirectory(), _projectType);
            Type _configMigrateType = _configMigrateInstance.GetType();
            _loadWebConfigMethod = TestUtils.GetPrivateMethod(_configMigrateType, "LoadWebConfig");
            _portServerConfigMethod = TestUtils.GetPrivateMethod(_configMigrateType, "PortServerConfig");
        }

        [Test]
        public void MigrateServerConfig_WhitespaceIsCorrect()
        {
            var webConfig = @"
                    <configuration>
                            <system.webServer>
                                 <modules>
			                        <add name=""AppShutDownModule"" type=""TestMvcApplication.AppShutDownModule"" />
		                        </modules>
		                        <handlers>  
			                        <add name=""AppShutDownHandler"" verb=""*"" path=""*.cshtml"" type=""TestMvcApplication.AppShutDownHandler"" />  
		                        </handlers>
                           </system.webServer>	
                </configuration>";
            InvokeTestMethod(webConfig);

            var expectedResult =
@"/* Added by CTA: Please add the correponding references..If certs are not provided for deployment communication will be on http, please remove the https section of the kestrel config in appsettings.json and also remove middleware component app.UseHttpsRedirection(); */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace
#NAMESPACEPLACEHOLDER#
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ConfigurationManager.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(""/Home/Error"");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.MapWhen(context => context.Request.Path.ToString().EndsWith("".cshtml""), appBranch =>
            {
                appBranch.UseMiddleware<TestMvcApplication.AppShutDownHandler>();
            });
            app.UseMiddleware<TestMvcApplication.AppShutDownModule>();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: ""default"", pattern: ""{controller=Home}/{action=Index}/{id?}"");
            });
        }
    }

    public class ConfigurationManager
    {
        public static IConfiguration Configuration { get; set; }
    }
}";
            var startupcsContent = File.ReadAllText(Path.Combine(_projectDir, "Startup.cs"));
            Assert.AreEqual(expectedResult, startupcsContent);
        }

        [Test]
        public void MigrateServerConfig_HttpModulesandHandlers()
        {
            var webConfig = @"
                    <configuration>
                            <system.webServer>
                                 <modules>
			                        <add name=""AppShutDownModule"" type=""TestMvcApplication.AppShutDownModule"" />
		                        </modules>
		                        <handlers>  
			                        <add name=""AppShutDownHandler"" verb=""*"" path=""*.cshtml"" type=""TestMvcApplication.AppShutDownHandler"" />  
		                        </handlers>
                           </system.webServer>	
                </configuration>";
            InvokeTestMethod(webConfig);

            var startupcsContent = File.ReadAllText(Path.Combine(_projectDir, "Startup.cs"));
            Assert.True(startupcsContent.Contains("app.UseMiddleware<TestMvcApplication.AppShutDownModule>();"));
            Assert.True(startupcsContent.Contains("appBranch.UseMiddleware<TestMvcApplication.AppShutDownHandler>();"));         
        }

        [Test]
        public void MigrateServerConfig_NoHttpModulesandHandlers()
        {
            var webConfig = @"
                    <configuration>
                            <system.webServer>
                                 <modules>
		                        </modules>
		                        <handlers>  
		                        </handlers>
                           </system.webServer>	
                </configuration>";
            InvokeTestMethod(webConfig);

            var startupcsContent = File.ReadAllText(Path.Combine(_projectDir, "Startup.cs"));
            Assert.False(startupcsContent.Contains("app.UseMiddleware"));
            Assert.False(startupcsContent.Contains("appBranch.UseMiddleware"));
        }


        [Test]
        public void MigrateServerConfig_HttpCompression()
        {
            var webConfig = @"
                    <configuration>
                            <system.webServer>
                                <httpCompression directory="" % SystemDrive %\inetpub\temp\IIS Temporary Compressed Files"">
                                    <scheme name = ""gzip"" dll = ""%Windir%\system32\inetsrv\gzip.dll"" />
                                        <dynamicTypes>
                                            <add mimeType = ""text/*"" enabled = ""true"" />               
                                        </dynamicTypes>
                                        <staticTypes>
                                            <add mimeType = ""message/*"" enabled = ""true"" />
                                        </staticTypes>
                              </httpCompression>
                           </system.webServer>	
                </configuration>";

            InvokeTestMethod(webConfig);

            var startupcsContent = File.ReadAllText(Path.Combine(_projectDir, "Startup.cs"));
            Assert.True(startupcsContent.Contains("using Microsoft.AspNetCore.ResponseCompression;"));
            Assert.True(startupcsContent.Contains("message/*"));
            Assert.True(startupcsContent.Contains("text/*"));
            Assert.True(startupcsContent.Contains("app.UseResponseCaching();"));           
        }

        [Test]
        public void MigrateServerConfig_HttpRedirect()
        {
            var webConfig = @"
                    <configuration>
                            <system.webServer>                              
                              <httpRedirect enabled=""true"" exactDestination=""true"" httpResponseStatus=""Found"">
                                      <add wildcard = ""*.php"" destination = ""/default.htm"" />
                              </httpRedirect>
                           </system.webServer>	
                </configuration>";
            InvokeTestMethod(webConfig);

            var startupcsContent = File.ReadAllText(Path.Combine(_projectDir, "Startup.cs"));
            Assert.True(startupcsContent.Contains(".php"));
            Assert.True(startupcsContent.Contains("default.htm"));
            Assert.True(startupcsContent.Contains("app.UseRewriter"));
        }

        [Test]
        public void MigrateServerConfig_Security()
        {
            var webConfig = @"
                    <configuration>
                            <system.webServer>
                                <security>
                                    <authentication>
                                        <anonymousAuthentication enabled=""false"" />
                                        <windowsAuthentication enabled = ""true"" />
                                    </authentication>
                                    <requestFiltering>
                                        <requestLimits maxAllowedContentLength=""52428800"" />
                                    </requestFiltering>
                                </security>
                           </system.webServer>	
                </configuration>";
            InvokeTestMethod(webConfig);

            var startupcsContent = File.ReadAllText(Path.Combine(_projectDir, "Startup.cs"));
            Assert.True(startupcsContent.Contains("using Microsoft.AspNetCore.Authentication.Negotiate;"));

            var programcsContent = File.ReadAllText(Path.Combine(_projectDir, "Program.cs"));        
            Assert.True(programcsContent.Contains("webBuilder.UseKestrel"));
            Assert.True(programcsContent.Contains("options.Limits.MaxRequestBodySize = 52428800"));
        }

        private void InvokeTestMethod(string webConfig)
        {
            File.WriteAllText(Path.Combine(_projectDir, "web.config"), webConfig);

            var configuration = (Configuration)_loadWebConfigMethod.Invoke(_configMigrateInstance, new object[] { _projectDir });

            // Invoke port server config method
            _portServerConfigMethod.Invoke(_configMigrateInstance, new object[] { configuration, _projectDir, _projectType });

        }


        [TearDown]
        public void Cleanup()
        {
            //clean up
            File.Delete(Path.Combine(_projectDir, "Startup.cs"));
            File.Delete(Path.Combine(_projectDir, "Program.cs"));
            File.Delete(Path.Combine(_projectDir, "web.config"));

        }

        private string GetstartupTestTemplate()
        {
            return @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace #NAMESPACEPLACEHOLDER#
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ConfigurationManager.Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(""/Home/Error"");
                app.UseHsts();
            }
            //Added Middleware
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: ""default"",
                    pattern: ""{controller=Home}/{action=Index}/{id?}"");
            });
        }
    }
    public class ConfigurationManager
    {
            public static IConfiguration Configuration { get; set; }
    }
    }
";
        }

        private string GetProgramcsTestTemplate()
        {
            return @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace #NAMESPACEPLACEHOLDER#
{

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }


}
";
        }
    }
}
