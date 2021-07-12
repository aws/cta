using System;
using System.Collections.Generic;
using CTA.WebForms2Blazor.ControlConverters;

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
            // ["asp:gridview"] = new GridViewControlConverter()
        };
    }
}
