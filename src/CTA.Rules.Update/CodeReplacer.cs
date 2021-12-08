using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using CTA.FeatureDetection.Common.Extensions;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.PortCore;
using CTA.WebForms2Blazor;
using CTA.Rules.Update.Rewriters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using TextChange = CTA.Rules.Models.TextChange;
using TextSpan = Codelyzer.Analysis.Model.TextSpan;
using WCFConstants = CTA.Rules.Update.WCF.Constants;
using System.Threading;

namespace CTA.Rules.Update
{
    public class CodeReplacer
    {
        private readonly ProjectConfiguration _projectConfiguration;
        private readonly IEnumerable<SourceFileBuildResult> _sourceFileBuildResults;
        private readonly List<string> _metadataReferences;
        private readonly AnalyzerResult _analyzerResult;
        private readonly ProjectResult _projectResult;

        public CodeReplacer(List<SourceFileBuildResult> sourceFileBuildResults, ProjectConfiguration projectConfiguration, List<string> metadataReferences, AnalyzerResult analyzerResult,
            List<string> updatedFiles = null)
        {
            _sourceFileBuildResults = sourceFileBuildResults;
            if(updatedFiles != null)
            {
                _sourceFileBuildResults = _sourceFileBuildResults.Where(s => updatedFiles.Contains(s.SourceFileFullPath));
            }
            _analyzerResult = analyzerResult;
            _projectConfiguration = projectConfiguration;
            _metadataReferences = metadataReferences;
        }

        public CodeReplacer(List<SourceFileBuildResult> sourceFileBuildResults, ProjectConfiguration projectConfiguration, List<string> metadataReferences, AnalyzerResult analyzerResult,
            List<string> updatedFiles = null, ProjectResult projectResult = null)
        {
            _sourceFileBuildResults = sourceFileBuildResults;
            if (updatedFiles != null)
            {
                _sourceFileBuildResults = _sourceFileBuildResults.Where(s => updatedFiles.Contains(s.SourceFileFullPath));
            }
            _analyzerResult = analyzerResult;
            _projectConfiguration = projectConfiguration;
            _metadataReferences = metadataReferences;
            _projectResult = projectResult;
        }

        public Dictionary<string, List<GenericActionExecution>> Run(ProjectActions projectActions, ProjectType projectType)
        {
            IEnumerable<FileActions> fileActions = projectActions.FileActions;

            ConcurrentDictionary<string, List<GenericActionExecution>> actionsPerProject = new ConcurrentDictionary<string, List<GenericActionExecution>>();

            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = Constants.ThreadCount };
            var fileActionsCount = fileActions.Count();

