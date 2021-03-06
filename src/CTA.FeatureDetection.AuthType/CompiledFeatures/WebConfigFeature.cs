using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.Rules.Config;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public abstract class WebConfigFeature : CompiledFeature
    {
        private const string WebConfigFileName = Rules.Config.Constants.WebConfig;

        protected IEnumerable<Configuration> LoadWebConfigs(AnalyzerResult analyzerResult)
        {
            var directory = analyzerResult.ProjectResult.ProjectRootPath;

            var webConfigsFound = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                    .Where(name => string.Equals(name, WebConfigFileName, StringComparison.InvariantCultureIgnoreCase));

            var webConfigurations = new List<Configuration>();
            foreach (var webConfig in webConfigsFound)
            {
                try
                {
                    var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = webConfig };
                    var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                    webConfigurations.Add(configuration);
                }
                catch (XmlException ex)
                {
                    LogHelper.LogError(ex, $"Error processing web.config file : {webConfig}");
                }
            }

            return webConfigurations;
        }
    }
}
