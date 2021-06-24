using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using CTA.Rules.Actions;
using CTA.Rules.Models;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.ProjectManagement;
using CTA.WebForms2Blazor.Services;
using CTA.Rules.PortCore;
using CTA.Rules.Config;
using CTA.Rules.ProjectFile;
using CTA.Rules.Update;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class ProjectFileConverter : FileConverter
    {
        private readonly WorkspaceManagerService _blazorWorkspaceManager;
        private readonly ProjectAnalyzer _webFormsProjectAnalyzer;

        public ProjectFileConverter(
            string sourceProjectPath,
            string fullPath,
            WorkspaceManagerService blazorWorkspaceManager,
            ProjectAnalyzer projectAnalyzer
            ) : base(sourceProjectPath, fullPath)
        {
            _blazorWorkspaceManager = blazorWorkspaceManager;
            _webFormsProjectAnalyzer = projectAnalyzer;

            _blazorWorkspaceManager.NotifyNewExpectedProject();
        }

        private string GenerateProjectFileContents(string csProjFileName)
        {
            var analyzerResult = _webFormsProjectAnalyzer.AnalyzerResult;
            
            var ctaArgs = new[]
            {
                "-p", csProjFileName, // can hardcode for local use
                "-v", "net5.0",                // set the Target Framework version
                "-d", "true",                         // use the default rules files (these will get downloaded from S3 and will tell CTA which packages to add to the new .csproj file)
                "-m", "false",                        // this is the "mock run" flag. Setting it to false means rules will be applied if we do a full port.
            };

            // Handle argument assignment
            PortCoreRulesCli cli = new PortCoreRulesCli();
            cli.HandleCommand(ctaArgs);
            if (cli.DefaultRules)
            {
                // Since we're using default rules, we want to specify where to find those rules (once they are downloaded)
                cli.RulesDir = Constants.RulesDefaultPath;
            }
            
            var packageReferences = new Dictionary<string, Tuple<string, string>>
            {
                { "Autofac", new Tuple<string, string>("4.9.1.0", "4.9.3")},
                { "EntityFramework", new Tuple<string, string>("6.0.0.0", "6.4.4")},
                { "log4net", new Tuple<string, string>("2.0.8.0", "2.0.12")},
                { "Microsoft.Extensions.Logging.Log4Net.AspNetCore", new Tuple<string, string>("1.0.0", "2.2.12")}
            };
                
            // Create a configuration object using the CLI and other arbitrary values
            PortCoreConfiguration projectConfiguration = new PortCoreConfiguration()
            {
                ProjectPath = cli.FilePath,
                RulesDir = cli.RulesDir,
                IsMockRun = cli.IsMockRun,
                UseDefaultRules = cli.DefaultRules,
                PackageReferences = packageReferences,
                PortCode = false,
                PortProject = true,
                TargetVersions = new List<string> { cli.Version }
            };
            
            // Instantiating a solution port object downloads the default rules files. This is otherwise not needed
            // var solutionPort = new SolutionPort(solutionPath,
            //     new List<AnalyzerResult> {analyzerResult},
            //     new List<PortCoreConfiguration> {projectConfiguration});

            // Use the analyzer results and project config to create a project rewriter
            var projectRewriter = new ProjectRewriter(analyzerResult, projectConfiguration);

            // Initialize() will run a rules analysis and identify which porting rules to execute.
            // This will also detect which packages to add to the new .csproj file
            var projectResult = projectRewriter.Initialize();
            var projectActions = projectResult.ProjectActions;
            var packages = projectActions.PackageActions.Distinct()
                .ToDictionary(p => p.Name, p => p.Version);

            // Now we can finally create the ProjectFileCreator and use it
            var projectReferences = analyzerResult?.ProjectBuildResult?.ExternalReferences?.ProjectReferences.Select(p => p.AssemblyLocation).ToList();
            var metaReferences = analyzerResult?.ProjectBuildResult?.Project?.MetadataReferences?.Select(m => m.Display).ToList();
            var projectFileCreator = new ProjectFileCreator(FullPath, projectConfiguration.TargetVersions, packages, 
                projectReferences, ProjectType.WebForms, metaReferences);

            return projectFileCreator.CreateContents();
        }


        public override async Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            // TODO: Extract info from project files and
            // call _blazorWorkspaceManager.CreateProjectFile
            // and _webFormsWorkspaceManager.CreateProjectFile

            // TODO: Retrieve cancellation token from thread manager service
            // _blazorWorkspaceManager.WaitUntilAllDocumentsInWorkspace(token);

            // TODO: Extract accumulated project info from 
            // workspace and use it to build the actual
            // project file
            
            string newCsProjContent = GenerateProjectFileContents(FullPath);
            FileInformation fi = new FileInformation(RelativePath, Encoding.UTF8.GetBytes(newCsProjContent));

            var fileList = new List<FileInformation>() { fi };

            return fileList;
        }
    }
}
