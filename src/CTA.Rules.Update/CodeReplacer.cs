using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.Rules.Update.Rewriters;
using Codelyzer.Analysis.Build;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;

namespace CTA.Rules.Update
{
    public class CodeReplacer
    {
        private ProjectConfiguration _projectConfiguration;
        private IEnumerable<SourceFileBuildResult> _sourceFileBuildResults;

        public CodeReplacer(List<SourceFileBuildResult> sourceFileBuildResults, ProjectConfiguration projectConfiguration) {
            _sourceFileBuildResults = sourceFileBuildResults;
            _projectConfiguration = projectConfiguration;
        }

        public Dictionary<string, List<GenericActionExecution>> Run(ProjectActions projectActions, ProjectType projectType)
        {
            IEnumerable<FileActions> fileActions = projectActions.FileActions;
            Dictionary<string, List<GenericActionExecution>> actionsPerProject = new Dictionary<string, List<GenericActionExecution>>();

            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = Constants.ThreadCount };

            Parallel.ForEach(_sourceFileBuildResults, parallelOptions, sourceFileBuildResult => {
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

                        actionsPerProject.Add(sourceFileBuildResult.SourceFileFullPath, processedActions);
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
            foreach(var projectLevelAction in projectActions.ProjectLevelActions)
            {
                var projectActionExecution = new GenericActionExecution(projectLevelAction, _projectConfiguration.ProjectPath);
                projectActionExecution.TimesRun = 1;
                if (!_projectConfiguration.IsMockRun)
                {
                    if (projectLevelAction.ProjectLevelActionFunc != null)
                    {
                        try
                        {
                            projectLevelAction.ProjectLevelActionFunc(_projectConfiguration.ProjectPath, projectType);
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
                            projectLevelAction.ProjectFileActionFunc(_projectConfiguration.ProjectPath, projectType, _projectConfiguration.TargetVersions, projectActions.PackageActions.Distinct().ToDictionary(p => p.Name, p => p.Version), projectActions.ProjectReferenceActions.ToList());
                        }
                        catch (Exception ex)
                        {
                            var actionExecutionException = new ActionExecutionException(projectLevelAction.Name, projectLevelAction.Key, ex);
                            projectActionExecution.InvalidExecutions = 1;
                            LogHelper.LogError(actionExecutionException);
                        }
                    }
                }
                projectRunActions.Add(projectActionExecution);
                LogHelper.LogInformation(projectLevelAction.Description);
            }

            actionsPerProject.Add(Constants.Project, projectRunActions);

            return actionsPerProject;
        }

        private List<GenericActionExecution> AddActionsWithoutExecutions(FileActions currentFileActions, List<GenericActionExecution> allActions)
        {
            var filePath = currentFileActions.FilePath;

            //We are looking for an action that is in our file actions but has no executions:
            var actionsWithNoMatch = currentFileActions.AllActions
                .Where(a => !allActions.Any(runInstance => runInstance.Name == a.Name && runInstance.Type == a.Type && runInstance.Key == a.Key && runInstance.Value == a.Value));

            foreach (var action in actionsWithNoMatch)
            {
                var genericActionExecution = new GenericActionExecution(action, filePath);
                genericActionExecution.TimesRun = 0;
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

                if(actionValidation == null) { continue; }

                if(!string.IsNullOrEmpty(actionValidation.Contains) && !trimmedResult.Contains(actionValidation.Contains))
                {
                    //Validation token is not in the result source file:
                    actionValid = false;
                }

                if (!string.IsNullOrEmpty(actionValidation.NotContains) && trimmedResult.Contains(actionValidation.NotContains))
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
