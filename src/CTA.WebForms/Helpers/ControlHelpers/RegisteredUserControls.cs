using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CTA.WebForms.ControlConverters;

namespace CTA.WebForms.Helpers.ControlHelpers
{
    public class RegisteredUserControls
    {
        // This Dictionary will be filled out dynamically by the RegisterDirectiveConverter
        // and identifies the user controls to be converted
        public readonly ConcurrentDictionary<string, ControlConverter> UserControlRulesMap = 
            new ConcurrentDictionary<string, ControlConverter>(StringComparer.InvariantCultureIgnoreCase);
    }
}
