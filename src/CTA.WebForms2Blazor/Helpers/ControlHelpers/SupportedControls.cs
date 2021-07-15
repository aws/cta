using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CTA.WebForms2Blazor.ControlConverters;
using CTA.WebForms2Blazor.DirectiveConverters;

namespace CTA.WebForms2Blazor.Helpers.ControlHelpers
{
    public static class SupportedControls
    {
        private const string UnsupportedAspExpressionTypeCommentTemplate = "@* Asp expresion type {0} with content {1} not currently supported *@";

        /// <summary>
        /// Used to identify tags whose names follow web forms control format, it will match
        /// if the name consists of 2 strings of the following characters [a-zA-Z0-9] separated
        /// by a single colon
        /// </summary>
        public static readonly Regex ControlNameFormatRegex = new Regex(@"^[a-zA-Z0-9]+:[a-zA-Z0-9]+$");

        public static readonly Dictionary<string, ControlConverter> ControlRulesMap = new Dictionary<string, ControlConverter>()
        {
            ["asp:hyperlink"] = new HyperLinkControlConverter(),
            ["asp:button"] = new ButtonControlConverter(),
            ["asp:label"] = new LabelControlConverter(),
            ["asp:listview"] = new ListViewControlConverter(),
            // ["asp:gridview"] = new GridViewControlConverter()
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

        public static readonly ControlConverter DefaultControlConverter = new UnknownControlConverter();
        public static readonly DirectiveConverter DefaultDirectiveConverter = new DirectiveConverter();
        public static readonly Func<string, string, string> DefaultAspExpressionConverter = (exprType, expr) => string.Format(UnsupportedAspExpressionTypeCommentTemplate, exprType, expr);
    }
}
