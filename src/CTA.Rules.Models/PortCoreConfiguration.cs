﻿using System.Collections.Generic;

namespace CTA.Rules.Models
{
    public class PortCoreConfiguration : ProjectConfiguration
    {
        public PortCoreConfiguration() : base()
        {
        }
        public bool UseDefaultRules { get; set; }
        public List<string> FrameworkMetaReferences;
    }
}
