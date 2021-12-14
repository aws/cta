using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.Rules.Test.Models
{
    public class ExpectedOutputConstants
    {
        public const string AspNetRoutesStartup =
@"/* Added by CTA: OAuth is now handled by using the public void ConfigureServices(IServiceCollection services) method in the Startup.cs class. The basic process is to use services.AddAuthentication(options => and then set a series of options. We can chain unto that the actual OAuth settings call services.AddOAuth(""Auth_Service_here_such_as_GitHub_Canvas..."", options =>. Also remember to add a call to IApplicationBuilder.UseAuthentication() in your public void Configure(IApplicationBuilder app, IHostingEnvironment env) method. Please ensure this call comes before setting up your routes. */
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using Microsoft.IdentityModel.Protocols;
using Microsoft.AspNetCore.Owin;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace AspNetRoutes
{
    public class Startup
    {
        // Invoked once at startup to configure your application.
        public void Configuration(IApplicationBuilder app)
        {
            app.Run(Invoke);
        }

        // Invoked once per request.
        public Task Invoke(HttpContext context)
        {
            IResponseCookies cookies = context.Response.Cookies;
            cookies.Append(""OwinCookieKey"", ""OwinCookieValue"");
            Uri.TryCreate(@""C:\Users"", UriKind.Absolute, out Uri uriValue);
            PathString path = PathString.FromUriComponent(uriValue);
            bool check = path.Equals(new PathString(@""/C:/Users""));
            context.Response.ContentType = ""text/plain"";
            return context.Response.WriteAsync(""Hello World "" + check);
        }

        public async Task<bool> SignUpUser(HttpContext authenticationManager, ISecureDataFormat<AuthenticationTicket> df)
        {
            List<ClaimsIdentity> claims = new List<ClaimsIdentity>()
            {new ClaimsIdentity()};
            AuthenticationProperties authProp = new AuthenticationProperties();
            var authResult = await authenticationManager.AuthenticateAsync("""");
            /* Added by CTA: Please use your AuthenticationProperties object with this ChallengeAsync api if needed. You can also include a scheme. */
            await authenticationManager.ChallengeAsync();
            var authResults = await /* Added by CTA: This method does not take a list of string as a parameter, it can only accept a single scheme or no parameters. */
            authenticationManager.AuthenticateAsync();
            /* Added by CTA: Please use your AuthenticationProperties object with this ChallengeAsync api if needed. You can also include a scheme. */
            await authenticationManager.ChallengeAsync();
            var authTypes = /* Added by CTA: This method is now deprecated and most authentication methods can be accessed directly from the HttpContext class through their Microsoft.AspNetCore.Authentication namespace extension. */
            authenticationManager.GetAuthenticationTypes();
            /* Added by CTA: Please use a ClaimsPrincipal object to wrap your list of ClaimsIdentity to pass it to this method as a parameter, you can also include a scheme and an AuthenticationProperties object as well. */
            await authenticationManager.SignInAsync(claims.ToArray());
            /* Added by CTA: You can only pass in a single scheme as a parameter and you can also include an AuthenticationProperties object as well. */
            await authenticationManager.SignOutAsync();
            ExternalLoginInfo info = /* Added by CTA: You must declare a Microsoft.AspNetCore.Identity.SignInManager<TUser> object and access the GetExternalLoginInfoAsync method to replace this invocation. */
            authenticationManager.GetExternalLoginInfo();
            ExternalLoginInfo infoAsync1 = await /* Added by CTA: You must declare a Microsoft.AspNetCore.Identity.SignInManager<TUser> object and access the GetExternalLoginInfoAsync method to replace this invocation. */
            authenticationManager.GetExternalLoginInfoAsync();
            ExternalLoginInfo infoAsync2 = await /* Added by CTA: You must declare a Microsoft.AspNetCore.Identity.SignInManager<TUser> object and access the GetExternalLoginInfoAsync method to replace this invocation. */
            authenticationManager.GetExternalLoginInfoAsync(""key_here"", ""expected"");
            List<AuthenticationScheme> desc = /* Added by CTA: You must declare a Microsoft.AspNetCore.Identity.SignInManager<TUser> object and access the GetExternalAuthenticationSchemesAsync method to replace this invocation. */
            authenticationManager.GetExternalAuthenticationTypes().ToList();
            bool rem = /* Added by CTA: You must declare a Microsoft.AspNetCore.Identity.SignInManager<TUser> object and access the TwoFactorBrowserRemembered(TUser) method to replace this invocation. */
            authenticationManager.TwoFactorBrowserRemembered(""userID"");
            bool remAsync = await /* Added by CTA: You must declare a Microsoft.AspNetCore.Identity.SignInManager<TUser> object and access the TwoFactorBrowserRememberedAsync(TUser) method to replace this invocation. */
            authenticationManager.TwoFactorBrowserRememberedAsync(""userID"");
            AuthenticationTicket at = /* Added by CTA: Please use a ClaimsPrincipal object to wrap your ClaimsIdentity parameter to pass to this method, you can optionally include an AuthenticationProperties object and must include a scheme. */
            new AuthenticationTicket(claims.First(), authProp);
            string prot = df.Protect(at);
            AuthenticationTicket unProtectedAT = df.Unprotect(prot);
            OAuthAuthorizationServerOptions auth = new OAuthAuthorizationServerOptions()
            {AccessTokenExpireTimeSpan = new TimeSpan(), AllowInsecureHttp = true, };
            return at.Equals(unProtectedAT);
        }

        public void Protector(IDataProtectionProvider protector)
        {
            string[] purposes = new string[]{"""", """"};
            IDataProtector prot1 = protector.CreateProtector(purposes);
            DpapiDataProtectionProvider dpapi = /* Added by CTA: DpapiDataProtectionProvider should be replaced with a Dependency injection for the IDataProtectionProvider interface. Please include a parameter with this interface to replace this class. */
            new DpapiDataProtectionProvider();
            IDataProtector prot2 = dpapi.CreateProtector(purposes);
        }

        public void UtilitiesLogger(ILogger logger, IApplicationBuilder app)
        {
            /* Added by CTA: Please replace your parameters as the AddQueryString(string, string) method has been deprecated and you must either use the AddQueryString(string, string, string) method or the AddQueryString(string, System.Collections.Generic.IDictionary<string, string>) method. */
            QueryHelpers.AddQueryString(""uri"", ""queryString"");
            QueryHelpers.AddQueryString(""uri"", new Dictionary<string, string>());
            QueryHelpers.AddQueryString(""uri"", ""name"", ""value"");
            logger.LogInformation(""message"");
            /* Added by CTA: Please add IApplicationBuilder.UseAuthentication() in your configure method. Within your public void ConfigureServices(IServiceCollection services) method please add services.AddAuthentication().AddGoogle(options => { IConfigurationSection googleAuthNSection = Configuration.GetSection(""Authentication:Google""); options.ClientId = googleAuthNSection[""ClientId""]; options.ClientSecret = googleAuthNSection[""ClientSecret""]; } ); */
            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions());
        }

        public void OnAuthenticationFailed(HttpContext context, BaseNotification<OpenIdConnectOptions> baseNotif)
        {
            OpenIdConnectOptions options = new OpenIdConnectOptions();
            RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectOptions> redirectContext = new RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectOptions>(context, options);
            /* Added by CTA: Please replace RedirectToIdentityProviderNotification with RedirectContext. The constructor for this new class takes the following parameters: new RedirectContext(HttpContext, AuthenticationScheme, OpenIdConnectOptions, AuthenticationProperties); */
            redirectContext.HandleResponse();
            /* Added by CTA: Please replace BaseNotification with a class from the Microsoft.AspNetCore.Authentication.OpenIdConnect namespace. The MessageReceivedContext class is a good candidate. Its constructor takes the following parameters: new MessageReceivedContext(HttpContext, AuthenticationScheme, OpenIdConnectOptions, AuthenticationProperties); */
            baseNotif.HandleResponse();
        }

        public void OwinSecurityInfrastructure(TicketSerializer create, TicketSerializer receive)
        {
            string serializedTicket = /* Added by CTA: Please add a new parameter as the new method requires Microsoft.AspNetCore.Authentication.AuthenticationTicket to be passed in. The new return type is also a byte[]. */
            create.Serialize();
            /* Added by CTA: Please use instead the methods of the TicketSerializer class: WriteIdentity, WriteClaim, Write for AuthenticationTicket or the ReadIdentity, ReadClaim, Read for AuthenticationTicket. */
            create.SetToken("""");
            /* Added by CTA: Please change the parameter type from string to a byte[]. This is also the data type returned by the TicketSerializer.Serialize method. This new method also returns the actual Microsoft.AspNetCore.Authentication.AuthenticationTicket object serialized with the TicketSerializer.Serialize method. */
            receive.Deserialize(serializedTicket);
            SecurityHelper helper = new SecurityHelper();
            /* Added by CTA: Please use UseAuthentication() in your Configure method in your Startup class and in your ConfigureServices method in your Startup class use AddAuthentication to set all your various options and middleware as needed. */
            helper.LookupChallenge(""Authentication_Type"", AuthenticationMode.Active);
        }
    }
}";

        public const string AspNetRoutesOwinApp2 =
@"using System.Threading.Tasks;
using Microsoft.AspNetCore.Owin;
using Microsoft.AspNetCore.Http;

namespace AspNetRoutes
{
    public class OwinApp2
    {
        // Invoked once per request.
        public static Task Invoke(HttpContext context)
        {
            IRequestCookieCollection cookies = context.Request.Cookies;
            string owinCookieValue = cookies[""OwinCookieKey""];
            context.Request.QueryString = /* Added by CTA: This method only takes a single parameter for the value of the new query string or none at all. */
            new QueryString(""owin"", ""owinValue"");
            IQueryCollection stringquery = context.Request.Query;
            string owinquery = stringquery[""owin""];
            context.Response.ContentType = ""text/plain"";
            return context.Response.WriteAsync(""Hello World 2"");
        }
    }
}";

        public const string BranchingPipelinesDisplayBreadCrumbs =
@"using System.Threading.Tasks;
using Microsoft.AspNetCore.Owin;
using Microsoft.AspNetCore.Http;

namespace BranchingPipelines
{
    public class DisplayBreadCrumbs
    {
        RequestDelegate _next = null;
        public DisplayBreadCrumbs(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            response.ContentType = ""text/plain"";
            string responseText = /* Added by CTA: Please replace Get with TryGetValue. */
            request.Headers.Get(""breadcrumbs"") + ""\r\n"" + ""PathBase: "" + request.PathBase + ""\r\n"" + ""Path: "" + request.Path + ""\r\n"";
            return response.WriteAsync(responseText);
        }
    }
}";

        public const string BranchingPipelinesAddBreadCrumbMiddleware =
@"using System.Threading.Tasks;
using Microsoft.AspNetCore.Owin;
using Microsoft.AspNetCore.Http;

namespace BranchingPipelines
{
    public class AddBreadCrumbMiddleware
    {
        RequestDelegate _next = null;
        private string _breadcrumb;
        public AddBreadCrumbMiddleware(RequestDelegate next, string breadcrumb)
        {
            _breadcrumb = breadcrumb;
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            HttpRequest request = context.Request;
            request.Headers.Append(""breadcrumbs"", _breadcrumb);
            return _next.Invoke(context);
        }
    }
}";

        public const string BranchingPipelinesStartup =
@"using System.Collections.Generic;
using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Owin;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BranchingPipelines
{
    public class Startup
    {
        public void Configuration(IApplicationBuilder app)
        {
            /* Added by CTA: Please replace CreatePerOwinContext<T>(System.Func<T>) and add a new ConfigureServices method: public void ConfigureServices(IServiceCollection services) { Register your service here instead of using CreatePerOwinContext }. For example, app.CreatePerOwinContext(ApplicationDbContext.Create); would become: services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(config.GetConnectionString(""DefaultConnection"")));  */
            app.CreatePerOwinContext<AppBuilderProvider>(() => new AppBuilderProvider(app));
            /* Added by CTA: Please replace CreatePerOwinContext<T>(System.Func<Microsoft.AspNet.Identity.Owin.IdentityFactoryOptions<T>, Microsoft.Owin.IOwinContext, T>) and add a new ConfigureServices method: public void ConfigureServices(IServiceCollection services) { Register your service here instead of using CreatePerOwinContext }. For example, app.CreatePerOwinContext(ApplicationDbContext.Create); would become: services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(config.GetConnectionString(""DefaultConnection"")));  */
            app.CreatePerOwinContext((IdentityFactoryOptions<AppBuilderProvider> options, HttpContext owin) => new AppBuilderProvider(app));
            /* Added by CTA: Please replace CreatePerOwinContext<T>(System.Func<Microsoft.AspNet.Identity.Owin.IdentityFactoryOptions<T>, Microsoft.Owin.IOwinContext, T>, System.Action<Microsoft.AspNet.Identity.Owin.IdentityFactoryOptions<T>, T>) and add a new ConfigureServices method: public void ConfigureServices(IServiceCollection services) { Register your service here instead of using CreatePerOwinContext }. For example, app.CreatePerOwinContext(ApplicationDbContext.Create); would become: services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(config.GetConnectionString(""DefaultConnection"")));  */
            app.CreatePerOwinContext((IdentityFactoryOptions<AppBuilderProvider> options, HttpContext owin) => new AppBuilderProvider(app), (IdentityFactoryOptions<AppBuilderProvider> options, AppBuilderProvider appProv) => new AppBuilderProvider(app));
            app.UseMiddleware<AddBreadCrumbMiddleware>(""start-of-the-line"");
            app.Map(""/branch1"", app1 =>
            {
                app1.UseMiddleware<AddBreadCrumbMiddleware>(""took-branch1"");
                // Nesting paths, e.g. /branch1/branch2
                app1.Map(""/branch2"", app2 =>
                {
                    app2.UseMiddleware<AddBreadCrumbMiddleware>(""took-branch2"");
                    app2.UseMiddleware<DisplayBreadCrumbs>();
                });
                MapIfIE(app1);
                app1.UseMiddleware<DisplayBreadCrumbs>();
            });
            // Only full segments are matched, so /branch1 does not match /branch100
            app.Map(""/branch100"", app1 =>
            {
                app1.UseMiddleware<AddBreadCrumbMiddleware>(""took-branch100"");
                app1.UseMiddleware<DisplayBreadCrumbs>();
            });
            MapIfIE(app);
            app.UseMiddleware<AddBreadCrumbMiddleware>(""no-branches-taken"");
            app.UseMiddleware<DisplayBreadCrumbs>();
        }

        private void MapIfIE(IApplicationBuilder app)
        {
            app.MapWhen(IsIE, app2 =>
            {
                app2.UseMiddleware<AddBreadCrumbMiddleware>(""took-IE-branch"");
                app2.UseMiddleware<DisplayBreadCrumbs>();
            });
        }

        private bool IsIE(HttpContext context)
        {
            IHeaderDictionary headers = context.Request.Headers;
            return /* Added by CTA: Please replace Get with TryGetValue. */
            headers.Get(""User-Agent"").Contains(""Trident"");
        }

        public class AppBuilderProvider : IDisposable
        {
            private IApplicationBuilder _app;
            public AppBuilderProvider(IApplicationBuilder app)
            {
                _app = app;
            }

            public IApplicationBuilder Get()
            {
                return _app;
            }

            public void Dispose()
            {
            }
        }
    }
}";

        public const string EmbeddedStartup =
@"using System.Threading.Tasks;
using Microsoft.AspNetCore.Owin;
using Microsoft.AspNetCore.Builder;

namespace Embedded
{
    public class Startup
    {
        // Invoked once at startup to configure your application.
        public void Configuration(IApplicationBuilder app)
        {
            app.UseWelcomePage();
        }
    }
}";

        public const string EmbeddedProgram =
@"using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel;

namespace Embedded
{
    public class Program
    {
        // Shows how to launch an OWIN HTTP server in your own exe.
        public static void Main(string[] args)
        {
            string baseUrl = ""http://localhost:12345/"";
            using (/* Added by CTA: Replace Microsoft.Owin.Hosting.WebApp.Start with WebHostBuilder such as: var host = new WebHostBuilder().UseKestrel().UseUrls(URL_HERE).UseStartup<Startup>().Build(); host.Start(); Start options can be added into the new format as needed. */
            WebApp.Start<Startup>(new StartOptions(baseUrl)
            {ServerFactory = ""Microsoft.Owin.Host.HttpListener""}))
            {
                // Launch the browser
                Process.Start(baseUrl);
                // Keep the server going until we're done
                Console.WriteLine(""Press Any Key To Exit"");
                Console.ReadKey();
            }
        }
    }
}";

        public const string CustomServerStartup =
@"using System.Threading.Tasks;
using Microsoft.AspNetCore.Owin;
using Microsoft.AspNetCore.Builder;

namespace MyApp
{
    public class Startup
    {
        // Invoked once at startup to configure your application.
        public void Configuration(IApplicationBuilder app)
        {
            app.UseWelcomePage();
        }
    }
}";

        public const string CustomServerProgram =
@"using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel;

namespace MyApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string baseUrl = ""http://localhost:12345/"";
            using (/* Added by CTA: Replace Microsoft.Owin.Hosting.WebApp.Start with WebHostBuilder such as: var host = new WebHostBuilder().UseKestrel().UseUrls(URL_HERE).UseStartup<Startup>().Build(); host.Start(); Start options can be added into the new format as needed. */
            WebApp.Start<Startup>(new StartOptions(baseUrl)
            {ServerFactory = ""MyCustomServer""}))
            {
                // Note: CustomServer has not actually been implemented, no requests will be accepted.
                // Launch the browser
                // Process.Start(baseUrl);
                Console.WriteLine(""Started, Press any key to stop."");
                Console.ReadKey();
                Console.WriteLine(""Stopped"");
            }
        }
    }
}";

        public const string HelloWorldStartup =
