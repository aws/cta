
namespace CTA.Rules.Test.Models
{
    public class WebFormsFullExpectedOutputConstants
    {
        public const string HostFile = @"@page ""/""
@namespace WebFormsFull
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers

<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8""/>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Unknown_app_title</title>
    <base href=""~/""/>
    <link rel=""stylesheet"" href=""Content\base.css"" />
    <link rel=""stylesheet"" href=""Content\bootstrap-grid.css"" />
    <link rel=""stylesheet"" href=""Content\bootstrap-grid.min.css"" />
    <link rel=""stylesheet"" href=""Content\bootstrap-reboot.css"" />
    <link rel=""stylesheet"" href=""Content\bootstrap-reboot.min.css"" />
    <link rel=""stylesheet"" href=""Content\bootstrap.css"" />
    <link rel=""stylesheet"" href=""Content\bootstrap.min.css"" />
    <link rel=""stylesheet"" href=""Content\custom.css"" />
    <link rel=""stylesheet"" href=""Content\site.css"" />
</head>
<body>
    <app>@(await Html.RenderComponentAsync<App>(RenderMode.ServerPrerendered))</app>

    <script src=""_framework/blazor.server.js""></script>
</body>
</html>";

        public const string ImportsFile = @"@using BlazorWebFormsComponents
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using System.Net.Http";

        public const string ActivityIdHelperFile = @"using Autofac;
using Autofac.Integration.Web;
using WebFormsFull.Models;
using WebFormsFull.Models.Infrastructure;
using WebFormsFull.Modules;
using log4net;
using System;
using System.Configuration;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace WebFormsFull
{
    public class ActivityIdHelper
    {
        public override string ToString()
        {
            if (Trace.CorrelationManager.ActivityId == Guid.Empty)
            {
                Trace.CorrelationManager.ActivityId = Guid.NewGuid();
            }

            return Trace.CorrelationManager.ActivityId.ToString();
        }
    }
}";
        
        public const string AppFile = @"<Router AppAssembly=""typeof(Program).Assembly"">
    <Found Context=""routeData"">
        <RouteView RouteData=""routeData"" />
    </Found>
    <NotFound>
        <h1>Page not found</h1>
        <p>Sorry, but there's nothing here!</p>
    </NotFound>
</Router>";

        public const string AppSettings = @"{
  ""appsettings"": {
    ""UseMockData"": ""true"",
    ""UseCustomizationData"": ""false""
  },
  ""ConnectionStrings"": {
    ""LocalSqlServer"": ""data source=.\\SQLEXPRESS;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|aspnetdb.mdf;User Instance=true"",
    ""CatalogDBContext"": ""Data Source=(localdb)\\MSSQLLocalDB; Initial Catalog=Microsoft.eShopOnContainers.Services.CatalogDb; Integrated Security=True; MultipleActiveResultSets=True;""
  },
  ""Logging"": {
    ""LogLevel"": {
      ""Default"": ""Information"",
      ""Microsoft"": ""Warning"",
      ""Microsoft.Hosting.Lifetime"": ""Information""
    }
  },
  ""AllowedHosts"": ""*"",
  ""Kestrel"": {
    ""Endpoints"": {
      ""Http"": {
        ""Url"": ""http://0.0.0.0:80""
      },
      ""Https"": {
        ""Url"": ""https://0.0.0.0:443"",
        ""Certificate"": {
          ""Path"": ""<please provide path to cert>"",
          ""Password"": ""<certificate password>""
        }
      }
    }
  }
}";

        public const string ProgramFile = @"using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace WebFormsFull
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
}";

        public const string StartupFile =
