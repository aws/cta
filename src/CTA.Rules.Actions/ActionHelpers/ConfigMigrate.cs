using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CTA.Rules.Config;
using CTA.Rules.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CTA.Rules.Actions
{
    public class ConfigMigrate
    {
        private const string ConfigFoundMessage = "Found and migrated settings from web.config";
        private readonly string _projectDir;
        private readonly ProjectType _projectType;
        private bool _hasData;

        /// <summary>
        /// If connection string is encrypted,  "configProtectionProvider" is present.
        /// </summary>
        private const string providerForEncryptedConnString = "configProtectionProvider";

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
        private string MigrateWebConfig()
        {
            var migrateConfigMessage = string.Empty;

            var configXml = LoadWebConfig(_projectDir);
            if (configXml == null) { return migrateConfigMessage; }

            var config = ProcessWebConfig(configXml, TemplateHelper.GetTemplateFileContent(string.Empty, _projectType, Constants.AppSettingsJson));
            if (_hasData)
            {
                migrateConfigMessage = ConfigFoundMessage;
                AddAppSettingsJsonFile(config, _projectDir);
            }

            if (_projectType == ProjectType.Mvc || _projectType == ProjectType.WebApi)
            {
                // port server configuration
                PortServerConfig(configXml, _projectDir, _projectType);
            }

            return migrateConfigMessage;
        }

        private void PortServerConfig(Configuration configXml, string projectDir, ProjectType projectType)
        {
            ConfigurationSection serverConfig = configXml.Sections[Constants.WebServer];

            if (serverConfig != null)
            {
                ServerConfigMigrate serverConfigMigrate = new ServerConfigMigrate(projectDir, projectType);
                serverConfigMigrate.PortServerConfiguration(serverConfig);
            }
            return;
        }

        private Configuration LoadWebConfig(string projectDir)
        {
            string webConfigFilePath = Path.Combine(projectDir, Constants.WebConfig);

            if (File.Exists(webConfigFilePath))
            {
                try
                {

                    XElement webConfigXml = XElement.Load(webConfigFilePath);
                    System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
                    xmlDocument.Load(webConfigFilePath);

                    // Comment out connection strings if type has a configProtectionProvider
                    // This can comment out connection strings that do not have "EncryptedData"
                    IEnumerable<XElement> encryptedConnectionStringElement =
                        from element in webConfigXml.Elements("connectionStrings")
                        where (string)element.Attribute("configProtectionProvider") != null
                        select element;

                    if (encryptedConnectionStringElement.HasAny())
                    {
                        System.Xml.XmlNode elementToComment = xmlDocument.SelectSingleNode("/configuration/connectionStrings");
                        string commentContents = elementToComment.OuterXml;

                        // Its contents are the XML content of target node
                        System.Xml.XmlComment commentNode = xmlDocument.CreateComment(commentContents);

                        // Get a reference to the parent of the target node
                        System.Xml.XmlNode parentNode = elementToComment.ParentNode;

                        // Replace the target node with the comment
                        parentNode.ReplaceChild(commentNode, elementToComment);
                        xmlDocument.Save(webConfigFilePath);
                    }

                    var fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = webConfigFilePath };
                    var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                    return configuration;
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, string.Format("Error processing web.config file {0}", webConfigFilePath));
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
            JObject defaultContent = JsonConvert.DeserializeObject<JObject>(templateContent);
            var connectionStringsObjects = GetConnectionStrings(webConfig);
            var appSettingsObjects = GetAppSettingObjects(webConfig);
            var kestrelHttpConfig = ServerConfigTemplates.DefaultKestrelHttpConfig;

            if(!string.IsNullOrEmpty(kestrelHttpConfig))
            {
                var kestrelConfigJobj = JObject.Parse(kestrelHttpConfig);
                defaultContent.Add(Constants.Kestrel, kestrelConfigJobj);
            }

            _hasData = connectionStringsObjects.Any() || appSettingsObjects.Any();

            if (_hasData)
            {
                if (connectionStringsObjects.Count > 0)
                {
                    AddToJsonObject(defaultContent, Constants.ConnectionStrings, connectionStringsObjects);
                }
                if (appSettingsObjects.Count > 0)
                {
                    AddToJsonObject(defaultContent, Constants.AppSettings, appSettingsObjects);
                }
            }
            return defaultContent;
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