@"using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Owin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace HelloWorld
{
    // Note: By default all requests go through this OWIN pipeline. Alternatively you can turn this off by adding an appSetting owin:AutomaticAppStartup with value “false”. 
    // With this turned off you can still have OWIN apps listening on specific routes by adding routes in global.asax file using MapOwinPath or MapOwinRoute extensions on RouteTable.Routes
    public class Startup
    {
        // Invoked once at startup to configure your application.
        public void Configuration(IApplicationBuilder app)
        {
            app.Run(Invoke);
        }

        // Invoked once per request.
        public Task Invoke(HttpContext context)
        {
            context.Response.ContentType = ""text/plain"";
            return context.Response.WriteAsync(""Hello World"");
        }
    }
}";

        public const string HelloWorldRawOwinStartup =
@"using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace HelloWorld
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    // Note the Web.Config owin:AutomaticAppStartup setting that is used to direct all requests to your OWIN application.
    // Alternatively you can specify routes in the global.asax file.
    public class Startup
    {
        // Invoked once at startup to configure your application.
        public void Configuration(IApplicationBuilder builder)
        {
            /* Added by CTA: Invoke being the function you wanted to call in your lambda function. */
            builder.UseOwin(pipeline =>
            {
                pipeline(next => Invoke);
            });
        }

        // Invoked once per request.
        public Task Invoke(IDictionary<string, object> environment)
        {
            string responseText = ""Hello World"";
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseText);
            // See http://owin.org/spec/owin-1.0.0.html for standard environment keys.
            Stream responseStream = (Stream)environment[""owin.ResponseBody""];
            IDictionary<string, string[]> responseHeaders = (IDictionary<string, string[]>)environment[""owin.ResponseHeaders""];
            responseHeaders[""Content-Length""] = new string[]{responseBytes.Length.ToString(CultureInfo.InvariantCulture)};
            responseHeaders[""Content-Type""] = new string[]{""text/plain""};
            return Task.Factory.FromAsync(responseStream.BeginWrite, responseStream.EndWrite, responseBytes, 0, responseBytes.Length, null);
        // 4.5: return responseStream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }
    }
}";

        public const string OwinSelfHostStartup =