@"using Autofac;
using Autofac.Integration.Web;
using WebFormsFull.Models;
using WebFormsFull.Models.Infrastructure;
using WebFormsFull.Modules;
using log4net;
using System;
using System.Configuration;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using eShopLegacyWebForms.HttpModules;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebFormsFull
{
    public class Startup
    {
        RequestDelegate _next = null;
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static IContainerProvider _containerProvider;
        IContainer container;
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Env { get; }

        public IContainerProvider ContainerProvider
        {
            get
            {
                return _containerProvider;
            }
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

            
            // Code that runs on application startup
            // The following lines were extracted from Application_Start
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ConfigureContainer();
            ConfigDataBase();
            // This code replaces the original handling
            // of the BeginRequest event
            app.Use(async (context, next) =>
            {
                _log.Debug(""Application_BeginRequest"");
                await next();
            });
            // This code replaces the original handling
            // of the ResolveRequestCache event
            // This class was generated using a portion
            // of TestProperHttpModuleAlternate
            app.UseMiddleware<TestProperHttpModuleAlternateResolveRequestCache>();
            // This code replaces the original handling
            // of the PostResolveRequestCache event
            // This class was generated using a portion
            // of TestProperHttpModuleAlternate
            app.UseMiddleware<TestProperHttpModuleAlternatePostResolveRequestCache>();
            // This code replaces the original handling
            // of the PreRequestHandlerExecute event
            app.Use(async (context, next) =>
            {
                _log.Debug(""Application_PreRequestHandlerExecute"");
                await next();
            });
            // This code replaces the original handling
            // of the EndRequest event
            app.UseMiddleware<TestProperHttpModule>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
             // Did not attempt to migrate service layer
            // and configure dependency injection in ConfigureServices(),
            // this must be done manually
        }

        /// <summary>
        /// http://docs.autofac.org/en/latest/integration/webforms.html
        /// </summary>
        private void ConfigureContainer()
        {
            var builder = new ContainerBuilder();
            var mockData = bool.Parse(ConfigurationManager.Configuration.GetSection(""appSettings"")[""UseMockData""]);
            builder.RegisterModule(new ApplicationModule(mockData));
            container = builder.Build();
            _containerProvider = new ContainerProvider(container);
        }

        private void ConfigDataBase()
        {
            var mockData = bool.Parse(ConfigurationManager.Configuration.GetSection(""appSettings"")[""UseMockData""]);
            if (!mockData)
            {
                Database.SetInitializer<CatalogDBContext>(container.Resolve<CatalogDBInitializer>());
            }
        }
        // Unable to migrate the following code, as a result it was removed
        // /// <summary>
/// Track the machine name and the start time for the session inside the current session
/// </summary>
protected void Session_Start(Object sender, EventArgs e)
        // {
        //     HttpContext.Current.Session[""MachineName""] = Environment.MachineName;
        //     HttpContext.Current.Session[""SessionStartTime""] = DateTime.Now;
        // }
    }
}";

        public const string WebRequestInfoFile = @"using Autofac;
using Autofac.Integration.Web;
using WebFormsFull.Models;
using WebFormsFull.Models.Infrastructure;
using WebFormsFull.Modules;
using log4net;
using System;
using System.Configuration;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace WebFormsFull
{
    public class WebRequestInfo
    {
        public override string ToString()
        {
            return HttpContext.Current?.Request?.RawUrl + "", "" + HttpContext.Current?.Request?.UserAgent;
        }
    }
}";

        public const string DefaultRazorCsFile = @"using WebFormsFull.Models;
