using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using CTA.Rules.Config;

namespace CTA.Rules.Common.WebConfigManagement
{
    public class WebConfigManager
    {
        private static readonly Dictionary<string, Configuration> ConfigurationCache = new Dictionary<string, Configuration>();
        private static readonly Dictionary<string, XDocument> XDocumentCache = new Dictionary<string, XDocument>();
        private delegate object ConfigLoadingDelegate(string configFile);

        public static Configuration LoadWebConfigAsConfiguration(string projectDir)
        {
            var config = LoadWebConfig(projectDir, webConfigFile =>
            {
                if (ConfigurationCache.TryGetValue(webConfigFile, out var cached))
                {
                    return cached;
                }

                var fileMap = new ExeConfigurationFileMap {ExeConfigFilename = webConfigFile};
                var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                ConfigurationCache[webConfigFile] = configuration;

                return ConfigurationCache[webConfigFile];
            }) as Configuration;
            
            return config;
        }

        public static WebConfigXDocument LoadWebConfigAsXDocument(string projectDir)
        {
            var config = LoadWebConfig(projectDir, webConfigFile =>
            {
                if (XDocumentCache.TryGetValue(webConfigFile, out var cached))
                {
                    return cached;
                }

                var xDocument = XDocument.Load(webConfigFile);
                XDocumentCache[webConfigFile] = xDocument;

                return XDocumentCache[webConfigFile];
            }) as XDocument;

            return new WebConfigXDocument(config);
        }

        private static object LoadWebConfig(string projectDir, ConfigLoadingDelegate configLoadingDelegate)
        {
            string webConfigFile = Path.Combine(projectDir, Constants.WebConfig);

            if (File.Exists(webConfigFile))
            {
                try
                {
                    return configLoadingDelegate.Invoke(webConfigFile);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, string.Format("Error processing web.config file {0}", webConfigFile));
                }
            }
            return null;
        }
    }
}