@"using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OwinSelfhostSample
{
    public class Startup
    {
        // This code configures Web API contained in the class Startup, which is additionally specified as the type parameter in WebApplication.Start
        public void Configuration(IApplicationBuilder appBuilder)
        {
            // Configure Web API for Self-Host
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(name: ""DefaultApi"", routeTemplate: ""api/{controller}/{id}"", defaults: new
            {
            id = RouteParameter.Optional
            }

            );
            /* Added by CTA: Please add a new ConfigureServices method: public void ConfigureServices(IServiceCollection services) { services.AddControllers(); } */
            appBuilder.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}";

        public const string OwinSelfHostProgram =
@"using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel;

namespace OwinSelfhostSample
{
    public class Program
    {
        static void Main()
        {
            string baseAddress = ""http://localhost:10281/"";
            // Start OWIN host
            using (/* Added by CTA: Replace Microsoft.Owin.Hosting.WebApp.Start with WebHostBuilder such as: var host = new WebHostBuilder().UseKestrel().UseUrls(URL_HERE).UseStartup<Startup>().Build(); host.Start(); UseStartup can contain the TStartup class used before. */
            WebApp.Start<Startup>(url: baseAddress))
            {
                // Create HttpCient and make a request to api/values
                HttpClient client = new HttpClient();
                HttpResponseMessage response = client.GetAsync(baseAddress + ""api/values"").Result;
                Console.WriteLine(response);
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            }

            Console.ReadLine();
        }
    }
}";

        public const string SignalRStartup =
@"using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;

namespace SignalR
{
    public class Startup
    {
        public void Configuration(IApplicationBuilder app)
        {
            /* Added by CTA: Please add a new ConfigureServices method: public void ConfigureServices(IServiceCollection services) { services.AddSignalR(); } */
            app.UseEndpoints(routes => routes.MapHub<Hub>(""pattern""));
        }
    }
}";

        public const string SignalRProgram =
@"using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel;

namespace SignalR
{
    class Program
    {
        static void Main(string[] args)
        {
            string uri = ""http://localhost:9999/"";
            using (/* Added by CTA: Replace Microsoft.Owin.Hosting.WebApp.Start with WebHostBuilder such as: var host = new WebHostBuilder().UseKestrel().UseUrls(URL_HERE).UseStartup<Startup>().Build(); host.Start(); UseStartup can contain the TStartup class used before. */
            WebApp.Start<Startup>(uri))
            {
                Console.WriteLine(""Started"");
                // Open the SignalR negotiation page to make sure things are working.
                Process.Start(uri + ""signalr/negotiate"");
                Console.ReadKey();
                Console.WriteLine(""Finished"");
            }
        }
    }
}";

        public const string StaticFilesSampleStartup =
@"using Microsoft.AspNetCore.Owin;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace StaticFilesSample
{
    public class Startup
    {
        public void Configuration(IApplicationBuilder app)
        {
#if DEBUG
            app.UseDeveloperExceptionPage();
            PhysicalFileProvider defaultFS = new PhysicalFileProvider(@"".\defaults"");
#endif
            // Remap '/' to '.\defaults\'.
            // Turns on static files and default files.
            app.UseFileServer(/* Added by CTA: For FileServerOptions, if FileSystem was not present before FileProvider was added, please initialize this new value. Using the value from RequestPath with System.IO.Directory.GetCurrentDirectory() as a prefix may be a good starting point. */
            new FileServerOptions()
            {RequestPath = PathString.Empty, FileProvider = defaultFS, });
            // Only serve files requested by name.
            app.UseStaticFiles(""/files"");
            // Turns on static files, directory browsing, and default files.
            app.UseFileServer(/* Added by CTA: For FileServerOptions, if FileSystem was not present before FileProvider was added, please initialize this new value. Using the value from RequestPath with System.IO.Directory.GetCurrentDirectory() as a prefix may be a good starting point. */
            new FileServerOptions()
            {RequestPath = new PathString(""/public""), EnableDirectoryBrowsing = true, FileProvider = new PhysicalFileProvider(@"""")});
            // Browse the root of your application (but do not serve the files).
            // NOTE: Avoid serving static files from the root of your application or bin folder,
            // it allows people to download your application binaries, config files, etc..
            /* Added by CTA: Please add a new ConfigureServices method: public void ConfigureServices(IServiceCollection services) { services.AddDirectoryBrowser(); } */
            app.UseDirectoryBrowser(/* Added by CTA: For DirectoryBrowserOptions, if FileSystem was not present before FileProvider was added, please initialize this new value. Using the value from RequestPath with System.IO.Directory.GetCurrentDirectory() as a prefix may be a good starting point. */
            new DirectoryBrowserOptions()
            {RequestPath = new PathString(""/src""), FileProvider = new PhysicalFileProvider(@""""), });
            // Anything not handled will land at the welcome page.
            app.UseWelcomePage();
        }
    }
}";

        public const string StaticFilesSampleProgram =
@"using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel;

namespace StaticFilesSample
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = ""http://localhost:12345"";
            using (/* Added by CTA: Replace Microsoft.Owin.Hosting.WebApp.Start with WebHostBuilder such as: var host = new WebHostBuilder().UseKestrel().UseUrls(URL_HERE).UseStartup<Startup>().Build(); host.Start(); UseStartup can contain the TStartup class used before. */
            WebApp.Start<Startup>(url))
            {
                Process.Start(url); // Launch the browser.
                Console.WriteLine(""Press Enter to exit..."");
                Console.ReadLine();
            }
        }
    }
}";

        public const string OwinWebApiStartup =
@"using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace WebApi
{
    // Note: By default all requests go through this OWIN pipeline. Alternatively you can turn this off by adding an appSetting owin:AutomaticAppStartup with value “false”. 
    // With this turned off you can still have OWIN apps listening on specific routes by adding routes in global.asax file using MapOwinPath or MapOwinRoute extensions on RouteTable.Routes
    public class Startup
    {
        // Invoked once at startup to configure your application.
        public void Configuration(IApplicationBuilder builder)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(""Default"", ""{controller}/{customerID}"", new
            {
            controller = ""Customer"", customerID = RouteParameter.Optional
            }

            );
            config.Formatters.XmlFormatter.UseXmlSerializer = true;
            config.Formatters.Remove(config.Formatters.JsonFormatter);
            // config.Formatters.JsonFormatter.UseDataContractJsonSerializer = true;
            /* Added by CTA: Please add a new ConfigureServices method: public void ConfigureServices(IServiceCollection services) { services.AddControllers(); } */
            builder.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            /* Added by CTA: Please add services.AddAuthentication().AddOpenIdConnect(); inside of your public void ConfigureServices(IServiceCollection services) method. You can also optionally add your OpenIdConnectOptions. */
            builder.UseAuthentication();
        }
    }
}";

        public const string WebSocketClientProgram =
@"using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            RunSample().Wait();
            Console.WriteLine(""Finished"");
            Console.ReadKey();
        }

        public static async Task RunSample()
        {
            ClientWebSocket websocket = new ClientWebSocket();

            string url = ""ws://localhost:5000/"";
            Console.WriteLine(""Connecting to: "" + url);
            await websocket.ConnectAsync(new Uri(url), CancellationToken.None);

            string message = ""Hello World"";
            Console.WriteLine(""Sending message: "" + message);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            await websocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);

            byte[] incomingData = new byte[1024];
            WebSocketReceiveResult result = await websocket.ReceiveAsync(new ArraySegment<byte>(incomingData), CancellationToken.None);

            if (result.CloseStatus.HasValue)
            {
                Console.WriteLine(""Closed; Status: "" + result.CloseStatus + "", "" + result.CloseStatusDescription);
            }
            else
            {
                Console.WriteLine(""Received message: "" + Encoding.UTF8.GetString(incomingData, 0, result.Count));
            }
        }
    }
}";

        public const string WebSocketServerStartup =
