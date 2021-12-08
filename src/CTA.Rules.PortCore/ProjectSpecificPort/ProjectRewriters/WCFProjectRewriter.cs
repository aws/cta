using System;
using System.Collections.Generic;
using System.IO;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.PortCore;
using Microsoft.CodeAnalysis.CSharp;

namespace CTA.Rules.Update
{
    /// <summary>
    /// Runs rule updates on a Project
    /// </summary>
    public class WCFProjectRewriter : ProjectRewriter
    {
        /// <summary>
        /// Initializes a new instance of ProjectRewriter using an existing analysis
        /// </summary>
        /// <param name="analyzerResult">The analysis results of the project</param>
        /// <param name="projectConfiguration">ProjectConfiguration for this project</param>
        public WCFProjectRewriter(AnalyzerResult analyzerResult, ProjectConfiguration projectConfiguration)
            : base(analyzerResult, projectConfiguration)
        {
        }

        public WCFProjectRewriter(IDEProjectResult projectResult, ProjectConfiguration projectConfiguration)
            : base(projectResult, projectConfiguration)
        {
        }

        /// <summary>
        /// Initializes the ProjectRewriter then runs it
        /// </summary>
        public override ProjectResult Run()
        {
            var projectResult = Initialize();
            return Run(projectResult.ProjectActions);
        }

        /// <summary>
        /// Runs the project rewriter using a previously initialized analysis
        /// </summary>
        /// <param name="projectActions"></param>
        public override ProjectResult Run(ProjectActions projectActions)
        {
            base.Run(projectActions);

            // After running all other actions, run WCF-specific changes
            if (ProjectConfiguration.PortCode)
            {
                RunWCFChanges();
            }

            return _projectResult;
        }

        public override List<IDEFileActions> RunIncremental(List<string> updatedFiles, RootNodes projectRules)
        {
            base.RunIncremental(updatedFiles, projectRules);
            var ideFileActions = new List<IDEFileActions>();

            // After running all other actions, run WCF-specific changes
            if (ProjectConfiguration.PortCode)
            {
                RunWCFChanges();
            }

            return ideFileActions;
        }

        private void RunWCFChanges()
        {
            var projectDir = Path.GetDirectoryName(ProjectConfiguration.ProjectPath);
            var programFile = Path.Combine(projectDir, FileTypeCreation.Program.ToString() + ".cs");

            WCFServicePort wcfServicePort = new WCFServicePort(projectDir, ProjectConfiguration.ProjectType, _analyzerResult);

            try
            {
                if (File.Exists(programFile))
                {
                    var programFileTree = CSharpSyntaxTree.ParseText(File.ReadAllText(programFile));

                    var newRootNode = wcfServicePort.ReplaceProgramFile(programFileTree);

                    File.WriteAllText(programFile, newRootNode.ToFullString());
                }
            }
            catch (Exception e)
            {
                LogHelper.LogError("WCF Porting Error: Error while writing to Program.cs file: ", e.Message);
            }

            var startupFile = Path.Combine(projectDir, FileTypeCreation.Startup.ToString() + ".cs");

            try
            {
                if (File.Exists(startupFile))
                {
                    var newStartupFileText = wcfServicePort.ReplaceStartupFile(startupFile);

                    File.WriteAllText(startupFile, newStartupFileText);
                }
            }
            catch (Exception e)
            {
                LogHelper.LogError("WCF Porting Error: Error while writing to Startup file: ", e.Message);
            }

            try
            {
                if (ProjectConfiguration.ProjectType == ProjectType.WCFConfigBasedService)
                {
                    var newConfigFileText = wcfServicePort.GetNewConfigFile();

                    var newConfigPath = Path.Combine(projectDir, PortCore.WCF.Constants.PortedConfigFileName);

                    if (newConfigFileText != null)
                    {
                        File.WriteAllText(newConfigPath, newConfigFileText);
                    }

                    var configFilePath = wcfServicePort.GetConfigFilePath();

                    string backupFile = string.Concat(configFilePath, ".bak");
                    if (File.Exists(backupFile))
                    {
                        File.Delete(backupFile);
                    }
                    File.Move(configFilePath, backupFile);
                }
            }
            catch (Exception e)
            {
                LogHelper.LogError("WCF Porting Error: Error while creating config file: ", e.Message);
            }
        }
    }
}
