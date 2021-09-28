using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CTA.Rules.Models;

namespace CTA.Rules.Actions
{
    public static class ServerConfigTemplates
    {
        public const string ConfigureServicesMethod = "ConfigureServices";
        public const string ConfigureMiddlewareMethod = "Configure";

        public static List<string> middlewareConfigMethods = new List<string>()
        {
            ConfigureServicesMethod,
            ConfigureMiddlewareMethod
        };

        public static Dictionary<string, string> configurationExpressionTemplates = new Dictionary<string, string>()
        {
            {WebServerConfigAttributes.Authentication.ToString(), "app.UseAuthentication();"},
            {WebServerConfigAttributes.Authorization.ToString(), "app.UseAuthorization();" },
            {WebServerConfigAttributes.Modules.ToString(),"app.UseMiddleware<{0}>();"},
            {WebServerConfigAttributes.Handlers.ToString(), @"app.MapWhen(context => context.Request.Path.ToString().EndsWith({0}),appBranch => {{ appBranch.UseMiddleware<{1}>();}});" }
        };

        public static Dictionary<string, string> serviceExpressionTemplates = new Dictionary<string, string>()
        {
            {WebServerConfigAttributes.Authorization.ToString(), @"services.AddAuthorization(options =>{options.FallbackPolicy = options.DefaultPolicy;});"},
            {WebServerConfigAttributes.WindowsAuthentication.ToString(), @"services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate(options =>{options.Events = new NegotiateEvents(){OnAuthenticationFailed = context => { return Task.CompletedTask}};});" }
        };

        public static Dictionary<string, List<string>> directives = new Dictionary<string, List<string>>()
        {
            {WebServerConfigAttributes.WindowsAuthentication.ToString(),
                                        new List<string>(){"Microsoft.AspNetCore.Authentication.Negotiate"}},
        };

        public static bool HasAny(this IEnumerable<XElement> element)
        {
            return element != null && element.Any();
        }
    }
}
