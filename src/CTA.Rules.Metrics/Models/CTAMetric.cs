using System;
using Newtonsoft.Json;

namespace CTA.Rules.Metrics
{
    public abstract class CTAMetric
    {
        [JsonProperty("metricsType", Order = 1)]
        public string MetricsType => "CTA";

        [JsonProperty("language", Order = 2)]
        public string Language { get; set; }
        
        protected static string GetLanguage(string projectPath)
        {
            return projectPath.EndsWith(".csproj",
                StringComparison.InvariantCultureIgnoreCase)
                ? "csharp"
                : projectPath.EndsWith(".vbproj",
                    StringComparison.InvariantCultureIgnoreCase)
                    ? "visualbasic"
                    : "unknown";
        }
    }
}
