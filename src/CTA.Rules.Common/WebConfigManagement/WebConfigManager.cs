using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Linq;
using CTA.Rules.Config;

namespace CTA.Rules.Common.WebConfigManagement
{
    /// <summary>
    /// Consumers can use this class to load a Web.config into memory specify its representation based on need.
    /// For accessing more well-defined Web.config properties (e.g. connection strings), it can be loaded as a
    /// Configuration object. For more flexible navigation, it can be loaded as an XDocument with additional
    /// helper functions.
    /// </summary>
    public class WebConfigManager
    {
        private static Dictionary<string, Configuration> _configurationCache = new Dictionary<string, Configuration>();
        private static Dictionary<string, XDocument> _xDocumentCache = new Dictionary<string, XDocument>();
        private delegate object ConfigLoadingDelegate(string configFile);

        // Currently unused
        [ExcludeFromCodeCoverage]
        public static Configuration LoadWebConfigAsConfiguration(string projectDir)
        {
            var config = LoadWebConfig(projectDir, webConfigFile =>
            {
                if (_configurationCache.TryGetValue(webConfigFile, out var cached))
                {
                    return cached;
                }

                var fileMap = new ExeConfigurationFileMap {ExeConfigFilename = webConfigFile};
                var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                _configurationCache[webConfigFile] = configuration;

                return _configurationCache[webConfigFile];
            }) as Configuration;
            
            return config;
        }

        public static WebConfigXDocument LoadWebConfigAsXDocument(string projectDir)
        {
            var config = LoadWebConfig(projectDir, webConfigFile =>
            {
                if (_xDocumentCache.TryGetValue(webConfigFile, out var cached))
                {
                    return cached;
                }

                var xDocument = XDocument.Load(webConfigFile);
                _xDocumentCache[webConfigFile] = xDocument;

                return _xDocumentCache[webConfigFile];
            }) as XDocument;

            return new WebConfigXDocument(config);
        }

        public static WebConfigXDocument LoadAppConfigAsXDocument(string projectDir)
        {
            var config = LoadAppConfig(projectDir, appConfigFile =>
            {
                if (_xDocumentCache.TryGetValue(appConfigFile, out var cached))
                {
                    return cached;
                }

                var xDocument = XDocument.Load(appConfigFile);
                _xDocumentCache[appConfigFile] = xDocument;

                return _xDocumentCache[appConfigFile];
            }) as XDocument;

            return new WebConfigXDocument(config);
        }

        public static void ClearCache()
        {
            _configurationCache = new Dictionary<string, Configuration>();
            _xDocumentCache = new Dictionary<string, XDocument>();
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

        private static object LoadAppConfig(string projectDir, ConfigLoadingDelegate configLoadingDelegate)
        {
            string appConfigFile = Path.Combine(projectDir, Constants.AppConfig);

            if(File.Exists(appConfigFile))
            {
                try
                {
                    return configLoadingDelegate.Invoke(appConfigFile);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, string.Format("Error processing App.config file {0}", appConfigFile));
                }
            }
            return null;
        }
    }
}
