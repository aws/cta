using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CTA.Rules.Actions;
using CTA.Rules.Models;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Services;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class ConfigFileConverter : FileConverter
    {
        private const string WebConfigFile = "web.config";
        private string _relativeDirectory;

        public ConfigFileConverter(string sourceProjectPath, string fullPath, TaskManagerService taskManagerService)
            : base(sourceProjectPath, fullPath, taskManagerService)
        {
            _relativeDirectory = Path.GetDirectoryName(RelativePath);
        }

        public override Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            LogStart();

            string filename = Path.GetFileName(RelativePath);
            var fileList = new List<FileInformation>();

            //Currently only handles web.config, package.config handled by ProjectFileConverter, others not handled
            if (filename.Equals(WebConfigFile, StringComparison.InvariantCultureIgnoreCase))
            {
                //ProjectType WebForms doesn't really exist yet, but can be added for more specific configuration
                ConfigMigrate configMigrate = new ConfigMigrate(FullPath, ProjectType.WebForms);
                var migratedString = configMigrate.WebformsWebConfigMigrateHelper();

                string newPath = Path.Combine(_relativeDirectory, "appsettings.json");
                fileList.Add(new FileInformation(newPath, Encoding.UTF8.GetBytes(migratedString)));
            }

            DoCleanUp();
            LogEnd();

            return Task.FromResult((IEnumerable<FileInformation>)fileList);
        }
    }
}
