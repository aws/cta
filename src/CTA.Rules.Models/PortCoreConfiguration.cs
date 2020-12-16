using CTA.Rules.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.Rules.Models
{
    public class PortCoreConfiguration : ProjectConfiguration
    {
        public PortCoreConfiguration() : base()
        {
        }
        public bool UseDefaultRules { get; set; }
    }
}
