using CTA.Rules.Config;
using CTA.Rules.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CTA.Rules.Actions
{
    public class ConfigMigrate
    {
        private string _projectDir;
        private ProjectType _projectType;
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

            var config = ProcessWebConfig(configXml, TemplateHelper.GetTemplateFileContent("", _projectType, Constants.appSettingsJson));
            if (_hasData)
            {
                isConfigFound = "Found and migrated settings from web.config";
                AddAppSettingsJsonFile(config, _projectDir);
            }

            return isConfigFound;
        }
        private XDocument LoadWebConfig(string projectDir)
        {
            string webConfigFile = Path.Combine(projectDir, Constants.webConfig);

            if (File.Exists(webConfigFile))
            {
                try
                {
                    var webConfig = XDocument.Load(webConfigFile);
                    return webConfig;
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
        private JObject ProcessWebConfig(XDocument webConfig, string templateContent)
        {
            var connectionStringsObjects = GetConnectionStrings(webConfig);
            var appSettingsObjects = GetAppSettingObjects(webConfig);

            if(_hasData)
            {
                var defaultContent = JsonConvert.DeserializeObject<JObject>(templateContent);

                if (connectionStringsObjects.Count > 0)
                {
                    AddToJsonObject(defaultContent, Constants.ConnectionStrings, connectionStringsObjects);
                }
                if (appSettingsObjects.Count > 0)
                {
                    AddToJsonObject(defaultContent, Constants.appSettings, appSettingsObjects);
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
        private Dictionary<string, string> GetConnectionStrings(XDocument webConfig)
        {
            Dictionary<string, string> connectionStringObjects = new Dictionary<string, string>();
            webConfig.Descendants()?.Where(d => d.Name.ToString().ToLower() == Constants.connectionstrings)?.Descendants()?.ToList().ForEach(connectionString =>
            {
                try
                {
                    var name = connectionString.Attributes()?.First(a => a.Name?.ToString().ToLower() == Constants.name).Value;
                    var value = connectionString.Attributes()?.First(a => a.Name?.ToString().ToLower() == Constants.connectionstring).Value;

                    connectionStringObjects.Add(name, value);
                    _hasData = true;
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, "Error while parsing connection string");
                }
            });
            return connectionStringObjects;
        }

        /// <summary>
        /// Gets app settings from web.config file
        /// </summary>
        /// <param name="webConfig"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetAppSettingObjects(XDocument webConfig)
        {
            Dictionary<string, string> appSettingsObjects = new Dictionary<string, string>();

            webConfig.Descendants()?.Where(d => d.Name?.ToString().ToLower() == Constants.appSettings)?.Descendants()?.ToList().ForEach(appSetting =>
            {
                try
                {
                    var key = appSetting.Attributes()?.First(a => a.Name?.ToString().ToLower() == Constants.key).Value;
                    var value = appSetting.Attributes()?.First(a => a.Name?.ToString().ToLower() == Constants.value).Value;
                    if (!Constants.appSettingsExclusions.Contains(key))
                    {
                        appSettingsObjects.Add(key, value);
                        _hasData = true;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, "Error while parsing appsettings");
                }
            });

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
                new JArray(
                    arrayContent.Select(c =>
                    new JObject(new JProperty(c.Key, c.Value))))
                ));
        }

        /// <summary>
        /// Writes the appSettings.json file to the project dir
        /// </summary>        
        /// <param name="content">The content of the file</param>
        /// <param name="projectDir">The project directory where this file will be created</param>
        private void AddAppSettingsJsonFile(JObject content, string projectDir)
        {
            File.WriteAllText(Path.Combine(projectDir, Constants.appSettingsJson), content.ToString());
            LogChange(string.Format("Create appsettings.json file using web.config settings"));
        }
        private void LogChange(string message)
        {
            LogHelper.LogInformation(message);
        }
    }
}
