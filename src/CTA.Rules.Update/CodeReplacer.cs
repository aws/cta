using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Codelyzer.Analysis.Build;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.Update.Rewriters;
using Microsoft.CodeAnalysis;

namespace CTA.Rules.Update
{
    public class CodeReplacer
    {
        private readonly ProjectConfiguration _projectConfiguration;
        private readonly IEnumerable<SourceFileBuildResult> _sourceFileBuildResults;

        public CodeReplacer(List<SourceFileBuildResult> sourceFileBuildResults, ProjectConfiguration projectConfiguration)
        {
            _sourceFileBuildResults = sourceFileBuildResults;
            _projectConfiguration = projectConfiguration;
        }

        public Dictionary<string, List<GenericActionExecution>> Run(ProjectActions projectActions, ProjectType projectType)
        {
            IEnumerable<FileActions> fileActions = projectActions.FileActions;

            ConcurrentDictionary<string, List<GenericActionExecution>> actionsPerProject = new ConcurrentDictionary<string, List<GenericActionExecution>>();

            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = Constants.ThreadCount };

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

                        ActionsRewriter oneRewriter = new ActionsRewriter(sourceFileBuildResult.SemanticModel, sourceFileBuildResult.SyntaxGenerator, currentFileActions);
                        root = oneRewriter.Visit(root);


                        var result = root.NormalizeWhitespace().ToFullString();
                        //TODO : Can you send a result
                        if (!_projectConfiguration.IsMockRun)
                        {
                            File.WriteAllText(sourceFileBuildResult.SourceFileFullPath, result);
                        }
                        var processedActions = ValidateActions(oneRewriter.allActions, result);
                        processedActions = AddActionsWithoutExecutions(currentFileActions, oneRewriter.allActions);

                        if (!actionsPerProject.TryAdd(sourceFileBuildResult.SourceFileFullPath, processedActions))
                        {
                            throw new FilePortingException(sourceFileBuildResult.SourceFilePath, new Exception("File already exists in collection"));
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
                            runResult = projectLevelAction.ProjectFileActionFunc(_projectConfiguration.ProjectPath, projectType, _projectConfiguration.TargetVersions, projectActions.PackageActions.Distinct().ToDictionary(p => p.Name, p => p.Version), projectActions.ProjectReferenceActions.ToList());
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

            if (!actionsPerProject.TryAdd(Constants.Project, projectRunActions))
            {
                LogHelper.LogError(new FilePortingException(Constants.Project, new Exception("Error adding project to actions collection")));
            }

            return actionsPerProject.ToDictionary(a => a.Key, a => a.Value);
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


        public List<GenericActionExecution> ValidateActions(List<GenericActionExecution> actions, string fileResult)
        {
            var trimmedResult = Utils.EscapeAllWhitespace(fileResult);

            foreach (var action in actions)
            {
                var actionValidation = action.ActionValidation;
                var actionValid = true;

                if (actionValidation == null) { continue; }

                var contains = !string.IsNullOrEmpty(actionValidation.Contains) ? Utils.EscapeAllWhitespace(actionValidation.Contains) : "";
                var notContains = !string.IsNullOrEmpty(actionValidation.NotContains) ? Utils.EscapeAllWhitespace(actionValidation.NotContains) : "";

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
