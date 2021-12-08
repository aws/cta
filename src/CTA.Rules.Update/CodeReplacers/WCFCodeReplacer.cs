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
    public class WCFCodeReplacer : CodeReplacer
    {
        public WCFCodeReplacer(List<SourceFileBuildResult> sourceFileBuildResults, ProjectConfiguration projectConfiguration, List<string> metadataReferences, AnalyzerResult analyzerResult,
            List<string> updatedFiles = null, ProjectResult projectResult = null)
            : base(sourceFileBuildResults, projectConfiguration, metadataReferences, analyzerResult, updatedFiles, projectResult)
        {
        }
        
        protected override List<GenericActionExecution> ApplyProjectActions(ProjectActions projectActions, ProjectType projectType)
        {
            var genericActionExecutions = base.ApplyProjectActions(projectActions, projectType);
            
            if (_projectConfiguration.PortProject)
            {
                // TODO: Return actions executed here and append them to genericActionExecutions so they can be collected by telemetry
                RunWCFChanges();
            }

            return genericActionExecutions;
        }

        private void RunWCFChanges()
        {
            var projectDir = Path.GetDirectoryName(_projectConfiguration.ProjectPath);
            var programFile = Path.Combine(projectDir, FileTypeCreation.Program.ToString() + ".cs");

            WCFServicePort wcfServicePort = new WCFServicePort(projectDir, _projectConfiguration.ProjectType, _analyzerResult);

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
                if (_projectConfiguration.ProjectType == ProjectType.WCFConfigBasedService)
                {
                    var newConfigFileText = wcfServicePort.GetNewConfigFile();

                    var newConfigPath = Path.Combine(projectDir, WCF.Constants.PortedConfigFileName);

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
