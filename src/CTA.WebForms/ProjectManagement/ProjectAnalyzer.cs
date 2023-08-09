using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using CTA.Rules.Models;
using Microsoft.Extensions.Logging;
using CTA.Rules.Config;
using Codelyzer.Analysis.Model;

namespace CTA.WebForms.ProjectManagement
{
    public class ProjectAnalyzer
    {
        private const string DiscoveredFilesLogTemplate = "{0}: Discovered {1} Files in Project at {2}";
        private const string RetrievedFilesForProcessingLogTemplate = "{0}: Retrieved {1}/{2} Discovered Files for Processing";

        private IEnumerable<FileInfo> _migrationFileInfo;
        private IEnumerable<FileInfo> _ignorableFileInfo;

        private readonly string _inputProjectPath;
        public AnalyzerResult AnalyzerResult { get; }
        public ProjectConfiguration ProjectConfiguration { get; }
        public ProjectResult ProjectResult { get; }
        public List<String> ProjectReferences { get; }
        public List<String> MetaReferences { get; }
        
        
        public string InputProjectPath { get { return _inputProjectPath; } }

        public ProjectAnalyzer(string inputProjectPath, AnalyzerResult analyzerResult, ProjectConfiguration projectConfiguration, ProjectResult projectResult)
        {
            _inputProjectPath = inputProjectPath;
            AnalyzerResult = analyzerResult;
            ProjectConfiguration = projectConfiguration;

            ProjectResult = projectResult;

            ProjectReferences = analyzerResult?.ProjectBuildResult?.ExternalReferences?.ProjectReferences.Select(p => p.AssemblyLocation).ToList();
            MetaReferences = analyzerResult?.ProjectBuildResult?.Project?.MetadataReferences?.Select(m => m.Display).ToList();
        }

        public IEnumerable<FileInfo> GetProjectFileInfo()
        {
            if (_migrationFileInfo == null)
            {
                DoAnalysis();
            }

            return _migrationFileInfo;
        }

        public IEnumerable<FileInfo> GetProjectIgnoredFiles()
        {
            if (_ignorableFileInfo == null)
            {
                DoAnalysis();
            }

            return _ignorableFileInfo;
        }

        private void DoAnalysis()
        {
            var directoryInfo = new DirectoryInfo(_inputProjectPath);
            var allFiles = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            LogHelper.LogInformation(string.Format(DiscoveredFilesLogTemplate, GetType().Name, allFiles.Length, InputProjectPath));

            var migrationFiles = allFiles.Where(fileInfo => !FileFilter.ShouldIgnoreFileAtPath(Path.GetRelativePath(_inputProjectPath, fileInfo.FullName)));
            LogHelper.LogInformation(string.Format(RetrievedFilesForProcessingLogTemplate, GetType().Name, migrationFiles.Count(), allFiles.Length));

            _migrationFileInfo = migrationFiles;
            _ignorableFileInfo = allFiles.Where(fileInfo => !migrationFiles.Contains(fileInfo));
        }
    }
}
