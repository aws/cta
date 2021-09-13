using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using CTA.Rules.Actions.ActionHelpers;
using CTA.Rules.Config;
using CTA.Rules.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CTA.Rules.Actions
{
    public class ConfigMigrate
    {
        private readonly string _projectDir;
        private readonly ProjectType _projectType;
        private bool _hasData;

        public ConfigMigrate(string projectDir, ProjectType projectType)
        {
            _projectDir = Directory.GetParent(projectDir).FullName;
            _projectType = projectType;
            _hasData = false;
        }
        public string Run()
        {
            return MigrateWebConfig();
        }
        /// <summary>
        /// Migrates the web.config file, if it exists
        /// </summary>
        /// <param name="projectDir">Directory of the project</param>
        /// <param name="projectType">Type of the project</param>
        private string MigrateWebConfig()
        {
            var isConfigFound = string.Empty;

            var configXml = LoadWebConfig(_projectDir);
            if (configXml == null) { return isConfigFound; }

            var config = ProcessWebConfig(configXml, TemplateHelper.GetTemplateFileContent("", _projectType, Constants.AppSettingsJson));
            if (_hasData)
            {
                isConfigFound = "Found and migrated settings from web.config";
                AddAppSettingsJsonFile(config, _projectDir);
            }

            if (_projectType == ProjectType.Mvc || _projectType == ProjectType.CoreMvc || _projectType == ProjectType.WebApi || _projectType == ProjectType.CoreWebApi)
            {
                // add Nginx configuration
                bool hasServerSettings = configXml.Sections["system.webServer"] != null;

                if (hasServerSettings)
                {
                    NginxMigrate nginxMigrate = new NginxMigrate(_projectDir, _projectType);
                    nginxMigrate.MigrateConfigToNginx(configXml);
                }
            }
                
            return isConfigFound;
        }

        private Configuration LoadWebConfig(string projectDir)
        {
            string webConfigFile = Path.Combine(projectDir, Constants.WebConfig);

            if (File.Exists(webConfigFile))
            {
                try
                {
                    var fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = webConfigFile };
                    var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                    return configuration;
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, string.Format("Error processing web.config file {0}", webConfigFile));
                }
            }
            return null;
        }

        /// <summary>
        /// Reads data from web.config and writes any relevant entries to appsettings.json file
        /// </summary>
        /// <param name="webConfig">The XML representation of the web.config file</param>
        /// <returns>A JSON object representing the appSettings.json file </returns>
        private JObject ProcessWebConfig(Configuration webConfig, string templateContent)
        {
            var connectionStringsObjects = GetConnectionStrings(webConfig);
            var appSettingsObjects = GetAppSettingObjects(webConfig);

            _hasData = connectionStringsObjects.Any() || appSettingsObjects.Any();

            if (_hasData)
            {
                var defaultContent = JsonConvert.DeserializeObject<JObject>(templateContent);

                if (connectionStringsObjects.Count > 0)
                {
                    AddToJsonObject(defaultContent, Constants.ConnectionStrings, connectionStringsObjects);
                }
                if (appSettingsObjects.Count > 0)
                {
                    AddToJsonObject(defaultContent, Constants.AppSettings, appSettingsObjects);
                }
                return defaultContent;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets connection strings from web.config file
        /// </summary>
        /// <param name="webConfig"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetConnectionStrings(Configuration webConfig)
        {
            Dictionary<string, string> connectionStringObjects = new Dictionary<string, string>();
            var connectionStrings = webConfig.ConnectionStrings?.ConnectionStrings;

            if (connectionStrings != null)
            {
                foreach (ConnectionStringSettings connectionString in connectionStrings)
                {
                    connectionStringObjects.Add(connectionString.Name, connectionString.ConnectionString);
                }
            }

            return connectionStringObjects;
        }

        /// <summary>
        /// Gets app settings from web.config file
        /// </summary>
        /// <param name="webConfig"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetAppSettingObjects(Configuration webConfig)
        {
            Dictionary<string, string> appSettingsObjects = new Dictionary<string, string>();
            var appSettings = webConfig.AppSettings?.Settings;

            if (appSettings != null)
            {
                foreach (KeyValueConfigurationElement appSetting in appSettings)
                {
                    if (!Constants.appSettingsExclusions.Contains(appSetting.Key))
                    {
                        appSettingsObjects.Add(appSetting.Key, appSetting.Value);
                    }
                }
            }
            return appSettingsObjects;
        }

        /// <summary>
        /// Adds data to json object
        /// </summary>
        /// <param name="jObject">The object to add data to</param>
        /// <param name="name">The name of the new entry</param>
        /// <param name="arrayContent">The content of the new entry</param>
        private void AddToJsonObject(JObject jObject, string name, Dictionary<string, string> arrayContent)
        {
            jObject.AddFirst(
                new JProperty(name,
                new JObject(
                    arrayContent.Select(c =>
                    (new JProperty(c.Key, c.Value)))
                )));
        }

        /// <summary>
        /// Writes the appSettings.json file to the project dir
        /// </summary>        
        /// <param name="content">The content of the file</param>
        /// <param name="projectDir">The project directory where this file will be created</param>
        private void AddAppSettingsJsonFile(JObject content, string projectDir)
        {
            File.WriteAllText(Path.Combine(projectDir, Constants.AppSettingsJson), content.ToString());
            LogChange(string.Format("Create appsettings.json file using web.config settings"));
        }

       

        private void LogChange(string message)
        {
            LogHelper.LogInformation(message);
        }
    }
}
