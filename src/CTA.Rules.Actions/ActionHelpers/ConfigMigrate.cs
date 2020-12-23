using CTA.Rules.Config;
using CTA.Rules.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.Xml.Serialization;

namespace CTA.Rules.Actions
{
    public class ConfigMigrate
    {
        private string _projectDir;
        private ProjectType _projectType;

        public ConfigMigrate(string projectDir, ProjectType projectType)
        {
            _projectDir = Directory.GetParent(projectDir).FullName;
            _projectType = projectType;
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

            var config = ProcessWebConfig(configXml);
            if (!string.IsNullOrEmpty(config))
            {
                isConfigFound = "Found and migrated settings from web.config";
                AddAppSettingsJsonFile(config, TemplateHelper.GetTemplateFileContent("", _projectType, "appsettings.json"), _projectDir);
            }

            return isConfigFound;
        }
        private XDocument LoadWebConfig(string projectDir)
        {
            string webConfigFile = Path.Combine(projectDir, "web.config");

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
        /// Deserialize web.config file to an object
        /// </summary>
        /// <param name="projectDir">The project directory containing the web.config file</param>
        /// <returns></returns>
        private string ProcessWebConfig(XDocument webConfig)
        {
            StringBuilder config = new StringBuilder();

            webConfig.Descendants()?.Where(d => d.Name == "connectionStrings")?.Descendants()?.ToList().ForEach(connectionString =>
            {
                try
                {
                    var name = connectionString.Attributes()?.First(a => a.Name?.ToString().ToLower() == "name").Value;
                    var value = connectionString.Attributes()?.First(a => a.Name?.ToString().ToLower() == "connectionstring").Value?.Replace(@"\", @"\\");

                    config.Append("\"").Append(name).Append("\"").Append(":").Append("\"").Append(value).Append("\"").Append(",");
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, "Error while parsing connection string");
                }
            });

            //This means, we found at least one connection string:
            if (config.Length > 0)
            {
                config.Remove(config.Length - 1, 1);
                config.Insert(0, "\"ConnectionStrings\": { ");
                config.Append("},");
            }

            webConfig.Descendants()?.First(d => d.Name == "appSettings")?.Descendants()?.ToList().ForEach(appSetting =>
            {
                try
                {
                    var key = appSetting.Attributes()?.First(a => a.Name?.ToString().ToLower() == "key").Value;
                    var value = appSetting.Attributes()?.First(a => a.Name?.ToString().ToLower() == "value").Value;
                    if (!Constants.appSettingsExclusions.Contains(key))
                    {
                        config.Append("\"").Append(key).Append("\"").Append(":").Append("\"").Append(value).Append("\"").Append(",");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, "Error while parsing appsettings");
                }
            });

            return config.ToString();
        }



        /// <summary>
        /// Adds a config (json) file using the project template, then populates it with data from appsettings and connectionstrings in web.config
        /// </summary>
        /// <param name="webConfig">The object representing the web.config file</param>
        /// <param name="templateContent">The template file to be used</param>
        /// <param name="projectDir">The project directory where this file will be created</param>
        private void AddAppSettingsJsonFile(string content, string templateContent, string projectDir)
        {
            string fileContent = templateContent.Insert(1, content);

            var obj = JsonConvert.DeserializeObject(fileContent);
            fileContent = JsonConvert.SerializeObject(obj, Formatting.Indented);

            // Any escaped backslashes were duplicated when the content was deserialized and serialized
            File.WriteAllText(Path.Combine(projectDir, "appsettings.json"), fileContent.Replace(@"\\", @"\"));
            LogChange(string.Format("Create appsettings.json file using web.config settings"));
        }
        private void LogChange(string message)
        {
            LogHelper.LogInformation(message);
        }
    }
}
