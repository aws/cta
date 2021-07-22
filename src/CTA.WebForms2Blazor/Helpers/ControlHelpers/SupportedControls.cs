using System;
using System.Collections.Generic;
using CTA.WebForms2Blazor.ControlConverters;
using CTA.WebForms2Blazor.DirectiveConverters;

namespace CTA.WebForms2Blazor.Helpers.ControlHelpers
{
    public static class SupportedControls
    {
        private const string UnsupportedAspExpressionTypeCommentTemplate = "@* Asp expresion type {0} with content {1} not currently supported *@";

        public static readonly Dictionary<string, ControlConverter> ControlRulesMap = new Dictionary<string, ControlConverter>()
        {
            ["asp:hyperlink"] = new HyperLinkControlConverter(),
            ["asp:button"] = new ButtonControlConverter(),
            ["asp:label"] = new LabelControlConverter(),
            ["asp:listview"] = new ListViewControlConverter(),
            ["asp:gridview"] = new GridViewControlConverter(),
            ["asp:content"] = new RemoveNodeKeepContentsConverter(),
            ["html"] = new RemoveNodeKeepContentsConverter(),
            ["body"] = new RemoveNodeKeepContentsConverter(),
            ["head"] = new RemoveNodeAndContentsConverter(),
            ["scripts"] = new RemoveNodeAndContentsConverter(),
            ["!doctype"] = new RemoveNodeAndContentsConverter(),
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
        public static readonly Func<string, string, string> DefaultAspExpressionConverter = (exprType, expr) => string.Format(UnsupportedAspExpressionTypeCommentTemplate, exprType, expr);
    }
}
