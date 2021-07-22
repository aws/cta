using System;
using System.Collections.Generic;
using CTA.WebForms2Blazor.ControlConverters;
using CTA.WebForms2Blazor.DirectiveConverters;

namespace CTA.WebForms2Blazor.Helpers.ControlHelpers
{
    public static class SupportedControls
    {
        public static readonly Dictionary<string, ControlConverter> ControlRulesMap = new Dictionary<string, ControlConverter>()
        {
            ["asp:hyperlink"] = new HyperLinkControlConverter(),
            ["asp:button"] = new ButtonControlConverter(),
            ["asp:label"] = new LabelControlConverter(),
            ["asp:listview"] = new ListViewControlConverter(),
            ["asp:gridview"] = new GridViewControlConverter(),
            ["asp:content"] = new ContentControlConverter(),
            ["asp:contentplaceholder"] = new ContentPlaceHolderControlConverter()
        };

        // NOTE: Directive names appear to be case-insensitive
        public static readonly Dictionary<string, DirectiveConverter> DirectiveRulesMap = new Dictionary<string, DirectiveConverter>(StringComparer.InvariantCultureIgnoreCase)
        {
            ["Master"] = new MasterDirectiveConverter(),
            ["Page"] = new PageDirectiveConverter()
        };

        public static readonly Dictionary<string, Func<string, string>> AspExpressionRulesMap = new Dictionary<string, Func<string, string>>()
        {
            ["AppSettings"] = expr => string.Format(Constants.RazorConfigurationAccessTemplate, $"{Constants.AppSettingsSection}:{expr}"),
            ["ConnectionStrings"] = expr => string.Format(Constants.RazorConfigurationAccessTemplate, $"{Constants.ConnectionStringsSection}:{expr}")
        };

        public static readonly DirectiveConverter DefaultDirectiveConverter = new DirectiveConverter();
        public static readonly Func<string, string, string> DefaultAspExpressionConverter = (exprType, expr) => string.Format(Constants.RazorExplicitEmbeddingTemplate, $"{exprType}.{expr}");
    }
}