            Parallel.ForEach(_sourceFileBuildResults, parallelOptions, sourceFileBuildResult =>
            {
                try
                {
                    var currentFileActions = fileActions.Where(f => f.FilePath == sourceFileBuildResult.SourceFileFullPath).FirstOrDefault();

                    var root = sourceFileBuildResult.SyntaxTree.GetRoot();

                    if (currentFileActions != null)
                    {
                        LogHelper.LogInformation("---------------------------------------------------------------------------");
                        LogHelper.LogInformation("Processing file " + sourceFileBuildResult.SourceFilePath);

                        if (_projectConfiguration.PortCode)
                        {
                            if (_projectConfiguration.ProjectType == ProjectType.WebForms)
                            {
                                RunWebFormsChanges();
                            }
                            else
                            {
                                RunCodeChanges(root, sourceFileBuildResult, currentFileActions, actionsPerProject);
                            }
                        }                       
                        else
                        {
                            GenerateCodeChanges(root, sourceFileBuildResult, currentFileActions, fileActionsCount, actionsPerProject);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var filePortingException = new FilePortingException(sourceFileBuildResult.SourceFilePath, ex);
                    LogHelper.LogError(filePortingException);
                }
            });

            var projectRunActions = new List<GenericActionExecution>();

            if (_projectConfiguration.PortProject)
            {
                if (_projectConfiguration.ProjectType == ProjectType.WebForms)
                {
                    RunWebFormsChanges();
                }
                else
                {
                    projectRunActions = ApplyProjectActions(projectActions, projectType);

                    if (!actionsPerProject.TryAdd(Constants.Project, projectRunActions))
                    {
                        LogHelper.LogError(new FilePortingException(Constants.Project, new Exception("Error adding project to actions collection")));
                    }

                    if (_projectConfiguration.ProjectType == ProjectType.WCFConfigBasedService || _projectConfiguration.ProjectType == ProjectType.WCFCodeBasedService)
                    {
                        RunWCFChanges();
                    }
                }
            }
            return actionsPerProject.ToDictionary(a => a.Key, a => a.Value);
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

                    var newConfigPath = Path.Combine(projectDir, WCFConstants.PortedConfigFileName);

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

        private void RunWebFormsChanges()
        {
            var projectDir = Path.GetDirectoryName(_projectConfiguration.ProjectPath);
            var projectParentDir = Path.GetDirectoryName(projectDir);

            try 
            {
                MigrationManager migrationManager = new MigrationManager(projectDir, "", _analyzerResult, _projectConfiguration, _projectResult);
                Task.Run(() => migrationManager.PerformMigration()).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                LogHelper.LogError("WebForms Porting Error: Error while migrating WebForms to Blazor: ", e.Message);
            }  
        }

        private void RunCodeChanges(SyntaxNode root, SourceFileBuildResult sourceFileBuildResult, FileActions currentFileActions, ConcurrentDictionary<string, List<GenericActionExecution>> actionsPerProject)
        {
            ActionsRewriter oneRewriter = new ActionsRewriter(sourceFileBuildResult.SemanticModel, sourceFileBuildResult.PrePortSemanticModel, sourceFileBuildResult.SyntaxGenerator, currentFileActions.FilePath, currentFileActions.AllActions);
            root = oneRewriter.Visit(root);
            var result = root.NormalizeWhitespace().ToFullString();

            if (!_projectConfiguration.IsMockRun)
            {
                File.WriteAllText(sourceFileBuildResult.SourceFileFullPath, result);
            }

            var processedActions = ValidateActions(oneRewriter.allExecutedActions, root);
            processedActions = AddActionsWithoutExecutions(currentFileActions, oneRewriter.allExecutedActions);

            if (!actionsPerProject.TryAdd(sourceFileBuildResult.SourceFileFullPath, processedActions))
            {
                throw new FilePortingException(sourceFileBuildResult.SourceFilePath, new Exception("File already exists in collection"));
            }
        }

        private void GenerateCodeChanges(SyntaxNode root, SourceFileBuildResult sourceFileBuildResult, FileActions currentFileActions, int fileActionsCount, ConcurrentDictionary<string, List<GenericActionExecution>> actionsPerProject)
        {
            if (currentFileActions != null)
            {
                var normalizedRoot = root.NormalizeWhitespace().ToFullString();
                //If true, and we are doing a full analysis, line endings and spaces need to be normalized:
                //TODO change the condition to be a config value in ProjectConfiguration instead of file count
                if (normalizedRoot != root.ToFullString() && fileActionsCount > 1)
                {
                    File.WriteAllText(sourceFileBuildResult.SourceFileFullPath, normalizedRoot);
                }

                currentFileActions.NodeTokens.ForEach(nodetoken =>
                {
                    nodetoken.AllActions.ForEach(nodeAction => {
                        ActionsRewriter oneRewriter = new ActionsRewriter(sourceFileBuildResult.SemanticModel, sourceFileBuildResult.PrePortSemanticModel, sourceFileBuildResult.SyntaxGenerator, currentFileActions.FilePath, nodeAction);

                        var newRoot = oneRewriter.Visit(root);
                        var allChanges = newRoot.SyntaxTree.GetChanges(root.SyntaxTree);

                        foreach (var textChange in allChanges)
                        {
                            var fileLinePositionSpan = root.SyntaxTree.GetMappedLineSpan(textChange.Span);
                            var newTextChange = new TextChange() { FileLinePositionSpan = fileLinePositionSpan, NewText = textChange.NewText };
                            if (!nodetoken.TextChanges.Contains(newTextChange))
                            {
                                nodetoken.TextChanges.Add(newTextChange);
                            }
                        }
                    });
                });
            }
        }

        private List<GenericActionExecution> ApplyProjectActions(ProjectActions projectActions, ProjectType projectType)
        {
            var projectRunActions = new List<GenericActionExecution>();
            //Project Level Actions
            foreach (var projectLevelAction in projectActions.ProjectLevelActions)
            {
                var projectActionExecution = new GenericActionExecution(projectLevelAction, _projectConfiguration.ProjectPath)
                {
                    TimesRun = 1
                };
                var runResult = string.Empty;
                if (!_projectConfiguration.IsMockRun)
                {
                    if (projectLevelAction.ProjectLevelActionFunc != null)
                    {
                        try
                        {
                            runResult = projectLevelAction.ProjectLevelActionFunc(_projectConfiguration.ProjectPath, projectType);
                        }
                        catch (Exception ex)
                        {
                            var actionExecutionException = new ActionExecutionException(projectLevelAction.Name, projectLevelAction.Key, ex);
                            projectActionExecution.InvalidExecutions = 1;
                            LogHelper.LogError(actionExecutionException);
                        }
                    }
                    else if (projectLevelAction.ProjectFileActionFunc != null)
                    {
                        try
                        {
                            runResult = projectLevelAction.ProjectFileActionFunc(_projectConfiguration.ProjectPath,
                                projectType,
                                _projectConfiguration.TargetVersions,
                                projectActions.PackageActions.GroupBy(g=>g.Name).Select(g=>g.FirstOrDefault()).ToDictionary(p => p.Name, p => p.Version),
                                projectActions.ProjectReferenceActions.ToList(),
                                _metadataReferences);
                        }
                        catch (Exception ex)
                        {
                            var actionExecutionException = new ActionExecutionException(projectLevelAction.Name, projectLevelAction.Key, ex);
                            projectActionExecution.InvalidExecutions = 1;
                            LogHelper.LogError(actionExecutionException);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(runResult))
                {
                    projectActionExecution.Description = string.Concat(projectActionExecution.Description, ": ", runResult);
                    projectRunActions.Add(projectActionExecution);
                    LogHelper.LogInformation(projectLevelAction.Description);
                }
            }

            RunProjectSpecificChanges(projectType);

            return projectRunActions;
        }

        private void RunProjectSpecificChanges(ProjectType projectType)
        {
            if (projectType == ProjectType.WCFConfigBasedService || projectType == ProjectType.WCFCodeBasedService)
            {
                RunWCFChanges();
            }
        }


        private List<GenericActionExecution> AddActionsWithoutExecutions(FileActions currentFileActions, List<GenericActionExecution> allActions)
        {
            var filePath = currentFileActions.FilePath;

            //We are looking for an action that is in our file actions but has no executions:
            var actionsWithNoMatch = currentFileActions.AllActions
                .Where(a => !allActions.Any(runInstance => runInstance.Name == a.Name && runInstance.Type == a.Type && runInstance.Key == a.Key && runInstance.Value == a.Value));

            foreach (var action in actionsWithNoMatch)
            {
                var genericActionExecution = new GenericActionExecution(action, filePath)
                {
                    TimesRun = 0
                };
                allActions.Add(genericActionExecution);
            }

            allActions = allActions
                .GroupBy(a => new { a.Type, a.Name, a.Value })
                .Select(g => new GenericActionExecution()
                {
                    Type = g.First().Type,
                    Name = g.First().Name,
                    Value = g.First().Value,
                    FilePath = filePath,
                    TimesRun = g.Sum(gt => gt.TimesRun),
                    InvalidExecutions = g.Sum(gi => gi.InvalidExecutions)
                })
                .ToList();

            return allActions;
        }


        public List<GenericActionExecution> ValidateActions(List<GenericActionExecution> actions, SyntaxNode root)
        {
            // Matches all types of comments and strings
            //string regComments = @"\/\*(?:(?!\*\/)(?:.|[\r\n]+))*\*\/|\/\/(.*?)\r?\n|""((\\[^\n]|[^""\n])*)""|@(""[^""""]*"")+";

            //We should only validate actions that did not throw an exception during execution.
            var validActions = actions.Where(a => a.InvalidExecutions == 0).ToList();

            foreach (var action in validActions)
            {
                var actionValidation = action.ActionValidation;
                string trimmedResult;
                var actionValid = true;

                if (actionValidation == null) { continue; }

                if (string.IsNullOrEmpty(actionValidation.CheckComments) || !bool.Parse(actionValidation.CheckComments))
                {
                    trimmedResult = Utils.EscapeAllWhitespace(root.NoComments().NormalizeWhitespace().ToFullString());
                }
                else
                {
                    trimmedResult = Utils.EscapeAllWhitespace(root.NormalizeWhitespace().ToFullString());
                }

                var contains = !string.IsNullOrEmpty(actionValidation.Contains) ? Utils.EscapeAllWhitespace(actionValidation.Contains) : string.Empty;
                var notContains = !string.IsNullOrEmpty(actionValidation.NotContains) ? Utils.EscapeAllWhitespace(actionValidation.NotContains) : string.Empty;

                if (!string.IsNullOrEmpty(contains) && !trimmedResult.Contains(contains))
                {
                    //Validation token is not in the result source file:
                    actionValid = false;
                }

                if (!string.IsNullOrEmpty(notContains) && trimmedResult.Contains(notContains))
                {
                    //Validation Negative token is in the result source file:
                    actionValid = false;
                }
                //Action is not valid, but didn't throw an exception (we don't want to log an invalid execution twice)
                if (!actionValid && action.InvalidExecutions == 0)
                {
                    var actionValidationException = new ActionValidationException(action.Key, action.Name);
                    action.InvalidExecutions = 1;
                    LogHelper.LogError(actionValidationException);
                }
            }
            return actions;
        }
    }
}