@"using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Owin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace WebSocketServer
{
    // http://owin.org/extensions/owin-WebSocket-Extension-v0.4.0.htm
    using WebSocketAccept = Action<IDictionary<string, object>, // options
 Func<IDictionary<string, object>, Task>>; // callback
    using WebSocketCloseAsync = Func<int /* closeStatus */, string /* closeDescription */, CancellationToken /* cancel */, Task>;
    using WebSocketReceiveAsync = Func<ArraySegment<byte> /* data */, CancellationToken /* cancel */, Task<Tuple<int /* messageType */, bool /* endOfMessage */, int /* count */>>>;
    using WebSocketSendAsync = Func<ArraySegment<byte> /* data */, int /* messageType */, bool /* endOfMessage */, CancellationToken /* cancel */, Task>;
    using WebSocketReceiveResult = Tuple<int, // type
 bool, // end of message?
 int>; // count

    /// <summary>
    /// This sample requires Windows 8, .NET 4.5, and Microsoft.Owin.Host.HttpListener.
    /// </summary>
    public class Startup
    {
        // Run at startup
        public void Configuration(IApplicationBuilder app)
        {
            app.Use(UpgradeToWebSockets);
            app.UseWelcomePage();
        }

        // Run once per request
        private Task UpgradeToWebSockets(HttpContext context, Func<Task> next)
        {
            WebSocketAccept accept = /* Added by CTA: Please replace Get with Items. Such as (Casting_Type)context.Items[""String_To_Search_For""]; */
            context.Get<WebSocketAccept>(""websocket.Accept"");
            if (accept == null)
            {
                // Not a websocket request
                return next();
            }

            accept(null, WebSocketEcho);
            return Task.FromResult<object>(null);
        }

        private async Task WebSocketEcho(IDictionary<string, object> websocketContext)
        {
            var sendAsync = (WebSocketSendAsync)websocketContext[""websocket.SendAsync""];
            var receiveAsync = (WebSocketReceiveAsync)websocketContext[""websocket.ReceiveAsync""];
            var closeAsync = (WebSocketCloseAsync)websocketContext[""websocket.CloseAsync""];
            var callCancelled = (CancellationToken)websocketContext[""websocket.CallCancelled""];
            byte[] buffer = new byte[1024];
            WebSocketReceiveResult received = await receiveAsync(new ArraySegment<byte>(buffer), callCancelled);
            object status;
            while (!websocketContext.TryGetValue(""websocket.ClientCloseStatus"", out status) || (int)status == 0)
            {
                // Echo anything we receive
                await sendAsync(new ArraySegment<byte>(buffer, 0, received.Item3), received.Item1, received.Item2, callCancelled);
                received = await receiveAsync(new ArraySegment<byte>(buffer), callCancelled);
            }

            await closeAsync((int)websocketContext[""websocket.ClientCloseStatus""], (string)websocketContext[""websocket.ClientCloseDescription""], callCancelled);
        }
    }
}";

        public const string WebSocketServerProgram =
