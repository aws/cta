using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.Rules.Config;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public abstract class WebConfigFeature : CompiledFeature
    {
        private const string WebConfigFileName = Rules.Config.Constants.WebConfig;

        protected static Dictionary<string, IEnumerable<XDocument>> ConfigCache =
            new Dictionary<string, IEnumerable<XDocument>>();

        protected IEnumerable<XDocument> LoadWebConfigs(AnalyzerResult analyzerResult)
        {
            var directory = analyzerResult.ProjectResult.ProjectRootPath;
            if (ConfigCache.TryGetValue(directory, out var cachedConfigs))
            {
                return cachedConfigs;
            }

            var webConfigsFound = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                    .Where(name => name.EndsWith(WebConfigFileName, StringComparison.InvariantCultureIgnoreCase));

            var webConfigurations = new List<XDocument>();
            foreach (var webConfig in webConfigsFound)
            {
                try
                {
                    var configuration = XDocument.Load(webConfig);
                    webConfigurations.Add(configuration);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, $"Error processing web.config file : {webConfig}");
                }
            }

            ConfigCache[directory] = webConfigurations;
            return ConfigCache[directory];
        }
    }
}
