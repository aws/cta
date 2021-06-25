using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using CTA.Rules.Models;
using CTA.Rules.Update;
using Microsoft.Extensions.Logging;

namespace CTA.WebForms2Blazor.ProjectManagement
{
    public class ProjectAnalyzer
    {
        private readonly string _inputProjectPath;
        public AnalyzerResult AnalyzerResult { get; }
        public PortCoreConfiguration ProjectConfiguration { get; }
        public ProjectResult ProjectResult { get; }
        public List<String> ProjectReferences { get; }
        public List<String> MetaReferences { get; }
        
        

        public string InputProjectPath { get { return _inputProjectPath; } }

        public ProjectAnalyzer(string inputProjectPath, AnalyzerResult analyzerResult, PortCoreConfiguration projectConfiguration)
        {
            _inputProjectPath = inputProjectPath;
            AnalyzerResult = analyzerResult;
            ProjectConfiguration = projectConfiguration;
            
            var projectRewriter = new ProjectRewriter(analyzerResult, projectConfiguration);

            // Initialize() will run a rules analysis and identify which porting rules to execute.
            // This will also detect which packages to add to the new .csproj file
            ProjectResult = projectRewriter.Initialize();

            // Now we can finally create the ProjectFileCreator and use it
            ProjectReferences = analyzerResult?.ProjectBuildResult?.ExternalReferences?.ProjectReferences.Select(p => p.AssemblyLocation).ToList();
            MetaReferences = analyzerResult?.ProjectBuildResult?.Project?.MetadataReferences?.Select(m => m.Display).ToList();
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
