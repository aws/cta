using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CTA.Rules.Models;

namespace CTA.Rules.Actions
{
    public static class ServerConfigTemplates
    {
        internal const string ConfigureServicesMethod = "ConfigureServices";
        internal const string ConfigureMiddlewareMethod = "Configure";

        internal static List<string> MiddlewareConfigMethods = new List<string>()
        {
            ConfigureServicesMethod,
            ConfigureMiddlewareMethod
        };

        internal static Dictionary<string, string> ConfigurationExpressionTemplates = new Dictionary<string, string>()
        {
            {WebServerConfigAttributes.Authentication.ToString(), "app.UseAuthentication();"},
            {WebServerConfigAttributes.Authorization.ToString(), "app.UseAuthorization();" },
            {WebServerConfigAttributes.Modules.ToString(),"app.UseMiddleware<{0}>();"},
            {WebServerConfigAttributes.Handlers.ToString(), @"app.MapWhen(context => context.Request.Path.ToString().EndsWith({0}),appBranch => {{ appBranch.UseMiddleware<{1}>();}});" }
        };

        internal static Dictionary<string, string> ServiceExpressionTemplates = new Dictionary<string, string>()
        {
            {WebServerConfigAttributes.Authorization.ToString(), @"services.AddAuthorization(options =>{options.FallbackPolicy = options.DefaultPolicy;});"},
            {WebServerConfigAttributes.WindowsAuthentication.ToString(), @"services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate(options =>{options.Events = new NegotiateEvents(){OnAuthenticationFailed = context => { return Task.CompletedTask}};});" }
        };

        internal static Dictionary<string, List<string>> Directives = new Dictionary<string, List<string>>()
        {
            {WebServerConfigAttributes.WindowsAuthentication.ToString(),
                                        new List<string>(){"Microsoft.AspNetCore.Authentication.Negotiate"}},
        };

        internal static bool HasAny(this IEnumerable<XElement> element)
        {
            return element != null && element.Any();
        }
    }
}
