using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace CTA.WebForms2Blazor.ProjectManagement
{
    public class ProjectAnalyzer
    {
        private readonly string _inputProjectPath;

        public string InputProjectPath { get { return _inputProjectPath; } }

        public ProjectAnalyzer(string inputProjectPath)
        {
            _inputProjectPath = inputProjectPath;
        }

        public IEnumerable<FileInfo> GetProjectFileInfo()
        {
            var directoryInfo = new DirectoryInfo(_inputProjectPath);

            return directoryInfo.GetFiles("*", SearchOption.AllDirectories).Where(fileInfo =>
                !FileFilter.ShouldIgnoreFileAtPath(Path.GetRelativePath(_inputProjectPath, fileInfo.FullName))
            );
        }
    }
}
