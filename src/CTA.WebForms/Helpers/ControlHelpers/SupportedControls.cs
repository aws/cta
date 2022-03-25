using System;
using System.Collections.Concurrent;
using CTA.WebForms.DirectiveConverters;

namespace CTA.WebForms.Helpers.ControlHelpers
{
    public static class SupportedControls
    {
        // NOTE: Directive names appear to be case-insensitive
        public static readonly ConcurrentDictionary<string, DirectiveConverter> DirectiveRulesMap = new ConcurrentDictionary<string, DirectiveConverter>(StringComparer.InvariantCultureIgnoreCase)
        {
            ["Master"] = new MasterDirectiveConverter(),
            ["Page"] = new PageDirectiveConverter(),
            ["Register"] = new RegisterDirectiveConverter(),
            ["Control"] = new ControlDirectiveConverter()
        };

        public static readonly ConcurrentDictionary<string, Func<string, string>> AspExpressionRulesMap = new ConcurrentDictionary<string, Func<string, string>>()
        {
            ["AppSettings"] = expr => string.Format(Constants.RazorConfigurationAccessTemplate, $"{Constants.AppSettingsSection}:{expr}"),
            ["ConnectionStrings"] = expr => string.Format(Constants.RazorConfigurationAccessTemplate, $"{Constants.ConnectionStringsSection}:{expr}")
        };

        public static readonly DirectiveConverter DefaultDirectiveConverter = new DirectiveConverter();
        public static readonly Func<string, string, string> DefaultAspExpressionConverter = (exprType, expr) => string.Format(Constants.RazorExplicitEmbeddingTemplate, $"{exprType}.{expr}");
    }
}
