using System;
using System.Collections.Generic;
using CTA.WebForms2Blazor.ControlConverters;

namespace CTA.WebForms2Blazor.Helpers.ControlHelpers
{
    public class RegisteredUserControls
    {
        // This Dictionary will be filled out dynamically by the RegisterDirectiveConverter
        // and identifies the user controls to be converted
        public readonly Dictionary<string, ControlConverter> UserControlRulesMap = 
            new Dictionary<string, ControlConverter>(StringComparer.InvariantCultureIgnoreCase);
    }
}
