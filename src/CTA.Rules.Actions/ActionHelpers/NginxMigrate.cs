using System;
using System.Configuration;
using System.IO;
using System.Text;
using CTA.Rules.Config;
using CTA.Rules.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CTA.Rules.Actions.ActionHelpers
{
    public class NginxMigrate
    {
        private readonly string _projectDir;
        private readonly ProjectType _projectType;

        public NginxMigrate(string projectDir, ProjectType projectType)
        {
            _projectDir = projectDir;
            _projectType = projectType;
        }

        public void MigrateConfigToNginx(Configuration webConfig)
        {
            JObject nginxConfig = PortServerConfiguration(TemplateHelper.GetTemplateFileContent("", _projectType, Constants.nginxConfig));
            AddNginxConfigFile(nginxConfig, _projectDir);
        }


        private JObject PortServerConfiguration(string templateContent)
        {
            JObject contentJobject = JsonConvert.DeserializeObject<JObject>(templateContent);

            JObject obj = (JObject)contentJobject["server"];
            // value of the Jproperty key might change based on input provided
            // Add basic nginx server configuration
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach (var kvp in Constants.nginxBaseAttributes)
            {
                foreach (string value in kvp.Value)
                {
                    sb.AppendFormat($"{kvp.Key} {value};", Environment.NewLine);
                }
            }
            sb.Append("}");
            obj.Add(new JProperty("location /", sb.ToString()));
            return contentJobject;
        }

        private void AddNginxConfigFile(JObject content, string projectDir)
        {
            var formattedContent = content.ToString().Replace(",", ";").Replace(":", "").Replace("\"", "");
            File.WriteAllText(Path.Combine(projectDir, "nginx.conf"), formattedContent);
            LogHelper.LogInformation(string.Format("Create nginx.conf file using web.config settings"));
        }
    }
}
