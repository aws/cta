using System;
using System.Collections.Generic;
using CTA.WebForms2Blazor.ControlConverters;
using CTA.WebForms2Blazor.DirectiveConverters;

namespace CTA.WebForms2Blazor.Helpers.ControlHelpers
{
    public static class SupportedControls
    {
        //Uncomment when implemented
        public static readonly Dictionary<String, ControlConverter> ControlRulesMap = new Dictionary<string, ControlConverter>()
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

        public static readonly DirectiveConverter DefaultDirectiveConverter = new DirectiveConverter();
    }
}