@"using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel;

namespace WebSocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (/* Added by CTA: Replace Microsoft.Owin.Hosting.WebApp.Start with WebHostBuilder such as: var host = new WebHostBuilder().UseKestrel().UseUrls(URL_HERE).UseStartup<Startup>().Build(); host.Start(); UseStartup can contain the TStartup class used before. */
            WebApp.Start<Startup>(""http://localhost:5000/""))
            {
                Console.WriteLine(""Ready, press any key to exit..."");
                Console.ReadKey();
            }
        }
    }
}";

        public const string OwinExtraAPIProgram =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;

namespace OwinExtraAPI
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        public void OwinAuthorization(IAuthorizationRequirement req)
        {
            List<IAuthorizationRequirement> listReq = new List<IAuthorizationRequirement>()
            {req};
            AuthorizationHandlerContext ahc = new AuthorizationHandlerContext(listReq, new ClaimsPrincipal(), null);
            ahc.Succeed(req);
            AuthorizationOptions aops = new AuthorizationOptions();
            AuthorizationPolicy apol = new AuthorizationPolicy(listReq, new List<string>()
            {""""});
            aops.AddPolicy("""", apol);
            aops.AddPolicy("""", policy =>
            {
            });
            AuthorizationPolicyBuilder apolBuild = new AuthorizationPolicyBuilder(apol);
            apolBuild = apolBuild.AddRequirements(req);
        }

        public void OwinDataHandler(ITextEncoder encoder, SecureDataFormat<AuthenticationProperties> secure)
        {
            AuthenticationProperties props = new AuthenticationProperties();
            string secured = secure.Protect(props);
            AuthenticationProperties unsecured = secure.Unprotect(secured);
            byte[] decoded = new byte[10];
            string encoded = /* Added by CTA: Please replace the ITextEncoder with its appropriate class either Microsoft.AspNetCore.WebUtilities.Base64UrlTextEncoder to encode and decode URLs or System.Text.Encoding for general encoding needs. */
            encoder.Encode(decoded);
            decoded = /* Added by CTA: Please replace the ITextEncoder with its appropriate class either Microsoft.AspNetCore.WebUtilities.Base64UrlTextEncoder to encode and decode URLs or System.Text.Encoding for general encoding needs. */
            encoder.Decode(encoded);
        }
    }
}";
        public const string WCFConfigStartup =
@"
using CoreWCF.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace {0}
{{
   public class Startup
    {{
        public void ConfigureServices(IServiceCollection services)
        {{
            string pathToXml = @""{1}"";
            services.AddServiceModelServices();
            services.AddServiceModelConfigurationManagerFile(pathToXml);
        }}

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {{
            app.UseServiceModel();
        }}
    }}
}}
";

        public const string WCFConfigStartupWithBehavior =
@"
using CoreWCF.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace {0}
{{
   public class Startup
    {{
        public void ConfigureServices(IServiceCollection services)
        {{
            string pathToXml = @""{1}"";
            services.AddServiceModelServices();
            services.AddServiceModelConfigurationManagerFile(pathToXml);
  // The API does not support behaviors section inside config. Please modify the configure method for service behaviors support. Refer to https://github.com/CoreWCF/CoreWCF
        }}

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {{
            app.UseServiceModel();
        }}
    }}
}}
";
        public const string WCFTC2Program =
@"
using CoreWCF.Configuration;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;


namespace TC2BasicHttpTransportSecurity
{
	public class Program
	{
		public static void Main(string[] args)
		{
      //All Ports set are default.
			IWebHost host = CreateWebHostBuilder(args).Build();
      host.Run();
		}

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
				 .UseKestrel(options => { 
options.ListenLocalhost(8080);
        options.Listen(address: IPAddress.Loopback, 8888, listenOptions =>
        {
            listenOptions.UseHttps(httpsOptions =>
            {
#if NET472
                httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
#endif // NET472
            });
        });})
				 .UseStartup<Startup>();
	}
}
";
        public const string WCFTC2ConfigText =
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<configuration>
  <system.serviceModel>
    <bindings>
      <basicHttpBinding>
        <binding name=""Basic"">
          <security mode=""Transport""></security>
        </binding>
      </basicHttpBinding>
    </bindings>
    <services>
      <service name=""TC2BasicHttpTransportSecurity.Service1"">
        <endpoint address=""basicHttp"" binding=""basicHttpBinding"" name=""Basic"" contract=""TC2BasicHttpTransportSecurity.IService1"" />
      </service>
    </services>
    <behaviors>
      <serviceBehaviors>
        <behavior>
          <!-- To avoid disclosing metadata information, set the values below to false before deployment -->
          <serviceMetadata httpGetEnabled=""true"" httpsGetEnabled=""true"" />
          <!-- To receive exception details in faults for debugging purposes, set the value below to true.  Set to false before deployment to avoid disclosing exception information -->
          <serviceDebug includeExceptionDetailInFaults=""false"" />
        </behavior>
      </serviceBehaviors>
    </behaviors>
    <protocolMapping>
      <add binding=""basicHttpsBinding"" scheme=""https"" />
    </protocolMapping>
    <serviceHostingEnvironment aspNetCompatibilityEnabled=""true"" multipleSiteBindingsEnabled=""true"" />
  </system.serviceModel>
</configuration>";

        public const string WCFTC3Program =
@"
using CoreWCF.Configuration;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;


namespace TC3BasicHttpTransportMessageCredUserName
{
	public class Program
	{
		public static void Main(string[] args)
		{
      //All Ports set are default.
			IWebHost host = CreateWebHostBuilder(args).Build();
      host.Run();
		}

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
				 .UseKestrel(options => { 
options.ListenLocalhost(8080);
        options.Listen(address: IPAddress.Loopback, 8888, listenOptions =>
        {
            listenOptions.UseHttps(httpsOptions =>
            {
#if NET472
                httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
#endif // NET472
            });
        });})
				 .UseStartup<Startup>();
	}
}
";
        public const string WCFTC3ConfigText =
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<configuration>
  <system.serviceModel>
    <bindings>
      <basicHttpBinding>
        <binding name=""Basic"">
          <security mode=""TransportWithMessageCredential"">
            <message clientCredentialType=""UserName"" />
          </security>
        </binding>
      </basicHttpBinding>
    </bindings>
    <services>
      <service name=""TC3BasicHttpTransportMessageCredUserName.Service1"">
        <endpoint address=""basicHttp"" binding=""basicHttpBinding"" name=""Basic"" contract=""TC3BasicHttpTransportMessageCredUserName.IService1"" />
      </service>
    </services>
    <behaviors>
      <serviceBehaviors>
        <behavior>
          <!-- To avoid disclosing metadata information, set the values below to false before deployment -->
          <serviceMetadata httpGetEnabled=""true"" httpsGetEnabled=""true"" />
          <!-- To receive exception details in faults for debugging purposes, set the value below to true.  Set to false before deployment to avoid disclosing exception information -->
          <serviceDebug includeExceptionDetailInFaults=""false"" />
          <serviceCredentials>
            <userNameAuthentication userNamePasswordValidationMode=""Custom"" customUserNamePasswordValidatorType=""TC3BasicHttpTransportMessageCredUserName.CustomTestValidator, TC3BasicHttpTransportMessageCredUserName"" />
          </serviceCredentials>
        </behavior>
      </serviceBehaviors>
    </behaviors>
    <protocolMapping>
      <add binding=""basicHttpsBinding"" scheme=""https"" />
    </protocolMapping>
    <serviceHostingEnvironment aspNetCompatibilityEnabled=""true"" multipleSiteBindingsEnabled=""true"" />
  </system.serviceModel>
</configuration>";

        public const string WCFTC4Program =
@"
using CoreWCF.Configuration;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;


namespace TC4BasicHttpTransportMessageCredCertificate
{
	public class Program
	{
		public static void Main(string[] args)
		{
      //All Ports set are default.
			IWebHost host = CreateWebHostBuilder(args).Build();
      host.Run();
		}

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
				 .UseKestrel(options => { 
options.ListenLocalhost(8080);
        options.Listen(address: IPAddress.Loopback, 8888, listenOptions =>
        {
            listenOptions.UseHttps(httpsOptions =>
            {
#if NET472
                httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
#endif // NET472
            });
        });})
				 .UseStartup<Startup>();
	}
}
";
        public const string WCFTC4ConfigText =
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<configuration>
  <system.serviceModel>
    <bindings>
      <basicHttpBinding>
        <binding name=""Basic"">
          <security mode=""TransportWithMessageCredential"">
            <message clientCredentialType=""Certificate"" />
          </security>
        </binding>
      </basicHttpBinding>
    </bindings>
    <services>
      <service behaviorConfiguration=""certificatedetails"" name=""TC4BasicHttpTransportMessageCredCertificate.Service1"">
        <endpoint address=""basicHttp"" binding=""basicHttpBinding"" name=""Basic"" contract=""TC4BasicHttpTransportMessageCredCertificate.IService1"" />
      </service>
    </services>
    <behaviors>
      <serviceBehaviors>
        <behavior name=""certificatedetails"">
          <serviceMetadata httpGetEnabled=""true"" httpsGetEnabled=""true"" />
          <serviceDebug includeExceptionDetailInFaults=""false"" />
          <serviceCredentials>
            <serviceCertificate storeLocation=""LocalMachine"" findValue=""4df903d7ad017c8a4346b0c21b909ba8"" x509FindType=""FindBySerialNumber"" />
            <clientCertificate>
              <authentication certificateValidationMode=""PeerOrChainTrust"" trustedStoreLocation=""CurrentUser"" />
            </clientCertificate>
          </serviceCredentials>
        </behavior>
      </serviceBehaviors>
    </behaviors>
    <serviceHostingEnvironment aspNetCompatibilityEnabled=""true"" multipleSiteBindingsEnabled=""true"" />
  </system.serviceModel>
</configuration>";

        public const string WCFTC5Program =
@"
using CoreWCF.Configuration;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;


namespace TC5WSHttpBindingWithWindowsAuth
{
	public class Program
	{
		public static void Main(string[] args)
		{
      //All Ports set are default.
			IWebHost host = CreateWebHostBuilder(args).Build();
      host.Run();
		}

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
				 .UseKestrel(options => { 
options.ListenLocalhost(8080);
        options.Listen(address: IPAddress.Loopback, 8888, listenOptions =>
        {
            listenOptions.UseHttps(httpsOptions =>
            {
#if NET472
                httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
#endif // NET472
            });
        });})
				 .UseStartup<Startup>();
	}
}
";
        public const string WCFTC5ConfigText =
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<configuration>
  <system.serviceModel>
    <bindings>
      <wsHttpBinding>
        <binding name=""WsHtt"">
          <security mode=""TransportWithMessageCredential"">
            <message clientCredentialType=""Windows"" />
          </security>
        </binding>
      </wsHttpBinding>
    </bindings>
    <services>
      <service name=""TC5WSHttpBindingWithWindowsAuth.Service1"">
        <endpoint address=""wsHttp"" binding=""wsHttpBinding"" name=""Basic"" contract=""TC5WSHttpBindingWithWindowsAuth.IService1"" />
      </service>
    </services>
    <serviceHostingEnvironment aspNetCompatibilityEnabled=""true"" multipleSiteBindingsEnabled=""true"" />
  </system.serviceModel>
</configuration>";

        public const string WCFTC7Program =
@"
using CoreWCF.Configuration;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;


namespace TC7NetTCPBindingDefaultSecurity
{
	public class Program
	{
		public static void Main(string[] args)
		{
      //All Ports set are default.
			IWebHost host = CreateWebHostBuilder(args).Build();
      host.Run();
		}

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
				 .UseKestrel(options => { })
.UseNetTcp(8000)				 .UseStartup<Startup>();
	}
}
";
        public const string WCFTC7ConfigText =
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<configuration>
  <system.serviceModel>
    <bindings>
      <netTcpBinding>
        <binding name=""netTCP""></binding>
      </netTcpBinding>
    </bindings>
    <services>
      <service name=""TC7NetTCPBindingDefaultSecurity.Service1"">
        <endpoint address=""netTCP"" binding=""netTcpBinding"" name=""Basic"" contract=""TC7NetTCPBindingDefaultSecurity.IService1"" />
      </service>
    </services>
    <behaviors>
      <serviceBehaviors>
        <behavior>
          <!-- To avoid disclosing metadata information, set the values below to false before deployment -->
          <serviceMetadata httpGetEnabled=""true"" httpsGetEnabled=""true"" />
          <!-- To receive exception details in faults for debugging purposes, set the value below to true.  Set to false before deployment to avoid disclosing exception information -->
          <serviceDebug includeExceptionDetailInFaults=""false"" />
        </behavior>
      </serviceBehaviors>
    </behaviors>
    <serviceHostingEnvironment aspNetCompatibilityEnabled=""true"" multipleSiteBindingsEnabled=""true"" />
  </system.serviceModel>
</configuration>";

        public const string WCFTC9Program =
@"
using CoreWCF.Configuration;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;


namespace TC9BasicHttpAndNetTCPSupported
{
	public class Program
	{
		public static void Main(string[] args)
		{
      //All Ports set are default.
			IWebHost host = CreateWebHostBuilder(args).Build();
      host.Run();
		}

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
				 .UseKestrel(options => { 
options.ListenLocalhost(8080);
        options.Listen(address: IPAddress.Loopback, 8888, listenOptions =>
        {
            listenOptions.UseHttps(httpsOptions =>
            {
#if NET472
                httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
#endif // NET472
            });
        });})
.UseNetTcp(8000)				 .UseStartup<Startup>();
	}
}
";
        public const string WCFTC9ConfigText =
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<configuration>
  <system.serviceModel>
    <bindings>
      <basicHttpBinding>
        <binding name=""Basic"">
          <security mode=""TransportWithMessageCredential"">
            <message clientCredentialType=""Certificate"" />
          </security>
        </binding>
      </basicHttpBinding>
      <netTcpBinding>
        <binding name=""TCP"">
          <security mode=""TransportWithMessageCredential"">
            <message clientCredentialType=""Certificate"" />
          </security>
        </binding>
      </netTcpBinding>
    </bindings>
    <services>
      <service behaviorConfiguration=""certificatedetails"" name=""TC9BasicHttpAndNetTCPSupported.Service1"">
        <endpoint address=""basicHttp"" binding=""basicHttpBinding"" name=""Basic"" contract=""TC9BasicHttpAndNetTCPSupported.IService1"" />
        <endpoint address=""netTCP"" binding=""netTcpBinding"" bindingConfiguration="""" name=""TCP"" contract=""TC9BasicHttpAndNetTCPSupported.IService1"" />
      </service>
    </services>
    <behaviors>
      <serviceBehaviors>
        <behavior name=""certificatedetails"">
          <serviceMetadata httpGetEnabled=""true"" httpsGetEnabled=""true"" />
          <serviceDebug includeExceptionDetailInFaults=""false"" />
          <serviceCredentials>
            <serviceCertificate storeLocation=""LocalMachine"" findValue=""4df903d7ad017c8a4346b0c21b909ba8"" x509FindType=""FindBySerialNumber"" />
            <clientCertificate>
              <authentication certificateValidationMode=""PeerOrChainTrust"" trustedStoreLocation=""CurrentUser"" />
            </clientCertificate>
          </serviceCredentials>
        </behavior>
      </serviceBehaviors>
    </behaviors>
    <serviceHostingEnvironment aspNetCompatibilityEnabled=""true"" multipleSiteBindingsEnabled=""true"" />
  </system.serviceModel>
</configuration>";

        public const string WCFTC10Program =
@"
using CoreWCF.Configuration;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;


namespace TC10WsHttpAndNetPipe
{
	public class Program
	{
		public static void Main(string[] args)
		{
      //All Ports set are default.
			IWebHost host = CreateWebHostBuilder(args).Build();
      host.Run();
		}

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
				 .UseKestrel(options => { 
options.ListenLocalhost(8080);
        options.Listen(address: IPAddress.Loopback, 8888, listenOptions =>
        {
            listenOptions.UseHttps(httpsOptions =>
            {
#if NET472
                httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
#endif // NET472
            });
        });})
				 .UseStartup<Startup>();
	}
}
";
        public const string WCFTC10ConfigText =
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<configuration>
  <system.serviceModel>
    <bindings>
      <wsHttpBinding>
        <binding name=""wsHttp"">
          <security mode=""TransportWithMessageCredential"">
            <message clientCredentialType=""Certificate"" />
          </security>
        </binding>
      </wsHttpBinding>
      <netNamedPipeBinding>
        <binding name=""netPipe""></binding>
      </netNamedPipeBinding>
    </bindings>
    <services>
      <service behaviorConfiguration=""certificatedetails"" name=""TC10WsHttpAndNetPipe.Service1"">
        <endpoint address=""wsHttp"" binding=""wsHttpBinding"" name=""Basic"" contract=""TC10WsHttpAndNetPipe.IService1"" />
        <endpoint address=""netPipe"" binding=""netNamedPipeBinding"" bindingConfiguration="""" name=""TCP"" contract=""TC10WsHttpAndNetPipe.IService1"" />
      </service>
    </services>
    <behaviors>
      <serviceBehaviors>
        <behavior name=""certificatedetails"">
          <serviceMetadata httpGetEnabled=""true"" httpsGetEnabled=""true"" />
          <serviceDebug includeExceptionDetailInFaults=""false"" />
          <serviceCredentials>
            <serviceCertificate storeLocation=""LocalMachine"" findValue=""4df903d7ad017c8a4346b0c21b909ba8"" x509FindType=""FindBySerialNumber"" />
            <clientCertificate>
              <authentication certificateValidationMode=""PeerOrChainTrust"" trustedStoreLocation=""CurrentUser"" />
            </clientCertificate>
          </serviceCredentials>
        </behavior>
      </serviceBehaviors>
    </behaviors>
    <serviceHostingEnvironment aspNetCompatibilityEnabled=""true"" multipleSiteBindingsEnabled=""true"" />
  </system.serviceModel>
</configuration>";

        public const string WCFTC1CodeStartupText =
@"
using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Channels;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

//This is a template based on existing bindings.

namespace TC1CodeBasicHttpDefaultSecurity
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServiceModelServices();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseServiceModel(builder =>
            {
                builder.AddService <EchoService>();
builder.AddServiceEndpoint<EchoService, IEchoService>(new BasicHttpBinding(), ""/basicHttp"");
            });
        }
    }
}


";
        public const string WCFTC1CodeBasedProgramText =
@"
using CoreWCF.Configuration;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;


namespace TC1CodeBasicHttpDefaultSecurity
{
	public class Program
	{
		public static void Main(string[] args)
		{
      //All Ports set are default.
			IWebHost host = CreateWebHostBuilder(args).Build();
      host.Run();
		}

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
				 .UseKestrel(options => { 
options.ListenLocalhost(8080);})
				 .UseStartup<Startup>();
	}
}
";
        public const string WCFTC2CodeBasedStartupText =
@"
using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Channels;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

//This is a template based on existing bindings.

namespace TC2CodeBasicHttpTransportSecurity
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServiceModelServices();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseServiceModel(builder =>
            {
                builder.AddService <EchoService>();
builder.AddServiceEndpoint<EchoService, IEchoService>(new BasicHttpBinding(BasicHttpSecurityMode.Transport), ""/basicHttps"");
            });
        }
    }
}


";
        public const string WCFTC2CodeBasedProgramText =
@"
using CoreWCF.Configuration;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;


namespace TC2CodeBasicHttpTransportSecurity
{
	public class Program
	{
		public static void Main(string[] args)
		{
      //All Ports set are default.
			IWebHost host = CreateWebHostBuilder(args).Build();
      host.Run();
		}

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
				 .UseKestrel(options => { 
options.ListenLocalhost(8080);
        options.Listen(address: IPAddress.Loopback, 8888, listenOptions =>
        {
            listenOptions.UseHttps(httpsOptions =>
            {
#if NET472
                httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
#endif // NET472
            });
        });})
				 .UseStartup<Startup>();
	}
}
";
        public const string WCFTC3CodeBasedStartupText =
@"
using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Channels;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

//This is a template based on existing bindings.

namespace TC3CodeBasicHttpTransportMessageCredUserName
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServiceModelServices();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseServiceModel(builder =>
            {
                builder.AddService <EchoService>();
builder.AddServiceEndpoint<EchoService, IEchoService>(new WSHttpBinding(SecurityMode.TransportWithMessageCredential), ""/wsHttp"");
            });
        }
    }
}


";
        public const string WCFTC3CodeBasedProgramText =
@"
using CoreWCF.Configuration;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;


namespace TC3CodeBasicHttpTransportMessageCredUserName
{
	public class Program
	{
		public static void Main(string[] args)
		{
      //All Ports set are default.
			IWebHost host = CreateWebHostBuilder(args).Build();
      host.Run();
		}

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
				 .UseKestrel(options => { 
options.ListenLocalhost(8080);
        options.Listen(address: IPAddress.Loopback, 8888, listenOptions =>
        {
            listenOptions.UseHttps(httpsOptions =>
            {
#if NET472
                httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
#endif // NET472
            });
        });})
				 .UseStartup<Startup>();
	}
}
";
        public const string WCFTC9CodeBasedStartupText =
@"
using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Channels;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

//This is a template based on existing bindings.

namespace TC9CodeBasicHttpNetTCPSupported
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServiceModelServices();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseServiceModel(builder =>
            {
                builder.AddService <EchoService>();
builder.AddServiceEndpoint<EchoService, IEchoService>(new NetTcpBinding(), ""/nettcp"");
builder.AddServiceEndpoint<EchoService, IEchoService>(new BasicHttpBinding(BasicHttpSecurityMode.Transport), ""/basichttp"");
            });
        }
    }
}


";
        public const string WCFTC9CodeBasedProgramText =
@"
using CoreWCF.Configuration;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;


namespace TC9CodeBasicHttpNetTCPSupported
{
	public class Program
	{
		public static void Main(string[] args)
		{
      //All Ports set are default.
			IWebHost host = CreateWebHostBuilder(args).Build();
      host.Run();
		}

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
				 .UseKestrel(options => { 
options.ListenLocalhost(8080);
        options.Listen(address: IPAddress.Loopback, 8888, listenOptions =>
        {
            listenOptions.UseHttps(httpsOptions =>
            {
#if NET472
                httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
#endif // NET472
            });
        });})
.UseNetTcp(8000)				 .UseStartup<Startup>();
	}
}
";
        public const string WebFormsProgramText =
@"using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ASP.NETWebForms
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
        public const string WebFormsStartupText =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ASP.NET_WebForms
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Env { get; }

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
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
             // Did not attempt to migrate service layer
            // and configure depenency injection in ConfigureServices(),
            // this must be done manually
        }
    }
}";

        public const string BlazorConfigureFunctionText =
            @"public void Configure(IApplicationBuilder app)
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
                //set the property to our new object
                LogicalThreadContext.Properties[""activityid""] = new ActivityIdHelper();
                LogicalThreadContext.Properties[""requestinfo""] = new WebRequestInfo();
                _log.Debug(""Application_BeginRequest"");
                await next();
            });
        }";

        public const string BlazorProgramText = @"
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
    }";
        public const string BlazorWebRequestInfoText = @"
    public class WebRequestInfo
    {
        public override string ToString()
        {
            return HttpContext.Current?.Request?.RawUrl + "", "" + HttpContext.Current?.Request?.UserAgent;
        }
    }";
    }
}
