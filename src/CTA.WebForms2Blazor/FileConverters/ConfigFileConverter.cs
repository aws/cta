using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.Rules.Actions;
using CTA.Rules.Models;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using System.Linq;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class ConfigFileConverter : FileConverter
    {
        private const string webConfigFile = "web.config";
        private string _projectPrefix;
        private string _fullPath;
        private string _relativeDirectory;

        public ConfigFileConverter(string relativePath) : base(relativePath)
        {
            var workingDirectory = Environment.CurrentDirectory;
            _projectPrefix = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            _fullPath = Path.Combine(_projectPrefix, RelativePath);
            _relativeDirectory = Path.GetDirectoryName(RelativePath);

        }


        public override async Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            string filename = Path.GetFileName(RelativePath);
            FileInformation fi;

            if (filename.Equals(webConfigFile))
            {
                //ProjectType WebForms doesn't really exist yet, but can be added for more specific configuration
                ConfigMigrate configMigrate = new ConfigMigrate(_fullPath, ProjectType.WebForms);
                configMigrate.WebformsWebConfigMigrateHelper();

                string newPath = Path.Combine(_relativeDirectory, "appsettings.json");
                string fullNewPath = Path.Combine(_projectPrefix, newPath);
                fi = new FileInformation(newPath, File.ReadAllBytes(fullNewPath));
            } else
            {
                fi = null;
            }

            var fileList = new List<FileInformation>();
            fileList.Add(fi);

            return fileList;
        }
    }
}
