
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
using System.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

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

        public const string StartupFile = @"using Autofac;
using Autofac.Integration.Web;
using WebFormsFull.Models;
using WebFormsFull.Models.Infrastructure;
using WebFormsFull.Modules;
using log4net;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace WebFormsFull
{
    public class Startup
    {
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
            // of the PreRequestHandlerExecute event
            app.Use(async (context, next) =>
            {
                _log.Debug(""Application_PreRequestHandlerExecute"");
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
        protected void Session_Start(Object sender, EventArgs e)// {
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
using System.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

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
using Microsoft.AspNetCore.Components;

namespace WebFormsFull
{
    public partial class _Default : ComponentBase, IDisposable
    {
        public const int DefaultPageIndex = 0;
        public const int DefaultPageSize = 2;
        public ICatalogService CatalogService { get; set; }

        protected int EventsTriggered { get; set; }

        protected PaginatedItemsViewModel<CatalogItem> Model { get; set; }

        protected List<People> PeopleModel { get; set; }

        protected void AddPerson(object sender, EventArgs e)
        {
            PeopleModel.Add(new People(""New"", ""New"", ""New""));
            PeopleGrid.DataBind();
        }

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
            {new People(""Andy"", ""Wayne"", ""PG""), new People(""Bill"", ""Johnson"", ""SD""), new People(""Caroline"", ""Barry"", ""Manager"")};
            PeopleGrid.DataSource = PeopleModel;
            PeopleGrid.DataBind();
            productList.DataSource = Model.Data;
            productList.DataBind();
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
using Microsoft.AspNetCore.Components;

namespace WebFormsFull
{
    public partial class OtherPage : ComponentBase
    {
    }
}";
    }
}