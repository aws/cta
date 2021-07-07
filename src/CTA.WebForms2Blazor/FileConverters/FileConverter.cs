using System.Threading.Tasks;
using System.Collections.Generic;
using CTA.WebForms2Blazor.FileInformationModel;
using System.IO;
using CTA.Rules.Config;

namespace CTA.WebForms2Blazor.FileConverters
{
    public abstract class FileConverter
    {
        private readonly string _relativePath;
        private readonly string _fullPath;
        private readonly string _sourceProjectPath;

        public string RelativePath { get { return _relativePath; } }

        public string FullPath { get { return _fullPath; } }

        public string ProjectPath { get { return _sourceProjectPath; } }

        protected FileConverter(string sourceProjectPath, string fullPath)
        {
            _relativePath = Path.GetRelativePath(sourceProjectPath, fullPath);
            _fullPath = fullPath;
            _sourceProjectPath = sourceProjectPath;
        }

        public abstract Task<IEnumerable<FileInformation>> MigrateFileAsync();

        private protected void LogStart()
        {
            LogHelper.LogInformation(string.Format(
                Constants.StartedAtLogTemplate,
                GetType().Name,
                Constants.FileMigrationLogAction,
                _fullPath));
        }

        private protected void LogEnd()
        {
            LogHelper.LogInformation(string.Format(
                Constants.EndedAtLogTemplate,
                GetType().Name,
                Constants.FileMigrationLogAction,
                _fullPath));
        }
    }
}
