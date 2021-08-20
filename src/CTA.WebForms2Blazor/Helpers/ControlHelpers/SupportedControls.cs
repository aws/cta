using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CTA.WebForms2Blazor.ControlConverters;
using CTA.WebForms2Blazor.DirectiveConverters;

namespace CTA.WebForms2Blazor.Helpers.ControlHelpers
{
    public static class SupportedControls
    {
        //RegisteredUserControls is a wrapper class for the dictionary UserControlMap
        //This was done to create a modifiable static dictionary that is passed into every Register directive,
        //which adds all user controls detected in the file to one overall dictionary
        public static readonly RegisteredUserControls UserControls = new RegisteredUserControls();
        
        public static readonly ConcurrentDictionary<string, ControlConverter> ControlRulesMap = new ConcurrentDictionary<string, ControlConverter>()
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
        public static readonly ConcurrentDictionary<string, DirectiveConverter> DirectiveRulesMap = new ConcurrentDictionary<string, DirectiveConverter>(StringComparer.InvariantCultureIgnoreCase)
        {
            ["Master"] = new MasterDirectiveConverter(),
            ["Page"] = new PageDirectiveConverter(),
            ["Register"] = new RegisterDirectiveConverter(UserControls),
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
