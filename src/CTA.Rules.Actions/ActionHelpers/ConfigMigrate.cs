using CTA.Rules.Config;
using CTA.Rules.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
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
        public void Run()
        {
            MigrateWebConfig();
        }
        /// <summary>
        /// Migrates the web.config file, if it exists
        /// </summary>
        /// <param name="projectDir">Directory of the project</param>
        /// <param name="projectType">Type of the project</param>
        private void MigrateWebConfig()
        {
            configuration config = ProcessWebConfig(_projectDir);
            if (config != null)
            {
                AddAppSettingsJsonFile(config, TemplateHelper.GetTemplateFileContent("", _projectType, "appsettings.json"), _projectDir);
            }
        }

        /// <summary>
        /// Deserialize web.config file to an object
        /// </summary>
        /// <param name="projectDir">The project directory containing the web.config file</param>
        /// <returns></returns>
        private configuration ProcessWebConfig(string projectDir)
        {
            string webConfigFileDir = Path.Combine(projectDir, "web.config");

            configuration webConfig = null;
            if (File.Exists(webConfigFileDir))
            {
                try
                {
                    using (FileStream reader = File.OpenRead(webConfigFileDir))
                    {
                        XmlSerializer ser = new XmlSerializer(typeof(configuration));
                        webConfig = (configuration)ser.Deserialize(reader);
                    }
                }
                catch (Exception)
                {
                    LogHelper.LogError("Error processing web.config file {0}", webConfigFileDir);
                }
            }
            return webConfig;
        }

        /// <summary>
        /// Adds a config (json) file using the project template, then populates it with data from appsettings and connectionstrings in web.config
        /// </summary>
        /// <param name="webConfig">The object representing the web.config file</param>
        /// <param name="templateContent">The template file to be used</param>
        /// <param name="projectDir">The project directory where this file will be created</param>
        private void AddAppSettingsJsonFile(configuration webConfig, string templateContent, string projectDir)
        {
            StringBuilder str = new StringBuilder();

            if (webConfig.connectionStrings != null)
            {
                //Connection Strings
                foreach (var connectionString in webConfig.connectionStrings)
                {
                    // Escape any backslashes in connection string value
                    var formattedConnectionString = connectionString.connectionString.Replace(@"\", @"\\");
                    str.Append("\"").Append(connectionString.name).Append("\"").Append(":").Append("\"").Append(formattedConnectionString).Append("\"").Append(",");
                }
                str.Remove(str.Length - 1, 1);
                str.Insert(0, "\"ConnectionStrings\": { ");
                str.Append("},");
            }

            if (webConfig.appSettings != null)
            {
                //App settings:
                foreach (var setting in webConfig.appSettings)
                {
                    if (!Constants.appSettingsExclusions.Contains(setting.key))
                    {
                        str.Append("\"").Append(setting.key).Append("\"").Append(":").Append("\"").Append(setting.value).Append("\"").Append(",");
                    }
                }
            }
            if (str.Length > 0)
            {
                string fileContent = templateContent.Insert(1, str.ToString());

                var obj = JsonConvert.DeserializeObject(fileContent);
                fileContent = JsonConvert.SerializeObject(obj, Formatting.Indented);

                // Any escaped backslashes were duplicated when the content was deserialized and serialized
                File.WriteAllText(Path.Combine(projectDir, "appsettings.json"), fileContent.Replace(@"\\", @"\"));
                LogChange(string.Format("Create appsettings.json file using web.config settings"));
            }
        }
        private void LogChange(string message)
        {
            LogHelper.LogInformation(message);
        }
    }
}
