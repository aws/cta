using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using Microsoft.Extensions.Logging;

namespace CTA.WebForms2Blazor.ProjectManagement
{
    public class ProjectAnalyzer
    {
        private readonly string _inputProjectPath;
        private readonly AnalyzerResult _analyzerResult;
        
        public AnalyzerResult AnalyzerResult { get { return _analyzerResult; } }

        public string InputProjectPath { get { return _inputProjectPath; } }

        public ProjectAnalyzer(string inputProjectPath, AnalyzerResult analyzerResult)
        {
            _inputProjectPath = inputProjectPath;
            _analyzerResult = analyzerResult;

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
