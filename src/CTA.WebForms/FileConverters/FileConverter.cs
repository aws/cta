using System.Threading.Tasks;
using System.Collections.Generic;
using CTA.WebForms.FileInformationModel;
using System.IO;
using CTA.Rules.Config;
using CTA.WebForms.Services;

namespace CTA.WebForms.FileConverters
{
    public abstract class FileConverter
    {
        protected const string ConverterType = "FileConverter";
        private readonly string _relativePath;
        private readonly string _fullPath;
        private readonly string _sourceProjectPath;
        private protected readonly TaskManagerService _taskManager;
        private protected int _taskId;

        public string RelativePath { get { return _relativePath; } }

        public string FullPath { get { return _fullPath; } }

        public string ProjectPath { get { return _sourceProjectPath; } }

        protected FileConverter(string sourceProjectPath, string fullPath, TaskManagerService taskManager)
        {
            _relativePath = Path.GetRelativePath(sourceProjectPath, fullPath);
            _fullPath = fullPath;
            _sourceProjectPath = sourceProjectPath;
            _taskManager = taskManager;

            // We want to force the use of the task manager even if each file doesn't
            // necessarily have to do any managed runs, this is because they may end up
            // unblocking other processes by simply running normally
            var taskDescriptionProperties = new Dictionary<string, string>
            {
                { "Type", GetType().Name },
                { "Path", FullPath }
            };
            _taskId = _taskManager.RegisterNewTask(taskDescriptionProperties);
            LogHelper.LogInformation(string.Format(
                Constants.RegisteredAsTaskLogTemplate,
                GetType().Name,
                Constants.FileMigrationLogAction,
                _fullPath,
                _taskId));
        }

        public abstract Task<IEnumerable<FileInformation>> MigrateFileAsync();

        private protected void DoCleanUp()
        {
            // TODO: Put other general clean up
            // tasks here as they come up
            _taskManager.RetireTask(_taskId);
        }

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