using WebFormsFull.Services;
using WebFormsFull.ViewModel;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace WebFormsFull
{
    public partial class _Default : ComponentBase, IDisposable
    {
        RequestDelegate _next = null;
        public const int DefaultPageIndex = 0;
        public const int DefaultPageSize = 2;
        public ICatalogService CatalogService { get; set; }

        protected int EventsTriggered { get; set; }

        protected PaginatedItemsViewModel<CatalogItem> Model { get; set; }

        protected List<People> PeopleModel { get; set; }

        protected void AddPerson(object sender, EventArgs e)
        {
            PeopleModel.Add(new People(""New"", ""New"", ""New""));
             // PeopleGrid.DataBind();
        }

        public _Default(RequestDelegate next)
        {
        }

        public Object PeopleGrid_DataSource { get; set; }

        public Object productList_DataSource { get; set; }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            // This code replaces the original handling
            // of the PreInit event
            EventsTriggered += 1;
            DataBind();
            
            // This code replaces the original handling
            // of the Init event
            EventsTriggered += 1;
            DataBind();
            
            await base.SetParametersAsync(parameters);
        }

        protected override void OnInitialized()
        {
            // This code replaces the original handling
            // of the InitComplete event
            EventsTriggered += 1;
            DataBind();
            
            // This code replaces the original handling
            // of the PreLoad event
            EventsTriggered += 1;
            DataBind();
            
            // This code replaces the original handling
            // of the Load event
            EventsTriggered += 1;
            Model = CatalogService.GetCatalogItemsPaginated(DefaultPageSize, DefaultPageIndex);
            PeopleModel = new List<People>()
            {
                new People(""Andy"", ""Wayne"", ""PG""),
                new People(""Bill"", ""Johnson"", ""SD""),
                new People(""Caroline"", ""Barry"", ""Manager"")
            };
            PeopleGrid_DataSource = PeopleModel;
             // PeopleGrid.DataBind();
            productList_DataSource = Model.Data;
             // productList.DataBind();
            DataBind();
            
            // This code replaces the original handling
            // of the LoadComplete event
            EventsTriggered += 1;
            DataBind();
        }

        protected override void OnParametersSet()
        {
            // This code replaces the original handling
            // of the PreRender event
            EventsTriggered += 1;
            DataBind();
            
            // This code replaces the original handling
            // of the PreRenderComplete event
            EventsTriggered += 1;
            DataBind();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            // This code replaces the original handling
            // of the SaveStateComplete event
            EventsTriggered += 1;
            DataBind();
            
            // This code replaces the original handling
            // of the Render event
            EventsTriggered += 1;
            DataBind();
        }

        public void Dispose()
        {
            // This code replaces the original handling
            // of the Unload event
            EventsTriggered += 1;
            DataBind();
        }
    }
}";

        public const string OtherPageRazorCsFile = @"using log4net;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace WebFormsFull
{
    public partial class OtherPage : ComponentBase
    {
        RequestDelegate _next = null;
        public OtherPage(RequestDelegate next)
        {
        }
    }
}";

        public const string TestHttpHandlerFile = @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace eShopLegacyWebForms.HttpHandlers
{
    public class TestHttpHandler
    {
        private readonly RequestDelegate _next;
        RequestDelegate _next = null;
        public bool IsReusable => false;
        public TestHttpHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Could not identify ProcessRequest method,
            // dependant middleware Invoke method population
            // operation must be done manually
            
        }

        public async Task Invoke(HttpContext context)
        {
            // Do some request handling
            context.Response.StatusCode = 200;
        }
    }
}";

        public const string TestProperHttpModuleFile = @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace eShopLegacyWebForms.HttpModules
{
    public class TestProperHttpModule
    {
        private readonly RequestDelegate _next;
        RequestDelegate _next = null;
        public TestProperHttpModule(RequestDelegate next)
        {
            _next = next;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next.Invoke(context);
            var application = (HttpApplication)source;
            var endContext = application.Context;
        }

        public void Dispose()
        {
        // Do cleanup
        }
    }
}";

        public const string TestProperHttpModuleAlternateResolveRequestCacheFile = @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace eShopLegacyWebForms.HttpModules
{
    // Heavy modifications likely necessary, please
    // review
    // This class was generated using a portion
    // of TestProperHttpModuleAlternate
    public class TestProperHttpModuleAlternateResolveRequestCache
    {
        private readonly RequestDelegate _next;
        RequestDelegate _next = null;
        public string X { get; set; }

        public TestProperHttpModuleAlternateResolveRequestCache(RequestDelegate next)
        {
            _next = next;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Do some stuff here
            X = source.GetType().ToString();
            await _next.Invoke(context);
        }

        public void Dispose()
        {
        // Do cleanup
        }
    }
}";

        public const string TestProperHttpModuleAlternatePostResolveRequestCacheFile = @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace eShopLegacyWebForms.HttpModules
{
    // Heavy modifications likely necessary, please
    // review
    // This class was generated using a portion
    // of TestProperHttpModuleAlternate
    public class TestProperHttpModuleAlternatePostResolveRequestCache
    {
        private readonly RequestDelegate _next;
        RequestDelegate _next = null;
        public string X { get; set; }

        public TestProperHttpModuleAlternatePostResolveRequestCache(RequestDelegate next)
        {
            _next = next;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Do some other stuff over here
            int x = 12;
            int y = x + 1;
            x = y - 1;
            X = x.ToString() + X;
            await _next.Invoke(context);
        }

        public void Dispose()
        {
        // Do cleanup
        }
    }
}";

        public const string TestImproperHttpModuleFile = @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace eShopLegacyWebForms.HttpModules
{
    public class TestImproperHttpModule
    {
        private readonly RequestDelegate _next;
        RequestDelegate _next = null;
        public TestImproperHttpModule(RequestDelegate next)
        {
            _next = next;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next.Invoke(context);
            // Could not identify lifecycle hook method,
            // dependant middleware Invoke method population
            // operation must be done manually
            
        }

        public void Dispose()
        {
        // Do cleanup
        }
    }
}";
    }
}